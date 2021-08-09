using Extract.Database;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Extract.Utilities.SqlCompactToSqliteConverter.Test
{
    /// A database with only a Settings table in it
    public class SqlCompactDatabaseWithSettingsTable : DataContext
    {
        /// Connect to a database
        public SqlCompactDatabaseWithSettingsTable(string fileName) :
            base(SqlCompactMethods.BuildDBConnectionString(fileName))
        {
        }

        /// Return the Settings table
        public Table<Settings> Settings => GetTable<Settings>();
    }

    /// Table to mock the one that the FAM Service uses
    [Table(Name = "FPSFile")]
    public class FpsFile
    {
        [Column(Name = "ID", IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true)]
        public int ID { get; set; }
    }

    /// A database with a Settings and an FPSFile table in it
    public class SqlCompactDatabaseWithFPSFileTable : DataContext
    {
        /// Connect to a database
        public SqlCompactDatabaseWithFPSFileTable(string fileName) :
            base(SqlCompactMethods.BuildDBConnectionString(fileName))
        {
        }

        /// Return the Settings table
        public Table<Settings> Settings => GetTable<Settings>();

        /// Return the Settings table
        public Table<FpsFile> FpsFile => GetTable<FpsFile>();
    }

    /// An unused table needed to satisfy linq-to-sql
    [Table(Name = "OtherTable")]
    public class OtherTable
    {
        [Column(Name = "ID", DbType = "INT")]
        public int ID { get; set; }
    }

    /// A database with neither Settings nor FPSFile table
    public class SqlCompactDatabaseWithNoSpecialTables : DataContext
    {
        /// Connect to a database
        public SqlCompactDatabaseWithNoSpecialTables(string fileName) :
            base(SqlCompactMethods.BuildDBConnectionString(fileName))
        {
        }

        /// Return the table
        public Table<OtherTable> OtherTable => GetTable<OtherTable>();
    }
}
