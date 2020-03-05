using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeWebAppConfig : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT  
	                            WebAppConfig.ID
	                            , WebAppConfig.Type
	                            , WebAppConfig.WorkflowID
	                            , WebAppConfig.Settings
	                            , Name
                            FROM 
	                            [dbo].[WebAppConfig]
		                            LEFT OUTER JOIN dbo.Workflow
			                            ON dbo.Workflow.ID = dbo.WebAppConfig.ID";

        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
