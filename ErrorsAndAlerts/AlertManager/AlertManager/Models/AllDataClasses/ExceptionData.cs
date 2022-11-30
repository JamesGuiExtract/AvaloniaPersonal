namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// This class holds data for a key value pair 
    /// </summary>
    [System.Serializable]
    public class ExceptionData
    {
        public string Key { get; set; } = "";
        public object? Value { get; set; } 
    }
}
