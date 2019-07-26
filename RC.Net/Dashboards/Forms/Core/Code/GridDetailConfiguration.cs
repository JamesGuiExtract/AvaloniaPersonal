using System;
using System.Collections.Generic;

namespace Extract.Dashboard.Forms
{
    /// <summary>
    /// Configuration data class for DashboardFileDetailConfigurationForm
    /// </summary>
    public class GridDetailConfiguration
    {
        public string DashboardGridName { get; set; }

        /// <summary>
        /// SQL Query used for populating the DashboardFileDetailForm used when double clicking on a row 
        /// in a grid that has a FileName member that is defined with DataMemberUsedForFileName
        /// </summary>
        public string RowQuery { get; set; }

        /// <summary>
        /// The DataMember defined in the data query used for a grid that is included in a grid as a
        /// Dimension that will be interpeted as a FileName used for any actions that use a file name
        /// </summary>
        public string DataMemberUsedForFileName { get; set; }

        /// <summary>
        /// This contains all the dashboards in a database that have been configured to be opened 
        /// via the "Open Dashboard" menu for a grid
        /// </summary>
        public HashSet<string> DashboardLinks { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
