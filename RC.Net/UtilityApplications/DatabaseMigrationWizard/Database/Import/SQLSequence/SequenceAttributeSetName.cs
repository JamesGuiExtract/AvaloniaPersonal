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

        private readonly string InsertReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
                                    SELECT
                                        'Insert'
	                                    , 'Warning'
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
	                                    dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
                                    SELECT
	                                    'Insert'
	                                    , 'Info'
	                                    , 'AttributeSetName'
	                                    , CONCAT('The Attribute Set Name ', ##AttributeSetName.Description, ' will be added to the database')
                                    FROM
	                                    ##AttributeSetName
		                                    LEFT OUTER JOIN dbo.AttributeSetName
			                                    ON dbo.AttributeSetName.Guid = ##AttributeSetName.GUID
                                    WHERE
	                                    dbo.AttributeSetName.Guid IS NULL";

        private readonly string UpdateReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
                                    SELECT
	                                    'Update'
	                                    , 'Info'
	                                    , 'AttributeSetName'
	                                    , 'The AttributeSetName description will be updated'
	                                    , dbo.AttributeSetName.Description
	                                    , ##AttributeSetName.Description
                                    FROM
	                                    ##AttributeSetName
		                                    INNER JOIN dbo.AttributeSetName
			                                    ON ##AttributeSetName.Guid = dbo.AttributeSetName.Guid
                                    WHERE
	                                    ISNULL(##AttributeSetName.Description, '') <> ISNULL(dbo.AttributeSetName.Description, '')";

        public Priorities Priority => Priorities.High;

        public string TableName => "AttributeSetName";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<AttributeSetName>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
            importOptions.ExecuteCommand(this.UpdateReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
