using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceAction : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##Action](
	                                    [ASCName] [nvarchar](50) NOT NULL,
	                                    [Description] [nvarchar](255) NULL,
	                                    [MainSequence] [bit] NULL,
										[ActionGUID] uniqueidentifier NOT NULL,
										[WorkflowGUID] uniqueidentifier null
                                        )";

        private readonly string insertSQL = @"
									UPDATE 
										dbo.Action 
									SET
										ASCName = UpdatingAction.ASCName
										, Description = UpdatingAction.Description
										, MainSequence = UpdatingAction.MainSequence
										, WorkflowID = dbo.Workflow.ID
									FROM
										##Action AS UpdatingAction
												LEFT OUTER JOIN dbo.Workflow
													ON dbo.Workflow.GUID = UpdatingAction.WorkflowGUID

									WHERE
										UpdatingAction.ActionGUID = dbo.Action.GUID
									;
                                    INSERT INTO dbo.Action (ASCName, Description, MainSequence, WorkflowID, GUID)

									SELECT
										UpdatingAction.ASCName
										, UpdatingAction.Description
										, UpdatingAction.MainSequence
										, Workflow.ID
										, ActionGUID

									FROM 
										##Action AS UpdatingAction
											LEFT OUTER JOIN dbo.Workflow
													ON dbo.Workflow.GUID = UpdatingAction.WorkflowGUID
									WHERE
										UpdatingAction.ActionGUID NOT IN (SELECT GUID FROM dbo.Action)
									;";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Action (ASCName, Description, MainSequence, ActionGUID, WorkflowGUID)
                                            VALUES
                                            ";

		private readonly string ReportingSQL = @"
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
											SELECT
												'Warning'
												, 'Action'
												, CONCAT('The action ', dbo.action.ASCName, ' is present in the destination database, but NOT in the importing source.')
											FROM
												dbo.Action
													LEFT OUTER JOIN ##Action
														ON dbo.Action.Guid = ##Action.ActionGUID
											WHERE
												##Action.ActionGUID IS NULL
											;
											INSERT INTO
												dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
											SELECT
												'Info'
												, 'Action'
												, CONCAT('The action ', ##Action.ASCName, ' will be added to the database')
											FROM
												##Action
													LEFT OUTER JOIN dbo.Action
														ON dbo.Action.Guid = ##Action.ActionGUID
											WHERE
												dbo.Action.Guid IS NULL";

		public Priorities Priority => Priorities.MediumHigh;

		public string TableName => "Action";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Action>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
