using System;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for ErrorDetails
    /// Used to initialize errors
    /// </summary>
    [System.Serializable]
    public class LoggingTargetAlert
    {
        public string hits { get; set; } = "";
        public string query { get; set; } = "";
        public string name { get; set; } = "";
    }
}
