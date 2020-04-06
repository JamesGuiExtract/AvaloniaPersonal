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

        private readonly string ReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Warning'
	                                    , 'MetadataField'
	                                    , CONCAT('The MetadataField ', dbo.MetadataField.Name, ' is present in the destination database, but NOT in the importing source.')
                                    FROM
	                                    dbo.MetadataField
		                                    LEFT OUTER JOIN ##MetadataField
			                                    ON dbo.MetadataField.Guid = ##MetadataField.GUID
                                    WHERE
	                                    ##MetadataField.GUID IS NULL
                                    ;
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Info'
	                                    , 'MetadataField'
	                                    , CONCAT('The MetadataField ', ##MetadataField.Name, ' will be added to the database')
                                    FROM
	                                    ##MetadataField
		                                    LEFT OUTER JOIN dbo.MetadataField
			                                    ON dbo.MetadataField.Guid = ##MetadataField.GUID
                                    WHERE
	                                    dbo.MetadataField.Guid IS NULL";

        public Priorities Priority => Priorities.High;

        public string TableName => "MetadataField";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<MetadataField>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
