using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using Avalonia.Collections;
using DynamicData.Kernel;
using Extract.ErrorHandling;
using ReactiveUI;
using Splat;

namespace AlertManager.ViewModels
{
    public class EnvironnmentInformationViewModel : ReactiveObject
    {
        public readonly record struct EnvironmentDataGridRow(DateTime Time, string Context, string Type, string Entity, string Data);

        public ObservableCollection<EnvironmentDataGridRow> Environments { get; } = new();

        //these controls will eventually be used for filtering functionality
        //null for the Avalonia date/time controls indicates that the user hasn't selected
        private DateTimeOffset? _startDate = null;
        public DateTimeOffset? StartDate
        {
            get => _startDate == null ? null : _startDate.Value.Date;
            set => this.RaiseAndSetIfChanged(ref _startDate, value);
        }

        private DateTimeOffset? _endDate = null;
        public DateTimeOffset? EndDate
        {
            get => _endDate == null ? null : _endDate.Value.Date;
            set => this.RaiseAndSetIfChanged(ref _endDate, value);
        }

        private TimeSpan? _startTime = null;
        public TimeSpan? StartTime
        {
            get => _startTime == null ? null : _startTime.Value;
            set => this.RaiseAndSetIfChanged(ref _startTime, value);
        }

        private TimeSpan? _endTime = null;
        public TimeSpan? EndTime
        {
            get => _endTime == null ? null : _endTime.Value;
            set => this.RaiseAndSetIfChanged(ref _endTime, value);
        }

        /// <summary>
        /// Constructor built from a single error event.
        /// </summary>
        /// <param name="error">Error to display environment information for</param>
        public EnvironnmentInformationViewModel(ExceptionEvent error)
        {
            PopulateSnapshot(error.ExceptionTime);
            string data = "";
            data += "testKey1: testVal1\n";
            data += "testKey2: testVal2\n";
            data += "testKey3: testVal3\n";

            this.Environments.Add(new(new(), "testContext1", "testType1", "testEntity1", data));
            this.Environments.Add(new(new(), "testContext2", "testType2", "testEntity2", data));
            this.Environments.Add(new(new(), "testContext3", "testType3", "testEntity3", data));
            this.Environments.Add(new(new(), "testContext4", "testType4", "testEntity4", data));
        }

        /// <summary>
        /// Constructor built from an alert, which contains a list of error events.
        /// </summary>
        /// <param name="alert">Alert to display environment information for</param>
        public EnvironnmentInformationViewModel(AlertsObject alert)
        {
            PopulateSnapshot(alert.ActivationTime);
            string data = "";
            data += "testKey1: testVal1\n";
            data += "testKey2: testVal2\n";
            data += "testKey3: testVal3\n";

            this.Environments.Add(new(new(), "testContext1", "testType1", "testEntity1", data));
            this.Environments.Add(new(new(), "testContext2", "testType2", "testEntity2", data));
            this.Environments.Add(new(new(), "testContext3", "testType3", "testEntity3", data));
            this.Environments.Add(new(new(), "testContext4", "testType4", "testEntity4", data));
        }

        /// <summary>
        /// Queries using an elastic search client to retrieve and display data.
        /// </summary>
        /// <param name="upToTime">Time of alert or event. Snapshot data will be retrieved from documents recently before.</param>
        void PopulateSnapshot(DateTime upToTime)
        {
            ElasticSearchService searchClient = (ElasticSearchService)Locator.Current.GetService<IElasticSearchLayer>();
            searchClient ??= new();

            var serviceEnv = searchClient.TryGetInfoWithDataEntry(upToTime, "name");
            if (serviceEnv.Count > 0)
            {
                var service = serviceEnv[0];
                string serviceData = "";
                foreach (var key in service.Data.Keys)
                { 
                    serviceData += key + ": " + service.Data[key] + "\n";
                }
                this.Environments.Add(new(
                    service.CollectionTime, service.Context, service.MeasurementType, service.Entity, serviceData));
            }
            var queueEnv = searchClient.TryGetInfoWithDataEntry(upToTime, "completedocuments");
            if (queueEnv.Count > 0)
            {
                var queue = queueEnv[0];
                string queueData = "";
                foreach (var key in queue.Data.Keys)
                {
                    queueData += key + ": " + queue.Data[key] + "\n";
                }
                this.Environments.Add(new(
                    queue.CollectionTime, queue.Context, queue.MeasurementType, queue.Entity, queueData));
            }
        }

        /// <summary>
        /// Gets a list of all contexts associated with a list of environment objects.
        /// </summary>
        /// <param name="envObjects">List of environment objects to collect contexts from.</param>
        /// <returns>List of context names.</returns>
        private List<string> GetContexts(List<EnvironmentInformation> envObjects)
        {
            List<string> contexts = new();

            foreach (EnvironmentInformation environment in envObjects)
            {
                contexts.Add(environment.Context);
            }

            return contexts;
        }

        /// <summary>
        /// Gets a list of all keys associated with a list of environment objects.
        /// </summary>
        /// <param name="envObjects">List of environment objects to collect keys from.</param>
        /// <returns>List of key names.</returns>
        private List<string> GetKeys(List<EnvironmentInformation> envObjects)
        {
            List<string> keys = new();

            foreach (EnvironmentInformation environment in envObjects)
            {
                keys.AddRange(environment.Data.Keys.AsList());
            }

            return keys;
        }
    }

    
}