using Extract.Utilities.Authentication;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Extract.Utilities.EmailGraphApi
{
    public class EmailManagement
    {
        private readonly GraphServiceClient _graphServiceClient;
        public GraphServiceClient GraphServiceClient { get { return _graphServiceClient; } }

        private readonly IUserMailFoldersCollectionRequestBuilder SharedMailRequestBuilder;
        private readonly EmailManagementConfiguration EmailManagementConfiguration;

        public EmailManagement(EmailManagementConfiguration configuration)
        {
            try
            {
                this.EmailManagementConfiguration = configuration;
                var authResultTask = new Authenticator(configuration.FileProcessingDB).GetATokenForGraphUsernamePassword(configuration.Password, configuration.UserName);
                _graphServiceClient =
                    new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                    {
                        var authenticationResult = await authResultTask;
                        // Add the access token in the Authorization header of the API request.
                        requestMessage.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                    }));
                SharedMailRequestBuilder = _graphServiceClient.Users[configuration.SharedEmailAddress].MailFolders;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53167");
            }
        }

        public async Task<MailFolder> GetSharedEmailAddressInbox()
        {
            try
            {
                return await SharedMailRequestBuilder.Inbox.Request().GetAsync();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53149");
            }
        }

        public async Task<IUserMailFoldersCollectionPage> GetSharedEmailAddressMailFolders()
        {
            try
            {
                return await SharedMailRequestBuilder.Request().GetAsync();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53150");
            }
        }

        public async Task CreateQueuedFolderIfNotExists()
        {
            try
            {
                if (!GetSharedEmailAddressMailFolders().Result.Where(mailFolder => mailFolder.DisplayName.Equals(EmailManagementConfiguration.QueuedMailFolderName)).Any())
                {
                    var mailFolder = new MailFolder
                    {
                        DisplayName = EmailManagementConfiguration.QueuedMailFolderName,
                        IsHidden = false
                    };

                    await SharedMailRequestBuilder.Request().AddAsync(mailFolder);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53151");
            }
        }

        /// <summary>
        /// Returns messages to process. 
        /// </summary>
        /// <param name="messagesToProcess">The maximum number of messages to retrieve.</param>
        /// <returns></returns>
        public async Task<Collection<Message>> GetMessagesToProcessBatches(int messagesToProcess = 50)
        {
            try
            {
                var messageCollection = await SharedMailRequestBuilder[EmailManagementConfiguration.InputMailFolderName].Messages.Request().GetAsync();

                return new Collection<Message>(messageCollection.Batch(messagesToProcess).ToList().FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53173");
            }
        }

        /// <summary>
        /// This method will download email messages from the input mail folder, write them to disk, and move them to the queued folder.
        /// </summary>
        /// <param name="folderPath">The directory to write the files to.</param>
        /// <returns></returns>
        public async Task<Collection<string>> DownloadMessagesToDiskAndQueue(string folderPath, Collection<Message> messages)
        {
            try
            {
                System.IO.Directory.CreateDirectory(folderPath);
                await CreateQueuedFolderIfNotExists();
                Collection<string> filesDownlaoded = new();

                foreach (var message in messages)
                {
                    try
                    {
                        string fileName = folderPath + GetNewFileName(folderPath, message);

                        filesDownlaoded.Add(fileName);
                        var stream = await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress].Messages[message.Id].Content.Request().GetAsync();
                        StreamMethods.WriteStreamToFile(fileName, stream);
                        await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress].Messages[message.Id].Move(GetQueuedFolderID()).Request().PostAsync();
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

        private string GetQueuedFolderID()
        {
            try
            {
                return GetSharedEmailAddressMailFolders().Result.Where(mailFolder => mailFolder.DisplayName.Equals(EmailManagementConfiguration.QueuedMailFolderName)).Single().Id;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53154");
            }
        }

        private string GetNewFileName(string folderPath, Message message)
        {
            // Replace any invalid path characters in the subject with nothing.
            var invalid = Path.GetInvalidFileNameChars();
            foreach (char c in invalid)
            {
                message.Subject = message.Subject.Replace(c.ToString(), string.Empty);
            }

            var fileName = message.Subject + ((DateTimeOffset)message.ReceivedDateTime).ToString("yyyy-MM-dd-HH-mm") + ".eml";

            // Check to see if the file already exists, if it does append Copy to the subject.
            if (System.IO.File.Exists(folderPath + fileName))
            {
                message.Subject += "Duplicate";
                fileName = GetNewFileName(folderPath, message);
            }

            return fileName;
        }
    }
}
