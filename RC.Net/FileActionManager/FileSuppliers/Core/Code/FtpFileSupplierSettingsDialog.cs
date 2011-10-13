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
                _remoteDownloadFolderTextBox.Text = string.IsNullOrWhiteSpace(_settings.RemoteDownloadFolder) ?
                    "/" : _settings.RemoteDownloadFolder;
                _fileExtensionSpecificationTextBox.Text = _settings.FileExtensionsToDownload;
                _recursiveDownloadCheckBox.Checked = _settings.RecursivelyDownload;
                _downloadOnceRadioButton.Checked = _settings.PollingMethod == PollingMethod.NoPolling;
                _pollContinuouslyRadioButton.Checked = _settings.PollingMethod == PollingMethod.Continuously;
                _pollAtSetTimesRadioButton.Checked = _settings.PollingMethod == PollingMethod.SetTimes;
                _pollingIntervalNumericUpDown.Value = _settings.PollingIntervalInMinutes;
                _pollingTimesTextBox.Text = _settings.GetPollingTimesAsText();
                _ftpConnectionSettingsControl.NumberOfConnections = _settings.NumberOfConnections;
                _ftpConnectionSettingsControl.TimeBetweenRetries = _settings.TimeToWaitBetweenRetries;
                _ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure = _settings.NumberOfTimesToRetry;

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
                if (_downloadOnceRadioButton.Checked)
                {
                    _settings.PollingMethod = PollingMethod.NoPolling;
                }
                else if (_pollContinuouslyRadioButton.Checked)
                {
                    _settings.PollingMethod = PollingMethod.Continuously;
                }
                else
                {
                    _settings.PollingMethod = PollingMethod.SetTimes;
                }

                _settings.PollingIntervalInMinutes = (int)_pollingIntervalNumericUpDown.Value;

                try
                {
                    _settings.PollingTimes = FtpFileSupplier.ConvertTextToTimes(_pollingTimesTextBox.Text);
                }
                catch
                {
                    // Eat any exceptions parsing the set times unless PollingMethod.SetTimes is selected.
                    if (_settings.PollingMethod == PollingMethod.SetTimes)
                    {
                        throw;
                    }
                }

                _settings.NumberOfConnections = _ftpConnectionSettingsControl.NumberOfConnections;
                _settings.NumberOfTimesToRetry = _ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure;
                _settings.TimeToWaitBetweenRetries = _ftpConnectionSettingsControl.TimeBetweenRetries;

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

        /// <summary>
        /// Handles the polling method checked changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePollingMethodCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControlState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33995");
            }
        }

        #endregion

        #region Helper functions
        
        /// <summary>
        /// Updates the enabled status of controls based on the current value of related controls
        /// </summary>
        void UpdateControlState()
        {
            _pollingIntervalNumericUpDown.Enabled = _pollContinuouslyRadioButton.Checked;
            _pollingTimesTextBox.Enabled = _pollAtSetTimesRadioButton.Checked;
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
                UtilityMethods.ShowMessageBox(
                    "Remote download folder must be specified.\r\nIf you wish to use the home directory use '/'.",
                    "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _remoteDownloadFolderTextBox.Focus();
            }
            else if (_remoteDownloadFolderTextBox.Text.Trim()[0] == '.')
            {
                UtilityMethods.ShowMessageBox("Remote download folder name cannot begin with '.'.", 
                    "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _remoteDownloadFolderTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(_fileExtensionSpecificationTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("Extensions to download must be specified", 
                    "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _fileExtensionSpecificationTextBox.Focus();
            }
            else if (_pollContinuouslyRadioButton.Checked &&
                _pollingIntervalNumericUpDown.Value < 1)
            {
                UtilityMethods.ShowMessageBox("Polling interval must be greater than 0.", 
                    "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _pollingIntervalNumericUpDown.Focus();
            }
            else if (_pollAtSetTimesRadioButton.Checked &&
                string.IsNullOrWhiteSpace(_pollingTimesTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "Specify times to poll the server in a comma separated list",
                    "Configuration error", true);

                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _pollingTimesTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(_localWorkingFolderTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("Local working folder must be specified.",
                    "Configuration error", true);
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
                try
                {
                    if (_pollAtSetTimesRadioButton.Checked)
                    {
                        FtpFileSupplier.ConvertTextToTimes(_pollingTimesTextBox.Text);
                    }

                    // If ConvertTextToTimes did not throw an exception, its value is valid.
                    returnValue = true;
                }
                catch (Exception ex)
                {
                    UtilityMethods.ShowMessageBox(ex.Message, "Configuration error", true);
                    _settingsTabControl.SelectTab(_generalSettingsTabPage);
                    _pollingTimesTextBox.Focus();
                }
            }
            return returnValue;
        }

        #endregion
    }
}
