using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract
{
    /// <summary>
    /// Class contains helper methods to aid in serialization tasks.
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Deserializes the data from a hexadecimal string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedHexString">The serialized hexadecimal string.</param>
        /// <returns>A new <typeparamref name="T"/> that is equivalent to the serialized
        /// string representation.</returns>
        public static T DeserializeFromHexString<T>(string serializedHexString) where T : ISerializable
        {
            if (string.IsNullOrEmpty(serializedHexString))
            {
                throw new ArgumentException("Serialized data string must not be null or empty.",
                    "serializedHexString");
            }

            var bytes = serializedHexString.ToByteArray();
            using (var stream = new MemoryStream(bytes))
            {
                stream.Position = 0;
                var formatter = new BinaryFormatter();
                var data = (T)formatter.Deserialize(stream);
                return data;
            }
        }
    }

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

        #endregion Serialization Methods
    }
}
