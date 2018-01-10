using Extract.ETL;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Extract.UtilityApplications.NERTrainer
{
    public class NERTrainer : DatabaseService
    {
        #region Constants

        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Query to get MLData from the FAM DB
        /// </summary>
        static readonly string _GET_MLDATA =
            @"SELECT TOP(@Max) MLData.ID, Data FROM MLData
                JOIN MLModel ON MLData.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND IsTrainingData = @IsTrainingData
                AND CanBeDeleted = 'False'
                ORDER BY DateTimeStamp DESC";

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
        readonly string _TOTAL_ACCURACY_PATTERN =
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
        public int LastIDProcessed { get; set; }

        /// <summary>
        /// The average F1Score from the last time the testing command was successfully executed
        /// </summary>
        [DataMember]
        public double LastF1Score { get; set; }

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
        public int MaximumTrainingDocuments { get; set; }

        /// <summary>
        /// The maximum number of MLData records that will be used for testing
        /// </summary>
        [DataMember]
        public int MaximumTestingDocuments { get; set; }

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

        public override bool Processing => _processing;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an instance
        /// </summary>
        public NERTrainer()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Processes using configured DB
        /// </summary>
        public override void Process()
        {
            Process(DatabaseServer, DatabaseName);
        }

        /// <summary>
        /// Runs the training/testing process
        /// </summary>
        /// <param name="databaseServer">The database server</param>
        /// <param name="databaseName">The name of the database</param>
        public void Process(string databaseServer, string databaseName)
        {
            try
            {
                _processing = true;

                using (var tempModelFile = new TemporaryFile(true))
                using (var tempExceptionLog = new TemporaryFile(".uex", false))
                {
                    var pathTags = new SourceDocumentPathTags();
                    pathTags.AddTag(TempModelPathTag, tempModelFile.FileName);

                    bool copyToDestination = false;
                    int lastIDProcessed = -1;

                    // Train
                    if (!string.IsNullOrWhiteSpace(TrainingCommand))
                    {
                        lastIDProcessed = Train(pathTags, databaseServer, databaseName, tempExceptionLog.FileName);
                        copyToDestination = true;
                    }

                    // Test
                    if (!string.IsNullOrWhiteSpace(TestingCommand))
                    {
                        var result = Test(pathTags, databaseServer, databaseName, tempExceptionLog.FileName);
                        copyToDestination = result.criteriaMet;
                        lastIDProcessed = Math.Max(lastIDProcessed, result.lastIDProcessed);
                    }

                    LastIDProcessed = Math.Max(LastIDProcessed, lastIDProcessed);

                    if (copyToDestination && !string.IsNullOrWhiteSpace(ModelDestination))
                    {
                        var dest = ModelDestination;
                        if (dest.EndsWith(".etf", StringComparison.OrdinalIgnoreCase))
                        {
                            int exitCode = SystemMethods.RunExecutable(
                                _ENCRYPT_FILE_APPLICATION,
                                new[] { tempModelFile.FileName, dest },
                                createNoWindow: true);
                            ExtractException.Assert("ELI45285", "Failed to create output file", exitCode == 0, "Destination file", dest);
                        }
                        else
                        {
                            FileSystemMethods.MoveFile(tempModelFile.FileName, dest, true);
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
                                File.WriteAllText(body.FileName, "Exception(s) logged while training/testing an NER model. Top-level exception message: " + warning.Message);
                                SystemMethods.RunExtractExecutable(_EMAIL_FILE_APPLICATION,
                                    new[]
                                    {
                                    EmailAddressesToNotifyOnFailure
                                    ,tempExceptionLog.FileName
                                    ,"/subject"
                                    ,string.IsNullOrWhiteSpace(EmailSubject) ? "NER Training failure" : EmailSubject
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
        /// Deserializes a <see cref="NERTrainer"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="NERTrainer"/> was previously saved</param>
        public static new NERTrainer FromJson(string settings)
        {
            try
            {
                return (NERTrainer)DatabaseService.FromJson(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45107");
            }
        }

        #endregion Public Methods

        #region Private Methods

        private int Train(SourceDocumentPathTags pathTags, string databaseServer, string databaseName, string logFileToEmail)
        {
            bool success = false;
            int lastIDProcessed = -1;
            string errorMessage = null;
            int maxToProcess = MaximumTrainingDocuments;

            while (!success)
            {
                try
                {
                    (string trainingData, int currentCount, int lastCount, int lastProcessedID) =
                        GetDataFromDB(databaseServer, databaseName, ModelName, true, maxToProcess);

                    ExtractException.Assert("ELI45287", "No training data found",
                        currentCount > 0, "Model", ModelName);

                    using (var trainingDataFile = new TemporaryFile(true))
                    {
                        File.WriteAllText(trainingDataFile.FileName, trainingData);
                        pathTags.AddTag(DataFilePathTag, trainingDataFile.FileName);

                        var command = pathTags.Expand(TrainingCommand);
                        int exitCode = SystemMethods.RunExecutable(command, out var _, out errorMessage);

                        if (exitCode == 0)
                        {
                            success = true;
                            lastIDProcessed = lastProcessedID;
                            MaximumTrainingDocuments = maxToProcess;
                        }
                        else if (errorMessage.ToLowerInvariant().Contains("memory"))
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

        private (bool criteriaMet, int lastIDProcessed) Test(SourceDocumentPathTags pathTags, string databaseServer, string databaseName, string logFileToEmail)
        {
            bool success = false;
            bool criteriaMet = false;
            int lastIDProcessed = -1;
            string errorMessage = null;
            int maxToProcess = MaximumTestingDocuments;

            while (!success)
            {
                try
                {
                    (string testingData, int currentCount, int lastCount, int lastProcessedID) =
                        GetDataFromDB(databaseServer, databaseName, ModelName, false, maxToProcess);

                    ExtractException.Assert("ELI45289", "No testing data found",
                        currentCount > 0, "Model", ModelName);

                    using (var testingDataFile = new TemporaryFile(true))
                    {
                        File.WriteAllText(testingDataFile.FileName, testingData);
                        pathTags.AddTag(DataFilePathTag, testingDataFile.FileName);

                        var command = pathTags.Expand(TestingCommand);

                        int exitCode = SystemMethods.RunExecutable(command, out string output, out errorMessage);
                        if (exitCode == 0)
                        {
                            success = true;
                            lastIDProcessed = lastProcessedID;
                            MaximumTestingDocuments = maxToProcess;

                            var appTrace = new ExtractException("ELI45118", "Application trace: Testing complete");
                            var match = Regex.Match(output, _TOTAL_ACCURACY_PATTERN);

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

                            appTrace.Log();
                            if (!criteriaMet)
                            {
                                appTrace.Log(logFileToEmail);
                            }
                        }
                        else if (errorMessage.ToLowerInvariant().Contains("memory"))
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

        (string data, int currentCount, int lastCount, int lastIDProcessed) GetDataFromDB(string dbserver, string dbname, string model, bool trainingData, int maxRecords)
        {
            try
            {
                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = dbserver,
                    InitialCatalog = dbname,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                string trainingOutput = null;
                int currentCount = 0;
                int lastCount = 0;
                int lastIDProcessed = 0;
                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();
                    
                    // Get last processed count
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = _GET_LAST_PROCESSED_COUNT;
                        cmd.Parameters.AddWithValue("@Name", model);
                        cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);
                        cmd.Parameters.AddWithValue("@LastIDProcessed", LastIDProcessed);

                        var lines = new List<string>();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lastCount = reader.GetInt32(0);
                            }
                            reader.Close();
                        }
                    }

                    // Get the data
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = _GET_MLDATA;
                        cmd.Parameters.AddWithValue("@Name", model);
                        cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);
                        cmd.Parameters.AddWithValue("@Max", maxRecords);

                        var lines = new List<string>();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lastIDProcessed = reader.GetInt32(0);
                                do
                                {
                                    lines.Add(reader.GetString(1));
                                }
                                while (reader.Read());
                            }
                            currentCount = lines.Count;

                            // Reverse the data so that it is in inserted order
                            lines.Reverse();

                            trainingOutput = string.Join("", lines);
                            reader.Close();
                        }
                    }
                    connection.Close();
                }

                return (trainingOutput, currentCount, lastCount, lastIDProcessed);
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI45093");
                ue.AddDebugData("Database Server", dbserver, false);
                ue.AddDebugData("Database Name", dbname, false);
                ue.AddDebugData("MLModel", model, false);
                throw ue;
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

        #endregion Private Methods
    }
}