using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace Extract.Process.Logger
{
    /// <summary>
    /// Interface definition for the ILogDBInfo
    /// </summary>
    [ComVisible(true)]
    [Guid("2E4D9915-EB29-4205-90ED-126608E28219")]
    //[CLSCompliant(false)]
    public interface ILogDBInfo
    {
        /// <summary>
        /// Database server to log
        /// </summary>
        string DatabaseServer { get; set; }

        /// <summary>
        /// Database on database server to log
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// Directory for the log file 
        /// </summary>
        string LogDirectory { get; set; }

        /// <summary>
        /// Name of file to write logs to
        /// </summary>
        string LogFileName { get; set; }

        /// <summary>
        /// Polling time between calls to log locks
        /// </summary>
        int PollingTime { get; set; }

        /// <summary>
        /// Start the logging process
        /// </summary>
        void StartLogging();

        /// <summary>
        /// Stop the logging process
        /// </summary>
        void StopLogging();
    }

 
}