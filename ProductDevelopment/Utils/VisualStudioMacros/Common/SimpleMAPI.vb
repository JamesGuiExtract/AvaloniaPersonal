Option Strict On
Option Explicit On
Option Compare Binary
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics
Imports System.Runtime.InteropServices

Class SimpleMAPI
    Structure MAPIMessage
        Public reserved As UInt32
        Public subject As String
        Public noteText As String
        Public messageType As String
        Public dateReceived As String
        Public conversationID As String
        Public flags As UInt32
        Public originator As IntPtr ' to MAPIRecipDesc
        Public recipCount As UInt32
        Public recips As IntPtr ' to MAPIRecipDesc 
        Public fileCount As UInt32
        Public files As IntPtr ' to MAPIFileDesc
    End Structure

    Structure MAPIRecipDesc
        Public reserved As UInt32
        Public recipClass As UInt32
        Public name As String
        Public address As String
        Public EIDSize As UInt32
        Public entryID As IntPtr ' to void*
    End Structure

    Structure MAPIFileDesc
        Public reserved As UInt32
        Public flags As UInt32
        Public position As UInt32
        Public pathName As String
        Public fileName As String
        Public fileType As IntPtr ' to void*
    End Structure

    Enum SendMailFlags As UInt32

        ' Prompt user to log on if required. If not set, returns an error value if the user is not logged on.
        MAPI_LOGON_UI = 1

        ' Create a new session rather than acquire the environment's shared session. If not set, acquires existing session.
        MAPI_NEW_SESSION = 2

        ' Prompt user for recipients and other sending options. If not set, at least one recipient must be specified.
        MAPI_DIALOG = 8
    End Enum

    Enum RecipClass As UInt32

        ' Indicates the original sender of the message. 
        MAPI_ORIG = 0

        ' Indicates a primary message recipient. 
        MAPI_TO = 1

        ' Indicates a recipient of a message copy. 
        MAPI_CC = 2

        ' Indicates a recipient of a blind copy.
        MAPI_BCC = 3
    End Enum

    Enum MAPIErrorCodes As UInt32

        ' The call succeeded and the message was sent.
        SUCCESS_SUCCESS = 0

        ' The user canceled one of the dialog boxes. No message was sent.
        MAPI_E_USER_ABORT = 1

        ' One or more unspecified errors occurred. No message was sent.
        MAPI_E_FAILURE = 2

        ' There was no default logon, and the user failed to log on successfully when the logon dialog box was displayed. No message was sent.
        MAPI_E_LOGIN_FAILURE = 3

        ' There was insufficient memory to proceed. No message was sent.
        MAPI_E_INSUFFICIENT_MEMORY = 5

        ' There were too many file attachments. No message was sent.
        MAPI_E_TOO_MANY_FILES = 9

        ' There were too many recipients. No message was sent.
        MAPI_E_TOO_MANY_RECIPIENTS = 10

        ' The specified attachment was not found. No message was sent.
        MAPI_E_ATTACHMENT_NOT_FOUND = 11

        ' The specified attachment could not be opened. No message was sent.
        MAPI_E_ATTACHMENT_OPEN_FAILURE = 12

        ' A recipient did not appear in the address list. No message was sent.
        MAPI_E_UNKNOWN_RECIPIENT = 14

        ' The type of a recipient was not MAPI_TO, MAPI_CC, or MAPI_BCC. No message was sent.
        MAPI_E_BAD_RECIPTYPE = 15

        ' The text in the message was too large. No message was sent.
        MAPI_E_TEXT_TOO_LARGE = 18

        ' A recipient matched more than one of the recipient descriptor structures and MAPI_DIALOG was not set. No message was sent.
        MAPI_E_AMBIGUOUS_RECIPIENT = 21

        ' One or more recipients were invalid or did not resolve to any address.
        MAPI_E_INVALID_RECIPS = 25
    End Enum

    Declare Function MAPISendMail Lib "mapi32.dll" ( _
        ByVal session As UInt32, _
        ByVal UIParam As UInt32, _
        ByRef MAPIMessage As MAPIMessage, _
        ByVal flags As UInt32, _
        ByVal reserved As UInt32) As UInt32

    Sub SendMail(ByVal subject As String, ByVal body As String, ByVal recipients As String())

        ' Allocate a buffer to hold the recipients structure
        Dim recipDescSize As Integer = Marshal.SizeOf(GetType(MAPIRecipDesc))
        Dim recipsBuff As IntPtr = Marshal.AllocCoTaskMem(recipDescSize * recipients.Length)
        Dim recips(recipients.Length - 1) As MAPIRecipDesc

        Try

            ' Iterate through each recipient
            Dim offset As Integer = recipsBuff.ToInt32()
            For i As Integer = 0 To recipients.Length() - 1

                ' Create the recipient structure
                With recips(i)
                    .reserved = 0
                    .recipClass = RecipClass.MAPI_TO
                    .name = recipients(i)
                    .address = Nothing
                    .EIDSize = 0
                    .entryID = Nothing
                End With

                ' Copy the structure into the buffer
                Marshal.StructureToPtr(recips(i), CType(offset, IntPtr), False)

                ' Iterate to next recipient
                offset += recipDescSize
            Next

            Dim message As MAPIMessage
            With message
                .reserved = 0
                .subject = subject
                .noteText = body
                .messageType = ""
                .dateReceived = ""
                .conversationID = ""
                .flags = 0
                .originator = Nothing
                .recipCount = CType(recips.Length, UInteger)
                .recips = recipsBuff
                .fileCount = 0
                .files = Nothing
            End With

            Dim retval As UInteger = MAPISendMail(0, 0, message, SendMailFlags.MAPI_DIALOG, 0)

            ' Throw an exception if the message was not sent successfully
            If retval <> MAPIErrorCodes.SUCCESS_SUCCESS Then
                Throw New System.Exception("Unable to send message: " + retval.ToString())
            End If

        Finally

            If recipsBuff <> IntPtr.Zero Then
                Marshal.FreeCoTaskMem(recipsBuff)
            End If
        End Try

    End Sub

End Class
