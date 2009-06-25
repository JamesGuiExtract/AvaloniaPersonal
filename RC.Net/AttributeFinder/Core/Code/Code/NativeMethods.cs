using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a wrapper around calls into native code.
    /// </summary>
    internal static class NativeMethods
    {
        #region NativeMethods Constants

        /// <summary>
        /// The maximum profile profileSection size.
        /// </summary>
        const int _MAX_BUFFER = 32767;

        #endregion NativeMethods Constants

        #region NativeMethods P/Invokes

        /// <summary>
        /// Retrieves all the keys and values for the specified profileSection of an initialization file.
        /// </summary>
        /// <param name="lpAppName">The name of the profileSection in the initialization file.</param>
        /// <param name="lpReturnedString">A pointer to a buffer that receives the key name and 
        /// value pairs associated with the named profileSection. The buffer is filled with one or more 
        /// null-terminated profileSection; the last string is followed by a second null character.
        /// </param>
        /// <param name="nSize">The size of the buffer pointed to by the 
        /// <paramref name="lpReturnedString"/> parameter, in characters. The maximum profile 
        /// profileSection size is <see cref="_MAX_BUFFER"/>.</param>
        /// <param name="lpFileName">The name of the initialization file. If this parameter does 
        /// not contain a full path to the file, the system searches for the file in the Windows 
        /// directory.</param>
        /// <returns>The number of characters copied to the buffer, not including the terminating 
        /// null character. If the buffer is not large enough to contain all the key name and value 
        /// pairs associated with the named profileSection, the return value is equal to 
        /// <paramref name="nSize"/> minus two.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms724348(VS.85).aspx"/>
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, 
            uint nSize, string lpFileName);

        #endregion NativeMethods P/Invokes

        #region NativeMethods Methods

        /// <summary>
        /// Gets the key value pairs in the specified section of the specified INI file.
        /// </summary>
        /// <param name="section">The section of the INI file from which to retrieve values.</param>
        /// <param name="fileName">The name of the INI file.</param>
        /// <returns>The key value pairs in the specified section of the specified INI file.</returns>
        public static IDictionary<string, string> GetSectionFromFile(string section, string fileName)
        {
            // Ensure the file exists
            if (!File.Exists(fileName))
            {
                ExtractException ee = new ExtractException("ELI26460", "Invalid file name.");
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }

            // Get the private profile section
            string profileSection = GetPrivateProfileSection(section, fileName);

            // Split the section into lines
            string[] lines = profileSection.Split('\0');

            // Split each line into a key value pair
            Dictionary<string, string> result = new Dictionary<string,string>(lines.Length);
            foreach (string line in lines)
            {
                long i = line.IndexOf('=');
                string key = line.Substring(0, i);
                string value = line.Substring(i + 1);
                result.Add(key, value);
            }

            return result;
        }

        /// <summary>
        /// Gets the specified section of the specified INI file.
        /// </summary>
        /// <param name="section">The section of the INI file from which to retrieve values.</param>
        /// <param name="fileName">The name of the INI file.</param>
        /// <returns>The specified section of the specified INI file.</returns>
        static string GetPrivateProfileSection(string section, string fileName)
        {
            IntPtr profileSection = IntPtr.Zero;
            try
            {
                // Allocate space for the result
                profileSection = Marshal.AllocCoTaskMem(_MAX_BUFFER);

                // Get the lines in this section
                uint bytesRead = GetPrivateProfileSection(section, profileSection, _MAX_BUFFER, fileName);
                if (bytesRead == (_MAX_BUFFER - 2))
                {
                    ExtractException ee = new ExtractException("ELI26461",
                        "Unable to read ini file.");
                    ee.AddDebugData("File name", fileName, false);
                    throw ee;
                }

                // Subtract 1 from bytesRead to remove trailing \0
                return Marshal.PtrToStringAuto(profileSection, (int)(bytesRead - 1));
            }
            finally
            {
                if (profileSection != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(profileSection);
                }
            }
        }

        #endregion NativeMethods Methods
    }
}
