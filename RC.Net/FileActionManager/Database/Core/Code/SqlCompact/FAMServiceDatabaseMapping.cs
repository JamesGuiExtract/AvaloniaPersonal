using Extract.Database;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlServerCe;

namespace Extract.FileActionManager.Database
{
    #region AllVersions

    /// <summary>
    /// DataContext mapping for the FAMService database. Use this connection
    /// to get the settings table and get the current SchemaVersion
    /// </summary>
    public class FAMServiceDatabase : DataContext
    {
        /// <summary>
        /// The current schema version for the FAM service database
        /// </summary>
        public static readonly int CurrentSchemaVersion = 7;

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabase"/> class.
        /// </summary>
        /// <param name="fileName">The Sql compact file name.</param>
        public FAMServiceDatabase(string fileName) :
            base(SqlCompactMethods.BuildDBConnectionString(fileName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabase"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public FAMServiceDatabase(SqlCeConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets the settings table.
        /// </summary>
        /// <value>The settings table.</value>
        public Table<Settings> Settings
        {
            get
            {
                return GetTable<Settings>();
            }
        }
    }

    #endregion AllVersions

    #region Version6

    /// <summary>
    /// Table mapping for the version 5 schema FPSFile table
    /// </summary>
    [Table(Name = "FPSFile")]
    public class FpsFileTableV6
    {
        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>The ID.</value>
        [Column(Name = "ID", IsPrimaryKey = true,
            DbType = "INT NOT NULL IDENTITY",IsDbGenerated = true)]
        public int ID { get; set;}

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Column(Name = "FileName", DbType = "NVARCHAR(512)")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets the number of instances to launch
        /// </summary>
        /// <value>The number of instances to launch.</value>
        [Column(Name = "NumberOfInstances", DbType = "INT DEFAULT 1 NOT NULL")]
        public int NumberOfInstances { get; set; }

        /// <summary>
        /// Gets the number of files to process.
        /// </summary>
        /// <value>The number of files to process.</value>
        [Column(Name = "NumberOfFilesToProcess", DbType = "INT DEFAULT -1 NOT NULL")]
        public int NumberOfFilesToProcess { get; set; }

    }

    /// <summary>
    /// DataContext mapping for the Version 6 FAMService database.
    /// </summary>
    public class FAMServiceDatabaseV6 : FAMServiceDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV5"/> class.
        /// </summary>
        /// <param name="fileName">The SQL Compact file name.</param>
        public FAMServiceDatabaseV6(string fileName) :
            base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV5"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public FAMServiceDatabaseV6(SqlCeConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets the FPS file table.
        /// </summary>
        /// <value>The FPS file table.</value>
        public Table<FpsFileTableV6> FpsFile
        {
            get
            {
                return GetTable<FpsFileTableV6>();
            }
        }
    }

    #endregion Version6

    #region Version5

    /// <summary>
    /// Table mapping for the version 5 schema FPSFile table
    /// </summary>
    [Table(Name = "FPSFile")]
    public class FpsFileTableV5
    {
        /// <summary>
        /// The ID for the FPS file row.
        /// </summary>
        private int _id;

        /// <summary>
        /// The name of the FPS file.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The number of files to process before respawning the FPS file.
        /// </summary>
        string _numberOfFilesToProcess;

        /// <summary>
        /// The number of times to use the FPS file (ie the number of instances to launch).
        /// </summary>
        int _numberOfInstances;

        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>The ID.</value>
        [Column(Name = "ID", IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true)]
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Column(Name = "FileName", DbType = "NVARCHAR(512)")]
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        /// <summary>
        /// Gets the number of files to process.
        /// </summary>
        /// <value>The number of files to process.</value>
        [Column(Name = "NumberOfFilesToProcess", DbType = "NVARCHAR(15)")]
        public string NumberOfFilesToProcess
        {
            get
            {
                return _numberOfFilesToProcess;
            }
            set
            {
                _numberOfFilesToProcess = value;
            }
        }

        /// <summary>
        /// Gets the number of times to use.
        /// </summary>
        /// <value>The number of times to use.</value>
        [Column(Name = "NumberOfInstances", DbType = "INT DEFAULT 1 NOT NULL")]
        public int NumberOfInstances
        {
            get
            {
                return _numberOfInstances;
            }
            set
            {
                _numberOfInstances = value;
            }
        }
    }

