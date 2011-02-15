using Extract.Interop;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Extract.Redaction
{
    #region Enums

    /// <summary>
    /// Represents a class of characters. Used by <see cref="CreateRedactedTextSettings"/> to
    /// determine which characters in sensitive data to replace.
    /// </summary>
    public enum CharacterClass
    {
        /// <summary>
        /// All characters
        /// </summary>
        All,

        /// <summary>
        /// Letters, digits, and underscores
        /// </summary>
        Alphanumeric
    }

    #endregion Enums

    /// <summary>
    /// Represents the settings used for the create redacted text task.
    /// </summary>
    public class CreateRedactedTextSettings
    {
        #region Constants

        /// <summary>
        /// The default XML element to use when surrounding sensitive text.
        /// </summary>
        static readonly string _DEFAULT_REPLACEMENT_VALUE = "X";

        /// <summary>
        /// The default XML element to use when surrounding sensitive text.
        /// </summary>
        static readonly string _DEFAULT_XML_ELEMENT =  "Sensitive";

        /// <summary>
        /// The default location of the data file.
        /// </summary>
        static readonly string _DEFAULT_DATA_FILE =  "<SourceDocName>.voa";

        /// <summary>
        /// The default output filename.
        /// </summary>
        static readonly string _DEFAULT_OUTPUT_FILE = "$InsertBeforeExt(<SourceDocName>,.redacted)";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Data types that are "standard" rule output and will be specified via checkboxes (as
        /// opposed to the "Other" data type box in the configuration dialog).
        /// </summary>
        static ReadOnlyCollection<string> _standardDataTypes = new ReadOnlyCollection<string>(
            new string[]
            {
                Constants.HCData,
                Constants.MCData,
                Constants.LCData,
                Constants.Manual
            });

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedTextSettings"/> class.
        /// </summary>
        public CreateRedactedTextSettings()
            : this(true, false, new string[] { }, true, CharacterClass.All,
                _DEFAULT_REPLACEMENT_VALUE, _DEFAULT_XML_ELEMENT, _DEFAULT_DATA_FILE,
                _DEFAULT_OUTPUT_FILE)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedTextSettings"/> class.
        /// </summary>
        /// <param name="redactAllTypes">Indicates whether all data types should be redacted.</param>
        /// <param name="redactOtherTypes">Indicates non-standard ID Shield data types should be
        /// redacted.</param>
        /// <param name="dataTypes">Gets all data types referenced in the settings. NOTE: A data
        /// type in this list is not necessarily configured to be redacted. Only if the
        /// <see cref="IsTypeToRedact"/> returns <see langword="true"/> for a given type should it
        /// be redacted.</param>
        /// <param name="replaceCharacters">Indicates whether the sensitive text will be redacted
        /// by replacing characters in the sensitive data.</param>
        /// <param name="charactersToReplace">Indicates which <see cref="CharacterClass"/> should be
        /// replaced when redacting sensitive data. (Ignored if <see cref="ReplaceCharacters"/> is
        /// <see langword="false"/>)</param>
        /// <param name="replacementValue">Specifies the <see langword="string"/> that should
        /// replace characters when redacting sensitive data. (Ignored if
        /// <see cref="ReplaceCharacters"/> is <see langword="false"/>)</param>
        /// <param name="xmlElementName">Specifies the name of XML element which should be used to
        /// surround sensitive data. data. Ignored if <see cref="ReplaceCharacters"/> is
        /// <see langword="true"/></param>
        /// <param name="dataFile">The vector of attributes (VOA) file containing the data to be
        /// replaced in the source document.</param>
        /// <param name="outputFileName">The filename to which the redacted text is to be written.
        /// </param>
        public CreateRedactedTextSettings(bool redactAllTypes, bool redactOtherTypes,
            string[] dataTypes, bool replaceCharacters, CharacterClass charactersToReplace,
            string replacementValue, string xmlElementName, string dataFile, string outputFileName)
        {
            try
            {
                RedactAllTypes = redactAllTypes;
                RedactOtherTypes = redactOtherTypes;
                DataTypes = new ReadOnlyCollection<string>(dataTypes);
                ReplaceCharacters = replaceCharacters;
                CharactersToReplace = charactersToReplace;
                ReplacementValue = replacementValue;
                XmlElementName = xmlElementName;
                DataFile = dataFile;
                OutputFileName = outputFileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31651");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets data types that are "standard" rule output and will be specified via checkboxes
        /// (as opposed to the "Other" data type box in the configuration dialog).
        /// </summary>
        public static ReadOnlyCollection<string> StandardDataTypes
        {
            get
            {
                return _standardDataTypes;
            }
        }

        /// <summary>
        /// Gets whether all data types should be redacted.
        /// </summary>
        /// <value><see langword="true"/> if all data types should be redacted; 
        /// <see langword="false"/> if specified data types should be redacted.</value>
        public bool RedactAllTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether non-standard ID Shield data types should be redacted.
        /// </summary>
        /// <value><see langword="true"/> if  non-standard data types may be redacted; 
        /// <see langword="false"/> otherwise.</value>
        public bool RedactOtherTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all data types referenced in the settings.
        /// <para><b>NOTE</b></para>
        /// A data type in this list is not necessarily configured to be redacted. Only if the
        /// <see cref="IsTypeToRedact"/> returns <see langword="true"/> for a given type should it
        /// be redacted.
        /// </summary>
        /// <value>The specific data types that should be redacted.</value>
        public ReadOnlyCollection<string> DataTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the sensitive text will be redacted by replacing characters in the
        /// sensitive data.
        /// </summary>
        /// <value><see langword="true"/> if characters in the sensitive 
        /// <see langword="false"/> if specified text should be redacted.</value>
        public bool ReplaceCharacters
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets which <see cref="CharacterClass"/> should be replaced when redacting sensitive
        /// data. (Ignored if <see cref="ReplaceCharacters"/> is <see langword="false"/>)
        /// </summary>
        public CharacterClass CharactersToReplace
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see langword="string"/> that should replace characters when redacting
        /// sensitive data. (Ignored if <see cref="ReplaceCharacters"/> is <see langword="false"/>)
        /// </summary>
        public string ReplacementValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a the name of XML element which should be used to surround sensitive data.
        /// data. Ignored if <see cref="ReplaceCharacters"/> is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// The name of XML element which should be used to surround sensitive data.
        /// </value>
        public string XmlElementName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the vector of attributes (VOA) file containing the data to be replaced in the
        /// source document.
        /// </summary>
        /// <value>The vector of attributes (VOA) file containing the data to be replaced in the
        /// source document.</value>
        public string DataFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the filename to which the redacted text is to be written.
        /// </summary>
        /// <value>The filename to which the redacted text is to be written.</value>
        public string OutputFileName
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether the text of the specified type should be redacted.
        /// </summary>
        /// <param name="type">The data type to test.</param>
        /// <returns><see langword="true"/> if the specified type should be redacted;
        /// <see langword="false"/> if the specified type should not be redacted.</returns>
        public bool IsTypeToRedact(string type)
        {
            try
            {
                if (RedactAllTypes)
                {
                    return true;
                }
                // If not redacting all types, check to see whether the specified type has been
                // configured.
                else if (DataTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                {
                    // If it has been configured, check that either the type is a standard type or
                    // that other types have been configured to be redacted.
                    return (RedactOtherTypes ||
                            StandardDataTypes.Contains(type, StringComparer.OrdinalIgnoreCase));
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31654", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="CreateRedactedTextSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="CreateRedactedTextSettings"/>.</param>
        /// <returns>A <see cref="CreateRedactedTextSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        public static CreateRedactedTextSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                ExtractException.Assert("ELI31630", "Unable to load newer " + 
                    CreateRedactedTextTask._COMPONENT_DESCRIPTION + ".",
                    reader.Version <= CreateRedactedTextTask._CURRENT_VERSION);

                bool replaceAllTypes = reader.ReadBoolean();
                bool redactOtherTypes = reader.ReadBoolean();
                string[] dataTypes = reader.ReadStringArray();
                bool replaceCharacters = reader.ReadBoolean();
                CharacterClass charactersToReplace = (CharacterClass)reader.ReadInt32();
                string replacementValue = reader.ReadString();
                string xmlElementName = reader.ReadString();
                string dataFile = reader.ReadString();
                string outputFileName = reader.ReadString();

                return new CreateRedactedTextSettings(replaceAllTypes, redactOtherTypes, dataTypes,
                    replaceCharacters, charactersToReplace, replacementValue, xmlElementName,
                    dataFile, outputFileName);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI31631",
                    "Unable to read replace indexed text settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="CreateRedactedTextSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="CreateRedactedTextSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(RedactAllTypes);
                writer.Write(RedactOtherTypes);
                writer.Write(DataTypes.ToArray());
                writer.Write(ReplaceCharacters);
                writer.Write((int)CharactersToReplace);
                writer.Write(ReplacementValue);
                writer.Write(XmlElementName);
                writer.Write(DataFile);
                writer.Write(OutputFileName);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI31632",
                    "Unable to write create redacted text settings.", ex);
            }
        }

        #endregion Methods
    }
}
