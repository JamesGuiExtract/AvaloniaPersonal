using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileSuppliers;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Utilities.EmailGraphApi.Test.Utilities;
using Microsoft.Graph;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient.Test
{
    [TestFixture]
    [Category("EmailGraphApi")]
    [SingleThreaded]
    public class EmailFileSupplierTests
    {
        private static FAMTestDBManager<GraphTests> FAMTestDBManager;
        private static EmailManagementConfiguration EmailManagementConfiguration;
        private static FileProcessingDB Database;
        private static string SharedEmailAddress;
        private static readonly TestFileManager<GraphTests> TestFileManager = new();
        private static readonly string GetEmailSourceValues = "SELECT * FROM dbo.EmailSource";
        private static GraphTestsConfig GraphTestsConfig;
        private static string TestActionName = "TestAction";
        private static EmailManagement EmailManagement;


        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GraphTestsConfig = new GraphTestsConfig();
            FAMTestDBManager = new FAMTestDBManager<GraphTests>();
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

            GraphTestsConfig.DatabaseName = "Test_EmailFileSupplier";

            Database = FAMTestDBManager.GetNewDatabase("Test_EmailFileSupplier");
            Database.DefineNewAction(TestActionName);

            SharedEmailAddress = GraphTestsConfig.SharedEmailAddress;
            Database.SetDBInfoSetting("AzureClientId", GraphTestsConfig.AzureClientId, true, false);
            Database.SetDBInfoSetting("AzureTenant", GraphTestsConfig.AzureTenantID, true, false);
            Database.SetDBInfoSetting("AzureInstance", GraphTestsConfig.AzureInstance, true, false);

            EmailManagementConfiguration = new()
            {
                FileProcessingDB = Database,
                InputMailFolderName = GenerateName(9),
                QueuedMailFolderName = GenerateName(8),
                Password = GraphTestsConfig.EmailPassword,
                SharedEmailAddress = SharedEmailAddress,
                UserName = GraphTestsConfig.EmailUserName,
                FilepathToDownloadEmails = GraphTestsConfig.FolderToSaveEmails,
            };
            EmailManagement = new EmailManagement(EmailManagementConfiguration);
        }

        [Test]
        public static async Task TestDataEmailSourceTable()
        {
            int messagesToTest = 1;
            var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            ExtractRoleConnection connection = new ExtractRoleConnection(Database.DatabaseServer, Database.DatabaseName);

            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);

            // Delete all the .eml files, and cleanup the mailbox
            DeleteAllEMLFiles(emailFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails);
            await GraphTests.ClearAllMessages(EmailManagement);

            try
            {
                connection.Open();

                // Add new emails for testing.
                await GraphTests.AddInputMessage(EmailManagement, messagesToTest);

                fileProcessingManager.StartProcessing();

                // Give the timer a moment to download emails.
                await Task.Delay(7000);
                fileProcessingManager.StopProcessing();

                var emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                var command = connection.CreateCommand();
                command.CommandText = GetEmailSourceValues;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Assert.IsNotNull(reader["OutlookEmailID"].ToString());
                    Assert.AreEqual("Recipient0@extracttest.com, Test_Recipient0@extracttest.com", reader["Recipients"].ToString());
                    Assert.AreEqual(EmailManagement.EmailManagementConfiguration.SharedEmailAddress, reader["EmailAddress"].ToString());
                    Assert.AreEqual("The cake is a lie0. ", reader["Subject"].ToString());
                    Assert.IsTrue(DateTime.Parse(reader["Received"].ToString()) > DateTime.Now.AddMinutes(-5));
                    Assert.AreEqual("", reader["Sender"].ToString());
                    Assert.AreEqual("1", reader["FAMSessionID"].ToString());
                    Assert.AreEqual("1", reader["QueueEventID"].ToString());
                    Assert.AreEqual("1", reader["FAMFileID"].ToString());
                }
            }
            finally
            {
                // Remove all downloaded emails
                DeleteAllEMLFiles(emailFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails);
                connection.Dispose();
            }
        }

        [Test]
        public static void TestCopyFromFileSupplier()
        {
            EmailFileSupplier emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            EmailFileSupplier copy = new EmailFileSupplier(new EmailManagementConfiguration()
            {
                FilepathToDownloadEmails = "Test",
                InputMailFolderName = "Meh",
                Password = new NetworkCredential("", "lol").SecurePassword,
                UserName = "Yes",
                QueuedMailFolderName = "Yay",
                SharedEmailAddress = "42"
            });


            copy.CopyFrom(emailFileSupplier);

            Assert.AreEqual(emailFileSupplier.DownloadDirectory, copy.DownloadDirectory);
            Assert.AreEqual(emailFileSupplier.InputMailFolderName, copy.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.Password.Unsecure(), copy.Password.Unsecure());
            Assert.AreEqual(emailFileSupplier.UserName, copy.UserName);
            Assert.AreEqual(emailFileSupplier.QueuedMailFolderName, copy.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.SharedEmailAddress, copy.SharedEmailAddress);

            // Verify clone: Changing the password on the copy should _not_ modify the original
            copy.EmailManagementConfiguration.Password.Clear();
            Assert.AreNotEqual(emailFileSupplier.Password.Unsecure(), copy.Password.Unsecure());
        }

        [Test]
        public static void TestCloneFileSupplier()
        {
            EmailFileSupplier emailFileSupplier = new(EmailManagementConfiguration);

            EmailFileSupplier clone = (EmailFileSupplier)emailFileSupplier.Clone();


            Assert.AreEqual(emailFileSupplier.DownloadDirectory, clone.DownloadDirectory);
            Assert.AreEqual(emailFileSupplier.InputMailFolderName, clone.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.Password.Unsecure(), clone.Password.Unsecure());
            Assert.AreEqual(emailFileSupplier.UserName, clone.UserName);
            Assert.AreEqual(emailFileSupplier.QueuedMailFolderName, clone.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.SharedEmailAddress, clone.SharedEmailAddress);

            // Verify clone: Changing the password on the copy should _not_ modify the original
            clone.EmailManagementConfiguration.Password.Clear();
            Assert.AreNotEqual(emailFileSupplier.Password.Unsecure(), clone.Password.Unsecure());
        }

        [Test]
        public static void TestSaveAndLoadFileSupplier()
        {
            var stream = new MemoryStream();
            var istream = new IStreamWrapper(stream);
            var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            emailFileSupplier.Save(istream, false);

            stream.Position = 0;
            var loadedFileSupplier = new EmailFileSupplier();
            loadedFileSupplier.Load(istream);

            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails, loadedFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.InputMailFolderName, loadedFileSupplier.EmailManagementConfiguration.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.Password.Unsecure(), loadedFileSupplier.EmailManagementConfiguration.Password.Unsecure());
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.UserName, loadedFileSupplier.EmailManagementConfiguration.UserName);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.QueuedMailFolderName, loadedFileSupplier.EmailManagementConfiguration.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.SharedEmailAddress, loadedFileSupplier.EmailManagementConfiguration.SharedEmailAddress);
        }

        [Test]
        public static void TestStartEmailFileSupplier()
        {
            var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);

            // Ensure the task can be started, stoped, paused and resumed.
            fileProcessingManager.StartProcessing();
            Thread.Sleep(1000);
            fileProcessingManager.PauseProcessing();
            fileProcessingManager.StartProcessing();
            fileProcessingManager.StopProcessing();
        }

        [Test]
        public static async Task TestEmailDownloadAndQueueFileSupplier()
        {
            int messagesToTest = 15;
            var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);

            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);

            try
            {
                await GraphTests.ClearAllMessages(EmailManagement);

                // Add new emails for testing.
                await GraphTests.AddInputMessage(EmailManagement, messagesToTest);
                DeleteAllEMLFiles(emailFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails);

                fileProcessingManager.StartProcessing();

                // Give the thread a moment to download emails.
                await Task.Delay(15000);

                var emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                // Stop processing, add another message, make sure the service can start again.
                fileProcessingManager.StopProcessing();
                await GraphTests.AddInputMessage(EmailManagement, 1, "StopStart");
                fileProcessingManager.StartProcessing();

                // Give the timer a moment to download emails.
                await Task.Delay(10000);

                emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest + 1, emlFilesOnDisk.Length);

                // Pause processing, add an email, and ensure it can resume.
                fileProcessingManager.PauseProcessing();
                await GraphTests.AddInputMessage(EmailManagement, 1, "PauseResume");
                fileProcessingManager.StartProcessing();

                // Give the thread a moment to download emails.
                await Task.Delay(10000);

                emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest + 2, emlFilesOnDisk.Length);
            }
            finally
            {
                // Remove all downloaded emails
                DeleteAllEMLFiles(emailFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails);
            }
        }

        [Test]
        public static void TestConfiguredFileSupplier()
        {
            var config = new GraphTestsConfig();
            EmailFileSupplier emailFileSupplier = new(new EmailManagementConfiguration()
            {
                FilepathToDownloadEmails = config.FolderToSaveEmails,
                InputMailFolderName = "Inbox",
                Password = config.EmailPassword,
                UserName = config.EmailUserName,
                QueuedMailFolderName = "Inbox",
                SharedEmailAddress = config.SharedEmailAddress
            });

            // All values above are populated so this should be a valid config
            Assert.IsTrue(emailFileSupplier.IsConfigured());

            // Username cannot be empty
            emailFileSupplier.EmailManagementConfiguration.UserName = string.Empty;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Password cannot be empty
            emailFileSupplier.EmailManagementConfiguration.UserName = config.EmailUserName;
            emailFileSupplier.EmailManagementConfiguration.Password = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Shared email address cannot be empty
            emailFileSupplier.EmailManagementConfiguration.Password = config.EmailPassword;
            emailFileSupplier.EmailManagementConfiguration.SharedEmailAddress = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Input mail folder cannot be empty
            emailFileSupplier.EmailManagementConfiguration.SharedEmailAddress = config.SharedEmailAddress;
            emailFileSupplier.EmailManagementConfiguration.InputMailFolderName = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Queued mail folder cannot be empty
            emailFileSupplier.EmailManagementConfiguration.InputMailFolderName = "Inbox";
            emailFileSupplier.EmailManagementConfiguration.QueuedMailFolderName = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Download file path cannot be empty
            emailFileSupplier.EmailManagementConfiguration.QueuedMailFolderName = "Inbox";
            emailFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());
        }

        [OneTimeTearDown]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:Compound words should be cased correctly", Justification = "Nunit name")]
        public static void TearDown()
        {
            GraphTests.DeleteMailFolder(EmailManagementConfiguration.QueuedMailFolderName, EmailManagement);
            GraphTests.DeleteMailFolder(EmailManagementConfiguration.InputMailFolderName, EmailManagement);
            FAMTestDBManager?.Dispose();
            TestFileManager?.Dispose();
            System.IO.Directory.Delete(EmailManagement.EmailManagementConfiguration.FilepathToDownloadEmails, true);
        }

        private static void DeleteAllEMLFiles(string directory)
        {
            foreach (string sFile in System.IO.Directory.GetFiles(directory, "*.eml"))
            {
                System.IO.File.Delete(sFile);
            }
        }

        private static IFileProcessingManager CreateFileSupplierFAM(IFileSupplier fileSupplier)
        {
            var fpManager = new FileProcessingManagerClass
            {
                DatabaseServer = Database.DatabaseServer,
                DatabaseName = Database.DatabaseName,
                ActionName = TestActionName
            };
            ((IFileActionMgmtRole)fpManager.FileSupplyingMgmtRole).Enabled = true;
            fpManager.FileSupplyingMgmtRole.FileSuppliers.PushBack(new FileSupplierDataClass
            {
                FileSupplier = new ObjectWithDescriptionClass
                {
                    Object = fileSupplier,
                    Enabled = true
                },
                ForceProcessing = true,
            });
            return fpManager;
        }

        /// <summary>
        /// Generates a random name.
        /// Code taken from: https://stackoverflow.com/questions/14687658/random-name-generator-in-c-sharp
        /// </summary>
        /// <param name="len">How long you want the name to be.</param>
        /// <returns>Returns the random name.</returns>
        public static string GenerateName(int len)
        {
            Random r = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper(CultureInfo.InvariantCulture);
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }

            return Name;
        }
    }
}
