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
    public partial class PageCountConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PageCountConditionSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes static data for the <see cref="PageCountConditionSettingsDialog"/>
        /// class.
        /// </summary>
        // FXCop seems to believe this is here to initialize static fields. That is not the case.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PageCountConditionSettingsDialog()
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
                throw ex.AsExtract("ELI36693");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageCountConditionSettingsDialog"/> class.
        /// </summary>
        public PageCountConditionSettingsDialog(PageCountCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI36694",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36697");
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
        public PageCountCondition Settings { get; set; }

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
                _comparisonComboBox.InitializeWithReadableEnum<PageCountComparisonOperator>(false);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _comparisonComboBox.SelectEnumValue(Settings.PageCountComparisonOperator);
                    _pageCountUpDown.Value = Settings.PageCount;
                    _useDBPageCountCheckBox.Checked = Settings.UseDBPageCount;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36699");
            }
        }

        #endregion Overrides

        #region Event Handlers

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
                Settings.PageCountComparisonOperator =
                    _comparisonComboBox.ToEnumValue<PageCountComparisonOperator>();
                Settings.PageCount = (int)_pageCountUpDown.Value;
                Settings.UseDBPageCount = _useDBPageCountCheckBox.Checked;
                
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36700");
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
            ExtractException.Assert("ELI36701",
                "Page count condition settings have not been provided.", Settings != null);

            if (_comparisonComboBox.ToEnumValue<PageCountComparisonOperator>() ==
                    PageCountComparisonOperator.NotDefined)
            {
                UtilityMethods.ShowMessageBox(
                    "Select a comparison operator.", "Invalid configuration", true);
                _pageCountUpDown.Focus();

                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
