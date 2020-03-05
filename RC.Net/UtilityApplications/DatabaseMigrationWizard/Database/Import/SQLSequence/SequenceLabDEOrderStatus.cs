using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceLabDEOrderStatus : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##LabDEOrderStatus](
	                                    [Code] [nchar](1) NOT NULL,
	                                    [Meaning] [nvarchar](255) NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.LabDEOrderStatus(Code, Meaning)

                                    SELECT
	                                    Code
	                                    , Meaning
                                    FROM 
	                                    ##LabDEOrderStatus
                                    WHERE
	                                    Code NOT IN (SELECT Code FROM dbo.LabDEOrderStatus)
                                    ;
                                    UPDATE
	                                    dbo.LabDEOrderStatus
                                    SET
	                                    Meaning = UpdatingLabDEOrderStatus.Meaning

                                    FROM
	                                    ##LabDEOrderStatus AS UpdatingLabDEOrderStatus
                                    WHERE
                                        LabDEOrderStatus.Code = UpdatingLabDEOrderStatus.Code";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##LabDEOrderStatus (Code, Meaning)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Medium;

        public string TableName => "LabDEOrderStatus";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);
            
            ImportHelper.PopulateTemporaryTable<LabDEOrderStatus>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);
            
            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
