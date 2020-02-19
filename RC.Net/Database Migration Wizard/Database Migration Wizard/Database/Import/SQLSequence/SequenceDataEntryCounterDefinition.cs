using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceDataEntryCounterDefinition : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##DataEntryCounterDefinition](
	                                    [Name] [nvarchar](50) NOT NULL,
	                                    [AttributeQuery] [nvarchar](255) NOT NULL,
	                                    [RecordOnLoad] [bit] NOT NULL,
	                                    [RecordOnSave] [bit] NOT NULL,
                                       )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.DataEntryCounterDefinition(Name, AttributeQuery, RecordOnLoad, RecordOnSave)

                                    SELECT
	                                    Name
	                                    , AttributeQuery
	                                    , RecordOnLoad
	                                    , RecordOnSave
                                    FROM 
	                                    ##DataEntryCounterDefinition
                                    WHERE
	                                    Name NOT IN (SELECT Name FROM dbo.DataEntryCounterDefinition)
                                    ;
                                    UPDATE
	                                    dbo.DataEntryCounterDefinition
                                    SET
	                                    AttributeQuery = UpdatingDataEntryCounterDefinition.AttributeQuery
	                                    , RecordOnLoad = UpdatingDataEntryCounterDefinition.RecordOnLoad
	                                    , RecordOnSave = UpdatingDataEntryCounterDefinition.RecordOnSave
                                    FROM
	                                    ##DataEntryCounterDefinition AS UpdatingDataEntryCounterDefinition
                                    WHERE
                                        DataEntryCounterDefinition.Name = UpdatingDataEntryCounterDefinition.Name";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DataEntryCounterDefinition (Name, AttributeQuery, RecordOnLoad, RecordOnSave)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DataEntryCounterDefinition";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DataEntryCounterDefinition>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
