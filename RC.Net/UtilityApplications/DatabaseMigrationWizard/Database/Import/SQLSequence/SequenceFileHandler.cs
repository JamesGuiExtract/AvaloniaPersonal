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

		public Priorities Priority => Priorities.Low;

		public string TableName => "FileHandler";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<FileHandler>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
