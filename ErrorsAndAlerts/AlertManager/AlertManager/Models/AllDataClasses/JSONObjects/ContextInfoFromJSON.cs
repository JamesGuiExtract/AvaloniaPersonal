using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Models.AllDataClasses.JSONObjects
{
    public class ContextInfoFromJSON
    {
        [JsonProperty("applicationName")]
        public string ApplicationName { get; set; } = "";

        [JsonProperty("applicationVersion")]
        public string ApplicationVersion { get; set; } = "";

        [JsonProperty("machineName")]
        public string MachineName { get; set; } = "";

        [JsonProperty("userName")]
        public string UserName { get; set; } = "";

        [JsonProperty("pID")]
        public UInt32 PID { get; set; } = 0;

        [JsonProperty("fileID")]
        public Int32 FileID { get; set; } = 0;

        [JsonProperty("actionID")]
        public Int32 ActionID { get; set; } = 0;

        [JsonProperty("databaseServer")]
        public string DatabaseServer { get; set; } = "";

        [JsonProperty("databaseName")]
        public string DatabaseName { get; set; } = "";

        [JsonProperty("fpsContext")]
        public string FpsContext { get; set; } = "";
    }
}
