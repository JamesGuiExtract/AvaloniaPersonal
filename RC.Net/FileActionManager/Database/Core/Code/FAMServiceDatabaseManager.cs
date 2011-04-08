using Extract.Database;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using CurrentFAMServiceDB = Extract.FileActionManager.Database.FAMServiceDatabaseV6;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// Manages the FAMService Database. Provides updating functionality and other methods
    /// for working with the FAMService Database.
    /// </summary>
    public class FAMServiceDatabaseManager : IDatabaseSchemaUpdater
    {
        #region Constants

        /// <summary>
        /// The object name use in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMServiceDatabaseManager).ToString();

        /// <summary>
        /// The setting key for the current fam service database schema
        /// </summary>
        public static readonly string ServiceDBSchemaVersionKey = "ServiceDBSchemaVersion";

        /// <summary>
        /// The current service database schema version.
        /// </summary>
        public static readonly int CurrentSchemaVersion = FAMServiceDatabase.CurrentSchemaVersion;

        /// <summary>
        /// The class that manages this schema and can perform upgrades to the latest schema.
        /// </summary>
        public static readonly string DBSchemaManager = _OBJECT_NAME;

        /// <summary>
        /// The setting key for the sleep time on startup
        /// </summary>
        public static readonly string SleepTimeOnStartupKey = "SleepTimeOnStart";

        /// <summary>
        /// The default sleep time the service should use when starting (default is 2 minutes)
        /// </summary>
        public static readonly int DefaultSleepTimeOnStartup = 120000;

        /// <summary>
        /// The setting key for the default number of files to process for all fps files.
        /// </summary>
        public static readonly string NumberOfFilesToProcessGlobalKey =
            "NumberOfFilesToProcessPerFAMInstance";

        /// <summary>
        /// The default number of files to process before respawning the FAMProcess
        /// <para><b>Note:</b></para>
        /// A value of 0 indicates that the process should keep processing until it is
        /// stopped and will not be respawned. Negative values are not allowed.
        /// </summary>
        public static readonly int DefaultNumberOfFilesToProcess = 0;

        /// <summary>
        /// The settings key for the dependent services list.
        /// </summary>
        public static readonly string DependentServicesKey = "DependentServices";

        /// <summary>
        /// Array used to trim quotes from file names in the FPS file table.
        /// </summary>
        static readonly char[] _TRIM_QUOTES = new char[] { '"' };

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path to the database file for the FAMService.
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
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseManager"/> class.
        /// </summary>
        public FAMServiceDatabaseManager()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseManager"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public FAMServiceDatabaseManager(string fileName)
        {
            try
            {
                _databaseFile = string.IsNullOrWhiteSpace(fileName) ? null : fileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31075", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseManager"/> class.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        public FAMServiceDatabaseManager(SqlCeConnection connection)
        {
            try
            {
                SetDatabaseConnection(connection);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31158", ex);
            }
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the name of the database file.
        /// <para><b>Note:</b></para>
        /// Cannot set database file to <see langword="null"/>
        /// or <see cref="string.Empty"/>
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
                    ExtractException.Assert("ELI31076", "Database file name cannot be null or empty.",
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
                    throw ExtractException.AsExtractException("ELI31162", ex);
                }
            }
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public Dictionary<string, string> Settings
        {
            get
            {
                FAMServiceDatabase db = null;
                try
                {
                    if (_connection != null)
                    {
                        db = new FAMServiceDatabase(_connection);
                    }
                    else
                    {
                        db = new FAMServiceDatabase(_databaseFile);
                    }

                    return db.Settings.ToDictionary(s => s.Name, s => s.Value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31161", ex);
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
                    File.Delete(_databaseFile);
                }

                bool created = false;
                if (_connection == null || _connection.State == ConnectionState.Closed)
                {
                    using (var serviceDB = new CurrentFAMServiceDB(_databaseFile))
                    {
                        if (!serviceDB.DatabaseExists())
                        {
                            // Create the DB and initialize the settings table
                            serviceDB.CreateDatabase();
                            serviceDB.Settings.InsertAllOnSubmit<Settings>(
                                BuildListOfDefaultSettings());
                            serviceDB.SubmitChanges(ConflictMode.FailOnFirstConflict);
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
                throw ExtractException.AsExtractException("ELI31077", ex);
            }
        }

        /// <summary>
        /// Gets a collection containing the data from the FPSFile table in the service database.
        /// </summary>
        /// <param name="ignoreZeroRows">If <see langword="true"/> then any row whose
        /// number of instances value is 0 will be ignored.</param>
        /// <returns>The collection of rows in the FPS file table.</returns>
        public ReadOnlyCollection<FpsFileTableData> GetFpsFileData(bool ignoreZeroRows)
        {
            CurrentFAMServiceDB db = null;
            try
            {
                if (_connection != null)
                {
                    db = new CurrentFAMServiceDB(_connection);
                }
                else
                {
                    db = new CurrentFAMServiceDB(_databaseFile);
                }

                var returnList = new List<FpsFileTableData>();
                var fpsFiles = db.FpsFile.Select(f =>
                        new FpsFileTableData(f.FileName.Trim(_TRIM_QUOTES),
                            f.NumberOfInstances,
                            f.NumberOfFilesToProcess));

                if (fpsFiles.Count() > 0)
                {
                    foreach (var data in fpsFiles)
                    {
                        if (!ignoreZeroRows || data.NumberOfInstances > 0)
                        {
                            returnList.Add(data);
                        }
                    }
                }

                return returnList.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31078", ex);
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
        }

        /// <summary>
        /// Builds the list of default settings.
        /// </summary>
        /// <returns></returns>
        static List<Settings> BuildListOfDefaultSettings()
        {
            var items = new List<Settings>();

            items.Add(new Settings()
            {
                Name = SleepTimeOnStartupKey,
                Value = DefaultSleepTimeOnStartup.ToString(CultureInfo.InvariantCulture)
            });
            items.Add(new Settings()
            {
                Name = DependentServicesKey,
                Value = ""
            });
            items.Add(new Settings()
            {
                Name = NumberOfFilesToProcessGlobalKey,
                Value = DefaultNumberOfFilesToProcess.ToString(CultureInfo.InvariantCulture)
            });
            items.Add(new Settings()
            {
                Name = ServiceDBSchemaVersionKey,
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
                FAMServiceDatabase db = null;
                try
                {
                    if (_connection != null)
                    {
                        db = new FAMServiceDatabase(_connection);
                    }
                    else
                    {
                        db = new FAMServiceDatabase(_databaseFile);
                    }

                    var settings = db.Settings;
                    var schemaVersion = from s in settings
                                        where s.Name == ServiceDBSchemaVersionKey
                                        select s.Value;
                    var count = schemaVersion.Count();
                    if (count != 1)
                    {
                        var ee = new ExtractException("ELI31079",
                            count > 1 ? "Should only be 1 schema version entry in database." :
                            "No schema version found in database.");
                        throw ee;
                    }
                    int version;
                    if (!int.TryParse(schemaVersion.First(), out version))
                    {
                        var ee = new ExtractException("ELI31080",
                            "Invalid schema version number format.");
                        ee.AddDebugData("Schema Version Number", schemaVersion.First(), false);
                        throw ee;
                    }
                    _versionNumber = version;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31081", ex);
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
        /// Copies the settings table values from the specified backup file to
        /// the current database.
        /// </summary>
        /// <param name="backupFile">The backup file to copy from.</param>
        void CopySettings(string backupFile)
        {
            using (var currentDb = new FAMServiceDatabaseV5(_databaseFile))
            {
                using (var oldDb = new FAMServiceDatabase(backupFile))
                {
                    var currentSettings = currentDb.Settings;

                    foreach (Settings table in oldDb.Settings)
                    {
                        // Do not copy schema version or schema manager key
                        if (table.Name.Equals(ServiceDBSchemaVersionKey, StringComparison.OrdinalIgnoreCase)
                            || table.Name.Equals(DatabaseHelperMethods.DatabaseSchemaManagerKey, StringComparison.OrdinalIgnoreCase))
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

        #region IDatabaseSchemaUpdater Members

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
                    throw ExtractException.AsExtractException("ELI31083", ex);
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
                    throw ExtractException.AsExtractException("ELI31166", ex);
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
                    throw new ExtractException("ELI31159",
                        "This schema updater only works on SqlCe connections.");
                }
                _connection = sqlConnection;
                _databaseFile = _connection.Database;
                _versionNumber = 0;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31160", ex);
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
                            case 2:
                                UpdateFromVersion2SchemaTo6();
                                break;

                            case 3:
                            case 4:
                                UpdateFromVersion3Or4SchemaTo6(backUpFileName);
                                break;

                            case 5:
                                UpdateFromVersion5SchemaTo6();
                                break;

                            default:
                                var ee = new ExtractException("ELI31085",
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
                throw ExtractException.AsExtractException("ELI31084", ex);
            }
        }

        #endregion

        #region Database Schema Update Methods

        /// <summary>
        /// Updates the database from the version 2 schema to the version 6 schema.
        /// <para><b>Note:</b></para>
        /// This should not be called unles <see cref="BackupDatabase"/> has been
        /// called first.
        /// </summary>
        void UpdateFromVersion2SchemaTo6()
        {
            var oldFpsFiles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var oldDb = new FAMServiceDatabaseV2(_databaseFile))
            {
                foreach (var oldTable in oldDb.FpsFile)
                {
                    string fileName = oldTable.FileName;

                    // Get the current count
                    int fileCount = 0;
                    oldFpsFiles.TryGetValue(fileName, out fileCount);

                    // Increment appropriately
                    fileCount += oldTable.AutoStart ? 1 : 0;

                    // Set the new count
                    oldFpsFiles[fileName] = fileCount;
                }

            }

            var fpsFileData = new List<FpsFileTableV6>();
            foreach (var pair in oldFpsFiles)
            {
                fpsFileData.Add(new FpsFileTableV6()
                    {
                        FileName = pair.Key,
                        NumberOfInstances = pair.Value,
                        NumberOfFilesToProcess = -1
                    });
            }

            UpdateToVersion6(fpsFileData);
        }

        /// <summary>
        /// Updates the database from either version 3 or 4 schema to the version 6 schema.
        /// <para><b>Note:</b></para>
        /// This should not be called unles <see cref="BackupDatabase"/> has been
        /// called first. The <see cref="string"/> returned from the call should be
        /// passed into this method.
        /// </summary>
        /// <param name="backupFile">The name of the backup file.</param>
        void UpdateFromVersion3Or4SchemaTo6(string backupFile)
        {
            var oldFpsFiles = new Dictionary<string, Tuple<int, int, bool>>(
                StringComparer.OrdinalIgnoreCase);
            using (var oldDb = new FAMServiceDatabaseV4(_databaseFile))
            {
                foreach (var oldTable in oldDb.FpsFile)
                {
                    string fileName = oldTable.FileName;
                    int increment = oldTable.AutoStart ? 1 : 0;
                    int numberOfFiles = 0;
                    if (string.IsNullOrWhiteSpace(oldTable.NumberOfFilesToProcess)
                        || !int.TryParse(oldTable.NumberOfFilesToProcess, out numberOfFiles))
                    {
                        numberOfFiles = -1;
                    }

                    Tuple<int, int, bool> fileData = null;
                    if (oldFpsFiles.TryGetValue(fileName, out fileData))
                    {
                        int count = fileData.Item1 + (oldTable.AutoStart ? 1 : 0);
                        bool countChanged = false;
                        if (numberOfFiles < fileData.Item2)
                        {
                            countChanged = true;
                        }
                        else
                        {
                            numberOfFiles = fileData.Item2;
                        }
                        fileData = new Tuple<int, int, bool>(count, numberOfFiles,
                            fileData.Item3 | countChanged);
                    }
                    else
                    {
                        fileData = new Tuple<int, int, bool>(increment, numberOfFiles, false);
                    }

                    // Set the new count
                    oldFpsFiles[fileName] = fileData;
                }
            }

            var filesWithDifferentCounts = new List<string>();
            var fpsFileData = new List<FpsFileTableV6>();
            foreach (var pair in oldFpsFiles)
            {
                var fileData = pair.Value;
                fpsFileData.Add(new FpsFileTableV6()
                    {
                        FileName = pair.Key,
                        NumberOfInstances = fileData.Item1,
                        NumberOfFilesToProcess = fileData.Item2
                    });
                if (fileData.Item3)
                {
                    filesWithDifferentCounts.Add(pair.Key);
                }
            }

            if (filesWithDifferentCounts.Count > 0)
            {
                string listOfFiles = string.Join(", ", filesWithDifferentCounts);
                var ee = new ExtractException("ELI31082",
                    "Application Trace: Service database had FPS files with different number of files to process.");
                ee.AddDebugData("Old Service Database", backupFile, false);
                ee.AddDebugData("New Service Database", _databaseFile, false);
                ee.AddDebugData("FPS Files With Different Number Of Files", listOfFiles, false);
                ee.Log();
            }

            UpdateToVersion6(fpsFileData);
        }

        /// <summary>
        /// Updates the database from version 5 schema to the version 6 schema.
        /// <para><b>Note:</b></para>
        /// This should not be called unles <see cref="BackupDatabase"/> has been
        /// called first.
        /// </summary>
        void UpdateFromVersion5SchemaTo6()
        {
            var fpsFileData = new List<FpsFileTableV6>();
            using (var oldDb = new FAMServiceDatabaseV5(_databaseFile))
            {
                // Set all null number of files to process to -1
                oldDb.ExecuteCommand("UPDATE [FPSFile] SET [NumberOfFilesToProcess] = '-1' "
                    + "WHERE [NumberOfFilesToProcess] IS NULL");

                // Read the data from the old FPS file table and create data entries
                // for the new FPS file table
                foreach (var oldTable in oldDb.FpsFile)
                {
                    int numberToProcess = 0;
                    if (string.IsNullOrWhiteSpace(oldTable.NumberOfFilesToProcess)
                        || !int.TryParse(oldTable.NumberOfFilesToProcess, out numberToProcess))
                    {
                        numberToProcess = -1;
                    }

                    fpsFileData.Add(new FpsFileTableV6()
                    {
                        FileName = oldTable.FileName,
                        NumberOfInstances = oldTable.NumberOfInstances,
                        NumberOfFilesToProcess = numberToProcess
                    });
                }
            }

            UpdateToVersion6(fpsFileData);
        }

        /// <summary>
        /// Updates the database to version 6
        /// </summary>
        /// <param name="fpsFileData">The data to add to the FPS file table.</param>
        void UpdateToVersion6(IEnumerable<FpsFileTableV6> fpsFileData)
        {
            FAMServiceDatabaseV6 currentDb = null;
            DbTransaction trans = null;
            try
            {
                currentDb = new FAMServiceDatabaseV6(_databaseFile);
                if (currentDb.Connection.State != ConnectionState.Open)
                {
                    currentDb.Connection.Open();
                }
                trans = currentDb.Connection.BeginTransaction();
                currentDb.Transaction = trans;

                // Drop the FPS file table
                currentDb.ExecuteCommand("DROP TABLE [FPSFile]");

                // Create the version 6 FPS file table
                currentDb.ExecuteCommand("CREATE TABLE FPSFile ("
                    + "[ID] INT IDENTITY (1,1) PRIMARY KEY,"
                    + "[FileName] NVARCHAR(512), "
                    + "[NumberOfFilesToProcess] INT DEFAULT -1 NOT NULL, "
                    + "[NumberOfInstances] INT DEFAULT 1 NOT NULL)");

                foreach (var dataRow in fpsFileData)
                {
                    currentDb.ExecuteCommand(string.Concat("INSERT INTO [FPSFile] ",
                        "([FileName], [NumberOfFilesToProcess], [NumberOfInstances]) ",
                        "VALUES('", dataRow.FileName, "', ", dataRow.NumberOfFilesToProcess,
                        ", ", dataRow.NumberOfInstances, ")"));
                }

                // If there is no schema manager defined, add it
                if (currentDb.Settings
                    .Count(s => s.Name == DatabaseHelperMethods.DatabaseSchemaManagerKey) == 0)
                {
                    currentDb.Settings.InsertOnSubmit(new Settings()
                    {
                        Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                        Value = DBSchemaManager
                    });
                }

                // Update the schema version to 6
                var setting = currentDb.Settings
                    .Where(s => s.Name == ServiceDBSchemaVersionKey)
                    .FirstOrDefault();
                if (setting.Value == null)
                {
                    var ee = new ExtractException("ELI32017",
                        "No Service db schema version key found.");
                    ee.AddDebugData("Database File", _databaseFile, false);
                    throw ee;
                }
                setting.Value = "6";

                currentDb.SubmitChanges(ConflictMode.FailOnFirstConflict);

                trans.Commit();
            }
            catch (Exception ex)
            {
                if (trans != null)
                {
                    trans.Rollback();
                }

                throw ex.AsExtract("ELI32018");
            }
            finally
            {
                if (trans != null)
                {
                    trans.Dispose();
                }
                if (currentDb != null)
                {
                    currentDb.Dispose();
                }
            }
        }

        #endregion Database Schema Update Methods
    }
}
