﻿using System;
using System.Threading;

namespace Smartcard
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new SmartcardReader();
            x.OnCardInserted += CardInserted;
            x.OnCardRemoved += CardRemoved;

            var cts = new CancellationTokenSource();

            x.StartMonitoring(cts.Token);
            Thread.Sleep(10000);
            Console.WriteLine("Stopping");
            cts.Cancel();

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
