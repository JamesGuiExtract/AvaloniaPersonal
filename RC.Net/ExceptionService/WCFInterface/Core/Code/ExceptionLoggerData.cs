﻿using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;

namespace Extract.ExceptionService
{
    /// <summary>
    /// Helper class containing the data to be serialized across the WCF wire
    /// </summary>
    [Serializable]
    public sealed class ExceptionLoggerData : ISerializable
    {
        #region Constants

        /// <summary>
        /// Constant for the endpoint of the TCP/IP channel for the service.
        /// </summary>
        public static readonly string WcfTcpEndPoint = "TcpESExceptionLogger";

        /// <summary>
        /// Current version of the exception logger data class.
        /// Version 2: Added ELI code value
        /// </summary>
        readonly static int _CURRENT_VERSION = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The exception data to serialize
        /// </summary>
        Exception _data;

        /// <summary>
        /// The ELI code associated with the exception
        /// </summary>
        string _eliCode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerData"/> class.
        /// </summary>
        /// <param name="data">The exception to be logged.</param>
        public ExceptionLoggerData(Exception data)
            : this(data, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerData"/> class.
        /// </summary>
        /// <param name="data">The exception to be logged.</param>
        /// <param name="eliCode">The ELI code to add to the exception data. This
        /// may not be <see langword="null"/></param>
        public ExceptionLoggerData(Exception data, string eliCode)
        {
            if (eliCode == null)
            {
                throw new ArgumentException("Value must not be null", "eliCode");
            }

            _data = data;
            _eliCode = eliCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerData"/> class.
        /// </summary>
        /// <param name="info">The serialization info collection.</param>
        /// <param name="context">The context for the the serialization.</param>
        ExceptionLoggerData(SerializationInfo info, StreamingContext context)
        {
            int version = info.GetInt32("CurrentVersion");
            if (version > _CURRENT_VERSION)
            {
                var ex = new FormatException("Unrecognized object version.");
                ex.Data["VersionLoaded"] = version;
                ex.Data["MaxVersion"] = _CURRENT_VERSION;
                throw ex;
            }

            string data = info.GetString("ExceptionString");
            _data = DeserializeExceptionFromHexString(data);

            if (version > 1)
            {
                _eliCode = info.GetString("EliCode");
            }
        }

        #endregion Constructors

        #region ISerializable Members

        /// <summary>
        /// Serailization method to get the serialized version of this object.
        /// </summary>
        /// <param name="info">The serialization info collection.</param>
        /// <param name="context">The context for the the serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CurrentVersion", _CURRENT_VERSION);
            info.AddValue("ExceptionString", SerializeExceptionToHexString(_data));
            info.AddValue("EliCode", _eliCode);
        }

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

        /// <summary>
        /// Deserializes an exception that has been serialized using a binary formatter
        /// and then converted to a hex string.
        /// </summary>
        /// <param name="hexException">The hex string version of the serialized exception.</param>
        /// <returns>The deserialized verison of the exception.</returns>
        public static Exception DeserializeExceptionFromHexString(string hexException)
        {
            // Convert the hex string back to bytes
            byte[] bytes = ConvertHexStringToBytes(hexException);

            if (bytes != null)
            {
                // Deserialize the exception and return it.
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream.Position = 0;

                    // Read the exception from the stream
                    Exception e = (Exception)formatter.Deserialize(stream);

                    return e;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a <see cref="string"/> of Hex values to an array of  <see cref="byte"/>.
        /// </summary>
        /// <param name="hexValue">A <see cref="string"/> of hex values.  Must
        /// have an even length (every two characters translate to one byte).</param>
        /// <returns>An array of <see cref="byte"/> containing the converted hex characters.</returns>
        static byte[] ConvertHexStringToBytes(string hexValue)
        {
            if (hexValue.Length % 2 == 0)
            {
                // Create an array of bytes to hold the converted bytes
                byte[] bytes = new byte[hexValue.Length / 2];
                for (int i = 0; i < hexValue.Length; i += 2)
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

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the <see cref="Exception"/> associated with this class.
        /// </summary>
        public Exception ExceptionData
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Gets the ELI code for this exception (may be <see cref="String.Empty"/>).
        /// </summary>
        public string EliCode
        {
            get
            {
                return _eliCode;
            }
        }

        #endregion Properties
    }
}
