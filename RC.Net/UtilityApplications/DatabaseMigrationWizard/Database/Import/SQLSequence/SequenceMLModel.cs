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
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    UPDATE
	                                    dbo.MLModel
                                    SET
	                                    Name = UpdatingMLModel.Name
                                    FROM
	                                    ##MLModel AS UpdatingMLModel
                                    WHERE
	                                    dbo.MLModel.Guid = UpdatingMLModel.Guid
                                    ;
                                    INSERT INTO dbo.MLModel(Name, Guid)

                                    SELECT
	                                    Name
	                                    , Guid
                                    FROM 
	                                    ##MLModel
                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.MLModel)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##MLModel (Name, Guid)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "MLModel";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<MLModel>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
