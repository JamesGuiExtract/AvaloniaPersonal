using Extract.Utilities;
using Extract.Utilities.Authentication;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient
{
    public class EmailManagement : IDisposable
    {
        private readonly GraphServiceClient _graphServiceClient;
        public GraphServiceClient GraphServiceClient { get { return _graphServiceClient; } }

        private readonly IUserMailFoldersCollectionRequestBuilder SharedMailRequestBuilder;
        private bool disposedValue;

        public EmailManagementConfiguration EmailManagementConfiguration { get; internal set; }

        /// <summary>
        /// A constructor that will obtain the initial authorization token for the graph API
        /// and initialize default values based off the configuration provided.
        /// </summary>
        /// <param name="configuration">The configuration used for reading emails from the graph API.</param>
        public EmailManagement(EmailManagementConfiguration configuration)
        {
            try
            {
                _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

                this.EmailManagementConfiguration = configuration;
                var authResultTask = new Authenticator(configuration.FileProcessingDB)
                    .GetATokenForGraphUserNamePassword(configuration.Password, configuration.UserName);
                _graphServiceClient =
                    new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                    {
                        var authenticationResult = await authResultTask.ConfigureAwait(false);
                        // Add the access token in the Authorization header of the API request.
                        requestMessage.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                    }));
                SharedMailRequestBuilder = _graphServiceClient.Users[configuration.SharedEmailAddress].MailFolders;
                Task.Run(async () => await CreateInputAndQueuedMailFolders().ConfigureAwait(false)).Wait();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53167");
            }
        }

        /// <summary>
        /// This method takes the input mail folder supplied durring construction and returns the given mail folder.
        /// </summary>
        /// <returns>Returns a mail folder.</returns>
        public async Task<MailFolder> GetSharedAddressInputMailFolder()
        {
            try
            {
                var inputMailFolderID = await GetMailFolderID(this.EmailManagementConfiguration.InputMailFolderName).ConfigureAwait(false);
                return await SharedMailRequestBuilder[inputMailFolderID].Request().GetAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53149");
            }
        }

        public async Task<string> GetMailFolderID(string mailFolderName)
        {
            try
            {
                var mailFolders = await GetSharedEmailAddressMailFolders().ConfigureAwait(false);
                return mailFolders.Where(folder => folder.DisplayName.Equals(mailFolderName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Id;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53226");
            }
        }

        /// <summary>
        /// Helper method to obtain all of the mail folders for a shared email address.
        /// </summary>
        /// <returns>Returns a task containing a collection of mail folders.</returns>
        public async Task<IEnumerable<MailFolder>> GetSharedEmailAddressMailFolders()
        {
            List<MailFolder> result = new();
            var next = SharedMailRequestBuilder.Request();
            while (next != null)
            {
                var page = await next.GetAsync().ConfigureAwait(false);
                result.AddRange(page);
                next = page.NextPageRequest;
            }
            return result;
        }

        /// <summary>
        /// This method will create the mail folder if it does not exist in the shared email.
        /// </summary>
        /// <param name="mailFolderName">The name of the folder to create</param>
        /// <returns>Returns a task that will preform the mail creation</returns>
        public async Task CreateMailFolder(string mailFolderName)
        {
            try
            {
                _ = mailFolderName ?? throw new ArgumentNullException(nameof(mailFolderName));
                if (!await DoesMailFolderExist(mailFolderName).ConfigureAwait(false))
                {
                    var mailFolder = new MailFolder
                    {
                        DisplayName = mailFolderName,
                        IsHidden = false
                    };

                    await SharedMailRequestBuilder.Request().AddAsync(mailFolder).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53207");
            }
        }

        /// <summary>
        /// A helper method to determine if a mail folder exists.
        /// </summary>
        /// <param name="mailFolderName">The mail folder to check</param>
        /// <returns>Returns a task that upon evaluation will state if the mail folder exists or not.</returns>
        public async Task<bool> DoesMailFolderExist(string mailFolderName)
        {
            try
            {
                return (await GetSharedEmailAddressMailFolders().ConfigureAwait(false))
                    .Any(mailFolder => mailFolder.DisplayName.Equals(mailFolderName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53151");
            }
        }

        /// <summary>
        /// Get the top 10 messages from the input mail folder. Message fields are limited to Id, Subject and ReceivedDateTime
        /// </summary>
        /// <returns>Returns a task containing a collection of 10 messages.</returns>
        public async Task<IMailFolderMessagesCollectionPage> GetMessagesToProcessAsync()
        {
            try
            {
                if ((await GetSharedAddressInputMailFolder().ConfigureAwait(false)).TotalItemCount > 0)
                {
                    var messageCollection = await SharedMailRequestBuilder[await GetMailFolderID(EmailManagementConfiguration.InputMailFolderName).ConfigureAwait(false)]
                        .Messages
                        .Request()
                        .Header("Prefer", "IdType=\"ImmutableId\"")
                        //.Select(m => new { m.Id, m.Subject, m.ReceivedDateTime, m.ToRecipients, m.Sender })
                        .GetAsync()
                        .ConfigureAwait(false);
                    return messageCollection;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53173");
            }
        }


        /// <summary>
        /// This method will first check to see if the directory to download the emails exists.
        /// Finally it will attempt to download all of the provided messages.
        /// </summary>
        /// <param name="messages">The messages to download.</param>
        /// <returns>Returns a string array containing all of the file names that were downloaded.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Standard Practice here.")]
        public async Task<Collection<string>> DownloadMessagesToDisk(Message[] messages)
        {
            try
            {
                _ = messages ?? throw new ArgumentNullException(nameof(messages));

                System.IO.Directory.CreateDirectory(EmailManagementConfiguration.FilepathToDownloadEmails);
                Collection<string> filesDownlaoded = new();

                foreach (var message in messages)
                {
                    try
                    {
                        string fileName = GetNewFileName(EmailManagementConfiguration.FilepathToDownloadEmails, message);

                        var stream = await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress].Messages[message.Id].Content.Request().GetAsync().ConfigureAwait(false);
                        StreamMethods.WriteStreamToFile(fileName, stream);
                        filesDownlaoded.Add(fileName);
                    }
                    catch (Exception ex)
                    {
                        var ee = ex.AsExtract("ELI53172");
                        ee.AddDebugData("Information", "Failed to process message");
                        ee.AddDebugData("Email Subject", message.Subject);
                        ee.AddDebugData("Email ID", message.Id);
                        ee.Log();
                    }
                }

                return filesDownlaoded;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53152");
            }
        }

        /// <summary>
        /// This method will move the provided message the the queued folder that was confiugred durring construction.
        /// </summary>
        /// <param name="message">The message to move to the queued folder</param>
        /// <returns>Returns a task containing the message.</returns>
        public async Task<Message> MoveMessageToQueuedFolder(Message message)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                return await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress]
                    .Messages[message.Id]
                    .Move(await GetQueuedFolderID().ConfigureAwait(false))
                    .Request()
                    .Header("Prefer", "IdType=\"ImmutableId\"")
                    .Select("Id")
                    .PostAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53206");
            }
        }

        private async Task<string> GetQueuedFolderID()
        {
            try
            {
                return (await GetSharedEmailAddressMailFolders().ConfigureAwait(false))
                    .Where(mailFolder => mailFolder.DisplayName.Equals(EmailManagementConfiguration.QueuedMailFolderName, StringComparison.OrdinalIgnoreCase)).Single().Id;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53154");
            }
        }

        private async Task CreateInputAndQueuedMailFolders()
        {
            await CreateMailFolder(this.EmailManagementConfiguration.InputMailFolderName).ConfigureAwait(false);
            await CreateMailFolder(this.EmailManagementConfiguration.QueuedMailFolderName).ConfigureAwait(false);
        }

        // TODO: Re-work this method: https://extract.atlassian.net/browse/ISSUE-18027
        private string GetNewFileName(string folderPath, Message message)
        {
            try
            {
                _ = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
                _ = message ?? throw new ArgumentNullException(nameof(message));

                // Replace any invalid path characters in the subject with nothing.
                var invalid = Path.GetInvalidFileNameChars();
                foreach (char c in invalid)
                {
                    message.Subject = message.Subject.Replace(c.ToString(), string.Empty);
                }

                var fileName = Path.Combine(folderPath, GetNewFileNameHelper(message, folderPath));

                return fileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53203");
            }
        }

        // Appends "x" to the the filename until its unique.
        private string GetNewFileNameHelper(Message message, string folderPath)
        {
            string newFileName = message.Subject + ((DateTimeOffset)message.ReceivedDateTime).ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture) + ".eml";

            if (System.IO.File.Exists(Path.Combine(folderPath, newFileName)))
            {
                message.Subject += "X";
                newFileName = GetNewFileNameHelper(message, folderPath);
            }

            return newFileName;
        }

        ~EmailManagement()
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
                }
                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.EmailManagementConfiguration.Password.Dispose();
                disposedValue = true;
            }
        }
    }
}
