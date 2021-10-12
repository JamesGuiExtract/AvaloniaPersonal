using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;

namespace Extract.LabResultsCustomComponents
{
    #region AllVersions

    /// <summary>
    /// Class wrapper for LabDE order mapper database so it can be accessed in a type safe
    /// manner via LINQ.
    /// </summary>
    public class LabDEOrderMapperDatabase : DataContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperDatabase"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public LabDEOrderMapperDatabase(string fileName)
            : base(SqlCompactMethods.BuildDBConnectionString(fileName))
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperDatabase"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public LabDEOrderMapperDatabase(SqlCeConnection connection)
            : base(connection)
        {
            Initialize();
        }

        void Initialize()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.LabDECoreObjects,
                    "ELI31223", GetType().ToString());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31224", ex);
            }
        }

        /// <summary>
        /// Gets the Settings table.
        /// </summary>
        public Table<Settings> Settings
        {
            get
            {
                return GetTable<Settings>();
            }
        }
    }

    #endregion AllVersion

    #region V2

    /// <summary>
    /// Mapping for the alternate test name table
    /// </summary>
    [Table(Name = "AternateTestName")]
    public class AlternateTestNameV2
    {
        /// <summary>
        /// The alternate test name.
        /// </summary>
        private string _Name;

        /// <summary>
        /// The test code the alternate name maps to
        /// </summary>
        private string _TestCode;

        /// <summary>
        /// The ID column for this table
        /// </summary>
        private long _ID;

        /// <summary>
        /// Foreign key relation to the LabTest table (relates TestCode to TestCode)
        /// </summary>
        private EntityRef<LabTestV2> _LabTest;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateTestNameV2"/> class.
        /// </summary>
        public AlternateTestNameV2()
        {
            _LabTest = default(EntityRef<LabTestV2>);
        }

        /// <summary>
        /// Gets/sets the test name for the row.
        /// </summary>
        [Column(Storage = "_Name", DbType = "NVarChar(50)")]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if ((_Name != value))
                {
                    _Name = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the test code for the row.
        /// </summary>
        [Column(Storage = "_TestCode", DbType = "NVarChar(25) NOT NULL", CanBeNull = false)]
        public string TestCode
        {
            get
            {
                return _TestCode;
            }
            set
            {
                try
                {
                    if ((_TestCode != value))
                    {
                        if (_LabTest.HasLoadedOrAssignedValue)
                        {
                            throw new ForeignKeyReferenceAlreadyHasValueException();
                        }
                        _TestCode = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31225", ex);
                }
            }
        }

        /// <summary>
        /// Gets/sets the ID for the row.
        /// </summary>
        [Column(Storage = "_ID", AutoSync = AutoSync.OnInsert, DbType = "BigInt NOT NULL IDENTITY",
            IsPrimaryKey = true, IsDbGenerated = true)]
        public long ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if ((_ID != value))
                {
                    _ID = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the LabTest table that contains the foreign key relation.
        /// </summary>
        [Association(Name = "FK__AlternateTestName__0000000000000111", Storage = "_LabTest",
            ThisKey = "TestCode", OtherKey = "TestCode", IsForeignKey = true, DeleteOnNull = true)]
        public LabTestV2 LabTest
        {
            get
            {
                return _LabTest.Entity;
            }
            set
            {
                try
                {
                    LabTestV2 previousValue = _LabTest.Entity;
                    if (((previousValue != value)
                                || (_LabTest.HasLoadedOrAssignedValue == false)))
                    {
                        if ((previousValue != null))
                        {
                            _LabTest.Entity = null;
                            previousValue.AlternateTestName.Remove(this);
                        }
                        _LabTest.Entity = value;
                        if ((value != null))
                        {
                            value.AlternateTestName.Add(this);
                            _TestCode = value.TestCode;
                        }
                        else
                        {
                            _TestCode = default(string);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31226", ex);
                }
            }
        }
    }

    /// <summary>
    /// Mapping for the LabOrder table.
    /// </summary>
    [Table(Name = "LabOrder")]
    public partial class LabOrderV2
    {
        /// <summary>
        /// The lab order code.
        /// </summary>
        private string _Code;

        /// <summary>
        /// The lab order name.
        /// </summary>
        private string _Name;

        /// <summary>
        /// The epic code for the order.
        /// </summary>
        private string _EpicCode;

        /// <summary>
        /// Foreign key relation to the LabOrderTest table.
        /// </summary>
        private EntitySet<LabOrderTestV2> _LabOrderTest;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabOrderV2"/> class.
        /// </summary>
        public LabOrderV2()
        {
            _LabOrderTest = new EntitySet<LabOrderTestV2>(
                new Action<LabOrderTestV2>(attach_LabOrderTest), new Action<LabOrderTestV2>(detach_LabOrderTest));
        }

        /// <summary>
        /// Gets/sets the code for this row.
        /// </summary>
        [Column(Storage = "_Code", DbType = "NVarChar(25) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public string Code
        {
            get
            {
                return _Code;
            }
            set
            {
                if ((_Code != value))
                {
                    _Code = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the name for this row.
        /// </summary>
        [Column(Storage = "_Name", DbType = "NVarChar(50)")]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if ((_Name != value))
                {
                    _Name = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the epic code for this row.
        /// </summary>
        [Column(Storage = "_EpicCode", DbType = "NVarChar(25)")]
        public string EpicCode
        {
            get
            {
                return _EpicCode;
            }
            set
            {
                if ((_EpicCode != value))
                {
                    _EpicCode = value;
                }
            }
        }

        /// <summary>
        /// Gets/set the LabOrderTest table foreign key relation.
        /// </summary>
        [Association(Name = "FK__LabOrderTest__00000000000000FE", Storage = "_LabOrderTest",
            ThisKey = "Code", OtherKey = "OrderCode", DeleteRule = "CASCADE")]
        public EntitySet<LabOrderTestV2> LabOrderTest
        {
            get
            {
                return _LabOrderTest;
            }
        }

        /// <summary>
        /// Attaches to the LabOrderTest table.
        /// </summary>
        /// <param name="entity">The entity to attach the table to.</param>
        private void attach_LabOrderTest(LabOrderTestV2 entity)
        {
            entity.LabOrder = this;
        }

        /// <summary>
        /// Detaches from the LabOrderTest table.
        /// </summary>
        /// <param name="entity">The entity to detach the table from.</param>
        private void detach_LabOrderTest(LabOrderTestV2 entity)
        {
            entity.LabOrder = null;
        }
    }

    /// <summary>
    /// Mapping for the LabOrderTest table.
    /// </summary>
    [Table(Name = "LabOrderTest")]
    public partial class LabOrderTestV2
    {
        /// <summary>
        /// Order code for the test.
        /// </summary>
        private string _OrderCode;

        /// <summary>
        /// Test code.
        /// </summary>
        private string _TestCode;

        /// <summary>
        /// Indicates whether the test is mandatory or not.
        /// </summary>
        private bool _Mandatory;

        /// <summary>
        /// ID - primary key for the table.
        /// </summary>
        private long _ID;

        /// <summary>
        /// Foreign key relation to the LabTest table.
        /// </summary>
        private EntityRef<LabTestV2> _LabTest;

        /// <summary>
        /// Foreign key relation to the LabOrder table
        /// </summary>
        private EntityRef<LabOrderV2> _LabOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabOrderTestV2"/> class.
        /// </summary>
        public LabOrderTestV2()
        {
            _LabTest = default(EntityRef<LabTestV2>);
            _LabOrder = default(EntityRef<LabOrderV2>);
        }

        /// <summary>
        /// Gets/sets the order code for this row.
        /// </summary>
        [Column(Storage = "_OrderCode", DbType = "NVarChar(25)")]
        public string OrderCode
        {
            get
            {
                return _OrderCode;
            }
            set
            {
                try
                {
                    if ((_OrderCode != value))
                    {
                        if (_LabOrder.HasLoadedOrAssignedValue)
                        {
                            throw new ForeignKeyReferenceAlreadyHasValueException();
                        }
                        _OrderCode = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31227", ex);
                }
            }
        }

        /// <summary>
        /// Gets/sets the test code for this row.
        /// </summary>
        [Column(Storage = "_TestCode", DbType = "NVarChar(25)")]
        public string TestCode
        {
            get
            {
                return _TestCode;
            }
            set
            {
                try
                {
                    if ((_TestCode != value))
                    {
                        if (_LabTest.HasLoadedOrAssignedValue)
                        {
                            throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
                        }
                        _TestCode = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31228", ex);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether this row is mandatory.
        /// </summary>
        [Column(Storage = "_Mandatory", DbType = "Bit NOT NULL")]
        public bool Mandatory
        {
            get
            {
                return _Mandatory;
            }
            set
            {
                if ((_Mandatory != value))
                {
                    _Mandatory = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the ID for this row.
        /// </summary>
        [Column(Storage = "_ID", AutoSync = AutoSync.OnInsert, DbType = "BigInt NOT NULL IDENTITY",
            IsPrimaryKey = true, IsDbGenerated = true)]
        public long ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if ((_ID != value))
                {
                    _ID = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the LabTest table for the foreign key relationship.
        /// </summary>
        [Association(Name = "FK__LabOrderTest__00000000000000F7", Storage = "_LabTest",
            ThisKey = "TestCode", OtherKey = "TestCode", IsForeignKey = true)]
        public LabTestV2 LabTest
        {
            get
            {
                return _LabTest.Entity;
            }
            set
            {
                try
                {
                    LabTestV2 previousValue = _LabTest.Entity;
                    if (((previousValue != value)
                                || (_LabTest.HasLoadedOrAssignedValue == false)))
                    {
                        if ((previousValue != null))
                        {
                            _LabTest.Entity = null;
                            previousValue.LabOrderTest.Remove(this);
                        }
                        _LabTest.Entity = value;
                        if ((value != null))
                        {
                            value.LabOrderTest.Add(this);
                            _TestCode = value.TestCode;
                        }
                        else
                        {
                            _TestCode = default(string);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31229", ex);
                }
            }
        }

        /// <summary>
        /// Gets/sets the LabOrder table for the foreign key relationship.
        /// </summary>
        [Association(Name = "FK__LabOrderTest__00000000000000FE", Storage = "_LabOrder",
            ThisKey = "OrderCode", OtherKey = "Code", IsForeignKey = true)]
        public LabOrderV2 LabOrder
        {
            get
            {
                return _LabOrder.Entity;
            }
            set
            {
                try
                {
                    LabOrderV2 previousValue = _LabOrder.Entity;
                    if (((previousValue != value)
                                || (_LabOrder.HasLoadedOrAssignedValue == false)))
                    {
                        if ((previousValue != null))
                        {
                            _LabOrder.Entity = null;
                            previousValue.LabOrderTest.Remove(this);
                        }
                        _LabOrder.Entity = value;
                        if ((value != null))
                        {
                            value.LabOrderTest.Add(this);
                            _OrderCode = value.Code;
                        }
                        else
                        {
                            _OrderCode = default(string);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31230", ex);
                }
            }
        }
    }

    /// <summary>
    /// Mapping for the LabTest table.
    /// </summary>
    [Table(Name = "LabTest")]
    public partial class LabTestV2
    {
        /// <summary>
        /// The test code.
        /// </summary>
        private string _TestCode;

        /// <summary>
        /// The official name for the test.
        /// </summary>
        private string _OfficialName;

        /// <summary>
        /// Foreign key mapping to the alternate test name table.
        /// </summary>
        private EntitySet<AlternateTestNameV2> _AlternateTestName;

        /// <summary>
        /// Foreign key mapping to the LabOrderTest table.
        /// </summary>
        private EntitySet<LabOrderTestV2> _LabOrderTest;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabTestV2"/> class.
        /// </summary>
        public LabTestV2()
        {
            _AlternateTestName = new EntitySet<AlternateTestNameV2>(new Action<AlternateTestNameV2>(
                attach_AlternateTestName), new Action<AlternateTestNameV2>(detach_AlternateTestName));
            _LabOrderTest = new EntitySet<LabOrderTestV2>(
                new Action<LabOrderTestV2>(attach_LabOrderTest), new Action<LabOrderTestV2>(detach_LabOrderTest));
        }

        /// <summary>
        /// Gets/sets the test code for this row.
        /// </summary>
        [Column(Storage = "_TestCode", DbType = "NVarChar(25) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public string TestCode
        {
            get
            {
                return _TestCode;
            }
            set
            {
                if ((_TestCode != value))
                {
                    _TestCode = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the official name for this row.
        /// </summary>
        [Column(Storage = "_OfficialName", DbType = "NVarChar(255)")]
        public string OfficialName
        {
            get
            {
                return _OfficialName;
            }
            set
            {
                if ((_OfficialName != value))
                {
                    _OfficialName = value;
                }
            }
        }

        /// <summary>
        /// Get/sets the AlternateTestName table for the foreign key relationship.
        /// </summary>
        [Association(Name = "FK__AlternateTestName__0000000000000111", Storage = "_AlternateTestName",
            ThisKey = "TestCode", OtherKey = "TestCode", DeleteRule = "CASCADE")]
        public EntitySet<AlternateTestNameV2> AlternateTestName
        {
            get
            {
                return _AlternateTestName;
            }
        }

        /// <summary>
        /// Gets/sets the LabOrderTest table for the foreign key relationship.
        /// </summary>
        [Association(Name = "FK__LabOrderTest__00000000000000F7", Storage = "_LabOrderTest",
            ThisKey = "TestCode", OtherKey = "TestCode", DeleteRule = "CASCADE")]
        public EntitySet<LabOrderTestV2> LabOrderTest
        {
            get
            {
                return _LabOrderTest;
            }
        }

        /// <summary>
        /// Attaches to the AlternateTestName table.
        /// </summary>
        /// <param name="entity">The entity to attach the table to.</param>
        private void attach_AlternateTestName(AlternateTestNameV2 entity)
        {
            entity.LabTest = this;
        }

        /// <summary>
        /// Detaches from the AlternateTestName table.
        /// </summary>
        /// <param name="entity">The entity to detach the table from.</param>
        private void detach_AlternateTestName(AlternateTestNameV2 entity)
        {
            entity.LabTest = null;
        }

        /// <summary>
        /// Attaches to the LabOrderTest table.
        /// </summary>
        /// <param name="entity">The entity to attach the table to.</param>
        private void attach_LabOrderTest(LabOrderTestV2 entity)
        {
            entity.LabTest = this;
        }

        /// <summary>
        /// Detaches from the LabOrderTest table.
        /// </summary>
        /// <param name="entity">The entity to detach the table from.</param>
        private void detach_LabOrderTest(LabOrderTestV2 entity)
        {
            entity.LabTest = null;
        }
    }

    /// <summary>
    /// Class wrapper for LabDE order mapper database so it can be accessed in a type safe
    /// manner via LINQ.
    /// </summary>
    public class LabDEOrderMapperDatabaseV2 : LabDEOrderMapperDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperDatabaseV2"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public LabDEOrderMapperDatabaseV2(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperDatabaseV2"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public LabDEOrderMapperDatabaseV2(SqlCeConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets the alternate test name table.
        /// </summary>
        /// <value>The the alternate test name table..</value>
        public Table<AlternateTestNameV2> AlternateTestName
        {
            get
            {
                return GetTable<AlternateTestNameV2>();
            }
        }

        /// <summary>
        /// Gets the lab order table.
        /// </summary>
        /// <value>The lab order table.</value>
        public Table<LabOrderV2> LabOrder
        {
            get
            {
                return GetTable<LabOrderV2>();
            }
        }

        /// <summary>
        /// Gets the lab order test table.
        /// </summary>
        /// <value>The lab order test table.</value>
        public Table<LabOrderTestV2> LabOrderTest
        {
            get
            {
                return GetTable<LabOrderTestV2>();
            }
        }

        /// <summary>
        /// Gets the lab test table.
        /// </summary>
        /// <value>The lab test table.</value>
        public Table<LabTestV2> LabTest
        {
            get
            {
                return GetTable<LabTestV2>();
            }
        }
    }

    #endregion V2

    /// <summary>
    /// Lab DE order mapper database schema manager class
    /// </summary>
    public class OrderMapperDatabaseSchemaManager : IDatabaseSchemaManager
    {
        #region Constants

        /// <summary>
        /// The object name use in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(OrderMapperDatabaseSchemaManager).ToString();

        /// <summary>
        /// The setting key for the current order mapper database schema
        /// </summary>
        public static readonly string OrderMapperSchemaVersionKey = "OrderMapperSchemaVersion";

        /// <summary>
        /// The current order mapper database schema version.
        /// </summary>
        public static readonly int CurrentSchemaVersion = 2;

        /// <summary>
        /// The class that manages this schema and can perform upgrades to the latest schema.
        /// </summary>
        public static readonly string DBSchemaManager = _OBJECT_NAME;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path to the database file for the order mapper.
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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderMapperDatabaseSchemaManager"/> class.
        /// </summary>
        public OrderMapperDatabaseSchemaManager()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.LabDECoreObjects,
                    "ELI31231", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31232", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderMapperDatabaseSchemaManager"/> class.
        /// </summary>
        /// <param name="fileName">Name of the SDF file.</param>
        public OrderMapperDatabaseSchemaManager(string fileName)
            : this()
        {
            _databaseFile = fileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderMapperDatabaseSchemaManager"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public OrderMapperDatabaseSchemaManager(SqlCeConnection connection)
            : this()
        {
            SetDatabaseConnection(connection);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the settings from the settings table.
        /// </summary>
        /// <value>The settings from the settings table.</value>
        public Dictionary<string, string> Settings
        {
            get
            {
                using LabDEOrderMapperDatabase db = GetDbConnection<LabDEOrderMapperDatabase>();
                return db.Settings.ToDictionary(s => s.Name, s => s.Value);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the db connection of the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of connection to retrieve.</typeparam>
        /// <returns>The connection.</returns>
        T GetDbConnection<T>() where T : LabDEOrderMapperDatabase
        {
            if (_connection != null)
            {
                return typeof(T).GetConstructor(new Type[] { typeof(SqlCeConnection) }).Invoke(
                    new object[] { _connection }) as T;
            }
            else
            {
                return typeof(T).GetConstructor(new Type[] { typeof(string) }).Invoke(
                    new object[] { _databaseFile }) as T;
            }
        }

        /// <summary>
        /// Will create a database if does not exist.
        /// </summary>
        /// <returns><see langword="true"/> if a database was created, otherwise returns
        /// <see langword="false"/></returns>
        public bool CreateDatabase()
        {
            try
            {
                if (!File.Exists(_databaseFile))
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(_databaseFile));
                }

                bool created = false;
                using (var orderMapperDB = new LabDEOrderMapperDatabaseV2(_databaseFile))
                {
                    if (!orderMapperDB.DatabaseExists())
                    {
                        // Create the DB and initialize the settings table
                        orderMapperDB.CreateDatabase();
                        orderMapperDB.Settings.InsertAllOnSubmit<Settings>(
                            BuildListOfDefaultSettings());
                        orderMapperDB.SubmitChanges(ConflictMode.FailOnFirstConflict);
                        created = true;
                    }
                }

                // Reset version number
                _versionNumber = 0;
                return created;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31234", ex);
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
                Name = OrderMapperSchemaVersionKey,
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
        /// <returns>The schema version for the database.</returns>
        // This is better suited as a method since it performs significant
        // computation.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetSchemaVersion()
        {
            try
            {
                if (_versionNumber == 0)
                {
                    using (var db = GetDbConnection<LabDEOrderMapperDatabase>())
                    {
                        var settings = db.Settings;
                        var schemaVersion = from s in settings
                                            where s.Name == OrderMapperSchemaVersionKey
                                            select s.Value;
                        int count = 0;
                        try
                        {
                            count = schemaVersion.Count();
                        }
                        catch
                        {
                            // Exception indicates there is no settings table
                            _versionNumber = 1;
                            return _versionNumber;
                        }

                        if (count != 1)
                        {
                            var ee = new ExtractException("ELI31235",
                                count > 1 ? "Should only be 1 schema version entry in database." :
                                "No schema version found in database.");
                            throw ee;
                        }
                        int version;
                        if (!int.TryParse(schemaVersion.First(), out version))
                        {
                            var ee = new ExtractException("ELI31236",
                                "Invalid schema version number format.");
                            ee.AddDebugData("Schema Version Number", schemaVersion.First(), false);
                            throw ee;
                        }
                        _versionNumber = version;
                    }
                }

                return _versionNumber;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31245", ex);
            }
        }

        /// <summary>
        /// Computes the timestamped file name for the backup file and moves the
        /// current database file to that location.
        /// </summary>
        /// <returns>The path to the backup file.</returns>
        public string BackupDatabase()
        {
            try
            {
                string backupFile = FileSystemMethods.BuildTimeStampedBackupFileName(_databaseFile, true);

                File.Copy(_databaseFile, backupFile, false);
                return backupFile;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31237", ex);
            }
        }

        /// <summary>
        /// Updates LabDE schema from version 1 to version 2.
        /// </summary>
        void UpdateFromVersion1ToVersion2Schema()
        {
            // Changes from version 1 to version 2 - added settings table, set schema
            // manager and set schema version to 2
            using (var db = GetDbConnection<LabDEOrderMapperDatabase>())
            {
                if (db.Connection.State != System.Data.ConnectionState.Open)
                {
                    db.Connection.Open();
                }

                using (var transaction = db.Connection.BeginTransaction())
                {
                    string query = "CREATE TABLE Settings (Name NVARCHAR(100) PRIMARY KEY, "
                        + "Value NVARCHAR(512)); ";
                    db.ExecuteCommand(query);
                    query = "ALTER TABLE [AlternateTestName] ADD COLUMN "
                        + "ID BIGINT IDENTITY(1,1) PRIMARY KEY; ";
                    db.ExecuteCommand(query);
                    query = "ALTER TABLE [LabOrderTest] ADD COLUMN "
                        + "ID BIGINT IDENTITY(1,1) PRIMARY KEY;";
                    db.ExecuteCommand(query);

                    db.Settings.InsertAllOnSubmit(new Settings[]
                    {
                        new Settings()
                        {
                            Name=OrderMapperSchemaVersionKey,
                            Value="2"
                        },
                        new Settings()
                        {
                            Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                            Value = DBSchemaManager
                        }
                    });
                    db.SubmitChanges();

                    transaction.Commit();
                }
            }
        }

        // Replace the schema manager with OrderMapperSqliteDatabaseManager
        private void UpdateFromVersion2ToVersion3Schema()
        {
            using LabDEOrderMapperDatabase db = GetDbConnection<LabDEOrderMapperDatabase>();

            if (db.Connection.State != System.Data.ConnectionState.Open)
            {
                db.Connection.Open();
            }
            using DbTransaction trans = db.Connection.BeginTransaction();
            db.Transaction = trans;

            var schemaManager = db.Settings
                .Where(s => s.Name == DatabaseHelperMethods.DatabaseSchemaManagerKey)
                .FirstOrDefault();

            if (schemaManager is null)
            {
                db.Settings.InsertOnSubmit(new Settings()
                {
                    Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                    Value = typeof(OrderMapperSqliteDatabaseManager).ToString()
                });
            }
            else
            {
                schemaManager.Value = typeof(OrderMapperSqliteDatabaseManager).ToString();
            }

            // Update the schema version to 3
            var setting = db.Settings
                .Where(s => s.Name == OrderMapperSchemaVersionKey)
                .FirstOrDefault();
            if (setting.Value is null)
            {
                var ee = new ExtractException("ELI51918",
                    "No context tags db schema version key found.");
                ee.AddDebugData("Database File", _databaseFile, false);
                throw ee;
            }
            setting.Value = "3";

            db.SubmitChanges(ConflictMode.FailOnFirstConflict);
            trans.Commit();
        }

        #endregion Methods

        #region IDatabaseSchemaManager Members

        /// <summary>
        /// Sets the database connection to be used by the schema updater.
        /// </summary>
        /// <param name="connection"></param>
        /// <value>The database connection.</value>
        public void SetDatabaseConnection(DbConnection connection)
        {
            try
            {
                _ = connection ?? throw new ArgumentNullException(nameof(connection));

                var ceConnection = connection as SqlCeConnection;
                if (ceConnection == null)
                {
                    throw new InvalidCastException(
                        "Supplied database connection is not a SqlCeConnection.");
                }

                _connection = ceConnection;
                _databaseFile = ceConnection.Database;
                _versionNumber = 0;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31238", ex);
            }
        }

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
                return GetSchemaVersion() < CurrentSchemaVersion + 1;
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
                return GetSchemaVersion() > CurrentSchemaVersion;
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
        /// the update task. Must not be <see langword="null"/>. Cancellation
        /// is up to the implementer, there is no guarantee that the update task is
        /// cancellable.</param>
        /// <returns>
        /// A handle to the task that is updating the schema. The task will have a result
        /// <see cref="string"/>. This result should contain the path to the backed up copy
        /// of the database before it was updated to the latest schema.
        /// </returns>
        public Task<string> BeginUpdateToLatestSchema(IProgressStatus progressStatus,
            CancellationTokenSource cancelTokenSource)
        {
            try
            {
                _ = cancelTokenSource ?? throw new ArgumentNullException(nameof(cancelTokenSource));

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

                    string backUpFileName = BackupDatabase();

                    while (version < 3)
                    {
                        switch (version)
                        {
                            case 1:
                                UpdateFromVersion1ToVersion2Schema();
                                break;
                            case 2:
                                UpdateFromVersion2ToVersion3Schema();
                                break;

                            default:
                                ExtractException.ThrowLogicException("ELI31241");
                                break;
                        }
                        _versionNumber = 0; // Force an actual query of the DB in GetSchemaVersion
                        version = GetSchemaVersion();
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
                throw ExtractException.AsExtractException("ELI31242", ex);
            }
        }

        /// <summary>
        /// Gets or sets the SQLCDBEditorPlugin implementation(s) that should completely replace the
        /// normal SQLCDBEditor UI (no tables, queries or tabs)
        /// </summary>
        public IEnumerable<object> UIReplacementPlugins => Enumerable.Empty<object>();

        #endregion
    }
}
