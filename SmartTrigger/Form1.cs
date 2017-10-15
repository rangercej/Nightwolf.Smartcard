// <div>Icons made by <a href="http://www.freepik.com" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

using System.Collections.Generic;

namespace Nightwolf.SmartTrigger
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Nightwolf.Smartcard;

    /// <summary>
    /// Smartcard trigger and actions
    /// </summary>
    /// <inheritdoc cref="Form"/>
    public partial class ScPinWindow : Form
    {
        private ManualResetEvent monitorNotify;

        internal string PinPassback;

        /// <summary>Notification tray icon</summary>
        private NotifyIcon trayIcon;

        /// <summary>Smartcard monitor cancellation</summary>
        private CancellationTokenSource monitorCancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScPinWindow"/> class. 
        /// </summary>
        /// <inheritdoc cref="Form()" />
        public ScPinWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Form load event handler
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <inheritdoc cref="OnLoad(EventArgs)"/>
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
            var monitor = new CardMonitor(this.monitorCancellationToken.Token);
            Task.Factory.StartNew(() => monitor.Start(this));
        }

        /// <summary>
        /// When form is minimised, hide it from the taskbar
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Resize parameters</param>
        private void ScPinWindow_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
            }
        }

        internal void ShowPinWindow(ManualResetEvent continueNotifier)
        {
            this.monitorNotify = continueNotifier;
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.Focus();
        }

        /// <summary>
        /// When form is closed, cleanly shutdown the smartcard monitor
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Form close parameters</param>
        private void ScPinWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.monitorCancellationToken.Cancel();
            this.trayIcon.Dispose();
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.PinPassback = this.textPin.Text;
            this.monitorNotify.Set();
        }
    }
}
