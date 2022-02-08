using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities.EmailGraphApi.Test.Utilities;
using Microsoft.Graph;
using MimeKit;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Utilities.EmailGraphApi.Test
{
    [TestFixture]
    [Category("EmailGraphApi")]
    [SingleThreaded]
    public class GraphTests
    {
        private static FAMTestDBManager<GraphTests> FAMTestDBManager;
        private static FileProcessingDB Database;
        private static EmailManagement EmailManagement;
        private static string SharedEmailAddress;
        private static readonly TestFileManager<GraphTests> TestFileManager = new();
        private static GraphTestsConfig GraphTestsConfig;
        private static int BatchSize = 5;


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

            if(System.IO.File.Exists(configFile))
            {
                GraphTestsConfig = JsonConvert.DeserializeObject<GraphTestsConfig>(System.IO.File.ReadAllText(configFile));
                TestsRunningFromConfigFile = true;
                Database = new FileProcessingDB()
                {
                    DatabaseServer = GraphTestsConfig.DatabaseServer,
                    DatabaseName = GraphTestsConfig.DatabaseName,
                };
            }

            if(!TestsRunningFromConfigFile)
            {
                GraphTestsConfig = new GraphTestsConfig();
                Database = FAMTestDBManager.GetNewDatabase(GraphTestsConfig.DatabaseName);
            }
            
            SharedEmailAddress = GraphTestsConfig.SharedEmailAddress;
            Database.SetDBInfoSetting("AzureClientId", GraphTestsConfig.AzureClientId, true, false);
            Database.SetDBInfoSetting("AzureTenant", GraphTestsConfig.AzureTenantID, true, false);
            Database.SetDBInfoSetting("AzureInstance", GraphTestsConfig.AzureInstance, true, false);

            SecureString secureString = new();
            foreach (char c in GraphTestsConfig.EmailPassword.ToCharArray())
            {
                secureString.AppendChar(c);
            }

            EmailManagementConfiguration emailManagementConfiguration = new()
            {
                FileProcessingDB = Database,
                InputMailFolderName = "Inbox",
                QueuedMailFolderName = "Queued",
                Password = secureString,
                SharedEmailAddress = SharedEmailAddress,
                UserName = GraphTestsConfig.EmailUserName,
                Authority = GraphTestsConfig.Authority,
                EmailBatchSize = BatchSize,
                FilepathToDownloadEmails = GraphTestsConfig.FolderToSaveEmails
            };

            EmailManagement = new EmailManagement(emailManagementConfiguration);
        }

        [Test]
        public async static Task EnsureInboxCanBeRead()
        {
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await ClearAllMessages();
                await AddInboxMessage();
                // Emails take time to send.
                await Task.Delay(10000);
            }

            var inbox = await EmailManagement.GetSharedAddressInputMailFolder();

            Assert.That(inbox.UnreadItemCount > 0);
        }

        [Test]
        public async static Task TestBatching()
        {
            // This test is limited because supplying test emails is tedious when you have to do it manually.
            if (GraphTestsConfig.SupplyTestEmails)
            {
                await ClearAllMessages();
                await AddInboxMessage(BatchSize + 1);
                // Emails take time to send.
                await Task.Delay(10000);
                var messages = await EmailManagement.GetMessagesToProcessBatches();

                Assert.That(messages.Length == 5);
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
                await AddInboxMessage();
                // Emails take time to send.
                await Task.Delay(10000);
            }

            var messages = await EmailManagement.GetMessagesToProcessBatches();

            var files = await EmailManagement.DownloadMessagesToDisk(messages);
            Assert.That(files.Length > 0);

            foreach(var file in files)
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
                await AddInboxMessage(1, "\\:**<>$+|==%");
                // Emails take time to send.
                await Task.Delay(10000);

                var messages = await EmailManagement.GetMessagesToProcessBatches();

                var files = await EmailManagement.DownloadMessagesToDisk(messages);
                Assert.That(files.Length > 0);

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
                await ClearAllMessages();
                // These two will have identical file names.
                await AddInboxMessage();
                await AddInboxMessage();
                // Emails take time to send.
                await Task.Delay(10000);

                var messages = await EmailManagement.GetMessagesToProcessBatches();

                var files = await EmailManagement.DownloadMessagesToDisk(messages);
                Assert.That(files.Length > 0);

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
            try
            {
                var queuedMailFolder = EmailManagement.GetSharedEmailAddressMailFolders().Result.Where(mailFolder => mailFolder.DisplayName.Equals("Queued")).Single();
                await EmailManagement.GraphServiceClient.Users[SharedEmailAddress].MailFolders[queuedMailFolder.Id].Request().DeleteAsync();
            }
            catch { }

            // Attempt to create the mail folder.
            await EmailManagement.CreateMailFolder("Queued");
            Assert.That(EmailManagement.GetSharedEmailAddressMailFolders().Result.Where(mailFolder => mailFolder.DisplayName.Equals("Queued")).Any());

            // Try to create the mail folder again and ensure no error is triggered.
            await EmailManagement.CreateMailFolder("Queued");
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            FAMTestDBManager?.Dispose();
            TestFileManager?.Dispose();
        }

        /// <summary>
        /// Sends an email from the user logged in, to the shared mailbox.
        /// Adds an attachment for good measure.
        /// </summary>
        /// <returns></returns>
        private static async Task AddInboxMessage(int messagesToAdd = 1, string subjectModifier = "")
        {
            if (GraphTestsConfig.SupplyTestEmails)
            {
                EmailService emailService = new();
                var file = TestFileManager.GetFile("TestImageAttachments.A418.tif");
                emailService.AddAttachment(file);
                var saveToSentItems = false;
                for (int i = 0; i < messagesToAdd; i++)
                {
                    await EmailManagement.GraphServiceClient.Me
                    .SendMail(emailService.CreateStandardEmail(SharedEmailAddress, $"The cake is a lie{i}. {subjectModifier}", "Portals are everywhere."), saveToSentItems)
                    .Request()
                    .PostAsync();
                }
            }
        }

        /// <summary>
        /// Removes ALL messages from an inbox.
        /// </summary>
        /// <returns>Nothing.</returns>
        private async static Task ClearAllMessages()
        {
            if (GraphTestsConfig.SupplyTestEmails)
            {
                IUserMessagesCollectionPage messageCollection = await EmailManagement.GraphServiceClient.Users[SharedEmailAddress].Messages.Request()
                .GetAsync();
                List<Message> messages = new();
                messages.AddRange(messageCollection.CurrentPage);
                while (messageCollection.NextPageRequest != null && messages.Count < 100)
                {
                    await messageCollection.NextPageRequest.GetAsync();
                    messages.AddRange(messageCollection.CurrentPage);
                }

                foreach (var message in messages)
                {
                    await EmailManagement.GraphServiceClient.Users[SharedEmailAddress].Messages[message.Id].Request().DeleteAsync();
                }
            }
        }

        private static MimeMessage ReadEMLFile(string fileName)
        {
            return MimeMessage.Load(fileName);
        }
    }
}
