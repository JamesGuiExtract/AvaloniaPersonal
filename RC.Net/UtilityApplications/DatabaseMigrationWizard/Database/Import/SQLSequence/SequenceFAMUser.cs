using DatabaseMigrationWizard.Database.Input.DataTransformObject;
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
                    dbo.FAMUser.UserName = UpdatingFAMUser.UserName
                    OR
                    (
                        dbo.FAMUser.UserName IS NULL
                        AND
                        UpdatingFAMUser.UserName IS NULL
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
                        ON dbo.FAMUser.UserName = UpdatingFAMUser.UserName

            WHERE
                UpdatingFAMUser.UserName IS NULL
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
		            LEFT OUTER JOIN dbo.FAMUser
			            ON UpdatingFAMUser.UserName = dbo.FAMUser.UserName
            WHERE
                dbo.FAMUser.UserName IS NULL
            ;
			INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'N/A'
                , 'Warning'
                , 'FAMUser'
                , CONCAT('The FAMUser ', UpdatingFAMUser.UserName
					, ' has a full username of: '
					, dbo.FAMUser.FullUserName
					, ' in the database but a value of: '
					, UpdatingFAMUser.FullUserName
					, ' in the importing source. This will NOT be updated by this utility so please make manual adjustments as necessary.')
            FROM
                ##FAMUser AS UpdatingFAMUser
					INNER JOIN dbo.FAMUser
						ON dbo.FAMUser.UserName = UpdatingFAMUser.UserName
						AND dbo.FAMUser.FullUserName <> UpdatingFAMUser.FullUserName";

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
