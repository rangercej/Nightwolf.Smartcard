namespace Nightwolf.Smartcard
{
    using System.ComponentModel;

    /// <summary>
    /// Smartcard exception
    /// </summary>
    /// <inheritdoc cref="Win32Exception"/>
    public sealed class SmartcardException : Win32Exception
    {
        // Error codes from winscard.h
        public const int SCardSuccess = 0;
        public const int SCardFInternalError = unchecked((int)0x80100001);
        public const int SCardECancelled = unchecked((int)0x80100002);
        public const int SCardEInvalidHandle = unchecked((int)0x80100003);
        public const int SCardEInvalidParameter = unchecked((int)0x80100004);
        public const int SCardEInvalidTarget = unchecked((int)0x80100005);
        public const int SCardENoMemory = unchecked((int)0x80100006);
        public const int SCardFWaitedTooLong = unchecked((int)0x80100007);
        public const int SCardEInsufficientBuffer = unchecked((int)0x80100008);
        public const int SCardEUnknownReader = unchecked((int)0x80100009);
        public const int SCardETimeout = unchecked((int)0x8010000A);
        public const int SCardESharingViolation = unchecked((int)0x8010000B);
        public const int SCardENoSmartcard = unchecked((int)0x8010000C);
        public const int SCardEUnknownCard = unchecked((int)0x8010000D);
        public const int SCardECantDispose = unchecked((int)0x8010000E);
        public const int SCardEProtoMismatch = unchecked((int)0x8010000F);
        public const int SCardENotReady = unchecked((int)0x80100010);
        public const int SCardEInvalidValue = unchecked((int)0x80100011);
        public const int SCardESystemCancelled = unchecked((int)0x80100012);
        public const int SCardFCommError = unchecked((int)0x80100013);
        public const int SCardFUnknownError = unchecked((int)0x80100014);
        public const int SCardEInvalidAtr = unchecked((int)0x80100015);
        public const int SCardENotTransacted = unchecked((int)0x80100016);
        public const int SCardEReaderUnavailable = unchecked((int)0x80100017);
        public const int SCardPShutdown = unchecked((int)0x80100018);
        public const int SCardEPciTooSmall = unchecked((int)0x80100019);
        public const int SCardEReaderUnsupported = unchecked((int)0x8010001A);
        public const int SCardEDuplicateReader = unchecked((int)0x8010001B);
        public const int SCardECardUnsupported = unchecked((int)0x8010001C);
        public const int SCardENoService = unchecked((int)0x8010001D);
        public const int SCardEServiceStopped = unchecked((int)0x8010001E);
        public const int SCardEUnexpected = unchecked((int)0x8010001F);
        public const int SCardEIccInstallation = unchecked((int)0x80100020);
        public const int SCardEIccCreateorder = unchecked((int)0x80100021);
        public const int SCardEUnsupportedFeature = unchecked((int)0x80100022);
        public const int SCardEDirNotFound = unchecked((int)0x80100023);
        public const int SCardEFileNotFound = unchecked((int)0x80100024);
        public const int SCardENoDir = unchecked((int)0x80100025);
        public const int SCardENoFile = unchecked((int)0x80100026);
        public const int SCardENoAccess = unchecked((int)0x80100027);
        public const int SCardEWriteTooMany = unchecked((int)0x80100028);
        public const int SCardEBadSeek = unchecked((int)0x80100029);
        public const int SCardEInvalidChv = unchecked((int)0x8010002A);
        public const int SCardEUnknownResMng = unchecked((int)0x8010002B);
        public const int SCardENoSuchCertificate = unchecked((int)0x8010002C);
        public const int SCardECertificateUnavailable = unchecked((int)0x8010002D);
        public const int SCardENoReadersAvailable = unchecked((int)0x8010002E);
        public const int SCardECommDataLost = unchecked((int)0x8010002F);
        public const int SCardENoKeyContainer = unchecked((int)0x80100030);
        public const int SCardEServerTooBusy = unchecked((int)0x80100031);
        public const int SCardEPinCacheExpired = unchecked((int)0x80100032);
        public const int SCardENoPinCache = unchecked((int)0x80100033);
        public const int SCardEReadOnlyCard = unchecked((int)0x80100034);
        public const int SCardWUnsupportedCard = unchecked((int)0x80100065);
        public const int SCardWUnresponsiveCard = unchecked((int)0x80100066);
        public const int SCardWUnpoweredCard = unchecked((int)0x80100067);
        public const int SCardWResetCard = unchecked((int)0x80100068);
        public const int SCardWRemovedCard = unchecked((int)0x80100069);
        public const int SCardWSecurityViolation = unchecked((int)0x8010006A);
        public const int SCardWWrongChv = unchecked((int)0x8010006B);
        public const int SCardWChvBlocked = unchecked((int)0x8010006C);
        public const int SCardWEof = unchecked((int)0x8010006D);
        public const int SCardWCancelledByUser = unchecked((int)0x8010006E);
        public const int SCardWCardNotAuthenticated = unchecked((int)0x8010006F);
        public const int SCardWCacheItemNotFound = unchecked((int)0x80100070);
        public const int SCardWCacheItemStale = unchecked((int)0x80100071);
        public const int SCardWCacheItemTooBig = unchecked((int)0x80100072);

        // Crypto API errors from winerror.h
        public const int NteOpOk = 0;
        public const int NteBadUid = unchecked((int)0x80090001);
        public const int NteBadHash = unchecked((int)0x80090002);
        public const int NteBadKey = unchecked((int)0x80090003);
        public const int NteBadLen = unchecked((int)0x80090004);
        public const int NteBadData = unchecked((int)0x80090005);
        public const int NteBadSignature = unchecked((int)0x80090006);
        public const int NteBadVer = unchecked((int)0x80090007);
        public const int NteBadAlgId = unchecked((int)0x80090008);
        public const int NteBadFlags = unchecked((int)0x80090009);
        public const int NteBadType = unchecked((int)0x8009000A);
        public const int NteBadKeyState = unchecked((int)0x8009000B);
        public const int NteBadHashState = unchecked((int)0x8009000C);
        public const int NteNoKey = unchecked((int)0x8009000D);
        public const int NteNoMemory = unchecked((int)0x8009000E);
        public const int NteExists = unchecked((int)0x8009000F);
        public const int NtePerm = unchecked((int)0x80090010);
        public const int NteNotFound = unchecked((int)0x80090011);
        public const int NteDoubleEncrypt = unchecked((int)0x80090012);
        public const int NteBadProvider = unchecked((int)0x80090013);
        public const int NteBadProvType = unchecked((int)0x80090014);
        public const int NteBadPublicKey = unchecked((int)0x80090015);
        public const int NteBadKeyset = unchecked((int)0x80090016);
        public const int NteProvTypeNotDef = unchecked((int)0x80090017);
        public const int NteProvTypeEntryBad = unchecked((int)0x80090018);
        public const int NteKeysetNotDef = unchecked((int)0x80090019);
        public const int NteKeysetEntryBad = unchecked((int)0x8009001A);
        public const int NteProvTypeNoMatch = unchecked((int)0x8009001B);
        public const int NteSignatureFileBad = unchecked((int)0x8009001C);
        public const int NteProviderDllFail = unchecked((int)0x8009001D);
        public const int NteProvDllNotFound = unchecked((int)0x8009001E);
        public const int NteBadKeysetParam = unchecked((int)0x8009001F);
        public const int NteFail = unchecked((int)0x80090020);
        public const int NteSysErr = unchecked((int)0x80090021);
        public const int NteSilentContext = unchecked((int)0x80090022);
        public const int NteTokenKeysetStorageFull = unchecked((int)0x80090023);
        public const int NteTemporaryProfile = unchecked((int)0x80090024);
        public const int NteFixedParameter = unchecked((int)0x80090025);
        public const int NteInvalidHandle = unchecked((int)0x80090026);
        public const int NteInvalidParameter = unchecked((int)0x80090027);
        public const int NteBufferTooSmall = unchecked((int)0x80090028);
        public const int NteNotSupported = unchecked((int)0x80090029);
        public const int NteNoMoreItems = unchecked((int)0x8009002A);
        public const int NteBuffersOverlap = unchecked((int)0x8009002B);
        public const int NteDecryptionFailure = unchecked((int)0x8009002C);
        public const int NteInternalError = unchecked((int)0x8009002D);
        public const int NteUIRequired = unchecked((int)0x8009002E);
        public const int NteHmacNotSupported = unchecked((int)0x8009002F);
        public const int NteDeviceNotReady = unchecked((int)0x80090030);
        public const int NteAuthenticationIgnored = unchecked((int)0x80090031);
        public const int NteValidationFailed = unchecked((int)0x80090032);
        public const int NteIncorrectPassword = unchecked((int)0x80090033);
        public const int NteEncryptionFailure = unchecked((int)0x80090034);
        public const int NteDeviceNotFound = unchecked((int)0x80090035);
        public const int NteUserCancelled = unchecked((int)0x80090036);
        public const int NtePasswordChangeRequired = unchecked((int)0x80090037);

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartcardException"/> class. 
        /// </summary>
        public SmartcardException() : base() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartcardException"/> class. 
        /// </summary>
        /// <param name="result">Exception status code</param>
        public SmartcardException(int result) : base(result) {}

        public int Status
        {
            get { return base.NativeErrorCode; }
        }
    }
}
