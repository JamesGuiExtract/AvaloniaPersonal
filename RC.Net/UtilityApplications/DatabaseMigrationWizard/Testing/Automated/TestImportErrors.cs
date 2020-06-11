using DatabaseMigrationWizard.Database.Input;
using Extract;
using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using NUnit.Framework;
using System;
using System.IO;

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
            var database = FamTestDbManager.GetNewDatabase("EnsureRollback");
            ImportOptions ImportOptions = new ImportOptions()
            {
                ClearDatabase = false,
                ImportPath = Path.GetTempPath() + $"EnsureRollback\\",
                ConnectionInformation = new Database.ConnectionInformation() { DatabaseName = "EnsureRollback", DatabaseServer = "(local)" }
            };

            Directory.CreateDirectory(ImportOptions.ImportPath);
            var databaseMigrationWizardTestHelper = new DatabaseMigrationWizardTestHelper();
            databaseMigrationWizardTestHelper.LoadInitialValues();
            databaseMigrationWizardTestHelper.Actions.Add(new Database.Input.DataTransformObject.Action() { ActionGuid = Guid.Parse("1c317bec-bfc3-4b7a-b2f3-0a4eb8ffe173"), ASCName = "SameNameAction" });
            databaseMigrationWizardTestHelper.Actions.Add(new Database.Input.DataTransformObject.Action() { ActionGuid = Guid.Parse("56f6ccc6-da61-483c-bfd7-0c80af951bca"), ASCName = "SameNameAction" });
            databaseMigrationWizardTestHelper.WriteEverythingToDirectory(ImportOptions.ImportPath);
            try
            {
                new ImportHelper(ImportOptions, new Progress<string>((garbage) => { })).Import();
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
                FamTestDbManager.RemoveDatabase("EnsureRollback");
                Directory.Delete(ImportOptions.ImportPath, true);
            }
        }
    }
}