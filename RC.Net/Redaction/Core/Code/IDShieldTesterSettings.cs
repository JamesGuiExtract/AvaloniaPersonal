using Extract.Interop;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Extract.Redaction
{
    /// <summary>
    /// A parameter used by ID Shield Tester to execute a test. Each parameter is specified as a
    /// separate line in a settings file (typically with a .dat file extension).
    /// </summary>
    abstract public class IDShieldTesterParameter
    {
        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(IDShieldTesterParameter).ToString();

        /// <summary>
        /// The name of the current parameter type.
        /// </summary>
        string _type;

        /// <summary>
        /// The names of the columns available for this parameter.
        /// </summary>
        string[] _columnNames;

        /// <summary>
        /// Indicates whether this parameter instance has been populated (either via settings file
        /// or via direct population of a column value).
        /// </summary>
        bool _populated;

        /// <summary>
        /// Maps all column names assigned a value to its value.
        /// </summary>
        Dictionary<string, string> _columns = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new <see cref="IDShieldTesterParameter"/> instance.
        /// </summary>
        /// <param name="type">The name of the parameter type.</param>
        /// <param name="columnNames">The names of the columns available for this parameter.</param>
        protected IDShieldTesterParameter(string type, string[] columnNames)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI28632", _OBJECT_NAME);

                _type = type;
                _columnNames = columnNames;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28633", ex);
            }
        }

        /// <summary>
        /// Gets or sets whether this parameter instance has been populated (either via settings
        /// file or via direct population of a column value).
        /// </summary>
        /// <value><see langword="true"/> if this parameter has been populated,
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if this parameter has been populated,
        /// <see langword="false"/> otherwise.</returns>
        public bool Populated
        {
            get
            {
                return _populated;
            }

            set
            {
                _populated = value;
            }
        }

        /// <summary>
        /// Populates the parameter list using the specified file.
        /// </summary>
        /// <param name="settingsFileName">The file from which the parameter values should be read.
        /// </param>
        /// <param name="parameters">The parameters to be populated from the file.
        /// <para><b>Note:</b></para>
        /// Parameters may be added to the parameter list if repeatable parameter types are read
        /// from the file. Only parameters whose <see cref="Populated"/> value is
        /// <see langword="true"/> should be used.</param>
        public static void ReadFromFile(string settingsFileName,
            Collection<IDShieldTesterParameter> parameters)
        {
            try
            {
                ExtractException.Assert("ELI28634",
                    "Invalid or missing ID Shield settings filename!",
                    File.Exists(settingsFileName));

                string[] lines = File.ReadAllLines(settingsFileName);
                foreach (string line in lines)
                {
                    int count = parameters.Count;
                    for (int i = 0; i < count; i++)
                    {
                        IDShieldTesterParameter parameter = parameters[i];

                        if (parameter.ReadFromString(line))
                        {
                            IRepeatableDShieldTesterParameter repeatableParameter =
                                parameter as IRepeatableDShieldTesterParameter;

                            if (repeatableParameter != null)
                            {
                                parameters.Add(repeatableParameter.CreateNewInstance());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28635",
                    "Failure when reading ID Shield Tester settings file!", ex);
                ee.AddDebugData("Filename", settingsFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes the specifed parameter list to the specified file.
        /// </summary>
        /// <param name="settingsFileName">The name of the file to be written.
        /// <para><b>Note:</b></para>
        /// Any existing file by this name will be overwritten.
        /// </param>
        /// <param name="parameters">The list of parameters to be written to file.
        /// <para><b>Note:</b></para>
        /// Only parameters whose <see cref="Populated"/> value is <see langword="true"/> will be
        /// written to file.</param>
        public static void WriteToFile(string settingsFileName,
            Collection<IDShieldTesterParameter> parameters)
        {
            try
            {
                List<string> outputLines = new List<string>();

                foreach (IDShieldTesterParameter parameter in parameters)
                {
                    if (parameter.Populated)
                    {
                        string outputLine = parameter.WriteToString();
                        if (!string.IsNullOrEmpty(outputLine))
                        {
                            outputLines.Add(outputLine);
                        }
                    }
                }

                File.WriteAllLines(settingsFileName, outputLines.ToArray());
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28636",
                    "Failed to write ID Shield Tester settings file!", ex);
                ee.AddDebugData("Filename", settingsFileName, false);
            }
        }

        /// <summary>
        /// Gets the <see langword="string"/> representation of the value from the specified
        /// parameter column.
        /// </summary>
        /// <param name="columnName">The name of the column to get.</param>
        /// <returns>The <see langword="string"/> value from the specified column.</returns>
        protected virtual string GetValue(string columnName)
        {
            try
            {
                string value = null;
                _columns.TryGetValue(columnName, out value);
                return value;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28637", ex);
            }
        }

        /// <summary>
        /// Sets the <see langword="string"/> representation of the value of the specified
        /// parameter column.
        /// </summary>
        /// <param name="columnName">The name of the column to set.</param>
        /// <param name="columnValue">The <see langword="string"/> value to apply to the specified
        /// column.</param>
        protected virtual void SetValue(string columnName, string columnValue)
        {
            try
            {
                _columns[columnName] = columnValue;
                _populated = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28638", ex);
            }
        }

        /// <summary>
        /// Writes the parameter to a string which will become a line of an IDShieldTester settings
        /// file.
        /// </summary>
        /// <returns>A string representing the parameter.</returns>
        protected virtual string WriteToString()
        {
            try
            {
                StringBuilder outputText = new StringBuilder();

                outputText.Append(_type);
                foreach (string columnName in _columnNames)
                {
                    outputText.Append(";" + GetValue(columnName));
                }

                return outputText.ToString();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28639", ex);
                ee.AddDebugData("Parameter type", _type, false);
                throw ee;
            }
        }

        /// <summary>
        /// Attempts to reads the parameter from a string which represents one line of an
        /// IDShieldTester settings file.
        /// </summary>
        /// <param name="parameterLine">The line to attempt to read into this parameter.</param>
        /// <returns><see langword="true"/> if the line was successfully read into the parameter,
        /// <see langword="false"/> if the line does not represent the current parameter type.
        /// </returns>
        protected virtual bool ReadFromString(string parameterLine)
        {
            try
            {
                if (parameterLine.StartsWith(_type, StringComparison.OrdinalIgnoreCase))
                {
                    string[] values = parameterLine.Split(';');
                    ExtractException.Assert("ELI28640", "Too many parameter values specified!",
                        values.Length <= _columnNames.Length + 1);

                    for (int i = 1; i < values.Length; i++)
                    {
                        SetValue(_columnNames[i - 1], values[i]);
                    }

                    _populated = true;

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28641", ex);
                ee.AddDebugData("Line", parameterLine, false);
                throw ee;
            }
        }
    }

    /// <summary>
    /// Implemented by any <see cref="IDShieldTesterParameter"/> where multiple instances of the
    /// parameter are allowed in the same file.
    /// </summary>
    public interface IRepeatableDShieldTesterParameter
    {
        /// <summary>
        /// Creates a new blank instance of the <see cref="IDShieldTesterParameter"/>.
        /// </summary>
        /// <returns>A new blank instance of the <see cref="IDShieldTesterParameter"/></returns>
        IDShieldTesterParameter CreateNewInstance();
    }

    /// <summary>
    /// A setting defining how ID Shield Tester is to execute a test.
    /// </summary>
    class IDShieldTesterSetting<T> : IDShieldTesterParameter
    {
        #region Fields

        // The one and only column for a setting parameter:
        // a mapping of the setting name to setting value
        const string _COLUMN_NAME = "SettingNameValue";

        // The name of the setting;
        string _settingName;

        // The current value of the setting.
        T _value;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new IDShieldTesterSetting instance.
        /// </summary>
        /// <param name="settingName">The name of the setting.</param>
        public IDShieldTesterSetting(string settingName)
            : base("<SETTING>", new string[] { _COLUMN_NAME })
        {
            _settingName = settingName;
        }

        #endregion Constructors

        #region Properties
        
        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <value>The value of the setting.</value>
        public T Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                Populated = true;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Attempts to read the setting from a string which represents one line of an
        /// IDShieldTester settings file.
        /// </summary>
        /// <param name="parameterLine">The line to attempt to read into this setting.</param>
        /// <returns><see langword="true"/> if the line was successfully read into the setting,
        /// <see langword="false"/> if the line does not represent the current setting.
        /// </returns>
        protected override bool ReadFromString(string parameterLine)
        {
            try
            {
                if (base.ReadFromString(parameterLine))
                {
                    string value = GetValue();
                    if (value != null)
                    {
                        if (value.Length > 0)
                        {
                            _value =
                                (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value);
                        }

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28642", ex);
                ee.AddDebugData("Line", parameterLine, false);
                ee.AddDebugData("Setting name", _settingName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes the setting to a string which will become a line of an IDShieldTester settings
        /// file.
        /// </summary>
        /// <returns>A string representing the setting.</returns>
        protected override string WriteToString()
        {
            try
            {
                if (Populated && _value != null)
                {
                    SetValue();

                    return base.WriteToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28643", ex);
                ee.AddDebugData("Setting name", _settingName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the <see langword="string"/> representation of the setting's value.
        /// </summary>
        /// <returns>The setting's <see langword="string"/> value.</returns>
        string GetValue()
        {
            try
            {
                string[] valueArray = GetValue(_COLUMN_NAME).Split(new char[] { '=' }, 2);
                string settingName = (valueArray.Length == 0) ? "" : valueArray[0].Trim();
                if (settingName.Equals(_settingName, StringComparison.OrdinalIgnoreCase))
                {
                    string value = (valueArray.Length < 2) ? "" : valueArray[1];

                    if (typeof(T) == typeof(bool))
                    {
                        ExtractException.Assert("ELI28644",
                            "Bool value must be specified as either '0' or '1'",
                            value == "0" || value == "1");

                        value = (value == "0") ? false.ToString() : true.ToString();
                    }

                    return value;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28645", ex);
            }
        }

        /// <summary>
        /// Sets the <see langword="string"/> representation of the setting's value.
        /// </summary>
        /// <value>The <see langword="string"/> value to apply to the setting.</value>
        void SetValue()
        {
            string value = (_value == null) ? "" : _value.ToString();
            if (typeof(T) == typeof(bool))
            {
                value = (value == true.ToString()) ? "1" : "0";
            }

            SetValue(_COLUMN_NAME, _settingName + "=" + value);
        }

        #endregion Methods
    }

    /// <summary>
    /// Specifies a folder whose contents should be tested by ID Shield Tester.
    /// </summary>
    public class IDShieldTesterFolder : IDShieldTesterParameter, IRepeatableDShieldTesterParameter
    {
        #region Fields

        /// <summary>
        /// The parameter column containing the name of the rules file to use.
        /// </summary>
        const string _RULES_FILE_COLUMN = "RulesFile";

        /// <summary>
        /// The parameter column containing the name of the folder to test.
        /// </summary>
        const string _TEST_FOLDER_COLUMN = "TestFolderName";

        /// <summary>
        /// The parameter column specifying the where to find the found data.
        /// </summary>
        const string _FOUND_DATA_COLUMN = "FoundData";

        /// <summary>
        /// The parameter column specifying the where to find the expected data.
        /// </summary>
        const string _EXPECTED_DATA_COLUMN = "ExpectedData";

        /// <summary>
        /// The collection of parameter columns used by IDShieldTesterFolder.
        /// </summary>
        static readonly string[] _COLUMNS_NAMES = new string[] { _RULES_FILE_COLUMN,
                _TEST_FOLDER_COLUMN, _FOUND_DATA_COLUMN, _EXPECTED_DATA_COLUMN};

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialized a new <see cref="IDShieldTesterFolder"/>
        /// </summary>
        public IDShieldTesterFolder()
            : base("<TESTFOLDER>", _COLUMNS_NAMES)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the rules file to use.
        /// </summary>
        /// <value>The name of the rules file to use.</value>
        public string RulesFile
        {
            get
            {
                return GetValue(_RULES_FILE_COLUMN);
            }

            set
            {
                SetValue(_RULES_FILE_COLUMN, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the folder to test.
        /// </summary>
        /// <value>The name of the folder to test</value>
        public string TestFolderName
        {
            get
            {
                return GetValue(_TEST_FOLDER_COLUMN);
            }

            set
            {
                SetValue(_TEST_FOLDER_COLUMN, value);
            }
        }

        /// <summary>
        /// Gets or sets the location of the found data.
        /// </summary>
        /// <value>The location of the found data.</value>
        public string FoundDataLocation
        {
            get
            {
                return GetValue(_FOUND_DATA_COLUMN);
            }

            set
            {
                SetValue(_FOUND_DATA_COLUMN, value);
            }
        }

        /// <summary>
        /// Gets or sets the location of the expected data.
        /// </summary>
        /// <value>The location of the expected data.</value>
        public string ExpectedDataLocation
        {
            get
            {
                return GetValue(_EXPECTED_DATA_COLUMN);
            }

            set
            {
                SetValue(_EXPECTED_DATA_COLUMN, value);
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Writes the folder parameter to a string which will become a line of an IDShieldTester
        /// settings file.
        /// </summary>
        /// <returns>A string representing the test folder info.</returns>
        protected override string WriteToString()
        {
            try
            {
                return "\r\n" + base.WriteToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28646", ex);
            }
        }

        /// <summary>
        /// Creates a new blank <see cref="IDShieldTesterFolder"/> instance.
        /// </summary>
        /// <returns>A new blank instance as a <see cref="IDShieldTesterParameter"/></returns>
        public IDShieldTesterParameter CreateNewInstance()
        {
            return new IDShieldTesterFolder();
        }

        #endregion Overrides
    }

    /// <summary>
    /// Represents the collection of parameters to use for running an ID Shield tester.
    /// </summary>
    public class IDShieldTesterSettings
    {
        #region Fields

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(IDShieldTesterSettings).ToString();

        /// <summary>
        /// The path to the IDShieldTester settings file (dat file).
        /// </summary>
        readonly string _settingsFileName;

        /// <summary>
        /// Specifies which documents would be verified in a standard workflow.
        /// </summary>
        IDShieldTesterSetting<string> _verificationCondition =
            new IDShieldTesterSetting<string>("VerificationCondition");

        /// <summary>
        /// Specifies how the VerificationCondition should be used in determining which documents
        /// would be verified in a standard workflow.
        /// </summary>
        IDShieldTesterSetting<string> _verificationConditionQuantifier =
                new IDShieldTesterSetting<string>("VerificationConditionQuantifier");

        /// <summary>
        /// Specifies which document types would be verified in a standard workflow.
        /// </summary>
        IDShieldTesterSetting<string> _docTypesToVerify =
            new IDShieldTesterSetting<string>("DocTypesToVerify");

        /// <summary>
        /// A query that specifies which attributes should be automatically redacted.
        /// </summary>
        IDShieldTesterSetting<string> _queryForAutomatedRedaction =
            new IDShieldTesterSetting<string>("QueryForAutomatedRedaction");

        /// <summary>
        /// Specifies which documents would be automatically redacted in an automated or hybrid 
        /// workflow.
        /// </summary>
        IDShieldTesterSetting<string> _automatedCondition =
            new IDShieldTesterSetting<string>("AutomatedCondition");

        /// <summary>
        /// Specifies how the AutomatedCondition should be used in determining which documents
        /// would be automatically redacted in an automated or hybrid workflow.
        /// </summary>
        IDShieldTesterSetting<string> _automatedConditionQuantifier =
            new IDShieldTesterSetting<string>("AutomatedConditionQuantifier");

        /// <summary>
        /// Specifies which document types would be automatically redacted in an automated or
        /// hybrid workflow.
        /// </summary>
        IDShieldTesterSetting<string> _docTypesToRedact =
            new IDShieldTesterSetting<string>("DocTypesToRedact");

        /// <summary>
        /// Specifies whether output files should be created.
        /// </summary>
        IDShieldTesterSetting<bool> _createTestOutputVoaFiles =
            new IDShieldTesterSetting<bool>("CreateTestOutputVOAFiles");

        /// <summary>
        /// Specifies whether to generate attribute name file lists.
        /// </summary>
        IDShieldTesterSetting<bool> _outputAttributeNamesFileLists =
            new IDShieldTesterSetting<bool>("OutputAttributeNamesFileLists");

        /// <summary>
        /// Specifies whether the test status should be updated as it progresses or whether
        /// the test should run without updating status and only output stats at the end.
        /// </summary>
        IDShieldTesterSetting<bool> _outputFinalStatsOnly =
            new IDShieldTesterSetting<bool>("OutputFinalStatsOnly");

        /// <summary>
        /// Specifies whether to generate hybrid stats.
        /// </summary>
        IDShieldTesterSetting<bool> _outputHybridStats =
            new IDShieldTesterSetting<bool>("OutputHybridStats");

        /// <summary>
        /// Specifies whether to output only automated stats.
        /// </summary>
        IDShieldTesterSetting<bool> _outputAutomatedStatsOnly =
            new IDShieldTesterSetting<bool>("OutputAutomatedStatsOnly");

        /// <summary>
        /// Specifies attribute types to which are to be included in the test.
        /// </summary>
        IDShieldTesterSetting<string> _typesToBeTested =
            new IDShieldTesterSetting<string>("TypesToBeTested");

        /// <summary>
        /// Specifies the folder where the test results will be output.
        /// </summary>
        IDShieldTesterSetting<string> _outputFilesFolder =
            new IDShieldTesterSetting<string>("OutputFilesFolder");

        /// <summary>
        /// The collection of possible parameters in an ID Shield settings (.dat) file.
        /// </summary>
        Collection<IDShieldTesterParameter> _parameters = new Collection<IDShieldTesterParameter>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="IDShieldTesterSettings"/> instance.
        /// </summary>
        public IDShieldTesterSettings()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="IDShieldTesterSettings"/> instance.
        /// </summary>
        /// <param name="settingsFileName">The path to the IDShieldTester settings file (dat file).
        /// </param>
        public IDShieldTesterSettings(string settingsFileName)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI28647", _OBJECT_NAME);

                // [FlexIDSCore:4048]
                // Initialize typically used defaults.
                _verificationCondition.Value = "HCData|MCData|LCData|Clues";
                _queryForAutomatedRedaction.Value = "HCData|MCData|LCData";

                _settingsFileName = settingsFileName;

                // Initialize the parameters.
                _parameters.Add(_verificationCondition);
                _parameters.Add(_verificationConditionQuantifier);
                _parameters.Add(_docTypesToVerify);
                _parameters.Add(_queryForAutomatedRedaction);
                _parameters.Add(_automatedCondition);
                _parameters.Add(_automatedConditionQuantifier);
                _parameters.Add(_docTypesToRedact);
                _parameters.Add(_createTestOutputVoaFiles);
                _parameters.Add(_outputAttributeNamesFileLists);
                _parameters.Add(_outputFinalStatsOnly);
                _parameters.Add(_outputHybridStats);
                _parameters.Add(_outputAutomatedStatsOnly);
                _parameters.Add(_typesToBeTested);
                _parameters.Add(_outputFilesFolder);
                _parameters.Add(new IDShieldTesterFolder());

                // If a settings file name was provided, read in the settings from the file.
                if (!string.IsNullOrEmpty(settingsFileName))
                {
                    IDShieldTesterParameter.ReadFromFile(_settingsFileName, _parameters);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28537", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a list of attribute names which, together with
        /// <see cref="VerificationConditionQuantifier"/> specify which documents would be
        /// verified in a standard workflow.
        /// </summary>
        /// <value>A list of attribute names specifying which documents would be verified in a
        /// standard workflow.
        /// </value>
        public string VerificationCondition
        {
            get
            {
                return _verificationCondition.Value;
            }

            set
            {
                _verificationCondition.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the method the <see cref="VerificationCondition"/> should be used in
        /// determining which documents would be verified in a standard workflow.
        /// </summary>
        /// <value>The method that should be used in determining which documents would be verified
        /// in a standard workflow.</value>
        public string VerificationConditionQuantifier
        {
            get
            {
                return _verificationConditionQuantifier.Value;
            }

            set
            {
                _verificationConditionQuantifier.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets which document types would be verified in a standard workflow.
        /// </summary>
        /// <value>The document types that would be verified in a standard workflow.</value>
        public string DocTypesToVerify
        {
            get
            {
                return _docTypesToVerify.Value;
            }

            set
            {
                _docTypesToVerify.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a query that specifies which attributes should be automatically redacted.
        /// </summary>
        /// <value>A query that specifies which attributes should be automatically redacted.</value>
        public string QueryForAutomatedRedaction
        {
            get
            {
                return _queryForAutomatedRedaction.Value;
            }

            set
            {
                _queryForAutomatedRedaction.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a list of attribute names which, together with
        /// <see cref="AutomatedConditionQuantifier"/> specify which documents would be
        /// automatically redacted in an automated or hybrid workflow.
        /// </summary>
        /// <value>A list of attribute names specifying which documents would be automatically
        /// redacted in an automated or hybrid workflow.
        /// </value>
        public string AutomatedCondition
        {
            get
            {
                return _automatedCondition.Value;
            }

            set
            {
                _automatedCondition.Value = value;
            }
        }

        
        /// <summary>
        /// Gets or sets the method the <see cref="AutomatedCondition"/> should be used in
        /// determining which documents would be automatically redacted in an automated or hybrid
        /// workflow.
        /// </summary>
        /// <value>The method that should be used in determining which documents would be
        /// automatically redacted in an automated or hybrid workflow.</value>
        public string AutomatedConditionQuantifier
        {
            get
            {
                return _automatedConditionQuantifier.Value;
            }

            set
            {
                _automatedConditionQuantifier.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets which document types would be automatically redacted in an automated or
        /// hybrid workflow.
        /// </summary>
        /// <value>The document types that would be automatically redacted in an automated or hybrid
        /// workflow.</value>
        public string DocTypesToRedact
        {
            get
            {
                return _docTypesToRedact.Value;
            }

            set
            {
                _docTypesToRedact.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets whether output files should be created.
        /// </summary>
        /// <value><see langword="true"/> to create output analysis files; <see langword="false"/>
        /// otherwise.</value>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Voa")]
        public bool CreateTestOutputVoaFiles
        {
            get
            {
                return _createTestOutputVoaFiles.Value;
            }

            set
            {
                _createTestOutputVoaFiles.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the test status should be updated as it progresses or whether
        /// the test should run without updating status and only output stats at the end.
        /// </summary>
        /// <value><see langword="true"/> to output final stats only; <see langword="false"/>
        /// to update test case results as the test progresses.</value>
        public bool OutputFinalStatsOnly
        {
            get
            {
                return _outputFinalStatsOnly.Value;
            }

            set
            {
                _outputFinalStatsOnly.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to generate attribute name file lists.
        /// </summary>
        /// <value><see langword="true"/> to generate attribute name file lists;
        /// <see langword="false"/> otherwise.</value>
        public bool OutputAttributeNamesFileLists
        {
            get
            {
                return _outputAttributeNamesFileLists.Value;
            }

            set
            {
                _outputAttributeNamesFileLists.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to generate hybrid stats.
        /// </summary>
        /// <value><see langword="true"/> to generate hybrid stats;
        /// <see langword="false"/> otherwise.</value>
        public bool OutputHybridStats
        {
            get
            {
                return _outputHybridStats.Value;
            }

            set
            {
                _outputHybridStats.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to output only automated stats
        /// </summary>
        /// <value><see langword="true"/> to output only automated stats;
        /// <see langword="false"/> otherwise.</value>
        public bool OutputAutomatedStatsOnly
        {
            get
            {
                return _outputAutomatedStatsOnly.Value;
            }

            set
            {
                _outputAutomatedStatsOnly.Value = value;
            }
        }
        
        /// <summary>
        /// Gets or sets which attribute types are to be included in the test.
        /// </summary>
        /// <value>If specified, which attribute types are to be included in the test. If not
        /// specified all types will be included.</value>
        public string TypesToBeTested
        {
            get
            {
                return _typesToBeTested.Value;
            }

            set
            {
                _typesToBeTested.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the folder to which test results will be output.
        /// </summary>
        /// <value>The folder to which test results will be output..</value>
        public string OutputFilesFolder
        {
            get
            {
                return _outputFilesFolder.Value;
            }

            set
            {
                _outputFilesFolder.Value = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Saves the current ID Shield tester parameters to disk.
        /// </summary>
        /// <param name="settingsFileName">The file to save the parameters to.
        /// <para><b>Note:</b></para>
        /// Any existing file of the specified name will be overwritten.</param>
        public void Save(string settingsFileName)
        {
            try
            {
                IDShieldTesterParameter.WriteToFile(settingsFileName, _parameters);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28648", ex);
            }
        }

        /// <summary>
        /// Retrieves a collection of all test folders.
        /// </summary>
        /// <returns>A collection of all test <see cref="IDShieldTesterFolder"/>s.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Collection<IDShieldTesterFolder> GetTestFolders()
        {
            try
            {
                Collection<IDShieldTesterFolder> testFolders =
                    new Collection<IDShieldTesterFolder>();

                foreach (IDShieldTesterParameter parameter in _parameters)
                {
                    IDShieldTesterFolder testFolder = parameter as IDShieldTesterFolder;
                    if (testFolder != null && testFolder.Populated)
                    {
                        testFolders.Add(testFolder);
                    }
                }

                return testFolders;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28649", ex);
            }
        }

        /// <summary>
        /// Adds a new test folder to the parameter list.
        /// </summary>
        /// <param name="testFolder">The <see cref="IDShieldTesterFolder"/> to add.</param>
        public void AddTestFolder(IDShieldTesterFolder testFolder)
        {
            _parameters.Add(testFolder);
        }

        #endregion Methods
    }
}
