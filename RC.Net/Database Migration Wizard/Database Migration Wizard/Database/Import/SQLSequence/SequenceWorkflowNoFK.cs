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
	                                    [Name] [nvarchar](100) NULL,
	                                    [WorkflowTypeCode] [nvarchar](1) NULL,
	                                    [Description] [nvarchar](max) NULL,
	                                    [StartActionID] [int] NULL,
	                                    [EndActionID] [int] NULL,
	                                    [PostWorkflowActionID] [int] NULL,
	                                    [DocumentFolder] [nvarchar](255) NULL,
	                                    [OutputAttributeSetID] [bigint] NULL,
	                                    [OutputFileMetadataFieldID] [int] NULL,
	                                    [OutputFilePathInitializationFunction] [nvarchar](255) NULL,
	                                    [LoadBalanceWeight] [int] NOT NULL,
	                                    [EditActionID] [int] NULL,
	                                    [PostEditActionID] [int] NULL,
                                        [StartAction] NVARCHAR(MAX) NULL,
                                        [EditAction] NVARCHAR(MAX) NULL,
                                        [EndAction] NVARCHAR(MAX) NULL,
                                        [PostEditAction] NVARCHAR(MAX) NULL,
                                        [PostWorkflowAction] NVARCHAR(MAX) NULL,
                                        [AttributeSetName] NVARCHAR(MAX) NULL,
                                        [MetadataFieldName] NVARCHAR(MAX) NULL
                                    )";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.Workflow (Name, WorkflowTypeCode, Description, DocumentFolder, LoadBalanceWeight)

                                    SELECT
	                                    Name
	                                    , WorkflowTypeCode
	                                    , Description
	                                    , DocumentFolder
	                                    , LoadBalanceWeight
                                    FROM 
	                                    ##Workflow
                                    WHERE
	                                    Name NOT IN (SELECT Name FROM dbo.Workflow)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Workflow (
                                                                    Name
                                                                    , WorkflowTypeCode
                                                                    , Description
                                                                    , StartActionID
                                                                    , EndActionID
                                                                    , PostWorkflowActionID
                                                                    , DocumentFolder
                                                                    , OutputAttributeSetID
                                                                    , OutputFileMetadataFieldID
                                                                    , OutputFilePathInitializationFunction
                                                                    , LoadBalanceWeight
                                                                    , EditActionID
                                                                    , PostEditActionID
                                                                    , StartAction
                                                                    , EditAction
                                                                    , EndAction
                                                                    , PostEditAction
                                                                    , PostWorkflowAction
                                                                    , AttributeSetName
                                                                    , MetadataFieldName)
                                            VALUES
                                            ";

        public Priorities Priority => Priorities.High;

        public string TableName => "Workflow";

        public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Workflow>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
