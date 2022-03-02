using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents entry points to unmanaged code.
    /// </summary>
    internal static class NativeMethods
    {
        #region NativeMethods Constants

        /// <summary>
        /// The maximum size of the buffer to use for the value in an initialization file.
        /// </summary>
        const int _MAX_BUFFER_SIZE = 2048;

        #endregion NativeMethods Constants

        #region Enums

        /// <summary>
        /// Describes the event that has occurred.
        /// Typically, only one event is specified at a time.
        /// If more than one event is specified, the values contained
        /// in the <i>dwItem1</i> and <i>dwItem2</i>
        /// parameters must be the same, respectively, for all specified events.
        /// This parameter can be one or more of the following values.
        /// </summary>
        /// <remarks>
        /// <para><b>Windows NT/2000/XP:</b> <i>dwItem2</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem1</i> is not used and should be <see langword="null"/>.</para>
        /// <para><b>Windows 95/98:</b> <i>dwItem1</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem2</i> is not used and should be <see langword="null"/>.</para>
        /// </remarks>
        [Flags]
        enum HChangeNotifyEventID
        {
            /// <summary>
            /// All events have occurred.
            /// </summary>
            SHCNE_ALLEVENTS = 0x7FFFFFFF,

            /// <summary>
            /// A file type association has changed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// must be specified in the <i>uFlags</i> parameter.
            /// <i>dwItem1</i> and <i>dwItem2</i> are not used and must be <see langword="null"/>.
            /// </summary>
            SHCNE_ASSOCCHANGED = 0x08000000,

            /// <summary>
            /// The attributes of an item or folder have changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item or folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_ATTRIBUTES = 0x00000800,

            /// <summary>
            /// A non-folder item has been created.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_CREATE = 0x00000002,

            /// <summary>
            /// A non-folder item has been deleted.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was deleted.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DELETE = 0x00000004,

            /// <summary>
            /// A drive has been added.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADD = 0x00000100,

            /// <summary>
            /// A drive has been added and the Shell should create a new window for the drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADDGUI = 0x00010000,

            /// <summary>
            /// A drive has been removed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEREMOVED = 0x00000080,

            /// <summary>
            /// Not currently used.
            /// </summary>
            SHCNE_EXTENDED_EVENT = 0x04000000,

            /// <summary>
            /// The amount of free space on a drive has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive on which the free space changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_FREESPACE = 0x00040000,

            /// <summary>
            /// Storage media has been inserted into a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that contains the new media.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAINSERTED = 0x00000020,

            /// <summary>
            /// Storage media has been removed from a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive from which the media was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAREMOVED = 0x00000040,

            /// <summary>
            /// A folder has been created. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// or HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MKDIR = 0x00000008,

            /// <summary>
            /// A folder on the local computer is being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETSHARE = 0x00000200,

            /// <summary>
            /// A folder on the local computer is no longer being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is no longer being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETUNSHARE = 0x00000400,

            /// <summary>
            /// The name of a folder has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous pointer to an item identifier list (PIDL) or name of the folder.
            /// <i>dwItem2</i> contains the new PIDL or name of the folder.
            /// </summary>
            SHCNE_RENAMEFOLDER = 0x00020000,

            /// <summary>
            /// The name of a non-folder item has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous PIDL or name of the item.
            /// <i>dwItem2</i> contains the new PIDL or name of the item.
            /// </summary>
            SHCNE_RENAMEITEM = 0x00000001,

            /// <summary>
            /// A folder has been removed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_RMDIR = 0x00000010,

            /// <summary>
            /// The computer has disconnected from a server.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the server from which the computer was disconnected.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_SERVERDISCONNECT = 0x00004000,

            /// <summary>
            /// The contents of an existing folder have changed,
            /// but the folder still exists and has not been renamed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or
            /// SHCNE_RENAMEFOLDER, respectively, instead.
            /// </summary>
            SHCNE_UPDATEDIR = 0x00001000,

            /// <summary>
            /// An image in the system image list has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_DWORD"/> must be specified in <i>uFlags</i>.
            /// </summary>
            SHCNE_UPDATEIMAGE = 0x00008000
        }

        /// <summary>
        /// Flags that indicate the meaning of the <i>dwItem1</i> and <i>dwItem2</i> parameters.
        /// The uFlags parameter must be one of the following values.
        /// </summary>
        [Flags]
        public enum HChangeNotifyFlags
        {
            /// <summary>
            /// The <i>dwItem1</i> and <i>dwItem2</i> parameters are DWORD values.
            /// </summary>
            SHCNF_DWORD = 0x0003,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of ITEMIDLIST structures that
            /// represent the item(s) affected by the change.
            /// Each ITEMIDLIST must be relative to the desktop folder.
            /// </summary>
            SHCNF_IDLIST = 0x0000,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHA = 0x0001,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHW = 0x0005,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERA = 0x0002,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERW = 0x0006,
            /// <summary>
            /// The function should not return until the notification
            /// has been delivered to all affected components.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSH = 0x1000,
            /// <summary>
            /// The function should begin delivering notifications to all affected components
            /// but should return as soon as the notification process has begun.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSHNOWAIT = 0x2000
        }

        /// <summary>
        /// Specifies how the credentials are packed.
        /// </summary>
        [Flags]
        enum CredPackFlags
        {
            /// <summary>
            /// Encrypts the credential so that it can only be decrypted by processes in the caller's logon session.
            /// </summary>
            CRED_PACK_PROTECTED_CREDENTIALS = 1,
            /// <summary>
            /// Encrypts the credential in a WOW buffer.
            /// </summary>
            CRED_PACK_WOW_BUFFER = 2,
            /// <summary>
            /// Encrypts the credential in a CRED_GENERIC buffer.
            /// </summary>
            CRED_PACK_GENERIC_CREDENTIALS = 4,
            /// <summary>
            /// Encrypts the credential of an online identity into a SEC_WINNT_AUTH_IDENTITY_EX2 structure.
            /// If CRED_PACK_GENERIC_CREDENTIALS and CRED_PACK_ID_PROVIDER_CREDENTIALS are not set, encrypts
            /// the credentials in a KERB_INTERACTIVE_LOGON buffer.
            /// Windows 7, Windows Server 2008 R2, Windows Vista, Windows Server 2008: This value is not supported.
            /// </summary>
            CRED_PACK_ID_PROVIDER_CREDENTIALS = 8
        }


        /// <summary>
        /// Specifies special behavior for <see cref="CredUIPromptForCredentials"/>.
        /// </summary>
        [Flags]
        enum CREDUI_FLAGS
        {
            INCORRECT_PASSWORD = 0x1,
            DO_NOT_PERSIST = 0x2,
            REQUEST_ADMINISTRATOR = 0x4,
            EXCLUDE_CERTIFICATES = 0x8,
            REQUIRE_CERTIFICATE = 0x10,
            SHOW_SAVE_CHECK_BOX = 0x40,
            ALWAYS_SHOW_UI = 0x80,
            REQUIRE_SMARTCARD = 0x100,
            PASSWORD_ONLY_OK = 0x200,
            VALIDATE_USERNAME = 0x400,
            COMPLETE_USERNAME = 0x800,
            PERSIST = 0x1000,
            SERVER_CREDENTIAL = 0x4000,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            KEEP_USERNAME = 0x100000
        }

        /// <summary>
        /// Specifies behavior for <see cref="CredUIPromptForWindowsCredentials"/>.
        /// </summary>
        [Flags]
        enum PromptForWindowsCredentialsFlags
        {
            /// <summary>
            /// The caller is requesting that the credential provider return the user name and password in plain text.
            /// This value cannot be combined with SECURE_PROMPT.
            /// </summary>
            CREDUIWIN_GENERIC = 0x1,
            /// <summary>
            /// The Save check box is displayed in the dialog box.
            /// </summary>
            CREDUIWIN_CHECKBOX = 0x2,
            /// <summary>
            /// Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.
            /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
            /// </summary>
            CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
            /// <summary>
            /// Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.
            /// If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.
            /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
            /// </summary>
            CREDUIWIN_IN_CRED_ONLY = 0x20,
            /// <summary>
            /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
            /// </summary>
            CREDUIWIN_ENUMERATE_ADMINS = 0x100,
            /// <summary>
            /// Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated.
            /// </summary>
            CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
            /// <summary>
            /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
            /// Windows Vista: This value is not supported until Windows Vista with SP1.
            /// </summary>
            CREDUIWIN_SECURE_PROMPT = 0x1000,
            /// <summary>
            /// The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
            /// </summary>
            CREDUIWIN_PACK_32_WOW = 0x10000000,
        }

        /// <summary>
        /// The return value type for <see cref="CredUIPromptForCredentials"/> and 
        /// <see cref="CredUIPromptForWindowsCredentials"/>.
        /// </summary>
        enum CredUIReturnCode
        {
            NO_ERROR = 0,
            ERROR_CANCELLED = 1223,
            ERROR_NO_SUCH_LOGON_SESSION = 1312,
            ERROR_NOT_FOUND = 1168,
            ERROR_INVALID_ACCOUNT_NAME = 1315,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_FLAGS = 1004,
            ERROR_BAD_ARGUMENTS = 160
        }

        /// <summary>
        /// Type of logon operations for LogonUser.
        /// </summary>
        public enum LogonType
        {
            /// <summary>
            /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
            /// by a terminal server, remote shell, or similar process.
            /// This logon type has the additional expense of caching logon information for disconnected operations; 
            /// therefore, it is inappropriate for some client/server applications,
            /// such as a mail server.
            /// </summary>
            LOGON32_LOGON_INTERACTIVE = 2,

            /// <summary>
            /// This logon type is intended for high performance servers to authenticate plaintext passwords.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_NETWORK = 3,

            /// <summary>
            /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without 
            /// their direct intervention. This type is also for higher performance servers that process many plaintext
            /// authentication attempts at a time, such as mail or Web servers. 
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_BATCH = 4,

            /// <summary>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled. 
            /// </summary>
            LOGON32_LOGON_SERVICE = 5,

            /// <summary>
            /// This logon type is for GINA DLLs that log on users who will be interactively using the computer. 
            /// This logon type can generate a unique audit record that shows when the workstation was unlocked. 
            /// </summary>
            LOGON32_LOGON_UNLOCK = 7,

            /// <summary>
            /// This logon type preserves the name and password in the authentication package, which allows the server to make 
            /// connections to other network servers while impersonating the client. A server can accept plaintext credentials 
            /// from a client, call LogonUser, verify that the user can access the system across the network, and still 
            /// communicate with other servers.
            /// NOTE: Windows NT:  This value is not supported. 
            /// </summary>
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,

            /// <summary>
            /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            /// The new logon session has the same local identifier but uses different credentials for other network connections. 
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT:  This value is not supported. 
            /// </summary>
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum LogonProvider
        {
            /// <summary>
            /// Use the standard logon provider for the system. 
            /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name 
            /// is not in UPN format. In this case, the default provider is NTLM. 
            /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
            /// </summary>
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        }

        #endregion Enums

        #region Structs

        /// <summary>
        /// Contains a 64-bit value representing the number of 100-nanosecond intervals since
        /// January 1, 1601 (UTC).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        /// <summary>
        /// Used to pass information to <see cref="CredUIPromptForCredentials"/> and
        /// <see cref="CredUIPromptForWindowsCredentials"/>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        #endregion Structs

        #region NativeMethods P/Invokes

        /// <summary>
        /// Retrieves a string from the specified section in an initialization file.
        /// </summary>
        /// <param name="lpAppName">The name of the section containing the key name. If 
        /// <see langword="null"/>, copies all section names in the file to 
        /// <paramref name="lpReturnedString"/>.</param>
        /// <param name="lpKeyName">The name of the key whose associated string is to be retrieved. 
        /// If <see langword="null"/>, all key names in the section specified by 
        /// <paramref name="lpAppName"/> are copied to <paramref name="lpReturnedString"/>.</param>
        /// <param name="lpDefault">If the <paramref name="lpKeyName"/> key cannot be found in the 
        /// initialization file, copies the default string to the 
        /// <paramref name="lpReturnedString"/> buffer. If <see langword="null"/>, the default is 
        /// the empty string, "".</param>
        /// <param name="lpReturnedString">The buffer that receives the retrieved string.</param>
        /// <param name="nSize">The size of the buffer pointed to by 
        /// <paramref name="lpReturnedString"/>, in characters.</param>
        /// <param name="lpFileName">The name of the initialization file. If this parameter does 
        /// not contain a full path to the file, the system searches for the file in the Windows 
        /// directory.</param>
        /// <returns>The number of characters copied to the buffer, not including the terminating 
        /// null character.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms724353%28VS.85%29.aspx"/>
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, 
            string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// Copies a string into the specified section of an initialization file.
        /// </summary>
        /// <param name="lpAppName">The name of the section (case-insensitive) to which the string 
        /// will be copied. If the section does not exist, it is created.</param>
        /// <param name="lpKeyName">The name of the key to be associated with a string. If the key 
        /// does not exist in the specified section, it is created. If <see langword="null"/>, the 
        /// entire section, including all entries within the section, is deleted.</param>
        /// <param name="lpString">A string to be written to the file. If <see langword="null"/>, 
        /// the key pointed to by the <paramref name="lpKeyName"/> is deleted.</param>
        /// <param name="lpFileName">The name of the initialization file.</param>
        /// <returns>Nonzero if successful. Zero if the function fails or if it flushes the cached 
        /// version of the most recently accessed initialization file.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms725501%28VS.85%29.aspx"/>
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, 
            string lpString, string lpFileName);

        /// <summary>
        /// Checks whether the specified process is running under Wow64 on a 64 bit OS.
        /// </summary>
        /// <param name="processHandle">The process handle to check.</param>
        /// <param name="wow64Process">Out parameter containing the result of
        /// the Wow64 check. If <see langword="true"/> then the process is running
        /// under Wow64.</param>
        /// <returns>Whether the function was successful or not.</returns>
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr processHandle,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        /// <summary>
        /// Notifies the windows shell of changes that have been made.
        /// see MSDN for complete description of SHChangeNotify function.
        /// http://msdn.microsoft.com/en-us/library/bb762118%28VS.85%29.aspx
        /// </summary>
        /// <param name="wEventId">The event ID associated with the event that occurred.</param>
        /// <param name="uFlags">Flags that, when combined bitwise with SHCNF_TYPE,
        /// indicate the meaning of the dwItem1 and dwItem2 parameters.</param>
        /// <param name="dwItem1">Optional. First event-dependent value.</param>
        /// <param name="dwItem2">Optional. Second event-dependent value.</param>
        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(HChangeNotifyEventID wEventId, HChangeNotifyFlags uFlags,
                                           IntPtr dwItem1,
                                           IntPtr dwItem2);

        /// <summary>
        /// Retrieves information about the specified registry key.
        /// </summary>
        /// <param name="hKey">A handle to an open registry key.</param>
        /// <param name="lpClass">A pointer to a buffer that receives the user-defined class of the
        /// key. This parameter can be NULL.</param>
        /// <param name="lpcbClass">A pointer to a variable that specifies the size of the buffer
        /// pointed to by the lpClass parameter, in characters. If lpClass is NULL, lpcClass can
        /// be NULL.</param>
        /// <param name="lpReserved">This parameter is reserved and must be NULL.</param>
        /// <param name="lpcSubKeys">A pointer to a variable that receives the number of subkeys
        /// that are contained by the specified key. This parameter can be NULL.</param>
        /// <param name="lpcbMaxSubKeyLen">A pointer to a variable that receives the size of the
        /// key's subkey with the longest name, in Unicode characters, not including the
        /// terminating null character. This parameter can be NULL.</param>
        /// <param name="lpcbMaxClassLen">A pointer to a variable that receives the size of the
        /// longest string that specifies a subkey class, in Unicode characters. The count returned
        /// does not include the terminating null character. This parameter can be NULL.</param>
        /// <param name="lpcValues">A pointer to a variable that receives the number of values that
        /// are associated with the key. This parameter can be NULL.</param>
        /// <param name="lpcbMaxValueNameLen">A pointer to a variable that receives the size of the
        /// key's longest value name, in Unicode characters. The size does not include the
        /// terminating null character. This parameter can be NULL.</param>
        /// <param name="lpcbMaxValueLen">A pointer to a variable that receives the size of the
        /// longest data component among the key's values, in bytes. This parameter can be NULL.
        /// </param>
        /// <param name="lpcbSecurityDescriptor">A pointer to a variable that receives the size of
        /// the key's security descriptor, in bytes. This parameter can be NULL.</param>
        /// <param name="lpftLastWriteTime">A pointer to a FILETIME structure that receives the
        /// last write time. This parameter can be NULL.</param>
        /// <returns>If the function succeeds, the return value is ERROR_SUCCESS.</returns>
        [DllImport("advapi32.dll", EntryPoint = "RegQueryInfoKey", CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        static extern int RegQueryInfoKey(
            IntPtr hKey, out StringBuilder lpClass, ref uint lpcbClass,
            IntPtr lpReserved, out uint lpcSubKeys, out uint lpcbMaxSubKeyLen,
            out uint lpcbMaxClassLen, out uint lpcValues, out uint lpcbMaxValueNameLen,
            out uint lpcbMaxValueLen, out uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        /// <summary>
        /// Gets the number of milliseconds that have elapsed since the system was started.
        /// </summary>
        /// <returns>The number of milliseconds that have elapsed since the system was started.
        /// </returns>
        [DllImport("kernel32.dll")]
        static extern long GetTickCount64();

        /// <summary>
        /// Determines whether this process is running internally at Extract by checking for drive
        /// mappings to either fnp2 or es-it-dc-01.
        /// </summary>
        /// <returns> <see langword="true"/> if the software is running internally at Extract;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        [DllImport("BaseUtils.dll", EntryPoint = "?isInternalToolsLicensed@@YA_NXZ",
            CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true,
            CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool isInternalToolsLicensed();

        // Decode requirements encoded as #+[U|L|D|P]+
        //  i.e., one or more digits specifying the minimum length followed by letters denoting the required character categories
        //  where U = Uppercase, L = Lowercase, D = Digit and P = Punctuation
        // If encodedRequirements is empty then the only requirement is length > 0
        // If encodedRequirements is invalid then 8ULDP will be used (require length >= 8 and at least one of each category)
        [DllImport("BaseUtils.dll", EntryPoint = "?decodePasswordComplexityRequirements@Util@@YAXPBDAAJAA_N222@Z",
            CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void decodePasswordComplexityRequirements(
            string complexityRequirements,
            out int lengthRequirement,
            [MarshalAs(UnmanagedType.U1)] out bool requireUppercase,
            [MarshalAs(UnmanagedType.U1)] out bool requireLowercase,
            [MarshalAs(UnmanagedType.U1)] out bool requireDigit,
            [MarshalAs(UnmanagedType.U1)] out bool requirePunctuation);

        /// <summary>
        /// Converts a command-line string into an array of args, including the program name
        /// </summary>
        [DllImport("shell32.dll", SetLastError = true)]
        internal static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);


        /// <summary>
        /// The Windows GetVolumeInformation function
        /// </summary>
        /// <param name="PathName">Path for the volume to get information on such as C:\</param>
        /// <param name="VolumeNameBuffer">Buffer to receive the name of the volume</param>
        /// <param name="VolumeNameSize">The length of the buffer in <paramref name="VolumeNameBuffer"/></param>
        /// <param name="VolumeSerialNumber">Reference to a variable for the Volume Serial Number</param>
        /// <param name="MaximumComponentLength">Reference to a variable for the Maximum file name component (between \)</param>
        /// <param name="FileSystemFlags">Reference to a variable for the flags associated with the specified file system</param>
        /// <param name="FileSystemNameBuffer">Buffer to receive the name of the file system</param>
        /// <param name="FileSystemNameSize">The length of the buffer in <paramref name="FileSystemNameSize"/></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetVolumeInformation(
            string PathName,
            StringBuilder VolumeNameBuffer,
            UInt32 VolumeNameSize,
            ref UInt32 VolumeSerialNumber,
            ref UInt32 MaximumComponentLength,
            ref UInt32 FileSystemFlags,
            StringBuilder FileSystemNameBuffer,
            UInt32 FileSystemNameSize);

        /// <summary>
        /// Use the file extension or Nuance/LeadTools to determine the number of pages in an image
        /// </summary>
        /// <param name="szImageFileName">The file to check</param>
        /// <returns>1 if the image extension is .txt or .rtf, else attempts to get the number of pages using
        /// an imaging toolkit</returns>
        [DllImport("LeadUtils.dll", EntryPoint = "?getNumberOfPagesInImage@@YAHPBD@Z",
            CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int getNumberOfPagesInImage(string szImageFileName);

        /// <summary>
        /// Check whether LeadTools PDF write support is licensed
        /// </summary>
        [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
        [DllImport("COMLMCore.dll", EntryPoint = "?isPDFLicensed@LicenseManagement@@SA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool isPDFLicensed();

        #endregion NativeMethods P/Invokes

        #region NativeMethods Methods

        /// <summary>
        /// Retrieves a string from the specified section in an initialization file.
        /// </summary>
        /// <param name="file">The ini file from which to read.</param>
        /// <param name="section">The section in which the <paramref name="key"/> appears.</param>
        /// <param name="key">The key to read.</param>
        /// <returns>The value of the <paramref name="key"/> in the specified 
        /// <paramref name="section"/> of the initialization <paramref name="file"/>.</returns>
        public static string ReadIniFileString(string file, string section, string key)
        {
            StringBuilder result = new StringBuilder(_MAX_BUFFER_SIZE);
            uint size = 
                GetPrivateProfileString(section, key, "", result, (uint)result.Capacity, file);
            if (size >= result.Capacity - 1)
            {
                ExtractException ee = new ExtractException("ELI27069", 
                    "Ini file value is too large.");
                ee.AddDebugData("Ini file", file, false);
                ee.AddDebugData("Section", section, false);
                ee.AddDebugData("Key", key, false);
                ee.AddDebugData("Max characters", result.Capacity, false);
                throw ee;
            }

            return result.ToString();
        }

        /// <summary>
        /// Writes the specified value to the specified initialization (ini) file.
        /// </summary>
        /// <param name="file">The ini file in which to write.</param>
        /// <param name="section">The section in which to write.</param>
        /// <param name="key">The key of the value to write.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteIniFileString(string file, string section, string key, string value)
        {
            try
            {
                bool success = WritePrivateProfileString(section, key, value, file);

                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27707",
                        "Unable to write to ini file.", ex);
                ee.AddDebugData("Ini file", file, false);
                ee.AddDebugData("Section", section, false);
                ee.AddDebugData("Key", key, false);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Checks whether the specified process is running under Wow64.
        /// </summary>
        /// <param name="process">The process to check.</param>
        /// <returns><see langword="true"/> if <paramref name="process"/> is
        /// running under Wow64 and <see langword="false"/> otherwise.</returns>
        public static bool IsProcessRunningWow64(Process process)
        {
            try
            {
                bool isWow64;
                bool success = IsWow64Process(process.Handle, out isWow64);
                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return isWow64;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30188", ex);
            }
        }

        /// <summary>
        /// Notifies windows shell that file association lists have been modified.
        /// </summary>
        public static void NotifyFileAssociationsChanged()
        {
            try
            {
                SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED,
                    HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30191", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> the registry key or any of its values were last written
        /// to.
        /// </summary>
        /// <param name="registryKey">The registry key to check.</param>
        /// <returns>The <see cref="DateTime"/> the registry key or any of its values were last
        /// written to.</returns>
        public static DateTime GetRegistryKeyLastWriteTime(RegistryKey registryKey)
        {
            try
            {
                StringBuilder lpClass;
                uint lpcbClass = 0;
                IntPtr lpReserved = IntPtr.Zero;
                uint lpcSubKeys;
                uint lpcbMaxSubKeyLen;
                uint lpcbMaxClassLen;
                uint lpcValues;
                uint lpcbMaxValueNameLen;
                uint lpcbMaxValueLen;
                uint lpcbSecurityDescriptor;
                FILETIME lpftLastWriteTime = new FILETIME();
                int result = RegQueryInfoKey(registryKey.Handle.DangerousGetHandle(), out lpClass,
                    ref lpcbClass, lpReserved, out lpcSubKeys, out lpcbMaxSubKeyLen,
                    out lpcbMaxClassLen, out lpcValues, out lpcbMaxValueNameLen,
                    out lpcbMaxValueLen, out lpcbSecurityDescriptor, ref lpftLastWriteTime);

                // Throw an exception if the result is not ERROR_SUCCESS.
                if (result != 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                long lastWriteTime = (((long)lpftLastWriteTime.dwHighDateTime) << 32)
                                     + (long)lpftLastWriteTime.dwLowDateTime;

                DateTime lastWrite = DateTime.FromFileTime(lastWriteTime);
                return lastWrite;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32871");
            }
        }

        /// <summary>
        /// Gets the time elapsed since the system was last started or rebooted.
        /// </summary>
        /// <returns>The <see cref="TimeSpan"/> elapsed since the system was last started or
        /// rebooted.</returns>
        public static TimeSpan SystemUptime
        {
            get
            {
                try
                {
                    TimeSpan systemUptime;

                    // [DotNetRCAndUtils:866]
                    // GetTickCount64 is not available on XP/Server 2003
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        long millisecondsUptime = GetTickCount64();

                        // 100 nanoseconds * 10,000 = 1 millisecond
                        systemUptime = new TimeSpan(millisecondsUptime * 10000);
                    }
                    else
                    {
                        // The "System Up Time" performance counter is another way to get up time,
                        // but not all users have permission to read them on >= Vista.
                        using (var uptimeCounter = new PerformanceCounter("System", "System Up Time"))
                        {
                            // The first call to NextValue will return 0. Call an extra time first to
                            // avoid this issue.
                            uptimeCounter.NextValue();
                            systemUptime = TimeSpan.FromSeconds(uptimeCounter.NextValue());
                        }
                    }

                    return systemUptime;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35293");
                }
            }
        }

        #endregion NativeMethods Methods
    }
}