using EnterpriseDT.Net.Ftp;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Configuration Dialog for <see cref="FtpTask"/>
    /// </summary>
    public partial class FtpTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The label to use for the _remoteOrOldFileNameTextBox when renaming.
        /// </summary>
        const string _RENAME_OLD_FILE_LABEL = "Existing remote filename";
        
        /// <summary>
        /// The label to use for the _localOrNewFileNameTextBox when renaming.
        /// </summary>
        const string _RENAME_NEW_FILE_LABEL = "New remote filename";

        /// <summary>
        /// The label to use for the _remoteOrOldFileNameTextBox when not renaming.
        /// </summary>
        const string _REMOTE_FILE_LABEL = "Remote filename";
        
        /// <summary>
        /// The label to use for the _localOrNewFileNameTextBox when not renaming.
        /// </summary>
        const string _LOCAL_FILE_LABEL = "Local filename";

        #endregion Constants

        #region Fields
        
        // FtpFileTransferTask that contains the configured settings
        FtpTask _settings;
        
        #endregion
        
        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public FtpTask Settings 
        { 
            get
            {
                return _settings;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpTaskSettingsDialog"/> class
        /// </summary>
        public FtpTaskSettingsDialog():this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpTaskSettingsDialog"/> class
        /// with settings
        /// </summary>
        /// <param name="settings"><see cref="FtpTask"/> that has the intial settings.</param>
        public FtpTaskSettingsDialog(FtpTask settings)
        {
            InitializeComponent();

            // Initialize the Path tags buttons to have <SourceDocName>, <FPSFileDir> and <RemoteSourceDocName> tags
            _localFileNamePathTagsButton.PathTags = new FileActionManagerPathTags("", "", "");
            _remoteFileNamePathTagsButton.PathTags = new FileActionManagerPathTags("", "", "");

            _settings = settings ?? new FtpTask();
        }

        #endregion

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
                _remoteOrOldFileNameTextBox.Text = _settings.RemoteOrOldFileName;
                _localOrNewFileNameTextBox.Text = _settings.LocalOrNewFileName;

                // Set the AfterDownloadAction radio buttons
                switch (_settings.ActionToPerform)
                {
                    case EFTPAction.kDeleteFileFromFtpServer:
                        _deleteFileRadioButton.Checked = true;
                        break;
                    case EFTPAction.kRenameFileOnFtpServer:
                        _renameFileRadioButton.Checked = true;
                        break;
                    case EFTPAction.kDownloadFileFromFtpServer:
                        _downloadFileRadioButton.Checked = true;
                        break;
                    case EFTPAction.kUploadFileToFtpServer:
                        _uploadFileRadioButton.Checked = true;
                        break;
                    default:
                        break;
                }

                _deleteEmptyFolderCheckBox.Checked = _settings.DeleteEmptyFolder;

                _ftpConnectionSettingsControl.FtpConnection = 
                    _settings.ConfiguredFtpConnection ?? new SecureFTPConnection();

                _ftpConnectionSettingsControl.TimeBetweenRetries = Settings.TimeToWaitBetweenRetries;
                _ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure = Settings.NumberOfTimesToRetry;
                _ftpConnectionSettingsControl.ReestablishConnectionBeforeRetry =
                    Settings.ReestablishConnectionBeforeRetry;
                _ftpConnectionSettingsControl.KeepConnectionOpen = Settings.KeepConnectionOpen;

                UpdateControlState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32566");
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
                _settings.RemoteOrOldFileName = _remoteOrOldFileNameTextBox.Text;
                if (!_deleteFileRadioButton.Checked)
                {
                    _settings.LocalOrNewFileName = _localOrNewFileNameTextBox.Text;
                }
                if (_uploadFileRadioButton.Checked)
                {
                    _settings.ActionToPerform = EFTPAction.kUploadFileToFtpServer;
                }
                else if (_downloadFileRadioButton.Checked)
                {
                    _settings.ActionToPerform = EFTPAction.kDownloadFileFromFtpServer;
                }
                else if (_renameFileRadioButton.Checked)
                {
                    _settings.ActionToPerform = EFTPAction.kRenameFileOnFtpServer;
                }
                else if (_deleteFileRadioButton.Checked)
                {
                    _settings.ActionToPerform = EFTPAction.kDeleteFileFromFtpServer;
                }
                else
                {
                    throw new ExtractException("ELI34056", "FTP operation not specified.");
                }

                _settings.DeleteEmptyFolder = _deleteEmptyFolderCheckBox.Checked;

                _settings.ConfiguredFtpConnection = _ftpConnectionSettingsControl.FtpConnection;

                _settings.TimeToWaitBetweenRetries = _ftpConnectionSettingsControl.TimeBetweenRetries;

                _settings.NumberOfTimesToRetry = _ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure;

                _settings.ReestablishConnectionBeforeRetry =
                    _ftpConnectionSettingsControl.ReestablishConnectionBeforeRetry;

                _settings.KeepConnectionOpen = _ftpConnectionSettingsControl.KeepConnectionOpen;

                // Still need to verify settings
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32567");
            }

        }

        /// <summary>
        /// Handles click event for all radio buttons in order 
        /// to update the state of the controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleRadioClick(object sender, EventArgs e)
        {
            try
            {
                UpdateControlState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32568");
            }
        }

        #endregion

        #region Helper functions
        
        /// <summary>
        /// Updates the enabled status of controls based on the current value of related controls
        /// </summary>
        void UpdateControlState()
        {
            _localOrNewFileNameTextBox.Enabled = !_deleteFileRadioButton.Checked;
            _localFileNamePathTagsButton.Enabled = !_deleteFileRadioButton.Checked;
            _localFileNameBrowseButton.Enabled = !_deleteFileRadioButton.Checked;
            _deleteEmptyFolderCheckBox.Enabled = _deleteFileRadioButton.Checked;
            if (_renameFileRadioButton.Checked)
            {
                _remoteOrOldFileNameLabel.Text = _RENAME_OLD_FILE_LABEL;
                _localOrNewFileNameLabel.Text = _RENAME_NEW_FILE_LABEL;

                // If the _localOrNewFileNameTextBox equals <SourceDocName>, that is most likely
                // just because it is the default value. <SourceDocName> on its own doesn't make
                // sense as a new remote file name, so clear this value.
                if (_localOrNewFileNameTextBox.Text == SourceDocumentPathTags.SourceDocumentTag)
                {
                    _localOrNewFileNameTextBox.Text = "";
                }
            }
            else
            {
                _remoteOrOldFileNameLabel.Text = _REMOTE_FILE_LABEL;
                _localOrNewFileNameLabel.Text = _LOCAL_FILE_LABEL;
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

            string remoteOrOldLabel = _renameFileRadioButton.Checked
                ? _RENAME_OLD_FILE_LABEL
                : _REMOTE_FILE_LABEL;

            string localOrNewLabel = _renameFileRadioButton.Checked
                ? _RENAME_NEW_FILE_LABEL
                : _LOCAL_FILE_LABEL;

            if (string.IsNullOrWhiteSpace(_remoteOrOldFileNameTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(remoteOrOldLabel + " must be specified.", "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _remoteOrOldFileNameTextBox.Focus();
            }
            else if (_remoteOrOldFileNameTextBox.Text.Trim()[0] == '.')
            {
                UtilityMethods.ShowMessageBox(remoteOrOldLabel + " cannot begin with '.'.", "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _remoteOrOldFileNameTextBox.Focus();
            }
            else if (!_deleteFileRadioButton.Checked && string.IsNullOrWhiteSpace(_localOrNewFileNameTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(localOrNewLabel + " must be specified.", "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _localOrNewFileNameTextBox.Focus();
            }
            else if (_renameFileRadioButton.Checked && _localOrNewFileNameTextBox.Text.Trim()[0] == '.')
            {
                UtilityMethods.ShowMessageBox(localOrNewLabel + " cannot begin with '.'.", "Configuration error", true);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _localOrNewFileNameTextBox.Focus();
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
