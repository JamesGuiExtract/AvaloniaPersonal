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
        /// Frees a block of task memory previously allocated through a call to the CoTaskMemAlloc
        /// or CoTaskMemRealloc function.
        /// </summary>
        /// <param name="pv">A pointer to the memory block to be freed. If this parameter is NULL,
        /// the function has no effect.</param>
        [DllImport("ole32.dll")]
        static extern void CoTaskMemFree(IntPtr pv);

        /// <summary>
        /// Creates and displays a configurable dialog box that accepts credentials information from
        /// a user. This method should be used only for XP and Server 2003 machines.
        /// </summary>
        /// <param name="uiInfo">A pointer to a CREDUI_INFO structure that contains information for
        /// customizing the appearance of the dialog box.</param>
        /// <param name="targetName">A pointer to a null-terminated string that contains the name of
        /// the target for the credentials, typically a server name. This parameter is used to
        /// identify target information when storing and retrieving credentials.</param>
        /// <param name="reserved">This parameter is reserved for future use.</param>
        /// <param name="authenticationError">Specifies why the credential dialog box is needed. A
        /// caller can pass this Windows error parameter, returned by another authentication call,
        /// to allow the dialog box to accommodate certain errors.</param>
        /// <param name="userNameBuffer">A pointer to a null-terminated string that contains the
        /// user name for the credentials. If a nonzero-length string is passed, the UserName option
        /// of the dialog box is prefilled with the string.</param>
        /// <param name="userNameBufferSize">The maximum number of characters that can be copied to
        /// userName including the terminating null character.</param>
        /// <param name="passwordBuffer">A pointer to a null-terminated string that contains the
        /// password for the credentials. If a nonzero-length string is specified for pszPassword,
        /// the password option of the dialog box will be prefilled with the string.</param>
        /// <param name="passwordBufferSize">The maximum number of characters that can be copied to
        /// password including the terminating null character.</param>
        /// <param name="save">A pointer to a BOOL that specifies the initial state of the Save
        /// check box and receives the state of the Save check box after the user has responded to
        /// the dialog box. If this value is not NULL and CredUIPromptForCredentials returns
        /// NO_ERROR, then pfSave returns the state of the Save check box when the user chose OK in
        /// the dialog box. If the CREDUI_FLAGS_PERSIST flag is specified, the Save check box is not
        /// displayed, but is considered to be selected. If the CREDUI_FLAGS_DO_NOT_PERSIST flag is
        /// specified and CREDUI_FLAGS_SHOW_SAVE_CHECK_BOX is not specified, the Save check box is
        /// not displayed, but is considered to be cleared.</param>
        /// <param name="flags">A DWORD value that specifies special behavior for this function.
        /// </param>
        /// <returns>A <see cref="CredUIReturnCode"/> indicating whether the function succeeded.
        /// </returns>
        [DllImport("credui", CharSet = CharSet.Unicode)]
        static extern CredUIReturnCode CredUIPromptForCredentials(ref CREDUI_INFO uiInfo,
          string targetName,
          IntPtr reserved,
          int authenticationError,
          StringBuilder userNameBuffer,
          uint userNameBufferSize,
          IntPtr passwordBuffer,
          uint passwordBufferSize,
          [MarshalAs(UnmanagedType.Bool)] ref bool save,
          CREDUI_FLAGS flags);

        /// <summary>
        /// Creates and displays a configurable dialog box that accepts credentials information from
        /// a user. This method should be used for Vista or Server 2008 and later machines.
        /// </summary>
        /// <param name="uiInfo">A pointer to a CREDUI_INFO structure that contains information for
        /// customizing the appearance of the dialog box.</param>
        /// <param name="authenticationError">Specifies why the credential dialog box is needed. A
        /// caller can pass this Windows error parameter, returned by another authentication call,
        /// to allow the dialog box to accommodate certain errors.</param>
        /// <param name="authenticatioPackage">On input, the value of this parameter is used to
        /// specify the authentication package for which the credentials in the pvInAuthBuffer
        /// buffer are serialized. To get the appropriate value to use for this parameter on input,
        /// call the LsaLookupAuthenticationPackage function and use the value of the
        /// AuthenticationPackage parameter of that function.</param>
        /// <param name="inAuthBuffer">A pointer to a credential BLOB that is used to populate the
        /// credential fields in the dialog box. Set the value of this parameter to NULL to leave
        /// the credential fields empty.</param>
        /// <param name="inAuthBufferSize">The size, in bytes, of the InAuthBuffer buffer.</param>
        /// <param name="outAuthBuffer">The address of a pointer that, on output, specifies the
        /// credential BLOB. For Kerberos, NTLM, or Negotiate credentials, call the
        /// CredUnPackAuthenticationBuffer function to convert this BLOB to string representations
        /// of the credentials. When you have finished using the credential BLOB, clear it from
        /// memory by calling the SecureZeroMemory function, and free it by calling the
        /// CoTaskMemFree function.</param>
        /// <param name="outAuthBufferSize">The size, in bytes, of the outAuthBuffer buffer.
        /// </param>
        /// <param name="save">A boolean value that, on input, specifies whether the Save check box
        /// is selected in the dialog box that this function displays. On output, the value of this
        /// parameter specifies whether the Save check box was selected when the user clicks the
        /// Submit button in the dialog box. Set this parameter to NULL to ignore the Save check
        /// box.</param>
        /// <param name="flags">A value that specifies behavior for this function. This value can be
        /// a bitwise-OR combination of one or more of the following values.</param>
        /// <returns>A <see cref="CredUIReturnCode"/> indicating whether the function succeeded.
        /// </returns>
        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        static extern CredUIReturnCode CredUIPromptForWindowsCredentials(
          ref CREDUI_INFO uiInfo,
          int authenticationError,
          ref uint authenticatioPackage,
          IntPtr inAuthBuffer,
          uint inAuthBufferSize,
          out IntPtr outAuthBuffer,
          out uint outAuthBufferSize,
          [MarshalAs(UnmanagedType.Bool)] ref bool save,
          PromptForWindowsCredentialsFlags flags);

        /// <summary>
        /// Converts a string user name and password into an authentication buffer.
        /// </summary>
        /// <param name="flags">Specifies how the credential should be packed.</param>
        /// <param name="userName">The user name to be converted. For domain users, the string must
        /// be in the following format: DomainName\UserName</param>
        /// <param name="password">The password to be converted.</param>
        /// <param name="credBuffer">A pointer to an array of bytes that, on output, receives the
        /// packed authentication buffer. This parameter can be NULL to receive the required buffer
        /// size in the pcbPackedCredentials parameter.</param>
        /// <param name="credBufferSize">A pointer to a DWORD value that specifies the size, in
        /// bytes, of the pPackedCredentials buffer. On output, if the buffer is not of sufficient
        /// size, specifies the required size, in bytes, of the pPackedCredentials buffer.</param>
        /// <returns>TRUE if the function succeeds; otherwise, FALSE.</returns>
        [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CredPackAuthenticationBuffer(
          CredPackFlags flags,
          string userName,
          string password,
          IntPtr credBuffer,
          ref uint credBufferSize);

        /// <summary>
        /// Converts an authentication buffer into a string user name and password.
        /// </summary>
        /// <param name="flags">Specifies how the credentials are packed.</param>
        /// <param name="credBuffer">A pointer to the authentication buffer to be converted.</param>
        /// <param name="credBufferSize">The size, in bytes, of the pAuthBuffer buffer.</param>
        /// <param name="userNameBuffer">A pointer to a null-terminated string that receives the
        /// user name.</param>
        /// <param name="userNameBufferSize">A pointer to a DWORD value that specifies the size, in
        /// characters, of the userNameBuffer.</param>
        /// <param name="domainNameBuffer">A pointer to a null-terminated string that receives the
        /// name of the user's domain.</param>
        /// <param name="domainNameBufferSize">A pointer to a DWORD value that specifies the size,
        /// in characters, of the domainNameBuffer buffer. </param>
        /// <param name="passwordBuffer">A pointer to a null-terminated string that receives the
        /// password.</param>
        /// <param name="passwordBufferSize">A pointer to a DWORD value that specifies the size, in
        /// characters, of the passwordBuffer.</param>
        /// <returns>TRUE if the function succeeds; otherwise, FALSE.</returns>
        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CredUnPackAuthenticationBuffer(CredPackFlags flags, IntPtr credBuffer,
            uint credBufferSize, StringBuilder userNameBuffer, ref uint userNameBufferSize,
            StringBuilder domainNameBuffer, ref uint domainNameBufferSize, IntPtr passwordBuffer,
            ref uint passwordBufferSize);

        /// <summary>
        /// Extracts the domain and user account name from a fully qualified user name.
        /// </summary>
        /// <param name="userName">String that contains the user name to be parsed. The name must be
        /// in UPN or down-level format, or a certificate.</param>
        /// <param name="userBuffer">Pointer to a null-terminated string that receives the user
        /// account name.</param>
        /// <param name="userBufferSize">Maximum number of characters to write to the pszUser string
        /// including the terminating null character.</param>
        /// <param name="domainBuffer">Pointer to a null-terminated string that receives the domain
        /// name. If pszUserName specifies a certificate, pszDomain will be NULL.</param>
        /// <param name="domainBufferSize">Maximum number of characters to write to the pszDomain
        /// string including the terminating null character.</param>
        /// <returns>A <see cref="CredUIReturnCode"/></returns>
        [DllImport("credui.dll", EntryPoint = "CredUIParseUserNameW", CharSet = CharSet.Unicode)]
        private static extern CredUIReturnCode CredUIParseUserName(
                string userName,
                StringBuilder userBuffer,
                int userBufferSize,
                StringBuilder domainBuffer,
                int domainBufferSize);

        /// <summary>
        /// The LogonUser function attempts to log a user on to the local computer. If the function
        /// succeeds, you receive a handle to a token that represents the logged-on user. You can
        /// then use this token handle to impersonate the specified user or, in most cases, to
        /// create a process that runs in the context of the specified user.
        /// </summary>
        /// <param name="userName">The name of the user. This is the name of the user account to log
        /// on to. If you use the user principal name (UPN) format, User@DNSDomainName, the domain
        /// parameter must be NULL.</param>
        /// <param name="domain">he name of the domain or server whose account database contains the
        /// userName account. If this parameter is NULL, the user name must be specified in UPN
        /// format. If this parameter is ".", the function validates the account by using only the
        /// local account database.</param>
        /// <param name="password">A pointer to a null-terminated string that specifies the
        /// plaintext password for the user account specified by lpszUsername. When you have
        /// finished using the password, clear the password from memory by calling the
        /// SecureZeroMemory function.</param>
        /// <param name="logonType">The type of logon operation to perform.</param>
        /// <param name="logonProvider">Specifies the logon provider.</param>
        /// <param name="token">Handle variable that receives a handle to a token that represents
        /// the specified user.</param>
        /// <returns>TRUE if the function succeeds; otherwise, FALSE.</returns>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LogonUser(
            StringBuilder userName,
            StringBuilder domain,
            IntPtr password,
            LogonType logonType,
            LogonProvider logonProvider,
            out IntPtr token
            );

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

        /// <summary>
        /// Converts a command-line string into an array of args, including the program name
        /// </summary>
        [DllImport("shell32.dll", SetLastError = true)]
        internal static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

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

        /// <summary>
        /// Displays a prompt for windows credentials and validates the credentials entered against
        /// the domain or machine. If invalid credentials are entered, the prompt will be
        /// re-displayed to allow the user to try again.
        /// </summary>
        /// <param name="parent">The prompting <see cref="Control"/> that, if specified, will cause
        /// the prompt to run modally; if <see langword="null"/> the prompt will be displayed
        /// non-modally.</param>
        /// <param name="caption">The caption to display in the prompt.</param>
        /// <param name="message">The message to display in the prompt.</param>
        /// <param name="defaultToCurrentUser"><see langword="true"/> to initialize the prompt with
        /// the current user's account; <see langword="false"/> to not initialize the prompt with
        /// any account.</param>
        /// <returns>A <see cref="WindowsIdentity"/> representing the authenticated credentials if
        /// successful, or <see langword="null"/> if the user cancelled without having entered valid
        /// credentials.</returns>
        public static WindowsIdentity PromptForAndValidateWindowsCredentials(Control parent,
            string caption, string message, bool defaultToCurrentUser)
        {
            // Initialize the prompt settings
            CREDUI_INFO creduiInfo = new CREDUI_INFO();
            creduiInfo.cbSize = Marshal.SizeOf(creduiInfo);
            creduiInfo.pszCaptionText = caption;
            creduiInfo.pszMessageText = message;
            creduiInfo.hbmBanner = IntPtr.Zero;
            creduiInfo.hwndParent = (parent == null) ? IntPtr.Zero : parent.Handle;

            // Initialize the buffer to receive the username. This will be persisted between
            // attempts so that the username does not need to be re-entered due to a bad password.
            StringBuilder userName = new StringBuilder(256);
            if (defaultToCurrentUser)
            {
                if (!string.IsNullOrWhiteSpace(Environment.UserDomainName))
                {
                    userName.Append(Environment.UserDomainName + "\\");
                }
                userName.Append(Environment.UserName);
            }

            // Indicates any authentication error that should be displayed in the prompt.
            Win32ErrorCode authenticationError = Win32ErrorCode.Success;

            // Loop until successful or the user cancels.
            while (true)
            {
                // For security, the password will never be placed into a managed object and will be
                // cleared and re-initialized with any retries.
                uint passwordBufferSize = 256;
                // Using 16 bit unicode strings, so the buffer size in bytes is chars * 2.
                IntPtr passwordBuffer = Marshal.AllocCoTaskMem((int)passwordBufferSize * 2);
                ClearBuffer(passwordBuffer, passwordBufferSize * 2);

                try
                {
                    // Display the prompt and validate the input for this attempt.
                    WindowsIdentity identity = null;
                    CredUIReturnCode promptResult = PromptForAndValidateWindowsCredentials(
                        creduiInfo, authenticationError, userName, passwordBuffer,
                        passwordBufferSize, out identity);

                    switch (promptResult)
                    {
                        case CredUIReturnCode.ERROR_CANCELLED:
                            // The user hit cancel or the title bar close button.
                            return null;

                        case CredUIReturnCode.NO_ERROR:
                            // The user hit ok. 
                            if (identity != null)
                            {
                                // If an identity was returned, the credentials entered were valid.
                                return identity;
                            }
                            else
                            {
                                // The credentials entered were not valid; retry with an indication
                                // that the credentials were not valid.
                                authenticationError = Win32ErrorCode.LogOnFailure;
                            }
                            break;

                        default:
                            var ee = new ExtractException("ELI37643", "Failed to validate credentials.");
                            ee.AddDebugData("Error code", promptResult, false);
                            throw ee;
                    }
                }
                finally
                {
                    // Ensure the password buffer is cleared and freed after each attempt whether or
                    // not the attempt was successful.
                    ClearBuffer(passwordBuffer, passwordBufferSize * 2);
                    Marshal.FreeCoTaskMem(passwordBuffer);
                }
            }
        }

        /// <summary>
        /// Displays a prompt for windows credentials and validates the credentials entered against
        /// the domain or machine. This overload does not re-display the prompt when incorrect
        /// credentials are entered.
        /// </summary>
        /// <param name="creduiInfo">The <see cref="CREDUI_INFO"/> instance that specifies the
        /// settings for the credential prompt.</param>
        /// <param name="authenticationError">Specifies any error that be indicated as the reason
        /// for the prompt.</param>
        /// <param name="userName">The <see cref="StringBuilder"/> to receive the entered user name.
        /// </param>
        /// <param name="passwordBuffer">An <see cref="IntPtr"/> to an unmanaged buffer to receive
        /// the entered password.
        /// NOTE: For security reasons, don't copy this data into any managed object.</param>
        /// <param name="passwordBufferSize">The size of the <see paramref="passwordBuffer"/> in
        /// characters.</param>
        /// <param name="identity">The <see cref="WindowsIdentity"/> indicated by the credentials
        /// entered if validation was successful; otherwise, <see langword="null"/></param>
        /// <returns>A <see cref="CredUIReturnCode"/> that indicates the result of displaying the
        /// prompt. <see cref="CredUIReturnCode.NO_ERROR"/> indicates credentials were successfully
        /// but does not necessarily mean they were valid; the validity of the credentials is
        /// indicated by whether an <see paramref="identity"/> was returned.</returns>
        static CredUIReturnCode PromptForAndValidateWindowsCredentials(CREDUI_INFO creduiInfo,
            Win32ErrorCode authenticationError, StringBuilder userName, IntPtr passwordBuffer,
            uint passwordBufferSize, out WindowsIdentity identity)
        {
            CredUIReturnCode result;
            identity = null;
            uint userNameSize = (uint)userName.Capacity;
            StringBuilder domain = new StringBuilder(256);
            uint domainSize = (uint)domain.Capacity;
            bool save = false;

            // CredUIPromptForCredentials needs to be used for operating systems before Vista or
            // Server 2008.
            if (Environment.OSVersion.Version.Major < 6)
            {
                result = CredUIPromptForCredentials(ref creduiInfo, 
                    Environment.UserDomainName, IntPtr.Zero, (int)authenticationError,
                    userName, userNameSize, passwordBuffer, passwordBufferSize, ref save,
                    CREDUI_FLAGS.DO_NOT_PERSIST);
            }
            // CredUIPromptForWindowsCredentials should be used for Vista or Server 2008 or later.
            else
            {
                IntPtr inCredBuffer = IntPtr.Zero;
                uint inCredBufferSize = 0;
                IntPtr outCredBuffer = IntPtr.Zero;
                uint outCredBufferSize = 0;
                uint authenticationPackage = 0;

                try
                {
                    // If a username is already pre-filled in, use it to initialize inCredBuffer for
                    // the prompt.
                    if (userName.Length > 0)
                    {
                        // The first attempt is to receive the needed buffer size; it is expected to
                        // fail.
                        bool success = CredPackAuthenticationBuffer(
                            CredPackFlags.CRED_PACK_PROTECTED_CREDENTIALS, userName.ToString(),
                            "", inCredBuffer, ref inCredBufferSize);
                        if (!success)
                        {
                            inCredBuffer = Marshal.AllocCoTaskMem((int)inCredBufferSize);
                            CredPackAuthenticationBuffer(0, userName.ToString(),
                               "", inCredBuffer, ref inCredBufferSize);
                        }
                    }

                    result = CredUIPromptForWindowsCredentials(ref creduiInfo,
                        (int)authenticationError, ref authenticationPackage, inCredBuffer,
                        inCredBufferSize, out outCredBuffer, out outCredBufferSize, ref save, 0);

                    // If the prompt was ok'd extract the credentials that were entered.
                    if (result == CredUIReturnCode.NO_ERROR)
                    {
                        CredUnPackAuthenticationBuffer(
                            CredPackFlags.CRED_PACK_PROTECTED_CREDENTIALS, outCredBuffer,
                            outCredBufferSize, userName, ref userNameSize, domain, ref domainSize,
                            passwordBuffer, ref passwordBufferSize);
                    }
                }
                finally
                {
                    // For security reasons, ensure the credential buffers are cleared and freed
                    // with each attempt.
                    if (outCredBuffer != IntPtr.Zero)
                    {
                        ClearBuffer(outCredBuffer, outCredBufferSize);
                        CoTaskMemFree(outCredBuffer);
                    }

                    if (inCredBuffer != IntPtr.Zero)
                    {
                        ClearBuffer(inCredBuffer, inCredBufferSize);
                        CoTaskMemFree(inCredBuffer);
                    }
                }
            }

            // If the prompt was ok'd, attempt to validate the credentials and return the associated
            // WindowsIdentity.
            if (result == CredUIReturnCode.NO_ERROR)
            {
                identity = ValidateCredentials(userName, domain, passwordBuffer);
            }
                
            return result;
        }

        /// <summary>
        /// Validates the specified credentials.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="passwordBuffer">The unmanaged buffer containing the password.</param>
        /// <returns>If the credentials are valid, <see cref="WindowsIdentity"/> indicated
        /// or <see langword="null"/> if the credentials were not valid.</returns>
        static WindowsIdentity ValidateCredentials(StringBuilder userName, 
            StringBuilder domain, IntPtr passwordBuffer)
        {
            IntPtr loginToken = IntPtr.Zero;

            // In most cases the domain name will be passed in as part of the userName field.
            // Attempt to parse the domain out.
            var parsedUserName = new StringBuilder(256);
            var parsedDomain = new StringBuilder(256);
            if (domain.Length == 0)
            {
                parsedUserName = new StringBuilder(256);
                parsedDomain = new StringBuilder(256);
                CredUIParseUserName(userName.ToString(), parsedUserName,
                    parsedUserName.Capacity, parsedDomain, parsedDomain.Capacity);
            }
            else
            {
                parsedUserName = new StringBuilder(userName.ToString());
                parsedDomain = new StringBuilder(domain.ToString());
            }

            // The Windows XP prompt will suggest "localhost" as a domain but
            // LogonUser will not recognize this special value. If entered, translate it to the
            // explicit machine name.
            if (parsedDomain.ToString().Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                parsedDomain.Clear();
                parsedDomain.Append(Environment.MachineName);
            }

            // Validate the parsed credentials.
            if (LogonUser(parsedUserName, parsedDomain, passwordBuffer,
                LogonType.LOGON32_LOGON_INTERACTIVE, LogonProvider.LOGON32_PROVIDER_DEFAULT,
                out loginToken))
            {
                return new WindowsIdentity(loginToken);
            }

            return null;
        }

        /// <summary>
        /// Zeros the memory in the unmanaged memory buffer specified by <see paramref="buffer"/>.
        /// </summary>
        /// <param name="buffer">An <see cref="IntPtr"/> to the unmanaged memory buffer to clear.
        /// </param>
        /// <param name="length">The length of the buffer in bytes.</param>
        static void ClearBuffer(IntPtr buffer, uint length)
        {
            Marshal.Copy(Enumerable.Repeat((byte)0, (int)length).ToArray(), 0, buffer, (int)length);
        }

        #endregion NativeMethods Methods
    }
}