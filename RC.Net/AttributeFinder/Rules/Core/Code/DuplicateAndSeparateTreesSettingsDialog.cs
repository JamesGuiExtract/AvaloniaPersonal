using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="DuplicateAndSeparateTrees"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class DuplicateAndSeparateTreesSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(NumericSequencerSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateAndSeparateTreesSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public DuplicateAndSeparateTreesSettingsDialog(DuplicateAndSeparateTrees settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI33472", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33473");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="DuplicateAndSeparateTrees"/> to configure.
        /// </summary>
        /// <value>The <see cref="DuplicateAndSeparateTrees"/> to configure.</value>
        public DuplicateAndSeparateTrees Settings
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
                    _attributeSelectorControl.ConfigurableObject =
                        (ICategorizedComponent)Settings.AttributeSelector;
                    _dividingAttributeTextBox.Text = Settings.DividingAttributeName;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33474");
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

                Settings.AttributeSelector =
                    (IAttributeSelector)_attributeSelectorControl.ConfigurableObject;
                Settings.DividingAttributeName = _dividingAttributeTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33475");
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
            if (_attributeSelectorControl.ConfigurableObject == null)
            {
                _attributeSelectorControl.Focus();
                UtilityMethods.ShowMessageBox("Please specify an attribute selector to use.",
                    "Specify attribute selector", false);
                return true;
            }

            IMustBeConfiguredObject configurable =
                _attributeSelectorControl.ConfigurableObject as IMustBeConfiguredObject;
            if (configurable != null && !configurable.IsConfigured())
            {
                _attributeSelectorControl.Focus();
                UtilityMethods.ShowMessageBox("The selected attribute selector has not been " +
                    "properly configured.",
                    "Attribute selector not configured", false);
                return true;
            }

            _dividingAttributeTextBox.Text = _dividingAttributeTextBox.Text.Trim();
            if (!UtilityMethods.IsValidIdentifier(_dividingAttributeTextBox.Text))
            {
                _dividingAttributeTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a valid attribute name to be used " +
                    "to divide the attribute tree.",
                    "Specify dividing attribute", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
