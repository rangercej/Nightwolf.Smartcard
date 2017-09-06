namespace Nightwolf.Smartcard
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// This class contains C# versions of method calls, structures and constants from winscard.h and wincrypt.h.
    /// </summary>
    public static class SmartcardInterop
    {
        /// <summary>
        /// SCARD_SCOPE_ constants from winscard.h
        /// </summary>
        public enum Scope
        {
            User = 0,
            Terminal = 1,
            System = 2
        }

        /// <summary>
        /// SCARD_PROVIDER_ constants from winscard.h
        /// </summary>
        public enum Provider
        {
            Primary = 1,
            Csp = 2,
            Ksp = 3,
            CardModule = unchecked((int)0x80000001)
        }

        /// <summary>
        /// PROV_ constants from wincrypt.h
        /// </summary>
        public enum CryptoProvider
        {
            RsaFull = 1,
            RsaSig = 2,
            Dss = 3,
            Fortezza = 4,
            MsExchange = 5,
            Ssl = 6,
            RsaSChannel = 12,
            DssDh = 13,
            EcEcdsaSig = 14,
            EcEcnraSig = 15,
            EcEcdsaFull = 16,
            EcEcnraFull = 17,
            DhSChannel = 18,
            SpyrusLynks = 20,
            Rng = 21,
            IntelSec = 22,
            ReplaceOwf = 23,
            RsaAes = 24
        }

        /// <summary>
        // PP_xxx constants for CryptoGetParam from wincrypt.h
        /// </summary>
        public enum ProviderParamGet
        {
            AdminPin = 0x1F,
            AppliCert = 0x12,
            ChangePassword = 0x07,
            CertChain = 0x09,
            Container = 0x06,
            CryptCountKeyUse = 0x29,
            EnumAlgs = 0x01,
            EnumAlgsEx = 0x16,
            EnumContainters = 0x02,
            EnumElectRoots = 0x1A,
            EnumExSigningProt = 0x28,
            EnumMandRoots = 0x19,
            ImpType = 0x03,
            KeyTypeSubtype = 0x0A,
            KeyExchangePin = 0x20,
            KeysetSecDescr = 0x08,
            KeysetType = 0x1B,
            KeySpec = 0x27,
            KeyxKeysizeInc = 0x23,
            Name = 0x04,
            ProvType = 0x10,
            RootCertStore = 0x2E,
            SessionKeySize = 0x14,
            SgcInfo = 0x25,
            SigKeysizeInc = 0x22,
            SignaturePin = 0x21,
            SmartcardGuid = 0x2D,
            SmartcardReader = 0x2B,
            SymKeysize = 0x13,
            UiPrompt = 0x15,
            UniqueContainer = 0x24,
            UseHardwareRng = 0x26,
            UserCertStore = 0x2A,
            Version = 0x05
        }

        /// <summary>
        // PP_xxx constants for CryptoSetParam from wincrypt.h
        /// </summary>
        public enum ProviderParamSet
        {
            ClientHwnd = 0x01,
            DeleteKey = 0x18,
            KeyExchangePin = 0x20,
            KeysetSecDescr = 0x08,
            PinPromptString = 0x2C,
            RootCertStore = 0x2E,
            SignaturePin = 0x21,
            UiPrompt = 0x15,
            UseHardwareRng = 0x26,
            UserCertStore = 0x2A,
            SecureKeyExchangePin = 0x2F,
            SecureSignaturePin = 0x30,
            SmartcardReader = 0x2B,
            SmartcardGuid = 0x2D
        }

        public enum KeyFlags
        {
            AtKeyExchange = 1,
            AtSignature = 2
        }

        public enum KeyParam
        {
            KpCertificate = 26
        }

        [Flags]
        public enum ProviderParamFlags
        {
            CryptFirst = 0x01,
            CryptNext = 0x02,
            CryptSgcEnum = 0x04,
        }
        
        [Flags]
        public enum CryptoFlags
        {
            VerifyContext = unchecked((int)0xF0000000),
            NewKeySet = 0x08,
            DeleteKeySet = 0x10,
            MachineKeySet = 0x20,
            Silent = 0x40,
            DefaultContainerOptional = 0x80
        }

        [Flags]
        public enum State
        {
            Unaware = 0x00,
            Ignore = 0x01,
            Changed = 0x02,
            Unknown = 0x04,
            Unavailable = 0x08,
            Empty = 0x10,
            Present = 0x20,
            AtrMatch = 0x40,
            Exclusive = 0x80,
            InUse = 0x100,
            Mute = 0x200,
            Unpowered = 0x400,
            ReservedFlag = 0x10000
        }

        public const int Infinite = unchecked((int)0xFFFFFFFF);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ScardReaderState
        {
            public string reader;
            public IntPtr userData;
            [MarshalAs(UnmanagedType.U4)]
            public State currentState;
            [MarshalAs(UnmanagedType.U4)]
            public State eventState;
            public int atrLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            public byte[] atr;
        }

        [DllImport("winscard.dll")]
        public static extern int SCardEstablishContext([MarshalAs(UnmanagedType.U4)]Scope scope, IntPtr reserved1, IntPtr reserved2, out IntPtr context);

        [DllImport("winscard.dll")]
        public static extern int SCardReleaseContext(IntPtr context);

        [DllImport("winscard.dll")]
        public static extern int SCardCancel(IntPtr context);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public static extern int SCardListReadersW(IntPtr context, string groups, char[] readers, out int readersLength);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public static extern int SCardListCardsW(IntPtr context, byte[] atr, IntPtr interfaces, int interfaceCount, char[] cards, out int cardsLength);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public static extern int SCardLocateCards(IntPtr context, string cards, [In,Out] ScardReaderState[] states, int readersLength);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public static extern int SCardGetStatusChange(IntPtr context, int timeout, [In,Out] ScardReaderState[] states, int readersLength);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public static extern int SCardGetCardTypeProviderNameW(IntPtr context, string cardname, [MarshalAs(UnmanagedType.U4)] Provider providerId, StringBuilder provider, out int providerLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CryptAcquireContextW(out IntPtr context, string container, string provider, [MarshalAs(UnmanagedType.U4)] CryptoProvider provType, int flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptReleaseContext(IntPtr context, int flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptSetProvParam(IntPtr context, [MarshalAs(UnmanagedType.U4)] ProviderParamSet param, byte[] data, [MarshalAs(UnmanagedType.U4)] ProviderParamFlags flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetProvParam(IntPtr context, [MarshalAs(UnmanagedType.U4)] ProviderParamGet param, byte[] data, out int datalen, [MarshalAs(UnmanagedType.U4)] ProviderParamFlags flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetUserKey(IntPtr context, [MarshalAs(UnmanagedType.U4)] KeyFlags keySpec, out IntPtr keyContext);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDestroyKey(IntPtr keyContext);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetKeyParam(IntPtr keyContext, [MarshalAs(UnmanagedType.U4)] KeyParam param, byte[] data, out int datalen, int flags);

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CertCloseStore(IntPtr storeHandle, int flags);

        /// <summary>
        /// Convert a C-style null seperated, double-null terminated string to a c# list of strings
        /// </summary>
        /// <param name="multistring">C-style multistring to convert</param>
        /// <returns>List of strings obtained from the multistring</returns>
        public static IList<string> MultiStringToArray(char[] multistring)
        {
            var stringList = new List<string>();
            var i = 0;
            while (i < multistring.Length)
            {
                var j = i;
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

        /// <summary>
        /// Convert a list of strings to a C-style null-seperated, double-null terminated string
        /// </summary>
        /// <param name="stringlist">List of strings to convert</param>
        /// <returns>C-style multistring</returns>
        public static string ArrayToMultiString(IList<string> stringlist)
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
