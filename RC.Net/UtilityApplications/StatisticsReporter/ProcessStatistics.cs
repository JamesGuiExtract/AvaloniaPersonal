using Extract;
using Extract.DataCaptureStats;
using Extract.Interop;
using Extract.Interop.Zip;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using UCLID_COMUTILSLib;

namespace StatisticsReporter
{
    #region Settings Classes

    /// <summary>
    /// Settings used by the per file action
    /// </summary>
    public class PerFileSettings
    {
        /// <summary>
        /// Type of statistics being ran
        /// </summary>
        public string TypeOfStatistics { get; set; }

        /// <summary>
        /// XPath query for attributes to ignore
        /// </summary>
        public string XPathToIgnore { get; set; }
        
        /// <summary>
        /// XPath Query to select the container only attributes
        /// </summary>
        public string XPathOfContainerOnlyAttributes { get; set; }
    }

    /// <summary>
    /// Class that contains the settings needed 
    /// </summary>
    public class DataCaptureSettings
    {

        /// <summary>
        /// Database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Database Server name
        /// </summary>
        public string DatabaseServer { get; set; }

        /// <summary>
        /// Include items that have not expected voa
        /// </summary>
        public bool IncludeFilesIfNoExpectedVOA { get; set; }

        /// <summary>
        /// Set name for the Expected attributes
        /// </summary>
        public string ExpectedAttributeSetName { get; set; }

        /// <summary>
        /// Set name for the Found attributes
        /// </summary>
        public string FoundAttributeSetName { get; set; }

        /// <summary>
        /// The start date used to select Found set
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date used to select the Found Set
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// If True indicates that the StartDate and EndDate are applied to Found set
        /// If False indicates that the StartDate and EndDate are applied to Expected set
        /// </summary>
        public bool ApplyDatesToFound { get; set; }
        
        /// <summary>
        /// Per File settings property
        /// </summary>
        public PerFileSettings FileSettings
        {
            get
            {
                _FileSettings = _FileSettings ?? new PerFileSettings();
                return _FileSettings;
            }
            set
            {
                _FileSettings = value;
            }
        }

        // Used by the FileSettings property
        PerFileSettings _FileSettings = new PerFileSettings();

    }

    #endregion

    /// <summary>
    /// Class that processes the data capture stats from the database
    /// </summary>
    public class ProcessStatistics : IDisposable
    {
        #region Action Delegates

        /// <summary>
        /// Delegate for the action that will be ran for each file
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Func<IUnknownVector, IUnknownVector, string, string, IEnumerable<AccuracyDetail>> PerFileAction { get; set; }

        #endregion

        #region Constants

        static readonly string AttributeSetNames =
            "   SELECT Description FROM dbo.[AttributeSetName] WHERE Description in ('<Expected>', '<Found>');";

        // Query used to get the expected and found voa's
        // The variable <ApplyDateRangeSet> needs to be replaced with "Found" or "Expected"
        static readonly string ExpectedFoundSQL =
            "   SELECT Expected.VOA AS ExpectedVOA, " +
            "       Found.VOA AS FoundVOA, " +
            "       Found.FileID, " +
            "       FAMFile.FileName, " +
            "       <ApplyDateRangeSet>.DateTimeStamp " +
            "FROM " +
            "( " +
            "    SELECT dbo.AttributeSetForFile.VOA, " +
            "           FTS.FileID, " +
            "           FTS.DateTimeStamp, " +
            "           dbo.AttributeSetName.ID as FoundSetID " +
            "    FROM dbo.AttributeSetForFile " +
            "         INNER JOIN dbo.AttributeSetName ON dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID " +
            "         INNER JOIN dbo.FileTaskSession FTS ON dbo.AttributeSetForFile.FileTaskSessionID = FTS.ID " +
            "    WHERE Description = '<Found>' " +
            "          AND DateTimeStamp = " +
            "           ( " +
            "               SELECT MAX(DateTimeStamp) " +
            "               FROM dbo.FileTaskSession " +
            "                   INNER JOIN dbo.AttributeSetForFile ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID " +
            "               WHERE FileID = FTS.FileID " +
            "                   AND dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID " +
            "           ) " +
            ") AS Found " +
            "INNER JOIN dbo.FAMFile ON Found.FileID = dbo.FAMFile.ID " +
            "<IncludeAll> " +
            "( " +
            "    SELECT dbo.AttributeSetForFile.VOA, " +
            "           FTS.FileID, " +
            "          FTS.DateTimeStamp, " +
            "           dbo.AttributeSetName.ID as ExpectedSetID " +
            "    FROM dbo.AttributeSetForFile " +
            "         INNER JOIN dbo.AttributeSetName ON dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID " +
            "         INNER JOIN dbo.FileTaskSession FTS ON dbo.AttributeSetForFile.FileTaskSessionID = FTS.ID " +
            "    WHERE Description = '<Expected>' " +
            "          AND DateTimeStamp = " +
            "           ( " +
            "               SELECT MAX(DateTimeStamp) " +
            "               FROM dbo.FileTaskSession " +
            "                   INNER JOIN dbo.AttributeSetForFile ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID " +
            "               WHERE FileID = FTS.FileID " +
            "                   AND dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID " +
            "           ) " +
            ") AS Expected ON dbo.FAMFile.ID = Expected.FileID " +
            "WHERE <ApplyDateRangeSet>.DateTimeStamp >= '<StartDateTime>' AND <ApplyDateRangeSet>.DateTimeStamp <= '<EndDateTime>';";

        
        //  These are used to replace the IncludeAll tag in the ExpectedFoundSQL string
        static readonly string IncludeTrue = "LEFT JOIN";
        static readonly string IncludeFalse = "INNER JOIN";

