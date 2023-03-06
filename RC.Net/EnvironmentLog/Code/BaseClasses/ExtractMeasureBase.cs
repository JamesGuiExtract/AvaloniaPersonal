using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.EnvironmentLog
{
    public class ExtractMeasureBase : IExtractMeasure
    {
        public string Customer { get; set; }
        public string Context { get; set; }
        public string Entity { get; set; }
        public string MeasurementType { get; set; }
        public int MeasurementInterval { get; set; }
        public bool Enabled { get; set; }

        public virtual List<Dictionary<string,string>> Execute() 
        {
            return new List<Dictionary<string, string>>();
        }
    }
}
