using Nest;
using System;
using System.Collections.Generic;

namespace Extract.ErrorsAndAlerts.ElasticDTOs
{
    [ElasticsearchType]
    public class EnvironmentDto
    {
        [PropertyName("collection_time")]
        public DateTime CollectionTime { get; set; }

        [PropertyName("customer")]
        public string Customer { get; set; } = "";

        //Since EventDto has a Data field, this field's type must match that in EventDto
        [PropertyName("data")]
        //[Nested] this property may be needed to query for pairs in elastic
        public IList<KeyValuePair<string, string>> Data { get; set; } = Array.Empty<KeyValuePair<string, string>>();

        [Keyword]
        [PropertyName("measurement_type")]
        public string MeasurementType { get; set; } = "";

        //Since EventDto has a Context field that's type cannot be matched here, this field must be named differently
        [Keyword]
        [PropertyName("context_type")]
        public string ContextType { get; set; } = "";

        [Keyword]
        [PropertyName("entity")]
        public string Entity { get; set; } = "";
    }
}
