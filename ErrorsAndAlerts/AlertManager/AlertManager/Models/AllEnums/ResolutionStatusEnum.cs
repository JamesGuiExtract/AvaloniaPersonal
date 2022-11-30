using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Models.AllEnums
{
    [System.Serializable]
    public enum TypeOfResolutionAlerts
    {
        Unresolved = 0,
        Resolved = 1,
        Snoozed = 2,
        AutoResolving = 3
    }
}


