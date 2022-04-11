using Extract.Email.GraphClient.Test.Utilities;
using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities;
using MimeKit;
using Newtonsoft.Json;
using NUnit.Framework;
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
        private static FileProcessingDB Database;
        private static EmailManagement EmailManagement;
        
        private static GraphTestsConfig GraphTestsConfig;

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            bool TestsRunningFromConfigFile = false;
            FAMTestDBManager = new FAMTestDBManager<GraphTests>();

            string configFile = Path.Combine(FileSystemMethods.CommonApplicationDataPath, "GraphTestsConfig.json");

            if (File.Exists(configFile))
            {
                GraphTestsConfig = JsonConvert.DeserializeObject<GraphTestsConfig>(File.ReadAllText(configFile));
                TestsRunningFromConfigFile = true;
                Database = new FileProcessingDB()
                {
                    DatabaseServer = GraphTestsConfig.DatabaseServer,
                    DatabaseName = GraphTestsConfig.DatabaseName,
                };
            }

            if (!TestsRunningFromConfigFile)
            {
                GraphTestsConfig = new GraphTestsConfig();
                GraphTestsConfig.DatabaseName = FAMTestDBManager.GenerateDatabaseName();
                Database = FAMTestDBManager.GetNewDatabase(GraphTestsConfig.DatabaseName);

                Database.LoginUser("admin", "a");
                Database.SetExternalLogin(
                    Constants.EmailFileSupplierExternalLoginDescription,
                    GraphTestsConfig.EmailUserName,
                    GraphTestsConfig.EmailPassword);
            }

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

            EmailManagement = new EmailManagement(emailManagementConfiguration);
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            EmailTestHelper.CleanupTests(EmailManagement).Wait();
            EmailTestHelper.DeleteMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName, EmailManagement).Wait();
            EmailTestHelper.DeleteMailFolder(EmailManagement.EmailManagementConfiguration.InputMailFolderName, EmailManagement).Wait();
            EmailTestHelper.DeleteMailFolder(EmailManagement.EmailManagementConfiguration.FailedMailFolderName, EmailManagement).Wait();
            Directory.Delete(EmailManagement.EmailManagementConfiguration.FilePathToDownloadEmails, true);

            FAMTestDBManager.Dispose();
        }

        #endregion Overhead

        [Test]
        public async static Task EnsureInputFolderCanBeRead()
        {
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await EmailTestHelper.ClearAllMessages(EmailManagement);
                await EmailTestHelper.AddInputMessage(EmailManagement);
            }

            var inbox = await EmailManagement.GetSharedAddressInputMailFolder();

            Assert.That(inbox.TotalItemCount > 0);
        }

        [Test]
        public async static Task TestDownloadLimit()
        {
            // This test is limited because supplying test emails is tedious when you have to do it manually.
            int maxGraphDownload = 10;
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await EmailTestHelper.ClearAllMessages(EmailManagement);
                await EmailTestHelper.AddInputMessage(EmailManagement, maxGraphDownload + 1);
                var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

                Assert.That(messages.Length == maxGraphDownload);
            }
        }

        /// <summary>
        /// Sends an email to the folder, then downloads the email to disk, and moves the email to Queued.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async static Task DownloadEmailToDisk()
        {
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await EmailTestHelper.AddInputMessage(EmailManagement);
            }

            var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

            Collection<string> files = new();
            foreach(var message in messages)
            {
                files.Add(await EmailManagement.DownloadMessageToDisk(message));
            }

            Assert.That(files.Count > 0);

            foreach (var file in files)
            {
                var message = ReadEMLFile(file);
                Assert.That(message.Subject != null);
                if (GraphTestsConfig.SupplyTestEmails)
                {
                    FileSystemMethods.DeleteFile(file);
                }
            }
        }

        /// <summary>
        /// Email subjects can have invalid filename characters.
        /// In case this happens remove the invalid characters from the filename.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async static Task DownloadEmailToDiskInvalidFileName()
        {
            // Supplying emails with invalid subjects for file names is tedious.
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await EmailTestHelper.AddInputMessage(EmailManagement, 1, "\\:**<>$+|==%");

                var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

                Collection<string> files = new();
                foreach (var message in messages)
                {
                    files.Add(await EmailManagement.DownloadMessageToDisk(message));
                }
                Assert.That(files.Count > 0);

                FileSystemMethods.DeleteFile(files[0]);
            }
        }

        /// <summary>
        /// Email subjects can have duplicated file names.
        /// In case this happens, append "x" to the filename.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async static Task DownloadEmailToDiskNameCollision()
        {
            // Creating a name collision for unit tests manually takes too much time.
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await EmailTestHelper.ClearAllMessages(EmailManagement);
                // These two will have identical file names.
                await EmailTestHelper.AddInputMessage(EmailManagement);
                await EmailTestHelper.AddInputMessage(EmailManagement);

                var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

                Collection<string> files = new();
                foreach (var message in messages)
                {
                    files.Add(await EmailManagement.DownloadMessageToDisk(message));
                }
                Assert.That(files.Count > 0);

                Assert.That(files[0].ToString() != files[1].ToString());

                FileSystemMethods.DeleteFile(files[0]);
                FileSystemMethods.DeleteFile(files[1]);
            }
        }

        /// <summary>
        /// If an email has already been processed use the same file name.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async static Task DownloadEmailToDiskAlreadyProcessed()
        {
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await EmailTestHelper.ClearAllMessages(EmailManagement);
                await EmailTestHelper.AddInputMessage(EmailManagement);

                var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

                // Download the message first pass.
                List<string> files = new();
                foreach (var message in messages)
                {
                    files.Add(await EmailManagement.DownloadMessageToDisk(message));
                }

                Assert.That(files.Count == 1);

                // Attempt to download again. It should be the same file name.
                var messagesToFiles = files
                    .Zip(messages, (file, message) => (file, message))
                    .ToList();
                foreach (var (file, message) in messagesToFiles)
                {
                    files.Add(await EmailManagement.DownloadMessageToDisk(message, file));
                }

                Assert.That(files.Distinct().Count() == 1);
                FileSystemMethods.DeleteFile(files[0]);
            }
        }

        /// <summary>
        /// Ensures the queued folder can be created.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async static Task CreateQueuedFolder()
        {
            // Try to delete the queued folder if it exists.
            await EmailTestHelper.CleanupTests(EmailManagement);
            
            // There appears to be some kind of timeout for deleting a folder and instantly re-creating it.
            await Task.Delay(1000);

            // Attempt to create the mail folder.
            await EmailManagement.CreateMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName);
            
            var sharedMailFolders = await EmailManagement.GetSharedEmailAddressMailFolders();
            Assert.That(sharedMailFolders.Where(mailFolder => mailFolder.DisplayName.Equals(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName)).Any());

            // Try to create the mail folder again and ensure no error is triggered.
            await EmailManagement.CreateMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName);
        }

        [Test]
        public async static Task EnsureIdsRemainConstantAfterMove()
        {
            await EmailTestHelper.ClearAllMessages(EmailManagement);
            await EmailManagement.CreateMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName);
            await EmailTestHelper.AddInputMessage(EmailManagement);
            var messages = (await EmailManagement.GetMessagesToProcessAsync().ConfigureAwait(false)).ToArray();
            var movedMessage = await EmailManagement.MoveMessageToQueuedFolder(messages[0]);
            Assert.AreEqual(messages[0].Id, movedMessage.Id);
        }
        
        [Test, Category("Automated")]
        public async static Task MoveMessageToFailed()
        {
            await EmailTestHelper.ClearAllMessages(EmailManagement);
            await EmailManagement.CreateMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName);
            await EmailTestHelper.AddInputMessage(EmailManagement);
            var messages = (await EmailManagement.GetMessagesToProcessAsync().ConfigureAwait(false)).ToArray();
            var movedMessage = await EmailManagement.MoveMessageToFailedFolder(messages[0]);
            Assert.AreEqual(messages[0].Id, movedMessage.Id);
            var parentID = await EmailManagement.GetFailedFolderID();
            Assert.AreEqual(parentID, movedMessage.ParentFolderId);
            Assert.AreNotEqual(messages[0].ParentFolderId, movedMessage.ParentFolderId);   
        }

        private static MimeMessage ReadEMLFile(string fileName)
        {
            return MimeMessage.Load(fileName);
        }
    }
}
