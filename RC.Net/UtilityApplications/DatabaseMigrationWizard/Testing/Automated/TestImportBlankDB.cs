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

namespace DatabaseMigrationWizard.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="DataEntryQuery"/> class.
    /// </summary>
    [TestFixture]
    [Category("DatabaseMigrationWizardImports")]
    public static class TestImportBlankDB
    {
        private static readonly FAMTestDBManager<TestExports> FamTestDbManager = new FAMTestDBManager<TestExports>();

        private static readonly string DatabaseName = "Test_ImportBlankDB";

        private static ImportOptions ImportOptions;

        private static DatabaseMigrationWizardTestHelper DatabaseMigrationWizardTestHelper;

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            FamTestDbManager.GetNewDatabase(DatabaseName);

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

            ImportHelper importHelper = new ImportHelper(ImportOptions, new Progress<string>((garbage) => { }));
            importHelper.Import();
            importHelper.CommitTransaction();
        }

        /// <summary>
        /// TearDown method to destroy testing environment.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Nunit made me")]
        [OneTimeTearDown]
        public static void TearDown()
        {
            ImportOptions.Dispose();
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
        public static void WebAPIConfiguration()
        {
            var WebAPIConfigurationFromDB = JsonConvert.DeserializeObject<List<WebAPIConfiguration>>(BuildAndWriteTable(new SerializeWebAPIConfiguration()).ToString());

            foreach (var webAPIConfiguration in DatabaseMigrationWizardTestHelper.WebAPIConfigurations)
            {
                Assert.IsTrue(WebAPIConfigurationFromDB.Where(m => m.Equals(webAPIConfiguration)).Any());
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

            using (var connection = new ExtractRoleConnection("(local)", DatabaseName))
            {
                connection.Open();
                serialize.SerializeTable(connection, stringWriter);
            }

            return stringWriter;
        }
    }
}