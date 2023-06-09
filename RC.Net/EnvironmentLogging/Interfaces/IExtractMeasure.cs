﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractEnvironmentService
{
    internal interface IExtractMeasure
    {
        public string Customer { get; set; }
        public string Context { get; set; }
        public string Entity { get; set; }
        public string MeasurementType { get; set; }
        public int MeasurementInterval { get; set; }
        public bool PinThread { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// Collects information and logs it to Elasticsearch
        /// </summary>
        ReadOnlyCollection<Dictionary<string,string>> Execute();
    }
}
