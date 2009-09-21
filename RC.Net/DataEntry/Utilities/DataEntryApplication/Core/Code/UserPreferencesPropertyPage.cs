using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.DataEntryCoreComponents, _OBJECT_NAME);

        #endregion Fields

        #region Constructors

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
                _licenseCache.Validate("ELI27015");

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

                RefreshSettings();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27028", ex);
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

            bool enableContextControl = (_userPreferences.AutoZoomMode == AutoZoomMode.AutoZoom);
            _zoomContextTrackBar.Enabled = enableContextControl;
            _zoomContextLabel.Enabled = enableContextControl;
            _leastContextLabel.Enabled = enableContextControl;
            _mostContextLabel.Enabled = enableContextControl;

            _dirty = false;
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
                // Enable/disable the auto-zoom context control as appropriate
                _zoomContextTrackBar.Enabled = _autoZoomRadioButton.Checked;
                _zoomContextLabel.Enabled = _autoZoomRadioButton.Checked;
                _leastContextLabel.Enabled = _autoZoomRadioButton.Checked;
                _mostContextLabel.Enabled = _autoZoomRadioButton.Checked;

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
