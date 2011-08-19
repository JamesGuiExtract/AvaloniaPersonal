using EnterpriseDT.Net.Ftp;
using EnterpriseDT.Net.Ftp.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
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
        /// Number of unsuccessful retries before letting the operation fail.
        /// </summary>
        public int NumberOfRetriesBeforeFailure
        {
            set
            {
                _numberRetriesControl.Value = value;
            }
            get
            {
                return (int)_numberRetriesControl.Value;
            }
        }

        /// <summary>
        /// Time to wait between retrying commands to the FTP server
        /// </summary>
        public int TimeBetweenRetries
        {
            set
            {
                _timeBetweenRetriesControl.Int32Value = value;
            }
            get
            {
                return _timeBetweenRetriesControl.Int32Value;
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
                using (new TemporaryWaitCursor())
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
            }
            catch (Exception ex)
            {
                // Wrap the exception so that it is clear the connection was unsuccessful
                ExtractException ee = new ExtractException("ELI32037", "Connection attempt was unsuccessful.", ex);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for Show advanced check box
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>        
        void HandleAdvanced(object sender, EventArgs e)
        {
            try
            {
                if (!_showAdvancedSettingsCheckBox.Checked)
                {
                    _ftpConnectionEditor.Properties = GetSimpleFtpConnectionProperties();
                }
                else
                {
                    _ftpConnectionEditor.Properties = GetAdvancedFtpConnectionProperties();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32559");
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

        #region Private Methods

        /// <summary>
        /// Gets all the properties to be displayed when viewing advanced connection settings
        /// </summary>
        /// <returns>Connection properties that should be displayed for the advanced settings</returns>
        static FTPConnectionProperties GetAdvancedFtpConnectionProperties()
        {
            FTPConnectionProperties advancedProperties = new FTPConnectionProperties();
            
            advancedProperties.AddCategory("Connection", "Connection", true);
            advancedProperties.AddProperty("Connection", "Protocol", "Protocol", "File transfer protocol to use.", true, 0);
            advancedProperties.AddProperty("Connection", "ServerAddress", "Server Address", "The domain-name or IP address of the FTP server.", true, 1);
            advancedProperties.AddProperty("Connection", "ServerPort", "Server Port", "Port on the server to which to connect the control-channel.", true, 2);
            advancedProperties.AddProperty("Connection", "UserName", "User Name", "User-name of account on the server.", true, 3);
            advancedProperties.AddProperty("Connection", "Password", "Password", "Password of account on the server.", true, 4);
            advancedProperties.AddProperty("Connection", "ProxySettings", "Proxy Settings", "Settings for HTTP and SOCKS proxies.", true, 9);
            advancedProperties.AddProperty("Connection", "KeepAliveIdle", "Keep Alive Idle", "If KeepAliveIdle is set then the client will periodically contact the server in i" +
        "dle time so that the connection doesn\'t time out.", true, 10);
            advancedProperties.AddProperty("Connection", "KeepAliveTransfer", "Keep Alive Transfer", "If KeepAliveTransfer is set then the client will periodically contact the server " +
        "during large transfers so that the connection doesn\'t time out.", true, 11);
            advancedProperties.AddProperty("Connection", "KeepAlivePeriodSecs", "Keep Alive Period Secs", "KeepAlivePeriodSecs is the period (in seconds) at which the client contacts the s" +
        "erver so that the connection doesn\'t time out.", true, 12);
            advancedProperties.AddProperty("Connection", "ConcurrentTransferSettings", "Concurrent Transfer Settings", "Settings for concurrent transfers.", true, 13);
            advancedProperties.AddProperty("Connection", "Name", "Name", "Name of the connection.", true, 14);
            advancedProperties.AddCategory("FTP/FTPS", "FTP/FTPS", true);
            advancedProperties.AddProperty("FTP/FTPS", "AccountInfo", "Account Info", "Account information string used in FTP/FTPS.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "ActivePortRange", "Active Port Range", "Specifies the range of ports to be used for data-channels in active mode.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "AutoLogin", "Auto Login", "Determines if the component will automatically log in upon connection.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "AutoPassiveIPSubstitution", "Auto Passive IPSubstitution", "Ensures that data-socket connections are made to the same IP address that the con" +
        "trol socket is connected to.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "AutoSecure", "Auto Secure", " Determines if the component will automatically switch to SSL/TLS in upon connect" +
        "ion.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "CipherSuites", "Cipher Suites", " Get/sets the cipher-suites permissible during establishment of a secure connecti" +
        "on.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "ClientCertificate", "Client Certificate", " The certificate to be presented to the server.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "ConnectMode", "Connect Mode", "The connection-mode of data-channels.  Usually passive when FTP client is behind " +
        "a firewall.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "DirectoryEmptyMessages", "Directory Empty Messages", "Holds fragments of server messages that indicate a directory is empty.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "FileNotFoundMessages", "File Not Found Messages", "Holds fragments of server messages that indicate a file was not found.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "IntegrityCheckTransfers", "Integrity Check Transfers", "Enable integrity checking for transfers in binary mode.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "MultiTransferCountBeforeSleep", "Multi Transfer Count Before Sleep", "Number of transfers before \"sleeping\" during multiple FTP/FTPS data transfers.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "MultiTransferSleepEnabled", "Multi Transfer Sleep Enabled", "Determines whether or not \"sleeping\" is enabled during multiple FTP/FTPS data tra" +
        "nsfers.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "MultiTransferSleepTime", "Multi Transfer Sleep Time", "Number of seconds spent \"sleeping\" during multiple FTP/FTPS data transfers.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "ParsingCulture", "Parsing Culture", "The culture for parsing file listings.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "PublicIPAddress", "Public IPAddress", "IP address of the client as the server sees it.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "ServerCommonName", "Server Common Name", " The name to be used when performing a name-check during the validation of the se" +
        "rver certificate.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "ServerValidationCertificate", "Server Validation Certificate", "The certificate used to validate the server\'s certificate (only if it is self-sig" +
        "ned).", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "ShowHiddenFiles", "Show Hidden Files", "Include hidden files in operations that involve directory listings.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "SSLVersion", "SSLVersion", " SSL/TLS version to use.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "StrictReturnCodes", "Strict Return Codes", "Controls whether or not checking of return codes is strict.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "SynchronizePassiveConnections", "Synchronize Passive Connections", "Used to synchronize the creation of passive data sockets.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "TimeDifference", "Time Difference", "Time difference between server and client (relative to client).", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "TransferCompleteMessages", "Transfer Complete Messages", "Holds fragments of server messages that indicate a transfer completed.", true, -1);
            advancedProperties.AddProperty("FTP/FTPS", "UseUnencryptedCommands", "Use Unencrypted Commands", " Switches to an unencrypted command channel", true, -1);
            advancedProperties.AddCategory("FTP/FTPS/HTTP", "FTP/FTPS/HTTP", true);
            advancedProperties.AddProperty("FTP/FTPS/HTTP", "DataEncoding", "Data Encoding", "The character-encoding to use for data transfers in ASCII mode only.", true, -1);
            advancedProperties.AddCategory("FTP/FTPS/SFTP", "FTP/FTPS/SFTP", true);
            advancedProperties.AddProperty("FTP/FTPS/SFTP", "CommandEncoding", "Command Encoding", "The character-encoding to use for commands.", true, -1);
            advancedProperties.AddCategory("FTPS/SFTP", "FTPS/SFTP", true);
            advancedProperties.AddProperty("FTPS/SFTP", "ServerCompatibility", "Server Compatibility", " Controls various server security compatibility features.", true, -1);
            advancedProperties.AddProperty("FTPS/SFTP", "ServerValidation", "Server Validation", "Method by which the server\'s certificate (FTPS) or public key (SFTP) is validated" +
        ".", true, -1);
            advancedProperties.AddCategory("SFTP", "SFTP", true);
            advancedProperties.AddProperty("SFTP", "AuthenticationMethod", "Authentication Method", "Current authentication type being used", true, -1);
            advancedProperties.AddProperty("SFTP", "ClientPrivateKeyFile", "Client Private Key File", "Path of the private key file used to authenticate the client.", true, -1);
            advancedProperties.AddProperty("SFTP", "ClientPrivateKeyPassphrase", "Client Private Key Passphrase", "Passphrase of the private key file used to authenticate the client.", true, -1);
            advancedProperties.AddProperty("SFTP", "KBIPrompts", "KBIPrompts", "List of prompts used for keyboard interactive authentication", true, -1);
            advancedProperties.AddProperty("SFTP", "KnownHosts", "Known Hosts", "Manager of the list of known hosts used to authenticate the server.", true, -1);
            advancedProperties.AddProperty("SFTP", "ParallelTransferMode", "Parallel Transfer Mode", "Parallel transfer mode", true, -1);
            advancedProperties.AddProperty("SFTP", "PreferredCipherAlgorithms", "Preferred Cipher Algorithms", "Preferred algorithms to use for encryption", true, -1);
            advancedProperties.AddProperty("SFTP", "PreferredCompressionAlgorithms", "Preferred Compression Algorithms", "Preferred algorithms to use for compression", true, -1);
            advancedProperties.AddProperty("SFTP", "PreferredHostKeyAlgorithms", "Preferred Host Key Algorithms", "Preferred algorithms to use for server authentication via public key", true, -1);
            advancedProperties.AddProperty("SFTP", "PreferredKeyExchangeMethods", "Preferred Key Exchange Methods", "Preferred key exchange methods to use", true, -1);
            advancedProperties.AddProperty("SFTP", "PreferredMACAlgorithms", "Preferred MACAlgorithms", "Preferred MAC algorithms to use", true, -1);
            advancedProperties.AddProperty("SFTP", "SendInitialWindowAdjust", "Send Initial Window Adjust", "Sets whether or not an initial WINDOW_ADJUST message is sent.", true, -1);
            advancedProperties.AddProperty("SFTP", "ServerLineTerminator", "Server Line Terminator", "Server line terminator to use", true, -1);
            advancedProperties.AddProperty("SFTP", "SSHMaxPacketSize", "SSHMax Packet Size", "SSH maximum packet size.", true, -1);
            advancedProperties.AddProperty("SFTP", "SSHWindowSize", "SSHWindow Size", "SSH window size", true, -1);
            advancedProperties.AddProperty("SFTP", "UMask", "UMask", "File creation mode mask applied to the default permissions to create the final pe" +
        "rmission set.", true, -1);
            advancedProperties.AddCategory("Transfer", "Transfer", true);
            advancedProperties.AddProperty("Transfer", "CacheListings", "Cache Listings", "Cache directory listings", true, -1);
            advancedProperties.AddProperty("Transfer", "CloseStreamsAfterTransfer", "Close Streams After Transfer", "Determines if stream-based transfer-methods should close the stream once the tran" +
        "sfer is completed.", true, -1);
            advancedProperties.AddProperty("Transfer", "CompressionPreferred", "Compression Preferred", "Enable/disable compression for transfers.", true, -1);
            advancedProperties.AddProperty("Transfer", "DefaultSyncRules", "Default Sync Rules", "Rules that are used during synchronization operations when no other rules are spe" +
        "cified.", true, -1);
            advancedProperties.AddProperty("Transfer", "DeleteOnFailure", "Delete On Failure", "Controls whether or not a file is deleted when a failure occurs while it is trans" +
        "ferred.", true, -1);
            advancedProperties.AddProperty("Transfer", "DetectTransferMode", "Detect Transfer Mode", "Determines whether the transfer mode in operations involving multiple files is au" +
        "tomatically changed between ASCII and binary as appropriate.", true, -1);
            advancedProperties.AddProperty("Transfer", "MaxTransferRate", "Max Transfer Rate", "Controls maximum transfer rate in bytes/sec.", true, -1);
            advancedProperties.AddProperty("Transfer", "Timeout", "Timeout", "TCP timeout (in milliseconds) on the underlying sockets (0 means none).", true, -1);
            advancedProperties.AddProperty("Transfer", "TransferBufferSize", "Transfer Buffer Size", "The size of the buffers used in writing to and reading from the data sockets.", true, -1);
            advancedProperties.AddProperty("Transfer", "TransferNotifyInterval", "Transfer Notify Interval", "The number of bytes transferred between each notification of the BytesTransferred" +
        " event.", true, -1);
            advancedProperties.AddProperty("Transfer", "TransferNotifyListings", "Transfer Notify Listings", "Controls if BytesTransferred event is triggered during directory listings.", true, -1);
            advancedProperties.AddProperty("Transfer", "TransferType", "Transfer Type", "The type of file transfer to use, i.e. BINARY or ASCII.", true, -1);

            return advancedProperties;
        }

        /// <summary>
        /// Gets all the properties to be displayed when viewing simple connection settings
        /// </summary>
        /// <returns>Connection properties that should be displayed for the advanced settings</returns>
        static FTPConnectionProperties GetSimpleFtpConnectionProperties()
        {
            FTPConnectionProperties simpleProperties = new FTPConnectionProperties();

            simpleProperties.AddCategory("Connection", "Connection", true);
            simpleProperties.AddProperty("Connection", "Protocol", "Protocol", "File transfer protocol to use.", true, 0);
            simpleProperties.AddProperty("Connection", "ServerAddress", "Server Address", "The domain-name or IP address of the FTP server.", true, 1);
            simpleProperties.AddProperty("Connection", "ServerPort", "Server Port", "Port on the server to which to connect the control-channel.", true, 2);
            simpleProperties.AddProperty("Connection", "UserName", "User Name", "User-name of account on the server.", true, 3);
            simpleProperties.AddProperty("Connection", "Password", "Password", "Password of account on the server.", true, 4);

            return simpleProperties;
        }

        #endregion

    }
}
