using Extract.Database;
using Newtonsoft.Json;
using System.Data.Common;
using System.IO;

namespace DatabaseMigrationWizard.Database.Output
{
    public class SerializeWorkflow : ISerialize
    {
        private readonly string sql =
							@"
                            SELECT  
								Workflow.[Guid] AS WorkflowGuid
								, Workflow.[Name]
								, Workflow.[WorkflowTypeCode]
								, Workflow.[Description]
								, Workflow.[LoadBalanceWeight]

							FROM 
								[dbo].[Workflow]";

        public void SerializeTable(DbConnection dbConnection, TextWriter writer)
        {
			ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
