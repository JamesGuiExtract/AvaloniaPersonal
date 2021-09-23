using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Dashboard.Utilities
{
    public interface IExtractDashboardCommon : IWin32Window, ISynchronizeInvoke, IDisposable
    {
        /// <summary>
        /// Name of the application that this is for
        /// </summary>
        string ApplicationName { get; }
        /// <summary>
        /// Gets the active dashboard from the underlying control
        /// </summary>
        DevExpress.DashboardCommon.Dashboard CurrentDashboard { get; }

        /// <summary>
        /// Dictionary to track drill down level for Dashboard controls
        /// </summary>
        Dictionary<string, int> DrilldownLevelForItem { get; }

        /// <summary>
        /// Tracks if the Drill down level has increased for the control
        /// </summary>
        Dictionary<string, bool> DrilldownLevelIncreased { get; }

        /// <summary>
        /// The server name to use for the Dashboard
        /// </summary>
        string ServerName { get; set; }

        /// <summary>
        /// The DatabaseName to use for the dashboard
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// The Server configured in the Dashboard
        /// </summary>
        string ConfiguredServerName { get; set; }

        /// <summary>
        /// The Database configured in the Dashboard
        /// </summary>
        string ConfiguredDatabaseName { get; set; }

        /// <summary>
        /// Indicates that the Server and DatabaseName have been overridden
        /// </summary>
        bool IsDatabaseOverridden { get; }

        /// <summary>
        /// List of files that were selected in the control when the Popup was 
        /// displayed
        /// </summary>
        HashSet<string> CurrentFilteredFiles { get; }

        /// <summary>
        /// The key value pairs for the currently filter dimension selected in the grid
        /// </summary>
        Dictionary<string, object> CurrentFilteredDimensions { get; }

        /// <summary>
        /// If the control that is implementing this interface has a <see cref="DevExpress.DashboardWin.DashboardViewer"/>
        /// instance then this returns that instance otherwise this should return null
        /// </summary>
        DashboardViewer Viewer { get; }

        /// <summary>
        /// If the control that is implementing this interface has a <see cref="DevExpress.DashboardWin.DashboardDesigner"/>
        /// instance then this returns that instance otherwise this should return null
        /// </summary>
        DashboardDesigner Designer { get; }

        /// <summary>
        /// Gets the current filtered values for the named dashboard item
        /// </summary>
        /// <param name="dashboardItemName">Dashboard item name</param>
        /// <returns>List of current <see cref="AxisPointTuple"/>s for the named control</returns>
        IList<AxisPointTuple> GetCurrentFilterValues(string dashboardItemName);

        /// <summary>
        /// Safe invoke the specified <see paramref="action"/> asynchronously within a try/catch handler
        /// that will display any exceptions.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with any exception.</param>
        /// <param name="action">The <see cref="Action"/> to be invoked.</param>
        /// <param name="displayExceptions"><see langword="true"/> to display any exception caught;
        /// <see langword="false"/> to log instead.</param>
        /// <param name="exceptionAction">A second action that should be executed in the case of an
        /// exception an exception in <see paramref="action"/>.</param>
        void SafeBeginInvokeForShared(string eliCode, Action action,
                    bool displayExceptions = true, Action<Exception> exceptionAction = null);

        /// <summary>
        /// Opens a dashboard viewer with the given dashboard name and the filter data
        /// </summary>
        /// <param name="dashboardName">This will be assumed another dashboard in the current database for the open dashboard </param>
        /// <param name="filterData">The dictionary contains the filter data</param>
        void OpenDashboardForm(string dashboardName, Dictionary<string, object> filterData);
    }
}
