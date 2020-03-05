using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceAttributeName : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##AttributeName](
	                                    [Name] [nvarchar](255) NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.AttributeName(Name)

                                    SELECT
	                                    Name
                                    FROM 
	                                    ##AttributeName

                                    WHERE
	                                    Name NOT IN (SELECT Name FROM dbo.AttributeName)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##AttributeName (Name)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "AttributeName";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<AttributeName>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
