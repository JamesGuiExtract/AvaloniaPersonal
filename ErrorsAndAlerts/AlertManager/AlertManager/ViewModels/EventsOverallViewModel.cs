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

        private readonly EventObject Error = new();

        public EventObject GetEvent {get => Error;}

        IDBService? DbService;

        public IDBService? GetService { get => DbService; }

        #endregion fields


        #region Reactive UI Binding
        //reactive UI binding
        [Reactive]
        public string? GreetingOpen { get; set; } 

        [Reactive]
        public DataNeededForPage? UserData { get; set; } 

        [Reactive]
        public List<int>? ButtonIds { get; set; } 

        /// <summary>
        /// Id Number value
        /// </summary>
        [Reactive]
        public int IdNumber { get; set; }

        [Reactive]
        public DateTime DateErrorCreated { get; set; }

        [Reactive]
        public ErrorSeverityEnum EventSeverity { get; set; }

        #endregion Reactive UI Binding

        #region constructors
        //below are the constructors for dependency injection, uses splat reactive UI for dependency inversion
        public EventsOverallViewModel() : this(Locator.Current.GetService<IDBService>(), new EventObject())
        {
        }

        public EventsOverallViewModel(EventObject errorObject) : this(Locator.Current.GetService<IDBService>(), errorObject)
        {
        }

        public EventsOverallViewModel(EventObject errorObject, EventsOverallView thisWindow) : this(Locator.Current.GetService<IDBService>(), errorObject)
        {
        }

        /// <summary>
        /// constructor, initializes everything in the class, uses dependency injection from above
        /// </summary>
        /// <param name="db">IDBService, the backend server class</param>
        /// <param name="errorObject">Object to have everything initialized to</param>
        /// <param name="thisWindow">The window associated with the current data model</param>
        public EventsOverallViewModel(IDBService? databaseService, EventObject errorObject)
        {
            databaseService =  (databaseService == null) ? new DBService() : databaseService;

            if(errorObject == null)
            {
                errorObject = new();
                throw new ExtractException("ELI53772", "Issue passing in error object, error object is null");
            }

            if(databaseService != null)
            {
                DbService = databaseService;
                Error = errorObject;
                SetNewValues(DbService.ReturnFromDatabase(0));
                GreetingOpen = "Error Resolution";
                UserData = new DataNeededForPage();
                IdNumber = UserData.id_Number;
                DateErrorCreated = UserData.date_Error_Created;
                ButtonIds = new List<int>();
                SetNewValues(databaseService.ReturnFromDatabase(0));
                ButtonIds = databaseService.AllIssueIds();
                EventSeverity = UserData.severity_Status;
            }
            

        }
        #endregion constructors

        #region methods

        /// <summary>
        /// This method retrieves the DataNeededForPage from the id number from the database and 
        /// sets the values on the page to retrieved values
        /// </summary>
        /// <param name="itemId">id Number of the </param>
        public void ChangeInterfaceElements(int itemId)
        {
            try
            {
                //todo throw error if incorrect result returned
                if(DbService != null)
                {
                    DataNeededForPage passedData = DbService.ReturnFromDatabase(itemId);
                    SetNewValues(passedData);
                }
                
            }
            catch (Exception e)
            {
                ExtractException ex = new ExtractException("ELI53859", "Issue changing the interface elements, id of element being accessed is " + itemId  , e);
                throw ex;
            }

        }

        /// <summary>
        /// This method changes all the values displayed on the page, the Inotify on 
        /// each of the setters of the methods auto notifies the bound values to update
        /// TODO set this to refresh page...
        /// </summary>
        /// <param name="newData">Type of DataNeededForPage, contains values -
        /// that the page will be updated with</param>
        public void SetNewValues(DataNeededForPage newData)
        {
            IdNumber = newData.id_Number;
            DateErrorCreated = newData.date_Error_Created;
        }

        /// <summary>
        /// This method is bound to view, opens a new window to resolve current ErrorObject
        /// Sets the resolveIssueWindow datacontext to ResolveIssueWindowViewModel
        ///
        /// </summary>
        /// <returns>a string value refresh if successful, "" on issue</returns>
        public string MakeAlert()
        {
            string? result = "";
            MakeAlertView makeAlert = new ();

            try
            {
                MakeAlertViewModel resolveIssueVM = new MakeAlertViewModel(this.Error, DbService);

                makeAlert.DataContext = resolveIssueVM;

                makeAlert.Show();
            }
            catch(Exception e)
            {
                ExtractException ex = new ExtractException("ELI53860", "Issue creating alert ", e);
                throw ex;
            }

            if (result == null)
            {
                return "";
            }

            return result;
        }

        #endregion methods
    }
}