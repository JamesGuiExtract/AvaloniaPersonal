using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceFieldSearch : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##FieldSearch](
	                                    [Enabled] [bit] NOT NULL,
	                                    [FieldName] [nvarchar](64) NOT NULL,
	                                    [AttributeQuery] [nvarchar](256) NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.FieldSearch(Enabled, FieldName, AttributeQuery)

                                    SELECT
	                                    Enabled
	                                    , FieldName
	                                    , AttributeQuery
                                    FROM 
	                                    ##FieldSearch

                                    WHERE
	                                    FieldName NOT IN (SELECT FieldName FROM dbo.FieldSearch)
                                    ;
                                    UPDATE
	                                    dbo.FieldSearch
                                    SET
	                                    Enabled = UpdatingFieldSearch.Enabled
	                                    , AttributeQuery = UpdatingFieldSearch.AttributeQuery
                                    FROM
	                                    ##FieldSearch AS UpdatingFieldSearch
                                    WHERE
	                                    UpdatingFieldSearch.FieldName = dbo.FieldSearch.FieldName";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##FieldSearch (Enabled, FieldName, AttributeQuery)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "FieldSearch";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FieldSearch>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
