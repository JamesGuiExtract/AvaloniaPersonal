using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows for configuration of an <see cref="ModifySpatialMode"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class ModifySpatialModeSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ModifySpatialModeSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes static data for the <see cref="ModifySpatialModeSettingsDialog"/>
        /// class.
        /// </summary>
        // FXCop seems to believe this is here to initialize static fields. That is not the case.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ModifySpatialModeSettingsDialog()
        {
            try
            {
                // Assign readable values for enums to be displayed in
                // _convertToPseudoSpatialradioButton.
                ModifySpatialModeRasterZoneCountCondition.Single.SetReadableValue(
                    "is a single raster zone");
                ModifySpatialModeRasterZoneCountCondition.Multiple.SetReadableValue(
                    "are multiple raster zones");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34695");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifySpatialModeSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="ModifySpatialMode"/> instance to configure.</param>
        public ModifySpatialModeSettingsDialog(ModifySpatialMode settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI34696", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34697");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ModifySpatialMode"/> to configure.
        /// </summary>
        /// <value>The <see cref="ModifySpatialMode"/> to configure.</value>
        public ModifySpatialMode Settings
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

                _zoneCountConditionComboBox.
                    InitializeWithReadableEnum<ModifySpatialModeRasterZoneCountCondition>(false);

                switch (Settings.ModifySpatialModeAction)
                {
                    case ModifySpatialModeAction.DowngradeToHybrid:
                        _downgradeToHybridRadioButton.Checked = true;
                        break;

                    case ModifySpatialModeAction.ConvertToPseudoSpatial:
                        _convertToPseudoSpatialRadioButton.Checked = true;
                        break;

                    case ModifySpatialModeAction.Remove:
                        _removeSpatialInfoRadioButton.Checked = true;
                        break;
                }

                _modifyRecursivelyCheckBox.Checked = Settings.ModifyRecursively;

                _useConditionCheckBox.Checked = Settings.UseCondition;
                _zoneCountConditionComboBox.SelectEnumValue(Settings.ZoneCountCondition);

                _zoneCountConditionComboBox.Enabled =
                    _useConditionCheckBox.Checked && !_convertToPseudoSpatialRadioButton.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34698");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="T:CheckBox.CheckChanged"/> event for
        /// <see cref="_convertToPseudoSpatialRadioButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleConvertToPseudoSpatialCheckChanged(object sender, EventArgs e)
        {
            try
            {
                if (_convertToPseudoSpatialRadioButton.Checked)
                {
                    _useConditionCheckBox.Checked = true;
                    _zoneCountConditionComboBox.SelectEnumValue(
                        ModifySpatialModeRasterZoneCountCondition.Single);

                    _useConditionCheckBox.Enabled = false;
                    _zoneCountConditionComboBox.Enabled = false;
                }
                else
                {
                    _useConditionCheckBox.Enabled = true;
                    _zoneCountConditionComboBox.Enabled = _useConditionCheckBox.Checked;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34699");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:CheckBox.CheckChanged"/> event for
        /// <see cref="_useConditionCheckBox"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleUseConditionCheckChanged(object sender, EventArgs e)
        {
            try
            {
                _zoneCountConditionComboBox.Enabled =
                    _useConditionCheckBox.Checked && !_convertToPseudoSpatialRadioButton.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34705");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for <see cref="_okButton"/>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_downgradeToHybridRadioButton.Checked)
                {
                    Settings.ModifySpatialModeAction = ModifySpatialModeAction.DowngradeToHybrid;
                }
                else if (_convertToPseudoSpatialRadioButton.Checked)
                {
                    Settings.ModifySpatialModeAction = ModifySpatialModeAction.ConvertToPseudoSpatial;
                }
                else if (_removeSpatialInfoRadioButton.Checked)
                {
                    Settings.ModifySpatialModeAction = ModifySpatialModeAction.Remove;
                }
                else
                {
                    ExtractException.ThrowLogicException("ELI34700");
                }

                Settings.ModifyRecursively = _modifyRecursivelyCheckBox.Checked;

                Settings.UseCondition = _useConditionCheckBox.Checked;
                Settings.ZoneCountCondition = _zoneCountConditionComboBox
                    .ToEnumValue<ModifySpatialModeRasterZoneCountCondition>();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34701");
            }
        }

        #endregion Event Handlers
    }
}
