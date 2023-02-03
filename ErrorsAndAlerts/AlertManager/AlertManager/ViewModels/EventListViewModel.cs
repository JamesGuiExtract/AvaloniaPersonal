using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlertManager.Interfaces;
using AlertManager.Services;
using AlertManager.Views;
using Avalonia.Controls;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;


namespace AlertManager.ViewModels
{
	public class EventListViewModel : ReactiveObject
	{
	    [Reactive]
		public List<ExceptionEvent> exceptionEventList { get; set; } = new();

        [Reactive]
        public ObservableCollection<ExceptionEvent> _ErrorAlertsCollection { get; set; } = new();


        public IAlertStatus loggingTarget;

        public EventListViewModel(List<ExceptionEvent> exceptionEventList, IAlertStatus? loggingTarget)
		{
			this.exceptionEventList = (exceptionEventList == null) ? new() : exceptionEventList;
            this.loggingTarget = (loggingTarget == null) ? new AlertStatusElasticSearch() : loggingTarget;

            foreach (ExceptionEvent e in this.exceptionEventList)
            {
                e.Open_Event_Window = ReactiveCommand.Create<int>(x => DisplayEventsWindow(e));
                _ErrorAlertsCollection.Add(e);
            }
        }

        public EventListViewModel() : this(new(), Locator.Current.GetService<IAlertStatus>())
        {

        }

        public EventListViewModel(List<ExceptionEvent> exceptionEventList) : this(exceptionEventList, Locator.Current.GetService<IAlertStatus>())
        {

        }



        /// <summary>
        /// Refreshes the observable collection bound to the Events table
        /// </summary>
        public void RefreshEventTable()
        {
            try
            {
                _ErrorAlertsCollection.Clear();
                IList<ExceptionEvent> events = loggingTarget.GetAllEvents(page: 0);

                foreach (ExceptionEvent e in events)
                {
                    e.Open_Event_Window = ReactiveCommand.Create<int>(x => DisplayEventsWindow(e));
                    _ErrorAlertsCollection.Add(e);
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53872", "Issue refreshing the events table, getting information from page 0", e);
                ex.Log();
            }

        }


        /// <summary>
        /// This method creates a new window from data from the database (_dbService)
        /// Initalizes the window with the instance of current database being used
        /// <paramref name="errorObject"/>
        /// </param>
        /// </summary>
        public string DisplayEventsWindow(ExceptionEvent errorObject)
        {
            string? result = "";

            EventsOverallView eventsWindow = new();
            EventsOverallViewModel eventsWindowView = new EventsOverallViewModel(errorObject, eventsWindow);
            eventsWindow.DataContext = eventsWindowView;

            try
            {
                 eventsWindow.Show();
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53874", "Issue displaying the the events table", e);
                ex.Log();
            }

            if (result == null)
            {
                result = "";
            }

            return result;

        }


    }
}