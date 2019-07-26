using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.Utils.Extensions;
using DevExpress.XtraBars;
using Extract.Dashboard.Forms;
using Extract.FileActionManager.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Extract.Dashboard.Utilities
{
    /// <summary>
    /// Class used to share code between DashboardCreator and DashboardViewer
    /// </summary>
    /// <typeparam name="T">Type that Implements <see cref="IExtractDashboardCommon"/></typeparam>
    public class DashboardShared<T> : IDisposable where T : IExtractDashboardCommon
    {
        #region Constants

        string _BEGIN_GROUP = "BeginGroup";
        string _EXPORT_TO = "Export To";

        #endregion

        #region Structs

        /// <summary>
        /// Represents an entry from the FAM DB's FileHandler table
        /// </summary>
        struct FileHandlerItem
        {
            /// <summary>
            /// Gets or sets the name the application should be presented to the user as.
            /// </summary>
            /// <value>
            /// The name the application should be presented to the user as.
            /// </value>
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the full to the executable to run.
            /// </summary>
            /// <value>
            /// The full to the executable to run.
            /// </value>
            public string ApplicationPath
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the command-line arguments to use for the application. The
            /// SourceDocName path tag and path functions are supported.
            /// </summary>
            /// <value>
            /// The command-line arguments to use for the application.
            /// </value>
            public string Arguments
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the application item should be available for
            /// multiple files at once.
            /// </summary>
            /// <value><see langword="true"/> if the application item should be available for
            /// multiple files at once; <see langword="false"/> if the application item should be
            /// allowed for only one file at a time.
            /// </value>
            public bool AllowMultipleFiles
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the application supports the /ef
            /// command-line parameter.
            /// </summary>
            /// <value><see langword="true"/> if the application supports the /ef command-line
            /// parameter; otherwise, <see langword="false"/>.
            /// </value>
            public bool SupportsErrorHandling
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the launched application should block until
            /// complete.
            /// </summary>
            /// <value><see langword="true"/> if application should block until
            /// complete; <see langword="false"/> if the application run in the background without
            /// blocking.</value>
            public bool Blocking
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the workflow.
            /// </summary>
            /// <value>
            /// The workflow.
            /// </value>
            public string Workflow
            {
                get;
                set;
            }
        }

        #endregion Structs

        #region Fields

        /// <summary>
        /// The form that implements IExtractDashboardCommon
        /// </summary>
        T _dashboardForm;

        /// <summary>
        /// Field to indicate if the Export menu item needs to be put in 
        /// </summary>
        bool _withExportMenu;

        /// <summary>
        /// List of menu items to add to the context menu
        /// </summary>
        Collection<BarItem> _menuItems;

        /// <summary>
        /// Menu that the configured dashboard open items will be placed on
        /// </summary>
        BarSubItem _dashboardOpenSubMenu;

        /// <summary>
        /// Flag indicating that existing <see cref="_menuItems"/> need to be removed and replaced with updated menu 
        /// items
        /// </summary>
        bool _menuNeedsUpdating;

        /// <summary>
        /// Allows background non-blocking file handler operations to be cancelled before processing
        /// any additional files.
        /// </summary>
        CancellationTokenSource _fileHandlerCanceler = new CancellationTokenSource();

        /// <summary>
        /// Tracks how many file handler operations are currently executing.
        /// </summary>
        CountdownEvent _fileHandlerCountdownEvent = new CountdownEvent(0);

        #endregion

        #region Public Properties

        /// <summary>
        /// The key used is the control name
        /// </summary>
        public Dictionary<string, GridDetailConfiguration> CustomGridValues { get; } = new Dictionary<string, GridDetailConfiguration>();

        /// <summary>
        /// Property for the list of context menu items. Uses <see cref="_menuItems"/> to store the list
        /// </summary>
        public Collection<BarItem> ContextFileMenuActions
        {
            get
            {
                if (_menuItems is null)
                {
                    CreateContextFileMenuActionsList();
                    _menuNeedsUpdating = false;
                }
                return _menuItems;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs <see cref="DashboardShared{T}"/> with the given form with out adding export menu context.
        /// Should be used by designer control
        /// </summary>
        /// <param name="dashboardForm">Form that implements the <see cref="IExtractDashboardCommon"/> interface that 
        /// either contains a dashboard viewer or dashboard designer</param>
        public DashboardShared(T dashboardForm)
        {
            _dashboardForm = dashboardForm;
            _withExportMenu = false;
            _dashboardOpenSubMenu = CreateOpenDashboardSubMenu();
        }

        /// <summary> 
        /// Constructs <see cref="DashboardShared{T}"/> with the given form that adds export menu based on parameter
        /// </summary>
        /// <param name="dashboardForm">Form that implements the <see cref="IExtractDashboardCommon"/> interface that 
        /// either contains a dashboard viewer or dashboard designer</param>
        /// <param name="withExportMenu">Flag indicating whether or not an export menu should be added to the context menu</param>
        public DashboardShared(T dashboardForm, bool withExportMenu)
        {
            _dashboardForm = dashboardForm;

            _withExportMenu = withExportMenu;

            _dashboardOpenSubMenu = CreateOpenDashboardSubMenu();
        }

        #endregion

        #region Public Methods

        #region Handlers for Designer and Viewer

        public void HandleConfigureDataConnection(object sender, ConfigureDataConnectionEventArgs e)
        {
            try
            {
                SqlServerConnectionParametersBase sqlParameters = e.ConnectionParameters as SqlServerConnectionParametersBase;

                // Since the database connection is changing the custom context menu items should be removed
                _menuNeedsUpdating = true;

                // Only override SQL Server connection
                if (sqlParameters == null)
                {
                    return;
                }

                _dashboardForm.ConfiguredServerName = sqlParameters.ServerName;
                _dashboardForm.ConfiguredDatabaseName = sqlParameters.DatabaseName;

                // Set timeout to 0 (infinite) for all DataSources
                foreach (var ds in _dashboardForm.CurrentDashboard.DataSources)
                {
                    var sqlDataSource = ds as DashboardSqlDataSource;
                    if (sqlDataSource != null)
                    {
                        sqlDataSource.ConnectionOptions.DbCommandTimeout = 0;
                    }
                }

                if (!_dashboardForm.IsDatabaseOverridden)
                {
                    return;
                }
                sqlParameters.ServerName = _dashboardForm.ServerName;
                sqlParameters.DatabaseName = _dashboardForm.DatabaseName;

            }
            catch (Exception ex)
            {
                // Throw so the calling Event handler in Viewer or Creator can handle
                throw ex.AsExtract("ELI46176");
            }
        }

        public void HandlePopupMenuShowing(object sender, DashboardPopupMenuShowingEventArgs e)
        {
            try
            {
                // Clear the current filtered files
                _dashboardForm.CurrentFilteredFiles.Clear();

                var grid = _dashboardForm.CurrentDashboard.Items[e.DashboardItemName] as GridDashboardItem;

                // check if the context menu needs to be updated
                if (_menuNeedsUpdating)
                {
                    // remove the custom items
                    RemoveCustomContextMenus(e.Menu);
                    _menuNeedsUpdating = false;
                }

                // if menu is not for a grid control there is nothing else to do
                if (grid is null)
                {
                    return;
                }

                IEnumerable<BarItemLink> extractMenuLinks = PreprareExtractMenu(e.Menu, grid);

                SetupExportMenus(e.Menu, e.DashboardItemName);

                // Add the selected files
                _dashboardForm.CurrentFilteredFiles.AddRange(GetSelectedFiles(e.DashboardItemName));

                _dashboardForm.CurrentFilteredDimensions.Clear();

                // For now only one row should be selected
                var selectRowsDimensions = GetSelectedRowsDimensions(e.DashboardItemName);
                if (selectRowsDimensions.Count() == 1)
                {
                    var dimensions = selectRowsDimensions?.FirstOrDefault();
                    if (dimensions != null)
                    {
                        _dashboardForm.CurrentFilteredDimensions.AddRange(dimensions);
                    }
                }

                MakeMenusVisible(extractMenuLinks);
            }
            catch (Exception ex)
            {
                // Throw so the calling Event handler in Viewer or Creator can handle
                throw ex.AsExtract("ELI46967");
            }
        }

        public void HandleGridDashboardItemDoubleClick(object sender, DashboardItemMouseActionEventArgs e)
        {
            try
            {
                if (CustomGridValues.ContainsKey(e.DashboardItemName))
                {
                    GridDashboardItem gridItem = _dashboardForm.CurrentDashboard.Items[e.DashboardItemName] as GridDashboardItem;

                    if (gridItem is null)
                    {
                        return;
                    }

                    int drillLevel;
                    _dashboardForm.DrilldownLevelForItem.TryGetValue(e.DashboardItemName, out drillLevel);

                    bool drillDownLevelIncreased;
                    _dashboardForm.DrilldownLevelIncreased.TryGetValue(e.DashboardItemName, out drillDownLevelIncreased);

                    if (!gridItem.InteractivityOptions.IsDrillDownEnabled ||
                        drillDownLevelIncreased && (gridItem.GetDimensions().Count - 1 == drillLevel))
                    {
                        DisplayDashboardDetailForm(gridItem, e);
                    }
                    _dashboardForm.DrilldownLevelIncreased[e.DashboardItemName] = false;
                }
            }
            catch (Exception ex)
            {
                // Throw so the calling Event handler in Viewer or Creator can handle
                throw ex.AsExtract("ELI46177");
            }
        }

        #endregion

        /// <summary>
        /// Extracts the GridDetailConfiguration for the configured controls from the xml
        /// </summary>
        /// <param name="xml">UserData portion of XML</param>
        /// <returns>Dictionary with ConfigurationDetailConfigurations for configured components from the XML</returns>
        public void GridConfigurationsFromXml(XNode xml)
        {
            try
            {
                CustomGridValues.Clear();

                if (xml is null)
                {
                    return;
                }

                var extractGrids = xml.XPathSelectElements("//ExtractConfiguredGrids/Component");

                foreach (var e in extractGrids)
                {
                    var dashboardItem = _dashboardForm.CurrentDashboard.Items.Contains(i => i.ComponentName == e.Attribute("Name").Value);
                    if (dashboardItem)
                    {
                        CustomGridValues[e.Attribute("Name").Value] = new GridDetailConfiguration
                        {
                            RowQuery = e.Element("RowQuery").Value,
                            DataMemberUsedForFileName = e.Element("DataMemberUsedForFileName")?.Value ?? "FileName",
                            DashboardLinks = e.Element("DashboardLinks")?.Value.Split(',')
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToArray().ToHashSet() ?? new HashSet<string>()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46968");
            }
        }

        /// <summary>
        /// Displays a dialog if there are active custom non blocking file handlers running
        /// </summary>
        /// <returns>True if the there are no file handlers running or they have been stopped. 
        /// false if user decides not to close</returns>
        public bool RequestDashboardClose()
        {
            if (!_fileHandlerCountdownEvent.Wait(0))
            {
                if (DialogResult.OK == MessageBox.Show(
                    "One or more operations are still running.\r\n\r\n" +
                    "Stop the operation(s) before the next file is processed " +
                    "and close the application?",
                    "Stop operation?", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, 0))
                {
                    _fileHandlerCanceler.Cancel();
                    try
                    {
                        // While waiting for the background process(es) to stop, display a modal
                        // message box.
                        ShowMessageBoxWhileBlocking("Waiting for operation(s) to stop...", "Dashboard",
                            () => _fileHandlerCountdownEvent.Wait());
                    }
                    catch { }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Private Methods

        #region Menu Creation

        /// <summary>
        /// Creates the <see cref="_menuItems"/> to be displayed in the context menu of a grid if FileName is in the grid
        /// </summary>
        void CreateContextFileMenuActionsList()
        {
            // Add the common File Menu actions
            Collection<BarItem> items = new Collection<BarItem>();
            FileHandlerItem handlerItem = new FileHandlerItem { AllowMultipleFiles = true };
            Dictionary<string, object> menuItemData = new Dictionary<string, object>
            {
                { "FileHanderRecord", handlerItem },
                { _BEGIN_GROUP, false }
            };
            Dictionary<string, object> menuItemDataBeginGroup = new Dictionary<string, object>
            {
                { "FileHanderRecord", handlerItem },
                { _BEGIN_GROUP, true }
            };

            BarButtonItem newItem = new BarButtonItem();
            // Copy filename(s)
            newItem.Caption = "&Copy filename(s)";
            newItem.Name = "copyFileNames";
            newItem.Tag = menuItemDataBeginGroup;

            newItem.ItemClick += HandleCopyFilenameItemClick;
            items.Add(newItem);

            newItem = new BarButtonItem();
            newItem.Caption = "Copy file(s)";
            newItem.Name = "copyFiles";
            newItem.Tag = menuItemData;
            newItem.ItemClick += HandleCopyFileItemClick;
            items.Add(newItem);

            newItem = new BarButtonItem();
            newItem.Caption = "Copy file(s) and data";
            newItem.Name = "copyFilesAndData";
            newItem.Tag = menuItemData;
            newItem.ItemClick += HandleCopyFileAndDataItemClick;
            items.Add(newItem);

            newItem = new BarButtonItem();
            newItem.Caption = "Open file location";
            newItem.Name = "openFileLocation";
            newItem.Tag = new Dictionary<string, object>
            {
                { "FileHanderRecord", new FileHandlerItem { AllowMultipleFiles = false } },
                { _BEGIN_GROUP, true }

            };
            newItem.ItemClick += HandleOpenFileLocationItemClick;
            items.Add(newItem);

            items.Add(_dashboardOpenSubMenu);

            if (_withExportMenu)
            {

                newItem = new BarButtonItem();
                newItem.Caption = "Export To Excel";
                newItem.Name = "exportToExcel";
                newItem.Tag = new Dictionary<string, object>
                {
                    { "FileHanderRecord", null },
                    { _BEGIN_GROUP, true }
                };
                newItem.ItemClick += HandleExportToExcelItemClick;
                items.Add(newItem);
            }

            AddItemsFromDB(items);

            _menuItems = items;

        }

        /// <summary>
        /// Creates the menu item that will contain the dashboards that are configured to be opened from the context menu
        /// </summary>
        /// <returns></returns>
        BarSubItem CreateOpenDashboardSubMenu()
        {
            var dashboardSubItems = new BarSubItem();
            dashboardSubItems.Tag = new Dictionary<string, object>
            {
                {"DashboardItems", true },
                { _BEGIN_GROUP, true }
            };
            dashboardSubItems.Caption = "Open Dashboard";
            dashboardSubItems.Enabled = true;
            dashboardSubItems.HideWhenEmpty = true;
            return dashboardSubItems;
        }

        /// <summary>
        /// Adds the configured dashboard menu items to the dashboardSubItems menu used to open dashboard menu links
        /// </summary>
        /// <param name="dashboardLinks"></param>
        void AddConfiguredDashboardOpenMenus(HashSet<string> dashboardLinks)
        {
            try
            {
                _dashboardOpenSubMenu.ClearLinks();
                var singleItemDict = new Dictionary<string, object>
                {
                    { "DashboardMenuItem", true } ,
                    { _BEGIN_GROUP, false }
                };
                var missingDashboards = GetMissingDashboardsInDatabase(dashboardLinks);
                foreach (var link in dashboardLinks.Except(missingDashboards, StringComparer.OrdinalIgnoreCase))
                {
                    var newItem = new BarButtonItem();
                    newItem.Caption = link;
                    newItem.Tag = singleItemDict;
                    newItem.ItemClick += HandleOpenDashboardMenuClick;
                    _dashboardOpenSubMenu.AddItem(newItem);
                }
                
                // Handle the missing dashboards
                if (missingDashboards.Count > 0)
                {
                    var staticItem = new BarHeaderItem();
                    staticItem.Caption = "Missing dashboards";
                    _dashboardOpenSubMenu.AddItem(staticItem);
                    foreach (var missing in missingDashboards)
                    {
                        var missingItem = new BarButtonItem();
                        missingItem.Caption = missing;
                        _dashboardOpenSubMenu.AddItem(missingItem);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47183");
            }
        }

        /// <summary>
        /// Returns a list of the dashboards in the <paramref name="dashboardLinks"/> that are not
        /// currently saved in the database.
        /// </summary>
        /// <param name="dashboardLinks">Set of dashboards</param>
        /// <returns>A list of dashboards that are not in the database that were in the <paramref name="dashboardLinks"/></returns>
        IList<string> GetMissingDashboardsInDatabase(HashSet<string> dashboardLinks)
        {
            try
            {
                using (var connection = NewSqlDBConnection())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "Select DashboardName FROM dbo.Dashboard";
                    connection.Open();
                    var dashboardsInDatabase = cmd.ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select(r => r.GetString(0));
                    return dashboardLinks.Except(dashboardsInDatabase, StringComparer.OrdinalIgnoreCase).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47190");
            }

        }

        /// <summary>
        /// Adds menuItems from the current configured Database FileHandler table to the items parameter"/>
        /// </summary>
        /// <param name="items">List of menu items to add the new menus</param>
        void AddItemsFromDB(Collection<BarItem> items)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_dashboardForm.ServerName) || string.IsNullOrWhiteSpace(_dashboardForm.DatabaseName))
                {
                    return;
                }

                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT [AppName], [ApplicationPath], [Arguments], [AllowMultipleFiles], 
                                [SupportsErrorHandling], [Blocking], [WorkflowName]
                            FROM [FileHandler] WHERE [Enabled] = 1 AND [AdminOnly] = 0 
                            ORDER BY [AppName]";

                        var results = cmd.ExecuteReader().Cast<IDataRecord>();

                        var fileHandlers = results.Select(row => new FileHandlerItem
                        {
                            Name = (string)row["AppName"],
                            ApplicationPath = (string)row["ApplicationPath"],
                            Arguments = row["Arguments"].ToString(),
                            AllowMultipleFiles = (bool)row["AllowMultipleFiles"],
                            SupportsErrorHandling = (bool)row["SupportsErrorHandling"],
                            Blocking = (bool)row["Blocking"],
                            Workflow = (string)((row["WorkflowName"] == DBNull.Value) ? string.Empty : row["WorkflowName"])
                        }).ToList();

                        bool firstInGroupSet = false;

                        foreach (var fileHander in fileHandlers)
                        {
                            var menuItem = new BarButtonItem();

                            menuItem.Caption = fileHander.Name;

                            if (!firstInGroupSet)
                            {
                                menuItem.Tag = new Dictionary<string, object>
                                {
                                    { "FileHanderRecord", fileHander },
                                    { _BEGIN_GROUP, true }
                                };
                                firstInGroupSet = true;
                            }
                            else
                            {
                                menuItem.Tag = new Dictionary<string, object>
                                {
                                    { "FileHanderRecord", fileHander },
                                    { _BEGIN_GROUP, false }
                                };
                            }
                            menuItem.ItemClick += HandleCustomFileHanderClick;
                            items.Add(menuItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46207");
            }
        }

        #endregion

        #region Handlers for Custom menu items

        void HandleOpenDashboardMenuClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _dashboardForm.OpenDashboardForm(e.Item.Caption, _dashboardForm.CurrentFilteredDimensions);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47064");
            }
        }

        void HandleCustomFileHanderClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Dictionary<string, object> menuItemData = e.Item.Tag as Dictionary<string, object>;
                FileHandlerItem? nullableHandlerItem = menuItemData["FileHanderRecord"] as FileHandlerItem?;

                if (nullableHandlerItem is null)
                {
                    return;
                }

                FileHandlerItem handlerItem = (FileHandlerItem)nullableHandlerItem;

                if (_dashboardForm.CurrentFilteredFiles?.Count() == 0)
                {
                    return;
                }

                if (handlerItem.Blocking == true)
                {
                    // If blocking, use a modal message box to block rather than calling
                    // RunApplication on this thread; the latter causes the for to report "not
                    // responding" in some circumstances.
                    ShowMessageBoxWhileBlocking("Running " + handlerItem.Name.Quote() + "...", "Dashboard",
                        () => RunApplication(handlerItem, _dashboardForm.CurrentFilteredFiles, _fileHandlerCanceler.Token));
                }
                else
                {
                    // If not blocking, run the application on a background thread; allow the UI
                    // thread to continue.
                    try
                    {
                        // Increment the current _fileHandlerCountdownEvent to prevent the form from
                        // closing while the file handler item is still running. (AddCount cannot be
                        // called when the CurrentCount is already zero.)
                        if (!_fileHandlerCountdownEvent.TryAddCount())
                        {
                            _fileHandlerCountdownEvent.Reset(1);
                        }

                        Task.Factory.StartNew(() =>
                            RunApplication(handlerItem, _dashboardForm.CurrentFilteredFiles, _fileHandlerCanceler.Token),
                                _fileHandlerCanceler.Token)
                        .ContinueWith((task) =>
                        {
                            // Regardless of whether there was a failure, indicate that the file
                            // handler process has finished.
                            _fileHandlerCountdownEvent.Signal();

                            // Handle any failure launching the operation to prevent unhandled
                            // exceptions from crashing the application.
                            if (task.IsFaulted)
                            {
                                Exception[] exceptions = task.Exception.InnerExceptions.ToArray();
                                _dashboardForm.SafeBeginInvokeForShared("ELI46215", () =>
                                {
                                    foreach (Exception ex in exceptions)
                                    {
                                        ex.ExtractDisplay("ELI46216");
                                    }
                                });
                            }
                        });
                    }
                    catch
                    {
                        // In case the file handler task was never started, return
                        // _fileHandlerCountdownEvent to its previous value.
                        _fileHandlerCountdownEvent.Signal();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {

                ex.ExtractDisplay("ELI46210");
            }
        }

        void HandleExportToExcelItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Dictionary<string, object> menuItemData = e.Item.Tag as Dictionary<string, object>;

                if (menuItemData is null)
                {
                    return;
                }

                if (_dashboardForm?.Viewer != null)
                {
                    string dashboardItemName = menuItemData["DashboardItemName"] as string;
                    if (string.IsNullOrWhiteSpace(dashboardItemName))
                    {
                        return;
                    }
                    _dashboardForm.Viewer.ShowExportDashboardItemDialog(dashboardItemName, DashboardExportFormat.Excel);
                }
            }
            catch (Exception ex)
            {

                ex.ExtractDisplay("ELI46209");
            }
        }


        void HandleCopyFileAndDataItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Dictionary<string, object> menuItemData = e.Item.Tag as Dictionary<string, object>;

                if (menuItemData is null)
                {
                    return;
                }

                Clipboard.Clear();
                if (_dashboardForm.CurrentFilteredFiles.Count() > 0)
                {

                    StringCollection fileCollection = new StringCollection();
                    fileCollection.AddRange(_dashboardForm.CurrentFilteredFiles
                        .SelectMany(fileName => new[] { fileName }
                            .Concat(Directory.EnumerateFiles(
                                Path.GetDirectoryName(fileName),
                                Path.GetFileName(fileName) + "*")
                                .Where(dataFileName =>
                                    dataFileName.StartsWith(fileName + ".", StringComparison.OrdinalIgnoreCase))))
                        .ToArray());
                    Clipboard.SetFileDropList(fileCollection);
                }
            }
            catch (Exception ex)
            {

                ex.ExtractDisplay("ELI46203");
            }
        }

        void HandleOpenFileLocationItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Dictionary<string, object> menuItemData = e.Item.Tag as Dictionary<string, object>;

                if (menuItemData is null)
                {
                    return;
                }

                if (_dashboardForm.CurrentFilteredFiles.Count() > 0)
                {
                    string fileName = _dashboardForm.CurrentFilteredFiles.Single();

                    string argument = null;

                    if (File.Exists(fileName))
                    {
                        argument = "/select," + fileName.Quote();
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(fileName);
                        if (Directory.Exists(directory))
                        {
                            argument = "/root," + directory.Quote();
                        }
                        else
                        {
                            UtilityMethods.ShowMessageBox(
                                "Neither the file nor its containing directory could be found.",
                                "File not found.", true);
                            return;
                        }
                    }

                    using (var process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo("explorer.exe", argument);
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        process.Start();
                    }


                }
            }
            catch (Exception ex)
            {

                ex.ExtractDisplay("ELI46653");
            }
        }

        void HandleCopyFilenameItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Dictionary<string, object> menuItemData = e.Item.Tag as Dictionary<string, object>;

                if (menuItemData is null)
                {
                    return;
                }

                Clipboard.Clear();
                if (_dashboardForm.CurrentFilteredFiles.Count() > 0)
                {
                    Clipboard.SetText(string.Join("\r\n", _dashboardForm.CurrentFilteredFiles));
                }
            }
            catch (Exception ex)
            {

                ex.ExtractDisplay("ELI46654");
            }
        }

        void HandleCopyFileItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Dictionary<string, object> menuItemData = e.Item.Tag as Dictionary<string, object>;

                if (menuItemData is null)
                {
                    return;
                }

                Clipboard.Clear();
                if (_dashboardForm.CurrentFilteredFiles.Count() > 0)
                {
                    StringCollection fileCollection = new StringCollection();
                    fileCollection.AddRange(_dashboardForm.CurrentFilteredFiles.ToArray());
                    Clipboard.SetFileDropList(fileCollection);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46204");
            }
        }

        #endregion

        #region Private Helper functions

        void MakeMenusVisible(IEnumerable<BarItemLink> extractMenuLinks)
        {
            int numberOfFiles = _dashboardForm.CurrentFilteredFiles.Count;
            if (numberOfFiles > 0)
            {
                extractMenuLinks.First().BeginGroup = true;

                foreach (var item in extractMenuLinks)
                {
                    SetMenuVisiblity(numberOfFiles, item);
                }
            }

            // Make the configured open dashboard menu items available
            if (_dashboardForm.CurrentFilteredDimensions.Count != 0)
            {
                // Find the link for dashboardOpenSubMenu
                var menuLink = extractMenuLinks
                    .Where(l => l.Item == _dashboardOpenSubMenu as BarItem).Single();
                menuLink.Visible = true;
                SetMenuVisiblity(1, menuLink);
            }
        }

        static void SetMenuVisiblity(int numberOfItems, BarItemLink item)
        {
            var subMenu = item.Item as BarSubItem;
            if (subMenu != null)
            {
                foreach (BarItemLink subItem in subMenu.ItemLinks)
                {
                    subItem.Item.Manager = subMenu.Manager;
                    subItem.Item.Enabled = subItem.Item.Tag != null && (SupportsMultipleItems(subItem) || numberOfItems == 1);
                    subItem.Visible = true;

                    if (subItem.Item is BarSubItem)
                    {
                        SetMenuVisiblity(numberOfItems, subItem);
                    }
                }
            }

            item.Item.Enabled = SupportsMultipleItems(item) || numberOfItems == 1;
            item.Visible = true;
        }

        void SetupExportMenus(PopupMenu menu, string dashboardItemName)
        {
            var exportMenus = menu.ItemLinks.Where(link => link.Caption.Contains(_EXPORT_TO));
            foreach (var link in exportMenus)
            {
                Dictionary<string, object> menuItemData = link.Item.Tag as Dictionary<string, object>;
                if (menuItemData != null)
                {
                    menuItemData["DashboardItemName"] = dashboardItemName;
                }
                link.Visible = true;
            }
        }

        IEnumerable<BarItemLink> PreprareExtractMenu(PopupMenu menu, GridDashboardItem grid)
        {
            // get the extract menu item links
            var extractMenuLinks = menu.ItemLinks.Where(link => IsExtractMenuItem(link));

            // If none were found add them and this is for a grid control add the menus
            if (extractMenuLinks.Count() == 0 && grid != null)
            {
                // Add the defined context menu items
                menu.AddItems(ContextFileMenuActions.ToArray());

                // Get the links of the added menu items
                extractMenuLinks = menu.ItemLinks.Where(link => IsExtractMenuItem(link));

                // Define the start of groups
                var startOfGroups = extractMenuLinks.Where(link =>
                {
                    Dictionary<string, object> menuItemData = link.Item.Tag as Dictionary<string, object>;
                    if (menuItemData != null)
                    {
                        return true == menuItemData[_BEGIN_GROUP] as bool?;
                    }
                    return false;
                }).ToList();
                startOfGroups.ForEach(l => l.BeginGroup = true);
            }

            if (CustomGridValues.TryGetValue(grid.ComponentName, out var gridDetail) && gridDetail.DashboardLinks.Count > 0)
            {
                AddConfiguredDashboardOpenMenus(gridDetail.DashboardLinks);
            }

            // if menu items exists hide them - the should only be visible if this is a grid
            foreach (var item in extractMenuLinks)
            {
                var subMenu = item.Item as BarSubItem;
                if (subMenu != null)
                {
                    foreach (BarItemLink subItem in subMenu.ItemLinks)
                    {
                        subItem.Visible = false;
                    }
                }
                item.Visible = false;
            }

            return extractMenuLinks;
        }

        /// <summary>
        /// Display the DashboardDetailForm 
        /// </summary>
        /// <param name="gridItem">Grid item that needs to display the detail</param>
        /// <param name="e">Event args</param>
        /// <param name="configuration">The configuration to pass to the DashboardDetailForm</param>
        void DisplayDashboardDetailForm(GridDashboardItem gridItem, DashboardItemMouseActionEventArgs e)
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

                        var customValues = CustomGridValues[e.DashboardItemName];

                        // the form will only be displayed if there is a FileName specified and the datasource is SQL database
                        if (columnValues.Count > 0
                            && columnValues.ContainsKey(customValues.DataMemberUsedForFileName)
                            && !string.IsNullOrWhiteSpace(_dashboardForm.ServerName)
                            && !string.IsNullOrWhiteSpace(_dashboardForm.DatabaseName))
                        {
                            if (File.Exists(((string)columnValues[customValues.DataMemberUsedForFileName])))
                            {
                                DashboardFileDetailForm detailForm = new DashboardFileDetailForm(
                                    columnValues, _dashboardForm.ServerName, _dashboardForm.DatabaseName, CustomGridValues[e.DashboardItemName]);
                                detailForm.ShowDialog();
                            }
                            else
                            {
                                ExtractException ee = new ExtractException("ELI46114", "File does not exist.");
                                if (customValues != null)
                                {
                                    ee.AddDebugData(customValues.DataMemberUsedForFileName, (string)columnValues[customValues.DataMemberUsedForFileName], false);
                                }
                                throw ee;
                            }
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
        /// Test if <see cref="BarItemLink"/> argument is an Extract menu items by checking for Item.Tag being a 
        /// <see cref="Dictionary{String, Object}"/> type
        /// </summary>
        /// <param name="link">The <see cref="BarItemLink"/> to check</param>
        /// <returns>If it is a Extract menu item return true else return false</returns>
        static bool IsExtractMenuItem(BarItemLink link)
        {
            Dictionary<string, object> itemMenuData = link.Item.Tag as Dictionary<string, object>;
            if (itemMenuData is null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if <see cref="BarItemLink"/> argument supports multiple items
        /// </summary>
        /// <param name="link">The <see cref="BarItemLink"/> to check</param>
        /// <returns>returns true if it is not Extract menu object or it supports multiple items otherwise returns false</returns>
        static bool SupportsMultipleItems(BarItemLink link)
        {
            Dictionary<string, object> itemMenuData = link.Item.Tag as Dictionary<string, object>;
            if (itemMenuData is null)
            {
                return true;
            }
            if (itemMenuData.TryGetValue("FileHanderRecord", out var fileHandler))
            {
                FileHandlerItem? fileHandlerItem = fileHandler as FileHandlerItem?;
                return fileHandlerItem?.AllowMultipleFiles != false;
            }

            return false;
        }

        /// <summary>
        /// Removes the menuItemLinks in the menu parameter that are in the _menuItems list
        /// </summary>
        /// <param name="menu">Menu to remove the menu item links</param>
        void RemoveCustomContextMenus(PopupMenu menu)
        {
            try
            {
                if (_menuItems != null)
                {
                    _menuItems.ForEach(m =>
                    {
                        var link = menu.ItemLinks.SingleOrDefault(l => l.Item == m);
                        if (link != null)
                        {
                            menu.RemoveLink(link);
                        }
                    });
                    _menuItems = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46208");
            }
        }


        /// <summary>
        /// Returns a connection to the configured database
        /// </summary>
        /// <returns>SqlConnection that connects to the Server and Database from the form</returns>
        SqlConnection NewSqlDBConnection()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = _dashboardForm.ServerName;
            sqlConnectionBuild.InitialCatalog = _dashboardForm.DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }


        /// <summary>
        /// Shows a modal, non-closeable message box with the specified <see param="messageText"/>
        /// while the specified <see param="action"/> runs on a background thread.
        /// </summary>
        /// <param name="messageText">The message to be displayed.</param>
        /// <param name="action">The action to run on a background thread.</param>
        void ShowMessageBoxWhileBlocking(string messageText, string caption, Action action)
        {
            var messageBox = new CustomizableMessageBox();
            messageBox.UseDefaultOkButton = false;
            messageBox.Caption = caption;
            messageBox.Text = messageText;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                    _dashboardForm.SafeBeginInvokeForShared("ELI46211", () => messageBox.Close(string.Empty));
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI46212");
                }
                finally
                {
                    _dashboardForm.SafeBeginInvokeForShared("ELI46213", () => messageBox.Dispose());
                }
            })
            .ContinueWith((task) =>
            {
                // Handle any exceptions to prevent unhandled exceptions from crashing the
                // application.
                foreach (Exception ex in task.Exception.InnerExceptions)
                {
                    ex.ExtractLog("ELI46214");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            messageBox.Show(_dashboardForm);
        }


        /// <summary>
        /// Launches the specified <see paramref="fileNames"/> in the application defined by
        /// <see paramref="fileHanderItem"/>.
        /// </summary>
        /// <param name="fileHanderItem">The <see cref="FileHandlerItem"/> defining the application to
        /// be run.</param>
        /// <param name="fileNames">The files to be run in <see paramref="appLaunchItem"/></param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the that should be checked
        /// before each file to see if the operation has been canceled.</param>
        void RunApplication(FileHandlerItem fileHanderItem, IEnumerable<string> fileNames,
            CancellationToken cancelToken)
        {
            try
            {
                // Collects any exceptions that occur when processing the files.
                var exceptions = new List<ExtractException>();

                // Process each filename in sequence.
                foreach (string fileName in fileNames)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    try
                    {
                        // Expand the command line arguments using path tags/functions.
                        FileActionManagerPathTags pathTags = new FileActionManagerPathTags(null, fileName);
                        pathTags.DatabaseServer = _dashboardForm.ServerName;
                        pathTags.DatabaseName = _dashboardForm.DatabaseName;
                        pathTags.Workflow = fileHanderItem.Workflow;

                        string applicationPath = fileHanderItem.ApplicationPath;
                        if (!string.IsNullOrEmpty(applicationPath))
                        {
                            applicationPath = pathTags.Expand(applicationPath);
                        }

                        string arguments = fileHanderItem.Arguments;
                        if (!string.IsNullOrEmpty(arguments))
                        {
                            arguments = pathTags.Expand(arguments);
                        }

                        if (fileHanderItem.SupportsErrorHandling)
                        {
                            SystemMethods.RunExtractExecutable(applicationPath, arguments, cancelToken);
                        }
                        else
                        {
                            SystemMethods.RunExecutable(applicationPath, arguments, int.MaxValue, false, cancelToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex.AsExtract("ELI35820"));
                    }
                }

                int exceptionCount = exceptions.Count;
                if (exceptionCount > 0)
                {
                    // If there was only a single file selected, just throw the exception as-is.
                    if (fileNames.Count() == 1)
                    {
                        throw exceptions.First();
                    }
                    // If more than one file was selected report all exceptions in one aggregate
                    // exception after processing.
                    else
                    {
                        // Aggregating a large number of exceptions can bog down, potentially
                        // making the quickly making the app appear hung. Aggregate a maximum of 10
                        // exceptions.
                        var exceptionsToAggregate = exceptions.Take(10).Union(new[] {
                            new ExtractException("ELI35819",
                                string.Format(CultureInfo.CurrentCulture,
                                "{0:D} file(s) failed {1}", exceptionCount, fileHanderItem.Name.Quote())) });

                        throw ExtractException.AsAggregateException(exceptionsToAggregate);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // If canceled, the user didn't want to wait around for the operation to complete;
                // they don't need to see an exception about the operation being canceled.
            }
            catch (Exception ex)
            {
                // [DotNetRCAndUtils:1029]
                // Exceptions should be displayed on the UI thread, blocking it which prevents the
                // form from being closed which can lead to a crash. Using BeginInvoke does not
                // always block the UI thread (not sure why) so use Invoke instead to guarantee the
                // UI thread is blocked.
                _dashboardForm.Invoke((MethodInvoker)(() =>
                {
                    ex.ExtractDisplay("ELI35811");
                }), null);
            }
        }

        /// <summary>
        /// Gets the currently selected files using the configured <see cref="GridDetailConfiguration.DataMemberUsedForFileName"/>
        /// as the column containing the filenames
        /// </summary>
        /// <param name="gridName">The ComponentName of the grid to get the selected files</param>
        /// <returns><see cref="IEnumerable{String}"/> that containsthe selected files</returns>
        IEnumerable<string> GetSelectedFiles(string gridName)
        {
            GridDetailConfiguration configuration;

            if (CustomGridValues.TryGetValue(gridName, out configuration)
                && !string.IsNullOrWhiteSpace(configuration.DataMemberUsedForFileName))
            {
                return GetSelectedRowsDimensions(gridName)
                    .Where(row => row.ContainsKey(configuration.DataMemberUsedForFileName))
                    .Select(row => row[configuration.DataMemberUsedForFileName].ToString());
            }

            return new List<string>();
        }

        /// <summary>
        /// Get Selected rows as IEnumerable of dictionaries that has all the dimensions and values for those dimensions
        /// </summary>
        /// <param name="gridName">The ComponentName of the grid to get the selected files</param>
        /// <returns></returns>
        IEnumerable<Dictionary<string, object>> GetSelectedRowsDimensions(string gridName)
        {
            var axisPointTuples = _dashboardForm.GetCurrentFilterValues(gridName);

            return axisPointTuples.Select(at => at.ToDictionary())?
                .Where(v => v != null);
        }

        #endregion

        #endregion

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileHandlerCanceler?.Dispose();
                _fileHandlerCanceler = null;
                _fileHandlerCountdownEvent?.Dispose();
                _fileHandlerCountdownEvent = null;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
