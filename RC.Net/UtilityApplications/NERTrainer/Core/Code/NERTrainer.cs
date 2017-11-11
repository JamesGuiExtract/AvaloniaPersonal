using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Extract.UtilityApplications.NERTrainer
{
    public class NERTrainer
    {
        #region Constants

        static readonly string _GET_MLDATA =
            @"SELECT Data FROM MLData
            JOIN MLModel ON MLData.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND IsTrainingData = @IsTrainingData";

        internal static readonly string DataFilePathTag = "<DataFile>";
        internal static readonly string TempModelPathTag = "<TempModelPath>";
        readonly string _TOTAL_ACCURACY_PATTERN =
            @"(?minx)^\s+TOTAL:
                \s+precision:\s+(?'precision'[\d.]+)%;
                \s+recall:\s+(?'recall'[\d.]+)%;
                \s+F1:\s+(?'f1'[\d.]+)%";

        #endregion Constants

        #region Properties

        /// <summary>
        /// The name used to group training/testing data in FAMDB (table MLModel)
        /// </summary>
        public string ModelName { get; set; }


        /// <summary>
        /// Training command
        /// </summary>
        public string TrainingCommand { get; set; }

        /// <summary>
        /// The attribute set (VOAs stored in the DB) that will both determine which files get
        /// processed and be the source of annotation categories
        /// </summary>
        public string TestingCommand { get; set; }

        /// <summary>
        /// The path to which to write the trained model
        /// </summary>
        public string ModelDestination { get; set; }

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
        /// Runs the training/testing process
        /// </summary>
        /// <param name="databaseServer">The database server</param>
        /// <param name="databaseName">The name of the database</param>
        public void Process(string databaseServer, string databaseName)
        {
            try
            {
                using (var tempModelFile = new TemporaryFile(true))
                {
                    bool copyToDestination = false;

                    var pathTags = new SourceDocumentPathTags();
                    pathTags.AddTag(TempModelPathTag, tempModelFile.FileName);

                    // Train
                    if (!string.IsNullOrWhiteSpace(TrainingCommand))
                    {
                        string trainingData = GetDataFromDB(databaseServer, databaseName, ModelName, true);
                        using (var trainingDataFile = new TemporaryFile(true))
                        {
                            File.WriteAllText(trainingDataFile.FileName, trainingData);
                            pathTags.AddTag(DataFilePathTag, trainingDataFile.FileName);

                            var command = pathTags.Expand(TrainingCommand);
                            int exitCode = SystemMethods.RunExecutable(command, out string output, out string error);

                            ExtractException.Assert("ELI45116", "Training failed", exitCode == 0, "Error message", error);

                            copyToDestination = true;
                        }
                    }

                    // Test
                    if (!string.IsNullOrWhiteSpace(TestingCommand))
                    {
                        string testingData = GetDataFromDB(databaseServer, databaseName, ModelName, false);
                        using (var testingDataFile = new TemporaryFile(true))
                        {
                            File.WriteAllText(testingDataFile.FileName, testingData);
                            pathTags.AddTag(DataFilePathTag, testingDataFile.FileName);

                            var command = pathTags.Expand(TestingCommand);

                            int exitCode = SystemMethods.RunExecutable(command, out string output, out string error);

                            ExtractException.Assert("ELI45117", "Testing failed", exitCode == 0, "Error message", error);

                            var ue = new ExtractException("ELI45118", "Application trace: Testing complete");
                            var match = Regex.Match(output, _TOTAL_ACCURACY_PATTERN);

                            // TODO: Evaluate the score to decide whether to copy
                            if (match.Success)
                            {
                                ue.AddDebugData("F1", match.Groups["f1"].Value, false);
                                copyToDestination = true;
                            }
                            else
                            {
                                copyToDestination = false;
                            }
                            ue.Log();
                        }
                    }

                    if (copyToDestination && !string.IsNullOrWhiteSpace(ModelDestination))
                    {
                        var dest = ModelDestination;
                        if (dest.EndsWith(".etf", StringComparison.OrdinalIgnoreCase))
                        {
                            // TODO: Add support for creating an encrypted file. Currently this requires RDT license...
                            dest = dest.Substring(0, dest.Length - 4);
                        }

                        FileSystemMethods.MoveFile(tempModelFile.FileName, dest, true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45094");
            }
        }

        /// <summary>
        /// Load instance from a YAML string
        /// </summary>
        /// <param name="settings">The string containing the serialized object</param>
        public static NERTrainer LoadFromString(string settings)
        {
            try
            {
                var deserializer = new Deserializer();
                return deserializer.Deserialize<NERTrainer>(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45107");
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
                throw ex.AsExtract("ELI45108");
            }
        }

        #endregion Public Methods

        #region Private Methods

        string GetDataFromDB(string dbserver, string dbname, string model, bool trainingData)
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
                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = _GET_MLDATA;
                        cmd.Parameters.AddWithValue("@Name", model);
                        cmd.Parameters.AddWithValue("@IsTrainingData", trainingData);

                        var lines = new List<string>();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lines.Add(reader.GetString(0));
                            }

                            trainingOutput = string.Join("", lines);
                            reader.Close();
                        }
                    }
                    connection.Close();
                }
                return trainingOutput;
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
        
        #endregion Private Methods
    }
}