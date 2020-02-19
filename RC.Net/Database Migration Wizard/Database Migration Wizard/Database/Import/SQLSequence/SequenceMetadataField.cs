using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceMetadataField : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##MetadataField](
	                                    [Name] [nvarchar](50) NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.MetadataField (Name)

                                    SELECT
	                                    Name
                                    FROM 
	                                    ##MetadataField
                                    WHERE
	                                    Name NOT IN (SELECT Name FROM dbo.MetadataField)
                                    ";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##MetadataField (Name)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.High;

        public string TableName => "MetadataField";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<MetadataField>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