    /// <summary>
    /// DataContext mapping for the Version 5 FAMService database.
    /// </summary>
    public class FAMServiceDatabaseV5 : FAMServiceDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV5"/> class.
        /// </summary>
        /// <param name="fileName">The SQL Compact file name.</param>
        public FAMServiceDatabaseV5(string fileName) :
            base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV5"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public FAMServiceDatabaseV5(SqlCeConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets the FPS file table.
        /// </summary>
        /// <value>The FPS file table.</value>
        public Table<FpsFileTableV5> FpsFile
        {
            get
            {
                return GetTable<FpsFileTableV5>();
            }
        }
    }

    #endregion Version5

    #region Version4

    /// <summary>
    /// Table mapping for the version 4 schema FPSFile table
    /// </summary>
    [Table(Name = "FPSFile")]
    public class FpsFileTableV4
    {
        /// <summary>
        /// Whether the FPS file should be launched when the service starts.
        /// </summary>
        bool _autoStart;

        /// <summary>
        /// The ID for the FPS file row.
        /// </summary>
        int _id;

        /// <summary>
        /// The name of the FPS file.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The number of files to process before respawning the FPS file.
        /// </summary>
        string _numberOfFilesToProcess;

        /// <summary>
        /// Gets AutoStart value.
        /// </summary>
        /// <value>
        ///	<see langword="true"/> if AutoStart; otherwise, <see langword="false"/>.
        /// </value>
        [Column(Name = "AutoStart", DbType = "BIT DEFAULT 1 NOT NULL")]
        public bool AutoStart
        {
            get
            {
                return _autoStart;
            }
            set
            {
                _autoStart = value;
            }
        }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>The ID.</value>
        [Column(Name = "ID", IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true)]
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Column(Name = "FileName", DbType = "NVARCHAR(512)")]
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        /// <summary>
        /// Gets the number of files to process.
        /// </summary>
        /// <value>The number of files to process.</value>
        [Column(Name = "NumberOfFilesToProcess", DbType = "NVARCHAR(50)")]
        public string NumberOfFilesToProcess
        {
            get
            {
                return _numberOfFilesToProcess;
            }
            set
            {
                _numberOfFilesToProcess = value;
            }
        }
    }

    /// <summary>
    /// DataContext mapping for the Version 4 FAMService database.
    /// </summary>
    public class FAMServiceDatabaseV4 : FAMServiceDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV4"/> class.
        /// </summary>
        /// <param name="fileName">The Sql compact file name.</param>
        public FAMServiceDatabaseV4(string fileName) :
            base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV4"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public FAMServiceDatabaseV4(SqlCeConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets the FPS file table.
        /// </summary>
        /// <value>The FPS file table.</value>
        public Table<FpsFileTableV4> FpsFile
        {
            get
            {
                return GetTable<FpsFileTableV4>();
            }
        }
    }

    #endregion Version4

    #region Version3

    /// <summary>
    /// Table mapping for the version 3 schema FPSFile table
    /// </summary>
    [Table(Name = "FPSFile")]
    public class FpsFileTableV3
    {
        /// <summary>
        /// Whether the FPS file should be launched when the service starts.
        /// </summary>
        bool _autoStart;

        /// <summary>
        /// The ID for the FPS file row.
        /// </summary>
        int _id;

        /// <summary>
        /// The name of the FPS file.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The number of files to process before respawning the FPS file.
        /// </summary>
        string _numberOfFilesToProcess;

        /// <summary>
        /// Gets AutoStart value.
        /// </summary>
        /// <value>
        ///	<see langword="true"/> if AutoStart; otherwise, <see langword="false"/>.
        /// </value>
        [Column(Name = "AutoStart", DbType = "BIT DEFAULT 1 NOT NULL")]
        public bool AutoStart
        {
            get
            {
                return _autoStart;
            }
            set
            {
                _autoStart = value;
            }
        }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>The ID.</value>
        [Column(Name = "ID", IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true)]
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Column(Name = "FileName", DbType = "NVARCHAR(512) NOT NULL")]
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        /// <summary>
        /// Gets the number of files to process.
        /// </summary>
        /// <value>The number of files to process.</value>
        [Column(Name = "NumberOfFilesToProcess", DbType = "NVARCHAR(50)")]
        public string NumberOfFilesToProcess
        {
            get
            {
                return _numberOfFilesToProcess;
            }
            set
            {
                _numberOfFilesToProcess = value;
            }
        }
    }

    /// <summary>
    /// DataContext mapping for the Version 3 FAMService database.
    /// </summary>
    public class FAMServiceDatabaseV3 : FAMServiceDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV3"/> class.
        /// </summary>
        /// <param name="fileName">The Sql compact file name.</param>
        public FAMServiceDatabaseV3(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV3"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public FAMServiceDatabaseV3(SqlCeConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets the FPS file table.
        /// </summary>
        /// <value>The FPS file table.</value>
        public Table<FpsFileTableV3> FpsFile
        {
            get
            {
                return GetTable<FpsFileTableV3>();
            }
        }
    }

    #endregion Version3

    #region Version2

    /// <summary>
    /// Table mapping for the version 2 schema FPSFile table
    /// </summary>
    [Table(Name = "FPSFile")]
    public class FpsFileTableV2
    {
        /// <summary>
        /// Whether the FPS file should be launched when the service starts.
        /// </summary>
        bool _autoStart;

        /// <summary>
        /// The ID for the FPS file row.
        /// </summary>
        int _id;

        /// <summary>
        /// The name of the FPS file.
        /// </summary>
        string _fileName;

        /// <summary>
        /// Gets AutoStart value.
        /// </summary>
        /// <value>
        ///	<see langword="true"/> if AutoStart; otherwise, <see langword="false"/>.
        /// </value>
        [Column(Name = "AutoStart", DbType = "BIT DEFAULT 1 NOT NULL")]
        public bool AutoStart
        {
            get
            {
                return _autoStart;
            }
            set
            {
                _autoStart = value;
            }
        }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>The ID.</value>
        [Column(Name = "ID", IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true)]
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Column(Name = "FileName", DbType = "NVARCHAR(512)")]
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }
    }

    /// <summary>
    /// DataContext mapping for the Version 2 FAMService database.
    /// </summary>
    public class FAMServiceDatabaseV2 : FAMServiceDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV2"/> class.
        /// </summary>
        /// <param name="fileName">The Sql compact file name.</param>
        public FAMServiceDatabaseV2(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMServiceDatabaseV2"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public FAMServiceDatabaseV2(SqlCeConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets the FPS file table.
        /// </summary>
        /// <value>The FPS file table.</value>
        public Table<FpsFileTableV2> FpsFile
        {
            get
            {
                return GetTable<FpsFileTableV2>();
            }
        }
    }

    #endregion Version2
}
