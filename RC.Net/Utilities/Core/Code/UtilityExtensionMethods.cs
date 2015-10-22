﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Extract.Licensing;

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

        #region Methods

        /// <summary>
        /// Converts a <see langword="string"/> to a <see langword="bool"/> where "0" and "1" are
        /// recognized as well as "true" and "false".
        /// </summary>
        /// <param name="value">The <see langword="string"/> to be converted.</param>
        /// <returns>The <see langword="bool"/> equivalent.</returns>
        public static bool ToBoolean(this string value)
        {
            try
            {
                if (value == "1")
                {
                    return true;
                }
                else if (value == "0")
                {
                    return false;
                }

                return bool.Parse(value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36996");
            }
        }

        /// <summary>
        /// Converts to type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object ConvertToType(this string value, Type type)
        {
            try
            {
                if (type == typeof(bool))
                {
                    return value.ToBoolean();
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(type);
                    return converter.ConvertFromString(value);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI36923");
                ee.AddDebugData("Type", type.Name, false);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates a memoized version of a unary function
        /// </summary>
        /// <typeparam name="T1">The type of the function parameter</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <param name="fun">The non-memoized unary function</param>
        /// <returns>A memoized version of the function</returns>
        public static Func<T1, TResult> Memoize<T1, TResult>(this Func<T1, TResult> fun)
        {
            try
            {
                var map = new Dictionary<T1, TResult>();
                return a => map.GetOrAdd(a, () => fun(a));
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI38961");
                throw ee;
            }
        }

        /// <summary>
        /// Creates a memoized version of a binary function
        /// </summary>
        /// <typeparam name="T1">The type of the first function parameter</typeparam>
        /// <typeparam name="T2">The type of the second function parameter</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <param name="fun">The non-memoized binary function</param>
        /// <returns>A memoized version of the function</returns>
        public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>
            (this Func<T1, T2, TResult> fun)
        {
            try
            {
                var map = new Dictionary<Tuple<T1, T2>, TResult>();
                return (a, b) => map.GetOrAdd(Tuple.Create(a, b), () => fun(a, b));
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI38962");
                throw ee;
            }
        }


        #endregion Methods

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
