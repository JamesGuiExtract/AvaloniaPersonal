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

        IAlertStatus? alertStatus;

        public IAlertStatus? GetService { get => alertStatus; }

        #endregion fields


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
        public EventsOverallViewModel() : this(Locator.Current.GetService<IAlertStatus>(), new ExceptionEvent())
        {
        }

        public EventsOverallViewModel(ExceptionEvent errorObject) : this(Locator.Current.GetService<IAlertStatus>(), errorObject)
        {
        }

        public EventsOverallViewModel(ExceptionEvent errorObject, EventsOverallView thisWindow) : this(Locator.Current.GetService<IAlertStatus>(), errorObject)
        {
        }

        /// <summary>
        /// constructor, initializes everything in the class, uses dependency injection from above
        /// </summary>
        /// <param name="errorObject">Object to have everything initialized to</param>
        /// <param name="alertStatus">The interface associated with the current data model</param>
        public EventsOverallViewModel(IAlertStatus? alertStatus, ExceptionEvent errorObject)
        {
            alertStatus ??= new AlertStatusElasticSearch();

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

        #endregion methods
    }
}