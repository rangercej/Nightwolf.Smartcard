using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartcard
{
    public class SmartcardReader : IDisposable
    {
        public event EventHandler OnCardInserted;

        IntPtr context = IntPtr.Zero;

        private IList<string> ReaderNames = new List<string>();
        private IList<string> CardNames = new List<string>();

        private IList<SmartcardInterop.ScardReaderState> currentState = null;

        public SmartcardReader()
        {
            var result = SmartcardInterop.SCardEstablishContext((uint)SmartcardInterop.Scope.User, IntPtr.Zero, IntPtr.Zero, out context);

            RefreshCards();
            RefreshReaders();
            RefreshState();
        }

        public void StartMonitoring()
        {
            if (currentState != null)
            {
                var readersWithCards = currentState.Where(x => (x.eventState & SmartcardInterop.State.Present) != 0);
                foreach (var reader in readersWithCards)
                {
                    FireCardPresentEvent(reader);
                }
            }

            Task.Factory.StartNew(WaitForReaderStateChange);
        }

        private void FireCardPresentEvent(SmartcardInterop.ScardReaderState state)
        {
            var cardname = FindCardWithAtr(state.atr);
            var scard = new Smartcard(state.reader, cardname);
            var args = new SmartcardEventArgs(scard);

            OnCardInserted(this, args);
        }

        private void RefreshReaders()
        {
            uint readerLen = 1024;
            var readers = new char[1024];

            var result = SmartcardInterop.SCardListReadersW(context, null, readers, out readerLen);
            if (result != SmartcardException.SCardSuccess && result != SmartcardException.SCardENoReadersAvailable)
            {
                throw new SmartcardException(result);
            }

            var r = MultiStringToArray(readers);
            this.ReaderNames = r.ToList();
        }

        private void RefreshCards()
        {
            uint cardLen = 16384;
            var cards = new char[cardLen];

            var result = SmartcardInterop.SCardListCardsW(context, null, IntPtr.Zero, 0, cards, out cardLen);
            if (result != SmartcardException.SCardSuccess)
            {
                throw new SmartcardException(result);
            }

            var c = MultiStringToArray(cards);
            this.CardNames = c.ToList();
        }

        private void RefreshState()
        {
            IList<SmartcardInterop.ScardReaderState> scardstatelist;
            if (this.currentState == null)
            {
                scardstatelist = CreatePendingReaderState();
            }
            else
            {
                scardstatelist = this.currentState;
            }
            
            SmartcardInterop.ScardReaderState[] scardstate = null;
            if (this.ReaderNames.Count > 0)
            {
                var d = ArrayToMultiString(this.CardNames);

                scardstate = scardstatelist.ToArray();
                var result = SmartcardInterop.SCardLocateCards(context, d, scardstate, Convert.ToUInt32(scardstate.Length));
                if (result != SmartcardException.SCardSuccess)
                {
                    throw new SmartcardException(result);
                }

                for (var i = 0; i < scardstate.Length; i++)
                {
                    scardstate[i].currentState = scardstate[i].eventState;
                }
            }

            this.currentState = scardstate.ToList();
        }

        private void WaitForReaderStateChange()
        {
            while (true)
            {
                System.Diagnostics.Debug.Print("StateChange - start");
                var scardstatelist = this.currentState;
                SmartcardInterop.ScardReaderState[] scardstate = null;

                const string NotificationReader = @"\\?PnP?\Notification";
                var state = new SmartcardInterop.ScardReaderState
                {
                    reader = NotificationReader,
                    currentState = 0,
                    eventState = 0,
                    atrLength = 0
                };

                scardstatelist.Add(state);

                scardstate = scardstatelist.ToArray();
                var result = SmartcardInterop.SCardGetStatusChange(context, 500, scardstate, Convert.ToUInt32(scardstate.Length));
                if (result == SmartcardException.SCardETimeout)
                {
                    continue;
                }

                if (result != SmartcardException.SCardSuccess)
                {
                    throw new SmartcardException(result);
                }

                var scardChanges = scardstate.Where(x => (x.eventState & SmartcardInterop.State.Changed) != 0).ToList();
                if (scardChanges.Count == 0)
                {
                    continue;
                }

                if (scardChanges.Any(x => x.reader == NotificationReader))
                {
                    RefreshReaders();
                }

                RefreshState();

                var readersWithCards = currentState.Where(x => (x.eventState & SmartcardInterop.State.Present) != 0);
                foreach (var reader in readersWithCards)
                {
                    FireCardPresentEvent(reader);
                }
            }
        }

        private IList<SmartcardInterop.ScardReaderState> CreatePendingReaderState()
        {
            var scardstatelist = new List<SmartcardInterop.ScardReaderState>();

            foreach (var reader in this.ReaderNames)
            {
                var state = new SmartcardInterop.ScardReaderState
                {
                    reader = reader,
                    currentState = SmartcardInterop.State.Unaware,
                    eventState = 0,
                    atrLength = 0
                };

                scardstatelist.Add(state);
            }

            return scardstatelist;
        }

        private string FindCardWithAtr(byte[] atr)
        {
            uint cardLen = 1024;
            var cards = new char[cardLen];
            var result = SmartcardInterop.SCardListCardsW(IntPtr.Zero, atr, IntPtr.Zero, 0, cards, out cardLen);
            if (result != SmartcardException.SCardSuccess)
            {
                throw new SmartcardException(result);
            }

            var card = MultiStringToArray(cards);
            if (card.Count == 0)
            {
                throw new SmartcardException(SmartcardException.SCardECardUnsupported);
            }

            if (card.Count > 1)
            {
                throw new SmartcardException(SmartcardException.SCardEUnexpected);
            }

            return card[0];
        }

        private static IList<string> MultiStringToArray(char[] multistring)
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

        private static string ArrayToMultiString(IList<string> stringlist)
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

                if (this.context != IntPtr.Zero)
                {
                    SmartcardInterop.SCardReleaseContext(this.context);
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
