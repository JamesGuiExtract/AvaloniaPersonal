using Extract.Encryption;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Rules
{
    /// <summary>
    /// Represents path information for the 
    /// </summary>
    public class DataType
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataType).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The full path of the data rule or <see langword="null"/> if there is no data rule for 
        /// this data type.
        /// </summary>
        readonly string _dataFile;

        /// <summary>
        /// The regular expression associated with the data rule.
        /// </summary>
        Regex _dataRegex;

        /// <summary>
        /// The full path of the clues rule or <see langword="null"/> if there is no clues rule 
        /// for this data type.
        /// </summary>
        readonly string _cluesFile;

        /// <summary>
        /// The regular expression associated with the clues rule.
        /// </summary>
        Regex _cluesRegex;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        /// <param name="dataFile">The full path of the data rule or <see langword="null"/> if 
        /// there is no data file for this data type.</param>
        /// <param name="cluesFile">The full path of the clues rule or <see langword="null"/> if 
        /// there is no clues file for this data type.</param>
        public DataType(string dataFile, string cluesFile)
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23191",
                _OBJECT_NAME);
                
            _dataFile = dataFile;
            _cluesFile = cluesFile;
        }

        #endregion Constructors
        
        #region Properties

        /// <summary>
        /// Gets or sets the full path of the data rule.
        /// </summary>
        /// <returns>The full path of the data rule or <see langword="null"/> if there is no data 
        /// rule for this data type.</returns>
        public string DataFile
        {
            get
            {
                return _dataFile;
            }
        }

        /// <summary>
        /// Gets or sets the full path of the clues rule.
        /// </summary>
        /// <returns>The full path of the clues rule or <see langword="null"/> if there is no 
        /// clues rule for this data type.</returns>
        public string CluesFile
        {
            get
            {
                return _cluesFile;
            }
        }

        /// <summary>
        /// Gets the regular expression associated with the data rule.
        /// </summary>
        /// <returns>The regular expression associated with the data rule or 
        /// <see langword="null"/> if no data rule exists.</returns>
        public Regex DataRegex
        {
            get
            {
                // Create the regular expression if not already created
                if (_dataRegex == null && _dataFile != null)
                {
                    string regex = ExtractEncryption.DecryptTextFile(_dataFile, Encoding.Default, 
                        new MapLabel());
                    _dataRegex = new Regex(regex, RegexOptions.ExplicitCapture);
                }

                return _dataRegex;
            }
        }

        /// <summary>
        /// Gets the regular expression associated with the clues rule.
        /// </summary>
        /// <returns>The regular expression associated with the clues rule or 
        /// <see langword="null"/> if no clues rule exists.</returns>
        public Regex CluesRegex
        {
            get
            {
                // Create the regular expression if not already created
                if (_cluesRegex == null && _cluesFile != null)
                {
                    string regex = ExtractEncryption.DecryptTextFile(_cluesFile, Encoding.Default,
                        new MapLabel());
                    _cluesRegex = new Regex(regex);
                }

                return _cluesRegex;
            }
        }

	    #endregion Properties
    }

    /// <summary>
    /// A class that implements <see cref="IRule"/> that will search a
    /// a SpatialString for specified pre-defined data types.
    /// </summary>
    public class DataTypeRule : IRule, IDisposable
    {
        #region Constants

        /// <summary>
        /// The name for this rule (for use with specifying the rule that produced a particular
        /// match result).
        /// </summary>
        const string _RULE_NAME = "Data type rule";

        /// <summary>
        /// Placeholder for the parent directory of the IDShieldOffice data types rules directory.
        /// </summary>
        static readonly string _RULES_DIR =
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"..\Rules\DataTypes");

        /// <summary>
        /// The file name of the data file.
        /// </summary>
        const string _DATA_FILE = "Data.dat.ese";

        /// <summary>
        /// The file name of the clues file.
        /// </summary>
        const string _CLUES_FILE = "Clues.dat.ese";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataTypeRule).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="string"/> objects that specify what
        /// data types to search for when searching the SpatialString.
        /// </summary>
        readonly List<string> _dataTypeList = new List<string>();

        /// <summary>
        /// A <see cref="Dictionary{T,T}"/> that maps all possible data types to their associated 
        /// rules file.
        /// </summary>
        static Dictionary<string, DataType> _validDataTypes;

        /// <summary>
        /// The property page of the <see cref="DataTypeRule"/>.
        /// </summary>
        DataTypeRulePropertyPage _propertyPage;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="DataTypeRule"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="DataTypeRule"/> class.
        /// </summary>
        public DataTypeRule()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RedactionCoreObjects, "ELI23192",
                    _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23193", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="DataTypeRule"/> class with the specified data types.
        /// </summary>
        /// <param name="dataTypes">An <see cref="IEnumerable{T}"/> collection of
        /// <see cref="string"/> objects that contain the data types to search for.</param>
        public DataTypeRule(IEnumerable<string> dataTypes) : this()
        {
            try
            {
               _dataTypeList.AddRange(dataTypes);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22056", "Failed to initialize DataTypeRule!", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the data type list to be searched.
        /// </summary>
        /// <value>The <see cref="List{T}"/> of <see cref="string"/> objects that
        /// define the data types to be searched.</value>
        /// <returns>The <see cref="List{T}"/> of <see cref="string"/> objects that
        /// define the data types to be searched.</returns>
        public IList<string> DataTypeList
        {
            get
            {
                return _dataTypeList;
            }
        }

        /// <summary>
        /// Gets the list of all the possible data types for which to search.
        /// </summary>
        /// <value>The list of all the possible data types for which to search.</value>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public static Dictionary<string, DataType> ValidDataTypes
        {
            get
            {
                try
                {
                    if (_validDataTypes == null)
                    {
                        _validDataTypes = new Dictionary<string, DataType>();

                        // If the rules directory doesn't exist, log an error and halt
                        if (!Directory.Exists(_RULES_DIR))
                        {
                            ExtractException ee = new ExtractException("ELI22601",
                                "Data types rule directory not found.");
                            ee.AddDebugData("Rules directory", _RULES_DIR, false);
                            ee.Log();
                            return _validDataTypes;
                        }

                        // Iterate through each subdirectory
                        foreach (string directory in Directory.GetDirectories(_RULES_DIR))
                        {
                            // Get the data type name (the subdirectory's name)
                            string dataType = directory.Substring(_RULES_DIR.Length + 1);

                            // Get the data file
                            string dataFile = GetRulesFullPath(directory, _DATA_FILE);

                            // Get the clues file
                            string cluesFile = GetRulesFullPath(directory, _CLUES_FILE);

                            // If neither file exists, skip this directory
                            if (dataFile == null && cluesFile == null)
                            {
                                continue;
                            }

                            // Add the key value pair
                            DataType rulesFile = new DataType(dataFile, cluesFile);
                            _validDataTypes.Add(dataType, rulesFile);
                        }
                    }

                    return _validDataTypes;
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI22136",
                        "Unable to determine data type rules.", ex);
                }
            }
        }

        static string GetRulesFullPath(string directory, string rulesFileName)
        {
            // Get the full path of the rules file for this directory
            string fullPath = Path.Combine(directory, rulesFileName);

            // Return the full path or null
            return File.Exists(fullPath) ? fullPath : null;
        }

        #endregion Properties

        #region IRule Members

        /// <summary>
        /// Searches the specified SpatialString for the specified list of data types
        /// and returns a <see cref="List{T}"/> of <see cref="MatchResult"/> objects.
        /// </summary>
        /// <param name="ocrOutput">The SpatialString to be searched for matches.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="MatchResult"/> objects containing
        /// the found items in the SpatialString.</returns>
        [CLSCompliant(false)]
        public MatchResultCollection GetMatches(SpatialString ocrOutput)
        {
            try
            {
                // Iterate through all the selected data types
                MatchResultCollection result = new MatchResultCollection();
                foreach (string dataTypeName in _dataTypeList)
                {
                    // Get the data type 
                    DataType dataType = _validDataTypes[dataTypeName];

                    // Add the matches associated with this data type
                    if (dataType.DataFile != null)
                    {
                        result.AddRange(MatchResult.ComputeMatches(_RULE_NAME, dataType.DataRegex,
                            ocrOutput, MatchType.Match, false));
                    }
                    if (dataType.CluesFile != null)
                    {
                        result.AddRange(MatchResult.ComputeMatches(_RULE_NAME, dataType.CluesRegex,
                            ocrOutput, MatchType.Clue, false));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29228", ex);
            }
        }

        /// <summary>
        /// Indicates whether the rule uses clues or not.
        /// </summary>
        /// <returns><see langword="true"/> if the rule uses clues. <see langword="false"/>
        /// if the rule does not use clues.</returns>
        public bool UsesClues
        {
            get
            {
                foreach (string dataTypeName in _dataTypeList)
                {
                    // Get the data type 
                    DataType dataType = _validDataTypes[dataTypeName];

                    if (dataType.CluesFile != null)
                    {
                        // If this data type has a rule for clues, return true
                        return true;
                    }  
                }

                // None of the data types were found to use clues.
                return false;
            }
        }

        #endregion IRule Members

        #region IUserConfigurableComponent Members

        /// <summary>
        /// Gets or sets the <see cref="UserControl"/> that contains the configurable objects 
        /// property page.
        /// </summary>
        /// <return>The <see cref="UserControl"/> that contains the configurable objects 
        /// property page.</return>
        public UserControl PropertyPage
        {
            get
            {
                // Create the property page if not already created
                if (_propertyPage == null)
                {
                    _propertyPage = new DataTypeRulePropertyPage(this);
                }

                return _propertyPage;
            }
        }

        #endregion IUserConfigurableComponent Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DataTypeRule"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="DataTypeRule"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataTypeRule"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            // Dispose of managed resources
            if (disposing)
            {
                if (_propertyPage != null)
                {
                    _propertyPage.Dispose();
                }
            }

            // No unmanaged resources to release
        }

        #endregion
    }
}
