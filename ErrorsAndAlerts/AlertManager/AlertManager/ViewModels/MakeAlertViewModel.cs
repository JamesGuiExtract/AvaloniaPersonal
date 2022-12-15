using AlertManager.Interfaces;
using AlertManager.Models;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.Views;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;

namespace AlertManager.ViewModels
{
    /// <summary>
    /// This class is responsible for binding methods and fields
    /// to the resolve issue window where a specific errorobject can be resolved
    /// </summary>
    public class MakeAlertViewModel : ReactiveObject
    {
        #region fields
        private IDBService dbService;

        public IDBService GetDB { get => dbService; }
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
        public MakeAlertViewModel() : this(new EventObject(), Locator.Current.GetService<IDBService>())
        {

        }

        /// <summary>
        /// Dependencies
        /// </summary>
        /// <param name="errorObject"></param>
        public MakeAlertViewModel(EventObject errorObject) : this(errorObject ,Locator.Current.GetService<IDBService>())
        {

        }

        /// <summary>
        /// Creates a New AlertObject
        /// </summary>
        /// <param name="errorObject"></param>
        /// <param name="thisWindow"></param>
        public MakeAlertViewModel(EventObject? errorObject, IDBService? dbService)
        {
            try
            {
                if (errorObject == null)
                {
                    errorObject = new();
                    throw new ExtractException("ELI53773", "Issue passing in error object, error object is null");
                }

                RefreshScreen(errorObject);


                if (dbService != null)
                {
                    this.dbService = dbService;
                }
                else
                {
                    dbService = new DBService();
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53861", "Issue creating new viewmodel for alerts", e);
            }
        }


        public void RefreshScreen(EventObject newErrorObject)
        {
            try
            {
                if(newErrorObject == null)
                {
                    throw new ExtractException("ELI53863", "Issue passing in object, null object");
                }

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
            catch(Exception e)
            {
                ExtractException ex = new("ELI53865", "Issue refreshing screen", e);
            }
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
                dbService.AddAlertToDatabase(testAlert);
            }
            catch(Exception e)
            {
                throw new ExtractException("ELI53866","Issue sending alert to JSON", e);
            }

        }


    }
}