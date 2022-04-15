using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient
{
    public interface IEmailManagement : IDisposable
    {
        /// <summary>
        /// Create the specified mail folder if it does not exist in the shared email
        /// </summary>
        /// <param name="mailFolderName">The name of the folder to create</param>
        Task CreateMailFolder(string mailFolderName);

        /// <summary>
        /// Get whether a mail folder exists
        /// </summary>
        /// <param name="mailFolderName">The mail folder to check</param>
        Task<bool> DoesMailFolderExist(string mailFolderName);

        /// <summary>
        /// Download a message
        /// </summary>
        /// <param name="message">The message to download</param>
        /// <param name="filePath">The file path to use or null if the path should be generated from the message</param>
        /// <returns>The file path of the message that was downloaded</returns>
        Task<string> DownloadMessageToDisk(Message message, string filePath = null);

        /// <summary>
        /// Get the input mail folder ID
        /// </summary>
        Task<string> GetInputMailFolderID();

        /// <summary>
        /// Get the ID of a mail folder
        /// </summary>
        Task<string> GetMailFolderID(string mailFolderName);

        /// <summary>
        /// Get the top 10 messages from the input mail folder
        /// </summary>
        /// <remarks>
        /// Message fields are limited to Id, Subject, ReceivedDateTime, ToRecipients, Sender
        /// </remarks>
        Task<IList<Message>> GetMessagesToProcessAsync();

        /// <summary>
        /// Get the queued mail folder ID
        /// </summary>
        Task<string> GetQueuedFolderID();

        /// <summary>
        /// Get the failed mail folder ID
        /// </summary>
        Task<string> GetFailedFolderID();

        /// <summary>
        /// Get a <see cref="MailFolder"/> for the configured input mail folder
        /// </summary>
        Task<MailFolder> GetSharedAddressInputMailFolder();

        /// <summary>
        /// Get all of the mail folders for the shared email address
        /// </summary>
        Task<IEnumerable<MailFolder>> GetSharedEmailAddressMailFolders();

        /// <summary>
        /// Move the provided message to the queued folder
        /// </summary>
        /// <param name="message">The message to move to the queued folder</param>
        /// <returns>A task containing the moved message</returns>
        Task<Message> MoveMessageToQueuedFolder(Message message);

        /// <summary>
        /// Move the provided message to the failed folder
        /// </summary>
        /// <param name="message">The message to move to the failed folder</param>
        /// <returns>A task containing the moved message</returns>
        Task<Message> MoveMessageToFailedFolder(Message message);

        /// <summary>
        /// Create a record in the EmailSource table of the configured FileProcessingDB
        /// </summary>
        void WriteEmailToEmailSourceTable(Message message, int fileID, string emailAddress);

        /// <summary>
        /// Attempt to get the path of an email file by checking for the message's OutlookEmailID
        /// in the EmailSource table of the configured FileProcessingDB
        /// </summary>
        /// <returns>Whether the email exists in the EmailSource table</returns>
        bool TryGetExistingEmailFilePath(Message message, out string filePath);
    }
}