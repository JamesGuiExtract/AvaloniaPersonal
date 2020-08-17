using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
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

        private readonly string InsertReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Warning'
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
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'FieldSearch'
                , CONCAT('The FieldSearch ', ##FieldSearch.FieldName, ' will be added to the database')
            FROM
                ##FieldSearch
                    LEFT OUTER JOIN dbo.FieldSearch
                        ON dbo.FieldSearch.Guid = ##FieldSearch.GUID
            WHERE
                dbo.FieldSearch.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FieldSearch'
                , 'The FieldName will be updated'
                , dbo.FieldSearch.FieldName
                , UpdatingFieldSearch.FieldName
            FROM
                ##FieldSearch AS UpdatingFieldSearch
                    
                    INNER JOIN dbo.FieldSearch
                        ON dbo.FieldSearch.Guid = UpdatingFieldSearch.Guid

            WHERE
                ISNULL(UpdatingFieldSearch.FieldName, '') <> ISNULL(dbo.FieldSearch.FieldName, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FieldSearch'
                , CONCAT('The enabled field on ', dbo.FieldSearch.FieldName ,' will be updated.')
                , dbo.FieldSearch.Enabled
                , UpdatingFieldSearch.Enabled
            FROM
                ##FieldSearch AS UpdatingFieldSearch
                    
                    INNER JOIN dbo.FieldSearch
                        ON dbo.FieldSearch.Guid = UpdatingFieldSearch.Guid

            WHERE
                ISNULL(UpdatingFieldSearch.Enabled, '') <> ISNULL(dbo.FieldSearch.Enabled, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FieldSearch'
                , CONCAT('The AttributeQuery field on ', dbo.FieldSearch.FieldName ,' will be updated')
                , dbo.FieldSearch.AttributeQuery
                , UpdatingFieldSearch.AttributeQuery
            FROM
                ##FieldSearch AS UpdatingFieldSearch
                    
                    INNER JOIN dbo.FieldSearch
                        ON dbo.FieldSearch.Guid = UpdatingFieldSearch.Guid

            WHERE
                ISNULL(UpdatingFieldSearch.AttributeQuery, '') <> ISNULL(dbo.FieldSearch.AttributeQuery, '')";


		public Priorities Priority => Priorities.Low;

        public string TableName => "FieldSearch";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FieldSearch>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
