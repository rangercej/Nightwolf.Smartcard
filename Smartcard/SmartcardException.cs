using System.ComponentModel;

namespace Smartcard
{
    public sealed class SmartcardException : Win32Exception
    {
        // Error codes from winscard.h
        public const uint SCardSuccess = 0;
        public const uint SCardFInternalError = 0x80100001;
        public const uint SCardECancelled = 0x80100002;
        public const uint SCardEInvalidHandle = 0x80100003;
        public const uint SCardEInvalidParameter = 0x80100004;
        public const uint SCardEInvalidTarget = 0x80100005;
        public const uint SCardENoMemory = 0x80100006;
        public const uint SCardFWaitedTooLong = 0x80100007;
        public const uint SCardEInsufficientBuffer = 0x80100008;
        public const uint SCardEUnknownReader = 0x80100009;
        public const uint SCardETimeout = 0x8010000A;
        public const uint SCardESharingViolation = 0x8010000B;
        public const uint SCardENoSmartcard = 0x8010000C;
        public const uint SCardEUnknownCard = 0x8010000D;
        public const uint SCardECantDispose = 0x8010000E;
        public const uint SCardEProtoMismatch = 0x8010000F;
        public const uint SCardENotReady = 0x80100010;
        public const uint SCardEInvalidValue = 0x80100011;
        public const uint SCardESystemCancelled = 0x80100012;
        public const uint SCardFCommError = 0x80100013;
        public const uint SCardFUnknownError = 0x80100014;
        public const uint SCardEInvalidAtr = 0x80100015;
        public const uint SCardENotTransacted = 0x80100016;
        public const uint SCardEReaderUnavailable = 0x80100017;
        public const uint SCardPShutdown = 0x80100018;
        public const uint SCardEPciTooSmall = 0x80100019;
        public const uint SCardEReaderUnsupported = 0x8010001A;
        public const uint SCardEDuplicateReader = 0x8010001B;
        public const uint SCardECardUnsupported = 0x8010001C;
        public const uint SCardENoService = 0x8010001D;
        public const uint SCardEServiceStopped = 0x8010001E;
        public const uint SCardEUnexpected = 0x8010001F;
        public const uint SCardEIccInstallation = 0x80100020;
        public const uint SCardEIccCreateorder = 0x80100021;
        public const uint SCardEUnsupportedFeature = 0x80100022;
        public const uint SCardEDirNotFound = 0x80100023;
        public const uint SCardEFileNotFound = 0x80100024;
        public const uint SCardENoDir = 0x80100025;
        public const uint SCardENoFile = 0x80100026;
        public const uint SCardENoAccess = 0x80100027;
        public const uint SCardEWriteTooMany = 0x80100028;
        public const uint SCardEBadSeek = 0x80100029;
        public const uint SCardEInvalidChv = 0x8010002A;
        public const uint SCardEUnknownResMng = 0x8010002B;
        public const uint SCardENoSuchCertificate = 0x8010002C;
        public const uint SCardECertificateUnavailable = 0x8010002D;
        public const uint SCardENoReadersAvailable = 0x8010002E;
        public const uint SCardECommDataLost = 0x8010002F;
        public const uint SCardENoKeyContainer = 0x80100030;
        public const uint SCardEServerTooBusy = 0x80100031;
        public const uint SCardEPinCacheExpired = 0x80100032;
        public const uint SCardENoPinCache = 0x80100033;
        public const uint SCardEReadOnlyCard = 0x80100034;
        public const uint SCardWUnsupportedCard = 0x80100065;
        public const uint SCardWUnresponsiveCard = 0x80100066;
        public const uint SCardWUnpoweredCard = 0x80100067;
        public const uint SCardWResetCard = 0x80100068;
        public const uint SCardWRemovedCard = 0x80100069;
        public const uint SCardWSecurityViolation = 0x8010006A;
        public const uint SCardWWrongChv = 0x8010006B;
        public const uint SCardWChvBlocked = 0x8010006C;
        public const uint SCardWEof = 0x8010006D;
        public const uint SCardWCancelledByUser = 0x8010006E;
        public const uint SCardWCardNotAuthenticated = 0x8010006F;
        public const uint SCardWCacheItemNotFound = 0x80100070;
        public const uint SCardWCacheItemStale = 0x80100071;
        public const uint SCardWCacheItemTooBig = 0x80100072;

