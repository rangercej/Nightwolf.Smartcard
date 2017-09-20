namespace Nightwolf.SmartTrigger.Config
{
    using System.Configuration;

    internal class Certificate : ConfigurationElement
    {
        [ConfigurationProperty("subject", IsRequired = true)]
        internal string Subject
        {
            get => (string)this["subject"];
            set => this["subject"] = value;
        }

        [ConfigurationProperty("actions", IsRequired = false)]
        internal ActionCollection Actions
        {
            get => (ActionCollection)this["actions"];
        }
    }
}
