using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
	class SequenceWorkflowWithFK : ISequence
    {
		private readonly string CreateTempTableSQL = @"
            CREATE TABLE [dbo].[##Workflow1](
            [Guid] uniqueidentifier NOT NULL,
            [Name] [nvarchar](100) NULL,
            [WorkflowTypeCode] [nvarchar](1) NULL,
            [Description] [nvarchar](max) NULL,
            [DocumentFolder] [nvarchar](255) NULL,
            [OutputFilePathInitializationFunction] [nvarchar](255) NULL,
            [LoadBalanceWeight] [int] NOT NULL,
            [EditActionGUID] uniqueidentifier NULL,
            [EndActionGUID] uniqueidentifier NULL,
            [PostEditActionGUID] uniqueidentifier NULL,
            [PostWorkflowActionGUID] uniqueidentifier NULL,
            [StartActionGUID] uniqueidentifier NULL,
            [AttributeSetNameGuid] uniqueidentifier NULL,
            [MetadataFieldNameGuid] uniqueidentifier NULL
            )";

		private readonly string insertSQL = @"
            UPDATE
                dbo.Workflow 
            SET
                Name = UpdatingWorkflow.Name
                , WorkflowTypeCode = UpdatingWorkflow.WorkflowTypeCode
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
                            ON UpdatingWorkflow.StartActionGUID = StartAction.GUID

                        LEFT OUTER JOIN Action AS EndAction
                            ON UpdatingWorkflow.EndActionGUID = EndAction.GUID

                        LEFT OUTER JOIN Action AS PostWorkflowAction
                            ON UpdatingWorkflow.PostWorkflowActionGUID = PostWorkflowAction.GUID

                        LEFT OUTER JOIN Action AS EditAction
                            ON UpdatingWorkflow.EditActionGUID = EditAction.GUID

                        LEFT OUTER JOIN Action AS PostEditAction
                            ON UpdatingWorkflow.PostEditActionGUID = PostEditAction.GUID

                        LEFT OUTER JOIN AttributeSetName
                            ON AttributeSetName.Guid = UpdatingWorkflow.AttributeSetNameGuid

                        LEFT OUTER JOIN dbo.MetadataField
                            ON dbo.MetadataField.Guid = UpdatingWorkflow.MetadataFieldNameGuid
            WHERE
                dbo.Workflow.GUID = UpdatingWorkflow.GUID";

		private readonly string insertTempTableSQL = @"
            INSERT INTO ##Workflow1 (
                                    GUID
                                    , Name
                                    , WorkflowTypeCode
                                    , Description
                                    , DocumentFolder
                                    , OutputFilePathInitializationFunction
                                    , LoadBalanceWeight
                                    , EditActionGUID
                                    , EndActionGUID
                                    , PostEditActionGUID
                                    , PostWorkflowActionGUID
                                    , StartActionGUID
                                    , AttributeSetNameGuid
                                    , MetadataFieldNameGuid)
            VALUES
            ";

		public Priorities Priority => Priorities.MediumLow;

		public string TableName => "Workflow";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<Workflow>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
