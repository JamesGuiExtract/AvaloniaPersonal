using Extract.SqlDatabase;
using Extract.Utilities;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient
{
    public class EmailManagement : IEmailManagement
    {
        private readonly GraphServiceClient _graphServiceClient;

        private readonly IUserMailFoldersCollectionRequestBuilder _sharedMailRequestBuilder;

        private readonly IFileProcessingDB _fileProcessingDB;

        private ExtractRoleConnection _fileProcessingDatabaseConnection;

        // Return current connection or create one
        private ExtractRoleConnection FileProcessingDatabaseConnection
        {
            get
            {
                if (_fileProcessingDatabaseConnection == null)
                {
                    _fileProcessingDatabaseConnection = new(_fileProcessingDB.DatabaseServer, _fileProcessingDB.DatabaseName);
                    _fileProcessingDatabaseConnection.Open();
                }

                return _fileProcessingDatabaseConnection;
            }
        }

        private bool _isDisposed;

        public GraphServiceClient GraphServiceClient { get { return _graphServiceClient; } }

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

                _fileProcessingDB = configuration.FileProcessingDB;
                string accessToken = _fileProcessingDB.GetAzureAccessToken(configuration.ExternalLoginDescription);

                _graphServiceClient =
                    new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                    {
                        // Add the access token in the Authorization header of the API request.
                        requestMessage.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", accessToken);

                        await Task.CompletedTask.ConfigureAwait(false);
                    }));

                _sharedMailRequestBuilder = _graphServiceClient.Users[configuration.SharedEmailAddress].MailFolders;
                Task.Run(async () => await CreateRequiredMailFolders().ConfigureAwait(false)).Wait();
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
                return await _sharedMailRequestBuilder[inputMailFolderID].Request().GetAsync().ConfigureAwait(false);
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
            var next = _sharedMailRequestBuilder.Request();
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

                    await _sharedMailRequestBuilder.Request().AddAsync(mailFolder).ConfigureAwait(false);
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
                    var messageCollection = await _sharedMailRequestBuilder[await GetMailFolderID(EmailManagementConfiguration.InputMailFolderName).ConfigureAwait(false)]
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

                System.IO.Directory.CreateDirectory(EmailManagementConfiguration.FilePathToDownloadEmails);

                string fileName = GetNewFileName(EmailManagementConfiguration.FilePathToDownloadEmails, message, messageAlreadyProceed);

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
                    .PostAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53206");
            }
        }

        /// <summary>
        /// Move the provided message to the failed folder
        /// </summary>
        /// <param name="message">The message to move to the failed folder</param>
        /// <returns>The moved message</returns>
        public async Task<Message> MoveMessageToFailedFolder(Message message)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                return await _graphServiceClient.Users[EmailManagementConfiguration.SharedEmailAddress]
                    .Messages[message.Id]
                    .Move(await GetFailedFolderID().ConfigureAwait(false))
                    .Request()
                    .Header("Prefer", "IdType=\"ImmutableId\"")
                    .PostAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53339");
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

        /// <summary>
        /// Get the queued mail folder ID
        /// </summary>
        public async Task<string> GetFailedFolderID()
        {
            try
            {
                return await GetMailFolderID(EmailManagementConfiguration.FailedMailFolderName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53338");
            }
        }

        private async Task CreateRequiredMailFolders()
        {
            await CreateMailFolder(this.EmailManagementConfiguration.InputMailFolderName).ConfigureAwait(false);
            await CreateMailFolder(this.EmailManagementConfiguration.QueuedMailFolderName).ConfigureAwait(false);
            await CreateMailFolder(this.EmailManagementConfiguration.FailedMailFolderName).ConfigureAwait(false);
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

        /// <summary>
        /// Create a record in the EmailSource table of the configured FileProcessingDB
        /// </summary>
        public void WriteEmailToEmailSourceTable(Message message, int fileID, string emailAddress)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                const string insertEmailSourceSQL = @"
                    INSERT INTO dbo.EmailSource
                        (OutlookEmailID, EmailAddress, Subject, Received, Recipients, Sender, FAMSessionID, QueueEventID, FAMFileID)
                    SELECT TOP 1
                        @OutlookEmailID
                        ,@EmailAddress
                        ,@Subject
                        ,@Received
                        ,@Recipients
                        ,@Sender
                        ,@FAMSessionID
                        ,QueueEvent.ID
                        ,@FAMFileID
                    FROM dbo.QueueEvent
                    WHERE dbo.QueueEvent.FileID = @FAMFileID
                    ORDER BY dbo.QueueEvent.ID DESC";

                string recipients = String.Join(", ", message.ToRecipients.Select(recipient => recipient.EmailAddress.Address));

                using var command = FileProcessingDatabaseConnection.CreateCommand();
                command.CommandText = insertEmailSourceSQL;
                command.Parameters.AddWithValue("@OutlookEmailID", message.Id);
                command.Parameters.AddWithValue("@EmailAddress", emailAddress);
                command.Parameters.AddWithValue("@Subject", message.Subject == null ? DBNull.Value : message.Subject);
                command.Parameters.AddWithValue("@Received", message.ReceivedDateTime);
                command.Parameters.AddWithValue("@Recipients", recipients);
                command.Parameters.AddWithValue("@Sender", message.Sender == null ? DBNull.Value : message.Sender.EmailAddress.Address);
                command.Parameters.AddWithValue("@FAMSessionID", _fileProcessingDB.FAMSessionID);
                command.Parameters.AddWithValue("@FAMFileID", fileID);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53233");
            }
        }

        /// <summary>
        /// Whether a record for the message's OutlookEmailID exists in the configured FileProcessingDB
        /// </summary>
        public bool DoesEmailExistInEmailSourceTable(Message message)
        {
            try
            {
                const string checkForEmailIdSQL = @"
                SELECT OutlookEmailID
                FROM dbo.EmailSource
                WHERE OutlookEmailID = @OutlookEmailID";

                _ = message ?? throw new ArgumentNullException(nameof(message));

                using var command = FileProcessingDatabaseConnection.CreateCommand();
                command.CommandText = checkForEmailIdSQL;
                command.Parameters.AddWithValue("@OutlookEmailID", message.Id);
                var result = command.ExecuteScalar();

                return result != null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53283");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_fileProcessingDatabaseConnection != null)
                    {
                        _fileProcessingDatabaseConnection.Dispose();
                        _fileProcessingDatabaseConnection = null;
                    }
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
