using DatabaseMigrationWizard.Database.Input;
using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using DatabaseMigrationWizard.Database.Output;
using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using Extract.SqlDatabase;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public class TestImportNewRecords
    {
        private static readonly string DropTempTables = @"declare @sql nvarchar(max)
                                                        select @sql = isnull(@sql+';', '') + 'drop table ' + quotename(name)
                                                        from tempdb..sysobjects
                                                        where name like '##%'
                                                        exec (@sql)";
        private static readonly FAMTestDBManager<TestExports> FamTestDbManager = new FAMTestDBManager<TestExports>();

        private static readonly string DatabaseName = "Test_ImportNewRecords";

        private static ImportOptions ImportOptions;

        private static DatabaseMigrationWizardTestHelper DatabaseMigrationWizardTestHelper;

        /// <summary>
        /// The testing methodology here is as follows
        /// 1. Run the import to populate initial values in the database.
        /// 2. Add a bunch of new records to the existing exported files
        /// 3. Rerun the import with those new values added
        /// 4. Ensure those new values are merged in.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            IFileProcessingDB dataBase = FamTestDbManager.GetNewDatabase(DatabaseName);

            ImportOptions = new ImportOptions()
            {
                ImportLabDETables = true,
                ImportPath = Path.GetTempPath() + $"{DatabaseName}\\",
                ConnectionInformation = new Database.ConnectionInformation() { DatabaseName = DatabaseName, DatabaseServer = "(local)" }
            };

            Directory.CreateDirectory(ImportOptions.ImportPath);
            DatabaseMigrationWizardTestHelper = new DatabaseMigrationWizardTestHelper();
            DatabaseMigrationWizardTestHelper.LoadInitialValues();
            DatabaseMigrationWizardTestHelper.WriteEverythingToDirectory(ImportOptions.ImportPath);
            using var importHelper = new ImportHelper(ImportOptions, new Progress<string>((garbage) => { }));
            importHelper.Import();
            importHelper.CommitTransaction();
            dataBase.ExecuteCommandQuery(DropTempTables);

            AddNewRecords();
            DatabaseMigrationWizardTestHelper.WriteEverythingToDirectory(ImportOptions.ImportPath);
            using var importHelper1 = new ImportHelper(ImportOptions, new Progress<string>((garbage) => { }));
            importHelper1.Import();
            importHelper1.CommitTransaction();
        }

        /// <summary>
        /// TearDown method to destroy testing environment.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Nunit made me")]
        [OneTimeTearDown]
        public static void TearDown()
        {
            FamTestDbManager.RemoveDatabase(DatabaseName);
            Directory.Delete(ImportOptions.ImportPath, true);
        }

        /// <summary>
        /// Tests to make sure Action imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void Action()
        {
            var actionFromDB = JsonConvert.DeserializeObject<List<Database.Input.DataTransformObject.Action>>(BuildAndWriteTable(new SerializeAction()).ToString());

            foreach (var action in DatabaseMigrationWizardTestHelper.Actions)
            {
                Assert.IsTrue(actionFromDB.Where(m => m.Equals(action)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure AttributeName imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeSetName()
        {
            var attributeSetNameFromDB = JsonConvert.DeserializeObject<List<AttributeSetName>>(BuildAndWriteTable(new SerializeAttributeSetName()).ToString());

            foreach (var attributeSetName in DatabaseMigrationWizardTestHelper.AttributeSetNames)
            {
                Assert.IsTrue(attributeSetNameFromDB.Where(m => m.Equals(attributeSetName)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure Dashboard imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void Dashboard()
        {
            var dashboardFromDB = JsonConvert.DeserializeObject<List<Dashboard>>(BuildAndWriteTable(new SerializeDashboard()).ToString());

            foreach (var dashboard in DatabaseMigrationWizardTestHelper.Dashboards)
            {
                Assert.IsTrue(dashboardFromDB.Where(m => m.Equals(dashboard)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure DatabaseService imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void DatabaseService()
        {
            var databaseServiceFromDB = JsonConvert.DeserializeObject<List<DatabaseService>>(BuildAndWriteTable(new SerializeDatabaseService()).ToString());

            foreach (var databaseService in DatabaseMigrationWizardTestHelper.DatabaseServices)
            {
                Assert.IsTrue(databaseServiceFromDB.Where(m => m.Equals(databaseService)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure DataEntryCounterDefinition imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void DataEntryCounterDefinition()
        {
            var DataEntryCounterDefinitionFromDB = JsonConvert.DeserializeObject<List<DataEntryCounterDefinition>>(BuildAndWriteTable(new SerializeDataEntryCounterDefinition()).ToString());

            foreach (var DataEntryCounterDefinition in DatabaseMigrationWizardTestHelper.DataEntryCounterDefinitions)
            {
                Assert.IsTrue(DataEntryCounterDefinitionFromDB.Where(m => m.Equals(DataEntryCounterDefinition)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure DBInfo imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void DBInfo()
        {
            var DBInfoFromDB = JsonConvert.DeserializeObject<List<DBInfo>>(BuildAndWriteTable(new SerializeDBInfo()).ToString());
            DatabaseMigrationWizardTestHelper.CompareDBInfo(DBInfoFromDB);
        }

        /// <summary>
        /// Tests to make sure FAMUser imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void FAMUser()
        {
            var FAMUserFromDB = JsonConvert.DeserializeObject<List<FAMUser>>(BuildAndWriteTable(new SerializeFAMUser()).ToString());

            foreach (var FAMUser in DatabaseMigrationWizardTestHelper.FAMUsers)
            {
                Assert.IsTrue(FAMUserFromDB.Where(m => m.Equals(FAMUser)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure FieldSearch imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void FieldSearch()
        {
            var FieldSearchFromDB = JsonConvert.DeserializeObject<List<FieldSearch>>(BuildAndWriteTable(new SerializeFieldSearch()).ToString());

            foreach (var FieldSearch in DatabaseMigrationWizardTestHelper.FieldSearches)
            {
                Assert.IsTrue(FieldSearchFromDB.Where(m => m.Equals(FieldSearch)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure FileHandler imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void FileHandler()
        {
            var FileHandlerFromDB = JsonConvert.DeserializeObject<List<FileHandler>>(BuildAndWriteTable(new SerializeFileHandler()).ToString());

            foreach (var FileHandler in DatabaseMigrationWizardTestHelper.FileHandlers)
            {
                Assert.IsTrue(FileHandlerFromDB.Where(m => m.Equals(FileHandler)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure LabDEEncounter imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEEncounter()
        {
            var LabDEEncounterFromDB = JsonConvert.DeserializeObject<List<LabDEEncounter>>(BuildAndWriteTable(new SerializeLabDEEncounter()).ToString());

            foreach (var LabDEEncounter in DatabaseMigrationWizardTestHelper.LabDEEncounters)
            {
                Assert.IsTrue(LabDEEncounterFromDB.Where(m => m.Equals(LabDEEncounter)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure LabDEOrder imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEOrder()
        {
            var LabDEOrderFromDB = JsonConvert.DeserializeObject<List<LabDEOrder>>(BuildAndWriteTable(new SerializeLabDEOrder()).ToString());

            foreach (var LabDEOrder in DatabaseMigrationWizardTestHelper.LabDEOrders)
            {
                Assert.IsTrue(LabDEOrderFromDB.Where(m => m.Equals(LabDEOrder)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure LabDEPatient imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEPatient()
        {
            var LabDEPatientFromDB = JsonConvert.DeserializeObject<List<LabDEPatient>>(BuildAndWriteTable(new SerializeLabDEPatient()).ToString());

            foreach (var LabDEPatient in DatabaseMigrationWizardTestHelper.LabDEPatients)
            {
                Assert.IsTrue(LabDEPatientFromDB.Where(m => m.Equals(LabDEPatient)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure LabDEProvider imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEProvider()
        {
            var LabDEProviderFromDB = JsonConvert.DeserializeObject<List<LabDEProvider>>(BuildAndWriteTable(new SerializeLabDEProvider()).ToString());

            foreach (var LabDEProvider in DatabaseMigrationWizardTestHelper.LabDEProviders)
            {
                Assert.IsTrue(LabDEProviderFromDB.Where(m => m.Equals(LabDEProvider)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure Login imported properly.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Naming violations are a result of acronyms in the database.")]
        [Test, Category("Automated")]
        public static void Login()
        {
            var LoginFromDB = JsonConvert.DeserializeObject<List<Login>>(BuildAndWriteTable(new SerializeLogin()).ToString());

            foreach (var Login in DatabaseMigrationWizardTestHelper.Logins.Where(m => !m.UserName.Equals("admin")))
            {
                Assert.IsTrue(LoginFromDB.Where(m => m.Equals(Login)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure MetadataField imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void MetadataField()
        {
            var MetadataFieldFromDB = JsonConvert.DeserializeObject<List<MetadataField>>(BuildAndWriteTable(new SerializeMetadataField()).ToString());

            foreach (var MetadataField in DatabaseMigrationWizardTestHelper.MetadataFields)
            {
                Assert.IsTrue(MetadataFieldFromDB.Where(m => m.Equals(MetadataField)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure MlModel imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void MLModel()
        {
            var MLModelFromDB = JsonConvert.DeserializeObject<List<MLModel>>(BuildAndWriteTable(new SerializeMLModel()).ToString());

            foreach (var MLModel in DatabaseMigrationWizardTestHelper.MLModels)
            {
                Assert.IsTrue(MLModelFromDB.Where(m => m.Equals(MLModel)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure Tag imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void Tag()
        {
            var TagFromDB = JsonConvert.DeserializeObject<List<Tag>>(BuildAndWriteTable(new SerializeTag()).ToString());

            foreach (var Tag in DatabaseMigrationWizardTestHelper.Tags)
            {
                Assert.IsTrue(TagFromDB.Where(m => m.Equals(Tag)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure UserCreatedCounter imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void UserCreatedCounter()
        {
            var UserCreatedCounterFromDB = JsonConvert.DeserializeObject<List<UserCreatedCounter>>(BuildAndWriteTable(new SerializeUserCreatedCounter()).ToString());

            foreach (var UserCreatedCounter in DatabaseMigrationWizardTestHelper.UserCreatedCounters)
            {
                Assert.IsTrue(UserCreatedCounterFromDB.Where(m => m.Equals(UserCreatedCounter)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure WebAppConfig imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void WebAppConfig()
        {
            var WebAppConfigFromDB = JsonConvert.DeserializeObject<List<WebAppConfig>>(BuildAndWriteTable(new SerializeWebAppConfig()).ToString());

            foreach (var WebAppConfig in DatabaseMigrationWizardTestHelper.WebAppConfigurations)
            {
                Assert.IsTrue(WebAppConfigFromDB.Where(m => m.Equals(WebAppConfig)).Any());
            }
        }

        /// <summary>
        /// Tests to make sure Workflow imported properly.
        /// </summary>
        [Test, Category("Automated")]
        public static void Workflow()
        {
            var WorkflowFromDB = JsonConvert.DeserializeObject<List<Workflow>>(BuildAndWriteTable(new SerializeWorkflow()).ToString());

            foreach (var Workflow in DatabaseMigrationWizardTestHelper.Workflows)
            {
                Assert.IsTrue(WorkflowFromDB.Where(m => m.Equals(Workflow)).Any());
            }
        }

        private static StringWriter BuildAndWriteTable(ISerialize serialize)
        {
            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);

            using var sqlConnection = new ExtractRoleConnection("(local)", DatabaseName);
            sqlConnection.Open();
            
            serialize.SerializeTable(sqlConnection, stringWriter);

            return stringWriter;
        }

        /// <summary>
        /// Adds a new record to every table except for DBInfo, because thats hard coded and you will get an error if you add to that table.
        /// </summary>
        private static void AddNewRecords()
        {
            DatabaseMigrationWizardTestHelper.Actions.Add(new Database.Input.DataTransformObject.Action() { ASCName = "NewAction", MainSequence = false, ActionGuid = Guid.Parse("3d4c06ac-2b37-41de-ba6d-4ad04f7d98d2"), WorkflowGuid = Guid.Parse("0e7193e0-7416-47b3-b5fe-24e26fdf6520") });

            DatabaseMigrationWizardTestHelper.AttributeSetNames.Add(new AttributeSetName() { Description = "TurtleAttributeSet", Guid = Guid.Parse("011ef56c-0cc4-4050-a10e-002f2b2177ce") });

            DatabaseMigrationWizardTestHelper.Dashboards.Add(new Dashboard()
            {
                DashboardName = "NewDashboard",
                Definition = "<Dashboard CurrencyCulture=\"en-US\"><Title Text=\"Dashboard\" /><DataSources><SqlDataSource Name=\"SQL Data Source 1\" ComponentName=\"dashboardSqlDataSource1\" DataProcessingMode=\"Client\"><Connection Name=\"localhost_Essentia_Customer_Builds_Connection\" ProviderKey=\"MSSqlServer\"><Parameters><Parameter Name=\"server\" Value=\"zeus\" /><Parameter Name=\"database\" Value=\"Essentia_Customer_Builds\" /><Parameter Name=\"useIntegratedSecurity\" Value=\"True\" /><Parameter Name=\"read only\" Value=\"1\" /><Parameter Name=\"generateConnectionHelper\" Value=\"false\" /><Parameter Name=\"userid\" Value=\"\" /><Parameter Name=\"password\" Value=\"\" /></Parameters></Connection><Query Type=\"CustomSqlQuery\" Name=\"Query\"><Parameter Name=\"ReportingPeroid\" Type=\"DevExpress.DataAccess.Expression\">(System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)(GetDate(Iif(?ReportingPeroid = '2 Weeks', AddDays(Today(), -14), ?ReportingPeroid = '1 Month', AddMonths(Today(), -1), ?ReportingPeroid = '2 Months', AddMonths(Today(), -3), ?ReportingPeroid = '6 Months', AddMonths(Today(), -6), ?ReportingPeroid = '1 Year', AddYears(Today(), -1), ?ReportingPeroid = 'Since Go Live (9/25/2018)', GetDate('9/25/2018 12:00AM'), AddDays(Today(), -14)))\r\n)</Parameter><Sql>--DECLARE @REPORTINGPEROID DATE;\r\n--SET @REPORTINGPEROID = '1/20/2016';\r\n\r\nSELECT\r\n\tCTE.FullUserName\r\n\t, COUNT( DISTINCT OriginalFileID) AS InputDocumentCount\r\n\t, COUNT( DISTINCT CTE.DestFileID) AS OutputDocumentCount\r\n\t, CTE.InputDate\r\n\t, MAX(CurrentlyActive) AS CurrentlyActive\r\n\t, SUM( TotalMinutes) AS ActiveMinutes\r\n\t, CTE.Name\r\n\t, SUM( Pages) AS TotalPages\r\n\t, SUM(Correct) AS CorrectSum\r\n\t, SUM(Expected) AS ExpectedSum\r\n\t, SUM(Expected) - SUM(Correct) AS IncorrectPlusMissed\r\nFROM\r\n\t( -- This is necessary for getting the total minutes and pages. These values should only be applied once per doc, hence the rownum\r\n\tSELECT DISTINCT\r\n\t\tdbo.FAMUser.FullUserName\r\n\t\t, ReportingHIMStats.OriginalFileID\r\n\t\t, ReportingHIMStats.DestFileID\r\n\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t, vUsersWithActive.CurrentlyActive\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY TotalMinutes ORDER BY TotalMinutes ) = 1 THEN TotalMinutes\r\n\t\t\tELSE 0\r\n\t\t  END AS TotalMinutes\r\n\t\t, Workflow.Name\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY dbo.FAMFile.Pages, dbo.FAMFile.ID ORDER BY dbo.FAMFile.Pages desc ) = 1 THEN dbo.FAMFile.Pages\r\n\t\t\tELSE 0\r\n\t\t  END AS Pages\r\n\r\n\tFROM\r\n\t\tdbo.FAMUser\r\n\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.FAMFile\r\n\t\t\t\t\tON dbo.FAMFile.ID = ReportingHIMStats.OriginalFileID\r\n\r\n\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\tINNER JOIN dbo.vUsersWithActive \r\n\t\t\t\t\t\tON vUsersWithActive.FAMUserID = vFAMUserInputEventsTime.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\tWHERE\r\n\t\tFAMUser.FullUserName IS NOT NULL\r\n\t) AS CTE\r\n\t\tLEFT OUTER JOIN ( --This subquery is necessary for reporing on the ReportingDataCaptureAccuracy. They require a different grouping than pages/active hours\r\n\t\t\t\t\t\t\tSELECT DISTINCT\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Correct) AS Correct\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Expected) AS Expected\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\tdbo.FAMUser\r\n\t\t\t\t\t\t\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\t\t\t\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\t\t\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\t\t\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\t\t\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\t\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\t\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\t\t\t\t\t\t\t\t\t\tLEFT OUTER JOIN dbo.ReportingDataCaptureAccuracy\r\n\t\t\t\t\t\t\t\t\t\t\tON ReportingDataCaptureAccuracy.FileID = ReportingHIMStats.DestFileID\r\n\r\n\t\t\t\t\t\t\tWHERE\r\n\t\t\t\t\t\t\t\tFAMUser.FullUserName IS NOT NULL\r\n\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\t\t\t\t\t\t\t) AS CTE2\r\n\t\t\t\t\t\t\t\tON CTE2.DestFileID = CTE.DestFileID\r\n\t\t\t\t\t\t\t\tAND CTE2.FullUserName = CTE.FullUserName\r\n\t\t\t\t\t\t\t\tAND CTE.InputDate = CTE2.InputDate\r\n\t\t\t\t\t\t\t\tAND CTE.Name = CTE2.Name\r\nGROUP BY\r\n\tCTE.FullUserName\r\n\t, CTE.InputDate\r\n\t, CTE.Name\r\n</Sql></Query><ResultSchema><DataSet Name=\"SQL Data Source 1\"><View Name=\"Query\"><Field Name=\"FullUserName\" Type=\"String\" /><Field Name=\"InputDocumentCount\" Type=\"Int32\" /><Field Name=\"OutputDocumentCount\" Type=\"Int32\" /><Field Name=\"InputDate\" Type=\"DateTime\" /><Field Name=\"CurrentlyActive\" Type=\"Int32\" /><Field Name=\"ActiveMinutes\" Type=\"Double\" /><Field Name=\"Name\" Type=\"String\" /><Field Name=\"TotalPages\" Type=\"Int32\" /><Field Name=\"CorrectSum\" Type=\"Int64\" /><Field Name=\"ExpectedSum\" Type=\"Int64\" /><Field Name=\"IncorrectPlusMissed\" Type=\"Int64\" /></View></DataSet></ResultSchema><ConnectionOptions CloseConnection=\"true\" DbCommandTimeout=\"0\" /><CalculatedFields><CalculatedField Name=\"Pages/Hour\" Expression=\"Sum([TotalPages]) / (SUM([ActiveMinutes]) / 60)\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"ActiveHours\" Expression=\"SUM([ActiveMinutes]) / 60\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"Documents/Hour\" Expression=\"SUM([OutputDocumentCount]) / [ActiveHours]\" DataType=\"Auto\" DataMember=\"Query\" /></CalculatedFields></SqlDataSource></DataSources><Parameters><Parameter Name=\"ReportingPeroid\" Value=\"2 Weeks\"><StaticListLookUpSettings><Values><Value>2 Weeks</Value><Value>1 Month</Value><Value>2 Months</Value><Value>6 Months</Value><Value>1 Year</Value><Value>Since Go Live (9/25/2018)</Value></Values></StaticListLookUpSettings></Parameter></Parameters><Items><Card ComponentName=\"cardDashboardItem1\" Name=\"Cards 1\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"InputDocumentCount\" Name=\"Input Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem2\" Name=\"Cards 2\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem1\" Name=\"User Productivity\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Measure DataMember=\"CurrentlyActive\" Name=\"Active\" DefaultId=\"DataItem0\" /><Dimension DataMember=\"FullUserName\" DefaultId=\"DataItem1\" /><Measure DataMember=\"InputDocumentCount\" Name=\"Input Docs\" DefaultId=\"DataItem2\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Docs\" DefaultId=\"DataItem3\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ActiveMinutes\" Name=\"Active Minutes\" DefaultId=\"DataItem4\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem5\" /><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem6\" /><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem7\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ExpectedSum\" Name=\"Indexed Fields\" DefaultId=\"DataItem8\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"CorrectSum\" Name=\"Auto-Populated\" DefaultId=\"DataItem9\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"IncorrectPlusMissed\" DefaultId=\"DataItem10\" /></DataItems><FormatRules><GridItemFormatRule Name=\"FormatRule 1\" DataItem=\"DataItem0\"><FormatConditionRangeSet ValueType=\"Number\"><RangeSet><Ranges><RangeInfo><Value Type=\"System.Decimal\" Value=\"0\" /><IconSettings IconType=\"ShapeRedCircle\" /></RangeInfo><RangeInfo><Value Type=\"System.Int32\" Value=\"1\" /><IconSettings IconType=\"ShapeGreenCircle\" /></RangeInfo></Ranges></RangeSet></FormatConditionRangeSet></GridItemFormatRule></FormatRules><GridColumns><GridMeasureColumn Weight=\"29.785894206549106\"><Measure DefaultId=\"DataItem0\" /></GridMeasureColumn><GridDimensionColumn Weight=\"90.050377833753117\"><Dimension DefaultId=\"DataItem1\" /></GridDimensionColumn><GridMeasureColumn Weight=\"83.816120906800975\"><Measure DefaultId=\"DataItem2\" /></GridMeasureColumn><GridMeasureColumn Weight=\"63.72795969773297\"><Measure DefaultId=\"DataItem3\" /></GridMeasureColumn><GridMeasureColumn Weight=\"78.967254408060413\"><Measure DefaultId=\"DataItem7\" /></GridMeasureColumn><GridMeasureColumn Weight=\"70.654911838790909\"><Measure DefaultId=\"DataItem4\" /></GridMeasureColumn><GridMeasureColumn Weight=\"95.591939546599463\"><Measure DefaultId=\"DataItem8\" /></GridMeasureColumn><GridMeasureColumn Weight=\"86.586901763224148\"><Measure DefaultId=\"DataItem9\" /></GridMeasureColumn><GridMeasureColumn Weight=\"60.9571788413098\"><Measure DefaultId=\"DataItem10\" /></GridMeasureColumn><GridMeasureColumn Weight=\"89.357682619647321\"><Measure DefaultId=\"DataItem6\" /></GridMeasureColumn><GridMeasureColumn Weight=\"75.503778337531443\"><Measure DefaultId=\"DataItem5\" /></GridMeasureColumn></GridColumns><GridOptions EnableBandedRows=\"true\" ColumnWidthMode=\"Manual\" /></Grid><RangeFilter ComponentName=\"rangeFilterDashboardItem1\" Name=\"Filter by Date\" ShowCaption=\"true\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Dimension DataMember=\"InputDate\" DateTimeGroupInterval=\"DayMonthYear\" DefaultId=\"DataItem0\" /><Measure DataMember=\"TotalPages\" DefaultId=\"DataItem1\" /></DataItems><Argument DefaultId=\"DataItem0\" /><Series><Simple SeriesType=\"Line\"><Value DefaultId=\"DataItem1\" /></Simple></Series></RangeFilter><Card ComponentName=\"cardDashboardItem3\" Name=\"Cards 3\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem4\" Name=\"Cards 4\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"ActiveHours\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem5\" Name=\"Cards 5\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem6\" Name=\"Cards 6\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem2\" Name=\"By Queue\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Dimension DataMember=\"Name\" DefaultId=\"DataItem0\" /><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem1\" /></DataItems><GridColumns><GridDimensionColumn><Dimension DefaultId=\"DataItem0\" /></GridDimensionColumn><GridMeasureColumn><Measure DefaultId=\"DataItem1\" /></GridMeasureColumn></GridColumns><GridOptions /></Grid></Items><LayoutTree><LayoutGroup Orientation=\"Vertical\" Weight=\"100\"><LayoutGroup Weight=\"16.867469879518072\"><LayoutItem DashboardItem=\"cardDashboardItem1\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem2\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem5\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"19.277108433734941\"><LayoutItem DashboardItem=\"cardDashboardItem4\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem6\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem3\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"29.036144578313252\"><LayoutItem DashboardItem=\"gridDashboardItem2\" Weight=\"19.761273209549071\" /><LayoutItem DashboardItem=\"gridDashboardItem1\" Weight=\"80.238726790450926\" /></LayoutGroup><LayoutItem DashboardItem=\"rangeFilterDashboardItem1\" Weight=\"34.819277108433738\" /></LayoutGroup></LayoutTree></Dashboard>",
                LastImportedDate = "2020-05-13T12:46:02.203",
                UseExtractedData = true,
                ExtractedDataDefinition = null,
                DashboardGuid = Guid.Parse("05e7d279-49c2-467c-975c-dc8e96db48dc"),
                FullUserName = "Legend",
                UserName = "SuperVerifier"
            });

            DatabaseMigrationWizardTestHelper.DatabaseServices.Add(new DatabaseService()
            {
                Description = "newService",
                Settings = "{\r\n  \"$type\": \"Extract.Dashboard.ETL.DashboardExtractedDataService, Extract.Dashboard.ETL\",\r\n  \"Version\": 1,\r\n  \"Description\": \"TestService\",\r\n  \"Schedule\": {\r\n    \"$type\": \"Extract.Utilities.ScheduledEvent, Extract.Utilities\",\r\n    \"Exclusions\": [],\r\n    \"Version\": 1,\r\n    \"Start\": \"2020-03-12T15:56:59\",\r\n    \"End\": \"2020-03-12T15:56:59\",\r\n    \"RecurrenceUnit\": 2,\r\n    \"Duration\": null\r\n  }\r\n}",
                Enabled = true,
                Guid = Guid.Parse("845bf536-c741-404e-8136-952074acac74")
            });

            DatabaseMigrationWizardTestHelper.DataEntryCounterDefinitions.Add(new DataEntryCounterDefinition()
            {
                Name = "NewDataEntry",
                AttributeQuery = "SELECT WIN",
                RecordOnLoad = true,
                RecordOnSave = true,
                Guid = Guid.Parse("0d4bf118-65a5-40a5-bc2d-c45ba65d6166")
            });

            DatabaseMigrationWizardTestHelper.FAMUsers.Add(new FAMUser() { UserName = "SuperVerifier", FullUserName = "Legend" });

            DatabaseMigrationWizardTestHelper.FieldSearches.Add(new FieldSearch() { Enabled = true, FieldName = "FunestField", AttributeQuery = "SELECT", Guid = Guid.Parse("9daf1bd1-abd0-4fed-9db0-37dbd5fc510a") });

            DatabaseMigrationWizardTestHelper.FileHandlers.Add(new FileHandler()
            {
                Enabled = true,
                AppName = "SuperFunApp",
                IconPath = "C:\\Wut",
                ApplicationPath = "C:\\Why",
                Arguments = "Turtles",
                AdminOnly = true,
                AllowMultipleFiles = true,
                SupportsErrorHandling = true,
                Blocking = true,
                WorkflowName = "Test Workflow",
                Guid = Guid.Parse("5b5ba4df-1ecb-4f9c-8172-6a56c8172951")
            });

            DatabaseMigrationWizardTestHelper.LabDEEncounters.Add(new LabDEEncounter()
            {
                CSN = "3000006096",
                PatientMRN = "30000001",
                EncounterDateTime = "2015-01-06T00:00:00",
                Department = "LK FP",
                EncounterType = "ANCILLARY",
                EncounterProvider = "LAKESIDE ANCILLARY",
                DischargeDate = "2013-01-06T00:00:00",
                AdmissionDate = "2014-01-06T00:00:00",
                ADTMessage = "<ADT_A01><MSH><MSH.1>|</MSH.1><MSH.2>^~\\&amp;</MSH.2><MSH.3><HD.1>EPIC</HD.1></MSH.3><MSH.5><HD.1>EXTRACT</HD.1></MSH.5><MSH.7><TS.1>20190529</TS.1></MSH.7><MSH.8>23221</MSH.8><MSH.9><MSG.1>ADT</MSG.1><MSG.2>A08</MSG.2></MSH.9><MSH.10>396019498</MSH.10><MSH.11><PT.1>P</PT.1></MSH.11></MSH><PV1><PV1.3><PL.1>LK FP</PL.1><PL.4><HD.1>LK</HD.1></PL.4></PV1.3><PV1.7><XCN.1>740044</XCN.1><XCN.2><FN.1>ANCILLARY</FN.1></XCN.2><XCN.3>LAKESIDE</XCN.3></PV1.7><PV1.19><CX.1>1000006096</CX.1></PV1.19><PV1.44><TS.1>20120106</TS.1></PV1.44></PV1></ADT_A01>",
                Guid = Guid.Parse("f66ebe4b-265d-4f55-a003-7347aa3d6a27")
            });

            DatabaseMigrationWizardTestHelper.LabDEOrders.Add(new LabDEOrder()
            {
                OrderNumber = "300014135",
                OrderCode = "90686",
                PatientMRN = "30000001",
                ReceivedDateTime = "2018-10-02T15:22:00.53",
                OrderStatus = "A",
                ReferenceDateTime = "2018-10-02T00:00:00",
                ORMMessage = "<ORM_O01><MSH><MSH.1>|</MSH.1><MSH.2>^~\\&amp;</MSH.2><MSH.3><HD.1>EPIC</HD.1></MSH.3><MSH.5><HD.1>EXTRACT</HD.1></MSH.5><MSH.7><TS.1>20181002152150</TS.1></MSH.7><MSH.9><MSG.1>ORM</MSG.1><MSG.2>O01</MSG.2></MSH.9><MSH.10>7124912</MSH.10><MSH.12><VID.1>2.3</VID.1></MSH.12></MSH><ORM_O01.PATIENT><ORM_O01.PATIENT_VISIT><PV1><PV1.3><PL.1>WAC WI</PL.1><PL.4><HD.1>WACL</HD.1></PL.4></PV1.3><PV1.7><XCN.1>741102</XCN.1><XCN.2><FN.1>ANCILLARY</FN.1></XCN.2><XCN.3>WACL</XCN.3></PV1.7><PV1.19><CX.1>1173770646</CX.1></PV1.19><PV1.44><TS.1>20181002145829</TS.1></PV1.44></PV1></ORM_O01.PATIENT_VISIT></ORM_O01.PATIENT><ORM_O01.ORDER><ORC><ORC.1>NW</ORC.1><ORC.2><EI.1>100014135</EI.1></ORC.2><ORC.9><TS.1>20181002152148</TS.1></ORC.9><ORC.10><XCN.1>80836</XCN.1><XCN.2><FN.1>STICKLER</FN.1></XCN.2><XCN.3>KATHRYN</XCN.3></ORC.10><ORC.12><XCN.1>30069</XCN.1><XCN.2><FN.1>LUNDE</FN.1></XCN.2><XCN.3>LARA</XCN.3><XCN.4>N</XCN.4></ORC.12><ORC.13><PL.1>1030100140037</PL.1><PL.4><HD.1>103010014</HD.1></PL.4></ORC.13></ORC><ORM_O01.ORDER_DETAIL><ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP><OBR><OBR.1>1</OBR.1><OBR.2><EI.1>100014135</EI.1></OBR.2><OBR.4><CE.1>90686</CE.1><CE.2>FLU VACCINE,QUAD, NO PRESERV,IM  .5 ML</CE.2></OBR.4><OBR.6><TS.1>20181002</TS.1></OBR.6><OBR.16><XCN.1>30069</XCN.1><XCN.2><FN.1>LUNDE</FN.1></XCN.2><XCN.3>LARA</XCN.3><XCN.4>N</XCN.4></OBR.16></OBR></ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP></ORM_O01.ORDER_DETAIL></ORM_O01.ORDER></ORM_O01>",
                EncounterID = "3000006096",
                AccessionNumber = "6666",
                Guid = Guid.Parse("4c75f464-7abb-47c9-b86d-b60f7ca7e691")
            });

            DatabaseMigrationWizardTestHelper.LabDEPatients.Add(new LabDEPatient()
            {
                MRN = "30000001",
                FirstName = "John",
                MiddleName = "P",
                LastName = "Doe",
                Suffix = "JR",
                DOB = "1993-04-08T00:00:00",
                Gender = "F",
                MergedInto = null,
                CurrentMRN = "30000001",
                Guid = Guid.Parse("2b0cb535-8e44-4aa1-8bd0-28561fa3624a")
            });

            DatabaseMigrationWizardTestHelper.LabDEProviders.Add(new LabDEProvider()
            {
                ID = "300",
                FirstName = "Mike",
                MiddleName = "T M",
                LastName = "BamFord",
                ProviderType = "PHYSICIAN",
                Title = "MDS",
                Degree = "MD",
                Departments = "DC DER;AS DER;IF DER;HI DER",
                Specialties = "DERM",
                Phone = "(218)786-3434",
                Fax = "(218)786-3066",
                Address = "400 EAST THIRD STREET, DULUTH, MN, 55805",
                OtherProviderID = "1326081662",
                Inactive = true,
                MFNMessage = "<MFN_M02><MSH><MSH.1>|</MSH.1><MSH.2>^~\\&amp;</MSH.2><MSH.3><HD.1 /><HD.2 /><HD.3 /></MSH.3><MSH.4><HD.1>EPIC</HD.1><HD.2 /><HD.3 /></MSH.4><MSH.5><HD.1 /><HD.2 /><HD.3 /></MSH.5><MSH.6><HD.1 /><HD.2 /><HD.3 /></MSH.6><MSH.7><TS.1>20190305170108</TS.1><TS.2 /></MSH.7><MSH.8>872170</MSH.8><MSH.9><MSG.1>MFN</MSG.1><MSG.2>M02</MSG.2><MSG.3 /></MSH.9><MSH.10>776567</MSH.10><MSH.11><PT.1>P</PT.1><PT.2 /></MSH.11></MSH><MFI><MFI.1><CE.1>SER</CE.1></MFI.1><MFI.3>UPD</MFI.3><MFI.6>NE</MFI.6></MFI><MFN_M02.MF_STAFF><MFE><MFE.1>MDL</MFE.1><MFE.4>100</MFE.4></MFE><STF><STF.1><CE.1>100</CE.1><CE.2 /><CE.3>DENTP</CE.3></STF.1><STF.3><XPN.1><FN.1>BAMFORD</FN.1></XPN.1><XPN.2>JOEL</XPN.2><XPN.3>T M</XPN.3><XPN.5>MD</XPN.5><XPN.6>MD</XPN.6></STF.3><STF.4>PERSON</STF.4><STF.5>M</STF.5><STF.6><TS.1 /></STF.6><STF.7>I</STF.7><STF.8><CE.1>DC DER</CE.1></STF.8><STF.10><XTN.1>(218)786-3434</XTN.1><XTN.4 /></STF.10><STF.11><XAD.1><SAD.1>400 EAST THIRD STREET</SAD.1></XAD.1><XAD.3>DULUTH</XAD.3><XAD.4>MN</XAD.4><XAD.5>55805</XAD.5></STF.11></STF><PRA><PRA.1><CE.1>100</CE.1><CE.2 /><CE.3>DENTP</CE.3></PRA.1><PRA.2><CE.1 /></PRA.2><PRA.3>PHYSICIAN</PRA.3><PRA.4>Y</PRA.4><PRA.5><SPD.1>Derm</SPD.1></PRA.5><PRA.6><PLN.1>D80218</PLN.1><PLN.2 /><PLN.3 /><PLN.4 /></PRA.6><PRA.8 /><PRA.9><CE.1 /><CE.2 /><CE.3 /></PRA.9><PRA.10 /></PRA></MFN_M02.MF_STAFF></MFN_M02>",
                Guid = Guid.Parse("88400184-5190-4a66-b6d7-fb22b04e2077")
            });

            DatabaseMigrationWizardTestHelper.Logins.Add(new Login() { UserName = "notAdmin", Password = "e086da2321be72f0525b25d5d5b0c6d7", Guid = Guid.Parse("d9ca9ee6-ae9b-496b-8c48-8db752fe6940") });

            DatabaseMigrationWizardTestHelper.MetadataFields.Add(new MetadataField() { Name = "NewMetadataField", Guid = Guid.Parse("cb557f82-6a09-40b6-96e5-f2f9e232cff9") });

            DatabaseMigrationWizardTestHelper.MLModels.Add(new MLModel() { Name = "53", Guid = Guid.Parse("bc25ce05-bf79-449d-bbdd-0e9fc6d32356") });

            DatabaseMigrationWizardTestHelper.Tags.Add(new Tag() { TagName = "NewTag", TagDescription = "NewTestTagDescription", Guid = Guid.Parse("f9e9a0a8-3af8-499f-982d-28e332942ac7") });

            DatabaseMigrationWizardTestHelper.UserCreatedCounters.Add(new UserCreatedCounter() { CounterName = "NewTestCounter", Value = "1337", Guid = Guid.Parse("bbfef742-86ad-400d-9b20-360714b719e9") });

            DatabaseMigrationWizardTestHelper.WebAppConfigurations.Add(new WebAppConfig()
            {
                Type = "NewSetting",
                Settings = "{\"DocumentTypes\":\"C:\\\\TestAvail\",\"InactivityTimeout\":5,\"RedactionTypes\":[\"Test\"]}",
                WebAppConfigGuid = Guid.Parse("fa6709a9-734d-4a5a-97c1-250e951151ce"),
                WorkflowGuid = Guid.Parse("0e7193e0-7416-47b3-b5fe-24e26fdf6520")
            });

            DatabaseMigrationWizardTestHelper.Workflows.Add(new Workflow()
            {
                WorkflowGuid = Guid.Parse("087dc45e-72de-43c1-a2de-4e9e676bac14"),
                Name = "New Workflow",
                WorkflowTypeCode = "R",
                Description = "Test Description Workflow",
                DocumentFolder = "C:\\TestFolderWorkflow",
                OutputFilePathInitializationFunction = "c:\\TestFun",
                LoadBalanceWeight = 2,
                MetadataFieldGuid = Guid.Parse("1cffbe35-a3b8-4ded-b45d-73109085760b"),
                EditActionGuid = Guid.Parse("04f0e473-5714-4687-a147-8b7fb6f5335e"),
                EndActionGuid = Guid.Parse("04f0e473-5714-4687-a147-8b7fb6f5335e"),
                PostEditActionGuid = Guid.Parse("cd27650d-dfe4-44c3-9fc8-96ea99f7a4e2"),
                PostWorkflowActionGuid = Guid.Parse("cd27650d-dfe4-44c3-9fc8-96ea99f7a4e2"),
                StartActionGuid = Guid.Parse("04f0e473-5714-4687-a147-8b7fb6f5335e"),
                AttributeSetNameGuid = Guid.Parse("7c081610-2f63-4f0c-9a3b-d018176bd5ea"),
            });
        }
    }
}