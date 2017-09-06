namespace Nightwolf.Smartcard
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    /// <summary>
    /// Handle smartcard interactions
    /// </summary>
    /// <inheritdoc cref="IDisposable"/>
    public sealed class Smartcard : IDisposable
    {
        /// <summary>Smartcard property: root container identifier</summary>
        private readonly string smartcardRootContainer;

        /// <summary>Smartcard property: name of cryptographic provider for the smartcard</summary>
        private readonly string smartcardCryptoProvider;

        /// <summary>Disposing flag to detect redundant calls</summary>
        private bool disposedValue;

        /// <summary>Certificate store for the smartcard</summary>
        private X509Store certStore;

        /// <summary>Unmanaged pointer to the smartcard certificate store</summary>
        private IntPtr certStoreHandle;

        /// <summary>Unmanaged handler for smartcard crypto operations</summary>
        private IntPtr cardContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Smartcard"/> class. 
        /// </summary>
        /// <param name="reader">Reader containing the smartcard</param>
        /// <param name="cardname">Type name of the smartcard in the reader</param>
        public Smartcard(string reader, string cardname)
        {
            this.smartcardRootContainer = @"\\.\" + reader + @"\";

            this.ReaderName = reader;
            this.CardName = cardname;

            this.smartcardCryptoProvider = this.GetSmartcardCryptoProvider(this.CardName);
            this.cardContext = this.GetSmartcardCryptoContext();

            this.certStoreHandle = IntPtr.Zero;
            this.certStore = null;
            this.disposedValue = false;
        }

        /// <summary>
        /// Gets the name of the smartcard's associated reader
        /// </summary>
        public string ReaderName { get; }

        /// <summary>
        /// Gets the name of the smartcard's type
        /// </summary>
        public string CardName { get; }

        /// <summary>
        /// Gets the certificate store held on the smartcard
        /// </summary>
        public X509Store CertificateStore
        {
            get
            {
                if (this.certStore != null)
                {
                    return this.certStore;
                }

                var success = SmartcardInterop.CryptGetProvParam(this.cardContext, SmartcardInterop.ProviderParamGet.UserCertStore, null, out var certStoreLen, 0);
                if (!success)
                {
                    throw new SmartcardException(Marshal.GetLastWin32Error());
                }

                if (certStoreLen < 1)
                {
                    throw new SmartcardException(SmartcardException.SCardEInsufficientBuffer);
                }

                var byteArray = new byte[certStoreLen];
                success = SmartcardInterop.CryptGetProvParam(this.cardContext, SmartcardInterop.ProviderParamGet.UserCertStore, byteArray, out certStoreLen, 0);
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
            if (!this.disposedValue)
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
                    SmartcardInterop.CryptReleaseContext(this.cardContext, 0);
                    this.cardContext = IntPtr.Zero;
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose this class, release smartcard context
        /// </summary>
        /// <inheritdoc cref="Dispose()"/>
        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion

        /// <summary>
        /// Get the smartcard crypto provider driver ID
        /// </summary>
        /// <param name="cardname">Card type name for which to identify the crypto provider</param>
        /// <returns>Crypto provider identity</returns>
        private string GetSmartcardCryptoProvider(string cardname)
        {
            var provider = new StringBuilder();
            var len = 256;
            provider.EnsureCapacity(len);
            
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
        /// <returns>Crypto context</returns>
        private IntPtr GetSmartcardCryptoContext()
        {
            var success = SmartcardInterop.CryptAcquireContextW(out var context, this.smartcardRootContainer, this.smartcardCryptoProvider, SmartcardInterop.CryptoProvider.RsaFull, 0);
            if (!success)
            {
                throw new SmartcardException(Marshal.GetLastWin32Error());
            }

            if (this.smartcardCryptoProvider.Length == 0)
            {
                throw new SmartcardException(SmartcardException.SCardECardUnsupported);
            }

            return context;
        }
    }
}
