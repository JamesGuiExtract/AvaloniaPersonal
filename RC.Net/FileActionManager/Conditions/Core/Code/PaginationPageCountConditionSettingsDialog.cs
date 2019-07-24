using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// A <see cref="Form"/> that allows for configuration of an <see cref="PageCountCondition"/>
    /// instance.
    /// </summary>
    public partial class PaginationPageCountConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PaginationPageCountConditionSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes static data for the <see cref="PaginationPageCountConditionSettingsDialog"/>
        /// class.
        /// </summary>
        // FXCop seems to believe this is here to initialize static fields. That is not the case.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PaginationPageCountConditionSettingsDialog()
        {
            try
            {
                PageCountComparisonOperator.Equal.SetReadableValue("equal to");
                PageCountComparisonOperator.NotEqual.SetReadableValue("not equal to");
                PageCountComparisonOperator.LessThan.SetReadableValue("less than");
                PageCountComparisonOperator.LessThanOrEqual.SetReadableValue("less than or equal to");
                PageCountComparisonOperator.GreaterThan.SetReadableValue("greater than");
                PageCountComparisonOperator.GreaterThanOrEqual.SetReadableValue("greater than or equal to");
                PageCountComparisonOperator.NotDefined.SetReadableValue("");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47093");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationPageCountConditionSettingsDialog"/> class.
        /// </summary>
        public PaginationPageCountConditionSettingsDialog(PaginationPageCountCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI47094",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47095");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public PaginationPageCountCondition Settings { get; set; }

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

                // Load the combo boxes with the readable values of their associates enums.
                _outputPagesComparisonComboBox.InitializeWithReadableEnum<PageCountComparisonOperator>(false);
                _deletedPagesComparisonComboBox.InitializeWithReadableEnum<PageCountComparisonOperator>(false);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _outputPagesCheckBox.Checked =
                        Settings.OutputPageCountComparisonOperator != PageCountComparisonOperator.NotDefined;
                    _outputPagesComparisonComboBox.SelectEnumValue(Settings.OutputPageCountComparisonOperator);
                    _outputPageCountUpDown.Value = Settings.OutputPageCount;
                    _deletedPagesCheckBox.Checked =
                        Settings.DeletedPageCountComparisonOperator != PageCountComparisonOperator.NotDefined;
                    _deletedPagesComparisonComboBox.SelectEnumValue(Settings.DeletedPageCountComparisonOperator);
                    _deletedPageCountUpDown.Value = Settings.DeletedPageCount;
                    switch (Settings.DeletedPageAllowance)
                    {
                        case DeletedPageAllowance.OnlyFirst:
                            _onlyAllowFirstCheckBox.Checked = true;
                            break;

                        case DeletedPageAllowance.OnlyLast:
                            _onlyAllowLastCheckBox.Checked = true;
                            break;

                        case DeletedPageAllowance.OnlyFirstOrLast:
                            _onlyAllowFirstOrLastCheckBox.Checked = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47096");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the CheckChanged event of _outputPagesCheckBox in order to enable/disable
        /// the dependent controls.
        /// </summary>
        void HandleOutputPagesCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            try
            {
                _outputPagesComparisonComboBox.Enabled = _outputPagesCheckBox.Checked;
                _outputPageCountUpDown.Enabled = _outputPagesCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47120");
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of _deletedPagesCheckBox in order to enable/disable
        /// the dependent controls.
        /// </summary>
        void HandleDeletedPagesCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            try
            {
                _deletedPagesComparisonComboBox.Enabled = _deletedPagesCheckBox.Checked;
                _deletedPageCountUpDown.Enabled = _deletedPagesCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47121");
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of _onlyAllowFirstCheckBox in order to deactivate any
        /// previously selected deletion allowance.
        /// </summary>
        void HandleOnlyAllowFirstCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_onlyAllowFirstCheckBox.Checked)
                {
                    _onlyAllowLastCheckBox.Checked = false;
                    _onlyAllowFirstOrLastCheckBox.Checked = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47122");
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of _onlyAllowLastCheckBox in order to deactivate any
        /// previously selected deletion allowance.
        /// </summary>
        void HandleOnlyAllowLastCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_onlyAllowLastCheckBox.Checked)
                {
                    _onlyAllowFirstCheckBox.Checked = false;
                    _onlyAllowFirstOrLastCheckBox.Checked = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47123");
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of _onlyAllowFirstOrLastCheckBox in order to deactivate any
        /// previously selected deletion allowance.
        /// </summary>
        void HandleOnlyAllowFirstOrLastCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_onlyAllowFirstOrLastCheckBox.Checked)
                {
                    _onlyAllowFirstCheckBox.Checked = false;
                    _onlyAllowLastCheckBox.Checked = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47124");
            }
        }

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                // Apply the UI values to the Settings instance.
                Settings.OutputPageCountComparisonOperator = _outputPagesCheckBox.Checked
                    ? _outputPagesComparisonComboBox.ToEnumValue<PageCountComparisonOperator>()
                    : PageCountComparisonOperator.NotDefined;
                Settings.OutputPageCount = (int)_outputPageCountUpDown.Value;
                Settings.DeletedPageCountComparisonOperator = _deletedPagesCheckBox.Checked
                    ? _deletedPagesComparisonComboBox.ToEnumValue<PageCountComparisonOperator>()
                    : PageCountComparisonOperator.NotDefined;
                Settings.DeletedPageCount = (int)_deletedPageCountUpDown.Value;

                if (_onlyAllowFirstCheckBox.Checked)
                {
                    Settings.DeletedPageAllowance = DeletedPageAllowance.OnlyFirst;
                }
                else if (_onlyAllowLastCheckBox.Checked)
                {
                    Settings.DeletedPageAllowance = DeletedPageAllowance.OnlyLast;
                }
                else if (_onlyAllowFirstOrLastCheckBox.Checked)
                {
                    Settings.DeletedPageAllowance = DeletedPageAllowance.OnlyFirstOrLast;
                }
                else
                {
                    Settings.DeletedPageAllowance = DeletedPageAllowance.NotRestricted;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47097");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            ExtractException.Assert("ELI47098",
                "Page count condition settings have not been provided.", Settings != null);

            if (!_outputPagesCheckBox.Checked
                && !_deletedPagesCheckBox.Checked
                && !_onlyAllowFirstCheckBox.Checked
                && !_onlyAllowLastCheckBox.Checked
                && !_onlyAllowFirstOrLastCheckBox.Checked)
            {
                UtilityMethods.ShowMessageBox(
                    "At least one page count restriction must be selected", "Invalid configuration", true);

                return true;
            }

            if (_outputPagesCheckBox.Checked)
            {
                if (_outputPagesComparisonComboBox.ToEnumValue<PageCountComparisonOperator>() ==
                        PageCountComparisonOperator.NotDefined)
                {
                    UtilityMethods.ShowMessageBox(
                        "Select a comparison operator for checking the output page count.", "Invalid configuration", true);
                    _outputPageCountUpDown.Focus();

                    return true;
                }
            }

            if (_deletedPagesCheckBox.Checked)
            {
                if (_deletedPagesComparisonComboBox.ToEnumValue<PageCountComparisonOperator>() ==
                        PageCountComparisonOperator.NotDefined)
                {
                    UtilityMethods.ShowMessageBox(
                        "Select a comparison operator for checking the deleted page count.", "Invalid configuration", true);
                    _deletedPageCountUpDown.Focus();

                    return true;
                }
            }

            return false;
        }

        #endregion Private Members
    }
}
