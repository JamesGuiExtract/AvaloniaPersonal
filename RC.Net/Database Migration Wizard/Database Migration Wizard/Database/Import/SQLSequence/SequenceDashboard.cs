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
										[UserName] NVARCHAR(MAX) NULL,
										[FullUserName] NVARCHAR(MAX) NULL
										)";

        private readonly string insertSQL = @"
                                    INSERT INTO dbo.Dashboard(DashboardName, Definition, FAMUserID, LastImportedDate, UseExtractedData, ExtractedDataDefinition)

									SELECT
										UpdatingDashboard.DashboardName
										, UpdatingDashboard.Definition
										, dbo.FAMUser.ID
										, UpdatingDashboard.LastImportedDate
										, UpdatingDashboard.UseExtractedData
										, UpdatingDashboard.ExtractedDataDefinition
									FROM 
										##Dashboard AS UpdatingDashboard
												LEFT OUTER JOIN dbo.FAMUser
													ON dbo.FAMUser.UserName = UpdatingDashboard.UserName
													AND (
															dbo.FAMUser.FullUserName = UpdatingDashboard.FullUserName
															OR
															(
																UpdatingDashboard.FullUserName IS NULL
																AND
																dbo.FAMUser.FullUserName IS NULL
															)
														)
									WHERE
										UpdatingDashboard.DashboardName NOT IN (SELECT DashboardName FROM dbo.Dashboard)
									;

									UPDATE
										dbo.Dashboard
									SET
										Definition = UpdatingDashboard.Definition
										, ExtractedDataDefinition = UpdatingDashboard.ExtractedDataDefinition
										, FAMUserID = dbo.FAMUser.ID
										, LastImportedDate = UpdatingDashboard.LastImportedDate
										, UseExtractedData = UpdatingDashboard.UseExtractedData
									FROM
										##Dashboard AS UpdatingDashboard
																LEFT OUTER JOIN dbo.FAMUser
																	ON dbo.FAMUser.UserName = UpdatingDashboard.UserName
																	AND (
																			dbo.FAMUser.FullUserName = UpdatingDashboard.FullUserName
																			OR
																			(
																				UpdatingDashboard.FullUserName IS NULL
																				AND
																				dbo.FAMUser.FullUserName IS NULL
																			)
																		)
									WHERE
										dbo.Dashboard.DashboardName = UpdatingDashboard.DashboardName";

        private readonly string insertTempTableSQL = @"
                                            INSERT INTO ##Dashboard (DashboardName, Definition, LastImportedDate, UseExtractedData, ExtractedDataDefinition, UserName, FullUserName)
                                            VALUES
                                            ";

		public Priorities Priority => Priorities.Low;

		public string TableName => "Dashboard";

		public void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions)
        {
            DBMethods.ExecuteDBQuery(dbConnection, this.CreateTempTableSQL);

            ImportHelper.PopulateTemporaryTable<Dashboard>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, dbConnection);

            DBMethods.ExecuteDBQuery(dbConnection, this.insertSQL);
        }
    }
}
