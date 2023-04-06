using Nest;

namespace Extract.ErrorsAndAlerts.ElasticDTOs
{
    public class AlertActionDto
    {
        [PropertyName("action_comment")]
        public string ActionComment { get; set; } = "";

        [PropertyName("action_time")]
        public DateTime? ActionTime { get; set; } = null;

        [PropertyName("snooze_duration")]
        public DateTime? SnoozeDuration { get; set; } = null;

        //placeholder for enum TypeOfResolutionAlerts
        [PropertyName("action_type")]
        public string? ActionType { get; set; } = "";

    }
}
