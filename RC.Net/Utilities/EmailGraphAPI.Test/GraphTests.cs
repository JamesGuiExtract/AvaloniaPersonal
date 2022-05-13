using Extract.Email.GraphClient.Test.Utilities;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Utilities;
using MimeKit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient.Test
{
    [TestFixture]
    [Category("EmailGraphApi")]
    [NonParallelizable]
    public class GraphTests
    {
        private static FAMTestDBManager<GraphTests> FAMTestDBManager;
        private static EmailTestHelper EmailTestHelper;
        private static FileProcessingDB Database;
        private static Lazy<EmailManagement> LazyEmailManagement;
        private static EmailManagement EmailManagement => LazyEmailManagement.Value;
        private static GraphTestsConfig GraphTestsConfig;
        private static int[] ErrorPercents => EmailTestHelper.ErrorPercents;

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            FAMTestDBManager = new FAMTestDBManager<GraphTests>();
            EmailTestHelper = new EmailTestHelper();

            GraphTestsConfig = new GraphTestsConfig();
            GraphTestsConfig.DatabaseName = FAMTestDBManager.GenerateDatabaseName();
            Database = FAMTestDBManager.GetNewDatabase(GraphTestsConfig.DatabaseName);

            Database.LoginUser("admin", "a");
            Database.SetExternalLogin(
                Constants.EmailFileSupplierExternalLoginDescription,
                GraphTestsConfig.EmailUserName,
                GraphTestsConfig.EmailPassword);

            Database.SetDBInfoSetting("AzureClientId", GraphTestsConfig.AzureClientId, true, false);
            Database.SetDBInfoSetting("AzureTenant", GraphTestsConfig.AzureTenantID, true, false);
            Database.SetDBInfoSetting("AzureInstance", GraphTestsConfig.AzureInstance, true, false);

            EmailManagementConfiguration emailManagementConfiguration = new()
            {
                ExternalLoginDescription = Constants.EmailFileSupplierExternalLoginDescription,
                FileProcessingDB = Database,
                InputMailFolderName = UtilityMethods.GetRandomString(8, true, true, false),
                QueuedMailFolderName = UtilityMethods.GetRandomString(9, true, true, false),
                FailedMailFolderName = UtilityMethods.GetRandomString(10, true, true, false),
                SharedEmailAddress = GraphTestsConfig.SharedEmailAddress,
                FilePathToDownloadEmails = GraphTestsConfig.FolderToSaveEmails
            };

            LazyEmailManagement = new(() =>
            {
                var emailManagement = new EmailManagement(emailManagementConfiguration);
                emailManagement.CreateMailFolder(emailManagementConfiguration.InputMailFolderName).GetAwaiter().GetResult();
                emailManagement.CreateMailFolder(emailManagementConfiguration.QueuedMailFolderName).GetAwaiter().GetResult();
                emailManagement.CreateMailFolder(emailManagementConfiguration.FailedMailFolderName).GetAwaiter().GetResult();

                return emailManagement;
            });
        }

        [OneTimeTearDown]
        public static async Task FinalCleanup()
        {
            await EmailTestHelper.CleanupTests(EmailManagement);
            if (LazyEmailManagement.IsValueCreated)
            {
                await EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.QueuedMailFolderName, EmailManagement);
                await EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.InputMailFolderName, EmailManagement);
                await EmailTestHelper.DeleteMailFolder(EmailManagement.Configuration.FailedMailFolderName, EmailManagement);

                EmailManagement.Dispose();
            }

            Directory.Delete(EmailManagement.Configuration.FilePathToDownloadEmails, true);

            FAMTestDBManager.Dispose();
            EmailTestHelper.Dispose();
        }

        #endregion Overhead

        [Test]
        [TestCaseSource(nameof(ErrorPercents))]
        public async static Task EnsureInputFolderCanBeRead(int errorPercent)
        {
            // Arrange
            await EmailTestHelper.ClearAllMessages(EmailManagement);
            await EmailTestHelper.AddInputMessage(EmailManagement);
            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(errorPercent);

            // Act
            var inbox = await emailManagementWithErrors.GetSharedAddressInputMailFolder();

            // Assert
            Assert.That(inbox.TotalItemCount > 0);
        }

        [Test]
        [TestCaseSource(nameof(ErrorPercents))]
        public async static Task TestDownloadLimit(int errorPercent)
        {
            // Arrange
            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(errorPercent);

            int maxGraphDownload = 10;

            await EmailTestHelper.ClearAllMessages(EmailManagement);
            await EmailTestHelper.AddInputMessage(EmailManagement, maxGraphDownload + 1);

            // Act
            var messages = (await emailManagementWithErrors.GetMessagesToProcessAsync()).ToArray();

            // Assert
            Assert.That(messages.Length == maxGraphDownload);
        }

        /// <summary>
        /// Sends an email to the folder, then downloads the email to disk, and moves the email to Queued.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(ErrorPercents))]
        public async static Task DownloadEmailToDisk(int errorPercent)
        {
            // Arrange
            await EmailTestHelper.AddInputMessage(EmailManagement);

            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(errorPercent);
            using var emailDatabaseManager = new EmailDatabaseManager(EmailManagement.Configuration);

            // Act
            var messages = (await emailManagementWithErrors.GetMessagesToProcessAsync()).ToArray();

            Collection<string> files = new();
            foreach(var message in messages)
            {
                string filePath = emailDatabaseManager.GetNewFileName(message);
                await emailManagementWithErrors.DownloadMessageToDisk(message, filePath);
                files.Add(filePath);
            }

            // Assert
            Assert.That(files.Count > 0);

            foreach (var file in files)
            {
                var message = ReadEMLFile(file);
                Assert.That(message.Subject != null);
                FileSystemMethods.DeleteFile(file);
            }
        }

        /// <summary>
        /// Email subjects can have invalid filename characters.
        /// In case this happens remove the invalid characters from the filename.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(ErrorPercents))]
        public async static Task DownloadEmailToDiskInvalidFileName(int errorPercent)
        {
            // Arange
            await EmailTestHelper.AddInputMessage(EmailManagement, 1, "\\:**<>$+|==%");

            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(errorPercent);
            using var emailDatabaseManager = new EmailDatabaseManager(EmailManagement.Configuration);

            // Act
            var messages = (await emailManagementWithErrors.GetMessagesToProcessAsync()).ToArray();

            Collection<string> files = new();
            foreach (var message in messages)
            {
                string filePath = emailDatabaseManager.GetNewFileName(message);
                await emailManagementWithErrors.DownloadMessageToDisk(message, filePath);
                files.Add(filePath);
            }

            // Assert
            Assert.That(files.Count > 0);

            FileSystemMethods.DeleteFile(files[0]);
        }

        /// <summary>
        /// Email subjects can have duplicated file names.
        /// In case this happens a number will be added to the filename.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(ErrorPercents))]
        public async static Task DownloadEmailToDiskNameCollision(int errorPercent)
        {
            // Arrange
            await EmailTestHelper.ClearAllMessages(EmailManagement);
            // These two will have identical file names.
            await EmailTestHelper.AddInputMessage(EmailManagement);
            await EmailTestHelper.AddInputMessage(EmailManagement);

            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(errorPercent);
            using var emailDatabaseManager = new EmailDatabaseManager(EmailManagement.Configuration);

            // Act
            var messages = (await emailManagementWithErrors.GetMessagesToProcessAsync()).ToArray();

            Collection<string> files = new();
            foreach (var message in messages)
            {
                string filePath = emailDatabaseManager.GetNewFileName(message);
                await emailManagementWithErrors.DownloadMessageToDisk(message, filePath);
                files.Add(filePath);
            }

            // Assert
            Assert.That(files.Count > 0);

            Assert.That(files[0].ToString() != files[1].ToString());

            FileSystemMethods.DeleteFile(files[0]);
            FileSystemMethods.DeleteFile(files[1]);
        }

        /// <summary>
        /// Ensures the queued folder can be created.
        /// </summary>
        [Test]
        public async static Task CreateQueuedFolder()
        {
            // Try to delete the queued folder if it exists.
            await EmailTestHelper.CleanupTests(EmailManagement);
            
            // There appears to be some kind of timeout for deleting a folder and instantly re-creating it.
            await Task.Delay(1000);

            // Attempt to create the mail folder.
            await EmailManagement.CreateMailFolder(EmailManagement.Configuration.QueuedMailFolderName);
            
            var sharedMailFolders = await EmailManagement.GetSharedEmailAddressMailFolders();
            Assert.That(sharedMailFolders.Where(mailFolder => mailFolder.DisplayName.Equals(EmailManagement.Configuration.QueuedMailFolderName)).Any());

            // Try to create the mail folder again and ensure no error is triggered.
            await EmailManagement.CreateMailFolder(EmailManagement.Configuration.QueuedMailFolderName);
        }

        [Test]
        [TestCaseSource(nameof(ErrorPercents))]
        public async static Task EnsureIdsRemainConstantAfterMove(int errorPercent)
        {
            // Arrange
            await EmailTestHelper.ClearAllMessages(EmailManagement);
            await EmailManagement.CreateMailFolder(EmailManagement.Configuration.QueuedMailFolderName);
            await EmailTestHelper.AddInputMessage(EmailManagement);

            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(errorPercent);

            // Act
            var messages = (await emailManagementWithErrors.GetMessagesToProcessAsync().ConfigureAwait(false)).ToArray();
            var movedMessage = await emailManagementWithErrors.MoveMessageToQueuedFolder(messages[0].Id);

            // Assert
            Assert.AreEqual(messages[0].Id, movedMessage.Id);
        }
        
        [Test, Category("Automated")]
        [TestCaseSource(nameof(ErrorPercents))]
        public async static Task MoveMessageToFailed(int errorPercent)
        {
            // Arrange
            await EmailTestHelper.ClearAllMessages(EmailManagement);
            await EmailManagement.CreateMailFolder(EmailManagement.Configuration.QueuedMailFolderName);
            await EmailTestHelper.AddInputMessage(EmailManagement);
            var parentID = await EmailManagement.GetFailedFolderID();

            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(errorPercent);

            // Act
            var messages = (await EmailManagement.GetMessagesToProcessAsync().ConfigureAwait(false)).ToArray();
            var movedMessage = await EmailManagement.MoveMessageToFailedFolder(messages[0].Id);

            // Assert
            Assert.AreEqual(messages[0].Id, movedMessage.Id);
            Assert.AreEqual(parentID, movedMessage.ParentFolderId);
            Assert.AreNotEqual(messages[0].ParentFolderId, movedMessage.ParentFolderId);   
        }

        private static MimeMessage ReadEMLFile(string fileName)
        {
            return MimeMessage.Load(fileName);
        }

        private static EmailManagement CreateEmailManagementWithErrorGenerator(int errorPercent)
        {
            return EmailTestHelper.CreateEmailManagementWithErrorGenerator(EmailManagement.Configuration, EmailManagement.AccessToken, errorPercent);
        }
    }
}
