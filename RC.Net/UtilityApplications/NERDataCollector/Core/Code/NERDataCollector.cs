using Extract.Interfaces;
using Extract.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Extract.UtilityApplications.NERDataCollector
{
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    public class NERDataCollector : IDatabaseService
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

        bool _enabled;
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
        /// The description
        /// </summary>
        [DataMember]
        [YamlIgnore]
        public string Description { get; set; } = "";

        [YamlIgnore]
        public string DatabaseName { get; set; } = "";

        [YamlIgnore]
        public string DatabaseServer { get; set; } = "";

        [YamlIgnore]
        public int DatabaseServiceID { get; set; }

        /// <summary>
        /// The schedule
        /// </summary>
        [DataMember]
        [YamlIgnore]
        ScheduledEvent ScheduledEvent { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        [YamlIgnore]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                try
                {
                    if (value != _enabled)
                    {
                        _enabled = value;
                        if (ScheduledEvent != null)
                        {
                            ScheduledEvent.Enabled = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45418");
                }
            }
        }

        /// <summary>
        /// The schedule
        /// </summary>
        [YamlIgnore]
        public IScheduledEvent Schedule => ScheduledEvent;

        /// <summary>
        /// Whether processing
        /// </summary>
        [YamlIgnore]
        public bool Processing => _processing;


        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        [YamlIgnore]
        public int Version { get; } = CURRENT_VERSION;

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

        /// <summary>
        /// Processes using configured DB
        /// </summary>
        public void Process()
        {
            Process(DatabaseServer, DatabaseName);
        }

        /// <summary>
        /// Serializes to JSON string
        /// </summary>
        public string GetSettings()
        {
            return JsonConvert.SerializeObject(this,
                new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
        }

        #endregion Public Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="NERDataCollector"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="NERDataCollector"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                ScheduledEvent?.Dispose();
                ScheduledEvent = null;
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

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
        }

        #endregion Private Methods
    }
}