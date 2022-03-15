using Extract.Testing.Utilities;
using Extract.Utilities.EmailGraphApi.Test.Utilities;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                await emailManagement.GraphServiceClient.Users[emailManagement.EmailManagementConfiguration.SharedEmailAddress].MailFolders[queuedMailFolder].Request().DeleteAsync();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53249");
            }
        }

        /// <summary>
        /// Sends an email from the user logged in, to the shared mailbox.
        /// Adds an attachment for good measure.
        /// </summary>
        /// <returns></returns>
        public static async Task AddInputMessage(EmailManagement emailManagement, int messagesToAdd = 1, string subjectModifier = "")
        {
            try
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
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI53248");
            }
        }

        public static async Task AddInputMessageBlankSubject(EmailManagement emailManagement)
        {
            try
            {
                EmailService emailService = new();
                var file = TestFileManager.GetFile("TestImageAttachments.A418.tif");
                emailService.AddAttachment(file);
                var inputMailFolderID = await emailManagement.GetMailFolderID(emailManagement.EmailManagementConfiguration.InputMailFolderName);

                await emailManagement
                        .GraphServiceClient
                        .Users[emailManagement.EmailManagementConfiguration.SharedEmailAddress]
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
                    .Users[emailManagement.EmailManagementConfiguration.SharedEmailAddress]
                    .Messages
                    .Request()
                    .Top(999)
                    .GetAsync()).ToArray();

                    if (messageCollection.Length == 0)
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
                DeleteAllEMLFiles(emailManagement.EmailManagementConfiguration.FilepathToDownloadEmails);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53245");
            }
        }

        /// <summary>
        /// Generates a random name.
        /// Code taken from: https://stackoverflow.com/questions/14687658/random-name-generator-in-c-sharp
        /// </summary>
        /// <param name="len">How long you want the name to be.</param>
        /// <returns>Returns the random name.</returns>
        public static string GenerateName(int len)
        {
            try
            {
                Random r = new Random();
                string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
                string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
                string Name = "";
                Name += consonants[r.Next(consonants.Length)].ToUpper(CultureInfo.InvariantCulture);
                Name += vowels[r.Next(vowels.Length)];
                int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
                while (b < len)
                {
                    Name += consonants[r.Next(consonants.Length)];
                    b++;
                    Name += vowels[r.Next(vowels.Length)];
                    b++;
                }

                return Name;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53244");
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
