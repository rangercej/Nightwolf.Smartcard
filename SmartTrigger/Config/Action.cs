namespace Nightwolf.SmartTrigger.Config
{
    using System.Configuration;

    internal class Action : ConfigurationElement
    {
        [ConfigurationProperty("on", IsRequired = false)]
        internal string On
        {
            get => (string)this["on"];
            set => this["on"] = value;
        }

        [ConfigurationProperty("target", IsRequired = false)]
        internal string Target
        {
            get => (string)this["target"];
            set => this["target"] = value;
        }

        [ConfigurationProperty("parameters", IsRequired = false)]
        internal ParameterCollection Parameters
        {
            get => (ParameterCollection)this["parameters"];
        }
    }
}
