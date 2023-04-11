using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Models.AllDataClasses.JSONObjects
{
    public class EventSource
    {
        [JsonProperty("eliCode")]
        public string EliCode { get; set; } = "";

        [JsonProperty("message")]
        public string Message { get; set; } = "";

        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("exceptionTime")]
        public DateTime ExceptionTime { get; set; } = DateTime.MinValue;

        [JsonProperty("context")]
        public ContextInfoFromJSON Context { get; set; } = new ContextInfoFromJSON();
    }
}
