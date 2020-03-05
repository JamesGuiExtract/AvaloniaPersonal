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

		public Priorities Priority => Priorities.High;

		public string TableName => "FAMUser";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FAMUser>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
