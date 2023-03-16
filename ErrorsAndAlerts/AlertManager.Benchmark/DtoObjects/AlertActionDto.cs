using AlertManager.Models.AllEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Benchmark.DtoObjects
{
    internal class AlertActionDto
    {
        public string ActionComment { get; set; } = "";
        public DateTime? ActionTime { get; set; } = null;
        public DateTime? SnoozeDuration { get; set; } = null;

        //placeholder for enum TypeOfResolutionAlerts
        public string? ActionType { get; set; } = "";

    }
}
