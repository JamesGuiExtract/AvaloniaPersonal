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

        private readonly string ReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Warning'
	                                    , 'FieldSearch'
	                                    , CONCAT('The FieldSearch ', dbo.FieldSearch.FieldName, ' is present in the destination database, but NOT in the importing source.')
                                    FROM
	                                    dbo.FieldSearch
		                                    LEFT OUTER JOIN ##FieldSearch
			                                    ON dbo.FieldSearch.Guid = ##FieldSearch.GUID
                                    WHERE
	                                    ##FieldSearch.GUID IS NULL
                                    ;
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Info'
	                                    , 'FieldSearch'
	                                    , CONCAT('The FieldSearch ', ##FieldSearch.FieldName, ' will be added to the database')
                                    FROM
	                                    ##FieldSearch
		                                    LEFT OUTER JOIN dbo.FieldSearch
			                                    ON dbo.FieldSearch.Guid = ##FieldSearch.GUID
                                    WHERE
	                                    dbo.FieldSearch.Guid IS NULL";

        public Priorities Priority => Priorities.Low;

        public string TableName => "FieldSearch";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FieldSearch>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
