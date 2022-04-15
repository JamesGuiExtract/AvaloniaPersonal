using Extract.SqlDatabase;
using Extract.Utilities;
using Microsoft.Graph;
using System;
using System.Collections.Concurrent;
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

        private readonly IUserMailFoldersCollectionRequestBuilder _mailFoldersRequestBuilder;

        private readonly IFileProcessingDB _fileProcessingDB;

        private ExtractRoleConnection _fileProcessingDatabaseConnection;

        // Cache of mail folder DisplayNames to IDs
        private ConcurrentDictionary<string, string> _mailFolderNameToID;

        private readonly EmailManagementConfiguration _emailManagementConfiguration;

        private bool _isDisposed;

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

        public GraphServiceClient GraphServiceClient => _graphServiceClient;

        public EmailManagementConfiguration Configuration => _emailManagementConfiguration;

        // Private constructor because this object requires async initialization
        private EmailManagement(
            EmailManagementConfiguration configuration,
            GraphServiceClient graphServiceClient,
            IUserMailFoldersCollectionRequestBuilder mailFoldersRequestBuilder,
            IFileProcessingDB fileProcessingDB)
        {
            _emailManagementConfiguration = configuration;
            _graphServiceClient = graphServiceClient;
            _mailFoldersRequestBuilder = mailFoldersRequestBuilder;
            _fileProcessingDB = fileProcessingDB;
        }


        /// <summary>
        /// Factory method to create and fully initialize an <see cref="EmailManagement"/> instance
        /// </summary>
        public static async Task<EmailManagement> CreateEmailManagementAsync(EmailManagementConfiguration configuration)
        {
            try
            {
                configuration = configuration?.ShallowCopy() ?? throw new ArgumentNullException(nameof(configuration));

                var fileProcessingDB = configuration.FileProcessingDB;
                string accessToken = fileProcessingDB.GetAzureAccessToken(configuration.ExternalLoginDescription);

                var graphServiceClient =
                    new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                    {
                        // Add the access token in the Authorization header of the API request.
                        requestMessage.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", accessToken);

                        await Task.CompletedTask.ConfigureAwait(false);
                    }));

                var mailFoldersRequestBuilder = graphServiceClient.Users[configuration.SharedEmailAddress].MailFolders;

                EmailManagement emailManagement = new(
                    configuration,
                    graphServiceClient,
                    mailFoldersRequestBuilder,
                    fileProcessingDB);

                await emailManagement.CreateRequiredMailFolders().ConfigureAwait(false);

                return emailManagement;
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
                var inputMailFolderID = await GetMailFolderID(Configuration.InputMailFolderName).ConfigureAwait(false);
                return await _mailFoldersRequestBuilder[inputMailFolderID]
                    .Request()
                    .UseImmutableID()
                    .GetAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53149");
            }
        }

        /// <summary>
        /// Get the ID of a mail folder
        /// </summary>
        /// <remarks>This method will cache a map of all mail folder names to IDs</remarks>
        private async Task<string> GetMailFolderID(string mailFolderName, bool throwExceptionIfMissing = true)
        {
            try
            {
                // Retrieve and cache all mail folders if this hasn't been done already
                if (_mailFolderNameToID is null)
                {
                    var nameToID = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var folder in await GetSharedEmailAddressMailFolders().ConfigureAwait(false))
                    {
                        nameToID.TryAdd(folder.DisplayName, folder.Id);
                    }

                    _mailFolderNameToID = nameToID;
                }

                // Get the value from the cache
                if (_mailFolderNameToID.TryGetValue(mailFolderName, out string folderID))
                {
                    return folderID;
                }
                else if (throwExceptionIfMissing)
                {
                    throw new ExtractException("ELI53301", UtilityMethods.FormatInvariant($"Folder {mailFolderName} not found!"));
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53226");
            }
        }

        /// <summary>
        /// Get the ID of a mail folder
        /// </summary>
        /// <exception cref="ExtractException">If the folder does not exist</exception>
        public async Task<string> GetMailFolderID(string mailFolderName)
        {
            return await GetMailFolderID(mailFolderName, true).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all of the mail folders for the shared email address
        /// </summary>
        public async Task<IEnumerable<MailFolder>> GetSharedEmailAddressMailFolders()
        {
            try
            {
                List<MailFolder> result = new();
                var next = _mailFoldersRequestBuilder.Request();
                while (next != null)
                {
                    var page = await next.UseImmutableID().GetAsync().ConfigureAwait(false);
                    result.AddRange(page);
                    next = page.NextPageRequest;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53381");
            }
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

                    var folder = await _mailFoldersRequestBuilder
                        .Request()
                        .UseImmutableID()
                        .AddAsync(mailFolder)
                        .ConfigureAwait(false);

                    // Update the cache
                    _mailFolderNameToID.TryAdd(folder.DisplayName, folder.Id);
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
                return await GetMailFolderID(mailFolderName, false).ConfigureAwait(false) is not null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53151");
            }
        }

        /// <summary>
        /// Get the oldest 10 messages from the input mail folder
        /// </summary>
        /// <remarks>
        /// Message fields are limited to Id, Subject, ReceivedDateTime, ToRecipients, Sender
        /// </remarks>
        public async Task<IMailFolderMessagesCollectionPage> GetMessagesToProcessAsync()
        {
            try
            {
                string inputFolderID = await GetInputMailFolderID().ConfigureAwait(false);
                var folder = await _mailFoldersRequestBuilder[inputFolderID]
                    .Request()
                    .UseImmutableID()
                    .GetAsync()
                    .ConfigureAwait(false);

                int totalEmailsInFolder = folder.TotalItemCount ?? 0;
                if (totalEmailsInFolder > 0)
                {
                    var messageCollection = await _mailFoldersRequestBuilder[inputFolderID]
                        .Messages
                        .Request()
                        .UseImmutableID()
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
        /// <param name="filePath">The file path to use or null if the path should be generated from the message</param>
        /// <returns>The file path of the message that was downloaded</returns>
        public async Task<string> DownloadMessageToDisk(Message message, string filePath = null)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                System.IO.Directory.CreateDirectory(Configuration.FilePathToDownloadEmails);

                filePath = filePath ?? GetNewFileName(Configuration.FilePathToDownloadEmails, message);

                var stream = await _graphServiceClient
                    .Users[Configuration.SharedEmailAddress]
                    .Messages[message.Id]
                    .Content
                    .Request()
                    .UseImmutableID()
                    .GetAsync()
                    .ConfigureAwait(false);

                StreamMethods.WriteStreamToFile(filePath, stream);

                return filePath;
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

                return await _graphServiceClient.Users[Configuration.SharedEmailAddress]
                    .Messages[message.Id]
                    .Move(await GetQueuedFolderID().ConfigureAwait(false))
                    .Request()
                    .UseImmutableID()
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

                return await _graphServiceClient.Users[Configuration.SharedEmailAddress]
                    .Messages[message.Id]
                    .Move(await GetFailedFolderID().ConfigureAwait(false))
                    .Request()
                    .UseImmutableID()
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
                return await GetMailFolderID(Configuration.QueuedMailFolderName).ConfigureAwait(false);
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
                return await GetMailFolderID(Configuration.FailedMailFolderName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53338");
            }
        }

        private async Task CreateRequiredMailFolders()
        {
            await CreateMailFolder(Configuration.InputMailFolderName).ConfigureAwait(false);
            await CreateMailFolder(Configuration.QueuedMailFolderName).ConfigureAwait(false);
            await CreateMailFolder(Configuration.FailedMailFolderName).ConfigureAwait(false);
        }

        // Build a unique file name from a message
        private static string GetNewFileName(string folderPath, Message message)
        {
            try
            {
                _ = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
                _ = message ?? throw new ArgumentNullException(nameof(message));

                string prefix = String.Concat((message.Subject ?? "").Split(Path.GetInvalidFileNameChars()));

                DateTimeOffset receivedDate = message.ReceivedDateTime ?? DateTimeOffset.UtcNow;
                string infix = receivedDate.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);

                // Keep trying to find an original name
                for (int copy = 0; ; copy++)
                {
                    string suffix = copy > 0 ? UtilityMethods.FormatInvariant($" ({copy})") : string.Empty;
                    string newFileName = Path.Combine(folderPath, UtilityMethods.FormatInvariant($"{prefix} {infix}{suffix}.eml"));

                    // Check the file system
                    if (System.IO.File.Exists(newFileName))
                    {
                        continue;
                    }

                    return newFileName;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53203");
            }
        }

        /// <summary>
        /// Get the input mail folder ID
        /// </summary>
        public async Task<string> GetInputMailFolderID()
        {
            return await GetMailFolderID(Configuration.InputMailFolderName, false).ConfigureAwait(false);
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
                command.Parameters.AddWithValue("@Received", message.ReceivedDateTime ?? DateTimeOffset.UtcNow);
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
        /// Attempt to get the path of an email file by checking for the message's OutlookEmailID
        /// in the EmailSource table of the configured FileProcessingDB
        /// </summary>
        /// <returns>Whether the email exists in the EmailSource table</returns>
        public bool TryGetExistingEmailFilePath(Message message, out string filePath)
        {
            try
            {
                const string checkForEmailIdSQL = @"
                SELECT [FileName]
                FROM dbo.EmailSource
                JOIN dbo.FAMFile ON FAMFileID = dbo.FAMFile.ID
                WHERE OutlookEmailID = @OutlookEmailID";

                _ = message ?? throw new ArgumentNullException(nameof(message));

                using var command = FileProcessingDatabaseConnection.CreateCommand();
                command.CommandText = checkForEmailIdSQL;
                command.Parameters.AddWithValue("@OutlookEmailID", message.Id);
                filePath = command.ExecuteScalar() as string;

                return filePath != null;
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

    internal static class ExtensionMethods
    {
        /// <summary>
        /// Add a 'Prefer IdType="ImmutableId"' header to a request
        /// </summary>
        public static TRequest UseImmutableID<TRequest>(this TRequest request) where TRequest : IBaseRequest
        {
            return request.Header("Prefer", @"IdType=""ImmutableId""");
        }
    }
}
