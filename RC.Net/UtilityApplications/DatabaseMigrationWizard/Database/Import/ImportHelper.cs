using DatabaseMigrationWizard.Pages.Utility;
using Extract;
using Extract.SqlDatabase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UCLID_FILEPROCESSINGLib;

using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input
{
    public class ImportHelper : IDisposable
    {
        private ImportOptions ImportOptions { get; set; }

        private readonly IProgress<string> Progress;

        public ImportHelper(ImportOptions importOptions, IProgress<string> progress)
        {
            this.ImportOptions = importOptions;
            this.Progress = progress;

            // Any admin operations that need to alter the database in any way except writing to existing tables need
            // to do so via the authority of the current AD account rather than the "ExtractRole" application role
            this.ImportOptions.SqlConnection = new NoAppRoleConnection(ImportOptions.ConnectionInformation.DatabaseServer, ImportOptions.ConnectionInformation.DatabaseName);
            this.ImportOptions.SqlConnection.Open();
            this.ImportOptions.Transaction = this.ImportOptions.SqlConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Populates a temporary table in batches from a json file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablePath">The file path to the exported table</param>
        /// <param name="insertTemporaryTableSql">The insert command for a given table</param>
        /// <param name="dbConnection">A connection to the database</param>
        [SuppressMessage("Microsoft.Design", "CA1004:IdentifiersShouldBeCasedCorrectly", Justification = "The generic type is required for NewtonsoftJSON, and therefore it required.")]
        public static void PopulateTemporaryTable<T>(string tablePath, string insertTemporaryTableSql, ImportOptions importOptions)
        {
            try
            {
                // 999 batch size because that is the maximum our driver will allow... (I wanted 50k =( )
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
                        List<T> deSerializedTable = result.deSerializedTable;
                        keepReadingFile = result.keepReadingFile;

                        for (int i = 0; i < deSerializedTable.Count; i++)
                        {
                            insertBuilder.Append($" {deSerializedTable[i].ToString()},");
                            if ((i % batchSize == 0 && i != 0) || i + 1 == deSerializedTable.Count)
                            {
                                try
                                {
                                    importOptions.ExecuteCommand(insertBuilder.ToString().TrimEnd(','));
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
                ExtractException extractException = new ExtractException("ELI49683", 
                    Invariant($"Error processing {Path.GetFileNameWithoutExtension(tablePath)} table"), e); 
                extractException.AddDebugData("FilePath", tablePath);
                throw extractException;
            }
        }

        /// <summary>
        /// Begins the import from the file system to the database.
        /// <para><b>NOTE</b></para>
        /// Upon exception, it is the caller's responsibility to call <see cref="RollbackTransaction"/>
        /// once any reporting data from the current operation is gathered.
        /// </summary>
        public void Import()
        {
            try
            {
                IEnumerable<ISequence> instances = FilteredInstances();
                ExecutePriority(instances);
            }
            catch(Exception e)
            {
                // Don't rollback the transaction at this point to allow for the report page to list
                // processing errors.
                throw e.AsExtract("ELI49681");
            }
        }   
        
        public void CommitTransaction()
        {
            this.ImportOptions.Transaction.Commit();
        }

        public void RollbackTransaction()
        {
            this.ImportOptions.Transaction.Rollback();
        }

        /// <summary>
        /// Clears the database.
        /// </summary>
        public void ClearDatabase()
        {
            try
            {
                var fileProcessingDb = new FileProcessingDB()
                {
                    DatabaseServer = this.ImportOptions.ConnectionInformation.DatabaseServer,
                    DatabaseName = this.ImportOptions.ConnectionInformation.DatabaseName
                };

                fileProcessingDb.Clear(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50249");
            }
        }

        /// <summary>
        /// Checks to see if the labde tables are present, and if they are not filter them out
        /// If tables are missing throw an exception.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ISequence> FilteredInstances()
        {
            IEnumerable<ISequence> instances = Universal.GetClassesThatImplementInterface<ISequence>();

            if(!this.ImportOptions.ImportCoreTables)
            {
                IEnumerable<ISequence> coreTables =  instances.Where(instance => !instance.TableName.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE"));
                instances = instances.Except(coreTables);
            }
            if(!this.ImportOptions.ImportLabDETables)
            {
                IEnumerable<ISequence> labDeTables = instances.Where(instance => instance.TableName.ToUpper(CultureInfo.InvariantCulture).Contains("LABDE"));
                instances = instances.Except(labDeTables);
            }
            
            this.CheckForMissingJsonFiles(instances);

            return instances;
        }

        private void CheckForMissingJsonFiles(IEnumerable<ISequence> instances)
        {
            IEnumerable<ISequence> missingFiles = instances.Where(instance => !File.Exists(this.ImportOptions.ImportPath + "\\" + instance.TableName + ".json"));
            if (missingFiles.Any())
            {
                ExtractException extractException = new ExtractException("ELI49697", "You are missing required files to run this import!");
                foreach (ISequence sequence in missingFiles)
                {
                    extractException.AddDebugData("FileMissing", sequence.TableName);
                }

                throw extractException;
            }
        }

        /// <summary>
        /// Executes all instances for a given priority (Waits for them ALL to finish before returning).
        /// </summary>
        /// <param name="instances">All of the instances that implement SequenceInterface</param>
        /// <param name="priority">The priority level to execute.</param>
        private void ExecutePriority(IEnumerable<ISequence> instances)
        {
            instances = instances.OrderByDescending(m => m.Priority);
            foreach (ISequence instance in instances)
            {
                string instanceName = instance.ToString().Substring(instance.ToString().LastIndexOf("Sequence", StringComparison.OrdinalIgnoreCase)).Replace("Sequence", string.Empty);
                App.Current?.Dispatcher.Invoke(delegate
                {
                    this.Progress.Report(instanceName);
                });

                instance.ExecuteSequence(this.ImportOptions);

                App.Current?.Dispatcher.Invoke(delegate
                {
                    this.Progress.Report(instanceName);
                });
            }
        }

        /// <summary>
        /// Reads and deserializes a json file in a batch.
        /// </summary>
        /// <typeparam name="T">The generic type to deserialize to</typeparam>
        /// <param name="reader">The reference to the json reader.</param>
        /// <param name="serializer">The reference to the serializer</param>
        /// <returns>Returns a list of deserialized objects, and returns true if there are still records to process</returns>
        private static (List<T> deSerializedTable, bool keepReadingFile) LoadFromJSON<T>(ref JsonTextReader reader, ref JsonSerializer serializer)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ImportOptions?.Dispose();
            }
        }
    }
}
