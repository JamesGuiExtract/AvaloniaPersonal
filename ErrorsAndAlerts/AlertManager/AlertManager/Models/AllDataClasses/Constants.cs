using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.Models.AllDataClasses
{
    public static class Constants
    {
        public static readonly Dictionary<EActionStatus, string> ActionStatusToDescription = new()
        {
            { EActionStatus.kActionUnattempted, "Unattempted"},
            { EActionStatus.kActionPending, "Pending"},
            { EActionStatus.kActionProcessing, "Processing"},
            { EActionStatus.kActionCompleted, "Completed"},
            { EActionStatus.kActionFailed, "Failed"},
            { EActionStatus.kActionSkipped, "Skipped"}
        };
    }
}
