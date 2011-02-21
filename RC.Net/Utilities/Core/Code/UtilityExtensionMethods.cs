using Extract.Licensing;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Extract.Utilities
{
    /// <summary>
    /// Helper class containing extension methods.
    /// </summary>
    public static class UtilityExtensionMethods
    {
        #region Constants

        /// <summary>
        /// Name of object used in license validation calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(UtilityExtensionMethods).ToString();

        /// <summary>
        /// The default buffer size to use when reading from streams.
        /// </summary>
        const int _DEFAULT_BUFFER = 8192;

        #endregion Constants

        #region Helper Methods

        /// <summary>
        /// Validates the license.
        /// </summary>
        /// <param name="eliCode">The eli code to associate with the license validation check.</param>
        static void ValidateLicense(string eliCode)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                eliCode, _OBJECT_NAME);
        }

        #endregion Helper Methods

        #region Serialization Methods

        /// <summary>
        /// Serializes the specified object to a hexadecimal string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <returns>A hexadecimal string.</returns>
        public static string ToSerializedHexString(this ISerializable data)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException("data");
                }

                ValidateLicense("ELI31807");

                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                    return stream.ToArray().ToHexString();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31808");
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
            try
            {
                if (string.IsNullOrEmpty(serializedHexValue))
                {
                    throw new ArgumentException("Serialized data string must not be null or empty.",
                        "serializedHexValue");
                }

                ValidateLicense("ELI31809");

                var bytes = serializedHexValue.ToByteArray();
                using (var stream = new MemoryStream(bytes))
                {
                    stream.Position = 0;
                    var formatter = new BinaryFormatter();
                    var data = (T)formatter.Deserialize(stream);
                    return data;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31810");
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
            source.ReadFromStream(destination, _DEFAULT_BUFFER);
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
            try
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
                if (destination == null)
                {
                    throw new ArgumentNullException("destination");
                }
                if (bufferSize <= 0)
                {
                    throw new ArgumentOutOfRangeException("bufferSize", bufferSize,
                        "Must be > 0");
                }

                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                do
                {
                    bytesRead = source.Read(buffer, 0, bufferSize);
                    destination.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31811");
            }
        }

        /// <summary>
        /// Converts the specified stream into an array of bytes.
        /// <para><b>Note:</b></para>
        /// Caller is responsible to close the provided stream.
        /// </summary>
        /// <param name="source">The stream to read from.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this Stream source)
        {
            try
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }

                using (var dest = new MemoryStream())
                {
                    source.ReadFromStream(dest);
                    return dest.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31812");
            }
        }

        #endregion Stream Methods
    }
}
