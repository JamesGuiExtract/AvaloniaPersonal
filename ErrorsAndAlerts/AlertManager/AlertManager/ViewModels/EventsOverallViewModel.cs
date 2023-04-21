using AlertManager.Interfaces;
using AlertManager.Services;
using AlertManager.Views;
using Avalonia.Controls;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlertManager.ViewModels
{
    public class EventsOverallViewModelFactory
    {
        readonly IElasticSearchLayer _elastic;

        public EventsOverallViewModelFactory(IElasticSearchLayer elastic)
        {
            _elastic = elastic;
        }

        public EventsOverallViewModel Create()
        {
            return Create(new EventDto(), new EventsOverallView());
        }

        public EventsOverallViewModel Create(EventDto eventObject)
        {
            return Create(eventObject, new EventsOverallView());
        }

        public EventsOverallViewModel Create(EventDto eventObject, EventsOverallView eventsOverallView)
        {
            return new(_elastic, eventObject, eventsOverallView);
        }
    }

    /// <summary>
    /// This class is responsible for holding data and methods that will be bound to the MoreStatisticsWindow
    /// This data in the class is geared towards presenting data on a specific error as well as giving various options forward
    /// </summary>
    public class EventsOverallViewModel : ReactiveObject
    {
        #region fields
        private readonly EventDto Error = new();

        public EventDto GetEvent { get => Error; }

        IElasticSearchLayer? elasticService;

        public IElasticSearchLayer? GetService { get => elasticService; }

        #endregion fields

        private Window thisWindow;

        [Reactive]
        public string AdditionalInformation { get; set; } = string.Empty;

        #region Reactive UI Binding
        //reactive UI binding
        [Reactive]
        public string? GreetingOpen { get; set; }
        [Reactive]
        public string EventDetails { get; set; } = "";
        [Reactive]
        public string StackTrace { get; set; } = "";

        public ReactiveCommand<EventDto, string> OpenEnvironmentView { get; set; }

        #endregion Reactive UI Binding

        #region constructors

        /// <summary>
        /// constructor, initializes everything in the class, uses dependency injection from above
        /// </summary>
        /// <param name="elasticSearch">Instance of the elastic service singleton</param>
        /// <param name="eventObject">Object to have everything initialized to</param>
        /// <param name="thisWindow">View for the view model</param>
        public EventsOverallViewModel(IElasticSearchLayer? elasticSearch, EventDto eventObject, Window thisWindow)
        {
            this.thisWindow = thisWindow;

            elasticService ??= new ElasticSearchService();

            if (eventObject == null)
            {
                eventObject = new();
                ExtractException ex = new ExtractException("ELI53772", "Issue passing in error object, error object is null");
                RxApp.DefaultExceptionHandler.OnNext(ex);

            }

            this.elasticService = elasticSearch;
            Error = eventObject;
            GreetingOpen = "Events Information";
            if (eventObject.StackTrace != null)
            {
                for (int i = eventObject.StackTrace.Count - 1; i >= 0; i--)
                {
                    if (eventObject.StackTrace.ElementAt(i) != null)
                    {
                        StackTrace += eventObject.StackTrace.ElementAt(i) + "\n";
                    }
                }
            }
            EventDetails = eventObject.Message + "\n";
            if (eventObject.Data != null)
            {
                foreach (KeyValuePair<string, string> d in eventObject.Data)
                {
                    EventDetails += d.Key + ": " + d.Value + "\n";
                }
            }

            AdditionalInformation = CreateAdditionalInformation(Error);

            OpenEnvironmentView = ReactiveCommand.Create<EventDto, string>( x => OpenEnvironmentViewImpl(this.Error));
        }

        #endregion constructors

        #region methods

        /// <summary>
        /// Creates a formatted string containing additional information about an exception event.
        /// </summary>
        /// <param name="exceptionEvent">An instance of the ExceptionEvent class.</param>
        /// <returns>A formatted string containing additional information about the exception event.</returns>
        private static string CreateAdditionalInformation(EventDto exceptionEvent)
        {
            // Initialize the return string as an empty string
            string returnString = "";
            try
            {
                if(exceptionEvent == null)
                {
                    throw new Exception("issue with EventDto");
                }
                returnString = "Stack Trace: " + exceptionEvent.StackTrace ?? "no Stack Trace"
                    + "\nInner Exception: " + exceptionEvent.Inner.ToString() ?? "no inner string" +
                    "\n Message: " + exceptionEvent.Message ?? "no Message";
            }
            catch (Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54256"));
            }
            return returnString;
        }

        private string OpenEnvironmentViewImpl(EventDto error)
        {
            string? result = "";

            EnvironmentInformationViewModel environmentViewModel = new(error, new ElasticSearchService());

            EnvironmentInformationView environmentWindow = new()
            {
                DataContext = (environmentViewModel)
            };

            try
            {
                result = environmentWindow.ShowDialog<string>(thisWindow).ToString();
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI54142", "Issue displaying the events table", e);  
            }

            result ??= "";

            return result;
        }
        #endregion methods
    }
}