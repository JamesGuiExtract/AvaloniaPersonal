using Extract.Email.GraphClient.Test.Utilities;
using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileSuppliers;
using Extract.Interop;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Microsoft.Graph;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient.Test
{
    [TestFixture]
    [Category("EmailGraphApi")]
    [NonParallelizable]
    public class EmailFileSupplierTests
    {
        private static FAMTestDBManager<GraphTests> FAMTestDBManager = new();
        private static EmailManagementConfiguration EmailManagementConfiguration = new();
        private static readonly TestFileManager<GraphTests> TestFileManager = new();
        private static readonly string GetEmailSourceValues = "SELECT * FROM dbo.EmailSource";
        private static EmailManagement EmailManagement;
        private static readonly string TestActionName = "TestAction";

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            var graphTestsConfig = new GraphTestsConfig();

            EmailManagementConfiguration.ExternalLoginDescription = Constants.EmailFileSupplierExternalLoginDescription;
            EmailManagementConfiguration.FileProcessingDB = GetNewAzureDatabase();
            EmailManagementConfiguration.InputMailFolderName = EmailTestHelper.GenerateName(9);
            EmailManagementConfiguration.QueuedMailFolderName = EmailTestHelper.GenerateName(8);
            EmailManagementConfiguration.SharedEmailAddress = graphTestsConfig.SharedEmailAddress;
            EmailManagementConfiguration.FilepathToDownloadEmails = graphTestsConfig.FolderToSaveEmails;

            EmailManagement = new EmailManagement(EmailManagementConfiguration);
        }

        /// <summary>
        /// Cleanup after all tests have run
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            try
            {
                EmailTestHelper.DeleteMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName, EmailManagement).Wait();
                EmailTestHelper.DeleteMailFolder(EmailManagement.EmailManagementConfiguration.InputMailFolderName, EmailManagement).Wait();
            }
            finally
            {
                FAMTestDBManager?.Dispose();
                TestFileManager?.Dispose();
                System.IO.Directory.Delete(EmailManagement.EmailManagementConfiguration.FilepathToDownloadEmails, true);
            }
        }

        #endregion Overhead

        [Test]
        public static async Task TestEmailSourceTable()
        {
            await EmailTestHelper.CleanupTests(EmailManagement);

            int messagesToTest = 1;
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);
            using var connection = new ExtractRoleConnection(EmailManagementConfiguration.FileProcessingDB.DatabaseServer, EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                // Add new emails for testing.
                await EmailTestHelper.AddInputMessage(EmailManagement, messagesToTest);

                fileProcessingManager.StartProcessing();

                // Give the timer a moment to download emails.
                await Task.Delay(5000);

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
                await EmailTestHelper.CleanupTests(EmailManagement);
            }
        }

        [Test]
        public static async Task TestEmailSourceTableBlankSubject()
        {
            await EmailTestHelper.CleanupTests(EmailManagement);

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);
            using var connection = new ExtractRoleConnection(EmailManagementConfiguration.FileProcessingDB.DatabaseServer, EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                // Add new emails for testing.
                await EmailTestHelper.AddInputMessageBlankSubject(EmailManagement);

                fileProcessingManager.StartProcessing();

                // Give the timer a moment to download emails.
                await Task.Delay(5000);

                var emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(1, emlFilesOnDisk.Length);

                var command = connection.CreateCommand();
                command.CommandText = GetEmailSourceValues;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Assert.IsNotNull(reader["OutlookEmailID"].ToString());
                    Assert.AreEqual("Recipient@extracttest.com, Test_Recipient@extracttest.com", reader["Recipients"].ToString());
                    Assert.AreEqual(EmailManagement.EmailManagementConfiguration.SharedEmailAddress, reader["EmailAddress"].ToString());
                    Assert.AreEqual(string.Empty, reader["Subject"].ToString());
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
                await EmailTestHelper.CleanupTests(EmailManagement);
                fileProcessingManager.StopProcessing();
            }
        }

        [Test]
        public static async Task TestEmailSourceTableForceProcessing()
        {
            int messagesToTest = 5;
            await EmailTestHelper.CleanupTests(EmailManagement);

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier, true);
            using var connection = new ExtractRoleConnection(EmailManagementConfiguration.FileProcessingDB.DatabaseServer, EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                // Add new emails for testing.
                await EmailTestHelper.AddInputMessage(EmailManagement, messagesToTest);

                // Process all messages normally one time around.
                fileProcessingManager.StartProcessing();

                // Give the timer a moment to download emails.
                await Task.Delay(5000);

                var emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                fileProcessingManager.PauseProcessing();

                // Ensure all files were set to pending.
                for(int i = 1; i <= messagesToTest; i++)
                {
                    var status = EmailManagementConfiguration.FileProcessingDB.GetFileStatus(i, TestActionName, false);
                    Assert.AreEqual(EActionStatus.kActionPending, status);
                }

                // Fail all the files just to get them out of the queued status.
                for (int i = 1; i <= messagesToTest; i++)
                {
                    EmailManagementConfiguration.FileProcessingDB.NotifyFileFailed(i, TestActionName, -1, "Not really", true);
                }
                await MoveAllMessagesToInputFolder();

                // Process all messages again. Note this file supplier was built with force processing = true
                fileProcessingManager.StartProcessing();

                // Give the timer a moment to download emails.
                await Task.Delay(5000);

                // They should have used the same name, so there should be no additional files.
                emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                // Ensure all files were set to pending.
                for (int i = 1; i <= messagesToTest; i++)
                {
                    var status = EmailManagementConfiguration.FileProcessingDB.GetFileStatus(i, TestActionName, false);
                    Assert.AreEqual(EActionStatus.kActionPending, status);
                }
            }
            finally
            {
                // Remove all downloaded emails
                await EmailTestHelper.CleanupTests(EmailManagement);
                fileProcessingManager.StopProcessing();
            }
        }

        [Test]
        public static void TestCopyFromFileSupplier()
        {
            using EmailFileSupplier emailFileSupplier = new(EmailManagementConfiguration);
            using EmailFileSupplier copy = new(new EmailManagementConfiguration()
            {
                FilepathToDownloadEmails = "Test",
                InputMailFolderName = "Meh",
                QueuedMailFolderName = "Yay",
                SharedEmailAddress = "42"
            });


            copy.CopyFrom(emailFileSupplier);

            Assert.AreEqual(emailFileSupplier.DownloadDirectory, copy.DownloadDirectory);
            Assert.AreEqual(emailFileSupplier.InputMailFolderName, copy.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.QueuedMailFolderName, copy.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.SharedEmailAddress, copy.SharedEmailAddress);
        }

        [Test]
        public static void TestCloneFileSupplier()
        {
            using EmailFileSupplier emailFileSupplier = new(EmailManagementConfiguration);
            using EmailFileSupplier clone = (EmailFileSupplier)emailFileSupplier.Clone();

            Assert.AreEqual(emailFileSupplier.DownloadDirectory, clone.DownloadDirectory);
            Assert.AreEqual(emailFileSupplier.InputMailFolderName, clone.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.QueuedMailFolderName, clone.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.SharedEmailAddress, clone.SharedEmailAddress);
        }

        [Test]
        public static void TestEmailFileSupplierLoadAndSave()
        {
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            using var stream = new MemoryStream();
            var istream = new IStreamWrapper(stream);
            emailFileSupplier.Save(istream, false);

            stream.Position = 0;
            using var loadedFileSupplier = new EmailFileSupplier();
            loadedFileSupplier.Load(istream);

            Assert.AreEqual(emailFileSupplier.DownloadDirectory, loadedFileSupplier.DownloadDirectory);
            Assert.AreEqual(emailFileSupplier.InputMailFolderName, loadedFileSupplier.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.QueuedMailFolderName, loadedFileSupplier.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.SharedEmailAddress, loadedFileSupplier.SharedEmailAddress);
        }

        [Test]
        public static void TestEmailFileSupplierStart()
        {
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);

            // Ensure the task can be started, stoped, paused and resumed.
            fileProcessingManager.StartProcessing();
            fileProcessingManager.PauseProcessing();
            fileProcessingManager.StartProcessing();
            fileProcessingManager.StopProcessing();
            emailFileSupplier.Dispose();
        }

        [Test]
        public static async Task TestEmailDownloadAndQueueFileSupplier()
        {
            await EmailTestHelper.CleanupTests(EmailManagement);

            int messagesToTest = 15;
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);

            try
            {
                // Add new emails for testing.
                await EmailTestHelper.AddInputMessage(EmailManagement, messagesToTest);

                fileProcessingManager.StartProcessing();

                // Give the thread a moment to download emails.
                await Task.Delay(10000);

                var emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                // Stop processing, add another message, make sure the service can start again.
                fileProcessingManager.StopProcessing();
                await EmailTestHelper.AddInputMessage(EmailManagement, 1, "StopStart");

                // For some reason the COM framework still thinks its processing if this fires too fast. Likey due to the Active FAM not being cleared fast enough.
                await Task.Delay(5000);
                fileProcessingManager.StartProcessing();

                // Give the timer a moment to download emails.
                await Task.Delay(6000);

                emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest + 1, emlFilesOnDisk.Length);

                // Pause processing, add an email, and ensure it can resume.
                fileProcessingManager.PauseProcessing();
                await EmailTestHelper.AddInputMessage(EmailManagement, 1, "PauseResume");
                fileProcessingManager.StartProcessing();

                // Give the thread a moment to download emails.
                await Task.Delay(6000);

                emlFilesOnDisk = System.IO.Directory.GetFiles(EmailManagementConfiguration.FilepathToDownloadEmails, "*.eml");
                Assert.AreEqual(messagesToTest + 2, emlFilesOnDisk.Length);
            }
            finally
            {
                await EmailTestHelper.CleanupTests(EmailManagement);
                fileProcessingManager.StopProcessing();
            }
        }

        [Test]
        public static void TestConfiguredFileSupplier()
        {
            var config = new GraphTestsConfig();
            using EmailFileSupplier emailFileSupplier = new(new EmailManagementConfiguration()
            {
                FilepathToDownloadEmails = config.FolderToSaveEmails,
                InputMailFolderName = "Inbox",
                QueuedMailFolderName = "Inbox",
                SharedEmailAddress = config.SharedEmailAddress
            });

            // All values above are populated so this should be a valid config
            Assert.IsTrue(emailFileSupplier.IsConfigured());

            // Shared email address cannot be empty
            emailFileSupplier.SharedEmailAddress = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Input mail folder cannot be empty
            emailFileSupplier.SharedEmailAddress = config.SharedEmailAddress;
            emailFileSupplier.InputMailFolderName = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Queued mail folder cannot be empty
            emailFileSupplier.InputMailFolderName = "Inbox";
            emailFileSupplier.QueuedMailFolderName = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());

            // Download file path cannot be empty
            emailFileSupplier.QueuedMailFolderName = "Inbox";
            emailFileSupplier.DownloadDirectory = null;
            Assert.IsFalse(emailFileSupplier.IsConfigured());
        }

        /// <summary>
        /// Confirm that tags/functions in property values get expanded
        /// </summary>
        [Test]
        public static void TestPathTagExpansion()
        {
            // Arrange
            EmailManagementConfiguration sourceConfig = new()
            {
                FilepathToDownloadEmails = @"D:\$FileOf(<EmailDownloadDirectory>)",
                InputMailFolderName = "<EmailInboxFolder>",
                QueuedMailFolderName = "<EmailQueuedMailFolder>",
                SharedEmailAddress = "<EmailAddress>"
            };

            FAMTagManagerClass tagManager = new();
            tagManager.AddTag("EmailDownloadDirectory", @"\\server\Share\FileFolder");
            tagManager.AddTag("EmailInboxFolder", "The inbox!");
            tagManager.AddTag("EmailQueuedMailFolder", "The post-download folder");
            tagManager.AddTag("EmailAddress", "The shared email address");

            // Save the config object that gets passed to the IEmailManagement creator function
            EmailManagementConfiguration expandedConfig = null;
            IEmailManagement CreateEmailManagement(EmailManagementConfiguration config)
            {
                expandedConfig = config;
                return new Mock<IEmailManagement>().Object;
            }

            using EmailFileSupplier emailFileSupplier = new(sourceConfig, CreateEmailManagement);

            // Act
            // Start/stop the supplier so that the IEmailManagement creator function is called with the expanded configuration
            emailFileSupplier.Start(new Mock<IFileSupplierTarget>().Object, tagManager, new Mock<FileProcessingDB>().Object, 1);
            emailFileSupplier.Stop();

            // Assert
            // Confirm that the paths are expanded correctly
            Assert.AreEqual(@"D:\FileFolder", expandedConfig.FilepathToDownloadEmails);
            Assert.AreEqual("The inbox!", expandedConfig.InputMailFolderName);
            Assert.AreEqual("The post-download folder", expandedConfig.QueuedMailFolderName);
            Assert.AreEqual("The shared email address", expandedConfig.SharedEmailAddress);
        }

        #region Helper Methods

        /// <summary>
        /// Moves all of the messages from the queued folder to the input folder.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        private static async Task MoveAllMessagesToInputFolder()
        {
            bool findingNewMessages = true;
            var inputMailFolderID = await EmailManagement.GetInputMailFolderID().ConfigureAwait(false);
            var queuedMailFolderID = await EmailManagement.GetQueuedFolderID().ConfigureAwait(false);
            while (findingNewMessages)
            {
                var messageCollection = (await EmailManagement
                    .GraphServiceClient
                    .Users[EmailManagementConfiguration.SharedEmailAddress]
                    .MailFolders[queuedMailFolderID]
                    .Messages
                    .Request()
                    .Top(999)
                    .GetAsync().ConfigureAwait(false)).ToArray();

                if (messageCollection.Length == 0)
                    findingNewMessages = false;

                foreach (var message in messageCollection)
                {
                    await EmailManagement.GraphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress]
                    .Messages[message.Id]
                    .Move(inputMailFolderID)
                    .Request()
                    .Header("Prefer", "IdType=\"ImmutableId\"")
                    .Select("Id")
                    .PostAsync()
                    .ConfigureAwait(false);
                }
            }
        }

        private static IFileProcessingManager CreateFileSupplierFAM(IFileSupplier fileSupplier, bool forceProcessing = false)
        {
            EmailManagementConfiguration.FileProcessingDB = GetNewAzureDatabase();

            var fpManager = new FileProcessingManagerClass
            {
                DatabaseServer = EmailManagementConfiguration.FileProcessingDB.DatabaseServer,
                DatabaseName = EmailManagementConfiguration.FileProcessingDB.DatabaseName,
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
                ForceProcessing = forceProcessing,
            });
            return fpManager;
        }

        private static FileProcessingDB GetNewAzureDatabase()
        {
            var graphTestsConfig = new GraphTestsConfig();
            var database = FAMTestDBManager.GetNewDatabase($"Test_EmailFileSupplier{Guid.NewGuid()}");
            database.DefineNewAction(TestActionName);
            database.SetDBInfoSetting("AzureClientId", graphTestsConfig.AzureClientId, true, false);
            database.SetDBInfoSetting("AzureTenant", graphTestsConfig.AzureTenantID, true, false);
            database.SetDBInfoSetting("AzureInstance", graphTestsConfig.AzureInstance, true, false);

            database.LoginUser("admin", "a");
            database.SetExternalLogin(
                Constants.EmailFileSupplierExternalLoginDescription,
                graphTestsConfig.EmailUserName,
                graphTestsConfig.EmailPassword);

            return database;
        }

        #endregion Helper Methods
    }
}
