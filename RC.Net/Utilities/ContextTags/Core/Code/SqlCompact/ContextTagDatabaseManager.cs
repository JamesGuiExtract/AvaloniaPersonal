using Extract.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// Manages the context tags database.
    /// </summary>
    public class ContextTagDatabaseManager : IDatabaseSchemaManager, IDisposable
    {
        #region Constants

        /// <summary>
        /// The object name use in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ContextTagDatabaseManager).ToString();

        /// <summary>
        /// The setting key for the current context-specific tags database schema
        /// </summary>
        public static readonly string ContextTagsDBSchemaVersionKey = "ContextTagsDBSchemaVersion";

        /// <summary>
        /// The current context-specific tag schema version.
        /// </summary>
        public static readonly int CurrentSchemaVersion = ContextTagDatabase.CurrentSchemaVersion;

        /// <summary>
        /// The class that manages this schema and can perform upgrades to the latest schema.
        /// </summary>
        public static readonly string DBSchemaManager = _OBJECT_NAME;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path to the context-specific tag database file.
        /// </summary>
        string _databaseFile;

        /// <summary>
        /// Db connection associated with this manager.
        /// </summary>
        DbConnection _connection;

        /// <summary>
        /// The connection information for the currently open custom tags database.
        /// </summary>
        DatabaseConnectionInfo _connectionInfo;

        /// <summary>
        /// The currently open <see cref="ContextTagDatabase"/>.
        /// </summary>
        ContextTagDatabase _contextTagDatabase;

        /// <summary>
        /// true to use a temporary local read-only copy of the database or false to open the
        /// database for writing in it's original location.
        /// </summary>
        bool _readonly;

        /// <summary>
        /// The current schema version of the database.
        /// </summary>
        int _versionNumber;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabaseManager"/> class.
        /// </summary>
        public ContextTagDatabaseManager()
            : this(fileName: string.Empty, readOnly: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabaseManager"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="readOnly"><c>true</c> to use a temporary local read-only copy of the
        /// database;  <c>false</c> to open the database for writing in it's original location.
        /// </param>
        public ContextTagDatabaseManager(string fileName, bool readOnly)
        {
            try
            {
                _databaseFile = string.IsNullOrWhiteSpace(fileName) ? null : fileName;
                _readonly = readOnly;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37963", ex);
            }
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the name of the database file.
        /// <para><b>Note:</b></para>
        /// Cannot set database file to <see langword="null"/> or <see cref="string.Empty"/>
        /// </summary>
        /// <value>The name of the database file.</value>
        public string DatabaseFileName
        {
            get
            {
                return _databaseFile;
            }
            set
            {
                try
                {
                    ExtractException.Assert("ELI37965", "Database file name cannot be null or empty.",
                        !string.IsNullOrWhiteSpace(value));
                    if (!value.Equals(_databaseFile, StringComparison.OrdinalIgnoreCase))
                    {
                        ResetDatabase();
                        _databaseFile = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI37966", ex);
                }
            }
        }

        /// <summary>
        /// Gets the values from the Settings table of the database.
        /// </summary>
        /// <value>The values from the Settings table of the database.</value>
        public Dictionary<string, string> Settings
        {
            get
            {
                try
                {
                    return ContextTagDatabase.Settings.ToDictionary(s => s.Name, s => s.Value);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37967");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Will create a database if does not exist. If <paramref name="backup"/>
        /// is <see langword="true"/> then will create a backup copy of the file
        /// before creating a new database.
        /// </summary>
        /// <param name="backup">if set to <see langword="true"/> then will create
        /// a timestamped backup copy of the file before creating a new database.</param>
        /// <returns><see langword="true"/> if a database was created, otherwise returns
        /// <see langword="false"/></returns>
        public bool CreateDatabase(bool backup)
        {
            string temp = null;
            return CreateDatabase(backup, out temp);
        }

        /// <summary>
        /// Will create a database if does not exist. If <paramref name="backup"/>
        /// is <see langword="true"/> then will create a backup copy of the file
        /// before creating a new database.
        /// </summary>
        /// <param name="backup">if set to <see langword="true"/> then will create
        /// a timestamped backup copy of the file before creating a new database.</param>
        /// <param name="backupFile">If the file was backed up, this will contain the name
        /// the backup file.</param>
        /// <returns><see langword="true"/> if a database was created, otherwise returns
        /// <see langword="false"/></returns>
        // Using an out parameter here so that the user can retrieve the name of the backup
        // file that was created if a new database was created and a backup was created.
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public bool CreateDatabase(bool backup, out string backupFile)
        {
            try
            {
                ExtractException.Assert("ELI44952", "Unable to create database with read-only connection!", !_readonly);

                backupFile = null;
                if (!File.Exists(_databaseFile))
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(_databaseFile));
                }
                else if (backup)
                {
                    backupFile = BackupDatabase();
                    FileSystemMethods.DeleteFile(_databaseFile);
                }

                bool created = false;
                if (DbConnection == null || DbConnection.State == ConnectionState.Closed)
                {
                    if (!ContextTagDatabase.DatabaseExists())
                    {
                        // Create the DB and initialize the settings table
                        ContextTagDatabase.CreateDatabase();

                        // I have not been successful in setting cascade deletes via the
                        // Association attribute in TagValueTableV1. For now, manually re-add
                        // the foreign key constraints to enable the cascade of deletes.
                        ContextTagDatabase.ExecuteCommand(
                            "ALTER TABLE TagValue DROP FK_TagValue_Context");
                        ContextTagDatabase.ExecuteCommand(
                            "ALTER TABLE TagValue ADD CONSTRAINT FK_TagValue_Context " +
                            "FOREIGN KEY (ContextID) REFERENCES Context(ID) ON DELETE CASCADE");
                        ContextTagDatabase.ExecuteCommand(
                            "ALTER TABLE TagValue DROP FK_TagValue_CustomTag");
                        ContextTagDatabase.ExecuteCommand(
                            "ALTER TABLE TagValue ADD CONSTRAINT FK_TagValue_CustomTag " +
                            "FOREIGN KEY (TagID) REFERENCES CustomTag(ID) ON DELETE CASCADE");

                        ContextTagDatabase.ExecuteCommand(
                            "ALTER TABLE Context ADD CONSTRAINT UC_ContextName UNIQUE (Name)");
                        ContextTagDatabase.ExecuteCommand(
                            "ALTER TABLE Context ADD CONSTRAINT UC_ContextFPSFileDir UNIQUE (FPSFileDir)");
                        ContextTagDatabase.ExecuteCommand(
                            "ALTER TABLE CustomTag ADD CONSTRAINT UC_CustomTagName UNIQUE (Name)");

                        ContextTagDatabase.Settings.InsertAllOnSubmit<Settings>(
                            BuildListOfDefaultSettings());
                        ContextTagDatabase.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        created = true;
                    }
                }

                // Reset version number
                _versionNumber = 0;
                return created;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37968", ex);
            }
        }

        /// <summary>
        /// Builds the list of default settings.
        /// </summary>
        /// <returns>The list of default settings.</returns>
        static List<Settings> BuildListOfDefaultSettings()
        {
            var items = new List<Settings>();

            items.Add(new Settings()
            {
                Name = ContextTagsDBSchemaVersionKey,
                Value = CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture)
            });
            items.Add(new Settings()
            {
                Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                Value = DBSchemaManager
            });

            return items;
        }

        /// <summary>
        /// Gets the schema version.
        /// </summary>
        /// <param name="forceUpdate">If <see langword="true"/> then will go back
        /// to the database to update the cached schema version number; otherwise
        /// only updates the schema version number if it is 0.</param>
        /// <returns>The schema version for the database.</returns>
        // This is better suited as a method since it performs significant
        // computation.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetSchemaVersion(bool forceUpdate = false)
        {
            if (_versionNumber == 0 || forceUpdate)
            {
                try
                {
                    var settings = ContextTagDatabase.Settings;
                    var schemaVersion = from s in settings
                                    where s.Name == ContextTagsDBSchemaVersionKey
                                    select s.Value;
                    var count = schemaVersion.Count();

                    if (count != 1)
                    {
                        var ee = new ExtractException("ELI37969",
                            count > 1 ? "Should only be 1 schema version entry in database." :
                            "No schema version found in database.");
                        throw ee;
                    }
                    if (!int.TryParse(schemaVersion.First(), out int version))
                    {
                        var ee = new ExtractException("ELI37970",
                            "Invalid schema version number format.");
                        ee.AddDebugData("Schema Version Number", schemaVersion.First(), false);
                        throw ee;
                    }
                    _versionNumber = version;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI37971", ex);
                }
            }

            return _versionNumber;
        }

        /// <summary>
        /// Computes the timestamped file name for the backup file and copies the
        /// current database file to that location.
        /// </summary>
        /// <returns>The path to the backup file.</returns>
        string BackupDatabase()
        {
            string backupFile = FileSystemMethods.BuildTimeStampedBackupFileName(_databaseFile, true);

            File.Copy(_databaseFile, backupFile, true);
            return backupFile;
        }

        /// <summary>
        /// Refreshes data from the database
        /// </summary>
        /// <returns></returns>
        public void RefreshDatabase()
        {
            try
            {
                // In the case that a local read-only database is being used, forcing
                // _contextTagDatabase to be re-created next use will force _connectionInfo's
                // ManagedDBConnection property to be accessed which will for the temp DB copy to be
                // updated if the source has a newer modified date.
                if (_contextTagDatabase != null)
                {
                    _contextTagDatabase.Dispose();
                    _contextTagDatabase = null;
                }

                _versionNumber = 0;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44954");
            }
        }

        #endregion Methods

        #region IDatabaseSchemaManager Members

        /// <summary>
        /// Gets a value indicating whether a database schema update is required.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if an update is required; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsUpdateRequired
        {
            get
            {
                try
                {
                    // Update the schema past the current version to hand-off management to ContextTagsSqliteDatabaseManager
                    return GetSchemaVersion() < 3;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI37973", ex);
                }
            }

        }

        /// <summary>
        /// Gets a value indicating whether the current database schema is of a newer version.
        /// </summary>
        /// <value><see langword="true"/> if the schema is a newer version.</value>
        public bool IsNewerVersion
        {
            get
            {
                try
                {
                    return GetSchemaVersion() > CurrentSchemaVersion;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI37974", ex);
                }
            }
        }

        /// <summary>
        /// Sets the database connection to be used by the schema updater.
        /// </summary>
        /// <param name="connection"></param>
        /// <value>The database connection.</value>
        public void SetDatabaseConnection(DbConnection connection)
        {
            try
            {
                if (connection == null)
                {
                    throw new ArgumentNullException(nameof(connection));
                }

                if (!(connection is SqlCeConnection))
                {
                    throw new ExtractException("ELI37975",
                        "This schema updater only works on SqlCe connections.");
                }

                if (connection != DbConnection)
                {
                    ResetDatabase();
                    _connection = connection;
                    _databaseFile = _connection.Database;
                    _readonly = connection.ConnectionString.IndexOf("Read Only", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37976", ex);
            }
        }

        /// <summary>
        /// Begins the update to latest schema.
        /// </summary>
        /// <param name="progressStatus">The progress status object to update, if
        /// <see langword="null"/> then no progress status will be given. Otherwise it
        /// will be reinitialized to the appropriate number of steps and updated by the
        /// update task as it runs.</param>
        /// <param name="cancelTokenSource">The cancel token that can be used to cancel
        /// the update task. Must not be <see langword="null"/>.</param>
        /// <returns>
        /// A handle to the task that is updating the schema. The task will have a result
        /// <see cref="string"/>. This result should contain the path to the backed up copy
        /// of the database before it was updated to the latest schema.
        /// </returns>
        [CLSCompliant(false)]
        public Task<string> BeginUpdateToLatestSchema(
            IProgressStatus progressStatus, CancellationTokenSource cancelTokenSource)
        {
            try
            {
                ExtractException.Assert("ELI44951", "Unable to update schema with read-only connection!", !_readonly);

                if (cancelTokenSource == null)
                {
                    throw new ArgumentNullException(nameof(cancelTokenSource));
                }

                int version = GetSchemaVersion();
                if (progressStatus != null)
                {
                    progressStatus.InitProgressStatus("Test", 0, 1, true);
                }

                CancellationToken ct = cancelTokenSource.Token;

                var task = Task.Run(() =>
                {
                    // Check if the task has already been cancelled
                    ct.ThrowIfCancellationRequested();

                    // In order for the database to be backed up, the connection must be closed.
                    ResetDatabase();
                    string backUpFileName = BackupDatabase();

                    while (version < 3)
                    {
                        switch (version)
                        {
                            case 1:
                                UpdateFromVersion1SchemaToVersion2();
                                break;
                            case 2:
                                UpdateFromVersion2SchemaToVersion3();
                                break;

                            default:
                                var ee = new ExtractException("ELI37977",
                                    "Unrecognized schema version.");
                                ee.AddDebugData("Schema Version", version, false);
                                throw ee;
                        }

                        version = GetSchemaVersion(true);
                    }

                    if (progressStatus != null)
                    {
                        progressStatus.CompleteCurrentItemGroup();
                    }

                    return backUpFileName;
                });

                return task;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37978", ex);
            }
        }

        /// <summary>
        /// Gets or sets the SQLCDBEditorPlugin implementation(s) that should completely replace the
        /// normal SQLCDBEditor UI (no tables, queries or tabs)
        /// </summary>
        public IEnumerable<object> UIReplacementPlugins
        {
            get
            {
                return new object[] { new ContextTagsPlugin() };
            }
        }

        #endregion IDatabaseSchemaManager Members

        #region Database Schema Update Methods

        /// <summary>
        /// Updates from version 1 schema to version 2
        /// </summary>
        private void UpdateFromVersion1SchemaToVersion2()
        {
            using (var currentDb = new ContextTagDatabase(_databaseFile))
            {
                if (currentDb.Connection.State != ConnectionState.Open)
                {
                    currentDb.Connection.Open();
                }
                using (var trans = currentDb.Connection.BeginTransaction())
                {
                    try
                    {
                        currentDb.ExecuteCommand("ALTER TABLE TagValue DROP CONSTRAINT PK_TagValue");
                        currentDb.ExecuteCommand("ALTER TABLE TagValue ADD COLUMN Workflow NVARCHAR(100) NOT NULL DEFAULT ''");
                        currentDb.ExecuteCommand("ALTER TABLE TagValue ADD CONSTRAINT PK_TagValue PRIMARY KEY([ContextID], [TagID], [Workflow])");

                        // Update the schema version to 2
                        var setting = currentDb.Settings
                            .Where(s => s.Name == ContextTagsDBSchemaVersionKey)
                            .FirstOrDefault();
                        if (setting.Value == null)
                        {
                            var ee = new ExtractException("ELI43186",
                                "No Context tag db schema version key found.");
                            ee.AddDebugData("Database File", _databaseFile, false);
                            throw ee;
                        }
                        setting.Value = "2";
                        currentDb.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        trans.Commit();
                    }
                    catch(Exception ex)
                    {
                        trans.Rollback();
                        throw ex.AsExtract("ELI43185");
                    }
                }
            }
        }

        // Replace the schema manager with ContextTagsSqliteDatabaseManager
        private void UpdateFromVersion2SchemaToVersion3()
        {
            using ContextTagDatabase currentDb = new(_databaseFile);
            if (currentDb.Connection.State != ConnectionState.Open)
            {
                currentDb.Connection.Open();
            }
            using DbTransaction trans = currentDb.Connection.BeginTransaction();
            currentDb.Transaction = trans;

            var schemaManager = currentDb.Settings
                .Where(s => s.Name == DatabaseHelperMethods.DatabaseSchemaManagerKey)
                .FirstOrDefault();

            if (schemaManager is null)
            {
                currentDb.Settings.InsertOnSubmit(new Settings()
                {
                    Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                    Value = ContextTagsSqliteDatabaseManager.DBSchemaManager
                });
            }
            else
            {
                schemaManager.Value = ContextTagsSqliteDatabaseManager.DBSchemaManager;
            }

            // Update the schema version to 3
            var setting = currentDb.Settings
                .Where(s => s.Name == ContextTagsDBSchemaVersionKey)
                .FirstOrDefault();
            if (setting.Value is null)
            {
                var ee = new ExtractException("ELI51830",
                    "No context tags db schema version key found.");
                ee.AddDebugData("Database File", _databaseFile, false);
                throw ee;
            }
            setting.Value = "3";

            currentDb.SubmitChanges(ConflictMode.FailOnFirstConflict);
            trans.Commit();
        }

        #endregion

        #region IDisposable

        /// <overloads>Releases resources used by the <see cref="ContextTagDatabaseManager"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="ContextTagDatabaseManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ContextTagDatabaseManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_contextTagDatabase != null)
                    {
                        _contextTagDatabase.Dispose();
                        _contextTagDatabase = null;
                    }

                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }

                    if (_connectionInfo != null)
                    {
                        _connectionInfo.Dispose();
                        _connectionInfo = null;
                    }
                }
                catch { }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable

        #region Internal Members

        /// <summary>
        /// Gets the <see cref="ContextTagDatabase"/> instance to use.
        /// </summary>
        internal ContextTagDatabase ContextTagDatabase
        {
            get
            {
                if (_contextTagDatabase == null)
                {
                    _contextTagDatabase = (DbConnection == null)
                        ? new ContextTagDatabase(_databaseFile, _readonly)
                        : new ContextTagDatabase(DbConnection);
                }

                return _contextTagDatabase;
            }
        }

        #endregion Internal Members

        #region Private Members

        /// <summary>
        /// Gets the <see cref="DbConnection"/> to use.
        /// </summary>
        DbConnection DbConnection
        {
            get
            {
                if (_connection == null && _connectionInfo == null && File.Exists(_databaseFile))
                {
                    ExtractException.Assert("ELI51886", "Deprecated SQL Compact code", !_readonly);

                    // Make a local copy (SQL compact can't handle network shares from multiple processes)
                    // https://extract.atlassian.net/browse/ISSUE-14936
                    _connectionInfo = new DatabaseConnectionInfo(typeof(SqlCeConnection).AssemblyQualifiedName,
                            SqlCompactMethods.BuildDBConnectionString(_databaseFile))
                    {
                        //UseLocalDbCopy = _readonly
                    };
                }

                return _connection ?? _connectionInfo?.ManagedDbConnection;
            }
        }

        /// <summary>
        /// Reset and dispose of the database connection and related objects.
        /// </summary>
        public void ResetDatabase()
        {
            try
            {
                _contextTagDatabase?.Dispose();
                _contextTagDatabase = null;
                _connection?.Dispose();
                _connection = null;
                _connectionInfo?.Dispose();
                _connectionInfo = null;
                _versionNumber = 0;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45639");
            }
        }

        #endregion Private Members
    }
}
