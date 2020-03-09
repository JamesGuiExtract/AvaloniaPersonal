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
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    UPDATE
	                                    dbo.MetadataField
                                    SET
	                                    Name = UpdatingMetadataField.Name
                                    FROM
	                                    ##MetadataField AS UpdatingMetadataField
                                    WHERE
	                                    UpdatingMetadataField.Guid = dbo.MetadataField.Guid
                                    ;
                                    INSERT INTO dbo.MetadataField (Name, Guid)

                                    SELECT
	                                    Name
	                                    , Guid
                                    FROM 
	                                    ##MetadataField
                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.MetadataField)
                                    ";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##MetadataField (Name, Guid)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.High;

        public string TableName => "MetadataField";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<MetadataField>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
