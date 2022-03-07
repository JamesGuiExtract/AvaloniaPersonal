﻿using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileSuppliers;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Email.GraphClient.Test.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using System.Linq;
using Microsoft.Graph;

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

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

            var graphTestsConfig = new GraphTestsConfig();

            EmailManagementConfiguration.FileProcessingDB = GetNewAzureDatabase();
            EmailManagementConfiguration.InputMailFolderName = EmailTestHelper.GenerateName(9);
            EmailManagementConfiguration.QueuedMailFolderName = EmailTestHelper.GenerateName(8);
            EmailManagementConfiguration.Password = graphTestsConfig.EmailPassword;
            EmailManagementConfiguration.SharedEmailAddress = graphTestsConfig.SharedEmailAddress;
            EmailManagementConfiguration.UserName = graphTestsConfig.EmailUserName;
            EmailManagementConfiguration.FilepathToDownloadEmails = graphTestsConfig.FolderToSaveEmails;

            EmailManagement = new EmailManagement(EmailManagementConfiguration);
        }

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
            using EmailFileSupplier emailFileSupplier = new(EmailManagementConfiguration);
            using EmailFileSupplier clone = (EmailFileSupplier)emailFileSupplier.Clone();


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
        public static void TestEmailFileSupplierLoadAndSave()
        {
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            using var stream = new MemoryStream();
            var istream = new IStreamWrapper(stream);
            emailFileSupplier.Save(istream, false);

            stream.Position = 0;
            using var loadedFileSupplier = new EmailFileSupplier();
            loadedFileSupplier.Load(istream);

            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails, loadedFileSupplier.EmailManagementConfiguration.FilepathToDownloadEmails);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.InputMailFolderName, loadedFileSupplier.EmailManagementConfiguration.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.Password.Unsecure(), loadedFileSupplier.EmailManagementConfiguration.Password.Unsecure());
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.UserName, loadedFileSupplier.EmailManagementConfiguration.UserName);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.QueuedMailFolderName, loadedFileSupplier.EmailManagementConfiguration.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.EmailManagementConfiguration.SharedEmailAddress, loadedFileSupplier.EmailManagementConfiguration.SharedEmailAddress);
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
            return database;
        }
    }
}