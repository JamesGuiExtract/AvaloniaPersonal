using Extract.FileActionManager.Database;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Email;
using System;
using System.Globalization;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    ///  A <see cref="Form"/> to view and modify settings for an <see cref="SendEmailTask"/> instance.
    /// </summary>
    public partial class SendEmailTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(SendEmailTaskSettingsDialog).ToString();

        /// <summary>
        /// The text for the attachment button.
        /// </summary>
        static readonly string _ATTACHMENTS_BUTTON_TEXT = "Attachments ({0:D}) ...";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="IPathTags"/> instance to be used for all path tags buttons in the dialog.
        /// </summary>
        IPathTags _pathTags;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTaskSettingsDialog"/> class.
        /// </summary>
        public SendEmailTaskSettingsDialog()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings"><see cref="SendEmailTask"/> to be configured.</param>
        public SendEmailTaskSettingsDialog(SendEmailTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI35977",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _pathTags = new FileActionManagerPathTags("", "", null, 0);
                _subjectPathTagsButton.PathTags = _pathTags;
                _bodyPathTagsButton.PathTags = _pathTags;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35978");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public SendEmailTask Settings
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

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _recipientTextBox.Text = Settings.Recipient;
                    _carbonCopyRecipient.Text = Settings.CarbonCopyRecipient;
                    _subjectTextBox.Text = Settings.Subject;
                    _bodyTextBox.Text = Settings.Body;
                }

                UpdateAttachmentButtonText();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35979");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_attachmentsButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAttachmentsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new SendEmailTaskAttachmentsDialog(Settings, _pathTags))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        UpdateAttachmentButtonText();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35980");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_advancedButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAdvancedButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new SendEmailTaskAdvancedDialog(Settings, _pathTags))
                {
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35981");
            }
        }

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

                Settings.Recipient = _recipientTextBox.Text;
                Settings.CarbonCopyRecipient = _carbonCopyRecipient.Text;
                Settings.Subject = _subjectTextBox.Text;
                Settings.Body = _bodyTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35982", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates the the text of the attachment button based on the current number of attachments.
        /// </summary>
        void UpdateAttachmentButtonText()
        {
            _attachmentsButton.Text = string.Format(CultureInfo.CurrentCulture,
                _ATTACHMENTS_BUTTON_TEXT, (Settings == null) ? 0 : Settings.Attachments.Length);
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_recipientTextBox.Text))
            {
                _recipientTextBox.Focus();
                MessageBox.Show("Please specify the email recipient.",
                    "Missing recipient", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_subjectTextBox.Text))
            {
                _recipientTextBox.Focus();
                MessageBox.Show("Please specify the email subject.",
                    "Missing subject", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            try
            {
                FileProcessingDB fileProcessingDB = new FileProcessingDB();
                fileProcessingDB.ConnectLastUsedDBThisProcess();

                var emailSettings =
                    new FAMDatabaseSettings<ExtractSmtp>(
                        fileProcessingDB, false, SmtpEmailSettings.PropertyNameLookup);

                // If the email server settings have not been configured in the DB, display a
                // warning, but still allow the settings dialog to close since those settings
                // cannot be configured here.
                if (string.IsNullOrEmpty(emailSettings.Settings.Server))
                {
                    UtilityMethods.ShowMessageBox("This task uses outgoing email server settings " +
                        "in the FAM database, but these settings have not been configured.\r\n\r\n" +
                        "In the DB Administration utility, select the \"Database | Database options...\" " +
                        "menu option, then use the \"Email\" tab to configure the outgoing email server.",
                        "Outgoing email server not configured", false);
                }   
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI35960",
                    "Unable to validate database email settings", ex);
                ee.Display();
            }

            return false;
        }

        #endregion Private Members
    }
}
