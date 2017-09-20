namespace Nightwolf.SmartTrigger.Config
{
    using System.Configuration;

    internal class Parameter : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = false)]
        internal string Name
        {
            get => (string)this["name"];
            set => this["name"] = value;
        }

        [ConfigurationProperty("value", IsRequired = false)]
        internal string Value
        {
            get => (string)this["value"];
            set => this["value"] = value;
        }
    }
}
