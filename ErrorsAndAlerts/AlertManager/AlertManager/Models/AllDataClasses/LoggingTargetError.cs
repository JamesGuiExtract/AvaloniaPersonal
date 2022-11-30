using System;
using System.Collections.Generic;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for ErrorDetails
    /// Used to initialize errors
    /// </summary>
    [System.Serializable]
    public class LoggingTargetError
    {
        public string id { get; set; } = "";
        public string eliCode { get; set; } = "";
        public string message { get; set; } = "";
        public string stackTrace { get; set; } = "";
        public DateTime exceptionTime { get; set; } = new DateTime();
        public ApplicationState applicationState { get; set; } = new();
        public List<ExceptionData> data { get; set; } = new List<ExceptionData>();
    }
}
