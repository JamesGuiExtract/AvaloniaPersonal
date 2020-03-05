using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceUserCreatedCounter : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##UserCreatedCounter](
	                                    [CounterName] [nvarchar](50) NOT NULL,
	                                    [Value] [bigint] NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.UserCreatedCounter(CounterName, Value)

                                    SELECT
	                                    CounterName
	                                    , Value
                                    FROM 
	                                    ##UserCreatedCounter
                                    WHERE
	                                    CounterName NOT IN (SELECT CounterName FROM dbo.UserCreatedCounter)
                                    ;

                                    UPDATE 
	                                    dbo.UserCreatedCounter
                                    SET
	                                    Value = UpdatingCounter.Value
                                    FROM
	                                    ##UserCreatedCounter AS UpdatingCounter
                                    WHERE
	                                    UpdatingCounter.CounterName = dbo.UserCreatedCounter.CounterName";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##UserCreatedCounter (Countername, Value)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.High;

        public string TableName => "UserCreatedCounter";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<UserCreatedCounter>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
