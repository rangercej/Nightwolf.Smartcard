namespace Nightwolf.SmartTrigger
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    using Common.Logging;

    using Nightwolf.SmartTrigger.Action;

    internal sealed class ActionProcessor
    {
        private readonly List<ActionProperties> targetActions;

        private readonly ManualResetEvent processingCompleted;

        private readonly ReadOnlyDictionary<string, ActionBase> actionHandlers;

        private readonly ILog logger = LogManager.GetLogger(typeof(ActionProcessor));

        private bool? pinRequired;

        internal ActionProcessor()
        {
            this.targetActions = new List<ActionProperties>();
            this.processingCompleted = new ManualResetEvent(false);
            this.actionHandlers = ActionBase.GetHandlers();
            this.pinRequired = null;
        }

        internal int ActionCount => this.targetActions.Count;

        internal bool PinRequired
        {
            get
            {
                if (this.pinRequired == null)
                {
                    this.pinRequired = this.targetActions.Any(x => x.Action.PinRequired);
                }

                return this.pinRequired.Value;
            }
        }

        internal void AddAction(Smartcard.Smartcard scard, X509Certificate2 cert, Config.Action action)
        {
            if (this.processingCompleted.WaitOne(0))
            {
                throw new OperationCanceledException();
            }

            this.targetActions.Add(new ActionProperties
                                 {
                                     TargetSmartcard = scard,
                                     TargetCertificate = cert,
                                     Action = action
                                 });
        }

        internal void AddActions(Smartcard.Smartcard scard, X509Certificate2 cert, IEnumerable<Config.Action> actions)
        {
            if (this.processingCompleted.WaitOne(0))
            {
                throw new OperationCanceledException();
            }

            foreach (var act in actions)
            {
                this.AddAction(scard, cert, act);
            }
        }

        internal void ProcessInsertActions(string pin)
        {
            if (this.processingCompleted.WaitOne(0))
            {
                throw new OperationCanceledException();
            }

            foreach (var act in this.targetActions)
            {
                this.logger.DebugFormat("Processing insert action: {0}", act.Action.Target);
                if (this.actionHandlers.ContainsKey(act.Action.Target))
                {
                    this.logger.DebugFormat("Fire: {0}", act.Action.Target);
                    this.actionHandlers[act.Action.Target].PerformInsertAction(
                        act.TargetSmartcard,
                        act.TargetCertificate,
                        pin,
                        act.Action.Parameters.ToList());
                }
            }

            this.processingCompleted.Set();
        }

        internal void ProcessRemoveActions()
        {
            if (this.processingCompleted.WaitOne(0))
            {
                throw new OperationCanceledException();
            }

            foreach (var act in this.targetActions)
            {
                this.logger.DebugFormat("Processing insert action: {0}", act.Action.Target);
                if (this.actionHandlers.ContainsKey(act.Action.Target))
                {
                    this.logger.DebugFormat("Fire: {0}", act.Action.Target);
                    this.actionHandlers[act.Action.Target].PerformRemoveAction(
                        act.TargetSmartcard,
                        act.Action.Parameters.ToList());
                }
            }

            this.processingCompleted.Set();
        }

        internal void Reset()
        {
            this.targetActions.Clear();
            this.processingCompleted.Set();
        }

        internal void Wait()
        {
            this.processingCompleted.WaitOne();
        }

        internal struct ActionProperties
        {
            internal Smartcard.Smartcard TargetSmartcard;

            internal X509Certificate2 TargetCertificate;

            internal Config.Action Action;
        }
    }
}
