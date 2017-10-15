namespace Nightwolf.SmartTrigger.Action
{
    using System;
    using System.Collections.Generic;

    using Common.Logging;

    using Nightwolf.Smartcard;

    internal sealed class Bitlocker : ActionBase
    {
        /// <summary>
        /// Log manager
        /// </summary>
        private readonly ILog logger = LogManager.GetLogger(typeof(Bitlocker));

        internal Bitlocker()
            : base("bitlocker")
        {
        }

        internal override bool PerformInsertAction(Smartcard scard, string targetCertSubject, string pin, IList<Config.Parameter> parameters)
        {
            this.logger.Debug("Firing insert action bitlocker");

            return true;
        }

        internal override bool PerformRemoveAction(Smartcard scard, IList<Config.Parameter> parameters)
        {
            this.logger.Debug("Firing remove action bitlocker");

            return true;
        }
    }
}
