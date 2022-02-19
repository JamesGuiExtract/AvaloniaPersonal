using Extract.SqlDatabase;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileSuppliers
{
    [CLSCompliant(false)]
    public static class EmailFileSupplierLogger
    {
        private static readonly string InsertEmailSourceSQL =
@"
INSERT INTO 
	dbo.EmailSource (OutlookEmailID, EmailAddress, Subject, Received, Recipients, Sender, FAMSessionID, QueueEventID, FAMFileID)  
SELECT TOP 1
 @OutlookEmailID
 , @EmailAddress
 , @Subject
 , @Received
 , @Recipients
 , @Sender
 , @FAMSessionID
 , QueueEvent.ID
 , @FAMFileID
FROM
	dbo.QueueEvent
WHERE
	dbo.QueueEvent.FileID = @FAMFileID
ORDER BY
	dbo.QueueEvent.ID DESC";

        public static void WriteEmailToEmailSourceTable(IFileProcessingDB fileProcessingDB
            , Message message
            , IFileRecord fileRecord
            , SqlAppRoleConnection connection
            , string emailAddress)
        {
            try
            {
                string recipients = string.Empty;
                foreach(var recipient in message.ToRecipients)
                {
                    recipients += string.IsNullOrEmpty(recipients) ? string.Empty : ", ";
                    recipients += recipient.EmailAddress.Address;
                }

                var command = connection.CreateCommand();
                command.CommandText = InsertEmailSourceSQL;
                command.Parameters.AddWithValue("@OutlookEmailID", message.Id);
                command.Parameters.AddWithValue("@EmailAddress", emailAddress);
                command.Parameters.AddWithValue("@Subject", message.Subject == null ? DBNull.Value : message.Subject);
                command.Parameters.AddWithValue("@Received", message.ReceivedDateTime);
                command.Parameters.AddWithValue("@Recipients", recipients);
                command.Parameters.AddWithValue("@Sender", message.Sender == null ? DBNull.Value : message.Sender);
                command.Parameters.AddWithValue("@FAMSessionID", fileProcessingDB.FAMSessionID);
                command.Parameters.AddWithValue("@FAMFileID", fileRecord.FileID);
                command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53233");
            }
        }
    }
}
