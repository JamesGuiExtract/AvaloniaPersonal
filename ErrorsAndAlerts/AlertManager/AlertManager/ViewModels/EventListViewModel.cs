using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using AlertManager.Interfaces;
using AlertManager.Services;
using AlertManager.Views;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Linq;
using Newtonsoft.Json.Linq;


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

        [Reactive]
        public ReactiveCommand<string, Unit> RefreshPage { get; set; }

        [Reactive]
        public List<List<ExceptionEvent>> SeperatedEventList { get; set; } = new();

        public int PageCutoffValue = 30;

        [Reactive]
        public string EventTitle { get; set; }

        /// <summary>
        /// loads page from elasticsearch
        /// </summary>
        /// <param name="elastic"></param>
        public EventListViewModel(IElasticSearchLayer? elastic, string eventTitle)
		{   
            this.elasticService = elastic ?? new ElasticSearchService();

            LoadPage = ReactiveCommand.Create<string>(loadPageFromElastic);
            maxPage = this.elasticService.GetMaxEventPages();
            updatePageCounts("first");

            _ErrorAlertsCollection = prepEventList(elasticService.GetAllEvents(page: currentPage - 1));

            RefreshPage = ReactiveCommand.Create<string>(RefreshEventTableFromElastic);

            EventTitle = eventTitle;
        }

        /// <summary>
        /// Loads page from page of events
        /// </summary>
        /// <param name="exceptionEventList"></param>
        public EventListViewModel(List<ExceptionEvent> exceptionEventList, string eventTitle)
        {
            try
            {
                //todo add elastic service in future
                this.exceptionEventList = exceptionEventList ?? new();
                SeperatedEventList = divideIntoPages(this.exceptionEventList);

                maxPage = this.SeperatedEventList.Count;
                updatePageCounts("first");
                LoadPage = ReactiveCommand.Create<string>(loadPageFromList);

                _ErrorAlertsCollection = prepEventList(SeperatedEventList[currentPage - 1]);

                EventTitle = eventTitle;
                RefreshPage = ReactiveCommand.Create<string>(RefreshEventTableFromObject);

            }
            catch (Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e);
            }
        }

        /// <summary>
        /// Refreshes the observable collection bound to the Events table
        /// </summary>
        public void RefreshEventTableFromElastic(string placeholderString)
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
                ExtractException ex = new("ELI54148", "Issue refreshing the Elasticsearch events table, getting information from page 0", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
        }

        /// <summary>
        /// Refreshes the observable collection bound to the Events table
        /// </summary>
        public void RefreshEventTableFromObject(string placeholderString)
        {
            try
            {
                _ErrorAlertsCollection.Clear();
                IList<ExceptionEvent> events = SeperatedEventList[0];
                _ErrorAlertsCollection = prepEventList(events);
                maxPage = SeperatedEventList.Count;
                updatePageCounts("first");
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53872", "Issue refreshing the events table from lists of events", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
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
                ExtractException ex = new("ELI54143", "Issue displaying the the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            if (result == null)
            {
                result = "";
            }

            return result;

        }

        /// <summary>
        /// Helper method to help retrieve events from elastic search
        /// </summary>
        /// <param name="direction">direction of page changes</param>
        /// <returns>Generic List of ExceptionEvents</returns>
        private IList<ExceptionEvent> eventsFromElasticSearch(string direction)
        {
            IList<ExceptionEvent> events = new List<ExceptionEvent>();
            try
            {
                maxPage = elasticService.GetMaxEventPages();
                bool successfulUpdate = updatePageCounts(direction);
                if (!successfulUpdate)
                {
                    ExtractException ex = new ExtractException("ELI53980", "Invalid Page Update Command");
                    throw ex;
                }
                
                events = elasticService.GetAllEvents(page: currentPage - 1);
            }
            catch (Exception e)
            {
                ExtractException ex = new ExtractException("ELI53771", "Error retrieving alerts from logging target", e);
                throw ex;
            }

            return events;
        }

        /// <summary>
        /// Command function run when a user changes what page they are viewing on the alerts table
        /// </summary>
        /// <param name="direction">Command parameter indicating what page to display next</param>
        private void loadPageFromElastic(string direction)
        {
            IList<ExceptionEvent> events = eventsFromElasticSearch(direction);
            _ErrorAlertsCollection.Clear();
            _ErrorAlertsCollection = prepEventList(events);
        }

        /// <summary>
        /// loads page from list of events, not elasticsearch
        /// </summary>
        /// <param name="direction">Direction that the page is moving in</param>
        private void loadPageFromList(string direction)
        {
            try
            {
                bool successfulUpdate = updatePageCounts(direction);
                if (!successfulUpdate)
                {
                    ExtractException ex = new ExtractException("ELI53980", "Invalid Page Update Command");
                    throw ex;
                }

                IList<ExceptionEvent> events = this.SeperatedEventList[currentPage - 1]; //have to subtract 1 due to retrieving from list
                _ErrorAlertsCollection.Clear();
                _ErrorAlertsCollection = prepEventList(events);
            }
            catch(Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e);
            }
        }

        /// <summary>
        /// Helper method that needs to be called upon initialization of page that breaks 
        /// list of events into list of lists that will serve as pages, adjusts global variable 
        /// </summary>
        /// <param name="initialList">List of events to break up</param>
        private List<List<ExceptionEvent>> divideIntoPages(List<ExceptionEvent> initialList)
        {
            try
            {
                List < List < ExceptionEvent >> returnList = 
                    initialList.Select((x, i) => new { Index = i, Value = x })
                        .GroupBy(x => x.Index / PageCutoffValue) 
                        .Select(x => x.Select(v => v.Value).ToList())
                        .ToList();

                if(returnList.Count < 1)
                //since its a list of lists, will throw error if there are no lists in the lists
                //this can happen naturally if there are no attached events (manually created alert)
                //but still want to initialize the table with a empty list if thats the case
                {
                    returnList.Add(new());
                }

                return returnList;
            }
            catch(Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e);
            }

            return new();
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

            try
            {
                foreach (ExceptionEvent e in events)
                {
                    e.Open_Event_Window = ReactiveCommand.Create<int>(x => DisplayEventsWindow(e));
                    eventTable.Add(e);
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new ExtractException("ELI54071", "Error Creating Event List", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
            return eventTable;
        }
    }
}