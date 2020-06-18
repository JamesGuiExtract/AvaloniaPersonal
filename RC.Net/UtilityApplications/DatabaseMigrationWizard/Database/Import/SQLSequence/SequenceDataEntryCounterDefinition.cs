using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
    class SequenceDataEntryCounterDefinition : ISequence
    {
        private readonly string CreateTempTableSQL = @"
CREATE TABLE [dbo].[##DataEntryCounterDefinition](
[Name] [nvarchar](50) NOT NULL,
[AttributeQuery] [nvarchar](255) NOT NULL,
[RecordOnLoad] [bit] NOT NULL,
[RecordOnSave] [bit] NOT NULL,
[Guid] uniqueidentifier NOT NULL,
)";

        private readonly string insertSQL = @"
UPDATE
	dbo.DataEntryCounterDefinition
SET
	Name = UpdatingDataEntryCounterDefinition.Name
	, AttributeQuery = UpdatingDataEntryCounterDefinition.AttributeQuery
	, RecordOnLoad = UpdatingDataEntryCounterDefinition.RecordOnLoad
	, RecordOnSave = UpdatingDataEntryCounterDefinition.RecordOnSave
FROM
	##DataEntryCounterDefinition AS UpdatingDataEntryCounterDefinition
WHERE
    DataEntryCounterDefinition.Guid = UpdatingDataEntryCounterDefinition.Guid
;
INSERT INTO dbo.DataEntryCounterDefinition(Name, AttributeQuery, RecordOnLoad, RecordOnSave, Guid)

SELECT
	Name
	, AttributeQuery
	, RecordOnLoad
	, RecordOnSave
	, Guid
FROM 
	##DataEntryCounterDefinition
WHERE
	Guid NOT IN (SELECT Guid FROM dbo.DataEntryCounterDefinition)";

        private readonly string insertTempTableSQL = @"
INSERT INTO ##DataEntryCounterDefinition (Name, AttributeQuery, RecordOnLoad, RecordOnSave, Guid)
VALUES
";

        private readonly string InsertReportingSQL = @"
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'Insert'
	, 'Warning'
	, 'DataEntryCounterDefinition'
	, CONCAT('The DataEntryCounterDefinition ', dbo.DataEntryCounterDefinition.Name, ' is present in the destination database, but NOT in the importing source.')
FROM
	dbo.DataEntryCounterDefinition
		LEFT OUTER JOIN ##DataEntryCounterDefinition
			ON dbo.DataEntryCounterDefinition.Guid = ##DataEntryCounterDefinition.GUID
WHERE
	##DataEntryCounterDefinition.GUID IS NULL
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'Insert'
	, 'Info'
	, 'DataEntryCounterDefinition'
	, CONCAT('The DataEntryCounterDefinition ', ##DataEntryCounterDefinition.Name, ' will be added to the database')
FROM
	##DataEntryCounterDefinition
		LEFT OUTER JOIN dbo.DataEntryCounterDefinition
			ON dbo.DataEntryCounterDefinition.Guid = ##DataEntryCounterDefinition.GUID
WHERE
	dbo.DataEntryCounterDefinition.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
SELECT
	'Update'
	, 'Info'
	, 'DataEntryCounterDefinition'
	, 'The Data Entry Counter Definition Name will be updated'
	, dbo.DataEntryCounterDefinition.Name
	, UpdatingDataEntryCounterDefinition.Name
FROM
	##DataEntryCounterDefinition AS UpdatingDataEntryCounterDefinition
		
		INNER JOIN dbo.DataEntryCounterDefinition
			ON dbo.DataEntryCounterDefinition.Guid = UpdatingDataEntryCounterDefinition.Guid

WHERE
	ISNULL(UpdatingDataEntryCounterDefinition.Name, '') <> ISNULL(dbo.DataEntryCounterDefinition.Name, '')
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
SELECT
	'Update'
	, 'Info'
	, 'DataEntryCounterDefinition'
	, CONCAT('The DataEntryCounterDefinition ', dbo.DataEntryCounterDefinition.Name, ' will have its AttributeQuery updated')
	, dbo.DataEntryCounterDefinition.AttributeQuery
	, UpdatingDataEntryCounterDefinition.AttributeQuery
FROM
	##DataEntryCounterDefinition AS UpdatingDataEntryCounterDefinition
		
		INNER JOIN dbo.DataEntryCounterDefinition
			ON dbo.DataEntryCounterDefinition.Guid = UpdatingDataEntryCounterDefinition.Guid

WHERE
	ISNULL(UpdatingDataEntryCounterDefinition.AttributeQuery, '') <> ISNULL(dbo.DataEntryCounterDefinition.AttributeQuery, '')
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
SELECT
	'Update'
	, 'Info'
	, 'DataEntryCounterDefinition'
	, CONCAT('The DataEntryCounterDefinition ', dbo.DataEntryCounterDefinition.Name, ' will have its RecordOnLoad updated')
	, dbo.DataEntryCounterDefinition.RecordOnLoad
	, UpdatingDataEntryCounterDefinition.RecordOnLoad
FROM
	##DataEntryCounterDefinition AS UpdatingDataEntryCounterDefinition
		
		INNER JOIN dbo.DataEntryCounterDefinition
			ON dbo.DataEntryCounterDefinition.Guid = UpdatingDataEntryCounterDefinition.Guid

WHERE
	ISNULL(UpdatingDataEntryCounterDefinition.RecordOnLoad, '') <> ISNULL(dbo.DataEntryCounterDefinition.RecordOnLoad, '')
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
SELECT
	'Update'
	, 'Info'
	, 'DataEntryCounterDefinition'
	, CONCAT('The DataEntryCounterDefinition ', dbo.DataEntryCounterDefinition.Name, ' will have its RecordOnSave updated')
	, dbo.DataEntryCounterDefinition.RecordOnSave
	, UpdatingDataEntryCounterDefinition.RecordOnSave
FROM
	##DataEntryCounterDefinition AS UpdatingDataEntryCounterDefinition
		
		INNER JOIN dbo.DataEntryCounterDefinition
			ON dbo.DataEntryCounterDefinition.Guid = UpdatingDataEntryCounterDefinition.Guid

WHERE
	ISNULL(UpdatingDataEntryCounterDefinition.RecordOnSave, '') <> ISNULL(dbo.DataEntryCounterDefinition.RecordOnSave, '')";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DataEntryCounterDefinition";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DataEntryCounterDefinition>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
