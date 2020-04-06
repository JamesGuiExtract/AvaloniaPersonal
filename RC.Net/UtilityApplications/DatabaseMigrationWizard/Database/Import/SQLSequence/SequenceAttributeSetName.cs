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

        private readonly string ReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Warning'
	                                    , 'AttributeSetName'
	                                    , CONCAT('The Attribute Set Name ', dbo.AttributeSetName.Description, ' is present in the destination database, but NOT in the importing source.')
                                    FROM
	                                    dbo.AttributeSetName
		                                    LEFT OUTER JOIN ##AttributeSetName
			                                    ON dbo.AttributeSetName.Guid = ##AttributeSetName.GUID
                                    WHERE
	                                    ##AttributeSetName.GUID IS NULL
                                    ;
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Info'
	                                    , 'AttributeSetName'
	                                    , CONCAT('The Attribute Set Name ', ##AttributeSetName.Description, ' will be added to the database')
                                    FROM
	                                    ##AttributeSetName
		                                    LEFT OUTER JOIN dbo.AttributeSetName
			                                    ON dbo.AttributeSetName.Guid = ##AttributeSetName.GUID
                                    WHERE
	                                    dbo.AttributeSetName.Guid IS NULL";

        public Priorities Priority => Priorities.High;

        public string TableName => "AttributeSetName";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<AttributeSetName>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
