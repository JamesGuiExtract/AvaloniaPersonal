using System;
using System.Collections.Generic;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for LogAlert
    /// Data needed to create the log
    /// </summary>
    [Serializable]
    public class LogAlert
    {
        public int Id { get; set; }
        public List<LogError> Errors { get; set; } = new();
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime Created { get; set; }
        public string Status { get; set; } = "";
        public string? Resolution { get; set; }
    }
}
