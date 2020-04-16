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
                                        [Guid] uniqueidentifier NOT NULL,
                                       )";

        private readonly string insertSQL = @"
                                    UPDATE
	                                    dbo.DataEntryCounterDefinition
                                    SET
	                                    Name = UpdatingDataEntryCounterDefinition.Name
	                                    , AttributeQuery = UpdatingDataEntryCounterDefinition.AttributeQuery
	                                    , RecordOnLoad = UpdatingDataEntryCounterDefinition.RecordOnLoad
	                                    , RecordOnSave = UpdatingDataEntryCounterDefinition.RecordOnSave
                                    FROM
	                                    ##DataEntryCounterDefinition AS UpdatingDataEntryCounterDefinition
                                    WHERE
                                        DataEntryCounterDefinition.Guid = UpdatingDataEntryCounterDefinition.Guid
                                    ;
                                    INSERT INTO dbo.DataEntryCounterDefinition(Name, AttributeQuery, RecordOnLoad, RecordOnSave, Guid)

                                    SELECT
	                                    Name
	                                    , AttributeQuery
	                                    , RecordOnLoad
	                                    , RecordOnSave
	                                    , Guid
                                    FROM 
	                                    ##DataEntryCounterDefinition
                                    WHERE
	                                    Guid NOT IN (SELECT Guid FROM dbo.DataEntryCounterDefinition)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##DataEntryCounterDefinition (Name, AttributeQuery, RecordOnLoad, RecordOnSave, Guid)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DataEntryCounterDefinition";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DataEntryCounterDefinition>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
