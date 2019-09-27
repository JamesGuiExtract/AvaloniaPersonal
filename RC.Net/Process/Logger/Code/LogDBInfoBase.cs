using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.Process.Logger
{
    /// <summary>
    /// Base class for Db info logging
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(true)]
    public abstract class LogDBInfoBase : ILogDBInfo, IDisposable
    {
        #region Fields

        CancellationTokenSource _cancelToken = new CancellationTokenSource();

        protected CancellationTokenSource CancellationTokenSource
        {
            get
            {
                return _cancelToken;
            }
        }

        protected Task LoggingTask { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default contructor
        /// </summary>
        protected LogDBInfoBase()
        {
        }

        /// <summary>
        /// Constructor that initiallizes all properties
        /// </summary>
        /// <param name="logDirectory">The directory for the log file </param>
        /// <param name="logFileName">The name of the log file</param>
        /// <param name="databaseServer">The Database server containing the database to log db info from</param>
        /// <param name="databaseName">The database on the server to log db info</param>
        /// <param name="pollingTime">Time between log entries</param>
        protected LogDBInfoBase(string logDirectory, string logFileName, string databaseServer, string databaseName, int pollingTime)
        {
            LogDirectory = logDirectory;
            DatabaseName = databaseName;
            DatabaseServer = databaseServer;
            PollingTime = pollingTime;
            LogFileName = logFileName;
        }

        #endregion

        #region ILogDBInfo implementation

        /// <summary>
        /// Database server to log
        /// </summary>
        public string DatabaseServer { get; set; }

        /// <summary>
        /// Database on database server to log
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Directory for the log file 
        /// </summary>
        public string LogDirectory { get; set; }

        /// <summary>
        /// Name of file to write logs to
        /// </summary>
        public string LogFileName { get; set; }

        /// <summary>
        /// Polling time between calls to log db info
        /// </summary>
        public int PollingTime { get; set; } = 10000; // Default to 10 sec


        /// <summary>
        /// Start the logging process
        /// </summary>
        public virtual void StartLogging()
        {
            try
            {
                string fileName = Path.Combine(LogDirectory, LogFileName);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                LoggingTask = Task.Run(() =>
                {
                    try
                    {
                        using (var outputFile = File.CreateText(fileName))
                        {
                            bool headersAdded = false;
                            while (!CancellationTokenSource.IsCancellationRequested)
                            {
                                using (var connection = GetSqlConnection())
                                using (var cmd = connection.CreateCommand())
                                {
                                    connection.Open();

                                    cmd.CommandText = InfoQuery;

                                    AddParameters(cmd);

                                    var reader = cmd.ExecuteReader();
                                    if (!headersAdded)
                                    {
                                        var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName);
                                        outputFile.WriteLine(string.Join(",", columnNames));
                                        headersAdded = true;
                                    }
                                    while (reader.Read())
                                    {
                                        var rowValues = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetValue(i).ToString());
                                        outputFile.WriteLine(string.Join(",", rowValues));
                                    }

                                    reader.Close();
                                    outputFile.Flush();
                                }
                                CancellationTokenSource.Token.WaitHandle.WaitOne(PollingTime);
                            }
                            outputFile.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = new ExtractException("ELI48337", $"Logging exited for {LogFileName}", ex);
                        ee.Log();
                    }
                });
            }
            catch (Exception exception)
            {
                throw exception.CreateComVisible("ELI46814", "Database logging failed to start.");
            }
        }


        /// <summary>
        /// Stop the logging process
        /// </summary>
        public virtual void StopLogging()
        {
            try
            {
                CancellationTokenSource.Cancel();
                LoggingTask?.Wait(PollingTime * 2);
            }
            catch (Exception exception)
            {
                throw exception.CreateComVisible("ELI46815", "Error stopping database db info logging.");
            }
        }

        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_cancelToken != null)
                    {
                        if (!_cancelToken.IsCancellationRequested)
                        {
                            StopLogging();
                        }
                        _cancelToken.Dispose();
                        _cancelToken = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI48331", "Unable to dispose");
            }
        }
        #endregion

        #region Protected Methods


        /// <summary>
        /// Query that returns the data to be logged
        /// NOTE: All columns returned should be logged
        /// </summary>
        protected abstract string InfoQuery { get; }

        /// <summary>
        /// Adds parameters to the SqlCommand that is passed in if required
        /// </summary>
        /// <param name="command">The command</param>
        protected virtual void AddParameters(SqlCommand command)
        {
        }

        protected SqlConnection GetSqlConnection()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = DatabaseServer;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
