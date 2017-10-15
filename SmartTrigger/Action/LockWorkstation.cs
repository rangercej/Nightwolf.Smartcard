namespace Nightwolf.SmartTrigger.Action
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using Common.Logging;
    using Nightwolf.SmartTrigger.Config;

    internal class LockWorkstation : ActionBase
    {
        /// <summary>
        /// Log manager
        /// </summary>
        private readonly ILog logger = LogManager.GetLogger(typeof(Bitlocker));

        /// <summary>
        /// Lock the workstation on smartcard remove
        /// </summary>
        /// <param name="scard">Smartcard removed</param>
        /// <param name="parameters">Action parameters</param>
        /// <returns>True on success, false otherwise</returns>
        /// <inheritdoc cref="ActionBase.PerformRemoveAction"/>
        internal override bool PerformRemoveAction(Nightwolf.Smartcard.Smartcard scard, IList<Parameter> parameters)
        {
            this.logger.Debug("Locking workstation");
            return LockWorkStation();
        }

        /// <summary>
        /// Import the Win32 lockworkstation method
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LockWorkStation();
    }
}
