using Extract.AttributeFinder;
using Extract.Code.Attributes;
using Extract.ETL;
using Extract.SqlDatabase;
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
using System.Text.RegularExpressions;
using System.Threading;
using YamlDotNet.RepresentationModel;

namespace Extract.UtilityApplications.MachineLearning
{
    [ExtractCategory("DatabaseService", "ML Model Trainer")]
    public class MLModelTrainer : MachineLearningService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Constants

        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Query to get MLData from the FAM DB
        /// </summary>
        static readonly string _GET_MLDATA =
            @"SELECT ID, Data FROM
                (SELECT TOP(@Max) MLData.ID, Data, DateTimeStamp
                    FROM MLData WITH (NOLOCK)
                    JOIN MLModel ON MLData.MLModelID = MLModel.ID
                    WHERE Name = @Name
                    AND IsTrainingData = @IsTrainingData
                    AND CanBeDeleted = 'False'
                    ORDER BY DateTimeStamp DESC
                ) a ORDER BY ID ASC";

        /// <summary>
        /// Query to get last processed count
        /// </summary>
        static readonly string _GET_LAST_PROCESSED_COUNT =
            @"SELECT COUNT(*) FROM MLData WITH (NOLOCK)
                JOIN MLModel ON MLData.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND IsTrainingData = @IsTrainingData
                AND CanBeDeleted = 'False'
                AND MLData.ID <= @LastIDProcessed";

        /// <summary>
        /// Query to get new data count
        /// </summary>
        static readonly string _GET_NEW_DATA_COUNT =
            @"SELECT COUNT(*) FROM MLData WITH (NOLOCK)
                JOIN MLModel ON MLData.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND IsTrainingData = @IsTrainingData
                AND CanBeDeleted = 'False'
                AND MLData.ID > @LastIDProcessed";

        static readonly string _MARK_OLD_DATA_FOR_DELETION =
            @"UPDATE d
                SET CanBeDeleted = 'True'
                FROM MLData d
                JOIN MLModel ON d.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND CanBeDeleted = 'False'
                AND ( 
                      IsTrainingData = 'True' AND d.ID < @FirstIDTrained
                   OR IsTrainingData = 'False' AND d.ID < @FirstIDTested
                )";

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

        static readonly string _DEFAULT_NER_TRAINING_COMMAND =
            @"""<CommonComponentsDir>\opennlp.ikvm.exe"" TokenNameFinderTrainer -model ""<TempModelPath>""" +
            @" -lang en -data ""<DataFile>"" -featuregen ""<ComponentDataDir>\NER\ExampleFeatureGen.xml""";

        static readonly string _DEFAULT_NER_TESTING_COMMAND =
            @"""<CommonComponentsDir>\opennlp.ikvm.exe"" TokenNameFinderEvaluator -model ""<TempModelPath>"" -data ""<DataFile>""";

        #endregion Constants

        #region Fields

        bool _processing;
        TemporaryFile _tempModelFile;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Training command
        /// </summary>
        [DataMember]
        public string TrainingCommand { get; set; } = _DEFAULT_NER_TRAINING_COMMAND;

        /// <summary>
        /// The attribute set (VOAs stored in the DB) that will both determine which files get
        /// processed and be the source of annotation categories
        /// </summary>
        [DataMember]
        public string TestingCommand { get; set; } = _DEFAULT_NER_TESTING_COMMAND;

        /// <summary>
        /// The path to which to write the trained model
        /// </summary>
        [DataMember]
        public string ModelDestination { get; set; }

        /// <summary>
        /// The path to which to write the trained model, relative to the <see cref="RootDir"/>
        /// </summary>
        public string QualifiedModelDestination =>
            string.IsNullOrWhiteSpace(ModelDestination) || Path.IsPathRooted(ModelDestination) || string.IsNullOrWhiteSpace(RootDir)
            ? ModelDestination
            : Path.Combine(RootDir, ModelDestination);

        /// <summary>
        /// The ID of the last MLData record processed
        /// </summary>
        [DataMember]
        public override long LastIDProcessed { get; set; }

        /// <summary>
        /// The average F1Score from the last time the testing command was successfully executed
        /// </summary>
        [DataMember]
        public double LastF1Score { get; set; }

