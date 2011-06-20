using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Specifies the comparison used by the VOA contents condition to determine if the number of
    /// qualifying attributes in a VOA file meets the requirement.
    /// </summary>
    [ComVisible(true)]
    [Guid("A1AF2E04-C809-47EB-AE9D-568E4D6B94B9")]
    public enum VOAContentsConditionRequirement
    {
        /// <summary>
        /// The VOA file is required to contain exactly this many qualifying attributes.
        /// </summary>
        ContainsExactly = 0,

        /// <summary>
        /// The VOA file is required to contain greater than or equal to this many qualifying
        /// attributes.
        /// </summary>
        ContainsAtLeast = 1,

        /// <summary>
        /// The VOA file is required to contain less than or equal to this many qualifying attributes.
        /// </summary>
        ContainsAtMost = 2,

        /// <summary>
        /// The VOA file is required to contain an number except this many qualifying attributes.
        /// </summary>
        DoesNotContainExactly = 3
    }

    /// <summary>
    /// Specifies which attribute field will be used to qualify attributes in the VOA contents
    /// condition.
    /// </summary>
    [ComVisible(true)]
    [Guid("67F8E160-D963-4669-B2D7-698C92BE0226")]
    public enum AttributeField
    {
        /// <summary>
        /// The name of the attribute will be compared.
        /// </summary>
        Name = 0,

        /// <summary>
        /// The value of the attribute will be compared.
        /// </summary>
        Value = 1,

        /// <summary>
        /// The type of the attribute will be compared.
        /// </summary>
        Type = 2
    }

    /// <summary>
    /// Specifies the way in which the attribute's field will be tested for qualification. 
    /// </summary>
    [ComVisible(true)]
    [Guid("7D71BF7A-342C-42FC-A956-271A35F474CD")]
    public enum AttributeComparisonMethod
    {
        /// <summary>
        /// The attribute field will be directly compared to another value.
        /// </summary>
        Comparison = 0,

        /// <summary>
        /// The attribute field will be tested as to whether it falls in a specified range.
        /// </summary>
        Range = 1,

        /// <summary>
        /// The attribute field will be searched for a specified value.
        /// </summary>
        Search = 2,

        /// <summary>
        /// The attribute field will be compared to a list of possible qualifying values.
        /// </summary>
        List = 3
    }

    /// <summary>
    /// In the case that <see cref="AttributeComparisonMethod.Comparison"/> is being used by the
    /// VOA file contents condition, specifies how the attribute field will be compared.
    /// </summary>
    [ComVisible(true)]
    [Guid("FE4CC2BE-F351-401F-B00F-8B4081B04347")]
    public enum ComparisonOperator
    {
        /// <summary>
        /// The attribute field qualifies if it is equal to the comparison value.
        /// </summary>
        Equal = 0,

        /// <summary>
        /// The attribute field qualifies if it is not equal to the comparison value.
        /// </summary>
        NotEqual = 1,

        /// <summary>
        /// The attribute field qualifies if it is less than the comparison value.
        /// </summary>
        LessThan = 2,

        /// <summary>
        /// The attribute field qualifies if it is less than or equal to the comparison value.
        /// </summary>
        LessThanEqual = 3,

        /// <summary>
        /// The attribute field qualifies if it is greater than the comparison value.
        /// </summary>
        GreaterThan = 4,

        /// <summary>
        /// The attribute field qualifies if it is greater than or equal to the comparison value.
        /// </summary>
        GreaterThanEqual = 5
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on the attributes in a VOA data file.
    /// </summary>
    [ComVisible(true)]
    [Guid("19531D71-4DC8-4819-A151-E0B4DC38133F")]
    [ProgId("Extract.FileActionManager.Conditions.VOAFileContentsCondition")]
    public class VOAFileContentsCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFAMCondition, ILicensedComponent,
        IPersistStream
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "VOA file contents condition";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// If <see langword="true"/> the condition will be met if the specified condition is
        /// <see langword="true"/>; if <see langword="false"/> the condition will be met if the
        /// specified condition is <see langword="false"/>.
        /// </summary>
        bool _metIfTrue = true;

        /// <summary>
        /// Specifies the filename of the VOA file that is to be tested.
        /// </summary>
        string _voaFileName = "<SourceDocName>.voa";

        /// <summary>
        /// Specifies the <see cref="VOAContentsConditionRequirement"/> the condition is to use.
        /// </summary>
        VOAContentsConditionRequirement _requirement =
            VOAContentsConditionRequirement.ContainsAtLeast;

        /// <summary>
        /// Specifies the required number of attributes for the condition to be true.
        /// </summary>
        int _attributeCount = 1;

        /// <summary>
        /// Specifies the attribute query specifying the domain of attributes to be tested.
        /// </summary>
        string _attributeQuery = "*";

        /// <summary>
        /// Specifies the <see cref="AttributeField"/> whose value is to be compared.
        /// </summary>
        AttributeField _comparisonField;

        /// <summary>
        /// Specifies the <see cref="AttributeComparisonMethod"/> the condition is to use.
        /// </summary>
        AttributeComparisonMethod _comparisonMethod;

        /// <summary>
        /// Specifies the <see cref="ComparisonOperator"/> the condition is to use.
        /// </summary>
        ComparisonOperator _comparisonOperator;

        /// <summary>
        /// Specifies the value the specified <see cref="AttributeField"/> is to be compared to.
        /// </summary>
        string _comparisonValue;

        /// <summary>
        /// Specifies the lower end of a range the <see cref="AttributeField"/> must fall within.
        /// </summary>
        string _rangeMinValue;

        /// <summary>
        /// Specifies the upper end of a range the <see cref="AttributeField"/> must fall within.
        /// </summary>
        string _rangeMaxValue;

        /// <summary>
        /// Specifies whether a search must fully match the specified <see cref="AttributeField"/>.
        /// </summary>
        bool _searchFullyMatches;

        /// <summary>
        /// Specifies the pattern in the specified <see cref="AttributeField"/> to search for.
        /// </summary>
        string _searchPattern;

        /// <summary>
        /// Specifies the possible values the specified <see cref="AttributeField"/> must match.
        /// </summary>
        string[] _listValues;

        /// <summary>
        /// The <see cref="StringComparison"/> to use whenever strings are to be compared when
        /// evaluating the condition.
        /// </summary>
        StringComparison _stringComparisonMode = StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Specifies whether a search term should be treated as a regular expression (where
        /// applicable).
        /// </summary>
        bool _useRegex;

        /// <summary>
        /// An <see cref="AFUtility"/> used to evaluate attribute queries.
        /// </summary>
        AFUtility _afUtility;

        /// <summary>
        /// <see langword="true"/> if the <see cref="AttributeField"/> should be treated as a
        /// <see langword="double"/>.
        /// </summary>
        bool _usingDoubleComparison;

        /// <summary>
        /// When _usingDoubleComparison is <see langword="true"/>, the <see langword="double"/>
        /// equivalent of <see cref="ComparisonValue"/>.
        /// </summary>
        double _doubleCompareValue;

        /// <summary>
        /// When _usingDoubleComparison is <see langword="true"/>, the <see langword="double"/>
        /// equivalent of <see cref="RangeMinValue"/>.
        /// </summary>
        double _rangeMinDoubleValue;

        /// <summary>
        /// When _usingDoubleComparison is <see langword="true"/>, the <see langword="double"/>
        /// equivalent of <see cref="RangeMaxValue"/>.
        /// </summary>
        double _rangeMaxDoubleValue;

        /// <summary>
        /// Indicates whether the current settings have been validated.
        /// </summary>
        bool _settingsValidated;

        /// <summary>
        /// The <see cref="DotNetRegexParser"/> used to perform any regular expression searches.
        /// </summary>
        DotNetRegexParser _regexParser;

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="VOAFileContentsCondition"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileContentsCondition"/> class.
        /// </summary>
        public VOAFileContentsCondition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileContentsCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="VOAFileContentsCondition"/> from which
        /// settings should be copied.</param>
        public VOAFileContentsCondition(VOAFileContentsCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32664");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the condition is met when the specified
        /// condition is <see langword="true"/>.
        /// </summary>
        /// <value>If <see langword="true"/> the condition will be met if the specified condition is
        /// <see langword="true"/>; if <see langword="false"/> the condition will be met if the
        /// specified condition is <see langword="false"/>.
        /// </value>
        public bool MetIfTrue
        {
            get
            {
                return _metIfTrue;
            }

            set
            {
                try
                {
                    if (value != _metIfTrue)
                    {
                        _metIfTrue = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32665", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the VOA file to be tested.
        /// </summary>
        /// <value>
        /// The name of the VOA file to be tested.
        /// </value>
        public string VOAFileName
        {
            get
            {
                return _voaFileName;
            }

            set
            {
                try
                {
                    if (value != _voaFileName)
                    {
                        _voaFileName = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32666", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="VOAContentsConditionRequirement"/> the condition
        /// is to use.
        /// </summary>
        /// <value>
        /// The <see cref="VOAContentsConditionRequirement"/> the condition is to use.
        /// </value>
        public VOAContentsConditionRequirement Requirement
        {
            get
            {
                return _requirement;
            }

            set
            {
                try
                {
                    if (value != _requirement)
                    {
                        _requirement = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32734", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the required number of attributes for the condition to be true.
        /// </summary>
        /// <value>
        /// The required number of attributes for the condition to be true.
        /// </value>
        public int AttributeCount
        {
            get
            {
                return _attributeCount;
            }

            set
            {
                try
                {
                    if (value != _attributeCount)
                    {
                        ExtractException.Assert("ELI32667",
                            "Attribute count must be greater than or equal to 0.",
                            value >= 0);

                        _attributeCount = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32668", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the attribute query specifying the domain of attributes to be tested.
        /// <para><b>Note:</b></para>
        /// A comma separated list of attribute names will also be accepted by this field in lieu of
        /// the standard attribute query syntax.
        /// </summary>
        /// <value>
        /// The the attribute query specifying the domain of attributes to be tested.
        /// </value>
        public string AttributeQuery
        {
            get
            {
                return _attributeQuery;
            }

            set
            {
                try
                {
                    if (value != _attributeQuery)
                    {
                        _attributeQuery = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32669", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AttributeField"/> whose value is to be compared.
        /// </summary>
        /// <value>
        /// The <see cref="AttributeField"/> whose value is to be compared.
        /// </value>
        public AttributeField ComparisonField
        {
            get
            {
                return _comparisonField;
            }

            set
            {
                try
                {
                    if (value != _comparisonField)
                    {
                        _comparisonField = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32701", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AttributeComparisonMethod"/> the condition is to use.
        /// </summary>
        /// <value>
        /// The <see cref="AttributeComparisonMethod"/> the condition is to use.
        /// </value>
        public AttributeComparisonMethod ComparisonMethod
        {
            get
            {
                return _comparisonMethod;
            }

            set
            {
                try
                {
                    if (value != _comparisonMethod)
                    {
                        _comparisonMethod = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32702", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ComparisonOperator"/> the condition is to use.
        /// </summary>
        /// <value>The <see cref="ComparisonOperator"/> the condition is to use.</value>
        public ComparisonOperator ComparisonOperator
        {
            get
            {
                return _comparisonOperator;
            }

            set
            {
                try
                {
                    if (value != _comparisonOperator)
                    {
                        _comparisonOperator = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32670", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value the specified <see cref="AttributeField"/> is to be compared to.
        /// </summary>
        /// <value>
        /// The value the specified <see cref="AttributeField"/> is to be compared to.
        /// </value>
        public string ComparisonValue
        {
            get
            {
                return _comparisonValue;
            }

            set
            {
                try
                {
                    if (value != _comparisonValue)
                    {
                        _comparisonValue = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32671", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower end of a range the <see cref="AttributeField"/> must fall within.
        /// </summary>
        /// <value>The lower end of a range the <see cref="AttributeField"/> must fall within.
        /// </value>
        public string RangeMinValue
        {
            get
            {
                return _rangeMinValue;
            }

            set
            {
                try
                {
                    if (value != _rangeMinValue)
                    {
                        _rangeMinValue = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32672", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the upper end of a range the <see cref="AttributeField"/> must fall within.
        /// </summary>
        /// <value>The upper end of a range the <see cref="AttributeField"/> must fall within.
        /// </value>
        public string RangeMaxValue
        {
            get
            {
                return _rangeMaxValue;
            }

            set
            {
                try
                {
                    if (value != _rangeMaxValue)
                    {
                        _rangeMaxValue = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32673", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether a search must fully match the specified
        /// <see cref="AttributeField"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if a search must fully match the specified
        /// <see cref="AttributeField"/>; <see langword="false"/> if the <see cref="AttributeField"/>
        /// must only contain a match.</value>
        public bool SearchFullyMatches
        {
            get
            {
                return _searchFullyMatches;
            }

            set
            {
                try
                {
                    if (value != _searchFullyMatches)
                    {
                        _searchFullyMatches = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32703", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the pattern in the specified <see cref="AttributeField"/> to search for.
        /// </summary>
        /// <value>
        /// The pattern in the specified <see cref="AttributeField"/> to search for.
        /// </value>
        public string SearchPattern
        {
            get
            {
                return _searchPattern;
            }

            set
            {
                try
                {
                    if (value != _searchPattern)
                    {
                        _searchPattern = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32674", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the possible values the specified <see cref="AttributeField"/> must match.
        /// </summary>
        /// <value>The possible values the specified <see cref="AttributeField"/> must match.</value>
        // Allow the return value to be an array since this is a COM visible property.
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] ListValues
        {
            get
            {
                return _listValues;
            }

            set
            {
                try
                {
                    if (value == null)
                    {
                        _listValues = null;
                    }
                    else
                    {
                        _listValues = new string[value.Length];
                        value.CopyTo(_listValues, 0);
                    }

                    SetDirty(true);
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32675", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="StringComparison"/> to use whenever strings are to be
        /// searched for or compared when evaluating the condition.
        /// <para><b>Note:</b></para>
        /// For a regular expression search, only the case sensitivity from this setting applies.
        /// </summary>
        /// <value>The <see cref="StringComparison"/> to use whenever strings are to be
        /// searched for or compared when evaluating the condition.</value>
        public StringComparison StringComparisonMode
        {
            get
            {
                return _stringComparisonMode;
            }

            set
            {
                try
                {
                    if (value != _stringComparisonMode)
                    {
                        _stringComparisonMode = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32676", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether a search term should be treated as a regular expression (where
        /// applicable).
        /// <para><b>Note:</b></para>
        /// Fuzzy search syntax is supported.
        /// </summary>
        /// <value><see langword="true"/> if to treat the search term as a regular expression;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool UseRegex
        {
            get
            {
                return _useRegex;
            }

            set
            {
                try
                {
                    if (value != _useRegex)
                    {
                        _useRegex = value;
                        SetDirty(true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32677", ex.Message);
                }
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Validates the instance's current settings.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the instance's settings are not valid.</throws>
        public void ValidateSettings()
        {
            try
            {
                if (_settingsValidated)
                {
                    return;
                }

                ValidateAttributeQuery();

                ValidateComparison();

                ValidateRange();

                ValidateSearchPattern();

                ValidateList();

                _settingsValidated = true;
            }
            catch (Exception ex)
            {
                _settingsValidated = false;
                _regexParser = null;

                throw ex.CreateComVisible("ELI32678", ex.Message);
            }
        }

        #endregion Public Methods

        #region IFAMCondition Members

        /// <summary>
        /// Tests the VOA data file indicated by the specified <see paramref="pFileRecord"/> against
        /// the specified settings to determine if the condition is met.
        /// </summary>
        /// <param name="pFileRecord">A <see cref="FileRecord"/> specifing the file to be tested.
        /// </param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><see langword="true"/> if the condition was met, <see langword="false"/> if it
        /// was not.</returns>
        public bool FileMatchesFAMCondition(FileRecord pFileRecord, FileProcessingDB pFPDB,
            int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI32679",
                    _COMPONENT_DESCRIPTION);

                // Validating the settings also initializes objects used by the condition.
                ValidateSettings();

                string voaFileName = pFAMTagManager.ExpandTags(VOAFileName, pFileRecord.Name);

                if (!File.Exists(voaFileName))
                {
                    ExtractException ee =
                        new ExtractException("ELI32680", "Could not find specified VOA file");
                    ee.AddDebugData("Configured Path", VOAFileName, false);
                    ee.AddDebugData("Expanded Path", voaFileName, false);
                    throw ee;
                }

                // Load the attributes from the VOA file.
                IUnknownVector attributes = new IUnknownVector();
                attributes.LoadFrom(voaFileName, false);

                // Allow for query to be a simple comma delimited list of attribute names.
                string attributeQuery = AttributeQuery.Replace(',', '|');
                attributeQuery = attributeQuery.Replace(" ", "");
                attributes = AFUtility.QueryAttributes(attributes, attributeQuery, false);

                // Retrieves the AttributeField for each attribute.
                IEnumerable<string> comparisonValues = GetComparisonValues(attributes);

                // Get the number of these values that qualify.
                int qualifyingCount = comparisonValues
                    .Where(value => QualifyValue(value))
                    .Count();

                // Determine if this count makes the condition true.
                bool comparisonResult;
                switch (Requirement)
                {
                    case VOAContentsConditionRequirement.ContainsExactly:
                        comparisonResult = (qualifyingCount == AttributeCount);
                        break;

                    case VOAContentsConditionRequirement.ContainsAtLeast:
                        comparisonResult = (qualifyingCount >= AttributeCount);
                        break;

                    case VOAContentsConditionRequirement.ContainsAtMost:
                        comparisonResult = (qualifyingCount <= AttributeCount);
                        break;

                    case VOAContentsConditionRequirement.DoesNotContainExactly:
                        comparisonResult = (qualifyingCount != AttributeCount);
                        break;

                    default:
                        throw new ExtractException("ELI32681",
                            "Unexpected VOA file contents condition requirement.");
                }

                // Determine if the condition is met.
                return comparisonResult == MetIfTrue;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32682",
                    "Error occured in '" + _COMPONENT_DESCRIPTION + "'", ex);
            }
        }

        /// <summary>
        /// Returns bool value indicating if the condition requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the condition requires admin access
        /// <see langword="false"/> if condition does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFAMCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="VOAFileContentsCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI32683",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                VOAFileContentsCondition cloneOfThis = (VOAFileContentsCondition)Clone();

                using (VOAFileContentsConditionSettingsDialog dlg
                    = new VOAFileContentsConditionSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32684", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                try
                {
                    // Return true if ValidateSettings does not throw an exception.
                    ValidateSettings();

                    return true;
                }
                catch
                {
                    // Otherwise return false and eat the exception.
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32685",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="VOAFileContentsCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="VOAFileContentsCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new VOAFileContentsCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32686",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="VOAFileContentsCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as VOAFileContentsCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to VOAFileContentsCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32687",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.FlexIndexCoreObjects);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns>
        ///   <see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    MetIfTrue = reader.ReadBoolean();
                    VOAFileName = reader.ReadString();
                    Requirement = (VOAContentsConditionRequirement)reader.ReadInt32();
                    AttributeCount = reader.ReadInt32();
                    AttributeQuery = reader.ReadString();
                    ComparisonField = (AttributeField)reader.ReadInt32();
                    ComparisonMethod = (AttributeComparisonMethod)reader.ReadInt32();
                    ComparisonOperator = (ComparisonOperator)reader.ReadInt32();
                    ComparisonValue = reader.ReadString();
                    RangeMinValue = reader.ReadString();
                    RangeMaxValue = reader.ReadString();
                    SearchFullyMatches = reader.ReadBoolean();
                    SearchPattern = reader.ReadString();
                    ListValues = reader.ReadStringArray();
                    StringComparisonMode = (StringComparison)reader.ReadInt32();
                    UseRegex = reader.ReadBoolean();
                }

                // Freshly loaded object is no longer dirty
                SetDirty(false);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32688",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                ValidateSettings();

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(MetIfTrue);
                    writer.Write(VOAFileName);
                    writer.Write((int)Requirement);
                    writer.Write(AttributeCount);
                    writer.Write(AttributeQuery);
                    writer.Write((int)ComparisonField);
                    writer.Write((int)ComparisonMethod);
                    writer.Write((int)ComparisonOperator);
                    writer.Write(ComparisonValue);
                    writer.Write(RangeMinValue);
                    writer.Write(RangeMaxValue);
                    writer.Write(SearchFullyMatches);
                    writer.Write(SearchPattern);
                    writer.Write(ListValues);
                    writer.Write((int)StringComparisonMode);
                    writer.Write(UseRegex);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    SetDirty(false);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32689",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Copies the specified <see cref="VOAFileContentsCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="VOAFileContentsCondition"/> from which to copy.
        /// </param>
        void CopyFrom(VOAFileContentsCondition source)
        {
            MetIfTrue = source.MetIfTrue;
            VOAFileName = source.VOAFileName;
            Requirement = source.Requirement;
            AttributeCount = source.AttributeCount;
            AttributeQuery = source.AttributeQuery;
            ComparisonField = source.ComparisonField;
            ComparisonMethod = source.ComparisonMethod;
            ComparisonOperator = source.ComparisonOperator;
            ComparisonValue = source.ComparisonValue;
            RangeMinValue = source.RangeMinValue;
            RangeMaxValue = source.RangeMaxValue;
            SearchFullyMatches = source.SearchFullyMatches;
            SearchPattern = source.SearchPattern;
            ListValues = source.ListValues;
            StringComparisonMode = source.StringComparisonMode;
            UseRegex = source.UseRegex;

            SetDirty(true);
        }

        /// <summary>
        /// Gets the <see cref="AFUtility"/> used to evaluate attribute queries.
        /// </summary>
        AFUtility AFUtility
        {
            get
            {
                if (_afUtility == null)
                {
                    _afUtility = new AFUtility();
                }

                return _afUtility;
            }
        }

        /// <summary>
        /// Gets the <see cref="DotNetRegexParser"/> used to perform any regular expression searches.
        /// </summary>
        DotNetRegexParser RegexParser
        {
            get
            {
                if (_regexParser == null)
                {
                    _regexParser = new DotNetRegexParser();

                    // Specify a the pattern based on the configured ComparisonMethod.
                    switch (ComparisonMethod)
                    {
                        case AttributeComparisonMethod.Comparison:
                            _regexParser.Pattern = ComparisonValue;
                            break;

                        case AttributeComparisonMethod.Search:
                            _regexParser.Pattern = SearchPattern;
                            break;

                        case AttributeComparisonMethod.List:
                            {
                                // The pattern should be all the values of the list or'd together.
                                StringBuilder pattern = new StringBuilder("(");
                                bool firstValue = true;
                                foreach (string value in ListValues)
                                {
                                    if (firstValue)
                                    {
                                        firstValue = false;
                                    }
                                    else
                                    {
                                        pattern.Append(")|(");
                                    }
                                    pattern.Append(value);
                                }
                                pattern.Append(")");
                                _regexParser.Pattern = pattern.ToString();
                            }
                            break;

                        default:
                            throw new ExtractException("ELI32690",
                                "Unsupported regex comparison method.");
                    }

                    // Set ignore case based on the specified StringComparisonMode.
                    _regexParser.IgnoreCase =
                        (StringComparisonMode == StringComparison.OrdinalIgnoreCase ||
                         StringComparisonMode == StringComparison.InvariantCultureIgnoreCase ||
                         StringComparisonMode == StringComparison.CurrentCultureIgnoreCase);
                }

                return _regexParser;
            }
        }

        /// <summary>
        /// Sets the dirty flag.
        /// </summary>
        /// <param name="dirty"><see langword="true"/> to set the dirty flag; <see langword="false"/>
        /// to clear it.</param>
        void SetDirty(bool dirty)
        {
            _dirty = dirty;
            if (dirty)
            {
                _settingsValidated = false;
                _regexParser = null;
            }
        }

        /// <summary>
        /// Validates the attribute query.
        /// </summary>
        void ValidateAttributeQuery()
        {
            // Allow for query to be a simple comma delimited list of attribute names.
            string attributeQuery = AttributeQuery.Replace(',', '|');
            attributeQuery = attributeQuery.Replace(" ", "");
            if (!AFUtility.IsValidQuery(attributeQuery))
            {
                ExtractException ee = new ExtractException("ELI32691", "Invalid attribute query");
                ee.AddDebugData("Query", AttributeQuery, false);
                throw ee;
            }
        }

        /// <summary>
        /// If <see cref="AttributeComparisonMethod.Comparison"/> is being used, validates the
        /// associated settings.
        /// </summary>
        void ValidateComparison()
        {
            if (ComparisonMethod == AttributeComparisonMethod.Comparison)
            {
                ExtractException.Assert("ELI32692", "Comparison value not specified.",
                    !string.IsNullOrWhiteSpace(ComparisonValue));

                _usingDoubleComparison = false;

                if (ComparisonOperator == Conditions.ComparisonOperator.Equal ||
                    ComparisonOperator == Conditions.ComparisonOperator.NotEqual)
                {
                    // A regex can apply when testing for equality or non-equality.
                    if (UseRegex)
                    {
                        RegexParser.ValidatePattern();
                    }
                }
                else
                {
                    // A numeric (double) comparison can apply when any greater than or less than
                    // operations are being used.
                    _usingDoubleComparison = double.TryParse(ComparisonValue, out _doubleCompareValue);
                }
            }
        }

        /// <summary>
        /// If <see cref="AttributeComparisonMethod.Range"/> is being used, validates the associated
        /// settings.
        /// </summary>
        void ValidateRange()
        {
            try
            {
                if (ComparisonMethod == AttributeComparisonMethod.Range)
                {
                    _usingDoubleComparison =
                        double.TryParse(RangeMinValue, out _rangeMinDoubleValue) &&
                        double.TryParse(RangeMaxValue, out _rangeMaxDoubleValue);

                    if (_usingDoubleComparison)
                    {
                        ExtractException.Assert("ELI32693",
                            "Range maximum needs to be greater than or equal to the range minimum",
                            _rangeMaxDoubleValue >= _rangeMinDoubleValue);
                    }
                    else
                    {
                        ExtractException.Assert("ELI32694",
                            "Range maximum needs to be greater than or equal to the range minimum",
                            string.Compare(RangeMaxValue, RangeMinValue, StringComparisonMode) >= 0);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI32695", "Invalid range", ex);
                ee.AddDebugData("Range minimum", RangeMinValue, false);
                ee.AddDebugData("Range maximum", RangeMaxValue, false);
                throw ee;
            }
        }

        /// <summary>
        /// If <see cref="AttributeComparisonMethod.Search"/> is being used, validates the associated
        /// settings.
        /// </summary>
        void ValidateSearchPattern()
        {
            if (ComparisonMethod == AttributeComparisonMethod.Search)
            {
                ExtractException.Assert("ELI32696", "Search pattern not specified.",
                    !string.IsNullOrWhiteSpace(SearchPattern));

                if (UseRegex)
                {
                    RegexParser.ValidatePattern();
                }
            }
        }

        /// <summary>
        /// If <see cref="AttributeComparisonMethod.List"/> is being used, validates the associated
        /// settings.
        /// </summary>
        void ValidateList()
        {
            if (ComparisonMethod == AttributeComparisonMethod.List)
            {
                ExtractException.Assert("ELI32697", "No list values have been specified.",
                    ListValues.Length > 0);

                if (UseRegex)
                {
                    RegexParser.ValidatePattern();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="AttributeField"/> for each of the specifed
        /// <see paramref="attributes"/> that will be used to test if the condition is met.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s to be tested.</param>
        /// <returns>The <see cref="AttributeField"/> for each of the specifed</returns>
        IEnumerable<string> GetComparisonValues(IUnknownVector attributes)
        {
            IEnumerable<string> comparisonValues = attributes
                .ToIEnumerable<IAttribute>()
                .Select((attribute) =>
                {
                    switch (ComparisonField)
                    {
                        case AttributeField.Name: return attribute.Name;
                        case AttributeField.Value: return attribute.Value.String;
                        case AttributeField.Type: return attribute.Type;
                        default: throw new ExtractException("ELI32698", "Unexpected attribute field");
                    }
                });

            return comparisonValues;
        }

        /// <summary>
        /// Determines whether the <see paramref="comparisonValue"/> qualifies.
        /// </summary>
        /// <param name="comparisonValue">The value to test for qualification.</param>
        /// <returns><see langword="true"/> if <see paramref="comparisonValue"/> qualifies,
        /// <see langword="false"/> otherwise.</returns>
        bool QualifyValue(string comparisonValue)
        {
            switch (ComparisonMethod)
            {
                case AttributeComparisonMethod.Comparison:
                    return CompareValue(comparisonValue);

                case AttributeComparisonMethod.Range:
                    return IsValueInRange(comparisonValue);

                case AttributeComparisonMethod.Search:
                    return SearchForValue(comparisonValue);

                case AttributeComparisonMethod.List:
                    return IsValueInList(comparisonValue);

                default:
                    {
                        throw new ExtractException("ELI32699", "Unexpected comparison method");
                    }
            }
        }

        /// <summary>
        /// Determines if <see paramref="comparisonValue"/> qualifies by comparing it to
        /// <see cref="ComparisonValue"/>.
        /// </summary>
        /// <param name="comparisonValue">The value to test for qualification.</param>
        /// <returns><see langword="true"/> if <see paramref="comparisonValue"/> qualifies,
        /// <see langword="false"/> otherwise.</returns>
        bool CompareValue(string comparisonValue)
        {
            // Regex searches will be performed only for equality or inequality tests.
            if (UseRegex && ComparisonOperator == Conditions.ComparisonOperator.Equal)
            {
                return RegexParser.StringMatchesPattern(comparisonValue);
            }
            else if (UseRegex && ComparisonOperator == Conditions.ComparisonOperator.NotEqual)
            {
                return !RegexParser.StringMatchesPattern(comparisonValue);
            }
            // Otherwise the qualification will be based on the comparison result combined with
            // the ComparisonOperator.
            else
            {
                double comparisonResult = 0;

                double doubleValue;
                if (_usingDoubleComparison)
                {
                    if (!double.TryParse(comparisonValue, out doubleValue))
                    {
                        // If _usingDoubleComparison and comparisonValue cannot be converted to a double,
                        // consider the value unqualified.
                        return false;
                    }
                    else
                    {
                        comparisonResult = (doubleValue - _doubleCompareValue);
                    }
                }
                else
                {
                    comparisonResult = (int)string.Compare(
                        comparisonValue, ComparisonValue, StringComparisonMode);
                }

                switch (ComparisonOperator)
                {
                    case Conditions.ComparisonOperator.Equal:
                        return comparisonResult == 0;
                    case Conditions.ComparisonOperator.NotEqual:
                        return comparisonResult != 0;
                    case Conditions.ComparisonOperator.GreaterThan:
                        return comparisonResult > 0;
                    case Conditions.ComparisonOperator.GreaterThanEqual:
                        return comparisonResult >= 0;
                    case Conditions.ComparisonOperator.LessThan:
                        return comparisonResult < 0;
                    case Conditions.ComparisonOperator.LessThanEqual:
                        return comparisonResult <= 0;
                    default:
                        throw new ExtractException("ELI32700", "Unexpected comparison operator.");
                }
            }
        }

        /// <summary>
        /// Determines whether <see paramref="comparisonValue"/> qualifies by testing that it is in
        /// the range from <see cref="RangeMinValue"/> to <see cref="RangeMaxValue"/>.
        /// </summary>
        /// <param name="comparisonValue">The value to test for qualification.</param>
        /// <returns><see langword="true"/> if <see paramref="comparisonValue"/> qualifies,
        /// <see langword="false"/> otherwise.</returns>
        bool IsValueInRange(string comparisonValue)
        {
            if (_usingDoubleComparison)
            {
                double doubleValue;
                if (!double.TryParse(comparisonValue, out doubleValue))
                {
                    // If _usingDoubleComparison and comparisonValue cannot be converted to a double,
                    // consider the value unqualified.
                    return false;
                }
                else if (doubleValue >= _rangeMinDoubleValue &&
                         doubleValue <= _rangeMaxDoubleValue)
                {
                    return true;
                }
            }
            else if (string.Compare(comparisonValue, RangeMinValue,
                            StringComparisonMode) >= 0 &&
                     string.Compare(comparisonValue, RangeMaxValue,
                            StringComparisonMode) <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if <see paramref="comparisonValue"/> qualifies by searching it for the
        /// specified <see cref="SearchPattern"/>.
        /// </summary>
        /// <param name="comparisonValue">The value to test for qualification.</param>
        /// <returns><see langword="true"/> if <see paramref="comparisonValue"/> qualifies,
        /// <see langword="false"/> otherwise.</returns>
        bool SearchForValue(string comparisonValue)
        {
            if (UseRegex)
            {
                if (SearchFullyMatches)
                {
                    return RegexParser.StringMatchesPattern(comparisonValue);
                }
                else
                {
                    return RegexParser.StringContainsPattern(comparisonValue);
                }
            }
            else if (SearchFullyMatches)
            {
                return (string.Compare(comparisonValue, SearchPattern, StringComparisonMode) == 0);
            }
            else
            {
                return comparisonValue.IndexOf(SearchPattern, StringComparisonMode) != -1;
            }
        }

        /// <summary>
        /// Determines whether [is value in list] [the specified comparison value].
        /// </summary>
        /// <param name="comparisonValue">The value to test for qualification.</param>
        /// <returns><see langword="true"/> if <see paramref="comparisonValue"/> qualifies,
        /// <see langword="false"/> otherwise.</returns>
        bool IsValueInList(string comparisonValue)
        {
            if (UseRegex)
            {
                return RegexParser.StringMatchesPattern(comparisonValue);
            }
            else
            {
                foreach (string listValue in ListValues)
                {
                    if (string.Compare(comparisonValue, listValue, StringComparisonMode) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Private Members
    }
}
