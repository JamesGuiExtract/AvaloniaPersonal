﻿using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Dashboard.Utilities
{
    public interface IExtractDashboardCommon : IWin32Window, ISynchronizeInvoke
    {
        /// <summary>
        /// Gets the active dashboard from the underlying control
        /// </summary>
        DevExpress.DashboardCommon.Dashboard Dashboard { get; }

        /// <summary>
        /// Dictionary to track drill down level for Dashboard controls
        /// </summary>
        Dictionary<string, int> DrillDownLevelForItem { get; }

        /// <summary>
        /// Tracks if the Drill down level has increased for the control
        /// </summary>
        Dictionary<string, bool> DrillDownLevelIncreased { get; }

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
        IEnumerable<string> CurrentFilteredFiles { get; set; }

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
    }
}
