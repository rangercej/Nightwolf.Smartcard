namespace Nightwolf.SmartTrigger
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    using Nightwolf.Smartcard;

    internal class CardMonitor
    {
        /// <summary>Smartcard monitor class</summary>
        private readonly SmartcardMonitor smartcardMonitor;

        /// <summary>Cache of certificate subjects associated with a reader</summary>
        private readonly Dictionary<string, List<string>> certificateCache;

        /// <summary>Smartcard event queue</summary>
        private readonly BlockingCollection<EventParams> eventQueue = new BlockingCollection<EventParams>(new ConcurrentQueue<EventParams>());

        /// <summary>app.config configuration</summary>
        private readonly Config.Smartcard configuration;

        /// <summary>Shutdown token</summary>
        private CancellationToken cancellationToken;

        /// <summary>Form containing get pin field</summary>
        private ScPinWindow requestPinForm;

        internal CardMonitor(CancellationToken ct)
        {
            this.configuration = (Config.Smartcard)ConfigurationManager.GetSection("smartcard");

            this.cancellationToken = ct;

            this.smartcardMonitor = new SmartcardMonitor();
            this.smartcardMonitor.OnCardInserted += this.CardInsertedEventHandler;
            this.smartcardMonitor.OnCardRemoved += this.CardRemovedEventHandler;

            this.certificateCache = new Dictionary<string, List<string>>();
            this.MonitoringStopped = new ManualResetEvent(false);
        }

        protected enum EventType
        {
            Insert,
            Remove
        }

        internal ManualResetEvent MonitoringStopped { get; }

        internal void Start(ScPinWindow pinForm)
        {
            this.requestPinForm = pinForm;
            this.smartcardMonitor.StartMonitoring(this.cancellationToken);
            while (!this.cancellationToken.IsCancellationRequested)
            {
                var evt = this.eventQueue.Take(this.cancellationToken);
                if (evt.Action == EventType.Insert)
                {
                    this.DoCardInserted(evt.EventArgs);
                }
                else if (evt.Action == EventType.Remove)
                {
                    this.DoCardRemoved(evt.EventArgs);
                }
            }

            this.smartcardMonitor.MonitoringStoppedTrigger.WaitOne();
            this.MonitoringStopped.Set();
        }

        /// <summary>
        /// Smartcard inserted handler
        /// </summary>
        /// <param name="e">Smartcard event args</param>
        private void DoCardInserted(SmartcardEventArgs e)
        {
            var processor = new ActionProcessor();
            var certList = new List<string>();

            // Find matching actions
            foreach (var cardCert in e.SmartCard.CertificateStore.Certificates)
            {
                var subj = cardCert.Subject;
                var certActions = this.configuration.Certificates.Where(x => x.Subject == subj).ToList();
                certList.Add(subj);

                foreach (var action in certActions)
                {
                    var insertActions = action.Actions.Where(act => (act.OnEvent & Config.Action.SmartcardAction.Insert) == Config.Action.SmartcardAction.Insert);
                    processor.AddActions(e.SmartCard, cardCert, insertActions);
                }
            }

            this.certificateCache.Add(e.ReaderName, certList);

            // Any actions allocated to this smartcard?
            if (processor.ActionCount == 0)
            {
                return;
            }

            // Do any actions want the PIN?
            if (processor.PinRequired)
            {
                var mre = new ManualResetEvent(false);
                this.requestPinForm.Invoke((MethodInvoker)(() => this.requestPinForm.ShowPinWindow(mre)));
                mre.WaitOne();
                processor.ProcessInsertActions(this.requestPinForm.PinPassback);
            }
            else
            {
                processor.ProcessInsertActions(null);
            }
        }

        /// <summary>
        /// Smartcard removed handler
        /// </summary>
        /// <param name="e">Smartcard event args</param>
        private void DoCardRemoved(SmartcardEventArgs e)
        {
            var processor = new ActionProcessor();
            var certs = this.certificateCache[e.ReaderName];

            // Find matching actions
            foreach (var subj in certs)
            {
                var certActions = this.configuration.Certificates.Where(x => x.Subject == subj).ToList();

                foreach (var action in certActions)
                {
                    var removeActions = action.Actions.Where(act => (act.OnEvent & Config.Action.SmartcardAction.Remove) == Config.Action.SmartcardAction.Remove);
                    processor.AddActions(e.SmartCard, null, removeActions);
                }
            }

            // Any actions allocated to this smartcard?
            if (processor.ActionCount == 0)
            {
                return;
            }

            processor.ProcessRemoveActions();
            this.certificateCache.Remove(e.ReaderName);
        }

        /// <summary>
        /// Receive smartcard insert events and queue them for processing
        /// </summary>
        /// <param name="o">Sender of the event</param>
        /// <param name="e">Smartcard event arguments</param>
        private void CardInsertedEventHandler(object o, SmartcardEventArgs e)
        {
            this.eventQueue.Add(new EventParams {Action = EventType.Insert, EventArgs = e});
        }

        /// <summary>
        /// Receive smartcard remove events and queue them for processing
        /// </summary>
        /// <param name="o">Sender of the event</param>
        /// <param name="e">Smartcard event arguments</param>
        private void CardRemovedEventHandler(object o, SmartcardEventArgs e)
        {
            this.eventQueue.Add(new EventParams { Action = EventType.Remove, EventArgs = e });
        }

        protected struct EventParams
        {
            internal EventType Action;

            internal SmartcardEventArgs EventArgs;
        }
    }
}
