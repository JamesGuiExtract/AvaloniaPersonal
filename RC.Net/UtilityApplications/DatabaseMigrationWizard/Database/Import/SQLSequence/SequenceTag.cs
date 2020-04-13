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
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.Tag(TagName, TagDescription)

                                    SELECT
	                                    TagName
	                                    , TagDescription
                                    FROM 
	                                    ##Tag
                                    WHERE
	                                    TagName NOT IN (SELECT TagName FROM dbo.Tag)
                                    ;

                                    UPDATE 
	                                    dbo.Tag
                                    SET
	                                    TagDescription = UpdatingTag.TagDescription
                                    FROM
	                                    ##Tag AS UpdatingTag
                                    WHERE
	                                    UpdatingTag.TagName = dbo.Tag.TagName";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Tag (TagName, TagDescription)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.High;

        public string TableName => "Tag";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Tag>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
