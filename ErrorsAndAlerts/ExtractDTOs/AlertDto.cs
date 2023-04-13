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

        [PropertyName("alert_type")]
        public string AlertType { get; set; } = "";

        [PropertyName("actions")]
        public List<AlertActionDto>? Actions { get; set; } = null;

        //This field should only ever hold a list of EventDto or a list of EnvironmentDto
        [PropertyName("hits")]
        public object? Hits { get; set; } = null;
    }
}
