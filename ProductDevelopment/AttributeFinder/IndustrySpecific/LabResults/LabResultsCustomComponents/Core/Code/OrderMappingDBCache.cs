using Extract.Database;
using Extract.DataEntry;
using Extract.Utilities;
using Extract.Utilities.Parsers;
using Spring.Core.TypeResolution;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;

namespace Extract.LabResultsCustomComponents
{
    class OrderMappingDBCache : IDisposable
    {
        #region Constants

        /// <summary>
        /// The number of times to retry if failed connecting to the database file.
        /// </summary>
        const int _DB_CONNECTION_RETRIES = 5;

        /// <summary>
        /// Ignore non-word chars in test names (e.g., AKAs) unless they have special meaning
        /// (every non-word char except % and #).
        /// </summary>
        const string _IGNORE_PATTERN = @"[_\W-[%#]]+";

        const string _COMPONENT_DATA_DATABASE_PATH
            = @"<ComponentDataDir>\LabDE\TestResults\OrderMapper\OrderMappingDB.sqlite";

        const RegexOptions _CASE_SENSITIVE = RegexOptions.ExplicitCapture
            | RegexOptions.IgnorePatternWhitespace;

        const RegexOptions _IGNORE_CASE = _CASE_SENSITIVE | RegexOptions.IgnoreCase;

        public const string _BLOOD_SAMPLE_TYPE = "BLOOD";
        public const string _URINE_SAMPLE_TYPE = "URINE";

        #endregion Constants

        #region Fields

        SQLiteConnection _customerDBConnection;
        SQLiteConnection _componentDataDBConnection;

        /// <summary>
        /// The name of the database file to use for order mapping.
        /// </summary>
        private string _customerDatabaseFile;

        /// <summary>
        /// Mapping of normalized names to customer test codes 
        /// </summary>
        private Dictionary<string, HashSet<string>> _normalizedNameToCustomerTestCodes
            = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Mapping of normalized names to ES test codes 
        /// </summary>
        private Dictionary<string, HashSet<Tuple<string, int>>> _normalizedNameToESTestCodes
            = new Dictionary<string, HashSet<Tuple<string, int>>>();

        /// <summary>
        /// Mapping of ES test codes to customer test codes
        /// </summary>
        private Dictionary<string, HashSet<string>> _esCodeToCustomerCodes
            = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Mapping of customer test codes to match scoring queries
        /// </summary>
        private Dictionary<string, string> _testCodeToMatchScoringQuery
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Mapping of customer test codes to sample types
        /// </summary>
        private Dictionary<string, string> _testCodeToSampleType
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Mapping of customer codes to ES official names
        /// </summary>
        private Dictionary<string, HashSet<string>> _customerCodeToESNames
            = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A memoized function to compute the normalized (noise characters removed) version of a test name
        /// </summary>
        private Func<string, string> _getNormalizedName;

        /// <summary>
        /// A memoized function that gets a collection of expanded fuzzy regex patterns based on a test name
        /// </summary>
        private Func<string, IEnumerable<string>> _getFuzzyPatterns;

        /// <summary>
        /// A memoized function that gets a collection of possible order codes for a test name
        /// </summary>
        private Func<string, ReadOnlyCollection<string>> _getPotentialOrderCodes;

        /// <summary>
        /// A memoized function that gets a collection of possible test codes for a test name
        /// </summary>
        private Func<string, Tuple<IEnumerable<string>, bool>> _getPotentialTestCodes;

        /// <summary>
        /// A memoized function that computes a fitness score for a mapping of a test code to a test attribute
        /// </summary>
        private Func<string, IAttribute, int> _getMappingScore;

