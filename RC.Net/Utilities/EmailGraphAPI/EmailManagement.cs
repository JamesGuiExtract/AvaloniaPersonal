using Extract.Utilities;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient
{
    public class EmailManagement : IEmailManagement
    {
        private readonly GraphServiceClient _graphServiceClient;
        public GraphServiceClient GraphServiceClient { get { return _graphServiceClient; } }

        private readonly IUserMailFoldersCollectionRequestBuilder SharedMailRequestBuilder;

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
                EmailManagementConfiguration = configuration?.ShallowCopy() ?? throw new ArgumentNullException(nameof(configuration));

                string accessToken = configuration.FileProcessingDB.GetAzureAccessToken(configuration.ExternalLoginDescription);

                _graphServiceClient =
                    new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                    {
                        // Add the access token in the Authorization header of the API request.
                        requestMessage.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", accessToken);

                        await Task.CompletedTask.ConfigureAwait(false);
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
        /// Get a <see cref="MailFolder"/> for the configured input mail folder
        /// </summary>
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

        /// <summary>
        /// Get the ID of a mail folder
        /// </summary>
        public async Task<string> GetMailFolderID(string mailFolderName)
        {
            try
            {
                var mailFolders = await GetSharedEmailAddressMailFolders().ConfigureAwait(false);
                var folderID = mailFolders.FirstOrDefault(folder => folder.DisplayName.Equals(mailFolderName, StringComparison.OrdinalIgnoreCase))?.Id;

                return folderID ?? throw new ExtractException("ELI53301", UtilityMethods.FormatInvariant($"Folder {mailFolderName} not found!"));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53226");
            }
        }

        /// <summary>
        /// Get all of the mail folders for the shared email address
        /// </summary>
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
        /// Create the specified mail folder if it does not exist in the shared email
        /// </summary>
        /// <param name="mailFolderName">The name of the folder to create</param>
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
        /// Get whether a mail folder exists
        /// </summary>
        /// <param name="mailFolderName">The mail folder to check</param>
        public async Task<bool> DoesMailFolderExist(string mailFolderName)
        {
            try
            {
                var mailFolders = await GetSharedEmailAddressMailFolders().ConfigureAwait(false);
                return mailFolders.Any(mailFolder => mailFolder.DisplayName.Equals(mailFolderName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53151");
            }
        }

        /// <summary>
        /// Get the top 10 messages from the input mail folder
        /// </summary>
        /// <remarks>
        /// Message fields are limited to Id, Subject, ReceivedDateTime, ToRecipients, Sender
        /// </remarks>
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
                        .Select(m => new { m.Id, m.Subject, m.ReceivedDateTime, m.ToRecipients, m.Sender })
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
        /// Download a message
        /// </summary>
        /// <param name="message">The message to download</param>
        /// <returns>The file name of the message that was downloaded</returns>
        public async Task<string> DownloadMessageToDisk(Message message, bool messageAlreadyProceed = false)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                System.IO.Directory.CreateDirectory(EmailManagementConfiguration.FilepathToDownloadEmails);

                string fileName = GetNewFileName(EmailManagementConfiguration.FilepathToDownloadEmails, message, messageAlreadyProceed);

                var stream = await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress].Messages[message.Id].Content.Request().GetAsync().ConfigureAwait(false);
                StreamMethods.WriteStreamToFile(fileName, stream);
                string fileDownlaoded = fileName;


                return fileDownlaoded;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53152");
            }
        }

        /// <summary>
        /// Move the provided message to the queued folder
        /// </summary>
        /// <param name="message">The message to move to the queued folder</param>
        /// <returns>The moved message</returns>
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

        /// <summary>
        /// Get the queued mail folder ID
        /// </summary>
        public async Task<string> GetQueuedFolderID()
        {
            try
            {
                return await GetMailFolderID(EmailManagementConfiguration.QueuedMailFolderName).ConfigureAwait(false);
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
        private string GetNewFileName(string folderPath, Message message, bool enforceUniqueFileName)
        {
            try
            {
                _ = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
                _ = message ?? throw new ArgumentNullException(nameof(message));

                if (message.Subject != null)
                {
                    // Replace any invalid path characters in the subject with nothing.
                    var invalid = Path.GetInvalidFileNameChars();
                    foreach (char c in invalid)
                    {
                        message.Subject = message.Subject.Replace(c.ToString(), string.Empty);
                    }
                }

                var fileName = Path.Combine(folderPath, GetNewFileNameHelper(message, folderPath, enforceUniqueFileName));

                return fileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53203");
            }
        }

        // Appends "x" to the the filename until its unique. unless it has already been processed
        private string GetNewFileNameHelper(Message message, string folderPath, bool enforceUniqueFileName)
        {
            string newFileName = (string.IsNullOrEmpty(message.Subject) ? string.Empty : message.Subject)
                + ((DateTimeOffset)message.ReceivedDateTime).ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture) + ".eml";

            if (System.IO.File.Exists(Path.Combine(folderPath, newFileName)) && !enforceUniqueFileName)
            {
                message.Subject += "X";
                newFileName = GetNewFileNameHelper(message, folderPath, enforceUniqueFileName);
            }

            return newFileName;
        }

        /// <summary>
        /// Get the input mail folder ID
        /// </summary>
        public async Task<string> GetInputMailFolderID()
        {
            return (await GetSharedEmailAddressMailFolders().ConfigureAwait(false))
                    .Where(mailFolder => mailFolder.DisplayName.Equals(EmailManagementConfiguration.InputMailFolderName, StringComparison.OrdinalIgnoreCase)).Single().Id;
        }
    }
}
