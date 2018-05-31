﻿using Extract.Code.Attributes;
using Extract.ETL;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Extract.UtilityApplications.TrainingCoordinator
{
    [DataContract]
    [ExtractCategory("DatabaseService", "Training coordinator")]
    public class TrainingCoordinator : DatabaseService, ITrainingCoordinator, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Constants

        const int CURRENT_VERSION = 1;

        static readonly string _REMOVE_MARKED_MLDATA = @"DELETE MLData WHERE CanBeDeleted = 'True'";

        #endregion Constants

        #region Fields

        bool _processing;
        TrainingCoordinatorStatus _status;
        string _log;
        Dictionary<Guid, DatabaseServiceStatus> _serviceStatuses;

        #endregion Fields

        #region Properties

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
        /// This value is used to prefix MLModel names of contained services, like a namespace
        /// </summary>
        [DataMember]
        public string ProjectName { get; set; }

        /// <summary>
        /// The directory to use to resolve any paths, e.g., to LM files
        /// </summary>
        [DataMember]
        public string RootDir { get; set; }

        /// <summary>
        /// Whether to delete ML data that has been marked for deletion after running
        /// </summary>
        [DataMember]
        public bool DeleteMarkedMLData { get; set; } = true;

        /// <summary>
        /// Value used to skip the training process if no/little new data has been stored
        /// </summary>
        [DataMember]
        public int MinimumNewRecordsRequiredForTraining { get; set; }

        /// <summary>
        /// The list of contained services that are <see cref="TrainingDataCollector.TrainingDataCollector"/>s
        /// </summary>
        [DataMember]
        public List<MachineLearningService> DataCollectors { get; set; } = new List<MachineLearningService>();

        /// <summary>
        /// The list of contained services that are <see cref="MLModelTrainer.MLModelTrainer"/>s
        /// </summary>
        [DataMember]
        public List<MachineLearningService> ModelTrainers { get; set; } = new List<MachineLearningService>();

        /// <summary>
        /// An enumeration of both data collectors and model trainers (data collectors first)
        /// </summary>
        public IEnumerable<MachineLearningService> Services =>
            (DataCollectors ?? Enumerable.Empty<MachineLearningService>())
            .Concat(ModelTrainers ?? Enumerable.Empty<MachineLearningService>());

        /// <summary>
        /// The record of what services were run and when they were run
        /// </summary>
        [DataMember]
        public string Log
        {
            get
            {
                if (_status != null)
                {
                    return _status.Log;
                }
                else
                {
                    return _log;
                }
            }
            set
            {
                if (_status != null)
                {
                    _status.Log = value;
                }
                else
                {
                    _log = value;
                }
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// A collection of individual statuses for the contained services
        /// (since these do not have their own records in the DB)
        /// </summary>
        [DataMember]
        public Dictionary<Guid,DatabaseServiceStatus> ServiceStatuses
        {
            get
            {
                if (_status != null)
                {
                    return _status.ServiceStatuses;
                }
                else
                {
                    return _serviceStatuses;
                }
            }
            set
            {
                if (_status != null)
                {
                    _status.ServiceStatuses = value;
                }
                else
                {
                    _serviceStatuses = value;
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Create an instance
        /// </summary>
        public TrainingCoordinator()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Deserializes a <see cref="TrainingCoordinator"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="TrainingCoordinator"/> was previously saved</param>
        public static new TrainingCoordinator FromJson(string settings)
        {
            try
            {
                return (TrainingCoordinator)DatabaseService.FromJson(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45798");
            }
        }

        /// <summary>
        /// Runs the data collection/training processes
        /// </summary>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                foreach (var service in Services.Where(s => s.Enabled))
                {
                    cancelToken.ThrowIfCancellationRequested();

                    string type = service is TrainingDataCollector.TrainingDataCollector
                        ? "data collector"
                        : "model trainer";

                    if (service is IConfigSettings config && !config.IsConfigured())
                    {
                        Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                            $"Skipping {type} \"{service.Description}\" because of invalid configuration\r\n\r\n");

                        continue;
                    }

                    int newDataCount = service.CalculateUnprocessedRecordCount();
                    if (newDataCount == 0
                        && service is TrainingDataCollector.TrainingDataCollector
                        || newDataCount < MinimumNewRecordsRequiredForTraining
                        && service is MLModelTrainer.MLModelTrainer trainer)
                    {
                        var unit = newDataCount == 1 ? "record" : "records";
                        Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                            $"Skipping {type} \"{service.Description}\" ",
                            $"because of insufficient new data ({newDataCount} new {unit})\r\n\r\n");

                        continue;
                    }

                    Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                            $"Starting {type} \"{service.Description}\"\r\n");

                    try
                    {
                        service.Container = this;
                        service.Process(cancelToken);

                        Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                                $"Done running {type} \"{service.Description}\"\r\n\r\n");
                    }
                    catch (AggregateException ex)
                    {
                        Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                            $"Error occurred: {ex.Flatten().InnerExceptions.First().Message}\r\n\r\n");
                        ex.ExtractLog("ELI45799");
                    }
                    catch (Exception ex)
                    {
                        Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                            $"Error occurred: {ex.Message}\r\n\r\n");
                        ex.ExtractLog("ELI45800");
                    }

                    SaveStatus(Status);
                }

                if (DeleteMarkedMLData && !cancelToken.IsCancellationRequested)
                {
                    Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                            $"Removing old ML data... \r\n");

                    using (var connection = NewSqlDBConnection())
                    {
                        try
                        {
                            connection.Open();
                        }
                        catch (Exception ex)
                        {
                            var ue = ex.AsExtract("ELI45801");
                            ue.AddDebugData("Database Server", DatabaseServer, false);
                            ue.AddDebugData("Database Name", DatabaseName, false);
                            throw ue;
                        }

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = _REMOVE_MARKED_MLDATA;

                            // Set the timeout so that it waits indefinitely
                            cmd.CommandTimeout = 0;

                            try
                            {
                                int rowsRemoved = cmd.ExecuteNonQuery();
                                Log += UtilityMethods.FormatCurrent($"{rowsRemoved} records removed\r\n\r\n");
                            }
                            catch (Exception ex)
                            {
                                Log += UtilityMethods.FormatCurrent($"{DateTime.Now}\r\n",
                                    $"Error occurred: {ex.Message}\r\n\r\n");
                                ex.ExtractLog("ELI45802");
                            }
                        }
                    }

                    SaveStatus(Status);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45803");
            }
            finally
            {
                _processing = false;
            }
        }

        /// <summary>
        /// Sets the LastIDProcessed value to zero for every <see cref="MachineLearningService"/>
        /// </summary>
        public void ResetProcessedStatus()
        {
            try
            {
                foreach (var service in Services)
                {
                    service.LastIDProcessed = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45992");
            }
        }

        #endregion Public Methods

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        /// <remarks>This will either be the status retrieved from the database, and possibly updated,
        /// or it will be newly created using serialized properties, if this object didn't have a valid
        /// DatabaseServiceID when it was previously edited</remarks>
        public DatabaseServiceStatus Status
        {
            get => _status ?? new TrainingCoordinatorStatus
            {
                Log = Log,
                ServiceStatuses = Services.ToDictionary(
                    service => service.Guid,
                    service => service.Status)
            };

            set => _status = value as TrainingCoordinatorStatus;
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
                    // Get status from DB or initialize an empty object
                    _status = GetLastOrCreateStatus(() => new TrainingCoordinatorStatus());

                    // Refresh each service's status
                    if (ServiceStatuses != null)
                    {
                        foreach (var service in Services)
                        {
                            if (ServiceStatuses.TryGetValue(service.Guid, out var status))
                            {
                                service.Status = status;
                            }
                            else
                            {
                                service.Status = null;
                            }
                        }
                    }

                    // Set the status to be null so that it gets rebuilt out of the individual
                    // service statuses on save. Otherwise UI edits may not be persisted
                    _log = _status.Log;
                    _status = null;
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45804");
            }
        }

        #endregion IHasConfigurableDatabaseServiceStatus

        #region IConfigSettings implementation

        /// <summary>
        /// Displays configuration dialog for the TrainingDataCoordinator
        /// </summary>
        /// <returns><c>true</c> if configuration was accepted, <c>false</c> if it was not</returns>
        public bool Configure()
        {
            try
            {
                var trainingDataCoordinatorConfiguration = new TrainingCoordinatorConfigurationDialog(this, DatabaseServer, DatabaseName);
                return trainingDataCoordinatorConfiguration.ShowDialog() == System.Windows.Forms.DialogResult.OK;

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45805");
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
                bool returnVal = !string.IsNullOrWhiteSpace(ProjectName);

                returnVal = returnVal && !string.IsNullOrWhiteSpace(RootDir);
                returnVal = returnVal && !string.IsNullOrWhiteSpace(Description);

                return returnVal;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45806");
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
                ExtractException ee = new ExtractException("ELI45807", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }
            Version = CURRENT_VERSION;

            foreach (var service in Services)
            {
                service.Container = this;
            }
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(Log))
                {
                    // Truncate history so that it doesn't go over 1000 actions
                    var actions = Log.Split(new[] { "\r\n\r\n" }, StringSplitOptions.None);
                    if (actions.Length > 1000)
                    {
                        Log = string.Join("\r\n\r\n", actions.Skip(actions.Length - 1000));
                    }
                }
            }
            catch { }
        }

        #endregion Private Methods

        #region Internal classes

        /// <summary>
        /// Class for the TrainingDataCoordinatorStatus stored in the DatabaseService record
        /// </summary>
        [DataContract]
        public class TrainingCoordinatorStatus : DatabaseServiceStatus
        {
            #region TrainingCoordinatorStatus Constants

            const int _CURRENT_VERSION = 1;

            #endregion

            #region TrainingCoordinatorStatus Properties

            [DataMember]
            public override int Version { get; protected set; } = _CURRENT_VERSION;

            [DataMember]
            public string Log { get; set; }

            [DataMember]
            public Dictionary<Guid,DatabaseServiceStatus> ServiceStatuses { get; set; }

            #endregion

            #region TrainingCoordinatorStatus Serialization

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI45808", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                    throw ee;
                }

                Version = CURRENT_VERSION;
            }

            [OnSerializing]
            void OnSerializing(StreamingContext context)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Log))
                    {
                        // Truncate history so that it doesn't go over 1000 actions
                        var actions = Log.Split(new[] { "\r\n\r\n" }, StringSplitOptions.None);
                        if (actions.Length > 1000)
                        {
                            Log = string.Join("\r\n\r\n", actions.Skip(actions.Length - 1000));
                        }
                    }
                }
                catch { }
            }
            #endregion
        }

        internal void AddModels(IEnumerable<string> names)
        {
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                var trans = connection.BeginTransaction();

                try
                {
                    foreach (var modelName in names)
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO MLModel(Name) VALUES(@ModelName)";
                            cmd.Parameters.AddWithValue("@ModelName", modelName);
                            cmd.Transaction = trans;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        trans.Rollback();
                    }
                    catch (Exception rollbackException)
                    {
                        rollbackException.ExtractLog("ELI45811");
                    }
                    throw new ExtractException("ELI45812", "Unable to add model name", ex);
                }
            }
        }

        #endregion
    }
}