using DevExpress.DashboardCommon;
using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using DevExpress.Utils.Extensions;
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
        private string _dashboardFileName;

        /// <summary>
        /// Server name to override server name in dashboard config if empty or null dashboard will be opened with
        /// configured server
        /// </summary>
        private string _serverName;

        /// <summary>
        /// Database name to override database name in dashboard config if empty or null dashboard will be opened with
        /// configured database
        /// </summary>
        private string _databaseName;

        /// <summary>
        /// Instance of <see cref="DashboardShared{T}"/> that contains shared code between Creator and Viewer
        /// </summary>
        private DashboardShared<DashboardCreatorForm> _dashboardShared;

        /// <summary>
        /// Flag to indicate if there are custom changes that need to be saved
        /// </summary>
        private bool _dirty;

        #endregion

        #region IExtractDashboardCommon Implementation

        public string ApplicationName { get; } = "Extract Dashboard Designer";

        /// <summary>
        /// Gets the active dashboard from the underlying control
        /// </summary>
        public Dashboard CurrentDashboard { get { return dashboardDesigner.Dashboard; } }

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
                return IsDatabaseOverridden ? _serverName : ServerNameFromDefinition;
            }
            set { _serverName = value; }
        }

        /// <summary>
        /// The DatabaseName to use for the dashboard
        /// </summary>
        public string DatabaseName
        {
            get { return IsDatabaseOverridden ? _databaseName : DatabaseNameFromDefinition; }

            set { _databaseName = value; }
        }

        /// <summary>
        /// The Server configured in the Dashboard
        /// </summary>
        public string ServerNameFromDefinition { get; set; }

        /// <summary>
        /// The Database configured in the Dashboard
        /// </summary>
        public string DatabaseNameFromDefinition { get; set; }

        /// <summary>
        /// Indicates that the Server and DatabaseName have been overridden
        /// </summary>
        public bool IsDatabaseOverridden
        {
            get { return !(string.IsNullOrWhiteSpace(_serverName) || string.IsNullOrWhiteSpace(_databaseName)); }
        }

        /// <summary>
        /// List of files that were selected in the control when the Popup was  displayed
        /// </summary>
        public HashSet<string> CurrentFilteredFiles { get; } = new HashSet<string>();

        /// <summary>
        /// Since this instance is not a <see cref="DevExpress.DashboardWin.DashboardViewer"/> it should return null
        /// </summary>
        public DevExpress.DashboardWin.DashboardViewer Viewer { get; } = null;

        /// <summary>
        /// Since this instance has a <see cref="DevExpress.DashboardWin.DashboardDesigner"/> it should return the
        /// designer
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
        /// Calls the <see cref="FormsExtensionMethods.SafeBeginInvoke(Control, string, Action, bool,
        /// Action{Exception})"/>  the specified <see paramref="action"/> asynchronously within a try/catch handler that
        /// will display any exceptions.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with any exception.</param>
        /// <param name="action">The <see cref="Action"/> to be invoked.</param>
        /// <param name="displayExceptions">
        /// <see langword="true"/> to display any exception caught; <see langword="false"/> to log instead.
        /// </param>
        /// <param name="exceptionAction">
        /// A second action that should be executed in the case of an exception an exception in <see
        /// paramref="action"/>.
        /// </param>
        public void SafeBeginInvokeForShared(string eliCode,
                                             Action action,
                                             bool displayExceptions = true,
                                             Action<Exception> exceptionAction = null)
        {
            this.SafeBeginInvoke(eliCode, action, displayExceptions, exceptionAction);
        }

        /// <summary>
        /// Opens a dashboard viewer with the given dashboard name and the filter data
        /// </summary>
        /// <param name="dashboardName">This will be assumed another dashboard in the current database for the open dashboard</param>
        /// <param name="filterData">The dictionary contains the filter data</param>
        public void OpenDashboardForm(string dashboardName, Dictionary<string, object> filterData)
        {
            try
            {
                bool isFile = string.IsNullOrWhiteSpace(Path.GetExtension(dashboardName));
                DashboardViewerForm form = new DashboardViewerForm(dashboardName, isFile, ServerName, DatabaseName, filterData);
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
            try
            {
                _dashboardShared = new DashboardShared<DashboardCreatorForm>(this);
                InitializeComponent();
                ExtractSettingsRibbonPage.Visible = SystemMethods.IsExtractInternal();
                this.Designer.ValidateCustomSqlQuery += DashboardHelpers.HandleDashboardCustomSqlQuery;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51929");
            }
        }

        /// <summary>
        /// Constructor that loads the given fileName
        /// </summary>
        /// <param name="fileName">File containing the dashboard to open</param>
        public DashboardCreatorForm(string fileName)
            :  this()
        {
            try
            {
                _dashboardFileName = fileName;
                if (!string.IsNullOrWhiteSpace(_dashboardFileName))
                {
                    var xdoc = XDocument.Load(_dashboardFileName);
                    dashboardDesigner.Dashboard.LoadFromXDocument(xdoc);
                }
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46173");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Used to bind the CoreLicensed value to the button on the menu
        /// </summary>
        public bool CoreLicensed
        {
            get { return _dashboardShared?.CustomData.CoreLicensed ?? false; }

            set
            {
                try
                {
                    if (value != _dashboardShared.CustomData.CoreLicensed)
                    {
                        _dashboardShared.CustomData.CoreLicensed = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI49769");
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleCoreLicensedBarCheckItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                CoreLicensed = !CoreLicensed;
                coreLicensedBarCheckItem.Checked = CoreLicensed;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49768");
            }
        }

        private void Handle_BarButtonItemConfigureDashboardLinks_ItemClick(object sender, ItemClickEventArgs e)
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
                var existingDashboards = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                existingDashboards.AddRange(_dashboardShared.DashboardList().Select(d => d.SourceName));
                if (grid.InteractivityOptions.MasterFilterMode == DashboardItemMasterFilterMode.None)
                {
                    MessageBox.Show("Grid must be part of a Master filter",
                                    "No Master Filter",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    return;
                }
                var linksForm = new ConfigureDashboardLinksForm(configurationData.DashboardLinks, existingDashboards);
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

        private void HandleDashboardDesigner_DashboardClosing(object sender, DashboardClosingEventArgs e)
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

        private void Handle_BarButtonItemConfigureFileNameColumn_ItemClick(object sender, ItemClickEventArgs e)
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
                                               .Select(d => d.DataMember)
                                               .ToList();

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

        private void Handle_BarCreateDataExtractDataSourcesItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                DashboardDataConverter.AddExtractDataSources(Path.GetFileName(_dashboardFileName),
                                                             dashboardDesigner.Dashboard,
                                                             Path.GetDirectoryName(_dashboardFileName));
                _dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46849");
            }
        }

        private void HandleDashboardCreatorForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void HandleDashboardDesignerConfigureDataConnection(object sender,
                                                                    DashboardConfigureDataConnectionEventArgs e)
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

        private void HandleDashboardDesignerDrillDownPerformed(object sender, DrillActionEventArgs e)
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

        private void HandleDashboardDesignerDrillUpPerformed(object sender, DrillActionEventArgs e)
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

        private void HandleRecentDashboardsControlRecentItemClick(object sender, RecentItemClickEventArgs e)
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

        private void HandleConfigureRowQueryItemClick(object sender, ItemClickEventArgs e)
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

        private void HandleDashboardDesignerPopupMenuShowing(object sender, DashboardPopupMenuShowingEventArgs e)
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

        private void HandleDashboardDesignerDashboardItemDoubleClick(object sender, DashboardItemMouseActionEventArgs e)
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

        private void HandleDashboardDesignerDashboardSaving(object sender, DashboardSavingEventArgs e)
        {
            try
            {
                bool saveAs = 
                    e.Command == DashboardSaveCommand.SaveAs ||
                    !SystemMethods.IsExtractInternal() &&
                    _dashboardShared.CustomData.CoreLicensed;

                if (_dashboardShared.CustomData.CoreLicensed && SystemMethods.IsExtractInternal())
                {
                    if (MessageBox.Show("This dashboard is set to be a Core licensed dashboard. Continue?",
                                        "Core Licensed Dashboard",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button2) !=
                        DialogResult.Yes)
                    {
                        e.Handled = true;
                        e.Saved = false;
                        return;
                    }
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveAs ||
                    e.Command == DashboardSaveCommand.Save &&
                    String.IsNullOrEmpty(_dashboardFileName))
                {
                    saveFileDialog.Filter = "ESDX|*.esdx|All|*.*";
                    saveFileDialog.DefaultExt = "esdx";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Only allow saving CoreLicense = true if internal license ( running on our network )
                        _dashboardShared.CustomData.CoreLicensed = SystemMethods.IsExtractInternal() &&
                            _dashboardShared.CustomData.CoreLicensed;

                        _dashboardFileName = saveFileDialog.FileName;
                        _dashboardShared.CustomData
                                        .AddExtractCustomDataToDashboardXml(CurrentDashboard.SaveToXDocument())
                                        .Save(_dashboardFileName, SaveOptions.None);

                        UpdateTitle();
                        e.Saved = true;
                    }
                }
                else
                {
                    _dashboardShared.CustomData
                                    .AddExtractCustomDataToDashboardXml(CurrentDashboard.SaveToXDocument())
                                    .Save(_dashboardFileName, SaveOptions.None);
                    e.Saved = true;
                }
                e.Handled = true;
                _dirty = false;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45303");
            }
        }

        private void HandleDashboardDesignerDashboardCreating(object sender, DashboardCreatingEventArgs e)
        {
            try
            {
                _dashboardFileName = null;
                _dirty = false;
                _dashboardShared.CustomData.ClearData();
                coreLicensedBarCheckItem.Checked = CoreLicensed;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45304");
            }
        }

        private void HandleDashboardDesignerDashboardChanged(object sender, EventArgs e)
        {
            try
            {
                _dirty = false;
                _dashboardShared.CustomData.AssignDataFromDashboardDefinition(CurrentDashboard.SaveToXDocument());
                coreLicensedBarCheckItem.Checked = CoreLicensed;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45305");
            }
        }

        private void HandleFileOpenButtonItemItemClick(object sender, BackstageViewItemEventArgs e)
        {
            try
            {
                FileBrowser fileBrowser = new FileBrowser();
                string selectedFile = fileBrowser.BrowseForFile("ESDX|*.esdx|XML|*.xml|All|*.*", string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
                {
                    _dashboardFileName = selectedFile;

                    var xdoc = XDocument.Load(_dashboardFileName, LoadOptions.PreserveWhitespace);
                    dashboardDesigner.Dashboard.LoadFromXDocument(xdoc);

                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45306");
            }
        }

        private void HandleFileOpenBarButtonItemItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                FileBrowser fileBrowser = new FileBrowser();
                string selectedFile = fileBrowser.BrowseForFile("ESDX|*.esdx|XML|*.xml|All|*.*", string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
                {
                    _dashboardFileName = selectedFile;

                    var xdoc = XDocument.Load(_dashboardFileName, LoadOptions.PreserveWhitespace);
                    dashboardDesigner.Dashboard.LoadFromXDocument(xdoc);

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
        private GridDetailConfiguration GetDetailConfigurationData(string component)
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
        private void UpdateTitle()
        {
            if (_dashboardFileName is null)
            {
                Text = "Dashboard creator";
            }
            else
            {
                string coreLicenseText = 
                    (SystemMethods.IsExtractInternal() && _dashboardShared.CustomData.CoreLicensed)
                        ? " - CORE LICENSED - "
                        : " - ";
                Text = _dashboardFileName + coreLicenseText + dashboardDesigner.Dashboard.Title.Text;
            }
        }

        /// <summary>
        /// Enables or disables the 'Configure Extract settings' menu item
        /// </summary>
        /// <param name="menu">the menu to show or hide the menu item</param>
        /// <param name="show">if <c>true</c> make the menu item visible. if <c>false</c> hide the menu item</param>
        private static void ShowConfigureExtractSettingsMenuItem(PopupMenu menu, bool show)
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
