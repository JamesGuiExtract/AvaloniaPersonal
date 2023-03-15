using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using DatabaseMigrationWizard.Database.Output;
using Extract.Database;
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

        private static readonly string DatabaseName = "Test_Export";

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
                                                VALUES('CoolDBName', '<SomeDefinition></SomeDefinition>', 2, '2020-3-11', 1, '<SomeDefinition2></SomeDefinition2>')",
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
                @"INSERT INTO [dbo].[Workflow]([Name],[WorkflowTypeCode],[Description],[LoadBalanceWeight])
                    VALUES('MehWorkflow', 'w', 'TheSuperDescr', 4)",
                "INSERT INTO dbo.Login (UserName, Password, Guid) VALUES ('notAdmin', 'e086da2321be72f0525b25d5d5b0c6d7', 'd9ca9ee6-ae9b-496b-8c48-8db752fe6940')",
                "INSERT INTO dbo.WebAPIConfiguration(Name,Settings) VALUES ('Yes','123')",
            };

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            IFileProcessingDB dataBase = FamTestDbManager.GetNewDatabase(DatabaseName);
            
            using SqlConnection connection = SqlUtil.NewSqlDBConnection("(local)", DatabaseName);
            connection.Open();
            DBMethods.ExecuteDBQuery(connection, DisableAllForigenKeysInDatabase);

            foreach(string query in BuildDummyDatabase)
            {
                DBMethods.ExecuteDBQuery(connection, query);
            }
        }

        /// <summary>
        /// TearDown method to destory testing environment.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Nunit made me")]
        [OneTimeTearDown]
        public static void TearDown()
        {
            FamTestDbManager.RemoveDatabase(DatabaseName);
        }

        /// <summary>
        /// Tests exporting an action.
        /// </summary>
        [Test, Category("Automated")]
        public static void Action()
        {
            var writer = BuildAndWriteTable(new SerializeAction());

            var action = JsonConvert.DeserializeObject<List<Database.Input.DataTransformObject.Action>>(writer.ToString()).First();
                
            Assert.AreEqual("TestAction", action.ASCName);
            Assert.AreEqual(true, action.MainSequence);
            Assert.AreEqual("TestDescription", action.Description);
            Assert.IsNotNull(action.ActionGuid);
            Assert.IsNotNull(action.WorkflowGuid);
        }

        /// <summary>
        /// Tests exporting an AttributeSetName.
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeSetName()
        {
            var writer = BuildAndWriteTable(new SerializeAttributeSetName());

            var attributeSetName = JsonConvert.DeserializeObject<List<AttributeSetName>>(writer.ToString()).First();

            Assert.AreEqual("TestAttributeSetDescripton", attributeSetName.Description);
            Assert.NotNull(attributeSetName.Guid);
        }

        /// <summary>
        /// Tests exporting the dashboard.
        /// </summary>
        [Test, Category("Automated")]
        public static void Dashboard()
        {
            var writer = BuildAndWriteTable(new SerializeDashboard());

            var dashboard = JsonConvert.DeserializeObject<List<Dashboard>>(writer.ToString()).First();

            Assert.AreEqual("CoolDBName", dashboard.DashboardName);
            Assert.AreEqual(new DateTime(2020, 3, 11).Date, DateTime.Parse(dashboard.LastImportedDate, CultureInfo.InvariantCulture).Date);
            Assert.AreEqual(true, dashboard.UseExtractedData);
            Assert.NotNull(dashboard.DashboardGuid);
            Assert.AreEqual(dashboard.FullUserName, "McBoberson");
            Assert.NotNull(dashboard.UserName, "Bob");
            // XML is being auto converted to its shortened form. Below is fine even if it does not 100% match insert statement.
            Assert.AreEqual("<SomeDefinition />", dashboard.Definition);
            Assert.AreEqual("<SomeDefinition2 />", dashboard.ExtractedDataDefinition);
        }

        /// <summary>
        /// Tests exporting the DatabaseService.
        /// </summary>
        [Test, Category("Automated")]
        public static void DatabaseService()
        {
            var writer = BuildAndWriteTable(new SerializeDatabaseService());

            var databaseService = JsonConvert.DeserializeObject<List<DatabaseService>>(writer.ToString()).First();

            Assert.AreEqual("TestServiceDescription", databaseService.Description);
            Assert.AreEqual(true, databaseService.Enabled);
            Assert.AreEqual("{\"SomeJsonSettings\":\"True\"}", databaseService.Settings);
            Assert.NotNull(databaseService.Guid);
        }

        /// <summary>
        /// Tests exporting the DataEntryCounterDefinition.
        /// </summary>
        [Test, Category("Automated")]
        public static void DataEntryCounterDefinition()
        {
            var writer = BuildAndWriteTable(new SerializeDataEntryCounterDefinition());

            var dataEntryCounterDefinition = JsonConvert.DeserializeObject<List<DataEntryCounterDefinition>>(writer.ToString()).First();

            Assert.AreEqual("AmazingCounter", dataEntryCounterDefinition.Name);
            Assert.AreEqual("AmazingQuery", dataEntryCounterDefinition.AttributeQuery);
            Assert.AreEqual(true, dataEntryCounterDefinition.RecordOnLoad);
            Assert.AreEqual(false, dataEntryCounterDefinition.RecordOnSave);
            Assert.NotNull(dataEntryCounterDefinition.Guid);
        }

        /// <summary>
        /// Tests exporting the DBInfo. This one is a bit weird because its already populated.
        /// So my theory to test it is to look for the database id, and ensure its not null.
        /// </summary>
        [Test, Category("Automated")]
        public static void DBInfo()
        {
            var writer = BuildAndWriteTable(new SerializeDBInfo());

            var dbInfo = JsonConvert.DeserializeObject<List<DBInfo>>(writer.ToString()).Where(m => m.Name.Equals("DatabaseID")).First();

            Assert.AreEqual("DatabaseID", dbInfo.Name);
            Assert.NotNull(dbInfo.Value);
        }

        /// <summary>
        /// Tests exporting the FAMUser.
        /// </summary>
        [Test, Category("Automated")]
        public static void FAMUser()
        {
            var writer = BuildAndWriteTable(new SerializeFAMUser());

            var famUser = JsonConvert.DeserializeObject<List<FAMUser>>(writer.ToString())[1];

            Assert.AreEqual("Bob", famUser.UserName);
            Assert.AreEqual("McBoberson", famUser.FullUserName);
        }

        /// <summary>
        /// Tests exporting the FieldSearch.
        /// </summary>
        [Test, Category("Automated")]
        public static void FieldSearch()
        {
            var writer = BuildAndWriteTable(new SerializeFieldSearch());

            var fieldSearch = JsonConvert.DeserializeObject<List<FieldSearch>>(writer.ToString()).First();

            Assert.AreEqual(true, fieldSearch.Enabled);
            Assert.AreEqual("FunField", fieldSearch.FieldName);
            Assert.AreEqual("SuperQuery", fieldSearch.AttributeQuery);
            Assert.NotNull(fieldSearch.Guid);
        }

        /// <summary>
        /// Tests exporting the FileHandler.
        /// </summary>
        [Test, Category("Automated")]
        public static void FileHandler()
        {
            var writer = BuildAndWriteTable(new SerializeFileHandler());

            var dataEntryCounterDefinition = JsonConvert.DeserializeObject<List<FileHandler>>(writer.ToString()).First();

            Assert.AreEqual(true, dataEntryCounterDefinition.Enabled);
            Assert.AreEqual("AppFunName", dataEntryCounterDefinition.AppName);
            Assert.AreEqual(@"C:\Icon", dataEntryCounterDefinition.IconPath);
            Assert.AreEqual(@"C:\App", dataEntryCounterDefinition.ApplicationPath);
            Assert.AreEqual("yes", dataEntryCounterDefinition.Arguments);
            Assert.AreEqual(true, dataEntryCounterDefinition.AdminOnly);
            Assert.AreEqual(true, dataEntryCounterDefinition.AllowMultipleFiles);
            Assert.AreEqual(true, dataEntryCounterDefinition.SupportsErrorHandling);
            Assert.AreEqual(true, dataEntryCounterDefinition.Blocking);
            Assert.AreEqual("Turtle", dataEntryCounterDefinition.WorkflowName);
            Assert.NotNull(dataEntryCounterDefinition.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEEncounter.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEEncounter()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEEncounter());

            var labDEEncounter = JsonConvert.DeserializeObject<List<LabDEEncounter>>(writer.ToString()).First();

            Assert.AreEqual("1159480588", labDEEncounter.CSN);
            Assert.AreEqual("10272749", labDEEncounter.PatientMRN);
            Assert.AreEqual(new DateTime(2017, 10, 17).Date, DateTime.Parse(labDEEncounter.EncounterDateTime, CultureInfo.InvariantCulture).Date);
            Assert.AreEqual("52CPOD", labDEEncounter.Department);
            Assert.AreEqual("CONSULT", labDEEncounter.EncounterType);
            Assert.AreEqual("ROBERT RENSCHLER", labDEEncounter.EncounterProvider);
            Assert.AreEqual(new DateTime(2020, 1, 1).Date, DateTime.Parse(labDEEncounter.DischargeDate, CultureInfo.InvariantCulture).Date);
            Assert.AreEqual(new DateTime(2020, 1, 2).Date, DateTime.Parse(labDEEncounter.AdmissionDate, CultureInfo.InvariantCulture).Date);
            Assert.AreEqual("<ADTMessage />", labDEEncounter.ADTMessage);
            Assert.NotNull(labDEEncounter.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEOrder.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEOrder()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEOrder());

            var labDEOrder = JsonConvert.DeserializeObject<List<LabDEOrder>>(writer.ToString()).First();

            Assert.AreEqual("111", labDEOrder.OrderNumber);
            Assert.AreEqual("c", labDEOrder.OrderCode);
            Assert.AreEqual("222", labDEOrder.PatientMRN);
            Assert.AreEqual(new DateTime(2020, 1, 1).Date, DateTime.Parse(labDEOrder.ReceivedDateTime, CultureInfo.InvariantCulture).Date);
            Assert.AreEqual("t", labDEOrder.OrderStatus);
            Assert.AreEqual(new DateTime(2020, 1, 2).Date, DateTime.Parse(labDEOrder.ReferenceDateTime, CultureInfo.InvariantCulture).Date);
            Assert.AreEqual("orm", labDEOrder.ORMMessage);
            Assert.AreEqual("23", labDEOrder.EncounterID);
            Assert.AreEqual("32", labDEOrder.AccessionNumber);
            Assert.NotNull(labDEOrder.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEPatient.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEPatient()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEPatient());

            var labDEPatient = JsonConvert.DeserializeObject<List<LabDEPatient>>(writer.ToString()).First();

            Assert.AreEqual("123", labDEPatient.MRN);
            Assert.AreEqual("Ninja", labDEPatient.FirstName);
            Assert.AreEqual("Turtles", labDEPatient.MiddleName);
            Assert.AreEqual("Unite", labDEPatient.LastName);
            Assert.AreEqual("Sr", labDEPatient.Suffix);
            Assert.AreEqual(new DateTime(2020, 1, 1).Date, DateTime.Parse(labDEPatient.DOB, CultureInfo.InvariantCulture).Date);
            Assert.AreEqual("w", labDEPatient.Gender);
            Assert.AreEqual("F", labDEPatient.MergedInto);
            Assert.AreEqual("FF", labDEPatient.CurrentMRN);
            Assert.NotNull(labDEPatient.Guid);
        }

        /// <summary>
        /// Tests exporting the LabDEProvider.
        /// </summary>
        [Test, Category("Automated")]
        public static void LabDEProvider()
        {
            var writer = BuildAndWriteTable(new SerializeLabDEProvider());

            var labDeProvider = JsonConvert.DeserializeObject<List<LabDEProvider>>(writer.ToString()).First();

            Assert.AreEqual("James", labDeProvider.FirstName);
            Assert.AreEqual("Unknown", labDeProvider.MiddleName);
            Assert.AreEqual("Bond", labDeProvider.LastName);
            Assert.AreEqual("Spy", labDeProvider.ProviderType);
            Assert.AreEqual("MD", labDeProvider.Title);
            Assert.AreEqual("MS", labDeProvider.Degree);
            Assert.AreEqual("M6", labDeProvider.Departments);
            Assert.AreEqual("Classified", labDeProvider.Specialties);
            Assert.AreEqual("111-111-1111", labDeProvider.Phone);
            Assert.AreEqual("111-111-1112", labDeProvider.Fax);
            Assert.AreEqual("200 South Main Street", labDeProvider.Address);
            Assert.AreEqual("43", labDeProvider.OtherProviderID);
            Assert.AreEqual(false, labDeProvider.Inactive);
            Assert.AreEqual("<msg />", labDeProvider.MFNMessage);
            Assert.NotNull(labDeProvider.Guid);
        }

        /// <summary>
        /// Tests exporting the Login.
        /// </summary>
        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Its like this in the database")]
        public static void Login()
        {
            var writer = BuildAndWriteTable(new SerializeLogin());

            var login = JsonConvert.DeserializeObject<List<Login>>(writer.ToString()).Where(m => !m.UserName.Equals("admin")).First();
            Assert.AreEqual("notAdmin", login.UserName);
            Assert.AreEqual("e086da2321be72f0525b25d5d5b0c6d7", login.Password);
            Assert.NotNull(login.Guid);
        }

        /// <summary>
        /// Tests exporting the MetadataField.
        /// </summary>
        [Test, Category("Automated")]
        public static void MetadataField()
        {
            var writer = BuildAndWriteTable(new SerializeMetadataField());

            var metadataField = JsonConvert.DeserializeObject<List<MetadataField>>(writer.ToString()).First();
            Assert.AreEqual("MegaField", metadataField.Name);
            Assert.NotNull(metadataField.Guid);
        }

        /// <summary>
        /// Tests exporting the MLModel.
        /// </summary>
        [Test, Category("Automated")]
        public static void MLModel()
        {
            var writer = BuildAndWriteTable(new SerializeMLModel());

            var mLModel = JsonConvert.DeserializeObject<List<MLModel>>(writer.ToString()).First();
            Assert.AreEqual("WutFace", mLModel.Name);
            Assert.NotNull(mLModel.Guid);
        }

        /// <summary>
        /// Tests exporting the Tag.
        /// </summary>
        [Test, Category("Automated")]
        public static void Tag()
        {
            var writer = BuildAndWriteTable(new SerializeTag());

            var tag = JsonConvert.DeserializeObject<List<Tag>>(writer.ToString()).First();
            Assert.AreEqual("AwesomeTag", tag.TagName);
            Assert.AreEqual("AwesomeDescription", tag.TagDescription);
            Assert.NotNull(tag.Guid);
        }

        /// <summary>
        /// Tests exporting the UserCreatedCounter.
        /// </summary>
        [Test, Category("Automated")]
        public static void UserCreatedCounter()
        {
            var writer = BuildAndWriteTable(new SerializeUserCreatedCounter());

            var userCreatedCounter = JsonConvert.DeserializeObject<List<UserCreatedCounter>>(writer.ToString()).First();
            Assert.AreEqual("DaName", userCreatedCounter.CounterName);
            Assert.AreEqual("9001", userCreatedCounter.Value);
            Assert.NotNull(userCreatedCounter.Guid);
        }

        /// <summary>
        /// Tests exporting the WebappConfig.
        /// </summary>
        [Test, Category("Automated")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API")]
        public static void WebAPIConfiguration()
        {
            var writer = BuildAndWriteTable(new SerializeWebAPIConfiguration());

            var webAppConfig = JsonConvert.DeserializeObject<List<WebAPIConfiguration>>(writer.ToString()).First();
            Assert.AreEqual("Yes", webAppConfig.Name);
            Assert.AreEqual("123", webAppConfig.Settings);
            Assert.NotNull(webAppConfig.Guid);
        }

        /// <summary>
        /// Tests exporting the Workflow.
        /// </summary>
        [Test, Category("Automated")]
        public static void Workflow()
        {
            var writer = BuildAndWriteTable(new SerializeWorkflow());

            var workFlow = JsonConvert.DeserializeObject<List<Workflow>>(writer.ToString()).Where(m => m.Name.Equals("MehWorkflow")).First();
            Assert.AreEqual("MehWorkflow", workFlow.Name);
            Assert.AreEqual("w", workFlow.WorkflowTypeCode);
            Assert.AreEqual("TheSuperDescr", workFlow.Description);
            Assert.AreEqual(4, workFlow.LoadBalanceWeight);
        }

        /// <summary>
        /// Tests running everything
        /// The output does not really matter because that is tested individually in this class.
        /// </summary>
        [Test, Category("Automated")]
        public static void ExportEverything()
        {
            var exportDir = Directory.CreateDirectory(Path.GetTempPath() + "TableExports");
            try
            {
                var exportOptions = new ExportOptions() { ConnectionInformation = new Database.ConnectionInformation() };
                string[] files;
                exportOptions.ConnectionInformation.DatabaseServer = "(local)";
                exportOptions.ConnectionInformation.DatabaseName = DatabaseName;
                exportOptions.ExportLabDETables = false;
                exportOptions.ExportPath = exportDir.FullName + "\\";

                // Ensure LabDE tables are NOT exported
                ExportHelper.Export(exportOptions, new Progress<string>((garbage) => { }));
                files = Directory.GetFiles(exportOptions.ExportPath);
                CollectionAssert.IsEmpty(files.Where(file => file.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE")));

                // Ensure LabDE tables are exported
                exportOptions.ExportLabDETables = true;
                ExportHelper.Export(exportOptions, new Progress<string>((garbage) => { }));
                files = Directory.GetFiles(exportOptions.ExportPath);
                CollectionAssert.IsNotEmpty(files.Where(file => file.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE")));
            }
            finally
            {
                exportDir.Delete(true);
            }
        }

        private static StringWriter BuildAndWriteTable(ISerialize serialize)
        {
            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);

            using var connection = new ExtractRoleConnection($@"Server=(local);Database={DatabaseName};Integrated Security=SSPI;Pooling=false");
            connection.Open();

            serialize.SerializeTable(connection, stringWriter);

            return stringWriter;
        }
    }
}