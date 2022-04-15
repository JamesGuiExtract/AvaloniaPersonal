using Extract.Testing.Utilities;
using Microsoft.Graph;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient.Test.Utilities
{
    public class EmailTestHelper : IDisposable
    {
        private static readonly TestFileManager<EmailTestHelper> TestFileManager = new();
        private bool disposedValue;

        public async static Task DeleteMailFolder(string folderName, EmailManagement emailManagement)
        {
            try
            {
                var queuedMailFolder = await emailManagement.GetMailFolderID(folderName);
                await emailManagement.GraphServiceClient.Users[emailManagement.Configuration.SharedEmailAddress].MailFolders[queuedMailFolder].Request().DeleteAsync();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53249");
            }
        }

        /// <summary>
        /// Add an email to the shared mailbox
        /// </summary>
        public static async Task AddInputMessage(
            EmailManagement emailManagement,
            string inputMailFolderID,
            string subject,
            string recipient = "Recipient@extracttest.com",
            EmailService emailService = null)
        {
            try
            {
                emailService = emailService ?? new();
                inputMailFolderID = inputMailFolderID
                    ?? await emailManagement.GetMailFolderID(emailManagement.Configuration.InputMailFolderName);

                string body = "Portals are everywhere.";
                Message message = emailService.CreateStandardEmail(recipient, subject, body);
                message.Sender = new()
                {
                    EmailAddress = new()
                    {
                        Name = "Test Sender",
                        Address = "TestSender@everything2.com"
                    }
                };

                await emailManagement
                    .GraphServiceClient
                    .Users[emailManagement.Configuration.SharedEmailAddress]
                    .MailFolders[inputMailFolderID]
                    .Messages
                    .Request()
                    .AddAsync(message);
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI53248");
            }
        }

        /// <summary>
        /// Add emails with attachment to the shared mailbox
        /// </summary>
        public static async Task AddInputMessage(EmailManagement emailManagement, int messagesToAdd = 1, string subjectModifier = "")
        {
            EmailService emailService = new();
            var file = TestFileManager.GetFile("TestImageAttachments.A418.tif");
            emailService.AddAttachment(file);

            string inputMailFolderID = await emailManagement.GetMailFolderID(emailManagement.Configuration.InputMailFolderName);
            for (int i = 0; i < messagesToAdd; i++)
            {
                string recipient = $"Recipient{i}@extracttest.com";
                string subject = $"The cake is a lie{i}. {subjectModifier}";
                await AddInputMessage(emailManagement, inputMailFolderID, subject, recipient, emailService);
            }
        }


        public static async Task AddInputMessageBlankSubject(EmailManagement emailManagement)
        {
            try
            {
                EmailService emailService = new();
                var file = TestFileManager.GetFile("TestImageAttachments.A418.tif");
                emailService.AddAttachment(file);
                var inputMailFolderID = await emailManagement.GetMailFolderID(emailManagement.Configuration.InputMailFolderName);

                await emailManagement
                        .GraphServiceClient
                        .Users[emailManagement.Configuration.SharedEmailAddress]
                        .MailFolders[inputMailFolderID]
                        .Messages
                        .Request()
                        .AddAsync(emailService.CreateStandardEmail($"Recipient@extracttest.com", null, "Portals are everywhere."));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53247");
            }
        }

        /// <summary>
        /// Removes ALL messages from an inbox.
        /// </summary>
        /// <returns>Nothing.</returns>
        public async static Task ClearAllMessages(EmailManagement emailManagement)
        {
            try
            {
                bool findingNewMessages = true;
                while (findingNewMessages)
                {
                    var messageCollection = (await emailManagement
                    .GraphServiceClient
                    .Users[emailManagement.Configuration.SharedEmailAddress]
                    .Messages
                    .Request()
                    .Top(999)
                    .GetAsync()).ToArray();

                    if (messageCollection.Length == 0)
                        findingNewMessages = false;

                    foreach (var message in messageCollection)
                    {
                        await emailManagement.GraphServiceClient
                            .Users[emailManagement.Configuration.SharedEmailAddress]
                            .Messages[message.Id]
                            .Request()
                            .DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53246");
            }
        }

        public async static Task CleanupTests(EmailManagement emailManagement)
        {
            try
            {
                await ClearAllMessages(emailManagement);
                DeleteAllEMLFiles(emailManagement.Configuration.FilePathToDownloadEmails);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53245");
            }
        }

        private static void DeleteAllEMLFiles(string directory)
        {
            foreach (string sFile in System.IO.Directory.GetFiles(directory, "*.eml"))
            {
                System.IO.File.Delete(sFile);
            }
        }

        ~EmailTestHelper()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }
                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                TestFileManager?.Dispose();
                // The thread will keep running as long as the process runs if it isn't stopped        
                disposedValue = true;
            }
        }
    }
}
