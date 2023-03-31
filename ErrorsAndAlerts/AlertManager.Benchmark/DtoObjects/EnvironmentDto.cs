using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Benchmark.DtoObjects
{
    [ElasticsearchType]
    internal class EnvironmentDto
    {
        public DateTime CollectionTime { get; set; }
        public string Customer { get; set; } = "";
        public Dictionary<string, string> Data { get; set; } = new();
        [Keyword]
        public string MeasurementType { get; set; } = "";
        [Keyword]
        public string Context { get; set; } = "";
        [Keyword]
        public string Entity { get; set; } = "";
    }
}
