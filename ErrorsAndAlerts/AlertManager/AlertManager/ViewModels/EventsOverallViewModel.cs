using AlertManager.Services;
using System.Collections;
using System.Linq;
using Avalonia.Controls;
using AlertManager.Interfaces;
using AlertManager.Views;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using Splat;
using System;
using Extract.ErrorHandling;

namespace AlertManager.ViewModels
{
    /// <summary>
    /// This class is responsible for holding data and methods that will be bound to the MoreStatisticsWindow
    /// This data in the class is geared towards presenting data on a specific error as well as giving various options forward
    /// </summary>
    public class EventsOverallViewModel : ReactiveObject
    {
        #region fields
        private readonly ExceptionEvent Error = new();

        public ExceptionEvent GetEvent { get => Error; }

        IElasticSearchLayer? elasticService;

        public IElasticSearchLayer? GetService { get => elasticService; }

        #endregion fields

        private Window thisWindow;

        #region Reactive UI Binding
        //reactive UI binding
        [Reactive]
        public string? GreetingOpen { get; set; }
        [Reactive]
        public string EventDetails { get; set; } = "";
        [Reactive]
        public string StackTrace { get; set; } = "";

        public ReactiveCommand<ExceptionEvent, string> OpenEnvironmentView { get; set; }

        #endregion Reactive UI Binding

        #region constructors
        //below are the constructors for dependency injection, uses splat reactive UI for dependency inversion
        public EventsOverallViewModel() : this(Locator.Current.GetService<IElasticSearchLayer>(), new ExceptionEvent(), new EventsOverallView())
        {
        }

        public EventsOverallViewModel(ExceptionEvent errorObject) : this(Locator.Current.GetService<IElasticSearchLayer>(), errorObject, new EventsOverallView())
        {
        }

        public EventsOverallViewModel(ExceptionEvent errorObject, EventsOverallView thisWindow) : this(Locator.Current.GetService<IElasticSearchLayer>(), errorObject, thisWindow)
        {
        }

        /// <summary>
        /// constructor, initializes everything in the class, uses dependency injection from above
        /// </summary>
        /// <param name="elasticSearch">Instance of the elastic service singleton</param>
        /// <param name="errorObject">Object to have everything initialized to</param>
        /// <param name="thisWindow">View for the view model</param>
        public EventsOverallViewModel(IElasticSearchLayer? elasticSearch, ExceptionEvent errorObject, Window thisWindow)
        {
            this.thisWindow = thisWindow;

            elasticService ??= new ElasticSearchService();

            if (errorObject == null)
            {
                errorObject = new();
                ExtractException ex = new ExtractException("ELI53772", "Issue passing in error object, error object is null");
                RxApp.DefaultExceptionHandler.OnNext(ex);

            }

            this.elasticService = elasticSearch;
            Error = errorObject;
            GreetingOpen = "Events Information";
            if (errorObject.StackTrace != null)
            {
                for (int i = errorObject.StackTrace.Count - 1; i >= 0; i--)
                {
                    if (errorObject.StackTrace.ElementAt(i) != null)
                    {
                        StackTrace += errorObject.StackTrace.ElementAt(i) + "\n";
                    }
                }
            }
            EventDetails = errorObject.Message + "\n";
            if (errorObject.Data != null)
            {
                foreach (DictionaryEntry d in errorObject.Data)
                {
                    EventDetails += d.Key + ": " + d.Value + "\n";
                }
            }

            OpenEnvironmentView = ReactiveCommand.Create<ExceptionEvent, string>( x => OpenEnvironmentViewImpl(this.Error));

        }

        #endregion constructors

        #region methods
        private string OpenEnvironmentViewImpl(ExceptionEvent error)
        {
            string? result = "";

            EnvironmentInformationViewModel environmentViewModel = new(error, null);

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