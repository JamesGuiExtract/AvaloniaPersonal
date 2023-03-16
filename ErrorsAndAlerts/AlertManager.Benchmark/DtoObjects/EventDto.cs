using Extract.ErrorHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Benchmark.DtoObjects
{
    internal class EventDto
    {
        public string EliCode { get; set; } = "";
        public string Message { get; set; } = "";
        public string Id { get; set; } = "";
        public DateTime ExceptionTime { get; set; } = new();
        public ApplicationStateDto ApplicationState { get; set; } = new();
        public List<DictionaryEntry> Data { get; set; } = new();
        public Stack<string> StackTrace { get; set; } = new();
        public string Level { get; set; } = ""; 
        public EventDto Inner { get; set; } = new();

    }
}
