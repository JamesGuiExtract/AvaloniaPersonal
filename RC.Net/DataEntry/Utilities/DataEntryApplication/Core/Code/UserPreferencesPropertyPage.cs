using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// Represents the property page of the data entry application user preferences.
    /// </summary>
    public partial class UserPreferencesPropertyPage : UserControl, IPropertyPage
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(UserPreferencesPropertyPage).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="UserPreferences"/> to which settings will be applied.
        /// </summary>
        UserPreferences _userPreferences;

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Provides static initializatoin for the <see cref="UserPreferencesPropertyPage"/> class.
        /// </summary>
        // FXCop believes static members are being initialized here.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static UserPreferencesPropertyPage()
        {
            try
            {
                OcrTradeoff.Accurate.SetReadableValue("Accurate");
                OcrTradeoff.Balanced.SetReadableValue("Balanced");
                OcrTradeoff.Fast.SetReadableValue("Fast");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34062");
            }
        }

        /// <summary>
        /// Initializes a new <see cref="UserPreferencesPropertyPage"/> class.
        /// </summary>
        /// <param name="userPreferences">The user preferences to be configured.</param>
        public UserPreferencesPropertyPage(UserPreferences userPreferences)
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI27015", _OBJECT_NAME);

                InitializeComponent();

                _userPreferences = userPreferences;

                _noZoomRadioButton.Click += HandleAutoZoomModeButtonClicked;
                _zoomOutIfNecessaryRadioButton.Click += HandleAutoZoomModeButtonClicked;
                _autoZoomRadioButton.Click += HandleAutoZoomModeButtonClicked;
                _zoomContextTrackBar.ValueChanged += HandleAutoZoomContextLevelChanged;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27016", ex);
            }
        }

        #endregion Constructors

        #region IPropertyPage Members

        /// <summary>
        /// Event raised when the dirty flag is set.
        /// </summary>
        public event EventHandler PropertyPageModified;

        /// <summary>
        /// Raises the <see cref="PropertyPageModified"/> event.
        /// </summary>
        private void OnPropertyPageModified()
        {
            try
            {
                // Set the dirty flag
                _dirty = true;

                // If there is a listener for the event then raise it.
                if (PropertyPageModified != null)
                {
                    PropertyPageModified(this, null);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27055", ex);
            }
        }

        /// <summary>
        /// Applies the changes to the <see cref="UserPreferences"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                // Store the auto-ocr settings.
                _userPreferences.AutoOcr = _autoOcrCheckBox.Checked;
                _userPreferences.OcrTradeoff = _ocrTradeOffComboBox.ToEnumValue<OcrTradeoff>();

                // Store the specified zoom mode.
                if (_noZoomRadioButton.Checked)
                {
                    _userPreferences.AutoZoomMode = AutoZoomMode.NoZoom;
                }
                else if (_zoomOutIfNecessaryRadioButton.Checked)
                {
                    _userPreferences.AutoZoomMode = AutoZoomMode.ZoomOutIfNecessary;
                }
                else
                {
                    _userPreferences.AutoZoomMode = AutoZoomMode.AutoZoom;
                }

                // Store the specified auto-zoom context
                _userPreferences.AutoZoomContext =
                    (double)_zoomContextTrackBar.Value / (double)_zoomContextTrackBar.Maximum;

                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27018", ex);
            }
        }

        /// <summary>
        /// Gets whether the settings on the property page have been modified.
        /// </summary>
        /// <return><see langword="true"/> if the settings on the property page have been modified;
        /// <see langword="false"/> if they have not been modified.</return>
        public bool IsDirty
        {
            get
            {
                return _dirty;
            }
        }

        /// <summary>
        /// Gets whether the user-specified settings on the property page are valid.
        /// </summary>
        /// <value><see langword="true"/> if the user-specified settings are valid; 
        /// <see langword="false"/> if the settings are not valid.</value>
        public bool IsValid
        {
            get
            {
                // There is not currently any way to specify invalid settings.
                return true;
            }
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        public void SetFocusToFirstControl()
        {
            _noZoomRadioButton.Focus();
        }

        #endregion IPropertyPage Members

        #region Overrides

        /// <summary>
        /// Raises the <see cref="UserControl.Load"/> event.
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/> that contain the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Add the available OcrTradeoff options to the _ocrTradeOffComboBox.
                _ocrTradeOffComboBox.InitializeWithReadableEnum<OcrTradeoff>(false);

                RefreshSettings();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI27028");
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Resets all the values to the values stored in <see cref="_userPreferences"/> to the page
        /// controls and resets the dirty flag to <see langword="false"/>.
        /// </summary>
        void RefreshSettings()
        {
            // Display the stored auto-OCR settings.
            _autoOcrCheckBox.Checked = _userPreferences.AutoOcr;
            _ocrTradeOffComboBox.SelectEnumValue(_userPreferences.OcrTradeoff);
            _ocrTradeOffComboBox.Enabled = _autoOcrCheckBox.Checked;

            // Display the stored AutoZoomMode setting
            switch (_userPreferences.AutoZoomMode)
            {
                case AutoZoomMode.NoZoom:
                    {
                        _noZoomRadioButton.Checked = true;
                    }
                    break;

                case AutoZoomMode.ZoomOutIfNecessary:
                    {
                        _zoomOutIfNecessaryRadioButton.Checked = true;
                    }
                    break;

                case AutoZoomMode.AutoZoom:
                    {
                        _autoZoomRadioButton.Checked = true;
                    }
                    break;
            }

            // Display the stored auto-zoom context setting.
            _zoomContextTrackBar.Value =
                   (int)(_userPreferences.AutoZoomContext * _zoomContextTrackBar.Maximum);

            _dirty = false;
        }

        /// <summary>
        /// Handles the case that the state of the <see cref="_autoOcrCheckBox"/> changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAutoOcrCheckChanged(object sender, EventArgs e)
        {
            try
            {
                _ocrTradeOffComboBox.Enabled = _autoOcrCheckBox.Checked;

                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34063");
            }
        }

        /// <summary>
        /// Handles the ocr trade off selected index changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOcrTradeOffSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35388");
            }
        }

        /// <summary>
        /// Handles the case that a AutoZoomMode option has been selected.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleAutoZoomModeButtonClicked(object sender, EventArgs e)
        {
            try
            {
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27032", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the auto-zoom context level has been changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleAutoZoomContextLevelChanged(object sender, EventArgs e)
        {
            try
            {
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27033", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Private Members
    }
}
