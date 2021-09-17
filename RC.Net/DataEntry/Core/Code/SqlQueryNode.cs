using Extract.Database;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
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
        /// Cached SQL query results for frequently used or expensive queries (per DB connection string).
        /// </summary>
        [ThreadStatic]
        static Dictionary<string, DataCache<string, CachedQueryData<string[]>>> _threadCacheManager = 
            new Dictionary<string, DataCache<string, CachedQueryData<string[]>>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// While ordinarily AttributeStatusInfo environments will be separate for every thread and, thus, we
        /// want data caches to be thread specific, in some contexts such as loading many documents for in the
        /// paginination UI via UpdateDocumentStatusThread, we do want to share the cache across all threads in
        /// the process. _processCacheManager will be used for this shared cache data.
        /// </summary>
        static Dictionary<string, DataCache<string, CachedQueryData<string[]>>> _processCacheManager = 
            new Dictionary<string, DataCache<string, CachedQueryData<string[]>>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Synchronizes access to shared cache (_processCacheManager)
        /// </summary>
        static object _lock = new object();

        #endregion Statics

        #region Fields

        /// <summary>
        /// The <see cref="DataCache"/> currently being used for this query node.
        /// </summary>
        DataCache<string, CachedQueryData<string[]>> _currentCache = null;

        /// <summary>
        /// The current connection
        /// </summary>
        DbConnection _currentConnection;

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
        /// Static initializer for the <see cref="SqlQueryNode"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SqlQueryNode()
        {
            try
            {
                AttributeStatusInfo.ClearedProcessWideCache += (o, e) =>
                    {
                        lock (_lock)
                        {
                            try
                            {
                                _processCacheManager = null;
                                DatabaseConnectionInfo.ResetSharedDatabaseCopies();
                            }
                            catch (Exception ex)
                            {
                                ex.ExtractLog("ELI45622");
                            }
                        }
                    };

                Application.ThreadExit += (o, e) =>
                {
                // https://extract.atlassian.net/browse/ISSUE-12987
                // dotMemory indicates leaks related to the _connectionInfo dictionary when
                // processing is stopped and restarted (causing a new UI thread to be created). Even
                // though the cached data for each DbConnectionWrapper is cleared when the UI thread
                // exits, the dictionary still has space reserved for cached data. If the thread is
                // exiting, remove references to all DbConnectionWrapper instances.
                _threadCacheManager?.Clear();
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45621");
            }
        }

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
                // query, allow newline chars to be treated as literal newline chars.
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

                lock (_lock)
                {
                    var dataCache = AttributeStatusInfo.ProcessWideDataCache
                        ? _processCacheManager ?? (_processCacheManager = new Dictionary<string, DataCache<string, CachedQueryData<string[]>>>())
                        : _threadCacheManager ?? (_threadCacheManager = new Dictionary<string, DataCache<string, CachedQueryData<string[]>>>());

                    _currentConnection = DatabaseConnections[connectionName];
                    string connectionString = _currentConnection.ConnectionString;

                    // Look for an existing cache for this connectionString.
                    if (!dataCache.TryGetValue(connectionString, out _currentCache))
                    {
                        // A new DataCache needs to be created for this name.
                        _currentCache = new DataCache<string, CachedQueryData<string[]>>(
                            QueryNode.QueryCacheLimit, CachedQueryData<string[]>.GetScore);
                        dataCache[connectionString] = _currentCache;
                    }
                }

                string dbType = _currentConnection.GetType().ToString();
                if (dbType.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _parameterMarker = ":";  // Oracle
                    _useNamedParameters = true;
                }
                else if (dbType.IndexOf("OleDb", StringComparison.OrdinalIgnoreCase) >= 0)
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
                    _currentCache != null && _currentConnection != null);

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

                if (FlushCache && !AttributeStatusInfo.ProcessWideDataCache)
                {
                    _currentCache.Clear();
                }

                // If there are cached results for this query, retrieve them.
                string[] queryResults;
                CachedQueryData<string[]> cachedResults;
                cacheKey += sqlQuery.ToString();
                if (_currentCache.TryGetData(cacheKey, out cachedResults))
                {
                    queryResults = cachedResults.Data;
                }
                // Otherwise, execute the query and submit the results for caching.
                else if (!string.IsNullOrWhiteSpace(sqlQuery.ToString()))
                {
                    // Lock here to prevent timeout exceptions
                    // https://extract.atlassian.net/browse/ISSUE-16590
                    lock (_lock)
                    {
                        DateTime startTime = DateTime.Now;

                        // Execute the query.
                        queryResults = DBMethods.GetQueryResultsAsStringArray(
                            _currentConnection, sqlQuery.ToString(), parameters, ", ", SplitCsv);

                        if (AllowCaching && !FlushCache)
                        {
                            // Attempt to cache the results.
                            double executionTime = (DateTime.Now - startTime).TotalMilliseconds;
                            cachedResults = new CachedQueryData<string[]>(queryResults, executionTime);
                            _currentCache.CacheData(cacheKey, cachedResults);
                        }
                    }
                }
                else
                {
                    // If the query is blank, just return an empty result (not an error condition).
                    queryResults = new string[0];
                }

                return new QueryResult(this, queryResults);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI26755");
            }
        }

        /// <summary>
        /// Handles the query cache cleared event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void HandleQueryCacheCleared(object sender, EventArgs e)
        {
            base.HandleQueryCacheCleared(sender, e);

            // Clear the db wrapper's cache too
            if (_currentCache != null && !AttributeStatusInfo.ProcessWideDataCache)
            {
                _currentCache.Clear();
            }
        }

        #endregion Overrides
    }
}
