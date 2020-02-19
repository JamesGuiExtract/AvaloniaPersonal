using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceDataEntryCounterType : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##DataEntryCounterType](
	                                    [Type] [nvarchar](1) NOT NULL,
	                                    [Description] [nvarchar](255) NOT NULL,
                                        )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.DataEntryCounterType(Type, Description)

                                    SELECT
	                                    Type
	                                    , Description
                                    FROM 
	                                    ##DataEntryCounterType
                                    WHERE
	                                    Type NOT IN (SELECT Type FROM dbo.DataEntryCounterType)
                                    ;
                                    UPDATE
	                                    dbo.DataEntryCounterType
                                    SET
	                                    Description = UpdatingDataEntryCounterType.Description

                                    FROM
	                                    ##DataEntryCounterType AS UpdatingDataEntryCounterType
                                    WHERE
                                        DataEntryCounterType.Type = UpdatingDataEntryCounterType.Type";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DataEntryCounterType (Type, Description)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DataEntryCounterType";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DataEntryCounterType>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
