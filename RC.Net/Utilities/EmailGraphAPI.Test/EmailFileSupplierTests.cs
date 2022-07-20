using Extract.Email.GraphClient.Test.Mocks;
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
using System.Transactions;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient.Test
{
    [TestFixture]
    [Category("EmailGraphApi"), Category("Automated")]
    [NonParallelizable]
    public class EmailFileSupplierTests
    {
        static readonly FAMTestDBManager<GraphTests> FAMTestDBManager = new();
        static readonly EmailTestHelper EmailTestHelper = new();
        static readonly EmailManagementConfiguration EmailManagementConfiguration = new();
        static readonly TestFileManager<GraphTests> TestFileManager = new();
        static readonly string GetEmailSourceValues =
            "SELECT * FROM dbo.EmailSource JOIN dbo.FAMFile ON FAMFileID = dbo.FAMFile.ID";

        static Lazy<EmailManagement> LazyEmailManagement;
        static EmailManagement EmailManagement => LazyEmailManagement.Value;
        static string AccessToken => EmailManagement.AccessToken;

        static readonly string TestActionName = "TestAction";
        static int[] ErrorPercents => EmailTestHelper.ErrorPercents;
        static readonly int[] NumFileSuppliers = new[] { 2 };

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
            EmailManagementConfiguration.InputMailFolderName = UtilityMethods.GetRandomString(9, true, true, false);
            EmailManagementConfiguration.QueuedMailFolderName = UtilityMethods.GetRandomString(8, true, true, false);
            EmailManagementConfiguration.FailedMailFolderName = UtilityMethods.GetRandomString(10, true, true, false);
            EmailManagementConfiguration.SharedEmailAddress = graphTestsConfig.SharedEmailAddress;
            EmailManagementConfiguration.FilePathToDownloadEmails = graphTestsConfig.FolderToSaveEmails;

            LazyEmailManagement = new(() =>
            {
                var emailManagement = new EmailManagement(EmailManagementConfiguration);
                emailManagement.CreateMailFolder(EmailManagementConfiguration.InputMailFolderName).GetAwaiter().GetResult();
                emailManagement.CreateMailFolder(EmailManagementConfiguration.QueuedMailFolderName).GetAwaiter().GetResult();
                emailManagement.CreateMailFolder(EmailManagementConfiguration.FailedMailFolderName).GetAwaiter().GetResult();

                return emailManagement;
            });
        }

        /// <summary>
        /// Cleanup after all tests have run
        /// </summary>
        [OneTimeTearDown]
        public static async Task FinalCleanup()
        {
            try
            {
                if (LazyEmailManagement.IsValueCreated)
                {
                    await EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.QueuedMailFolderName, EmailManagement);
                    await EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.InputMailFolderName, EmailManagement);
                    await EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.FailedMailFolderName, EmailManagement);

                    EmailManagement.Dispose();
                }
            }
            finally
            {
                EmailTestHelper?.Dispose();
                FAMTestDBManager?.Dispose();
                TestFileManager?.Dispose();
                System.IO.Directory.Delete(EmailManagementConfiguration.FilePathToDownloadEmails, true);
            }
        }

        #endregion Overhead

        /// <summary>
        /// Test that the EmailSource table is populated correctly
        /// </summary>
        [Test]
        public static async Task TestEmailSourceTable(
            [ValueSource(nameof(ErrorPercents))] int errorPercent,
            [ValueSource(nameof(NumFileSuppliers))] int numFileSuppliers)
        {
            // --------------------------------------------------------------------------------
            // Arrange
            // --------------------------------------------------------------------------------
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
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration, configuration => configuration.CreateWithErrorGenerator(AccessToken, errorPercent));

            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier, numSuppliers: numFileSuppliers);
            using var connection = new ExtractRoleConnection(EmailManagementConfiguration.FileProcessingDB.DatabaseServer, EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                // Get current time with seconds precision
                DateTimeOffset emailsSentTime = DateTimeOffset.UtcNow;
                emailsSentTime = emailsSentTime.AddTicks(-emailsSentTime.Ticks % TimeSpan.TicksPerSecond);

                // Add new emails for testing.
                string inputMailFolderID = await EmailManagement.GetMailFolderID(EmailManagementConfiguration.InputMailFolderName);
                foreach (string subject in subjects)
                {
                    await EmailTestHelper.AddInputMessage(EmailManagement, inputMailFolderID, subject);
                }

                // ----------------------------------------------------------------------------
                // Act
                // ----------------------------------------------------------------------------
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

                // ----------------------------------------------------------------------------
                // Assert
                // ----------------------------------------------------------------------------
                static void AddValueToDictionary(Dictionary<string, string> dict, SqlDataReader reader, string field)
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
                        DateTimeOffset receivedTime = DateTimeOffset.Parse(emailSourceValues["Received"]);
                        Assert.That(receivedTime, Is.GreaterThanOrEqualTo(emailsSentTime), "Received time is too old");
                        Assert.That(receivedTime, Is.LessThan(now), "Received time is too new");

                        Assert.AreEqual("TestSender@everything2.com", emailSourceValues["Sender"], "Sender is incorrect");
                        Assert.AreEqual("1", emailSourceValues["FAMSessionID"], "FAMSessionID is incorrect");
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
        public static async Task TestEmailSourceTableBlankSubject(
            [ValueSource(nameof(ErrorPercents))] int errorPercent,
            [ValueSource(nameof(NumFileSuppliers))] int numFileSuppliers)
        {
            // --------------------------------------------------------------------------------
            // Arrange
            // --------------------------------------------------------------------------------
            await EmailTestHelper.CleanupTests(EmailManagement);

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration, configuration => configuration.CreateWithErrorGenerator(AccessToken, errorPercent));
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier, numSuppliers: numFileSuppliers);
            using var connection = new ExtractRoleConnection(EmailManagementConfiguration.FileProcessingDB.DatabaseServer, EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                // Add new emails for testing.
                await EmailTestHelper.AddInputMessageBlankSubject(EmailManagement);

                // ----------------------------------------------------------------------------
                // Act
                // ----------------------------------------------------------------------------
                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                // ----------------------------------------------------------------------------
                // Assert
                // ----------------------------------------------------------------------------
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
        public static async Task TestEmailSourceTableForceProcessing(
            [ValueSource(nameof(ErrorPercents))] int errorPercent,
            [ValueSource(nameof(NumFileSuppliers))] int numFileSuppliers)
        {
            // --------------------------------------------------------------------------------
            // Arrange
            // --------------------------------------------------------------------------------
            int messagesToTest = 5;
            await EmailTestHelper.CleanupTests(EmailManagement);

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration, configuration => configuration.CreateWithErrorGenerator(AccessToken, errorPercent));
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier, true, numSuppliers: numFileSuppliers);
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

                // ----------------------------------------------------------------------------
                // Act
                // ----------------------------------------------------------------------------
                // Process all messages normally one time around.
                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                var emlFilesOnDisk = GetDownloadedEmails();
                Assert.AreEqual(messagesToTest, emlFilesOnDisk.Length);

                fileProcessingManager.PauseProcessing();
                emailFileSupplier.WaitForSupplyingToStop();

                // Ensure all files were set to pending.
                for (int i = 1; i <= messagesToTest; i++)
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

                // ----------------------------------------------------------------------------
                // Assert
                // ----------------------------------------------------------------------------
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
        public static void TestEmailFileSupplierStart(
            [ValueSource(nameof(ErrorPercents))] int errorPercent,
            [ValueSource(nameof(NumFileSuppliers))] int numFileSuppliers)
        {
            // Arrange
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration, configuration => configuration.CreateWithErrorGenerator(AccessToken, errorPercent));
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier, numSuppliers: numFileSuppliers);

            // Act
            // Ensure the task can be started, stopped, paused and resumed.
            fileProcessingManager.StartProcessing();
            fileProcessingManager.PauseProcessing();
            fileProcessingManager.StartProcessing();
            fileProcessingManager.StopProcessing();
            emailFileSupplier.WaitForSupplyingToStop();
        }

        [Test]
        public static async Task TestEmailDownloadAndQueueFileSupplier(
            [ValueSource(nameof(ErrorPercents))] int errorPercent,
            [ValueSource(nameof(NumFileSuppliers))] int numFileSuppliers)
        {
            // --------------------------------------------------------------------------------
            // Arrange
            // --------------------------------------------------------------------------------
            await EmailTestHelper.CleanupTests(EmailManagement);

            int messagesToTest = 15;
            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration, configuration => configuration.CreateWithErrorGenerator(AccessToken, errorPercent));
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier, numSuppliers: numFileSuppliers);

            try
            {
                // Add new emails for testing.
                await EmailTestHelper.AddInputMessage(EmailManagement, messagesToTest);

                // ----------------------------------------------------------------------------
                // Act
                // ----------------------------------------------------------------------------
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

                // ----------------------------------------------------------------------------
                // Assert
                // ----------------------------------------------------------------------------
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
                var emailManagerMock = new Mock<IEmailManagement>();
                emailManagerMock.Setup(m => m.DoesMailFolderExist(It.IsAny<string>())).ReturnsAsync(true);
                emailManagerMock.Setup(m => m.GetMessagesToProcessAsync()).ReturnsAsync(Array.Empty<Message>());

                return emailManagerMock.Object;
            }
            IEmailDatabaseManager CreateEmailDatabaseManager(EmailManagementConfiguration config)
            {
                return new Mock<IEmailDatabaseManager>().Object;
            }

            using EmailFileSupplier emailFileSupplier = new(sourceConfig, CreateEmailManagement, CreateEmailDatabaseManager);

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

        [Test]
        public static void TestFailedFolderMove()
        {
            var graphTestsConfig = new GraphTestsConfig();

            var messages = new Mock<IList<Message>>(MockBehavior.Loose);
            messages.Setup(m => m.Count).Returns(1);
            List<Message> messagesArray = new() { new Message { Id = "placeholder" } };
            messages.Setup(m => m.GetEnumerator()).Returns(messagesArray.GetEnumerator());

            var emailManager = new Mock<IEmailManagement>(MockBehavior.Loose);
            emailManager.Setup(m => m.DoesMailFolderExist(It.IsAny<string>())).ReturnsAsync(true);
            emailManager.Setup(m => m.GetMessagesToProcessAsync()).ReturnsAsync(messages.Object);
            emailManager.Setup(m => m.DownloadMessageToDisk(It.IsAny<Message>(), It.IsAny<string>())).ThrowsAsync(new ExtractException("ELI53337", "Test"));
            emailManager.Setup(m => m.IsMessageInInputFolder(It.IsAny<string>())).ReturnsAsync(true);

            var emailDatabaseManager = new Mock<IEmailDatabaseManager>(MockBehavior.Loose);
            emailDatabaseManager.Setup(m => m.GetEmailsPendingMoveFromInbox()).Returns(Array.Empty<string>());
            emailDatabaseManager.Setup(m => m.LockEmailSource()).Returns(() => new TransactionScope());
            string filePath = "";
            emailDatabaseManager.Setup(m => m.TryGetExistingEmailFilePath(It.IsAny<Message>(), out filePath)).Returns(true);

            using EmailFileSupplier emailFileSupplier = new(new EmailManagementConfiguration()
            {
                FilePathToDownloadEmails = graphTestsConfig.FolderToSaveEmails,
                InputMailFolderName = "Inbox",
                QueuedMailFolderName = "Q",
                FailedMailFolderName = "Failed",
                SharedEmailAddress = graphTestsConfig.SharedEmailAddress
            },
            _ => emailManager.Object,
            _ => emailDatabaseManager.Object);

            emailFileSupplier.Start(new Mock<IFileSupplierTarget>().Object, new Mock<FAMTagManager>().Object, new Mock<FileProcessingDB>().Object, 1);
            Thread.Sleep(1_000);
            emailFileSupplier.Stop();

            emailManager.Verify(e => e.DownloadMessageToDisk(It.IsAny<Message>(), It.IsAny<string>()), Times.Once);
            emailManager.Verify(e => e.MoveMessageToFailedFolder(It.IsAny<string>()), Times.Once);
            emailManager.Verify(e => e.MoveMessageToQueuedFolder(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Test that emails are downloaded in FIFO order
        /// </summary>
        [Test]
        public static async Task TestDownloadOrder(
            [ValueSource(nameof(ErrorPercents))] int errorPercent,
            [ValueSource(nameof(NumFileSuppliers))] int numFileSuppliers)
        {
            // --------------------------------------------------------------------------------
            // Arrange
            // --------------------------------------------------------------------------------
            await EmailTestHelper.CleanupTests(EmailManagement);

            int messagesToTest = 13;

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration, configuration => configuration.CreateWithErrorGenerator(AccessToken, errorPercent));
            var fileProcessingManager = CreateFileSupplierFAM(emailFileSupplier, numSuppliers: numFileSuppliers);
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

                // ----------------------------------------------------------------------------
                // Act
                // ----------------------------------------------------------------------------
                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                // Stop processing to avoid logged exceptions
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();

                // ----------------------------------------------------------------------------
                // Assert
                // ----------------------------------------------------------------------------
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
            catch (ExtractException ex) when (ex.Message == "Timeout waiting for sleep")
            {
                // Stop processing to avoid logged exceptions
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();
                throw;
            }
            finally
            {
                // Remove all downloaded emails
                await EmailTestHelper.CleanupTests(EmailManagement);
            }
        }

        [Test]
        public static async Task TestVeryLongSubjects()
        {
            // --------------------------------------------------------------------------------
            // Arrange
            // --------------------------------------------------------------------------------
            await EmailTestHelper.CleanupTests(EmailManagement);

            int messagesToTest = 2;

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration, configuration => configuration.CreateWithErrorGenerator(AccessToken, 0));
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
                    // Make the subject a 1000 character long string. We want two of them just to ensure there are no name collisions
                    string subject = $"pXjbGy59OhJdCLkXHHrrhoO5NkB4w6q4ghsYNZ0dHbInvjBvpYSeecj7oNrhnUQW1UXNuprnO1wfZogTe5FQ6GHjAcBiZdlMEpzch75AFQkld7YVEBwdUznDgCBA7S8xPbLOaHaRNQLDtNDfFkyvY251wYviMbboV89FYo1SaWa17IVsUnIvsdqh4aXZc2BVCZmoPM5ETSxQ8gaExpqnToqa5GIhwtNLKvpeurRKb2plouePnCnnk5ryI6Hv5OdK1dsbvjPqbCFTD9m9ORpWpW0lD1SdA0CmPksAfpKyzPaEg7onHiSJ39S8QbJkEmjtNp9Vo975YlipEHlVKdKjpUqRhQmSuBzWm4OWzi1hxapB4nSuqJw22dxp7ijkZzb8ZFnPQtVBukUSgihv5yCV2oJFv5KaGELvZTmVolR4MIAwHhZSDHza72THkEInvKMOxIMvEJJ9LflLQJkzHqGbKWPTIkuCMxVmsUSxdmPuAJJ77X2MwN557B8pJIAgorLFfKrYN8GabCkc19fWc70YWlhwmMWZLZtr9MFgX5ACsQvEJyehbXLTzegXvWmHbZnBZpZ6UpxUmS6lQV8ZiFnWv2VEdC0tIBPAJjAPuisYP8anLn9zumQS15utl5glM6fdyYzMovALaKiKETTjmwvxJLmIE4Q1bxPfxLqpm97nnl5jsPD1iCBmyIk86Kynn0w8WkRo7xZmhPHfTPUnfkHcKDLznIfKDJZqQdoIfcadfPZwHuYtg7u7NZv7ELsPRLswHUmKEtLtKRvzAMGRSX4uwqr6xPX57lcQHcvNMgoeL4Ni4aO9xiAgqeYNw5w1yYhIXZMvqc2k1EuUX1wz1dbxHUz5PGJdYXPCpJeNK8RB0bwEbx0RMs2JsS92frnsJoq3IF6PFjAAgFxTe26FnLdjuEKX36t6bGc3IIO0bQYtUcelLfxbchrkEs2bFVoKsnvxsgT8wNv5J7u8AbUm0KEviBNqrTpONg5bhxBELEH4";
                    await EmailTestHelper.AddInputMessage(EmailManagement, inputMailFolderID, subject);

                    // Wait between adding files because the received date field that is used for ordering the emails doesn't have ms precision
                    await Task.Delay(1_000);
                }

                // ----------------------------------------------------------------------------
                // Act
                // ----------------------------------------------------------------------------
                fileProcessingManager.StartProcessing();

                // Give the thread a second to get started
                await Task.Delay(1_000);

                // Wait for all the available emails to be downloaded
                emailFileSupplier.WaitForSleep();

                // Stop processing to avoid logged exceptions
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();

                // ----------------------------------------------------------------------------
                // Assert
                // ----------------------------------------------------------------------------

                Assert.AreEqual(messagesToTest, GetDownloadedEmails().Count());
            }
            catch (ExtractException ex) when (ex.Message == "Timeout waiting for sleep")
            {
                // Stop processing to avoid logged exceptions
                fileProcessingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();
                throw;
            }
            finally
            {
                // Remove all downloaded emails
                await EmailTestHelper.CleanupTests(EmailManagement);
            }
        }

        /// <summary>
        /// Test that errors that occur outside of the main file supplying transaction are handled properly
        /// </summary>
        [Test]
        public static async Task TestFailurePoints([Values] bool forceProcessing)
        {
            Assume.That(forceProcessing, Is.False,
                message: "Force processing can cause double-queuing and processing to happen when there are database errors");

            // --------------------------------------------------------------------------------
            // Arrange
            // --------------------------------------------------------------------------------
            await EmailTestHelper.CleanupTests(EmailManagement);

            int messagesToTest = 13;
            int errorPercent = 10;

            using var emailFileSupplier = new EmailFileSupplier(EmailManagementConfiguration,
                configuration => new ErrorGeneratingEmailManagement(configuration, errorPercent),
                configuration => new ErrorGeneratingEmailDatabaseManager(configuration, errorPercent),
                target => new ErrorGeneratingFileSupplierTarget(target, errorPercent));

            var fileSupplyingManager = CreateFileSupplierFAM(emailFileSupplier, forceProcessing);

            List<string> filesProcessed = new();
            HashSet<string> uniqueFilesProcessed = new();
            ManualResetEvent allFilesProcessed = new(false);
            bool stopProcessing = false;

            void fileProcessor()
            {
                var fpDB = EmailManagementConfiguration.FileProcessingDB;
                fpDB.RecordFAMSessionStart("placeholder", TestActionName, false, true);
                fpDB.RegisterActiveFAM();

                try
                {
                    while (!stopProcessing)
                    {
                        var files = fpDB.GetFilesToProcess(TestActionName, 25, false, "");
                        foreach (var fileRecord in files.ToIEnumerable<FileRecord>())
                        {
                            filesProcessed.Add(fileRecord.Name);
                            uniqueFilesProcessed.Add(fileRecord.Name);
                            fpDB.NotifyFileProcessed(fileRecord.FileID, TestActionName, fileRecord.WorkflowID, true);

                            if (uniqueFilesProcessed.Count == messagesToTest)
                            {
                                allFilesProcessed.Set();
                            }
                        }
                        Thread.Sleep(200);
                    }
                }
                finally
                {
                    fpDB.UnregisterActiveFAM();
                    fpDB.RecordFAMSessionStop();
                }
            }
            Task fileProcessorTask = null;

            using var connection = new ExtractRoleConnection(
                EmailManagementConfiguration.FileProcessingDB.DatabaseServer,
                EmailManagementConfiguration.FileProcessingDB.DatabaseName);
            connection.Open();

            try
            {
                await EmailTestHelper.AddInputMessage(EmailManagement, messagesToTest);

                // ----------------------------------------------------------------------------
                // Act
                // ----------------------------------------------------------------------------

                // Start supplying and processing
                fileSupplyingManager.StartProcessing();
                fileProcessorTask = Task.Run(fileProcessor);

                // Wait for all files to be supplied
                emailFileSupplier.WaitForSleep();

                // Wait for all files to be processed
                allFilesProcessed.WaitOne(TimeSpan.FromSeconds(1));

                // Stop processing to avoid logged exceptions
                stopProcessing = true;
                fileSupplyingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();
                await fileProcessorTask;

                // ----------------------------------------------------------------------------
                // Assert
                // ----------------------------------------------------------------------------

                // Confirm that all files are in the completed status and that there are no pending actions remaining
                List<FileStatus> databaseInfo = new();
                using var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT [FileName], PendingMoveFromEmailFolder, PendingNotifyFromEmailFolder, ASCName, ActionStatus FROM dbo.FAMFile" +
                    " JOIN dbo.EmailSource ON EmailSource.FAMFileID = FAMFile.ID" +
                    " JOIN dbo.FileActionStatus ON EmailSource.FAMFileID = FileActionStatus.FileID" +
                    " JOIN dbo.[Action] ON FileActionStatus.ActionID = [Action].ID";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    databaseInfo.Add(new(
                        reader.GetString(0),
                        reader[1] as string,
                        reader[2] as string,
                        reader.GetString(3),
                        reader.GetString(4)));
                }

                Assert.Multiple(() =>
                {
                    var expectedStatuses = Enumerable.Repeat("C", messagesToTest);
                    var expectedPendingActions = Enumerable.Repeat<string>(null, messagesToTest);

                    Assert.AreEqual(messagesToTest, uniqueFilesProcessed.Count);
                    CollectionAssert.AreEquivalent(uniqueFilesProcessed, filesProcessed,
                        message: forceProcessing
                        ? "Force processing can cause double-processing to happen when there is an error" +
                            " executing the database command that clears the pending notification field"
                        : "Double-processing should not happen when force processing is false");

                    CollectionAssert.AreEqual(expectedStatuses, databaseInfo.Select(x => x.ActionStatus),
                        message: forceProcessing
                        ? "Force processing can cause double-queuing to happen when there is an error" +
                            " executing the database command that clears the pending notification field"
                        : "Double-queuing should not happen when force processing is false");

                    CollectionAssert.AreEqual(expectedPendingActions, databaseInfo.Select(x => x.PendingMove),
                        message: "There should be no pending move operations remaining although it is possible" +
                        " if there are a lot of failures running database commands");

                    CollectionAssert.AreEqual(expectedPendingActions, databaseInfo.Select(x => x.PendingNotify),
                        message: "There should be no pending notify operations remaining although it is possible" +
                        " if there are a lot of failures running database commands");
                });
            }
            catch (ExtractException ex) when (ex.Message == "Timeout waiting for sleep")
            {
                // Stop processing to avoid logged exceptions
                stopProcessing = true;
                fileSupplyingManager.StopProcessing();
                emailFileSupplier.WaitForSupplyingToStop();
                if (fileProcessorTask is not null)
                {
                    try
                    {
                        await fileProcessorTask;
                    }
                    catch { }
                }
                throw;
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

        private static IFileProcessingManager CreateFileSupplierFAM(IFileSupplier fileSupplier, bool forceProcessing = false, int numSuppliers = 1)
        {
            EmailManagementConfiguration.FileProcessingDB = GetNewAzureDatabase();

            var fpManager = new FileProcessingManagerClass
            {
                DatabaseServer = EmailManagementConfiguration.FileProcessingDB.DatabaseServer,
                DatabaseName = EmailManagementConfiguration.FileProcessingDB.DatabaseName,
                ActionName = TestActionName
            };

            ((IFileActionMgmtRole)fpManager.FileSupplyingMgmtRole).Enabled = true;
            for (int i = 0; i < numSuppliers; i++)
            {
                var fileSupplierCopy = i == 0 ? fileSupplier : (IFileSupplier)((ICopyableObject)fileSupplier).Clone();

                fpManager.FileSupplyingMgmtRole.FileSuppliers.PushBack(new FileSupplierDataClass
                {
                    FileSupplier = new ObjectWithDescriptionClass
                    {
                        Object = fileSupplierCopy,
                        Enabled = true
                    },
                    ForceProcessing = forceProcessing,
                });
            }
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

        #region Helper Classes

        class FileStatus
        {
            public FileStatus(string fileName, string pendingMove, string pendingNotify, string actionName, string actionStatus)
            {
                PendingMove = pendingMove;
                PendingNotify = pendingNotify;
                FileName = fileName;
                ActionName = actionName;
                ActionStatus = actionStatus;
            }

            public string FileName { get; }
            public string PendingMove { get; }
            public string PendingNotify { get; }
            public string ActionName { get; }
            public string ActionStatus { get; }
        }

        #endregion Helper Classes
    }
}
