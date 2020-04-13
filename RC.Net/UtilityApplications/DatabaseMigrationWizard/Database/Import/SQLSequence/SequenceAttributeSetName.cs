using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceAttributeSetName : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##AttributeSetName](
										[Description] [nvarchar](255) NULL,
										)";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.AttributeSetName(Description)

                                    SELECT
	                                    Description
                                    FROM 
	                                    ##AttributeSetName

                                    WHERE
	                                    Description NOT IN (SELECT Description FROM dbo.AttributeSetName)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##AttributeSetName (Description)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.High;

        public string TableName => "AttributeSetName";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<AttributeSetName>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
