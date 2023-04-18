using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.ErrorsAndAlerts.AlertManager.Test.MockData
{
    internal class MockAlertResolutionLogger : IAlertActionLogger
    {
        //todo change this so it logs to target logger
        public void LogAction(AlertsObject alert)
        {
            return;
        }

    }
}
