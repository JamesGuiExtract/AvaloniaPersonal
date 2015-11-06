using Extract.Interfaces;
using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;
using System.Globalization;

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

                RefreshCounterGrid();
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI39062");
	        }
        }

        #endregion Overrides

        #region Event Handlers

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

                    RefreshCounterGrid();
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
                // and alert multiple values to numbers.
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
                    // multiple.
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
                if (e.ColumnIndex >= 2)
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
                            UtilityMethods.ShowMessageBox("Please enter a valid number.",
                                "Invalid number", true);
                            e.Cancel = true;
                        }
                        else if (intValue == 0)
                        {
                            UtilityMethods.ShowMessageBox("Please enter a positive number.",
                                "Invalid number", true);
                            e.Cancel = true;
                        }
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
                // Store counter alert settings
                _licenseContactSettings.Settings.SendAlertsToExtract =
                    _emailSupportCheckBox.Checked;
                _licenseContactSettings.Settings.SendAlertsToSpecified =
                    _emailSpecifiedRecipientsCheckBox.Checked;
                _licenseContactSettings.Settings.SpecifiedAlertRecipients =
                    _emailAlertRecipients.Text;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39071");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Refreshes the counter grid with the current counter data from
        /// <see cref="_fileProcessingDB"/>.
        /// </summary>
        void RefreshCounterGrid()
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
                        _counterDataGridView.Rows.Add(secureCounter.ID, secureCounter.Name,
                            secureCounter.Value);
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

            if (!_countersAreValid)
            {
                _generateRequestButton.Text = "Generate unlock code";
                _applyUpdateCodeButton.Text = "Apply unlock code";
            }
        }

        #endregion Private Members
    }
}
