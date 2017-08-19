using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartcard
{
    public class SmartcardEventArgs : EventArgs
    {
        public readonly Smartcard SmartCard;

        public readonly string ReaderName;

        public SmartcardEventArgs(Smartcard scard, string reader)
        {
            this.SmartCard = scard;
            this.ReaderName = reader;
        }
    }
}
