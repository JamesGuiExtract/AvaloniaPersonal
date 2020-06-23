using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
    class SequenceDBInfo : ISequence
    {
        private readonly string CreateTempTableSQL = @"
CREATE TABLE [dbo].[##DBInfo](
[Name] [nvarchar](50) NOT NULL,
[Value] [nvarchar](max) NULL
)";

        private readonly string insertSQL = @"
UPDATE
	dbo.DBInfo
SET
	Value = UpdatingDBInfo.Value
FROM
	##DBInfo AS UpdatingDBInfo
WHERE
	LOWER(dbo.DBInfo.Name) NOT LIKE '%version%'
	AND
	dbo.DBInfo.Name <> 'DatabaseID'
	AND
	dbo.DBInfo.Name = UpdatingDBInfo.Name
";

        private readonly string insertTempTableSQL = @"
INSERT INTO ##DBInfo (Name, Value)
VALUES
";

        private readonly string ReportingSQL = @"
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'Insert'
	, 'Warning'
	, 'DBInfo'
	, CONCAT('The DBInfo ', dbo.DBInfo.Name, ' is present in the destination database, but NOT in the importing source.')
FROM
	dbo.DBInfo
		LEFT OUTER JOIN ##DBInfo
			ON dbo.DBInfo.Name = ##DBInfo.Name
WHERE
	##DBInfo.Name IS NULL
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'Insert'
	, 'Info'
	, 'DBInfo'
	, CONCAT('The DBInfo ', ##DBInfo.Name, ' will be added to the database')
FROM
	##DBInfo
		LEFT OUTER JOIN dbo.DBInfo
			ON dbo.DBInfo.Name = ##DBInfo.Name
WHERE
	dbo.DBInfo.Name IS NULL
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
SELECT
	'Update'
	, 'Warning'
	, 'DBInfo'
	, 'The ' + dbo.DBInfo.Name + ' will have its value updated'
	, dbo.DBInfo.Value
	, ##DBInfo.Value
FROM
	dbo.DBInfo
		LEFT OUTER JOIN ##DBInfo
			ON dbo.DBInfo.Name = ##DBInfo.Name
WHERE
	LOWER(dbo.DBInfo.Name) NOT LIKE '%schema%'
	AND
	dbo.DBInfo.Name <> 'DatabaseID'
	AND
	dbo.DBInfo.Value != ##DBInfo.Value
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'N/A'
	, 'Warning'
	, 'DBInfo'
	, CONCAT(dbo.DBInfo.Name, ' has a value of ', ##DBInfo.Value, ' in the importing source but a value of ', dbo.DBInfo.Value, ' in the destination. It is possible that the import may not behave as expected (missing tables/columns) if you proceed.')
FROM
	dbo.DBInfo
		LEFT OUTER JOIN ##DBInfo
			ON dbo.DBInfo.Name = ##DBInfo.Name
WHERE
	LOWER(dbo.DBInfo.Name) LIKE '%schema%'
	AND
	dbo.DBInfo.Value != ##DBInfo.Value
";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DBInfo";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DBInfo>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);
			importOptions.ExecuteCommand(this.GetSchemaCheckQuery(importOptions));

            importOptions.ExecuteCommand(this.insertSQL);
        }

		private int GetDBSchemaVersion(ImportOptions importOptions)
		{
			var fileProcessingDb = new FileProcessingDB()
			{
				DatabaseServer = importOptions.ConnectionInformation.DatabaseServer,
				DatabaseName = importOptions.ConnectionInformation.DatabaseName
			};

			return fileProcessingDb.CurrentDBSchemaVersion;
		}

		private string GetSchemaCheckQuery(ImportOptions importOptions)
		{
			return $@"
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'N/A'
	, 'Warning'
	, 'DBInfo'
	, 'Version mismatch! ApplicationVersion: {GetDBSchemaVersion(importOptions)} Database Version: ' + dbo.DBInfo.Value

FROM
	dbo.DBInfo
WHERE
	dbo.DBInfo.Name = 'FAMDBSchemaVersion'
	AND
	dbo.DBInfo.Value != { GetDBSchemaVersion(importOptions)}
";
		}
    }
}
