using Extract;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace IDShieldOffice
{
    /// <summary>
    /// Specifies the set of ID Shield Office output formats.
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>
        /// Tagged image file format.
        /// </summary>
        Tif,

        /// <summary>
        /// Non-searchable PDF format.
        /// </summary>
        Pdf,

        /// <summary>
        /// ID Shield Office file format.
        /// </summary>
        Idso
    }

    /// <summary>
    /// Represents the user-specified preferences for the ID Shield Office application.
    /// </summary>
    public class UserPreferences : IUserConfigurableComponent, IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(UserPreferences).ToString();

        #endregion Constants

        #region UserPreferences Fields

        /// <summary>
        /// The tradeoff setting between fast or accurate OCR.
        /// </summary>
        OcrTradeoff _ocrTradeoff = RegistryManager._OCR_TRADEOFF_DEFAULT;

        /// <summary>
        /// Whether to save ID Shield Office files whenever an output image is created.
        /// </summary>
        bool _saveIdsoWithImage = RegistryManager._VERIFY_ALL_PAGES_DEFAULT;

        /// <summary>
        /// The default output file format.
        /// </summary>
        OutputFormat _outputFormat = RegistryManager._OUTPUT_FORMAT_DEFAULT;

        /// <summary>
        /// The default path for ID Shield Office output files.
        /// </summary>
        string _outputPath = RegistryManager._OUTPUT_PATH_DEFAULT;

        /// <summary>
        /// The default use output path check box state.
        /// </summary>
        bool _useOutputPath = RegistryManager._USE_OUTPUT_PATH_DEFAULT;

        /// <summary>
        /// The fill color of new redactions.
        /// </summary>
        RedactionColor _redactionFillColor = RegistryManager._REDACTION_FILL_COLOR_DEFAULT;

        /// <summary>
        /// <see langword="true"/> if all pages need to be visited before a dirty document can be 
        /// saved or printed; <see langword="false"/> if a dirty document can be saved or printed 
        /// regardless of whether all pages have been visited.
        /// </summary>
        bool _verifyAllPages = RegistryManager._VERIFY_ALL_PAGES_DEFAULT;

        /// <summary>
        /// Manages the Bates numbering of documents.
        /// </summary>
        readonly BatesNumberManager _batesManager;

        /// <summary>
        /// The property page associated with the <see cref="UserPreferences"/>.
        /// </summary>
        UserPreferencesPropertyPage _propertyPage;

        /// <summary>
        /// Miscellaneous utilities, used to expand path tags in the default output path.
        /// </summary>
        MiscUtils _miscUtils;

        /// <summary>
        /// The image viewer with which the <see cref="UserPreferences"/> are associated.
        /// </summary>
        readonly ImageViewer _imageViewer;

        #endregion UserPreferences Fields

        #region UserPreferences Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferences"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer for which the preferences apply.</param>
        public UserPreferences(ImageViewer imageViewer)
            : this(imageViewer, new BatesNumberManager(imageViewer))
        {
            
        }

        /// <overloads>Initializes a new instance of the <see cref="UserPreferences"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferences"/> class with the 
        /// specified <see cref="BatesNumberManager"/>.
        /// </summary>
        /// <param name="imageViewer">The image viewer for which the preferences apply.</param>
        /// <param name="manager">The Bates number manager to use for the preferences.</param>
        private UserPreferences(ImageViewer imageViewer, BatesNumberManager manager)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23202",
                    _OBJECT_NAME);

                _imageViewer = imageViewer;

                _batesManager = manager;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23203", ex);
            }
        }

        #endregion UserPreferences Constructors

        #region UserPreferences Properties

        /// <summary>
        /// Gets or sets the tradeoff between fast or accurate OCR.
        /// </summary>
        /// <value>The tradeoff between fast or accurate OCR.</value>
        /// <returns>The tradeoff between fast or accurate OCR.</returns>
        public OcrTradeoff OcrTradeoff
        {
            get
            {
                return _ocrTradeoff;
            }
            set
            {
                _ocrTradeoff = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to save an ID Shield Office file whenever an image file is saved.
        /// </summary>
        /// <value><see langword="true"/> if an ID Shield Office file should be saved when an image 
        /// file is saved; <see langword="false"/> if only an image file should be saved.</value>
        /// <returns><see langword="true"/> if an ID Shield Office file should be saved when an image 
        /// file is saved; <see langword="false"/> if only an image file should be saved.</returns>
        public bool SaveIdsoWithImage
        {
            get
            {
                return _saveIdsoWithImage;
            }
            set
            {
                _saveIdsoWithImage = value;
            }
        }

        /// <summary>
        /// Gets or sets the default output file format.
        /// </summary>
        /// <value>The default output file format.</value>
        /// <returns>The default output file format.</returns>
        public OutputFormat OutputFormat
        {
            get
            {
                return _outputFormat;
            }
            set
            {
                _outputFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the default path for ID Shield Office output files.
        /// </summary>
        /// <value>The default path for ID Shield Office output files.</value>
        /// <returns>The default path for ID Shield Office output files.</returns>
        public string OutputPath
        {
            get
            {
                return _outputPath;
            }
            set
            {
                _outputPath = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to use the output path for default save location.
        /// </summary>
        /// <value>If <see langword="true"/> then IDSO will use <see cref="OutputPath"/> as
        /// the default save path. If <see langword="false"/> the IDSO will not use
        /// <see cref="OutputPath"/> as the default save path.</value>
        /// <returns>Whether IDSO will use <see cref="OutputPath"/> as the default save path.
        /// </returns>
        public bool UseOutputPath
        {
            get
            {
                return _useOutputPath;
            }
            set
            {
                _useOutputPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the fill color of new redactions.
        /// </summary>
        /// <value>The fill color of new redactions.</value>
        /// <returns>The fill color of new redactions.</returns>
        public RedactionColor RedactionFillColor
        {
            get
            {
                return _redactionFillColor;
            }
            set
            {
                _redactionFillColor = value;

                // Also update the image viewer
                _imageViewer.DefaultRedactionFillColor = _redactionFillColor;
            }
        }

        /// <summary>
        /// Gets or sets whether all pages of a document must be visited before it can be saved or 
        /// printed.
        /// </summary>
        /// <value><see langword="true"/> if all pages of a dirty document must be visited before 
        /// it can be saved or printed; <see langword="false"/> if a dirty document can be saved 
        /// or printed without visiting all pages.</value>
        /// <returns><see langword="true"/> if all pages of a dirty document must be visited 
        /// before it can be saved or printed; <see langword="false"/> if a dirty document can be 
        /// saved or printed without visiting all pages.</returns>
        public bool VerifyAllPages
        {
            get
            {
                return _verifyAllPages;
            }
            set
            {
                _verifyAllPages = value;
            }
        }

        /// <summary>
        /// Gets the Bates numbering manager.
        /// </summary>
        /// <value>The Bates numbering manager.</value>
        internal BatesNumberManager BatesNumberManager
        {
            get
            {
                return _batesManager;
            }
        }

        #endregion UserPreferences Properties

        #region UserPreferences Methods

        /// <summary>
        /// Gets the fully expanded output path.
        /// </summary>
        /// <returns>The fully expanded output path.</returns>
        // This method performs a computation, so is better suited as a method
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetFullOutputPath()
        {
            try
            {
                // Instantiate MiscUtils if not already
                if (_miscUtils == null)
                {
                    _miscUtils = new MiscUtils();
                }

                // Return the expanded output path
                return _miscUtils.GetExpandedTags(
                    _outputFormat == OutputFormat.Idso ? @"<SourceDocName>.idso" : _outputPath, 
                    _imageViewer.ImageFile);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23357", ex);
            }
        }

        /// <summary>
        /// Reads a <see cref="UserPreferences"/> object from the registry.
        /// </summary>
        /// <param name="imageViewer">The image viewer with which the preferences are associated.
        /// </param>
        /// <returns>A <see cref="UserPreferences"/> object that was read from the registry.
        /// </returns>
        public static UserPreferences FromRegistry(ImageViewer imageViewer)
        {
            try
            {
                // Create the user preferences
                UserPreferences preferences = new UserPreferences(imageViewer);

                // Set the properties from the registry
                preferences.ReadFromRegistry();

                // Return the result
                return preferences;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23360", ex);
            }
        }

        /// <summary>
        /// Reads teh user preferences from the registry.
        /// </summary>
        public void ReadFromRegistry()
        {
            try
            {
                // Read the Bates manager from the registry
                _batesManager.ReadFromRegistry();

                // Read the settings from the registry
                _ocrTradeoff = RegistryManager.OcrTradeoff;
                _useOutputPath = RegistryManager.UseOutputPath;
                _outputFormat = RegistryManager.OutputFormat;
                _outputPath = RegistryManager.OutputPath;
                _redactionFillColor = RegistryManager.RedactionFillColor;
                _imageViewer.DefaultRedactionFillColor = _redactionFillColor;
                _saveIdsoWithImage = RegistryManager.SaveIdsoWithImage;
                _verifyAllPages = RegistryManager.VerifyAllPages;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23361", ex);
            }
        }

        /// <summary>
        /// Writes the user preferences to the registry.
        /// </summary>
        public void WriteToRegistry()
        {
            try
            {
                // Write the Bates number manager's settings
                _batesManager.WriteToRegistry();

                // Write other settings to the registry
                RegistryManager.OcrTradeoff = _ocrTradeoff;
                RegistryManager.OutputFormat = _outputFormat;
                RegistryManager.UseOutputPath = _useOutputPath;
                RegistryManager.OutputPath = _outputPath;
                RegistryManager.RedactionFillColor = _redactionFillColor;
                RegistryManager.SaveIdsoWithImage = _saveIdsoWithImage;
                RegistryManager.VerifyAllPages = _verifyAllPages;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23362", ex);
            }
        }

        /// <summary>
        /// Resets all user preferences to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            try
            {
                _batesManager.ResetToDefaults();

                // Reset the settings
                _ocrTradeoff = RegistryManager._OCR_TRADEOFF_DEFAULT;
                _useOutputPath = RegistryManager._USE_OUTPUT_PATH_DEFAULT;
                _outputFormat = RegistryManager._OUTPUT_FORMAT_DEFAULT;
                _outputPath = RegistryManager._OUTPUT_PATH_DEFAULT;
                _redactionFillColor = RegistryManager._REDACTION_FILL_COLOR_DEFAULT;
                _imageViewer.DefaultRedactionFillColor = _redactionFillColor;
                _saveIdsoWithImage = RegistryManager._SAVE_IDSO_WITH_IMAGE_DEFAULT;
                _verifyAllPages = RegistryManager._VERIFY_ALL_PAGES_DEFAULT;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23443", ex);
            }
        }

        #endregion UserPreferences Methods

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
                    _propertyPage = new UserPreferencesPropertyPage(_imageViewer, this);
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
                }
                if (_batesManager != null)
                {
                    _batesManager.Dispose();
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
