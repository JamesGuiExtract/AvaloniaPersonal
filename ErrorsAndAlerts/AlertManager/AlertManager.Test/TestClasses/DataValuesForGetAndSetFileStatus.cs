using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.ErrorsAndAlerts.AlertManager.Test.TestClasses
{
    public class DataValuesForGetAndSetFileStatus
    {
        public DataValuesForGetAndSetFileStatus(int idNumber,
                        string actionName,
                        int actionId,
                        EActionStatus actionStatus,
                        int workflowId,
                        string server,
                        string dataBaseName,
                        string workFlowName)
        {
            this.idNumber = idNumber;
            this.actionName = actionName;
            this.actionId = actionId;
            this.actionStatus = actionStatus;
            this.workflowId = workflowId;
            this.server = server;
            this.dataBaseName = dataBaseName;
            this.workFlowName = workFlowName;
        }

        public int idNumber;
        public string actionName;
        public int actionId;
        public EActionStatus actionStatus;
        public int workflowId;
        public string server;
        public string dataBaseName;
        public string workFlowName;
    }
}
