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
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    UPDATE
	                                    dbo.FieldSearch
                                    SET
	                                    Enabled = UpdatingFieldSearch.Enabled
	                                    , AttributeQuery = UpdatingFieldSearch.AttributeQuery
	                                    , FieldName = UpdatingFieldSearch.FieldName
                                    FROM
	                                    ##FieldSearch AS UpdatingFieldSearch
                                    WHERE
	                                    UpdatingFieldSearch.Guid = dbo.FieldSearch.Guid
                                    ;
                                    INSERT INTO dbo.FieldSearch(Enabled, FieldName, AttributeQuery, Guid)

                                    SELECT
	                                    Enabled
	                                    , FieldName
	                                    , AttributeQuery
	                                    , Guid
                                    FROM 
	                                    ##FieldSearch

                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.FieldSearch)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##FieldSearch (Enabled, FieldName, AttributeQuery, Guid)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "FieldSearch";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FieldSearch>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
