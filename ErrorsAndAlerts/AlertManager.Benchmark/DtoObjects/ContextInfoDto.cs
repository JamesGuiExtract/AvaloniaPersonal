using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Benchmark.DtoObjects
{
    internal class ContextInfoDto
    {
        public string ApplicationName { get; set; } = "";
        public string ApplicationVersion { get; set; } = "";
        public string? MachineName { get; set; }
        public string? DatabaseServer { get; set; }
        public string? DatabaseName { get; set; }
        public string? FpsContext { get; set; }
        public string UserName { get; set; } = "";
        public Int32 PID { get; set; }
        public Int32 FileID { get; set; }
        public Int32 ActionID { get; set; }
    }
}
