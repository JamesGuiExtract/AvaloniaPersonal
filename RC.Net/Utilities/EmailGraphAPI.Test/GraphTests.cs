﻿using Extract.Email.GraphClient.Test.Utilities;
using Extract.FileActionManager.Database.Test;
using Extract.Licensing;
using Extract.Utilities;
using MimeKit;
using Newtonsoft.Json;
using NUnit.Framework;
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
                Database = FAMTestDBManager.GetNewDatabase(GraphTestsConfig.DatabaseName);
            }

            Database.SetDBInfoSetting("AzureClientId", GraphTestsConfig.AzureClientId, true, false);
            Database.SetDBInfoSetting("AzureTenant", GraphTestsConfig.AzureTenantID, true, false);
            Database.SetDBInfoSetting("AzureInstance", GraphTestsConfig.AzureInstance, true, false);


            EmailManagementConfiguration emailManagementConfiguration = new()
            {
                FileProcessingDB = Database,
                InputMailFolderName = EmailTestHelper.GenerateName(8),
                QueuedMailFolderName = EmailTestHelper.GenerateName(9),
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
        /// In case this happens, append "copy" to the filename.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async static Task DownloadEmailToDiskNameCollision()
        {
            // Creating a name collison for unit tests manually takes too much time.
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

        [OneTimeTearDown]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:Compound words should be cased correctly", Justification = "Nunit name")]
        public static void TearDown()
        {
            EmailTestHelper.CleanupTests(EmailManagement).Wait();
            EmailTestHelper.DeleteMailFolder(EmailManagement.EmailManagementConfiguration.QueuedMailFolderName, EmailManagement).Wait();
            EmailTestHelper.DeleteMailFolder(EmailManagement.EmailManagementConfiguration.InputMailFolderName, EmailManagement).Wait();
            FAMTestDBManager?.Dispose();
            Directory.Delete(EmailManagement.EmailManagementConfiguration.FilepathToDownloadEmails, true);
        }

        private static MimeMessage ReadEMLFile(string fileName)
        {
            return MimeMessage.Load(fileName);
        }
    }
}
