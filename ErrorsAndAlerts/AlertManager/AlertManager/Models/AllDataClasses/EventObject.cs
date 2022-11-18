using AvaloniaDashboard.Models.AllEnums;
using System;
using System.Windows.Input;

namespace AvaloniaDashboard.Models.AllDataClasses
{
    /// <summary>
    /// This class contains the data for the data object EventObject, which will be used to store all Error data
    /// Will be change to EventObject
    /// </summary>
    //todo make events data structure
    [Serializable]
    public class EventObject
    {
        ///Public constructor that initalizes all public fields
        public EventObject(string EliCode, string Message, int NumberDebug, bool ContainsStackTrace,
            DateTime TimeOfError, ErrorSeverityEnum errorSeverity, string AdditionalDetails,
            MachineAndCustomerInformation MachineAndCustomerInformation, string? StackTrace = null)
        {
            eliCode = EliCode;
            message = Message;
            number_Debug = NumberDebug;
            contains_Stack_Trace = ContainsStackTrace;
            time_Of_Error = TimeOfError;
            severity_Of_Error = errorSeverity;
            stack_Trace = StackTrace;
            additional_Details = AdditionalDetails;
            machine_And_Customer_Information = MachineAndCustomerInformation;

        }

        //generic constructor
        public EventObject()
        {
        }

        public string eliCode { get; set; } = "";
        public string message { get; set; } = "";
        public int number_Debug { get; set; }
        public bool contains_Stack_Trace { get; set; }
        public string? stack_Trace { get; set; }
        public DateTime time_Of_Error { get; set; }
        public ErrorSeverityEnum severity_Of_Error { get; set; }
        public string additional_Details { get; set; } = ""; //contains if auto retried to queue stuff, ect. 
        public MachineAndCustomerInformation machine_And_Customer_Information { get; set; } = new MachineAndCustomerInformation(); //should i just leave this blank?

        public ICommand open_Event_Window { get; set; }
    }
}
