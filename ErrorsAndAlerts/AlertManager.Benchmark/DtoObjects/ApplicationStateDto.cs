using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Benchmark.DtoObjects
{
    internal class ApplicationStateDto
    {
        public string ApplicationName { get; set; } = "";
        public string ApplicationVersion { get; set; } = "";
        public string ComputerName { get; set; } = "";
        public string UserName { get; set; } = "";
        public Int32 PID { get; set; }

    }
}