        /// <summary>
        /// The lowest acceptable average F1 score
        /// </summary>
        [DataMember]
        public double MinimumF1Score { get; set; } = 0.6;

        /// <summary>
        /// The maximum decrease in F1 score before the testing result is considered unacceptable
        /// </summary>
        [DataMember]
        public double AllowableAccuracyDrop { get; set; } = 0.05;

        /// <summary>
        /// The maximum number of MLData records that will be used for training
        /// </summary>
        [DataMember]
        public int MaximumTrainingRecords { get; set; } = 10000;

        /// <summary>
        /// The maximum number of MLData records that will be used for testing
        /// </summary>
        [DataMember]
        public int MaximumTestingRecords { get; set; } = 10000;

        /// <summary>
        /// A comma-separated list of email addresses to notify in the event of failure/unacceptable testing result
        /// </summary>
        [DataMember]
        public string EmailAddressesToNotifyOnFailure { get; set; }

        /// <summary>
        /// The subject to use for emails sent upon failure
        /// </summary>
        [DataMember]
        public string EmailSubject { get; set; } = "Training failure";

        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;


        /// <summary>
        /// The type of model to be trained
        /// </summary>
        [DataMember]
        public ModelType ModelType { get; set; }

        /// <summary>
        /// Whether to mark old, unused ML data for deletion after running
        /// </summary>
        [DataMember]
        public bool MarkOldDataForDeletion { get; set; } = true;

        /// <summary>
        /// Whether processing
        /// </summary>
        public override bool Processing => _processing;

