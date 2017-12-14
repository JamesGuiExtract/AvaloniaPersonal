using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Extract.AttributeFinder;
using Extract.DataCaptureStats;
using Extract.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UCLID_COMUTILSLib;


namespace Extract.ETL
{
    /// <summary>
    /// Class to implement the Database service for Data Capture accuracy stats
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709: CorrectCasingInTypeName")]
    public class DataCaptureAccuracy : IDatabaseService
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

        #region Constructors

        /// <summary>
        /// Default constructor for DataCaptureAccuracy
        /// </summary>
        public DataCaptureAccuracy()
        {
        }

        #endregion

        #region IDatabaseService implementation

        #region IDatabaseService Properties

        /// <summary>
        /// Description of the database service item
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Server. This value is not include in the settings
        /// </summary>
        public string DatabaseServer { get; set; } = string.Empty;

        /// <summary>
        /// Name of the database. This value is not included in the settings
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// This is the id from the DatabaseService table.  This value is not included in the settings
        /// </summary>
        public int DatabaseServiceID { get; set; } = 0;


        #endregion

        #region IDatabaseService Methods

        /// <summary>
        /// Performs the processing needed for the records in ReportingDataCaptureAccuracy table
        /// </summary>
        public void Process()
        {
            try
            {

                using (var connection = new SqlConnection(getConnectionString()))
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
                        while (ExpectedAndFoundReader.Read())
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
        }

        /// <summary>
        /// Loads the settings for DataCaptureAccuracy object from the <paramref name="settings"/>
        /// </summary>
        /// <param name="ID">ID of the record in the DatabaseService table</param>
        /// <param name="settings">The settings in json format that provide the settings this DataCaptureAccuracy instance</param>
        public void Load(int ID, string settings)
        {
            try
            {
                DatabaseServiceID = ID;

                JObject settingsObject = JObject.Parse(settings);

                // Version will be needed for future versions
                string versionString = (string)settingsObject["Version"];
                int version = JsonConvert.DeserializeObject<int>(versionString);
                
                if (version > CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI45382", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", version, false);
                    ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                    throw ee;
                }

                // Get the Version 1 properties
                Description = JsonConvert.DeserializeObject<string>((string)settingsObject["Description"]);
                XPathOfAttributesToIgnore = JsonConvert.DeserializeObject<string>((string)settingsObject["XPathOfAttributesToIgnore"]);
                XPathOfContainerOnlyAttributes = JsonConvert.DeserializeObject<string>((string)settingsObject["XPathOfContainerOnlyAttributes"]);

                ExpectedAttributeSetName = JsonConvert.DeserializeObject<string>((string)settingsObject["ExpectedAttributeSetName"]);
                FoundAttributeSetName = JsonConvert.DeserializeObject<string>((string)settingsObject["FoundAttributeSetName"]);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45367");
            }
        }

        /// <summary>
        /// Returns the settings in a json string
        /// </summary>
        public string GetSettings()
        {
            try
            {
                JObject settingsObject = new JObject();
                settingsObject["Type"] = JsonConvert.SerializeObject(GetType());

                settingsObject["Version"] = JsonConvert.SerializeObject(CURRENT_VERSION);
                settingsObject["Description"] = JsonConvert.SerializeObject(Description);
                settingsObject["XPathOfAttributesToIgnore"] = JsonConvert.SerializeObject(XPathOfAttributesToIgnore);
                settingsObject["XPathOfContainerOnlyAttributes"] = JsonConvert.SerializeObject(XPathOfContainerOnlyAttributes);
                settingsObject["ExpectedAttributeSetName"] = JsonConvert.SerializeObject(ExpectedAttributeSetName);
                settingsObject["FoundAttributeSetName"] = JsonConvert.SerializeObject(FoundAttributeSetName);

                return settingsObject.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45368");
            }
        }

        #endregion

        #endregion

        #region DataCaptureAccuracy Properties

        /// <summary>
        /// XPath query of attributes the be ignored when comparing attributes
        /// </summary>
        public string XPathOfAttributesToIgnore { get; set; } = "";                   

        /// <summary>
        /// XPath of Attributes that are only considered containers when comparing attributes
        /// </summary>
        public string XPathOfContainerOnlyAttributes { get; set; } = "";

        /// <summary>
        /// The set name of the expected attributes when doing the comparison
        /// </summary>
        public string ExpectedAttributeSetName { get; set; } = "DataSavedByOperator";

        /// <summary>
        /// The set name of the found attributes when doing the comparison
        /// </summary>
        public string FoundAttributeSetName { get; set; } = "DataFoundByRules";

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the connection string using the configured DatabaseServer and DatabaseName
        /// </summary>
        /// <returns>Connection string to connect to the configured DatabaseServer and DatabaseName</returns>
        string getConnectionString()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = DatabaseServer;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return sqlConnectionBuild.ConnectionString;
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
        /// <param name="connection">Database connnection to perform the deletion</param>
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
