using Extract.Licensing;
using Extract.Utilities;
using Microsoft.Data.ConnectionUI;
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Database
{
    /// <summary>
    /// Represents the information necessary to open a connection to a database that has been
    /// configured with the <see cref="DataConnectionDialog"/>.
    /// <para><b>NOTE</b></para>
    /// This class is thread-safe as long as it's properties are not modified once it goes into use.
    /// When used across multiple threads, each thread will use a separate thread-specific ManagedDBConnection.
    /// </summary>
    [CLSCompliant(false)]
    public class DatabaseConnectionInfo : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DatabaseConnectionInfo).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The type of connection if not provided via the <see cref="DataProvider"/> property.
        /// </summary>
        Type _targetConnectionType = null;

        /// <summary>
        /// Allows data sources and providers to be looked up by name.
        /// </summary>
        DataConnectionConfiguration _connectionConfig =
            new DataConnectionConfiguration(FileSystemMethods.CommonApplicationDataPath);

        /// <summary>
        /// A <see cref="DbConnection"/> managed by this instance for the case that the caller does
        /// not need to manually handle connection availability and updates of SQLite files.
        /// https://extract.atlassian.net/browse/ISSUE-15276
        /// As a result of some sporadic database errors that appear to be the result of thread safety
        /// that had been present with SQL CE DBs, each thread will now using it's own managed DB connection.
        /// Connection pooling should continue to allow DB connections to be shared in the background,
        /// but will shift thread safety responsibility with the connections.
        /// </summary>
        [ThreadStatic]
        DbConnection _managedDbConnection;

        /// <summary>
        /// The connection string associated with the current <see cref="_managedDbConnection"/>.
        /// </summary>
        [ThreadStatic]
        string _managedConnectionString;

        /// <summary>
        /// Synchronizes access for thread safety.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Handler to allow disposal of this instance to close all thread-specific connections.
        /// </summary>
        ThreadSpecificEventHandler<EventArgs> _disposeHandler = new ThreadSpecificEventHandler<EventArgs>();

        #endregion Fields

        #region Constructors

        /// <overrides>
        /// Initializes a new instance of the <see cref="DatabaseConnectionInfo"/> class.
        /// </overrides>
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionInfo"/> class.
        /// </summary>
        public DatabaseConnectionInfo()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI34748",
                    _OBJECT_NAME);

                ConnectionString = "";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34749");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionInfo"/> class.
        /// </summary>
        /// <param name="targetConnectionType">The fully qualified type name for the connection.
        /// </param>
        /// <param name="connectionString">The connection string.</param>
        public DatabaseConnectionInfo(string targetConnectionType, string connectionString)
            : base()
        {
            try
            {
                _targetConnectionType = Type.GetType(targetConnectionType);
                ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37787");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionInfo"/> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="connectionString">The connection string.</param>
        public DatabaseConnectionInfo(DataSource dataSource, DataProvider dataProvider,
            string connectionString)
            : base()
        {
            try
            {
                DataSource = dataSource;
                DataProvider = dataProvider;
                ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34750");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionInfo"/> class.
        /// </summary>
        /// <param name="dataSourceName">Name of the data source.</param>
        /// <param name="dataProviderName">Name of the data provider.</param>
        /// <param name="connectionString">The connection string.</param>
        public DatabaseConnectionInfo(string dataSourceName, string dataProviderName,
            string connectionString)
            : base()
        {
            try
            {
                DataSourceName = dataSourceName;
                DataProviderName = dataProviderName;
                ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34751");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionInfo"/> class.
        /// </summary>
        /// <param name="databaseConnectionInfo">The database connection info.</param>
        public DatabaseConnectionInfo(DatabaseConnectionInfo databaseConnectionInfo)
        {
            try
            {
                DataSourceName = databaseConnectionInfo.DataSourceName;
                DataProviderName = databaseConnectionInfo.DataProviderName;
                if (TargetConnectionType == null)
                {
                    _targetConnectionType = databaseConnectionInfo.TargetConnectionType;
                }
                ConnectionString = databaseConnectionInfo.ConnectionString;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34752");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the type of the connection to use.
        /// </summary>
        /// <value>
        /// The type of the connection to use.
        /// </value>
        public Type TargetConnectionType
        {
            get
            {
                try
                {
                    lock (_lock)
                    {
                        if (_targetConnectionType != null)
                        {
                            return _targetConnectionType;
                        }
                        else if (DataProvider != null)
                        {
                            return DataProvider.TargetConnectionType;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37788");
                }
            }

            set
            {
                try
                {
                    lock (_lock)
                    {
                        if (value != _targetConnectionType)
                        {
                            // If there is an existing DataProvider with a connection type that
                            // conflicts with the provided type, clear the DataProvider and DataSource
                            // and use only the newly provided connection type.
                            if (DataProvider != null && DataProvider.TargetConnectionType != value)
                            {
                                DataProvider = null;
                                DataSource = null;
                            }

                            _targetConnectionType = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37789");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the data source.
        /// </summary>
        /// <value>
        /// The name of the data source.
        /// </value>
        public string DataSourceName
        {
            get
            {
                lock (_lock)
                {
                    return (DataSource == null) ? "" : DataSource.DisplayName;
                }
            }

            set
            {
                try
                {
                    lock (_lock)
                    {
                        if (value != DataSourceName)
                        {
                            DataSource = null;

                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                DataSource = _connectionConfig.GetDataSourceFromName(value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34753");
                }
            }
        }

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>
        /// The data source.
        /// </value>
        public DataSource DataSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the data provider.
        /// </summary>
        /// <value>
        /// The name of the data provider.
        /// </value>
        public string DataProviderName
        {
            get
            {
                lock (_lock)
                {
                    return (DataProvider == null) ? "" : DataProvider.DisplayName;
                }
            }

            set
            {
                try
                {
                    lock (_lock)
                    {
                        if (value != DataProviderName)
                        {
                            DataProvider = null;

                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                DataProvider = _connectionConfig.GetDataProviderFromName(value);
                            }
                        } 
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34754");
                }
            }
        }

        /// <summary>
        /// Gets or sets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        public DataProvider DataProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IPathTags"/> instance used to expand path tags/functions in
        /// the connection string.
        /// </summary>
        /// <value>
        /// The <see cref="IPathTags"/> instance used to expand path tags/functions in the
        /// connection string.
        /// </value>
        public IPathTags PathTags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/> managed by this instance for the case that the
        /// caller does not need to manually handle connection availability and updates of SQLite
        /// files. 
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        public DbConnection ManagedDbConnection
        {
            get
            {
                try
                {
                    bool recreateConnection = false;

                    if (_managedDbConnection != null)
                    {
                        lock (_lock)
                        {
                            // The connection should be reset if any of the connection properties have
                            // changed.
                            recreateConnection =
                                _managedDbConnection.GetType() != TargetConnectionType ||
                                _managedConnectionString != ConnectionString;
                        }

                        if (recreateConnection)
                        {
                            CloseManagedDbConnection();
                        }
                    }

                    // Open a new connection if needed.
                    if (_managedDbConnection == null)
                    {
                        lock (_lock)
                        {
                            if (TargetConnectionType != null &&
                                !string.IsNullOrWhiteSpace(ConnectionString))
                            {
                                _managedDbConnection = OpenConnection();
                                _managedConnectionString = ConnectionString;
                            }
                        }

                        if (!recreateConnection)
                        {
                            // If !recreateConnection, this is the initial managed connection on
                            // this thread. Add an event handler to clean it up.
                            _disposeHandler.AddEventHandler(HandleDispose);
                        }
                    }

                    return _managedDbConnection;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37790");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Loads the connection dialog configuration.
        /// </summary>
        /// <param name="connectionDialog">The connection dialog.</param>
        public void LoadConnectionDialogConfiguration(DataConnectionDialog connectionDialog)
        {
            try
            {
                lock (_lock)
                {
                    try
                    {
                        _connectionConfig.LoadConfiguration(connectionDialog);
                    }
                    catch (Exception ex)
                    {
                        var ee = new ExtractException("ELI34797",
                            "Failed to load connection configuration; attempting to reset.", ex);
                        ee.Log();

                        _connectionConfig.ResetConfiguration();
                        _connectionConfig.LoadConfiguration(connectionDialog);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34755");
            }
        }

        /// <summary>
        /// Saves the dialog configuration.
        /// </summary>
        /// <param name="connectionDialog">The connection dialog.</param>
        public void SaveConnectionDialogConfiguration(DataConnectionDialog connectionDialog)
        {
            try
            {
                lock (_lock)
                {
                    _connectionConfig.SaveConfiguration(connectionDialog);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34756");
            }
        }

        /// <summary>
        /// Creates and opens a <see cref="DbConnection"/> for the current configuration.
        /// </summary>
        /// <returns>The open <see cref="DbConnection"/>.</returns>
        public DbConnection OpenConnection()
        {
            try
            {
                lock (_lock)
                {
                    ExtractException.Assert("ELI34758", "Database provider has not been specified.",
                        TargetConnectionType != null);

                    DbConnection dbConnection =
                        (DbConnection)Activator.CreateInstance(TargetConnectionType);
                    var expandedConnectionString = (PathTags == null)
                        ? ConnectionString
                        : PathTags.Expand(ConnectionString);

                    if (TargetConnectionType == typeof(SQLiteConnection))
                    {
                        // Fix UNC path for SQLite
                        // https://extract.atlassian.net/browse/ISSUE-17754
                        expandedConnectionString = SqliteMethods.FixConnectionString(expandedConnectionString);
                    }

                    dbConnection.ConnectionString = expandedConnectionString;
                    dbConnection.Open();

                    return dbConnection; 
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34759");
            }
        }

        /// <summary>
        /// Closes the <see cref="ManagedDbConnection"/> if it is currently active. This call has
        /// no effect and generates no exceptions if <see cref="ManagedDbConnection"/> is
        /// <see langword="null"/> or closed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        public void CloseManagedDbConnection()
        {
            try
            {
                if (_managedDbConnection != null)
                {
                    _managedDbConnection.Dispose();
                    _managedDbConnection = null;
                    _managedConnectionString = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37792");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            try
            {
                return (TargetConnectionType == null) ? 0 : TargetConnectionType.GetHashCode() ^
                       DataSourceName.GetHashCode() ^
                       DataProviderName.GetHashCode() ^
                       ConnectionString.GetHashCode();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34745");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns><see langword="true"/> if the specified <see cref="System.Object"/> is equal to
        /// this instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            try
            {
                return Equals(obj as DatabaseConnectionInfo);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34746");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="DatabaseConnectionInfo"/> is equal to this
        /// instance.
        /// </summary>
        /// <param name="other">The <see cref="DatabaseConnectionInfo"/> to compare with this
        /// instance.</param>
        /// <returns><see langword="true"/> if the specified <see cref="DatabaseConnectionInfo"/> is
        /// equal to this instance; otherwise, <see langword="false"/>.</returns>
        public bool Equals(DatabaseConnectionInfo other)
        {
            try
            {
                if (other == null)
                {
                    return false;
                }

                // Return true if the fields match:
                return (TargetConnectionType == other.TargetConnectionType &&
                    DataSourceName == other.DataSourceName &&
                    DataProviderName == other.DataProviderName &&
                    ConnectionString == other.ConnectionString);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34747");
            }
        }

        #endregion Overrides

        #region IDisposable

        /// <overloads>Releases resources used by the <see cref="DatabaseConnectionInfo"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="DatabaseConnectionInfo"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DatabaseConnectionInfo"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_disposeHandler != null)
                    {
                        _disposeHandler.ThreadEventHandler?.Invoke(this, new EventArgs());
                        _disposeHandler = null;
                    }

                }
                catch { }
            }

            // Dispose of unmanaged resources
        }

        /// <summary>
        /// Handler to allow the disposal of all thread-specific _managedDbConnections
        /// </summary>
        void HandleDispose(object sender, EventArgs args)
        {
            try
            {
                if (_managedDbConnection != null)
                {
                    _managedDbConnection.Dispose();
                    _managedDbConnection = null;
                }

                // We are now done with this thread.
                _disposeHandler.DisposeThread();
            }
            catch { }
        }

        #endregion IDisposable
    }
}
