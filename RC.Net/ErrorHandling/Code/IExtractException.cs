using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;

namespace Extract.ErrorHandling
{
    public interface IExtractException
    {
        /// <summary>
        /// The application information for the exception
        /// </summary>
        ApplicationStateInfo ApplicationState { get; }

        /// <summary>
        /// Dictionary that contains the DebugData with the key as the order the items are added.
        /// The Dictionary is the name-value pair
        /// </summary>
        IDictionary Data { get; }

        /// <summary>
        /// Property to return the ELICode for the exception
        /// </summary>
        string EliCode { get; set; }

        /// <summary>
        /// Unique Identifier of the exception
        /// </summary>
        Guid ExceptionIdentifier { get; }

        /// <summary>
        /// Time of the exception
        /// </summary>
        DateTime ExceptionTime { get; set; }

        /// <summary>
        /// ID of the active file when the exception was thrown 
        /// - Will be 0 if no active file
        /// </summary>
        Int32 FileID { get; set; }

        /// <summary>
        /// ID of the active action when the exception was thrown
        /// - Will be 0 if no active action
        /// </summary>
        Int32 ActionID { get; set; }

        /// <summary>
        /// The Database Server connected to at the time on the exception
        /// - if not connected to a database will be ""
        /// </summary>
        string DatabaseServer { get; set; }

        /// <summary>
        /// The Database Name connected to at the time on the exception
        /// - if not connected to a database will be ""
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// The Active Logger
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// The LoggingLevel for the exception
        /// </summary>
        LogLevel LoggingLevel { get; set; }

        /// <summary>
        /// Path used for the log file 
        /// </summary>
        string LogPath { get; set; }

        /// <summary>
        /// Resolutions to the exception added by the programmer
        /// </summary>
        List<string> Resolutions { get; }

        /// <summary>
        /// StackTraceValues from .net code
        /// </summary>
        Stack<string> StackTraceValues { get; }

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
        /// Log methods that log the specifie level 
        /// </summary>
        void LogDebug();
        void LogError();
        void LogInfo();
        void LogTrace();
        void LogWarn();
    }
}
