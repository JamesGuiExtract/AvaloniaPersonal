using Extract.AttributeFinder;
using Extract.Code.Attributes;
using Extract.DataCaptureStats;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.ETL
{
    /// <summary>
    /// Class to implement the Database service for Data Capture accuracy stats
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "Data capture accuracy")]
    [SuppressMessage("Microsoft.Naming", "CA1709: CorrectCasingInTypeName")]
    public class DataCaptureAccuracy : DatabaseService, IConfigSettings
    {
        #region Constants

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Query used to remove data from ReportingDataCaptureAccuracy table that will be replaced
        /// This query requires values for the following parameters
        ///     @FoundSetName - Name of the Attribute set for found values
        ///     @ExpectedSetName - Name of the Attribute set for Expected values
        ///     @DatabaseServiceID - Id of the record in the DatabaseService table for this service instance
        /// </summary>
        static readonly string REMOVE_OLD_ACCURACY_DATA =
            @"
                WITH MostRecent AS (
                SELECT AttributeSetName.Description
	                ,MAX(AttributeSetForFile.FileTaskSessionID) AS MostRecentFileTaskSession
	                ,AttributeSetForFile.AttributeSetNameID
	                ,FileTaskSession.FileID
	
                FROM AttributeSetForFile
                INNER JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
                INNER JOIN FileTaskSession ON AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID
                GROUP BY AttributeSetName.Description
	                ,AttributeSetForFile.AttributeSetNameID
	                ,FileTaskSession.FileID
                HAVING AttributeSetName.Description in (@FoundSetName,@ExpectedSetName)
                )

                DELETE FROM ReportingDataCaptureAccuracy
                WHERE ID IN(
                SELECT ReportingDataCaptureAccuracy.ID AS AccuracyDataID
                FROM MostRecent found 
	                INNER JOIN MostRecent expected 
		                ON found.Description = @FoundSetName AND Expected.Description = @ExpectedSetName 
			                AND found.FileID = expected.FileID
	                INNER JOIN AttributeSetForFile FoundAttributeSet 
		                ON FoundAttributeSet.FileTaskSessionID = found.MostRecentFileTaskSession 
			                AND FoundAttributeSet.AttributeSetNameID = found.AttributeSetNameID
	                INNER JOIN AttributeSetForFile ExpectedAttributeSet 
		                ON ExpectedAttributeSet.FileTaskSessionID = expected.MostRecentFileTaskSession 
			                AND ExpectedAttributeSet.AttributeSetNameID = expected.AttributeSetNameID
	                INNER JOIN ReportingDataCaptureAccuracy 
		                ON ReportingDataCaptureAccuracy.FileID = found.FileID 
                            AND ReportingDataCaptureAccuracy.DatabaseServiceID = @DatabaseServiceID 
                            AND (
                                ReportingDataCaptureAccuracy.FoundAttributeSetForFileID != FoundAttributeSet.ID
			                    OR ReportingDataCaptureAccuracy.ExpectedAttributeSetForFileID != ExpectedAttributeSet.ID
			                )
                )
            ";

        /// <summary>
        /// Query used to get the data used to create the records in ReportingDataCaptureAccuracy table
        /// This query requires values for the following parameters
        ///     @FoundSetName - Name of the Attribute set for found values
        ///     @ExpectedSetName - Name of the Attribute set for Expected values
        ///     @DatabaseServiceID - Id of the record in the DatabaseService table for this service instance
        /// </summary>
        static readonly string UPDATE_ACCURACY_DATA_SQL =
            @"
                WITH MostRecent AS (
                SELECT AttributeSetName.Description
	                ,MAX(AttributeSetForFile.FileTaskSessionID) AS MostRecentFileTaskSession
	                ,AttributeSetForFile.AttributeSetNameID
	                ,FileTaskSession.FileID
	
                FROM AttributeSetForFile
                INNER JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
                INNER JOIN FileTaskSession ON AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID
                GROUP BY AttributeSetName.Description
	                ,AttributeSetForFile.AttributeSetNameID
	                ,FileTaskSession.FileID
                HAVING AttributeSetName.Description in (@FoundSetName,@ExpectedSetName)
                )

                SELECT FoundAttributeSet.ID AS FoundAttributeSetFileID
	                  ,FoundAttributeSet.VOA FoundVOA
	                  ,ExpectedAttributeSet.ID AS ExpectedAttributeSetFileID
	                  ,ExpectedAttributeSet.VOA AS ExpectedVOA
	                  ,found.FileID
                FROM MostRecent found 
	                INNER JOIN MostRecent expected 
		                ON found.Description = @FoundSetName AND Expected.Description = @ExpectedSetName 
			                AND found.FileID = expected.FileID
	                INNER JOIN AttributeSetForFile FoundAttributeSet 
		                ON FoundAttributeSet.FileTaskSessionID = found.MostRecentFileTaskSession 
			                AND FoundAttributeSet.AttributeSetNameID = found.AttributeSetNameID
	                INNER JOIN AttributeSetForFile ExpectedAttributeSet 
		                ON ExpectedAttributeSet.FileTaskSessionID = expected.MostRecentFileTaskSession 
			                AND ExpectedAttributeSet.AttributeSetNameID = expected.AttributeSetNameID
	                LEFT JOIN ReportingDataCaptureAccuracy 
		                ON ReportingDataCaptureAccuracy.FoundAttributeSetForFileID = FoundAttributeSet.ID
			                AND ReportingDataCaptureAccuracy.ExpectedAttributeSetForFileID = ExpectedAttributeSet.ID
			                AND ReportingDataCaptureAccuracy.DatabaseServiceID = @DatabaseServiceID
                WHERE ReportingDataCaptureAccuracy.ID IS NULL
            ";

        #endregion

        #region Fields

        /// <summary>
        /// Indicates whether the Process method is currently executing.
        /// </summary>
        bool _processing;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default constructor for DataCaptureAccuracy
        /// </summary>
        public DataCaptureAccuracy()
        {
        }

        #endregion Constructors

        #region DatabaseService implementation

        #region DatabaseService Properties

        /// <summary>
        /// Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processing; otherwise, <c>false</c>.
        /// </value>
        public override bool Processing
        {
            get
            {
                return _processing;
            }
        }

        #endregion DatabaseService Properties

        #region DatabaseService Methods

        /// <summary>
        /// Performs the processing needed for the records in ReportingDataCaptureAccuracy table
        /// </summary>
        /// <param name="cancelToken">Cancel token to indicate that processing should stop</param>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                using (var connection = NewSqlDBConnection())
                {
                    // Open the connection
                    connection.Open();

                    deleteOldRecords(connection);

                    // Records to calculate stats
                    SqlCommand cmd = connection.CreateCommand();

                    // Set the timeout so that it waits indefinitely
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = UPDATE_ACCURACY_DATA_SQL;

                    addParametersToCommand(cmd);

                    // Get VOA data for each file
                    using (SqlDataReader ExpectedAndFoundReader = cmd.ExecuteReader())
                    {
                        // Get the ordinal for the FoundVOA and ExpectedVOA columns
                        int foundVOAColumn = ExpectedAndFoundReader.GetOrdinal("FoundVOA");
                        int expectedVOAColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedVOA");
                        int foundAttributeForFileSetColumn = ExpectedAndFoundReader.GetOrdinal("FoundAttributeSetFileID");
                        int expectedAttributeForFileSetColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedAttributeSetFileID");
                        int fileIDColumn = ExpectedAndFoundReader.GetOrdinal("FileID");

                        // Process the found records
                        while (ExpectedAndFoundReader.Read() && !cancelToken.IsCancellationRequested)
                        {
                            // Get the streams for the expected and found voa data (the thread will read the voa from the stream
                            Stream expectedStream = ExpectedAndFoundReader.GetStream(expectedVOAColumn);
                            Stream foundStream = ExpectedAndFoundReader.GetStream(foundVOAColumn);
                            Int64 foundID = ExpectedAndFoundReader.GetInt64(foundAttributeForFileSetColumn);
                            Int64 expectedID = ExpectedAndFoundReader.GetInt64(expectedAttributeForFileSetColumn);
                            Int32 fileID = ExpectedAndFoundReader.GetInt32(fileIDColumn);

                            try
                            {
                                // Put the expected and found streams in usings so they will be disposed
                                using (expectedStream)
                                {
                                    using (foundStream)
                                    {
                                        // Get the VOAs from the streams
                                        IUnknownVector ExpectedAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(expectedStream);
                                        IUnknownVector FoundAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(foundStream);

                                        // Compare the VOAs
                                        var output = AttributeTreeComparer.CompareAttributes(ExpectedAttributes,
                                            FoundAttributes, XPathOfAttributesToIgnore, XPathOfContainerOnlyAttributes)
                                            .ToList();

                                        // Add the comparison results to the Results
                                        var statsToSave = output.AggregateStatistics();

                                        var lookup = statsToSave.ToLookup(a => new { a.Path, a.Label });

                                        var attributePaths = statsToSave
                                            .Select(a => a.Path)
                                            .Distinct()
                                            .OrderBy(p => p)
                                            .ToList();

                                        List<string> valuesToAdd = new List<string>();
                                        foreach (var p in attributePaths)
                                        {
                                            int correct = lookup[new { Path = p, Label = AccuracyDetailLabel.Correct }].Sum(a => a.Value);
                                            int incorrect = lookup[new { Path = p, Label = AccuracyDetailLabel.Incorrect }].Sum(a => a.Value);
                                            int expected = lookup[new { Path = p, Label = AccuracyDetailLabel.Expected }].Sum(a => a.Value);
                                            if (correct != 0 || incorrect != 0 || expected != 0)
                                            {
                                                valuesToAdd.Add(string.Format(CultureInfo.InvariantCulture,
                                                    @"({0}, {1}, {2}, {3}, '{4}', {5}, {6}, {7})"
                                                    , DatabaseServiceID
                                                    , foundID
                                                    , expectedID
                                                    , fileID
                                                    , p
                                                    , correct
                                                    , expected
                                                    , incorrect
                                                    ));
                                            }
                                        }

                                        // Add the data to the ReportingDataCaptureAccuracy table
                                        var saveCmd = connection.CreateCommand();

                                        saveCmd.CommandText = string.Format(CultureInfo.InvariantCulture,
                                            @"INSERT INTO [dbo].[ReportingDataCaptureAccuracy]
                                                           ([DatabaseServiceID]
                                                           ,[FoundAttributeSetForFileID]
                                                           ,[ExpectedAttributeSetForFileID]
                                                           ,[FileID]
                                                           ,[Attribute]
                                                           ,[Correct]
                                                           ,[Expected]
                                                           ,[Incorrect]
                                                           )
                                                     VALUES
                                                           {0};", string.Join(",\r\n", valuesToAdd));
                                        saveCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.AsExtract("ELI41544").Log();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45366");
            }
            finally
            {
                _processing = false;
            }
        }

        #endregion

        #endregion

        #region IConfigSettings implementation

        /// <summary>
        /// Method returns the state of the configuration
        /// </summary>
        /// <returns>Returns <see langword="true"/> if configuration is valid, otherwise false</returns>
        public bool IsConfigured()
        {
            try
            {
                bool configured = true;
                configured = configured && !string.IsNullOrWhiteSpace(XPathOfAttributesToIgnore);
                configured = configured && !string.IsNullOrWhiteSpace(XPathOfContainerOnlyAttributes);
                configured = configured && !string.IsNullOrWhiteSpace(ExpectedAttributeSetName);
                configured = configured && !string.IsNullOrWhiteSpace(FoundAttributeSetName);
                configured = configured && (FoundAttributeSetName != ExpectedAttributeSetName);
                return configured;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45656");
            }
        }

        /// <summary>
        /// Displays a form to Configures the DataCaptureAccuracy service
        /// </summary>
        /// <returns><see langword="true"/> if configuration was ok'd. if configuration was canceled returns 
        /// <see langword="false"/></returns>
        public bool Configure()
        {
            try
            {
                DataCaptureAccuracyForm captureAccuracyForm = new DataCaptureAccuracyForm(this);
                return captureAccuracyForm.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45657");
            }
            return false;
        }

        #endregion

        #region DataCaptureAccuracy Properties

        /// <summary>
        /// XPath query of attributes the be ignored when comparing attributes
        /// </summary>
        [DataMember]
        public string XPathOfAttributesToIgnore { get; set; } = string.Empty;

        /// <summary>
        /// XPath of Attributes that are only considered containers when comparing attributes
        /// </summary>
        [DataMember]
        public string XPathOfContainerOnlyAttributes { get; set; } = string.Empty;

        /// <summary>
        /// The set name of the expected attributes when doing the comparison
        /// </summary>
        [DataMember]
        public string ExpectedAttributeSetName { get; set; } = "DataSavedByOperator";

        /// <summary>
        /// The set name of the found attributes when doing the comparison
        /// </summary>
        [DataMember]
        public string FoundAttributeSetName { get; set; } = "DataFoundByRules";

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        #endregion

        #region Private Methods

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI45382", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        /// <summary>
        /// Adds the following parameters using the configured properties
        ///     @FoundSetName - Set to FoundAttributeSetName
        ///     @ExpectedSetName - Set to ExpectedAttributeSetName
        ///     @DatabaseServiceID - Set to ID
        /// </summary>
        /// <param name="cmd">The SqlCommand that needs the parameters added</param>
        void addParametersToCommand(SqlCommand cmd)
        {
            cmd.Parameters.Add("@FoundSetName", SqlDbType.NVarChar);
            cmd.Parameters.Add("@ExpectedSetName", SqlDbType.NVarChar);
            cmd.Parameters.Add("@DatabaseServiceID", SqlDbType.Int);
            cmd.Parameters["@FoundSetName"].Value = FoundAttributeSetName;
            cmd.Parameters["@ExpectedSetName"].Value = ExpectedAttributeSetName;
            cmd.Parameters["@DatabaseServiceID"].Value = DatabaseServiceID;
        }

        /// <summary>
        /// Runs the query to remove the records that will be replaced
        /// </summary>
        /// <param name="connection">Database connection to perform the deletion</param>
        void deleteOldRecords(SqlConnection connection)
        {
            // Remove records to be replaced
            SqlCommand deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = REMOVE_OLD_ACCURACY_DATA;
            addParametersToCommand(deleteCommand);

            deleteCommand.Transaction = connection.BeginTransaction();
            try
            {

                deleteCommand.ExecuteNonQuery();
                deleteCommand.Transaction.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    deleteCommand.Transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    rollbackException.AsExtract("ELI45381").Log();
                }
                ExtractException ee = new ExtractException("ELI45380", "Unable to update ReportingDataCaptureAccuracy", ex);
                ee.AddDebugData("SaveQuery", deleteCommand.CommandText, false);
                throw ee;
            }
        }
        #endregion

    }
}
