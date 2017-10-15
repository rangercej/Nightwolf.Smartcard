namespace Nightwolf.SmartTrigger.Action
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using System.Security.Cryptography.X509Certificates;

    using Common.Logging;

    using Nightwolf.Smartcard;

    internal sealed class Bitlocker : ActionBase
    {
        /// <summary>
        /// Log manager
        /// </summary>
        private readonly ILog logger = LogManager.GetLogger(typeof(Bitlocker));

        internal override bool PerformInsertAction(Smartcard scard, X509Certificate2 targetCert, string pin, IList<Config.Parameter> parameters)
        {
            this.logger.InfoFormat("Bitlocker unlocking for certifiate {0}, thumbprint {1}", targetCert.Subject, targetCert.Thumbprint);

            var targetDrive = parameters.Where(x => x.Name == "target").First().Value.ToUpper();
            using (var vol = this.GetTargetVolume(targetDrive))
            {
                if (vol == null)
                {
                    return false;
                }

                var driveStatus = (uint)vol["ProtectionStatus"];
                if (driveStatus == 2)
                {
                    this.logger.DebugFormat("Unlocking drive {0}", targetDrive);
                    var result = vol.InvokeMethod(
                        "UnlockWithCertificateThumbprint",
                        new object[] { targetCert.Thumbprint, pin });
                }
            }

            return true;
        }

        internal override bool PerformRemoveAction(Smartcard scard, IList<Config.Parameter> parameters)
        {
            this.logger.Info("Locking drive");

            var targetDrive = parameters.Where(x => x.Name == "target").First().Value.ToUpper();
            using (var vol = this.GetTargetVolume(targetDrive))
            {
                vol.InvokeMethod("Lock", null);
            }

            return true;
        }

        private ManagementObject GetTargetVolume(string targetDrive)
        {
            var wmiPath = new ManagementPath
                              {
                                  NamespacePath = "\\ROOT\\CIMV2\\Security\\MicrosoftVolumeEncryption",
                                  ClassName = "Win32_EncryptableVolume"
                              };

            var wmiScope = new ManagementScope(wmiPath, new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate });
            var wmi = new ManagementClass(wmiScope, wmiPath, new ObjectGetOptions());
            foreach (ManagementObject vol in wmi.GetInstances())
            {
                var driveLetter = vol["DriveLetter"].ToString().ToUpper();
                if (driveLetter == targetDrive)
                {
                    return vol;
                }
            }

            return null;
        }
    }
}
