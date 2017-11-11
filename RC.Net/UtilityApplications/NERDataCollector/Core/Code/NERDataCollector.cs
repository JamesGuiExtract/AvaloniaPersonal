using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using YamlDotNet.Serialization;

namespace Extract.UtilityApplications.NERDataCollector
{
    public class NERDataCollector
    {
        #region Constants

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

        #region Properties

        /// <summary>
        /// The name used to group training/testing data in FAMDB (table MLModel)
        /// </summary>
        public string ModelName { get; set; }


        /// <summary>
        /// The path to saved NERAnnotator settings
        /// </summary>
        public string AnnotatorSettingsPath { get; set; }

        /// <summary>
        /// The attribute set (VOAs stored in the DB) that will both determine which files get
        /// processed and be the source of annotation categories
        /// </summary>
        public string AttributeSetName { get; set; }

        /// <summary>
        /// The last AttributeSetForFile.ID that was processed by this application
        /// </summary>
        public long LastIDProcessed { get; set; }

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
        }

        /// <summary>
        /// Load instance from a YAML string
        /// </summary>
        /// <param name="settings">The string containing the serialized object</param>
        public static NERDataCollector LoadFromString(string settings)
        {
            try
            {
                var deserializer = new Deserializer();
                return deserializer.Deserialize<NERDataCollector>(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45041");
            }
        }

        /// <summary>
        /// Save this instance to a string as YAML
        /// </summary>
        public string SaveToString()
        {
            try
            {
                var sb = new SerializerBuilder();
                sb.EmitDefaults();
                var serializer = sb.Build();
                return serializer.Serialize(this);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45042");
            }
        }

        #endregion Public Methods
    }
}