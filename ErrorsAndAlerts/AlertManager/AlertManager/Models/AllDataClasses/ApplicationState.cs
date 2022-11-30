using System;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for LogError
    /// Part of the data needed for ErrorObject
    /// </summary>
    [Serializable]
    public class ApplicationState
    {
        public string applicationName { get; set; } = "";
        public string applicationVersion { get; set; } = "";
        public string computerName { get; set; } = "";
        public string userName { get; set; } = "";
        public int pid { get; set; } = -1;
    }
}
