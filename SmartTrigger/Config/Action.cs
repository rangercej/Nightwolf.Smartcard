namespace Nightwolf.SmartTrigger.Config
{
    using System;
    using System.Configuration;

    internal class Action : ConfigurationElement
    {
        [Flags]
        internal enum SmartcardAction
        {
            Insert,

            Remove
        };

        [ConfigurationProperty("on", IsRequired = false)]
        internal string On
        {
            get => (string)this["on"];
            set => this["on"] = value;
        }

        internal SmartcardAction OnEvent
        {
            get
            {
                SmartcardAction result = 0;
                var parts = this.On.Split(',');
                foreach (var p in parts)
                {
                    switch (p)
                    {
                        case "insert":
                            result |= SmartcardAction.Insert;
                            break;
                        case "remove":
                            result |= SmartcardAction.Remove;
                            break;
                        default:
                            throw new ConfigurationErrorsException("Invalid action");
                    }
                }

                return result;
            }
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

        [ConfigurationProperty("requirepin", IsRequired = false)]
        internal bool PinRequired
        {
            get => bool.Parse((string)this["requirepin"]);
        }
    }
}
