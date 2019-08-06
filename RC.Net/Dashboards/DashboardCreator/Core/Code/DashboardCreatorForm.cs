using DevExpress.DashboardCommon;
using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using Extract;
using Extract.Dashboard.Forms;
using Extract.Dashboard.Utilities;
using Extract.DashboardViewer;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Flag to indicate if there are custom changes that need to be saved
        /// </summary>
        bool _dirty;

        #endregion

        #region IExtractDashboardCommon Implementation

        /// <summary>
        /// Gets the active dashboard from the underlying control
        /// </summary>
        public Dashboard CurrentDashboard
        {
            get
            {
                return dashboardDesigner.Dashboard;
            }
        }

        /// <summary>
        /// Dictionary to track drill down level for Dashboard controls
        /// </summary>
        public Dictionary<string, int> DrilldownLevelForItem { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Tracks if the Drill down level has increased for the control
        /// </summary>
        public Dictionary<string, bool> DrilldownLevelIncreased { get; } = new Dictionary<string, bool>();

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
        public DevExpress.DashboardWin.DashboardViewer Viewer { get; } = null;

        /// <summary>
        /// Since this instance has a <see cref="DevExpress.DashboardWin.DashboardDesigner"/> it should return the designer
        /// </summary>
        public DashboardDesigner Designer => dashboardDesigner;

        /// <summary>
        /// The key value pairs for the currently filter dimension selected in the grid
        /// </summary>
        public Dictionary<string, object> CurrentFilteredDimensions { get; } = new Dictionary<string, object>();

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

        /// <summary>
        /// Opens a dashboard viewer with the given dashboard name and the filter data
        /// </summary>
        /// <param name="dashboardName">This will be assumed another dashboard in the current database for the open dashboard </param>
        /// <param name="filterData">The dictionary contains the filter data</param>
        public void OpenDashboardForm(string dashboardName, Dictionary<string, object> filterData)
        {
            try
            {
                DashboardViewerForm form = new DashboardViewerForm(dashboardName, true, ServerName, DatabaseName);
                form.ParameterValues.AddRange(filterData);
                form.Show();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47067");
            }
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
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46173");
            }
        }

        #endregion

        #region Event Handlers

        void HandleBarButtonItemConfigureDashboardLinks_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string component = dashboardDesigner.SelectedDashboardItem.ComponentName;
                var grid = dashboardDesigner.Dashboard.Items[component] as GridDashboardItem;

                if (grid is null)
                {
                    return;
                }

                GridDetailConfiguration configurationData = GetDetailConfigurationData(component);

                ConfigureDashboardLinksForm linksForm = new ConfigureDashboardLinksForm(configurationData.DashboardLinks);
                linksForm.ShowDialog();
                if (!configurationData.DashboardLinks.SetEquals(linksForm.DashboardLinks))
                {
                    _dirty = true;
                    configurationData.DashboardLinks.Clear();
                    configurationData.DashboardLinks.UnionWith(linksForm.DashboardLinks);
                    _dashboardShared.CustomGridValues[component] = configurationData;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47087");
            }
        }

        void HandleDashboardDesigner_DashboardClosing(object sender, DashboardClosingEventArgs e)
        {
            try
            {
                e.IsDashboardModified = e.IsDashboardModified || _dirty;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47015");
            }
        }

        void HandleBarButtonItemConfigureFileNameColumn_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string component = dashboardDesigner.SelectedDashboardItem.ComponentName;
                var grid = dashboardDesigner.Dashboard.Items[component] as GridDashboardItem;

                if (grid is null)
                {
                    return;
                }

                GridDetailConfiguration configurationData = GetDetailConfigurationData(component);
                string selectedField = configurationData.DataMemberUsedForFileName;

                // Only need to look at the Dimensions
                IList<string> fieldNames = grid.GetDimensions()
                    .Where(d => d.DataSourceFieldType == typeof(string))
                    .Select(d => d.DataMember).ToList();

                // Add empty string at top of list
                fieldNames.Insert(0, string.Empty);

                using (var listForm = new SelectFromListInputBox
                {
                    Title = "Select file name field",
                    Prompt = "Data field for file name",
                    SelectionStrings = fieldNames,
                    DefaultResponse = selectedField
                })
                {
                    if (listForm.ShowDialog() == DialogResult.OK)
                    {
                        if (selectedField != listForm.ReturnValue)
                        {
                            _dirty = true;
                            configurationData.DataMemberUsedForFileName = listForm.ReturnValue;
                            _dashboardShared.CustomGridValues[component] = configurationData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47007");
            }
        }
        void HandleBarCreateDataExtractDatasourcesItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                DashboardDataConverter.AddExtractDataSources(
                    Path.GetFileName(_dashboardFileName), dashboardDesigner.Dashboard, Path.GetDirectoryName(_dashboardFileName));
                _dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46849");
            }
        }

        void HandleDashboardCreatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // May already be canceled so don't do anything if it is
                if (!e.Cancel)
                {
                    e.Cancel = !_dashboardShared.RequestDashboardClose();
                }
            }
            catch (Exception ex)
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
                DrilldownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
                DrilldownLevelIncreased[e.DashboardItemName] = true;
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
                DrilldownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
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
                    _dirty = false;
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
                        _dirty = false;
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45721");
            }
        }

        void HandleConfigureRowQueryItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string component = dashboardDesigner.SelectedDashboardItem.ComponentName;

                GridDetailConfiguration configurationData = GetDetailConfigurationData(component);

                // Display the configuration
                DashboardFileDetailConfigurationForm configurationForm = new DashboardFileDetailConfigurationForm
                    ("Configure: " + configurationData.DashboardGridName, configurationData.RowQuery);

                if (configurationForm.ShowDialog() == DialogResult.OK)
                {
                    if (configurationData.RowQuery != configurationForm.RowQuery)
                    {
                        _dirty = true;
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
                        .Where(kvp => CurrentDashboard.Items.Select(di => di.ComponentName).Contains(kvp.Key))
                        .Select(kv =>
                            new XElement("Component", new XAttribute("Name", kv.Key),
                            new XElement("RowQuery", kv.Value.RowQuery),
                            new XElement("DataMemberUsedForFileName",
                                string.IsNullOrWhiteSpace(kv.Value.DataMemberUsedForFileName) ? "FileName" : kv.Value.DataMemberUsedForFileName),
                            new XElement("DashboardLinks", string.Join(",", kv.Value.DashboardLinks))))
                        );
                    CurrentDashboard.UserData = userData;
                }
                else
                {
                    CurrentDashboard.UserData = null;
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
                        CurrentDashboard.SaveToXml(_dashboardFileName);
                        UpdateTitle();
                    }
                }
                else
                {
                    CurrentDashboard.SaveToXml(_dashboardFileName);
                }
                e.Handled = true;
                _dirty = false;
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
                _dirty = false;
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
                _dashboardShared?.GridConfigurationsFromXml(CurrentDashboard?.UserData);

                _dirty = false;
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
                FileBrowser fileBrowser = new FileBrowser();
                string selectedFile = fileBrowser.BrowseForFile("ESDX|*.esdx|XML|*.xml|All|*.*", string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
                {
                    _dashboardFileName = selectedFile;
                    dashboardDesigner.LoadDashboard(_dashboardFileName);
                    _dirty = false;
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
                FileBrowser fileBrowser = new FileBrowser();
                string selectedFile = fileBrowser.BrowseForFile("ESDX|*.esdx|XML|*.xml|All|*.*", string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
                {
                    _dashboardFileName = selectedFile;
                    dashboardDesigner.LoadDashboard(_dashboardFileName);
                    _dirty = false;
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
        /// Finds or creates the <see cref="GridDetailConfiguration"/> object for the given component
        /// </summary>
        /// <param name="component">The name of the component to get the <see cref="GridDetailConfiguration"/> object for</param>
        /// <returns>The <see cref="GridDetailConfiguration"/> for the component</returns>
        GridDetailConfiguration GetDetailConfigurationData(string component)
        {
            // get any existing configuration data for the control
            GridDetailConfiguration configurationData;
            if (!_dashboardShared.CustomGridValues.ContainsKey(component))
            {
                configurationData =
                    new GridDetailConfiguration
                    {
                        DashboardGridName = component,
                        RowQuery = string.Empty,
                        DataMemberUsedForFileName = "FileName"
                    };
            }
            else
            {
                configurationData = _dashboardShared.CustomGridValues[component];
            }

            return configurationData;
        }

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
