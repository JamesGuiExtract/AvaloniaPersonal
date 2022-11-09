using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDashboard.Interfaces
{
    internal interface IAlertResolution
    {

        /// <summary>
        /// Posts a resolution for a given alert
        /// </summary>
        /// <param name="alertGUID">GUID Associated with the resolved Alert in the logging source</param>
        /// <param name="resolution">Text describing the resolution of the alert</param>
        void ResolveAlert(Guid alertGUID, string resolution);
    }
}
