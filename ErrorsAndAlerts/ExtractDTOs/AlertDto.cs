using Nest;

namespace Extract.ErrorsAndAlerts.ElasticDTOs
{
    public class AlertDto
    {
        [PropertyName("alert_name")]
        public string AlertName { get; set; } = "";

        [PropertyName("configuration")]
        public string Configuration { get; set; } = "";

        [PropertyName("activation_time")]
        public DateTime ActivationTime { get; set; } = new();

        [PropertyName("associated_events")]
        public List<EventDto>? AssociatedEvents { get; set; } = null;

        [PropertyName("associated_environments")]
        public List<EnvironmentDto>? AssociatedEnvironments { get; set; } = null;

        [PropertyName("alert_type")]
        public string AlertType { get; set; } = "";

        [PropertyName("actions")]
        public List<AlertActionDto>? Actions { get; set; } = null;

        [PropertyName("hits")]
        public object? Hits { get; set; } = null;
    }
}
