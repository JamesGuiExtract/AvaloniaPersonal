using EnterpriseDT.Net.Ftp;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Configuration Dialog for <see cref="FtpFileTransferTask"/>
    /// </summary>
    public partial class FtpFileTransferSettingsDialog : Form
    {
        #region Fields
        
        // FtpFileTransferTask that contains the configured settings
        FtpFileTransferTask _settings;
        
        #endregion
        
        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public FtpFileTransferTask Settings 
        { 
            get
            {
                return _settings;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileTransferSettingsDialog"/> class
        /// </summary>
        public FtpFileTransferSettingsDialog():this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpFileTransferSettingsDialog"/> class
        /// with settings
        /// </summary>
        /// <param name="settings"><see cref="FtpFileTransferTask"/> that has the intial settings.</param>
        public FtpFileTransferSettingsDialog(FtpFileTransferTask settings)
        {
            InitializeComponent();

            _settings = settings ?? new FtpFileTransferTask();
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
                _remoteFileNameTextBox.Text = _settings.RemoteFileName;
                _localFileNameTextBox.Text = _settings.LocalFileName;

                // Set the AfterDownloadAction radio buttons
                switch (_settings.ActionToPerform)
                {
                    case TransferActionToPerform.DeleteFileFromFtpServer:
                        _deleteFileRadioButton.Checked = true;
                        break;
                    case TransferActionToPerform.DownloadFileFromFtpServer:
                        _downloadFileRadioButton.Checked = true;
                        break;
                    case TransferActionToPerform.UploadFileToFtpServer:
                        _uploadFileRadioButton.Checked = true;
                        break;
                    default:
                        break;
                }

                _ftpConnectionSettingsControl.FtpConnection = 
                    _settings.ConfiguredFtpConnection ?? new SecureFTPConnection();

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
                _settings.RemoteFileName = _remoteFileNameTextBox.Text;
                if (!_deleteFileRadioButton.Checked)
                {
                    _settings.LocalFileName = _localFileNameTextBox.Text;
                }
                if (_uploadFileRadioButton.Checked)
                {
                    _settings.ActionToPerform = TransferActionToPerform.UploadFileToFtpServer;
                }
                else if (_downloadFileRadioButton.Checked)
                {
                    _settings.ActionToPerform = TransferActionToPerform.DownloadFileFromFtpServer;
                }
                else if (_deleteFileRadioButton.Checked)
                {
                    _settings.ActionToPerform = TransferActionToPerform.DeleteFileFromFtpServer;
                }

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
                ex.ExtractDisplay("ELI32140");
            }
        }

        #endregion

        #region Helper functions
        
        /// <summary>
        /// Updates the enabled status of controls based on the current value of related controls
        /// </summary>
        void UpdateControlState()
        {
            _localFileNameTextBox.Enabled = !_deleteFileRadioButton.Checked;
            _localFileNamePathTagsButton.Enabled = !_deleteFileRadioButton.Checked;
            _localFileNameBrowseButton.Enabled = !_deleteFileRadioButton.Checked;
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

            if (string.IsNullOrWhiteSpace(_remoteFileNameTextBox.Text))
            {
                MessageBox.Show("Remote filename must be specified.", "Configuration error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _remoteFileNameTextBox.Focus();
            }
            else if (!_deleteFileRadioButton.Checked && string.IsNullOrWhiteSpace(_localFileNameTextBox.Text))
            {
                MessageBox.Show("Local filename must be specified.", "Configuration error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                _settingsTabControl.SelectTab(_generalSettingsTabPage);
                _localFileNameTextBox.Focus();
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
