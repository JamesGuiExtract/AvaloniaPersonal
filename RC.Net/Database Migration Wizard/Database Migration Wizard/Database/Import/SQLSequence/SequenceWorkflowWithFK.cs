using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceWorkflowWithFK : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##Workflow1](
	                                    [Name] [nvarchar](100) NULL,
	                                    [WorkflowTypeCode] [nvarchar](1) NULL,
	                                    [Description] [nvarchar](max) NULL,
	                                    [StartActionID] [int] NULL,
	                                    [EndActionID] [int] NULL,
	                                    [PostWorkflowActionID] [int] NULL,
	                                    [DocumentFolder] [nvarchar](255) NULL,
	                                    [OutputAttributeSetID] [bigint] NULL,
	                                    [OutputFileMetadataFieldID] [int] NULL,
	                                    [OutputFilePathInitializationFunction] [nvarchar](255) NULL,
	                                    [LoadBalanceWeight] [int] NOT NULL,
	                                    [EditActionID] [int] NULL,
	                                    [PostEditActionID] [int] NULL,
                                        [StartAction] NVARCHAR(MAX) NULL,
                                        [EditAction] NVARCHAR(MAX) NULL,
                                        [EndAction] NVARCHAR(MAX) NULL,
                                        [PostEditAction] NVARCHAR(MAX) NULL,
                                        [PostWorkflowAction] NVARCHAR(MAX) NULL,
                                        [AttributeSetName] NVARCHAR(MAX) NULL,
                                        [MetadataFieldName] NVARCHAR(MAX) NULL
                                    )";

        private readonly string insertSQL = @"
                                    UPDATE
										dbo.Workflow 
									SET
										WorkflowTypeCode = UpdatingWorkflow.WorkflowTypeCode
										, Description = UpdatingWorkflow.Description
										, StartActionID = StartAction.ID
										, EndActionID = EndAction.ID
										, PostWorkflowActionID = PostWorkflowAction.ID
										, DocumentFolder = UpdatingWorkflow.DocumentFolder
										, OutputAttributeSetID = AttributeSetName.ID
										, OutputFileMetadataFieldID = MetadataField.ID
										, OutputFilePathInitializationFunction = UpdatingWorkflow.OutputFilePathInitializationFunction
										, LoadBalanceWeight = UpdatingWorkflow.LoadBalanceWeight
										, EditActionID = EditAction.ID
										, PostEditActionID = PostEditAction.ID

									FROM 
										##Workflow1 AS UpdatingWorkflow

												LEFT OUTER JOIN Action AS StartAction
													ON UpdatingWorkflow.StartAction = StartAction.ASCName

												LEFT OUTER JOIN Action AS EndAction
													ON UpdatingWorkflow.EndAction = EndAction.ASCName

												LEFT OUTER JOIN Action AS PostWorkflowAction
													ON UpdatingWorkflow.PostWorkflowAction = PostWorkflowAction.ASCName

												LEFT OUTER JOIN Action AS EditAction
													ON UpdatingWorkflow.EditAction = EditAction.ASCName

												LEFT OUTER JOIN Action AS PostEditAction
													ON UpdatingWorkflow.PostEditAction = PostEditAction.ASCName

												LEFT OUTER JOIN AttributeSetName
													ON AttributeSetName.Description = UpdatingWorkflow.AttributeSetName

												LEFT OUTER JOIN dbo.MetadataField
													ON dbo.MetadataField.Name = UpdatingWorkflow.MetadataFieldName 
									WHERE
										dbo.Workflow.Name = UpdatingWorkflow.Name";

		private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Workflow1 (
                                                                    Name
                                                                    , WorkflowTypeCode
                                                                    , Description
                                                                    , StartActionID
                                                                    , EndActionID
                                                                    , PostWorkflowActionID
                                                                    , DocumentFolder
                                                                    , OutputAttributeSetID
                                                                    , OutputFileMetadataFieldID
                                                                    , OutputFilePathInitializationFunction
                                                                    , LoadBalanceWeight
                                                                    , EditActionID
                                                                    , PostEditActionID
                                                                    , StartAction
                                                                    , EditAction
                                                                    , EndAction
                                                                    , PostEditAction
                                                                    , PostWorkflowAction
                                                                    , AttributeSetName
                                                                    , MetadataFieldName)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Low;

		public string TableName => "Workflow";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Workflow>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
