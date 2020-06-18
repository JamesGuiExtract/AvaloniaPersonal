using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
    class SequenceAttributeName : ISequence
    {
        private readonly string CreateTempTableSQL = @"
CREATE TABLE [dbo].[##AttributeName](
[Name] [nvarchar](255) NULL,
[Guid] uniqueidentifier NOT NULL,
)";

        private readonly string insertSQL = @"
Update 
	dbo.AttributeName
SET
	Name = UpdatingAttributeName.Name
FROM
	##AttributeName AS UpdatingAttributeName
WHERE
	UpdatingAttributeName.Guid = dbo.AttributeName.Guid
;
INSERT INTO dbo.AttributeName(Name, Guid)

SELECT
	Name
	, Guid
FROM 
	##AttributeName

WHERE
	Guid NOT IN (SELECT Guid FROM dbo.AttributeName)";

        private readonly string insertTempTableSQL = @"
INSERT INTO ##AttributeName (Name, Guid)
VALUES
";

        private readonly string InsertReportingSQL = @"
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command,Classification, TableName, Message)
SELECT
    'Insert'
	, 'Warning'
	, 'AttributeName'
	, CONCAT('The Attribute Name ', dbo.AttributeName.Name, ' is present in the destination database, but NOT in the importing source.')
FROM
	dbo.AttributeName
		LEFT OUTER JOIN ##AttributeName
			ON dbo.AttributeName.Guid = ##AttributeName.GUID
WHERE
	##AttributeName.GUID IS NULL
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
    'Insert'
	, 'Info'
	, 'AttributeName'
	, CONCAT('The Attribute Name ', ##AttributeName.Name, ' will be added to the database')
FROM
	##AttributeName
		LEFT OUTER JOIN dbo.AttributeName
			ON dbo.AttributeName.Guid = ##AttributeName.GUID
WHERE
	dbo.AttributeName.Guid IS NULL";

        private readonly string UpdateReportingSQL = @"
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
SELECT
	'Update'
	, 'Info'
	, 'AttributeName'
	, 'The AttributeName will be updated'
	, dbo.AttributeName.Name
	, ##AttributeName.Name
FROM
	##AttributeName
		INNER JOIN dbo.AttributeName
			ON ##AttributeName.Guid = dbo.AttributeName.Guid
WHERE
	ISNULL(##AttributeName.Name, '') <> ISNULL(dbo.AttributeName.Name, '')";

        public Priorities Priority => Priorities.Low;

        public string TableName => "AttributeName";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<AttributeName>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
            importOptions.ExecuteCommand(this.UpdateReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
