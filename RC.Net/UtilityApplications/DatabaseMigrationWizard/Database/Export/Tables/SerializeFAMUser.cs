using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeFAMUser : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT
	                            *
                            FROM 
	                            [dbo].FAMUser";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
