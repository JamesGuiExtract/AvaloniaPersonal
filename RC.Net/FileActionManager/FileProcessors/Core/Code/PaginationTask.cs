using Extract.Database;
using Extract.DataEntry;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that allows pagination of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("865834B8-A9F7-491C-B8A3-853AE889590E")]
    [CLSCompliant(false)]
    public interface IPaginationTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
        IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending.
        /// </summary>
        string SourceAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path tag expression defining the filename that should be given to a
        /// pagination output document.
        /// </summary>
        string OutputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the action into which paginated output documents should be moved to pending.
        /// </summary>
        string OutputAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the filename assembly defining an IPaginationDocumentDataPanel to display
        /// document data.
        /// </summary>
        string DocumentDataPanelAssembly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets whether to output expected pagination attributes.
        /// </summary>
        bool OutputExpectedPaginationAttributesFiles
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        string ExpectedPaginationAttributesOutputPath
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a file processing task that allows pagination of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("DF414AD2-742A-4ED7-AD20-C1A1C4993175")]
    [ProgId("Extract.FileActionManager.PaginationTask")]
    [CLSCompliant(false)]
    public class PaginationTask : IPaginationTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Paginate files";

        /// <summary>
        /// Current task version.
        /// Versions:
        /// 1. Initial version
        /// 2. Added OutputExpectedPaginationAttributesFiles and ExpectedPaginationAttributesOutputPath
        /// </summary>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.PaginationUIObject;

        /// <summary>
        /// The default path for expected pagination attributes file
        /// </summary>
        const string _DEFAULT_EXPECTED_OUTPUT_PATH = "<SourceDocName>.pagination.evoa";
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// The form to display for paginating files.
        /// </summary>
        static VerificationForm<PaginationTaskForm> _form;

        /// <summary>
        /// The action (if any) into which paginated sources should be moved to pending.
        /// </summary>
        string _sourceAction;

        /// <summary>
        /// The path tag expression defining the filename that should be given to a pagination
        /// output document.
        /// </summary>
        string _outputPath = "$InsertBeforeExtension(<SourceDocName>,_<SubDocIndex>)";

        /// <summary>
        /// The action into which paginated output documents should be moved to pending.
        /// </summary>
        string _outputAction;

        /// <summary>
        /// The filename assembly defining an <see cref="IPaginationDocumentDataPanel"/> to display
        /// document data.
        /// </summary>
        string _documentDataPanelAssembly;

        /// <summary>
        /// A panel that is available to view/edit key data fields associated with either physical
        /// or proposed paginated documents.
        /// </summary>
        IPaginationDocumentDataPanel _paginationDocumentDataPanel;

        /// <summary>
        /// The name of the action currently being processed.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the action being processed.
        /// </summary>
        int _actionID;

        /// <summary>
        /// The <see cref="FAMTagManager"/> to use to expand path tags and functions.
        /// </summary>
        FAMTagManager _tagManager;

        /// <summary>
        /// The <see cref="IFileRequestHandler"/> that can be used by the task to carry out requests
        /// for files to be checked out, released or re-ordered in the queue.
        /// </summary>
        IFileRequestHandler _fileRequestHandler;

        /// <summary>
        /// Object used to mutex around the verification form creation.
        /// </summary>
        static readonly object _lock = new object();

        /// <summary>
        /// Indicates whether this task object is dirty or not
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        private bool _outputExpectedPaginationAttributesFiles;

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        private string _expectedPaginationAttributesOutputPath;

        /// <summary>
        /// DatabaseConnectionInfo instances to be used for any validation or auto-update queries
        /// requiring a database; The key is the connection name (blank for default connection).
        /// </summary>
        Dictionary<string, DatabaseConnectionInfo> _dbConnections =
            new Dictionary<string, DatabaseConnectionInfo>();

        /// <summary>
        /// If not <see langword="null"/> this configuration should be used for documents with
        /// missing or undefined document types.
        /// </summary>
        DataEntryConfiguration _defaultDataEntryConfig;

        /// <summary>
        /// The configuration that is currently loaded.
        /// </summary>
        DataEntryConfiguration _activeDataEntryConfig;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationTask"/> class.
        /// </summary>
        public PaginationTask()
        {
            try
            {
                // Lock around form creation
                lock (_lock)
                {
                    if (_form == null)
                    {
                        _form = new VerificationForm<PaginationTaskForm>();
                    }
                }
                _expectedPaginationAttributesOutputPath = _DEFAULT_EXPECTED_OUTPUT_PATH;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40135");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationTask"/> class.
        /// </summary>
        public PaginationTask(PaginationTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40136");
            }
        }
        
        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the action (if any) into which paginated sources should be moved to pending.
        /// </summary>
        public string SourceAction
        {
            get
            {
                return _sourceAction;
            }

            set
            {
                try
                {
                    if (value != _sourceAction)
                    {
                        _sourceAction = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40118");
                }
            }
        }

        /// <summary>
        /// Gets or sets the path tag expression defining the filename that should be given to a
        /// pagination output document.
        /// </summary>
        public string OutputPath
        {
            get
            {
                return _outputPath;
            }

            set
            {
                try
                {
                    if (value != _outputPath)
                    {
                        _outputPath = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40119");
                }
            }
        }

        /// <summary>
        /// Gets or sets the action into which paginated output documents should be moved to pending.
        /// </summary>
        public string OutputAction
        {
            get
            {
                return _outputAction;
            }

            set
            {
                try
                {
                    if (value != _outputAction)
                    {
                        _outputAction = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40120");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filename assembly defining an <see cref="IPaginationDocumentDataPanel"/>
        /// to display document data.
        /// </summary>
        public string DocumentDataPanelAssembly
        {
            get
            {
                return _documentDataPanelAssembly;
            }

            set
            {
                try
                {
                    if (value != _documentDataPanelAssembly)
                    {
                        _documentDataPanelAssembly = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40121");
                }
            }
        }

        /// <summary>
        /// Gets/sets whether to output expected pagination attributes to a file
        /// </summary>
        public bool OutputExpectedPaginationAttributesFiles
        {
            get
            {
                return _outputExpectedPaginationAttributesFiles;
            }
            set
            {
                if (value != _outputExpectedPaginationAttributesFiles)
                {
                    _outputExpectedPaginationAttributesFiles = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets/sets the file path to output expected pagination attributes to
        /// </summary>
        public string ExpectedPaginationAttributesOutputPath
        {
            get
            {
                return _expectedPaginationAttributesOutputPath;
            }
            set
            {
                if (value != _expectedPaginationAttributesOutputPath)
                {
                    _expectedPaginationAttributesOutputPath = value;
                    _dirty = true;
                }
            }
        }


        #endregion Properties

        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="PaginationTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="PaginationTask"/> from which to copy.</param>
        void CopyFrom(PaginationTask task)
        {
            SourceAction = task.SourceAction;
            OutputPath = task.OutputPath;
            OutputAction = task.OutputAction;
            DocumentDataPanelAssembly = task.DocumentDataPanelAssembly;
            OutputExpectedPaginationAttributesFiles = task.OutputExpectedPaginationAttributesFiles;
            ExpectedPaginationAttributesOutputPath = task.ExpectedPaginationAttributesOutputPath;

            _dirty = true;
        }

        /// <summary>
        /// Creates a <see cref="PaginationTaskForm"/> with the current settings.
        /// </summary>
        /// <returns>A <see cref="PaginationTaskForm"/> with the current settings.</returns>
        IVerificationForm CreatePaginationTaskForm()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(DocumentDataPanelAssembly))
                {
                    var pathTags = new FileActionManagerPathTags(_tagManager, "");
                    string paginationDocumentDataPanelAssembly =
                        pathTags.Expand(DocumentDataPanelAssembly);

                    _paginationDocumentDataPanel =
                        CreateDocumentDataPanel(paginationDocumentDataPanelAssembly);
                }

                var form = new PaginationTaskForm(this, _paginationDocumentDataPanel,
                    _fileProcessingDB, _actionID, _tagManager, _fileRequestHandler);

                _activeDataEntryConfig.DataEntryControlHost.DataEntryApplication = form;

                return form;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40137");
            }
        }
        
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
            var tagUtility = (ITagUtility)_tagManager;

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
                        databaseType = xmlNode.GetNodeValue(tagUtility, false, true);
                    }
                    else if (connectionProperty.Name.Equals("localDataSource",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        localDataSource = xmlNode.GetNodeValue(tagUtility, false, true);
                    }
                    else if (connectionProperty.Name.Equals("databaseConnectionString",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        databaseConnectionString = xmlNode.GetNodeValue(tagUtility, false, true);
                    }
                }
                while (connectionProperty.MoveToNext());

                if (string.IsNullOrWhiteSpace(connectionName))
                {
                    // If the connection is not named, assume it to be the default.
                    isDefaultConnection = true;
                }
                else
                {
                    SetDatabaseConnection(connectionName,
                        databaseType, localDataSource, databaseConnectionString);
                }

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
                ExtractException.Assert("ELI41346", "Either a database connection string " +
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
                if (_activeDataEntryConfig == null)
                {
                    CollectionMethods.ClearAndDispose(_dbConnections);
                }
                else
                {
                    // Retrieve the databaseConnections XML section from the active configuration if
                    // it exists
                    IXPathNavigable databaseConnections =
                        _activeDataEntryConfig.Config.GetSectionXml("databaseConnections");

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

                                ExtractException.Assert("ELI41344",
                                    "Multiple default connections are defined.",
                                    !isDefaultConnection || !loadedDefaultConnection);

                                loadedDefaultConnection |= isDefaultConnection;

                                // https://extract.atlassian.net/browse/ISSUE-13385
                                // If this is the default connection, use the FKB version (if
                                // specified) to be able to expand the <ComponentDataDir> using
                                // _tagUtility from this point forward (including for any subsequent
                                // connection definitions).
                                if (isDefaultConnection && _tagManager != null)
                                {
                                    AddComponentDataDirTag(connectionInfo);
                                }
                            }
                            while (databaseConnectionsNode.MoveToNext());
                        }
                    }

                    // If there was no default database connection specified via the
                    // databaseConnections section, attempt to load it via the legacy DB properties.
                    if (!loadedDefaultConnection && _activeDataEntryConfig != null)
                    {
                        SetDatabaseConnection("",
                            _activeDataEntryConfig.Config.Settings.DatabaseType,
                            _activeDataEntryConfig.Config.Settings.LocalDataSource,
                            _activeDataEntryConfig.Config.Settings.DatabaseConnectionString);
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
                throw ex.AsExtract("ELI41345");
            }
        }

        /// <summary>
        /// If an FKBVersion value is available in the Settings table of the specified
        /// <see paramref="connectionInfo"/>, adds the &lt;ComponentDataDir&gt; tag to
        /// <see cref="_tagManager"/>.
        /// </summary>
        /// <param name="connectionInfo">A <see cref="DatabaseConnectionInfo"/> that is expected to
        /// represent the default customer OrderMappingDB database. The tag will only be added if
        /// this database has a Settings table and that table has a populated FKBVersion setting.
        /// </param>
        void AddComponentDataDirTag(DatabaseConnectionInfo connectionInfo)
        {
            if (_tagManager != null &&
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
                    var ruleExecutionEnv = new RuleExecutionEnv();
                    ruleExecutionEnv.PushRSDFileName("");
                    try
                    {
                        ruleExecutionEnv.FKBVersion = FKBVersion;
                        if (_fileProcessingDB != null)
                        {
                            ruleExecutionEnv.AlternateComponentDataDir =
                                _fileProcessingDB.GetDBInfoSetting("AlternateComponentDataDir", false);
                        }

                        var afUtility = new AFUtility();
                        ((ITagUtility)_tagManager).AddTag("<ComponentDataDir>", afUtility.GetComponentDataFolder());
                    }
                    finally
                    {
                        ruleExecutionEnv.PopRSDFileName();
                    }
                }
            }
        }

        #endregion Methods

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ExtractImageAreaTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40122", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (PaginationTask)Clone();
                FileProcessingDB fileProcessingDB = new FileProcessingDB();
                fileProcessingDB.ConnectLastUsedDBThisProcess();
                using (var dialog = new PaginationTaskSettingsDialog(cloneOfThis, fileProcessingDB))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40123",
                    "Error configuring" + _COMPONENT_DESCRIPTION + ".", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="PaginationTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="PaginationTask"/> instance.</returns>
        public object Clone()
        {
            return new PaginationTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="PaginationTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            CopyFrom((PaginationTask)pObject);
        }

        #endregion ICopyableObject Members
        
        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The minimum stack size needed for the thread in which this task is to be run.
        /// </value>
        [CLSCompliant(false)]
        public uint MinStackSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40124", _COMPONENT_DESCRIPTION);

                _form.Cancel();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40125", 
                    "Error canceling paginate files task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40126", _COMPONENT_DESCRIPTION);

                _form.CloseForm();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40127",
                    "Error closing paginate files task.", ex);
            }
        }

        /// <summary>
        /// Called notify to the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means that call to <see cref="ProcessFile"/> or <see cref="Close"/> may come
        /// while the Standby call is still occurring. If this happens, the return value of Standby
        /// will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            try
            {
                return _form.Standby();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI40128", "Error stopping paginate files task.");
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40129", _COMPONENT_DESCRIPTION);

                _fileProcessingDB = pDB;
                _actionID = nActionID;
                _tagManager = pFAMTM;
                _fileRequestHandler = pFileRequestHandler;

                _form.ShowForm(CreatePaginationTaskForm, 0x400000);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40130",
                    "Error initializing paginate files task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="pFileRecord">The file record that contains the info of the file being 
        /// processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI40131", _COMPONENT_DESCRIPTION);

                if (bCancelRequested)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                EFileProcessingResult result = _form.ShowDocument(pFileRecord.Name, pFileRecord.FileID,
                    nActionID, pFAMTM, pDB);

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40132", "Unable to paginate files.", ex);
            }
        }

        #endregion IFileProcessingTask Members

		#region IAccessRequired Members

		/// <summary>
		/// Returns bool value indicating if the task requires admin access
		/// </summary>
		/// <returns><see langword="true"/> if the task requires admin access
		/// <see langword="false"/> if task does not require admin access</returns>
		public bool RequiresAdminAccess()
		{
			return false;
		}

		#endregion IAccessRequired Members

		#region ILicensedComponent Members

		/// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made; 
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    SourceAction = reader.ReadString();
                    OutputPath = reader.ReadString();
                    OutputAction = reader.ReadString();
                    DocumentDataPanelAssembly = reader.ReadString();

                    if (reader.Version >= 2)
                    {
                        OutputExpectedPaginationAttributesFiles = reader.ReadBoolean();
                        ExpectedPaginationAttributesOutputPath = reader.ReadString();
                    }
                }

                // Freshly loaded object is not dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI40133",
                    "Unable to load pagination files task.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try 
	        {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Save the settings
                    writer.Write(SourceAction);
                    writer.Write(OutputPath);
                    writer.Write(OutputAction);
                    writer.Write(DocumentDataPanelAssembly);
                    writer.Write(OutputExpectedPaginationAttributesFiles);
                    writer.Write(ExpectedPaginationAttributesOutputPath);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
	        }
	        catch (Exception ex)
	        {
		        throw ExtractException.CreateComVisible("ELI40134",
                    "Unable to save paginate files task.", ex);
	        }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Creates the <see cref="IPaginationDocumentDataPanel"/> that should be used to edit data
        /// for documents in the pagination pane.
        /// </summary>
        /// <param name="paginationDocumentDataPanelAssembly"></param>
        /// <returns>The <see cref="IPaginationDocumentDataPanel"/> that should be used to edit data
        /// for documents in the pagination pane.</returns>
        IPaginationDocumentDataPanel CreateDocumentDataPanel(string paginationDocumentDataPanelAssembly)
        {
            if (paginationDocumentDataPanelAssembly.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
            {
                ITagUtility tagUtility = (ITagUtility)new FAMTagManager();

                // Initialize the root directory the DataEntry framework should use when resolving
                // relative paths.
                DataEntryMethods.SolutionRootDirectory =
                    Path.GetDirectoryName(paginationDocumentDataPanelAssembly);

                // Load the configuration settings from file.
                ConfigSettings<Extract.DataEntry.Properties.Settings> config =
                    new ConfigSettings<Extract.DataEntry.Properties.Settings>(
                        paginationDocumentDataPanelAssembly, null, false, false, tagUtility);

                // Retrieve the name of the DEP assembly
                string dataEntryPanelFileName = DataEntryMethods.ResolvePath(
                    config.Settings.DataEntryPanelFileName);

                //// Create the data entry control host from the specified assembly
                DataEntryDocumentDataPanel dataEntryControlHost =
                    UtilityMethods.CreateTypeFromAssembly<DataEntryDocumentDataPanel>(dataEntryPanelFileName);

                config.ApplyObjectSettings(dataEntryControlHost);

                _activeDataEntryConfig
                    = _defaultDataEntryConfig
                    = new DataEntryConfiguration(config, dataEntryControlHost);

                dataEntryControlHost.SetDatabaseConnections(GetDatabaseConnections());

                return dataEntryControlHost;
            }
            else
            {
                // May be null if the an IPaginationDocumentDataPanel is not specified to be used in
                // this workflow.
                return UtilityMethods.CreateTypeFromAssembly<IPaginationDocumentDataPanel>(
                        paginationDocumentDataPanelAssembly);
            }
        }

        #endregion Private Members
    }
}