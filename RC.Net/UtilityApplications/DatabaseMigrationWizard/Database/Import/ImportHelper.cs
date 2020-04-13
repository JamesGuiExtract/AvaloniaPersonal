﻿using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using Extract.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard.Database.Input
{
    public class ImportHelper
    {
        private ImportOptions ImportOptions { get; set; }

        private IProgress<string> Progress;

        public ImportHelper(ImportOptions importOptions, IProgress<string> progress)
        {
            this.ImportOptions = importOptions;
            this.Progress = progress;
        }

        /// <summary>
        /// Populates a temporary table in batches from a json file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablePath">The filepath to the exported table</param>
        /// <param name="insertTemporaryTableSql">The insert command for a given table</param>
        /// <param name="dbConnection">A connection to the database</param>
        [SuppressMessage("Microsoft.Design", "CA1004:IdentifiersShouldBeCasedCorrectly", Justification = "The generic type is required for NewtonsoftJSON, and therefore it required.")]
        public static void PopulateTemporaryTable<T>(string tablePath, string insertTemporaryTableSql, DbConnection dbConnection)
        {
            try
            {
                // 999 batchsize because that is the maximum our driver will allow... (I wanted 50k =( )
                int batchSize = 999;
                StringBuilder insertBuilder = new StringBuilder(insertTemporaryTableSql);
                var serializer = new JsonSerializer();
                bool keepReadingFile = true;

                using (StreamReader streamReader = File.OpenText(tablePath))
                {
                    JsonTextReader jsonReader = new JsonTextReader(streamReader)
                    {
                        SupportMultipleContent = true
                    };

                    while (keepReadingFile)
                    {
                        // Read a batch of json items
                        var result = ImportHelper.LoadFromJSON<T>(ref jsonReader, ref serializer);
                        List<T> deSerializedTable = result.Item1;
                        keepReadingFile = result.Item2;

                        for (int i = 0; i < deSerializedTable.Count; i++)
                        {
                            insertBuilder.Append($" {deSerializedTable[i].ToString()},");
                            if ((i % batchSize == 0 && i != 0) || i + 1 == deSerializedTable.Count)
                            {
                                try
                                {
                                    DBMethods.ExecuteDBQuery(dbConnection, insertBuilder.ToString().TrimEnd(','));
                                }
                                catch(Exception e)
                                {
                                    ExtractException extractException = e.AsExtract("ELI49707");
                                    extractException.AddDebugData("SQL", insertBuilder.ToString().TrimEnd(','));
                                    throw extractException;
                                }

                                insertBuilder = new StringBuilder(insertTemporaryTableSql);
                            }
                        }
                    }

                    jsonReader.Close();
                }
            }
            catch(Exception e)
            {
                ExtractException extractException = new ExtractException("ELI49683", "Error populating temporary table", e);
                extractException.AddDebugData("FilePath", tablePath);
                throw extractException;
            }
        }

        /// <summary>
        /// Begins the import from the filesystem to the database.
        /// </summary>
        public void BeginImport()
        {
            try
            {
                IEnumerable<ISequence> instances = FilteredInstances();
                new Thread(() =>
                {
                    ClearDatabase();
                    ExecutePriority(instances, Priorities.High);
                    ExecutePriority(instances, Priorities.MediumHigh);
                    ExecutePriority(instances, Priorities.Medium);
                    ExecutePriority(instances, Priorities.MediumLow);
                    ExecutePriority(instances, Priorities.Low);
                }).Start();
            }
            catch(Exception e)
            {
                ExtractException.Display("ELI49681", e);
            }
        }

        /// <summary>
        /// Checks to see if the labde tables are present, and if they are not filter them out
        /// If tables are missing throw an exception.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ISequence> FilteredInstances()
        {
            string[] files = System.IO.Directory.GetFiles(this.ImportOptions.ImportPath);
            bool hasLabDeTables = files.Where(file => file.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE")).Any();
            IEnumerable<ISequence> instances = Universal.GetClassesThatImplementInterface<ISequence>();
            if(!hasLabDeTables)
            {
                instances = instances.Where(instance => !instance.TableName.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE"));
            }
            IEnumerable<ISequence> missingFiles = instances.Where(instance => !File.Exists(this.ImportOptions.ImportPath + "\\" + instance.TableName + ".json"));
            if(missingFiles.Any())
            {
                ExtractException extractException = new ExtractException("ELI49697", "You are missing required tables to run this import!");
                foreach(ISequence sequence in missingFiles)
                {
                    extractException.AddDebugData("TableMissing", sequence.TableName);
                }
                
                throw extractException;
            }
            return instances;
        }

        private void ClearDatabase()
        {
            if(ImportOptions.ClearDatabase)
            {
                var fileProcessingDb = new FileProcessingDB()
                {
                    DatabaseServer = this.ImportOptions.ConnectionInformation.DatabaseServer,
                    DatabaseName = this.ImportOptions.ConnectionInformation.DatabaseName
                };

                fileProcessingDb.Clear(false);
            }
        }

        /// <summary>
        /// Executes all instances for a given priority (Waits for them ALL to finish before returning).
        /// </summary>
        /// <param name="instances">All of the instances that implement SequenceInterface</param>
        /// <param name="priority">The priority level to execute.</param>
        private void ExecutePriority(IEnumerable<ISequence> instances, Priorities priority)
        {
            List<Thread> threads = new List<Thread>();
            foreach (ISequence instance in instances.Where(instance => instance.Priority == priority))
            {
                Thread thread = new Thread(() => 
                {
                    using (var sqlConnection = new SqlConnection($@"Server={ImportOptions.ConnectionInformation.DatabaseServer};Database={ImportOptions.ConnectionInformation.DatabaseName};Integrated Security=SSPI"))
                    {
                        sqlConnection.Open();
                        string instanceName = instance.ToString().Substring(instance.ToString().LastIndexOf("Sequence", StringComparison.OrdinalIgnoreCase)).Replace("Sequence", string.Empty);
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            this.Progress.Report(instanceName);
                        });

                        instance.ExecuteSequence(sqlConnection, this.ImportOptions);

                        App.Current.Dispatcher.Invoke(delegate
                        {
                            this.Progress.Report(instanceName);
                        });

                        sqlConnection.Close();
                    }
                });

                threads.Add(thread);
                thread.Start();
            }
            threads.WaitAll();
        }

        /// <summary>
        /// Reads and deserializes a json file in a batch.
        /// </summary>
        /// <typeparam name="T">The generic type to deserialize to</typeparam>
        /// <param name="reader">The reference to the json reader.</param>
        /// <param name="serializer">The reference to the serializer</param>
        /// <returns>Returns a list of deserialized obejcts, and returns true if there are still records to process</returns>
        private static (List<T>, bool) LoadFromJSON<T>(ref JsonTextReader reader, ref JsonSerializer serializer)
        {
            int bufferSize = 20000;
            int index = 0;
            List<T> list = new List<T>();
            bool stillJsonObjectsToDeserialize = true;

            while(index < bufferSize && reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    list.Add(serializer.Deserialize<T>(reader));
                }
                index += 1;
            }
            if(index < bufferSize)
            {
                stillJsonObjectsToDeserialize = false;
            }

            return (list, stillJsonObjectsToDeserialize);
        }
    }
}
