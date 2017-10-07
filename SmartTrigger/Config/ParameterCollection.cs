using System.Collections.Generic;
using System.Configuration;

namespace Nightwolf.SmartTrigger.Config
{
    [ConfigurationCollection(typeof(Certificate), AddItemName="add", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    internal class ParameterCollection : ConfigurationElementCollection, IEnumerable<Parameter>
    {
        public Parameter this[int index]
        {
            get { return (Parameter) BaseGet(index);  }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(Parameter certConfig)
        {
            BaseAdd(certConfig);
        }

        public void Clear(Parameter certConfig)
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Parameter();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Parameter) element).Name;
        }

        public void Remove(Parameter certConfig)
        {
            BaseRemove(certConfig.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public new IEnumerator<Parameter> GetEnumerator()
        {
            var count = base.Count;
            for (var i = 0; i < count; i++)
            {
                yield return (Parameter)base.BaseGet(i);
            }
        }

    }
}
