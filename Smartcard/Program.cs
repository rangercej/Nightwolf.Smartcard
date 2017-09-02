using System;
using System.Threading;
using System.Security.Cryptography;

namespace Nightwolf.Smartcard
{
    class Program
    {
        static string s = "Hello World!";

        static void Main(string[] args)
        {
            using (var x = new SmartcardReader())
            {
                x.OnCardInserted += CardInserted;
                x.OnCardRemoved += CardRemoved;

                var cts = new CancellationTokenSource();

                x.StartMonitoring(cts.Token);
                //Thread.Sleep(10000);
                //Console.WriteLine("Stopping");
                //cts.Cancel();

                Console.ReadLine();
            }
        }

        private static void CardInserted(object sender, EventArgs args)
        {
            using (var scard = ((SmartcardEventArgs)args).SmartCard)
            {
                var pin = "123456";
                var spin = new System.Security.SecureString();
                foreach (var ch in pin)
                {
                    spin.AppendChar(ch);
                }

                foreach (var cert in scard.CertificateStore.Certificates)
                {
                    Console.WriteLine(cert.Subject + ": " + cert.NotAfter);
                }

                var c = scard.CertificateStore.Certificates[0];
                var bytes = System.Text.Encoding.ASCII.GetBytes(s);

                scard.UnlockCard(pin);
                
                var encryptor = (RSACryptoServiceProvider)c.PublicKey.Key;
                var decryptor = (RSACryptoServiceProvider)c.PrivateKey;

                var encrypt = encryptor.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
                var decrypt = decryptor.Decrypt(encrypt, RSAEncryptionPadding.Pkcs1);
           
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(decrypt));
            }
        }

        private static void CardRemoved(object sender, EventArgs args)
        {
            var scard = ((SmartcardEventArgs)args).ReaderName;

            Console.WriteLine("Card removed " + scard);
        }
    }
}
