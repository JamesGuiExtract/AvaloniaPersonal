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

        private readonly string ReportingSQL = @"
										INSERT INTO
											dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
										SELECT
											'Warning'
											, 'DataEntryCounterDefinition'
											, CONCAT('The DataEntryCounterDefinition ', dbo.DataEntryCounterDefinition.Name, ' is present in the destination database, but NOT in the importing source.')
										FROM
											dbo.DataEntryCounterDefinition
												LEFT OUTER JOIN ##DataEntryCounterDefinition
													ON dbo.DataEntryCounterDefinition.Guid = ##DataEntryCounterDefinition.GUID
										WHERE
											##DataEntryCounterDefinition.GUID IS NULL
										;
										INSERT INTO
											dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
										SELECT
											'Info'
											, 'DataEntryCounterDefinition'
											, CONCAT('The DataEntryCounterDefinition ', ##DataEntryCounterDefinition.Name, ' will be added to the database')
										FROM
											##DataEntryCounterDefinition
												LEFT OUTER JOIN dbo.DataEntryCounterDefinition
													ON dbo.DataEntryCounterDefinition.Guid = ##DataEntryCounterDefinition.GUID
										WHERE
											dbo.DataEntryCounterDefinition.Guid IS NULL";

        public Priorities Priority => Priorities.Low;

        public string TableName => "DataEntryCounterDefinition";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<DataEntryCounterDefinition>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
