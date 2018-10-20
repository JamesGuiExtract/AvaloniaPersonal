using Extract;
using Extract.AttributeFinder;
using Extract.DataCaptureStats;
using Extract.Interop;
using Extract.Interop.Zip;
using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UCLID_COMUTILSLib;
using GroupByCriterion = Extract.Utilities.Union<
    StatisticsReporter.GroupByDBField,
    StatisticsReporter.GroupByFoundXPath,
    StatisticsReporter.GroupByExpectedXPath>;

namespace StatisticsReporter
{
    #region Settings Classes

    /// <summary>
    /// Built-in, per-file group-by fields that are available
    /// </summary>
    public enum GroupByDBField
    {
        None = 0,
        FoundUserName = 1,
        ExpectedUserName = 2,
        FileName = 3,
        ExpectedDateTime = 4,
        ExpectedDate = 5,
        ExpectedYear = 6,
        ExpectedMonth = 7,
        ExpectedDay = 8,
        ExpectedHour = 9,
        FoundDateTime = 10,
        FoundDate = 11,
        FoundYear = 12,
        FoundMonth = 13,
        FoundDay = 14,
        FoundHour = 15,
    }

    /// <summary>
    /// Group by fields that are resolved by querying the expected or found VOA file
    /// </summary>
    public class GroupByXPath
    {
        /// <summary>
        /// Gets or sets the Xpath value.
        /// </summary>
        public string XPath { get; set; }

