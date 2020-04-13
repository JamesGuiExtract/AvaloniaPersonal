using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Naming violations are a result of acronyms in the database.")]
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceLogin : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##Login](
	                                    [UserName] [nvarchar](50) NOT NULL,
	                                    [Password] [nvarchar](128) NOT NULL,
	                                    )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.Login(UserName, Password)

                                    SELECT
	                                    UserName
	                                    , Password
                                    FROM 
	                                    ##Login
                                    WHERE
	                                    UserName NOT IN (SELECT UserName FROM dbo.Login)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Login (UserName, Password)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "Login";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Login>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
