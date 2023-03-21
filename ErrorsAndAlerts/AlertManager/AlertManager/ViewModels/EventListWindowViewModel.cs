using System;
using System.Collections.Generic;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Views;
using Avalonia.Controls;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace AlertManager.ViewModels
{
	public class EventListWindowViewModel : ReactiveObject
	{
        [Reactive]
        public UserControl EventListUserControl { get; set; } = new();


        public EventListWindowViewModel(List<ExceptionEvent> eventList, string eventTitle)
		{
            EventListViewModel eventViewModel = new(eventList, eventTitle);
            this.EventListUserControl = new EventListUserControl()
            {
                DataContext = eventViewModel
            };
        }

        public EventListWindowViewModel(IElasticSearchLayer? elasticSearch, string eventTitle)
        {
            EventListViewModel eventViewModel = new(elasticSearch, eventTitle);
            this.EventListUserControl = new EventListUserControl()
            {
                DataContext = eventViewModel
            };
        }
    }
}