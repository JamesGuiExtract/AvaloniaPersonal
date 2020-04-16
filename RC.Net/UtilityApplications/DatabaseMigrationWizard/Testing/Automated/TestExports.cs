using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using DatabaseMigrationWizard.Database.Output;
using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
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
    [Category("DatabaseMigrationWizardExports")]
    public class TestExports
    {
        private static readonly string DisableAllForigenKeysInDatabase =
            @"
                DECLARE @sql NVARCHAR(MAX) = N'';

                ;WITH x AS 
                (
                  SELECT DISTINCT obj = 
                      QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' 
                    + QUOTENAME(OBJECT_NAME(parent_object_id)) 
                  FROM sys.foreign_keys
                )
                SELECT @sql += N'ALTER TABLE ' + obj + ' NOCHECK CONSTRAINT ALL;
                ' FROM x;

                EXEC sp_executesql @sql;
                ";

        private static readonly FAMTestDBManager<TestExports> FamTestDbManager = new FAMTestDBManager<TestExports>();

        private static readonly string DatabaseName = "TestExport";

        // In general I tried to keep these inserts in the order of the unit tests.
        private static readonly string[] BuildDummyDatabase =
                {
                @"INSERT INTO [dbo].[FileHandler] (Enabled, AppName, IconPath, ApplicationPath, Arguments, AdminOnly, AllowMultipleFiles, SupportsErrorHandling, Blocking, WorkflowName) 
                                            VALUES(1, 'AppFunName', 'C:\Icon', 'C:\App', 'yes', 1, 1, 1, 1, 'Turtle')",
                "INSERT INTO [dbo].[FieldSearch] (Enabled, FieldName, AttributeQuery) VALUES(1, 'FunField', 'SuperQuery')",
                "INSERT INTO [dbo].[DataEntryCounterDefinition] (Name, AttributeQuery, RecordOnLoad, RecordOnSave) VALUES('AmazingCounter', 'AmazingQuery', 1, 0)",
                "INSERT INTO [dbo].[DatabaseService] ([Description], Settings, Enabled) VALUES('TestServiceDescription', '{\"SomeJsonSettings\":\"True\"}', 1)",
                "INSERT INTO dbo.FAMUser (UserName, FullUsername) VALUES ('Bob', 'McBoberson')",
                @"INSERT INTO [dbo].[Dashboard] (DashboardName, Definition, FAMUserID, LastImportedDate, UseExtractedData, ExtractedDataDefinition) 
                                                VALUES('CoolDBName', '<SomeDefinition></SomeDefinition>', 1, '2020-3-11', 1, '<SomeDefinition2></SomeDefinition2>')",
                "INSERT INTO [dbo].[AttributeSetName] ([Description]) VALUES('TestAttributeSetDescripton')",
                "INSERT INTO [dbo].[AttributeName] ([Name]) VALUES('TestAttributeName')",
                "INSERT INTO [dbo].[Action] ([ASCName], [Description], [WorkflowID], [MainSequence]) VALUES('TestAction', 'TestDescription', 1, '1')",
                "INSERT INTO [dbo].[Workflow] ([Name]) VALUES ('TestWorkflow')",
                @"INSERT INTO dbo.LabDEEncounter (CSN, PatientMRN, EncounterDateTime, Department, EncounterType, EncounterProvider, DischargeDate, AdmissionDate, ADTMessage)
                                        VALUES ('1159480588', '10272749', '2017-10-17', '52CPOD', 'CONSULT', 'ROBERT RENSCHLER', '2020-1-1', '2020-1-2', '<ADTMessage></ADTMessage>')",
                @"INSERT INTO [dbo].[LabDEOrder]( [OrderNumber],[OrderCode],[PatientMRN],[ReceivedDateTime],[OrderStatus],[ReferenceDateTime],[ORMMessage],[EncounterID],[AccessionNumber])
                     VALUES('111', 'c', '222', '2020-1-1', 't', '2020-1-2', 'orm', '23',  '32')",
                @"INSERT INTO [dbo].[LabDEPatient]( [MRN],[FirstName] ,[MiddleName] ,[LastName] ,[Suffix] ,[DOB] ,[Gender] ,[MergedInto] ,[CurrentMRN])
                    VALUES ('123', 'Ninja', 'Turtles', 'Unite', 'Sr', '2020-1-1', 'w', 'F', 'FF')",
                @"INSERT INTO [dbo].[LabDEProvider]( [ID],[FirstName],[MiddleName],[LastName],[ProviderType],[Title],[Degree],[Departments],[Specialties],[Phone],[Fax],[Address],[OtherProviderID],[Inactive],[MFNMessage])
                    VALUES('1','James','Unknown','Bond','Spy','MD','MS','M6','Classified','111-111-1111','111-111-1112','200 South Main Street','43', '0', '<msg></msg>')",
                "INSERT INTO [dbo].[MetadataField] ( [Name]) VALUES ('MegaField')",
                "INSERT INTO [dbo].[MLModel] ([Name]) VALUES ('WutFace')",
                "INSERT INTO [dbo].[Tag] ([TagName],[TagDescription]) VALUES ('AwesomeTag', 'AwesomeDescription')",
                "INSERT INTO [dbo].[UserCreatedCounter]([CounterName],[Value]) VALUES ('DaName','9001')",
                "INSERT INTO [dbo].[WebAppConfig]([Type],[WorkflowID],[Settings]) VALUES ('Yes', '1', '123')",
                @"INSERT INTO [dbo].[Workflow]([Name],[WorkflowTypeCode],[Description],[StartActionID],[EndActionID],[PostWorkflowActionID],[DocumentFolder],[OutputAttributeSetID],[OutputFileMetadataFieldID],[OutputFilePathInitializationFunction],[LoadBalanceWeight],[EditActionID],[PostEditActionID])
                    VALUES('MehWorkflow', 'w', 'TheSuperDescr', 1,1,1,'C:\Wut', 1,1, 'SumFunc', 4, 1,1)",
            };

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            IFileProcessingDB dataBase = FamTestDbManager.GetNewDatabase(DatabaseName);
            dataBase.ExecuteCommandQuery(DisableAllForigenKeysInDatabase);

            foreach(string query in BuildDummyDatabase)
            {
                dataBase.ExecuteCommandQuery(query);
            }
        }

        /// <summary>
        /// TearDown method to destory testing environment.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Nunit made me")]
        [TestFixtureTearDown]
        public static void TearDown()
        {
            FamTestDbManager.RemoveDatabase(DatabaseName);
        }

        /// <summary>
        /// Tests exporting an action.
        /// </summary>
        [Test]
        public static void Action()
        {
            var writer = BuildAndWriteTable(new SerializeAction());

            var action = JsonConvert.DeserializeObject<List<Database.Input.DataTransformObject.Action>>(writer.ToString()).First();
                
            Assert.AreEqual(action.ASCName, "TestAction");
            Assert.AreEqual(action.MainSequence, true);
            Assert.AreEqual(action.Description, "TestDescription");
            Assert.IsNotNull(action.ActionGuid);
            Assert.IsNotNull(action.WorkflowGuid);
        }

        /// <summary>
        /// Tests exporting an AttributeName.
        /// </summary>
        [Test]
        public static void AttributeName()
        {
            var writer = BuildAndWriteTable(new SerializeAttributeName());

            var attributeName = JsonConvert.DeserializeObject<List<AttributeName>>(writer.ToString()).First();

            Assert.AreEqual(attributeName.Name, "TestAttributeName");
            Assert.NotNull(attributeName.Guid);
        }

        /// <summary>
        /// Tests exporting an AttributeSetName.
        /// </summary>
        [Test]
        public static void AttributeSetName()
        {
            var writer = BuildAndWriteTable(new SerializeAttributeSetName());

            var attributeSetName = JsonConvert.DeserializeObject<List<AttributeSetName>>(writer.ToString()).First();

            Assert.AreEqual(attributeSetName.Description, "TestAttributeSetDescripton");
            Assert.NotNull(attributeSetName.Guid);
        }

        /// <summary>
        /// Tests exporting the dashboard.
        /// </summary>
        [Test]
        public static void Dashboard()
        {
            var writer = BuildAndWriteTable(new SerializeDashboard());

            var dashboard = JsonConvert.DeserializeObject<List<Dashboard>>(writer.ToString()).First();

            Assert.AreEqual(dashboard.DashboardName, "CoolDBName");
            Assert.AreEqual(DateTime.Parse(dashboard.LastImportedDate, CultureInfo.InvariantCulture).Date, new DateTime(2020, 3, 11).Date);
            Assert.AreEqual(dashboard.UseExtractedData, true);
            Assert.NotNull(dashboard.DashboardGuid);
            Assert.NotNull(dashboard.FAMUserGuid);
            // XML is being auto converted to its shortened form. Below is fine even if it does not 100% match insert statement.
            Assert.AreEqual(dashboard.Definition, "<SomeDefinition />");
            Assert.AreEqual(dashboard.ExtractedDataDefinition, "<SomeDefinition2 />");
        }

        /// <summary>
        /// Tests exporting the DatabaseService.
        /// </summary>
        [Test]
        public static void DatabaseService()
        {
            var writer = BuildAndWriteTable(new SerializeDatabaseService());

            var databaseService = JsonConvert.DeserializeObject<List<DatabaseService>>(writer.ToString()).First();

            Assert.AreEqual(databaseService.Description, "TestServiceDescription");
            Assert.AreEqual(databaseService.Enabled, true);
            Assert.AreEqual(databaseService.Settings, "{\"SomeJsonSettings\":\"True\"}");
            Assert.NotNull(databaseService.Guid);
        }

        /// <summary>
        /// Tests exporting the DataEntryCounterDefinition.
        /// </summary>
        [Test]
        public static void DataEntryCounterDefinition()
        {
            var writer = BuildAndWriteTable(new SerializeDataEntryCounterDefinition());

            var dataEntryCounterDefinition = JsonConvert.DeserializeObject<List<DataEntryCounterDefinition>>(writer.ToString()).First();

            Assert.AreEqual(dataEntryCounterDefinition.Name, "AmazingCounter");
            Assert.AreEqual(dataEntryCounterDefinition.AttributeQuery, "AmazingQuery");
            Assert.AreEqual(dataEntryCounterDefinition.RecordOnLoad, true);
            Assert.AreEqual(dataEntryCounterDefinition.RecordOnSave, false);
            Assert.NotNull(dataEntryCounterDefinition.Guid);
        }

        /// <summary>
        /// Tests exporting the DBInfo. This one is a bit weird because its already populated.
        /// So my theory to test it is to look for the database id, and ensure its not null.
        /// </summary>
        [Test]
        public static void DBInfo()
        {
            var writer = BuildAndWriteTable(new SerializeDBInfo());

            var dbInfo = JsonConvert.DeserializeObject<List<DBInfo>>(writer.ToString()).Where(m => m.Name.Equals("DatabaseID")).First();

            Assert.AreEqual(dbInfo.Name, "DatabaseID");
            Assert.NotNull(dbInfo.Value);
        }

        /// <summary>
        /// Tests exporting the FAMUser.
        /// </summary>
        [Test]
        public static void FAMUser()
        {
            var writer = BuildAndWriteTable(new SerializeFAMUser());

            var famUser = JsonConvert.DeserializeObject<List<FAMUser>>(writer.ToString()).First();

            Assert.AreEqual(famUser.UserName, "Bob");
            Assert.AreEqual(famUser.FullUserName, "McBoberson");
            Assert.NotNull(famUser.Guid);
        }

        /// <summary>
        /// Tests exporting the FieldSearch.
        /// </summary>
        [Test]
        public static void FieldSearch()
        {
            var writer = BuildAndWriteTable(new SerializeFieldSearch());

            var fieldSearch = JsonConvert.DeserializeObject<List<FieldSearch>>(writer.ToString()).First();

            Assert.AreEqual(fieldSearch.Enabled, true);
            Assert.AreEqual(fieldSearch.FieldName, "FunField");
            Assert.AreEqual(fieldSearch.AttributeQuery, "SuperQuery");
            Assert.NotNull(fieldSearch.Guid);
        }

        /// <summary>
        /// Tests exporting the FileHandler.
        /// </summary>
        [Test]
        public static void FileHandler()
        {
            var writer = BuildAndWriteTable(new SerializeFileHandler());

            var dataEntryCounterDefinition = JsonConvert.DeserializeObject<List<FileHandler>>(writer.ToString()).First();

            Assert.AreEqual(dataEntryCounterDefinition.Enabled, true);
            Assert.AreEqual(dataEntryCounterDefinition.AppName, "AppFunName");
            Assert.AreEqual(dataEntryCounterDefinition.IconPath, @"C:\Icon");
            Assert.AreEqual(dataEntryCounterDefinition.ApplicationPath, @"C:\App");
            Assert.AreEqual(dataEntryCounterDefinition.Arguments, "yes");
            Assert.AreEqual(dataEntryCounterDefinition.AdminOnly, true);
            Assert.AreEqual(dataEntryCounterDefinition.AllowMultipleFiles, true);
            Assert.AreEqual(dataEntryCounterDefinition.SupportsErrorHandling, true);
            Assert.AreEqual(dataEntryCounterDefinition.Blocking, true);
            Assert.AreEqual(dataEntryCounterDefinition.WorkflowName, "Turtle");
            Assert.NotNull(dataEntryCounterDefinition.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEEncounter.
        /// </summary>
        [Test]
        public static void LabDEEncounter()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEEncounter());

            var labDEEncounter = JsonConvert.DeserializeObject<List<LabDEEncounter>>(writer.ToString()).First();

            Assert.AreEqual(labDEEncounter.CSN, "1159480588");
            Assert.AreEqual(labDEEncounter.PatientMRN, "10272749");
            Assert.AreEqual(DateTime.Parse(labDEEncounter.EncounterDateTime, CultureInfo.InvariantCulture).Date, new DateTime(2017,10,17).Date);
            Assert.AreEqual(labDEEncounter.Department, "52CPOD");
            Assert.AreEqual(labDEEncounter.EncounterType, "CONSULT");
            Assert.AreEqual(labDEEncounter.EncounterProvider, "ROBERT RENSCHLER");
            Assert.AreEqual(DateTime.Parse(labDEEncounter.DischargeDate, CultureInfo.InvariantCulture).Date, new DateTime(2020, 1, 1).Date);
            Assert.AreEqual(DateTime.Parse(labDEEncounter.AdmissionDate, CultureInfo.InvariantCulture).Date, new DateTime(2020, 1, 2).Date);
            Assert.AreEqual(labDEEncounter.ADTMessage, "<ADTMessage />");
            Assert.NotNull(labDEEncounter.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEOrder.
        /// </summary>
        [Test]
        public static void LabDEOrder()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEOrder());

            var labDEOrder = JsonConvert.DeserializeObject<List<LabDEOrder>>(writer.ToString()).First();

            Assert.AreEqual(labDEOrder.OrderNumber, "111");
            Assert.AreEqual(labDEOrder.OrderCode, "c");
            Assert.AreEqual(labDEOrder.PatientMRN, "222");
            Assert.AreEqual(DateTime.Parse(labDEOrder.ReceivedDateTime, CultureInfo.InvariantCulture).Date, new DateTime(2020, 1, 1).Date);
            Assert.AreEqual(labDEOrder.OrderStatus, "t");
            Assert.AreEqual(DateTime.Parse(labDEOrder.ReferenceDateTime, CultureInfo.InvariantCulture).Date, new DateTime(2020, 1, 2).Date);
            Assert.AreEqual(labDEOrder.ORMMessage, "orm");
            Assert.AreEqual(labDEOrder.EncounterID, "23");
            Assert.AreEqual(labDEOrder.AccessionNumber, "32");
            Assert.NotNull(labDEOrder.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEPatient.
        /// </summary>
        [Test]
        public static void LabDEPatient()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEPatient());

            var labDEPatient = JsonConvert.DeserializeObject<List<LabDEPatient>>(writer.ToString()).First();

            Assert.AreEqual(labDEPatient.MRN, "123");
            Assert.AreEqual(labDEPatient.FirstName, "Ninja");
            Assert.AreEqual(labDEPatient.MiddleName, "Turtles");
            Assert.AreEqual(labDEPatient.LastName, "Unite");
            Assert.AreEqual(labDEPatient.Suffix, "Sr");
            Assert.AreEqual(DateTime.Parse(labDEPatient.DOB, CultureInfo.InvariantCulture).Date, new DateTime(2020, 1, 1).Date);
            Assert.AreEqual(labDEPatient.Gender, "w");
            Assert.AreEqual(labDEPatient.MergedInto, "F");
            Assert.AreEqual(labDEPatient.CurrentMRN, "FF");
            Assert.NotNull(labDEPatient.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEProvider.
        /// </summary>
        [Test]
        public static void LabDEProvider()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEProvider());

            var labDeProvider = JsonConvert.DeserializeObject<List<LabDEProvider>>(writer.ToString()).First();

            Assert.AreEqual(labDeProvider.FirstName, "James");
            Assert.AreEqual(labDeProvider.MiddleName, "Unknown");
            Assert.AreEqual(labDeProvider.LastName, "Bond");
            Assert.AreEqual(labDeProvider.ProviderType, "Spy");
            Assert.AreEqual(labDeProvider.Title, "MD");
            Assert.AreEqual(labDeProvider.Degree, "MS");
            Assert.AreEqual(labDeProvider.Departments, "M6");
            Assert.AreEqual(labDeProvider.Specialties, "Classified");
            Assert.AreEqual(labDeProvider.Phone, "111-111-1111");
            Assert.AreEqual(labDeProvider.Fax, "111-111-1112");
            Assert.AreEqual(labDeProvider.Address, "200 South Main Street");
            Assert.AreEqual(labDeProvider.OtherProviderID, "43");
            Assert.AreEqual(labDeProvider.Inactive, false);
            Assert.AreEqual(labDeProvider.MFNMessage, "<msg />");
            Assert.NotNull(labDeProvider.Guid);
        }

        /// <summary>
        /// Tests exporting the Login.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Its like this in the database")]
        public static void Login()
        {
            var writer = BuildAndWriteTable(new SerializeLogin());

            var login = JsonConvert.DeserializeObject<List<Login>>(writer.ToString()).First();
            Assert.AreEqual(login.UserName, "admin");
            Assert.AreEqual(login.Password, "e086da2321be72f0525b25d5d5b0c6d7");
            Assert.NotNull(login.Guid);
        }

        /// <summary>
        /// Tests exporting the MetadataField.
        /// </summary>
        [Test]
        public static void MetadataField()
        {
            var writer = BuildAndWriteTable(new SerializeMetadataField());

            var metadataField = JsonConvert.DeserializeObject<List<MetadataField>>(writer.ToString()).First();
            Assert.AreEqual(metadataField.Name, "MegaField");
            Assert.NotNull(metadataField.Guid);
        }

        /// <summary>
        /// Tests exporting the MLModel.
        /// </summary>
        [Test]
        public static void MLModel()
        {
            var writer = BuildAndWriteTable(new SerializeMLModel());

            var mLModel = JsonConvert.DeserializeObject<List<MLModel>>(writer.ToString()).First();
            Assert.AreEqual(mLModel.Name, "WutFace");
            Assert.NotNull(mLModel.Guid);
        }

        /// <summary>
        /// Tests exporting the Tag.
        /// </summary>
        [Test]
        public static void Tag()
        {
            var writer = BuildAndWriteTable(new SerializeTag());

            var tag = JsonConvert.DeserializeObject<List<Tag>>(writer.ToString()).First();
            Assert.AreEqual(tag.TagName, "AwesomeTag");
            Assert.AreEqual(tag.TagDescription, "AwesomeDescription");
            Assert.NotNull(tag.Guid);
        }

        /// <summary>
        /// Tests exporting the UserCreatedCounter.
        /// </summary>
        [Test]
        public static void UserCreatedCounter()
        {
            var writer = BuildAndWriteTable(new SerializeUserCreatedCounter());

            var userCreatedCounter = JsonConvert.DeserializeObject<List<UserCreatedCounter>>(writer.ToString()).First();
            Assert.AreEqual(userCreatedCounter.CounterName, "DaName");
            Assert.AreEqual(userCreatedCounter.Value, "9001");
            Assert.NotNull(userCreatedCounter.Guid);
        }

        /// <summary>
        /// Tests exporting the WebappConfig.
        /// </summary>
        [Test]
        public static void WebAppConfig()
        {
            var writer = BuildAndWriteTable(new SerializeWebAppConfig());

            var webAppConfig = JsonConvert.DeserializeObject<List<WebAppConfig>>(writer.ToString()).First();
            Assert.AreEqual(webAppConfig.Type, "Yes");
            Assert.AreEqual(webAppConfig.Settings, "123");
            Assert.NotNull(webAppConfig.WorkflowGuid);
            Assert.NotNull(webAppConfig.WebAppConfigGuid);
        }

        /// <summary>
        /// Tests exporting the Workflow.
        /// </summary>
        [Test]
        public static void Workflow()
        {
            var writer = BuildAndWriteTable(new SerializeWorkflow());

            var workFlow = JsonConvert.DeserializeObject<List<Workflow>>(writer.ToString()).Where(m => m.Name.Equals("MehWorkflow")).First();
            Assert.AreEqual(workFlow.Name, "MehWorkflow");
            Assert.AreEqual(workFlow.WorkflowTypeCode, "w");
            Assert.AreEqual(workFlow.Description, "TheSuperDescr");
            Assert.AreEqual(workFlow.DocumentFolder, @"C:\Wut");
            Assert.AreEqual(workFlow.OutputFilePathInitializationFunction, @"SumFunc");
            Assert.AreEqual(workFlow.LoadBalanceWeight, 4);
            Assert.NotNull(workFlow.EditActionGuid);
            Assert.NotNull(workFlow.EndActionGuid);
            Assert.NotNull(workFlow.PostEditActionGuid);
            Assert.NotNull(workFlow.PostWorkflowActionGuid);
            Assert.NotNull(workFlow.StartActionGuid);
            Assert.NotNull(workFlow.AttributeSetNameGuid);
            Assert.NotNull(workFlow.MetadataFieldGuid);
        }

        /// <summary>
        /// Tests running everything
        /// The output does not really matter because that is tested individually in this class.
        /// </summary>
        [Test]
        public static void ExportEverything()
        {
            var exportOptions = new ExportOptions() { ConnectionInformation = new Database.ConnectionInformation() };
            string[] files;
            exportOptions.ConnectionInformation.DatabaseServer = "(local)";
            exportOptions.ConnectionInformation.DatabaseName = DatabaseName;
            exportOptions.IncludeLabDETables = false;
            exportOptions.ExportPath = Path.GetTempPath() + "TableExports\\";
            Directory.CreateDirectory(exportOptions.ExportPath);

            // Ensure LabDE tables are NOT exported
            ExportHelper.Export(exportOptions, new Progress<string>((garbage) => { }));
            files = System.IO.Directory.GetFiles(exportOptions.ExportPath);
            Assert.IsFalse(files.Where(file => file.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE")).Any());

            // Ensure LabDE tables are exported
            exportOptions.IncludeLabDETables = true;
            ExportHelper.Export(exportOptions, new Progress<string>((garbage) => { }));
            files = System.IO.Directory.GetFiles(exportOptions.ExportPath);
            Assert.IsTrue(files.Where(file => file.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE")).Any());

            Directory.Delete(exportOptions.ExportPath, true);
        }

        private static StringWriter BuildAndWriteTable(ISerialize serialize)
        {
            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);

            using(SqlConnection sqlConnection = new SqlConnection($@"Server=(local);Database={DatabaseName};Integrated Security=SSPI"))
            {
                sqlConnection.Open();
                serialize.SerializeTable(sqlConnection, stringWriter);
            }

            return stringWriter;
        }
    }
}