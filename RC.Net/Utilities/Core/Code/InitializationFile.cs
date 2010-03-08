using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents an initialization (ini) file.
    /// </summary>
    public class InitializationFile
    {
        #region InitializationFile Fields

        /// <summary>
        /// The name of the file.
        /// </summary>
        readonly string _file;

        #endregion InitializationFile Fields

        #region InitializationFile Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationFile"/> class.
        /// </summary>
        /// <param name="file">The full path to the initialization file.</param>
        public InitializationFile(string file)
        {
            // Ensure the file is valid
            string fullPath = FileSystemMethods.GetAbsolutePath(file);
            if (!File.Exists(fullPath))
            {
                ExtractException ee = new ExtractException("ELI27073", 
                    "Specified initialization file does not exist.");
                ee.AddDebugData("File", fullPath, false);
                throw ee;
            }

            _file = fullPath;
        }

        #endregion InitializationFile Constructors

        #region InitializationFile Methods

        /// <summary>
        /// Reads the value of the specified key from the <see cref="InitializationFile"/>.
        /// </summary>
        /// <param name="section">The section containing <paramref name="key"/>.</param>
        /// <param name="key">The key whose value should be retrieved.</param>
        /// <returns>The value of <paramref name="key"/>; the empty string if 
        /// <paramref name="key"/> could not be found in the ini file.</returns>
        public string ReadString(string section, string key)
        {
            try
            {
                return NativeMethods.ReadIniFileString(_file, section, key);
            }
            catch (Exception ex)
            {
                throw GetReadValueException(section, key, ex);
            }
        }

        /// <summary>
        /// Writes the value of the specified key to the <see cref="InitializationFile"/>.
        /// </summary>
        /// <param name="section">The section in which to write the value.</param>
        /// <param name="key">The key whose value should be written.</param>
        /// <param name="value">The value to write.</param>
        public void WriteString(string section, string key, string value)
        {
            try
            {
                NativeMethods.WriteIniFileString(_file, section, key, value);
            }
            catch (Exception ex)
            {
                throw GetWriteValueException(section, key, value, ex);
            }
        }

        /// <summary>
        /// Reads the value of the specified key from the <see cref="InitializationFile"/> as an
        /// <see cref="Int32"/>.
        /// </summary>
        /// <param name="section">The section containing <paramref name="key"/>.</param>
        /// <param name="key">The key whose value should be retrieved.</param>
        /// <returns>The value of <paramref name="key"/> as an <see cref="Int32"/>; zero if the 
        /// <paramref name="key"/> could not be found in the ini file or could not be converted to 
        /// an integer.</returns>
        public int ReadInt32(string section, string key)
        {
            // This is outside the try block, because ReadString generates its own ExtractException
            string value = ReadString(section, key);

            try
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    return 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw GetReadValueException(section, key, ex);
            }
        }

        /// <summary>
        /// Writes the specified value to the <see cref="InitializationFile"/> as an 
        /// <see cref="Int32"/>.
        /// </summary>
        /// <param name="section">The section in which to write the value.</param>
        /// <param name="key">The key whose value should be written.</param>
        /// <param name="value">The value to write.</param>
        public void WriteInt32(string section, string key, int value)
        {
            // This is not in a try block, because WriteString generates its own ExtractException
            WriteString(section, key, value.ToString(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Gets an exception associated with being unable to read value from the 
        /// <see cref="InitializationFile"/>.
        /// </summary>
        /// <param name="section">The section from which the read was attempted.</param>
        /// <param name="key">The key from which the read was attempted.</param>
        /// <param name="ex">The inner exception.</param>
        /// <returns>An exception containing the information provided.</returns>
        ExtractException GetReadValueException(string section, string key, Exception ex)
        {
            ExtractException ee = new ExtractException("ELI27071",
                "Unable to read initialization file value.", ex);
            ee.AddDebugData("File", _file, false);
            ee.AddDebugData("Section", section, false);
            ee.AddDebugData("Key", key, false);
            return ee;
        }

        /// <summary>
        /// Gets an exception associated with being unable to write a value to the 
        /// <see cref="InitializationFile"/>.
        /// </summary>
        /// <param name="section">The section to which the write was attempted.</param>
        /// <param name="key">The key to which the write was attempted.</param>
        /// <param name="value">The value that was attempted to be written.</param>
        /// <param name="ex">The inner exception.</param>
        /// <returns>An exception containing the information provided.</returns>
        ExtractException GetWriteValueException(string section, string key, string value, Exception ex)
        {
            ExtractException ee = new ExtractException("ELI27708",
                "Unable to read initialization file value.", ex);
            ee.AddDebugData("File", _file, false);
            ee.AddDebugData("Section", section, false);
            ee.AddDebugData("Key", key, false);
            ee.AddDebugData("Value", value, false);
            return ee;
        }

        #endregion InitializationFile Methods
    }
}
