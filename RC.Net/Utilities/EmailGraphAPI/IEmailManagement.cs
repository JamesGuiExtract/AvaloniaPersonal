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
        /// <param name="filePath">The file path to write the message to</param>
        Task DownloadMessageToDisk(Message message, string filePath);

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
        /// Message fields are limited to Id, Subject, ReceivedDateTime, ToRecipients, Sender, ParentFolderId
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
        /// Move the message with the provided ID to the queued folder
        /// </summary>
        /// <param name="messageID">The ID of the message to move to the queued folder</param>
        /// <returns>The moved message</returns>
        Task<Message> MoveMessageToQueuedFolder(string messageID);

        /// <summary>
        /// Move the message with the provided ID to the failed folder
        /// </summary>
        /// <param name="messageID">The ID of the message to move to the failed folder</param>
        /// <returns>The moved message</returns>
        Task<Message> MoveMessageToFailedFolder(string messageID);

        /// <summary>
        /// Check that the message's parent folder is the configured input folder
        /// </summary>
        /// <param name="messageID">The ID of the message to check</param>
        Task<bool> IsMessageInInputFolder(string messageID);
    }
}