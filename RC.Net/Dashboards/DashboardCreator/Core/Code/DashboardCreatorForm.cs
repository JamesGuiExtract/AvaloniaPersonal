using DevExpress.DashboardCommon;
using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using Extract;
using Extract.Dashboard.Forms;
using Extract.Dashboard.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DashboardCreator
{
    public partial class DashboardCreatorForm : RibbonForm, IExtractDashboardCommon
    {

        #region Fields

        /// <summary>
        /// The currently open dashboard file
        /// </summary>
        string _dashboardFileName;

        /// <summary>
        /// Server name to override server name in dashboard config
        /// if empty or null dashboard will be opened with configured server
        /// </summary>
        string _serverName;

        /// <summary>
        /// Database name to override database name in dashboard config
        /// if empty or null dashboard will be opened with configured database
        /// </summary>
        string _databaseName;

        /// <summary>
        /// Instance of <see cref="DashboardShared{T}"/> that contains shared code between Creator and Viewer
        /// </summary>
        DashboardShared<DashboardCreatorForm> _dashboardShared;

        #endregion

        #region IExtractDashboardCommon Implementation

        /// <summary>
        /// Gets the active dashboard from the underlying control
        /// </summary>
        public Dashboard Dashboard
        {
            get
            {
                return dashboardDesigner.Dashboard;
            }
        }

        /// <summary>
        /// Dictionary to track drill down level for Dashboard controls
        /// </summary>
        public Dictionary<string, int> DrillDownLevelForItem { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Tracks if the Drill down level has increased for the control
        /// </summary>
        public Dictionary<string, bool> DrillDownLevelIncreased { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// The server name to use for the Dashboard
        /// </summary>
        public string ServerName
        {
            get
            {
                return IsDatabaseOverridden ? _serverName : ConfiguredServerName; ;
            }
            set
            {
                _serverName = value;
            }
        }

        /// <summary>
        /// The DatabaseName to use for the dashboard
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return IsDatabaseOverridden ? _databaseName : ConfiguredDatabaseName;
            }

            set
            {
                _databaseName = value;
            }
        }

        /// <summary>
        /// The Server configured in the Dashboard
        /// </summary>
        public string ConfiguredServerName { get; set; }

        /// <summary>
        /// The Database configured in the Dashboard
        /// </summary>
        public string ConfiguredDatabaseName { get; set; }

        /// <summary>
        /// Indicates that the Server and DatabaseName have been overridden
        /// </summary>
        public bool IsDatabaseOverridden
        {
            get
            {
                return !(string.IsNullOrWhiteSpace(_serverName) || string.IsNullOrWhiteSpace(_databaseName));
            }
        }

        /// <summary>
        /// List of files that were selected in the control when the Popup was 
        /// displayed
        /// </summary>
        public HashSet<string> CurrentFilteredFiles { get; } = new HashSet<string>();

        /// <summary>
        /// Since this instance is not a <see cref="DevExpress.DashboardWin.DashboardViewer"/> it should return null
        /// </summary>
        public DashboardViewer Viewer => null;
        
        /// <summary>
        /// Since this instance has a <see cref="DevExpress.DashboardWin.DashboardDesigner"/> it should return the designer
        /// </summary>
        public DashboardDesigner Designer => dashboardDesigner;

        /// <summary>
        /// Gets the current filtered values for the named dashboard item
        /// </summary>
        /// <param name="dashboardItemName">Dashboard item name</param>
        /// <returns>List of current <see cref="AxisPointTuple"/>s for the named control</returns>
        public IList<AxisPointTuple> GetCurrentFilterValues(string dashboardItemName)
        {
            return dashboardDesigner.GetCurrentFilterValues(dashboardItemName);
        }

        /// <summary>
        /// Calls the <see cref="FormsExtensionMethods.SafeBeginInvoke(Control, string, Action, bool, Action{Exception})"/> 
        /// the specified <see paramref="action"/> asynchronously within a try/catch handler
        /// that will display any exceptions.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with any exception.</param>
        /// <param name="action">The <see cref="Action"/> to be invoked.</param>
        /// <param name="displayExceptions"><see langword="true"/> to display any exception caught;
        /// <see langword="false"/> to log instead.</param>
        /// <param name="exceptionAction">A second action that should be executed in the case of an
        /// exception an exception in <see paramref="action"/>.</param>
        public void SafeBeginInvokeForShared(string eliCode, Action action,
            bool displayExceptions = true, Action<Exception> exceptionAction = null)
        {
            this.SafeBeginInvoke(eliCode, action, displayExceptions, exceptionAction);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardCreatorForm()
        {
            InitializeComponent();
            _dashboardShared = new DashboardShared<DashboardCreatorForm>(this);
        }

        /// <summary>
        /// Constructor that loads the given fileName
        /// </summary>
        /// <param name="fileName">File containing the dashboard to open</param>
        public DashboardCreatorForm(string fileName)
        {
            try
            {
                InitializeComponent();

                _dashboardShared = new DashboardShared<DashboardCreatorForm>(this);

                _dashboardFileName = fileName;
                if (!string.IsNullOrWhiteSpace(_dashboardFileName))
                {
                    dashboardDesigner.Dashboard.LoadFromXml(_dashboardFileName);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46173");
            }
        }

        #endregion

        #region Event Handlers

        void HandleDashboardCreatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                e.Cancel = !_dashboardShared.RequestDashboardClose(); 
            }
            catch ( Exception ex)
            {
                ex.ExtractDisplay("ELI46219");
            }
        }

        void HandleDashboardDesignerConfigureDataConnection(object sender, DashboardConfigureDataConnectionEventArgs e)
        {
            try
            {
                _dashboardShared.HandleConfigureDataConnection(sender, e);

                UpdateTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46128");
            }
        }

        void HandleDashboardDesignerDrillDownPerformed(object sender, DrillActionEventArgs e)
        {
            try
            {
                DrillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
                DrillDownLevelIncreased[e.DashboardItemName] = true;
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
                DrillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
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
                if (!string.IsNullOrWhiteSpace(e.FilePath))
                {
                    _dashboardFileName = e.FilePath;
                    dashboardDesigner.Dashboard.LoadFromXml(_dashboardFileName);
                    e.Handled = true;
                }
                else if (!string.IsNullOrWhiteSpace(e.DirectoryPath))
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "ESDX|*.esdx|All|*.*";
                    openFileDialog.DefaultExt = "esdx";
                    openFileDialog.InitialDirectory = e.DirectoryPath;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _dashboardFileName = openFileDialog.FileName;
                        dashboardDesigner.Dashboard.LoadFromXml(_dashboardFileName);
                    }
                    e.Handled = true;
                }
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
                if (!_dashboardShared.CustomGridValues.ContainsKey(component))
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
                    configurationData = _dashboardShared.CustomGridValues[component];
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
                        _dashboardShared.CustomGridValues.Remove(component);
                    }
                    else
                    {
                        // Save the updated configuration data in the dictionary
                        _dashboardShared.CustomGridValues[component] = configurationData;
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

                _dashboardShared.HandlePopupMenuShowing(sender, e);
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
                _dashboardShared.HandleGridDashboardItemDoubleClick(sender, e);
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
                if (_dashboardShared.CustomGridValues.Count > 0)
                {
                    XElement userData = new XElement(
                    "ExtractConfiguredGrids",
                        _dashboardShared.CustomGridValues
                        .Where(kvp => Dashboard.Items.Select(di => di.ComponentName).Contains(kvp.Key))
                        .Select(kv =>
                            new XElement("Component", new XAttribute("Name", kv.Key),
                            new XElement("RowQuery", kv.Value.RowQuery))));
                    Dashboard.UserData = userData;
                }
                else
                {
                    Dashboard.UserData = null;
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
                        Dashboard.SaveToXml(_dashboardFileName);
                        UpdateTitle();
                    }
                }
                else
                {
                    Dashboard.SaveToXml(_dashboardFileName);
                }
                e.Handled = true;
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
                UpdateTitle();
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
                _dashboardShared?.GridConfigurationsFromXML(Dashboard?.UserData);

                UpdateTitle();
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
        /// Update the title with the currently loaded dashboard file name
        /// </summary>
        void UpdateTitle()
        {
            if (_dashboardFileName is null)
            {
                Text = "Dashboard creator";
            }
            else
            {
                Text = _dashboardFileName + " - " + dashboardDesigner.Dashboard.Title.Text;
            }
        }

        /// <summary>
        /// Enables or disables the 'Configure Extract settings' menu item
        /// </summary>
        /// <param name="menu">the menu to show or hide the menu item </param>
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
