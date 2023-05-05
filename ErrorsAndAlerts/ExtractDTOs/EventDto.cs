using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Extract.ErrorsAndAlerts.ElasticDTOs
{
    public class EventDto
    {
        [PropertyName("eli_code")]
        [JsonProperty("_index")]
        public string EliCode { get; set; } = "";

        [PropertyName("message")]
        public string Message { get; set; } = "";

        [PropertyName("id")]
        [JsonProperty("_id")]
        public string Id { get; set; } = "";

        [PropertyName("exception_time")]
        public DateTime ExceptionTime { get; set; } = new();

        [PropertyName("context")]
        public ContextInfoDto Context { get; set; } = new();

        [PropertyName("data")]
        public IList<KeyValuePair<string, string>> Data { get; set; } = Array.Empty<KeyValuePair<string, string>>();

        [PropertyName("stack_trace")]
        public Stack<string> StackTrace { get; set; } = new();

        [PropertyName("level")]
        public string Level { get; set; } = "";

        //needs to be nullable or infinite recursion will cause error
        [PropertyName("inner")]
        public EventDto? Inner { get; set; } = null;
    }
}