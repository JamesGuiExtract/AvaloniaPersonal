namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public class that serves to contain all data for DBAdminTable, 
    /// Table that is created in summary view
    /// </summary>
    [System.Serializable]
    public class DBAdminTable
    {
        //generic constructor
        public DBAdminTable()
        {

        }

        //constructor that initializes all fields with the parameters
        public DBAdminTable(string? action, int unattempted, int pending, int processing, int complete, int skipped, int failed, int total)
        {
            Action = action;
            Unattempted = unattempted;
            Pending = pending;
            Processing = processing;
            Complete = complete;
            Skipped = skipped;
            Failed = failed;
            Total = total;
        }

        //fields 
        public string? Action { get; set; }
        public int Unattempted { get; set; }
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Complete { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public int Total { get; set; }
    }
}
