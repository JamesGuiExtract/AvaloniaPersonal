namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// This class serves as a data structure that contains the specific information on computer performance/metrics
    /// Contains information such as cpu, memory percentage, and page erred
    /// </summary>
    [System.Serializable]
    public class UserMetrics
    {
        //constructor that initializes fields with inputted parameters
        public UserMetrics(float userCPUPercent, float userMemoryPercent, string? pageErroredOn)
        {
            this.userCPUPercent = userCPUPercent;
            this.userMemoryPercent = userMemoryPercent;
            this.pageErroredOn = pageErroredOn;
        }

        //generic constructor
        public UserMetrics()
        {

        }

        //public fields
        public float userCPUPercent;
        public float userMemoryPercent;
        public string? pageErroredOn;
    }
}
