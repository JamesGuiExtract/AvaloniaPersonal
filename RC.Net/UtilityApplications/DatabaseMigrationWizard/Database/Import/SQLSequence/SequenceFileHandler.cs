using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using reflection")]
	class SequenceFileHandler : ISequence
    {
        private readonly string CreateTempTableSQL = @"
            CREATE TABLE [dbo].[##FileHandler](
            [Enabled] [bit] NOT NULL,
            [AppName] [nvarchar](64) NOT NULL,
            [IconPath] [nvarchar](260) NULL,
            [ApplicationPath] [nvarchar](260) NOT NULL,
            [Arguments] [ntext] NULL,
            [AdminOnly] [bit] NOT NULL,
            [AllowMultipleFiles] [bit] NOT NULL,
            [SupportsErrorHandling] [bit] NOT NULL,
            [Blocking] [bit] NOT NULL,
            [WorkflowName] [nvarchar](100) NULL,
            [Guid] uniqueidentifier NOT NULL,
            )";

        private readonly string insertSQL = @"
            UPDATE
                dbo.FileHandler
            SET
                Enabled = UpdatingFileHandler.Enabled
                , IconPath = UpdatingFileHandler.IconPath
                , ApplicationPath = UpdatingFileHandler.ApplicationPath
                , Arguments = UpdatingFileHandler.Arguments
                , AdminOnly = UpdatingFileHandler.AdminOnly
                , AllowMultipleFiles = UpdatingFileHandler.AllowMultipleFiles
                , SupportsErrorHandling = UpdatingFileHandler.SupportsErrorHandling
                , Blocking = UpdatingFileHandler.Blocking
                , WorkflowName = UpdatingFileHandler.WorkflowName
                , AppName = UpdatingFileHandler.AppName
            FROM
                ##FileHandler AS UpdatingFileHandler
            WHERE
                dbo.FileHandler.Guid = UpdatingFileHandler.Guid
            ;
            INSERT INTO dbo.FileHandler(Enabled, AppName, IconPath, ApplicationPath, Arguments, AdminOnly, AllowMultipleFiles, SupportsErrorHandling, Blocking, WorkflowName, Guid)
            SELECT
                Enabled
                , AppName
                , IconPath
                , ApplicationPath
                , Arguments
                , AdminOnly
                , AllowMultipleFiles
                , SupportsErrorHandling
                , Blocking
                , WorkflowName
                , Guid
            FROM 
                ##FileHandler AS UpdatingFileHandler
            WHERE
                UpdatingFileHandler.Guid NOT IN (SELECT Guid From dbo.FileHandler)
            ";

        private readonly string insertTempTableSQL = @"
            INSERT INTO ##FileHandler (Enabled, AppName, IconPath, ApplicationPath, Arguments, AdminOnly, AllowMultipleFiles, SupportsErrorHandling, Blocking, WorkflowName, Guid)
            VALUES
            ";

		private readonly string InsertReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Warning'
                , 'FileHandler'
                , CONCAT('The FileHandler ', dbo.FileHandler.AppName, ' is present in the destination database, but NOT in the importing source.')
            FROM
                dbo.FileHandler
                    LEFT OUTER JOIN ##FileHandler
                        ON dbo.FileHandler.Guid = ##FileHandler.GUID
            WHERE
                ##FileHandler.GUID IS NULL
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Insert'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The FileHandler ', ##FileHandler.AppName, ' will be added to the database')
            FROM
                ##FileHandler
                    LEFT OUTER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = ##FileHandler.GUID
            WHERE
                dbo.FileHandler.Guid IS NULL
            ";

		private readonly string UpdateReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , 'The AppName will be updated'
                , dbo.FileHandler.AppName
                , UpdatingFileHandler.AppName
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.AppName, '') <> ISNULL(dbo.FileHandler.AppName, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The Enabled field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.Enabled
                , UpdatingFileHandler.Enabled
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.Enabled, '') <> ISNULL(dbo.FileHandler.Enabled, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The IconPath field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.IconPath
                , UpdatingFileHandler.IconPath
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.IconPath, '') <> ISNULL(dbo.FileHandler.IconPath, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The ApplicationPath field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.ApplicationPath
                , UpdatingFileHandler.ApplicationPath
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.ApplicationPath, '') <> ISNULL(dbo.FileHandler.ApplicationPath, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The Arguments field for ', dbo.FileHandler.AppName ,' will be updated')

            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                UpdatingFileHandler.Arguments NOT LIKE dbo.FileHandler.Arguments
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The AdminOnly field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.AdminOnly
                , UpdatingFileHandler.AdminOnly
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.AdminOnly, '') <> ISNULL(dbo.FileHandler.AdminOnly, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The AllowMultipleFiles field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.AllowMultipleFiles
                , UpdatingFileHandler.AllowMultipleFiles
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.AllowMultipleFiles, '') <> ISNULL(dbo.FileHandler.AllowMultipleFiles, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The SupportsErrorHandling field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.SupportsErrorHandling
                , UpdatingFileHandler.SupportsErrorHandling
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.SupportsErrorHandling, '') <> ISNULL(dbo.FileHandler.SupportsErrorHandling, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The Blocking field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.Blocking
                , UpdatingFileHandler.Blocking
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.Blocking, '') <> ISNULL(dbo.FileHandler.Blocking, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The WorkflowName field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.WorkflowName
                , UpdatingFileHandler.WorkflowName
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.WorkflowName, '') <> ISNULL(dbo.FileHandler.WorkflowName, '')
            ;
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
            SELECT
                'Update'
                , 'Info'
                , 'FileHandler'
                , CONCAT('The WorkflowName field for ', dbo.FileHandler.AppName ,' will be updated')
                , dbo.FileHandler.WorkflowName
                , UpdatingFileHandler.WorkflowName
            FROM
                ##FileHandler AS UpdatingFileHandler
                    
                    INNER JOIN dbo.FileHandler
                        ON dbo.FileHandler.Guid = UpdatingFileHandler.Guid

            WHERE
                ISNULL(UpdatingFileHandler.WorkflowName, '') <> ISNULL(dbo.FileHandler.WorkflowName, '')
            ;";

		private readonly string FilePathReportingSQL = @"
            INSERT INTO
                dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)

            --Select only rows that are defined without tags.
            SELECT
                'N/A'
                , 'Warning'
                , 'FileHandler'
                , 'In the filehandler table, the AppName' + ##FileHandler.AppName + ' has a realitive path defined. Please validate all filepaths in the filehandler after importing.'

            FROM 
                ##FileHandler
                    LEFT OUTER JOIN [dbo].[FileHandler]
                        ON FileHandler.GUID = ##FileHandler.Guid
            WHERE
                -- New Row
                (
                    [dbo].[FileHandler].GUID IS NULL
                    AND
                    (
                        ##FileHandler.IconPath LIKE '%<%>\%'
                        OR
                        ##FileHandler.ApplicationPath LIKE '%<%>\%'
                        OR
                        SUBSTRING(##FileHandler.Arguments, 0, CHARINDEX(' ', ##FileHandler.Arguments)) LIKE '%<%>\%'
                    )
                )
                OR
                -- Row being updated
                (
                    (
                        ISNULL(##FileHandler.IconPath, '') <> ISNULL([dbo].[FileHandler].IconPath, '')
                        AND
                        ##FileHandler.IconPath LIKE '%<%>\%'
                    )
                    OR
                    (
                        ISNULL(##FileHandler.ApplicationPath, '') <> ISNULL([dbo].[FileHandler].ApplicationPath, '')
                        AND
                        ##FileHandler.ApplicationPath LIKE '%<%>\%'
                    )
                    OR
                    (
                        ISNULL(CAST(##FileHandler.Arguments AS VARCHAR(MAX)), '') <> ISNULL(CAST([dbo].[FileHandler].Arguments AS VARCHAR(MAX)), '')
                        AND
                        --Only look at the first argument
                        SUBSTRING(##FileHandler.Arguments, 0, CHARINDEX(' ', ##FileHandler.Arguments)) LIKE '%<%>\%'
                    )
                )";

		public Priorities Priority => Priorities.Low;

		public string TableName => "FileHandler";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<FileHandler>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);
			importOptions.ExecuteCommand(this.FilePathReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
