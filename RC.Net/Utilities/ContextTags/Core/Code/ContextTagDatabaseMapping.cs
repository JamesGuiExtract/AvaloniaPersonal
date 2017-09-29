using Extract.Database;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;

namespace Extract.Utilities.ContextTags
{
    #region AllVersions

    /// <summary>
    /// DataContext for the context-specific tag database. Use this connection to get the settings
    /// table and get the current SchemaVersion
    /// </summary>
    public class ContextTagDatabase : DataContext
    {
        /// <summary>
        /// The current schema version for the context-specific tag database
        /// </summary>
        public static readonly int CurrentSchemaVersion = 2;

        /// <summary>
        /// The connection string as of the last call to <see cref="DatabaseDirectory"/>. Used to
        /// determine if <see cref="_databaseDirectory"/> needs to be re-calculated.
        /// </summary>
        string _lastConnectionString;

        /// <summary>
        /// The directory in which this database file resides.
        /// </summary>
        string _databaseDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabase"/> class.
        /// </summary>
        /// <param name="fileName">The Sql compact file name.</param>
        /// <param name="readOnly">Whether to open the database in read-only mode</param>
        public ContextTagDatabase(string fileName, bool readOnly = false) :
            base(SqlCompactMethods.BuildDBConnectionString(fileName, exclusive: false, readOnly: readOnly))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabase"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ContextTagDatabase(DbConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets or sets the settings table.
        /// </summary>
        /// <value>The settings table.</value>
        public Table<Settings> Settings
        {
            get
            {
                return GetTable<Settings>();
            }
        }

        /// <summary>
        /// Gets or sets the Context table.
        /// </summary>
        /// <value>The Context table</value>
        public Table<ContextTableV1> Context
        {
            get
            {
                return GetTable<ContextTableV1>();
            }
        }

        /// <summary>
        /// Gets or sets the CustomTag table.
        /// </summary>
        /// <value>The CustomTag table</value>
        public Table<CustomTagTableV1> CustomTag
        {
            get
            {
                return GetTable<CustomTagTableV1>();
            }
        }

        /// <summary>
        /// Gets or sets the TagValue table.
        /// </summary>
        /// <value>The TagValue table</value>
        public Table<TagValueTableV2> TagValue
        {
            get
            {
                return GetTable<TagValueTableV2>();
            }
        }

        /// <summary>
        /// Gets the directory in which this database file resides.
        /// </summary>
        public string DatabaseDirectory
        {
            get
            {
                try 
	            {
                    // If the connection string has changed since the last time the
                    // DatabaseDirectory was calculated, 
                    if (string.IsNullOrEmpty(_lastConnectionString) ||
                        _lastConnectionString != Connection.ConnectionString)
                    {
                        var connectionStringBuilder = new DbConnectionStringBuilder();
                        connectionStringBuilder.ConnectionString = Connection.ConnectionString;

                        object databaseFile = null;
                        if (!connectionStringBuilder.TryGetValue("Data Source", out databaseFile) &&
                            !connectionStringBuilder.TryGetValue("DataSource", out databaseFile))
                        {
                            ExtractException.ThrowLogicException("ELI38173");
                        }

                        // Attempt to retrieve the directory as a UNC path since UNC paths will
                        // provide a more consistent way of comparing paths.
                        string filename = (string)databaseFile;
                        FileSystemMethods.ConvertToNetworkPath(ref filename, false);
                        _databaseDirectory = Path.GetDirectoryName(filename);

                        _lastConnectionString = Connection.ConnectionString;
                    }

                    return _databaseDirectory;
	            }
	            catch (System.Exception ex)
	            {
		            throw ex.AsExtract("ELI38174");
	            }
            }
        }

        /// <summary>
        /// Indicates that Dispose has been called on this instance. Used to prevent logic
        /// exceptions from being displayed while the editor is being closed.
        /// https://extract.atlassian.net/browse/ISSUE-14429
        /// </summary>
        public bool IsDisposed
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or the name of the context that is mapped to the specified
        /// <see paramref="directory"/>.
        /// </summary>
        /// <value>
        /// The name of the context that is mapped to the specified <see paramref="directory"/> or
        /// <see langword="null"/> if there is no context associated with the directory.
        /// </value>
        public string GetContextNameForDirectory(string directory)
        {
            try
            {
                return Context
                    .Where(context => context.FPSFileDir == directory)
                    .Select(context => context.Name)
                    .FirstOrDefault();
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI38175");
            }
        }

