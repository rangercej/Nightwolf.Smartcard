namespace Nightwolf.SmartTrigger.Config
{
    using System.Configuration;

    internal class Smartcard : ConfigurationSection
    {
        [ConfigurationProperty("certificates")]
        internal CertificateCollection Certificates
        {
            get => (CertificateCollection)this["certificates"];
        }
    }
}
