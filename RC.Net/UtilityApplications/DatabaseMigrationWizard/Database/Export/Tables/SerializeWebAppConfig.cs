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
	                            WebAppConfig.Type
	                            , WebAppConfig.Settings
                                , WebAppConfig.GUID AS WebAppConfigGuid
	                            , dbo.Workflow.GUID AS WorkflowGuid
                            FROM 
	                            [dbo].[WebAppConfig]
		                            LEFT OUTER JOIN dbo.Workflow
			                            ON dbo.Workflow.ID = dbo.WebAppConfig.WorkflowID";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
