using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Smartcard
{
    public class SmartcardCertificate : IDisposable
    {
        private X509Certificate2 cert;
        private string containerPath;

        IntPtr cryptoContext = IntPtr.Zero;
        IntPtr keyContext = IntPtr.Zero;

        public SmartcardCertificate(string reader, string container, string provider)
        {
            this.cert = null;
            this.containerPath = reader + container;

            var success = SmartcardInterop.CryptAcquireContextW(out cryptoContext, containerPath, provider.ToString(), SmartcardInterop.CryptoProvider.RsaFull, 0);
            if (!success)
            {
                throw new SmartcardException(Marshal.GetLastWin32Error());
            }

            success = SmartcardInterop.CryptGetUserKey(cryptoContext, SmartcardInterop.KeyFlags.AtKeyExchange, out keyContext);
            if (!success)
            {
                throw new SmartcardException(Marshal.GetLastWin32Error());
            }

            int certLen;
            success = SmartcardInterop.CryptGetKeyParam(keyContext, SmartcardInterop.KeyParam.KpCertificate, null, out certLen, 0);
            if (!success)
            {
                throw new SmartcardException(Marshal.GetLastWin32Error());
            }

            var cert = new byte[certLen];
            success = SmartcardInterop.CryptGetKeyParam(keyContext, SmartcardInterop.KeyParam.KpCertificate, cert, out certLen, 0);
            if (!success)
            {
                throw new SmartcardException(Marshal.GetLastWin32Error());
            }

            this.cert = new X509Certificate2(cert);
            System.Diagnostics.Debug.Print(this.cert.Subject);
        }

        public X509Certificate2 Certificate
        {
            get { return this.cert; }
        }

        public string Container
        {
            get { return this.containerPath; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                if (this.keyContext != IntPtr.Zero)
                {
                    SmartcardInterop.CryptDestroyKey(this.keyContext);
                    this.keyContext = IntPtr.Zero;
                }

                if (this.cryptoContext != IntPtr.Zero)
                {
                    SmartcardInterop.CryptReleaseContext(this.cryptoContext, 0);
                    this.keyContext = IntPtr.Zero;
                }

                disposedValue = true;
            }
        }

        ~SmartcardCertificate()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
