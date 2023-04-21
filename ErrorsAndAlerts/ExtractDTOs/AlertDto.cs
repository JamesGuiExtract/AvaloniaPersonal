﻿using Nest;

namespace Extract.ErrorsAndAlerts.ElasticDTOs
{
    public class AlertDto
    {
        public string AlertName { get; set; } = "";

        [PropertyName("configuration")]
        public string Configuration { get; set; } = "";

        [PropertyName("activation_time")]
        public DateTime ActivationTime { get; set; } = new();

        [PropertyName("hits_type")]
        public string HitsType { get; set; } = "";

        [PropertyName("actions")]
        public List<AlertActionDto>? Actions { get; set; } = null;

        //This field should only ever hold a list of EventDto or a list of EnvironmentDto
        [PropertyName("hits")]
        public object? Hits { get; set; } = null;
    }
}
