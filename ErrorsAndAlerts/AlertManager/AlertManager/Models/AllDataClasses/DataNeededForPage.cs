using AlertManager.Models.AllEnums;
using System;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// This data structure contains the generic data used to populate the data error page
    /// </summary>
    [Serializable]
    public class DataNeededForPage
    {
        public DataNeededForPage(int idNumber, int logErrorId, DateTime dateErrorCreated, string LoggerErrors,
            ResolutionStatus resolutionStatus, ErrorSeverityEnum severityStatus,
            UserMetrics userMetrics, string? alertComments = null)
        {
            this.id_Number = idNumber;
            this.log_Error_Id = logErrorId;
            this.date_Error_Created = dateErrorCreated;
            this.Logger_Errors = LoggerErrors;
            this.resolution_Status = resolutionStatus;
            this.alert_Comments = alertComments;
            this.severity_Status = severityStatus;
            this.user_Metrics = userMetrics;
        }

        public DataNeededForPage()
        {

        }

        public int id_Number = -1;
        public int log_Error_Id;
        public DateTime date_Error_Created;
        public string Logger_Errors = "";
        public ResolutionStatus resolution_Status;
        public string? alert_Comments;
        public ErrorSeverityEnum severity_Status;
        public UserMetrics? user_Metrics;
    }
}
