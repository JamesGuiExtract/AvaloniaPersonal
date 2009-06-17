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
    }
}
