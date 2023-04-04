using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using DynamicData.Kernel;
using Extract.ErrorHandling;
using Nest;
using ReactiveUI;
using Splat;

namespace AlertManager.ViewModels
{
    public class EnvironmentInformationViewModel : ReactiveObject
    {
        public readonly record struct EnvironmentDataGridRow(DateTime Time, string Context, string Type, string Entity, string Data);

        public ObservableCollection<EnvironmentDataGridRow> EnvironmentInfos { get; } = new();

        private IElasticSearchLayer searchService;

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
        public EnvironmentInformationViewModel(ExceptionEvent eEvent, IElasticSearchLayer? elasticClient)
        {
            searchService = elasticClient 
                ?? (ElasticSearchService)Locator.Current.GetService<IElasticSearchLayer>() 
                ?? new();

            PopulateFromEvent(eEvent);
        }

        /// <summary>
        /// Constructor built from an alert, which contains a list of error events.
        /// Snapshot will be populated from each event contained by the alert.
        /// The snapshot will remove duplicate entries.
        /// </summary>
        /// <param name="alert">Alert to display environment information for</param>
        public EnvironmentInformationViewModel(AlertsObject alert, IElasticSearchLayer? elasticClient)
        {
            searchService = elasticClient 
                ?? (ElasticSearchService)Locator.Current.GetService<IElasticSearchLayer>() 
                ?? new();

            if (alert.AssociatedEvents != null)
            {
                foreach (var eEvent in alert.AssociatedEvents)
                { 
                    PopulateFromEvent(eEvent);
                }

                //Removing duplicates
                ObservableCollection<EnvironmentDataGridRow> newEnvironmentInfos = new();
                foreach (var gridRow in EnvironmentInfos.Distinct())
                { 
                    newEnvironmentInfos.Add(gridRow);
                }
                EnvironmentInfos = newEnvironmentInfos;
            }
        }

        /// <summary>
        /// Checks each relevant value in the event's context object, and populates snapshot
        /// based on those values.
        /// </summary>
        /// <param name="eEvent">Event to have snapshot data populated for.</param>
        private void PopulateFromEvent(ExceptionEvent eEvent)
        {
            ContextInfo eventContext = eEvent.Context;

            if (eventContext == null)
                return;

            if (eventContext.DatabaseServer != null)
            {
                PopulateSnapshot(eEvent.ExceptionTime, "DB", eventContext.DatabaseServer);
            }
            if (eventContext.MachineName != null)
            {
                PopulateSnapshot(eEvent.ExceptionTime, "Machine", eventContext.MachineName);
            }
            if (eventContext.FpsContext != null)
            {
                PopulateSnapshot(eEvent.ExceptionTime, "FPS", eventContext.FpsContext);
            }
        }

        /// <summary>
        /// Queries using an elastic search client to retrieve and display data.
        /// </summary>
        /// <param name="upToTime">Time of alert or event. Snapshot data will be retrieved from documents recently before.</param>
        void PopulateSnapshot(DateTime upToTime, string contextType, string entityName)
        {
            var environments = searchService.GetEnvInfoWithContextAndEntity(upToTime, contextType, entityName);
            DisplayEventList(environments);
        }

        /// <summary>
        /// Adds a list of environments to be displayed in the view.
        /// </summary>
        /// <param name="envList">List of environments to be displayed.</param>
        void DisplayEventList(List<EnvironmentInformation> envList)
        {
            foreach (var env in envList)
            {
                string envData = "";
                foreach (var key in env.Data.Keys)
                {
                    envData += key + ": " + env.Data[key] + "\n";
                }
                this.EnvironmentInfos.Add(new(
                    env.CollectionTime, env.Context, env.MeasurementType, env.Entity, envData));
            }
        }

        /// <summary>
        /// Gets a list of all contexts associated with a list of environment objects.
        /// </summary>
        /// <param name="envList">List of environment objects to collect contexts from.</param>
        /// <returns>List of context names.</returns>
        private List<string> GetContexts(List<EnvironmentInformation> envList)
        {
            List<string> contexts = new();

            foreach (EnvironmentInformation environment in envList)
            {
                contexts.Add(environment.Context);
            }

            return contexts;
        }

        /// <summary>
        /// Gets a list of all keys associated with a list of environment objects.
        /// </summary>
        /// <param name="envList">List of environment objects to collect keys from.</param>
        /// <returns>List of key names.</returns>
        private List<string> GetKeys(List<EnvironmentInformation> envList)
        {
            List<string> keys = new();

            foreach (EnvironmentInformation environment in envList)
            {
                keys.AddRange(environment.Data.Keys.AsList());
            }

            return keys;
        }
    }

    
}