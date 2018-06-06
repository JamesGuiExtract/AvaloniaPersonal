﻿using Extract.AttributeFinder;
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
using System.Transactions;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.ETL
{
    /// <summary>
    /// Class to implement the Database service for Redaction accuracy stats
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [SuppressMessage("Microsoft.Naming", "CA1709: CorrectCasingInTypeName")]
    [ExtractCategory("DatabaseService", "Redaction accuracy")]
    public class RedactionAccuracy : DatabaseService, IConfigSettings
    {
        #region Constants

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Query used to remove data from ReportingRedactionAccuracy table that will be replaced
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

                DELETE FROM ReportingRedactionAccuracy
                WHERE ID IN(
                SELECT ReportingRedactionAccuracy.ID AS AccuracyDataID
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
	                INNER JOIN ReportingRedactionAccuracy 
		                ON ReportingRedactionAccuracy.FileID = found.FileID 
                            AND ReportingRedactionAccuracy.DatabaseServiceID = @DatabaseServiceID 
                            AND (
                                ReportingRedactionAccuracy.FoundAttributeSetForFileID != FoundAttributeSet.ID
			                    OR ReportingRedactionAccuracy.ExpectedAttributeSetForFileID != ExpectedAttributeSet.ID
			                )
                )
            ";

        /// <summary>
        /// Query used to get the data used to create the records in ReportingRedactionAccuracy table
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
                      ,foundFTS.DateTimeStamp FoundDateTimeStamp
                      ,foundFTS.ActionID FoundActionID
                      ,foundFS.FAMUserID FoundFAMUserID
                      ,expectedFTS.DateTimeStamp ExpectedDateTimeStamp
                      ,expectedFTS.ActionID ExpectedActionID
                      ,expectedFS.FAMUserID ExpectedFAMUserID
                FROM MostRecent found 
	                INNER JOIN MostRecent expected 
		                ON found.Description = @FoundSetName AND Expected.Description = @ExpectedSetName 
			                AND found.FileID = expected.FileID
                    INNER JOIN FileTaskSession foundFTS 
                            ON found.MostRecentFileTaskSession = foundFTS.ID
                    INNER JOIN FAMSession foundFS 
                        ON foundFTS.FAMSessionID = foundFS.ID
                    INNER JOIN FileTaskSession expectedFTS 
                            ON expected.MostRecentFileTaskSession = expectedFTS.ID
                    INNER JOIN FAMSession expectedFS 
                        ON expectedFTS.FAMSessionID = expectedFS.ID
	                INNER JOIN AttributeSetForFile FoundAttributeSet 
		                ON FoundAttributeSet.FileTaskSessionID = found.MostRecentFileTaskSession 
			                AND FoundAttributeSet.AttributeSetNameID = found.AttributeSetNameID
	                INNER JOIN AttributeSetForFile ExpectedAttributeSet 
		                ON ExpectedAttributeSet.FileTaskSessionID = expected.MostRecentFileTaskSession 
			                AND ExpectedAttributeSet.AttributeSetNameID = expected.AttributeSetNameID
	                LEFT JOIN ReportingRedactionAccuracy 
		                ON ReportingRedactionAccuracy.FoundAttributeSetForFileID = FoundAttributeSet.ID
			                AND ReportingRedactionAccuracy.ExpectedAttributeSetForFileID = ExpectedAttributeSet.ID
			                AND ReportingRedactionAccuracy.DatabaseServiceID = @DatabaseServiceID
                WHERE ReportingRedactionAccuracy.ID IS NULL
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
        /// Default constructor for RedactionAccuracy
        /// </summary>
        public RedactionAccuracy()
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
        /// Performs the processing needed for the records in ReportingRedactionAccuracy table
        /// </summary>
        /// <param name="cancelToken">Token that can cancel the processing</param>
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
                        int foundDateTimeStampColumn = ExpectedAndFoundReader.GetOrdinal("FoundDateTimeStamp");
                        int foundActionIDColumn = ExpectedAndFoundReader.GetOrdinal("FoundActionID");
                        int foundFAMUserIDColumn = ExpectedAndFoundReader.GetOrdinal("FoundFAMUserID");
                        int expectedDateTimeStampColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedDateTimeStamp");
                        int expectedActionIDColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedActionID");
                        int expectedFAMUserIDColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedFAMUserID");

                        // Process the found records
                        while (ExpectedAndFoundReader.Read() && !cancelToken.IsCancellationRequested)
                        {
                            // Get the streams for the expected and found voa data (the thread will read the voa from the stream
                            Stream expectedStream = ExpectedAndFoundReader.GetStream(expectedVOAColumn);
                            Stream foundStream = ExpectedAndFoundReader.GetStream(foundVOAColumn);
                            Int64 foundID = ExpectedAndFoundReader.GetInt64(foundAttributeForFileSetColumn);
                            Int64 expectedID = ExpectedAndFoundReader.GetInt64(expectedAttributeForFileSetColumn);
                            Int32 fileID = ExpectedAndFoundReader.GetInt32(fileIDColumn);
                            DateTime foundDateTime = ExpectedAndFoundReader.GetDateTime(foundDateTimeStampColumn);
                            Int32 foundActionID = ExpectedAndFoundReader.GetInt32(foundActionIDColumn);
                            Int32 foundFAMUserID = ExpectedAndFoundReader.GetInt32(foundFAMUserIDColumn);
                            DateTime expectedDateTime = ExpectedAndFoundReader.GetDateTime(expectedDateTimeStampColumn);
                            Int32 expectedActionID = ExpectedAndFoundReader.GetInt32(expectedActionIDColumn);
                            Int32 expectedFAMUserID = ExpectedAndFoundReader.GetInt32(expectedFAMUserIDColumn);

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
                                        var output = IDShieldAttributeComparer.CompareAttributes(ExpectedAttributes, FoundAttributes, XPathOfSensitiveAttributes).ToList();

                                        // process output for each page
                                        foreach (var pageKeyPair in output)
                                        {
                                            // Add the comparison results to the Results
                                            var statsToSave = pageKeyPair.Value.AggregateStatistics();

                                            var lookup = statsToSave.ToLookup(a => new { a.Path, a.Label });

                                            var attributePaths = statsToSave
                                                .Select(a => a.Path)
                                                .Distinct()
                                                .OrderBy(p => p)
                                                .ToList();

                                            List<string> valuesToAdd = new List<string>();
                                            foreach (var p in attributePaths)
                                            {
                                                int expected = lookup[new { Path = p, Label = AccuracyDetailLabel.Expected }].Sum(a => a.Value);
                                                int found = lookup[new { Path = p, Label = AccuracyDetailLabel.Found }].Sum(a => a.Value);
                                                int correct = lookup[new { Path = p, Label = AccuracyDetailLabel.Correct }].Sum(a => a.Value);
                                                int falsePositives = lookup[new { Path = p, Label = AccuracyDetailLabel.FalsePositives }].Sum(a => a.Value);
                                                int overRedacted = lookup[new { Path = p, Label = AccuracyDetailLabel.OverRedacted }].Sum(a => a.Value);
                                                int underRedacted = lookup[new { Path = p, Label = AccuracyDetailLabel.UnderRedacted }].Sum(a => a.Value);
                                                int missed = lookup[new { Path = p, Label = AccuracyDetailLabel.Missed }].Sum(a => a.Value);
                                                int page = pageKeyPair.Key;

                                                if (expected != 0 || found != 0 || correct != 0 || falsePositives != 0
                                                    || overRedacted != 0 || underRedacted != 0 || missed != 0)
                                                {
                                                    valuesToAdd.Add(string.Format(CultureInfo.InvariantCulture,
                                                        @"({0}, {1}, {2}, {3}, {4}, '{5}', {6}, {7}, {8}, {9}, {10}, {11}, {12}, '{13:s}', {14}, {15}, '{16:s}', {17}, {18} )"
                                                        , DatabaseServiceID
                                                        , foundID
                                                        , expectedID
                                                        , fileID
                                                        , page
                                                        , p
                                                        , expected
                                                        , found
                                                        , correct
                                                        , falsePositives
                                                        , overRedacted
                                                        , underRedacted
                                                        , missed
                                                        , foundDateTime
                                                        , foundFAMUserID
                                                        , foundActionID
                                                        , expectedDateTime
                                                        , expectedFAMUserID
                                                        , expectedActionID
                                                        ));
                                                }
                                            }

                                            using (TransactionScope scope = new TransactionScope())
                                            using (var saveConnection = NewSqlDBConnection())
                                            {
                                                saveConnection.Open();
                                                // Add the data to the ReportingRedactionAccuracy table
                                                var saveCmd = saveConnection.CreateCommand();

                                                saveCmd.CommandText = string.Format(CultureInfo.InvariantCulture,
                                                    @"INSERT INTO [dbo].[ReportingRedactionAccuracy]
                                                       ([DatabaseServiceID]
                                                       ,[FoundAttributeSetForFileID]
                                                       ,[ExpectedAttributeSetForFileID]
                                                       ,[FileID]
                                                       ,[Page]
                                                       ,[Attribute]
                                                       ,[Expected]
                                                       ,[Found]
                                                       ,[Correct]
                                                       ,[FalsePositives]
                                                       ,[OverRedacted]
                                                       ,[UnderRedacted]
                                                       ,[Missed]
                                                       ,[FoundDateTimeStamp]
                                                       ,[FoundFAMUserID]
                                                       ,[FoundActionID]
													   ,[ExpectedDateTimeStamp]
                                                       ,[ExpectedFAMUserID]
                                                       ,[ExpectedActionID])
                                                     VALUES
                                                           {0};", string.Join(",\r\n", valuesToAdd));
                                                saveCmd.ExecuteNonQuery();
                                                scope.Complete();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.AsExtract("ELI45383").Log();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45384");
            }
            finally
            {
                _processing = false;
            }
        }

        #endregion

        #endregion

        #region IConfigSettings implementation
        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrWhiteSpace(Description) &&
                    !string.IsNullOrWhiteSpace(FoundAttributeSetName) &&
                    !string.IsNullOrWhiteSpace(ExpectedAttributeSetName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45684");
            }
        }
        public bool Configure()
        {
            try
            {
                RedactionAccuracyForm form = new RedactionAccuracyForm(this);
                return form.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45681");
            }
            return false;
        }

        #endregion


        #region RedactionAccuracy Properties

        /// <summary>
        /// XPath query of attributes to be compared
        /// </summary>
        [DataMember]
        public string XPathOfSensitiveAttributes{ get; set; } = @"                    
                    /*/HCData
                  | /*/MCData
                  | /*/LCData
                  | /*/Manual";

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
        /// <param name="context">The context.</param>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI45385", "Settings were saved with a newer version.");
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
                    rollbackException.AsExtract("ELI45388").Log();
                }
                ExtractException ee = new ExtractException("ELI45389", "Unable to update ReportingRedactionAccuracy", ex);
                ee.AddDebugData("SaveQuery", deleteCommand.CommandText, false);
                throw ee;
            }
        }
        #endregion
    }
}
