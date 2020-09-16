using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Naming violations are a result of acronyms in the database.")]
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
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

        private readonly string InsertReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Warning'
                , 'Login'
                , CONCAT('The Login ', dbo.Login.UserName, ' is present in the destination database, but NOT in the importing source.')
            FROM
                dbo.Login
                    LEFT OUTER JOIN ##Login
                        ON dbo.Login.Guid = ##Login.GUID
            WHERE
                ##Login.GUID IS NULL
                AND
                Login.UserName <> 'admin'
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'Login'
                , CONCAT('The Login ', ##Login.UserName, ' will be added to the database')
            FROM
                ##Login
                    LEFT OUTER JOIN dbo.Login
                        ON dbo.Login.Guid = ##Login.GUID
            WHERE
                dbo.Login.Guid IS NULL
                AND
                ##Login.UserName <> 'admin'";

        private readonly string UpdateReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'Login'
                , 'The Login will be updated'
                , dbo.Login.UserName
                , UpdatingLogin.UserName
            FROM
                ##Login AS UpdatingLogin

                        INNER JOIN dbo.Login
                            ON dbo.Login.Guid = UpdatingLogin.Guid

            WHERE
                ISNULL(UpdatingLogin.UserName, '') <> ISNULL(dbo.Login.UserName, '')";

        public Priorities Priority => Priorities.Low;

        public string TableName => "Login";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Login>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
            importOptions.ExecuteCommand(this.UpdateReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
