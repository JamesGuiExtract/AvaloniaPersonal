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
            : this(null, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTaskSettings"/> class.
        /// </summary>
        /// <param name="settings">Another <see cref="ModifyPdfFileTaskSettings"/>
        /// to copy the settings from.</param>
        public ModifyPdfFileTaskSettings(ModifyPdfFileTaskSettings settings)
            : this(settings.PdfFile, settings.RemoveAnnotations)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTaskSettings"/> class.
        /// </summary>
        /// <param name="pdfFile">The pdf file to process.</param>
        /// <param name="removeAnnotations">Whether annotations should be removed
        /// from the pdf file or not.</param>
        public ModifyPdfFileTaskSettings(string pdfFile, bool removeAnnotations)
        {
            try
            {
                _pdfFile = pdfFile;
                _removeAnnotations = removeAnnotations;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29639",
                    "Failed to initialize Modify Pdf task settings.", ex);
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
                _pdfFile = value;
                _dirty = true;
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
                _removeAnnotations = value;
                _dirty = true;
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
                    // Settings are valid iff:
                    // 1. A pdf file name is defined
                    // 2. The pdf file name does not contain any invalid doc tags
                    bool valid = !string.IsNullOrEmpty(_pdfFile)
                        && !(new FAMTagManagerClass().StringContainsInvalidTags(_pdfFile));

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
                string pdfFile = reader.ReadString();
                bool removeAnnotations = reader.ReadBoolean();

                return new ModifyPdfFileTaskSettings(pdfFile, removeAnnotations);
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
