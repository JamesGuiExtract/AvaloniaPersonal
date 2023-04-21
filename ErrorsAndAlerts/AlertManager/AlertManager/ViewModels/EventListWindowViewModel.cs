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


        public EventListWindowViewModel(EventsOverallViewModelFactory eventsOverallViewModelFactory, List<EventDto> eventList, string eventTitle)
		{
            EventListViewModel eventViewModel = new(eventsOverallViewModelFactory, eventList, eventTitle);
            EventListUserControl = new EventListUserControl()
            {
                DataContext = eventViewModel
            };
        }
    }
}