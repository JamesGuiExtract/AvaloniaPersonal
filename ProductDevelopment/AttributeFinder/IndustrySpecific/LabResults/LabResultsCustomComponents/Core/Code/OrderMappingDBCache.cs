using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
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
        static readonly int _DB_CONNECTION_RETRIES = 5;

        #endregion Constants

        #region Fields

        SqlCeConnection _dbConnection = null;

        /// <summary>
        /// For each db cache instance, keeps track of the local database copy to use.
        /// </summary>
        TemporaryFileCopyManager _localDatabaseCopyManager;

        /// <summary>
        /// The name of the database file to use for order mapping.
        /// </summary>
        string _databaseFile;

        /// <summary>
        /// Map of names, official or aka, to the order codes of the orders they could be a part of.
        /// This is used for memoization by the <see cref="GetPotentialOrderCodes"/> method
        /// </summary>
        Dictionary<string, ReadOnlyCollection<string>> _nameToPotentialOrderCodesCache;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The underlying connection to the order mapping database
        /// </summary>
        public SqlCeConnection DBConnection
        {
            get
            {
                return _dbConnection;
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
            _databaseFile = databaseFile;

            _localDatabaseCopyManager = new TemporaryFileCopyManager();

            _nameToPotentialOrderCodesCache = new Dictionary<string, ReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);

            // Get a database connection for processing (creating a local copy of the database).
            _dbConnection = GetDatabaseConnection(pDoc);

            // Open the database connection
            _dbConnection.Open();
        }
        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Gets the list of potential order codes for the specified test. Uses cached values if available.
        /// </summary>
        /// <param name="testName">The test to get the order codes for.</param>
        /// <returns>A list of potential order codes (or an empty list if no potential orders).
        /// </returns>
        public ReadOnlyCollection<string> GetPotentialOrderCodes(string testName)
        {
            ReadOnlyCollection<string> potentialOrderCodes;

            // Escape the single quote
            testName = testName.Replace("'", "''");

            if (_nameToPotentialOrderCodesCache.TryGetValue(testName, out potentialOrderCodes))
            {
                return potentialOrderCodes;
            }

            // Create a set to hold the potential codes
            HashSet<string> orderCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Query the official name table for potential order codes
            // Use LabOrder.Code instead of LabOrderTest.OrderCode because these two might not be strictly equal
            // because of the way that SQL handles trailing spaces
            // https://extract.atlassian.net/browse/ISSUE-12073
            string query = "SELECT DISTINCT [Code] FROM [LabOrder] JOIN [LabOrderTest] ON [Code] = [OrderCode] JOIN "
                + "[LabTest] ON [LabOrderTest].[TestCode] = [LabTest].[TestCode] "
                + "WHERE [LabTest].[OfficialName] = '" + testName + "'";
            using (SqlCeCommand command = new SqlCeCommand(query, _dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string code = reader.GetString(0);
                        if (!orderCodes.Contains(code))
                        {
                            orderCodes.Add(code);
                        }
                    }
                }
            }

            // Query the alternate test table for potential order codes
            // Use LabOrder.Code instead of LabOrderTest.OrderCode because these two might not be strictly equal
            // because of the way that SQL handles trailing spaces
            // https://extract.atlassian.net/browse/ISSUE-12073
            query = "SELECT DISTINCT [Code] FROM [LabOrder] JOIN [LabOrderTest] ON [Code] = [OrderCode] JOIN "
                + "[AlternateTestName] ON [LabOrderTest].[TestCode] = [AlternateTestName].[TestCode] "
                + "WHERE [AlternateTestName].[Name] = '" + testName + "'";
            using (SqlCeCommand command = new SqlCeCommand(query, _dbConnection))
            {
                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string code = reader.GetString(0);
                        if (!orderCodes.Contains(code))
                        {
                            orderCodes.Add(code);
                        }
                    }
                }
            }

            potentialOrderCodes = new ReadOnlyCollection<string>(orderCodes.ToList<string>());
            _nameToPotentialOrderCodesCache[testName] = potentialOrderCodes;

            return potentialOrderCodes;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the database connection to use for processing, creating a local copy if necessary.
        /// </summary>
        /// <param name="pDoc">The document object.</param>
        /// <returns>The <see cref="SqlCeConnection"/> to use for processing.</returns>
        SqlCeConnection GetDatabaseConnection(AFDocument pDoc)
        {
            try
            {
                // Expand the tags in the database file name
                AFUtility afUtility = new AFUtility();
                string databaseFile = afUtility.ExpandTagsAndFunctions(_databaseFile, pDoc);

                // Check for the database files existence
                if (!File.Exists(databaseFile))
                {
                    ExtractException ee = new ExtractException("ELI26170",
                        "Database file does not exist!");
                    ee.AddDebugData("Database File Name", databaseFile, false);
                    throw ee;
                }

                // [DataEntry:399, 688, 986]
                // Whether or not the file is accessed via a network share, retrieve and use a local
                // temp copy of the reference database file. Though multiple connections are allowed
                // to a local file, the connections cannot see each other's changes.
                string connectionString = "Data Source='" +
                    _localDatabaseCopyManager.GetCurrentTemporaryFileName(databaseFile, this, true) + "';";

                // Try to open the database connection, if there is a sqlce exception,
                // just increment retry count, sleep, and try again
                int retryCount = 0;
                Exception tempEx = null;
                SqlCeConnection dbConnection = null;
                while (dbConnection == null && retryCount < _DB_CONNECTION_RETRIES)
                {
                    try
                    {
                        dbConnection = new SqlCeConnection(connectionString);
                    }
                    catch (SqlCeException ex)
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
                ee.AddDebugData("Database", _databaseFile, false);
                throw ee;
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
                if (_dbConnection != null)
                {
                    _dbConnection.Dispose();
                    _dbConnection = null;
                }

                if (_localDatabaseCopyManager != null)
                {
                    _localDatabaseCopyManager.Dispose();
                    _localDatabaseCopyManager = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
