using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DocumentAPI
{
    /// <summary>
    /// A simple log class
    /// </summary>
    public class Log
    {
        static StreamWriter _logFile;

        /// <summary>
        /// LogPath - for debugging only, 
        /// </summary>
        // For debugging only
        static public string LogPath { get; private set; }

        /// <summary>
        /// Create - this is called once to create the log file
        /// </summary>
        /// <param name="logPathName">full path of folder to log files too</param>
        public static void Create(string logPathName)
        {
            try
            {
                if (String.IsNullOrEmpty(logPathName))
                {
                    return;
                }

                LogPath = logPathName;

                // Get the path and make sure it is present
                if (!Directory.Exists(logPathName))
                {
                    Directory.CreateDirectory(logPathName);
                }

                if (!logPathName.EndsWith("\\"))
                {
                    logPathName += "\\";
                }

                DateTime dt = DateTime.Now;
                string dtName = "Log_" + dt.ToString("MM_dd_yyyy_hh_mm");
                var fullName = logPathName + dtName + ".log";
                _logFile = new StreamWriter(path: fullName, append: true);

                _logFile.WriteLine($"Started at: {CurrentDateTimeAsString()}...");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        private static string CurrentDateTimeAsString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        /// <summary>
        /// write text to a log
        /// </summary>
        /// <param name="text">text to write - may optionally contain format items (e.g. {0})</param>
        /// NOTE: The following optional arguments are not meant to be explicitly set - let them default and
        /// they will have the correct, intended values.
        /// <param name="memberName">optional name of caller - normally this is specified by the compiler</param>
        /// <param name="filePath">The optional source file of the calling method</param>
        /// <param name="sourceLineNumber">optional caller line number - normally this is specified by the compiler</param>
        public static void WriteLine(string text,
                                     [CallerMemberName] string memberName = "",
                                     [CallerFilePath] string filePath = "",
                                     [CallerLineNumber] int sourceLineNumber = 0)
        {
            var msg = String.Format($"{CurrentDateTimeAsString()}: {text}, called from: {memberName}, file: {filePath}, line: {sourceLineNumber}");
            _logFile?.WriteLine(msg);
            _logFile?.Flush();
        }
    }
}
