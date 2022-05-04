using Extract.Testing.Utilities;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Graph;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient.Test.Utilities
{
    public class EmailTestHelper : IDisposable
    {
        private readonly TestFileManager<EmailTestHelper> TestFileManager = new();
        private bool disposedValue;

        /// <summary>
        /// Percentage of web requests that will result in errors when used as a TestCaseSource attribute parameter
        /// Increase or add higher values to see what happens (will increase the time it takes to run the tests)
        /// </summary>
        public static readonly int[] ErrorPercents = new[] { 10 };

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
        public async Task AddInputMessage(EmailManagement emailManagement, int messagesToAdd = 1, string subjectModifier = "")
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


        public async Task AddInputMessageBlankSubject(EmailManagement emailManagement)
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

        private  async static Task ClearAllMessages(EmailManagement emailManagement, string folderID)
        {
            bool findingNewMessages = true;
            while (findingNewMessages)
            {
                var messageCollection = (await emailManagement
                .GraphServiceClient
                .Users[emailManagement.Configuration.SharedEmailAddress]
                .MailFolders[folderID]
                .Messages
                .Request()
                .Top(999)
				.Select(m => new { m.Id })
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

        /// <summary>
        /// Removes ALL messages from the configured folders
        /// </summary>
        public async static Task ClearAllMessages(EmailManagement emailManagement)
        {
            try
            {
                string[] folders = new string[]
                {
                    await emailManagement.GetInputMailFolderID(),
                    await emailManagement.GetQueuedFolderID(),
                    await emailManagement.GetFailedFolderID()
                };

                foreach (var folderID in folders)
                {
                    await ClearAllMessages(emailManagement, folderID);
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
            foreach (string sFile in System.IO.Directory.GetFiles(directory, "*.eml", System.IO.SearchOption.AllDirectories))
            {
                System.IO.File.Delete(sFile);
            }
        }

        /// <summary>
        /// Create an EmailManagement instance with simulated errors for web requests
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="accessToken">A bearer token for authentication</param>
        /// <param name="errorPercent">The combined % of requests that should result in errors of some kind</param>
        /// <returns></returns>
        public static EmailManagement CreateEmailManagementWithErrorGenerator(
            EmailManagementConfiguration configuration,
            string accessToken,
            int errorPercent)
        {
            ExtractException.Assert("ELI53409", "Invalid error percent value", errorPercent >= 0 && errorPercent <= 100);

            return new EmailManagement(configuration, accessToken, handlers =>
            {
                handlers.Add(new LoggingHttpMessageHandler(new DebugLogger("EmailManagement")));

                if (errorPercent > 0)
                {
                    // Calculate a value that will achieve the overall error probability that was specified
                    // and distribute the errors evenly between the two forms
                    double errorProbability = errorPercent / 100.0;
                    double successProbability = 1 - errorProbability;
                    double successProbability1 = 1 - errorProbability / 2;
                    double successProbability2 = successProbability / successProbability1;

                    int errorPercent1 = (int)Math.Round((1 - successProbability1) * 100);
                    int errorPercent2 = (int)Math.Round((1 - successProbability2) * 100);

                    // Generate error responses
                    handlers.Add(new ChaosHandler(new ChaosHandlerOption() { ChaosPercentLevel = errorPercent1 }));

                    // Generate timeout exceptions
                    handlers.Add(new TimeoutGeneratingHandler(errorPercent2));
                }

                return GraphClientFactory.Create(handlers);
            });
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    TestFileManager?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null

                disposedValue = true;
            }
        }
    }

    static class ExtensionMethods
    {
        public static EmailManagement CreateWithErrorGenerator(this EmailManagementConfiguration configuration, string accessToken, int errorPercent)
        {
            return EmailTestHelper.CreateEmailManagementWithErrorGenerator(configuration, accessToken, errorPercent);
        }
    }
}
