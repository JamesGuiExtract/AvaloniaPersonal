using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="ExtractOcrTextInImageArea"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class ExtractOcrTextInImageAreaSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ExtractOcrTextInImageArea).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes static data for the <see cref="ExtractOcrTextInImageAreaSettingsDialog"/>
        /// class.
        /// </summary>
        // FXCop seems to believe this is here to initialize static fields. That is not the case.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ExtractOcrTextInImageAreaSettingsDialog()
        {
            try
            {
                // Create readable values for the ESpatialEntity enum.
                ESpatialEntity.kCharacter.SetReadableValue("chars");
                ESpatialEntity.kWord.SetReadableValue("words");
                ESpatialEntity.kLine.SetReadableValue("lines");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33719");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractOcrTextInImageAreaSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="ExtractOcrTextInImageArea"/> instance to configure.
        /// </param>
        public ExtractOcrTextInImageAreaSettingsDialog(ExtractOcrTextInImageArea settings)
        {

            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleWritingCoreObjects, "ELI33701",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33718");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ExtractOcrTextInImageArea"/> to configure.
        /// </summary>
        /// <value>The <see cref="ExtractOcrTextInImageArea"/> to configure.</value>
        public ExtractOcrTextInImageArea Settings
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

                _spatialEntityComboBox.InitializeWithReadableEnum<ESpatialEntity>(true);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    if (Settings.UseOriginalDocumentOcr)
                    {
                        _originalDocumentOcrRadioButton.Checked = true;
                    }
                    else
                    {
                        _documentContextRadioButton.Checked = true;
                    }

                    if (Settings.UseOverallBounds)
                    {
                        _overallBoundsRadioButton.Checked = true;
                    }
                    else
                    {
                        _separateZonesRadioButton.Checked = true;
                    }
                        
                    _includeExcludeComboBox.SelectedIndex =
                        Settings.IncludeTextOnBoundary ? 0 : 1;
                    _spatialEntityComboBox.SelectEnumValue(Settings.SpatialEntityType);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33702");
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
                Settings.UseOriginalDocumentOcr = _originalDocumentOcrRadioButton.Checked;
                Settings.UseOverallBounds = _overallBoundsRadioButton.Checked;
                Settings.IncludeTextOnBoundary = (_includeExcludeComboBox.SelectedIndex == 0);
                Settings.SpatialEntityType = _spatialEntityComboBox.ToEnumValue<ESpatialEntity>();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33720");
            }
        }

        #endregion Event Handlers
    }
}
