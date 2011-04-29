using EnterpriseDT.Net.Ftp;
using EnterpriseDT.Net.Ftp.Forms;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Control to configure the connection info for an FTP server
    /// </summary>
    public partial class FtpConnectionSettingsControl : UserControl
    {
        #region Properties

        /// <summary>
        /// Number of connections that will be used to download files
        /// </summary>
        public int NumberOfConnections 
        {
            set
            {
                _numberConnections.Value = value;
            }
            get
            {
                return (int)_numberConnections.Value;
            }
        }

        /// <summary>
        /// Connection object used to get the settings for connecting to the FTP Server
        /// </summary>
        [CLSCompliant(false)]
        public SecureFTPConnection FtpConnection 
        {
            set
            {
                try
                {
                    // The properties to display in the connection editor control may
                    // be changed when setting the inital values so save the properties so 
                    // they can be restored later.
                    FTPConnectionProperties saveProperties = _ftpConnectionEditor.Properties;

                    _ftpConnectionEditor.Connection = value;

                    // Reset the properties that should be displayed by the connection editor
                    _ftpConnectionEditor.Properties = saveProperties;
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI32396");
                }
            }
            get
            {
                return (SecureFTPConnection)_ftpConnectionEditor.Connection;
            }
        }

        /// <summary>
        /// Indicates if the "number of connections" control should be displayed
        /// </summary>
        public bool ShowConnectionsControl
        {
            set
            {
                _numberConnections.Visible = value;
                _connectionsLabel.Visible = value;
            }
            get
            {
                return _numberConnections.Visible;
            }
        }
                

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionSettingsControl"/>
        /// </summary>
        public FtpConnectionSettingsControl()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionSettingsControl"/>
        /// </summary>
        /// <param name="container">Container with components to add to the control</param>
        public FtpConnectionSettingsControl(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion
        
        #region Event Handlers

        /// <summary>
        /// Handles the UserTextCorrected event for the number of connections control
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleNumberConnectionsNumericUpDownUserTextCorrected(object sender, EventArgs e)
        {
            try
            {
                UtilityMethods.ShowMessageBox(
                    "The number of connections must be between 1 and 10.",
                    "Invalid number of connections", true);

                // Re-select the _numberConnections control, but only after any other events
                // in the message queue have been processed so those event don't undo this selection.
                BeginInvoke((MethodInvoker)(() =>
                {
                    try
                    {
                        _numberConnections.Focus();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractDisplay("ELI32251");
                    }
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32252");
            }
        }
        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the Test Connection button
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>        
        void HandleTestConnection(object sender, EventArgs e)
        {
            try
            {
                _ftpConnectionEditor.Connection.Connect();
                if (_ftpConnectionEditor.Connection.IsConnected)
                {
                    MessageBox.Show("Connection was successful", "Test Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
                }
                else
                {
                    MessageBox.Show("Connection attempt was unsuccessful", "Test Connection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
                }
                _ftpConnectionEditor.Connection.Close();
            }
            catch (Exception ex)
            {
                // Wrap the exception so that it is clear the connection was unsuccessful
                ExtractException ee = new ExtractException("ELI32037", "Connection attempt was unsuccessful.", ex);
                ee.Display();
            }
        }

        #endregion
        
        #region Public methods

        /// <summary>
        /// Checks if items are configured correctly
        /// </summary>
        /// <returns></returns>
        public bool IsConfigurationValid()
        {
            bool returnValue = false;
            try
            {

                if (string.IsNullOrWhiteSpace(_ftpConnectionEditor.Connection.ServerAddress))
                {
                    MessageBox.Show("Server address must be specified for ftp connection.", "Configuration error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                    _ftpConnectionEditor.Focus();
                }
                else if (string.IsNullOrWhiteSpace(_ftpConnectionEditor.Connection.UserName))
                {
                    MessageBox.Show("UserName must be specified for the ftp connection.", "Configuration error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                    _ftpConnectionEditor.Focus();
                }
                else if (string.IsNullOrWhiteSpace(_ftpConnectionEditor.Connection.Password))
                {
                    MessageBox.Show("Password must be specified for the ftp connection.", "Configuration error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                    _ftpConnectionEditor.Focus();
                }
                else
                {
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32397");
            }
            return returnValue;
        }

        #endregion
    }
}
