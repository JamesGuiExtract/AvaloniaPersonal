using Extract;
using Extract.Database;
using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="DataEntryQuery"/> class.
    /// </summary>
    [TestFixture]
    [Category("TestLaunchArguments")]
    public class TestLaunchArguments
    {
        private static readonly FAMTestDBManager<TestLaunchArguments> FamTestDbManager = new FAMTestDBManager<TestLaunchArguments>();
        private static string EmbeddedJsonFilePath;

        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            EmbeddedJsonFilePath = LoadJsonFilesFromAssembly();
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            string[] files = Directory.GetFiles(EmbeddedJsonFilePath + "DatabaseExportWithLABDE\\");
            foreach (var file in files)
            {
                File.Delete(file);
            }
            files = Directory.GetFiles(EmbeddedJsonFilePath + "DatabaseExportNoLABDE\\");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        [Test, Category("Automated")]
        public static void TestPasswordArgumentNoPassword()
        {
            try
            {
                List<string> arguments = new List<string>();
                arguments.Add("/Password");

                Assert.Throws<ExtractException>(() => new StartupConfigurator().Start(arguments.ToArray()));
            }
            finally
            {
                FamTestDbManager.RemoveDatabase("TestPasswordArgumentNoPassword");
            }
        }

        [Test, Category("Automated")]
        public static void TestPasswordArgumentNoConnectionInformation()
        {
            try
            {
                var database = FamTestDbManager.GetNewDatabase("TestPasswordArgumentNoConnectionInformation");
                database.LoginUser("admin", "a");
                List<string> arguments = new List<string>();
                arguments.Add("/Password");
                arguments.Add(database.GetOneTimePassword());

                Assert.Throws<ExtractException>(() => new StartupConfigurator().Start(arguments.ToArray()));
            }
            finally
            {
                FamTestDbManager.RemoveDatabase("TestPasswordArgumentNoConnectionInformation");
            }
        }

        [Test, Category("Automated")]
        public static void TestInvalidConnectionInformation()
        {
            string path = Path.GetTempPath() + "EFArgumentTest.uex";
            List<string> arguments = new List<string>();
            arguments.Add("/EF");
            arguments.Add(path);
            arguments.Add("/DatabaseName");
            // I hope this database name does not exist =)
            arguments.Add(Guid.NewGuid().ToString());
            arguments.Add("/DatabaseServer");
            arguments.Add("(local)");
            arguments.Add("/Password");
            arguments.Add("a");

            new StartupConfigurator().Start(arguments.ToArray());
            Assert.IsTrue(File.Exists(path));
            File.Delete(path);
        }

        [Test, Category("Automated")]
        public static void TestBothImportAndExportArgumentSpecified()
        {
            List<string> arguments = new List<string>();
            arguments.Add("/Import");
            arguments.Add("/Export");

            Assert.Throws<ExtractException>(() => new StartupConfigurator().Start(arguments.ToArray()));
        }

        [Test, Category("Automated")]
        public static void EFArgument()
        {
            string path = Path.GetTempPath() + "EFArgumentTest.uex";
            List<string> arguments = new List<string>();
            arguments.Add("/EF");
            arguments.Add(path);
            arguments.Add("/Password");

            // This should log the error to the exception file.
            new StartupConfigurator().Start(arguments.ToArray());
            Assert.IsTrue(File.Exists(path));
            File.Delete(path);
        }

        [Test, Category("Automated")]
        public static void CreateDatabaseArgumentNoPath()
        {
            List<string> arguments = new List<string>();
            arguments.Add("/CreateDatabase");
            arguments.Add("/DatabaseServer");
            arguments.Add("(local)");
            arguments.Add("/DatabaseName");
            arguments.Add("Turtle");

            Assert.Throws<ExtractException>(() => new StartupConfigurator().Start(arguments.ToArray()));
        }

        [Test, Category("Automated")]
        public static void CreateDatabaseArgumentNoLabDE()
        {
            string databaseName = "CreateDatabaseArgumentNoLabDE";
            try
            {
                List<string> arguments = new List<string>();
                arguments.Add("/CreateDatabase");
                arguments.Add("/DatabaseServer");
                arguments.Add("(local)");
                arguments.Add("/DatabaseName");
                arguments.Add(databaseName);
                arguments.Add("/path");
                arguments.Add(EmbeddedJsonFilePath + @"DatabaseExportNoLABDE");

                new StartupConfigurator().Start(arguments.ToArray());

                // If you can Get tag names, I am assuming the database has been created. Specific import tests are handled in other unit tests.
                Assert.DoesNotThrow(() =>
                {
                    new FileProcessingDB()
                    {
                        DatabaseServer = "(local)",
                        DatabaseName = databaseName
                    }.GetTagNames();
                });
            }
            finally
            {
                DBMethods.DropLocalDB(databaseName);
            }
        }

        [Test, Category("Automated")]
        public static void CreateDatabaseArgumentWithLabDE()
        {
            string databaseName = "CreateDatabaseArgumentWithLabDE";
            try
            {
                List<string> arguments = new List<string>();
                arguments.Add("/CreateDatabase");
                arguments.Add("/DatabaseServer");
                arguments.Add("(local)");
                arguments.Add("/DatabaseName");
                arguments.Add(databaseName);
                arguments.Add("/path");
                arguments.Add(EmbeddedJsonFilePath + @"DatabaseExportWithLABDE");

                new StartupConfigurator().Start(arguments.ToArray());

                // If you can Get tag names, I am assuming the database has been created. Specific import tests are handled in other unit tests.
                Assert.DoesNotThrow(() =>
                {
                    new FileProcessingDB()
                    {
                        DatabaseServer = "(local)",
                        DatabaseName = databaseName
                    }.GetTagNames();
                });
            }
            finally
            {
                DBMethods.DropLocalDB(databaseName);
            }
        }

        private static string LoadJsonFilesFromAssembly()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var tempFilePath = Path.GetTempPath();

            string[] files = assembly.GetManifestResourceNames();
            foreach(var file in files)
            {
                using Stream stream = assembly.GetManifestResourceStream(file);
                string fileName;
                Directory.CreateDirectory(tempFilePath + "DatabaseExportNoLABDE");
                Directory.CreateDirectory(tempFilePath + "DatabaseExportWithLABDE");
                if (file.Contains("NoLABDE"))
                {
                    fileName = "DatabaseExportNoLABDE\\" + file.Replace("DatabaseMigrationWizard.Test.DatabaseExportNoLABDE.", string.Empty);
                }
                else
                {
                    fileName = "DatabaseExportWithLABDE\\" + file.Replace("DatabaseMigrationWizard.Test.DatabaseExportWithLABDE.", string.Empty);
                }

                using var fileStream = File.Create(tempFilePath + fileName);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            return tempFilePath;
        }
    }
}