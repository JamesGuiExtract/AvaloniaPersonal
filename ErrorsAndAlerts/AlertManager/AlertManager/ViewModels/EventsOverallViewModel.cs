using Avalonia;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using Extract.ErrorHandling;
using AlertManager.Services;
using System.Collections;
using System.Linq;
using Avalonia.Controls;

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

        public ExceptionEvent GetEvent {get => Error;}

        ILoggingTarget? alertStatus;

        public ILoggingTarget? GetService { get => alertStatus; }

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

        #endregion Reactive UI Binding

        #region constructors
        //below are the constructors for dependency injection, uses splat reactive UI for dependency inversion
        public EventsOverallViewModel() : this(Locator.Current.GetService<ILoggingTarget>(), new ExceptionEvent(), new EventsOverallView())
        {
        }

        public EventsOverallViewModel(ExceptionEvent errorObject) : this(Locator.Current.GetService<ILoggingTarget>(), errorObject, new EventsOverallView())
        {
        }

        public EventsOverallViewModel(ExceptionEvent errorObject, EventsOverallView thisWindow) : this(Locator.Current.GetService<ILoggingTarget>(), errorObject, thisWindow)
        {
        }

        /// <summary>
        /// constructor, initializes everything in the class, uses dependency injection from above
        /// </summary>
        /// <param name="errorObject">Object to have everything initialized to</param>
        /// <param name="alertStatus">The interface associated with the current data model</param>
        public EventsOverallViewModel(ILoggingTarget? alertStatus, ExceptionEvent errorObject, Window thisWindow)
        {
            this.thisWindow = thisWindow;

            alertStatus ??= new LoggingTargetElasticsearch();

            if(errorObject == null)
            {
                errorObject = new();
                new ExtractException("ELI53772", "Issue passing in error object, error object is null").Log();
            }


            this.alertStatus = alertStatus;
            Error = errorObject;
            GreetingOpen = "Error Resolution";

            if(errorObject.StackTrace != null)
            {
                for (int i = errorObject.StackTrace.Count; i > 0; i--)
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
            
            

        }
        #endregion constructors

        #region methods
        public string OpenEnvironmentView()
        {
            string? result = "";

            EnvironnmentInformationViewModel environmentViewModel = new();

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
                ExtractException ex = new("ELI53874", "Issue displaying the events table", e);
                ex.Log();
            }

            result ??= "";

            return result;
        }
        #endregion methods
    }
}