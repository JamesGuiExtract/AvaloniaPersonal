using Extract.Interfaces;
using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;
using System.Globalization;
using Extract.Utilities.Email;

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
                    new FAMDatabaseSettings<LicenseContact>(fileProcessingDB, true);

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

                _emailSpecifiedRecipientsCheckBox.CheckedChanged += (sender, args) =>
                    _emailAlertRecipients.Enabled = _emailSpecifiedRecipientsCheckBox.Checked;

                _emailSupportCheckBox.Checked =
                    _licenseContactSettings.Settings.SendAlertsToExtract;
                _emailSpecifiedRecipientsCheckBox.Checked =
                    _licenseContactSettings.Settings.SendAlertsToSpecified;
                _emailAlertRecipients.Text =
                    _licenseContactSettings.Settings.SpecifiedAlertRecipients;

                RefreshCounterData();
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
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                // If email alerts are enabled make sure there are settings for the SMTP
                if (_emailSpecifiedRecipientsCheckBox.Checked || _emailSupportCheckBox.Checked)
                {
                    // Verify that there are settings for SMTP
                    SmtpEmailSettings _emailSettings = new SmtpEmailSettings();
                    _emailSettings.LoadSettings(
                                        new FAMDatabaseSettings<ExtractSmtp>(
                                            _fileProcessingDB, false, SmtpEmailSettings.PropertyNameLookup));
                    if (string.IsNullOrWhiteSpace(_emailSettings.Server))
                    {
                        UtilityMethods.ShowMessageBox("The alert email is sent using email server settings " +
                            "in the database, but these settings have not been configured.\r\n\r\n" +
                            "From the main screen, select the \"Database | Database options...\" " +
                            "menu option, then use the \"Email\" tab to configure the outgoing email server.",
                            "Outgoing email server not configured", true);
                        DialogResult = DialogResult.None;
                        return;
                    }
                }

                // Store counter alert settings
                _licenseContactSettings.Settings.SendAlertsToExtract =
                    _emailSupportCheckBox.Checked;
                _licenseContactSettings.Settings.SendAlertsToSpecified =
                    _emailSpecifiedRecipientsCheckBox.Checked;
                _licenseContactSettings.Settings.SpecifiedAlertRecipients =
                    _emailAlertRecipients.Text;

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
        /// Reloads secure counter data from <see cref="_fileProcessingDB"/> into the dialog.
        /// </summary>
        void RefreshCounterData()
        {
            try
            {
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

                        _counterDataGridView.Rows.Add(secureCounter.ID, secureCounter.Name,
                            FormatForDisplay(secureCounter.Value, false),
                            FormatForDisplay(FAMDBCounter.AlertLevel, true),
                            FormatForDisplay(FAMDBCounter.AlertMultiple, true));
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

        #endregion Private Members
    }
}
