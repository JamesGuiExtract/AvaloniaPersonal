using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    public abstract partial class SqlAppRoleConnection
    {
        // Manages a pool of SQL connections that can be re-used for efficiency. Doing so
        // avoids the need to re-authenticate SQL connections via an SQL application role.
        // https://extract.atlassian.net/browse/ISSUE-17845
        private class ConnectionPool : IDisposable
        {
            /// How long connections will sit in the pool unused before being closed out.
            public static TimeSpan ConnectionPoolTimeout { get; set; } = TimeSpan.FromMinutes(5);

            /// Each combination of connection string and role should have a separate pool of
            /// connections that have been authenticated for the specified role.
            static ConcurrentDictionary<(string ConnectionString, string RoleName),
                ConcurrentStack<ConnectionPool>> _connectionPool = new();

            /// The key indicating the connection pool to be used for this instance.
            (string ConnectionString, string RoleName) _poolKey;

            /// The role under which this instance's connection is authenticated to the DB.
            string _roleName;

            /// 1 indicates the connection is available for use; otherwise the connection has already been
            /// claimed or timed-out.
            int _isAvailable = 1;

            // The underlying SqlConnection that has been authenticated under _roleName
            SqlConnection _sqlConnection;

            // Used to cancel the ManageConnectionTimeout in the case a connection is being claimed.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>")]
            CancellationTokenSource _timeoutCanceller;

            /// Private constructor; creates an instance used to manage the specified SqlAppRoleConnection's
            /// underlying SqlConnection in a connection pool.
            ConnectionPool(SqlAppRoleConnection sqlAppRoleConnection)
            {
                ExtractException.Assert("ELI52976", "Connection not eligable to be pooled",
                    IsEligibleForPool(sqlAppRoleConnection));

                _sqlConnection = sqlAppRoleConnection.BaseSqlConnection;
                _roleName = sqlAppRoleConnection.RoleName;
                _poolKey = (_sqlConnection.ConnectionString, _roleName);
            }

            /// Attempts to find an available connection from the pool. If any are available, the most recently
            /// used connection will be the connection returned.
            /// <param name="sqlAppRoleConnection">The SqlAppRoleConnection any available connection should be
            /// used for.
            /// NOTE: The RoleName of this parameter will be used to identify an appropriate connection.</param>
            /// <param name="connectionString">The connection string an available connection should match.</param>
            public static bool TryGetPoolConnection(SqlAppRoleConnection sqlAppRoleConnection, string connectionString)
            {
                try
                {
                    if (UseApplicationRoles
                        && SqlAppRoleConnection.EnableConnectionPooling
                        && !string.IsNullOrEmpty(sqlAppRoleConnection.RoleName))
                    {
                        var pooledConnections = _connectionPool.GetOrAdd(
                            (connectionString, sqlAppRoleConnection.RoleName),
                                _ => new ConcurrentStack<ConnectionPool>());

                        while (pooledConnections.TryPop(out var pooledConnection))
                        {
                            if (pooledConnection.TakeIfAvailable())
                            {
                                var baseConnection = pooledConnection._sqlConnection;

                                pooledConnection._timeoutCanceller?.Cancel();
                                pooledConnection.Dispose();

                                if (baseConnection?.State == ConnectionState.Open)
                                {
                                    sqlAppRoleConnection.BaseSqlConnection = baseConnection;
                                    sqlAppRoleConnection._isRoleAssigned = true;

                                    return true;
                                }

                                // If for whatever reason, the connection is not longer open, discard and
                                // continue looking for another available connection.
                                baseConnection?.Dispose();
                            }
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI52991");
                }
            }

            /// Make the base SqlConnection for sqlAppRoleConnection available in the application
            /// pool to the next SqlAppRoleConnection instance that needs a connection sharing the
            /// same connection string and role
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
                Justification = "ConnectionPool instances are tracked and disposed beyond the scope of this method.")]
            public static bool TryAddPoolConnection(SqlAppRoleConnection sqlAppRoleConnection)
            {
                try
                {
                    if (IsEligibleForPool(sqlAppRoleConnection))
                    {
                        var pooledConnection = new ConnectionPool(sqlAppRoleConnection);

                        var pooledConnections = _connectionPool.GetOrAdd(
                            (sqlAppRoleConnection.ConnectionString, sqlAppRoleConnection.RoleName),
                                _ => new ConcurrentStack<ConnectionPool>());

                        sqlAppRoleConnection.BaseSqlConnection = null;

                        pooledConnections.Push(pooledConnection);

                        pooledConnection.ManageConnectionTimeout();

                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI52992");
                }
            }

            /// Determines if the specified connection is elibible to be pooled based on
            /// current settings and 
            static bool IsEligibleForPool(SqlAppRoleConnection sqlAppRoleConnection)
            {
                return
                    SqlAppRoleConnection.UseApplicationRoles
                    && SqlAppRoleConnection.EnableConnectionPooling
                    && !string.IsNullOrWhiteSpace(sqlAppRoleConnection.RoleName)
                    && sqlAppRoleConnection._isRoleAssigned
                    && sqlAppRoleConnection._ownsBaseConnection
                    && sqlAppRoleConnection.BaseSqlConnection?.State == ConnectionState.Open;
            }

            /// Claim the SqlConnection for this instance if it has not already been claimed.
            bool TakeIfAvailable()
            {
                return Interlocked.CompareExchange(ref _isAvailable, 0, 1) == 1;
            }

            /// Ensure the connection is closed and removed from the pool after
            /// ConnectionPoolTimeout if not claimed before then.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2008:Do not create tasks without passing a TaskScheduler", Justification = "<Pending>")]
            void ManageConnectionTimeout()
            {
                _timeoutCanceller = new();
                _ = Task.Run(async () =>
                {
                    await Task.Delay(ConnectionPoolTimeout, _timeoutCanceller.Token).ConfigureAwait(false);
                    ExpireConnection();
                    Dispose();
                }
                , _timeoutCanceller.Token)
                .ContinueWith(task =>
                {
                    task.Exception.ExtractLog("ELI52986");
                    _timeoutCanceller?.Dispose();
                }
                , TaskContinuationOptions.OnlyOnFaulted);
            }

            /// <summary>
            /// Close and remove the connection from the pool.
            /// </summary>
            void ExpireConnection()
            {
                if (TakeIfAvailable())
                {
                    _sqlConnection?.Dispose();
                    _sqlConnection = null;

                    ClearExpiredConnectionsFromPool(_poolKey);
                }
            }

            /// When connections expire per ConnectionPoolTimeout, this method removes any such 
            /// expired connection from the the _connectionPool. (ConcurrentStack does not provide
            /// a method to remove a specific instance).
            static void ClearExpiredConnectionsFromPool((string ConnectionString, string RoleName) poolKey)
            {
                if (_connectionPool.TryGetValue(poolKey, out var pooledConnections)
                    && !pooledConnections.IsEmpty)
                {
                    var tempArray = new ConnectionPool[pooledConnections.Count];
                    var count = pooledConnections.TryPopRange(tempArray);
                    tempArray = tempArray
                        .Where(c => c._isAvailable > 0)
                        .Reverse() // Needed to preserve original order when re-added via PushRange.
                        .ToArray();

                    if (tempArray.Length > 0)
                    {
                        pooledConnections.PushRange(tempArray);
                    }
                }
            }

            #region IDisposable Support

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    try
                    {
                        // Only dispose of the connection if it was not otherwise claimed.
                        if (TakeIfAvailable())
                        {
                            _sqlConnection?.Dispose();
                            _sqlConnection = null;
                        }
                    }
                    catch { }
                }
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion IDisposable
        }
    }
}
