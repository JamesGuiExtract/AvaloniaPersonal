using Extract.Database;
using Extract.DataEntry;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using Spring.Core.TypeResolution;
using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="DataQueryRuleObject"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("AEF0BBA3-0645-4183-957F-CCC093E1DEDD")]
    [CLSCompliant(false)]
    public interface IDataQueryRuleObject : IAttributeFindingRule, IOutputHandler,
        ICategorizedComponent, IConfigurableObject, ICopyableObject, ILicensedComponent,
        IPersistStream, IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// Gets or sets the data query.
        /// </summary>
        /// <value>
        /// The data query.
        /// </value>
        string Query
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current FAM DB connection should be used.
        /// </summary>
        /// <value><see langword="true"/> if the current FAM DB connection should be used;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool UseFAMDBConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the specified DB connection should be used.
        /// </summary>
        /// <value><see langword="true"/> if the specified DB connection should be used;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool UseSpecifiedDBConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the name of the data source. (Required if SQL query elements are used).
        /// </summary>
        /// <value>
        /// The name of the data source.
        /// </value>
        string DataSourceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the data provider. (Required if SQL query elements are used).
        /// </summary>
        /// <value>
        /// The name of the data provider.
        /// </value>
        string DataProviderName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the database connection string to be used by the query. (Required if SQL
        /// query elements are used).
        /// </summary>
        /// <value>
        /// The database connection string to be used by the query.
        /// </value>
        string DataConnectionString
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IOutputHandler"/> that evaluates a data query against the output
    /// <see cref="IAttribute"/>s. The result of the query is not used but side effects of the query
    /// can perform tasks such as updating a database.
    /// </summary>
    [ComVisible(true)]
    [Guid("9AA5818D-D2A2-4A58-90FE-D86D229C136D")]
    [CLSCompliant(false)]
    public class DataQueryRuleObject : IdentifiableObject, IDataQueryRuleObject, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Data query";

        /// <summary>
        /// Current version.
        /// <para>Version 2: Added UseFAMDbConnection and UseSpecifiedDbConnection.</para>
        /// <para>Version 3: Added OpenSqlCompactReadOnly.</para>
        /// <para>Version 4: Removed OpenSqlCompactReadOnly.</para>
        /// </summary>
        const int _CURRENT_VERSION = 4;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The data query.
        /// </summary>
        string _query;

        /// <summary>
        /// Indicates whether the current FAM DB connection should be used.
        /// </summary>
        bool _useFAMDBConnection;

        /// <summary>
        /// Indicates whether the specified DB connection should be used.
        /// </summary>
        bool _useSpecifiedDBConnection;

        /// <summary>
        /// The <see cref="DatabaseConnectionInfo"/> describing the data source to be used for SQL
        /// query elements.
        /// </summary>
        DatabaseConnectionInfo _databaseConnectionInfo = new DatabaseConnectionInfo();

        /// <summary>
        /// An <see cref="AttributeFinderPathTags"/> to expand any tags in the connection string.
        /// </summary>
        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="DataQueryRuleObject"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueryRuleObject"/> class.
        /// </summary>
        public DataQueryRuleObject()
        {
            try
            {
                _databaseConnectionInfo.PathTags = _pathTags;
                TypeRegistry.RegisterType("Regex", typeof(System.Text.RegularExpressions.Regex));
                TypeRegistry.RegisterType("StringUtils", typeof(Spring.Util.StringUtils));
                TypeRegistry.RegisterType("CultureInfo", typeof(System.Globalization.CultureInfo));
                TypeRegistry.RegisterType("LabDEUtils", typeof(DataEntry.LabDE.LabDEQueryUtilities));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34723");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueryRuleObject"/> class as a copy of
        /// the specified <see paramref="dataQueryRuleObject"/>.
        /// </summary>
        /// <param name="dataQueryRuleObject">The <see cref="DataQueryRuleObject"/> from which
        /// settings should be copied.</param>
        public DataQueryRuleObject(DataQueryRuleObject dataQueryRuleObject) : this()
        {
            try
            {
                CopyFrom(dataQueryRuleObject);
                _databaseConnectionInfo.PathTags = _pathTags;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34724");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the data query.
        /// </summary>
        /// <value>
        /// The data query.
        /// </value>
        public string Query
        {
            get
            {
                return _query;
            }

            set
            {
                try
                {
                    if (value != _query)
                    {
                        _query = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34782");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current FAM DB connection should be used.
        /// </summary>
        /// <value><see langword="true"/> if the current FAM DB connection should be used;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool UseFAMDBConnection
        {
            get
            {
                return _useFAMDBConnection;
            }

            set
            {
                try
                {
                    if (value != _useFAMDBConnection)
                    {
                        _useFAMDBConnection = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37872");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current FAM DB connection should be used.
        /// </summary>
        /// <value><see langword="true"/> if the current FAM DB connection should be used;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool UseSpecifiedDBConnection
        {
            get
            {
                return _useSpecifiedDBConnection;
            }

            set
            {
                try
                {
                    if (value != _useSpecifiedDBConnection)
                    {
                        _useSpecifiedDBConnection = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37873");
                }
            }
        }

        /// <summary>
        /// Gets or set the name of the data source. (Required if SQL query elements are used).
        /// </summary>
        /// <value>
        /// The name of the data source.
        /// </value>
        public string DataSourceName
        {
            get
            {
                return _databaseConnectionInfo.DataSourceName;
            }

            set
            {
                try
                {
                    if (value != _databaseConnectionInfo.DataSourceName)
                    {
                        _databaseConnectionInfo.DataSourceName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34783");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the data provider. (Required if SQL query elements are used).
        /// </summary>
        /// <value>
        /// The name of the data provider.
        /// </value>
        public string DataProviderName
        {
            get
            {
                return _databaseConnectionInfo.DataProviderName;
            }

            set
            {
                try
                {
                    if (value != _databaseConnectionInfo.DataProviderName)
                    {
                        _databaseConnectionInfo.DataProviderName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34784");
                }
            }
        }

        /// <summary>
        /// Gets or sets the database connection string to be used by the query. (Required if SQL
        /// query elements are used).
        /// </summary>
        /// <value>
        /// The database connection string to be used by the query.
        /// </value>
        public string DataConnectionString
        {
            get
            {
                return _databaseConnectionInfo.ConnectionString;
            }

            set
            {
                try
                {
                    if (value != _databaseConnectionInfo.ConnectionString)
                    {
                        _databaseConnectionInfo.ConnectionString = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34785");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseConnectionInfo"/> describing the data source to be
        /// used for SQL query elements.
        /// </summary>
        /// <value>
        /// The database connection info.
        /// </value>
        internal DatabaseConnectionInfo DatabaseConnectionInfo
        {
            get
            {
                return _databaseConnectionInfo;
            }

            set
            {
                _databaseConnectionInfo = value;
            }
        }

        #endregion Properties

        #region IAttributeFindingRule

        /// <summary>
        /// Parses the <see paramref="pDocument"/> and returns a vector of found
        /// <see cref="ComAttribute"/> objects.
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to parse.</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> to indicate processing
        /// progress.</param>
        /// <returns>An <see cref="IUnknownVector"/> of found <see cref="ComAttribute"/>s.</returns>
        public IUnknownVector ParseText(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI37383", _COMPONENT_DESCRIPTION);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pDocument.Attribute.ReportMemoryUsage();

                // Initialize for use in any embedded path tags/functions.
                _pathTags.Document = pDocument;

                QueryResult queryResults =
                    ExecuteQuery(pDocument.Attribute.SubAttributes, pDocument.Text.SourceDocName);

                IUnknownVector returnValue = new IUnknownVector();

                // Iterate all query results and apply the spatial or string value to a new
                // attribute for each result.
                foreach (QueryResult result in queryResults)
                {
                    IAttribute attribute = new ComAttribute();

                    if (result.IsSpatial)
                    {
                        // The query system will have already cloned any spatial values that came
                        // from other attributes, so the spatial string result can be used directly
                        // without any side-effects on other attributes.
                        attribute.Value = result.FirstSpatialString;
                    }
                    else
                    {
                        attribute.Value.ReplaceAndDowngradeToNonSpatial(result.FirstString);
                    }

                    returnValue.PushBack(attribute);
                }

                // So that the garbage collector knows of and properly manages the associated
                // memory from the created return value.
                returnValue.ReportMemoryUsage();

                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37384", "Failed to evaluate data query.");
            }
        }

        #endregion IAttributeFindingRule

        #region IOutputHandler

        /// <summary>
        /// Processes the output (<see paramref="pAttributes"/>) by 
        /// </summary>
        /// <param name="pAttributes">The output to process.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the output is from.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> that can be used to update
        /// processing status.</param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34786", _COMPONENT_DESCRIPTION);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttributes.ReportMemoryUsage();
                pDoc.Text.ReportMemoryUsage();

                // Initialize for use in any embedded path tags/functions.
                _pathTags.Document = pDoc;

                ExecuteQuery(pAttributes, pDoc.Text.SourceDocName);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34787", "Failed to evaluate data query.");
            }
        }

        #endregion IOutputHandler

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="DataQueryRuleObject"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34788", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if the dialog is ok'd
                DataQueryRuleObject cloneOfThis = (DataQueryRuleObject)Clone();

                using (DataQueryRuleObjectSettingsDialog dlg
                    = new DataQueryRuleObjectSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34789", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="DataQueryRuleObject"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DataQueryRuleObject"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new DataQueryRuleObject(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34790",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="DataQueryRuleObject"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as DataQueryRuleObject;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to DataQueryRuleObject");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34791",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

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
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    Query = reader.ReadString();
                    DataSourceName = reader.ReadString();
                    DataProviderName = reader.ReadString();
                    DataConnectionString = reader.ReadString();

                    UseFAMDBConnection = (reader.Version >= 2) ? reader.ReadBoolean() : false;
                    UseSpecifiedDBConnection = (reader.Version >= 2) ? reader.ReadBoolean() :
                        !string.IsNullOrWhiteSpace(DataProviderName);

                    if (reader.Version == 3)
                    {
                        // Removed OpenSqlCompactReadOnly
                        reader.ReadBoolean();
                    }

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34792",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(Query);
                    writer.Write(DataSourceName);
                    writer.Write(DataProviderName);
                    writer.Write(DataConnectionString);

                    writer.Write(UseFAMDBConnection);
                    writer.Write(UseSpecifiedDBConnection);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34793",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is configured; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Query))
                {
                    return false;
                }

                if (UseSpecifiedDBConnection && string.IsNullOrWhiteSpace(DataProviderName))
                {
                    return false;
                }

                if (!UseFAMDBConnection && !UseSpecifiedDBConnection && QueryUsesSQL)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI40325",
                    "Error checking configuration of '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DataQueryRuleObject"/>
        /// </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI37826", ex);
            }
        }

        /// <overloads>Releases all resources used by the <see cref="DataQueryRuleObject"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="DataQueryRuleObject"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects

                // NOTE: Rule objects should really not be disposable as there is no mechanism in
                // the rules engine that is able to dispose of disposable rule objects. However,
                // DatabaseConnectionInfo really only has disposable resources to manage in the case
                // that the ManagedDbConnection property is used (and it is not by this class).
                // IDisposable is therefore implemented here out of diligence and to appease FXCop. 
                if (_databaseConnectionInfo != null)
                {
                    _databaseConnectionInfo.Dispose();
                    _databaseConnectionInfo = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="DataQueryRuleObject"/> instance into this one.
        /// </summary><param name="source">The <see cref="DataQueryRuleObject"/> from which to copy.
        /// </param>
        void CopyFrom(DataQueryRuleObject source)
        {
            Query = source.Query;
            DataSourceName = source.DataSourceName;
            DataProviderName = source.DataProviderName;
            DataConnectionString = source.DataConnectionString;
            UseFAMDBConnection = source.UseFAMDBConnection;
            UseSpecifiedDBConnection = source.UseSpecifiedDBConnection;

            _dirty = true;
        }

        /// <summary>
        /// Gets a value indicating whether the configured query uses an SQL node.
        /// </summary>
        bool QueryUsesSQL
        {
            get
            {
                return Query.IndexOf("<SQL", StringComparison.OrdinalIgnoreCase) != -1;
            }
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IAttribute"/>s the query should be run
        /// against.</param>
        /// <param name="sourceDocName">The source document name.</param>
        /// <returns>The <see cref="QueryResult"/> returned by the query.</returns>
        QueryResult ExecuteQuery(IUnknownVector sourceAttributes, string sourceDocName)
        {
            DbConnection dbConnection = null;

            try
            {
                if (QueryUsesSQL)
                {
                    dbConnection = GetDatabaseConnection();
                }

                QueryResult result = null;
                AttributeStatusInfo.InitializeForQuery(sourceAttributes, sourceDocName, dbConnection);
                using (var query = DataEntryQuery.Create(Query, null, dbConnection))
                {
                    result = query.Evaluate();
                }

                AttributeStatusInfo.ResetData();

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34794");
            }
            finally
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                    dbConnection.Dispose();
                    dbConnection = null;
                }
            }
        }

        /// <summary>
        /// Opens the configured database (if any).
        /// <see paramref="workingCopyFileName"/> and <see paramref="originalFileName"/>
        /// will indicate the working and original database file names.
        /// </summary>
        /// <returns>A <see cref="DbConnection"/> if a database connection was specified;
        /// <see langword="false"/> otherwise.</returns>
        DbConnection GetDatabaseConnection()
        {
            if (UseFAMDBConnection)
            {
                // Initialize a database connection using the last configured
                // IFileProcessingDB settings used this process.
                // NOTE: The connection string may be for a database that has not actually
                // been used. The connection string is based only on the properties of an
                // IFileProcessingDB instance that have been set, not based on a connection
                // that has actually been opened. Thus, for example, if we are running
                // within a RunFPSFile instance with the /ignoreDB flag, even though the
                // FAM instance will not have connected to a database, the database info
                // will still have been loaded from the FPS file and that is what will be
                // used here.
                IFileProcessingDB fileProcessingDB = new FileProcessingDBClass();
                OleDbConnectionStringBuilder oleDbConnectionStringBuilder
                    = new OleDbConnectionStringBuilder(fileProcessingDB.GetLastConnectionStringConfiguredThisProcess());

                string server = (string)oleDbConnectionStringBuilder["Server"];
                string database = (string)oleDbConnectionStringBuilder["Database"];
                var dbConnection = new ExtractRoleConnection(SqlUtil.CreateConnectionString(server, database));
                dbConnection.Open();

                return dbConnection;
            }
            // If a database connection is configured, attempt to open it.
            else if (UseSpecifiedDBConnection &&
                !string.IsNullOrWhiteSpace(DataProviderName) &&
                !string.IsNullOrWhiteSpace(DataConnectionString))
            {
                return DatabaseConnectionInfo.OpenConnection();
            }

            // A database connection has not been configured.
            return null;
        }

        #endregion Private Members
    }
}
