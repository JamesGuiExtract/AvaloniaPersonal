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
	                                    [WorkflowID] [int] NULL,
	                                    [MainSequence] [bit] NULL,
										[Name] [nvarchar](100) NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.Action (ASCName, Description, MainSequence, WorkflowID)

									SELECT
										UpdatingAction.ASCName
										, UpdatingAction.Description
										, UpdatingAction.MainSequence
										, Workflow.ID

									FROM 
										##Action AS UpdatingAction
											LEFT OUTER JOIN dbo.Workflow
													ON dbo.Workflow.Name = UpdatingAction.Name
									WHERE
										NOT EXISTS
											(
												SELECT
													ASCName
													, WorkflowID
												FROM
													dbo.Action
												WHERE
													UpdatingAction.ASCName = dbo.Action.ASCName
													AND
													(
														Workflow.ID = dbo.Action.WorkflowID
														OR
														(
															dbo.Action.WorkflowID IS NULL
															AND
															Workflow.ID IS NULL
														)
													)
									)
									;
									UPDATE 
										dbo.Action 
									SET
										Description = UpdatingAction.Description
										, MainSequence = UpdatingAction.MainSequence

									FROM
										##Action AS UpdatingAction
												LEFT OUTER JOIN dbo.Workflow
													ON dbo.Workflow.Name = UpdatingAction.Name

									WHERE
										UpdatingAction.ASCName = dbo.Action.ASCName
										AND
										(
											dbo.Workflow.ID = dbo.Action.WorkflowID
											OR
											(
												dbo.Workflow.ID IS NULL
												AND
												dbo.Action.WorkflowID IS NULL
											)
									)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Action (ASCName, Description, WorkflowID, MainSequence, Name)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.MediumHigh;

		public string TableName => "Action";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Action>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
