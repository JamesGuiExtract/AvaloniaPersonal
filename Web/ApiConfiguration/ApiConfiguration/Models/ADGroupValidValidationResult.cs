using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Web.ApiConfiguration.Models
{
    internal class ADGroupValidValidationResult
    {
        public bool ItemIsEmpty { get; set; }
        public bool GroupsValid { get; set; }
        public bool IsValid => ItemIsEmpty || GroupsValid;
    }
}
