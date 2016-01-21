using Extract.Interfaces;
using Extract.Utilities;
using Extract.Utilities.Email;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// A <see cref="Form"/> that allows management of the secure counters in a
    /// <see cref="FileProcessingDB"/>.
    /// </summary>
    internal partial class ManageSecureCountersForm : Form
    {
        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> for which secure counters are being managed.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The customer contact info to be used for licensing related correspondence.
        /// </summary>
        FAMDatabaseSettings<LicenseContact> _licenseContactSettings;

        /// <summary>
        /// <see langword="true"/> if the counters are currently in a valid state; otherwise,
        /// <see langword="false"/>.
        /// </summary>
        bool _countersAreValid;

        /// <summary>
        /// The <see cref="DataGridViewTextBoxEditingControl"/> being used to edit the current cell
        /// in <see cref="_counterDataGridView"/>
        /// </summary>
        DataGridViewTextBoxEditingControl _editingControl;

        /// <summary>
        /// Indicates whether any of the alert level or multiple values have been touched.
        /// </summary>
        bool _alertLevelChanged;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageSecureCountersForm"/> class.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> for which secure
        /// counters are being managed.</param>
        public ManageSecureCountersForm(FileProcessingDB fileProcessingDB)
        {
            try
            {
                _fileProcessingDB = fileProcessingDB;

                _licenseContactSettings =
                    new FAMDatabaseSettings<LicenseContact>(fileProcessingDB, false);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39061");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try 
	        {	        
		        base.OnLoad(e);

                _emailSupportCheckBox.Checked =
                    _licenseContactSettings.Settings.SendAlertsToExtract;
                _emailSpecifiedRecipientsCheckBox.Checked =
                    _licenseContactSettings.Settings.SendAlertsToSpecified;
                _emailAlertRecipients.Text =
                    _licenseContactSettings.Settings.SpecifiedAlertRecipients;

                RefreshCounterData();

                _emailAlertRecipients.SetError(_manageCountersErrorProvider, String.Empty);
                _emailAlertRecipients.SetErrorGlyphPosition(_manageCountersErrorProvider);
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI39062");
	        }
        }

        /// <summary>
        /// Sets the control to the specified visible state.
        /// </summary>
        /// <param name="value"><see langword="true"/> to make the control visible; otherwise,
        /// <see langword="false"/>.</param>
        protected override void SetVisibleCore(bool value)
        {
            var autoSizeMode = _counterDataGridView.ColumnHeadersHeightSizeMode;

            try
            {
                // https://extract.atlassian.net/browse/ISSUE-12964
                // The sequence of events when this form sometimes results in an
                // InvalidOperationException related to column sizing during layout. The call stack
                // at the time of the exception is completely outside of Extract code. However,
                // since the situation seems to be related to enforcing ColumnHeadersHeightSizeMode,
                // temporarily disable any column header resizing while changing visibility.
                _counterDataGridView.ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                base.SetVisibleCore(value);

                _counterDataGridView.ColumnHeadersHeightSizeMode = autoSizeMode;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39167");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_refreshButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshCounterData();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39155");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_generateRequestButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleGenerateRequestButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var generateRequestForm =
                    new GenerateCounterRequestForm(_fileProcessingDB, !_countersAreValid))
                {
                    generateRequestForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39064");
            }
        }


        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_applyUpdateCodeButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleApplyUpdateCodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var applyUpdateCodeForm =
                    new ApplyCounterUpdateForm(_fileProcessingDB, !_countersAreValid))
                {
                    applyUpdateCodeForm.ShowDialog(this);

                    RefreshCounterData();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39065");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.EditingControlShowing"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewEditingControlShowingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleCounterDataGridView_EditingControlShowing(object sender,
            DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                _editingControl = (DataGridViewTextBoxEditingControl)e.Control;

                // Register to receive key press events in order to limit input on the alert level
                // and alert frequency values to numbers.
                _editingControl.KeyPress += HandleTextBox_KeyPress;
                _editingControl.VisibleChanged += HandleEditingControlVisibleChanged;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39066");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.VisibleChanged"/> event of the
        /// <see cref="_editingControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEditingControlVisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (_editingControl != null && !_editingControl.Visible)
                {
                    _editingControl.KeyPress -= HandleTextBox_KeyPress;
                    _editingControl.VisibleChanged -= HandleEditingControlVisibleChanged;
                    _editingControl = null;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39067");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyPress"/> event of the <see cref="_editingControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyPressEventArgs"/> instance
        /// containing the event data.</param>
        void HandleTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (!char.IsControl(e.KeyChar) && e.KeyChar != ',' && !char.IsDigit(e.KeyChar))
                {
                    // Don't allow any non-numeric characters to be entered for alert level or alert
                    // frequency.
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39068");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValidating"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellValidatingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleCounterDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try 
	        {
                // Ensure that valid numbers have been entered for the alert level settings.
                if (e.ColumnIndex >= 3)
                {
                    string value = e.FormattedValue.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        int intValue = 0;
                        if (!Int32.TryParse(value, 
                            NumberStyles.AllowThousands | 
                            NumberStyles.AllowLeadingWhite | 
                            NumberStyles.AllowTrailingWhite,
                            CultureInfo.CurrentCulture,
                            out intValue))
                        {
                            UtilityMethods.ShowMessageBox("Please enter a valid number (or leave blank).",
                                "Invalid number", true);
                            e.Cancel = true;
                        }
                        else if (intValue == 0)
                        {
                            UtilityMethods.ShowMessageBox("Please enter a positive number (or leave blank).",
                                "Invalid number", true);
                            e.Cancel = true;
                        }

                        var row = _counterDataGridView.Rows[e.RowIndex];
                        int alertLevel;
                        int alertFrequency;
                        if (e.ColumnIndex == 3)
                        {
                            alertLevel = intValue;
                            alertFrequency = ParseIntValue(row.Cells[4]);
                        }
                        else
                        {
                            alertLevel = ParseIntValue(row.Cells[3]);
                            alertFrequency = intValue;
                        }

                        if (alertLevel > 0 && alertFrequency > alertLevel)
                        {
                            UtilityMethods.ShowMessageBox(
                                "Alert frequency should be less than or equal to alert level " +
                                "(blank if alert should never be repeated).",
                                "Invalid alert frequency", true);
                            e.Cancel = true;
                        }
                    }

                    if (!e.Cancel)
                    {
                        _alertLevelChanged = true;
                    }
                }
            }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI39069");
	        }
        }
        
        /// <summary>
        /// Handles the CheckChanged event of the HandleEnableEmailAlertsToSupport control. 
        /// Set checked state to the outcome of making sure email settings are valid.
        /// </summary>
        private void HandleEnableEmailAlertsToSupport_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                _licenseContactSettings.Settings.SendAlertsToExtract = _emailSupportCheckBox.Checked;

                if (_emailSupportCheckBox.Checked)
                {
                    _emailSupportCheckBox.Checked = EmailSettingsAreValid();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39236");
            }
        }

        /// <summary>
        /// Handle event so that any existing displayed error state can be cleared if the user 
        /// decides to fix the issue by unchecking the "Enable email alters to:" checkbox.
        /// Also when checked, if the associated text field is blank (usual case), then add "Required"
        /// </summary>
        private void HandleEnableEmailAlertsTo_CheckStateChanged(object sender, EventArgs e)
        {
            try
            {
                _licenseContactSettings.Settings.SendAlertsToSpecified = 
                    _emailSpecifiedRecipientsCheckBox.Checked;

                _emailAlertRecipients.Enabled = _emailSpecifiedRecipientsCheckBox.Checked;

                if (!_emailSpecifiedRecipientsCheckBox.Checked)
                {
                    _emailAlertRecipients.SetError(_manageCountersErrorProvider, String.Empty);
                    _emailAlertRecipients.RemoveRequiredMarker();
                }
                else    // [x] Enable email alerts to: is checked
                {
                    if (EmailSettingsAreValid())
                    {
                        _emailAlertRecipients.SetRequiredMarker();
                    }
                    else
                    {
                        _emailSpecifiedRecipientsCheckBox.Checked = false;
                        _emailAlertRecipients.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39217");
            }
        }

        /// <summary>
        /// Handle event so that any existing displayed error state can be cleared if the user 
        /// decides to fix the issue by entering text in the recipients text box.
        /// </summary>
        private void HandleEmailAlertRecipients_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _licenseContactSettings.Settings.SpecifiedAlertRecipients = _emailAlertRecipients.Text;
                if (!_emailAlertRecipients.EmptyOrRequiredMarkerIsSet())
                {
                    _emailAlertRecipients.SetError(_manageCountersErrorProvider, String.Empty);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39218");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EnableEmailChecked_AreEmailSettingsCompleted() ||
                    !EnableEmailAlertTo_TextBoxValidation())
                {
                    // In this case the dialog will not be dismissed.
                    this.DialogResult = DialogResult.None;
                    return;
                }

                if (!ApplyAlertSettings())
                {
                    DialogResult = DialogResult.None;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39071");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Applies any changes to alert settings.
        /// </summary>
        /// <returns><see langword="true"/> if the settings were able to be applied.
        /// <see langword="false"/> if the settings could not be applied and the operation that
        /// triggered the apply should be canceled.</returns>
        bool ApplyAlertSettings()
        {
            if (_emailSpecifiedRecipientsCheckBox.Checked &&
               _emailAlertRecipients.EmptyOrRequiredMarkerIsSet())
            {
                _emailSpecifiedRecipientsCheckBox.Checked = false;
            }

            // Only save if the user made an explicit change to avoid overwriting changes another
            // user may have made in another instance that was active at the same time as this one.
            if (_licenseContactSettings.HasUnsavedChanges)
            {
                _licenseContactSettings.Save();
            }

            if (_alertLevelChanged)
            {
                foreach (var row in _counterDataGridView.Rows
                    .OfType<DataGridViewRow>())
                {
                    int counterID = ParseIntValue(row.Cells[0]);
                    int alertLevel = ParseIntValue(row.Cells[3]);
                    int alertFrequency = ParseIntValue(row.Cells[4]);

                    _fileProcessingDB.SetSecureCounterAlertLevel(
                        counterID, alertLevel, alertFrequency);
                }

                _alertLevelChanged = false;
            }

            return true;
        }

        /// <summary>
        /// Reloads secure counter data from <see cref="_fileProcessingDB"/> into the dialog.
        /// </summary>
        void RefreshCounterData()
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-13466
                // Changes to alert level/frequency should not be applied until the OK button is
                // pressed, but we don't want to lose changes the user has made to these settings
                // either. Keep track of the currently displayed values and restore them after
                // reloading the counter data from the DB.
                var displayedAlertLevels =
                    _counterDataGridView.Rows
                        .OfType<DataGridViewRow>()
                        .ToDictionary(
                            row => ParseIntValue(row.Cells[0]), // Counter ID
                            row => new Tuple<int, int>(
                                ParseIntValue(row.Cells[3]), // Alert level
                                ParseIntValue(row.Cells[4]))); // Alert multiple

                _counterDataGridView.Rows.Clear();
                _countersAreValid = true;

                foreach (var secureCounter in _fileProcessingDB.GetSecureCounters(true)
                    .ToIEnumerable<ISecureCounter>())
                {
                    if (secureCounter.ID == 0)
                    {
                        _counterDataGridView.Rows.Add(0, "Corrupted", "Corrupted");
                        _countersAreValid = false;
                    }
                    else if (!secureCounter.IsValid)
                    {
                        _counterDataGridView.Rows.Add(secureCounter.ID, secureCounter.Name,
                            "Corrupted");
                        _countersAreValid = false;
                    }
                    else
                    {
                        var FAMDBCounter = (IFAMDBSecureCounter)secureCounter;

                        int alertLevel = FAMDBCounter.AlertLevel;
                        int alertMultiple = FAMDBCounter.AlertMultiple;

                        // Restore alert level/frequency values that were displayed prior to the
                        // refresh.
                        Tuple<int, int> displayedAlertLevel;
                        if (displayedAlertLevels.TryGetValue(secureCounter.ID, out displayedAlertLevel))
                        {
                            alertLevel = displayedAlertLevel.Item1;
                            alertMultiple = displayedAlertLevel.Item2;
                        }

                        _counterDataGridView.Rows.Add(secureCounter.ID, secureCounter.Name,
                            FormatForDisplay(secureCounter.Value, false),
                            FormatForDisplay(alertLevel, true),
                            FormatForDisplay(alertMultiple, true));
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_fileProcessingDB.IsConnected)
                {
                    new ExtractException("ELI39070", "Unable to obtain database counter state.", ex).Display();

                    DialogResult = DialogResult.Cancel;
                    return;
                }

                _countersAreValid = false;
            }

            if (_countersAreValid)
            {
                _generateRequestButton.Text = "Generate update request";
                _applyUpdateCodeButton.Text = "Apply update code";
                _counterDataGridView.Enabled = true;
            }
            else
            {
                _generateRequestButton.Text = "Generate unlock request";
                _applyUpdateCodeButton.Text = "Apply unlock code";
                _counterDataGridView.Enabled = false;
            }
        }

        /// <summary>
        /// Parses the <see paramref="cell"/>'s value as an integer.
        /// </summary>
        /// <param name="cell">The <see cref="DataGridViewCell"/> to parse.</param>
        /// <returns>The cell's value as an <see langword="int"/>.</returns>
        static int ParseIntValue(DataGridViewCell cell)
        {
            if (cell.Value == null)
            {
                return 0;
            }

            string value = cell.Value.ToString();
            return string.IsNullOrWhiteSpace(value)
                ? 0
                : Int32.Parse(value,
                    NumberStyles.AllowThousands |
                    NumberStyles.AllowLeadingWhite |
                    NumberStyles.AllowTrailingWhite,
                    CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats the specified <see paramref="number"/> as a string with thousands separator
        /// commas.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="makeZeroBlank"><see langword="true"/> if the number zero should result in a
        /// blank value; <see langword="false"/> to return "0".</param>
        /// <returns>The formatted number</returns>
        static string FormatForDisplay(int number, bool makeZeroBlank)
        {
            if (number == 0 && makeZeroBlank)
            {
                return "";
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, "{0:n0}", number);
            }
        }

        /// <summary>
        /// If one or both enable email options are checked, make sure that email settings are valid.
        /// </summary>
        /// <returns>true if OK to continue processing in OK handler, false to halt processing 
        /// (and not close the dialog)</returns>
        bool EnableEmailChecked_AreEmailSettingsCompleted()
        {
            if (!_emailSupportCheckBox.Checked && !_emailSpecifiedRecipientsCheckBox.Checked)
            {
                return true;        // enable email... not checked
            }

            if (!EmailSettingsAreValid())
            {
                _emailSupportCheckBox.Checked = false;
                _emailSpecifiedRecipientsCheckBox.Checked = false;
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// This method determines whether email settings are valid - invoking settings dialog if necessary.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA1300:UseMessageBoxOptions")]
        bool EmailSettingsAreValid()
        {
            SmtpEmailSettings emailSettings = new SmtpEmailSettings();
            var dbSettings =
                    new FAMDatabaseSettings<ExtractSmtp>(_fileProcessingDB,
                                                         false,
                                                         SmtpEmailSettings.PropertyNameLookup);
            emailSettings.LoadSettings(dbSettings);

            if (!dbSettings.Settings.EnableEmailSettings ||
                String.IsNullOrWhiteSpace(dbSettings.Settings.Server))
            {
                var response =
                    MessageBox.Show(owner: this,
                                    text: "Email settings are not configured. Would you like to configure them now?\r\n" +
                                    "If you choose Yes, the email settings window will be displayed " +
                                    "and allow you to configure the email settings.\r\n" +
                                    "If you choose No, the email alert(s) you just enabled will be disabled.",
                                    caption: "Invalid email settings",
                                    buttons: MessageBoxButtons.YesNo,
                                    icon: MessageBoxIcon.Question,
                                    defaultButton: MessageBoxDefaultButton.Button1);

                if (DialogResult.Yes == response)
                {
                    return (emailSettings.RunConfiguration());
                }
                else
                {
                    return false;
                }
            }

            return true;    // email settings configured
        }


        /// <summary>
        /// There are two steps to validating that all is well with the Enable email alerts. 
        /// The first is to check that email settings are configured.
        /// The second is to make sure that iff "Enable email alerts to:" is check, then there is
        /// a defined recipient. This method handles this second check.
        /// It is possible that both checks will fail.
        /// </summary>
        /// <returns>true if OK to continue processing in OK handler, false to halt processing 
        /// (and not close the dialog) </returns>
        bool EnableEmailAlertTo_TextBoxValidation()
        {
            if (!_emailSpecifiedRecipientsCheckBox.Checked)
                return true;

            if (_emailAlertRecipients.EmptyOrRequiredMarkerIsSet())
            {
                _emailAlertRecipients.SetError(_manageCountersErrorProvider, "Add one or more email addresses to alert");

                _emailAlertRecipients.RemoveRequiredMarker();
                _emailAlertRecipients.Focus();

                return false;
            }

            return true;
        }

        #endregion Private Members
    }
}
