using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// A class to hold the order code, name, and epic code information for a particular
    /// lab order.  This class will also maintain a collection of mandatory and
    /// non-mandatory tests along with helper methods for matching collections of
    /// <see cref="LabTest"/> to this order.
    /// </summary>
    internal class LabOrder
    {
        #region Fields

        /// <summary>
        /// The order code for this lab order
        /// </summary>
        readonly string _orderCode;

        /// <summary>
        /// The order name for this lab lorder
        /// </summary>
        readonly string _orderName;

        /// <summary>
        /// The epic code for this lab order
        /// </summary>
        readonly string _epicCode;

        /// <summary>
        /// The tiebreaker string to use in deciding which order to map to in phase 2 when the
        /// number of combined groups for 2 different orders is the same.
        /// </summary>
        readonly string _tieBreakerString;

        /// <summary>
        /// A collection of mandatory tests mapping all possible test names
        /// (the official and alternate names) to their possible associated test codes.
        /// </summary>
        readonly Dictionary<string, List<string>> _mandatoryTests
            = new Dictionary<string, List<string>>();

        /// <summary>
        /// A collection of non-mandatory tests mapping all possible test names
        /// (the official and alternate names) to their their possible associated test codes.
        /// </summary>
        readonly Dictionary<string, List<string>> _otherTests
            = new Dictionary<string, List<string>>();

        /// <summary>
        /// A set containing the test codes for all mandatory tests for this order
        /// </summary>
        readonly HashSet<string> _mandatoryTestCodes = new HashSet<string>();

        /// <summary>
        /// A set containing the test codes for all non-mandatory tests for this order
        /// </summary>
        readonly HashSet<string> _otherTestCodes = new HashSet<string>();

        /// <summary>
        /// A set containing all the test codes for this order (mandatory and non-mandatory)
        /// </summary>
        readonly HashSet<string> _allTestCodes = new HashSet<string>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabOrder"/> class.
        /// </summary>
        /// <param name="orderCode">The order code.</param>
        /// <param name="orderName">The order name.</param>
        /// <param name="epicCode">The epic code.</param>
        /// <param name="tieBreakerString">The tiebreaker string to use in deciding which order to
        /// map to in phase 2 when the number of combined groups for 2 different orders is the same.
        /// </param>
        /// <param name="dbConnection">The database connection to use to fill the collections
        /// of tests.</param>
        public LabOrder(string orderCode, string orderName, string epicCode,
            string tieBreakerString, SqlCeConnection dbConnection)
        {
            _orderCode = orderCode.ToUpperInvariant();
            _orderName = orderName.ToUpperInvariant();
            _epicCode = epicCode.ToUpperInvariant();
            _tieBreakerString = tieBreakerString.ToUpperInvariant();

            // Fill the test collections
            FillMandatoryCollection(dbConnection);
            FillOtherCollection(dbConnection);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Fills the mandatory test collection for this order.
        /// </summary>
        /// <param name="dbConnection">The database connection to use.</param>
        void FillMandatoryCollection(SqlCeConnection dbConnection)
        {
            // Get the list of mandatory tests
            List<string> mandatoryTestCodes = GetTestCodes(dbConnection, true);
            foreach (string testCode in mandatoryTestCodes)
            {
                // Add the test code to the mandatory test code collection
                _mandatoryTestCodes.Add(testCode);

                // Add the test code to the all test code collection
                _allTestCodes.Add(testCode);

                // Get all names for this test code
                List<string> testNames = GetTestNames(dbConnection, testCode);
                foreach (string testName in testNames)
                {
                    // Add each name for this test code to the mandatory collection
                    List<string> testCodes = null;
                    if (!_mandatoryTests.TryGetValue(testName, out testCodes))
                    {
                        testCodes = new List<string>();
                        _mandatoryTests[testName] = testCodes;
                    }

                    testCodes.Add(testCode);
                }
            }
        }

        /// <summary>
        /// Fills the other test collection for this order.
        /// </summary>
        /// <param name="dbConnection">The database connection to use.</param>
        void FillOtherCollection(SqlCeConnection dbConnection)
        {
            // Get the list of other tests
            List<string> otherTestCodes = GetTestCodes(dbConnection, false);
            foreach (string testCode in otherTestCodes)
            {
                // Add the test code to the other test code collection
                _otherTestCodes.Add(testCode);

                // Add the test code to the all test code collection
                _allTestCodes.Add(testCode);

                // Get all names for this test code
                List<string> testNames = GetTestNames(dbConnection, testCode);
                foreach (string testName in testNames)
                {
                    // Add each name for this test code to the other collection
                    List<string> testCodes;
                    if (!_otherTests.TryGetValue(testName, out testCodes))
                    {
                        testCodes = new List<string>();
                        _otherTests[testName] = testCodes;
                    }

                    testCodes.Add(testCode);
                }
            }
        }

        /// <summary>
        /// Gets a collection of either mandatory or non-mandatory tests for this order.
        /// </summary>
        /// <param name="dbConnection">The database connection to use to get the test codes.</param>
        /// <param name="mandatory">Whether to get the mandatory tests or not.</param>
        /// <returns>A collection of tests codes.</returns>
        List<string> GetTestCodes(SqlCeConnection dbConnection, bool mandatory)
        {
            // Query to get all test codes
            string query = "SELECT [TestCode] FROM [LabOrderTest] WHERE [OrderCode] = '"
                + _orderCode.Replace("'", "''") + "' AND [Mandatory] = " + (mandatory ? "1" : "0");

            List<string> testCodes = new List<string>();
            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        testCodes.Add(reader.GetString(0).ToUpperInvariant());
                    }
                }
            }

            return testCodes;
        }

        /// <summary>
        /// Gets the list of all possible names for this test (the official and alternate names).
        /// </summary>
        /// <param name="dbConnection">The database connection to use.</param>
        /// <param name="testCode">The test code to find the names for.</param>
        /// <returns>A collection of all possible names for the specified test code.</returns>
        static List<string> GetTestNames(SqlCeConnection dbConnection, string testCode)
        {
            // Escape the single quote
            testCode = testCode.Replace("'", "''");

            string query = "SELECT [Name] FROM [AlternateTestName] WHERE [TestCode] = '"
                + testCode + "'";

            // Get the alternate names
            List<string> testNames = new List<string>();
            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        testNames.Add(reader.GetString(0).ToUpperInvariant());
                    }
                }
            }

            // Get the official name
            query = "SELECT [OfficialName] FROM [LabTest] WHERE [TestCode] = '"
                + testCode + "'";
            using (SqlCeCommand command = new SqlCeCommand(query, dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        testNames.Add(reader.GetString(0).ToUpperInvariant());
                    }
                }
            }

            return testNames;
        }

        /// <summary>
        /// Checks if a collection of <see cref="LabTest"/> contains all of the mandatory
        /// tests for this lab order.
        /// </summary>
        /// <param name="tests">The collection of tests to check.</param>
        /// <returns><see langword="true"/> if <paramref name="tests"/> contained
        /// all of the mandatory tests for this order; <see langword="false"/> if
        /// <paramref name="tests"/> does not contain all mandatory tests.</returns>
        public bool ContainsAllMandatoryTests(IEnumerable<LabTest> tests)
        {
            bool containsAllMandtory = 
                TestMapper.AllMandatoryTestsExist(tests, _mandatoryTestCodes, _mandatoryTests);

            return containsAllMandtory;
        }

        /// <summary>
        /// Builds a collection of all <see cref="LabTest"/> that match this order
        /// from the provided list of tests.  This method does not check if all mandatory
        /// match.
        /// <para><b>Note:</b></para>
        /// This method will modify the <see cref="LabTest.TestCode"/> value to contain
        /// the proper test code for the matching test.
        /// </summary>
        /// <param name="tests">The collection of <see cref="LabTest"/> to check for matching
        /// tests.</param>
        /// <returns>A subset of <paramref name="tests"/> that match this order.</returns>
        public List<LabTest> GetMatchingTests(IEnumerable<LabTest> tests)
        {
            List<LabTest> matchingTests = TestMapper.FindBestMapping(
                tests, _mandatoryTestCodes, _allTestCodes, _mandatoryTests.Union(_otherTests));

            return matchingTests;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the order code for this order.
        /// </summary>
        public string OrderCode
        {
            get
            {
                return _orderCode;
            }
        }

        /// <summary>
        /// Gets the order name for this order.
        /// </summary>
        public string OrderName
        {
            get
            {
                return _orderName;
            }
        }

        /// <summary>
        /// Gets the epic code for this order.
        /// </summary>
        public string EpicCode
        {
            get
            {
                return _epicCode;
            }
        }

        /// <summary>
        /// Gets the tiebreaker string to use in deciding which order to map to in phase 2 when the
        /// number of combined groups for 2 different orders is the same.
        /// </summary>
        public string TieBreakerString
        {
            get
            {
                return _tieBreakerString;
            }
        }

        #endregion Properties
    }
}
