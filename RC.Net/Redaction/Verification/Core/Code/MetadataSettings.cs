using Extract.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents verification metadata settings.
    /// </summary>
    public class MetadataSettings
    {
        #region MetadataSettings Fields

        /// <summary>
        /// <see langword="true"/> if verification metadata should always be output; 
        /// <see langword="false"/> if verification metadata should be output only for documents 
        /// that contain redactions.
        /// </summary>
        readonly bool _alwaysOutputMetadata;

        /// <summary>
        /// The path to the output verification metadata xml file. May contain tags.
        /// </summary>
        readonly string _metadataFile;

        #endregion MetadataSettings Fields

        #region MetadataSettings Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSettings"/> class with default 
        /// settings.
        /// </summary>
        public MetadataSettings() : this(true, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSettings"/> class.
        /// </summary>
        public MetadataSettings(bool alwaysOutputMetadata, string metadataFile)
        {
            _alwaysOutputMetadata = alwaysOutputMetadata;
            _metadataFile = metadataFile ?? @"<SourceDocName>.xml";
        }

        #endregion MetadataSettings Constructors

        #region MetadataSettings Properties

        /// <summary>
        /// Gets whether verification metadata should always be output.
        /// </summary>
        /// <returns><see langword="true"/> if verification metadata should always be output;
        /// <see langword="false"/> if verification metadata should be output only for documents 
        /// that contain redactions.</returns>
        public bool AlwaysOutputMetadata
        {
            get
            {
                return _alwaysOutputMetadata;
            }
        }

        /// <summary>
        /// Gets the path to the verification metadata xml file. May contain tags.
        /// </summary>
        /// <returns>The path to the verification metadata xml file. May contain tags.</returns>
        public string MetadataFile
        {
            get
            {
                return _metadataFile;
            }
        }

        #endregion MetadataSettings Properties

        #region MetadataSettings Methods

        /// <summary>
        /// Creates a <see cref="MetadataSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="MetadataSettings"/>.</param>
        /// <returns>A <see cref="MetadataSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        public static MetadataSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                bool alwaysOutputMetadata = reader.ReadBoolean();
                string metadataFile = reader.ReadString();

                return new MetadataSettings(alwaysOutputMetadata, metadataFile);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26518",
                    "Unable to read verification metadata settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="MetadataSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="MetadataSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_alwaysOutputMetadata);
                writer.Write(_metadataFile);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26519",
                    "Unable to write verification metadata settings.", ex);
            }
        }

        #endregion MetadataSettings Methods
    }
}
