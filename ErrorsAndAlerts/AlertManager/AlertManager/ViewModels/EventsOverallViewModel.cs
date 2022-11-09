using Avalonia;
using AvaloniaDashboard.Interfaces;
using AvaloniaDashboard.Models.AllDataClasses;
using AvaloniaDashboard.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;

namespace AvaloniaDashboard.ViewModels
{
    /// <summary>
    /// This class is responsible for holding data and methods that will be bound to the MoreStatisticsWindow
    /// This data in the class is geared towards presenting data on a specific error as well as giving various options forward
    /// </summary>
    public class EventsOverallViewModel : ReactiveObject
    {
        #region fields

        private readonly EventObject Error = new();

        private readonly EventsOverallView ThisWindow = new(); 

        IDBService? DbService;

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

        #endregion Reactive UI Binding

        #region constructors
        //below are the constructors for dependency injection, uses splat reactive UI for dependency inversion
        public EventsOverallViewModel() : this(Locator.Current.GetService<IDBService>(), new EventObject(), new EventsOverallView())
        {
        }

        public EventsOverallViewModel(EventObject errorObject) : this(Locator.Current.GetService<IDBService>(), errorObject, new EventsOverallView())
        {
        }

        public EventsOverallViewModel(EventObject errorObject, EventsOverallView thisWindow) : this(Locator.Current.GetService<IDBService>(), errorObject, thisWindow)
        {
        }

        /// <summary>
        /// constructor, initializes everything in the class, uses dependency injection from above
        /// </summary>
        /// <param name="db">IDBService, the backend server class</param>
        /// <param name="errorObject">Object to have everything initialized to</param>
        /// <param name="thisWindow">The window associated with the current data model</param>
        public EventsOverallViewModel(IDBService? db, EventObject errorObject, EventsOverallView thisWindow)
        {
            if(db != null)
            {
                DbService = db;
                Error = errorObject;
                SetNewValues(DbService.ReturnFromDatabase(0));
                GreetingOpen = "Error Resolution";
                UserData = new DataNeededForPage();
                IdNumber = UserData.id_Number;
                DateErrorCreated = UserData.date_Error_Created;
                ButtonIds = new List<int>();
                SetNewValues(db.ReturnFromDatabase(0));
                ButtonIds = db.AllIssueIds();

                this.ThisWindow = thisWindow;
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
                Console.WriteLine(e.StackTrace);
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
        /// todo open window as a dialog box
        /// </summary>
        public void MakeAlert()
        {
            MakeAlertView makeAlert = new ();
            MakeAlertViewModel resolveIssueVM = new MakeAlertViewModel(this.Error, makeAlert);

            makeAlert.DataContext = resolveIssueVM;
            makeAlert.ShowDialog(ThisWindow);

        }

        //TODO show and close window as a dialog
        /// <summary>
        /// Closes the window
        /// </summary>
        private void CloseWindow()
        {
            ThisWindow.Close("Refresh");
        }

        //TODO 
        /// <summary>
        /// This method is bound to button in view, changes the page to reflect a the next error object data 
        /// </summary>
        private void NextItem()
        {

        }

        //TODO 
        /// <summary>
        /// This method is bound to button in view, changes the page to reflect a the previous error object data 
        /// </summary>
        private void PreviousItem()
        {

        }
        #endregion methods
    }
}