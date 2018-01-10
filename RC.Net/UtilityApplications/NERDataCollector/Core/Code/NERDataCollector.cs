using Extract.ETL;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace Extract.UtilityApplications.NERDataCollector
{
    [DataContract]
    public class NERDataCollector : DatabaseService
    {
        #region Constants

        const int CURRENT_VERSION = 1;

        /// <summary>
        /// The path to the NERAnnotator application
        /// </summary>
        private static readonly string _NER_ANNOTATOR_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "NERAnnotator.exe");

        private static readonly string _GET_HIGHEST_ID_TO_PROCESS =
            @"SELECT MAX(AttributeSetForFile.ID)
            FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
                WHERE Description = @AttributeSetName
                AND AttributeSetForFile.ID > @LastIDProcessed";

        #endregion Constants

        #region Fields

        bool _processing;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The name used to group training/testing data in FAMDB (table MLModel)
        /// </summary>
        [DataMember]
        public string ModelName { get; set; }

        /// <summary>
        /// The path to saved NERAnnotator settings
        /// </summary>
        [DataMember]
        public string AnnotatorSettingsPath { get; set; }

        /// <summary>
        /// The attribute set (VOAs stored in the DB) that will both determine which files get
        /// processed and be the source of annotation categories
        /// </summary>
        [DataMember]
        public string AttributeSetName { get; set; }

        /// <summary>
        /// The last AttributeSetForFile.ID that was processed by this application
        /// </summary>
        [DataMember]
        public long LastIDProcessed { get; set; }

        /// <summary>
        /// Whether processing
        /// </summary>
        public override bool Processing => _processing;


        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an instance
        /// </summary>
        public NERDataCollector()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Runs the data collection process
        /// </summary>
        /// <param name="databaseServer">The database server</param>
        /// <param name="databaseName">The name of the database</param>
        public void Process(string databaseServer, string databaseName)
        {
            try
            {
                _processing = true;

                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = databaseServer,
                    InitialCatalog = databaseName,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                var connection = new SqlConnection(sqlConnectionBuild.ConnectionString);
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = _GET_HIGHEST_ID_TO_PROCESS;
                    cmd.Parameters.AddWithValue("@AttributeSetname", AttributeSetName);
                    cmd.Parameters.AddWithValue("@LastIDProcessed", LastIDProcessed);

                    var reader = cmd.ExecuteReader();
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        long lowestIDToProcess = LastIDProcessed + 1;
                        long highestIDToProcess = reader.GetInt64(0);
                        LastIDProcessed = highestIDToProcess;
                        var arguments = new List<string>
                        {
                            "-p",
                            AnnotatorSettingsPath,
                            "--UseDatabase",
                            "true",
                            "--DatabaseServer",
                            databaseServer,
                            "--DatabaseName",
                            databaseName,
                            "--AttributeSetName",
                            AttributeSetName,
                            "--ModelName",
                            ModelName,
                            "--FirstIDToProcess",
                            lowestIDToProcess.ToString(CultureInfo.InvariantCulture),
                            "--LastIDToProcess",
                            highestIDToProcess.ToString(CultureInfo.InvariantCulture),
                        };
                        SystemMethods.RunExtractExecutable(_NER_ANNOTATOR_APPLICATION, arguments);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45040");
            }
            finally
            {
                _processing = false;
            }
        }

        /// <summary>
        /// Deserializes a <see cref="NERDataCollector"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="NERDataCollector"/> was previously saved</param>
        public static new NERDataCollector FromJson(string settings)
        {
            try
            {
                return (NERDataCollector)DatabaseService.FromJson(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45041");
            }
        }

        /// <summary>
        /// Processes using configured DB
        /// </summary>
        public override void Process()
        {
            Process(DatabaseServer, DatabaseName);
        }

        #endregion Public Methods

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
                ExtractException ee = new ExtractException("ELI45417", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        #endregion Private Methods
    }
}