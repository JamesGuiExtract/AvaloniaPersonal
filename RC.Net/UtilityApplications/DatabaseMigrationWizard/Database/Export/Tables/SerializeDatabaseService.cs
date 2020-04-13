using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeDatabaseService : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT  
	                            Description
		                        , Settings
		                        , Enabled
                            FROM 
	                            [dbo].[DatabaseService]";

        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
