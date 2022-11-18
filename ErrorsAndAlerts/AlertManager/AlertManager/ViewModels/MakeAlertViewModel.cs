using AvaloniaDashboard.Interfaces;
using AvaloniaDashboard.Models;
using AvaloniaDashboard.Models.AllDataClasses;
using AvaloniaDashboard.Models.AllEnums;
using AvaloniaDashboard.Services;
using AvaloniaDashboard.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;

namespace AvaloniaDashboard.ViewModels
{
    /// <summary>
    /// This class is responsible for binding methods and fields
    /// to the resolve issue window where a specific errorobject can be resolved
    /// </summary>
    public class MakeAlertViewModel : ReactiveObject
    {
        #region fields
        private MakeAlertView ThisWindow;

        private IDBService db;

        #endregion fields

        #region Binding get and set
        [Reactive]
        public EventObject? ErrorObject { get; set; } 

        [Reactive]
        public string? EliCode { get; set; } 

        [Reactive]
        public string? Message { get; set; } 

        [Reactive]
        public int NumberDebug { get; set; } 

        [Reactive]
        public bool ContainsStackTrace { get; set; }

        [Reactive]
        public string? StackTrace { get; set; } = "";

        [Reactive]
        public DateTime? TimeOfError { get; set; } 

        [Reactive]
        public ErrorSeverityEnum? SeverityOfError { get; set; } 

        [Reactive]
        public string? AdditionalDetails { get; set; }

        [Reactive]
        public MachineAndCustomerInformation? MachineAndCustomerInformation { get; set; } 


        #endregion Binding get and set

        /// <summary>
        /// dependencies
        /// </summary>
        public MakeAlertViewModel() : this(new EventObject(), new MakeAlertView(), Locator.Current.GetService<IDBService>())
        {

        }

        /// <summary>
        /// Dependencies
        /// </summary>
        /// <param name="errorObject"></param>
        public MakeAlertViewModel(EventObject errorObject) : this(errorObject, new MakeAlertView(), Locator.Current.GetService<IDBService>())
        {

        }

        /// <summary>
        /// Dependencies
        /// </summary>
        /// <param name="errorObject"></param>
        public MakeAlertViewModel(EventObject errorObject, IDBService db) : this(errorObject, new(), db)
        {

        }

        /// <summary>
        /// Creates a New AlertObject
        /// </summary>
        /// <param name="errorObject"></param>
        /// <param name="thisWindow"></param>
        public MakeAlertViewModel(EventObject errorObject, MakeAlertView thisWindow, IDBService dbService)
        {
            RefreshScreen(errorObject);

            this.ThisWindow = thisWindow;

            this.db = dbService;
        }


        public void RefreshScreen(EventObject newErrorObject)
        {
            ErrorObject = newErrorObject;
            EliCode = ErrorObject.eliCode;
            Message = ErrorObject.message;
            NumberDebug = ErrorObject.number_Debug;
            ContainsStackTrace = ErrorObject.contains_Stack_Trace;
            StackTrace = (ErrorObject.stack_Trace == null) ? "" : ErrorObject.stack_Trace;
            TimeOfError = ErrorObject.time_Of_Error;
            SeverityOfError = ErrorObject.severity_Of_Error;
            AdditionalDetails = ErrorObject.additional_Details;
            MachineAndCustomerInformation = ErrorObject.machine_And_Customer_Information;
        }

        /// <summary>
        /// Initializes the alert to be entered into json
        /// todo make this based on values entered by user
        /// </summary>
        /// <returns></returns>
        private LogAlert InitializeAlertObject()
        {
            //TODO important, get guid or id from backend
            LogAlert returnAlert = new();

            returnAlert.Id = -1;
            returnAlert.Type = "TestType";
            returnAlert.Title = "testTytle";
            returnAlert.Created = DateTime.Now;
            returnAlert.Status = "testStatus";
            returnAlert.Resolution = "testResolution";

            return returnAlert;
        }

        /// <summary>
        /// sends the created alert from user into the database
        /// </summary>
        public void SendAlertToJSON()
        {
            //README since the logger never changes data, this will always just add to the json file
            try
            {
                LogAlert testAlert = InitializeAlertObject();
                db.AddAlertToDatabase(testAlert);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return;
            }

        }

        /// <summary>
        /// Closes the window, returns "Refresh" upon successful close
        /// </summary>
        private void CloseWindow()
        {
            ThisWindow.Close("Refresh");
        }

    }
}