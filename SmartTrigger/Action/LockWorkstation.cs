using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nightwolf.Smartcard;
using Nightwolf.SmartTrigger.Config;

namespace Nightwolf.SmartTrigger.Action
{
    using System.Runtime.InteropServices;

    using Common.Logging;

    internal class LockWorkstation : ActionBase
    {
        /// <summary>
        /// Log manager
        /// </summary>
        private readonly ILog logger = LogManager.GetLogger(typeof(Bitlocker));

        internal override string ActionId => "lockworkstation";

        internal override bool PerformInsertAction(Smartcard.Smartcard scard, string targetCertSubject, string pin, IList<Parameter> parameters)
        {
            return true;
        }

        internal override bool PerformRemoveAction(Smartcard.Smartcard scard, IList<Parameter> parameters)
        {
            return LockWorkStation();
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LockWorkStation();
    }
}
