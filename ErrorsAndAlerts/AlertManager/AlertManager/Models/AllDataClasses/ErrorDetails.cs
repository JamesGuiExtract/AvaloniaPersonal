using System.Collections.Generic;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for ErrorDetails
    /// Used to initialize errors
    /// </summary>
    [System.Serializable]
    public class ErrorDetails
    {
        public string ClassName { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Data { get; set; } //generic data about the error, user comments are included here
        public ErrorDetails InnerException { get; set; } = new();
        public string? HelpURL { get; set; }
        public string? StackTraceString { get; set; }
        public string? RemoteStackTraceString { get; set; }
        public int RemoteStackIndex { get; set; }
        public string? ExceptionMethod { get; set; }
        public int HResult { get; set; } //Completion status 
        public string? Source { get; set; }
        public string? WatsonBuckets { get; set; } //A type of crash reporting bucket, such as Module Version, Module build date, ect
        //List of class exception data
        public List<ExceptionData> ExceptionData { get; set; } = new();
        public int Version { get; set; } //version of program being run
        public string ELICode { get; set; } = "";
        public List<string> StackTraceValues { get; set; } = new();
    }
}
