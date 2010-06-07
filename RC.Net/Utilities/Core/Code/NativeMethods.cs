using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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

        #region enum HChangeNotifyEventID

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
            /// A nonfolder item has been created.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// HChangeNotifyFlags.SHCNF_PATH(A/W) must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_CREATE = 0x00000002,

            /// <summary>
            /// A nonfolder item has been deleted.
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
            /// The name of a nonfolder item has changed.
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

        #endregion // enum HChangeNotifyEventID

        #region public enum HChangeNotifyFlags

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

        #endregion // enum HChangeNotifyFlags

        #region NativeMethods P/Invokes

        /// <summary>
        /// Retrieves a string from the specified section in an initialization file.
        /// </summary>
        /// <param name="lpAppName">The name of the section containing the key name. If 
        /// <see langword="null"/>, copies all section names in the file to 
        /// <paramref name="lpRetunedString"/>.</param>
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

        #endregion NativeMethods Methods
    }
}