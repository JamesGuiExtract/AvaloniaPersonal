using EnterpriseDT.Net.Ftp;
using EnterpriseDT.Net.Ftp.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileSuppliers
{
    /// <summary>
    /// Form to get the settings for the FtpFileSupplier
    /// </summary>
    public partial class FtpFileSupplierSettingsDialog : Form
    {
        #region Fields
        
        // FtpFileSupplier that contains the settings passed to the constructor
        FtpFileSupplier _settings;

        #endregion

        #region Properties
        
        /// <summary>
        /// Settings property that returns the current applied settings
        /// </summary>
        public FtpFileSupplier Settings
        {
            get
            {
                return _settings;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileSupplierSettingsDialog"/> class
        /// </summary>
        public FtpFileSupplierSettingsDialog():this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileSupplierSettingsDialog"/> class
        /// with settings
        /// </summary>
        /// <param name="settings"><see cref="FtpFileSupplier"/> that has the intial settings.</param>
        public FtpFileSupplierSettingsDialog(FtpFileSupplier settings)
        {
            InitializeComponent();

            // Set the settings for the dialog
            _settings = settings ?? new FtpFileSupplier();
        }

        
        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // The properties to display in the connection editor control may
                // be changed when setting the inital values so save the properties so 
                // they can be restored later.
                FTPConnectionProperties saveProperties = _ftpConnectionEditor.Properties;

                // Set the initial values for the controls
                _remoteDownloadFolderTextBox.Text = _settings.RemoteDownloadFolder;
                _fileExtensionSpecificationTextBox.Text = _settings.FileExtensionsToDownload;
                _recursiveDownloadCheckBox.Checked = _settings.RecursivelyDownload;
                _pollRemoteCheckBox.Checked = _settings.PollRemoteLocation;
                _pollingIntervalNumericUpDown.Value = _settings.PollingIntervalInMinutes;
                _numberConnections.Value = _settings.NumberOfConnections;

                // Set the AfterDownloadAction radio buttons
                switch (_settings.AfterDownloadAction)
                {
                    case AfterDownloadRemoteFileActon.DeleteRemoteFile:
                        _deleteRemoteFileRadioButton.Checked = true;
                        break;
                    case AfterDownloadRemoteFileActon.ChangeRemoteFileExtension:
                        _changeRemoteExtensionRadioButton.Checked = true;
                        break;
                    case AfterDownloadRemoteFileActon.DoNothingToRemoteFile:
                        _doNothingRadioButton.Checked = true;
                        break;
                    default:
                       break;
                }

                _newExtensionTextBox.Text = _settings.NewExtensionForRemoteFile;
                _localWorkingFolderTextBox.Text = _settings.LocalWorkingFolder;

                _secureFTPConnection = _settings.ConfiguredFtpConnection ?? new SecureFTPConnection();
                
                // Set the ftp connection editor connection
                _ftpConnectionEditor.Connection = _secureFTPConnection;

                // Reset the properties that should be displayed by the connection editor
                _ftpConnectionEditor.Properties = saveProperties;
                
                UpdateControlState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32016");
            }
        }

        #endregion Overrides
        
        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the Ok button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>        
        void HandleOkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!IsConfigurationValid())
                {
                    DialogResult = DialogResult.None;
                    return;
                }

                // Transfer the settings from the controls to the _settings field
                _settings.RemoteDownloadFolder = _remoteDownloadFolderTextBox.Text;
                _settings.FileExtensionsToDownload = _fileExtensionSpecificationTextBox.Text;
                _settings.RecursivelyDownload = _recursiveDownloadCheckBox.Checked;
                _settings.PollRemoteLocation = _pollRemoteCheckBox.Checked;
                _settings.PollingIntervalInMinutes = (int)_pollingIntervalNumericUpDown.Value;
                _settings.NumberOfConnections = (int)_numberConnections.Value;

                if (_doNothingRadioButton.Checked)
                {
                    _settings.AfterDownloadAction = AfterDownloadRemoteFileActon.DoNothingToRemoteFile;
                }
                else if (_deleteRemoteFileRadioButton.Checked)
                {
                    _settings.AfterDownloadAction = AfterDownloadRemoteFileActon.DeleteRemoteFile;
                }
                else if (_changeRemoteExtensionRadioButton.Checked)
                {
                    _settings.AfterDownloadAction = AfterDownloadRemoteFileActon.ChangeRemoteFileExtension;
                }

                _settings.NewExtensionForRemoteFile = _newExtensionTextBox.Text;
                _settings.LocalWorkingFolder = _localWorkingFolderTextBox.Text;

                _settings.ConfiguredFtpConnection = (SecureFTPConnection) _ftpConnectionEditor.Connection;

                // Still need to verify settings
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32019");
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
                        MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0 );
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

        /// <summary>
        /// Handles click event for all check boxes and radio buttons in order 
        /// to update the state of the controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleCheckBoxOrRadioClick(object sender, EventArgs e)
        {
            try
            {
                UpdateControlState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32140");
            }
        }

        /// <summary>
        /// Handles the UserTextCorrected event for the polling interval control
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePollingIntervalNumericUpDownUserTextCorrected(object sender, EventArgs e)
        {
            try
            {
                UtilityMethods.ShowMessageBox(
                    "The polling interval must be between 1 and 1,440 minutes.",
                    "Invalid polling interval", true);

                // Re-select the _pollingIntervalNumericUpDown control, but only after any other events
                // in the message queue have been processed so those event don't undo this selection.
                BeginInvoke((MethodInvoker)(() =>
                {
                    try
                    {
                        _settingsTabControl.SelectedTab = _generalSettingsTabPage;
                        _pollingIntervalNumericUpDown.Focus();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractDisplay("ELI32249");
                    }
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32250");
            }
        }

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
                        _settingsTabControl.SelectedTab = _connectionSettingsTabPage;
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

        #endregion

        #region Helper functions
        
        /// <summary>
        /// Updates the enabled status of controls based on the current value of related controls
        /// </summary>
        void UpdateControlState()
        {
            _pollingIntervalNumericUpDown.Enabled = _pollRemoteCheckBox.Checked;
            _doNothingRadioButton.Enabled = !_pollRemoteCheckBox.Checked;
            _newExtensionTextBox.Enabled = _changeRemoteExtensionRadioButton.Checked;
            if (_doNothingRadioButton.Checked && _pollRemoteCheckBox.Checked)
            {
                _deleteRemoteFileRadioButton.Checked = true;
            }
        }

        /// <summary>
        /// Checks the controls for valid values and will set focus to the 
        /// first invalid control.
        /// </summary>
        /// <returns><see lang="true"/>If all of the controls have valid values, 
        /// otherwise returns <see lang="false"/>.</returns>
        bool IsConfigurationValid()
        {
            bool returnValue = false;
            if (string.IsNullOrWhiteSpace(_remoteDownloadFolderTextBox.Text))
            {
                MessageBox.Show("Remote download folder must be specified.", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _remoteDownloadFolderTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(_fileExtensionSpecificationTextBox.Text))
            {
                MessageBox.Show("Extensions to download must be specified", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _fileExtensionSpecificationTextBox.Focus();
            }
            else if (_pollRemoteCheckBox.Checked &&
                _pollingIntervalNumericUpDown.Value < 1)
            {
                MessageBox.Show("Polling interval must be greater than 0.", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _pollingIntervalNumericUpDown.Focus();
            }
            else if (_changeRemoteExtensionRadioButton.Checked && string.IsNullOrWhiteSpace(_newExtensionTextBox.Text))
            {
                MessageBox.Show("New extension for remote file must be specified.", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _newExtensionTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(_localWorkingFolderTextBox.Text))
            {
                MessageBox.Show("Local working folder must be specified.", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _localWorkingFolderTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(_ftpConnectionEditor.Connection.ServerAddress))
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
            return returnValue;
        }

        #endregion
    }
}
