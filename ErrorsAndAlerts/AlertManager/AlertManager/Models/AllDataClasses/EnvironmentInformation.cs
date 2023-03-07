using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// Class for storing environment information retrieved from Elastic Search
    /// </summary>
    [Serializable]
    public class EnvironmentInformation
    {
        public DateTime CollectionTime { get; set; }
        public string Customer { get; set; } = "";
        public Dictionary<string, string> Data { get; set; } = new();

        //determined by the service collecting the measurements
        //defines what key-value pairs will exist in the Data dictionary
        public string MeasurementType { get; set; } = "";

        //the type of the source of the collected info (e.g. "Machine", "Database")
        public string Context { get; set; } = "";

        //the specific, named, source of the collected info (e.g. server or machine name). 
        public string Entity { get; set; } = "";
    }
}
