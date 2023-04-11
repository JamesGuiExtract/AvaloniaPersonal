using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Models.AllDataClasses.JSONObjects
{
    public class EventFromJson
    {
        [JsonProperty("_index")]
        public string Index { get; set; } = "";

        [JsonProperty("_id")]
        public string Id { get; set; } = "";

        [JsonProperty("_score")]
        public object Score { get; set; } = new();

        [JsonProperty("_source")]
        public EventSource Source { get; set; } = new EventSource();

        [JsonProperty("fields")]
        public Dictionary<string, List<string>> Fields { get; set; } = new Dictionary<string, List<string>>();

        [JsonProperty("sort")]
        public List<long> Sort { get; set; } = new List<long>();
    }
}
