using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceTag : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##Tag](
	                                    [TagName] [nvarchar](100) NOT NULL,
	                                    [TagDescription] [nvarchar](255) NULL,
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    UPDATE 
	                                    dbo.Tag
                                    SET
	                                    TagDescription = UpdatingTag.TagDescription
	                                    , TagName = UpdatingTag.TagName
                                    FROM
	                                    ##Tag AS UpdatingTag
                                    WHERE
	                                    UpdatingTag.Guid = dbo.Tag.Guid
                                    ;
                                    INSERT INTO dbo.Tag(TagName, TagDescription, Guid)

                                    SELECT
	                                    TagName
	                                    , TagDescription
	                                    , Guid
                                    FROM 
	                                    ##Tag
                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.Tag)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Tag (TagName, TagDescription, Guid)
                                            VALUES
                                            ";

        private readonly string ReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Warning'
	                                    , 'Tag'
	                                    , CONCAT('The Tag ', dbo.Tag.TagName, ' is present in the destination database, but NOT in the importing source.')
                                    FROM
	                                    dbo.Tag
		                                    LEFT OUTER JOIN ##Tag
			                                    ON dbo.Tag.Guid = ##Tag.GUID
                                    WHERE
	                                    ##Tag.GUID IS NULL
                                    ;
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Info'
	                                    , 'Tag'
	                                    , CONCAT('The Tag ', ##Tag.TagName, ' will be added to the database')
                                    FROM
	                                    ##Tag
		                                    LEFT OUTER JOIN dbo.Tag
			                                    ON dbo.Tag.Guid = ##Tag.GUID
                                    WHERE
	                                    dbo.Tag.Guid IS NULL";

        public Priorities Priority => Priorities.High;

        public string TableName => "Tag";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Tag>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
