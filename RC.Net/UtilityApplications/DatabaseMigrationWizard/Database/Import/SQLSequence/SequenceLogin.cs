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
                                        [Guid] uniqueidentifier NOT NULL,
	                                    )";

        private readonly string insertSQL = @"
                                    UPDATE
	                                    dbo.Login
                                    SET
	                                    UserName = UpdatingLogin.UserName
                                    FROM
	                                    ##Login AS UpdatingLogin
                                    WHERE
	                                    dbo.Login.Guid = UpdatingLogin.Guid
                                    ;
                                    INSERT INTO dbo.Login(UserName, Password, Guid)

                                    SELECT
	                                    UserName
	                                    , Password
	                                    , Guid
                                    FROM 
	                                    ##Login AS UpdatingLogin
                                    WHERE
	                                    UpdatingLogin.Guid NOT IN (SELECT Guid FROM dbo.Login)
                                        AND
                                        UserName <> 'admin'";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Login (UserName, Password, Guid)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "Login";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Login>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
