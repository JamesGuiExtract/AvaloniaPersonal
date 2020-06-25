using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
	class SequenceFAMUser : ISequence
    {
        private readonly string CreateTempTableSQL = @"
CREATE TABLE [dbo].[##FAMUser](
[UserName] [nvarchar](50) NULL,
[FullUserName] [nvarchar](128) NULL
)";

        private readonly string insertSQL = @"
 INSERT INTO dbo.FAMUser(UserName, FullUserName)

SELECT
	UserName
	, FullUserName
FROM 
	##FAMUser AS UpdatingFAMUser
WHERE
	NOT EXISTS
	(
	SELECT
		*
	FROM
		dbo.FAMUser
	WHERE
		(
			dbo.FAMUser.FullUserName = UpdatingFAMUser.FullUserName
			OR
			(
				dbo.FAMUser.FullUserName IS NULL
				AND
				UpdatingFAMUser.FullUserName IS NULL
			)
		)
		AND
		(
			dbo.FAMUser.UserName = UpdatingFAMUser.UserName
			OR
			(
				dbo.FAMUser.UserName IS NULL
				AND
				UpdatingFAMUser.UserName IS NULL
			)
		)
	)
";

        private readonly string insertTempTableSQL = @"
INSERT INTO ##FAMUser (UserName, FullUserName)
VALUES
";

		private readonly string InsertReportingSQL = @"
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'Insert'
	, 'Warning'
	, 'FAMUser'
	, CONCAT('The FAMUser ', dbo.FAMUser.UserName, ' is present in the destination database, but NOT in the importing source.')
FROM
	dbo.FAMUser
		LEFT OUTER JOIN ##FAMUser AS UpdatingFAMUser
			ON dbo.FAMUser.FullUserName = UpdatingFAMUser.FullUserName
			OR
			(
				dbo.FAMUser.FullUserName IS NULL
				AND
				UpdatingFAMUser.FullUserName IS NULL
			)
			AND
			(
				dbo.FAMUser.UserName = UpdatingFAMUser.UserName
				OR
				(
					dbo.FAMUser.UserName IS NULL
					AND
					UpdatingFAMUser.UserName IS NULL
				)
			)
WHERE
	UpdatingFAMUser.UserName IS NULL
	AND
	UpdatingFAMUser.FullUserName IS NULL
;
INSERT INTO
	dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
SELECT
	'Insert'
	, 'Info'
	, 'FAMUser'
	, CONCAT('The FAMUser ', UpdatingFAMUser.UserName, ' will be added to the database')
FROM
	##FAMUser AS UpdatingFAMUser
WHERE
	NOT EXISTS
	(
	SELECT
		*
	FROM
		dbo.FAMUser
	WHERE
		(
			dbo.FAMUser.FullUserName = UpdatingFAMUser.FullUserName
			OR
			(
				dbo.FAMUser.FullUserName IS NULL
				AND
				UpdatingFAMUser.FullUserName IS NULL
			)
		)
		AND
		(
			dbo.FAMUser.UserName = UpdatingFAMUser.UserName
			OR
			(
				dbo.FAMUser.UserName IS NULL
				AND
				UpdatingFAMUser.UserName IS NULL
			)
		)
	)";

		public Priorities Priority => Priorities.High;

		public string TableName => "FAMUser";

		public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FAMUser>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
