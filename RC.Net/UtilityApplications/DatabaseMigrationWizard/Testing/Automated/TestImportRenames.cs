using DatabaseMigrationWizard.Database.Input;
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
    [Category("DatabaseMigrationWizardImports")]
    public class TestImportRenames
    {
        private static readonly string DropTempTables = @"declare @sql nvarchar(max)
                                                        select @sql = isnull(@sql+';', '') + 'drop table ' + quotename(name)
                                                        from tempdb..sysobjects
                                                        where name like '##%'
                                                        exec (@sql)";
        private static readonly FAMTestDBManager<TestExports> FamTestDbManager = new FAMTestDBManager<TestExports>();

        private static readonly string DatabaseName = "TestImportRenames";

        private static ImportOptions ImportOptions;

        private static SqlConnection SqlConnection;

        private static DatabaseMigrationWizardTestHelper DatabaseMigrationWizardTestHelper;

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
            var importHelper1 = new ImportHelper(ImportOptions, new Progress<string>((garbage) => { }));
            importHelper1.Import();
            importHelper1.CommitTransaction();
            dataBase.ExecuteCommandQuery(DropTempTables);

            RenameRecords();
            DatabaseMigrationWizardTestHelper.WriteEverythingToDirectory(ImportOptions.ImportPath);
            var importHelper2 = new ImportHelper(ImportOptions, new Progress<string>((garbage) => { }));
            importHelper2.Import();
            importHelper2.CommitTransaction();

            SqlConnection = new SqlConnection($@"Server={ImportOptions.ConnectionInformation.DatabaseServer};Database={ImportOptions.ConnectionInformation.DatabaseName};Integrated Security=SSPI");
            SqlConnection.Open();
        }

        /// <summary>
        /// TearDown method to destory testing environment.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Nunit made me")]
        [OneTimeTearDown]
        public static void TearDown()
        {
            SqlConnection?.Close();
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
        public static void AttributeName()
        {
            var attributeNameFromDB = JsonConvert.DeserializeObject<List<AttributeName>>(BuildAndWriteTable(new SerializeAttributeName()).ToString());

            foreach (var attributeName in DatabaseMigrationWizardTestHelper.AttributeNames)
            {
                Assert.IsTrue(attributeNameFromDB.Where(m => m.Equals(attributeName)).Any());
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

            foreach (var Login in DatabaseMigrationWizardTestHelper.Logins)
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

            using (SqlConnection sqlConnection = new SqlConnection($@"Server=(local);Database={DatabaseName};Integrated Security=SSPI"))
            {
                sqlConnection.Open();
                serialize.SerializeTable(sqlConnection, stringWriter);
            }

            return stringWriter;
        }

        /// <summary>
        /// Renames everything about the first record in each table (Except db info because hardcoded schema).
        /// </summary>
        private static void RenameRecords()
        {
            DatabaseMigrationWizardTestHelper.Actions[0].ASCName = "NewName";
            DatabaseMigrationWizardTestHelper.Actions[0].Description = "NewDescription";
            DatabaseMigrationWizardTestHelper.Actions[0].MainSequence = false;
            DatabaseMigrationWizardTestHelper.Actions[0].WorkflowGuid = Guid.Parse("0a3e4811-f55f-49d2-9c6f-0b64cd56961e");

            DatabaseMigrationWizardTestHelper.AttributeNames[0].Name = "NewAttributeName";

            DatabaseMigrationWizardTestHelper.AttributeSetNames[0].Description = "NewDescription";

            DatabaseMigrationWizardTestHelper.Dashboards[0].DashboardName = "NewDBName";
            DatabaseMigrationWizardTestHelper.Dashboards[0].Definition = @"<New />";
            DatabaseMigrationWizardTestHelper.Dashboards[0].ExtractedDataDefinition = @"<Extract />";
            DatabaseMigrationWizardTestHelper.Dashboards[0].LastImportedDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            DatabaseMigrationWizardTestHelper.Dashboards[0].UseExtractedData = false;
            DatabaseMigrationWizardTestHelper.Dashboards[0].FullUserName = "NewUsername";
            DatabaseMigrationWizardTestHelper.Dashboards[0].UserName = "Weee";

            DatabaseMigrationWizardTestHelper.DatabaseServices[0].Description = "NewerDefinition";
            DatabaseMigrationWizardTestHelper.DatabaseServices[0].Enabled = false;
            DatabaseMigrationWizardTestHelper.DatabaseServices[0].Settings = "New super settings";

            DatabaseMigrationWizardTestHelper.DataEntryCounterDefinitions[0].AttributeQuery = "Super duper query";
            DatabaseMigrationWizardTestHelper.DataEntryCounterDefinitions[0].Name = "Unknown";
            DatabaseMigrationWizardTestHelper.DataEntryCounterDefinitions[0].RecordOnLoad = false;
            DatabaseMigrationWizardTestHelper.DataEntryCounterDefinitions[0].RecordOnSave = true;

            DatabaseMigrationWizardTestHelper.DBInfos[0].Value = "Yassss";

            DatabaseMigrationWizardTestHelper.FAMUsers[0].FullUserName = "NewUsername";
            DatabaseMigrationWizardTestHelper.FAMUsers[0].UserName = "Weee";

            DatabaseMigrationWizardTestHelper.FieldSearches[0].AttributeQuery = "BestQueryEver";
            DatabaseMigrationWizardTestHelper.FieldSearches[0].Enabled = false;
            DatabaseMigrationWizardTestHelper.FieldSearches[0].FieldName = "NewFieldName";

            DatabaseMigrationWizardTestHelper.FileHandlers[0].AdminOnly = false;
            DatabaseMigrationWizardTestHelper.FileHandlers[0].AllowMultipleFiles = false;
            DatabaseMigrationWizardTestHelper.FileHandlers[0].ApplicationPath = "C:\\New";
            DatabaseMigrationWizardTestHelper.FileHandlers[0].AppName = "MegaApp";
            DatabaseMigrationWizardTestHelper.FileHandlers[0].Arguments = "No";
            DatabaseMigrationWizardTestHelper.FileHandlers[0].Blocking = false;
            DatabaseMigrationWizardTestHelper.FileHandlers[0].Enabled = false;
            DatabaseMigrationWizardTestHelper.FileHandlers[0].IconPath = "C:\\Old";
            DatabaseMigrationWizardTestHelper.FileHandlers[0].SupportsErrorHandling = false;
            DatabaseMigrationWizardTestHelper.FileHandlers[0].WorkflowName = "WorkflowRename";

            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].AdmissionDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].ADTMessage = "<adts />";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].CSN = "89735";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].Department = "Rename";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].DischargeDate = "2020-3-18";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].EncounterDateTime = "2020-4-18";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].EncounterProvider = "Bob";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].EncounterType = "Scan";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[1].PatientMRN = "69476";
            DatabaseMigrationWizardTestHelper.LabDEEncounters[0].PatientMRN = "69476";

            DatabaseMigrationWizardTestHelper.LabDEOrders[0].AccessionNumber = "1234";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].EncounterID = "89735";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].OrderCode = "645";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].OrderNumber = "666";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].OrderStatus = "A";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].ORMMessage = "ORM Messagesss";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].PatientMRN = "69476";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].ReceivedDateTime = "2020-3-23";
            DatabaseMigrationWizardTestHelper.LabDEOrders[0].ReferenceDateTime = "2020-6-11";
            
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].CurrentMRN = "69476";
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].DOB = "2019-1-1";
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].FirstName = "Jen";
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].Gender = "F";
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].LastName = "Bringstien";
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].MergedInto = null;
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].MiddleName = "nm";
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].MRN = "69476";
            DatabaseMigrationWizardTestHelper.LabDEPatients[0].Suffix = "sr";
            DatabaseMigrationWizardTestHelper.LabDEPatients[1].MergedInto = "69476";

            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Address = "Fun street";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Degree = "FunMaster";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Departments = "NoClue";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Fax = "123-456-3284";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].FirstName = "Joe";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].ID = "55";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Inactive = false;
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].LastName = "mi";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].MFNMessage = "UpdatedMFN";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].MiddleName = "lk";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].OtherProviderID = "89";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Phone = "987-847-3923";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].ProviderType = "Specialist";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Specialties = "Fun";
            DatabaseMigrationWizardTestHelper.LabDEProviders[0].Title = "Funster";

            DatabaseMigrationWizardTestHelper.Logins[0].UserName = "Bobersun";
            DatabaseMigrationWizardTestHelper.Logins[0].Password = "Shhhhh";

            DatabaseMigrationWizardTestHelper.MetadataFields[0].Name = "RenamedNewNamesName";

            DatabaseMigrationWizardTestHelper.MLModels[0].Name = "MlModelsMod";

            DatabaseMigrationWizardTestHelper.Tags[0].TagDescription = "RenamedDesc";
            DatabaseMigrationWizardTestHelper.Tags[0].TagName = "RenamedTag";

            DatabaseMigrationWizardTestHelper.UserCreatedCounters[0].CounterName = "RenamedCounter";
            DatabaseMigrationWizardTestHelper.UserCreatedCounters[0].Value = "742";

            DatabaseMigrationWizardTestHelper.WebAppConfigurations[0].Settings = "RenamedSettings";
            DatabaseMigrationWizardTestHelper.WebAppConfigurations[0].Type = "RenamedSettings";
            DatabaseMigrationWizardTestHelper.WebAppConfigurations[0].WorkflowGuid = Guid.Parse("0a3e4811-f55f-49d2-9c6f-0b64cd56961e");

            DatabaseMigrationWizardTestHelper.Workflows[0].Description = "New Description";
            DatabaseMigrationWizardTestHelper.Workflows[0].DocumentFolder = "C:\\Docs";
            DatabaseMigrationWizardTestHelper.Workflows[0].LoadBalanceWeight = 8;
            DatabaseMigrationWizardTestHelper.Workflows[0].Name = "WorkflowRename";
            DatabaseMigrationWizardTestHelper.Workflows[0].OutputFilePathInitializationFunction = "Plus Minus equal three";
            DatabaseMigrationWizardTestHelper.Workflows[0].WorkflowTypeCode = "C";
            DatabaseMigrationWizardTestHelper.Workflows[0].AttributeSetNameGuid = Guid.Parse("8dbc6db1-cd76-4329-80f9-74afbc02dd15");
            DatabaseMigrationWizardTestHelper.Workflows[0].EditActionGuid = Guid.Parse("ade5baae-0dae-4452-9679-0da6c9c4bf80");
            DatabaseMigrationWizardTestHelper.Workflows[0].EndActionGuid = Guid.Parse("ade5baae-0dae-4452-9679-0da6c9c4bf80");
            DatabaseMigrationWizardTestHelper.Workflows[0].MetadataFieldGuid = Guid.Parse("3cd22248-9e0f-41f9-8754-b4d0a9bc087d");
            DatabaseMigrationWizardTestHelper.Workflows[0].PostEditActionGuid = Guid.Parse("ade5baae-0dae-4452-9679-0da6c9c4bf80");
            DatabaseMigrationWizardTestHelper.Workflows[0].PostWorkflowActionGuid = Guid.Parse("ade5baae-0dae-4452-9679-0da6c9c4bf80");
            DatabaseMigrationWizardTestHelper.Workflows[0].StartActionGuid = Guid.Parse("ade5baae-0dae-4452-9679-0da6c9c4bf80");
        }
    }
}