using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
	class SequenceWebAPIConfiguration : ISequence
    {
        private readonly string CreateTempTableSQL = @"
            CREATE TABLE [dbo].[##WebAPIConfiguration](
            [Settings] [ntext] NULL,
            [Guid] uniqueidentifier NOT NULL,
            [Name] nvarchar(max) NOT NULL,
            )";

        private readonly string insertSQL = @"
            UPDATE
                dbo.WebAPIConfiguration
            SET
                Settings = UpdatingWebAPIConfiguration.Settings
                , Name = UpdatingWebAPIConfiguration.Name
            FROM
                ##WebAPIConfiguration AS UpdatingWebAPIConfiguration
                    LEFT OUTER JOIN dbo.WebAPIConfiguration
                        ON dbo.WebAPIConfiguration.Guid = UpdatingWebAPIConfiguration.Guid
            WHERE
                UpdatingWebAPIConfiguration.Guid = dbo.WebAPIConfiguration.Guid
            ;
            INSERT INTO dbo.WebAPIConfiguration(Name,Settings,Guid)
            SELECT
                UpdatingWebAPIConfiguration.Name
                , UpdatingWebAPIConfiguration.Settings
                , UpdatingWebAPIConfiguration.Guid
            FROM 
                ##WebAPIConfiguration AS UpdatingWebAPIConfiguration
                    LEFT OUTER JOIN dbo.WebAPIConfiguration
                        ON dbo.WebAPIConfiguration.Guid = UpdatingWebAPIConfiguration.Guid
            WHERE
                UpdatingWebAPIConfiguration.Guid NOT IN (SELECT Guid FROM dbo.WebAPIConfiguration)
            ";

        private readonly string insertTempTableSQL = @"
            INSERT INTO ##WebAPIConfiguration (Name,Settings,Guid)
            VALUES
            ";

        private readonly string InsertReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Warning'
                , 'UpdatingWebAPIConfiguration'
                , CONCAT('The WebAPIConfiguration ', dbo.WebAPIConfiguration.Name, ' is present in the destination database, but NOT in the importing source.')
            FROM
                dbo.WebAPIConfiguration
                    LEFT OUTER JOIN ##WebAPIConfiguration
                        ON dbo.WebAPIConfiguration.Guid = ##WebAPIConfiguration.Guid
            WHERE
                ##WebAPIConfiguration.Guid IS NULL
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'UpdatingWebAPIConfiguration'
                , CONCAT('The UpdatingWebAPIConfiguration ', ##WebAPIConfiguration.Name, ' will be added to the database')
            FROM
                ##WebAPIConfiguration
                    LEFT OUTER JOIN dbo.WebAPIConfiguration
                        ON dbo.WebAPIConfiguration.Guid = ##WebAPIConfiguration.Guid
            WHERE
                dbo.WebAPIConfiguration.Guid IS NULL";

        private readonly string UpdateReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'UpdatingWebAPIConfiguration'
                , CONCAT('The WebAPIConfiguration ', dbo.WebAPIConfiguration.Name, ' will have its settings updated')
                , CAST(dbo.WebAPIConfiguration.Settings AS NVARCHAR(MAX))
                , CAST(dbo.WebAPIConfiguration.Settings AS NVARCHAR(MAX))
            FROM
                ##WebAPIConfiguration AS UpdatingWebAPIConfiguration

                        INNER JOIN dbo.WebAPIConfiguration
                            ON dbo.WebAPIConfiguration.Guid = UpdatingWebAPIConfiguration.Guid

            WHERE
                ISNULL(CAST(UpdatingWebAPIConfiguration.Settings AS NVARCHAR(MAX)), '') <> ISNULL(CAST(dbo.WebAPIConfiguration.Settings AS NVARCHAR(MAX)), '')
            ;

            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'UpdatingWebAPIConfiguration'
                , CONCAT('The UpdatingWebAPIConfiguration ', dbo.WebAPIConfiguration.Name, ' will have its name updated')
                , dbo.WebAPIConfiguration.Name
                , dbo.WebAPIConfiguration.Name
            FROM
                ##WebAPIConfiguration AS UpdatingWebAPIConfiguration
                        INNER JOIN dbo.WebAPIConfiguration
                            ON dbo.WebAPIConfiguration.Guid = UpdatingWebAPIConfiguration.Guid

            WHERE
                ISNULL(dbo.WebAPIConfiguration.Name, '') <> ISNULL(UpdatingWebAPIConfiguration.Name, '')
            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "WebAPIConfiguration";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<WebAPIConfiguration>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
            importOptions.ExecuteCommand(this.UpdateReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
