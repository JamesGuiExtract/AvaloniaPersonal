using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
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

		private readonly string ReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
									SELECT
										'Warning'
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
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)
									SELECT
										'Info'
										, 'FileHandler'
										, CONCAT('The FileHandler ', ##FileHandler.AppName, ' will be added to the database')
									FROM
										##FileHandler
											LEFT OUTER JOIN dbo.FileHandler
												ON dbo.FileHandler.Guid = ##FileHandler.GUID
									WHERE
										dbo.FileHandler.Guid IS NULL
									;
									-- Find all the rows that only define paths using tags. If they use only tags then there are no issues.
									WITH TAGSONLY AS
									(
										SELECT
											[AppName]
											, [IconPath]
											, [ApplicationPath]
											, [Arguments]
										FROM 
											[dbo].[FileHandler]
										WHERE
											FileHandler.IconPath LIKE '%<%>\%'
											AND
											FileHandler.ApplicationPath LIKE '%<%>\%'
											AND
											--Only look at the first argument
											SUBSTRING(Arguments, 0, CHARINDEX(' ', Arguments)) LIKE '%<%>\%'
									)
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Classification, TableName, Message)

									--Select only rows that are defined without tags.
									SELECT
										'Warning'
										, 'FileHandler'
										, 'In the filehandler table, the AppName' + dbo.FileHandler.AppName ' has a realitive path defined. Please validate all filepaths in the filehandler after importing.'
									FROM
										dbo.FileHandler
											LEFT OUTER JOIN TAGSONLY
												ON dbo.FileHandler.AppName = TAGSONLY.AppName
									WHERE
										TAGSONLY.AppName IS NULL";

		public Priorities Priority => Priorities.Low;

		public string TableName => "FileHandler";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<FileHandler>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.ReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
