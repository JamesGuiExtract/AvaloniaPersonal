using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="AutoShrinkRedactionZones"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class AutoShrinkRedactionZonesSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(AutoShrinkRedactionZonesSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoShrinkRedactionZonesSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public AutoShrinkRedactionZonesSettingsDialog(AutoShrinkRedactionZones settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI38506", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38507");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="AutoShrinkRedactionZones"/> to configure.
        /// </summary>
        /// <value>The <see cref="AutoShrinkRedactionZones"/> to configure.</value>
        public AutoShrinkRedactionZones Settings
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
                    _autoExpandBeforeAutoShrinkCheckBox.Checked =
                        _maxPixelsToExpandNumericUpDown.Enabled =
                            Settings.AutoExpandBeforeAutoShrink;
                    _maxPixelsToExpandNumericUpDown.Value = (decimal)Settings.MaxPixelsToExpand;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38508");
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

                Settings.AutoExpandBeforeAutoShrink = _autoExpandBeforeAutoShrinkCheckBox.Checked;
                Settings.MaxPixelsToExpand = (float)_maxPixelsToExpandNumericUpDown.Value;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38509");
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

            return false;
        }

        #endregion Private Members

        #region Event Handlers

        private void _autoExpandBeforeAutoShrinkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _maxPixelsToExpandNumericUpDown.Enabled = _autoExpandBeforeAutoShrinkCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47211");
            }
        }

        #endregion Event Handlers
    }
}
