using Extract.Email.GraphClient.Test.Utilities;
using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileSuppliers;
using Extract.Interop;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Microsoft.Graph;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
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
        static FAMTestDBManager<GraphTests> FAMTestDBManager = new();
        static EmailManagementConfiguration EmailManagementConfiguration = new();
        static readonly TestFileManager<GraphTests> TestFileManager = new();
        static readonly string GetEmailSourceValues =
            "SELECT * FROM dbo.EmailSource JOIN dbo.FAMFile ON FAMFileID = dbo.FAMFile.ID";
        static EmailManagement EmailManagement;
        static readonly string TestActionName = "TestAction";

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static async Task Setup()
        {
            GeneralMethods.TestSetup();

            var graphTestsConfig = new GraphTestsConfig();

            EmailManagementConfiguration.ExternalLoginDescription = Constants.EmailFileSupplierExternalLoginDescription;
            EmailManagementConfiguration.FileProcessingDB = GetNewAzureDatabase();
            EmailManagementConfiguration.InputMailFolderName = UtilityMethods.GetRandomString(9, true, true, false);
            EmailManagementConfiguration.QueuedMailFolderName = UtilityMethods.GetRandomString(8, true, true, false);
            EmailManagementConfiguration.FailedMailFolderName = UtilityMethods.GetRandomString(10, true, true, false);
            EmailManagementConfiguration.SharedEmailAddress = graphTestsConfig.SharedEmailAddress;
            EmailManagementConfiguration.FilePathToDownloadEmails = graphTestsConfig.FolderToSaveEmails;

            EmailManagement = await EmailManagement.CreateEmailManagementAsync(EmailManagementConfiguration);
        }

        /// <summary>
        /// Cleanup after all tests have run
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            try
            {
                EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.QueuedMailFolderName, EmailManagement).Wait();
                EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.InputMailFolderName, EmailManagement).Wait();
                EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.FailedMailFolderName, EmailManagement).Wait();
            }
            finally
            {
                FAMTestDBManager?.Dispose();
                TestFileManager?.Dispose();
                System.IO.Directory.Delete(EmailManagement.Configuration.FilePathToDownloadEmails, true);
            }
        }

        #endregion Overhead

        /// <summary>
        /// Test that the EmailSource table is populated correctly
        /// </summary>
        [Test]
        public static async Task TestEmailSourceTable()
        {
            await EmailTestHelper.CleanupTests(EmailManagement);

            // Confirm that records in the email source table have the original subject,
            // even if it has an invalid path char or if the subject * received time pair isn't unique
            // https://extract.atlassian.net/browse/ISSUE-18170
            string[] subjects = new[]
            {
                "A boring subject",
                "RE: not so boring anymore",
                "RE: not so boring anymore",
                "RE: not so boring anymore",
                "RE: not so boring anymore",
                "RE: not so boring anymore"
            };

            int messagesToTest = subjects.Length;
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);
            using var connection = new ExtractRoleConnection(EmailManagementConfiguration.FileProcessingDB.DatabaseServer, EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                // Add new emails for testing.
                string inputMailFolderID = await EmailManagement.GetMailFolderID(EmailManagementConfiguration.InputMailFolderName);
                foreach (string subject in subjects)
                {
                    await EmailTestHelper.AddInputMessage(EmailManagement, inputMailFolderID, subject);
                }

                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                // Stop processing to avoid logged exceptions
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();

                using var command = connection.CreateCommand();
                command.CommandText = GetEmailSourceValues;
                using var reader = command.ExecuteReader();

                void AddValueToDictionary(Dictionary<string, string> dict, SqlDataReader reader, string field)
                {
                    dict.Add(field, reader[field].ToString());
                }

                var filePathToEmailSourceValues = new Dictionary<string, Dictionary<string, string>>();
                while (reader.Read())
                {
                    string filePath = reader["FileName"] as string;
                    Dictionary<string, string> emailSourceValues = new Dictionary<string, string>();
                    filePathToEmailSourceValues.Add(filePath, emailSourceValues);

                    AddValueToDictionary(emailSourceValues, reader, "OutlookEmailID");
                    AddValueToDictionary(emailSourceValues, reader, "Recipients");
                    AddValueToDictionary(emailSourceValues, reader, "EmailAddress");
                    AddValueToDictionary(emailSourceValues, reader, "Subject");
                    string receivedTime = reader.GetDateTimeOffset(reader.GetOrdinal("Received")).ToString();
                    emailSourceValues.Add("Received", receivedTime);
                    AddValueToDictionary(emailSourceValues, reader, "Sender");
                    AddValueToDictionary(emailSourceValues, reader, "FAMSessionID");
                    AddValueToDictionary(emailSourceValues, reader, "QueueEventID");
                    AddValueToDictionary(emailSourceValues, reader, "FAMFileID");
                }

                var emlFilesOnDisk = GetDownloadedEmails();
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                    List<string> actualSubjects = new();

                    foreach (string filePath in emlFilesOnDisk)
                    {
                        Assert.That(filePathToEmailSourceValues.ContainsKey(filePath));

                        var emailSourceValues = filePathToEmailSourceValues[filePath];
                        actualSubjects.Add(emailSourceValues["Subject"]);

                        Assert.IsNotNull(emailSourceValues["OutlookEmailID"], "OutlookEmailID should not be null");

                        Assert.AreEqual(
                            "Recipient@extracttest.com, Test_Recipient@extracttest.com",
                            emailSourceValues["Recipients"],
                            "Recipient is incorrect");

                        Assert.AreEqual(
                            EmailManagementConfiguration.SharedEmailAddress,
                            emailSourceValues["EmailAddress"],
                            "EmailAddress is incorrect");

                        DateTimeOffset now = DateTimeOffset.UtcNow;
                        DateTimeOffset fiveMinutesAgo = now.AddMinutes(-5);
                        DateTimeOffset receivedTime = DateTimeOffset.Parse(emailSourceValues["Received"]);
                        Assert.That(receivedTime, Is.GreaterThan(fiveMinutesAgo), "Received time is too old");
                        Assert.That(receivedTime, Is.LessThan(now), "Received time is too new");

                        Assert.AreEqual("TestSender@everything2.com", emailSourceValues["Sender"], "Sender is incorrect");
                        Assert.AreEqual("1", emailSourceValues["FAMSessionID"], "FAMSessionID is incorrect");

                        Assert.AreEqual(
                            emailSourceValues["QueueEventID"],
                            emailSourceValues["FAMFileID"],
                            "QueueEventID ought to be the same as the FAMFileID");
                    }

                    CollectionAssert.AreEquivalent(subjects, actualSubjects);
                });
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

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                var emlFilesOnDisk = GetDownloadedEmails();
                Assert.AreEqual(1, emlFilesOnDisk.Length);

                using var command = connection.CreateCommand();
                command.CommandText = GetEmailSourceValues;
                using var reader = command.ExecuteReader();
                Assert.IsTrue(reader.HasRows);
                while (reader.Read())
                {
                    Assert.IsNotNull(reader["OutlookEmailID"].ToString());
                    Assert.AreEqual("Recipient@extracttest.com, Test_Recipient@extracttest.com", reader["Recipients"].ToString());
                    Assert.AreEqual(EmailManagement.Configuration.SharedEmailAddress, reader["EmailAddress"].ToString());
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
                emailFileSupplier.WaitForSupplyingToStop();
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
                // Add emails with the same subject so as to test https://extract.atlassian.net/browse/ISSUE-18173
                foreach (var _ in Enumerable.Range(0, messagesToTest))
                {
                    await EmailTestHelper.AddInputMessage(EmailManagement);
                }

                // Process all messages normally one time around.
                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                var emlFilesOnDisk = GetDownloadedEmails();
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

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                // They should have used the same name, so there should be no additional files.
                emlFilesOnDisk = GetDownloadedEmails();
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
                emailFileSupplier.WaitForSupplyingToStop();
            }
        }

        [Test]
        public static void TestCopyFromFileSupplier()
        {
            using EmailFileSupplier emailFileSupplier = new(EmailManagementConfiguration);
            using EmailFileSupplier copy = new(new EmailManagementConfiguration()
            {
                FilePathToDownloadEmails = "Test",
                InputMailFolderName = "Meh",
                QueuedMailFolderName = "Yay",
                FailedMailFolderName = "Opps",
                SharedEmailAddress = "42"
            });


            copy.CopyFrom(emailFileSupplier);

            Assert.AreEqual(emailFileSupplier.DownloadDirectory, copy.DownloadDirectory);
            Assert.AreEqual(emailFileSupplier.InputMailFolderName, copy.InputMailFolderName);
            Assert.AreEqual(emailFileSupplier.QueuedMailFolderName, copy.QueuedMailFolderName);
            Assert.AreEqual(emailFileSupplier.SharedEmailAddress, copy.SharedEmailAddress);
            Assert.AreEqual(emailFileSupplier.FailedMailFolderName, copy.FailedMailFolderName);
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
            Assert.AreEqual(emailFileSupplier.FailedMailFolderName, clone.FailedMailFolderName);
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
            Assert.AreEqual(emailFileSupplier.FailedMailFolderName, loadedFileSupplier.FailedMailFolderName);
        }

        [Test]
        public static void TestEmailFileSupplierStart()
        {
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);

            // Ensure the task can be started, stopped, paused and resumed.
            fileProcessingManager.StartProcessing();
            fileProcessingManager.PauseProcessing();
            fileProcessingManager.StartProcessing();
            fileProcessingManager.StopProcessing();
            emailFileSupplier.WaitForSupplyingToStop();
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

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                var emlFilesOnDisk = GetDownloadedEmails();
                Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                // Stop processing, add another message, make sure the service can start again.
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();

                await EmailTestHelper.AddInputMessage(EmailManagement, 1, "StopStart");

                // Give the email a second to land
                await Task.Delay(1_000);

                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                emlFilesOnDisk = GetDownloadedEmails();
                Assert.AreEqual(messagesToTest + 1, emlFilesOnDisk.Length);

                // Pause processing, add an email, and ensure it can resume.
                fileProcessingManager.PauseProcessing();
                await EmailTestHelper.AddInputMessage(EmailManagement, 1, "PauseResume");
                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                emlFilesOnDisk = GetDownloadedEmails();
                Assert.AreEqual(messagesToTest + 2, emlFilesOnDisk.Length);

                // Confirm that emails are downloaded into Year\Month subfolders
                DateTimeOffset now = DateTimeOffset.UtcNow;
                string expectedFolder = UtilityMethods.FormatInvariant(
                    @$"{EmailManagementConfiguration.FilePathToDownloadEmails}\{now.Year}\{now.Month:D2}");
                string actualFolder = emlFilesOnDisk.Select(Path.GetDirectoryName).Distinct().Single();

                Assert.AreEqual(expectedFolder, actualFolder);
            }
            finally
            {
                await EmailTestHelper.CleanupTests(EmailManagement);
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();
            }
        }

        [Test]
        public static void TestConfiguredFileSupplier()
        {
            var config = new GraphTestsConfig();
            using EmailFileSupplier emailFileSupplier = new(new EmailManagementConfiguration()
            {
                FilePathToDownloadEmails = config.FolderToSaveEmails,
                InputMailFolderName = "Inbox",
                QueuedMailFolderName = "Inbox",
                FailedMailFolderName = "Failed",
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
                FilePathToDownloadEmails = @"D:\$FileOf(<EmailDownloadDirectory>)",
                InputMailFolderName = "<EmailInboxFolder>",
                QueuedMailFolderName = "<EmailQueuedMailFolder>",
                FailedMailFolderName = "<FailedMailFolder>",
                SharedEmailAddress = "<EmailAddress>"
            };

            FAMTagManagerClass tagManager = new();
            tagManager.AddTag("EmailDownloadDirectory", @"\\server\Share\FileFolder");
            tagManager.AddTag("EmailInboxFolder", "The inbox!");
            tagManager.AddTag("EmailQueuedMailFolder", "The post-download folder");
            tagManager.AddTag("EmailAddress", "The shared email address");
            tagManager.AddTag("FailedMailFolder", "The failed folder");

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
            Assert.AreEqual(@"D:\FileFolder", expandedConfig.FilePathToDownloadEmails);
            Assert.AreEqual("The inbox!", expandedConfig.InputMailFolderName);
            Assert.AreEqual("The post-download folder", expandedConfig.QueuedMailFolderName);
            Assert.AreEqual("The shared email address", expandedConfig.SharedEmailAddress);
            Assert.AreEqual("The failed folder", expandedConfig.FailedMailFolderName);
        }

        [Test, Category("Automated")]
        public static void TestFailedFolderMove()
        {
            var graphTestsConfig = new GraphTestsConfig();
           
            var messages = new Mock<IList<Message>>(MockBehavior.Loose);
            messages.Setup(m => m.Count).Returns(1);
            List<Message> messagesArray = new() { new Mock<Message>().Object };
            messages.Setup(m => m.GetEnumerator()).Returns(messagesArray.GetEnumerator());

            var emailManager = new Mock<IEmailManagement>(MockBehavior.Loose);

            emailManager.Setup(g => g.GetMessagesToProcessAsync()).Returns(Task.FromResult(messages.Object));
            emailManager.Setup(d => d.DownloadMessageToDisk(It.IsAny<Message>(), null)).Throws(new ExtractException("ELI53337", "Test"));
            emailManager.Setup(e => e.MoveMessageToFailedFolder(It.IsAny<Message>())).Returns(It.IsAny<Task<Message>>);

            using EmailFileSupplier emailFileSupplier = new(new EmailManagementConfiguration()
            {
                FilePathToDownloadEmails = graphTestsConfig.FolderToSaveEmails,
                InputMailFolderName = "Inbox",
                QueuedMailFolderName = "Inbox",
                FailedMailFolderName = "Failed",
                SharedEmailAddress = graphTestsConfig.SharedEmailAddress
            }, 
            (e) =>
            {
                return emailManager.Object;
            });

            emailFileSupplier.Start(new Mock<IFileSupplierTarget>().Object, new Mock<FAMTagManager>().Object, new Mock<FileProcessingDB>().Object, 1 );
            Thread.Sleep(1_000);
            emailFileSupplier.Stop();

            emailManager.Verify(e => e.DownloadMessageToDisk(It.IsAny<Message>(), null), Times.Once);
            emailManager.Verify(e => e.MoveMessageToFailedFolder(It.IsAny<Message>()), Times.Once);
            emailManager.Verify(e => e.MoveMessageToQueuedFolder(It.IsAny<Message>()), Times.Never);
        }

        /// <summary>
        /// Test that emails are downloaded in FIFO order
        /// </summary>
        [Test]
        public static async Task TestDownloadOrder()
        {
            await EmailTestHelper.CleanupTests(EmailManagement);

            int messagesToTest = 13;

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration);
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier);
            using var connection = new ExtractRoleConnection(EmailManagementConfiguration.FileProcessingDB.DatabaseServer, EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                // Add new emails for testing such that the alphabetic order of the filenames is
                // the same as the order they are added to the database
                string inputMailFolderID = await EmailManagement.GetMailFolderID(EmailManagementConfiguration.InputMailFolderName);
                for (int i = 1; i <= messagesToTest; i++)
                {
                    string subject = $"Email number {i:D3}";
                    await EmailTestHelper.AddInputMessage(EmailManagement, inputMailFolderID, subject);

                    // Wait between adding files because the received date field that is used for ordering the emails doesn't have ms precision
                    await Task.Delay(1_000);
                }

                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                // Stop processing to avoid logged exceptions
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();

                List<string> filesInDatabaseOrder = new();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT [FileName] FROM dbo.FAMFile ORDER BY ID";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    filesInDatabaseOrder.Add(reader.GetString(0));
                }

                List<string> filesInAddedOrder = filesInDatabaseOrder.OrderBy(path => path).ToList();

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(messagesToTest, filesInDatabaseOrder.Count);

                    CollectionAssert.AreEqual(filesInAddedOrder, filesInDatabaseOrder);
                });
            }
            finally
            {
                // Remove all downloaded emails
                await EmailTestHelper.CleanupTests(EmailManagement);
            }
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
            var database = FAMTestDBManager.GetNewDatabase(FAMTestDBManager.GenerateDatabaseName());
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

        private static string[] GetDownloadedEmails()
        {
            return System.IO.Directory.GetFiles(
                EmailManagementConfiguration.FilePathToDownloadEmails, "*.eml", SearchOption.AllDirectories);
        }

        #endregion Helper Methods
    }
}
