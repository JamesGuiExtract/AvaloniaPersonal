using EnterpriseDT.Net.Ftp;
using Extract.Utilities;
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

                // Set the initial values for the controls
                _remoteDownloadFolderTextBox.Text = _settings.RemoteDownloadFolder;
                _fileExtensionSpecificationTextBox.Text = _settings.FileExtensionsToDownload;
                _recursiveDownloadCheckBox.Checked = _settings.RecursivelyDownload;
                _pollRemoteCheckBox.Checked = _settings.PollRemoteLocation;
                _pollingIntervalNumericUpDown.Value = _settings.PollingIntervalInMinutes;
                _ftpConnectionSettingsControl.NumberOfConnections = _settings.NumberOfConnections;
                _ftpConnectionSettingsControl.TimeBetweenRetries = _settings.TimeToWaitBetweenRetries;
                _ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure = _settings.NumberOfTimesToRetry;

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
                _ftpConnectionSettingsControl.FtpConnection = _secureFTPConnection;
                
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
                _settings.NumberOfConnections = _ftpConnectionSettingsControl.NumberOfConnections;
                _settings.NumberOfTimesToRetry = _ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure;
                _settings.TimeToWaitBetweenRetries = _ftpConnectionSettingsControl.TimeBetweenRetries;

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

                _settings.ConfiguredFtpConnection = _ftpConnectionSettingsControl.FtpConnection;

                // Still need to verify settings
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32019");
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
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _remoteDownloadFolderTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(_fileExtensionSpecificationTextBox.Text))
            {
                MessageBox.Show("Extensions to download must be specified", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _fileExtensionSpecificationTextBox.Focus();
            }
            else if (_pollRemoteCheckBox.Checked &&
                _pollingIntervalNumericUpDown.Value < 1)
            {
                MessageBox.Show("Polling interval must be greater than 0.", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _pollingIntervalNumericUpDown.Focus();
            }
            else if (_changeRemoteExtensionRadioButton.Checked && string.IsNullOrWhiteSpace(_newExtensionTextBox.Text))
            {
                MessageBox.Show("New extension for remote file must be specified.", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _newExtensionTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(_localWorkingFolderTextBox.Text))
            {
                MessageBox.Show("Local working folder must be specified.", "Configuration error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _localWorkingFolderTextBox.Focus();
            }
            else if (!_ftpConnectionSettingsControl.IsConfigurationValid())
            {
                _settingsTabControl.SelectTab(_connectionSettingsTabPage);
                _ftpConnectionSettingsControl.Focus();
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
