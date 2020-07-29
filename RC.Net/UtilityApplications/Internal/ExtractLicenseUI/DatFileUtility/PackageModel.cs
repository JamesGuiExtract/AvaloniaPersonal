using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractLicenseUI.DatFileUtility
{
    class PackageModel
    {
        public Guid Guid { get; } = Guid.NewGuid();

        public HashSet<int> VariableIDs { get; } = new HashSet<int>();

        public HashSet<string> Variables { get; } = new HashSet<string>();

        public string PackageName { get; set; }

        public string PackageHeader { get; set; }

        public Guid Version { get; set; }
    }
}
