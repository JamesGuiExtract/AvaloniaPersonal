using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents the settings that will be used by the <see cref="ModifyPdfFileTask"/>
    /// file processing task.
    /// </summary>
    [ComVisible(true)]
    [Guid("84D38576-E354-42B6-A281-B4AF1C2FFFB6")]
    [ProgId("Extract.FileActionManager.FileProcessors.ModifyPdfFileTaskSettings")]
    public class ModifyPdfFileTaskSettings
    {
        #region Fields

        /// <summary>
        /// The PDF file to modify.
        /// </summary>
        string _pdfFile;

        /// <summary>
        /// Whether annotations should be removed or not.
        /// </summary>
        bool _removeAnnotations;

        /// <summary>
        /// Indicates whether hyperlinks should be added or not.
        /// </summary>
        bool _addHyperlinks;

        /// <summary>
        /// If <see cref="AddHyperlinks"/> is <see langword="true"/>, the names of the attributes
        /// for which hyperlinks should be added.
        /// </summary>
        string _hyperlinkAttributes;

        /// <summary>
        /// <see langword="true"/> if the attributes' values are the addresses to use,
        /// <see langword="false"/> if <see cref="HyperlinkAddress"/> specifies the address for all
        /// hyperlinks.
        /// </summary>
        bool _useValueAsAddress = true;

        /// <summary>
        /// If <see cref="UseValueAsAddress"/> is <see langword="false"/>, the address to use for
        /// all hyperlinks.
        /// </summary>
        string _hyperlinkAddress;

        /// <summary>
        /// The VOA file containing the attributes to use to create the hyperlinks.
        /// </summary>
        string _dataFileName = "<SourceDocName>.voa";

        /// <summary>
        /// Whether the object is dirty or not.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTaskSettings"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTaskSettings"/> class.
        /// </summary>
        public ModifyPdfFileTaskSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTaskSettings"/> class.
        /// </summary>
        /// <param name="settings">Another <see cref="ModifyPdfFileTaskSettings"/>
        /// to copy the settings from.</param>
        public ModifyPdfFileTaskSettings(ModifyPdfFileTaskSettings settings)
        {
            try
            {
                _pdfFile = settings.PdfFile;
                _removeAnnotations = settings.RemoveAnnotations;
                _addHyperlinks = settings.AddHyperlinks;
                _hyperlinkAttributes = settings.HyperlinkAttributes;
                _useValueAsAddress = settings.UseValueAsAddress;
                _hyperlinkAddress = settings.HyperlinkAddress;
                _dataFileName = settings.DataFileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36278");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the PDF file to process.
        /// </summary>
        /// <value>The file name to process (may contain DocTags).</value>
        public string PdfFile
        {
            get
            {
                return _pdfFile;
            }

            set
            {
                try
                {
                    if (value != _pdfFile)
                    {
                        _pdfFile = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36279");
                }
            }
        }

        /// <summary>
        /// Gets/sets whether annotations should be removed from the PDF file.
        /// </summary>
        /// <value>If <see langword="true"/> then annotations will be removed. If
        /// <see langword="false"/> annotations will not be removed.</value>
        public bool RemoveAnnotations
        {
            get
            {
                return _removeAnnotations;
            }

            set
            {
                try
                {
                    if (value != _removeAnnotations)
                    {
                        _removeAnnotations = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36280");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether hyperlinks should be added or not.
        /// </summary>
        /// <value><see langword="true"/> if hyperlinks should be added to the PDF; otherwise,
        /// <see langword="false"/>.</value>
        public bool AddHyperlinks
        {
            get
            {
                return _addHyperlinks;
            }

            set
            {
                try
                {
                    if (value != _addHyperlinks)
                    {
                        _addHyperlinks = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36281");
                }
            }
        }

        /// <summary>
        /// Gets or sets the names of the attributes for which hyperlinks should be added. 
        /// </summary>
        /// <value>
        /// The names of the attributes for which hyperlinks should be added. 
        /// </value>
        public string HyperlinkAttributes
        {
            get
            {
                return _hyperlinkAttributes;
            }

            set
            {
                try
                {
                    if (value != _hyperlinkAttributes)
                    {
                        _hyperlinkAttributes = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36282");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to use the attributes' values as the hyperlink addresses.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the attributes' values are the addresses to use,
        /// <see langword="false"/> if <see cref="HyperlinkAddress"/> specifies the address for all
        /// hyperlinks.
        /// </value>
        public bool UseValueAsAddress
        {
            get
            {
                return _useValueAsAddress;
            }

            set
            {
                try
                {
                    if (value != _useValueAsAddress)
                    {
                        _useValueAsAddress = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36283");
                }
            }
        }

        /// <summary>
        /// Gets or sets the address to use for all hyperlinks if <see cref="UseValueAsAddress"/>
        /// is <see langword="false"/>
        /// </summary>
        /// <value>
        /// The address to use for all hyperlinks.
        /// </value>
        public string HyperlinkAddress
        {
            get
            {
                return _hyperlinkAddress;
            }

            set
            {
                try
                {
                    if (value != _hyperlinkAddress)
                    {
                        _hyperlinkAddress = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36284");
                }
            }
        }

        /// <summary>
        /// Gets or sets the VOA file containing the attributes to use to create the hyperlinks.
        /// </summary>
        /// <value>
        /// The VOA file containing the attributes to use to create the hyperlinks.
        /// </value>
        public string DataFileName
        {
            get
            {
                return _dataFileName;
            }

            set
            {
                try
                {
                    if (value != _dataFileName)
                    {
                        _dataFileName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36285");
                }
            }
        }

        /// <summary>
        /// Gets whether the current settings are valid.
        /// </summary>
        /// <value><see langword="true"/> if the settings are valid and
        /// <see langword="false"/> if not valid.</value>
        [ComVisible(false)]
        public bool ValidSettings
        {
            get
            {
                try
                {
                    var tagManager = new FAMTagManagerClass();

                    // Settings are valid iff:
                    // 1. A pdf file name is defined
                    // 2. The pdf file name does not contain any invalid doc tags
                    // 3. Either annotations are configured to be removed or hyperlinks are
                    //  configured to be added.
                    // 4. If hyperlinks are being added, both the attribute names and data file have
                    //  been specified.
                    // 5. If configured to use a hard-coded hyperlink address, the address has been
                    //  specified.
                    bool valid = !string.IsNullOrEmpty(_pdfFile)
                        && !(tagManager.StringContainsInvalidTags(_pdfFile))
                        && (_removeAnnotations || _addHyperlinks);

                    if (valid && _addHyperlinks)
                    {
                        valid &= !string.IsNullOrEmpty(_hyperlinkAttributes);
                        valid &= !string.IsNullOrEmpty(_dataFileName) &&
                            !tagManager.StringContainsInvalidTags(_dataFileName);
                        valid &= _useValueAsAddress || !string.IsNullOrWhiteSpace(_hyperlinkAddress);
                    }

                    return valid;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI29640", ex);
                }
            }
        }

        /// <summary>
        /// Gets whether the object is dirty or not.
        /// </summary>
        /// <value><see langword="true"/> if the object is dirty.</value>
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="ModifyPdfFileTaskSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="ModifyPdfFileTaskSettings"/>.</param>
        /// <returns>A <see cref="ModifyPdfFileTaskSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        [ComVisible(false)]
        internal static ModifyPdfFileTaskSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                // Read the settings from the stream
                var settings = new ModifyPdfFileTaskSettings();
                settings.PdfFile = reader.ReadString();
                settings.RemoveAnnotations = reader.ReadBoolean();

                if (reader.Version >= 2)
                {
                    settings.AddHyperlinks = reader.ReadBoolean();
                    settings.HyperlinkAttributes = reader.ReadString();
                    settings.UseValueAsAddress = reader.ReadBoolean();
                    settings.HyperlinkAddress = reader.ReadString();
                    settings.DataFileName = reader.ReadString();
                }

                return settings;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29641", "Unable to read modify pdf settings.", ex);
            }
        }


        /// <summary>
        /// Writes the <see cref="ModifyPdfFileTaskSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="ModifyPdfFileTaskSettings"/> will be written.</param>
        /// <param name="clearDirty">Whether the dirty flag should be reset of not.</param>
        [ComVisible(false)]
        internal void WriteTo(IStreamWriter writer, bool clearDirty)
        {
            try
            {
                writer.Write(_pdfFile);
                writer.Write(_removeAnnotations);
                writer.Write(_addHyperlinks);
                writer.Write(_hyperlinkAttributes);
                writer.Write(_useValueAsAddress);
                writer.Write(_hyperlinkAddress);
                writer.Write(_dataFileName);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29642",
                    "Unable to write Modify pdf file settings.", ex);
            }
        }

        #endregion Methods
    }
}
