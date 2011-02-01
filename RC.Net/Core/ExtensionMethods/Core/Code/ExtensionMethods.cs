using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract
{
    /// <summary>
    /// Helper class containing extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Used to validate that a string only contains hex characters.
        /// </summary>
        static readonly Regex _hexValidation = new Regex("^[a-fA-F0-9]+$");

        #region Byte Methods

        /// <summary>
        /// Converts the <see cref="T:byte[]"/> to a hexadecimal string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A hexadecimal string representation of the
        /// specified <see cref="T:byte[]"/>.</returns>
        public static string ToHexString(this byte[] value)
        {
            // Create a string builder with a capacity of twice the length of the bytes
            // since it takes two characters to represent each byte
            var sb = new StringBuilder(value.Length * 2);
            foreach (byte bite in value)
            {
                sb.Append(bite.ToString("X2", CultureInfo.InvariantCulture));
            }

            // Return the string
            return sb.ToString();
        }

        #endregion Byte Methods

        #region String Methods

        /// <summary>
        /// Determines whether the specified <paramref name="hexValue"/> is a stringized byte array.
        /// </summary>
        /// <param name="hexValue">The hex value.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <paramref name="hexValue"/>
        /// is stringized byte array; otherwise, <see langword="false"/>.
        /// </returns>
        // Using the word Stringized since that is what this method checks. Is the passed in hexadecimal string
        // a valid stringized byte stream (i.e. string must contain an even number of characters
        // and only contain characters that are valid hexadecimal characters
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Stringized")]
        public static bool IsStringizedByteArray(this string hexValue)
        {
            return hexValue.Length % 2 == 0 && _hexValidation.IsMatch(hexValue);
        }

        /// <summary>
        /// Converts the stringized version of a <see cref="T:byte[]"/> back to a <see cref="T:byte[]"/>.
        /// <para><b>Note:</b></para>
        /// The specified <paramref name="hexValue"/> must be a stringized byte array, otherwise the
        /// return value will be <see langword="null"/>
        /// </summary>
        /// <param name="hexValue">The hex value.</param>
        /// <returns>The converted byte array.</returns>
        public static byte[] ToByteArray(this string hexValue)
        {
            if (hexValue.IsStringizedByteArray())
            {
                var length = hexValue.Length;

                // Create an array to hold the converted bytes
                byte[] bytes = new byte[length / 2];
                for (int i = 0; i < length; i += 2)
                {
                    // Convert each HEX value from the string to a byte (two characters per byte)
                    bytes[i / 2] = Convert.ToByte(hexValue.Substring(i, 2), 16);
                }

                // Return the converted bytes
                return bytes;
            }
            else
            {
                return null;
            }
        }

        #endregion String Methods

        #region Serialization Methods

        /// <summary>
        /// Serializes the specified object to a hexadecimal string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <returns>A hexadecimal string.</returns>
        public static string ToSerializedHexString(this ISerializable data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);
                return stream.ToArray().ToHexString();
            }
        }

        /// <summary>
        /// Deserializes the data from a hexadecimal string.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize from the string.</typeparam>
        /// <param name="serializedHexValue">The serialized hexadecimal string.</param>
        /// <returns>A new <typeparamref name="T"/> that is equivalent to the serialized
        /// string representation.</returns>
        public static T DeserializeFromHexString<T>(this string serializedHexValue) where T : ISerializable
        {
            if (string.IsNullOrEmpty(serializedHexValue))
            {
                throw new ArgumentException("Serialized data string must not be null or empty.",
                    "serializedHexValue");
            }

            var bytes = serializedHexValue.ToByteArray();
            using (var stream = new MemoryStream(bytes))
            {
                stream.Position = 0;
                var formatter = new BinaryFormatter();
                var data = (T)formatter.Deserialize(stream);
                return data;
            }
        }

        #endregion Serialization Methods

        #region Stream Methods

        /// <summary>
        /// Reads from stream with a default buffer size of 32768.
        /// <para><b>Note:</b></para>
        /// Caller is responsible to open and close all streams.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void ReadFromStream(this Stream source, Stream destination)
        {
            source.ReadFromStream(destination, 32768);
        }

        /// <summary>
        /// Reads from stream.
        /// <para><b>Note:</b></para>
        /// Caller is responsible to open and close all streams.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public static void ReadFromStream(this Stream source, Stream destination, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            do
            {
                bytesRead = source.Read(buffer, 0, bufferSize);
                destination.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);
        }

        #endregion Stream Methods

        #region Exception Methods

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

        /// <summary>
        /// Path to the exception helper application.
        /// </summary>
        static readonly string _EXCEPTION_HELPER_APP = Path.Combine(_COMMON_COMPONENTS,
            "ExceptionHelper.exe");

        /// <summary>
        /// Displays the exception in a message box.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void DisplayInMessageBox(this Exception ex)
        {
            MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// Logs the exception with helper app.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="eliCode">The eli code.</param>
        /// <returns><see langword="null"/> if all went well, or an <see cref="Exception"/>
        /// if there was an exception thrown while trying to log the exception.</returns>
        public static Exception LogExceptionWithHelperApp(this Exception ex, string eliCode)
        {
            string tempFile = null;
            try
            {
                if (!File.Exists(_EXCEPTION_HELPER_APP))
                {
                    throw new FileNotFoundException("Exception helper application was not found.",
                        _EXCEPTION_HELPER_APP);
                }

                // Check if there is an eli code, if so add it as debug data
                if (!string.IsNullOrEmpty(eliCode))
                {
                    AddEliDebugData(ex, eliCode);
                }

                // Serialize the exception as a hex string and write to temp file
                string hexException = null;
                try
                {
                    hexException = ex.ToSerializedHexString();
                }
                catch (SerializationException se)
                {
                    hexException = new UnableToSerializeException("ELI31492", ex, se)
                        .ToSerializedHexString();
                }

                tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, hexException);

                // Build the arguments for the exception helper app
                var arguments = string.Concat("\"", tempFile, "\"");

                // Launch the helper app
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = _EXCEPTION_HELPER_APP;
                    process.StartInfo.Arguments = arguments;
                    process.Start();
                    process.WaitForExit();
                }

                return null;
            }
            catch (Exception exception)
            {
                try
                {
                    // If there was a failure logging the exception, try to add an entry to the
                    // application log.
                    var assembly = Assembly.GetCallingAssembly().GetName();
                    EventLog.WriteEntry(assembly.Name,
                        eliCode + " " + exception.ToString(), EventLogEntryType.Error);
                }
                catch { }

                return exception;
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
        /// Adds the specified ELI code as debug data to the exception.
        /// </summary>
        /// <param name="ex">The exception to add the ELI code to.</param>
        /// <param name="eliCode">The ELI code to add.</param>
        // FxCop is catching the "ELICode" key value as an invalid eli code, safe to suppress
        // this warning.
        [SuppressMessage("ExtractRules", "ES0002:MethodsShouldContainValidEliCodes")]
        static void AddEliDebugData(Exception ex, string eliCode)
        {
            try
            {
                string rootKey = "ELICode";
                string key = rootKey;
                int i = 1;
                while (ex.Data.Contains(key))
                {
                    key = rootKey + i.ToString(CultureInfo.InvariantCulture);
                    i++;
                }

                ex.Data.Add(key, eliCode);
            }
            catch
            {
                // Just eat exceptions in this method
            }
        }
        #endregion Exception Methods
    }
}
