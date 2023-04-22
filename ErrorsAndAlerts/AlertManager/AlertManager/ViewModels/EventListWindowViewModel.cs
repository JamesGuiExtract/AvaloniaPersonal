using AlertManager.Interfaces;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;

namespace AlertManager.ViewModels
{
    public class EventListWindowViewModel : ViewModelBase
    {
        [Reactive]
        public EventListViewModel EventList { get; set; }


        public EventListWindowViewModel(
            IWindowService windowService,
            EventsOverallViewModelFactory eventsOverallViewModelFactory,
            IList<EventDto> eventList,
            string eventTitle)
        {
            EventList = new(windowService, eventsOverallViewModelFactory, eventList, eventTitle);
        }
    }
}