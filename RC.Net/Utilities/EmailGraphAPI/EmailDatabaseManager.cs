using Extract.FileActionManager.Database;
using Extract.SqlDatabase;
using Extract.Utilities;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Transactions;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient
{
    /// <summary>
    /// A class that handles <see cref="Message"/>-related functionality for an <see cref="IFileProcessingDB"/>
    /// </summary>
    public class EmailDatabaseManager: IEmailDatabaseManager
    {
        const int COMMAND_TIMEOUT = 300;
        readonly IFileProcessingDB _fileProcessingDB;
        readonly string _downloadFolder;
        readonly string _emailAddress;
        readonly string _inputMailFolder;
        readonly ExtractRoleConnection _databaseConnection;
        readonly Lazy<FAMFileUtility> _famFileUtil;
        private bool _isDisposed;

        private ExtractRoleConnection DatabaseConnection
        {
            get
            {
                if (_databaseConnection.State == System.Data.ConnectionState.Broken)
                {
                    _databaseConnection.Close();
                }

                if (_databaseConnection.State == System.Data.ConnectionState.Closed)
                {
                    _databaseConnection.Open();
                }

                return _databaseConnection;
            }
        }

        /// <summary>
        /// Create an instance using <see cref="EmailManagementConfiguration"/>
        /// </summary>
        public EmailDatabaseManager(EmailManagementConfiguration configuration)
        {
            try
            {
                _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

                _fileProcessingDB = configuration.FileProcessingDB;
                _databaseConnection = new(_fileProcessingDB.DatabaseServer, _fileProcessingDB.DatabaseName);
                _emailAddress = configuration.SharedEmailAddress;
                _inputMailFolder = configuration.InputMailFolderName;
                _famFileUtil = new Lazy<FAMFileUtility>(() => new FAMFileUtility(DatabaseConnection));

                // Make sure the download folder doesn't have any weird stuff like ..\..\ in it
                _downloadFolder = Path.GetFullPath(configuration.FilePathToDownloadEmails);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53429");
            }
        }

        /// <summary>
        /// Build a unique file name from a message
        /// </summary>
        /// <remarks>
        /// Unique means that the path does not exist on the file system or the FAM database
        /// </remarks>
        public string GetNewFileName(Message message)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                DateTimeOffset receivedDate = message.ReceivedDateTime ?? DateTimeOffset.UtcNow;

                // Build a path based on the email received year/month and create the folder if it doesn't already exist
                string folderPath = System.IO.Directory.CreateDirectory(Path.Combine(
                    _downloadFolder,
                    receivedDate.ToString("yyyy", CultureInfo.InvariantCulture),
                    receivedDate.ToString("MM", CultureInfo.InvariantCulture))).FullName;

                string prefix = String.Concat((message.Subject ?? "").Split(Path.GetInvalidFileNameChars()));

                string infix = receivedDate.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);

                // Keep trying to find an original name
                for (int copy = 0; ; copy++)
                {
                    string suffix = copy > 0 ? UtilityMethods.FormatInvariant($" ({copy})") : string.Empty;
                    string newFileName = Path.Combine(folderPath, UtilityMethods.FormatInvariant($"{prefix} {infix}{suffix}.eml"));

                    // Check the file system and database
                    if (System.IO.File.Exists(newFileName) || _famFileUtil.Value.IsFileInDatabase(newFileName))
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
        /// Create a record in the EmailSource table of the configured FileProcessingDB
        /// </summary>
        private void WriteEmailToEmailSourceTable(Message message, int fileID)
        {
            string insertEmailSourceSQL =
                "INSERT INTO dbo.EmailSource" +
                " (OutlookEmailID, EmailAddress, Subject, Received, Recipients, Sender, FAMSessionID, FAMFileID" +
                ", PendingMoveFromEmailFolder, PendingNotifyFromEmailFolder)" +
                " VALUES" +
                " (@OutlookEmailID,@EmailAddress,@Subject,@Received,@Recipients,@Sender,@FAMSessionID,@FAMFileID" +
                ", @InputMailFolder, @InputMailFolder)";
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                string recipients = String.Join(", ", message.ToRecipients.Select(recipient => recipient.EmailAddress.Address));

                using var command = DatabaseConnection.CreateCommand();
                command.CommandText = insertEmailSourceSQL;
                command.Parameters.AddWithValue("@OutlookEmailID", message.Id);
                command.Parameters.AddWithValue("@EmailAddress", _emailAddress);
                command.Parameters.AddWithValue("@Subject", message.Subject == null ? DBNull.Value : message.Subject);
                command.Parameters.AddWithValue("@Received", message.ReceivedDateTime ?? DateTimeOffset.UtcNow);
                command.Parameters.AddWithValue("@Recipients", recipients);
                command.Parameters.AddWithValue("@Sender", message.Sender == null ? DBNull.Value : message.Sender.EmailAddress.Address);
                command.Parameters.AddWithValue("@FAMSessionID", _fileProcessingDB.FAMSessionID);
                command.Parameters.AddWithValue("@FAMFileID", fileID);
                command.Parameters.AddWithValue("@InputMailFolder", _inputMailFolder);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53233");
            }
        }

        /// <summary>
        /// Add an email file to the configured FileProcessingDB
        /// </summary>
        /// <param name="message">The email message to be used for the EmailSource record</param>
        /// <param name="fileInfo">The path and other information to be added to the FAMFile and WorkflowFile tables</param>
        public void AddEmailToDatabase(Message message, FAMFileInfo fileInfo)
        {
            try
            {
                _ = message ?? throw new ArgumentNullException(nameof(message));

                int fileID = _famFileUtil.Value.AddFileNoQueue(fileInfo);

                WriteEmailToEmailSourceTable(message, fileID);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53412");
            }
        }

        /// <summary>
        /// Get the file path that corresponds to an OutlookEmailID
        /// </summary>
        /// <param name="messageID">The EmailSource.OutlookEmailID to get the file path for</param>
        public string GetExistingEmailFilePath(string messageID)
        {
            try
            {
                _ = messageID ?? throw new ArgumentNullException(nameof(messageID));

                using var command = DatabaseConnection.CreateCommand();
                command.CommandText =
                    "SELECT [FileName] FROM dbo.EmailSource" +
                    " JOIN dbo.FAMFile ON FAMFileID = dbo.FAMFile.ID" +
                    " WHERE OutlookEmailID = @OutlookEmailID";
                command.Parameters.AddWithValue("@OutlookEmailID", messageID);
                command.CommandTimeout = COMMAND_TIMEOUT;

                return command.ExecuteScalar() as string;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53439");
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
                _ = message ?? throw new ArgumentNullException(nameof(message));

                filePath = GetExistingEmailFilePath(message.Id);

                return filePath != null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53283");
            }
        }

        /// <summary>
        /// Create a transaction with an exclusive lock on the EmailSource table
        /// </summary>
        /// <returns>
        /// A <see cref="TransactionScope"/> instance that will unlock the EmailSource table when it is disposed
        /// </returns>
        public TransactionScope LockEmailSource()
        {
            TransactionScope scope = null;
            try
            {
                // Make sure the connection is open before starting the transaction (the app role cannot be set outside of a transaction)
                ExtractException.Assert("ELI53416", "Connection must be open before starting a transaction",
                    DatabaseConnection.State == System.Data.ConnectionState.Open);

                TransactionOptions transactionOptions = new()
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TransactionManager.MaximumTimeout,
                };
                scope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
                DatabaseConnection.EnlistTransaction(Transaction.Current);

                using var command = DatabaseConnection.CreateCommand();
                command.CommandText = "SELECT 1 FROM dbo.EmailSource WITH (TABLOCKX)";
                command.CommandTimeout = COMMAND_TIMEOUT;
                command.ExecuteNonQuery();

                return scope;
            }
            catch (Exception ex)
            {
                try
                {
                    scope?.Dispose();
                }
                catch { }

                throw ex.AsExtract("ELI53427");
            }
        }

        /// <summary>
        /// Whether the specified EmailSource table record has PendingMoveFromEmailFolder matching the configured input folder
        /// </summary>
        /// <param name="messageID">The OutlookEmailID of the record to query</param>
        public bool IsEmailPendingMoveFromInbox(string messageID)
        {
            try
            {
                using var command = DatabaseConnection.CreateCommand();
                command.CommandText =
                    "SELECT OutlookEmailID FROM dbo.EmailSource" +
                    " WHERE PendingMoveFromEmailFolder = @InputFolder" +
                    " AND OutlookEmailID = @OutlookEmailID";
                command.Parameters.AddWithValue("@InputFolder", _inputMailFolder);
                command.Parameters.AddWithValue("@OutlookEmailID", messageID);

                return command.ExecuteScalar() is not null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53428");
            }
        }

        /// <summary>
        /// Get EmailSource.OutlookEmailIDs for records where PendingMoveFromEmailFolder is the configured input folder
        /// </summary>
        public IList<string> GetEmailsPendingMoveFromInbox()
        {
            try
            {
                List<string> emailsToMove = new();

                using var command = DatabaseConnection.CreateCommand();
                command.CommandText =
                    "SELECT OutlookEmailID FROM dbo.EmailSource" +
                    " WHERE PendingMoveFromEmailFolder = @InputFolder";
                command.Parameters.AddWithValue("@InputFolder", _inputMailFolder);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    emailsToMove.Add(reader.GetString(0));
                }

                return emailsToMove;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53433");
            }
        }

        /// <summary>
        /// Clear the PendingMoveFromEmailFolder field for the specified record
        /// </summary>
        /// <param name="messageID">The OutlookEmailID of the record to clear</param>
        public virtual void ClearPendingMoveFromEmailFolder(string messageID)
        {
            try
            {
                using var command = DatabaseConnection.CreateCommand();
                command.CommandText =
                    "UPDATE dbo.EmailSource SET PendingMoveFromEmailFolder = NULL" +
                    " WHERE OutlookEmailID = @OutlookEmailID";
                command.Parameters.AddWithValue("@OutlookEmailID", messageID);
                command.CommandTimeout = COMMAND_TIMEOUT;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53422");
            }
        }

        /// <summary>
        /// Get file path and OutlookEmailIDs for records where PendingNotifyFromEmailFolder is the configured input folder
        /// </summary>
        public IList<string> GetEmailsPendingNotifyFromInbox()
        {
            try
            {
                List<string> emailsToMove = new();

                using var command = DatabaseConnection.CreateCommand();
                command.CommandText =
                    "SELECT OutlookEmailID FROM dbo.EmailSource" +
                    " WHERE PendingNotifyFromEmailFolder = @InputFolder";
                command.Parameters.AddWithValue("@InputFolder", _inputMailFolder);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    emailsToMove.Add(reader.GetString(0));
                }

                return emailsToMove;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53435");
            }
        }

        /// <summary>
        /// Clear the PendingNotifyFromEmailFolder field for the specified record
        /// </summary>
        /// <param name="messageID">The OutlookEmailID of the record to clear</param>
        public virtual void ClearPendingNotifyFromEmailFolder(string messageID)
        {
            try
            {
                using var command = DatabaseConnection.CreateCommand();
                command.CommandText =
                    "UPDATE dbo.EmailSource SET PendingNotifyFromEmailFolder = NULL" +
                    " WHERE OutlookEmailID = @OutlookEmailID";
                command.Parameters.AddWithValue("@OutlookEmailID", messageID);
                command.CommandTimeout = COMMAND_TIMEOUT;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53434");
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _databaseConnection.Dispose();
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
}
