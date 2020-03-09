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

        public Priorities Priority => Priorities.High;

        public string TableName => "Workflow";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Workflow>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
