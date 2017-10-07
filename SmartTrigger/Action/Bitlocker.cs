namespace Nightwolf.SmartTrigger.Action
{
    using System;
    using System.Collections.Generic;

    using Nightwolf.Smartcard;

    internal sealed class Bitlocker : ActionBase
    {
        internal new const string ActionId = "bitlocker";

        internal override bool PerformInsertAction(Smartcard scard, string targetCertSubject, string pin, IList<Config.Parameter> parameters)
        {
            throw new NotImplementedException();
        }

        internal override bool PerformRemoveAction(Smartcard scard, IList<Config.Parameter> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
