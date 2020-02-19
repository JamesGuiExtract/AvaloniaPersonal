using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceWebAppConfig : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##WebAppConfig](
										[Type] [nvarchar](100) NOT NULL,
										[Settings] [ntext] NULL,
										[Name] nvarchar(max) NULL
										)";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.WebAppConfig(Type,WorkflowID,Settings)
									SELECT
										UpdatingWebAppConfig.Type
										, dbo.Workflow.ID
										, UpdatingWebAppConfig.Settings
									FROM 
										##WebAppConfig AS UpdatingWebAppConfig
											LEFT OUTER JOIN dbo.Workflow
												ON dbo.Workflow.Name = UpdatingWebAppConfig.Name
									WHERE
										NOT EXISTS
											(
												SELECT
													WorkflowID
													, Type
												FROM
													dbo.WebAppConfig
												WHERE
													dbo.WebAppConfig.Type = UpdatingWebAppConfig.Type
													AND
													dbo.WebAppConfig.WorkflowID = dbo.Workflow.ID 
											)
									;
									UPDATE
										dbo.WebAppConfig
									SET
										Settings = UpdatingWebAppConfig.Settings
									FROM
										##WebAppConfig AS UpdatingWebAppConfig
											LEFT OUTER JOIN dbo.Workflow
												ON dbo.Workflow.Name = UpdatingWebAppConfig.Name
									WHERE
										dbo.WebAppConfig.Type = UpdatingWebAppConfig.Type
										AND
										dbo.WebAppConfig.WorkflowID = dbo.Workflow.ID
                                    ";

		private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##WebAppConfig (Type, Settings, Name)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Low;

		public string TableName => "WebAppConfig";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<WebAppConfig>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
