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
                                        [WebAppConfigGuid] uniqueidentifier NOT NULL,
                                        [WorkflowGuid] uniqueidentifier NOT NULL,
										)";

        private readonly string insertSQL = @"
                                    UPDATE
										dbo.WebAppConfig
									SET
										Settings = UpdatingWebAppConfig.Settings
										, Type = UpdatingWebAppConfig.Type
										, WorkflowID = Workflow.ID
									FROM
										##WebAppConfig AS UpdatingWebAppConfig
											LEFT OUTER JOIN dbo.Workflow
												ON dbo.Workflow.Guid = UpdatingWebAppConfig.WorkflowGuid
									WHERE
										UpdatingWebAppConfig.WebAppConfigGuid = dbo.WebAppConfig.Guid
									;
									INSERT INTO dbo.WebAppConfig(Type,WorkflowID,Settings, Guid)
									SELECT
										UpdatingWebAppConfig.Type
										, dbo.Workflow.ID
										, UpdatingWebAppConfig.Settings
										, WebAppConfigGuid
									FROM 
										##WebAppConfig AS UpdatingWebAppConfig
											LEFT OUTER JOIN dbo.Workflow
												ON dbo.Workflow.Guid = UpdatingWebAppConfig.WorkflowGuid
									WHERE
										UpdatingWebAppConfig.WebAppConfigGuid NOT IN (SELECT Guid FROM dbo.WebAppConfig)
                                    ";

		private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##WebAppConfig (Type, Settings, WorkflowGuid, WebAppConfigGuid)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Low;

		public string TableName => "WebAppConfig";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<WebAppConfig>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
