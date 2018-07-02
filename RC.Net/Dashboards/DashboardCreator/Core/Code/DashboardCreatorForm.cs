using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using Extract;
using Extract.Dashboard.Forms;
using Extract.Dashboard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DashboardCreator
{
    public partial class DashboardCreatorForm : RibbonForm
    {

        #region Fields

        /// <summary>
        /// The currently open dashboard file
        /// </summary>
        string _dashboardFileName;

        /// <summary>
        /// The key used is the control name
        /// </summary>
        Dictionary<string, GridDetailConfiguration> _customGridValues = new Dictionary<string, GridDetailConfiguration>();

        /// <summary>
        /// Dictionary to track drill down level for Dashboard controls
        /// </summary>
        Dictionary<string, int> _drillDownLevelForItem = new Dictionary<string, int>();

        bool _drillDownLevelIncreased = false;


        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardCreatorForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        void HandleDashboardDesignerDrillDownPerformed(object sender, DrillActionEventArgs e)
        {
            try
            {
                _drillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
                _drillDownLevelIncreased = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45726");
            }
        }

        void HandleDashboardDesignerDrillUpPerformed(object sender, DrillActionEventArgs e)
        {
            try
            {
                _drillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45727");
            }
        }

        void HandleRecentDashboardsControlRecentItemClick(object sender, RecentItemClickEventArgs e)
        {
            try
            {
                _dashboardFileName = e.FilePath;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45721");
            }
        }

        void HandleConfigureExtractItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string component = dashboardDesigner.SelectedDashboardItem.ComponentName;

                // get any existing configuration data for the control
                GridDetailConfiguration configurationData;
                if (!_customGridValues.ContainsKey(component))
                {
                    configurationData =
                        new GridDetailConfiguration
                        {
                            DashboardGridName = component,
                            RowQuery = string.Empty
                        };
                }
                else
                {
                    configurationData = _customGridValues[component];
                }

                // Display the configuration
                DashboardFileDetailConfigurationForm configurationForm = new DashboardFileDetailConfigurationForm
                    ("Configure: " + configurationData.DashboardGridName, configurationData.RowQuery);

                if (configurationForm.ShowDialog() == DialogResult.OK)
                {
                    configurationData.RowQuery = configurationForm.RowQuery;
                    if (string.IsNullOrWhiteSpace(configurationData.RowQuery))
                    {
                        // if updated configuration data has been cleared remove it from the dictionary
                        _customGridValues.Remove(component);
                    }
                    else
                    {
                        // Save the updated configuration data in the dictionary
                        _customGridValues[component] = configurationData;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45711");
            }
        }

        void HandleDashboardDesignerPopupMenuShowing(object sender, DashboardPopupMenuShowingEventArgs e)
        {

            try
            {
                ShowConfigureExtractSettingsMenuItem(e.Menu, 
                    dashboardDesigner.Dashboard.Items[e.DashboardItemName] is GridDashboardItem);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45720");
            }

        }

        void HandleDashboardDesignerDashboardItemDoubleClick(object sender, DashboardItemMouseActionEventArgs e)
        {
            try
            {
                if ((sender is DashboardDesigner dashboardDesigner) && _customGridValues.ContainsKey(e.DashboardItemName))
                {
                    Dashboard dashboard = dashboardDesigner.Dashboard;
                    GridDashboardItem gridItem = dashboard.Items[e.DashboardItemName] as GridDashboardItem;
                    if (gridItem is null)
                    {
                        return;
                    }
                    int drillLevel;
                    _drillDownLevelForItem.TryGetValue(e.DashboardItemName, out drillLevel);

                    if (!gridItem.InteractivityOptions.IsDrillDownEnabled || 
                        !_drillDownLevelIncreased && (gridItem.GetDimensions().Count - 1 == drillLevel))
                    {
                        DashboardHelper.DisplayDashboardDetailForm(gridItem, e, _customGridValues[e.DashboardItemName]);
                    }
                }
                _drillDownLevelIncreased = false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45693");
            }
        }

        void HandleDashboardDesignerDashboardSaving(object sender, DashboardSavingEventArgs e)
        {
            try
            {
                if (_customGridValues.Count > 0)
                {
                    XElement userData = new XElement(
                    "ExtractConfiguredGrids",
                        _customGridValues.Select(kv =>
                            new XElement("Component", new XAttribute("Name", kv.Key),
                            new XElement("RowQuery", kv.Value.RowQuery))));
                    dashboardDesigner.Dashboard.UserData = userData;
                }
                else
                {
                    dashboardDesigner.Dashboard.UserData = null;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (e.Command == DashboardSaveCommand.SaveAs ||
                    e.Command == DashboardSaveCommand.Save && String.IsNullOrEmpty(_dashboardFileName))
                {

                    saveFileDialog.Filter = "ESDX|*.esdx|All|*.*";
                    saveFileDialog.DefaultExt = "esdx";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _dashboardFileName = saveFileDialog.FileName;
                        dashboardDesigner.Dashboard.SaveToXml(_dashboardFileName);
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45303");
            }
        }

        void HandleDashboardDesignerDashboardCreating(object sender, DashboardCreatingEventArgs e)
        {
            try
            {
                _dashboardFileName = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45304");
            }
        }

        void HandleDashboardDesignerDashboardChanged(object sender, EventArgs e)
        {
            try
            {
                if (_dashboardFileName is null)
                {
                    Text = "Dashboard creator";
                    _customGridValues = new Dictionary<string, GridDetailConfiguration>();
                }
                else
                {
                    Text = _dashboardFileName + " - " + dashboardDesigner.Dashboard.Title.Text;
                    _customGridValues = DashboardHelper.GridConfigurationsFromXML(dashboardDesigner.Dashboard?.UserData);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45305");
            }
        }

        void HandleFileOpenButtonItemItemClick(object sender, BackstageViewItemEventArgs e)
        {
            try
            {
                string newFile;
                if (DashboardHelper.SelectDashboardFile(out newFile))
                {
                    _dashboardFileName = newFile;
                    dashboardDesigner.LoadDashboard(_dashboardFileName);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45306");
            }
        }


        void HandleFileOpenBarButtonItemItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string newFile;
                if (DashboardHelper.SelectDashboardFile(out newFile))
                {
                    _dashboardFileName = newFile;
                    dashboardDesigner.LoadDashboard(_dashboardFileName);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45307");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Enables or disables the 'Configure Extract settings' menu item
        /// </summary>
        /// <param name="menu">the menu to show or hid the menu item </param>
        /// <param name="show">if <c>true</c> make the menu item visible. if <c>false</c> hide the menu item</param>
        static void ShowConfigureExtractSettingsMenuItem(PopupMenu menu, bool show)
        {

            foreach (var item in menu.ItemLinks)
            {
                BarItemLink itemLink = item as BarItemLink;
                if (itemLink != null)
                {
                    if (itemLink.DisplayCaption == "Configure Extract Settings")
                    {
                        itemLink.Visible = show;
                    }
                }
            }
        }

        #endregion
    }

}
