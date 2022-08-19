using System;
using Extract;


namespace Extract.ErrorHandling
{
    public interface IExtractException
    {

        /// <summary>
        /// Adds a key and <see cref="EventArgs"/> pair of debug data to the 
        /// <see cref="ExtractException"/> and optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The string to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        void AddDebugData(string debugDataName, EventArgs debugDataValue, bool encrypt = false);

        /// <summary>
        /// Adds a key and string pair of debug data to the <see cref="ExtractException"/> and 
        /// optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The string to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        void AddDebugData(string debugDataName, string debugDataValue, bool encrypt = false);

        /// <overloads>Adds debug data to the exception.</overloads>
        /// <summary>
        /// Adds a key and value type pair of debug data to the <see cref="ExtractException"/> and
        /// optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The value to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        void AddDebugData(string debugDataName, ValueType debugDataValue, bool encrypt = false);

        /// <summary>
        /// Returns the exception as a stringized byte steam
        /// </summary>
        /// <returns></returns>
        string AsStringizedByteStream();

        /// <summary>
        /// Formats the exception as an output string that contains metadata fields for the exception
        /// as well as the stringized exception itself. (This is the format used by Log or SaveTo).
        /// </summary>
        /// <returns>The exception formatted as an output string.</returns>
        string CreateLogString();
        string CreateLogString(DateTime time);

        /// <summary>
        /// Display this exception using the standard exception viewer window used by all
        /// of Extract Systems' products.
        /// </summary>
        void Display();

        /// <summary>
        /// Log this exception to the standard exception log used by all of
        /// Extract Systems' products.
        /// <para>NOTE: This method should only be called once on a given ExtractException
        /// object.  If this method is called more than once on a given ExtractException
        /// object, the encrypted stack trace may contain duplicate entries.</para>
        /// </summary>
        void Log();

        /// <summary>
        /// Log this exception to the standard exception log used by all of
        /// Extract Systems' products.
        /// <para>NOTE: This method should only be called once on a given ExtractException
        /// object.  If this method is called more than once on a given ExtractException
        /// object, the encrypted stack trace may contain duplicate entries.</para>
        /// </summary>
        void Log(string fileName);

        /// <summary>
        /// Log this exception to the specified exception log file.
        /// <para>NOTE: This method should only be called once on a given ExtractException
        /// object.  If this method is called more than once on a given ExtractException
        /// object, the encrypted stack trace may contain duplicate entries.</para>
        /// </summary>
        void Log(string fileName, bool forceLocal);

        /// <summary>
        /// Log this exception to the standard exception log used by all of
        /// Extract Systems' products, but adds the specified machine,user, time, etc
        /// information to the log as opposed to using the default values that are read
        /// from the current machine and product instance.
        /// <para>NOTE: This method should only be called once on a given ExtractException
        /// object.  If this method is called more than once on a given ExtractException
        /// object, the encrypted stack trace may contain duplicate entries.</para>
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="dateTimeUtc">The date time UTC that represents the number of
        /// seconds since 01/01/1970 00:00:00.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="noRemote">If <see langword="true"/> then forces the logging
        /// of the exception to the local log even if a remote address is specified in
        /// the registry.</param>
        void Log(string machineName, string userName, Int64 dateTimeUtc, int processId, string applicationName, bool noRemote);
        void Log(string fileName, string machineName, string userName, Int64 dateTimeUtc, int processId, string applicationName, bool noRemote);

        /// <summary>
        /// Property to return the ELICode for the exception
        /// </summary>
        string EliCode { get; }
        string LogPath { get; set; }
    }
}
