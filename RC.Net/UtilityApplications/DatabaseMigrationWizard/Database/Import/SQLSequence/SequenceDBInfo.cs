using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceDBInfo : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##DBInfo](
	                                    [Name] [nvarchar](50) NOT NULL,
	                                    [Value] [nvarchar](max) NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.DBInfo(Name, Value)

                                    SELECT
	                                    UpdatingDBInfo.Name
	                                    , UpdatingDBInfo.Value
                                    FROM 
	                                    ##DBInfo AS UpdatingDBInfo
                                    WHERE
	                                    UpdatingDBInfo.Name NOT IN (SELECT Name FROM dbo.DBInfo)
	                                    AND
	                                    UpdatingDBInfo.Name NOT LIKE '%Version%'
                                    ;
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
                                        dbo.DBInfo.Name = UpdatingDBInfo.Name";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DBInfo (Name, Value)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DBInfo";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DBInfo>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
