using Extract.Database;
using Extract.DataEntry.Properties;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
        /// Cached SQL query results for frequently used or expensive queries.
        /// </summary>
        static DataCache<string, CachedQueryData<string[]>> _cachedResults =
            new DataCache<string, CachedQueryData<string[]>>(
                QueryNode.QueryCacheLimit, CachedQueryData<string[]>.GetScore);

        #endregion Statics

        #region Fields

        /// <summary>
        /// A string that indicates the following element in a query is a parameter.
        /// </summary>
        string _parameterMarker;

        /// <summary>
        /// Indicates whether named parameters are supported by the current database type.
        /// </summary>
        bool _useNamedParameters;

        #endregion Fields

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

                string dbType = dbConnection.GetType().ToString();
                if (dbType.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _parameterMarker = ":";  // Oracle
                    _useNamedParameters = true;
                }
                else if (dbType.IndexOf("Ole", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _parameterMarker = "?";  // MS Ole
                    _useNamedParameters = false;
                }
                else
                {
                    _parameterMarker = "@";   // MS SQL & SQL CE
                    _useNamedParameters = true;
                }
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
                ExtractException.Assert("ELI26733",
                    "Unable to evaluate query without database connection!",
                    DatabaseConnection != null);

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
                        string key = _parameterMarker +
                            parameters.Count.ToString(CultureInfo.InvariantCulture);
                        string value = childQueryResult.ToString();

                        cacheKey += value + ":";
                        parameters[key] = value;
                        sqlQuery.Append(_useNamedParameters ? key : _parameterMarker);
                    }
                    else
                    {
                        sqlQuery.Append(childQueryResult.ToString());
                    }
                }

                if (FlushCache)
                {
                    _cachedResults.Clear();
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
                else if (!string.IsNullOrWhiteSpace(sqlQuery.ToString()))
                {
                    DateTime startTime = DateTime.Now;

                    // Create a database command using the query.
                    using (DbCommand dbCommand = DBMethods.CreateDBCommand(
                        DatabaseConnection, sqlQuery.ToString(), parameters))
                    {
                        // Execute the query.
                        queryResults = DBMethods.ExecuteDBQuery(dbCommand, ", ");
                    }

                    if (AllowCaching && !FlushCache)
                    {
                        // Attempt to cache the results.
                        double executionTime = (DateTime.Now - startTime).TotalMilliseconds;
                        cachedResults = new CachedQueryData<string[]>(queryResults, executionTime);
                        _cachedResults.CacheData(cacheKey, cachedResults);
                    }
                }
                else
                {
                    // If the query is blank, just return an emptry result (not an error condition).
                    queryResults = new string[0];
                }

                return new QueryResult(this, queryResults);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI26755");
            }
        }

        #endregion Overrides
    }
}
