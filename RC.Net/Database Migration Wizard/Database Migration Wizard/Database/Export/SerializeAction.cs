using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;

namespace Database_Migration_Wizard.Database.Export
{
    public class SerializeAction : SerializeInterface
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

        public string SerializeTable(DbConnection dbConnection)
        {
            var dataTable = DBMethods.ExecuteDBQuery(dbConnection, this.sql);

            return JsonConvert.SerializeObject(dataTable);
        }
    }
}
