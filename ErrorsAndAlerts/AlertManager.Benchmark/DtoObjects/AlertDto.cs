using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Benchmark.DtoObjects
{
    internal class AlertDto
    {
        public string AlertName { get; set; } = "";
        public string AlertType { get; set; } = "";
        public string Configuration { get; set; } = "";
        public DateTime ActivationTime { get; set; } = new();
        public string UserFound { get; set; } = "";
        public string MachineFoundError { get; set; } = "";
        public string? AlertHistory { get; set; } = null;
        public List<EventDto>? AssociatedEvents { get; set; } = null;
        public List<EnvironmentDto>? AssociatedEnvironments { get; set; } = null;
        public List<AlertActionDto>? Actions { get; set; } = null;
    }
}
