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
                                        [Guid] uniqueidentifier NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    UPDATE 
	                                    dbo.UserCreatedCounter
                                    SET
	                                    Value = UpdatingCounter.Value
	                                    , CounterName = UpdatingCounter.CounterName
                                    FROM
	                                    ##UserCreatedCounter AS UpdatingCounter
                                    WHERE
	                                    UpdatingCounter.Guid = dbo.UserCreatedCounter.Guid
                                    ;
                                    INSERT INTO dbo.UserCreatedCounter(CounterName, Value, Guid)

                                    SELECT
	                                    CounterName
	                                    , Value
	                                    , Guid
                                    FROM 
	                                    ##UserCreatedCounter
                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.UserCreatedCounter)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##UserCreatedCounter (Countername, Value, Guid)
                                            VALUES
                                            ";

        private readonly string ReportingSQL = @"
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Warning'
	                                    , 'UserCreatedCounter'
	                                    , CONCAT('The UserCreatedCounter ', dbo.UserCreatedCounter.CounterName, ' is present in the destination database, but NOT in the importing source.')
                                    FROM
	                                    dbo.UserCreatedCounter
		                                    LEFT OUTER JOIN ##UserCreatedCounter
			                                    ON dbo.UserCreatedCounter.Guid = ##UserCreatedCounter.GUID
                                    WHERE
	                                    ##UserCreatedCounter.GUID IS NULL
                                    ;
                                    INSERT INTO
	                                    dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                    SELECT
	                                    'Info'
	                                    , 'UserCreatedCounter'
	                                    , CONCAT('The UserCreatedCounter ', ##UserCreatedCounter.CounterName, ' will be added to the database')
                                    FROM
	                                    ##UserCreatedCounter
		                                    LEFT OUTER JOIN dbo.UserCreatedCounter
			                                    ON dbo.UserCreatedCounter.Guid = ##UserCreatedCounter.GUID
                                    WHERE
	                                    dbo.UserCreatedCounter.Guid IS NULL";

        public Priorities Priority => Priorities.High;

        public string TableName => "UserCreatedCounter";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<UserCreatedCounter>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
