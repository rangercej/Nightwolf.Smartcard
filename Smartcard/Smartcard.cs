using System;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Text;

namespace Nightwolf.Smartcard
{
    public sealed class Smartcard : IDisposable
    {
        private X509Store certStore = null;
        private IntPtr certStoreHandle = IntPtr.Zero;

        private IntPtr cardContext = IntPtr.Zero;

        private bool disposedValue = false; // To detect redundant calls

        private readonly string SmartcardRootContainer = string.Empty;
        private readonly string SmartcardCryptoProvider = string.Empty;

        private readonly string SmartcardReaderName = string.Empty;
        private readonly string SmartcardName = string.Empty;

        /// <summary>
        /// Create a smartcard handling class
        /// </summary>
        /// <param name="reader">Reader containing the smartcard</param>
        /// <param name="cardname">Type name of the smartcard in the reader</param>
        public Smartcard(string reader, string cardname)
        {
            this.SmartcardRootContainer = @"\\.\" + reader + @"\";

            this.SmartcardReaderName = reader;
            this.SmartcardName = cardname;

            this.SmartcardCryptoProvider = GetSmartcardCryptoProvider(this.SmartcardName);
            this.cardContext = GetSmartcardCryptoContext(this.SmartcardReaderName, this.SmartcardName);
        }

        public string ReaderName
        {
            get { return this.SmartcardReaderName; }
        }

        public string CardName
        {
            get { return this.SmartcardName; }
        }

        public string ProviderName
        {
            get { return this.SmartcardCryptoProvider; }
        }

        /// <summary>
        /// Return the certificate store held on the smartcard
        /// </summary>
        public X509Store CertificateStore
        {
            get
            {
                if (this.certStore != null)
                {
                    return this.certStore;
                }

                int certStoreLen = 0;
                var success = SmartcardInterop.CryptGetProvParam(cardContext, SmartcardInterop.ProviderParamGet.UserCertStore, null, out certStoreLen, 0);
                if (!success)
                {
                    throw new SmartcardException(Marshal.GetLastWin32Error());
                }

                if (certStoreLen < 1)
                {
                    throw new SmartcardException(SmartcardException.SCardEInsufficientBuffer);
                }

                var byteArray = new byte[certStoreLen];
                success = SmartcardInterop.CryptGetProvParam(cardContext, SmartcardInterop.ProviderParamGet.UserCertStore, byteArray, out certStoreLen, 0);
                if (!success)
                {
                    throw new SmartcardException(Marshal.GetLastWin32Error());
                }

                this.certStoreHandle = (IntPtr)BitConverter.ToUInt32(byteArray, 0);
                this.certStore = new X509Store(this.certStoreHandle);

                return this.certStore;
            }
        }

        /// <summary>
        /// Get the smartcard crypto provider driver ID
        /// </summary>
        /// <param name="cardname">Card type name for which to identify the crypto provider</param>
        /// <returns>Crypto provider identity</returns>
        private string GetSmartcardCryptoProvider(string cardname)
        {
            var provider = new StringBuilder();
            int len = 256;
            provider.EnsureCapacity(256);
            
            var result = SmartcardInterop.SCardGetCardTypeProviderNameW(IntPtr.Zero, cardname, SmartcardInterop.Provider.Csp, provider, out len);
            if (result != SmartcardException.SCardSuccess)
            {
                throw new SmartcardException(result);
            }

            return provider.ToString();
        }

        /// <summary>
        /// Create a smartcard crypto context
        /// </summary>
        /// <param name="cardname">Smartcard reader containing the smartcard</param>
        /// <param name="cardname">Card type name</param>
        /// <returns>Crypto context</returns>
        private IntPtr GetSmartcardCryptoContext(string reader, string cardname)
        {
            IntPtr context = IntPtr.Zero;

            var success = SmartcardInterop.CryptAcquireContextW(out context, this.SmartcardRootContainer, this.SmartcardCryptoProvider, SmartcardInterop.CryptoProvider.RsaFull, 0);
            if (!success)
            {
                throw new SmartcardException(Marshal.GetLastWin32Error());
            }

            if (this.SmartcardCryptoProvider.Length == 0)
            {
                throw new SmartcardException(SmartcardException.SCardECardUnsupported);
            }

            return context;
        }

        /// <summary>
        /// Unlock the smartcard
        /// </summary>
        /// <param name="pin">PIN to unlock the card</param>
        /// <exception cref="SmartcardException">Exception thrown on unlock error</exception>
        public void UnlockCard(string pin)
        {
            var pinBytes = Encoding.ASCII.GetBytes(pin);
            var success = SmartcardInterop.CryptSetProvParam(this.cardContext, SmartcardInterop.ProviderParamSet.KeyExchangePin, pinBytes, 0);
            if (!success)
            {
                throw new SmartcardException(Marshal.GetLastWin32Error());
            }
        }

        #region IDisposable Support
        /// <summary>
        /// Dispose this class, release smartcard context
        /// </summary>
        /// <param name="disposing">Called from dispose()</param>
        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (this.certStoreHandle != IntPtr.Zero)
                {
                    SmartcardInterop.CertCloseStore(this.certStoreHandle, 0);
                    this.certStoreHandle = IntPtr.Zero;
                    this.certStore = null;
                }

                if (this.cardContext != IntPtr.Zero)
                {
                    SmartcardInterop.CryptReleaseContext(cardContext, 0);
                    this.cardContext = IntPtr.Zero;
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
