using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Smartcard
{
    public static class SmartcardInterop
    {
        public const uint SCardSuccess = 0;
        public const uint SCardENoReadersAvailable = 0x8010002e;
        public const uint SCardEReaderUnavailable = 0x80100017;
        public const uint SCardEUnknownReader = 0x80100009;

        public enum Scope : uint
        {
            User = 0,
            System = 2
        }

        public enum State : uint
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
            Unpowered = 0x400
        }

        public struct SCardGuid
        {
            uint Data1;
            ushort Data2;
            ushort Data3;
            ulong Data4;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ScardReaderState
        {
            public string reader;
            public IntPtr userData;
            [MarshalAs(UnmanagedType.U4)]
            public State currentState;
            [MarshalAs(UnmanagedType.U4)]
            public State eventState;
            public uint atrLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            public byte[] atr;

        }

        [DllImport("winscard.dll")]
        public extern static int SCardEstablishContext(uint scope, IntPtr reserved1, IntPtr reserved2, out IntPtr context);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public extern static int SCardListReadersW(IntPtr context, string groups, char[] readers, out uint readersLength);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public extern static int SCardListCardsW(IntPtr context, byte[] atr, IntPtr interfaces, uint interfaceCount, char[] cards, out uint cardsLength);

        [DllImport("winscard.dll", CharSet = CharSet.Unicode)]
        public extern static int SCardLocateCards(IntPtr context, string cards, ScardReaderState[] states, uint readersLength);
    }
}
