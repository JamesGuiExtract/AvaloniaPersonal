using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
    class SequenceWorkflowNoFK : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##Workflow](
										[Guid] uniqueidentifier NOT NULL,
	                                    [Name] [nvarchar](100) NULL,
	                                    [WorkflowTypeCode] [nvarchar](1) NULL,
	                                    [Description] [nvarchar](max) NULL,
	                                    [DocumentFolder] [nvarchar](255) NULL,
	                                    [OutputFilePathInitializationFunction] [nvarchar](255) NULL,
	                                    [LoadBalanceWeight] [int] NOT NULL,
										[EditActionGUID] uniqueidentifier NULL,
										[EndActionGUID] uniqueidentifier NULL,
										[PostEditActionGUID] uniqueidentifier NULL,
										[PostWorkflowActionGUID] uniqueidentifier NULL,
										[StartActionGUID] uniqueidentifier NULL,
                                        [AttributeSetNameGuid] uniqueidentifier NULL,
                                        [MetadataFieldNameGuid] uniqueidentifier NULL
                                    )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.Workflow (Name, WorkflowTypeCode, Description, DocumentFolder, LoadBalanceWeight, GUID)

                                    SELECT
	                                    Name
	                                    , WorkflowTypeCode
	                                    , Description
	                                    , DocumentFolder
	                                    , LoadBalanceWeight
                                        , [Guid]
                                    FROM 
	                                    ##Workflow
                                    WHERE
	                                    GUID NOT IN (SELECT GUID FROM dbo.Workflow)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Workflow (
																	GUID
                                                                    , Name
                                                                    , WorkflowTypeCode
                                                                    , Description
                                                                    , DocumentFolder
                                                                    , OutputFilePathInitializationFunction
                                                                    , LoadBalanceWeight
                                                                    , EditActionGUID
                                                                    , EndActionGUID
                                                                    , PostEditActionGUID
                                                                    , PostWorkflowActionGUID
                                                                    , StartActionGUID
                                                                    , AttributeSetNameGuid
                                                                    , MetadataFieldNameGuid)
                                            VALUES
                                            ";

        private readonly string ReportingSQL = @"
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                            SELECT
	                                            'Warning'
												, 'Workflow'
	                                            , CONCAT('The workflow ', dbo.Workflow.Name, ' is present in the destination database, but NOT in the importing source.')
                                            FROM
	                                            dbo.Workflow
		                                            LEFT OUTER JOIN ##Workflow
			                                            ON dbo.Workflow.Guid = ##Workflow.Guid
                                            WHERE
	                                            ##Workflow.Guid IS NULL
                                            ;
                                            INSERT INTO
	                                            dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
                                            SELECT
	                                            'Info'
												, 'Workflow'
	                                            , CONCAT('The workflow ', ##Workflow.Name, ' will be added to the database')
                                            FROM
	                                            ##Workflow
		                                            LEFT OUTER JOIN dbo.Workflow
			                                            ON dbo.Workflow.Guid = ##Workflow.Guid
                                            WHERE
	                                            dbo.Workflow.Guid IS NULL";

        public Priorities Priority => Priorities.High;

        public string TableName => "Workflow";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Workflow>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.ReportingSQL);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
