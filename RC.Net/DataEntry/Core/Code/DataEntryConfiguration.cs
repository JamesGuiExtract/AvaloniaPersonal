using Extract.Database;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Represents a data entry configuration used to display document data.
    /// </summary>
    public class DataEntryConfiguration : IDisposable
    {
        #region Fields

        /// <summary>
        /// The configuration settings specified via config file.
        /// </summary>
        ConfigSettings<Properties.Settings> _config;

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> instance associated with the configuration.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// DatabaseConnectionInfo instances to be used for any validation or auto-update queries
        /// requiring a database; The key is the connection name (blank for default connection).
        /// </summary>
        Dictionary<string, DatabaseConnectionInfo> _dbConnections =
            new Dictionary<string, DatabaseConnectionInfo>();

        /// <summary>
        /// The <see cref="ITagUtility"/> interface provided to
        /// expand path tags/functions.
        /// </summary>
        ITagUtility _tagUtility;

        /// <summary>
        /// The <see cref="FileProcessingDB"/> in use.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryConfiguration"/> instance.
        /// </summary>
        /// <param name="config">The configuration settings specified via config file.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> interface provided to expand path
        /// tags/functions.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public DataEntryConfiguration(
            ConfigSettings<Extract.DataEntry.Properties.Settings> config,
            ITagUtility tagUtility, FileProcessingDB fileProcessingDB)
        {
            try
            {
                _config = config;
                _tagUtility = tagUtility;
                _fileProcessingDB = fileProcessingDB;

                // Retrieve the name of the DEP assembly
                string dataEntryPanelFileName = DataEntryMethods.ResolvePath(
                    config.Settings.DataEntryPanelFileName);

                // Create a DataEntryControlHost instance from the specified assembly
                _dataEntryControlHost = CreateDataEntryControlHost(dataEntryPanelFileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41588");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The configuration settings specified via config file.
        /// </summary>
        public ConfigSettings<Extract.DataEntry.Properties.Settings> Config
        {
            get
            {
                return _config;
            }
        }

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> instance associated with the configuration.
        /// </summary>
        public DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return _dataEntryControlHost;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Opens the database connections used by this configuration.
        /// </summary>
        public void OpenDatabaseConnections()
        {
            try
            {
                DataEntryControlHost.SetDatabaseConnections(GetDatabaseConnections());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41590");
            }
        }


        /// <summary>
        /// Closes the database connections used by this configuration.
        /// </summary>
        public void CloseDatabaseConnections()
        {
            try
            {
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.SetDatabaseConnections(null);
                }

                if (_dbConnections != null)
                {
                    CollectionMethods.ClearAndDispose(_dbConnections);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41591");
            }
        }

        #endregion Methods

        #region IDisposable

        /// <overloads>Releases resources used by the <see cref="DataEntryConfiguration"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryConfiguration"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataEntryConfiguration"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.Dispose();
                    _dataEntryControlHost = null;
                }

                if (_dbConnections != null)
                {
                    CollectionMethods.ClearAndDispose(_dbConnections);
                    _dbConnections = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable

        #region Private Members

        /// <summary>
        /// Given the specified <see paramref="databaseConnectionsNode"/>, creates or updates the
        /// specified database connection for use in the <see cref="DataEntryControlHost"/>.
        /// </summary>
        /// <param name="databaseConnectionsNode">A <see cref="XPathNavigator"/> instance from a
        /// data entry config file defining the connection.</param>
        /// <param name="isDefaultConnection"><see langword="true"/> if the connection is defined as
        /// the default database connection; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="DatabaseConnectionInfo"/> representing the
        /// <see cref="DbConnection"/>.</returns>
        DatabaseConnectionInfo LoadDatabaseConnection(XPathNavigator databaseConnectionsNode,
            out bool isDefaultConnection)
        {
            isDefaultConnection = false;
            string connectionName = "";
            string databaseType = "";
            string localDataSource = "";
            string databaseConnectionString = "";

            XPathNavigator attribute = databaseConnectionsNode.Clone();
            if (attribute.MoveToFirstAttribute())
            {
                do
                {
                    if (attribute.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        connectionName = attribute.Value;
                    }
                    else if (attribute.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        isDefaultConnection = attribute.Value.ToBoolean();
                    }
                }
                while (attribute.MoveToNextAttribute());
            }

            XPathNavigator connectionProperty = databaseConnectionsNode.Clone();
            if (connectionProperty.MoveToFirstChild())
            {
                // Load all properties of the defined connection.
                do
                {
                    // Use GetNodeValue extension method for XmlNode to allow for expansion of path
                    // tags in the config file.
                    var xmlNode = (XmlNode)connectionProperty.UnderlyingObject;

                    if (connectionProperty.Name.Equals("databaseType",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        databaseType = xmlNode.GetNodeValue(_tagUtility, false, true);
                    }
                    else if (connectionProperty.Name.Equals("localDataSource",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        localDataSource = xmlNode.GetNodeValue(_tagUtility, false, true);
                    }
                    else if (connectionProperty.Name.Equals("databaseConnectionString",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        databaseConnectionString = xmlNode.GetNodeValue(_tagUtility, false, true);
                    }
                }
                while (connectionProperty.MoveToNext());

                    SetDatabaseConnection(connectionName,
                        databaseType, localDataSource, databaseConnectionString);

                // If this is the default connection, add the connection under a blank name as well
                // (blank in _dbConnections indicates the default connection.) 
                if (isDefaultConnection && !string.IsNullOrEmpty(connectionName))
                {
                    _dbConnections[""] = _dbConnections[connectionName];
                }

                return _dbConnections[connectionName];
            }

            return null;
        }

        /// <summary>
        /// Adds or updates <see cref="_dbConnections"/> with a <see cref="DatabaseConnectionInfo"/>
        /// instance under the specified <see paramref="name"/>.
        /// </summary>
        /// <param name="name">The name of the connection ("" for the default connection)</param>
        /// <param name="databaseType">The qualified type name of the database connection.</param>
        /// <param name="localDataSource">If using an SQL CE DB, the name of the DB file can be
        /// specified here in lieu of the <see paramref="connectionString"/>.</param>
        /// <param name="connectionString">The connection string to use when connecting to the DB.
        /// </param>
        void SetDatabaseConnection(string name, string databaseType, string localDataSource,
            string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(localDataSource))
            {
                ExtractException.Assert("ELI37778", "Either a database connection string " +
                    "can be specified, or a local datasource-- not both.",
                    string.IsNullOrEmpty(connectionString), "Local data source", localDataSource,
                    "Connection string", connectionString);

                connectionString = SqlCompactMethods.BuildDBConnectionString(
                    DataEntryMethods.ResolvePath(localDataSource).ToLower(CultureInfo.CurrentCulture));
            }

            DatabaseConnectionInfo dbConnInfo = null;
            if (_dbConnections.TryGetValue(name, out dbConnInfo))
            {
                // If there is an existing connection by this name that differs from the newly
                // specified one (or there is no newly specified connection) close the existing
                // connection.
                if (string.IsNullOrWhiteSpace(connectionString) ||
                    Type.GetType(databaseType) != dbConnInfo.TargetConnectionType ||
                    connectionString != dbConnInfo.ConnectionString)
                {
                    _dbConnections.Remove(name);
                    if (dbConnInfo != null)
                    {
                        dbConnInfo.Dispose();
                        dbConnInfo = null;
                    }
                }
            }

            // Create the DatabaseConnectionInfo instance if needed.
            if (dbConnInfo == null && !string.IsNullOrWhiteSpace(connectionString))
            {
                dbConnInfo = new DatabaseConnectionInfo(databaseType, connectionString);
                dbConnInfo.UseLocalSqlCeCopy = true;
                _dbConnections[name] = dbConnInfo;
            }
        }

        /// <summary>
        /// Attempts to open database connection(s) for use by the DEP for validation and
        /// auto-updates if connection information is specified in the config settings.
        /// </summary>
        /// <returns>A dictionary of <see cref="DbConnection"/>(s) where the key is the connection
        /// name (blank for default). If no database connection is currently configured, any open
        /// connection will be closed and <see langword="null"/> will returned.
        /// </returns>
        Dictionary<string, DbConnection> GetDatabaseConnections()
        {
            try
            {
                if (!_dbConnections.Any())
                {
                    // Retrieve the databaseConnections XML section from the active configuration if
                    // it exists
                    IXPathNavigable databaseConnections = Config.GetSectionXml("databaseConnections");

                    bool loadedDefaultConnection = false;

                    // Parse and create/update each specified connection.
                    XPathNavigator databaseConnectionsNode = null;
                    if (databaseConnections != null)
                    {
                        databaseConnectionsNode = databaseConnections.CreateNavigator();

                        if (databaseConnectionsNode.MoveToFirstChild())
                        {
                            do
                            {
                                bool isDefaultConnection = false;
                                var connectionInfo =
                                    LoadDatabaseConnection(databaseConnectionsNode, out isDefaultConnection);

                                ExtractException.Assert("ELI37781",
                                    "Multiple default connections are defined.",
                                    !isDefaultConnection || !loadedDefaultConnection);

                                loadedDefaultConnection |= isDefaultConnection;

                                // https://extract.atlassian.net/browse/ISSUE-13385
                                // If this is the default connection, use the FKB version (if
                                // specified) to be able to expand the <ComponentDataDir> using
                                // _tagUtility from this point forward (including for any subsequent
                                // connection definitions).
                                if (isDefaultConnection && _tagUtility != null)
                                {
                                    AddComponentDataDirTag(connectionInfo);
                                }
                            }
                            while (databaseConnectionsNode.MoveToNext());
                        }
                    }

                    // If there was no default database connection specified via the
                    // databaseConnections section, attempt to load it via the legacy DB properties.
                    if (!loadedDefaultConnection)
                    {
                        SetDatabaseConnection("",
                            Config.Settings.DatabaseType,
                            Config.Settings.LocalDataSource,
                            Config.Settings.DatabaseConnectionString);
                    }
                }

                // This class keeps track of DatabaseConfigurationInfo objects for ease of
                // management, but the DataEntryControlHost only cares about the DbConnections
                // themselves; return the managed connection for each.
                return _dbConnections.ToDictionary(
                    (conn) => conn.Key, (conn) => conn.Value.ManagedDbConnection);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI26159");
            }
        }

        /// <summary>
        /// If an FKBVersion value is available in the Settings table of the specified
        /// <see paramref="connectionInfo"/>, adds the &lt;ComponentDataDir&gt; tag to
        /// <see cref="_tagUtility"/>.
        /// </summary>
        /// <param name="connectionInfo">A <see cref="DatabaseConnectionInfo"/> that is expected to
        /// represent the default customer OrderMappingDB database. The tag will only be added if
        /// this database has a Settings table and that table has a populated FKBVersion setting.
        /// </param>
        void AddComponentDataDirTag(DatabaseConnectionInfo connectionInfo)
        {
            if (_tagUtility != null &&
                DBMethods.GetQueryResultsAsStringArray(connectionInfo.ManagedDbConnection,
                "SELECT COUNT(*) FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = 'Settings'")
                .Single() == "1")
            {
                string FKBVersion = DBMethods.GetQueryResultsAsStringArray(
                    connectionInfo.ManagedDbConnection,
                    "SELECT [Value] FROM [Settings] WHERE [Name] = 'FKBVersion'")
                    .SingleOrDefault();

                if (!string.IsNullOrWhiteSpace(FKBVersion))
                {
                    try
                    {
                        string alternateComponentDataDir = null;
                        if (_fileProcessingDB != null)
                        {
                            alternateComponentDataDir =
                                _fileProcessingDB.GetDBInfoSetting("AlternateComponentDataDir", false);
                        }

                        var afUtility = new AFUtility();
                        var componentDataFolder =
                            afUtility.GetComponentDataFolder2(FKBVersion, alternateComponentDataDir);
                        _tagUtility.AddTag("<ComponentDataDir>", componentDataFolder);
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI41593");
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates the one and only <see cref="DataEntryControlHost"/> implemented by the
        /// specified assembly.
        /// </summary>
        /// <param name="assemblyFileName">The filename of the assembly to use.</param>
        /// <returns>A <see cref="DataEntryControlHost"/> instantiated from the specified assembly.
        /// </returns>
        DataEntryControlHost CreateDataEntryControlHost(string assemblyFileName)
        {
            try
            {
                // A variable to store the return value
                DataEntryControlHost dataEntryControlHost =
                    UtilityMethods.CreateTypeFromAssembly<DataEntryControlHost>(assemblyFileName);

                ExtractException.Assert("ELI23676",
                    "Failed to find data entry control host implementation!", dataEntryControlHost != null);

                dataEntryControlHost.Config = Config;

                return dataEntryControlHost;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23677",
                    "Unable to initialize data entry control host!", ex);
                ee.AddDebugData("Assembly Name", assemblyFileName, false);
                throw ee;
            }
        }

        #endregion Private Members
    }
}
