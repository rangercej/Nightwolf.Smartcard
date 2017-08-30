using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Smartcard
{
    public sealed class Smartcard : IDisposable
    {
        List<SmartcardCertificate> certificates = new List<SmartcardCertificate>();

        public Smartcard(string reader, string cardname)
        {
            IntPtr cardContext = IntPtr.Zero;
            var containers = new List<string>();
            var rootContainer = @"\\.\" + reader + @"\";

            var provider = new StringBuilder();
            uint len = 256;
            provider.EnsureCapacity(256);

            try
            {
                var result = SmartcardInterop.SCardGetCardTypeProviderNameW(IntPtr.Zero, cardname, SmartcardInterop.Provider.Csp, provider, out len);
                if (result != SmartcardException.SCardSuccess)
                {
                    throw new SmartcardException(result);
                }

                var success = SmartcardInterop.CryptAcquireContextW(out cardContext, rootContainer, provider.ToString(), SmartcardInterop.CryptoProvider.RsaFull, 0);
                if (!success)
                {
                    throw new SmartcardException(Marshal.GetLastWin32Error());
                }

                if (provider.Length == 0)
                {
                    throw new SmartcardException(SmartcardException.SCardECardUnsupported);
                }

                uint buflen = 1024;
                var buffer = new byte[buflen];

                success = SmartcardInterop.CryptGetProvParam(cardContext, SmartcardInterop.ProviderParamGet.EnumContainters, buffer, out buflen, SmartcardInterop.ProviderParamFlags.CryptFirst);
                while (success)
                {
                    containers.Add(Encoding.ASCII.GetString(buffer, 0, Convert.ToInt32(buflen)));
                    success = SmartcardInterop.CryptGetProvParam(cardContext, SmartcardInterop.ProviderParamGet.EnumContainters, buffer, out buflen, SmartcardInterop.ProviderParamFlags.CryptNext);
                }
            }
            finally
            {
                if (cardContext != IntPtr.Zero)
                {
                    SmartcardInterop.CryptReleaseContext(cardContext, 0);
                }
            }

            foreach (var ct in containers)
            {
                var x509 = new SmartcardCertificate(rootContainer, ct, provider.ToString());
                this.certificates.Add(x509);
            }
        }

        public IReadOnlyList<SmartcardCertificate> Certificates
        {
            get
            {
                return this.certificates.AsReadOnly();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var ct in this.Certificates)
                    {
                        ct.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
