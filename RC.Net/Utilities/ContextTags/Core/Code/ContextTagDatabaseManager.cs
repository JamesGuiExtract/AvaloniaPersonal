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

using CurrentContextTagDatabase = Extract.Utilities.ContextTags.ContextTagDatabase;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// Manages the context tags database.
    /// </summary>
    public class ContextTagDatabaseManager : IDatabaseSchemaManager
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
        public static readonly int CurrentSchemaVersion =
            CurrentContextTagDatabase.CurrentSchemaVersion;

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
        SqlCeConnection _connection;

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
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabaseManager"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public ContextTagDatabaseManager(string fileName)
        {
            try
            {
                _databaseFile = string.IsNullOrWhiteSpace(fileName) ? null : fileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37963", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabaseManager"/> class.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        public ContextTagDatabaseManager(SqlCeConnection connection)
        {
            try
            {
                SetDatabaseConnection(connection);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37964", ex);
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
                        _connection = null;
                        _databaseFile = value;
                        _versionNumber = 0;
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
                CurrentContextTagDatabase db = null;
                try
                {
                    if (_connection != null)
                    {
                        db = new CurrentContextTagDatabase(_connection);
                    }
                    else
                    {
                        db = new CurrentContextTagDatabase(_databaseFile);
                    }

                    return db.Settings.ToDictionary(s => s.Name, s => s.Value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI37967", ex);
                }
                finally
                {
                    if (db != null)
                    {
                        db.Dispose();
                    }
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
                if (_connection == null || _connection.State == ConnectionState.Closed)
                {
                    using (var contextTagDB = new CurrentContextTagDatabase(_databaseFile))
                    {
                        if (!contextTagDB.DatabaseExists())
                        {
                            // Create the DB and initialize the settings table
                            contextTagDB.CreateDatabase();

                            // I have not been successful in setting cascade deletes via the
                            // Association attribute in TagValueTableV1. For now, manually re-add
                            // the foreign key constraints to enable the cascade of deletes.
                            contextTagDB.ExecuteCommand(
                                "ALTER TABLE TagValue DROP FK_TagValue_Context");
                            contextTagDB.ExecuteCommand(
                                "ALTER TABLE TagValue ADD CONSTRAINT FK_TagValue_Context " +
                                "FOREIGN KEY (ContextID) REFERENCES Context(ID) ON DELETE CASCADE");
                            contextTagDB.ExecuteCommand(
                                "ALTER TABLE TagValue DROP FK_TagValue_CustomTag");
                            contextTagDB.ExecuteCommand(
                                "ALTER TABLE TagValue ADD CONSTRAINT FK_TagValue_CustomTag " +
                                "FOREIGN KEY (TagID) REFERENCES CustomTag(ID) ON DELETE CASCADE");

                            contextTagDB.ExecuteCommand(
                                "ALTER TABLE Context ADD CONSTRAINT UC_ContextName UNIQUE (Name)");
                            contextTagDB.ExecuteCommand(
                                "ALTER TABLE Context ADD CONSTRAINT UC_ContextFPSFileDir UNIQUE (FPSFileDir)");
                            contextTagDB.ExecuteCommand(
                                "ALTER TABLE CustomTag ADD CONSTRAINT UC_CustomTagName UNIQUE (Name)");

                            contextTagDB.Settings.InsertAllOnSubmit<Settings>(
                                BuildListOfDefaultSettings());
                            contextTagDB.SubmitChanges(ConflictMode.FailOnFirstConflict);
                            created = true;
                        }
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
                CurrentContextTagDatabase db = null;
                try
                {
                    if (_connection != null)
                    {
                        db = new CurrentContextTagDatabase(_connection);
                    }
                    else
                    {
                        db = new CurrentContextTagDatabase(_databaseFile);
                    }

                    var settings = db.Settings;
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
                    int version;
                    if (!int.TryParse(schemaVersion.First(), out version))
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
                finally
                {
                    if (db != null)
                    {
                        db.Dispose();
                    }
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
        /// Copies the settings table values from the specified backup file to the current database.
        /// </summary>
        /// <param name="backupFile">The backup file to copy from.</param>
        void CopySettings(string backupFile)
        {
            using (var currentDb = new CurrentContextTagDatabase(_databaseFile))
            {
                using (var oldDb = new CurrentContextTagDatabase(backupFile))
                {
                    var currentSettings = currentDb.Settings;

                    foreach (Settings table in oldDb.Settings)
                    {
                        // Do not copy schema version or schema manager key
                        if (table.Name.Equals(ContextTagsDBSchemaVersionKey,
                                StringComparison.OrdinalIgnoreCase)
                            || table.Name.Equals(DatabaseHelperMethods.DatabaseSchemaManagerKey,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var setting = from s in currentSettings where s.Name == table.Name select s;
                        if (setting.Count() > 0)
                        {
                            var temp = setting.First();
                            temp.Value = table.Value;
                        }
                    }

                    currentDb.SubmitChanges(ConflictMode.FailOnFirstConflict);
                }
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
                    return GetSchemaVersion() < CurrentSchemaVersion;
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
                    throw new ArgumentNullException("connection");
                }

                var sqlConnection = connection as SqlCeConnection;
                if (sqlConnection == null)
                {
                    throw new ExtractException("ELI37975",
                        "This schema updater only works on SqlCe connections.");
                }
                _connection = sqlConnection;
                _databaseFile = _connection.Database;
                _versionNumber = 0;
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
                if (cancelTokenSource == null)
                {
                    throw new ArgumentNullException("cancelTokenSource");
                }

                int version = GetSchemaVersion();
                if (progressStatus != null)
                {
                    progressStatus.InitProgressStatus("Test", 0, 1, true);
                }

                CancellationToken ct = cancelTokenSource.Token;

                var task = Task.Factory.StartNew<string>(() =>
                {
                    // Check if the task has already been cancelled
                    ct.ThrowIfCancellationRequested();

                    string backUpFileName = BackupDatabase();

                    while (version < CurrentSchemaVersion)
                    {
                        switch (version)
                        {
                            case 1:
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
        /// Gets the type name of an SQLCDBEditorPlugin that should completely replace the normal
        /// SQLCDBEditor UI (no tables, queries or tabs)
        /// </summary>
        public string UIReplacementPlugin
        {
            get
            {
                return "Extract.Utilities.ContextTags.ContextTagsPlugin";
            }
        }

        #endregion IDatabaseSchemaManager Members
    }
}
