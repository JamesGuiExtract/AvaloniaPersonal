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
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.DatabaseService(Description, Settings, Enabled)

                                    SELECT
	                                    UpdatingDatabaseService.Description
	                                    , UpdatingDatabaseService.Settings
	                                    , UpdatingDatabaseService.Enabled
                                    FROM 
	                                    ##DatabaseService AS UpdatingDatabaseService
                                    WHERE
	                                    UpdatingDatabaseService.Description NOT IN (SELECT Description FROM dbo.DatabaseService)
                                    ;

                                    UPDATE
	                                    dbo.DatabaseService
                                    SET
	                                    Enabled = UpdatingDatabaseService.Enabled
	                                    , Settings = UpdatingDatabaseService.Settings
                                    FROM
	                                    ##DatabaseService AS UpdatingDatabaseService
                                    WHERE
	                                    UpdatingDatabaseService.Description = dbo.DatabaseService.Description
	                                    AND
	                                    --Required because Description is not enforced to be unique. This is to prevent weird errors.
	                                    (SELECT COUNT(*) FROM dbo.DatabaseService WHERE Description = UpdatingDatabaseService.Description) = 1

									";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DatabaseService (Description, Settings, Enabled)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Medium;

        public string TableName => "DatabaseService";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);
            
            ImportHelper.PopulateTemporaryTable<DatabaseService>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);
            
            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
