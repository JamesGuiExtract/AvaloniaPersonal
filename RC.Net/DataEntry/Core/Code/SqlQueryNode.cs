using Extract.DataEntry.Properties;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Linq;
using System.Text;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> that is to be resolved using an SQL query against the active
    /// database.
    /// </summary>
    internal class SqlQueryNode : CompositeQueryNode
    {
        #region Statics

        /// <summary>
        /// Keep track of the last DB connection; <see cref="_cachedResults"/> needs to be cleared
        /// every time the database is changed.
        /// </summary>
        static DbConnection _lastDBConnection;

        /// <summary>
        /// Provides access to settings in the config file.
        /// </summary>
        static ConfigSettings<Settings> _config = new ConfigSettings<Settings>();

        /// <summary>
        /// Cached SQL query results for frequently used or expensive queries.
        /// </summary>
        static DataCache<string, CachedQueryData<string[]>> _cachedResults =
            new DataCache<string, CachedQueryData<string[]>>(
                _config.Settings.QueryCacheLimit, CachedQueryData<string[]>.GetScore);

        #endregion Statics

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="SqlQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        public SqlQueryNode(IAttribute rootAttribute, DbConnection dbConnection)
            : base(rootAttribute, dbConnection)
        {
            try
            {
                // If the connection has changed since the last SqlQueryNode was created, the static
                // cache needs to be cleared.
                if (_lastDBConnection != dbConnection)
                {
                    _cachedResults.Clear();
                }

                _lastDBConnection = dbConnection;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34488");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Evaluates the query by using the combined result of all child
        /// <see cref="QueryNode"/>s as an SQL query against the active database.
        /// </summary>
        /// <param name="childQueryResults"><see cref="QueryResult"/>s representing the results of
        /// each child <see cref="QueryNode"/>.</param>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        protected override QueryResult Evaluate(IEnumerable<QueryResult> childQueryResults)
        {
            try
            {
                SqlCeConnection sqlCeConnection = DatabaseConnection as SqlCeConnection;

                ExtractException.Assert("ELI26733",
                    "Unable to evaluate query without SQL CE database connection!",
                    sqlCeConnection != null);

                StringBuilder sqlQuery = new StringBuilder();

                // Child query nodes whose results have been parameterized.
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                // Builds a key which uniquely identifies a query that will always return the same
                // result unless the database changes.
                string cacheKey = "";

                // Combine the result of all child queries parameterizing as necessary.
                foreach (QueryResult childQueryResult in childQueryResults)
                {
                    if (childQueryResult.QueryNode.Parameterize)
                    {
                        // If parameterizing, don't add the query result directly, rather add a
                        // parameter name to the query and add the key/value pair to parameters.
                        string key = "@" + parameters.Count.ToString(CultureInfo.InvariantCulture);
                        string value = childQueryResult.ToString();

                        cacheKey += value + ":";
                        parameters[key] = value;
                        sqlQuery.Append(key);
                    }
                    else
                    {
                        sqlQuery.Append(childQueryResult.ToString());
                    }
                }

                // If there are cached results for this query, retrieve them.
                string[] queryResults;
                CachedQueryData<string[]> cachedResults;
                cacheKey += sqlQuery.ToString();
                if (_cachedResults.TryGetData(cacheKey, out cachedResults))
                {
                    queryResults = cachedResults.Data;
                }
                // Otherwise, execute the query and submit the results for caching.
                else
                {
                    DateTime startTime = DateTime.Now;

                    // Create a database command using the query.
                    using (DbCommand dbCommand = DBMethods.CreateDBCommand(
                        sqlCeConnection, sqlQuery.ToString(), parameters))
                    {
                        // Execute the query.
                        queryResults = DBMethods.ExecuteDBQuery(dbCommand, ", ");
                    }

                    // Attempt to cache the results.
                    double executionTime = (DateTime.Now - startTime).TotalMilliseconds;
                    cachedResults = new CachedQueryData<string[]>(queryResults, executionTime);
                    _cachedResults.CacheData(cacheKey, cachedResults);
                }

                return new QueryResult(this, queryResults); ;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI26755");
            }
        }

        #endregion Overrides
    }
}
