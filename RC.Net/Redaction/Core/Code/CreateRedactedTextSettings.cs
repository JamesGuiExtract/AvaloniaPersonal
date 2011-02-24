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
        All = 0,

        /// <summary>
        /// Letters, digits, and underscores
        /// </summary>
        Alphanumeric = 1
    }

    /// <summary>
    /// Provides extension methods for the <see cref="CharacterClass"/> enum.
    /// </summary>
    public static class CharacterClassExtensionMethods
    {
        /// <summary>
        /// Converts the specified <see cref="CharacterClass"/> value into a readable string.
        /// </summary>
        /// <param name="characterClass"></param>
        /// <returns>The specified <see cref="CharacterClass"/> as a readable string.</returns>
        public static string ToReadableString(this CharacterClass characterClass)
        {
            try
            {
                switch (characterClass)
                {
                    case CharacterClass.All:
                        return "all";
                    case CharacterClass.Alphanumeric:
                        return "alpha numeric";

                    default:
                        throw new ExtractException("ELI31694", "Unhandled character class");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31697");
            }
        }
    }

    /// <summary>
    /// Represents the methods in which the <see cref="CreateRedactedTextTask"/> class can redact text
    /// files.
    /// </summary>
    public enum RedactionMethod
    {
        /// <summary>
        /// Replaces each individual character in sensitive data with the specified character.
        /// </summary>
        ReplaceCharacters,

        /// <summary>
        /// Replaces each sensitive data item with the specified text.
        /// </summary>
        ReplaceText,

        /// <summary>
        /// Surrounds the sensitive data with the specified XML element.
        /// </summary>
        SurroundWithXml
    }

    #endregion Enums

    /// <summary>
    /// Represents the settings used for the create redacted text task.
    /// </summary>
    public class CreateRedactedTextSettings
    {
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
        {
            try
            {
                RedactAllTypes = false;
                RedactOtherTypes = false;
                DataTypes = new ReadOnlyCollection<string>(_standardDataTypes);
                RedactionMethod = RedactionMethod.ReplaceCharacters;
                CharactersToReplace = CharacterClass.All;
                ReplacementCharacter = "X";
                AddCharactersToRedaction = false;
                MaxNumberAddedCharacters = 5;
                ReplacementText = "[REDACTED]";
                XmlElementName = "Sensitive";
                DataFile = "<SourceDocName>.voa";
                OutputFileName = "$InsertBeforeExt(<SourceDocName>,.redacted)";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31695");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedTextSettings"/> class.
        /// </summary>
        /// <param name="redactAllTypes">Indicates whether all data types should be redacted.</param>
        /// <param name="redactOtherTypes">Indicates non-standard ID Shield data types should be
        /// redacted.</param>
        /// <param name="dataTypes">The data types referenced in the settings. NOTE: A data
        /// type in this list is not necessarily configured to be redacted. Only if the
        /// <see cref="IsTypeToRedact"/> returns <see langword="true"/> for a given type should it
        /// be redacted.</param>
        /// <param name="redactionMethod">The <see cref="RedactionMethod"/> that determines how
        /// sensitive data will be replaced or protected in the output text.</param>
        /// <param name="charactersToReplace">Specifies which <see cref="CharacterClass"/> should be
        /// replaced when redacting sensitive data. Ignored if <see paramref="redactionMethod"/> is
        /// not ReplaceCharacters.</param>
        /// <param name="replacementCharacter">Specifies the <see langword="string"/> that should
        /// replace characters in sensitive data. Ignored if <see cref="RedactionMethod"/> is not
        /// ReplaceCharacters.</param>
        /// <param name="addCharactersToRedaction">Specifies whether a random number of characters
        /// should be appended to the redacted text to obscure the number of characters in the
        /// sensitive data. Ignored if <see cref="RedactionMethod"/> is not ReplaceCharacters.
        /// </param>
        /// <param name="maxNumberAddedCharacters">Specifies the maxium number of characters to be
        /// appended to redacted text when <see paramref="addCharactersToRedaction"/> is
        /// <see langword="true"/>. Ignored if <see cref="RedactionMethod"/> is not
        /// ReplaceCharacters.</param>
        /// <param name="replacementText">The <see cref="string"/> that should replace any discrete
        /// sensitive data item. Ignored if <see cref="RedactionMethod"/> is not ReplaceText.</param>
        /// <param name="xmlElementName">Specifies the name of XML element which should be used to
        /// surround sensitive data. data. Ignored if <see cref="RedactionMethod"/> is not
        /// SurroundWithXml.</param>
        /// <param name="dataFile">The vector of attributes (VOA) file containing the data to be
        /// replaced in the source document.</param>
        /// <param name="outputFileName">The filename to which the redacted text is to be written.
        /// </param>
        public CreateRedactedTextSettings(bool redactAllTypes, bool redactOtherTypes,
            string[] dataTypes, RedactionMethod redactionMethod, CharacterClass charactersToReplace,
            string replacementCharacter, bool addCharactersToRedaction, 
            int maxNumberAddedCharacters, string replacementText, string xmlElementName,
            string dataFile, string outputFileName)
        {
            try
            {
                RedactAllTypes = redactAllTypes;
                RedactOtherTypes = redactOtherTypes;
                DataTypes = new ReadOnlyCollection<string>(dataTypes);
                RedactionMethod = redactionMethod;
                CharactersToReplace = charactersToReplace;
                ReplacementCharacter = replacementCharacter;
                AddCharactersToRedaction = addCharactersToRedaction;
                MaxNumberAddedCharacters = maxNumberAddedCharacters;
                ReplacementText = replacementText;
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
        /// <value>A <see cref="ReadOnlyCollection{T}"/> of <see langword="string"/>s populated
        /// with the data types that are "standard" rule output.</value>
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
        /// Gets the <see cref="RedactionMethod"/> that determines how sensitive data will be
        /// replaced or protected in the output text.
        /// </summary>
        /// <value>The <see cref="RedactionMethod"/>.</value>
        public RedactionMethod RedactionMethod
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets which <see cref="CharacterClass"/> should be replaced when redacting sensitive
        /// data. Ignored if <see cref="RedactionMethod"/> is not ReplaceCharacters.
        /// </summary>
        /// <value>The <see cref="CharacterClass"/> that should be replaced.</value>
        public CharacterClass CharactersToReplace
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see langword="string"/> that should replace characters in sensitive data.
        /// Ignored if <see cref="RedactionMethod"/> is not ReplaceCharacters.
        /// </summary>
        /// <value>The <see langword="string"/> that should replace characters in sensitive data.
        /// </value>
        public string ReplacementCharacter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether a random number of characters should be appended to the redacted text to
        /// obscure the number of characters in the sensitive data. Ignored if
        /// <see cref="RedactionMethod"/> is not ReplaceCharacters.
        /// </summary>
        /// <value><see langword="true"/> if a random number of characters should be appended to the
        /// redacted text; otherwise, <see langword="false"/>.</value>
        public bool AddCharactersToRedaction
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maxium number of characters to be appended to redacted text when
        /// <see paramref="AddCharactersToRedaction"/>  is <see langword="true"/>. Ignored if
        /// <see cref="RedactionMethod"/> is not ReplaceCharacters.
        /// </summary>
        /// <value>The maxium number of characters to be appended.</value>
        public int MaxNumberAddedCharacters
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="string"/> that should replace any discrete
        /// sensitive data item. Ignored if <see cref="RedactionMethod"/> is not ReplaceText.
        /// </summary>
        /// <value>The <see cref="string"/> that should replace any discrete sensitive data item.
        /// </value>
        public string ReplacementText
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a the name of XML element which should be used to surround sensitive data.
        /// data. Ignored if <see cref="RedactionMethod"/> is not SurroundWithXml.
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

                CreateRedactedTextSettings settings = new CreateRedactedTextSettings();

                settings.RedactAllTypes = reader.ReadBoolean();
                settings.RedactOtherTypes = reader.ReadBoolean();
                settings.DataTypes = new ReadOnlyCollection<string>(reader.ReadStringArray());

                if (reader.Version < 2)
                {
                    // Convert old "ReplaceCharacters" setting to a RedactionMethod setting.
                    settings.RedactionMethod = reader.ReadBoolean()
                        ? RedactionMethod.ReplaceCharacters : RedactionMethod.SurroundWithXml;
                }
                else
                {
                    settings.RedactionMethod = (RedactionMethod)reader.ReadInt32();
                    settings.AddCharactersToRedaction = reader.ReadBoolean();
                    settings.MaxNumberAddedCharacters = reader.ReadInt32();
                    settings.ReplacementText = reader.ReadString();
                }

                settings.CharactersToReplace = (CharacterClass)reader.ReadInt32();
                settings.ReplacementCharacter = reader.ReadString();
                settings.XmlElementName = reader.ReadString();
                settings.DataFile = reader.ReadString();
                settings.OutputFileName = reader.ReadString();

                return settings;
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
                writer.Write((int)RedactionMethod);
                writer.Write(AddCharactersToRedaction);
                writer.Write(MaxNumberAddedCharacters);
                writer.Write(ReplacementText);
                writer.Write((int)CharactersToReplace);
                writer.Write(ReplacementCharacter);
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
