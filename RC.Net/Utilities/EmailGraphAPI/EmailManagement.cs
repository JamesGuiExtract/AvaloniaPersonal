using Extract.Utilities;
using Microsoft.Graph;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient
{
    public class EmailManagement : IEmailManagement
    {
        const int MAX_RETRIES = 10;
        const int DELAY_SECONDS = 3; // Base # of seconds to delay, increases exponentially with each retry
        const string TIMEOUT_CODE = "timeout";
        const string RETRY_ATTEMPT = "Retry-Attempt";
        const string REQUEST_NAME = "Request-Name";
        static readonly string MESSAGE_FIELDS_FILTER =
            string.Join(",",
                nameof(Message.Id),
                nameof(Message.Subject),
                nameof(Message.ReceivedDateTime),
                nameof(Message.ToRecipients),
                nameof(Message.Sender),
                nameof(Message.ParentFolderId));

        private readonly GraphServiceClient _graphServiceClient;
        private readonly HttpClient _httpClient;
        private readonly HttpRetryLogger _retryLogger = new(nameof(EmailManagement));
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly CancellationTokenSource _cancelPendingOperations = new();
        private readonly CancellationToken _cancelPendingOperationsToken;
        private readonly IUserMailFoldersCollectionRequestBuilder _mailFoldersRequestBuilder;
        private readonly IFileProcessingDB _fileProcessingDB;

        // Cache of mail folder DisplayNames to IDs
        private ConcurrentDictionary<string, string> _mailFolderNameToID;

        private readonly EmailManagementConfiguration _emailManagementConfiguration;

        private bool _isDisposed;

        private AuthenticationHeaderValue _authenticationHeaderValue;
        private DateTime _accessTokenConsideredExpiredOn;

        // Return the current authentication header or create a new one if needed
        private AuthenticationHeaderValue AuthenticationHeader
        {
            get
            {
                if (_authenticationHeaderValue is null || DateTime.UtcNow > _accessTokenConsideredExpiredOn)
                {
                    string accessToken = _fileProcessingDB.GetAzureAccessToken(_emailManagementConfiguration.ExternalLoginDescription);
                    DateTime validTo = ParseAccessToken(accessToken);

                    ExtractException.Assert("ELI53389", "New access token is already expired!", DateTime.UtcNow < validTo);
                }

                return _authenticationHeaderValue;
            }
        }

        // Used by nunit tests to share authentication between instances to avoid AAD throttling
        internal string AccessToken => _authenticationHeaderValue?.Parameter;

        public GraphServiceClient GraphServiceClient => _graphServiceClient;

        public EmailManagementConfiguration Configuration => _emailManagementConfiguration;

        /// <summary>
        /// Create an <see cref="EmailManagement"/> instance
        /// </summary>
        public EmailManagement(EmailManagementConfiguration configuration, string accessToken = null, Func<IList<DelegatingHandler>, HttpClient> httpClientCreator = null)
        {
            try
            {
                if (accessToken is not null)
                {
                    ParseAccessToken(accessToken);
                }

                httpClientCreator = httpClientCreator ?? (handlers => GraphClientFactory.Create(handlers));

                _emailManagementConfiguration = configuration?.ShallowCopy() ?? throw new ArgumentNullException(nameof(configuration));
                _fileProcessingDB = _emailManagementConfiguration.FileProcessingDB;

                var handlers = GraphClientFactory.CreateDefaultHandlers(new DelegateAuthenticationProvider(requestMessage =>
                {
                    requestMessage.Headers.Authorization = AuthenticationHeader;
                    return Task.CompletedTask;
                }));

                using (var retryHandler = handlers.OfType<RetryHandler>().First())
                {
                    handlers[handlers.IndexOf(retryHandler)] = new RetryHandler(new RetryHandlerOption { MaxRetry = MAX_RETRIES });
                }

                handlers.Add(_retryLogger);

                _httpClient = httpClientCreator(handlers);
                _graphServiceClient = new GraphServiceClient(_httpClient);
                _mailFoldersRequestBuilder = _graphServiceClient.Users[_emailManagementConfiguration.SharedEmailAddress].MailFolders;

                _cancelPendingOperationsToken = _cancelPendingOperations.Token;
                _retryPolicy = Policy.Handle<Exception>(ShouldRetry)
                    .WaitAndRetryAsync(MAX_RETRIES, CalculateSleepDuration, LogExceptionBeforeRetry);
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
                var context = new Context { { REQUEST_NAME, "GetInputFolder" } };

                var inputMailFolderID = await GetMailFolderID(Configuration.InputMailFolderName).ConfigureAwait(false);

                return await _retryPolicy
                    .ExecuteAsync((_, cancellationToken) =>
                        _mailFoldersRequestBuilder[inputMailFolderID]
                        .Request()
                        .UseImmutableID()
                        .GetAsync(cancellationToken), context, _cancelPendingOperationsToken)
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
        private async Task<string> GetMailFolderID(string mailFolderName, bool throwExceptionIfMissing)
        {
            try
            {
                bool foundID = false;

                // Retrieve and cache all mail folders if the name isn't in the cache
                if (_mailFolderNameToID is null
                    || !(foundID = _mailFolderNameToID.TryGetValue(mailFolderName, out string folderID)))
                {
                    var nameToID = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var folder in await GetSharedEmailAddressMailFolders().ConfigureAwait(false))
                    {
                        nameToID.TryAdd(folder.DisplayName, folder.Id);
                    }

                    _mailFolderNameToID = nameToID;

                    foundID = _mailFolderNameToID.TryGetValue(mailFolderName, out folderID);
                }

                if (throwExceptionIfMissing && !foundID)
                {
                    throw new ExtractException("ELI53301", UtilityMethods.FormatInvariant($"Folder {mailFolderName} not found!"));
                }

                return foundID ? folderID : null;
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
                var context = new Context { { REQUEST_NAME, "GetMailFolders" } };

                List<MailFolder> result = new();
                var next = _mailFoldersRequestBuilder.Request();
                while (next != null)
                {
                    var page = await _retryPolicy
                        .ExecuteAsync((_, cancellationToken) =>
                            next.UseImmutableID()
                            .GetAsync(cancellationToken), context, _cancelPendingOperationsToken)
                        .ConfigureAwait(false);

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
        /// Message fields are limited to Id, Subject, ReceivedDateTime, ToRecipients, Sender, ParentFolderId
        /// </remarks>
        public async Task<IList<Message>> GetMessagesToProcessAsync()
        {
            try
            {
                var context = new Context { { REQUEST_NAME, "GetMessagesToProcess" } };

                var folder = await GetSharedAddressInputMailFolder().ConfigureAwait(false);
                int totalEmailsInFolder = folder.TotalItemCount ?? 0;
                if (totalEmailsInFolder > 0)
                {
                    int pageSize = 10; // Default page size
                    int skip = Math.Max(totalEmailsInFolder - pageSize, 0);
                    string inputFolderID = await GetInputMailFolderID().ConfigureAwait(false);

                    var messageCollection = await _retryPolicy
                        .ExecuteAsync((_, cancellationToken) => _mailFoldersRequestBuilder[inputFolderID].Messages
                            .Request()
                            .UseImmutableID()
                            .Skip(skip)
                            .Top(pageSize)
                            .Select(MESSAGE_FIELDS_FILTER)
                            .GetAsync(cancellationToken), context, _cancelPendingOperationsToken)
                        .ConfigureAwait(false);

                    return messageCollection
                        .OrderBy(message => message.ReceivedDateTime)
                        .ToList();
                }

                return Array.Empty<Message>();
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
        /// <param name="filePath">The file path to write the message to</param>
        public async Task DownloadMessageToDisk(Message message, string filePath)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));
                _ = filePath ?? throw new ArgumentNullException(nameof(filePath));

                var context = new Context { { REQUEST_NAME, "DownloadMessage" } };

                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                var stream = await _retryPolicy
                    .ExecuteAsync((_, cancellationToken) =>
                        _graphServiceClient.Users[Configuration.SharedEmailAddress]
                        .Messages[message.Id]
                        .Content
                        .Request()
                        .UseImmutableID()
                        .GetAsync(cancellationToken), context, _cancelPendingOperationsToken)
                    .ConfigureAwait(false);

                StreamMethods.WriteStreamToFile(filePath, stream);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53152");
            }
        }

        /// <summary>
        /// Get an email from an ID
        /// </summary>
        /// <param name="messageID">The ID of the message</param>
        /// <param name="fields">Optional fields to retrieve for the message.
        /// If null then the same fields as GetMessagesToProcessAsync will be returned</param>
        /// <remarks>
        /// Default message fields are limited to Id, Subject, ReceivedDateTime, ToRecipients, Sender, ParentFolderId
        /// </remarks>
        private async Task<Message> GetMessage(string messageID, string fields = null)
        {
            var context = new Context { { REQUEST_NAME, "GetMessage" } };

            return await _retryPolicy
                .ExecuteAsync((_, cancellationToken) =>
                    _graphServiceClient.Users[Configuration.SharedEmailAddress]
                    .Messages[messageID]
                    .Request()
                    .UseImmutableID()
                    .Select(fields ?? MESSAGE_FIELDS_FILTER)
                    .GetAsync(cancellationToken), context, _cancelPendingOperationsToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Check that the message's parent folder is the configured input folder
        /// </summary>
        /// <param name="messageID">The ID of the message to check</param>
        public async Task<bool> IsMessageInInputFolder(string messageID)
        {
            try
            {
                string inputFolderID = await GetInputMailFolderID().ConfigureAwait(false);

                Message message = await GetMessage(messageID, fields: nameof(Message.ParentFolderId)).ConfigureAwait(false);

                return inputFolderID.Equals(message.ParentFolderId, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53417");
            }
        }

        // Move a message to the specified folder
        private async Task<Message> MoveMessageToFolder(string messageID, string folderID, [CallerMemberName] string requestName = null)
        {
            var context = new Context { { REQUEST_NAME, requestName } };

            return await _retryPolicy
                .ExecuteAsync(async (context, cancellationToken) =>
                {
                    // If this is a retry after an HTTP timeout then check to see if the message has already been moved
                    if (context.TryGetValue(RETRY_ATTEMPT, out object retryNumber))
                    {
                        Message currentMessage = await GetMessage(messageID).ConfigureAwait(false);
                        if (folderID.Equals(currentMessage.ParentFolderId, StringComparison.Ordinal))
                        {
                            return currentMessage;
                        }
                    }

                    return await
                        _graphServiceClient.Users[Configuration.SharedEmailAddress]
                        .Messages[messageID]
                        .Move(folderID)
                        .Request()
                        .UseImmutableID()
                        .PostAsync(cancellationToken)
                        .ConfigureAwait(false);
                }, context, _cancelPendingOperationsToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Move the message with the provided ID to the queued folder
        /// </summary>
        /// <param name="messageID">The ID of the message to move to the queued folder</param>
        /// <returns>The moved message</returns>
        public virtual async Task<Message> MoveMessageToQueuedFolder(string messageID)
        {
            try
            {
                _ = messageID ?? throw new ArgumentNullException(nameof(messageID));

                string queuedFolderID = await GetQueuedFolderID().ConfigureAwait(false);

                return await MoveMessageToFolder(messageID, queuedFolderID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53206");
            }
        }

        /// <summary>
        /// Move the message with the provided ID to the failed folder
        /// </summary>
        /// <param name="messageID">The ID of the message to move to the failed folder</param>
        /// <returns>The moved message</returns>
        public async Task<Message> MoveMessageToFailedFolder(string messageID)
        {
            try
            {
                _ = messageID ?? throw new ArgumentNullException(nameof(messageID));

                string failedFolderID = await GetFailedFolderID().ConfigureAwait(false);

                return await MoveMessageToFolder(messageID, failedFolderID).ConfigureAwait(false);
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

        // Set _authenticationHeaderValue and _accessTokenConsideredExpiredOn, return the ValidTo date from the token
        private DateTime ParseAccessToken(string accessToken)
        {
            _authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", accessToken);
            DateTime validTo = new JwtSecurityToken(accessToken).ValidTo;

            // Consider the token expired if within a minute of expiration to avoid requests getting rejected
            _accessTokenConsideredExpiredOn = validTo - TimeSpan.FromMinutes(1);

            return validTo;
        }

        /// <summary>
        /// Get the input mail folder ID
        /// </summary>
        public async Task<string> GetInputMailFolderID()
        {
            return await GetMailFolderID(Configuration.InputMailFolderName).ConfigureAwait(false);
        }

        #region Retry Policy

        // Called after a failure before the sleep has started
        private void LogExceptionBeforeRetry(Exception exception, TimeSpan sleepDuration, int retryNumber, Context context)
        {
            context[RETRY_ATTEMPT] = retryNumber;

            var uex = new ExtractException("ELI53402",
                UtilityMethods.FormatInvariant(
                    $"Application trace: ({nameof(EmailManagement)}) request timed-out. ",
                    $"Retrying in {sleepDuration.TotalSeconds} seconds ({retryNumber}/{MAX_RETRIES})"),
                exception);

            if (context.TryGetValue(REQUEST_NAME, out object value))
            {
                uex.AddDebugData("Request", (string)value);
            }
            uex.AddDebugData("Attempt", retryNumber);

            uex.Log();
        }

        // Calculate the time to wait before retry using exponential back-off strategy
        private static TimeSpan CalculateSleepDuration(int retryNumber)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, retryNumber - 1) * DELAY_SECONDS);
        }

        // Returns true if the exception represents a time-out and cancelation hasn't been requested
        private bool ShouldRetry(Exception ex)
        {
            if (_cancelPendingOperationsToken.IsCancellationRequested)
            {
                return false;
            }

            // timeout somewhere in the graph API can manifest as a cancellation
            if (ex is OperationCanceledException)
            {
                return true;
            }

            // An actual timeout code could still be returned for certain operations
            if (ex is ServiceException serviceException)
            {
                string errorCode = serviceException.Error.Code;
                return errorCode.Equals(TIMEOUT_CODE, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        _cancelPendingOperations.Cancel();
                    }
                    catch { }
                    _cancelPendingOperations.Dispose();

                    _retryLogger.Dispose();
                    _httpClient.Dispose();
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

        #endregion IDisposable
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
