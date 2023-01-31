using Extract.ErrorHandling;
using System;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for ErrorDetails
    /// Used to initialize errors
    /// </summary>
    [System.Serializable]
    public class LogIndexObject
    {
        public ExceptionEvent _source { get; set; } = new();
    }
}
