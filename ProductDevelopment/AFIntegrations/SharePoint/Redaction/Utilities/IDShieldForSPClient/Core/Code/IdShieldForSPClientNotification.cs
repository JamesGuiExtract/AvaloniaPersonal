using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;

namespace Extract.SharePoint.Redaction.Utilities
{
    /// <summary>
    /// Class to manage displaying the system tray notification icon and handling events on it.
    /// </summary>
    class IdShieldForSPClientNotification : ApplicationContext
    {
        #region Fields

        /// <summary>
        /// Maintains the open communication channels used to communicate with the service.
        /// </summary>
        ServiceHost _host;

        /// <summary>
        /// Mutex object used to ensure serialized access to the service host.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Event used to indicate that the service is stopping.
        /// </summary>
        ManualResetEvent _endService = new ManualResetEvent(false);

        /// <summary>
        /// The component container for the application.
        /// </summary>
        IContainer _components = new Container();

        /// <summary>
        /// Taskbar notification area control.
        /// </summary>
        NotifyIcon _notifyIcon;

        /// <summary>
        /// The context menu associated with the ID Shield application.
        /// </summary>
        ContextMenuStrip _contextMenu = new ContextMenuStrip();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdShieldForSPClientNotification"/> class.
        /// </summary>
        public IdShieldForSPClientNotification()
        {
            try
            {
                // Create the notification icon
                _notifyIcon = new NotifyIcon(_components);
                _notifyIcon.Icon = Properties.Resources.IDShieldLogo;
                _notifyIcon.Text = "ID Shield for SharePoint Client";
                _notifyIcon.ContextMenuStrip = _contextMenu;
                _notifyIcon.Visible = true;

                var exitMenuItem = new ToolStripMenuItem();
                exitMenuItem.Text = "Exit";
                exitMenuItem.Click += HandleExitApplicationMenuItemClick;

                var aboutMenuItem = new ToolStripMenuItem();
                aboutMenuItem.Text = "About...";
                aboutMenuItem.Click += HandleAboutMenuItemClick;

                _contextMenu.Items.Add(aboutMenuItem);
                _contextMenu.Items.Add(new ToolStripSeparator());
                _contextMenu.Items.Add(exitMenuItem);

                // Open the host channel for communication from SharePoint
                ResetHost();
            }
            catch 
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                }

                throw;
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ApplicationContext"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_components != null)
            {
                _components.Dispose();
                _components = null;
            }
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            if (_contextMenu != null)
            {
                _contextMenu.Dispose();
                _contextMenu = null;
            }
            if (_endService != null)
            {
                _endService.Close();
                _endService = null;
            }
            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }

        /// <summary>
        /// Handles the host faulted event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleHostFaulted(object sender, EventArgs e)
        {
            // Launch a thread to reset the host since it is in a faulted state.
            Thread resetThread = new Thread(ResetHostWithSleep);
            resetThread.Start();
        }


        /// <summary>
        /// Handles the host closed event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleHostClosed(object sender, EventArgs e)
        {
            lock (_lock)
            {
                if (_host != null)
                {
                    _host.Closed -= HandleHostClosed;
                    _host = null;
                }

                // If the service is still running, then respawn the host
                if (!_endService.WaitOne(0))
                {
                    ResetHost();
                }
            }
        }

        /// <summary>
        /// Closes the service host.
        /// </summary>
        void CloseHost()
        {
            lock (_lock)
            {
                if (_host != null)
                {
                    _host.Closed -= HandleHostClosed;
                    _host.Close();
                    _host = null;
                }
            }
        }

        /// <summary>
        /// Aborts the service host. This should only be called if there was an
        /// error initializing it.
        /// </summary>
        void AbortHost()
        {
            if (_host != null)
            {
                _host.Abort();
                _host = null;
            }
        }

        /// <summary>
        /// Handles starting/resetting the service host, but sleeps before
        /// resetting the service.
        /// </summary>
        void ResetHostWithSleep()
        {
            Thread.Sleep(1000);
            ResetHost();
        }

        /// <summary>
        /// Handles starting/resetting the service host.
        /// </summary>
        void ResetHost()
        {
            lock (_lock)
            {
                if (_host != null)
                {
                    CloseHost();
                }

                try
                {
                    var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
                    _host = new ServiceHost(typeof(IDShieldForSPClientHandler));
                    var address = string.Concat("http://localhost:",
                        RedactNowData.IdShieldClientPort, "/", RedactNowData.IdShieldForSPClientEndpoint);
                    _host.AddServiceEndpoint(typeof(IIDShieldForSPClient), binding, address);
                    _host.Open();
                    _host.Faulted += HandleHostFaulted;
                    _host.Closed += HandleHostClosed;
                }
                catch (Exception)
                {
                    AbortHost();
                    throw;
                }
 
            }
        }

        /// <summary>
        /// Handles the exit application menu item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleExitApplicationMenuItemClick(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;

            try
            {
                _endService.Set();
                CloseHost();
            }
            finally
            {
                base.ExitThreadCore();
            }
        }

        /// <summary>
        /// Handles the about menu item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleAboutMenuItemClick(object sender, EventArgs e)
        {
            using (var about = new IdShieldSPClientAboutBox())
            {
                about.ShowDialog();
            }
        }

        #endregion Methods
    }
}
