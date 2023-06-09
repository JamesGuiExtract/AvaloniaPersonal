using Extract.Interop;
using System;
namespace Extract.Redaction
{
    /// <summary>
    /// Represents settings for the <see cref="SurroundContextSettings"/>.
    /// </summary>
    public class SurroundContextSettings
    {
        #region Fields

        /// <summary>
        /// <see langword="true"/> if all data types should be extended; <see langword="false"/> 
        /// if specified data types should be extended.
        /// </summary>
        readonly bool _extendAllTypes;

        /// <summary>
        /// Specific data types that should be extended. Ignored if <see cref="_extendAllTypes"/> 
        /// is <see langword="true"/>.
        /// </summary>
        readonly string[] _dataTypes;

        /// <summary>
        /// <see langword="true"/> if context words should be redacted; <see langword="false"/> 
        /// if context words should not be redacted.
        /// </summary>
        readonly bool _redactWords;

        /// <summary>
        /// The maximum number of context words to redact. Ignored if <see cref="_redactWords"/> 
        /// is <see langword="false"/>.
        /// </summary>
        readonly int _maxWords;

        /// <summary>
        /// <see langword="true"/> if the height of the redactions should be extended; 
        /// <see langword="false"/> if the height of the redactions should not be extended.
        /// </summary>
        readonly bool _extendHeight;

        /// <summary>
        /// The vector of attributes (VOA) file to modify.
        /// </summary>
        readonly string _dataFile;
		
        #endregion Fields

        #region Constructors
	
        /// <overloads>Initializes a new instance of the <see cref="SurroundContextSettings"/> 
        /// class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="SurroundContextSettings"/> class with 
        /// default settings.
        /// </summary>
        public SurroundContextSettings() 
            : this(true, null, true, 2, true, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurroundContextSettings"/>.
        /// </summary>
        /// <param name="extendAllTypes"><see langword="true"/> if all data types should be 
        /// extended; <see langword="false"/> if specified data types should be extended.</param>
        /// <param name="dataTypes">Specific data types that should be extended. Ignored if 
        /// <paramref name="extendAllTypes"/> is <see langword="true"/>.</param>
        /// <param name="redactWords"><see langword="true"/> if context words should be redacted; 
        /// <see langword="false"/> if context words should not be redacted.</param>
        /// <param name="maxWords">The maximum number of context words to redact. Ignored if 
        /// <paramref name="redactWords"/> is <see langword="false"/></param>
        /// <param name="extendHeight"><see langword="true"/> if the height of the redactions 
        /// should be extended; <see langword="false"/> if the height of the redactions should not 
        /// be extended.</param>
        /// <param name="dataFile">The vector of attributes (VOA) file to modify.</param>
        public SurroundContextSettings(bool extendAllTypes, string[] dataTypes, bool redactWords, 
            int maxWords, bool extendHeight, string dataFile)
        {
            _extendAllTypes = extendAllTypes;
            _dataTypes = dataTypes ?? new string[0];
            _redactWords = redactWords;
            _maxWords = maxWords;
            _extendHeight = extendHeight;
            _dataFile = dataFile ?? @"<SourceDocName>.voa";
        }
		
        #endregion Constructors
        
        #region Properties
	
        /// <summary>
        /// Gets whether all data types should be extended.
        /// </summary>
        /// <value><see langword="true"/> if all data types should be extended; 
        /// <see langword="false"/> if specified data types should be extended.</value>
        public bool ExtendAllTypes
        {
            get
            {
                return _extendAllTypes;
            }
        }
		
        /// <summary>
        /// Gets whether context words should be redacted.
        /// </summary>
        /// <value><see langword="true"/> if context words should be redacted; 
        /// <see langword="false"/> if context words should not be redacted.</value>
        public bool RedactWords
        {
            get
            {
                return _redactWords;
            }
        }

        /// <summary>
        /// Gets the maximum number of context words to redact. Ignored if 
        /// <see cref="RedactWords"/> is <see langword="false"/>.
        /// </summary>
        /// <value>The maximum number of context words to redact.</value>
        public int MaxWords
        {
            get
            {
                return _maxWords;
            }
        }

        /// <summary>
        /// Gets whether the height of redactions should be extended.
        /// </summary>
        /// <value><see langword="true"/> if the height of the redactions should be extended;
        /// <see langword="false"/> if the height of the redactions should not be extended.</value>
        public bool ExtendHeight
        {
            get
            {
                return _extendHeight;
            }
        }

        /// <summary>
        /// Gets the vector of attributes (VOA) file to modify.
        /// </summary>
        /// <value>The vector of attributes (VOA) file to modify.</value>
        public string DataFile
        {
            get
            {
                return _dataFile;
            }
        }
		
        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the specific data types that should be extended. Ignored if 
        /// <see cref="ExtendAllTypes"/> is <see langword="true"/>.
        /// </summary>
        /// <value>The specific data types that should be extended.</value>
        public string[] GetDataTypes()
        {
            return (string[])_dataTypes.Clone();
        }

        /// <summary>
        /// Determines whether the specified type should be extended.
        /// </summary>
        /// <param name="type">The data type to test.</param>
        /// <returns><see langword="true"/> if the specified type should be extended;
        /// <see langword="false"/> if the specified type should not be extended.</returns>
        public bool IsTypeToExtend(string type)
        {
            try
            {
                if (_extendAllTypes)
                {
                    return true;
                }

                foreach (string dataType in _dataTypes)
                {
                    if (dataType.Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29624", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="SurroundContextSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="SurroundContextSettings"/>.</param>
        /// <returns>A <see cref="SurroundContextSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        internal static SurroundContextSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                bool extendAllTypes = reader.ReadBoolean();
                string[] dataTypes = reader.ReadStringArray();
                bool redactWords = reader.ReadBoolean();
                int maxWords = reader.ReadInt32();
                bool extendHeight = reader.ReadBoolean();
                string dataFile = null;

                if (reader.Version > 1)
                {
                    dataFile = reader.ReadString();
                }

                return new SurroundContextSettings(extendAllTypes, dataTypes, redactWords, 
                    maxWords, extendHeight, dataFile);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29518",
                    "Unable to read surround context settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="SurroundContextSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="SurroundContextSettings"/> will be written.</param>
        internal void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_extendAllTypes);
                writer.Write(_dataTypes);
                writer.Write(_redactWords);
                writer.Write(_maxWords);
                writer.Write(_extendHeight);
                writer.Write(_dataFile);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29519",
                    "Unable to write surround context settings.", ex);
            }
        }

        #endregion Methods
    }
}
