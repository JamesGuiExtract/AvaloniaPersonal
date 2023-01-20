namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// Class that serves as a data structure that contains the information of the customer and machine data
    /// Used as part of the ErrorObject class
    /// </summary>
    [System.Serializable]
    public class MachineAndCustomerInformation
    {
        //generic constructor
        public MachineAndCustomerInformation()
        {

        }

        //constructor that initializes all data structures below
        public MachineAndCustomerInformation(string OperatingSystem, string CurrentLicense, string UserName, int CoreCount, float MemoryUsage, float DataBaseUsage, float CPUUsage)
        {
            operating_System = OperatingSystem;
            current_License = CurrentLicense;
            user_Name = UserName;
            core_Count = CoreCount;
            memory_Usage = MemoryUsage;
            dataBase_Usage = DataBaseUsage;
            cpu_Usage = CPUUsage;
        }

        //fields
        //README should I make this a enum?
        public string operating_System { get; set; } = ""; 
        public string current_License { get; set; } = "";
        public string user_Name { get; set; } = "";
        public int core_Count { get; set; }

        public float memory_Usage { get; set; }
        public float dataBase_Usage { get; set; }
        public float cpu_Usage { get; set; }
    }
}
