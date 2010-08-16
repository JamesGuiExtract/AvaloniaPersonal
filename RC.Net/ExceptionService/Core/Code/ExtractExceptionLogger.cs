using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;

namespace Extract.ExceptionService
{
    class ExtractExceptionLogger : IExtractExceptionLogger
    {
        #region Fields

        /// <summary>
        /// Path to the Extract systems folder in the program files directory.
        /// </summary>
        static readonly string _EXTRACT_FOLDER = Path.Combine(
            Environment.GetEnvironmentVariable("ProgramFiles(x86)") ??
            Environment.GetEnvironmentVariable("ProgramFiles"), "Extract Systems");

        /// <summary>
        /// Path to the Extract common components folder.
        /// </summary>
        static readonly string _COMMON_COMPONENTS =
            Path.Combine(_EXTRACT_FOLDER, "CommonComponents");

#if DEBUG
        /// <summary>
        /// Path to the exception helper application.
        /// </summary>
        static readonly string _EXCEPTION_HELPER_APP = @"D:\Engineering\Binaries\Debug\ExceptionHelper.exe";
#else
        /// <summary>
        /// Path to the exception helper application.
        /// </summary>
        static readonly string _EXCEPTION_HELPER_APP = Path.Combine(_COMMON_COMPONENTS,
            "ExceptionHelper.exe");
#endif
        #endregion

        #region Methods

        /// <summary>
        /// Converts an array of <see cref="byte"/> into a <see cref="string"/> of hex characters.
        /// </summary>
        /// <param name="value">An array of <see cref="byte"/>.  Must not be null.</param>
        /// <returns>A string containing each of the bytes as a two character hex string.</returns>
        static string ConvertBytesToHexString(byte[] value)
        {
            if (value != null)
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
            else
            {
                return string.Empty;
            }
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

        #endregion Methods

        #region IExtractExceptionLogger Members

        public void LogException(ExceptionLoggerData exceptionData)
        {
            string tempFile = null;
            try
            {
                // Serialize the exception as a hex string and write to temp file
                string hexException = SerializeExceptionToHexString(exceptionData._data);
                tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, hexException);

                // Build the arguments for the exception helper app
                StringBuilder arguments = new StringBuilder();
                arguments.Append('"');
                arguments.Append(tempFile);
                arguments.Append('"');

                // Launch the helper app
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = _EXCEPTION_HELPER_APP;
                    process.StartInfo.Arguments = arguments.ToString();
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception exception)
            {
                throw new FaultException("Unable to log exception. " + exception.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        #endregion
    }
}
