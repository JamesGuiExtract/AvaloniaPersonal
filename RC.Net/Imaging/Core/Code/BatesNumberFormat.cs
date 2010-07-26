using Extract.Drawing;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents the format settings for Bates numbers.
    /// </summary>
    [Serializable]
    public sealed class BatesNumberFormat : IDisposable, ICloneable, ISerializable
    {
        #region Constants

        /// <summary>
        /// The current version number for this object
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(BatesNumberFormat).ToString();

        #endregion Constants

        #region BatesNumberFormat Fields

        /// <summary>
        /// The next specified Bates number.
        /// </summary>
        long _nextNumber = RegistryManager._NEXT_BATES_NUMBER_DEFAULT;

        /// <summary>
        /// Whether or not to use the next Bates number file.
        /// </summary>
        bool _useNextNumberFile = RegistryManager._USE_NEXT_NUMBER_FILE_DEFAULT;

        /// <summary>
        /// The file name containing the next Bates number.
        /// </summary>
        string _nextNumberFile = RegistryManager._NEXT_NUMBER_FILE_DEFAULT;

        /// <summary>
        /// Whether or not to use the database counter to get the next Bates number.
        /// </summary>
        bool _useDatabaseCounter;

        /// <summary>
        /// The name of the database counter to use for getting the Bates number.
        /// </summary>
        string _databaseCounterName;

        /// <summary>
        /// Whether to zero pad the Bates number.
        /// </summary>
        bool _zeroPad = RegistryManager._BATES_ZERO_PAD_DEFAULT;

        /// <summary>
        /// The number of digits in the Bates number.
        /// </summary>
        int _numDigits = RegistryManager._BATES_DIGITS_DEFAULT;

        /// <summary>
        /// <see langword="true"/> if the Bates number is unique to each document; 
        /// <see langword="false"/> if the Bates number is unique to each page.
        /// </summary>
        bool _appendPageNumber = RegistryManager._BATES_APPEND_PAGE_NUMBER_DEFAULT;

        /// <summary>
        /// Whether to zero pad the page number of the Bates number.
        /// </summary>
        bool _zeroPadPage = RegistryManager._BATES_ZERO_PAD_PAGE_DEFAULT;

        /// <summary>
        /// The number of digits in the page number of the Bates number.
        /// </summary>
        int _numPageDigits = RegistryManager._BATES_PAGE_DIGITS_DEFAULT;

        /// <summary>
        /// The characters that divide the Bates number from page specifier.
        /// </summary>
        string _pageNumberSeparator = RegistryManager._BATES_PAGE_NUMBER_SEPARATOR_DEFAULT;

        /// <summary>
        /// The text to prepend to each Bates number.
        /// </summary>
        string _prefix = RegistryManager._BATES_PREFIX_DEFAULT;

        /// <summary>
        /// The text to append to each Bates number.
        /// </summary>
        string _suffix = RegistryManager._BATES_SUFFIX_DEFAULT;

        /// <summary>
        /// The font to use to display a Bates number.
        /// </summary>
        Font _font = new Font(RegistryManager._BATES_FONT_FAMILY_DEFAULT, 
            RegistryManager._BATES_FONT_SIZE_DEFAULT, RegistryManager._BATES_FONT_STYLE_DEFAULT);

        /// <summary>
        /// The number of horizontal inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        float _horizontalInches = RegistryManager._BATES_HORIZONTAL_INCHES_DEFAULT;

        /// <summary>
        /// The number of vertical inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        float _verticalInches = RegistryManager._BATES_VERTICAL_INCHES_DEFAULT;

        /// <summary>
        /// The alignment of the anchor point for the page.
        /// </summary>
        AnchorAlignment _pageAnchorAlignment = RegistryManager._BATES_PAGE_ANCHOR_ALIGNMENT_DEFAULT;

        /// <summary>
        /// The alignment of the anchor point for the Bates number.
        /// </summary>
        AnchorAlignment _anchorAlignment = RegistryManager._BATES_ANCHOR_ALIGNMENT_DEFAULT;

        #endregion BatesNumberFormat Fields

        #region BatesNumberFormat Constructors

        /// <overloads>Initializes a new instance of <see cref="BatesNumberFormat"/> class.</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="BatesNumberFormat"/> class.
        /// </summary>
        public BatesNumberFormat()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatesNumberFormat"/> class.
        /// </summary>
        /// <param name="useDatabaseCounter">If <see langword="true"/> then this
        /// formatter will default to using a database counter.</param>
        public BatesNumberFormat(bool useDatabaseCounter)
        {       
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI27862",
					_OBJECT_NAME);

            _useDatabaseCounter = useDatabaseCounter;
        }

        /// <summary>
        /// Intializes a new instance of <see cref="BatesNumberFormat"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object to use
        /// to de-serialize this object.</param>
        /// <param name="context">The streaming context for the de-serialization.</param>
        private BatesNumberFormat(SerializationInfo info, StreamingContext context)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI27863",
					_OBJECT_NAME);

                int version = info.GetInt32("Version");
                if (version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI27864",
                        "Cannot load newer version of " + _OBJECT_NAME);
                    ee.AddDebugData("Current Version", _CURRENT_VERSION, false);
                    ee.AddDebugData("Version To Load", version, false);
                    throw ee;
                }

                _nextNumber = info.GetInt64("NextNumber");
                _useNextNumberFile = info.GetBoolean("UseNextNumberFile");
                _nextNumberFile = info.GetString("NextNumberFile");
                _zeroPad = info.GetBoolean("ZeroPad");
                _numDigits = info.GetInt32("NumDigits");
                _appendPageNumber = info.GetBoolean("AppendPageNumber");
                _zeroPadPage = info.GetBoolean("ZeroPadPage");
                _numPageDigits = info.GetInt32("NumPageDigits");
                _pageNumberSeparator = info.GetString("PageNumberSeparator");
                _prefix = info.GetString("Prefix");
                _suffix = info.GetString("Suffix");
                _anchorAlignment = 
                    (AnchorAlignment)info.GetValue("AnchorAlignment", typeof(AnchorAlignment));
                _pageAnchorAlignment =
                    (AnchorAlignment)info.GetValue("PageAnchorAlignment", typeof(AnchorAlignment));
                _font = (Font)info.GetValue("Font", typeof(Font));
                _horizontalInches = info.GetSingle("HorizontalInches");
                _verticalInches = info.GetSingle("VerticalInches");
                _useDatabaseCounter = info.GetBoolean("UseDatabaseCounter");
                _databaseCounterName = info.GetString("DatabaseCounterName");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27865", ex);
            }
        }

        #endregion BatesNumberFormat Constructors

        #region BatesNumberFormat Properties

        /// <summary>
        /// Gets or sets the value of the next Bates number.
        /// </summary>
        /// <returns>The value of the next Bates number.</returns>
        public long NextNumber
        {
            get
            {
                return _nextNumber;
            }
            set
            {
                _nextNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets whether a next Bates number file is being used.
        /// </summary>
        /// <value><see langword="true"/> if a next Bates number file is being used; 
        /// <see langword="false"/> if the next Bates number is stored in the registry.</value>
        /// <returns><see langword="true"/> if a next Bates number file is being used; 
        /// <see langword="false"/> if the next Bates number is stored in the registry.</returns>
        public bool UseNextNumberFile
        {
            get
            {
                return _useNextNumberFile;
            }
            set
            {
                _useNextNumberFile = value;
            }
        }

        /// <summary>
        /// Gets or sets the file name containing the next Bates number.
        /// </summary>
        /// <value>The file name containing the next Bates number.</value>
        /// <returns>The file name containing the next Bates number.</returns>
        public string NextNumberFile
        {
            get
            {
                return _nextNumberFile;
            }
            set
            {
                _nextNumberFile = value;
            }
        }

        /// <summary>
        /// Gets the next Bates number as text.
        /// </summary>
        /// <returns>The next Bates number as text.</returns>
        public string NextNumberString
        {
            get
            {
                return _nextNumber.ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets or sets whether a datbase counter is being used.
        /// </summary>
        /// <value><see langword="true"/> if a database counter is being used; 
        /// <see langword="false"/> if it is not being used.</value>
        /// <returns><see langword="true"/> if a database counter is being used; 
        /// <see langword="false"/> if it is not being used.</returns>
        public bool UseDatabaseCounter
        {
            get
            {
                return _useDatabaseCounter;
            }
            set
            {
                _useDatabaseCounter = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the database counter containing the next Bates number.
        /// </summary>
        /// <value>The database counter containing the next Bates number.</value>
        /// <returns>The database counter containing the next Bates number.</returns>
        public string DatabaseCounter
        {
            get
            {
                return _databaseCounterName;
            }
            set
            {
                _databaseCounterName = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to zero pad the Bates number.
        /// </summary>
        /// <value><see langword="true"/> if the Bates number should be zero padded;
        /// <see langword="false"/> if the Bates number should not be zero padded.</value>
        /// <returns><see langword="true"/> if the Bates number should be zero padded;
        /// <see langword="false"/> if the Bates number should not be zero padded.</returns>
        public bool ZeroPad
        {
            get
            {
                return _zeroPad;
            }
            set
            {
                _zeroPad = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of digits in the Bates number after zero padding.
        /// </summary>
        /// <value>The number of digits in the Bates number after zero padding.</value>
        /// <returns>The number of digits in the Bates number after zero padding.</returns>
        public int Digits
        {
            get
            {
                return _numDigits;
            }
            set
            {
                _numDigits = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to append the page number to the Bates number.
        /// </summary>
        /// <value>Whether to append the page number to the Bates number.</value>
        /// <returns>Whether to append the page number to the Bates number.</returns>
        public bool AppendPageNumber
        {
            get
            {
                return _appendPageNumber;
            }
            set
            {
                _appendPageNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to zero pad the page number.
        /// </summary>
        /// <value><see langword="true"/> if the page number should be zero padded;
        /// <see langword="false"/> if the page number should not be zero padded.</value>
        /// <returns><see langword="true"/> if the page number should be zero padded;
        /// <see langword="false"/> if the page number should not be zero padded.</returns>
        public bool ZeroPadPage
        {
            get
            {
                return _zeroPadPage;
            }
            set
            {
                _zeroPadPage = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of digits in the page number after zero padding.
        /// </summary>
        /// <value>The number of digits in the page number after zero padding.</value>
        /// <returns>The number of digits in the page number after zero padding.</returns>
        public int PageDigits
        {
            get
            {
                return _numPageDigits;
            }
            set
            {
                _numPageDigits = value;
            }
        }

        /// <summary>
        /// Gets or sets the characters that divide the Bates number from the page number.
        /// </summary>
        /// <value>The characters that divide the Bates number from the page number.</value>
        /// <returns>The characters that divide the Bates number from the page number.</returns>
        public string PageNumberSeparator
        {
            get
            {
                return _pageNumberSeparator;
            }
            set
            {
                _pageNumberSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets the text to prepend to the Bates number.
        /// </summary>
        /// <value>The text to prepend to the Bates number.</value>
        /// <returns>The text to prepend to the Bates number.</returns>
        public string Prefix
        {
            get
            {
                return _prefix;
            }
            set
            {
                _prefix = value;
            }
        }

        /// <summary>
        /// Gets or sets the text to append to the Bates number.
        /// </summary>
        /// <value>The text to append to the Bates number.</value>
        /// <returns>The text to append to the Bates number.</returns>
        public string Suffix
        {
            get
            {
                return _suffix;
            }
            set
            {
                _suffix = value;
            }
        }

        /// <summary>
        /// Gets or sets the font to use when displaying a Bates number.
        /// </summary>
        /// <value>The font to use when displaying a Bates number.</value>
        /// <returns>The font to use when displaying a Bates number.</returns>
        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                try
                {
                    // No need to set if the fonts are already the same
                    if (_font != value)
                    {
                        // Dispose of the previous font
                        if (_font != null)
                        {
                            _font.Dispose();
                        }

                        // Store the new font
                        _font = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27866", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        /// <value>The horizontal inches from the side of the page where the Bates number is 
        /// placed.</value>
        /// <returns>The horizontal inches from the side of the page where the Bates number is 
        /// placed.</returns>
        public float HorizontalInches
        {
            get
            {
                return _horizontalInches;
            }
            set
            {
                _horizontalInches = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical inches from the side of the page where the Bates number is 
        /// placed.
        /// </summary>
        /// <value>The vertical inches from the side of the page where the Bates number is 
        /// placed.</value>
        /// <returns>The vertical inches from the side of the page where the Bates number is 
        /// placed.</returns>
        public float VerticalInches
        {
            get
            {
                return _verticalInches;
            }
            set
            {
                _verticalInches = value;
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the anchor point for the page.
        /// </summary>
        /// <value>The alignment of the anchor point for the page.</value>
        /// <returns>The alignment of the anchor point for the page.</returns>
        public AnchorAlignment PageAnchorAlignment
        {
            get
            {
                return _pageAnchorAlignment;
            }
            set
            {
                _pageAnchorAlignment = value;
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the anchor point for the Bates number.
        /// </summary>
        /// <value>The alignment of the anchor point for the Bates number.</value>
        /// <returns>The alignment of the anchor point for the Bates number.</returns>
        public AnchorAlignment AnchorAlignment
        {
            get
            {
                return _anchorAlignment;
            }
            set
            {
                _anchorAlignment = value;
            }
        }
        #endregion BatesNumberFormat Properties

        #region BatesNumberFormat Methods

        /// <summary>
        /// Creates a <see cref="BatesNumberFormat"/> from the values in the registry.
        /// </summary>
        /// <returns></returns>
        public static BatesNumberFormat FromRegistry()
        {
            // Create the Bates number format
            BatesNumberFormat format = new BatesNumberFormat();

            // Read the settings from the registry
            format.ReadFromRegistry();

            return format;
        }

        /// <summary>
        /// Reads the settings from the registry
        /// </summary>
        public void ReadFromRegistry()
        {
            try
            {
                // Read the settings from the registry
                _nextNumber = RegistryManager.NextBatesNumber;
                _useNextNumberFile = RegistryManager.UseNextNumberFile;
                _nextNumberFile = RegistryManager.NextNumberFile;
                _zeroPad = RegistryManager.BatesZeroPad;
                _numDigits = RegistryManager.BatesDigits;
                _appendPageNumber = RegistryManager.BatesAppendPageNumber;
                _zeroPadPage = RegistryManager.BatesZeroPadPage;
                _numPageDigits = RegistryManager.BatesPageDigits;
                _pageNumberSeparator = RegistryManager.BatesPageNumberSeparator;
                _prefix = RegistryManager.BatesPrefix;
                _suffix = RegistryManager.BatesSuffix;
                _anchorAlignment = RegistryManager.BatesAnchorAlignment;
                _pageAnchorAlignment = RegistryManager.BatesPageAnchorAlignment;
                _horizontalInches = RegistryManager.BatesHorizontalInches;
                _verticalInches = RegistryManager.BatesVerticalInches;
                _useDatabaseCounter = RegistryManager.UseDatabaseCounter;
                _databaseCounterName = RegistryManager.DatabaseCounterName;

                // Use the property to ensure dispose is handled correctly
                Font = RegistryManager.BatesFont;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27867", ex);
            }
        }

        /// <summary>
        /// Writes all the format settings to the registry.
        /// </summary>
        public void WriteToRegistry()
        {
            try
            {
                // Write the settings to the registry
                RegistryManager.NextBatesNumber = _nextNumber;
                RegistryManager.UseNextNumberFile = _useNextNumberFile;
                RegistryManager.NextNumberFile = _nextNumberFile;
                RegistryManager.BatesZeroPad = _zeroPad;
                RegistryManager.BatesDigits = _numDigits;
                RegistryManager.BatesAppendPageNumber = _appendPageNumber;
                RegistryManager.BatesZeroPadPage = _zeroPadPage;
                RegistryManager.BatesPageDigits = _numPageDigits;
                RegistryManager.BatesPageNumberSeparator = _pageNumberSeparator;
                RegistryManager.BatesPrefix = _prefix;
                RegistryManager.BatesSuffix = _suffix;
                RegistryManager.BatesAnchorAlignment = _anchorAlignment;
                RegistryManager.BatesPageAnchorAlignment = _pageAnchorAlignment;
                RegistryManager.BatesFont = _font;
                RegistryManager.BatesHorizontalInches = _horizontalInches;
                RegistryManager.BatesVerticalInches = _verticalInches;
                RegistryManager.UseDatabaseCounter = _useDatabaseCounter;
                RegistryManager.DatabaseCounterName = _databaseCounterName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27868", ex);
            }
        }

        /// <summary>
        /// Resets the settings to their default values.
        /// </summary>
        public void ResetToDefaults()
        {
            try
            {
                _nextNumber = RegistryManager._NEXT_BATES_NUMBER_DEFAULT;
                _useNextNumberFile = RegistryManager._USE_NEXT_NUMBER_FILE_DEFAULT;
                _nextNumberFile = RegistryManager._NEXT_NUMBER_FILE_DEFAULT;
                _zeroPad = RegistryManager._BATES_ZERO_PAD_DEFAULT;
                _numDigits = RegistryManager._BATES_DIGITS_DEFAULT;
                _appendPageNumber = RegistryManager._BATES_APPEND_PAGE_NUMBER_DEFAULT;
                _zeroPadPage = RegistryManager._BATES_ZERO_PAD_PAGE_DEFAULT;
                _numPageDigits = RegistryManager._BATES_PAGE_DIGITS_DEFAULT;
                _pageNumberSeparator = RegistryManager._BATES_PAGE_NUMBER_SEPARATOR_DEFAULT;
                _prefix = RegistryManager._BATES_PREFIX_DEFAULT;
                _suffix = RegistryManager._BATES_SUFFIX_DEFAULT;
                _anchorAlignment = RegistryManager._BATES_ANCHOR_ALIGNMENT_DEFAULT;
                _pageAnchorAlignment = RegistryManager._BATES_PAGE_ANCHOR_ALIGNMENT_DEFAULT;
                _horizontalInches = RegistryManager._BATES_HORIZONTAL_INCHES_DEFAULT;
                _verticalInches = RegistryManager._BATES_VERTICAL_INCHES_DEFAULT;
                _useDatabaseCounter = false;
                _databaseCounterName = "";

                // Use the property to ensure dispose is handled correctly
                Font = new Font(RegistryManager._BATES_FONT_FAMILY_DEFAULT,
                RegistryManager._BATES_FONT_SIZE_DEFAULT, RegistryManager._BATES_FONT_STYLE_DEFAULT);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27869", ex);
            }
        }

        #endregion BatesNumberFormat Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BatesNumberFormat"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="BatesNumberFormat"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BatesNumberFormat"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_font != null)
                {
                    _font.Dispose();
                    _font = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region ICloneable Members

        /// <summary>
        /// Creates a clone of the current <see cref="BatesNumberFormat"/> object.
        /// </summary>
        /// <returns>A clone of the current <see cref="BatesNumberFormat"/> object.</returns>
        object ICloneable.Clone()
        {
            return GetClone();
        }

        /// <summary>
        /// Creates a clone of the current <see cref="BatesNumberFormat"/> object.
        /// </summary>
        /// <returns>A clone of the current <see cref="BatesNumberFormat"/> object.</returns>
        public BatesNumberFormat Clone()
        {
            return GetClone();
        }

        /// <summary>
        /// Creates a clone of the current <see cref="BatesNumberFormat"/> object.
        /// </summary>
        /// <returns>A clone of the current <see cref="BatesNumberFormat"/> object.</returns>
        private BatesNumberFormat GetClone()
        {
            // Performs a memberwise clone of the object
            BatesNumberFormat format = (BatesNumberFormat) this.MemberwiseClone();

            // Clone the font (do not set the font through the
            // property otherwise it will be disposed)
            format._font = (Font)_font.Clone();

            return format;
        }

        #endregion ICloneable

        #region ISerializable Members

        /// <summary>
        /// Serializes the object to the specified <see cref="SerializationInfo"/> stream.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> stream to serialize the
        /// object to.</param>
        /// <param name="context">The <see cref="StreamingContext"/> for the
        /// serialization stream.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            try
            {
                info.AddValue("Version", _CURRENT_VERSION);
                info.AddValue("NextNumber", _nextNumber);
                info.AddValue("UseNextNumberFile", _useNextNumberFile);
                info.AddValue("NextNumberFile", _nextNumberFile);
                info.AddValue("ZeroPad", _zeroPad);
                info.AddValue("NumDigits", _numDigits);
                info.AddValue("AppendPageNumber", _appendPageNumber);
                info.AddValue("ZeroPadPage", _zeroPadPage);
                info.AddValue("NumPageDigits", _numPageDigits);
                info.AddValue("PageNumberSeparator", _pageNumberSeparator);
                info.AddValue("Prefix", _prefix);
                info.AddValue("Suffix", _suffix);
                info.AddValue("AnchorAlignment", _anchorAlignment, typeof(AnchorAlignment));
                info.AddValue("PageAnchorAlignment", _pageAnchorAlignment, typeof(AnchorAlignment));
                info.AddValue("Font", _font, typeof(Font));
                info.AddValue("HorizontalInches", _horizontalInches);
                info.AddValue("VerticalInches", _verticalInches);
                info.AddValue("UseDatabaseCounter", _useDatabaseCounter);
                info.AddValue("DatabaseCounterName", _databaseCounterName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27870", ex);
            }
        }

        #endregion
    }
}
