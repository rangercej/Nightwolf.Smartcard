using System.Collections.Generic;
using System.Configuration;

namespace Nightwolf.SmartTrigger.Config
{
    [ConfigurationCollection(typeof(Certificate), AddItemName="certificate", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    internal class CertificateCollection : ConfigurationElementCollection, IEnumerable<Certificate>
    {
        public Certificate this[int index]
        {
            get { return (Certificate) BaseGet(index);  }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(Certificate certConfig)
        {
            BaseAdd(certConfig);
        }

        public void Clear(Certificate certConfig)
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Certificate();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Certificate) element).Subject;
        }

        public void Remove(Certificate certConfig)
        {
            BaseRemove(certConfig.Subject);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public new IEnumerator<Certificate> GetEnumerator()
        {
            var count = base.Count;
            for (var i = 0; i < count; i++)
            {
                yield return (Certificate) base.BaseGet(i);
            }
        }
    }
}