        #endregion

        #region Fields

        /// <summary>
        /// Connection to the database
        /// </summary>
        SqlConnection _Connection;

        #endregion

        #region Properties

        /// <summary>
        /// Settings for processing the data
        /// </summary>
        public DataCaptureSettings Settings { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the ProcessStatistics class
        /// </summary>
        /// <param name="settings">Settings for processing the data</param>
        public ProcessStatistics(DataCaptureSettings settings)
        {
            Settings = settings;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes the data by retrieving the voa data from the database and creating the results for each file
        /// </summary>
        /// <param name="fileCount">Variable for number of files processed</param>
        public IEnumerable<AccuracyDetail> ProcessData(out long fileCount)
        {
            try
            {
                // Set the fileCount to 0
                fileCount = 0;

                // Assert that settings have been set
                ExtractException.Assert("ELI41527", "Settings have not been configured for Processing", Settings != null);

                // Assert that the delegates have been set
                ExtractException.Assert("ELI41522", "Per file action has not be set", PerFileAction != null);

                // Used to hold the results from the comparisons
                ConcurrentBag<AccuracyDetail> Results = new ConcurrentBag<AccuracyDetail>();

                // Track the number of pending threads
                int numberPending = 0;

                // Get VOA data for each file
                using (SqlDataReader ExpectedAndFoundReader = GetExpectedAndFoundData())
                {
                    // Get the ordinal for the FoundVOA and ExpectedVOA columns
                    int FoundVOAColumn = ExpectedAndFoundReader.GetOrdinal("FoundVOA");
                    int ExpectedVOAColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedVOA");

                    // Create a semaphore to limit the number of threads that get queued (memory may be a problem)
                    Semaphore limitThreads = new Semaphore(100, 100);

                    // Process the found records
                    while (ExpectedAndFoundReader.Read())
                    {
                        // Increment the file count
                        fileCount++;

                        // Get the streams for the expected and found voa data (the thread will read the voa from the stream
                        Stream expectedStream = ExpectedAndFoundReader.GetStream(ExpectedVOAColumn);
                        Stream foundStream = ExpectedAndFoundReader.GetStream(FoundVOAColumn);

                        // Increment the number of pending threads
                        Interlocked.Increment(ref numberPending);

                        // Get Semaphore before creating the thread
                        limitThreads.WaitOne();

                        // Create a thread pool thread to do the comparison
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            try
                            {
                                // Put the expected and found streams in usings so they will be disposed
                                using (expectedStream)
                                using (foundStream)
                                {
                                    // Get the VOAs from the streams
                                    IUnknownVector ExpectedAttributes = GetVOAFromSQLBinary(expectedStream);
                                    IUnknownVector FoundAttributes = GetVOAFromSQLBinary(foundStream);

                                    // Compare the VOAs
                                    var output = PerFileAction(ExpectedAttributes,
                                              FoundAttributes,
                                              Settings.FileSettings.XPathToIgnore,
                                              Settings.FileSettings.XPathOfContainerOnlyAttributes);

                                    // Add the comparison results to the Results
                                    foreach (AccuracyDetail a in output)
                                    {
                                        Results.Add(a);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.AsExtract("ELI41544").Log();
                            }
                            finally
                            {
                                // Decrement the number of pending threads
                                Interlocked.Decrement(ref numberPending);

                                // Release semaphore after thread has been created
                                limitThreads.Release();
                            }
                        });
                    }
                }

                // Wait for all the pending threads to complete
                while (numberPending != 0)
                {
                    Thread.Sleep(1000);
                }
                return Results;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41496");
            }
            finally
            {
                _Connection.Close();
            }

        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Loads the IUnknownVector of attributes from the stream
        /// </summary>
        /// <param name="binaryAsStream">Stream containing the vector of attributes</param>
        /// <returns>IUnknownVector of attributes that was in the stream</returns>
        static IUnknownVector GetVOAFromSQLBinary(Stream binaryAsStream)
        {
            try
            {
                // Check if the stream has any data
                if (binaryAsStream.Length == 0)
                {
                    return new IUnknownVector();
                }
                // Unzip the data in the stream
                var zipStream = new ManagedInflater(binaryAsStream);
                using (var unZippedStream = zipStream.InflateToStream())
                {
                    // Advance the stream past the GUID
                    unZippedStream.Seek(16, SeekOrigin.Begin);

                    // Creating the IUnknownVector with the prog id because if the UCLID_COMUTILSLib reference property for Embed interop types
                    // is set to false the IUnknownVector created with new is unable to get the IPersistStream interface
                    Type type = Type.GetTypeFromProgID("UCLIDCOMUtils.IUnknownVector");
                    IUnknownVector voa = (IUnknownVector)Activator.CreateInstance(type);
                    IPersistStream persistVOA = voa as IPersistStream;
                    ExtractException.Assert("ELI41533", "Unable to Obtain IPersistStream Interface.", persistVOA != null);

                    // Wrap the unzipped stream for loading the VOA
                    IStreamWrapper binaryIPersistStream = new IStreamWrapper(unZippedStream);
                    if (persistVOA != null)
                    {
                        persistVOA.Load(binaryIPersistStream);
                    }
                    return voa;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41546");
            }
        } 

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the Expected and Found data from the database
        /// </summary>
        /// <returns></returns>
        SqlDataReader GetExpectedAndFoundData()
        {
            try
            {
                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
                sqlConnectionBuild.DataSource = Settings.DatabaseServer;
                sqlConnectionBuild.InitialCatalog = Settings.DatabaseName;
                sqlConnectionBuild.IntegratedSecurity = true;
                sqlConnectionBuild.NetworkLibrary = "dbmssocn";

                _Connection = new SqlConnection(sqlConnectionBuild.ConnectionString);

                // Open the connection
                _Connection.Open();

                // Verify that the Found and expected attributes sets exist
                ValidateSetNames(_Connection.CreateCommand());

                SqlCommand cmd = _Connection.CreateCommand();

                // Set the timeout so that it waits indefinitely
                cmd.CommandTimeout = 0;

                // Set up the sql to obtain the expected and found
                string sql = ExpectedFoundSQL;
                sql = sql.Replace("<Expected>", Settings.ExpectedAttributeSetName);
                sql = sql.Replace("<Found>", Settings.FoundAttributeSetName);
                sql = sql.Replace("<IncludeAll>", Settings.IncludeFilesIfNoExpectedVOA ? IncludeTrue : IncludeFalse);
                sql = sql.Replace("<StartDateTime>", Settings.StartDate.ToString("yyyy/MM/dd HH:mm:ss:fff", CultureInfo.CurrentCulture));
                sql = sql.Replace("<EndDateTime>", Settings.EndDate.ToString("yyyy/MM/dd HH:mm:ss:fff", CultureInfo.CurrentCulture));
                sql = sql.Replace("<ApplyDateRangeSet>", (Settings.ApplyDatesToFound) ? "Found" : "Expected");
                cmd.CommandText = sql;

                // Return the reader
                return cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41545");
            }
        }

        /// <summary>
        /// Validates the found and expected set names stored in Settings
        /// </summary>
        /// <param name="attributeSetsCommand"> An Sql command object created on an open connection.</param>
        void ValidateSetNames(SqlCommand attributeSetsCommand)
        {
            using (attributeSetsCommand)
            {
                attributeSetsCommand.CommandText = AttributeSetNames.Replace("<Expected>", Settings.ExpectedAttributeSetName);
                attributeSetsCommand.CommandText = attributeSetsCommand.CommandText.Replace("<Found>", Settings.FoundAttributeSetName);

                using (var setReader = attributeSetsCommand.ExecuteReader())
                {
                    bool ExpectedSetExists = false;
                    bool FoundSetExists = false;
                    foreach (DbDataRecord r in setReader)
                    {
                        string value = r.GetString(0);
                        if (value.Equals(Settings.ExpectedAttributeSetName, StringComparison.OrdinalIgnoreCase))
                        {
                            ExpectedSetExists = true;
                        }
                        if (value.Equals(Settings.FoundAttributeSetName, StringComparison.OrdinalIgnoreCase))
                        {
                            FoundSetExists = true;
                        }
                    }

                    if (!ExpectedSetExists || !FoundSetExists)
                    {
                        ExtractException ee = new ExtractException("ELI41587", "File attribute set does not exist.");
                        if (!ExpectedSetExists)
                        {
                            ee.AddDebugData("Expected Set", Settings.ExpectedAttributeSetName, false);
                        }
                        if (!FoundSetExists)
                        {
                            ee.AddDebugData("Found Set", Settings.FoundAttributeSetName, false);
                        }
                        throw ee;
                    }
                }
            }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_Connection != null)
                    {
                        _Connection.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProcessStatistics() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
