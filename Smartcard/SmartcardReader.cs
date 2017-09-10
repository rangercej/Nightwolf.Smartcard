namespace Nightwolf.Smartcard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Common.Logging;

    /// <summary>
    /// Smartcard reader handing and monitoring
    /// </summary>
    /// <inheritdoc cref="IDisposable"/>
    public sealed class SmartcardReader : IDisposable
    {
        /// <summary>
        /// Name of "smartcard" type to get notified of new smartcards attached to the 
        /// system whilst monitoring
        /// </summary>
        private const string NotificationReader = @"\\?PnP?\Notification";

        /// <summary>
        /// Log manager
        /// </summary>
        private ILog logger = LogManager.GetLogger(typeof(SmartcardReader));
        
        /// <summary>Unmanaged handler to the smartcard reader context</summary>
        private IntPtr readerContext = IntPtr.Zero;

        /// <summary>Attached smartcard readers</summary>
        private List<string> readerNames = new List<string>();

        /// <summary>Supported smartcard type names</summary>
        private List<string> cardNames = new List<string>();

        /// <summary>Smartcard reader state cache</summary>
        private List<SmartcardInterop.ScardReaderState> currentState;

        /// <summary>Reader monitoring task handler</summary>
        private Task monitorTask;

        /// <summary>Monitoring cancellation token</summary>
        private CancellationToken cancelToken;

        /// <summary>Disposing flag to detect redundant Dispose calls</summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartcardReader"/> class. 
        /// </summary>
        public SmartcardReader()
        {
            this.monitorTask = null;
            this.IsMonitoring = false;
            this.disposedValue = false;
            this.ResetContext();
        }

        /// <summary>
        /// Event fired when a smartcard is inserted
        /// </summary>
        public event EventHandler<SmartcardEventArgs> OnCardInserted;

        /// <summary>
        /// Event fired when a smartcard is removed
        /// </summary>
        public event EventHandler<SmartcardEventArgs> OnCardRemoved;

        /// <summary>
        /// Gets a value indicating whether reader monitoring is active or not
        /// </summary>
        public bool IsMonitoring
        {
            get; private set;
        }

        /// <summary>
        /// Gets the waitable event that triggers when monitoring stops
        /// </summary>
        public ManualResetEvent MonitoringStoppedTrigger { get; private set; }

        /// <summary>
        /// Start monitoring for smartcard changes
        /// </summary>
        /// <param name="ct">Cancellation token to stop monitoring</param>
        /// <returns>False is a monitor is already running; true otherwise</returns>
        public bool StartMonitoring(CancellationToken ct)
        {
            if (this.monitorTask != null)
            {
                this.logger.Debug("Monitoring currently active.");
                return false;
            }

            if (this.currentState != null)
            {
                var readersWithCards = this.currentState.Where(x => (x.eventState & SmartcardInterop.State.Present) != 0);
                foreach (var reader in readersWithCards)
                {
                    this.FireCardPresentEvent(reader);
                }
            }

            this.MonitoringStoppedTrigger = new ManualResetEvent(false);
            this.cancelToken = ct;
            this.cancelToken.Register(this.StopMonitoring);
            this.monitorTask = Task.Factory.StartNew(this.WaitForReaderStateChange, this.cancelToken);
            this.monitorTask.ContinueWith(task =>
                {
                    this.IsMonitoring = false;
                    this.MonitoringStoppedTrigger.Set();
                });

            this.IsMonitoring = true;
            this.logger.Debug("Smartcard monitoring started.");

            return true;
        }

        #region IDisposable Support
        /// <summary>
        /// Free unmanaged resources
        /// </summary>
        /// <param name="disposing">Called from Dispose() flag</param>
        public void Dispose(bool disposing)
        {
            if (this.disposedValue)
            {
                return;
            }

            if (this.readerContext != IntPtr.Zero)
            {
                SmartcardInterop.SCardReleaseContext(this.readerContext);
            }

            this.disposedValue = true;
        }

        /// <summary>
        /// Free unmanaged resources
        /// </summary>
        /// <inheritdoc cref="Dispose()"/>
        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion

        /// <summary>
        /// Stop the smartcard monitor
        /// </summary>
        private void StopMonitoring()
        {
            this.logger.Debug("Stopping monitoring");

            if (this.monitorTask == null)
            {
                return;
            }

            this.logger.Debug("Cancelling requests");
            SmartcardInterop.SCardCancel(this.readerContext);
            this.monitorTask.Wait();
            this.monitorTask = null;
        }

        /// <summary>
        /// Create a new smartcard tracking context
        /// </summary>
        private void ResetContext()
        {
            if (this.readerContext != IntPtr.Zero)
            {
                SmartcardInterop.SCardReleaseContext(this.readerContext);
            }

            var result = SmartcardInterop.SCardEstablishContext(SmartcardInterop.Scope.User, IntPtr.Zero, IntPtr.Zero, out this.readerContext);
            if (result != SmartcardException.SCardSuccess)
            {
                this.logger.ErrorFormat("Failed to create smartcard context, result = 0x{0:X}", result);
                throw new SmartcardException(result);
            }

            this.cardNames = this.FetchCards();
            this.readerNames = this.FetchReaders();
            this.currentState = this.FetchState(this.readerNames);
        }

        /// <summary>
        /// Call the card detected delegate for each changed reader
        /// </summary>
        /// <param name="state">State class containing the smartcard reader name</param>
        private void FireCardPresentEvent(SmartcardInterop.ScardReaderState state)
        {
            var cardname = this.FindCardWithAtr(state.atr);
            var scard = new Smartcard(state.reader, cardname);
            var args = new SmartcardEventArgs(scard, state.reader);

            this.logger.DebugFormat("Firing card insert event for reader {0}, card {1}", state.reader, cardname);
            this.OnCardInserted?.Invoke(this, args);
        }

        /// <summary>
        /// Call the card removed delegate for each changed reader
        /// </summary>
        /// <param name="state">State class containing the smartcard reader name</param>
        private void FireCardRemovedEvent(SmartcardInterop.ScardReaderState state)
        {
            var args = new SmartcardEventArgs(null, state.reader);

            this.logger.DebugFormat("Firing card removed event for reader {0}", state.reader);
            this.OnCardRemoved?.Invoke(this, args);
        }

        /// <summary>
        /// Find all attached smartcard readers
        /// </summary>
        /// <returns>List of attached readers</returns>
        private List<string> FetchReaders()
        {
            var readerLen = 1024;
            var readers = new char[readerLen];

            var result = SmartcardInterop.SCardListReadersW(this.readerContext, null, readers, out readerLen);
            if (result != SmartcardException.SCardSuccess && result != SmartcardException.SCardENoReadersAvailable)
            {
                this.logger.ErrorFormat("Failed to fetch smartcard readers, result = 0x{0:X}", result);
                throw new SmartcardException(result);
            }

            var r = SmartcardInterop.MultiStringToArray(readers);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.DebugFormat("Known readers; count = {0}", r.Count);
                foreach (var reader in r)
                {
                    this.logger.DebugFormat("    {0}", reader);
                }
            }

            return r.ToList();
        }

        /// <summary>
        /// Find all supported smartcard types
        /// </summary>
        /// <returns>List of supported card types</returns>
        private List<string> FetchCards()
        {
            var cardLen = 16384;
            var cards = new char[cardLen];

            var result = SmartcardInterop.SCardListCardsW(this.readerContext, null, IntPtr.Zero, 0, cards, out cardLen);
            if (result != SmartcardException.SCardSuccess)
            {
                this.logger.ErrorFormat("Failed to fetch smartcard names, result = 0x{0:X}", result);
                throw new SmartcardException(result);
            }

            var c = SmartcardInterop.MultiStringToArray(cards);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.DebugFormat("Known cards; count = {0}", c.Count);
                foreach (var card in c)
                {
                    this.logger.DebugFormat("    {0}", card);
                }
            }

            return c.ToList();
        }

        /// <summary>
        /// Fetch the state of named smartcard readers
        /// </summary>
        /// <param name="readers">List of readers for which to fetch state</param>
        /// <returns>Smartcard reader state</returns>
        private List<SmartcardInterop.ScardReaderState> FetchState(IList<string> readers)
        {
            IList<SmartcardInterop.ScardReaderState> scardstatelist = null;

            if (this.currentState == null)
            {
                this.currentState = new List<SmartcardInterop.ScardReaderState>();
            }

            var state = this.currentState.Where(x => readers.Contains(x.reader)).ToList();
            if (state.Count == 0)
            { 
                scardstatelist = this.CreatePendingReaderState(readers);
            }

            if (scardstatelist == null)
            {
                return null;
            }

            if (readers.Count != scardstatelist.Count)
            {
                foreach (var reader in readers)
                {
                    if (!scardstatelist.Any(x => x.reader.Equals(reader)))
                    {
                        scardstatelist.Add(this.CreatePendingReaderState(reader));
                    }
                }
            }

            var scardstate = scardstatelist.ToArray();
            if (readers.Count > 0)
            {
                var d = SmartcardInterop.ArrayToMultiString(this.cardNames);

                var result = SmartcardInterop.SCardLocateCards(this.readerContext, d, scardstate, scardstate.Length);
                if (result != SmartcardException.SCardSuccess)
                {
                    this.logger.ErrorFormat("Failed to fetch smartcard state, result = 0x{0:X}", result);
                    throw new SmartcardException(result);
                }
            }

            return scardstate.ToList();
        }

        /// <summary>
        /// Update cached current state for each reader with the latest obtained state.
        /// </summary>
        /// <param name="changedStates">List of states to update</param>
        private void SaveState(List<SmartcardInterop.ScardReaderState> changedStates)
        {
            var changedReaders = changedStates.Select(x => x.reader).ToList();
            var initialState = this.currentState.Where(x => !changedReaders.Contains(x.reader)).ToList();
            foreach (var state in changedStates)
            {
                var s = state;
                s.currentState = s.eventState;
                s.eventState = 0;
                initialState.Add(s);
            }

            this.currentState.Clear();
            this.currentState.AddRange(initialState);
        }

        /// <summary>
        /// Monitor for smartcard change events and fire events on detected changes
        /// </summary>
        private void WaitForReaderStateChange()
        {
            var awaitNewReader = new SmartcardInterop.ScardReaderState
            {
                reader = NotificationReader,
                currentState = 0,
                eventState = 0,
                atrLength = 0
            };

            this.currentState.Add(awaitNewReader);

            var i = 0;
            var result = 0;
            while (!this.cancelToken.IsCancellationRequested)
            {
                try
                {
                    i++;
                    this.logger.Debug(DateTime.Now + ": StateChange - start: " + i + "; last status = 0x" + result.ToString("X"));

                    var scardstate = this.currentState.ToArray();
                    result = SmartcardInterop.SCardGetStatusChange(this.readerContext, SmartcardInterop.Infinite, scardstate, scardstate.Length);
                    if (this.cancelToken.IsCancellationRequested || result == SmartcardException.SCardECancelled)
                    {
                        this.logger.Debug("Cancellation requested");
                        break;
                    }

                    if (result == SmartcardException.SCardETimeout)
                    {
                        continue;
                    }

                    this.logger.Debug(DateTime.Now + ": StateChange - result: 0x" + result.ToString("X"));
                    var scardstatelist = scardstate.ToList();

                    // If the service has stopped, then we need to flag all existing cards as removed
                    if (this.HandleStoppedService(scardstatelist, result))
                    {
                        // Need to reset the smartcard context
                        this.ResetContext();
                        continue;
                    }

                    // All other errors, throw an exception
                    if (result != SmartcardException.SCardSuccess)
                    {
                        this.logger.DebugFormat("Failed to get smartcard state: error 0x{0}", result);
                        throw new SmartcardException(result);
                    }

                    // Now deal with the actual smartcard changes
                    var scardChanges = scardstatelist.Where(x => (x.eventState & SmartcardInterop.State.Changed) != 0).ToList();
                    if (scardChanges.Count == 0)
                    {
                        continue;
                    }

                    this.DumpState(scardChanges);

                    this.HandleRemovedReaders(scardChanges, result);
                    this.HandleRemovedCards(scardChanges, result);
                    this.HandleInsertedCards(ref scardChanges, result);

                    this.SaveState(scardChanges);
                }
                catch (SmartcardException ex)
                {
                    if (ex.Status == SmartcardException.SCardEServiceStopped || ex.Status == SmartcardException.SCardENoService)
                    {
                        this.logger.Debug("Smartcard service stopped; resetting context");
                        this.ResetContext();
                        this.currentState.Add(awaitNewReader);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            this.logger.Debug("Monitoring stopped.");
        }

        /// <summary>
        /// Determine which readers have been removed and fire card removed events for those readers
        /// </summary>
        /// <param name="scardState">Smartcard status change list</param>
        /// <param name="fetchStatusResult">Result of last status fetch</param>
        /// <returns>True if handler had events to process, false otherwise</returns>
        private bool HandleStoppedService(List<SmartcardInterop.ScardReaderState> scardState, int fetchStatusResult)
        {
            bool processedEvents = false;

            if (fetchStatusResult == SmartcardException.SCardEServiceStopped || fetchStatusResult == SmartcardException.SCardENoService)
            {
                var removedReader = scardState.Where(x => x.reader != NotificationReader);
                foreach (var reader in removedReader.Where(x => (x.currentState & SmartcardInterop.State.Present) != 0))
                {
                    this.FireCardRemovedEvent(reader);
                }

                processedEvents = true;
            }

            return processedEvents;
        }

        /// <summary>
        /// Determine which readers have been removed and fire card removed events for those readers
        /// </summary>
        /// <param name="scardChanges">Smartcard status change list</param>
        /// <param name="fetchStatusResult">Result of last status fetch</param>
        /// <returns>True if handler had events to process, false otherwise</returns>
        private bool HandleRemovedReaders(List<SmartcardInterop.ScardReaderState> scardChanges, int fetchStatusResult)
        {
            var processedEvents = false;

            var unavailableReaders = scardChanges.Where(x => 
                    (x.eventState & SmartcardInterop.State.Unavailable) == 0 
                    && (x.currentState & SmartcardInterop.State.Unavailable) != 0);

            foreach (var reader in unavailableReaders.Where(x => x.currentState != SmartcardInterop.State.Unaware))
            {
                if (scardChanges.Any(x => x.reader == reader.reader))
                {
                    this.FireCardRemovedEvent(reader);
                    processedEvents = true;
                }
            }

            return processedEvents;
        }

        /// <summary>
        /// Determine which readers have had cards removed and fire card removed events for those readers
        /// </summary>
        /// <param name="scardChanges">Smartcard status change list</param>
        /// <param name="fetchStatusResult">Result of last status fetch</param>
        /// <returns>True if handler had events to process, false otherwise</returns>
        private bool HandleRemovedCards(List<SmartcardInterop.ScardReaderState> scardChanges, int fetchStatusResult)
        {
            var processedEvents = false;

            var readersWithoutCards = scardChanges.Where(x => 
                    (x.eventState & SmartcardInterop.State.Present) == 0 
                    && (x.currentState & SmartcardInterop.State.Present) != 0);

            foreach (var reader in readersWithoutCards.Where(x => x.currentState != SmartcardInterop.State.Unaware))
            {
                if (scardChanges.Any(x => x.reader == reader.reader))
                {
                    this.FireCardRemovedEvent(reader);
                    processedEvents = true;
                }
            }

            return processedEvents;
        }

        /// <summary>
        /// Determine which readers have had cards inserted and fire card removed events for those readers
        /// </summary>
        /// <param name="scardChanges">Smartcard status change list</param>
        /// <param name="fetchStatusResult">Result of last status fetch</param>
        /// <returns>True if handler had events to process, false otherwise</returns>
        private bool HandleInsertedCards(ref List<SmartcardInterop.ScardReaderState> scardChanges, int fetchStatusResult)
        {
            bool processedEvents = false;

            // Are any of the new cards due to new readers?
            if (scardChanges.Any(x => x.reader == NotificationReader))
            {
                var readers = this.FetchReaders();
                var newReaders = readers.Where(x => !this.readerNames.Contains(x)).ToList();
                var newReaderState = this.FetchState(newReaders);

                this.readerNames.AddRange(newReaders);
                scardChanges.AddRange(newReaderState);
            }

            var readersWithCards = scardChanges.Where(x => 
                    (x.eventState & SmartcardInterop.State.Present) != 0 
                    && (x.currentState & SmartcardInterop.State.Present) == 0);

            foreach (var reader in readersWithCards)
            {
                if (scardChanges.Any(x => x.reader == reader.reader))
                {
                    this.FireCardPresentEvent(reader);
                    processedEvents = true;
                }
            }

            return processedEvents;
        }

        /// <summary>
        /// Create an unknown state list for named readers
        /// </summary>
        /// <remarks>
        /// Sets the currentState and eventState to 'Unaware' for each reader
        /// </remarks>
        /// <param name="readerNames">List of reader names for which to create initial state</param>
        /// <returns>List of unknown reader states</returns>
        private IList<SmartcardInterop.ScardReaderState> CreatePendingReaderState(IList<string> readerNames)
        {
            var scardstatelist = new List<SmartcardInterop.ScardReaderState>();

            foreach (var reader in readerNames)
            {
                var state = this.CreatePendingReaderState(reader);
                scardstatelist.Add(state);
            }

            return scardstatelist;
        }

        /// <summary>
        /// Create an unknown state list for a named reader
        /// </summary>
        /// <remarks>
        /// Sets the currentState and eventState to 'Unaware' for the reader
        /// </remarks>
        /// <param name="readerName">Reader name for which to create initial state</param>
        /// <returns>List of unknown reader states</returns>
        private SmartcardInterop.ScardReaderState CreatePendingReaderState(string readerName)
        {
            var state = new SmartcardInterop.ScardReaderState
            {
                reader = readerName,
                currentState = SmartcardInterop.State.Unaware,
                eventState = 0,
                atrLength = 0
            };

            return state;
        }

        /// <summary>
        /// Locate the smartcard type with the identified ATR bytes
        /// </summary>
        /// <param name="atr">ATR byte array</param>
        /// <returns>Cardname matching the ATR</returns>
        private string FindCardWithAtr(byte[] atr)
        {
            var cardLen = 1024;
            var cards = new char[cardLen];
            var result = SmartcardInterop.SCardListCardsW(IntPtr.Zero, atr, IntPtr.Zero, 0, cards, out cardLen);
            if (result != SmartcardException.SCardSuccess)
            {
                this.logger.DebugFormat("Failed to find smartcard by ATR: error 0x{0}", result);
                throw new SmartcardException(result);
            }

            var card = SmartcardInterop.MultiStringToArray(cards);
            if (card.Count == 0)
            {
                throw new SmartcardException(SmartcardException.SCardECardUnsupported);
            }

            if (card.Count > 1)
            {
                throw new SmartcardException(SmartcardException.SCardEUnexpected);
            }

            return card[0];
        }

        /// <summary>
        /// Dump a list of states to the debug console
        /// </summary>
        /// <param name="states">States to dump</param>
        private void DumpState(IEnumerable<SmartcardInterop.ScardReaderState> states)
        {
            if (!this.logger.IsDebugEnabled)
            {
                return;
            }

            foreach (var s in states)
            {
                this.logger.DebugFormat("{0}: {1} => {2}", s.reader, s.currentState, s.eventState);
            }
        }
    }
}
