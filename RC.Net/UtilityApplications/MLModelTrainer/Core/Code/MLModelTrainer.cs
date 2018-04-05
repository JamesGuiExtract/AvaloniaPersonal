﻿using Extract.AttributeFinder;
using Extract.Code.Attributes;
using Extract.ETL;
using Extract.Utilities;
using Extract.UtilityApplications.TrainingDataCollector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using YamlDotNet.RepresentationModel;

namespace Extract.UtilityApplications.MLModelTrainer
{
    [ExtractCategory("DatabaseService", "ML Model Trainer")]
    public class MLModelTrainer : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Internal classes

        /// <summary>
        /// Class for the MLModelTrainerStatus stored in the DatabaseService record
        /// </summary>
        [DataContract]
        public class MLModelTrainerStatus : DatabaseServiceStatus
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
            public int LastIDProcessed { get; set; }

            /// <summary>
            /// The average F1Score from the last time the testing command was successfully executed
            /// </summary>
            [DataMember]
            public double LastF1Score { get; set; }

            /// <summary>
            /// The maximum number of MLData records that will be used for training
            /// </summary>
            [DataMember]
            public int MaximumTrainingDocuments { get; set; }

            /// <summary>
            /// The maximum number of MLData records that will be used for testing
            /// </summary>
            [DataMember]
            public int MaximumTestingDocuments { get; set; }

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
        /// Query to get MLData from the FAM DB
        /// </summary>
        static readonly string _GET_MLDATA =
            @"SELECT ID, Data FROM
                (SELECT TOP(@Max) MLData.ID, Data, DateTimeStamp
                    FROM MLData
                    JOIN MLModel ON MLData.MLModelID = MLModel.ID
                    WHERE Name = @Name
                    AND IsTrainingData = @IsTrainingData
                    AND CanBeDeleted = 'False'
                    ORDER BY DateTimeStamp DESC
                ) a ORDER BY DateTimeStamp ASC";

        /// <summary>
        /// Query to get last processed count
        /// </summary>
        static readonly string _GET_LAST_PROCESSED_COUNT =
            @"SELECT COUNT(*) FROM MLData
                JOIN MLModel ON MLData.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND IsTrainingData = @IsTrainingData
                AND CanBeDeleted = 'False'
                AND MLData.ID <= @LastIDProcessed";

        /// <summary>
        /// Path tag that will resolve to the temporary file containing ML training/testing data
        /// </summary>
        internal static readonly string DataFilePathTag = "<DataFile>";

        /// <summary>
        /// Path tag that will resolve to the temporary model file created by the training process
        /// </summary>
        internal static readonly string TempModelPathTag = "<TempModelPath>";

        /// <summary>
        /// Pattern to match the output of the open nlp NER testing command
        /// </summary>
        readonly string _TOTAL_ACCURACY_PATTERN_NER =
            @"(?minx)^\s+TOTAL:
                \s+precision:\s+(?'precision'[\d.]+)%;
                \s+recall:\s+(?'recall'[\d.]+)%;
                \s+F1:\s+(?'f1'[\d.]+)%";

