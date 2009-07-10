using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SourceControl
{
    /// <summary>
    /// Represents entry points into unmanaged code.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Represents an email message.
        /// </summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/dd296732%28VS.85%29.aspx"/>
        [BestFitMapping(false)]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        class MapiMessage
        {
            /// <summary>
            /// Reserved; must be zero.
            /// </summary>
            public uint reserved;

            /// <summary>
            /// The message subject, typically limited to 256 characters or less. If this field 
            /// is <see langword="null"/>, the user has not entered subject text.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string subject;

            /// <summary>
            /// A string containing the message text. If this field is <see langword="null"/>, 
            /// there is no message text.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string noteText;

            /// <summary>
            /// A string indicating a non-IPM type of message. Client applications can select 
            /// message types for their non-IPM messages. Clients that only support IPM messages 
            /// can ignore this field when reading messages and set it to <see langword="null"/> 
            /// when sending messages.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string messageType;

            /// <summary>
            /// A string indicating the date when the message was received. The format is 
            /// YYYY/MM/DD HH:MM, using a 24-hour clock.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string dateReceived;

            /// <summary>
            /// a string identifying the conversation thread to which the message belongs. Some 
            /// messaging systems can ignore and not return this field.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string conversationID;

            /// <summary>
            /// Bitmask of message status flags.
            /// </summary>
            public uint flags;

            /// <summary>
            /// Pointer to a <see cref="MapiRecipDesc"/> structure containing information about 
            /// the sender of the message. 
            /// </summary>
            public IntPtr originator;

            /// <summary>
            /// The number of message recipient structures in the array pointed to by the 
            /// <see cref="recips"/> field. A value of zero indicates no recipients are included.
            /// </summary>
            public uint recipCount;

            /// <summary>
            /// Pointer to an array of <see cref="MapiRecipDesc"/> structures, each containing 
            /// information about a message recipient.
            /// </summary>
            public IntPtr recips;

            /// <summary>
            /// The number of structures describing file attachments in the array pointed to by 
            /// the <see cref="files"/> field. A value of zero indicates no file attachments are 
            /// included.
            /// </summary>
            public uint fileCount;

            /// <summary>
            /// Pointer to an array of <see cref="MapiRecipDesc"/> structures, each containing 
            /// information about a file attachment.
            /// </summary>
            public IntPtr files;
        }
        
        /// <summary>
        /// Represents a message sender or recipient.
        /// </summary>
        /// <see href="http://msdn.microsoft.com/en-us/library/dd296720%28VS.85%29.aspx"/>
        struct MapiRecipDesc
        {
            /// <summary>
            /// Reserved; must be zero.
            /// </summary>
            public uint reserved;

            /// <summary>
            /// Contains a <see cref="RecipClass"/> that indicates the type of recipient.
            /// </summary>
            public uint recipClass;

            /// <summary>
            /// The display name of the message recipient or sender. 
            /// </summary>
            public string name;

            /// <summary>
            /// Optional pointer to the recipient or sender's address; this address is 
            /// provider-specific message delivery data. Generally, the messaging system provides 
            /// such addresses for inbound messages. For outbound messages, this field can point 
            /// to an address entered by the user for a recipient not in an address book (that is, 
            /// a custom recipient). The format of an address pointed to by this field is 
            /// [address type][e-mail address]. Examples of valid addresses are "FAX:206-555-1212" 
            /// and "SMTP:M@X.COM".
            /// </summary>
            public string address;

            /// <summary>
            /// The size, in bytes, of the entry identifier pointed to by the 
            /// <see cref="entryID"/> field.
            /// </summary>
            public uint entryIDSize;

            /// <summary>
            /// Pointer to an opaque entry identifier used by a messaging system service provider 
            /// to identify the message recipient. Entry identifiers have meaning only for the 
            /// service provider; client applications will not be able to decipher them. The 
            /// messaging system uses this field to return valid entry identifiers for all 
            /// recipients or senders listed in the address book.
            /// </summary>
            public IntPtr entryID;
        }
        
        /// <summary>
        /// Represents options for sending messages.
        /// </summary>
        [Flags]
        enum SendMailFlags : uint
        {
            /// <summary>
            /// Prompt user to log on if required. If not set, returns an error value if the user 
            /// is not logged on.
            /// </summary>
            MAPI_LOGON_UI = 1,
            
            /// <summary>
            /// Create a new session rather than acquire the environment's shared session. If not 
            /// set, acquires existing session.
            /// </summary>
            MAPI_NEW_SESSION = 2,
            
            /// <summary>
            /// Prompt user for recipients and other sending options. If not set, at least one 
            /// recipient must be specified.
            /// </summary>
            MAPI_DIALOG = 8
        }
        
        /// <summary>
        /// Represents the type of recipient.
        /// </summary>
        enum RecipClass : uint
        {
            /// <summary>
            /// Indicates the original sender of the message.
            /// </summary>
            MAPI_ORIG = 0,
            
            /// <summary>
            /// Indicates a primary message recipient.
            /// </summary>
            MAPI_TO = 1,
            
            /// <summary>
            /// Indicates a recipient of a message copy.
            /// </summary>
            MAPI_CC = 2,
            
            /// <summary>
            /// Indicates a recipient of a blind copy.
            /// </summary>
            MAPI_BCC = 3
        }
        
        /// <summary>
        /// Represents an error code returned by MAPI methods.
        /// </summary>
        enum MapiErrorCode : uint
        {
            /// <summary>
            /// The call succeeded and the message was sent.
            /// </summary>
            SUCCESS_SUCCESS = 0,
            
            /// <summary>
            /// The user canceled one of the dialog boxes. No message was sent.
            /// </summary>
            MAPI_E_USER_ABORT = 1,
            
            /// <summary>
            /// One or more unspecified errors occurred. No message was sent.
            /// </summary>
            MAPI_E_FAILURE = 2,
            
            /// <summary>
            /// There was no default logon, and the user failed to log on successfully when the 
            /// logon dialog box was displayed. No message was sent.
            /// </summary>
            MAPI_E_LOGIN_FAILURE = 3,
            
            /// <summary>
            /// There was insufficient memory to proceed. No message was sent.
            /// </summary>
            MAPI_E_INSUFFICIENT_MEMORY = 5,
            
            /// <summary>
            /// There were too many file attachments. No message was sent.
            /// </summary>
            MAPI_E_TOO_MANY_FILES = 9,
            
            /// <summary>
            /// There were too many recipients. No message was sent.
            /// </summary>
            MAPI_E_TOO_MANY_RECIPIENTS = 10,
            
            /// <summary>
            /// The specified attachment was not found. No message was sent.
            /// </summary>
            MAPI_E_ATTACHMENT_NOT_FOUND = 11,
            
            /// <summary>
            /// The specified attachment could not be opened. No message was sent.
            /// </summary>
            MAPI_E_ATTACHMENT_OPEN_FAILURE = 12,
            
            /// <summary>
            /// A recipient did not appear in the address list. No message was sent.
            /// </summary>
            MAPI_E_UNKNOWN_RECIPIENT = 14,
            
            /// <summary>
            /// The type of a recipient was not MAPI_TO, MAPI_CC, or MAPI_BCC. No message was sent.
            /// </summary>
            MAPI_E_BAD_RECIPTYPE = 15,
            
            /// <summary>
            /// The text in the message was too large. No message was sent.
            /// </summary>
            MAPI_E_TEXT_TOO_LARGE = 18,
            
            /// <summary>
            /// A recipient matched more than one of the recipient descriptor structures and 
            /// <see cref="SendMailFlags.MAPI_DIALOG"/> was not set. No message was sent.
            /// </summary>
            MAPI_E_AMBIGUOUS_RECIPIENT = 21,
            
            /// <summary>
            /// One or more recipients were invalid or did not resolve to any address.
            /// </summary>
            MAPI_E_INVALID_RECIPS = 25
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="session">Handle to a Simple MAPI session. If <see cref="IntPtr.Zero"/>, 
        /// MAPI logs on the user and creates a session that exists only for the duration of the 
        /// call. This temporary session can be an existing shared session or a new one. If 
        /// necessary, the logon dialog box is displayed.</param>
        /// <param name="uiParam">Parent window handle or <see cref="IntPtr.Zero"/>, indicating 
        /// that if a dialog box is displayed, it is application modal. If it is a parent window 
        /// handle, it is of type HWND (cast to a ULONG_PTR). If no dialog box is displayed during 
        /// the call, it is ignored.</param>
        /// <param name="message">Pointer to a <see cref="MapiMessage"/> structure containing the 
        /// message to be sent. If the <see cref="SendMailFlags.MAPI_DIALOG"/> flag is not set, 
        /// the <see cref="MapiMessage.recipCount"/> and <see cref="MapiMessage.recips"/> fields 
        /// must be valid for successful message delivery. Client applications can set the 
        /// <see cref="MapiMessage.flags"/> field to MAPI_RECEIPT_REQUESTED to request a read 
        /// report. All other fields are ignored and unused pointers should be 
        /// <see langword="IntPtr.Zero"/>.</param>
        /// <param name="flags"><see cref="SendMailFlags"/> representing option flags.</param>
        /// <param name="reserved">Reserved; must be zero.</param>
        /// <returns>An error code of type <see cref="MapiErrorCode"/>.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/dd296721%28VS.85%29.aspx"/>
        [DllImport("mapi32.dll", CharSet = CharSet.Ansi)]
        static extern uint MAPISendMail(IntPtr session, IntPtr uiParam, 
            MapiMessage message, uint flags, uint reserved);

        /// <summary>
        /// Sends an email message to the specified recipients.
        /// </summary>
        /// <param name="subject">The subject heading of the email to send.</param>
        /// <param name="body">The message of the email.</param>
        /// <param name="recipients">A comma-separated list of email recipients.</param>
        public static void SendEmail(string subject, string body, string[] recipients)
        {
            // Allocate a buffer to hold the recipients structure
            int recipDescSize = Marshal.SizeOf(typeof(MapiRecipDesc));
            IntPtr recipsBuff = Marshal.AllocCoTaskMem(recipDescSize * recipients.Length);
            
            try 
            {
                MapiRecipDesc[] recips = new MapiRecipDesc[recipients.Length];

                // Iterate through each recipient
                int offset = recipsBuff.ToInt32();
                for (int i = 0; i < recipients.Length; i++) 
                {
                    // Create the recipient structure
                    recips[i].reserved = 0;
                    recips[i].recipClass = (uint) RecipClass.MAPI_TO;
                    recips[i].name = recipients[i];
                    recips[i].address = null;
                    recips[i].entryIDSize = 0;
                    recips[i].entryID = IntPtr.Zero;
                    
                    // Copy the structure into the buffer
                    Marshal.StructureToPtr(recips[i], (IntPtr)offset, false);
                    
                    // Iterate to next recipient
                    offset += recipDescSize;
                }
                
                MapiMessage message = new MapiMessage();
                message.reserved = 0;
                message.subject = subject;
                message.noteText = body;
                message.messageType = "";
                message.dateReceived = "";
                message.conversationID = "";
                message.flags = 0;
                message.originator = IntPtr.Zero;
                message.recipCount = (uint)recips.Length;
                message.recips = recipsBuff;
                message.fileCount = 0;
                message.files = IntPtr.Zero;

                SendEmail(message);
            }
            finally 
            {
                if (recipsBuff != IntPtr.Zero) 
                {
                    Marshal.FreeCoTaskMem(recipsBuff);
                }
            }
        }

        /// <summary>
        /// Sends an email using the specified <see cref="MapiMessage"/>.
        /// </summary>
        /// <param name="message">The settings for the email to send.</param>
        static void SendEmail(MapiMessage message)
        {
            MapiErrorCode retval = (MapiErrorCode)MAPISendMail(IntPtr.Zero, IntPtr.Zero,
                message, (uint)(SendMailFlags.MAPI_DIALOG | SendMailFlags.MAPI_LOGON_UI), 0);

            // Throw an exception if the message was not sent successfully
            if (retval != MapiErrorCode.SUCCESS_SUCCESS)
            {
                throw new InvalidOperationException("Unable to send message: " + retval.ToString());
            }
        }
    }

}
