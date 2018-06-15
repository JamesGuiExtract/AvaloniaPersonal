﻿using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.DataAccess.ConnectionParameters;
using Extract.Dashboard.Forms;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Display the DashboardDetailForm 
        /// </summary>
        /// <param name="gridItem">Grid item that needs to display the detail</param>
        /// <param name="e">Event args</param>
        /// <param name="configuration">The configuration to pass to the DashboardDetailForm</param>
        public static void DisplayDashboardDetailForm(GridDashboardItem gridItem, DashboardItemMouseActionEventArgs e,
            GridDetailConfiguration configuration)
        {
            try
            {
                if (gridItem != null)
                {
                    bool drillDownEnabled = gridItem.InteractivityOptions.IsDrillDownEnabled;
                    var data = e.GetUnderlyingData();
                    if (data != null && (data.RowCount == 1 && drillDownEnabled || !drillDownEnabled))
                    {
                        // add the columns as parameter values
                        var columnNames = data.GetColumnNames();
                        Dictionary<string, object> columnValues = columnNames.ToDictionary(c => c, c => data[0][c]);

                        // Get the data source
                        DashboardSqlDataSource sqlDataSource = (DashboardSqlDataSource)gridItem.DataSource;
                        SqlServerConnectionParametersBase sqlParameters = sqlDataSource.ConnectionParameters as SqlServerConnectionParametersBase;

                        // the form will only be displayed if there is a FileName specified and the datasource is SQL database
                        if (columnValues.Count > 0 && columnValues.ContainsKey("FileName") && sqlParameters != null)
                        {
                            DashboardFileDetailForm detailForm = new DashboardFileDetailForm(columnValues, sqlParameters.ServerName, sqlParameters.DatabaseName, configuration);
                            detailForm.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45698");
            }
        }

        /// <summary>
        /// Extracts the GridDetailConfiguration for the configured controls from the xml
        /// </summary>
        /// <param name="xml">UserData portion of XML</param>
        /// <returns>Dictionary with ConfigurationDetailConfigurations for configured components from the XML</returns>
        public static Dictionary<string, GridDetailConfiguration> GridConfigurationsFromXML(XElement xml)
        {
            if (xml is null)
            {
                return new Dictionary<string, GridDetailConfiguration>();
            }

            Dictionary<string, GridDetailConfiguration> dict = new Dictionary<string, GridDetailConfiguration>();

            var extractGrids = xml.XPathSelectElements("//ExtractConfiguredGrids/Component");

            foreach (var e in extractGrids)
            {
                dict[e.Attribute("Name").Value] = new GridDetailConfiguration
                {
                    RowQuery = e.Element("RowQuery").Value,
                };
            }

            return dict;
        }

    }
}