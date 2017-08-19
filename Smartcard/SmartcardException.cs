﻿using System;

namespace Smartcard
{
    public class SmartcardException : Exception
    {
        // Error codes
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

        private long status = 0;
        private string message = string.Empty;

        public SmartcardException() : base() { }

        public SmartcardException(uint result)
        {
            this.Status = result;
        }

        public SmartcardException(int result)
        {
            this.Status = result;
        }

        public new string Message
        {
            get { return message; }
            private set { message = value; }
        }

        public long Status
        {
            get { return this.status; }
            private set { this.status = value; }
        }
    }
}