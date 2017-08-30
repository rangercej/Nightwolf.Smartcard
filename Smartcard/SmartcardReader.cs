using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smartcard
{
    public sealed class SmartcardReader : IDisposable
    {
        public event EventHandler OnCardInserted;
        public event EventHandler OnCardRemoved;

        private IntPtr context = IntPtr.Zero;

        private List<string> ReaderNames = new List<string>();
        private List<string> CardNames = new List<string>();

        private List<SmartcardInterop.ScardReaderState> CurrentState = null;

        private Task monitorTask;
        private CancellationToken cancelToken;

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Instantiate the smartcard reader class
        /// </summary>
        public SmartcardReader()
        {
            this.monitorTask = null;
            ResetContext();
        }

        /// <summary>
        /// Start monitoring for smartcard changes
        /// </summary>
        /// <param name="ct">Cancellation token to stop monitoring</param>
        public void StartMonitoring(CancellationToken ct)
        {
            if (this.monitorTask != null)
            {
                return;
            }

            if (CurrentState != null)
            {
                var readersWithCards = CurrentState.Where(x => (x.eventState & SmartcardInterop.State.Present) != 0);
                foreach (var reader in readersWithCards)
                {
                    FireCardPresentEvent(reader);
                }
            }

            this.cancelToken.Register(StopMonitoring);
            this.monitorTask = Task.Factory.StartNew(WaitForReaderStateChange);
        }

        /// <summary>
        /// Stop the smartcard monitor
        /// </summary>
        private void StopMonitoring()
        {
            if (this.monitorTask == null)
            {
                return;
            }

            SmartcardInterop.SCardCancel(this.context);
            this.monitorTask.Wait();
            this.monitorTask = null;
        }

        /// <summary>
        /// Create a new smartcard tracking context
        /// </summary>
        private void ResetContext()
        {
            if (this.context != IntPtr.Zero)
            {
                SmartcardInterop.SCardReleaseContext(this.context);
            }

            var result = SmartcardInterop.SCardEstablishContext((uint)SmartcardInterop.Scope.User, IntPtr.Zero, IntPtr.Zero, out context);
            if (result != SmartcardException.SCardSuccess)
            {
                throw new SmartcardException(result);
            }

            this.CardNames = FetchCards();
            this.ReaderNames = FetchReaders();
            this.CurrentState = FetchState(this.ReaderNames);
        }

        /// <summary>
        /// Call the card detected delegate for each changed reader
        /// </summary>
        /// <param name="state">State class containing the smartcard reader name</param>
        private void FireCardPresentEvent(SmartcardInterop.ScardReaderState state)
        {
            var cardname = FindCardWithAtr(state.atr);
            var scard = new Smartcard(state.reader, cardname);
            var args = new SmartcardEventArgs(scard, state.reader);

            OnCardInserted(this, args);
        }

        /// <summary>
        /// Call the card removed delegate for each changed reader
        /// </summary>
        /// <param name="state">State class containing the smartcard reader name</param>
        private void FireCardRemovedEvent(SmartcardInterop.ScardReaderState state)
        {
            var args = new SmartcardEventArgs(null, state.reader);

            OnCardRemoved(this, args);
        }

        /// <summary>
        /// Find all attached smartcard readers
        /// </summary>
        /// <returns>List of attached readers</returns>
        private List<string> FetchReaders()
        {
            uint readerLen = 1024;
            var readers = new char[1024];

            var result = SmartcardInterop.SCardListReadersW(context, null, readers, out readerLen);
            if (result != SmartcardException.SCardSuccess && result != SmartcardException.SCardENoReadersAvailable)
            {
                throw new SmartcardException(result);
            }

            var r = MultiStringToArray(readers);
            return r.ToList();
        }

        /// <summary>
        /// Find all supported smartcard types
        /// </summary>
        /// <returns>List of supported card types</returns>
        private List<string> FetchCards()
        {
            uint cardLen = 16384;
            var cards = new char[cardLen];

            var result = SmartcardInterop.SCardListCardsW(context, null, IntPtr.Zero, 0, cards, out cardLen);
            if (result != SmartcardException.SCardSuccess)
            {
                throw new SmartcardException(result);
            }

            var c = MultiStringToArray(cards);
            return c.ToList();
        }

        /// <summary>
        /// Fetch the state of named smartcard readers
        /// </summary>
        /// <returns>List of readers for which to fetch state</returns>
        private List<SmartcardInterop.ScardReaderState> FetchState(IList<string> readerNames)
        {
            IList<SmartcardInterop.ScardReaderState> scardstatelist = null;

            if (this.CurrentState == null)
            {
                this.CurrentState = new List<SmartcardInterop.ScardReaderState>();
            }

            var state = this.CurrentState.Where(x => readerNames.Contains(x.reader)).ToList();
            if (state == null || state.Count == 0)
            { 
                scardstatelist = CreatePendingReaderState(readerNames);
            }

            if (scardstatelist == null)
            {
                return null;
            }

            if (readerNames.Count != scardstatelist.Count)
            {
                foreach (var reader in readerNames)
                {
                    if (!scardstatelist.Any(x => x.reader.Equals(reader)))
                    {
                        scardstatelist.Add(CreatePendingReaderState(reader));
                    }
                }
            }

            SmartcardInterop.ScardReaderState[] scardstate = scardstatelist.ToArray();
            if (readerNames.Count > 0)
            {
                var d = ArrayToMultiString(this.CardNames);

                var result = SmartcardInterop.SCardLocateCards(context, d, scardstate, Convert.ToUInt32(scardstate.Length));
                if (result != SmartcardException.SCardSuccess)
                {
                    throw new SmartcardException(result);
                }
            }

            return scardstate.ToList();
        }

        /// <summary>
        /// Update cached current state for each reader with the latest obtained state.
        /// </summary>
        /// <param name="newState">List of states to update</param>
        private void SaveState(List<SmartcardInterop.ScardReaderState> newState)
        {
            this.CurrentState.Clear();
            foreach (var state in newState)
            {
                var s = state;
                s.currentState = s.eventState;
                s.eventState = 0;
                this.CurrentState.Add(s);
            }
        }

        /// <summary>
        /// Monitor for smartcard change events and fire events on detected changes
        /// </summary>
        private void WaitForReaderStateChange()
        {
            const string NotificationReader = @"\\?PnP?\Notification";
            var state = new SmartcardInterop.ScardReaderState
            {
                reader = NotificationReader,
                currentState = 0,
                eventState = 0,
                atrLength = 0
            };

            this.CurrentState.Add(state);

            int i = 0;
            uint result = 0;
            while (!this.cancelToken.IsCancellationRequested)
            {
                i++;
                System.Diagnostics.Debug.Print("StateChange - start: " + i.ToString() + "; last status = 0x" + result.ToString("X"));
                SmartcardInterop.ScardReaderState[] scardstate = null;

                scardstate = this.CurrentState.ToArray();
                result = SmartcardInterop.SCardGetStatusChange(context, SmartcardInterop.Infinite, scardstate, Convert.ToUInt32(scardstate.Length));
                if (this.cancelToken.IsCancellationRequested || result == SmartcardException.SCardECancelled)
                {
                    return;
                }

                if (result == SmartcardException.SCardETimeout)
                {
                    continue;
                }

                var scardstatelist = scardstate.ToList();

                // EServiceStopped thrown if last reader removed from machine.
                if (result == SmartcardException.SCardEServiceStopped || result == SmartcardException.SCardENoService)
                {
                    var removedReader = scardstatelist.Where(x => x.reader != NotificationReader);
                    foreach (var reader in removedReader)
                    {
                        FireCardRemovedEvent(reader);
                    }

                    // Need to reset the smartcard context
                    ResetContext();
                    continue;
                }

                if (result != SmartcardException.SCardSuccess)
                {
                    System.Diagnostics.Debug.Print("Exception happened: " + result);
                    throw new SmartcardException(result);
                }

                var scardChanges = scardstatelist.Where(x => (x.eventState & SmartcardInterop.State.Changed) != 0).ToList();
                if (scardChanges.Count == 0)
                {
                    continue;
                }

                if (scardChanges.Any(x => x.reader == NotificationReader))
                {
                    var readers = FetchReaders();
                    var newReaders = readers.Where(x => !this.ReaderNames.Contains(x)).ToList();
                    var newReaderState = FetchState(newReaders);

                    this.ReaderNames.AddRange(newReaders);
                    scardChanges.AddRange(newReaderState);
                    scardstatelist.AddRange(newReaderState);
                }

                var readersWithCards = scardChanges.Where(x => (x.eventState & SmartcardInterop.State.Present) != 0).ToList();
                foreach (var reader in readersWithCards)
                {
                    if (scardChanges.Any(x => x.reader == reader.reader))
                    {
                        FireCardPresentEvent(reader);
                    }
                }

                var readersWithoutCards = scardChanges.Where(x => (x.eventState & SmartcardInterop.State.Empty) != 0).ToList();
                foreach (var reader in readersWithoutCards)
                {
                    if (scardChanges.Any(x => x.reader == reader.reader))
                    {
                        if (reader.currentState != SmartcardInterop.State.Unaware)
                        {
                            FireCardRemovedEvent(reader);
                        }
                    }
                }

                SaveState(scardstatelist);
            }
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
                var state = CreatePendingReaderState(reader);
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
        /// <param name="readerNames">Reader name for which to create initial state</param>
        /// <returns>List of unknown reader states</returns>
        private SmartcardInterop.ScardReaderState CreatePendingReaderState (string readerName)
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
            uint cardLen = 1024;
            var cards = new char[cardLen];
            var result = SmartcardInterop.SCardListCardsW(IntPtr.Zero, atr, IntPtr.Zero, 0, cards, out cardLen);
            if (result != SmartcardException.SCardSuccess)
            {
                throw new SmartcardException(result);
            }

            var card = MultiStringToArray(cards);
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
        /// Convert a C-style null seperated, double-null terminated string to a c# list of strings
        /// </summary>
        /// <param name="multistring">C-style multistring to convert</param>
        /// <returns>List of strings obtained from the multistring</returns>
        private static IList<string> MultiStringToArray(char[] multistring)
        {
            List<string> stringList = new List<string>();
            int i = 0;
            while (i < multistring.Length)
            {
                int j = i;
                if (multistring[j++] == '\0')
                {
                    break;
                }

                while (j < multistring.Length)
                {
                    if (multistring[j++] == '\0')
                    {
                        stringList.Add(new string(multistring, i, j - i - 1));
                        i = j;
                        break;
                    }
                }
            }

            return stringList;
        }

        /// <summary>
        /// Convert a list of strings to a C-style null-seperated, double-null terminated string
        /// </summary>
        /// <param name="stringlist">List of strings to convert</param>
        /// <returns>C-style multistring</returns>
        private static string ArrayToMultiString(IList<string> stringlist)
        {
            var sb = new StringBuilder();

            if (stringlist == null)
            {
                return sb.ToString();
            }

            foreach (var s in stringlist)
            {
                sb.Append(s);
                sb.Append('\0');
            }

            return sb.ToString();
        }

        #region IDisposable Support
        /// <summary>
        /// Free unmanaged resources
        /// </summary>
        /// <param name="disposing">Called from Dispose() flag</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                if (this.context != IntPtr.Zero)
                {
                    SmartcardInterop.SCardReleaseContext(this.context);
                }
                
                disposedValue = true;
            }
        }

        /// <summary>
        /// Free unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
