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

		private readonly string ReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
									SELECT
										'Warning'
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
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
									SELECT
										'Info'
										, 'FAMUser'
										, CONCAT('The FAMUser ', ##FAMUser.UserName, ' will be added to the database')
									FROM
										##FAMUser
											LEFT OUTER JOIN dbo.FAMUser
												ON dbo.FAMUser.Guid = ##FAMUser.GUID
									WHERE
										dbo.FAMUser.Guid IS NULL";

		public Priorities Priority => Priorities.High;

		public string TableName => "FAMUser";

		public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FAMUser>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
