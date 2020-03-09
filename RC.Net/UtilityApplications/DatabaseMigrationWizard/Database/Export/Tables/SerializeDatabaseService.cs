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
                                , Guid
                            FROM 
	                            [dbo].[DatabaseService]";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
