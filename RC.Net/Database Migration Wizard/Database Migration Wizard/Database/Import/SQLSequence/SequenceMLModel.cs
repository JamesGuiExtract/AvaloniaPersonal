using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceMLModel : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##MLModel](
	                                    [Name] [nvarchar](255) NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.MLModel(Name)

                                    SELECT
	                                    Name
                                    FROM 
	                                    ##MLModel
                                    WHERE
	                                    Name NOT IN (SELECT Name FROM dbo.MLModel)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##MLModel (Name)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "MLModel";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<MLModel>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
