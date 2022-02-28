using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Utilities.EmailGraphApi.Test.Utilities;
using Microsoft.Graph;
using MimeKit;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient.Test
{
    [TestFixture]
    [Category("EmailGraphApi")]
    [SingleThreaded]
    public class GraphTests
    {
        private static FAMTestDBManager<GraphTests> FAMTestDBManager;
        private static FileProcessingDB Database;
        private static EmailManagement EmailManagement;
        private static readonly TestFileManager<GraphTests> TestFileManager = new();
        private static GraphTestsConfig GraphTestsConfig;


        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            bool TestsRunningFromConfigFile = false;
            FAMTestDBManager = new FAMTestDBManager<GraphTests>();
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

            string configFile = Path.Combine(FileSystemMethods.CommonApplicationDataPath, "GraphTestsConfig.json");

            if (System.IO.File.Exists(configFile))
            {
                GraphTestsConfig = JsonConvert.DeserializeObject<GraphTestsConfig>(System.IO.File.ReadAllText(configFile));
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
                Database = FAMTestDBManager.GetNewDatabase(GraphTestsConfig.DatabaseName);
            }

            Database.SetDBInfoSetting("AzureClientId", GraphTestsConfig.AzureClientId, true, false);
            Database.SetDBInfoSetting("AzureTenant", GraphTestsConfig.AzureTenantID, true, false);
            Database.SetDBInfoSetting("AzureInstance", GraphTestsConfig.AzureInstance, true, false);


            EmailManagementConfiguration emailManagementConfiguration = new()
            {
                FileProcessingDB = Database,
                InputMailFolderName = EmailFileSupplierTests.GenerateName(8),
                QueuedMailFolderName = EmailFileSupplierTests.GenerateName(9),
                Password = GraphTestsConfig.EmailPassword,
                SharedEmailAddress = GraphTestsConfig.SharedEmailAddress,
                UserName = GraphTestsConfig.EmailUserName,
                FilepathToDownloadEmails = GraphTestsConfig.FolderToSaveEmails
            };

            EmailManagement = new EmailManagement(emailManagementConfiguration);
        }

        [Test]
        public async static Task EnsureInputFolderCanBeRead()
        {
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await ClearAllMessages(EmailManagement);
                await AddInputMessage(EmailManagement);
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
                await ClearAllMessages(EmailManagement);
                await AddInputMessage(EmailManagement, maxGraphDownload + 1);
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
                await AddInputMessage(EmailManagement);
            }

            var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

            var files = await EmailManagement.DownloadMessagesToDisk(messages);
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
                await AddInputMessage(EmailManagement, 1, "\\:**<>$+|==%");

                var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

                var files = await EmailManagement.DownloadMessagesToDisk(messages);
                Assert.That(files.Count > 0);

                FileSystemMethods.DeleteFile(files[0]);
            }
        }

        /// <summary>
        /// Email subjects can have duplicated file names.
        /// In case this happens, append "copy" to the filename.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async static Task DownloadEmailToDiskNameCollision()
        {
            // Creating a name collison for unit tests manually takes too much time.
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await ClearAllMessages(EmailManagement);
                // These two will have identical file names.
                await AddInputMessage(EmailManagement);
                await AddInputMessage(EmailManagement);

                var messages = (await EmailManagement.GetMessagesToProcessAsync()).ToArray();

                var files = await EmailManagement.DownloadMessagesToDisk(messages);
                Assert.That(files.Count > 0);

                Assert.That(files[0].ToString() != files[1].ToString());

                FileSystemMethods.DeleteFile(files[0]);
                FileSystemMethods.DeleteFile(files[1]);
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
            DeleteMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName, EmailManagement);
            
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
            await ClearAllMessages(EmailManagement);
            await EmailManagement.CreateMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName);
            await AddInputMessage(EmailManagement);
            var messages = (await EmailManagement.GetMessagesToProcessAsync().ConfigureAwait(false)).ToArray();
            var movedMessage = await EmailManagement.MoveMessageToQueuedFolder(messages[0]);
            Assert.AreEqual(messages[0].Id, movedMessage.Id);
        }

        [OneTimeTearDown]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:Compound words should be cased correctly", Justification = "Nunit name")]
        public static void TearDown()
        {
            DeleteMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName, EmailManagement);
            DeleteMailFolder(EmailManagement.EmailManagementConfiguration.InputMailFolderName, EmailManagement);
            FAMTestDBManager?.Dispose();
            TestFileManager?.Dispose();
            System.IO.Directory.Delete(EmailManagement.EmailManagementConfiguration.FilepathToDownloadEmails, true);
        }

        public async static void DeleteMailFolder(string folderName, EmailManagement emailManagement)
        {
            try
            {
                var queuedMailFolder = await emailManagement.GetMailFolderID(folderName);
                await emailManagement.GraphServiceClient.Users[emailManagement.EmailManagementConfiguration.SharedEmailAddress].MailFolders[queuedMailFolder].Request().DeleteAsync();
            }
            catch(Exception ex) 
            {
                Console.Write(ex.Message);
            }
        }

        /// <summary>
        /// Sends an email from the user logged in, to the shared mailbox.
        /// Adds an attachment for good measure.
        /// </summary>
        /// <returns></returns>
        public static async Task AddInputMessage(EmailManagement emailManagement, int messagesToAdd = 1, string subjectModifier = "")
        {
            EmailService emailService = new();
            var file = TestFileManager.GetFile("TestImageAttachments.A418.tif");
            emailService.AddAttachment(file);
            var inputMailFolderID = await emailManagement.GetMailFolderID(emailManagement.EmailManagementConfiguration.InputMailFolderName);

            for (int i = 0; i < messagesToAdd; i++)
            {
                await emailManagement
                    .GraphServiceClient
                    .Users[emailManagement.EmailManagementConfiguration.SharedEmailAddress]
                    .MailFolders[inputMailFolderID]
                    .Messages
                    .Request()
                    .AddAsync(emailService.CreateStandardEmail($"Recipient{i}@extracttest.com", $"The cake is a lie{i}. {subjectModifier}", "Portals are everywhere."));
            }
        }

        /// <summary>
        /// Removes ALL messages from an inbox.
        /// </summary>
        /// <returns>Nothing.</returns>
        public async static Task ClearAllMessages(EmailManagement emailManagement)
        {
            HashSet<Message> messages = new();

            bool findingNewMessages = true;
            while (findingNewMessages)
            {
                var messageCollection = (await emailManagement
                .GraphServiceClient
                .Users[emailManagement.EmailManagementConfiguration.SharedEmailAddress]
                .Messages
                .Request()
                .Top(999)
                .GetAsync()).ToArray();

                if (messages.Count == 0)
                    findingNewMessages = false;

                foreach (var message in messageCollection)
                {
                    await emailManagement.GraphServiceClient
                        .Users[emailManagement.EmailManagementConfiguration.SharedEmailAddress]
                        .Messages[message.Id]
                        .Request()
                        .DeleteAsync();
                }
            }
        }

        private static MimeMessage ReadEMLFile(string fileName)
        {
            return MimeMessage.Load(fileName);
        }
    }
}
