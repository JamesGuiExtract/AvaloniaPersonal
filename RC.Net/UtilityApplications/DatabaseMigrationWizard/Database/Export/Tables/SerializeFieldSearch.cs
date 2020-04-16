using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeFieldSearch : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT
	                            *
                            FROM 
	                            [dbo].FieldSearch";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
