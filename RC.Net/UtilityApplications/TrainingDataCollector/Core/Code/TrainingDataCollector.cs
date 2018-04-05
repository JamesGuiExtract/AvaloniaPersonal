using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using Extract.AttributeFinder;
using Extract.ETL;
using Extract.Utilities;
using Extract.Code.Attributes;

namespace Extract.UtilityApplications.TrainingDataCollector
{

    /// <summary>
    /// The type of model to collect data for
    /// </summary>
    public enum ModelType
    {
        NamedEntityRecognition = 0,
        LearningMachine = 1
    }

    /// <summary>
    /// A <see cref="DatabaseService"/> that collects training/testing data for NER or other ML
    /// </summary>
    [DataContract]
    [ExtractCategory("DatabaseService", "Training data collector")]
    public class TrainingDataCollector : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Internal classes

        /// <summary>
        /// Class for the TrainingDataCollectorStatus stored in the DatabaseService record
        /// </summary>
        [DataContract]
        public class TrainingDataCollectorStatus : DatabaseServiceStatus
        {
            #region DatabaseVerificationStatus constants

            const int _CURRENT_VERSION = 1;

            #endregion

            #region MLModelTrainerStatus Properties

            [DataMember]
            public override int Version { get; protected set; } = _CURRENT_VERSION;

            /// <summary>
            /// The ID of the last MLData record processed
            /// </summary>
            [DataMember]
            public long LastIDProcessed { get; set; }

            #endregion

            #region MLModelTrainerStatus Serialization

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI45712", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                    throw ee;
                }

                Version = CURRENT_VERSION;
            } 

            #endregion
        }

        #endregion

        #region Constants

        const int CURRENT_VERSION = 1;

        /// <summary>
        /// The path to the NERAnnotator application
        /// </summary>
        static readonly string _NER_ANNOTATOR_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "NERAnnotator.exe");

        static readonly string _GET_HIGHEST_ID_TO_PROCESS =
            @"SELECT MAX(AttributeSetForFile.ID)
            FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
                WHERE Description = @AttributeSetName
                AND AttributeSetForFile.ID > @LastIDProcessed";

        #endregion Constants

        #region Fields

        bool _processing;
        long _lastIDProcessed;
        TrainingDataCollectorStatus _status;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The name used to group training/testing data in FAMDB (table MLModel)
        /// </summary>
        [DataMember]
        public string ModelName { get; set; }

        /// <summary>
        /// The path to saved NERAnnotator or LearningMachine settings
        /// </summary>
        [DataMember]
        public string DataGeneratorPath { get; set; }

        /// <summary>
        /// The attribute set (VOAs stored in the DB) that will both determine which files get
        /// processed and be the source of annotation categories, for NER, or DocumentTypes, for doc classifier LM
        /// </summary>
        [DataMember]
        public string AttributeSetName { get; set; }

        /// <summary>
        /// The last AttributeSetForFile.ID that was processed by this application
        /// </summary>
        [DataMember]
        public long LastIDProcessed
        {
            get
            {
                if (_status != null)
                {
                    return _status.LastIDProcessed;
                }
                else
                {
                    return _lastIDProcessed;
                }
            }
            set
            {
                if (_status != null)
                {
                    _status.LastIDProcessed = value;
                }
                else
                {
                    _lastIDProcessed = value;
                }
            }
        }


        /// <summary>
        /// Whether processing
        /// </summary>
        public override bool Processing => _processing;

        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        [DataMember]
        public ModelType ModelType { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an instance
        /// </summary>
        public TrainingDataCollector()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Runs the data collection process
        /// </summary>
        /// <param name="databaseServer">The database server</param>
        /// <param name="databaseName">The name of the database</param>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = _GET_HIGHEST_ID_TO_PROCESS;
                        cmd.Parameters.AddWithValue("@AttributeSetName", AttributeSetName);
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
                                DataGeneratorPath,
                                "--UseDatabase",
                                "true",
                                "--DatabaseServer",
                                DatabaseServer,
                                "--DatabaseName",
                                DatabaseName,
                                "--AttributeSetName",
                                AttributeSetName,
                                "--ModelName",
                                ModelName,
                                "--FirstIDToProcess",
                                lowestIDToProcess.ToString(CultureInfo.InvariantCulture),
                                "--LastIDToProcess",
                                highestIDToProcess.ToString(CultureInfo.InvariantCulture),
                            };

                            if (ModelType == ModelType.NamedEntityRecognition)
                            {
                                SystemMethods.RunExtractExecutable(_NER_ANNOTATOR_APPLICATION, arguments, cancelToken, true);
                            }
                            else if (ModelType == ModelType.LearningMachine)
                            {
                                using (var machine = LearningMachine.Load(DataGeneratorPath))
                                {
                                    machine.WriteDataToDatabase(cancelToken, DatabaseServer, DatabaseName, AttributeSetName, ModelName,
                                        lowestIDToProcess, highestIDToProcess);
                                }
                            }
                            else
                            {
                                throw new ExtractException("ELI45434", "Unknown model type");
                            }

                            // Save status to the DB
                            SaveStatus();
                        }
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
        /// Deserializes a <see cref="TrainingDataCollector"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="TrainingDataCollector"/> was previously saved</param>
        public static new TrainingDataCollector FromJson(string settings)
        {
            try
            {
                return (TrainingDataCollector)DatabaseService.FromJson(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45041");
            }
        }

        #endregion Public Methods

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status => _status ?? new TrainingDataCollectorStatus
        {
            LastIDProcessed = LastIDProcessed
        };

        /// <summary>
        /// Refreshes <see cref="_status"/> by loading from the database, creating a new instance,
        /// or setting it to null (if <see cref="DatabaseServiceID"/> is less than or equal to zero)
        /// </summary>
        public void RefreshStatus()
        {
            try
            {
                if (DatabaseServiceID > 0
                    && !string.IsNullOrEmpty(DatabaseServer)
                    && !string.IsNullOrEmpty(DatabaseName))
                {
                    _status = GetLastOrCreateStatus(() => new TrainingDataCollectorStatus());
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45724");
            }
        }

        #endregion IHasConfigurableDatabaseServiceStatus

        #region IConfigSettings implementation

        /// <summary>
        /// Displays configuration dialog for the TrainingDataCollector
        /// </summary>
        /// <returns><c>true</c> if configuration was accepted, <c>false</c> if it was not</returns>
        public bool Configure()
        {
            try
            {
                TrainingDataCollectorConfigurationDialog trainingDataCollectorConfiguration = new TrainingDataCollectorConfigurationDialog(this, DatabaseServer, DatabaseName);
                return trainingDataCollectorConfiguration.ShowDialog() == System.Windows.Forms.DialogResult.OK;

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45677");
            }
            return false;
        }

        /// <summary>
        /// Method returns whether the current instance is configured
        /// </summary>
        /// <returns><c>true</c> if configured, <c>false</c> if not</returns>
        public bool IsConfigured()
        {
            try
            {
                bool returnVal = !string.IsNullOrWhiteSpace(ModelName);

                returnVal = returnVal && !string.IsNullOrWhiteSpace(AttributeSetName);
                returnVal = returnVal && !string.IsNullOrWhiteSpace(Description);

                return returnVal;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45679");
            }
        }   
        
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
                ExtractException ee = new ExtractException("ELI45417", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        /// <summary>
        /// Saves the current <see cref="DatabaseServiceStatus"/> to the DB
        /// </summary>
        void SaveStatus()
        {
            SaveStatus(_status);
        }

        #endregion Private Methods
    }
}