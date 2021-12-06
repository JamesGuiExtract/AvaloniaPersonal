using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Extract;
using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="DataEntryQuery"/> class.
    /// </summary>
    [TestFixture]
    [Category("DatabaseMigrationWizardImports")]
    public class TestImportErrors
    {
        private static readonly FAMTestDBManager<TestExports> FamTestDbManager = new FAMTestDBManager<TestExports>();

        private static readonly string DropTempTables = @"declare @sql nvarchar(max)
                                                        select @sql = isnull(@sql+';', '') + 'drop table ' + quotename(name)
                                                        from tempdb..sysobjects
                                                        where name like '##%'
                                                        exec (@sql)";

        /// <summary>
        /// The testing methodology here is as follows
        /// I'm going to define a rename as changing both the name and the values of whatever is already there.
        /// 1. Run the import to populate inital values in the database.
        /// 2. Rename a bunch of values.
        /// 3. Rerun the import with those renames
        /// 4. Ensure those records got renamed.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
        }

        /// <summary>
        /// After triggering an error, make sure that all changes are rolled back.
        /// I'm testing this by making sure the action table remains empty.
        /// </summary>
        [Test, Category("Automated")]
        public static void EnsureRollbackIfError()
        {
            string databaseName = "Test_EnsureRollback";
            var database = FamTestDbManager.GetNewDatabase(databaseName);
            ImportOptions ImportOptions = new ImportOptions()
            {
                ImportPath = Path.GetTempPath() + $"EnsureRollback\\",
                ConnectionInformation = new Database.ConnectionInformation() { DatabaseName = databaseName, DatabaseServer = "(local)" }
            };

            Directory.CreateDirectory(ImportOptions.ImportPath);
            var databaseMigrationWizardTestHelper = new DatabaseMigrationWizardTestHelper();
            databaseMigrationWizardTestHelper.LoadInitialValues();
            databaseMigrationWizardTestHelper.Actions.Add(new Database.Input.DataTransformObject.Action() { ActionGuid = Guid.Parse("1c317bec-bfc3-4b7a-b2f3-0a4eb8ffe173"), ASCName = "SameNameAction" });
            databaseMigrationWizardTestHelper.Actions.Add(new Database.Input.DataTransformObject.Action() { ActionGuid = Guid.Parse("56f6ccc6-da61-483c-bfd7-0c80af951bca"), ASCName = "SameNameAction" });
            databaseMigrationWizardTestHelper.WriteEverythingToDirectory(ImportOptions.ImportPath);
            try
            {
                using var helper = new ImportHelper(ImportOptions, new Progress<string>((garbage) => { }));
                helper.Import();
                // The import should fail because dbinfo tables should not lign up
                Assert.True(false);
            }
            catch (ExtractException)
            {
                if(database.GetActions().Size != 0)
                {
                    throw new ExtractException("ELI49724", "The actions should not have been imported, and should have been rolledback");
                }
            }
            finally
            {
                FamTestDbManager.RemoveDatabase(databaseName);
                Directory.Delete(ImportOptions.ImportPath, true);
            }
        }

        /// <summary>
        /// Created as a result of: https://extract.atlassian.net/browse/ISSUE-17252
        /// Ensure thats existing FAM users never cause issues on import.
        /// </summary>
        [Test, Category("Automated")]
        public static void FAMUserBoundsCase()
        {
            string databaseName = "Test_FamUserBoundCase";
            var database = FamTestDbManager.GetNewDatabase(databaseName);

            ImportOptions ImportOptions = new ImportOptions()
            {
                ImportPath = Path.GetTempPath() + $"FamUserBoundCase\\",
                ConnectionInformation = new Database.ConnectionInformation() { DatabaseName = databaseName, DatabaseServer = "(local)" }
            };

            Directory.CreateDirectory(ImportOptions.ImportPath);
            var databaseMigrationWizardTestHelper = new DatabaseMigrationWizardTestHelper();
            databaseMigrationWizardTestHelper.LoadInitialValues();
            // Do the initial import to get user names in the FAM user table.
            databaseMigrationWizardTestHelper.WriteEverythingToDirectory(ImportOptions.ImportPath);
            using ImportHelper helper = new ImportHelper(ImportOptions, new Progress<string>((garbage) => { }));
            helper.Import();
            helper.CommitTransaction();
            database.ExecuteCommandQuery(DropTempTables);

            databaseMigrationWizardTestHelper.FAMUsers.Clear();
            databaseMigrationWizardTestHelper.FAMUsers.Add(new FAMUser() { UserName = "Trever_Gannon", FullUserName = "fgdsdfgdgfdfg" });
            databaseMigrationWizardTestHelper.Dashboards.Clear();
            databaseMigrationWizardTestHelper.Dashboards.Add(new Dashboard()
            {
                DashboardName = "DashboardDiffFullUserName",
                Definition = "<Dashboard CurrencyCulture=\"en-US\"><Title Text=\"Dashboard\" /><DataSources><SqlDataSource Name=\"SQL Data Source 1\" ComponentName=\"dashboardSqlDataSource1\" DataProcessingMode=\"Client\"><Connection Name=\"localhost_Essentia_Customer_Builds_Connection\" ProviderKey=\"MSSqlServer\"><Parameters><Parameter Name=\"server\" Value=\"zeus\" /><Parameter Name=\"database\" Value=\"Essentia_Customer_Builds\" /><Parameter Name=\"useIntegratedSecurity\" Value=\"True\" /><Parameter Name=\"read only\" Value=\"1\" /><Parameter Name=\"generateConnectionHelper\" Value=\"false\" /><Parameter Name=\"userid\" Value=\"\" /><Parameter Name=\"password\" Value=\"\" /></Parameters></Connection><Query Type=\"CustomSqlQuery\" Name=\"Query\"><Parameter Name=\"ReportingPeroid\" Type=\"DevExpress.DataAccess.Expression\">(System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)(GetDate(Iif(?ReportingPeroid = '2 Weeks', AddDays(Today(), -14), ?ReportingPeroid = '1 Month', AddMonths(Today(), -1), ?ReportingPeroid = '2 Months', AddMonths(Today(), -3), ?ReportingPeroid = '6 Months', AddMonths(Today(), -6), ?ReportingPeroid = '1 Year', AddYears(Today(), -1), ?ReportingPeroid = 'Since Go Live (9/25/2018)', GetDate('9/25/2018 12:00AM'), AddDays(Today(), -14)))\r\n)</Parameter><Sql>--DECLARE @REPORTINGPEROID DATE;\r\n--SET @REPORTINGPEROID = '1/20/2016';\r\n\r\nSELECT\r\n\tCTE.FullUserName\r\n\t, COUNT( DISTINCT OriginalFileID) AS InputDocumentCount\r\n\t, COUNT( DISTINCT CTE.DestFileID) AS OutputDocumentCount\r\n\t, CTE.InputDate\r\n\t, MAX(CurrentlyActive) AS CurrentlyActive\r\n\t, SUM( TotalMinutes) AS ActiveMinutes\r\n\t, CTE.Name\r\n\t, SUM( Pages) AS TotalPages\r\n\t, SUM(Correct) AS CorrectSum\r\n\t, SUM(Expected) AS ExpectedSum\r\n\t, SUM(Expected) - SUM(Correct) AS IncorrectPlusMissed\r\nFROM\r\n\t( -- This is necessary for getting the total minutes and pages. These values should only be applied once per doc, hence the rownum\r\n\tSELECT DISTINCT\r\n\t\tdbo.FAMUser.FullUserName\r\n\t\t, ReportingHIMStats.OriginalFileID\r\n\t\t, ReportingHIMStats.DestFileID\r\n\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t, vUsersWithActive.CurrentlyActive\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY TotalMinutes ORDER BY TotalMinutes ) = 1 THEN TotalMinutes\r\n\t\t\tELSE 0\r\n\t\t  END AS TotalMinutes\r\n\t\t, Workflow.Name\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY dbo.FAMFile.Pages, dbo.FAMFile.ID ORDER BY dbo.FAMFile.Pages desc ) = 1 THEN dbo.FAMFile.Pages\r\n\t\t\tELSE 0\r\n\t\t  END AS Pages\r\n\r\n\tFROM\r\n\t\tdbo.FAMUser\r\n\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.FAMFile\r\n\t\t\t\t\tON dbo.FAMFile.ID = ReportingHIMStats.OriginalFileID\r\n\r\n\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\tINNER JOIN dbo.vUsersWithActive \r\n\t\t\t\t\t\tON vUsersWithActive.FAMUserID = vFAMUserInputEventsTime.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\tWHERE\r\n\t\tFAMUser.FullUserName IS NOT NULL\r\n\t) AS CTE\r\n\t\tLEFT OUTER JOIN ( --This subquery is necessary for reporing on the ReportingDataCaptureAccuracy. They require a different grouping than pages/active hours\r\n\t\t\t\t\t\t\tSELECT DISTINCT\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Correct) AS Correct\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Expected) AS Expected\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\tdbo.FAMUser\r\n\t\t\t\t\t\t\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\t\t\t\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\t\t\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\t\t\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\t\t\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\t\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\t\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\t\t\t\t\t\t\t\t\t\tLEFT OUTER JOIN dbo.ReportingDataCaptureAccuracy\r\n\t\t\t\t\t\t\t\t\t\t\tON ReportingDataCaptureAccuracy.FileID = ReportingHIMStats.DestFileID\r\n\r\n\t\t\t\t\t\t\tWHERE\r\n\t\t\t\t\t\t\t\tFAMUser.FullUserName IS NOT NULL\r\n\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\t\t\t\t\t\t\t) AS CTE2\r\n\t\t\t\t\t\t\t\tON CTE2.DestFileID = CTE.DestFileID\r\n\t\t\t\t\t\t\t\tAND CTE2.FullUserName = CTE.FullUserName\r\n\t\t\t\t\t\t\t\tAND CTE.InputDate = CTE2.InputDate\r\n\t\t\t\t\t\t\t\tAND CTE.Name = CTE2.Name\r\nGROUP BY\r\n\tCTE.FullUserName\r\n\t, CTE.InputDate\r\n\t, CTE.Name\r\n</Sql></Query><ResultSchema><DataSet Name=\"SQL Data Source 1\"><View Name=\"Query\"><Field Name=\"FullUserName\" Type=\"String\" /><Field Name=\"InputDocumentCount\" Type=\"Int32\" /><Field Name=\"OutputDocumentCount\" Type=\"Int32\" /><Field Name=\"InputDate\" Type=\"DateTime\" /><Field Name=\"CurrentlyActive\" Type=\"Int32\" /><Field Name=\"ActiveMinutes\" Type=\"Double\" /><Field Name=\"Name\" Type=\"String\" /><Field Name=\"TotalPages\" Type=\"Int32\" /><Field Name=\"CorrectSum\" Type=\"Int64\" /><Field Name=\"ExpectedSum\" Type=\"Int64\" /><Field Name=\"IncorrectPlusMissed\" Type=\"Int64\" /></View></DataSet></ResultSchema><ConnectionOptions CloseConnection=\"true\" DbCommandTimeout=\"0\" /><CalculatedFields><CalculatedField Name=\"Pages/Hour\" Expression=\"Sum([TotalPages]) / (SUM([ActiveMinutes]) / 60)\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"ActiveHours\" Expression=\"SUM([ActiveMinutes]) / 60\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"Documents/Hour\" Expression=\"SUM([OutputDocumentCount]) / [ActiveHours]\" DataType=\"Auto\" DataMember=\"Query\" /></CalculatedFields></SqlDataSource></DataSources><Parameters><Parameter Name=\"ReportingPeroid\" Value=\"2 Weeks\"><StaticListLookUpSettings><Values><Value>2 Weeks</Value><Value>1 Month</Value><Value>2 Months</Value><Value>6 Months</Value><Value>1 Year</Value><Value>Since Go Live (9/25/2018)</Value></Values></StaticListLookUpSettings></Parameter></Parameters><Items><Card ComponentName=\"cardDashboardItem1\" Name=\"Cards 1\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"InputDocumentCount\" Name=\"Input Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem2\" Name=\"Cards 2\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem1\" Name=\"User Productivity\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Measure DataMember=\"CurrentlyActive\" Name=\"Active\" DefaultId=\"DataItem0\" /><Dimension DataMember=\"FullUserName\" DefaultId=\"DataItem1\" /><Measure DataMember=\"InputDocumentCount\" Name=\"Input Docs\" DefaultId=\"DataItem2\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Docs\" DefaultId=\"DataItem3\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ActiveMinutes\" Name=\"Active Minutes\" DefaultId=\"DataItem4\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem5\" /><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem6\" /><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem7\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ExpectedSum\" Name=\"Indexed Fields\" DefaultId=\"DataItem8\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"CorrectSum\" Name=\"Auto-Populated\" DefaultId=\"DataItem9\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"IncorrectPlusMissed\" DefaultId=\"DataItem10\" /></DataItems><FormatRules><GridItemFormatRule Name=\"FormatRule 1\" DataItem=\"DataItem0\"><FormatConditionRangeSet ValueType=\"Number\"><RangeSet><Ranges><RangeInfo><Value Type=\"System.Decimal\" Value=\"0\" /><IconSettings IconType=\"ShapeRedCircle\" /></RangeInfo><RangeInfo><Value Type=\"System.Int32\" Value=\"1\" /><IconSettings IconType=\"ShapeGreenCircle\" /></RangeInfo></Ranges></RangeSet></FormatConditionRangeSet></GridItemFormatRule></FormatRules><GridColumns><GridMeasureColumn Weight=\"29.785894206549106\"><Measure DefaultId=\"DataItem0\" /></GridMeasureColumn><GridDimensionColumn Weight=\"90.050377833753117\"><Dimension DefaultId=\"DataItem1\" /></GridDimensionColumn><GridMeasureColumn Weight=\"83.816120906800975\"><Measure DefaultId=\"DataItem2\" /></GridMeasureColumn><GridMeasureColumn Weight=\"63.72795969773297\"><Measure DefaultId=\"DataItem3\" /></GridMeasureColumn><GridMeasureColumn Weight=\"78.967254408060413\"><Measure DefaultId=\"DataItem7\" /></GridMeasureColumn><GridMeasureColumn Weight=\"70.654911838790909\"><Measure DefaultId=\"DataItem4\" /></GridMeasureColumn><GridMeasureColumn Weight=\"95.591939546599463\"><Measure DefaultId=\"DataItem8\" /></GridMeasureColumn><GridMeasureColumn Weight=\"86.586901763224148\"><Measure DefaultId=\"DataItem9\" /></GridMeasureColumn><GridMeasureColumn Weight=\"60.9571788413098\"><Measure DefaultId=\"DataItem10\" /></GridMeasureColumn><GridMeasureColumn Weight=\"89.357682619647321\"><Measure DefaultId=\"DataItem6\" /></GridMeasureColumn><GridMeasureColumn Weight=\"75.503778337531443\"><Measure DefaultId=\"DataItem5\" /></GridMeasureColumn></GridColumns><GridOptions EnableBandedRows=\"true\" ColumnWidthMode=\"Manual\" /></Grid><RangeFilter ComponentName=\"rangeFilterDashboardItem1\" Name=\"Filter by Date\" ShowCaption=\"true\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Dimension DataMember=\"InputDate\" DateTimeGroupInterval=\"DayMonthYear\" DefaultId=\"DataItem0\" /><Measure DataMember=\"TotalPages\" DefaultId=\"DataItem1\" /></DataItems><Argument DefaultId=\"DataItem0\" /><Series><Simple SeriesType=\"Line\"><Value DefaultId=\"DataItem1\" /></Simple></Series></RangeFilter><Card ComponentName=\"cardDashboardItem3\" Name=\"Cards 3\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem4\" Name=\"Cards 4\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"ActiveHours\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem5\" Name=\"Cards 5\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem6\" Name=\"Cards 6\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem2\" Name=\"By Queue\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Dimension DataMember=\"Name\" DefaultId=\"DataItem0\" /><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem1\" /></DataItems><GridColumns><GridDimensionColumn><Dimension DefaultId=\"DataItem0\" /></GridDimensionColumn><GridMeasureColumn><Measure DefaultId=\"DataItem1\" /></GridMeasureColumn></GridColumns><GridOptions /></Grid></Items><LayoutTree><LayoutGroup Orientation=\"Vertical\" Weight=\"100\"><LayoutGroup Weight=\"16.867469879518072\"><LayoutItem DashboardItem=\"cardDashboardItem1\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem2\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem5\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"19.277108433734941\"><LayoutItem DashboardItem=\"cardDashboardItem4\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem6\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem3\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"29.036144578313252\"><LayoutItem DashboardItem=\"gridDashboardItem2\" Weight=\"19.761273209549071\" /><LayoutItem DashboardItem=\"gridDashboardItem1\" Weight=\"80.238726790450926\" /></LayoutGroup><LayoutItem DashboardItem=\"rangeFilterDashboardItem1\" Weight=\"34.819277108433738\" /></LayoutGroup></LayoutTree></Dashboard>",
                LastImportedDate = "2020-03-13T12:46:02.203",
                UseExtractedData = true,
                ExtractedDataDefinition = "<test />",
                DashboardGuid = Guid.Parse("2fe484cd-3e10-45a8-9c0c-1bcee4da4491"),
                UserName = "Trever_Gannon",
                FullUserName = "Irrelevant"
            });

            databaseMigrationWizardTestHelper.WriteEverythingToDirectory(ImportOptions.ImportPath);

            try
            {
                helper.Import();
            }
            catch (ExtractException)
            {
                throw;
            }
            finally
            {
                FamTestDbManager.RemoveDatabase(databaseName);
                Directory.Delete(ImportOptions.ImportPath, true);
            }
        }
    }
}