        /// <summary>
        /// The number of backups to keep for each model
        /// </summary>
        /// <remarks>
        /// When this limit has been reached, the oldest backup will be deleted each time
        /// a new version is saved
        /// </remarks>
        public int NumberOfBackupsToKeep => Container?.NumberOfBackupModelsToKeep ?? 0;

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
            string previousDirectory = null;
            try
            {
                _processing = true;
                int firstIDTrained = 0;
                int firstIDTested = 0;

                // Set the working directory so that relative paths can be used for -resources and -params arguments to the OpenNLP TokenNameFinderTrainer 
                previousDirectory = Directory.GetCurrentDirectory();
                if (!string.IsNullOrEmpty(RootDir))
                {
                    Directory.SetCurrentDirectory(RootDir);
                }

                using (_tempModelFile = new TemporaryFile(true))
                using (var tempExceptionLog = new TemporaryFile(".uex", false))
                {
                    bool copyToDestination = false;
                    int lastIDProcessed = -1;
                    AttributeFinderPathTags pathTags = null;

                    if (ModelType == ModelType.NamedEntityRecognition)
                    {
                        pathTags = new AttributeFinderPathTags(new UCLID_AFCORELib.AFDocument());
                        pathTags.AddTag(TempModelPathTag, _tempModelFile.FileName);

                        // Train
                        if (!string.IsNullOrWhiteSpace(TrainingCommand))
                        {
                            (firstIDTrained, lastIDProcessed) =
                                Train(pathTags, tempExceptionLog.FileName, cancelToken);

                            copyToDestination = true;
                        }

                        // Test
                        if (!string.IsNullOrWhiteSpace(TestingCommand))
                        {
                            var result = Test(pathTags, tempExceptionLog.FileName, cancelToken);
                            copyToDestination = result.criteriaMet;
                            firstIDTested = result.firstIDProcessed;
                            lastIDProcessed = Math.Max(lastIDProcessed, result.lastIDProcessed);
                        }
                    }
                    else
                    {
                        // The LearningMachineTrainer modifies an existing .lm file
                        File.Copy(QualifiedModelDestination, _tempModelFile.FileName, true);

                        // Train
                        (firstIDTrained, lastIDProcessed) =
                            Train(pathTags, tempExceptionLog.FileName, cancelToken);
                        copyToDestination = true;

                        // Test
                        var result = Test(pathTags, tempExceptionLog.FileName, cancelToken);
                        copyToDestination = result.criteriaMet;
                        firstIDTested = result.firstIDProcessed;
                        lastIDProcessed = Math.Max(lastIDProcessed, result.lastIDProcessed);

                        // Verify that the machine was saved correctly
                        if (copyToDestination)
                        {
                            try
                            {
                                using (var lm = LearningMachine.Load(QualifiedModelDestination)) { }
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

                    if (copyToDestination && !string.IsNullOrWhiteSpace(QualifiedModelDestination))
                    {
                        try
                        {
                            BackupPreviousModel();
                        }
                        catch (Exception ex)
                        {
                            var ue = new ExtractException("ELI46572", "Error encountered backing up previous model", ex);
                            ue.AddDebugData("Model path", QualifiedModelDestination);
                        }

                        var dest = QualifiedModelDestination;
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
                        }
                       
                        using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                        connection.Open();

                        if (MarkOldDataForDeletion)
                        {
                            using var cmd = connection.CreateCommand();

                            cmd.CommandText = _MARK_OLD_DATA_FOR_DELETION;
                            cmd.Parameters.AddWithValue("@Name", QualifiedModelName);
                            cmd.Parameters.AddWithValue("@FirstIDTrained", firstIDTrained);
                            cmd.Parameters.AddWithValue("@FirstIDTested", firstIDTested);

                            // Set the timeout so that it waits indefinitely
                            cmd.CommandTimeout = 0;

                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                var ue = new ExtractException("ELI45762", "Failed to mark data for deletion", ex);
                                ue.AddDebugData("Database Server", DatabaseServer, false);
                                ue.AddDebugData("Database Name", DatabaseName, false);
                                throw ue;
                            }

                        }
                        // Save status to the DB
                        SaveStatus(connection);
                    }
                    else
                    {
                        var warning = new ExtractException("ELI45293", "Training/testing failed to produce an adequate model");
                        try
                        {
                            warning.AddDebugData("Model file", QualifiedModelDestination, false);
                            warning.Log(tempExceptionLog.FileName);

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
                        catch (Exception ex)
                        {
                            throw ex.AsExtract("ELI47071");
                        }

                        throw warning;
                    }
                }
            }
            catch (Exception ex)
            {
                cancelToken.ThrowIfCancellationRequested();
                throw ex.AsExtract("ELI45094");
            }
            finally
            {
                _processing = false;

                if (previousDirectory != null)
                {
                    try
                    {
                        Directory.SetCurrentDirectory(previousDirectory);
                    }
                    catch { }
                }
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

        public override int CalculateUnprocessedRecordCount()
        {

            using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = _GET_NEW_DATA_COUNT;
            cmd.Parameters.AddWithValue("@Name", QualifiedModelName);
            cmd.Parameters.AddWithValue("@IsTrainingData", true);
            cmd.Parameters.AddWithValue("@LastIDProcessed", LastIDProcessed);
            cmd.CommandTimeout = 0;

            int newCount = 0;
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                newCount = reader.GetInt32(0);
            }
            reader.Close();
            return newCount;

        }

        /// <summary>
        /// Changes an answer, e.g., a doctype, in the configured LearningMachine's Encoder and also
        /// updates the MLData stored in the DB
        /// </summary>
        /// <param name="oldAnswer">The answer to be changed (must exist in the LearningMachine)</param>
        /// <param name="newAnswer">The new answer to change to (must not exist in the LearningMachine)</param>
        /// <param name="silent">Whether to display exceptions and messages</param>
        public override bool ChangeAnswer(string oldAnswer, string newAnswer, bool silent)
        {
            return ChangeAnswer(oldAnswer, newAnswer, QualifiedModelDestination, silent);
        }

        /// <summary>
        /// Updates the properties of this object using the provided <see cref="MLModelTrainerStatus"/>
        /// </summary>
        public override void UpdateFromStatus(DatabaseServiceStatus status)
        {
            try
            {
                if (status is MLModelTrainerStatus correctStatus)
                {
                    correctStatus.UpdateMLModelTrainer(this);
                }
                else
                {
                    throw new ArgumentException("MLModelTrainer.UpdateFromStatus requires a MLModelTrainerStatus");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46576");
            }
        }

        #endregion Public Methods

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public override DatabaseServiceStatus Status
        {
            get => new MLModelTrainerStatus(this);
        }

        /// <summary>
        /// Refreshes the <see cref="DatabaseServiceStatus"/> by loading from the database, creating a new instance,
        /// or setting it to null (if <see cref="DatabaseServiceID"/>, <see cref="DatabaseServer"/> and
        /// <see cref="DatabaseName"/> are not configured)
        /// </summary>
        public void RefreshStatus()
        {
            try
            {
                if (DatabaseServiceID > 0
                    && !string.IsNullOrEmpty(DatabaseServer)
                    && !string.IsNullOrEmpty(DatabaseName))
                {
                    UpdateFromStatus(GetLastOrCreateStatus(() => new MLModelTrainerStatus()));
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
        /// Displays configuration dialog for the MLModelTrainer
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
            try
            {
                bool returnVal = !string.IsNullOrWhiteSpace(ModelName);
                returnVal = returnVal && !string.IsNullOrWhiteSpace(Description);
                returnVal = returnVal && !string.IsNullOrWhiteSpace(ModelDestination);

                if (ModelType == ModelType.NamedEntityRecognition)
                {
                    returnVal = returnVal && !string.IsNullOrWhiteSpace(TrainingCommand);
                    returnVal = returnVal && !string.IsNullOrWhiteSpace(TestingCommand);
                }

                return returnVal;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46453");
                return false;
            }
        }

        #endregion

        #region Private Methods

        private (int firstIDProcessed, int lastIDProcessed) Train
            (AttributeFinderPathTags pathTags, string logFileToEmail, CancellationToken cancelToken)
        {
            bool success = false;
            int firstIDProcessed = -1;
            int lastIDProcessed = -1;
            string errorMessage = null;
            string outputMessage = null;
            int maxToProcess = MaximumTrainingRecords;

            while (!success)
            {
                try
                {
                    AppendToLog("Training...");

                    int lastCount = GetLastProcessedCount(true);
                    IEnumerable<(string data, int id)> trainingData = GetDataFromDB(true, maxToProcess);

                    ExtractException.Assert("ELI45287", "No training data found",
                        trainingData.Any(), "Model", QualifiedModelName);

                    using (var trainingDataFile = new TemporaryFile(true))
                    {
                        int firstProcessedID = trainingData.First().id;
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

                        AppendToLog(UtilityMethods.FormatCurrent($"{currentCount} records to process"));

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
                            firstIDProcessed = firstProcessedID;
                            lastIDProcessed = lastProcessedID;
                            MaximumTrainingRecords = maxToProcess;
                        }
                        else if (errorMessage.ToUpperInvariant().Contains("MEMORY")
                            || outputMessage.ToUpperInvariant().Contains("MEMORY"))
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

            return (firstIDProcessed, lastIDProcessed);
        }

        private (bool criteriaMet, int firstIDProcessed, int lastIDProcessed)
            Test(AttributeFinderPathTags pathTags, string logFileToEmail, CancellationToken cancelToken)
        {
            bool success = false;
            int firstIDProcessed = -1;
            int lastIDProcessed = -1;
            bool criteriaMet = false;
            string errorMessage = null;
            string outputMessage = null;
            int maxToProcess = MaximumTestingRecords;

            while (!success)
            {
                try
                {
                    AppendToLog("Testing...");

                    int lastCount = GetLastProcessedCount(false);
                    IEnumerable<(string data, int id)> testingData = GetDataFromDB(false, maxToProcess);

                    // If no testing data, test on training data
                    if (!testingData.Any())
                    {
                        AppendToLog("No testing data found. Using training data for test.");
                        testingData = GetDataFromDB(true, maxToProcess);
                    }

                    ExtractException.Assert("ELI45289", "No testing data found",
                        testingData.Any(), "Model", QualifiedModelName);

                    using (var testingDataFile = new TemporaryFile(true))
                    {
                        int firstProcessedID = testingData.First().id;
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

                        AppendToLog(UtilityMethods.FormatCurrent($"{currentCount} records to process"));

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
                            firstIDProcessed = firstProcessedID;
                            lastIDProcessed = lastProcessedID;
                            MaximumTestingRecords = maxToProcess;

                            var appTrace = new ExtractException("ELI45118", "Application trace: Testing complete");
                            appTrace.AddDebugData("Model file", QualifiedModelDestination, false);

                            if (ModelType == ModelType.NamedEntityRecognition)
                            {
                                var match = Regex.Match(outputMessage, _TOTAL_ACCURACY_PATTERN_NER);

                                if (match.Success && double.TryParse(match.Groups["f1"].Value, out var f1Percent))
                                {
                                    double f1 = f1Percent / 100.0;
                                    double h = ULP(f1) / 2;
                                    appTrace.AddDebugData("F1", f1, false);
                                    criteriaMet = (f1 + h) >= MinimumF1Score && (f1 + h + AllowableAccuracyDrop) >= LastF1Score;
                                    if (criteriaMet)
                                    {
                                        LastF1Score = f1;
                                    }

                                    if (double.TryParse(match.Groups["precision"].Value, out var precisionPercent))
                                    {
                                        AppendToLog("Precision: " + (precisionPercent / 100.0).ToString("N4", CultureInfo.CurrentCulture));
                                    }
                                    if (double.TryParse(match.Groups["recall"].Value, out var recallPercent))
                                    {
                                        AppendToLog("Precision: " + (recallPercent / 100.0).ToString("N4", CultureInfo.CurrentCulture));
                                    }
                                    AppendToLog("F1 Score: " + f1.ToString("N4", CultureInfo.CurrentCulture));
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
                                    .Value is YamlMappingNode testingSetAccuracy)
                                {
                                    var precisionNode = testingSetAccuracy
                                        .FirstOrDefault(node => ((YamlScalarNode)node.Key).Value.StartsWith("Precision",
                                            StringComparison.OrdinalIgnoreCase));
                                    var recallNode = testingSetAccuracy
                                        .FirstOrDefault(node => ((YamlScalarNode)node.Key).Value.StartsWith("Recall",
                                            StringComparison.OrdinalIgnoreCase));
                                    var f1Node = testingSetAccuracy
                                        .FirstOrDefault(node => ((YamlScalarNode)node.Key).Value.StartsWith("F1 Score",
                                            StringComparison.OrdinalIgnoreCase));

                                    if (!precisionNode.Equals(default(KeyValuePair<YamlNode, YamlNode>)))
                                    {
                                        AppendToLog(((YamlScalarNode)precisionNode.Key).Value + ": " +
                                                    ((YamlScalarNode)precisionNode.Value).Value);
                                    }
                                    if (!recallNode.Equals(default(KeyValuePair<YamlNode, YamlNode>)))
                                    {
                                        AppendToLog(((YamlScalarNode)recallNode.Key).Value + ": " +
                                                    ((YamlScalarNode)recallNode.Value).Value);
                                    }
                                    if (!f1Node.Equals(default(KeyValuePair<YamlNode, YamlNode>)))
                                    {
                                        var f1String = ((YamlScalarNode)f1Node.Value).Value;
                                        AppendToLog(((YamlScalarNode)f1Node.Key).Value + ": " + f1String);

                                        if (!double.TryParse(f1String, out double f1))
                                        {
                                            // This was triggering a warning. I am not sure how you want to handle an error if this fails.
                                        }
                                        double h = ULP(f1) / 2;
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
                        else if (errorMessage.ToUpperInvariant().Contains("MEMORY")
                            || outputMessage.ToUpperInvariant().Contains("MEMORY"))
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

            return (criteriaMet, firstIDProcessed, lastIDProcessed);
        }

        int GetLastProcessedCount(bool trainingData)
        {

            using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
            connection.Open();

            // Get last processed count
            using var cmd = connection.CreateCommand();

            int max = trainingData ? MaximumTrainingRecords : MaximumTestingRecords;
            cmd.CommandText = _GET_LAST_PROCESSED_COUNT;
            cmd.Parameters.AddWithValue("@Name", QualifiedModelName);
            cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);
            cmd.Parameters.AddWithValue("@LastIDProcessed", LastIDProcessed);
            cmd.CommandTimeout = 0;

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


        IEnumerable<(string data, int id)> GetDataFromDB(bool trainingData, int maxRecords)
        {
            using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = _GET_MLDATA;
            cmd.Parameters.AddWithValue("@Name", QualifiedModelName);
            cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);
            cmd.Parameters.AddWithValue("@Max", maxRecords);
            cmd.CommandTimeout = 0;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string line = reader.GetString(1);
                yield return (line, id);
            }
        }


        // Get unit in last place value of a double
        // https://stackoverflow.com/a/16702336
        static double ULP(double value)
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
        void SaveStatus(SqlAppRoleConnection connection)
        {
            if (Container is DatabaseService hasStatusRecord
                    && Container is IHasConfigurableDatabaseServiceStatus hasStatus)
            {
                hasStatusRecord.SaveStatus(connection, hasStatus.Status);
            }
            else
            {
                SaveStatus(connection, new MLModelTrainerStatus(this));
            }
        }

        void BackupPreviousModel()
        {
            int maxBackups = NumberOfBackupsToKeep;
            if (maxBackups > 0 && File.Exists(QualifiedModelDestination))
            {
                string nextBackupDir = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd.HH.mm", CultureInfo.InvariantCulture) + "UTC";
                string path = Path.GetDirectoryName(QualifiedModelDestination);
                string fileName = Path.GetFileName(QualifiedModelDestination);
                string backupRoot = Path.Combine(path, "__ml_model_backups__");
                string backupDir = Path.Combine(backupRoot, nextBackupDir);

                // Backup the file
                Directory.CreateDirectory(backupDir);
                File.Copy(QualifiedModelDestination, Path.Combine(backupDir, fileName), true);

                // Remove extra backups
                var backups = Directory.GetDirectories(backupRoot, "*-*-*.*.*UTC")
                    .SelectMany(dir => Directory.GetFiles(dir, fileName))
                    .ToList();

                int extraBackups = backups.Count - maxBackups;
                if (extraBackups > 0)
                {
                    backups.Sort();
                    var backupsToDelete = backups.Take(extraBackups);

                    // Delete the files
                    foreach (var file in backupsToDelete)
                    {
                        FileSystemMethods.DeleteFile(file);
                    }

                    // Delete any newly empty dirs
                    foreach (var dir in backupsToDelete.Select(file => Path.GetDirectoryName(file)))
                    {
                        if (Directory.GetFiles(dir).Length == 0)
                        {
                            Directory.Delete(dir);
                        }
                    }
                }
            }
        }

        #endregion Private Methods

        #region Internal classes

        /// <summary>
        /// Class for the MLModelTrainerStatus stored in the DatabaseService record
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        [DataContract]
        public class MLModelTrainerStatus : DatabaseServiceStatus
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

            /// <summary>
            /// The average F1Score from the last time the testing command was successfully executed
            /// </summary>
            [DataMember]
            public double LastF1Score { get; set; }

            /// <summary>
            /// The maximum number of MLData records that will be used for training
            /// </summary>
            [DataMember]
            public int MaximumTrainingRecords { get; set; } = 10000;

            /// <summary>
            /// The maximum number of MLData records that will be used for testing
            /// </summary>
            [DataMember]
            public int MaximumTestingRecords { get; set; } = 10000;

            #endregion

            #region MLModelTrainerStatus Constructors

            /// <summary>
            /// Creates a new status object with default values
            /// </summary>
            public MLModelTrainerStatus()
            {
            }

            /// <summary>
            /// Creates a new status object using the values of a <see cref="MLModelTrainer"/>
            /// </summary>
            /// <param name="mlModelTrainer">The <see cref="MLModelTrainer"/> to copy the settings from</param>
            public MLModelTrainerStatus(MLModelTrainer mlModelTrainer)
            {
                try
                {
                    LastIDProcessed = mlModelTrainer.LastIDProcessed;
                    LastF1Score = mlModelTrainer.LastF1Score;
                    MaximumTrainingRecords = mlModelTrainer.MaximumTrainingRecords;
                    MaximumTestingRecords = mlModelTrainer.MaximumTestingRecords;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI46577");
                }
            }

            /// <summary>
            /// Updates a <see cref="MLModelTrainer"/> with settings from this status object
            /// </summary>
            /// <param name="mlModelTrainer">The <see cref="MLModelTrainer"/> to update</param>
            public void UpdateMLModelTrainer(MLModelTrainer mlModelTrainer)
            {
                try
                {
                    mlModelTrainer.LastIDProcessed = LastIDProcessed;
                    mlModelTrainer.LastF1Score = LastF1Score;
                    mlModelTrainer.MaximumTrainingRecords = MaximumTrainingRecords;
                    mlModelTrainer.MaximumTestingRecords = MaximumTestingRecords;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI46578");
                }
            }

            #endregion

            #region MLModelTrainerStatus Serialization

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI45712", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                    throw ee;
                }

                Version = _CURRENT_VERSION;
            } 

            #endregion
        }

        #endregion

    }
}