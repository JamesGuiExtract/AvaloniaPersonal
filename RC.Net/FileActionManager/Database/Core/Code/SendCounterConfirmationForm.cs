using Extract.Utilities;
using Extract.Utilities.Email;
using System;
using System.IO;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// A <see cref="Form"/> that allows a confirmation to be generated for Extract Systems of a
    /// successfully applied counter update or unlock code.
    /// </summary>
    internal partial class SendCounterConfirmationForm : Form
    {
        #region Fields

        /// <summary>
        /// Settings to be used when emailing the confirmation.
        /// </summary>
        SmtpEmailSettings _emailSettings = new SmtpEmailSettings();

        /// <summary>
        /// The customer contact info to be used for licensing related correspondence.
        /// </summary>
        FAMDatabaseSettings<LicenseContact> _licenseContactSettings;

        /// <summary>
        /// Formats database info and settings into a confirmation for Extract.
        /// </summary>
        SecureCounterTextManipulator _confirmationTextGenerator;

        /// <summary>
        /// <see langword="true"/> if the form is being displayed for the purposes of an unlock;
        /// <see langword="false"/> if it being displayed for a counter update.
        /// </summary>
        bool _unlockCode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCounterConfirmationForm"/> class.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> for which the
        /// confirmation is being sent.</param>
        /// <param name="appliedUpdates">The applied updates.</param>
        /// <param name="unlockCode"><see langword="true"/> if the form is being displayed for the
        /// purposes of an unlock; <see langword="false"/> if it being displayed for a counter update.</param>
        public SendCounterConfirmationForm(FileProcessingDB fileProcessingDB, string appliedUpdates,
            bool unlockCode)
        {
            try
            {
                _emailSettings.LoadSettings(
                    new FAMDatabaseSettings<ExtractSmtp>(
                        fileProcessingDB, false, SmtpEmailSettings.PropertyNameLookup));
                _licenseContactSettings =
                    new FAMDatabaseSettings<LicenseContact>(fileProcessingDB, false);

                _confirmationTextGenerator = new SecureCounterTextManipulator(fileProcessingDB);
                _confirmationTextGenerator.Organization =
                    _licenseContactSettings.Settings.LicenseContactOrganization;
                _confirmationTextGenerator.EmailAddress =
                    _licenseContactSettings.Settings.LicenseContactEmail;
                _confirmationTextGenerator.Phone =
                    _licenseContactSettings.Settings.LicenseContactPhone;
                _confirmationTextGenerator.Reason = appliedUpdates;

                InitializeComponent();

                _unlockCode = unlockCode;
                if (unlockCode)
                {
                    _confirmationLabel.Text = "The counter unlock was successful!";
                    _confirmationGroupBox.Text =
                        "Please send the following confirmation of the unlock to Extract Systems";
                }

                _confirmationTextBox.Text = _confirmationTextGenerator.GetConfirmationText();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39057");
            }
        }

        #endregion Constructors

        #region Event Handlers

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
                Clipboard.SetText(_confirmationTextBox.Text);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39058");
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
                        "in the database, but these settings have not been configured.",
                        "Outgoing email server not configured", true);

                    return;
                }

                var emailMessage = new ExtractEmailMessage();
                emailMessage.EmailSettings = _emailSettings;
                emailMessage.Recipients = new[] { "flex-license@extractsystems.com" }.ToVariantVector();
                emailMessage.Subject = _unlockCode
                    ? "FAM DB counter unlock confirmation"
                    : "FAM DB counter update confirmation";
                if (!string.IsNullOrWhiteSpace(_confirmationTextGenerator.EmailAddress))
                {
                    emailMessage.CarbonCopyRecipients =
                        new[] { _confirmationTextGenerator.EmailAddress }.ToVariantVector();
                }
                emailMessage.Body = _confirmationTextBox.Text;

                emailMessage.Send();

                UtilityMethods.ShowMessageBox("The confirmation email has been sent.", "Email sent", false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39059");
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
            using (var fileDialog = new SaveFileDialog())
            {
                fileDialog.InitialDirectory =
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                fileDialog.DefaultExt = "txt";
                fileDialog.FileName = _unlockCode
                    ? "Counter unlock confirmation.txt"
                    : "Counter update confirmation.txt";
                fileDialog.Filter = "Text files (*.txt)|*.txt";
                fileDialog.FilterIndex = 1;
                fileDialog.AddExtension = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(fileDialog.FileName, _confirmationTextBox.Text);

                    UtilityMethods.ShowMessageBox("The file has been saved.", "File saved", false);
                }
            }
        }

        #endregion Event Handlers
    }
}
