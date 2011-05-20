using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.ExceptionService
{
    internal static class InternalHelperExtensionMethods
    {
        #region Constants

        /// <summary>
        /// The default buffer size to use when reading from streams.
        /// </summary>
        const int _DEFAULT_BUFFER = 8192;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Regular expression used to validate hex strings
        /// </summary>
        static readonly Regex _hexValidation = new Regex(@"^[a-fA-F\d]+$");

        #endregion Fields

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
        public static bool IsStringizedByteArray(this string hexValue)
        {
            return hexValue != null
                && hexValue.Length % 2 == 0
                && _hexValidation.IsMatch(hexValue);
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
            if (string.IsNullOrEmpty(hexValue))
            {
                throw new ArgumentNullException("hexValue");
            }
            else if (!hexValue.IsStringizedByteArray())
            {
                throw new ArgumentException("Value was not a stringized byte array.", "hexValue");
            }

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

        #endregion String Methods

        #region Byte Array Methods

        /// <summary>
        /// Converts the <see cref="T:byte[]"/> to a hexadecimal string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A hexadecimal string representation of the
        /// specified <see cref="T:byte[]"/>.</returns>
        public static string ToHexString(this byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            // Create a string builder with a capacity of twice the length of the bytes
            // since it takes two characters to represent each byte
            StringBuilder sb = new StringBuilder(value.Length * 2);
            foreach (var item in value)
            {
                sb.Append(item.ToString("X2", CultureInfo.InvariantCulture));
            }

            // Return the string
            return sb.ToString();
        }

        #endregion Byte Array Methods
    }
}
