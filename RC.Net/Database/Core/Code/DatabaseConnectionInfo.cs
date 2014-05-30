using Extract.Licensing;
using Extract.Utilities;
using Microsoft.Data.ConnectionUI;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Database
{
    /// <summary>
    /// Represents the information necessary to open a connection to a database that has been
    /// configured with the <see cref="DataConnectionDialog"/>.
    /// </summary>
    [CLSCompliant(false)]
    public class DatabaseConnectionInfo
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DatabaseConnectionInfo).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// For SQL CE database connection opened, keeps track of the local database copy to use.
        /// </summary>
        static TemporaryFileCopyManager _localDatabaseCopyManager = new TemporaryFileCopyManager();

        /// <summary>
        /// Keeps track of the original database filename associated with each open SQL CE connection.
        /// </summary>
        static ConcurrentDictionary<DbConnection, string> _originalSqlCeDbFileNames =
            new ConcurrentDictionary<DbConnection, string>();

        /// <summary>
        /// Allows data sources and providers to be looked up by name.
        /// </summary>
        DataConnectionConfiguration _connectionConfig =
            new DataConnectionConfiguration(FileSystemMethods.ApplicationDataPath);

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
        /// Gets or sets the name of the data source.
        /// </summary>
        /// <value>
        /// The name of the data source.
        /// </value>
        public string DataSourceName
        {
            get
            {
                return (DataSource == null) ? "" : DataSource.DisplayName;
            }

            set
            {
                try
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
                return (DataProvider == null) ? "" : DataProvider.DisplayName;
            }

            set
            {
                try
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
                try
                {
                    _connectionConfig.LoadConfiguration(connectionDialog);
                }
                catch (Exception ex)
                {
                    var ee = new ExtractException("ELI34797",
                        "Failed to load connecton configuration; attempting to reset.", ex);
                    ee.Log();

                    _connectionConfig.ResetConfiguration();
                    _connectionConfig.LoadConfiguration(connectionDialog);
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
                _connectionConfig.SaveConfiguration(connectionDialog);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34756");
            }
        }

        /// <summary>
        /// Creates and opens a <see cref="DbConnection"/> for the current configuration.
        /// </summary>
        /// <param name="useLocalSqlCeCopy"><see langword="true"/> to open and manage a local copy
        /// of the database if this connection is for a SQL CE database.
        /// <para><b>Note</b></para>
        /// If <see langword="true"/>, re-opening <see cref="DbConnection"/> instances that have been
        /// closed is un-supported as the local database copy used may no longer exist.</param>
        /// <returns>The open <see cref="DbConnection"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ce")]
        public DbConnection OpenConnection(bool useLocalSqlCeCopy)
        {
            try
            {
                return OpenConnection(useLocalSqlCeCopy, null);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34757");
            }
        }

        /// <summary>
        /// Creates and opens a <see cref="DbConnection"/> for the current configuration.
        /// </summary>
        /// <param name="useLocalSqlCeCopy"><see langword="true"/> to open and manage a local copy
        /// of the database if this connection is for a SQL CE database.
        /// <para><b>Note</b></para>
        /// If <see langword="true"/>, re-opening <see cref="DbConnection"/> instances that have been
        /// closed is un-supported as the local database copy used may no longer exist.</param>
        /// <param name="pathTags">A <see cref="IPathTags"/> instance used to expand path
        /// tags/functions in the connection string.</param>
        /// <returns>The open <see cref="DbConnection"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ce")]
        public DbConnection OpenConnection(bool useLocalSqlCeCopy, IPathTags pathTags)
        {
            object sqlceDatabaseFileObject = null;
            string sqlceDatabaseFile = null;

            try
            {
                ExtractException.Assert("ELI34758", "Database provider has not been specified.",
                    DataProvider != null);

                DbConnection dbConnection =
                    (DbConnection)Activator.CreateInstance(DataProvider.TargetConnectionType);
                dbConnection.ConnectionString = (pathTags == null)
                    ? ConnectionString
                    : pathTags.Expand(ConnectionString);

                if (useLocalSqlCeCopy &&
                    DataSource.Name.Equals(SqlCe.SqlCeDataSource.Name, StringComparison.Ordinal))
                {
                    var connectionStringBuilder = new DbConnectionStringBuilder();
                    connectionStringBuilder.ConnectionString = dbConnection.ConnectionString;
                    string parameterName = null;
                    if (connectionStringBuilder.TryGetValue("Data Source", out sqlceDatabaseFileObject))
                    {
                        parameterName = "Data Source";
                    }
                    else if (connectionStringBuilder.TryGetValue("DataSource", out sqlceDatabaseFileObject))
                    {
                        parameterName = "DataSource";
                    }
                    else
                    {
                        ExtractException.ThrowLogicException("ELI36924");
                    }

                    sqlceDatabaseFile = (string)sqlceDatabaseFileObject;
                    connectionStringBuilder.Add(parameterName,
                        _localDatabaseCopyManager.GetCurrentTemporaryFileName(
                            sqlceDatabaseFile, this, true));
                    dbConnection.ConnectionString = connectionStringBuilder.ConnectionString;
                }

                dbConnection.Open();

                // Handle the StateChange event for SQL CE connections to be able to dereference
                // the local temp file when no longer in use. The Disposed event would be better
                // except that it is not raised when the connection is disposed.
                if (!string.IsNullOrEmpty(sqlceDatabaseFile))
                {
                    _originalSqlCeDbFileNames.TryAdd(dbConnection, sqlceDatabaseFile);
                    dbConnection.StateChange += HandleDbConnection_StateChange;
                }

                return dbConnection;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(sqlceDatabaseFile))
                {
                    _localDatabaseCopyManager.Dereference(sqlceDatabaseFile, this);
                }

                throw ex.AsExtract("ELI34759");
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
                return DataSourceName.GetHashCode() ^
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
                return (DataSourceName == other.DataSourceName &&
                    DataProviderName == other.DataProviderName &&
                    ConnectionString == other.ConnectionString);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34747");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DbConnection.StateChange"/> event of a <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Data.StateChangeEventArgs"/> instance containing
        /// the event data.</param>
        void HandleDbConnection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            try
            {
                // Ideally the Dispose event would be handled instead in order to dereference the
                // local DB copy, except that it does not appear to be raised when the connection
                // is disposed.
                if (e.CurrentState == System.Data.ConnectionState.Closed)
                {
                    DbConnection dbConnection = sender as DbConnection;

                    string sqlCeDatabaseFile;
                    if (_originalSqlCeDbFileNames.TryRemove(dbConnection, out sqlCeDatabaseFile))
                    {
                        _localDatabaseCopyManager.Dereference((string)sqlCeDatabaseFile, this);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exceptions here rather than displaying or throwing because there is a good
                // chance this event is occuring as part of a Dispose call on the connection.
                ex.ExtractLog("ELI36982");
            }
        }

        #endregion Event Handlers
    }
}
