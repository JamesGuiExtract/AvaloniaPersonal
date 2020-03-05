using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeAction : ISerialize
    {
        private readonly string sql = 
                            @"
                            SELECT
	                            dbo.[Action].[ID]
                                , dbo.[Action].[ASCName]
                                , dbo.[Action].[Description]
                                , dbo.[Action].[WorkflowID]
                                , dbo.[Action].[MainSequence]
	                            , dbo.Workflow.[Name]
                            FROM 
	                            [dbo].[Action]
		                            LEFT OUTER JOIN dbo.Workflow
			                            ON dbo.[Action].WorkflowID = dbo.Workflow.ID";

        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
