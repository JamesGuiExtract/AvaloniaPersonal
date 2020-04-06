using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceDatabaseService : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##DatabaseService](
										[Description] [nvarchar](256) NOT NULL,
										[Settings] [nvarchar](max) NOT NULL,
										[Enabled] [bit] NOT NULL,
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                            UPDATE
	                                            dbo.DatabaseService
                                            SET
	                                            Description = UpdatingDatabaseService.Description
	                                            , Enabled = UpdatingDatabaseService.Enabled
	                                            , Settings = UpdatingDatabaseService.Settings
                                            FROM
	                                            ##DatabaseService AS UpdatingDatabaseService
                                            WHERE
	                                            UpdatingDatabaseService.Guid = dbo.DatabaseService.Guid
                                            ;
                                            INSERT INTO dbo.DatabaseService(Description, Settings, Enabled, Guid)

                                            SELECT
	                                            UpdatingDatabaseService.Description
	                                            , UpdatingDatabaseService.Settings
	                                            , UpdatingDatabaseService.Enabled
	                                            , UpdatingDatabaseService.Guid
                                            FROM 
	                                            ##DatabaseService AS UpdatingDatabaseService
                                            WHERE
	                                            UpdatingDatabaseService.Guid NOT IN (SELECT Guid FROM dbo.DatabaseService)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DatabaseService (Description, Settings, Enabled, Guid)
                                            VALUES
                                            ";

        private readonly string ReportingSQL = @"
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                            SELECT
	                                            'Warning'
	                                            , 'DatabaseService'
	                                            , CONCAT('The DatabaseService ', dbo.DatabaseService.Description, ' is present in the destination database, but NOT in the importing source.')
                                            FROM
	                                            dbo.DatabaseService
		                                            LEFT OUTER JOIN ##DatabaseService
			                                            ON dbo.DatabaseService.Guid = ##DatabaseService.GUID
                                            WHERE
	                                            ##DatabaseService.GUID IS NULL
                                            ;
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                            SELECT
	                                            'Info'
	                                            , 'DatabaseService'
	                                            , CONCAT('The DatabaseService ', ##DatabaseService.Description, ' will be added to the database')
                                            FROM
	                                            ##DatabaseService
		                                            LEFT OUTER JOIN dbo.DatabaseService
			                                            ON dbo.DatabaseService.Guid = ##DatabaseService.GUID
                                            WHERE
	                                            dbo.DatabaseService.Guid IS NULL";

        public Priorities Priority => Priorities.Medium;

        public string TableName => "DatabaseService";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DatabaseService>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
