using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceFAMUser : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##FAMUser](
	                                    [UserName] [nvarchar](50) NULL,
	                                    [FullUserName] [nvarchar](128) NULL,
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    UPDATE
	                                    dbo.FAMUser
                                    SET
	                                    FullUserName = UpdatingFAMUser.FullUserName
	                                    , UserName = UpdatingFAMUser.UserName
                                    FROM
	                                    ##FAMUser AS UpdatingFAMUser
                                    WHERE
	                                    UpdatingFAMUser.Guid = dbo.FAMUser.Guid
                                    ;
                                    INSERT INTO dbo.FAMUser(UserName, FullUserName, Guid)

                                    SELECT
	                                    UserName
	                                    , FullUserName
	                                    , Guid
                                    FROM 
	                                    ##FAMUser AS UpdatingFAMUser
                                    WHERE
	                                    UpdatingFAMUser.Guid NOT IN (SELECT Guid FROM dbo.FAMUser)
                                    ";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##FAMUser (UserName, FullUserName, Guid)
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
											LEFT OUTER JOIN ##FAMUser
												ON dbo.FAMUser.Guid = ##FAMUser.GUID
									WHERE
										##FAMUser.GUID IS NULL
									;
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
									SELECT
										'Insert'
	                                    , 'Info'
										, 'FAMUser'
										, CONCAT('The FAMUser ', ##FAMUser.UserName, ' will be added to the database')
									FROM
										##FAMUser
											LEFT OUTER JOIN dbo.FAMUser
												ON dbo.FAMUser.Guid = ##FAMUser.GUID
									WHERE
										dbo.FAMUser.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
									SELECT
										'Update'
										, 'Info'
										, 'FAMUser'
										, 'The Full username will be updated'
										, dbo.FAMUser.FullUserName
										, UpdatingFAMUser.FullUserName
									FROM
										##FAMUser AS UpdatingFAMUser
		
											INNER JOIN dbo.FAMUser
												ON dbo.FAMUser.Guid = UpdatingFAMUser.Guid

									WHERE
										ISNULL(UpdatingFAMUser.FullUserName, '') <> ISNULL(dbo.FAMUser.FullUserName, '')
									;
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
									SELECT
										'Update'
										, 'Info'
										, 'FAMUser'
										, 'The username will be updated'
										, dbo.FAMUser.UserName
										, UpdatingFAMUser.UserName
									FROM
										##FAMUser AS UpdatingFAMUser
		
											INNER JOIN dbo.FAMUser
												ON dbo.FAMUser.Guid = UpdatingFAMUser.Guid

									WHERE
										ISNULL(UpdatingFAMUser.UserName, '') <> ISNULL(dbo.FAMUser.UserName, '')
									;";

		public Priorities Priority => Priorities.High;

		public string TableName => "FAMUser";

		public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FAMUser>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
