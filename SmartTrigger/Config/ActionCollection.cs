using System.Collections.Generic;
using System.Configuration;

namespace Nightwolf.SmartTrigger.Config
{
    [ConfigurationCollection(typeof(Certificate), AddItemName="action", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    internal class ActionCollection : ConfigurationElementCollection
    {
        public Action this[int index]
        {
            get { return (Action) BaseGet(index);  }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(Action certConfig)
        {
            BaseAdd(certConfig);
        }

        public void Clear(Action certConfig)
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Action();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Action) element).Target;
        }

        public void Remove(Action certConfig)
        {
            BaseRemove(certConfig.Target);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public new IEnumerator<Action> GetEnumerator()
        {
            var count = base.Count;
            for (var i = 0; i < count; i++)
            {
                yield return (Action)base.BaseGet(i);
            }
        }
    }
}
