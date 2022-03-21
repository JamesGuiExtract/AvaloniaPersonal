﻿using Microsoft.Graph;
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
        /// <returns>The file name of the message that was downloaded</returns>
        Task<string> DownloadMessageToDisk(Message message, bool messageAlreadyProceed = false);

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
        Task<IMailFolderMessagesCollectionPage> GetMessagesToProcessAsync();

        /// <summary>
        /// Get the queued mail folder ID
        /// </summary>
        Task<string> GetQueuedFolderID();

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
    }
}