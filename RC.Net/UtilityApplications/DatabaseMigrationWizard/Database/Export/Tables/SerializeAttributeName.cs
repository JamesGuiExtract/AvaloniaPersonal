using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeAttributeName : ISerialize
    {
        private readonly string sql = 
                            @"
                            SELECT
	                            *
                            FROM 
	                            [dbo].AttributeName";

        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
