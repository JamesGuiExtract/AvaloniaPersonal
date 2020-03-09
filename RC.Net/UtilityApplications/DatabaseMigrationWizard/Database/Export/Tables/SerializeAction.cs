using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeAction : ISerialize
    {
        private readonly string sql =
                            @"
                            SELECT
                                dbo.[Action].[ASCName]
                                , dbo.[Action].[Description]
                                , dbo.[Action].[MainSequence]
                                , dbo.[Action].[Guid] AS ActionGuid
                                , dbo.[Workflow].[Guid] AS WorkflowGuid
                            FROM 
	                            [dbo].[Action]
		                            LEFT OUTER JOIN dbo.Workflow
			                            ON dbo.[Action].WorkflowID = dbo.Workflow.ID";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
            ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
