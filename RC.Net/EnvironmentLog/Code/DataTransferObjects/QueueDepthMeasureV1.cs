using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.EnvironmentLog
{
    public class QueueDepthMeasureV1 : IDataTransferObject
    {
        public string DatabaseServer { get; set; }
        public string DatabaseName { get; set; }
        public string Customer { get; set; }
        public string Context { get; set; }
        public string Entity { get; set; }
        public string MeasurementType { get; set; }
        public int MeasurementInterval { get; set; }
        public bool Enabled { get; set; }

        public IDomainObject CreateDomainObject()
        {
            var instance = new QueueDepthMeasure();
            instance.CopyFrom(this);
            return instance;
        }
    }
}
