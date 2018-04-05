using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Extract64.Core
{
    /// <summary>
    /// Static helper methods for managing exceptions from 64 bit applications.
    /// </summary>
    public static class ExceptionMethods
    {
        /// <summary>
        /// Path to the Extract common components folder.
        /// </summary>
        static readonly string _COMMON_COMPONENTS =
            new Uri( Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

        /// <summary>
        /// Path to the exception helper application.
        /// </summary>
        static readonly string _EXCEPTION_HELPER_APP = Path.Combine(_COMMON_COMPONENTS,
            "ExceptionHelper.exe");

        /// <summary>
        /// Logs the specified exception to the Extract log file.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="fileName">Optional path of file to save the exception to</param>
        public static void LogException(Exception ex, string fileName = null)
        {
            HandleException(ex, false, fileName);
        }

        /// <summary>
        /// Displays the specified exception in the Extract exception viewer.
        /// </summary>
        /// <param name="ex">The exception to display.</param>
        public static void DisplayException(Exception ex)
        {
            HandleException(ex, true);
        }

        /// <summary>
        /// Handles the specified exception by either logging or displaying it
        /// with the Extract exception class.
        /// <para><b>Note:</b></para>
        /// This method handles the exception by serializing it to a file and calling
        /// a 32bit helper app that then calls into the Extract exception framework.
        /// </summary>
        /// <param name="ex">The exception to handle</param>
        /// <param name="display">If <see langword="true"/> then the exception will
        /// be displayed, otherwise it will be logged.</param>
        /// <param name="fileName">Optional path of file to save the exception to</param>
        static void HandleException(Exception ex, bool display, string fileName = null)
        {
            string tempFile = null;
            try
            {
                // Serialize the exception as a hex string and write to temp file
                string hexException = SerializeExceptionToHexString(ex);
                tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, hexException);

                // Build the arguments for the exception helper app
                StringBuilder arguments = new StringBuilder();
                arguments.Append('"');
                arguments.Append(tempFile);
                arguments.Append('"');
                if (display)
                {
                    arguments.Append(" /display");
                }
                if (fileName != null)
                {
                    arguments.Append(" /ef \"" + fileName + "\"");
                }

                // Launch the helper app
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = _EXCEPTION_HELPER_APP;
                    process.StartInfo.Arguments = arguments.ToString();
                    process.Start();
                    process.WaitForExit();
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>
        /// Converts an array of <see cref="byte"/> into a <see cref="string"/> of hex characters.
        /// </summary>
        /// <param name="value">An array of <see cref="byte"/>.  Must not be null.</param>
        /// <returns>A string containing each of the bytes as a two character hex string.</returns>
        /// <throws><see cref="Exception"/>If value is <see langword="null"/>.</throws>
        static string ConvertBytesToHexString(byte[] value)
        {
            // Create a string builder with a capacity of twice the length of the bytes
            // since it takes two characters to represent each byte
            StringBuilder sb = new StringBuilder(value.Length * 2);
            foreach (byte bite in value)
            {
                sb.Append(bite.ToString("X2", CultureInfo.InvariantCulture));
            }

            // Return the string
            return sb.ToString();
        }

        /// <summary>
        /// Serializes an exception using a the binary formatter into a string of hex characters.
        /// </summary>
        /// <param name="e">The exception to be serialized.</param>
        /// <returns>A hex string representing the binary formatted version of the exception.
        /// </returns>
        static string SerializeExceptionToHexString(Exception e)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, e);

                string hexException = ConvertBytesToHexString(stream.ToArray());

                return hexException;
            }
        }

    }
}
