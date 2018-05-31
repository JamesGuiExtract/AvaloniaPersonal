﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using Extract.AttributeFinder;
using Extract.ETL;
using Extract.Utilities;
using Extract.Code.Attributes;
using System.Transactions;
using System.Diagnostics.CodeAnalysis;

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
    public class TrainingDataCollector : MachineLearningService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Constants

        const int CURRENT_VERSION = 1;

        static readonly string _GET_AVAILABLE_IDS =
            @"SELECT AttributeSetForFile.ID
            FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
            JOIN FileTaskSession ON FileTaskSessionID = FileTaskSession.ID
                WHERE Description = @AttributeSetName
                AND AttributeSetForFile.ID > @LastIDProcessed
                AND FileTaskSession.DateTimeStamp >= @StartDate";

        static readonly string _GET_NEW_DATA_COUNT =
            @"SELECT COUNT(*)
            FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
            JOIN FileTaskSession ON FileTaskSessionID = FileTaskSession.ID
                WHERE Description = @AttributeSetName
                AND AttributeSetForFile.ID > @LastIDProcessed
                AND FileTaskSession.DateTimeStamp >= @StartDate";

        #endregion Constants

        #region Fields

        bool _processing;
        long _lastIDProcessed;
        TrainingDataCollectorStatus _status;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The path to saved NERAnnotator or LearningMachine settings
        /// </summary>
        [DataMember]
        public string DataGeneratorPath { get; set; }

        /// <summary>
        /// The path to which to write the trained model, relative to the <see cref="RootDir"/>
        /// </summary>
        public string QualifiedDataGeneratorPath =>
            string.IsNullOrWhiteSpace(DataGeneratorPath) || Path.IsPathRooted(DataGeneratorPath) || string.IsNullOrWhiteSpace(RootDir)
            ? DataGeneratorPath
            : Path.Combine(RootDir, DataGeneratorPath);

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

        /// <summary>
        /// The type of model that this is a collector for
        /// </summary>
        [DataMember]
        public ModelType ModelType { get; set; }

        /// <summary>
        /// Whether to override the value specified in the annotator or learning machine settings file
        /// </summary>
        [DataMember]
        public bool OverrideTrainingTestingSplit { get; set; } = true;

        /// <summary>
        /// The portion of the input to use for training data if overriding the settings file's value
        /// </summary>
        [DataMember]
        public int TrainingPercent { get; set; } = 80;

        /// <summary>
        /// Whether to use the configured attribute set (the same that controls the files that get processed)
        /// for expected values rather than the configured VOA file path in the settings file
        /// </summary>
        [DataMember]
        public bool UseAttributeSetForExpecteds { get; set; } = true;

        /// <summary>
        /// Whether to run a ruleset for candidate attributes or protofeature attributes rather than using
        /// the configured VOA file path in the settings file
        /// </summary>
        [DataMember]
        public bool RunRulesetForCandidateOrFeatures { get; set; }

        /// <summary>
        /// Whether to run the ruleset in the case that the VOA doesn't exist
        /// </summary>
        [DataMember]
        public bool RunRulesetIfVoaIsMissing { get; set; }

        /// <summary>
        /// The path to a ruleset to use for generating candidate or protofeature attributes
        /// </summary>
        [DataMember]
        public string FeatureRulesetPath { get; set; }

        /// <summary>
        /// The path to a ruleset to use for generating candidate or protofeature attributes, based on the <see cref="RootDir"/>
        /// </summary>
        public string QualifiedFeatureRulesetPath =>
            string.IsNullOrWhiteSpace(FeatureRulesetPath) || Path.IsPathRooted(FeatureRulesetPath) || string.IsNullOrWhiteSpace(RootDir)
            ? FeatureRulesetPath
            : Path.Combine(RootDir, FeatureRulesetPath);

        /// <summary>
        /// Limits the attribute set for files processed to the most recent only
        /// </summary>
        [DataMember]
        public TimeSpan LimitProcessingToMostRecent { get; set; } = TimeSpan.FromDays(30);

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
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                List<long> availableIDs = new List<long>();
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = _GET_AVAILABLE_IDS;
                        cmd.Parameters.AddWithValue("@AttributeSetName", AttributeSetName);
                        cmd.Parameters.AddWithValue("@LastIDProcessed", LastIDProcessed);
                        cmd.Parameters.AddWithValue("@StartDate", DateTime.Now.Add(-LimitProcessingToMostRecent));

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            availableIDs.Add(reader.GetInt64(0));
                        }
                    }
                }

                // If no new data since last processed, return
                if (availableIDs.Count == 0)
                {
                    return;
                }

                if (ModelType == ModelType.NamedEntityRecognition)
                {
                    var settings = NERAnnotator.Settings.LoadFrom(QualifiedDataGeneratorPath);
                    settings.UseDatabase = true;
                    settings.DatabaseServer = DatabaseServer;
                    settings.DatabaseName = DatabaseName;
                    settings.AttributeSetName = AttributeSetName;
                    settings.ModelName = QualifiedModelName;
                    settings.UseAttributeSetForTypes = UseAttributeSetForExpecteds;
                    if (OverrideTrainingTestingSplit)
                    {
                        settings.PercentToUseForTestingSet = 100 - TrainingPercent;
                    }

                    // Collect/add data in batches of 500 records at a time to mitigate memory issues
                    for (int i = 0; i < availableIDs.Count; i += 500)
                    {
                        long lowestIDToProcess = availableIDs[i];
                        long highestIDToProcess = availableIDs[Math.Min(i + 499, availableIDs.Count - 1)];

                        LastIDProcessed = highestIDToProcess;

                        settings.FirstIDToProcess = lowestIDToProcess;
                        settings.LastIDToProcess = highestIDToProcess;

                        using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                            { IsolationLevel = IsolationLevel.ReadCommitted,
                              Timeout = TransactionManager.MaximumTimeout }))
                        {
                            NERAnnotator.NERAnnotator.Process(settings, _ => { }, cancelToken);

                            // Save status to the DB each loop
                            SaveStatus();

                            ts.Complete();
                        }
                    }
                }
                else if (ModelType == ModelType.LearningMachine)
                {
                    using (var machine = LearningMachine.Load(QualifiedDataGeneratorPath))
                    {
                        if (OverrideTrainingTestingSplit)
                        {
                            machine.InputConfig.TrainingSetPercentage = TrainingPercent;
                        }
                        // Collect/add data in batches of 500 records at a time to mitigate memory issues
                        for (int i = 0; i < availableIDs.Count; i += 500)
                        {
                            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                                { IsolationLevel = IsolationLevel.ReadCommitted,
                                  Timeout = TransactionManager.MaximumTimeout }))
                            {
                                long lowestIDToProcess = availableIDs[i];
                                long highestIDToProcess = availableIDs[Math.Min(i + 499, availableIDs.Count - 1)];
                                machine.WriteDataToDatabase(cancelToken, DatabaseServer, DatabaseName, AttributeSetName, QualifiedModelName,
                                        lowestIDToProcess, highestIDToProcess, UseAttributeSetForExpecteds,
                                        RunRulesetForCandidateOrFeatures, RunRulesetIfVoaIsMissing, QualifiedFeatureRulesetPath);

                                LastIDProcessed = highestIDToProcess;

                                // Save status to the DB each loop
                                SaveStatus();

                                ts.Complete();
                            }
                        }
                    }
                }
                else
                {
                    throw new ExtractException("ELI45434", "Unknown model type");
                }
            }
            catch (Exception ex)
            {
                cancelToken.ThrowIfCancellationRequested();
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

        public override int CalculateUnprocessedRecordCount()
        {
            using (var connection = NewSqlDBConnection())
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    var ue = ex.AsExtract("ELI45759");
                    ue.AddDebugData("Database Server", DatabaseServer, false);
                    ue.AddDebugData("Database Name", DatabaseName, false);
                    ue.AddDebugData("MLModel", QualifiedModelName, false);
                    throw ue;
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = _GET_NEW_DATA_COUNT;
                    cmd.Parameters.AddWithValue("@AttributeSetName", AttributeSetName);
                    cmd.Parameters.AddWithValue("@LastIDProcessed", LastIDProcessed);
                    cmd.Parameters.AddWithValue("@StartDate", DateTime.Now.Add(-LimitProcessingToMostRecent));

                    int newCount = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            newCount = reader.GetInt32(0);
                        }
                        reader.Close();
                    }
                    return newCount;
                }
            }
            
        }

        /// <summary>
        /// Changes an answer, e.g., a doctype, in the configured LearningMachine's Encoder and also
        /// updates the MLData stored in the DB
        /// </summary>
        /// <param name="oldAnswer">The answer to be changed (must exist in the LearningMachine)</param>
        /// <param name="newAnswer">The new answer to change to (must not exist in the LearningMachine)</param>
        public override bool ChangeAnswer(string oldAnswer, string newAnswer)
        {
            return ChangeAnswer(oldAnswer, newAnswer, QualifiedDataGeneratorPath);
        }

        #endregion Public Methods

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public override DatabaseServiceStatus Status
        {
            get => _status ?? new TrainingDataCollectorStatus
            {
                LastIDProcessed = LastIDProcessed
            };

            set => _status = value as TrainingDataCollectorStatus;
        }

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
                TrainingDataCollectorConfigurationDialog trainingDataCollectorConfiguration
                    = new TrainingDataCollectorConfigurationDialog(this, DatabaseServer, DatabaseName);
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
                returnVal = returnVal && !string.IsNullOrWhiteSpace(DataGeneratorPath);

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
        /// Called when this instance is being deserialized.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            // Set default for new settings
            LimitProcessingToMostRecent = TimeSpan.FromDays(30);
            UseAttributeSetForExpecteds = true;
        }

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
            if (Container is DatabaseService hasStatusRecord
                && Container is IHasConfigurableDatabaseServiceStatus hasStatus)
            {
                hasStatusRecord.SaveStatus(hasStatus.Status);
            }
            else
            {
                SaveStatus(_status);
            }
        }

        #endregion Private Methods

        #region Internal classes

        /// <summary>
        /// Class for the TrainingDataCollectorStatus stored in the DatabaseService record
        /// </summary>
        [DataContract]
        public class TrainingDataCollectorStatus : DatabaseServiceStatus
        {
            #region MLModelTrainerStatus Constants

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

    }
}