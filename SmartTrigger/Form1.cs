// <div>Icons made by <a href="http://www.freepik.com" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

namespace Nightwolf.SmartTrigger
{
    using System;
    using System.Configuration;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    using Nightwolf.Smartcard;

    /// <summary>
    /// Smartcard trigger and actions
    /// </summary>
    /// <inheritdoc cref="Form"/>
    public partial class ScPinWindow : Form
    {
        /// <summary>Smartcard monitor class</summary>
        private readonly SmartcardMonitor smartcardMonitor;

        /// <summary>Notification tray icon</summary>
        private NotifyIcon trayIcon;

        /// <summary>Smartcard monitor cancellation</summary>
        private CancellationTokenSource monitorCancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScPinWindow"/> class. 
        /// </summary>
        /// <inheritdoc cref="Form()"/>
        public ScPinWindow()
        {
            this.InitializeComponent();

            var cfg = (Config.Smartcard)ConfigurationManager.GetSection("smartcard");

            this.smartcardMonitor = new SmartcardMonitor();
            this.smartcardMonitor.OnCardInserted += this.CardInsertedEventHandler;
            this.smartcardMonitor.OnCardRemoved += this.CardRemovedEventHandler;

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
            this.smartcardMonitor.StartMonitoring(this.monitorCancellationToken.Token);
        }

        /// <summary>
        /// Smartcard inserted handler
        /// </summary>
        /// <param name="e">Smartcard event args</param>
        private void DoCardInserted(SmartcardEventArgs e)
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.Focus();
        }

        /// <summary>
        /// Smartcard removed handler
        /// </summary>
        /// <param name="e">Smartcard event args</param>
        private void DoCardRemoved(SmartcardEventArgs e)
        {
            if (this.Visible)
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
            }
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

        /// <summary>
        /// When form is closed, cleanly shutdown the smartcard monitor
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Form close parameters</param>
        private void ScPinWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.smartcardMonitor.IsMonitoring)
            {
                this.monitorCancellationToken.Cancel();
                this.smartcardMonitor.MonitoringStoppedTrigger.WaitOne();
            }

            this.trayIcon.Dispose();
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
