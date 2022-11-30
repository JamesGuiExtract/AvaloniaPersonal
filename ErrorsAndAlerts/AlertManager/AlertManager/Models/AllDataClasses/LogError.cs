using System;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for LogError
    /// Part of the data needed for ErrorObject
    /// </summary>
    [Serializable]
    public class LogError
    {
        public int Id { get; set; }
        public DateTime DateOccurred { get; set; }
        public string Type { get; set; } = "";
        public string ObjectType { get; set; } = "";
        public object ErrorDetails { get; set; } = new();
    }
}
