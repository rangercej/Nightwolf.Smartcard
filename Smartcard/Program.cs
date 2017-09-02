using System;
using System.Threading;

namespace Smartcard
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var x = new SmartcardReader())
            {
                x.OnCardInserted += CardInserted;
                x.OnCardRemoved += CardRemoved;

                var cts = new CancellationTokenSource();

                x.StartMonitoring(cts.Token);
                Thread.Sleep(10000);
                Console.WriteLine("Stopping");
                cts.Cancel();

                Console.ReadLine();
            }
        }

        private static void CardInserted(object sender, EventArgs args)
        {
            using (var scard = ((SmartcardEventArgs)args).SmartCard)
            {
                foreach (var cert in scard.CertificateStore.Certificates)
                {
                    System.Console.WriteLine(cert.Subject + ": " + cert.NotAfter);
                }

                //scard.UnlockCard("4971");
            }
        }

        private static void CardRemoved(object sender, EventArgs args)
        {
            var scard = ((SmartcardEventArgs)args).ReaderName;

            System.Console.WriteLine("Card removed " + scard);
        }
    }
}