        /// <summary>
        /// Set IsDisposed. Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    #endregion AllVersions

    #region Version1

    /// <summary>
    /// DataContext for the context-specific tag database. Use this connection to get the settings
    /// table and get the current SchemaVersion
    /// </summary>
    public class ContextTagDatabaseV1 : DataContext
    {
        /// <summary>
        /// The current schema version for the context-specific tag database
        /// </summary>
        public static readonly int CurrentSchemaVersion = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabase"/> class.
        /// </summary>
        /// <param name="fileName">The Sql compact file name.</param>
        public ContextTagDatabaseV1(string fileName) :
            base(SqlCompactMethods.BuildDBConnectionString(fileName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabase"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ContextTagDatabaseV1(DbConnection connection)
            : base(connection)
        {
        }

        /// <summary>
        /// Gets or sets the settings table.
        /// </summary>
        /// <value>The settings table.</value>
        public Table<Settings> Settings
        {
            get
            {
                return GetTable<Settings>();
            }
        }

        /// <summary>
        /// Gets or sets the Context table.
        /// </summary>
        /// <value>The Context table</value>
        public Table<ContextTableV1> Context
        {
            get
            {
                return GetTable<ContextTableV1>();
            }
        }

        /// <summary>
        /// Gets or sets the CustomTag table.
        /// </summary>
        /// <value>The CustomTag table</value>
        public Table<CustomTagTableV1> CustomTag
        {
            get
            {
                return GetTable<CustomTagTableV1>();
            }
        }

        /// <summary>
        /// Gets or sets the TagValue table.
        /// </summary>
        /// <value>The TagValue table</value>
        public Table<TagValueTableV1> TagValue
        {
            get
            {
                return GetTable<TagValueTableV1>();
            }
        }


        /// <summary>
        /// Indicates that Dispose has been called on this instance. Used to prevent logic
        /// exceptions from being displayed while the editor is being closed.
        /// https://extract.atlassian.net/browse/ISSUE-14429
        /// </summary>
        public bool IsDisposed
        {
            get;
            protected set;
        }

        /// <summary>
        /// Set IsDisposed. Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Table mapping for the version 1 schema Context table
    /// </summary>
    [Table(Name = "Context")]
    public class ContextTableV1
    {
        /// <summary>
        /// Gets or sets the context ID.
        /// </summary>
        /// <value>The context ID.</value>
        [Column(Name = "ID", IsPrimaryKey = true,
            DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the context name.
        /// </summary>
        /// <value>The context name</value>
        [Column(Name = "Name", DbType = "NVARCHAR(50) NOT NULL")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the FPSFileDir associated with the context.
        /// </summary>
        /// <value>The FPSFileDir associated with the context.</value>
        [Column(Name = "FPSFileDir", DbType = "NVARCHAR(260) NOT NULL")]
        public string FPSFileDir { get; set; }
    }

    /// <summary>
    /// Table mapping for the version 1 schema CustomTag table
    /// </summary>
    [Table(Name = "CustomTag")]
    public class CustomTagTableV1
    {
        /// <summary>
        /// Gets or sets the tag ID.
        /// </summary>
        /// <value>The tag ID.</value>
        [Column(Name = "ID", IsPrimaryKey = true,
            DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the tag name.
        /// </summary>
        /// <value>The tag name.</value>
        [Column(Name = "Name", DbType = "NVARCHAR(50) NOT NULL")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Table mapping for the version 1 schema TagValue table
    /// </summary>
    [Table(Name = "TagValue")]
    public class TagValueTableV1
    {
        /// <summary>
        /// Foreign key relation to the Context table.
        /// </summary>
        EntityRef<ContextTableV1> _context;

        /// <summary>
        /// Foreign key relation to the CustomTag table.
        /// </summary>
        EntityRef<CustomTagTableV1> _customTag;

        /// <summary>
        /// Gets or sets the ID of the context for this tag value.
        /// </summary>
        /// <value>The ID of the context for this tag value.</value>
        [Column(IsPrimaryKey = true, DbType = "INT NOT NULL")]
        public int ContextID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the tag for this tag value.
        /// </summary>
        /// <value>The ID of the tag for this tag value.</value>
        [Column(IsPrimaryKey = true, DbType = "INT NOT NULL")]
        public int TagID {get; set;}

        /// <summary>
        /// Gets or sets the value for this context and tag combination.
        /// </summary>
        /// <value>The value for this context and tag combination.</value>
        [Column(DbType = "NVARCHAR(400) NOT NULL")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the Context the foreign key relation.
        /// </summary>
        // Note that the DeleteRule of Cascade seems to be ineffective here. I think a corresponding
        // EntitySet association is needed on the Context table with the DeleteRule, but that got
        // complicated by the fact that this table uses a composite foreign key.
        // For now there is code in ContextTagDatabaseManager to manually re-added the association
        // via a query to work around this issue.
        [Association(Name = "FK_TagValue_Context", Storage = "_context", ThisKey = "ContextID",
            OtherKey = "ID", IsForeignKey = true, DeleteRule = "Cascade")]
        public ContextTableV1 Context
        {
            get
            {
                return _context.Entity;
            }
            set
            {
                _context.Entity = value;
            }
        }

        /// <summary>
        /// Gets or sets the CustomTag foreign key relation.
        /// </summary>
        // Note that the DeleteRule of Cascade seems to be ineffective here. I think a corresponding
        // EntitySet association is needed on the CustomTag table with the DeleteRule, but that got
        // complicated by the fact that this table uses a composite foreign key.
        // For now there is code in ContextTagDatabaseManager to manually re-added the association
        // via a query to work around this issue.
        [Association(Name = "FK_TagValue_CustomTag", Storage = "_customTag", ThisKey = "TagID",
            OtherKey = "ID", IsForeignKey = true, DeleteRule = "Cascade")]
        public CustomTagTableV1 CustomTag
        {
            get
            {
                return _customTag.Entity;
            }
            set
            {
                _customTag.Entity = value;
            }
        }
    }

    #endregion Version1

    #region Version2

    /// <summary>
    /// Table mapping for the version 2 schema TagValue table
    /// </summary>
    [Table(Name = "TagValue")]
    public class TagValueTableV2
    {
        /// <summary>
        /// Foreign key relation to the Context table.
        /// </summary>
        EntityRef<ContextTableV1> _context;

        /// <summary>
        /// Foreign key relation to the CustomTag table.
        /// </summary>
        EntityRef<CustomTagTableV1> _customTag;

        /// <summary>
        /// Gets or sets the ID of the context for this tag value.
        /// </summary>
        /// <value>The ID of the context for this tag value.</value>
        [Column(IsPrimaryKey = true, DbType = "INT NOT NULL")]
        public int ContextID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the tag for this tag value.
        /// </summary>
        /// <value>The ID of the tag for this tag value.</value>
        [Column(IsPrimaryKey = true, DbType = "INT NOT NULL")]
        public int TagID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(IsPrimaryKey = true, DbType = "NVARCHAR(100) NOT NULL DEFAULT ''")]
        public string Workflow { get; set; }

        /// <summary>
        /// Gets or sets the value for this context and tag combination.
        /// </summary>
        /// <value>The value for this context and tag combination.</value>
        [Column(DbType = "NVARCHAR(400) NOT NULL")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the Context the foreign key relation.
        /// </summary>
        // Note that the DeleteRule of Cascade seems to be ineffective here. I think a corresponding
        // EntitySet association is needed on the Context table with the DeleteRule, but that got
        // complicated by the fact that this table uses a composite foreign key.
        // For now there is code in ContextTagDatabaseManager to manually re-added the association
        // via a query to work around this issue.
        [Association(Name = "FK_TagValue_Context", Storage = "_context", ThisKey = "ContextID",
            OtherKey = "ID", IsForeignKey = true, DeleteRule = "Cascade")]
        public ContextTableV1 Context
        {
            get
            {
                return _context.Entity;
            }
            set
            {
                _context.Entity = value;
            }
        }

        /// <summary>
        /// Gets or sets the CustomTag foreign key relation.
        /// </summary>
        // Note that the DeleteRule of Cascade seems to be ineffective here. I think a corresponding
        // EntitySet association is needed on the CustomTag table with the DeleteRule, but that got
        // complicated by the fact that this table uses a composite foreign key.
        // For now there is code in ContextTagDatabaseManager to manually re-added the association
        // via a query to work around this issue.
        [Association(Name = "FK_TagValue_CustomTag", Storage = "_customTag", ThisKey = "TagID",
            OtherKey = "ID", IsForeignKey = true, DeleteRule = "Cascade")]
        public CustomTagTableV1 CustomTag
        {
            get
            {
                return _customTag.Entity;
            }
            set
            {
                _customTag.Entity = value;
            }
        }
    }

    #endregion Version2

}
