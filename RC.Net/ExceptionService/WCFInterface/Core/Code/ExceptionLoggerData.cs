using System;
using System.Collections.Generic;
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
        /// The tag used to preface each piece of the split exception
        /// </summary>
        static readonly string _EXCEPTION_PIECE_TAG = "String_";

        /// <summary>
        /// Constant for the endpoint of the TCP/IP channel for the service.
        /// </summary>
        public static readonly string WcfTcpEndPoint = "TcpESExceptionLogger";

        /// <summary>
        /// Current version of the exception logger data class.
        /// Version 2: Added ELI code value
        /// Version 3: The exception data is now split into a collection of strings
        /// that are 8000 characters or less when it is serialized so that the
        /// data does not exceed the default XML serializer length limits.
        /// </summary>
        readonly static int _CURRENT_VERSION = 3;

        /// <summary>
        /// The length at which to split the serialized exception.
        /// </summary>
        const int _SPLIT_LENGTH = 8000;

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
            if (version < 3)
            {
                string data = info.GetString("ExceptionString");
                _data = SerializationHelper.DeserializeFromHexString<Exception>(data);
            }
            else
            {
                // Get the string count
                int count = info.GetInt32("StringCount");

                // Initialize capacity to approximate final length
                StringBuilder sb = new StringBuilder(count * _SPLIT_LENGTH);

                // Read each string and append to the StringBuilder
                for (int i = 0; i < count; i++)
                {
                    sb.Append(info.GetString(_EXCEPTION_PIECE_TAG
                        + i.ToString(CultureInfo.InvariantCulture)));
                }

                _data = SerializationHelper.DeserializeFromHexString<Exception>(sb.ToString());
            }

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

            // Serialize to a list of strings
            List<string> strings = SerializeExceptionToHexStrings(_data);

            // Store the count of string pieces that make up the serialized exception
            info.AddValue("StringCount", strings.Count);
            for (int i = 0; i < strings.Count; i++)
            {
                // Add each piece
                info.AddValue(_EXCEPTION_PIECE_TAG
                    + i.ToString(CultureInfo.InvariantCulture), strings[i]);
            }
            info.AddValue("EliCode", _eliCode);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes an exception using a the binary formatter into a string of hex characters.
        /// </summary>
        /// <param name="e">The exception to be serialized.</param>
        /// <returns>A collection of hex strings that when combined represent
        /// the binary formatted version of the exception.
        /// </returns>
        static List<string> SerializeExceptionToHexStrings(Exception e)
        {
            string hexException = e.ToSerializedHexString();

            // Need to split the string at 8000 character increments due to 
            // XML serializer length limitations.
            List<string> strings = new List<string>();
            while (hexException.Length > _SPLIT_LENGTH)
            {
                strings.Add(hexException.Substring(0, _SPLIT_LENGTH));
                hexException = hexException.Remove(0, _SPLIT_LENGTH);
            }

            // Add the remaining string to the list
            strings.Add(hexException);

            return strings;
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
