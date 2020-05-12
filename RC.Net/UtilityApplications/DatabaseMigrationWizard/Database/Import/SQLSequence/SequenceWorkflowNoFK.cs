using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceWorkflowNoFK : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##Workflow](
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
                                    INSERT INTO dbo.Workflow (Name, WorkflowTypeCode, Description, DocumentFolder, LoadBalanceWeight, GUID)

                                    SELECT
	                                    Name
	                                    , WorkflowTypeCode
	                                    , Description
	                                    , DocumentFolder
	                                    , LoadBalanceWeight
                                        , [Guid]
                                    FROM 
	                                    ##Workflow
                                    WHERE
	                                    GUID NOT IN (SELECT GUID FROM dbo.Workflow)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Workflow (
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

        private readonly string InsertReportingSQL = @"
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
                                            SELECT
	                                            'Insert'
	                                            , 'Warning'
												, 'Workflow'
	                                            , CONCAT('The workflow ', dbo.Workflow.Name, ' is present in the destination database, but NOT in the importing source.')
                                            FROM
	                                            dbo.Workflow
		                                            LEFT OUTER JOIN ##Workflow
			                                            ON dbo.Workflow.Guid = ##Workflow.Guid
                                            WHERE
	                                            ##Workflow.Guid IS NULL
                                            ;
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
                                            SELECT
	                                            'Insert'
	                                            , 'Info'
												, 'Workflow'
	                                            , CONCAT('The workflow ', ##Workflow.Name, ' will be added to the database')
                                            FROM
	                                            ##Workflow
		                                            LEFT OUTER JOIN dbo.Workflow
			                                            ON dbo.Workflow.Guid = ##Workflow.Guid
                                            WHERE
	                                            dbo.Workflow.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its WorkflowTypeCode updated')
												, dbo.Workflow.WorkflowTypeCode
												, UpdatingWorkflow.WorkflowTypeCode

											FROM
												##Workflow AS UpdatingWorkflow

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(UpdatingWorkflow.WorkflowTypeCode, '') <> ISNULL(dbo.Workflow.WorkflowTypeCode, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its Description updated')
												, CAST(dbo.Workflow.Description AS nvarchar(64))
												, CAST(UpdatingWorkflow.Description AS NVARCHAR(64))

											FROM
												##Workflow AS UpdatingWorkflow

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(UpdatingWorkflow.Description, '') <> ISNULL(dbo.Workflow.Description, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its LoadBalanceWeight updated')
												, dbo.Workflow.LoadBalanceWeight
												, UpdatingWorkflow.LoadBalanceWeight

											FROM
												##Workflow AS UpdatingWorkflow

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(UpdatingWorkflow.LoadBalanceWeight, '') <> ISNULL(dbo.Workflow.LoadBalanceWeight, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its OutputFilePathInitializationFunction updated')
												, dbo.Workflow.OutputFilePathInitializationFunction
												, UpdatingWorkflow.OutputFilePathInitializationFunction

											FROM
												##Workflow AS UpdatingWorkflow

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(UpdatingWorkflow.OutputFilePathInitializationFunction, '') <> ISNULL(dbo.Workflow.OutputFilePathInitializationFunction, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its DocumentFolder updated')
												, dbo.Workflow.DocumentFolder
												, UpdatingWorkflow.DocumentFolder

											FROM
												##Workflow AS UpdatingWorkflow

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(UpdatingWorkflow.DocumentFolder, '') <> ISNULL(dbo.Workflow.DocumentFolder, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its StartActionID updated')
												, dbo.Workflow.StartActionID
												, StartAction.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN Action AS StartAction
															ON UpdatingWorkflow.StartActionGUID = StartAction.GUID

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(StartAction.ID, '') <> ISNULL(dbo.Workflow.StartActionID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its EndActionID updated')
												, dbo.Workflow.EndActionID
												, EndAction.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN Action AS EndAction
															ON UpdatingWorkflow.EndActionGUID = EndAction.GUID

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(EndAction.ID, '') <> ISNULL(dbo.Workflow.EndActionID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its PostWorkflowActionID updated')
												, dbo.Workflow.PostWorkflowActionID
												, PostWorkflowAction.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN Action AS PostWorkflowAction
															ON UpdatingWorkflow.PostWorkflowActionGUID = PostWorkflowAction.GUID

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(PostWorkflowAction.ID, '') <> ISNULL(dbo.Workflow.PostWorkflowActionID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its EditActionID updated')
												, dbo.Workflow.EditActionID
												, EditAction.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN Action AS EditAction
															ON UpdatingWorkflow.EditActionGUID = EditAction.GUID

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(EditAction.ID, '') <> ISNULL(dbo.Workflow.EditActionID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its PostEditActionID updated')
												, dbo.Workflow.PostEditActionID
												, PostEditAction.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN Action AS PostEditAction
															ON UpdatingWorkflow.PostEditActionGUID = PostEditAction.GUID

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(PostEditAction.ID, '') <> ISNULL(dbo.Workflow.PostEditActionID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its PostEditActionID updated')
												, dbo.Workflow.PostEditActionID
												, PostEditAction.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN Action AS PostEditAction
															ON UpdatingWorkflow.PostEditActionGUID = PostEditAction.GUID

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(PostEditAction.ID, '') <> ISNULL(dbo.Workflow.PostEditActionID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its OutputAttributeSetID updated')
												, dbo.Workflow.OutputAttributeSetID
												, AttributeSetName.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN AttributeSetName
															ON AttributeSetName.Guid = UpdatingWorkflow.AttributeSetNameGuid

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(AttributeSetName.ID, '') <> ISNULL(dbo.Workflow.OutputAttributeSetID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its OutputFileMetadataFieldID updated')
												, dbo.Workflow.OutputFileMetadataFieldID
												, MetadataField.ID

											FROM
												##Workflow AS UpdatingWorkflow

														LEFT OUTER JOIN dbo.MetadataField
															ON dbo.MetadataField.Guid = UpdatingWorkflow.MetadataFieldNameGuid

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(MetadataField.ID, '') <> ISNULL(dbo.Workflow.OutputFileMetadataFieldID, '')
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
											SELECT
												'Update'
												, 'Info'
												, 'Workflow'
												, 'The workflow will have its name upated'
												, dbo.Workflow.Name
												, UpdatingWorkflow.Name

											FROM
												##Workflow AS UpdatingWorkflow

														INNER JOIN dbo.Workflow
															ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

											WHERE
												ISNULL(UpdatingWorkflow.Name, '') <> ISNULL(dbo.Workflow.Name, '')
											;";


		public Priorities Priority => Priorities.High;

        public string TableName => "Workflow";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Workflow>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
