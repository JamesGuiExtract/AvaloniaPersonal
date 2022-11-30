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
        public LoggingTargetError _source { get; set; } = new();
    }
}
