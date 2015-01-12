﻿using Extract.Database;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
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
        /// Cached SQL query results for frequently used or expensive queries.
        /// </summary>
        static Dictionary<string, DbConnectionWrapper> _connectionInfo =
            new Dictionary<string, DbConnectionWrapper>();

        #endregion Statics

        #region DbConnectionWrapper

        /// <summary>
        /// Encapsulates a <see cref="DbConnection"/> and its associated cached results.
        /// </summary>
        class DbConnectionWrapper
        {
            /// <summary>
            /// The <see cref="DbConnection"/>.
            /// </summary>
            public DbConnection DbConnection;

            /// <summary>
            /// The cached results associated with <see cref="DbConnection"/>
            /// </summary>
            public DataCache<string, CachedQueryData<string[]>> CachedResults =
                new DataCache<string, CachedQueryData<string[]>>(
                    QueryNode.QueryCacheLimit, CachedQueryData<string[]>.GetScore);

            /// <summary>
            /// Initializes a new instance of the <see cref="DbConnectionWrapper"/> class.
            /// </summary>
            /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
            public DbConnectionWrapper(DbConnection dbConnection)
            {
                DbConnection = dbConnection;
            }
        }

        #endregion DbConnectionWrapper

        #region Fields

        /// <summary>
        /// The <see cref="DbConnectionWrapper"/> currently being used for this query node.
        /// </summary>
        DbConnectionWrapper _currentConnection = null;

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
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) that should be used to
        /// evaluate any SQL queries; The key is the connection name (blank for default connection).
        /// </param>
        public SqlQueryNode(IAttribute rootAttribute, Dictionary<string, DbConnection> dbConnections)
            : base(rootAttribute, dbConnections)
        {
            try
            {
                // [DataEntry:1236]
                // By default, multiple lines from a Composite query will be treated as separate
                // results; when those results are subsequently combined before evaluation, there
                // will be no whitespace where the carriage return was. Since it can be assumed that
                // literal newline chars are not intended to delineate multiple results for an SQL
                // query, allow newline chars to be treated as liternal newline chars.
                TreatNewLinesAsWhiteSpace = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34488");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Loads the <see cref="QueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="namedReferences">A communal collection of named
        /// <see cref="NamedQueryReferences"/>s available to allow referencing of named nodes.</param>
        internal override void LoadFromXml(System.Xml.XmlNode xmlNode,
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                base.LoadFromXml(xmlNode, namedReferences);

                // Look up the appropriate connection for this node.
                XmlAttribute xmlAttribute = xmlNode.Attributes["Connection"];
                string connectionName = (xmlAttribute == null) ? "" : xmlAttribute.Value;

                ExtractException.Assert("ELI37783", "Database must be specified for SQL query nodes.",
                    DatabaseConnections != null && DatabaseConnections.ContainsKey(connectionName));

                // Look for an existing DbConnectionWrapper under this name.
                if (_connectionInfo.TryGetValue(connectionName, out _currentConnection))
                {
                    // If the connection itself has changed since the last time DbConnectionWrapper
                    // was used (such as if an SQL CE OrderMappingDB has been updated), the static
                    // cache needs to be cleared.
                    if (_currentConnection.DbConnection != DatabaseConnections[connectionName])
                    {
                        _currentConnection.CachedResults.Clear();
                        _currentConnection.DbConnection = DatabaseConnections[connectionName];
                    }
                }
                else
                {
                    // A DbConnectionWrapper needs to be created for this name.
                    _currentConnection = new DbConnectionWrapper(DatabaseConnections[connectionName]);
                    _connectionInfo[connectionName] = _currentConnection;
                }

                string dbType = _currentConnection.DbConnection.GetType().ToString();
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
                throw ex.AsExtract("ELI37797");
            }
        }

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
                    _currentConnection != null && _currentConnection.DbConnection != null);

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
                    _currentConnection.CachedResults.Clear();
                }

                // If there are cached results for this query, retrieve them.
                string[] queryResults;
                CachedQueryData<string[]> cachedResults;
                cacheKey += sqlQuery.ToString();
                if (_currentConnection.CachedResults.TryGetData(cacheKey, out cachedResults))
                {
                    queryResults = cachedResults.Data;
                }
                // Otherwise, execute the query and submit the results for caching.
                else if (!string.IsNullOrWhiteSpace(sqlQuery.ToString()))
                {
                    DateTime startTime = DateTime.Now;

                    // Execute the query.
                    queryResults = DBMethods.GetQueryResultsAsStringArray(
                        _currentConnection.DbConnection, sqlQuery.ToString(), parameters, ", ");

                    if (AllowCaching && !FlushCache)
                    {
                        // Attempt to cache the results.
                        double executionTime = (DateTime.Now - startTime).TotalMilliseconds;
                        cachedResults = new CachedQueryData<string[]>(queryResults, executionTime);
                        _currentConnection.CachedResults.CacheData(cacheKey, cachedResults);
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
