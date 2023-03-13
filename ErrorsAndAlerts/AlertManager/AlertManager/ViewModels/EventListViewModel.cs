using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
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
        private int currentPage;

        private int maxPage;

        private IElasticSearchLayer elasticService;

        [Reactive]
		public List<ExceptionEvent> exceptionEventList { get; set; } = new();

        [Reactive]
        public ObservableCollection<ExceptionEvent> _ErrorAlertsCollection { get; set; } = new();

        [Reactive]
        public string PageLabel { get; set; } = string.Empty;

        [Reactive]
        public bool PreviousEnabled { get; set; } = false;

        [Reactive]
        public bool NextEnabled { get; set; } = false;

        [Reactive]
        public ReactiveCommand<string, Unit> LoadPage { get; set; }

        //TODO: It is odd that we accept an event list here, but load an event list from LoggingTargetElasticsearch for each page besides the first
        //When we revisit this code for either filtering or associated events, reconsider how page 1 gets communicated
        public EventListViewModel(List<ExceptionEvent> exceptionEventList, IElasticSearchLayer? elastic)
		{
            this.exceptionEventList = exceptionEventList ?? new();
            this.elasticService = elastic ?? new ElasticSearchService();

            LoadPage = ReactiveCommand.Create<string>(loadPage);
            maxPage = this.elasticService.GetMaxEventPages();
            updatePageCounts("first");

            _ErrorAlertsCollection = prepEventList(this.exceptionEventList);
        }

        public EventListViewModel() : this(new(), Locator.Current.GetService<IElasticSearchLayer>())
        {

        }

        public EventListViewModel(List<ExceptionEvent> exceptionEventList) : this(exceptionEventList, Locator.Current.GetService<IElasticSearchLayer>())
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
                IList<ExceptionEvent> events = elasticService.GetAllEvents(page: 0);
                _ErrorAlertsCollection = prepEventList(events);
                maxPage = elasticService.GetMaxEventPages();
                updatePageCounts("first");
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

        /// <summary>
        /// Command function run when a user changes what page they are viewing on the alerts table
        /// </summary>
        /// <param name="direction">Command parameter indicating what page to display next</param>
        private void loadPage(string direction)
        {
            maxPage = elasticService.GetMaxEventPages();
            bool successfulUpdate = updatePageCounts(direction);
            if (!successfulUpdate)
            {
                new ExtractException("ELI53980", "Invalid Page Update Command").Log();
                return;
            }
            IList<ExceptionEvent> events = new List<ExceptionEvent>();
            try
            {
                events = elasticService.GetAllEvents(page: currentPage - 1);
            }
            catch (Exception e)
            {
                new ExtractException("ELI53771", "Error retrieving alerts from logging target", e).Log();
            }
            _ErrorAlertsCollection.Clear();
            _ErrorAlertsCollection = prepEventList(events);
        }

        /// <summary>
        /// Updates the appropriate page count fields based on the user-entered direction
        /// </summary>
        /// <param name="direction">user entered direction</param>
        /// <returns>true if valid updates were made, false otherwise</returns>
        private bool updatePageCounts(string direction)
        {
            switch (direction)
            {
                case "first":
                    currentPage = 1;
                    if (maxPage > currentPage) NextEnabled = true;
                    PreviousEnabled = false;
                    break;
                case "previous":
                    if (currentPage > 1) currentPage -= 1;
                    if (maxPage > currentPage) NextEnabled = true;
                    if (currentPage == 1) PreviousEnabled = false;
                    break;
                case "next":
                    if (currentPage < maxPage) currentPage += 1;
                    if (currentPage == maxPage) NextEnabled = false;
                    if (currentPage > 1) PreviousEnabled = true;
                    break;
                case "last":
                    currentPage = maxPage;
                    NextEnabled = false;
                    if (currentPage > 1) PreviousEnabled = true;
                    break;
                default:
                    return false;
            }
            PageLabel = $"Page {currentPage} of {maxPage}";
            return true;
        }

        /// <summary>
        /// Take a list of event objects and converts them into an Observable collection with the appropriate commands instantiated
        /// </summary>
        /// <param name="events">Incoming list of events</param>
        /// <returns>ObservableCollection of alerts, each with Open_Event_Window populated</returns>
        private ObservableCollection<ExceptionEvent> prepEventList(IList<ExceptionEvent> events)
        {
            ObservableCollection<ExceptionEvent> eventTable = new ObservableCollection<ExceptionEvent>();
            foreach (ExceptionEvent e in events)
            {
                e.Open_Event_Window = ReactiveCommand.Create<int>(x => DisplayEventsWindow(e));
                eventTable.Add(e);
            }
            return eventTable;
        }
    }
}