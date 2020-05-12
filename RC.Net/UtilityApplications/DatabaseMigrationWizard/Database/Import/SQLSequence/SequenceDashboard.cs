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

		private readonly string InsertReportingSQL = @"
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
									SELECT
										'Insert'
	                                    , 'Warning'
										, 'Dashboard'
										, CONCAT('The dashboard ', dbo.Dashboard.DashboardName, ' is present in the destination database, but NOT in the importing source.')
									FROM
										dbo.Dashboard
											LEFT OUTER JOIN ##Dashboard
												ON dbo.Dashboard.Guid = ##Dashboard.DashboardGuid
									WHERE
										##Dashboard.DashboardGuid IS NULL
									;
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
									SELECT
										'Insert'
	                                    , 'Info'
										, 'Dashboard'
										, CONCAT('The dashboard ', ##Dashboard.DashboardName, ' will be added to the database')
									FROM
										##Dashboard
											LEFT OUTER JOIN dbo.Dashboard
												ON dbo.Dashboard.Guid = ##Dashboard.DashboardGuid
									WHERE
										dbo.Dashboard.Guid IS NULL";

		private readonly string UpdateReportingSQL = @"
									--DashboardName
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
									SELECT
										'Update'
										, 'Info'
										, 'Dashboard'
										, 'The DashboardName will be updated'
										, dbo.Dashboard.DashboardName
										, UpdatingDashboard.DashboardName
									FROM
										##Dashboard AS UpdatingDashboard
											INNER JOIN dbo.Dashboard
												ON dbo.Dashboard.Guid = UpdatingDashboard.DashboardGuid
									WHERE
										ISNULL(UpdatingDashboard.DashboardName, '') <> ISNULL(dbo.Dashboard.DashboardName, '')
									;
									--Definition
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
									SELECT
										'Update'
										, 'Info'
										, 'Dashboard'
										, CONCAT('The ', dbo.Dashboard.DashboardName , ' Definition will be updated')

									FROM
										##Dashboard AS UpdatingDashboard
											INNER JOIN dbo.Dashboard
												ON dbo.Dashboard.Guid = UpdatingDashboard.DashboardGuid
									WHERE
										ISNULL(CAST(UpdatingDashboard.Definition AS VARCHAR(MAX)), '') <> ISNULL(CAST(dbo.Dashboard.Definition AS VARCHAR(MAX)), '')
									;
									--ExtractedDataDefinition
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message)
									SELECT
										'Update'
										, 'Info'
										, 'Dashboard'
										, CONCAT('The ', dbo.Dashboard.DashboardName , ' ExtractedDataDefinition will be updated')

									FROM
										##Dashboard AS UpdatingDashboard
											INNER JOIN dbo.Dashboard
												ON dbo.Dashboard.Guid = UpdatingDashboard.DashboardGuid
									WHERE
										ISNULL(CAST(UpdatingDashboard.ExtractedDataDefinition AS VARCHAR(MAX)), '') <> ISNULL(CAST(dbo.Dashboard.ExtractedDataDefinition AS VARCHAR(MAX)), '')
									;
									--FamUserID
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
									SELECT
										'Update'
										, 'Info'
										, 'Dashboard'
										, CONCAT('The ', dbo.Dashboard.DashboardName , ' FAMUserID will be updated')
										, dbo.Dashboard.FAMUserID
										, dbo.FAMUser.ID
									FROM
										##Dashboard AS UpdatingDashboard
											LEFT OUTER JOIN dbo.FAMUser
												ON dbo.FAMUser.Guid = UpdatingDashboard.FAMUserGuid
		
											INNER JOIN dbo.Dashboard
												ON dbo.Dashboard.Guid = UpdatingDashboard.DashboardGuid

									WHERE
										ISNULL(dbo.FAMUser.ID, '') <> ISNULL(dbo.Dashboard.FAMUserID, '')
									;
									--LastImportDate
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
									SELECT
										'Update'
										, 'Info'
										, 'Dashboard'
										, CONCAT('The ', dbo.Dashboard.DashboardName , ' LastImportDate will be updated')
										, dbo.Dashboard.LastImportedDate
										, UpdatingDashboard.LastImportedDate
									FROM
										##Dashboard AS UpdatingDashboard
		
											INNER JOIN dbo.Dashboard
												ON dbo.Dashboard.Guid = UpdatingDashboard.DashboardGuid

									WHERE
										ISNULL(UpdatingDashboard.LastImportedDate, '') <> ISNULL(dbo.Dashboard.LastImportedDate, '')
									;
									--UseExtractedData
									INSERT INTO
										dbo.ReportingDatabaseMigrationWizard(Command, Classification, TableName, Message, Old_Value, New_Value)
									SELECT
										'Update'
										, 'Info'
										, 'Dashboard'
										, CONCAT('The ', dbo.Dashboard.DashboardName , ' UseExtractedData will be updated')
										, dbo.Dashboard.UseExtractedData
										, UpdatingDashboard.UseExtractedData
									FROM
										##Dashboard AS UpdatingDashboard
		
											INNER JOIN dbo.Dashboard
												ON dbo.Dashboard.Guid = UpdatingDashboard.DashboardGuid

									WHERE
										ISNULL(UpdatingDashboard.UseExtractedData, '') <> ISNULL(dbo.Dashboard.UseExtractedData, '')
									;
									";

		public Priorities Priority => Priorities.Low;

		public string TableName => "Dashboard";

		public void ExecuteSequence(ImportOptions importOptions)
        {
			importOptions.ExecuteCommand(this.CreateTempTableSQL);

			ImportHelper.PopulateTemporaryTable<Dashboard>($"{importOptions.ImportPath}\\{TableName}.json", this.insertTempTableSQL, importOptions);

			importOptions.ExecuteCommand(this.InsertReportingSQL);
			importOptions.ExecuteCommand(this.UpdateReportingSQL);

			importOptions.ExecuteCommand(this.insertSQL);
		}
    }
}
