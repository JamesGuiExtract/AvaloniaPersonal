using Extract.Database;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlServerCe;

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
        public static readonly int CurrentSchemaVersion = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabase"/> class.
        /// </summary>
        /// <param name="fileName">The Sql compact file name.</param>
        public ContextTagDatabase(string fileName) :
            base(SqlCompactMethods.BuildDBConnectionString(fileName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagDatabase"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ContextTagDatabase(SqlCeConnection connection)
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
    }

    #endregion AllVersions

    #region Version1

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
}
