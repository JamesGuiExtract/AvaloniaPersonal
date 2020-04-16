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
                                        [Guid] uniqueidentifier NOT NULL,
										)";

        private readonly string insertSQL = @"
                                    Update 
	                                    dbo.AttributeSetName
                                    SET
	                                    Description = UpdatingAttributeSetName.Description
                                    FROM
	                                    ##AttributeSetName AS UpdatingAttributeSetName
                                    WHERE
	                                    UpdatingAttributeSetName.Guid = dbo.AttributeSetName.Guid
                                    ;
                                    INSERT INTO dbo.AttributeSetName(Description, Guid)

                                    SELECT
	                                    Description
	                                    , Guid
                                    FROM 
	                                    ##AttributeSetName

                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.AttributeSetName)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##AttributeSetName (Description, Guid)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.High;

        public string TableName => "AttributeSetName";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<AttributeSetName>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