        /// <summary>
        /// Gets or sets the friendly name to be displayed in reports.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Label) ? XPath : Label;
        }
    }

    /// <summary>
    /// Group by fields that are resolved by querying the expected VOA file
    /// </summary>
    public class GroupByExpectedXPath : GroupByXPath { }

    /// <summary>
    /// Group by fields that are resolved by querying the found VOA file
    /// </summary>
    public class GroupByFoundXPath : GroupByXPath { }

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

        /// <summary>
        /// Gets the list of tags used to select files
        /// </summary>
        public IEnumerable<string> Tagged { get; set; }

        /// <summary>
        /// Gets or sets the list of group by criteria.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<GroupByCriterion> GroupByCriteria { get; set; }

        // Used by the FileSettings property
        PerFileSettings _FileSettings = new PerFileSettings();

    }

    /// <summary>
    /// Helper class to allow string-array comparison
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEqualityComparer{System.String[]}" />
    public class GroupByComparer : IComparer<string[]>, IEqualityComparer<string[]>
    {
        public int Compare(string[] x, string[] y)
        {
            try
            {
                for (int i = 0; i < x.Length; i++)
                {
                    if (i >= y.Length)
                    {
                        return -1;
                    }
                    var compared = string.Compare(x[i], y[i],
                        StringComparison.OrdinalIgnoreCase);
                    if (compared != 0)
                    {
                        return compared;
                    }
                }
                if (y.Length > x.Length)
                {
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41899");
            }
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(string[] x, string[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; ++i)
            {
                if (!string.Equals(x[i], y[i],
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(string[] obj)
        {
            return obj.Aggregate(HashCode.Start,
                (h, s) => h.Hash(s.ToUpperInvariant()));
        }
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
        public Func<IUnknownVector, IUnknownVector, string, string, CancellationToken,IEnumerable<AccuracyDetail>> PerFileAction { get; set; }

        #endregion

        #region Constants

        static readonly string AttributeSetNames =
            "   SELECT Description FROM dbo.[AttributeSetName] WHERE Description in ('<Expected>', '<Found>');";

        // Query used to get the expected and found voa's
        // The variable <ApplyDateRangeSet> needs to be replaced with "Found" or "Expected"
        static readonly string ExpectedFoundSQL = @"
            SELECT Expected.VOA AS ExpectedVOA,
                   Expected.DateTimeStamp AS ExpectedDateTimeStamp,
                   Expected.UserName AS ExpectedUserName,
                   Found.FileID,
                   FAMFile.FileName,
                   Found.VOA AS FoundVOA,
                   Found.DateTimeStamp AS FoundDateTimeStamp,
                   Found.UserName AS FoundUserName
            FROM
            (
                SELECT dbo.AttributeSetForFile.VOA,
                    FTS.FileID,
                    FTS.DateTimeStamp,
                    dbo.AttributeSetName.ID as FoundSetID,
                    dbo.FAMUser.UserName
                FROM dbo.AttributeSetForFile
                    INNER JOIN dbo.AttributeSetName ON dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID
                    INNER JOIN dbo.FileTaskSession FTS ON dbo.AttributeSetForFile.FileTaskSessionID = FTS.ID
                    INNER JOIN dbo.FAMSession ON FTS.FAMSessionID = dbo.FAMSession.ID
                    INNER JOIN dbo.FAMUser ON dbo.FAMSession.FAMUserID = dbo.FAMUser.ID
                WHERE Description = '<Found>'
                    AND DateTimeStamp =
                    (
                        SELECT MAX(DateTimeStamp)
                        FROM dbo.FileTaskSession
                            INNER JOIN dbo.AttributeSetForFile ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                        WHERE FileID = FTS.FileID
                            AND dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID
                    )
            ) AS Found
            INNER JOIN dbo.FAMFile ON Found.FileID = dbo.FAMFile.ID
            <IncludeAll>
            (
                SELECT dbo.AttributeSetForFile.VOA,
                    FTS.FileID,
                    FTS.DateTimeStamp,
                    dbo.AttributeSetName.ID as ExpectedSetID,
                    dbo.FAMUser.UserName
                FROM dbo.AttributeSetForFile
                    INNER JOIN dbo.AttributeSetName ON dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID
                    INNER JOIN dbo.FileTaskSession FTS ON dbo.AttributeSetForFile.FileTaskSessionID = FTS.ID
                    INNER JOIN dbo.FAMSession ON FTS.FAMSessionID = dbo.FAMSession.ID
                    INNER JOIN dbo.FAMUser ON dbo.FAMSession.FAMUserID = dbo.FAMUser.ID
                WHERE Description = '<Expected>'
                    AND DateTimeStamp =
                    (
                        SELECT MAX(DateTimeStamp)
                        FROM dbo.FileTaskSession
                            INNER JOIN dbo.AttributeSetForFile ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                        WHERE FileID = FTS.FileID
                            AND dbo.AttributeSetForFile.AttributeSetNameID = dbo.AttributeSetName.ID
                    )
            ) AS Expected ON dbo.FAMFile.ID = Expected.FileID
            WHERE <ApplyDateRangeSet>.DateTimeStamp >= '<StartDateTime>'
            AND <ApplyDateRangeSet>.DateTimeStamp <= '<EndDateTime>' <TagCondition>;";

        
        //  These are used to replace the IncludeAll tag in the ExpectedFoundSQL string
        static readonly string IncludeTrue = "LEFT JOIN";
        static readonly string IncludeFalse = "INNER JOIN";

        static readonly string TagCondition = @"
            AND Found.FileID IN
                (SELECT FileID FROM FileTag JOIN Tag ON TagID = Tag.ID WHERE TagName IN ('{0}'))";

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
        /// <returns>An <see cref="IEnumerable{GroupStatistics}"/></returns>
        public List<GroupStatistics> ProcessData()
        {
            try
            {
                // Assert that settings have been set
                ExtractException.Assert("ELI41527", "Settings have not been configured for Processing", Settings != null);

                // Assert that the delegates have been set
                ExtractException.Assert("ELI41522", "Per file action has not be set", PerFileAction != null);

                // Used to hold the results from the comparisons
                var Results = new ConcurrentBag<Tuple<string[], IEnumerable<AccuracyDetail>>>();

                // Track the number of pending threads
                int numberPending = 0;

                // Get VOA data for each file
                using (SqlDataReader ExpectedAndFoundReader = GetExpectedAndFoundData())
                {
                    // Get the ordinal for the FoundVOA and ExpectedVOA columns
                    int FoundVOAColumn = ExpectedAndFoundReader.GetOrdinal("FoundVOA");
                    int ExpectedVOAColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedVOA");

                    // Associate GroupByDbField enum values with datareader functions
                    Dictionary<GroupByDBField, Func<string>> dBFields =
                        GetDBFieldReaders(ExpectedAndFoundReader);

                    // Collect the fields that are actually used
                    var referencedDBFields = Settings.GroupByCriteria
                        .Select(c => c.Match(dbField => (GroupByDBField?) dbField, _ => null, _ => null))
                        .Where(c => c != null)
                        .Select(c => c.Value)
                        .ToList();

                    // Create a semaphore to limit the number of threads that get queued (memory may be a problem)
                    Semaphore limitThreads = new Semaphore(100, 100);

                    // Process the found records
                    while (ExpectedAndFoundReader.Read())
                    {
                        // Get the streams for the expected and found voa data (the thread will read the voa from the stream
                        Stream expectedStream = ExpectedAndFoundReader.GetStream(ExpectedVOAColumn);
                        Stream foundStream = ExpectedAndFoundReader.GetStream(FoundVOAColumn);

                        // Get values for the group by fields
                        var dBFieldValues = new Dictionary<GroupByDBField, string>();
                        foreach (var key in referencedDBFields)
                        {
                            dBFieldValues[key] = dBFields[key]();
                        }

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
                                    IUnknownVector ExpectedAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(expectedStream);
                                    IUnknownVector FoundAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(foundStream);

                                    var foundXPathContext = new Lazy<XPathContext>(
                                        () => new XPathContext(FoundAttributes));
                                    var expectedXPathContext = new Lazy<XPathContext>(
                                        () => new XPathContext(ExpectedAttributes));

                                    // Create the group-by values
                                    string[] groupBy = Settings.GroupByCriteria
                                        .Select(criterion =>
                                            criterion.Match(
                                                dbField => dBFieldValues[dbField],
                                                found => string.Join("|",
                                                    foundXPathContext.Value.FindAllAsStrings(found.XPath).Distinct()),
                                                expected => string.Join("|",
                                                    expectedXPathContext.Value.FindAllAsStrings(expected.XPath).Distinct())))
                                        .ToArray();

                                    // Compare the VOAs
                                    var output = PerFileAction(ExpectedAttributes,
                                              FoundAttributes,
                                              Settings.FileSettings.XPathToIgnore,
                                              Settings.FileSettings.XPathOfContainerOnlyAttributes,
                                              default(CancellationToken))
                                              .ToList();

                                    // Add the comparison results to the Results
                                    Results.Add(Tuple.Create<string[], IEnumerable<AccuracyDetail>>(groupBy, output));
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

                // Group the results by the group-by value array and create GroupStatistics objects
                string[] groupByNames = Settings.GroupByCriteria
                    .Select(c => c.Match(e => e.ToReadableValue(),
                        xpath => xpath.ToString(),
                        xpath => xpath.ToString()))
                    .ToArray();
                return Results
                    .GroupBy(t => t.Item1, new GroupByComparer())
                    .Select(group =>
                        {
                            var perFile = group.Select(t => t.Item2).ToList();
                            return new GroupStatistics(perFile.Count, groupByNames, group.Key, perFile.SelectMany(items => items));
                        })
                    .OrderBy(g => g.GroupByValues, new GroupByComparer())
                    .ToList();
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
        /// Creates an association between <see cref="GroupByDBField"/> values and functions to
        /// extract the appropriate string value from the data reader
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A dictionary that maps enum values to string functions</returns>
        static Dictionary<GroupByDBField, Func<string>> GetDBFieldReaders(SqlDataReader reader)
        {
            var dBFields = new Dictionary<GroupByDBField, Func<string>>();
            int fileName = reader.GetOrdinal("FileName");
            int expectedUserName = reader.GetOrdinal("ExpectedUserName");
            int foundUserName = reader.GetOrdinal("FoundUserName");
            int expectedDate = reader.GetOrdinal("ExpectedDateTimeStamp");
            int foundDate = reader.GetOrdinal("FoundDateTimeStamp");


            dBFields[GroupByDBField.FileName] = () => reader.GetString(fileName);
            dBFields[GroupByDBField.ExpectedUserName] = () =>reader.GetString(expectedUserName);
            dBFields[GroupByDBField.FoundUserName] = () =>reader.GetString(foundUserName);

            dBFields[GroupByDBField.ExpectedDateTime] = () => reader.GetDateTime(expectedDate)
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.ExpectedDate] = () => reader.GetDateTime(expectedDate)
                    .ToString("d", CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.ExpectedYear] = () => reader.GetDateTime(expectedDate)
                    .Year
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.ExpectedMonth] = () => reader.GetDateTime(expectedDate)
                    .Month
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.ExpectedDay] = () => reader.GetDateTime(expectedDate)
                    .Day
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.ExpectedHour] = () => reader.GetDateTime(expectedDate)
                    .Hour
                    .ToString(CultureInfo.CurrentCulture);

            dBFields[GroupByDBField.FoundDateTime] = () => reader.GetDateTime(foundDate)
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.FoundDate] = () => reader.GetDateTime(foundDate)
                    .ToString("YYYY/MM/DD", CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.FoundYear] = () => reader.GetDateTime(foundDate)
                    .Year
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.FoundMonth] = () => reader.GetDateTime(foundDate)
                    .Month
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.FoundDay] = () => reader.GetDateTime(foundDate)
                    .Day
                    .ToString(CultureInfo.CurrentCulture);
            dBFields[GroupByDBField.FoundHour] = () => reader.GetDateTime(foundDate)
                    .Hour
                    .ToString(CultureInfo.CurrentCulture);
            return dBFields;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the Expected and Found data from the database
        /// </summary>
        /// <returns></returns>
        SqlDataReader GetExpectedAndFoundData()
        {
            string sql = ExpectedFoundSQL;

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
                sql = sql.Replace("<Expected>", Settings.ExpectedAttributeSetName);
                sql = sql.Replace("<Found>", Settings.FoundAttributeSetName);
                sql = sql.Replace("<IncludeAll>", Settings.IncludeFilesIfNoExpectedVOA ? IncludeTrue : IncludeFalse);
                sql = sql.Replace("<StartDateTime>", Settings.StartDate.ToString("yyyy/MM/dd HH:mm:ss:fff", CultureInfo.CurrentCulture));
                sql = sql.Replace("<EndDateTime>", Settings.EndDate.ToString("yyyy/MM/dd HH:mm:ss:fff", CultureInfo.CurrentCulture));
                sql = sql.Replace("<ApplyDateRangeSet>", (Settings.ApplyDatesToFound) ? "Found" : "Expected");
                if (Settings.Tagged.Any())
                {
                    sql = sql.Replace("<TagCondition>", string.Format(CultureInfo.CurrentCulture,
                        TagCondition, string.Join("','", Settings.Tagged)));
                }
                else
                {
                    sql = sql.Replace("<TagCondition>", "");
                }
                cmd.CommandText = sql;

                // Return the reader
                return cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI41545");
                ee.AddDebugData("Query", sql, true);
                throw ee;
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
