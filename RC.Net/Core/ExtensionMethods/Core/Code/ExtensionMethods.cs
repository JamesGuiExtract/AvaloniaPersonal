using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

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
    }
}
