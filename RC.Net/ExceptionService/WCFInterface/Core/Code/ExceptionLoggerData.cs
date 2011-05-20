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
        /// Version 4: Added Machine Name, User Name, DateTime, ProcessID, Software Version
        /// </summary>
        readonly static int _CURRENT_VERSION = 4;

        /// <summary>
        /// The length at which to split the serialized exception.
        /// </summary>
        const int _SPLIT_LENGTH = 8000;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The base time to use when calculating the DateTimeUtc value.
        /// </summary>
        static readonly DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets the <see cref="Exception"/> associated with this class.
        /// </summary>
        public Exception ExceptionData { get; private set;}

        /// <summary>
        /// Gets the ELI code for this exception (may be <see cref="String.Empty"/>).
        /// </summary>
        public string EliCode { get; private set; }

        /// <summary>
        /// Gets the machine name associated with the exception data.
        /// </summary>
        public string MachineName { get; private set; }

        /// <summary>
        /// Gets the user name associated with the exception data
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> that has been converted to the number of
        /// seconds since 01/01/1970 00:00:00 UTC.
        /// </summary>
        public int DateTimeUtc { get; private set; }

        /// <summary>
        /// Gets the process ID associated with the exception data.
        /// </summary>
        public int ProcessId { get; private set; }

        /// <summary>
        /// Gets the product version string associated with the exception data.
        /// </summary>
        public string ProductVersion { get; private set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerData"/> class.
        /// </summary>
        /// <param name="data">The exception to be logged.</param>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="dateTimeUtc">The date time in UTC from calling the method
        /// <see cref="DateTime.ToFileTimeUtc"/>.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="productVersion">The product version.</param>
        public ExceptionLoggerData(Exception data, string machineName,
            string userName, long dateTimeUtc, int processId, string productVersion)
            : this(data, string.Empty, machineName, userName, dateTimeUtc, processId, productVersion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLoggerData"/> class.
        /// </summary>
        /// <param name="data">The exception to be logged.</param>
        /// <param name="eliCode">The ELI code to add to the exception data. This
        /// may not be <see langword="null"/></param>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="dateTimeUtc">The date time in UTC from calling the method
        /// <see cref="DateTime.ToFileTimeUtc"/>.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="productVersion">The product version.</param>
        public ExceptionLoggerData(Exception data, string eliCode, string machineName,
            string userName, long dateTimeUtc, int processId, string productVersion)
        {
            if (eliCode == null)
            {
                throw new ArgumentNullException("eliCode", "Value must not be null");
            }

            ExceptionData = data;
            EliCode = eliCode;
            MachineName = machineName ?? string.Empty;
            UserName = userName ?? string.Empty;
            DateTimeUtc = (int)(DateTime.FromFileTimeUtc(dateTimeUtc) - baseTime).TotalSeconds;
            ProcessId = processId;
            ProductVersion = productVersion ?? string.Empty;
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

            // Set the default values
            EliCode = string.Empty;
            MachineName = string.Empty;
            UserName = string.Empty;
            ProcessId = 0;
            ProductVersion = string.Empty;
            if (version < 4)
            {
                // Versions less than 4 did not have time stamp, set to current time
                DateTimeUtc = (int)(DateTime.UtcNow - baseTime).TotalSeconds;
            }

            // Get the ELI code if after version 1
            if (version > 1)
            {
                EliCode = info.GetString("EliCode");
            }

            // Get the serialized exception data
            string data = string.Empty;
            if (version < 3)
            {
                data = info.GetString("ExceptionString");
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
                data = sb.ToString();
            }
            ExceptionData = data.DeserializeFromHexString<Exception>();

            if (version >= 4)
            {
                MachineName = info.GetString("MachineName");
                UserName = info.GetString("UserName");
                DateTimeUtc = info.GetInt32("DateTimeUtc");
                ProcessId = info.GetInt32("ProcessId");
                ProductVersion = info.GetString("ProductVersion");
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
            List<string> strings = SerializeExceptionToHexStrings(ExceptionData);

            // Store the count of string pieces that make up the serialized exception
            info.AddValue("StringCount", strings.Count);
            for (int i = 0; i < strings.Count; i++)
            {
                // Add each piece
                info.AddValue(_EXCEPTION_PIECE_TAG
                    + i.ToString(CultureInfo.InvariantCulture), strings[i]);
            }
            info.AddValue("EliCode", EliCode);
            info.AddValue("MachineName", MachineName);
            info.AddValue("UserName", UserName);
            info.AddValue("DateTimeUtc", DateTimeUtc);
            info.AddValue("ProcessId", ProcessId);
            info.AddValue("ProductVersion", ProductVersion);
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
            string hexException = null;
            try
            {
                hexException = e.ToSerializedHexString();
            }
            catch (SerializationException se)
            {
                // Handle the case where the exception cannot be serialized
                hexException = new UnableToSerializeException("ELI32592", e, se)
                    .ToSerializedHexString();
            }

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
    }
}
