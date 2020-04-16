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

		public Priorities Priority => Priorities.MediumHigh;

		public string TableName => "Action";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Action>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
