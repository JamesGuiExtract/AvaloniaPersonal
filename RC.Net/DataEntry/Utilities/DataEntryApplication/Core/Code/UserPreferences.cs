using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// Represents the user-specified preferences for a DataEntry application.
    /// </summary>
    public class UserPreferences : IUserConfigurableComponent, IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(UserPreferences).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The property page associated with the <see cref="UserPreferences"/>.
        /// </summary>
        UserPreferencesPropertyPage _propertyPage;

        /// <summary>
        /// Provides persistence of user DataEntry settings to the registry.
        /// </summary>
        static RegistrySettings<Properties.Settings> _registry =
            new RegistrySettings<Properties.Settings>(@"Software\Extract Systems\DataEntry");

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferences"/> class.
        /// </summary>
        public UserPreferences()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI27010", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27011", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether a document should automatically be OCR'd when
        /// displayed for verification if it does not have an associated uss file.
        /// </summary>
        /// <value><see langword="true"/> to OCR a document when displayed for verification if it
        /// does not have an associated uss file; otherwise, <see langword="false"/>.
        /// </value>
        public bool AutoOcr
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="OcrTradeoff"/> to use when automatically OCR'ing document
        /// text.
        /// </summary>
        /// <value>The <see cref="OcrTradeoff"/> to use when automatically OCR'ing document text.
        /// </value>
        public OcrTradeoff OcrTradeoff
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies how the <see cref="ImageViewer"/> zoom/view is adjusted when new fields are
        /// selected.
        /// </summary>
        /// <value>An <see cref="AutoZoomMode"/> value specifying how the <see cref="ImageViewer"/>
        /// zoom/view should be adjusted when new fields are selected.</value>
        /// <returns>An <see cref="AutoZoomMode"/> value specifying how the <see cref="ImageViewer"/>
        /// zoom/view is adjusted when new fields are selected.</returns>
        public AutoZoomMode AutoZoomMode
        {
            get;
            set;
        }

        /// <summary>
        /// The page space (context) that should be shown around an object selected when AutoZoom
        /// mode is active. 0 indicates no context space should be shown around the current
        /// selection where 1 indicates the maximum context space should be shown.
        /// </summary>
        /// <value>The page space (context) that should be shown around an object.</value>
        /// <returns>The page space (context) that will be shown around an object.</returns>
        public double AutoZoomContext
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Reads the user preferences from the registry.
        /// </summary>
        public void ReadFromRegistry()
        {
            try
            {
                // Read the settings from the registry
                AutoOcr = _registry.Settings.AutoOcr;
                OcrTradeoff = _registry.Settings.OcrTradeoff;
                AutoZoomMode = _registry.Settings.AutoZoomMode;
                AutoZoomContext = _registry.Settings.AutoZoomContext;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27021", ex);
            }
        }

        /// <summary>
        /// Writes the user preferences to the registry.
        /// </summary>
        public void WriteToRegistry()
        {
            try
            {
                // Write the settings to the registry
                _registry.Settings.AutoOcr = AutoOcr;
                _registry.Settings.OcrTradeoff = OcrTradeoff;
                _registry.Settings.AutoZoomMode = AutoZoomMode;
                _registry.Settings.AutoZoomContext = AutoZoomContext;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27022", ex);
            }
        }

        /// <summary>
        /// Reads a <see cref="UserPreferences"/> object from the registry.
        /// </summary>
        /// <returns>A <see cref="UserPreferences"/> object that was read from the registry.
        /// </returns>
        public static UserPreferences FromRegistry()
        {
            try
            {
                // Create the user preferences
                UserPreferences preferences = new UserPreferences();

                // Set the properties from the registry
                preferences.ReadFromRegistry();

                // Return the result
                return preferences;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27026", ex);
            }
        }

        #endregion Methods

        #region IUserConfigurableComponent Members

        /// <summary>
        /// Gets or sets the property page of the <see cref="UserPreferences"/>.
        /// </summary>
        /// <return>The property page of the <see cref="UserPreferences"/>.</return>
        public UserControl PropertyPage
        {
            get
            {
                // Create the property page if not already created
                if (_propertyPage == null)
                {
                    _propertyPage = new UserPreferencesPropertyPage(this);
                }

                return _propertyPage;
            }
        }

        #endregion IUserConfigurableComponent Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="UserPreferences"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="UserPreferences"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="UserPreferences"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_propertyPage != null)
                {
                    _propertyPage.Dispose();
                    _propertyPage = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
