using System;
using System.Collections.Generic;
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
        public static string GetPrivateProfileString(string file, string section, string key)
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

        #endregion NativeMethods Methods
    }
}
