using Extract.Interop;
using System;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents verification metadata settings.
    /// </summary>
    public class MetadataSettings
    {
        #region Fields

        /// <summary>
        /// The path to the input ID Shield data file (VOA). May contain tags.
        /// </summary>
        string _dataFile;

        /// <summary>
        /// The path to the output verification metadata xml file. May contain tags.
        /// </summary>
        string _metadataFile;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSettings"/> class with default 
        /// settings.
        /// </summary>
        public MetadataSettings() : this(null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSettings"/> class.
        /// </summary>
        public MetadataSettings(string dataFile, string metadataFile)
        {
            _dataFile = dataFile ?? "<SourceDocName>.voa";
            _metadataFile = metadataFile ?? @"<SourceDocName>.xml";
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the the path to the input ID Shield data file. May contain tags.
        /// </summary>
        /// <value>The the path to the input ID Shield data file. May contain tags.</value>
        public string DataFile
        {
            get 
            {
                return _dataFile;
            }
            internal set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }

                _dataFile = value;
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
            internal set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }

                _metadataFile = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="MetadataSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="MetadataSettings"/>.</param>
        /// <returns>A <see cref="MetadataSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        internal static MetadataSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                string dataFile = reader.ReadString();
                string metadataFile = reader.ReadString();

                return new MetadataSettings(dataFile, metadataFile);
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
        internal void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_dataFile);
                writer.Write(_metadataFile);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26519",
                    "Unable to write verification metadata settings.", ex);
            }
        }

        #endregion Methods
    }
}
