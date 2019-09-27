using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Process.Logger
{
    /// <summary>
    /// COM Class for logging database connections
    /// </summary>
    [ComVisible(true)]
    [Guid("CF780571-A59F-4A12-9212-AD0DF606C83D")]
    [ProgId("Extract.Process.Logger.LogDBConnections")]
    [CLSCompliant(false)]
    public class LogDBConnections : LogDBInfoBase
    {
        #region Constants

        static readonly string ConnectionQuery = @"
            SELECT GetDate() timeStamp, DB_NAME([dbid]) AS [DBName]
                 , [hostprocess]
                 , [program_name]
                 , COUNT([dbid]) AS   [NumberOfConnections]
                 , [hostname]
            FROM   
                 [sys].[sysprocesses]
            WHERE  [dbid] > 0 AND spid NOT IN (@@SPID) AND DB_NAME([dbid]) = @DatabaseName
            GROUP BY [dbid]
                   , [hostprocess]
                   , [program_name]
                   , [hostname]
        ";

        #endregion

        #region Constructors

        /// <summary>
        /// Default contructor
        /// </summary>
        public LogDBConnections() :
            base()
        {

        }

        /// <summary>
        /// Constructor that initiallizes all properties
        /// </summary>
        /// <param name="logDirectory">The directory for the log file </param>
        /// <param name="databaseServer">The Database server containing the database to log locks from</param>
        /// <param name="databaseName">The database on the server to log the locks</param>
        /// <param name="pollingTime">Time between log entries</param>
        public LogDBConnections(string logDirectory, string logFileName, string databaseServer, string databaseName, int pollingTime) :
            base(logDirectory, logFileName, databaseServer, databaseName, pollingTime)
        {

        }

        #endregion


        #region Overrides

        /// <summary>
        /// Query that returns the data to be logged
        /// NOTE: All columns returned should be logged
        /// </summary>
        protected override string InfoQuery
        {
            get
            {
                return ConnectionQuery;
            }
        }

        /// <summary>
        /// Adds parameters to the SqlCommand that is passed in if required
        /// </summary>
        /// <param name="command">The command</param>
        protected override void AddParameters(SqlCommand command)
        {
            try
            {
                command?.Parameters.AddWithValue("@DatabaseName", DatabaseName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48333");
            }
        }

        #endregion
    }
}
