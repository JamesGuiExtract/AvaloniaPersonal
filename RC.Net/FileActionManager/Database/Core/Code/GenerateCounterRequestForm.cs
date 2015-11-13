using Extract.Utilities;
using Extract.Utilities.Email;
using System;
using System.IO;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// A <see cref="Form"/> that allows a request to be generated for Extract Systems in order to
    /// obtain a counter update or unlock code.
    /// </summary>
    [CLSCompliant(false)]
    public partial class GenerateCounterRequestForm : Form
    {
        #region Fields

        /// <summary>
        /// Settings to be used when emailing the request.
        /// </summary>
        SmtpEmailSettings _emailSettings = new SmtpEmailSettings();

        /// <summary>
        /// The customer contact info to be used for licensing related correspondence.
        /// </summary>
        FAMDatabaseSettings<LicenseContact> _licenseContactSettings;

        /// <summary>
        /// Formats database info, settings and entered text into a request for Extract.
        /// </summary>
        SecureCounterTextManipulator _requestTextGenerator;

        /// <summary>
        /// <see langword="true"/> if the form is being displayed for the purposes of an unlock;
        /// <see langword="false"/> if it being displayed for a counter update.
        /// </summary>
        bool _unlockCode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateCounterRequestForm"/> class.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> for which the
        /// request is being sent.</param>
        /// <param name="unlockCode"><see langword="true"/> if the form is being displayed for the
        /// purposes of an unlock; <see langword="false"/> if it being displayed for a counter
        /// update.</param>
        public GenerateCounterRequestForm(FileProcessingDB fileProcessingDB, bool unlockCode)
        {
            try
            {
                _emailSettings.LoadSettings(
                    new FAMDatabaseSettings<ExtractSmtp>(
                        fileProcessingDB, false, SmtpEmailSettings.PropertyNameLookup));
                _licenseContactSettings =
                    new FAMDatabaseSettings<LicenseContact>(fileProcessingDB, false);
                _requestTextGenerator = new SecureCounterTextManipulator(fileProcessingDB);

                InitializeComponent();

                _unlockCode = unlockCode;
                if (unlockCode)
                {
                    Text = "Generate Counter Unlock Request";
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39052");
            }
        }

        #endregion Constructors

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

                _organizationTextBox.Text = _licenseContactSettings.Settings.LicenseContactOrganization;
                _emailTextBox.Text = _licenseContactSettings.Settings.LicenseContactEmail;
                _phoneTextBox.Text = _licenseContactSettings.Settings.LicenseContactPhone;

                UpdateRequestText();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39053");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event of any of the editable text
        /// controls on the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void Handle_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Changes to form's fields require that the request text be updated appropriately.
                UpdateRequestText();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39054");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_copyToClipboardButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCopyToClipboardButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Whenever the request is copied for Extract, store the licensing contact info to
                // the DBInfo table as it is currently entered.
                SaveLicenseContactInfo();

                Clipboard.SetText(_requestTextBox.Text);

                UtilityMethods.ShowMessageBox("The request message has been copied to the clipboard.",
                    "Copied to clipboard", false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39055");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_sendEmailButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSendEmailButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_emailSettings.Server))
                {
                    UtilityMethods.ShowMessageBox("The email is sent using email server settings " +
                        "in the database, but these settings have not been configured.\r\n\r\n" +
                        "From the main screen, select the \"Database | Database options...\" " +
                        "menu option, then use the \"Email\" tab to configure the outgoing email server.",
                        "Outgoing email server not configured", true);

                    return;
                }

                // Whenever the request is sent to Extract, store the licensing contact info to
                // the DBInfo table as it is currently entered.
                SaveLicenseContactInfo();

                var emailMessage = new ExtractEmailMessage();
                emailMessage.EmailSettings = _emailSettings;
                emailMessage.Recipients = new[] { "flex-license@extractsystems.com" }.ToVariantVector();
                emailMessage.Subject = _unlockCode
                    ? "FAM DB counter unlock request"
                    : "FAM DB counter update request";
                if (!string.IsNullOrWhiteSpace(_requestTextGenerator.EmailAddress))
                {
                    emailMessage.CarbonCopyRecipients =
                        new[] { _requestTextGenerator.EmailAddress }.ToVariantVector();
                }
                emailMessage.Body = _requestTextBox.Text;

                emailMessage.Send();

                UtilityMethods.ShowMessageBox("The request email has been sent.", "Email sent", false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39056");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_saveFileButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSaveToFile_Click(object sender, EventArgs e)
        {
            // Whenever a request file is saved, store the licensing contact info to the DBInfo
            // table as it is currently entered.
            SaveLicenseContactInfo();

            using (var fileDialog = new SaveFileDialog())
            {
                fileDialog.InitialDirectory =
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                fileDialog.DefaultExt = "txt";
                fileDialog.FileName = _unlockCode
                    ? "Counter unlock request.txt"
                    : "Counter update request.txt";
                fileDialog.Filter = "Text files (*.txt)|*.txt";
                fileDialog.FilterIndex = 1;
                fileDialog.AddExtension = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(fileDialog.FileName, _requestTextBox.Text);

                    UtilityMethods.ShowMessageBox("The file has been saved.", "File saved", false);
                }
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates the request text based upon the current text in the form's editable fields.
        /// </summary>
        void UpdateRequestText()
        {
            _requestTextGenerator.Organization = _organizationTextBox.Text;
            _requestTextGenerator.EmailAddress = _emailTextBox.Text;
            _requestTextGenerator.Phone = _phoneTextBox.Text;
            _requestTextGenerator.Reason = _reasonTextBox.Text;

            _requestTextBox.Text = _requestTextGenerator.GetRequestText();
        }

        /// <summary>
        /// Saves the license contact info to the FAM DB's DBInfo table using the current text in
        /// the form's contact-related fields.
        /// </summary>
        void SaveLicenseContactInfo()
        {
            _licenseContactSettings.Settings.LicenseContactOrganization =
                _requestTextGenerator.Organization;
            _licenseContactSettings.Settings.LicenseContactEmail =
                _requestTextGenerator.EmailAddress;
            _licenseContactSettings.Settings.LicenseContactPhone = _requestTextGenerator.Phone;
            _licenseContactSettings.Save();
        }

        #endregion Private Members
    }
}
