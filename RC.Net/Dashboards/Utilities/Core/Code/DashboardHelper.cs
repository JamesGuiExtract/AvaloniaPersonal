using DevExpress.DashboardCommon;
using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraBars;
using Extract.Dashboard.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Extract.Dashboard.Utilities
{
    public static class DashboardHelper
    {
        /// <summary>
        /// Displays OpenFileDialog to open file with ESDX extension by default or XML or all
        /// </summary>
        /// <param name="fileName">returns the name of the selected file</param>
        /// <returns><c>true</c> if a file was selected, <c>false</c> if no file was selected</returns>
        public static bool SelectDashboardFile(out string fileName)
        {
            fileName = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ESDX|*.esdx|XML|*.xml|All|*.*";
            openFileDialog.DefaultExt = "esdx";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;
                return true;
            }
            return false;
        }

    }
}
