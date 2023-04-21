using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;

namespace AlertManager.ViewModels
{
    public class EventListWindowViewModel : ReactiveObject
	{
        [Reactive]
        public EventListViewModel EventList { get; set; }


        public EventListWindowViewModel(EventsOverallViewModelFactory eventsOverallViewModelFactory, List<EventDto> eventList, string eventTitle)
		{
            EventList = new(eventsOverallViewModelFactory, eventList, eventTitle);
        }
    }
}