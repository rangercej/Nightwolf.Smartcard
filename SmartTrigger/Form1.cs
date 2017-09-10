// <div>Icons made by <a href="http://www.freepik.com" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

namespace SmartTrigger
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    using Nightwolf.Smartcard;

    public partial class ScPinWindow : Form
    {
        private NotifyIcon trayIcon;

        private CancellationTokenSource monitorCancellationToken;

        private SmartcardReader smartcardMonitor;

        public ScPinWindow()
        {
            InitializeComponent();

            this.smartcardMonitor = new SmartcardReader();
            this.smartcardMonitor.OnCardInserted += this.CardInsertedEventHandler;
            this.smartcardMonitor.OnCardRemoved += this.CardRemovedEventHandler;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.trayIcon = new NotifyIcon
                    {
                        Text = "SmartTrigger",
                        Icon = new Icon(SystemIcons.Application, 40, 40),
                        Visible = true
                    };

            this.Visible = false;
            this.ShowInTaskbar = false;

            base.OnLoad(e);

            this.monitorCancellationToken = new CancellationTokenSource();
            this.smartcardMonitor.StartMonitoring(this.monitorCancellationToken.Token);
        }

        private void DoCardInserted(SmartcardEventArgs e)
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.Focus();
        }

        private void DoCardRemoved(SmartcardEventArgs e)
        {
            if (this.Visible)
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
            }
        }

        private void ScPinWindow_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
            }
        }

        private void ScPinWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.smartcardMonitor.IsMonitoring)
            {
                this.monitorCancellationToken.Cancel();
                this.smartcardMonitor.MonitoringStoppedTrigger.WaitOne();
            }
        }

        #region Smartcard Event Handlers

        /// <summary>
        /// Receive smartcard insert events and marshal them onto the forms thread
        /// </summary>
        /// <param name="o">Sender of the event</param>
        /// <param name="e">Smartcard event arguments</param>
        private void CardInsertedEventHandler(object o, SmartcardEventArgs e)
        {
            this.Invoke((MethodInvoker)(() => this.DoCardInserted(e)));
        }

        /// <summary>
        /// Receive smartcard removed events and marshal them onto the forms thread
        /// </summary>
        /// <param name="o">Sender of the event</param>
        /// <param name="e">Smartcard event arguments</param>
        private void CardRemovedEventHandler(object o, SmartcardEventArgs e)
        {
            this.Invoke((MethodInvoker)(() => this.DoCardRemoved(e)));
        }

        #endregion

    }
}
