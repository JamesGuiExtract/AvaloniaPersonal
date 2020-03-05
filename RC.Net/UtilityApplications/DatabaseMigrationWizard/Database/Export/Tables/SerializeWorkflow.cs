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
								Workflow.[ID]
								, Workflow.[Name]
								, Workflow.[WorkflowTypeCode]
								, Workflow.[Description]
								, Workflow.[DocumentFolder]
								, Workflow.[OutputFilePathInitializationFunction]
								, Workflow.[LoadBalanceWeight]
								, Workflow.[EditActionID]
								, EditAction.ASCName AS EditAction 
								, Workflow.[EndActionID]
								, EndAction.ASCName AS EndAction 
								, Workflow.[PostEditActionID]
								, PostEditAction.ASCName AS PostEditAction 
								, Workflow.[PostWorkflowActionID]
								, PostWorkflowAction.ASCName AS PostWorkflowAction 
								, Workflow.[StartActionID]
								, StartAction.ASCName AS StartAction 
								, Workflow.[OutputAttributeSetID]
								, AttributeSetName.Description AS AttributeSetName 
								, Workflow.[OutputFileMetadataFieldID]
								, MetadataField.Name AS MetadataFieldName 
							FROM 
								[dbo].[Workflow]
									LEFT OUTER JOIN dbo.Action AS EditAction
										ON dbo.Workflow.EditActionID = EditAction.ID

									LEFT OUTER JOIN dbo.Action AS EndAction
										ON dbo.Workflow.EndActionID = EndAction.ID

									LEFT OUTER JOIN dbo.Action AS PostEditAction
										ON dbo.Workflow.PostEditActionID = PostEditAction.ID

									LEFT OUTER JOIN dbo.Action AS PostWorkflowAction
										ON dbo.Workflow.PostWorkflowActionID = PostWorkflowAction.ID

									LEFT OUTER JOIN dbo.Action AS StartAction
										ON dbo.Workflow.StartActionID = StartAction.ID

									LEFT OUTER JOIN dbo.AttributeSetName
										ON dbo.Workflow.OutputAttributeSetID = AttributeSetName.ID

									LEFT OUTER JOIN dbo.MetadataField
										ON dbo.Workflow.OutputFileMetadataFieldID = MetadataField.ID";

        public void SerializeTable(DbConnection dbConnection, StreamWriter writer)
        {
			ExportHelper.WriteTableInBulk(this.sql, writer, dbConnection);
        }
    }
}
