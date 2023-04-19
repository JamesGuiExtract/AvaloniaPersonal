using AlertManager.Interfaces;
using AlertManager.Views;
using Avalonia.Controls;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;

namespace AlertManager.ViewModels
{
    public class EventListWindowViewModel : ReactiveObject
	{
        [Reactive]
        public UserControl EventListUserControl { get; set; } = new();


        public EventListWindowViewModel(List<EventDto> eventList, string eventTitle)
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