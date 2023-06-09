﻿using Extract.Email.GraphClient.Test.Utilities;
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
        /// Send an email with a long subject to the email address, ensure that the names get shortened.
        /// </summary>
        [Test]
        public static void LimitFilePathNames()
        {
            // Arrange
            string subject = $"pXjbGy59OhJdCLkXHHrrhoO5NkB4w6q4ghsYNZ0dHbInvjBvpYSeecj7oNrhnUQW1UXNuprnO1wfZogTe5FQ6GHjAcBiZdlMEpzch75AFQkld7YVEBwdUznDgCBA7S8xPbLOaHaRNQLDtNDfFkyvY251wYviMbboV89FYo1SaWa17IVsUnIvsdqh4aXZc2BVCZmoPM5ETSxQ8gaExpqnToqa5GIhwtNLKvpeurRKb2plouePnCnnk5ryI6Hv5OdK1dsbvjPqbCFTD9m9ORpWpW0lD1SdA0CmPksAfpKyzPaEg7onHiSJ39S8QbJkEmjtNp9Vo975YlipEHlVKdKjpUqRhQmSuBzWm4OWzi1hxapB4nSuqJw22dxp7ijkZzb8ZFnPQtVBukUSgihv5yCV2oJFv5KaGELvZTmVolR4MIAwHhZSDHza72THkEInvKMOxIMvEJJ9LflLQJkzHqGbKWPTIkuCMxVmsUSxdmPuAJJ77X2MwN557B8pJIAgorLFfKrYN8GabCkc19fWc70YWlhwmMWZLZtr9MFgX5ACsQvEJyehbXLTzegXvWmHbZnBZpZ6UpxUmS6lQV8ZiFnWv2VEdC0tIBPAJjAPuisYP8anLn9zumQS15utl5glM6fdyYzMovALaKiKETTjmwvxJLmIE4Q1bxPfxLqpm97nnl5jsPD1iCBmyIk86Kynn0w8WkRo7xZmhPHfTPUnfkHcKDLznIfKDJZqQdoIfcadfPZwHuYtg7u7NZv7ELsPRLswHUmKEtLtKRvzAMGRSX4uwqr6xPX57lcQHcvNMgoeL4Ni4aO9xiAgqeYNw5w1yYhIXZMvqc2k1EuUX1wz1dbxHUz5PGJdYXPCpJeNK8RB0bwEbx0RMs2JsS92frnsJoq3IF6PFjAAgFxTe26FnLdjuEKX36t6bGc3IIO0bQYtUcelLfxbchrkEs2bFVoKsnvxsgT8wNv5J7u8AbUm0KEviBNqrTpONg5bhxBELEH4";

            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(0);
            using var emailDatabaseManager = new EmailDatabaseManager(EmailManagement.Configuration);

            // Act
            var message = new EmailService().CreateStandardEmail("Test", subject, "");
            string filePath = emailDatabaseManager.GetNewFileName(message);

            // Assert
            Assert.That(filePath.Length == 215);
        }

        /// <summary>
        /// Send an email where some/all characters in subject are not ASCII chars
        /// </summary>
        [Test]
        [TestCase("St. Mary’ s Medical Center")]
        [TestCase("¡Jalapeño!")]
        [TestCase("¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ")]
        public static void RemoveUnicodeChars(string subject)
        {
            // Arrange
            using var emailManagementWithErrors = CreateEmailManagementWithErrorGenerator(0);
            using var emailDatabaseManager = new EmailDatabaseManager(EmailManagement.Configuration);

            // Act
            var message = new EmailService().CreateStandardEmail("Test", subject, "");
            string filePath = emailDatabaseManager.GetNewFileName(message);

            Assert.That(filePath.All(c => c < 128), $"Unicode char in filepath: {filePath}");
            Assert.False(string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(filePath)));
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
