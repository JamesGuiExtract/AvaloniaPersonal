using Extract.FileActionManager.Database;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Transactions;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient
{
    public interface IEmailDatabaseManager: IDisposable
    {
        /// <summary>
        /// Get the file path that corresponds to an OutlookEmailID
        /// </summary>
        /// <param name="messageID">The EmailSource.OutlookEmailID to get the file path for</param>
        string GetExistingEmailFilePath(string messageID);

        /// <summary>
        /// Attempt to get the path of an email file by checking for the message's OutlookEmailID
        /// in the EmailSource table of the configured FileProcessingDB
        /// </summary>
        /// <returns>Whether the email exists in the EmailSource table</returns>
        bool TryGetExistingEmailFilePath(Message message, out string filePath);

        /// <summary>
        /// Build a unique file name from a message. Unique means that the path does not exist on the file system or the FAM database
        /// </summary>
        string GetNewFileName(Message message);

        /// <summary>
        /// Add an email file to the configured FileProcessingDB
        /// </summary>
        /// <param name="message">The email message to be used for the EmailSource record</param>
        /// <param name="fileInfo">The path and other information to be added to the FAMFile and WorkflowFile tables</param>
        void AddEmailToDatabase(Message message, FAMFileInfo fileInfo);

        /// <summary>
        /// Create a transaction with an exclusive lock on the EmailSource table
        /// </summary>
        /// <returns>
        /// A <see cref="TransactionScope"/> instance that will unlock the EmailSource table when it is disposed
        /// </returns>
        TransactionScope LockEmailSource();

        /// <summary>
        /// Whether the specified EmailSource table record has PendingMoveFromEmailFolder matching the configured input folder
        /// </summary>
        /// <param name="messageID">The OutlookEmailID of the record to query</param>
        bool IsEmailPendingMoveFromInbox(string messageID);

        /// <summary>
        /// Get EmailSource.OutlookEmailIDs for records where PendingMoveFromEmailFolder is the configured input folder
        /// </summary>
        IList<string> GetEmailsPendingMoveFromInbox();

        /// <summary>
        /// Clear the PendingMoveFromEmailFolder field for the specified record
        /// </summary>
        /// <param name="messageID">The OutlookEmailID of the record to clear</param>
        void ClearPendingMoveFromEmailFolder(string messageID);

        /// <summary>
        /// Get file path and OutlookEmailIDs for records where PendingNotifyFromEmailFolder is the configured input folder
        /// </summary>
        IList<string> GetEmailsPendingNotifyFromInbox();

        /// <summary>
        /// Clear the PendingNotifyFromEmailFolder field for the specified record
        /// </summary>
        /// <param name="messageID">The OutlookEmailID of the record to clear</param>
        void ClearPendingNotifyFromEmailFolder(string messageID);
    }
}