        // Crypto API errors from winerror.h
        public const uint NteOpOk = 0;
        public const uint NteBadUid = 0x80090001;
        public const uint NteBadHash = 0x80090002;
        public const uint NteBadKey = 0x80090003;
        public const uint NteBadLen = 0x80090004;
        public const uint NteBadData = 0x80090005;
        public const uint NteBadSignature = 0x80090006;
        public const uint NteBadVer = 0x80090007;
        public const uint NteBadAlgId = 0x80090008;
        public const uint NteBadFlags = 0x80090009;
        public const uint NteBadType = 0x8009000A;
        public const uint NteBadKeyState = 0x8009000B;
        public const uint NteBadHashState = 0x8009000C;
        public const uint NteNoKey = 0x8009000D;
        public const uint NteNoMemory = 0x8009000E;
        public const uint NteExists = 0x8009000F;
        public const uint NtePerm = 0x80090010;
        public const uint NteNotFound = 0x80090011;
        public const uint NteDoubleEncrypt = 0x80090012;
        public const uint NteBadProvider = 0x80090013;
        public const uint NteBadProvType = 0x80090014;
        public const uint NteBadPublicKey = 0x80090015;
        public const uint NteBadKeyset = 0x80090016;
        public const uint NteProvTypeNotDef = 0x80090017;
        public const uint NteProvTypeEntryBad = 0x80090018;
        public const uint NteKeysetNotDef = 0x80090019;
        public const uint NteKeysetEntryBad = 0x8009001A;
        public const uint NteProvTypeNoMatch = 0x8009001B;
        public const uint NteSignatureFileBad = 0x8009001C;
        public const uint NteProviderDllFail = 0x8009001D;
        public const uint NteProvDllNotFound = 0x8009001E;
        public const uint NteBadKeysetParam = 0x8009001F;
        public const uint NteFail = 0x80090020;
        public const uint NteSysErr = 0x80090021;
        public const uint NteSilentContext = 0x80090022;
        public const uint NteTokenKeysetStorageFull = 0x80090023;
        public const uint NteTemporaryProfile = 0x80090024;
        public const uint NteFixedParameter = 0x80090025;
        public const uint NteInvalidHandle = 0x80090026;
        public const uint NteInvalidParameter = 0x80090027;
        public const uint NteBufferTooSmall = 0x80090028;
        public const uint NteNotSupported = 0x80090029;
        public const uint NteNoMoreItems = 0x8009002A;
        public const uint NteBuffersOverlap = 0x8009002B;
        public const uint NteDecryptionFailure = 0x8009002C;
        public const uint NteInternalError = 0x8009002D;
        public const uint NteUIRequired = 0x8009002E;
        public const uint NteHmacNotSupported = 0x8009002F;
        public const uint NteDeviceNotReady = 0x80090030;
        public const uint NteAuthenticationIgnored = 0x80090031;
        public const uint NteValidationFailed = 0x80090032;
        public const uint NteIncorrectPassword = 0x80090033;
        public const uint NteEncryptionFailure = 0x80090034;
        public const uint NteDeviceNotFound = 0x80090035;
        public const uint NteUserCancelled = 0x80090036;
        public const uint NtePasswordChangeRequired = 0x80090037;

        private long status = 0;
        private string message = string.Empty;

        public SmartcardException() : base() {}

        public SmartcardException(uint result) : base((int)result) {}

        public SmartcardException(int result) : base(result) {}

        public new string Message
        {
            get { return base.Message; }
       }

        public uint Status
        {
            get { return (uint)base.HResult; }
        }
    }
}
