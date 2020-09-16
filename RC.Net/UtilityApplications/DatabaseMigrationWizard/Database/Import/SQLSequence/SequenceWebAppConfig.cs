using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
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

		private readonly string InsertReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Warning'
                , 'WebAppConfig'
                , CONCAT('The WebAppConfig ', dbo.WebAppConfig.Type, ' is present in the destination database, but NOT in the importing source.')
            FROM
                dbo.WebAppConfig
                    LEFT OUTER JOIN ##WebAppConfig
                        ON dbo.WebAppConfig.Guid = ##WebAppConfig.WebAppConfigGuid
            WHERE
                ##WebAppConfig.WebAppConfigGuid IS NULL
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'WebAppConfig'
                , CONCAT('The WebAppConfig ', ##WebAppConfig.Type, ' will be added to the database')
            FROM
                ##WebAppConfig
                    LEFT OUTER JOIN dbo.WebAppConfig
                        ON dbo.WebAppConfig.Guid = ##WebAppConfig.WebAppConfigGuid
            WHERE
                dbo.WebAppConfig.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'WebAppConfig'
                , CONCAT('The WebAppConfig ', dbo.WebAppConfig.Type, ' will have its settings updated')
                , CAST(dbo.WebAppConfig.Settings AS NVARCHAR(MAX))
                , CAST(UpdatingWebAppConfig.Settings AS NVARCHAR(MAX))
            FROM
                ##WebAppConfig AS UpdatingWebAppConfig

                        INNER JOIN dbo.WebAppConfig
                            ON dbo.WebAppConfig.Guid = UpdatingWebAppConfig.WebAppConfigGuid

            WHERE
                UpdatingWebAppConfig.Settings NOT LIKE dbo.WebAppConfig.Settings
            ;

            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'WebAppConfig'
                , CONCAT('The WebAppConfig ', dbo.WebAppConfig.Type, ' will have its WorkflowID updated')
                , dbo.WebAppConfig.WorkflowID
                , dbo.Workflow.ID
            FROM
                ##WebAppConfig AS UpdatingWebAppConfig
                        LEFT OUTER JOIN dbo.Workflow
                            ON dbo.Workflow.Guid = UpdatingWebAppConfig.WorkflowGuid

                        INNER JOIN dbo.WebAppConfig
                            ON dbo.WebAppConfig.Guid = UpdatingWebAppConfig.WebAppConfigGuid

            WHERE
                ISNULL(dbo.Workflow.ID, '') <> ISNULL(dbo.WebAppConfig.WorkflowID, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'WebAppConfig'
                , 'The WebAppConfig  will have its type updated'
                , dbo.WebAppConfig.Type
                , UpdatingWebAppConfig.Type
            FROM
                ##WebAppConfig AS UpdatingWebAppConfig

                        INNER JOIN dbo.WebAppConfig
                            ON dbo.WebAppConfig.Guid = UpdatingWebAppConfig.WebAppConfigGuid

            WHERE
                ISNULL(UpdatingWebAppConfig.Type, '') <> ISNULL(dbo.WebAppConfig.Type, '')";

		public Priorities Priority => Priorities.Low;

		public string TableName => "WebAppConfig";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<WebAppConfig>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
