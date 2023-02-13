using System;
using System.Collections.Generic;
using AlertManager.Views;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AlertManager.ViewModels
{
    public class EnvironnmentInformationViewModel : ReactiveObject
    {
        //Properties of top controls
        [Reactive]
        public string CustomerNameText { get; set; } = string.Empty;
        [Reactive]
        public string[] ContextTypesArray { get; set; } = new string[0];
        [Reactive]
        public string[] KeysArray { get; set; } = new string[0];
        [Reactive]
        public string SearchText { get; set; } = string.Empty;

        //Properties of middle controls
        [Reactive]
        public AvaloniaDictionary<string, string> Data { get; set; } = new();

        //Properties of bottom controls
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

        public EnvironnmentInformationViewModel()
		{
            //Load an ElasticSearch query
            //Set Customer
            //Collect and set Contexts
            //Collect and set Keys
            //Collect and set Data
            //Apply filters to Data
        }
	}
}