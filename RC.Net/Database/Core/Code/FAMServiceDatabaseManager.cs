using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;

namespace Extract.Database
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
        /// The path to this assembly.
        /// </summary>
        static readonly string _ASSEMBLY_LOCATION =
            Assembly.GetAssembly(typeof(FAMServiceDatabaseManager)).Location;

        /// <summary>
        /// The default FAM service database name.
        /// </summary>
        static readonly string _DEFAULT_FILE_NAME = Path.Combine(
            Path.GetDirectoryName(_ASSEMBLY_LOCATION), "ESFAMService.sdf");

        /// <summary>
        /// The setting key for the current fam service database schema
        /// </summary>
        public static readonly string ServiceDBSchemaVersionKey = "ServiceDBSchemaVersion";

        /// <summary>
        /// The current service database schema version.
        /// </summary>
        public static readonly int CurrentSchemaVersion = 5;

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

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path to the database file for the FAMService.
        /// </summary>
        string _databaseFile;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseManager"/> class.
        /// </summary>
        public FAMServiceDatabaseManager()
            : this(null)
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
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI31074", _OBJECT_NAME);

                _databaseFile =
                    string.IsNullOrWhiteSpace(fileName) ? _DEFAULT_FILE_NAME : fileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31075", ex);
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
                ExtractException.Assert("ELI31076", "Database file name cannot be null or empty.",
                    !string.IsNullOrWhiteSpace(value));
                _databaseFile = value;
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
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId="1#")]
        public bool CreateDatabase(bool backup, out string backupFile)
        {
            try
            {
                string backupFileName = null;
                if (!File.Exists(_databaseFile))
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(_databaseFile));
                }
                else if (backup)
                {
                    backupFileName = BackupDatabase();
                }

                bool created = false;
                using (var serviceDB = new FAMServiceDatabaseV5(_databaseFile))
                {
                    if (!serviceDB.DatabaseExists())
                    {
                        // Create the DB and initialize the settings table
                        serviceDB.CreateDatabase();
                        serviceDB.Settings.InsertAllOnSubmit<SettingsTable>(
                            BuildListOfDefaultSettings());
                        serviceDB.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        created = true;
                    }
                }

                backupFile = backupFileName;
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
        /// number of times to use value is 0 will be ignored.</param>
        /// <returns>The collection of rows in the FPS file table.</returns>
        public ReadOnlyCollection<FpsFileTableData> GetFpsFileData(bool ignoreZeroRows)
        {
            try
            {
                var returnList = new List<FpsFileTableData>();
                using (var db = new FAMServiceDatabaseV5(_databaseFile))
                {
                    var fpsFiles = db.FpsFile.Select(f =>
                            new FpsFileTableData(f.FileName, f.NumberOfTimesToUse,
                                f.NumberOfFilesToProcess));

                    if (fpsFiles.Count() > 0)
                    {
                        foreach (var data in fpsFiles)
                        {
                            if (!ignoreZeroRows || data.NumberOfTimesToUse > 0)
                            {
                                returnList.Add(data);
                            }
                        }
                    }

                    return returnList.AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31078", ex);
            }
        }

        /// <summary>
        /// Builds the list of default settings.
        /// </summary>
        /// <returns></returns>
        static List<SettingsTable> BuildListOfDefaultSettings()
        {
            var items = new List<SettingsTable>();

            items.Add(new SettingsTable()
            {
                Name = SleepTimeOnStartupKey,
                Value = DefaultSleepTimeOnStartup.ToString(CultureInfo.InvariantCulture)
            });
            items.Add(new SettingsTable()
            {
                Name = DependentServicesKey,
                Value = ""
            });
            items.Add(new SettingsTable()
            {
                Name = NumberOfFilesToProcessGlobalKey,
                Value = DefaultNumberOfFilesToProcess.ToString(CultureInfo.InvariantCulture)
            });
            items.Add(new SettingsTable()
            {
                Name = ServiceDBSchemaVersionKey,
                Value = CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture)
            });
            items.Add(new SettingsTable()
            {
                Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                Value = DBSchemaManager
            });

            return items;
        }

        /// <summary>
        /// Gets the schema version.
        /// </summary>
        /// <returns>The schema version for the database.</returns>
        // This is better suited as a method since it performs significant
        // computation.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetSchemaVersion()
        {
            try
            {
                using (var db = new FAMServiceDatabase(_databaseFile))
                {
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

                    return version;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31081", ex);
            }
        }

        /// <summary>
        /// Computes the timestamped file name for the backup file and moves the
        /// current database file to that location.
        /// </summary>
        /// <returns>The path to the backup file.</returns>
        string BackupDatabase()
        {
            string backupFile = FileSystemMethods.BuildTimeStampedBackupFileName(_databaseFile, true);

            FileSystemMethods.MoveFile(_databaseFile, backupFile, false);
            return backupFile;
        }

        /// <summary>
        /// Updates the database from the version 2 schema to the version 5 schema.
        /// <para><b>Note:</b></para>
        /// This should not be called unles <see cref="BackupDatabase"/> has been
        /// called first. The <see cref="string"/> returned from the call should be
        /// passed into this method.
        /// </summary>
        /// <param name="backupFile">The name of the backup file.</param>
        void UpdateFromVersion2Schema(string backupFile)
        {
            CopySettings(backupFile);
            var oldFpsFiles = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var oldDb = new FAMServiceDatabaseV2(backupFile))
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

            var newFpsFiles = new List<FpsFileTableV5>();
            foreach (var pair in oldFpsFiles)
            {
                newFpsFiles.Add(new FpsFileTableV5()
                    {
                        FileName = pair.Key,
                        NumberOfTimesToUse = pair.Value,
                        NumberOfFilesToProcess = "0"
                    });
            }
            using (var currentDb = new FAMServiceDatabaseV5(_databaseFile))
            {
                currentDb.FpsFile.InsertAllOnSubmit(newFpsFiles);
                currentDb.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
        }

        /// <summary>
        /// Updates the database from either version 3 or 4 schema to the version 5 schema.
        /// <para><b>Note:</b></para>
        /// This should not be called unles <see cref="BackupDatabase"/> has been
        /// called first. The <see cref="string"/> returned from the call should be
        /// passed into this method.
        /// </summary>
        /// <param name="backupFile">The name of the backup file.</param>
        void UpdateFromVersion3Or4Schema(string backupFile)
        {
            CopySettings(backupFile);

            var oldFpsFiles = new Dictionary<string, Tuple<int, int, bool>>(
                StringComparer.OrdinalIgnoreCase);
            using (var oldDb = new FAMServiceDatabaseV4(backupFile))
            {
                foreach (var oldTable in oldDb.FpsFile)
                {
                    string fileName = oldTable.FileName;
                    int increment = oldTable.AutoStart ? 1 : 0;
                    int numberOfFiles = 0;
                    if (!int.TryParse(oldTable.NumberOfFilesToProcess, out numberOfFiles))
                    {
                        numberOfFiles = 0;
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
            var newFpsFiles = new List<FpsFileTableV5>();
            foreach (var pair in oldFpsFiles)
            {
                var fileData = pair.Value;
                newFpsFiles.Add(new FpsFileTableV5()
                    {
                        FileName = pair.Key,
                        NumberOfTimesToUse = fileData.Item1,
                        NumberOfFilesToProcess = fileData.Item2.ToString(CultureInfo.InvariantCulture)
                    });
                if (fileData.Item3)
                {
                    filesWithDifferentCounts.Add(pair.Key);
                }
            }

            if (filesWithDifferentCounts.Count > 0)
            {
                string listOfFiles =
                    StringMethods.ConvertArrayToDelimitedList(filesWithDifferentCounts, ", ");
                var ee = new ExtractException("ELI31082",
                    "Application Trace: Service database had FPS files with different number of files to process.");
                ee.AddDebugData("Old Service Database", backupFile, false);
                ee.AddDebugData("New Service Database", _databaseFile, false);
                ee.AddDebugData("FPS Files With Different Number Of Files", listOfFiles, false);
                ee.Log();
            }

            using (var currentDb = new FAMServiceDatabaseV5(_databaseFile))
            {
                currentDb.FpsFile.InsertAllOnSubmit(newFpsFiles);
                currentDb.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
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

                    foreach (SettingsTable table in oldDb.Settings)
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

                    string backUpFileName = null;
                    CreateDatabase(true, out backUpFileName);

                    switch (version)
                    {
                        case 2:
                            UpdateFromVersion2Schema(backUpFileName);
                            break;

                        case 3:
                        case 4:
                            UpdateFromVersion3Or4Schema(backUpFileName);
                            break;

                        default:
                            ExtractException.ThrowLogicException("ELI31085");
                            break;
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
    }
}