        private string _commonWordsPattern;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The underlying connection to the order mapping database
        /// </summary>
        public SQLiteConnection DBConnection
        {
            get
            {
                return _customerDBConnection;
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderMappingDBCache"/>
        /// </summary>
        /// <param name="pDoc">The document object (used for expanding path tags)</param>
        /// <param name="databaseFile">Path to the order mapping database file (may contain path tags)</param>
        public OrderMappingDBCache(AFDocument pDoc, string databaseFile)
        {
            DefineMemberFunctions();
            TypeRegistry.RegisterType("Regex", typeof(System.Text.RegularExpressions.Regex));
            TypeRegistry.RegisterType("StringUtils", typeof(Spring.Util.StringUtils));

            _customerDatabaseFile = databaseFile;

            _customerDBConnection = GetDatabaseConnection(_customerDatabaseFile, pDoc);
            _componentDataDBConnection = GetDatabaseConnection(_COMPONENT_DATA_DATABASE_PATH, pDoc);

            _customerDBConnection.Open();
            _componentDataDBConnection.Open();

            // Populate various mapping dictionaries
            InitializeMappings();
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Gets a collection of potential customer test codes that the given test name could be mapped to.
        /// </summary>
        /// <param name="testName">The test name (or AKA)</param>
        /// <returns>A collection of potential customer test codes for the given test name</returns>
        public IEnumerable<string> GetPotentialTestCodes(string testName)
        {
            bool _;
            return GetPotentialTestCodes(testName, out _);
        }

        /// <summary>
        /// Gets a collection of potential customer test codes that the given test name could be mapped to.
        /// </summary>
        /// <param name="testName">The test name (or AKA)</param>
        /// <param name="fuzzyMatch">Whether fuzzy pattern matching was necessary to find the potential test codes</param>
        /// <returns>A collection of potential customer test codes for the given test name</returns>
        public IEnumerable<string> GetPotentialTestCodes(string testName, out bool fuzzyMatch)
        {
            try
            {
                var potentialTestCodesAndFuzzyMatch = _getPotentialTestCodes(testName);
                fuzzyMatch = potentialTestCodesAndFuzzyMatch.Item2;
                return potentialTestCodesAndFuzzyMatch.Item1;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI38971",
                    "Failed to get potential test codes for test name!", ex);
                ee.AddDebugData("Test name", testName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the official ES test name(s) that corresponds to a customer test code.
        /// </summary>
        /// <param name="testCode">A customer test code</param>
        /// <param name="esNames">ES test names that correspond to customer test codes</param>
        /// <returns>True if there are corresponding name(s) else false</returns>
        public bool TryGetESNames(string testCode, out HashSet<string> esNames)
        {
            try
            {
                return _customerCodeToESNames.TryGetValue(testCode, out esNames);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI38972",
                    "Failed to get ES names for test code!", ex);
                ee.AddDebugData("Test code", testCode, false);
                throw ee;
            }
        }
        
        /// <summary>
        /// Gets the list of potential order codes for the specified test. Uses cached values if available.
        /// </summary>
        /// <param name="testName">The test to get the order codes for.</param>
        /// <returns>A list of potential order codes (or an empty list if no potential orders).
        /// </returns>
        public ReadOnlyCollection<string> GetPotentialOrderCodes(string testName)
        {
            try
            {
                return _getPotentialOrderCodes(testName);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI38973",
                    "Failed to get potential order codes for test name!", ex);
                ee.AddDebugData("Test name", testName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Computes a fitness score for a mapping of a test code to a test attribute
        /// </summary>
        /// <param name="testCode"></param>
        /// <param name="testAttribute"></param>
        /// <returns></returns>
        public int GetMappingScore(string testCode, IAttribute testAttribute)
        {
            try
            {
                return _getMappingScore(testCode, testAttribute);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI38974",
                    "Failed to get mapping score!", ex);
                ee.AddDebugData("Test code", testCode, false);
                ee.AddDebugData("Test attribute value", testAttribute.Value.String, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the sample type associated with a customer test code.
        /// </summary>
        /// <param name="testCode">A customer test code</param>
        /// <returns>The sample type associated with the test code or null if no associated sample type</returns>
        public string GetSampleType(string testCode)
        {
            try
            {
                string sampleType;
                if (_testCodeToSampleType.TryGetValue(testCode, out sampleType))
                {
                    return sampleType;
                }
                return null;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI38975",
                    "Failed to get sample type for test code!", ex);
                ee.AddDebugData("Test code", testCode, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets whether filled requirements are defined for any orders
        /// </summary>
        /// <returns>True if any orders have filled requirements, else false</returns>
        public bool AreFilledRequirementsDefined()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM [LabOrder] WHERE [FilledRequirement] > 0";
                using (SQLiteCommand command = new(query, DBConnection))
                {
                    return (long)command.ExecuteScalar() > 0;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI39139",
                    "Failed to query FilledRequirement!", ex);
                throw ee;
            }
        }

        /// <summary>
        /// Gets whether mandatory test requirements are defined for any orders
        /// </summary>
        /// <returns>True if any orders have mandatory tests, else false</returns>
        public bool AreMandatoryRequirementsDefined()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM [LabOrderTest] WHERE [Mandatory] = 1";
                using (SQLiteCommand command = new(query, DBConnection))
                {
                    return (long)command.ExecuteScalar() > 0;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI39140",
                    "Failed to query for Mandatory requirements!", ex);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the ES test codes for a test name/AKA
        /// </summary>
        /// <remarks>Used to add ESTestCodes subattributes for debugging purposes</remarks>
        /// <param name="testName">The test name/AKA</param>
        /// <returns>A collection of ES test codes for the test name/AKA</returns>
        public IEnumerable<string> GetESTestCodesForName(string testName)
        {
            try
            {
                HashSet<Tuple<string, int>> esTestCodes;
                if (_normalizedNameToESTestCodes.TryGetValue(_getNormalizedName(testName), out esTestCodes)
                    && esTestCodes.Count > 0)
                {
                    return esTestCodes
                        .OrderByDescending(p => p.Item2)
                        .Select(p => p.Item1)
                        .Distinct();
                }
                else
                {
                    return Enumerable.Empty<string>();
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40199");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the database connection to use for processing
        /// </summary>
        /// <param name="databasePath">The path to the database file</param>
        /// <param name="pDoc">The document object.</param>
        /// <returns>The <see cref="SQLiteConnection"/> to use for processing.</returns>
        static SQLiteConnection GetDatabaseConnection(string databasePath, AFDocument pDoc)
        {
            try
            {
                // Expand the tags in the database file names
                AFUtility afUtility = new AFUtility();
                string expandedDatabasePath = afUtility.ExpandTagsAndFunctions(databasePath, pDoc);

                // Check for the database file existence
                if (!File.Exists(expandedDatabasePath))
                {
                    ExtractException ee = new ExtractException("ELI26170",
                        "Database file does not exist!");
                    ee.AddDebugData("Database File Name", expandedDatabasePath, false);
                    throw ee;
                }

                // Try to open the database connection, if there is a SQLite exception,
                // just increment retry count, sleep, and try again
                int retryCount = 0;
                Exception tempEx = null;
                SQLiteConnection dbConnection = null;
                while (dbConnection == null && retryCount < _DB_CONNECTION_RETRIES)
                {
                    try
                    {
                        dbConnection = new SQLiteConnection(
                            SqliteMethods.BuildConnectionString(expandedDatabasePath));
                    }
                    catch (SQLiteException ex)
                    {
                        tempEx = ex;
                        retryCount++;
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        throw ExtractException.AsExtractException("ELI26651", ex);
                    }
                }

                // If all the retries failed and the connection is still null, throw an exception
                if (retryCount >= _DB_CONNECTION_RETRIES && dbConnection == null)
                {
                    ExtractException ee = new ExtractException("ELI26652",
                        "Unable to open database connection!", tempEx);
                    ee.AddDebugData("Retries", retryCount, false);
                    throw ee;
                }
                return dbConnection;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27743",
                    "Failed to obtain a database connection!", ex);
                ee.AddDebugData("Database path", databasePath, false);
                throw ee;
            }
        }

        /// <summary>
        /// Defines various member functions
        /// </summary>
        private void DefineMemberFunctions()
        {
            // A memoized function to compute the normalized (noise characters removed) version of a test name
            _getNormalizedName = name => Regex.Replace(name, _IGNORE_PATTERN, "").ToUpperInvariant();
            _getNormalizedName = _getNormalizedName.Memoize();


            // A memoized function that computes a fitness score for a mapping of a test code to a test attribute
            _getMappingScore = (testCode, testAttribute) =>
            {
                string testScoringQuery = null;

                try
                {
                    if (!_testCodeToMatchScoringQuery.TryGetValue(testCode, out testScoringQuery)
                        || String.IsNullOrWhiteSpace(testScoringQuery))
                    {
                        return 0;
                    }

                    QueryResult result = null;
                    AttributeStatusInfo.InitializeForQuery(testAttribute.SubAttributes, testAttribute.Value.SourceDocName, null);
                    using (var query = DataEntryQuery.Create(testScoringQuery, testAttribute))
                    {
                        result = query.Evaluate();
                    }
                    AttributeStatusInfo.ResetData();

                    int retVal = 0;
                    return result.Max(r => Int32.TryParse(result.FirstString, out retVal) ? retVal : 0);
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI38969",
                        "Failed to score potential match!", ex);
                    ee.AddDebugData("Query", testScoringQuery, false);
                    throw ee;
                }
            };
            _getMappingScore = _getMappingScore.Memoize();


            // A memoized function that gets a collection of possible test codes for a test name
            _getPotentialTestCodes = testName =>
            {
                var normalizedName = _getNormalizedName(testName);
                bool fuzzy = false;

                var potentialESTestCodes = _normalizedNameToESTestCodes.GetOrAdd(normalizedName,
                    _ => new HashSet<Tuple<string, int>>());

                var potentialCustomerTestCodes = _normalizedNameToCustomerTestCodes.GetOrAdd(normalizedName,
                    _ => new HashSet<string>());

                if (potentialCustomerTestCodes.Count == 0 && potentialESTestCodes.Count == 0)
                {
                    // Try harder
                    IEnumerable<string> fuzzyPatterns = _getFuzzyPatterns(testName);

                    // Name was unsuitable for fuzzy search
                    if (!fuzzyPatterns.Any())
                    {
                        return Tuple.Create(fuzzyPatterns, fuzzy);
                    }

                    // Try each available fuzzy pattern until one matches at least one test
                    foreach (string fuzzyPattern in fuzzyPatterns)
                    {
                        bool foundMatch = false;
                        
                        // Try against built-in names first
                        foreach (string name in _normalizedNameToESTestCodes.Keys)
                        {
                            var existingCodes = _normalizedNameToESTestCodes[name];
                            if (existingCodes.Count > 0 && Regex.IsMatch(name, fuzzyPattern, _IGNORE_CASE))
                            {
                                potentialESTestCodes.UnionWith(existingCodes);
                                foundMatch = true;
                            }
                        }

                        // If the pattern didn't match any built-in names, try customer-specific names
                        foreach (string name in _normalizedNameToCustomerTestCodes.Keys)
                        {
                            var existingCodes = _normalizedNameToCustomerTestCodes[name];
                            if (existingCodes.Count > 0 && Regex.IsMatch(name, fuzzyPattern, _IGNORE_CASE))
                            {
                                potentialCustomerTestCodes.UnionWith(existingCodes);
                                foundMatch = true;
                            }
                        }

                        // If this pattern matched something, stop trying subsequent, looser patterns
                        if (foundMatch)
                        {
                            fuzzy = true;
                            break;
                        }
                    }
                }

                // Order result so that built-in mappings are preferred
                return Tuple.Create(potentialESTestCodes
                    .Where(esCodeWithFreq => _esCodeToCustomerCodes.ContainsKey(esCodeWithFreq.Item1))
                    .SelectMany(esCodeWithFreq => _esCodeToCustomerCodes[esCodeWithFreq.Item1]
                        .Select(custCode => new
                        {
                            CustCode = custCode,
                            ESCode = esCodeWithFreq.Item1,
                            Freq = esCodeWithFreq.Item2
                        }))
                    // More frequently occurring aka/testcode pairs are preferred
                    .OrderByDescending(o => o.Freq)
                    // Then prefer customer codes linked to only one ESCode (more specific)
                    .ThenBy(o => _customerCodeToESNames[o.CustCode].Count)
                    // Then prefer alphabetically lower ES test codes because these codes were
                    // chosen with this in mind (e.g., Lymphocytes_P vs LymphocytesA)
                    .ThenBy(o => o.ESCode)
                    .Select(o => o.CustCode)
                    .Union(potentialCustomerTestCodes), fuzzy);
            };
            _getPotentialTestCodes = _getPotentialTestCodes.Memoize();


            // A memoized function that gets a collection of possible order codes for a test name
            _getPotentialOrderCodes = testName =>
            {
                // Create a set to hold the potential codes
                HashSet<string> orderCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                List<string> testCodes = GetPotentialTestCodes(testName)
                    .Select(testCode => "'" + testCode.Replace("'", "''") + "'").ToList();

                if (testCodes.Count > 0)
                {
                    // Query for potential order codes
                    // Use LabOrder.Code instead of LabOrderTest.OrderCode because these two might not be strictly equal
                    // because of the way that SQL handles trailing spaces
                    // https://extract.atlassian.net/browse/ISSUE-12073
                    string query = "SELECT [Code] FROM [LabOrder]"
                        + " WHERE [Code] IN (SELECT [OrderCode] FROM [LabOrderTest]"
                        + " WHERE [TestCode] IN (" + String.Join(",", testCodes) + "))"
                        + " ORDER BY [TieBreaker]";
                    using (SQLiteCommand command = new(query, _customerDBConnection))
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = reader.GetString(0);
                            orderCodes.Add(code);
                        }
                    }
                }

                return new ReadOnlyCollection<string>(orderCodes.ToList<string>());
            };
            _getPotentialOrderCodes = _getPotentialOrderCodes.Memoize();


            // A memoized function that gets a collection of expanded fuzzy regex patterns based on a test name
            _getFuzzyPatterns = testName =>
            {
                // Name without noise characters and common words. Used to determine how many errors to allow
                string normalizedName = _getNormalizedName(Regex.Replace(testName, _commonWordsPattern, ""));

                // If the name has too few characters outside of common words then return an empty collection
                if (normalizedName.Length < 3)
                {
                    return Enumerable.Empty<string>();
                }

                string replacementsOption = @",replacements=([O0]=>[O0])([il]=>[il])(m=>(?>rn|m))(%=>(?>%|[569][il]?[569]?))";

                string largePrefix = @"(?~<method=fast,error=2" + replacementsOption;
                string smallPrefix = @"(?~<method=fast,error=1" + replacementsOption;
                string verySmallPrefix = @"(?~<method=fast,error=0,ws=.,xtra_ws=1" + replacementsOption;
                var prefixes = new string[3] {verySmallPrefix, smallPrefix, largePrefix};

                string subOption = normalizedName.IndexOf('%') >= 0 ? ",sub=[^#]"
                    : normalizedName.IndexOf('#') >= 0 ? ",sub=[^%]" : ",sub=.";

                // So that (?>rn|m) will be allowed per above replacements option
                string searchString = Regex.Escape(_getNormalizedName(Regex.Replace(testName, "rn", "m", _CASE_SENSITIVE)));

                // Allow for the option to try patterns with more errors if the normalized name is longer
                int numberOfPatternsToTry = normalizedName.Length > 6 ? 3 : normalizedName.Length > 3 ? 2 : 1;
                var fuzzyPatterns = new string[numberOfPatternsToTry];

                // Build the patterns
                for (int i = 0; i < numberOfPatternsToTry; i++)
                {
                    string fuzzyPattern = "^" + prefixes[i] + subOption + ">" + searchString + ").{0,2}$";
                    fuzzyPatterns[i] = FuzzySearchRegexBuilder.ExpandFuzzySearchExpressions(fuzzyPattern);
                }

                return fuzzyPatterns;
            };
            _getFuzzyPatterns = _getFuzzyPatterns.Memoize();
        }


        /// <summary>
        /// Query the customer and component data databases and build the mapping dictionaries
        /// </summary>
        private void InitializeMappings()
        {
            // Build the common words pattern
            string query = "SELECT [Word] FROM [ESCommonWords]";
            using (SQLiteCommand command = new(query, _componentDataDBConnection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                _commonWordsPattern = @"(\b|(?=\W))(" + String.Join("|", reader.Cast<IDataRecord>()
                    .Select(r => r.GetString(0))) + @")(\b|(?<=\W))";
            }

            // Populate mapping of names to test codes using customer-specific DB
            query = "SELECT DISTINCT [TestCode], [TestName], [ESComponentCode] FROM "
                + "(SELECT [TestCode], [OfficialName] AS [TestName] FROM [LabTest] "
                + "UNION SELECT [TestCode], [Name] AS [TestName] FROM [AlternateTestName] "
                + "WHERE [StatusCode] = 'A') [Tests] "
                + "LEFT JOIN [ComponentToESComponentMap] ON [TestCode] = [ComponentCode]";

            using (SQLiteCommand command = new(query, _customerDBConnection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            foreach(var r in reader.Cast<IDataRecord>())
            {
                string code = r.GetString(0);
                string name = r.GetString(1);
                // Use 'as string' to handle null values
                string esComponentCode = r[2] as string;

                string normalizedName = _getNormalizedName(name);
                HashSet<string> codes = _normalizedNameToCustomerTestCodes.GetOrAdd(normalizedName,
                    _ => new HashSet<string>());
                codes.Add(code);

                if (esComponentCode != null)
                {
                    // Add this customer code to the map of ES codes to customer codes
                    HashSet<string> customerCodes = _esCodeToCustomerCodes.GetOrAdd(esComponentCode,
                        _ => new HashSet<string>());
                    customerCodes.Add(code);
                }
            }

            // Get set of disabled ESComponentAKAs
            var disabledESComponentAKAs = new HashSet<Tuple<string, string>>();
            query = "SELECT [ESComponentCode], [ESComponentAKA] FROM [DisabledESComponentAKA]";
            using (SQLiteCommand command = new(query, _customerDBConnection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            foreach(var r in reader.Cast<IDataRecord>())
            {
                string code = r.GetString(0).ToUpperInvariant();
                string aka = r.GetString(1).ToUpperInvariant();

                disabledESComponentAKAs.Add(Tuple.Create(code, aka));
            }

            // Add additional mappings using the component data (URS) database
            query = "SELECT [ESComponent].[Code], [ESComponent].[Name], [ESComponentAKA].[Name], [SampleType]"
                    + ", [MatchScoringQuery], [Frequency]"
                    + " FROM [ESComponent] JOIN [ESComponentAKA] ON [ESComponent].[Code] = [ESComponentAKA].[ESComponentCode]"
                    + " LEFT JOIN [AKAFrequency]"
                    + " ON [ESComponent].[Code] = [AKAFrequency].[ESComponentCode]"
                    + " AND [ESComponentAKA].[Name] = [AKAFrequency].[Name]";
            using (SQLiteCommand command = new(query, _componentDataDBConnection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            foreach (var record in reader.Cast<IDataRecord>())
            {
                string testCode = record.GetString(0);
                string officialName = record.GetString(1);
                string aka = record.GetString(2);
                string sampleType = record[3] as string;
                string matchScoringQuery = record[4] as string;
                int frequency = record[5] as int? ?? 0;

                // Skip if this AKA has been disabled by the customer order mapping DB
                if (disabledESComponentAKAs.Contains(Tuple.Create(testCode.ToUpperInvariant(), aka.ToUpperInvariant())))
                {
                    continue;
                }

                // Add this AKA to the map of names to ES codes
                string normalizedAKA = _getNormalizedName(aka);
                HashSet<Tuple<string, int>> codesForAKA = _normalizedNameToESTestCodes.GetOrAdd(normalizedAKA,
                    _ => new HashSet<Tuple<string, int>>());
                codesForAKA.Add(Tuple.Create(testCode, frequency));

                // For all customer codes that are mapped to this ES code, add the ES info to the customer dictionaries
                HashSet<string> customerCodesForTest = null;
                if (_esCodeToCustomerCodes.TryGetValue(testCode, out customerCodesForTest))
                {
                    foreach (var customerCode in customerCodesForTest)
                    {
                        // Add the official ES name to the map of customer test codes to ES names
                        HashSet<string> esNames = _customerCodeToESNames.GetOrAdd(customerCode,
                            _ => new HashSet<string>());
                        esNames.Add(officialName);

                        // Add the sample type if there is one
                        // If there is already a sample type associated with this customer test,
                        // clear if not the same as this one.
                        string existingSampleType;
                        if (_testCodeToSampleType.TryGetValue(customerCode, out existingSampleType)
                            && existingSampleType != sampleType.ToUpperInvariant())
                        {
                            _testCodeToSampleType[customerCode] = null;
                        }
                        else if (!String.IsNullOrEmpty(sampleType))
                        {
                            _testCodeToSampleType[customerCode] = sampleType.ToUpperInvariant();
                        }

                        // Add the match scoring query unless it is empty
                        // if there already is a query and it is not the same as the new one, then
                        // append the new one.
                        string existingMatchScoringQuery;
                        if (_testCodeToMatchScoringQuery.TryGetValue(customerCode, out existingMatchScoringQuery)
                            && existingMatchScoringQuery != matchScoringQuery)
                        {
                            _testCodeToMatchScoringQuery[customerCode] += matchScoringQuery;
                        }
                        else if (!String.IsNullOrEmpty(matchScoringQuery))
                        {
                            _testCodeToMatchScoringQuery[customerCode] = matchScoringQuery;
                        }
                    }
                }
            }
        }

        #endregion Private Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="OrderMappingDBCache"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="OrderMappingDBCache"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="OrderMappingDBCache"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_customerDBConnection != null)
                {
                    _customerDBConnection.Dispose();
                    _customerDBConnection = null;
                }

                if (_componentDataDBConnection != null)
                {
                    _componentDataDBConnection.Dispose();
                    _componentDataDBConnection = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
