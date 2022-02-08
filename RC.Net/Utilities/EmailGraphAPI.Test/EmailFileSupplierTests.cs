using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileSuppliers;
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
    public class EmailFileSupplierTests
    {
        private static FAMTestDBManager<GraphTests> FAMTestDBManager;
        private static EmailManagementConfiguration EmailManagementConfiguration;
        private static FileProcessingDB Database;
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
            GraphTestsConfig = new GraphTestsConfig();
            FAMTestDBManager = new FAMTestDBManager<GraphTests>();
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

            GraphTestsConfig.DatabaseName = "Test_EmailFileSupplier";
            
            Database = FAMTestDBManager.GetNewDatabase(GraphTestsConfig.DatabaseName);
            
            
            SharedEmailAddress = GraphTestsConfig.SharedEmailAddress;
            Database.SetDBInfoSetting("AzureClientId", GraphTestsConfig.AzureClientId, true, false);
            Database.SetDBInfoSetting("AzureTenant", GraphTestsConfig.AzureTenantID, true, false);
            Database.SetDBInfoSetting("AzureInstance", GraphTestsConfig.AzureInstance, true, false);

            SecureString secureString = new();
            foreach (char c in GraphTestsConfig.EmailPassword.ToCharArray())
            {
                secureString.AppendChar(c);
            }

            EmailManagementConfiguration = new()
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
        }

        [Test]
        public static void TestConfigured()
        {
            EmailFileSupplier emailFileSupplier = new(EmailManagementConfiguration);
            Assert.IsTrue(emailFileSupplier.IsConfigured());
        }

        [Test]
        public static void TestDownloadOneEmail()
        {
            EmailFileSupplier emailFileSupplier = new(EmailManagementConfiguration);
            //IFileSupplierTarget fileSupplierTarget = new();
            //emailFileSupplier.Start()
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
            EmailService emailService = new();
            var file = TestFileManager.GetFile("TestImageAttachments.A418.tif");
            emailService.AddAttachment(file);
            var saveToSentItems = false;
            EmailManagement emailManangement = new EmailManagement(EmailManagementConfiguration);
            for (int i = 0; i < messagesToAdd; i++)
            {
                await emailManangement.GraphServiceClient.Me
                .SendMail(emailService.CreateStandardEmail(SharedEmailAddress, $"The cake is a lie{i}. {subjectModifier}", "Portals are everywhere."), saveToSentItems)
                .Request()
                .PostAsync();
            }
        }

        /// <summary>
        /// Removes ALL messages from an inbox.
        /// </summary>
        /// <returns>Nothing.</returns>
        private async static Task ClearAllMessages()
        {
            EmailManagement emailManangement = new EmailManagement(EmailManagementConfiguration);
            IUserMessagesCollectionPage messageCollection = await emailManangement.GraphServiceClient.Users[SharedEmailAddress].Messages.Request()
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
                await emailManangement.GraphServiceClient.Users[SharedEmailAddress].Messages[message.Id].Request().DeleteAsync();
            }
        }

        private static MimeMessage ReadEMLFile(string fileName)
        {
            return MimeMessage.Load(fileName);
        }
    }
}
