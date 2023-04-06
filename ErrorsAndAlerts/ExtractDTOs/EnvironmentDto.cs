using Nest;

namespace Extract.ErrorsAndAlerts.ElasticDTOs
{
    [ElasticsearchType]
    public class EnvironmentDto
    {
        [PropertyName("collection_time")]
        public DateTime CollectionTime { get; set; }

        [PropertyName("customer")]
        public string Customer { get; set; } = "";

        [PropertyName("data")]
        public Dictionary<string, string> Data { get; set; } = new();

        [Keyword]
        [PropertyName("measurement_type")]
        public string MeasurementType { get; set; } = "";

        [Keyword]
        [PropertyName("context")]
        public string Context { get; set; } = "";

        [Keyword]
        [PropertyName("entity")]
        public string Entity { get; set; } = "";
    }
}
