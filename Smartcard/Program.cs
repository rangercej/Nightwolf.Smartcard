using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartcard
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new SmartcardReader();
            x.OnCardInserted += CardInserted;
            x.OnCardRemoved += CardRemoved;

            x.StartMonitoring();

            Console.ReadLine();
        }

        private static void CardInserted(object sender, EventArgs args)
        {
            var scard = ((SmartcardEventArgs)args).SmartCard;

            foreach (var cert in scard.Certificates)
            {
                System.Console.WriteLine(cert.Certificate.Subject);
            }
        }

        private static void CardRemoved(object sender, EventArgs args)
        {
            var scard = ((SmartcardEventArgs)args).ReaderName;

            System.Console.WriteLine("Card removed " + scard);
        }
    }
}
