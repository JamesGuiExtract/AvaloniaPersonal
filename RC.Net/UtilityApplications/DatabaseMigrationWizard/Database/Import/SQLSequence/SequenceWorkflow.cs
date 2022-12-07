using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
    class SequenceWorkflow : ISequence
    {
        private readonly string CreateTempTableSQL = @"
            CREATE TABLE [dbo].[##Workflow](
            [Guid] uniqueidentifier NOT NULL,
            [Name] [nvarchar](100) NULL,
            [WorkflowTypeCode] [nvarchar](1) NULL,
            [Description] [nvarchar](max) NULL,
            [LoadBalanceWeight] [int] NOT NULL
            )";

        private readonly string insertSQL = @"
            UPDATE
                dbo.Workflow 
            SET
                Name = UpdatingWorkflow.Name
                , WorkflowTypeCode = UpdatingWorkflow.WorkflowTypeCode
                , Description = UpdatingWorkflow.Description
                , LoadBalanceWeight = UpdatingWorkflow.LoadBalanceWeight

            FROM 
                ##Workflow AS UpdatingWorkflow

            WHERE
                dbo.Workflow.GUID = UpdatingWorkflow.GUID
            ;
            INSERT INTO dbo.Workflow (Name, WorkflowTypeCode, Description, LoadBalanceWeight, GUID)

            SELECT
                Name
                , WorkflowTypeCode
                , Description
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
                                    , LoadBalanceWeight)
            VALUES
            ";

        private readonly string InsertReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Warning'
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
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'Workflow'
                , CONCAT('The workflow ', ##Workflow.Name, ' will be added to the database')
            FROM
                ##Workflow
                    LEFT OUTER JOIN dbo.Workflow
                        ON dbo.Workflow.Guid = ##Workflow.Guid
            WHERE
                dbo.Workflow.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'Workflow'
                , CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its WorkflowTypeCode updated')
                , dbo.Workflow.WorkflowTypeCode
                , UpdatingWorkflow.WorkflowTypeCode

            FROM
                ##Workflow AS UpdatingWorkflow

                        INNER JOIN dbo.Workflow
                            ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

            WHERE
                ISNULL(UpdatingWorkflow.WorkflowTypeCode, '') <> ISNULL(dbo.Workflow.WorkflowTypeCode, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'Workflow'
                , CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its Description updated')
                , dbo.Workflow.Description
                , UpdatingWorkflow.Description

            FROM
                ##Workflow AS UpdatingWorkflow

                        INNER JOIN dbo.Workflow
                            ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

            WHERE
                ISNULL(UpdatingWorkflow.Description, '') <> ISNULL(dbo.Workflow.Description, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'Workflow'
                , CONCAT('The Workflow ', dbo.Workflow.Name, ' will have its LoadBalanceWeight updated')
                , dbo.Workflow.LoadBalanceWeight
                , UpdatingWorkflow.LoadBalanceWeight

            FROM
                ##Workflow AS UpdatingWorkflow

                        INNER JOIN dbo.Workflow
                            ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

            WHERE
                ISNULL(UpdatingWorkflow.LoadBalanceWeight, '') <> ISNULL(dbo.Workflow.LoadBalanceWeight, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'Workflow'
                , 'The workflow will have its name upated'
                , dbo.Workflow.Name
                , UpdatingWorkflow.Name

            FROM
                ##Workflow AS UpdatingWorkflow

                        INNER JOIN dbo.Workflow
                            ON dbo.Workflow.Guid = UpdatingWorkflow.Guid

            WHERE
                ISNULL(UpdatingWorkflow.Name, '') <> ISNULL(dbo.Workflow.Name, '')
            ;";


		public Priorities Priority => Priorities.High;

        public string TableName => "Workflow";

        public void ExecuteSequence(ImportOptions importOptions)
        {
            importOptions.ExecuteCommand(this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Workflow>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

            importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
        }
    }
}
