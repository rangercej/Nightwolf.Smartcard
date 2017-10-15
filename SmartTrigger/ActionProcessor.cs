namespace Nightwolf.SmartTrigger
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    using Common.Logging;

    using Nightwolf.SmartTrigger.Action;

    internal sealed class ActionProcessor
    {
        private readonly List<ActionProperties> actions;

        private readonly ManualResetEvent processingCompleted;

        private readonly ReadOnlyDictionary<string, ActionBase> actionHandlers;

        private bool? pinRequired;

        private ILog logger = LogManager.GetLogger(typeof(ActionProcessor));

        internal ActionProcessor()
        {
            this.actions = new List<ActionProperties>();
            this.processingCompleted = new ManualResetEvent(false);
            this.actionHandlers = ActionBase.GetHandlers();
            this.pinRequired = null;
        }

        internal int ActionCount => this.actions.Count;

        internal bool PinRequired
        {
            get
            {
                if (this.pinRequired == null)
                {
                    this.pinRequired = this.actions.Any(x => x.action.PinRequired);
                }

                return this.pinRequired.Value;
            }
        }

        internal void AddAction(Nightwolf.Smartcard.Smartcard scard, string certSubject, Config.Action action)
        {
            if (this.processingCompleted.WaitOne(0))
            {
                throw new OperationCanceledException();
            }

            this.actions.Add(new ActionProperties
                                 {
                                     smartcard = scard,
                                     targetSubject = certSubject,
                                     action = action
                                 });
        }

        internal void AddActions(Nightwolf.Smartcard.Smartcard scard, string certSubject, IEnumerable<Config.Action> actions)
        {
            if (this.processingCompleted.WaitOne(0))
            {
                throw new OperationCanceledException();
            }

            foreach (var act in actions)
            {
                this.AddAction(scard, certSubject, act);
            }
        }

        internal void ProcessInsertActions(string pin)
        {
            if (this.processingCompleted.WaitOne(0))
            {
                throw new OperationCanceledException();
            }

            foreach (var act in this.actions)
            {
                this.logger.DebugFormat("Processing insert action: {0}", act.action.Target);
                if (this.actionHandlers.ContainsKey(act.action.Target))
                {
                    this.logger.DebugFormat("Fire: {0}", act.action.Target);
                    this.actionHandlers[act.action.Target].PerformInsertAction(
                        act.smartcard,
                        act.targetSubject,
                        pin,
                        act.action.Parameters.ToList());
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

            foreach (var act in this.actions)
            {
                this.logger.DebugFormat("Processing insert action: {0}", act.action.Target);
                if (this.actionHandlers.ContainsKey(act.action.Target))
                {
                    this.logger.DebugFormat("Fire: {0}", act.action.Target);
                    this.actionHandlers[act.action.Target].PerformRemoveAction(
                        act.smartcard,
                        act.action.Parameters.ToList());
                }
            }

            this.processingCompleted.Set();
        }

        internal void Reset()
        {
            this.actions.Clear();
            this.processingCompleted.Set();
        }

        internal void Wait()
        {
            this.processingCompleted.WaitOne();
        }

        internal struct ActionProperties
        {
            internal Nightwolf.Smartcard.Smartcard smartcard;

            internal string targetSubject;

            internal Config.Action action;
        }
    }
}
