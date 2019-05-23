using System;
using System.Windows.Forms;
using Extract.Licensing;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an
    /// <see cref="CloudOCRTask"/> instance.
    /// </summary>
    public partial class CloudOCRTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(CloudOCRTaskSettingsDialog).ToString();

        #endregion Constants

        #region Fields

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudOCRTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="CloudOCRTask"/> to configure</param>
        public CloudOCRTaskSettingsDialog(CloudOCRTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI46817",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46818");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public CloudOCRTask Settings
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _bucketBaseNameTextBox.Text = Settings.BucketBaseName;
                _credentialsJSONFileTextBox.Text = Settings.ProjectCredentialsFile;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46819");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.BucketBaseName = _bucketBaseNameTextBox.Text;
                Settings.ProjectCredentialsFile = _credentialsJSONFileTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI46820", ex);
            }
        }

        private void HandleGenerateBucketBaseNameButtonClick(object sender, EventArgs e)
        {
            _bucketBaseNameTextBox.Text = Guid.NewGuid().ToString();
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><c>true</c> if the settings are invalid; <c>false</c> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_credentialsJSONFileTextBox.Text))
            {
                MessageBox.Show("Please enter a project credentials json file path", "Missing credentials file", 
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _credentialsJSONFilePathTagButton.Focus();
                return true;
            }
            if (string.IsNullOrWhiteSpace(_bucketBaseNameTextBox.Text))
            {
                MessageBox.Show("Please enter a bucket base-name", "Missing bucket base-name", 
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _bucketBaseNameTextBox.Focus();
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