        /// <summary>
        /// The path to the EncryptFile application
        /// </summary>
        static readonly string _ENCRYPT_FILE_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "EncryptFile.exe");

        /// <summary>
        /// The path to the EncryptFile application
        /// </summary>
        static readonly string _EMAIL_FILE_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "EmailFile.exe");

        /// <summary>
        /// Path to the LearningMachineTrainer application
        /// </summary>
        static readonly string _LEARNING_MACHINE_TRAINER_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "LearningMachineTrainer.exe");

        #endregion Constants

        #region Fields

        bool _processing;
        TemporaryFile _tempModelFile;
        int _lastIDProcessed;
        double _lastF1Score;
        int _maximumTrainingDocuments;
        int _maximumTestingDocuments;
        MLModelTrainerStatus _status;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The name used to group training/testing data in FAMDB (table MLModel)
        /// </summary>
        [DataMember]
        public string ModelName { get; set; }

        /// <summary>
        /// Training command
        /// </summary>
        [DataMember]
        public string TrainingCommand { get; set; }

        /// <summary>
        /// The attribute set (VOAs stored in the DB) that will both determine which files get
        /// processed and be the source of annotation categories
        /// </summary>
        [DataMember]
        public string TestingCommand { get; set; }

        /// <summary>
        /// The path to which to write the trained model
        /// </summary>
        [DataMember]
        public string ModelDestination { get; set; }

        /// <summary>
        /// The ID of the last MLData record processed
        /// </summary>
        [DataMember]
        public int LastIDProcessed
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
        /// The average F1Score from the last time the testing command was successfully executed
        /// </summary>
        [DataMember]
        public double LastF1Score
        {
            get
            {
                if (_status != null)
                {
                    return _status.LastF1Score;
                }
                else
                {
                    return _lastF1Score;
                }
            }
            set
            {
                if (_status != null)
                {
                    _status.LastF1Score = value;
                }
                else
                {
                    _lastF1Score = value;
                }
            }
        }

        /// <summary>
        /// The lowest acceptable average F1 score
        /// </summary>
        [DataMember]
        public double MinimumF1Score { get; set; }

        /// <summary>
        /// The maximum decrease in F1 score before the testing result is considered unacceptable
        /// </summary>
        [DataMember]
        public double AllowableAccuracyDrop { get; set; }

        /// <summary>
        /// The maximum number of MLData records that will be used for training
        /// </summary>
        [DataMember]
        public int MaximumTrainingDocuments
        {
            get
            {
                if (_status != null)
                {
                    return _status.MaximumTrainingDocuments;
                }
                else
                {
                    return _maximumTrainingDocuments;
                }
            }
            set
            {
                if (_status != null)
                {
                    _status.MaximumTrainingDocuments = value;
                }
                else
                {
                    _maximumTrainingDocuments = value;
                }
            }
        }

        /// <summary>
        /// The maximum number of MLData records that will be used for testing
        /// </summary>
        [DataMember]
        public int MaximumTestingDocuments
        {
            get
            {
                if (_status != null)
                {
                    return _status.MaximumTestingDocuments;
                }
                else
                {
                    return _maximumTestingDocuments;
                }
            }
            set
            {
                if (_status != null)
                {
                    _status.MaximumTestingDocuments = value;
                }
                else
                {
                    _maximumTestingDocuments = value;
                }
            }
        }

        /// <summary>
        /// A comma-separated list of email addresses to notify in the event of failure/unacceptable testing result
        /// </summary>
        [DataMember]
        public string EmailAddressesToNotifyOnFailure { get; set; }

        /// <summary>
        /// The subject to use for emails sent upon failure
        /// </summary>
        [DataMember]
        public string EmailSubject { get; set; }

        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;


        [DataMember]
        public ModelType ModelType { get; set; }

        public override bool Processing => _processing;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an instance
        /// </summary>
        public MLModelTrainer()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Processes using configured DB
        /// </summary>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                using (_tempModelFile = new TemporaryFile(true))
                using (var tempExceptionLog = new TemporaryFile(".uex", false))
                {
                    bool copyToDestination = false;
                    int lastIDProcessed = -1;
                    SourceDocumentPathTags pathTags = null;

                    if (ModelType == ModelType.NamedEntityRecognition)
                    {
                        pathTags = new SourceDocumentPathTags();
                        pathTags.AddTag(TempModelPathTag, _tempModelFile.FileName);

                        // Train
                        if (!string.IsNullOrWhiteSpace(TrainingCommand))
                        {
                            lastIDProcessed = Train(pathTags, tempExceptionLog.FileName, cancelToken);
                            copyToDestination = true;
                        }

                        // Test
                        if (!string.IsNullOrWhiteSpace(TestingCommand))
                        {
                            var result = Test(pathTags, tempExceptionLog.FileName, cancelToken);
                            copyToDestination = result.criteriaMet;
                            lastIDProcessed = Math.Max(lastIDProcessed, result.lastIDProcessed);
                        }
                    }
                    else
                    {
                        // The LearningMachineTrainer modifies an existing .lm file
                        File.Copy(ModelDestination, _tempModelFile.FileName, true);

                        // Train
                        lastIDProcessed = Train(pathTags, tempExceptionLog.FileName, cancelToken);
                        copyToDestination = true;

                        // Test
                        var result = Test(pathTags, tempExceptionLog.FileName, cancelToken);
                        copyToDestination = result.criteriaMet;
                        lastIDProcessed = Math.Max(lastIDProcessed, result.lastIDProcessed);

                        // Verify that the machine was saved correctly
                        if (copyToDestination)
                        {
                            try
                            {
                                var lm = LearningMachine.Load(ModelDestination);
                            }
                            catch (Exception ex)
                            {
                                var ue = new ExtractException("ELI45704", "Failed to write LearningMachine correctly", ex);
                                ue.Log();
                                copyToDestination = false;
                            }
                        }
                    }

                    LastIDProcessed = Math.Max(LastIDProcessed, lastIDProcessed);

                    if (copyToDestination && !string.IsNullOrWhiteSpace(ModelDestination))
                    {
                        var dest = ModelDestination;
                        if (dest.EndsWith(".etf", StringComparison.OrdinalIgnoreCase))
                        {
                            int exitCode = SystemMethods.RunExecutable(
                                _ENCRYPT_FILE_APPLICATION,
                                new[] { _tempModelFile.FileName, dest },
                                createNoWindow: true, cancelToken: cancelToken);
                            ExtractException.Assert("ELI45285", "Failed to create output file", exitCode == 0, "Destination file", dest);
                        }
                        else
                        {
                            FileSystemMethods.MoveFile(_tempModelFile.FileName, dest, true);

                            // Save status to the DB
                            SaveStatus();
                        }
                    }
                    else
                    {
                        var warning = new ExtractException("ELI45293", "Training/testing failed to produce an adequate model");
                        warning.Log(tempExceptionLog.FileName);
                        warning.Log();

                        // Send email
                        if (!string.IsNullOrWhiteSpace(EmailAddressesToNotifyOnFailure))
                        {
                            using (var body = new TemporaryFile(".txt", false))
                            {
                                File.WriteAllText(body.FileName, "Exception(s) logged while training/testing an LM model. Top-level exception message: " + warning.Message);
                                SystemMethods.RunExtractExecutable(_EMAIL_FILE_APPLICATION,
                                    new[]
                                    {
                                    EmailAddressesToNotifyOnFailure
                                    ,tempExceptionLog.FileName
                                    ,"/subject"
                                    ,string.IsNullOrWhiteSpace(EmailSubject) ? "LM Model Training failure" : EmailSubject
                                    ,"/body"
                                    ,body.FileName
                                    });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45094");
            }
            finally
            {
                _processing = false;
            }
        }

        /// <summary>
        /// Deserializes a <see cref="MLModelTrainer"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="MLModelTrainer"/> was previously saved</param>
        public static new MLModelTrainer FromJson(string settings)
        {
            try
            {
                return (MLModelTrainer)DatabaseService.FromJson(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45107");
            }
        }

        #endregion Public Methods

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status => _status ?? new MLModelTrainerStatus
        {
            LastIDProcessed = LastIDProcessed,
            LastF1Score = LastF1Score,
            MaximumTrainingDocuments = MaximumTrainingDocuments,
            MaximumTestingDocuments = MaximumTestingDocuments
        };

        /// <summary>
        /// Refreshes the <see cref="DatabaseServiceStatus"/> by loading from the database, creating a new instance,
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
                    _status = GetLastOrCreateStatus(() => new MLModelTrainerStatus());
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
        /// Displays configuration dialog for the NERTrainer
        /// </summary>
        /// <returns><c>true</c> if configuration was accepted, <c>false</c> if it was not</returns>
        public bool Configure()
        {

            MLModelTrainerConfigurationDialog configurationDialog = new MLModelTrainerConfigurationDialog(this, DatabaseServer, DatabaseName);
            
            return configurationDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
        }

        /// <summary>
        /// Method returns whether the current instance is configured
        /// </summary>
        /// <returns><c>true</c> if configured, <c>false</c> if not</returns>
        public bool IsConfigured()
        {
            return true;
        }

        #endregion

        #region Private Methods

        private int Train(SourceDocumentPathTags pathTags, string logFileToEmail, CancellationToken cancelToken)
        {
            bool success = false;
            int lastIDProcessed = -1;
            string errorMessage = null;
            string outputMessage = null;
            int maxToProcess = MaximumTrainingDocuments;

            while (!success)
            {
                try
                {
                    int lastCount = GetLastProcessedCount(true);
                    IEnumerable<(string data, int id)> trainingData = GetDataFromDB(true, maxToProcess);

                    ExtractException.Assert("ELI45287", "No training data found",
                        trainingData.Any(), "Model", ModelName);

                    using (var trainingDataFile = new TemporaryFile(true))
                    {
                        int lastProcessedID = 0;
                        int currentCount = 0;
                        File.WriteAllLines(trainingDataFile.FileName, trainingData.Select(r =>
                        {
                            currentCount++;
                            lastProcessedID = r.id;
                            if (ModelType == ModelType.LearningMachine)
                            {
                                return ",," + r.data;
                            }
                            else
                            {
                                return r.data;
                            }
                        }));

                        int exitCode = 0;
                        if (ModelType == ModelType.NamedEntityRecognition)
                        {
                            pathTags.AddTag(DataFilePathTag, trainingDataFile.FileName);
                            var command = pathTags.Expand(TrainingCommand);
                            exitCode = SystemMethods.RunExecutable(command, out outputMessage, out errorMessage, cancelToken);
                        }
                        else
                        {
                            var trainingArgs = new[] { _tempModelFile.FileName, "/csvName", trainingDataFile.FileName };
                            exitCode = SystemMethods.RunExtractExecutable(_LEARNING_MACHINE_TRAINER_APPLICATION, trainingArgs,
                                out outputMessage, out errorMessage,
                                cancelToken, cancelToken != default(CancellationToken),
                                onlyLogExceptionsFromExecutable: true);
                        }

                        if (exitCode == 0)
                        {
                            success = true;
                            lastIDProcessed = lastProcessedID;
                            MaximumTrainingDocuments = maxToProcess;
                        }
                        else if (errorMessage.ToLowerInvariant().Contains("memory")
                            || outputMessage.ToLowerInvariant().Contains("memory"))
                        {
                            var warning = new ExtractException("ELI45291", "Possible out-of-memory error encountered training ML model");
                            warning.AddDebugData("Error message", errorMessage, false);
                            warning.Log();
                            warning.Log(logFileToEmail);

                            int diff = currentCount - lastCount;
                            int significantDiff = currentCount / 10;
                            if (diff > significantDiff)
                            {
                                maxToProcess = currentCount - diff / 2;
                            }
                            else
                            {
                                maxToProcess = currentCount - significantDiff;
                            }
                            if (maxToProcess <= 0)
                            {
                                var ue = new ExtractException("ELI45288", "Number of training documents has been reduced to zero");
                                ue.Log();
                                ue.Log(logFileToEmail);
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    break;
                }
            }

            ExtractException.Assert("ELI45116", "Training failed", success, "Error message", errorMessage);

            return lastIDProcessed;
        }

        private (bool criteriaMet, int lastIDProcessed) Test(SourceDocumentPathTags pathTags, string logFileToEmail, CancellationToken cancelToken)
        {
            bool success = false;
            int lastIDProcessed = -1;
            bool criteriaMet = false;
            string errorMessage = null;
            string outputMessage = null;
            int maxToProcess = MaximumTestingDocuments;

            while (!success)
            {
                try
                {
                    int lastCount = GetLastProcessedCount(false);
                    IEnumerable<(string data, int id)> testingData = GetDataFromDB(false, maxToProcess);

                    ExtractException.Assert("ELI45289", "No testing data found",
                        testingData.Any(), "Model", ModelName);

                    using (var testingDataFile = new TemporaryFile(true))
                    {
                        int lastProcessedID = 0;
                        int currentCount = 0;
                        File.WriteAllLines(testingDataFile.FileName, testingData.Select(r =>
                        {
                            currentCount++;
                            lastProcessedID = r.id;
                            if (ModelType == ModelType.LearningMachine)
                            {
                                return ",," + r.data;
                            }
                            else
                            {
                                return r.data;
                            }
                        }));

                        int exitCode = 0;
                        if (ModelType == ModelType.NamedEntityRecognition)
                        {
                            pathTags.AddTag(DataFilePathTag, testingDataFile.FileName);
                            var command = pathTags.Expand(TestingCommand);
                            exitCode = SystemMethods.RunExecutable(command, out outputMessage, out errorMessage, cancelToken);
                        }
                        else
                        {
                            var testingArgs = new[] { _tempModelFile.FileName, "/csvName", testingDataFile.FileName, "/testOnly" };
                            exitCode = SystemMethods.RunExtractExecutable(_LEARNING_MACHINE_TRAINER_APPLICATION, testingArgs, out outputMessage, out errorMessage,
                                cancelToken, cancelToken != default(CancellationToken),
                                onlyLogExceptionsFromExecutable: true);
                        }

                        if (exitCode == 0)
                        {
                            success = true;
                            lastIDProcessed = lastProcessedID;
                            MaximumTestingDocuments = maxToProcess;

                            var appTrace = new ExtractException("ELI45118", "Application trace: Testing complete");

                            if (ModelType == ModelType.NamedEntityRecognition)
                            {
                                var match = Regex.Match(outputMessage, _TOTAL_ACCURACY_PATTERN_NER);

                                if (match.Success && double.TryParse(match.Groups["f1"].Value, out var f1Percent))
                                {
                                    var f1 = f1Percent / 100;
                                    var h = ULP(f1) / 2;
                                    appTrace.AddDebugData("F1", f1, false);
                                    criteriaMet = (f1 + h) >= MinimumF1Score && (f1 + h + AllowableAccuracyDrop) >= LastF1Score;
                                    if (criteriaMet)
                                    {
                                        LastF1Score = f1;
                                    }
                                }
                                else
                                {
                                    criteriaMet = false;
                                }
                            }
                            else if (ModelType == ModelType.LearningMachine)
                            {
                                var yaml = new YamlStream();
                                yaml.Load(new StringReader(outputMessage));
                                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                                if (mapping
                                    .FirstOrDefault(node => ((YamlScalarNode)node.Key).Value.Equals("Testing Set Accuracy",
                                        StringComparison.OrdinalIgnoreCase))
                                    .Value is YamlMappingNode testingSetAccuracy
                                    &&
                                    testingSetAccuracy
                                    .FirstOrDefault(node => ((YamlScalarNode)node.Key).Value.StartsWith("F1 Score",
                                        StringComparison.OrdinalIgnoreCase))
                                    .Value is YamlScalarNode f1Node
                                    &&
                                    double.TryParse(f1Node.Value, out var f1))
                                {
                                    var h = ULP(f1) / 2;
                                    appTrace.AddDebugData("F1", f1, false);
                                    criteriaMet = (f1 + h) >= MinimumF1Score && (f1 + h + AllowableAccuracyDrop) >= LastF1Score;
                                    if (criteriaMet)
                                    {
                                        LastF1Score = f1;
                                    }
                                }
                                else
                                {
                                    criteriaMet = false;
                                }
                            }

                            appTrace.Log();
                            if (!criteriaMet)
                            {
                                appTrace.Log(logFileToEmail);
                            }
                        }
                        else if (errorMessage.ToLowerInvariant().Contains("memory")
                            || outputMessage.ToLowerInvariant().Contains("memory"))
                        {
                            var warning = new ExtractException("ELI45292", "Possible out-of-memory error encountered testing ML model");
                            warning.AddDebugData("Error message", errorMessage, false);
                            warning.Log();
                            warning.Log(logFileToEmail);

                            int diff = currentCount - lastCount;
                            int significantDiff = currentCount / 10;
                            if (diff > significantDiff)
                            {
                                maxToProcess = currentCount - diff / 2;
                            }
                            else
                            {
                                maxToProcess = currentCount - significantDiff;
                            }
                            if (maxToProcess <= 0)
                            {
                                var ue = new ExtractException("ELI45290", "Number of testing documents has been reduced to zero");
                                ue.Log();
                                ue.Log(logFileToEmail);
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    break;
                }
            }

            ExtractException.Assert("ELI45117", "Testing failed", success, "Error message", errorMessage);

            return (criteriaMet, lastIDProcessed);
        }

        int GetLastProcessedCount(bool trainingData)
        {
            using (var connection = NewSqlDBConnection())
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    var ue = ex.AsExtract("ELI45093");
                    ue.AddDebugData("Database Server", DatabaseServer, false);
                    ue.AddDebugData("Database Name", DatabaseName, false);
                    ue.AddDebugData("MLModel", ModelName, false);
                    throw ue;
                }

                // Get last processed count
                using (var cmd = connection.CreateCommand())
                {
                    int max = trainingData ? MaximumTrainingDocuments : MaximumTestingDocuments;
                    cmd.CommandText = _GET_LAST_PROCESSED_COUNT;
                    cmd.Parameters.AddWithValue("@Name", ModelName);
                    cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);
                    cmd.Parameters.AddWithValue("@LastIDProcessed", LastIDProcessed);

                    int lastCount = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lastCount = reader.GetInt32(0);
                        }
                        reader.Close();
                    }
                    return Math.Min(lastCount, max);
                }
            }
        }

        IEnumerable<(string data, int id)> GetDataFromDB(bool trainingData, int maxRecords)
        {
            using (var connection = NewSqlDBConnection())
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    var ue = ex.AsExtract("ELI45093");
                    ue.AddDebugData("Database Server", DatabaseServer, false);
                    ue.AddDebugData("Database Name", DatabaseName, false);
                    ue.AddDebugData("MLModel", ModelName, false);
                    throw ue;
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = _GET_MLDATA;
                    cmd.Parameters.AddWithValue("@Name", ModelName);
                    cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);
                    cmd.Parameters.AddWithValue("@Max", maxRecords);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            do
                            {
                                string line = reader.GetString(1);
                                yield return (line, id);
                            }
                            while (reader.Read());
                        }
                    }
                }
                connection.Close();
            }
        }


        // Get unit in last place value of a double
        // https://stackoverflow.com/a/16702336
        double ULP(double value)
        {
            if (double.IsNaN(value))
            {
                return value;
            }
            if (double.IsInfinity(value))
            {
                return double.PositiveInfinity;
            }

            Int64 bits = BitConverter.DoubleToInt64Bits(value);

            // Make positive
            bits &= 0x7FFFFFFFFFFFFFFFL;

            // if x == max_double (notice the _E_)
            if (bits == 0x7FEFFFFFFFFFFFFL)
            {
                return BitConverter.Int64BitsToDouble(bits) - BitConverter.Int64BitsToDouble(bits - 1);
            }

            double nextValue = BitConverter.Int64BitsToDouble(bits + 1);
            return nextValue - value;
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
                ExtractException ee = new ExtractException("ELI45430", "Settings were saved with a newer version.");
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