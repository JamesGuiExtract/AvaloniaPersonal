using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractLicenseUI.DatFileUtility
{
    public class PackageVariable
    {
        public HashSet<int> VariableIDs { get; } = new HashSet<int>();

        public HashSet<string> OtherVariables { get; } = new HashSet<string>();
        public string VariableName { get; set; }
    }
}
