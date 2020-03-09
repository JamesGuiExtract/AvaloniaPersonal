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

        public Priorities Priority => Priorities.Medium;

        public string TableName => "DatabaseService";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DatabaseService>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
