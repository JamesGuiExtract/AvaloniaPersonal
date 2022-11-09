namespace AvaloniaDashboard.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for FAMAction
    /// Contains just the Id and Name
    /// </summary>
    [System.Serializable]
    public class FAMAction
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
