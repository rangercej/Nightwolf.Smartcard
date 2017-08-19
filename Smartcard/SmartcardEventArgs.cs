using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartcard
{
    public class SmartcardEventArgs : EventArgs
    {
        public readonly Smartcard smartCard;

        public SmartcardEventArgs(Smartcard scard)
        {
            this.smartCard = scard;
        }
    }
}
