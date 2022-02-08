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
                var authResultTask = new Authenticator(configuration.FileProcessingDB).GetATokenForGraphUserNamePassword(configuration.Password, configuration.UserName, configuration.Authority);
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

        public async Task<MailFolder> GetSharedAddressInputMailFolder()
        {
            try
            {
                return await SharedMailRequestBuilder[EmailManagementConfiguration.InputMailFolderName].Request().GetAsync();
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

        public async Task CreateMailFolder(string mailFolderName)
        {
            try
            {
                if(!DoesMailFolderExist(mailFolderName))
                {
                    var mailFolder = new MailFolder
                    {
                        DisplayName = EmailManagementConfiguration.QueuedMailFolderName,
                        IsHidden = false
                    };

                    await SharedMailRequestBuilder.Request().AddAsync(mailFolder);
                }
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI53207");
            }
        }

        public bool DoesMailFolderExist(string mailFolderName)
        {
            try
            {
                return GetSharedEmailAddressMailFolders().Result.Where(mailFolder => mailFolder.DisplayName.Equals(mailFolderName)).Any();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53151");
            }
        }

        /// <summary>
        /// Returns messages to process. 
        /// </summary>
        /// <returns></returns>
        public async Task<Message[]> GetMessagesToProcessBatches()
        {
            try
            {
                if(GetSharedAddressInputMailFolder().Result.TotalItemCount > 0)
                {
                    var messageCollection = await SharedMailRequestBuilder[EmailManagementConfiguration.InputMailFolderName].Messages.Request().GetAsync();

                    return new Collection<Message>(messageCollection.Batch(EmailManagementConfiguration.EmailBatchSize).ToList().FirstOrDefault()).ToArray();
                }
                return new Message[0];
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53173");
            }
        }

        /// <summary>
        /// This method will download email messages from the input mail folder, write them to disk.
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> DownloadMessagesToDisk(Message[] messages)
        {
            try
            {
                System.IO.Directory.CreateDirectory(EmailManagementConfiguration.FilepathToDownloadEmails);
                await CreateMailFolder(EmailManagementConfiguration.QueuedMailFolderName);
                Collection<string> filesDownlaoded = new();

                foreach (var message in messages)
                {
                    try
                    {
                        string fileName = EmailManagementConfiguration.FilepathToDownloadEmails + GetNewFileName(EmailManagementConfiguration.FilepathToDownloadEmails, message);

                        filesDownlaoded.Add(fileName);
                        var stream = await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress].Messages[message.Id].Content.Request().GetAsync();
                        StreamMethods.WriteStreamToFile(fileName, stream);
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

                return filesDownlaoded.ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53152");
            }
        }

        public async Task MoveMessageToQueuedFolder(Message message)
        {
            try
            {
                await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress].Messages[message.Id].Move(GetQueuedFolderID()).Request().PostAsync();
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI53206");
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
            try
            {
                // Replace any invalid path characters in the subject with nothing.
                var invalid = Path.GetInvalidFileNameChars();
                foreach (char c in invalid)
                {
                    message.Subject = message.Subject.Replace(c.ToString(), string.Empty);
                }

                var fileName = message.Subject + ((DateTimeOffset)message.ReceivedDateTime).ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture) + ".eml";

                // Check to see if the file already exists, if it does append Copy to the subject.
                if (System.IO.File.Exists(folderPath + fileName))
                {
                    message.Subject += "Duplicate";
                    fileName = GetNewFileName(folderPath, message);
                }

                return fileName;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI53203");
            }
        }
    }
}
