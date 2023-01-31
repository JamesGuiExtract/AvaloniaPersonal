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
        public DataNeededForPage? UserData { get; set; } 

        /// <summary>
        /// Id Number value
        /// </summary>
        [Reactive]
        public string IdNumber { get; set; }

        [Reactive]
        public DateTime DateErrorCreated { get; set; }

        [Reactive]
        public ErrorSeverityEnum EventSeverity { get; set; }

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
        /// <param name="thisWindow">The window associated with the current data model</param>
        public EventsOverallViewModel(IAlertStatus? alertStatus, ExceptionEvent errorObject)
        {
            alertStatus =  (alertStatus == null) ? new AlertStatusElasticSearch() : alertStatus;

            if(errorObject == null)
            {
                errorObject = new();
                throw new ExtractException("ELI53772", "Issue passing in error object, error object is null");
            }

            if(alertStatus != null)
            {
                this.alertStatus = alertStatus;
                Error = errorObject;
                GreetingOpen = "Error Resolution";
                UserData = new DataNeededForPage();
                IdNumber = errorObject.ELICode;
                DateErrorCreated = UserData.date_Error_Created;
                SetNewValues(errorObject);
                EventSeverity = UserData.severity_Status;
            }
            

        }
        #endregion constructors

        #region methods


        /// <summary>
        /// This method changes all the values displayed on the page, the Inotify on 
        /// each of the setters of the methods auto notifies the bound values to update
        /// TODO set this to refresh page...
        /// </summary>
        /// <param name="newData">Type of DataNeededForPage, contains values -
        /// that the page will be updated with</param>
        public void SetNewValues(ExceptionEvent eventObj)
        {
            IdNumber = eventObj.ELICode;
            DateErrorCreated = eventObj.ExceptionTime;
        }


        #endregion methods
    }
}