using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartcard
{
    class SmartcardReader
    {
        IntPtr context;

        public SmartcardReader()
        {
            var result = SmartcardInterop.SCardEstablishContext((uint)SmartcardInterop.Scope.User, IntPtr.Zero, IntPtr.Zero, out context);

            uint readerLen = 1024;
            var readers = new char[1024];

            result = SmartcardInterop.SCardListReadersW(context, null, readers, out readerLen);
            var r = MultiStringToArray(readers);
            System.Diagnostics.Debug.Print(readerLen.ToString());

            uint cardLen = 16384;
            var cards = new char[cardLen];

            result = SmartcardInterop.SCardListCardsW(context, null, IntPtr.Zero, 0, cards, out cardLen);
            var c = MultiStringToArray(cards);
            System.Diagnostics.Debug.Print(cardLen.ToString());

            var d = ArrayToMultiString(c);
            var scardstate = new SmartcardInterop.ScardReaderState[r.Count];

            for (int i = 0; i < r.Count; i++)
            {
                scardstate[i].reader = r[i];
                scardstate[i].currentState = SmartcardInterop.State.Unaware;
            }

            result = SmartcardInterop.SCardLocateCards(context, d, scardstate, Convert.ToUInt32(r.Count));
            System.Diagnostics.Debug.Print(scardstate[0].currentState.ToString());

            cardLen = 16384;
            cards = new char[cardLen];
            result = SmartcardInterop.SCardListCardsW(IntPtr.Zero, scardstate[0].atr, IntPtr.Zero, 0, cards, out cardLen);
            var card = MultiStringToArray(cards);
            System.Diagnostics.Debug.Print(result.ToString());

            var provider = new StringBuilder();
            uint len = 256;
            provider.EnsureCapacity(256);
            result = SmartcardInterop.SCardGetCardTypeProviderNameW(IntPtr.Zero, card[0], SmartcardInterop.Provider.Csp, provider, out len);

            var container = @"\\.\" + scardstate[0].reader + @"\";
            IntPtr cctx;
            var success = SmartcardInterop.CryptAcquireContextW(out cctx, container, provider.ToString(), SmartcardInterop.CryptoProvider.RsaFull, 0);

            uint buflen = 1024;
            var buffer = new byte[buflen];

            var containers = new List<string>();

            success = SmartcardInterop.CryptGetProvParam(cctx, SmartcardInterop.ProviderParamGet.EnumContainters, buffer, out buflen, SmartcardInterop.ProviderParamFlags.CryptFirst);
            while (success)
            {
                containers.Add(Encoding.ASCII.GetString(buffer, 0, Convert.ToInt32(buflen)));
                success = SmartcardInterop.CryptGetProvParam(cctx, SmartcardInterop.ProviderParamGet.EnumContainters, buffer, out buflen, SmartcardInterop.ProviderParamFlags.CryptNext);
            }

            foreach (var ct in containers)
            {
                var containerPath = container + ct;

                IntPtr ctx;
                success = SmartcardInterop.CryptAcquireContextW(out ctx, containerPath, provider.ToString(), SmartcardInterop.CryptoProvider.RsaFull, 0);

                IntPtr keyctx;
                success = SmartcardInterop.CryptGetUserKey(ctx, SmartcardInterop.KeyFlags.AtKeyExchange, out keyctx);

                uint certLen;
                success = SmartcardInterop.CryptGetKeyParam(keyctx, SmartcardInterop.KeyParam.KpCertificate, null, out certLen, 0);

                var cert = new byte[certLen];
                success = SmartcardInterop.CryptGetKeyParam(keyctx, SmartcardInterop.KeyParam.KpCertificate, cert, out certLen, 0);

                var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(cert);
                System.Diagnostics.Debug.Print(x509.Subject);

                SmartcardInterop.CryptDestroyKey(keyctx);
                SmartcardInterop.CryptReleaseContext(ctx,0);
            }

            SmartcardInterop.CryptReleaseContext(cctx, 0);
            SmartcardInterop.SCardReleaseContext(context);
        }

        static IList<string> MultiStringToArray(char[] multistring)
        {
            List<string> stringList = new List<string>();
            int i = 0;
            while (i < multistring.Length)
            {
                int j = i;
                if (multistring[j++] == '\0')
                {
                    break;
                }

                while (j < multistring.Length)
                {
                    if (multistring[j++] == '\0')
                    {
                        stringList.Add(new string(multistring, i, j - i - 1));
                        i = j;
                        break;
                    }
                }
            }

            return stringList;
        }

        static string ArrayToMultiString(IList<string> stringlist)
        {
            var sb = new StringBuilder();

            if (stringlist == null)
            {
                return sb.ToString();
            }

            foreach (var s in stringlist)
            {
                sb.Append(s);
                sb.Append('\0');
            }

            return sb.ToString();
        }
    }
}
