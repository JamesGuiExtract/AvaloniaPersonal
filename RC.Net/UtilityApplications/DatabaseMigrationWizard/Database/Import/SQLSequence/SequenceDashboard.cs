using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract.Database;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.SQLSequence
{
	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This is instantiated using generics")]
	class SequenceDashboard : ISequence
    {
        private readonly string CreateTempTableSQL = @"
                                        CREATE TABLE [dbo].[##Dashboard](
										[DashboardName] [nvarchar](100) NOT NULL,
										[Definition] [xml] NOT NULL,
										[LastImportedDate] [datetime] NOT NULL,
										[UseExtractedData] [bit] NULL,
										[ExtractedDataDefinition] [xml] NULL,
                                        [DashboardGuid] uniqueidentifier NOT NULL,
                                        [FAMUserGuid] uniqueidentifier NOT NULL,
										)";

        private readonly string insertSQL = @"
                                    UPDATE
										dbo.Dashboard
									SET
										DashboardName = UpdatingDashboard.DashboardName
										, Definition = UpdatingDashboard.Definition
										, ExtractedDataDefinition = UpdatingDashboard.ExtractedDataDefinition
										, FAMUserID = dbo.FAMUser.ID
										, LastImportedDate = UpdatingDashboard.LastImportedDate
										, UseExtractedData = UpdatingDashboard.UseExtractedData
									FROM
										##Dashboard AS UpdatingDashboard
													LEFT OUTER JOIN dbo.FAMUser
														ON dbo.FAMUser.Guid = UpdatingDashboard.FAMUserGuid
									WHERE
										dbo.Dashboard.Guid = UpdatingDashboard.DashboardGuid
									;
									INSERT INTO dbo.Dashboard(DashboardName, Definition, FAMUserID, LastImportedDate, UseExtractedData, ExtractedDataDefinition, Guid)

									SELECT
										UpdatingDashboard.DashboardName
										, UpdatingDashboard.Definition
										, dbo.FAMUser.ID
										, UpdatingDashboard.LastImportedDate
										, UpdatingDashboard.UseExtractedData
										, UpdatingDashboard.ExtractedDataDefinition
										, UpdatingDashboard.DashboardGuid
									FROM 
										##Dashboard AS UpdatingDashboard
												LEFT OUTER JOIN dbo.FAMUser
													ON dbo.FAMUser.Guid = UpdatingDashboard.FAMUserGuid
									WHERE
										UpdatingDashboard.DashboardGuid NOT IN (SELECT Guid FROM dbo.Dashboard)";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Dashboard (DashboardName, Definition, LastImportedDate, UseExtractedData, ExtractedDataDefinition, DashboardGuid, FAMUserGuid)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Low;

		public string TableName => "Dashboard";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<Dashboard>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
