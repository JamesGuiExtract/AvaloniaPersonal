using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// A class to hold the order code, name, and order code information for a particular
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
        /// The order name for this lab order
        /// </summary>
        readonly string _orderName;

        /// <summary>
        /// The tiebreaker string to use in deciding which order to map to in phase 2 when the
        /// number of combined groups for 2 different orders is the same.
        /// </summary>
        readonly string _tieBreakerString;

        /// <summary>
        /// A set containing the test codes for all mandatory tests for this order
        /// </summary>
        readonly HashSet<string> _mandatoryTestCodes
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A set containing the test codes for all non-mandatory tests for this order
        /// </summary>
        readonly HashSet<string> _otherTestCodes
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A set containing all the test codes for this order (mandatory and non-mandatory)
        /// </summary>
        readonly HashSet<string> _allTestCodes
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The number of tests that this order needs to be considered filled
        /// </summary>
        readonly int _filledRequirement;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabOrder"/> class.
        /// </summary>
        /// <param name="orderCode">The order code.</param>
        /// <param name="orderName">The order name.</param>
        /// <param name="tieBreakerString">The tiebreaker string to use in deciding which order to
        /// map to in phase 2 when the number of combined groups for 2 different orders is the same.
        /// </param>
        /// <param name="filledRequirement">The number of tests required to consider this order filled.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use to fill the collections
        /// of tests.</param>
        public LabOrder(string orderCode, string orderName,
            string tieBreakerString, OrderMappingDBCache dbCache, int filledRequirement)
        {
            _orderCode = orderCode;
            _orderName = orderName;
            _tieBreakerString = tieBreakerString;
            _filledRequirement = filledRequirement;

            // Fill the test collections
            FillMandatoryCollection(dbCache);
            FillOtherCollection(dbCache);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Fills the mandatory test collection for this order.
        /// </summary>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use.</param>
        void FillMandatoryCollection(OrderMappingDBCache dbCache)
        {
            // Get the list of mandatory tests
            List<string> mandatoryTestCodes = GetTestCodes(dbCache, true);
            foreach (string testCode in mandatoryTestCodes)
            {
                // Add the test code to the mandatory test code collection
                _mandatoryTestCodes.Add(testCode);

                // Add the test code to the all test code collection
                _allTestCodes.Add(testCode);
            }
        }

        /// <summary>
        /// Fills the other test collection for this order.
        /// </summary>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use.</param>
        void FillOtherCollection(OrderMappingDBCache dbCache)
        {
            // Get the list of other tests
            List<string> otherTestCodes = GetTestCodes(dbCache, false);
            foreach (string testCode in otherTestCodes)
            {
                // Add the test code to the other test code collection
                _otherTestCodes.Add(testCode);

                // Add the test code to the all test code collection
                _allTestCodes.Add(testCode);
            }
        }

        /// <summary>
        /// Gets a collection of either mandatory or non-mandatory tests for this order.
        /// </summary>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use to get the test codes.</param>
        /// <param name="mandatory">Whether to get the mandatory tests or not.</param>
        /// <returns>A collection of tests codes.</returns>
        List<string> GetTestCodes(OrderMappingDBCache dbCache, bool mandatory)
        {
            // Query to get all test codes
            // Use LabTest.TestCode instead of LabOrderTest.TestCode because these two might not be strictly equal
            // because of the way that SQL handles trailing spaces
            // https://extract.atlassian.net/browse/ISSUE-12073
            string query = "SELECT [LabTest].[TestCode] FROM [LabTest] JOIN [LabOrderTest] ON "
                + " [LabTest].[TestCode] = [LabOrderTest].[TestCode] WHERE [OrderCode] = '"
                + _orderCode.Replace("'", "''") + "' AND [Mandatory] = " + (mandatory ? "1" : "0");

            List<string> testCodes = new List<string>();
            using (SQLiteCommand command = new(query, dbCache.DBConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        testCodes.Add(reader.GetString(0));
                    }
                }
            }

            return testCodes;
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
            return _mandatoryTestCodes.IsSubsetOf(tests.Select(test => test.TestCode));
        }

        /// <summary>
        /// Builds a collection of all <see cref="LabTest"/> that match this order
        /// from the provided list of tests.
        /// <para><b>Note:</b></para>
        /// This method will modify the <see cref="LabTest.TestCode"/> value to contain
        /// the proper test code for the matching test.
        /// </summary>
        /// <param name="tests">The collection of <see cref="LabTest"/> to check for matching
        /// tests.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for mapping.</param>
        /// <param name="finalPass">Whether this is the final pass of the algorithm</param>
        /// <param name="requireMandatory">Whether mandatory tests are required</param>
        /// <returns>A subset of <paramref name="tests"/> that match this order.</returns>
        public List<LabTest> GetMatchingTests(IEnumerable<LabTest> tests, OrderMappingDBCache dbCache,
            bool finalPass, bool requireMandatory)
        {
            TestMapper possibleMappings =
                    new TestMapper(tests, _mandatoryTestCodes, _otherTestCodes, dbCache, finalPass);

            return TestMapper.FindMapping(possibleMappings, requireMandatory, finalPass);
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

        /// <summary>
        /// Gets the number of tests that this order needs to be considered 'filled.'
        /// </summary>
        public int FilledRequirement
        {
            get
            {
                return _filledRequirement;
            }
        }

        /// <summary>
        /// Gets the maximum number of tests this order could contain
        /// </summary>
        public int MaxSize
        {
            get
            {
                return _mandatoryTestCodes.Count + _otherTestCodes.Count;
            }
        }
        #endregion Properties
    }
}
