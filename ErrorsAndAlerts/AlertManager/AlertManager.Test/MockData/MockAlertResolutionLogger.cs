﻿using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.ErrorsAndAlerts.AlertManager.Test.MockData
{
    internal class MockAlertResolutionLogger : IAlertResolutionLogger
    {
        //todo change this so it logs to target logger
        public void LogResolution(AlertsObject alert)
        {
            return;
        }

    }
}