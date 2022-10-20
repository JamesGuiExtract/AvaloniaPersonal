using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractVMManager.Models
{
    public class VMCreationRequest
    {
        public string? Name { get; set; }
        public string? TemplateName { get; set; }
    }
}
