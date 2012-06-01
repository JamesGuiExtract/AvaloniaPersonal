using Extract.Licensing;
using Extract.Utilities;
using Microsoft.Data.ConnectionUI;
using System;
using System.Data.Common;

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
        /// Creates the connection instance.
        /// </summary>
        /// <returns></returns>
        public DbConnection OpenConnection()
        {
            try
            {
                return OpenConnection(null);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34757");
            }
        }

        /// <summary>
        /// Creates the connection instance.
        /// </summary>
        /// <param name="pathTags"></param>
        /// <returns></returns>
        public DbConnection OpenConnection(IPathTags pathTags)
        {
            try
            {
                ExtractException.Assert("ELI34758", "Database provider has not been specified.",
                    DataProvider != null);

                DbConnection dbConnection =
                    (DbConnection)Activator.CreateInstance(DataProvider.TargetConnectionType);
                dbConnection.ConnectionString = (pathTags == null)
                    ? ConnectionString
                    : pathTags.Expand(ConnectionString);
                dbConnection.Open();

                return dbConnection;
            }
            catch (Exception ex)
            {
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
    }
}
