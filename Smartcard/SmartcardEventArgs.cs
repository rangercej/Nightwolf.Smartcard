using System;

namespace Nightwolf.Smartcard
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
