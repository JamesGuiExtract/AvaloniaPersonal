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
										)";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.FileHandler(Enabled, AppName, IconPath, ApplicationPath, Arguments, AdminOnly, AllowMultipleFiles, SupportsErrorHandling, Blocking, WorkflowName)
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
									FROM 
										##FileHandler AS UpdatingFileHandler
									WHERE
										UpdatingFileHandler.AppName NOT IN (SELECT Appname From dbo.FileHandler)
									;
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
									FROM
										##FileHandler AS UpdatingFileHandler
									WHERE
										dbo.FileHandler.AppName = UpdatingFileHandler.AppName
                                    ";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##FileHandler (Enabled, AppName, IconPath, ApplicationPath, Arguments, AdminOnly, AllowMultipleFiles, SupportsErrorHandling, Blocking, WorkflowName)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Low;

		public string TableName => "FileHandler";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<FileHandler>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
