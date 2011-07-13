using Extract.SQLCDBEditor;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Enumeration for referring to the grid columns.
    /// </summary>
    enum GridColumns
    {
        MachineName = 0,

        GroupName = 1,

        FAMService = 2,

        FDRSService = 3,

        CPUUsage = 4
    }

    /// <summary>
    /// Enumeration to determine what the start/stop service buttons control.
    /// </summary>
    [Flags]
    enum ServiceToControl
    {
        /// <summary>
        /// Button should control the FAM service.
        /// </summary>
        FamService = 0x1,

        /// <summary>
        /// Button should control the FDRS service.
        /// </summary>
        FdrsService = 0x2,

        /// <summary>
        /// Button should control both services.
        /// </summary>
        Both = 0x1 | 0x2
    }

    /// <summary>
    /// Form definition for the network dashboard.
    /// </summary>
    public partial class FAMNetworkDashboardForm : Form
    {
        #region ServiceControlThreadParamaters Class

        class ServiceControlThreadParameters
        {
            #region Fields

            /// <summary>
            /// The selected row that should have their service started/stopped.
            /// </summary>
            readonly FAMNetworkDashboardRow _row;

            /// <summary>
            /// The service to control (FAM, FDRS, or both).
            /// </summary>
            readonly ServiceToControl _serviceToControl;

            /// <summary>
            /// Whether the service should be started or stopped.
            /// </summary>
            readonly bool _startService;

            #endregion Fields

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceControlThreadParameters"/> class.
            /// </summary>
            /// <param name="row">The row.</param>
            /// <param name="serviceToControl">The service to control.</param>
            /// <param name="startService">if set to <see langword="true"/> start service
            /// otherwise stop service.</param>
            public ServiceControlThreadParameters(FAMNetworkDashboardRow row,
                ServiceToControl serviceToControl, bool startService)
            {
                _row = row;
                _serviceToControl = serviceToControl;
                _startService = startService;
            }

            #endregion Constructor

            #region Properties

            /// <summary>
            /// Gets the row.
            /// </summary>
            /// <value>The rows.</value>
            public FAMNetworkDashboardRow Row
            {
                get
                {
                    return _row;
                }
            }

            /// <summary>
            /// Gets the service to control.
            /// </summary>
            /// <value>The service to control.</value>
            public ServiceToControl ServiceToControl
            {
                get
                {
                    return _serviceToControl;
                }
            }

            /// <summary>
            /// Gets a value indicating whether to start the service or stop it.
            /// </summary>
            /// <value>
            ///	<see langword="true"/> if service should be started; otherwise <see langword="false"/>.
            /// </value>
            public bool StartService
            {
                get
                {
                    return _startService;
                }
            }

            #endregion Properties
        }

        #endregion ServiceControlThreadParamaters Class

        #region Constants

        /// <summary>
        /// File filter string for the open/save file dialog.
        /// </summary>
        const string _FILE_FILTER = "FAM Network Manager (*.fnm)|*.fnm|All Files (*.*)|*.*||";

        /// <summary>
        /// Default file extension for the open/save file dialog
        /// </summary>
        const string _DEFAULT_FILE_EXT = "fnm";

        /// <summary>
        /// File extension supported by the drag/drop event
        /// </summary>
        const string _DRAG_FILE_EXT = "." + _DEFAULT_FILE_EXT;

        /// <summary>
        /// Constant string used to indicate that data is refreshing
        /// </summary>
        const string _REFRESHING = "Refreshing";

        /// <summary>
        /// Sleep time for the service status update thread.
        /// </summary>
        const int _DEFAULT_REFRESH_THREAD_SLEEP_TIME = 1000;

        /// <summary>
        /// The title for the the FAM Manager application
        /// </summary>
        const string _FAM_MANAGER_TITLE = "FAM Network Manager";

        /// <summary>
        /// Path to the folder containing the application settings for this application.
        /// </summary>
        static readonly string _APPLICATION_SETTINGS_DIR =
            Path.Combine(FileSystemMethods.ApplicationDataPath, "FAM Network Manager");

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="FAMNetworkDashboardForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = Path.Combine(
            _APPLICATION_SETTINGS_DIR, "FamNetworkDashboard.xml");

        /// <summary>
        /// Name for the mutex used to serialize persistance of the control and form layout.
        /// </summary>
        static readonly string _MUTEX_STRING = "C26EBE45-3B95-4CA2-A843-34881D17E24C";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Service to control with the start and stop buttons.
        /// </summary>
        ServiceToControl _serviceToControl = ServiceToControl.Both;

        /// <summary>
        /// Indicates whether the refresh data thread should update the data.
        /// </summary>
        volatile bool _refreshData;

        /// <summary>
        /// Event to signal the threads to exit.
        /// </summary>
        ManualResetEvent _endThreads = new ManualResetEvent(false);

        /// <summary>
        /// Event to indicate the refresh thread has exited
        /// </summary>
        ManualResetEvent _refreshThreadEnded = new ManualResetEvent(false);

        /// <summary>
        /// Event to indicate the cleanup thread has exited
        /// </summary>
        ManualResetEvent _cleanupThreadEnded = new ManualResetEvent(false);

        /// <summary>
        /// Event to indicate that the refresh time has changed
        /// </summary>
        AutoResetEvent _refreshTimeChanged = new AutoResetEvent(false);

        /// <summary>
        /// Event to indicate that the auto refresh button was toggled
        /// </summary>
        AutoResetEvent _autoRefreshToggled = new AutoResetEvent(false);

        /// <summary>
        /// The file to open when the form loads.
        /// </summary>
        string _currentFile;

        /// <summary>
        /// Saves/restores window state info.
        /// </summary>
        FormStateManager _formStateManager;

        /// <summary>
        /// Flag to indicate whether the current FNM instance is dirty or not.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The sleep time for the refresh thread
        /// </summary>
        volatile int _refreshSleepTime;

        /// <summary>
        /// Collection of rows that have been deleted but not disposed yet.
        /// </summary>
        ConcurrentQueue<FAMNetworkDashboardRow> _deletedRows =
            new ConcurrentQueue<FAMNetworkDashboardRow>();

        /// <summary>
        /// Flag to indicate whether the form should be reset to installed default state.
        /// </summary>
        bool _resetForm;

        /// <summary>
        /// Configuration settings for this application
        /// </summary>
        readonly ConfigSettings<Properties.Settings> _config = new ConfigSettings<Properties.Settings>(
            Path.Combine(FileSystemMethods.ApplicationDataPath, "FAMNetworkManager.config"));

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Delegate method used to set the error information for a row.
        /// </summary>
        /// <param name="row">The row to set the information for.</param>
        /// <param name="ee">The exception to add to the row.</param>
        delegate void SetErrorTextDelegate(FAMNetworkDashboardRow row,
            ExtractException ee);

        /// <summary>
        /// Delegate method used to update the data for individual rows when a refresh is called.
        /// </summary>
        /// <param name="row">The row to update.</param>
        /// <param name="data">The data to update the row with.</param>
        delegate void RefreshRowDataDelegate(FAMNetworkDashboardRow row, ServiceStatusUpdateData data);

        /// <summary>
        /// Delegate method used to refresh the data grid.
        /// </summary>
        /// <param name="autoUpdate">Whether auto-updating should be triggered or not.</param>
        delegate void RefreshDataDelegate(bool autoUpdate);

        #endregion Delegates

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMNetworkDashboardForm"/> class.
        /// </summary>
        public FAMNetworkDashboardForm()
            : this(null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMNetworkDashboardForm"/> class.
        /// </summary>
        /// <param name="fileToOpen">The file to open.</param>
        public FAMNetworkDashboardForm(string fileToOpen)
            : this(fileToOpen, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMNetworkDashboardForm"/> class.
        /// </summary>
        /// <param name="fileToOpen">The file to open.</param>
        /// <param name="resetForm">if set to <see langword="true"/> then persisted
        /// form settings will not be used and form will be reset to install defaults.</param>
        public FAMNetworkDashboardForm(string fileToOpen, bool resetForm)
        {
            InitializeComponent();

            _currentFile = fileToOpen;

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                // Loads/save UI state properties
                _formStateManager = new FormStateManager(
                    this, _FORM_PERSISTENCE_FILE, _MUTEX_STRING, true, null);
            }

            _resetForm = resetForm;

            Text = _FAM_MANAGER_TITLE;
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (_resetForm && _formStateManager != null)
                {
                    _formStateManager.SaveState();
                }

                base.OnLoad(e);

                _startStopTargetComboBox.SelectedIndex = 0;

                LoadSettings();

                // Launch the refresh data thread
                Thread refreshThread = new Thread(RefreshDataThread);
                refreshThread.SetApartmentState(ApartmentState.MTA);
                refreshThread.Start();

                // Launch the row cleanup thread (handles disposing of rows
                // that are removed from the datagrid)
                Thread rowCleanupThread = new Thread(CleanupRowsThread);
                rowCleanupThread.SetApartmentState(ApartmentState.MTA);
                rowCleanupThread.Start();

                if (!string.IsNullOrEmpty(_currentFile))
                {
                    LoadMachineList(_currentFile);
                }

                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30801", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    e.Cancel = true;
                    return;
                }

                SaveSettings();

                // End the worker threads
                _endThreads.Set();

                ClearAllGridRows();

                // Display the wait form while waiting for the service controllers to
                // finish
                using (ManualResetEvent eventHandle = new ManualResetEvent(false))
                {
                    var task = Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                while (!_deletedRows.IsEmpty)
                                {
                                    var list = new List<FAMNetworkDashboardRow>();
                                    FAMNetworkDashboardRow row = null;
                                    while (_deletedRows.TryDequeue(out row))
                                    {
                                        if (row.ControllerOperating)
                                        {
                                            list.Add(row);
                                        }
                                        else
                                        {
                                            row.Dispose();
                                        }
                                    }

                                    if (list.Count > 0)
                                    {
                                        foreach (var rowToKeep in list)
                                        {
                                            _deletedRows.Enqueue(rowToKeep);
                                        }
                                        list.Clear();

                                        // Sleep to give controllers a chance to finish
                                        // before checking again
                                        Thread.Sleep(500);
                                    }
                                }
                            }
                            finally
                            {
                                eventHandle.Set();
                            }
                        }
                    );

                    if (!_deletedRows.IsEmpty)
                    {
                        using (var waitForm = new PleaseWaitForm(
                            "Waiting for service controllers to finish.", eventHandle))
                        {
                            waitForm.ShowDialog();
                        }
                    }

                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        throw ExtractException.AsExtractException("ELI30993", ae.Flatten());
                    }
                    finally
                    {
                        task.Dispose();
                    }
                }

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30994", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragEnter"/> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs"/> that contains the event data.</param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            try
            {
                // Check if this is a file drop
                if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the files being dragged
                    string[] fileNames = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                    // Check that there is only 1 file and that the extension is fnm
                    if (fileNames.Length == 1
                        && Path.GetExtension(fileNames[0]).Equals(_DRAG_FILE_EXT,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        drgevent.Effect = DragDropEffects.Copy;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31033", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragDrop"/> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs"/> that contains the event data.</param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            try
            {
                // Check if this is a file drop event
                if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the files being dragged
                    string[] fileNames = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                    // Check that there is only 1 file and that the extension is fnm
                    if (fileNames.Length == 1
                        && Path.GetExtension(fileNames[0]).Equals(_DRAG_FILE_EXT,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        // If the necessary, prompt for dirty file
                        if (PromptForDirtyFile())
                        {
                            // Load the machine list
                            LoadMachineList(Path.GetFullPath(fileNames[0]));
                        }
                    }
                    else if (fileNames.Length > 1)
                    {
                        // If trying to open more than one file, display an error message
                        MessageBox.Show("Cannot open more than one file.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31027", ex);
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message"/>, passed by reference, that represents the Win32 message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process.</param>
        /// <returns>
        /// true if the keystroke was processed and consumed by the control; otherwise, false to allow further processing.
        /// </returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                bool handled = true;
                switch (keyData)
                {
                    case Keys.Delete:
                        HandleRemoveMachineButtonClick(this, new EventArgs());
                        break;

                    case Keys.F2:
                        HandleEditMachineOrGroupButtonClick(this, new EventArgs());
                        break;

                    case Keys.F8:
                        HandleAddMachineButtonClick(this, new EventArgs());
                        break;

                    case Keys.F5:
                        if (!_refreshData)
                        {
                            RefreshData(false);
                        }
                        break;

                    default:
                        handled = false;
                        break;
                }

                return !handled ? base.ProcessCmdKey(ref msg, keyData) : handled;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30995", ex);
                return true;
            }
        }

        /// <summary>
        /// Handles the open file button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOpenFileClick(object sender, EventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    return;
                }

                using (OpenFileDialog open = new OpenFileDialog())
                {
                    open.Filter = _FILE_FILTER;
                    open.DefaultExt = _DEFAULT_FILE_EXT;
                    open.AddExtension = true;
                    open.CheckFileExists = true;
                    open.ValidateNames = true;
                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        Refresh();
                        LoadMachineList(open.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30775", ex);
            }
        }

        /// <summary>
        /// Handles the save file button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleSaveFileClick(object sender, EventArgs e)
        {
            try
            {
                SaveMachineList(_currentFile);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30776", ex);
            }
        }

        /// <summary>
        /// Handles the save as click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleSaveAsClick(object sender, EventArgs e)
        {
            try
            {
                SaveMachineList();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30978", ex);
            }
        }

        /// <summary>
        /// Handles the add machine button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleAddMachineButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Build the group list
                using (AddModifyMachineForm addForm = new AddModifyMachineForm(BuildGroupList()))
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        Refresh();
                        using (TemporaryWaitCursor wait = new TemporaryWaitCursor())
                        {
                            ServiceMachineController controller = new ServiceMachineController(
                                addForm.MachineName, addForm.GroupName);
                            var row = new FAMNetworkDashboardRow(
                                new RowDataItem(controller));

                            row.CreateCells(_machineListGridView,
                                new object[] { controller.MachineName, controller.GroupName,
                                _REFRESHING, _REFRESHING, _REFRESHING});
                            _machineListGridView.Rows.Add(row);

                            // Scroll the row into view
                            _machineListGridView.FirstDisplayedScrollingRowIndex = row.Index;

                            Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        var data = row.DataItem.Controller.RefreshData();
                                        RefreshRowData(row, data);
                                    }
                                    catch (Exception ex)
                                    {
                                        var ee = ExtractException.AsExtractException("ELI30821", ex);
                                        ee.AddDebugData("Machine Name",
                                            controller.MachineName, false);
                                        SetRowErrorText(row, ee);
                                    }
                                }
                            );

                            _dirty = true;
                            UpdateGroupFilterList();
                            UpdateTitle();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30777", ex);
            }
        }

        /// <summary>
        /// Handles the remove machine button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleRemoveMachineButtonClick(object sender, EventArgs e)
        {
            try
            {
                foreach (FAMNetworkDashboardRow row in _machineListGridView.SelectedRows)
                {
                    if (row.Visible)
                    {
                        _machineListGridView.Rows.RemoveAt(row.Index);
                        _deletedRows.Enqueue(row);
                        _dirty = true;
                    }
                }

                // Check if any rows were deleted and what the current row count is,
                // if there is no current file and there are no rows clear the dirty flag
                if (_dirty && string.IsNullOrWhiteSpace(_currentFile)
                    && _machineListGridView.Rows.Count == 0)
                {
                    _dirty = false;
                }

                UpdateGroupFilterList();
                UpdateTitle();
                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30778", ex);
            }
        }

        /// <summary>
        /// Handles the edit machine or group button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleEditMachineOrGroupButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_machineListGridView.SelectedRows.Count < 1)
                {
                    return;
                }

                var rows = new List<FAMNetworkDashboardRow>();
                foreach (FAMNetworkDashboardRow row in _machineListGridView.SelectedRows)
                {
                    if (row.Visible)
                    {
                        rows.Add(row);
                    }
                }

                using (AddModifyMachineForm modifyForm = new AddModifyMachineForm(rows, BuildGroupList()))
                {
                    if (modifyForm.ShowDialog() == DialogResult.OK)
                    {
                        if (modifyForm.DataChanged)
                        {
                            _dirty = true;

                            // Get the group name and the rows to update
                            string groupName = modifyForm.GroupName;
                            var rowsToUpdate = modifyForm.Rows;

                            // If only updating a single row, update machine name and group name
                            if (rowsToUpdate.Count == 1)
                            {
                                string machineName = modifyForm.MachineName;
                                var row = rowsToUpdate[0];
                                row.Cells[(int)GridColumns.MachineName].Value = machineName;
                                row.Cells[(int)GridColumns.GroupName].Value = groupName;
                                row.UpdateMachineAndGroupName(machineName, groupName);
                                Task.Factory.StartNew(RefreshServiceData, row);
                            }
                            else
                            {
                                // Update the group for each row
                                foreach (var row in rowsToUpdate)
                                {
                                    row.Cells[(int)GridColumns.GroupName].Value = groupName;
                                    row.UpdateMachineAndGroupName(null, groupName);
                                    Task.Factory.StartNew(RefreshServiceData, row);
                                }
                            }

                            UpdateGroupFilterList();
                            UpdateTitle();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30842", ex);
            }
        }

        /// <summary>
        /// Handles the start service button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleStartServiceButtonClick(object sender, EventArgs e)
        {
            try
            {
                ControlServiceForSelectedRows(true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30779", ex);
            }
        }

        /// <summary>
        /// Handles the stop service button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleStopServiceButtonClick(object sender, EventArgs e)
        {
            try
            {
                ControlServiceForSelectedRows(false);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30780", ex);
            }
        }

        /// <summary>
        /// Handles the edit service database button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings",
            MessageId = "Program Files")]
        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings",
            MessageId = "Program Files (x86)")]
        void HandleEditServiceDatabaseButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_machineListGridView.SelectedRows.Count > 1)
                {
                    MessageBox.Show("Cannot modify more than a single machine database at a time",
                        "Cannot Modify Multiple Databases", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                    return;
                }

                string machineName = (string)_machineListGridView.SelectedRows[0].Cells[
                    (int)GridColumns.MachineName].Value;

                var appDataPath = FileSystemMethods.GetRemoteExtractCommonApplicationDataPath(
                    machineName);

                string dbFile = Path.Combine(appDataPath, "ESFAMService", "ESFAMService.sdf");

                // Compute the name of the file
                var fileInfo = new FileInfo(dbFile);
                if (!fileInfo.Exists)
                {
                        MessageBox.Show("Cannot find service database file to modify."
                            + Environment.NewLine + dbFile, "Cannot Find File",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        return;
                }
                if (fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    MessageBox.Show("Cannot modify read-only service database file."
                        + Environment.NewLine + dbFile, "File Is Read-only",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    return;
                }

                using (TemporaryFile tempDb = new TemporaryFile(".sdf", false))
                {
                    File.Copy(dbFile, tempDb.FileName, true);
                    using (SQLCDBEditorForm editor = new SQLCDBEditorForm(tempDb.FileName, false))
                    {
                        editor.CustomTitle = SQLCDBEditorForm.DefaultTitle + " - "
                            + machineName + ": FAM Service Database";
                        editor.StartPosition = FormStartPosition.CenterParent;
                        editor.ShowDialog();
                        if (editor.FileSaved)
                        {
                            File.Copy(tempDb.FileName, dbFile, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30832", ex);
            }
        }

        /// <summary>
        /// Handles the auto refresh data button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleAutoRefreshDataButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_autoRefreshDataToolStripButton.Checked)
                {
                    _refreshDataToolStripButton.Enabled = false;
                    RefreshData(true);
                }
                else
                {
                    _refreshData = false;
                    _refreshDataToolStripButton.Enabled = true;
                }

                _autoRefreshToggled.Set();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31066", ex);
            }
        }

        /// <summary>
        /// Handles the refresh data button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleRefreshDataButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (TemporaryWaitCursor cursor = new TemporaryWaitCursor())
                {
                    RefreshData(false);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30781", ex);
            }
        }

        /// <summary>
        /// Handles the about fam network manager menu item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleAboutFamNetworkManagerMenuItem(object sender, EventArgs e)
        {
            try
            {
                using (AboutFAMNetworkManager aboutForm = new AboutFAMNetworkManager())
                {
                    aboutForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30783", ex);
            }
        }

        /// <summary>
        /// Handles the machine grid view selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleMachineGridViewSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30843", ex);
            }
        }

        /// <summary>
        /// Handles the start stop combo selected index changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleStartStopComboSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_startStopTargetComboBox.Text.Equals("FAM Service", StringComparison.OrdinalIgnoreCase))
                {
                    _serviceToControl = ServiceToControl.FamService;
                }
                else if (_startStopTargetComboBox.Text.Equals("FDRS Service", StringComparison.OrdinalIgnoreCase))
                {
                    _serviceToControl = ServiceToControl.FdrsService;
                }
                else
                {
                    _serviceToControl = ServiceToControl.Both;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30786", ex);
            }
        }

        /// <summary>
        /// Handles the filter groups selected index changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleFilterGroupsSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedGroup = _groupFilterComboBox.Text;
                if (string.IsNullOrEmpty(selectedGroup))
                {
                    // All rows should be visible
                    foreach (FAMNetworkDashboardRow row in _machineListGridView.Rows)
                    {
                        if (row.Selected)
                        {
                            row.Selected = false;
                        }
                        row.Visible = true;
                    }
                }
                else
                {
                    foreach (FAMNetworkDashboardRow row in _machineListGridView.Rows)
                    {
                        string temp = row.Cells[(int)GridColumns.GroupName].Value.ToString();
                        row.Visible = selectedGroup.Equals(temp, StringComparison.Ordinal);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30837", ex);
            }
        }

        /// <summary>
        /// Handles the machine grid view mouse double click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellMouseEventArgs"/> instance containing the event data.</param>
        void HandleMachineGridViewMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex != -1)
                {
                    var row = (FAMNetworkDashboardRow)
                        _machineListGridView.Rows[e.RowIndex];
                    if (row.Exception != null)
                    {
                        // Need to invoke the dialog so that the row selection from
                        // the double-click completes
                        BeginInvoke((MethodInvoker)(() =>
                            {
                                row.Exception.Display();
                            })
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30980", ex);
            }
        }

        /// <summary>
        /// Handles the exit menu item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleExitMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30787", ex);
            }
        }

        /// <summary>
        /// Handles the set auto refresh thread sleep time menu item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleSetAutoRefreshThreadSleepTime(object sender, EventArgs e)
        {
            try
            {
                string value = _refreshSleepTime.ToString(CultureInfo.CurrentCulture);
                do
                {
                    if (InputBox.Show(this, "Auto refresh frequency in milliseconds:",
                        "Refresh Time", ref value) == DialogResult.OK)
                    {
                        int temp;
                        if (!int.TryParse(value, out temp) || temp <= 0)
                        {
                            var sb = new StringBuilder("Invalid refresh time specified, ");
                            sb.Append("must be an integer between 1 and ");
                            sb.Append(int.MaxValue);
                            sb.Append(" inclusive.");
                            MessageBox.Show(sb.ToString(), "Invalid Refresh Time",
                                MessageBoxButtons.OK, MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1, 0);
                        }
                        else
                        {
                            if (temp != _refreshSleepTime)
                            {
                                _refreshSleepTime = temp;
                                _refreshTimeChanged.Set();
                            }
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30985", ex);
            }
        }

        /// <summary>
        /// Handles the new tool strip menu item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleNewToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    return;
                }

                ClearAllGridRows();
                _currentFile = null;
                _dirty = false;

                UpdateGroupFilterList();
                UpdateEnabledStates();
                UpdateTitle();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31034", ex);
            }
        }

        /// <summary>Handles the data grid view preview key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        void HandleDataGridViewPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                // The data grid view does not pass the shift-tab key down event
                // if the current selection is the top row in the grid.
                // In order to maintain the cycling behavior, we need
                // to ensure the key down event fires for this key combination.
                if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.Shift)
                {
                    e.IsInputKey = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31597");
            }
        }

        /// <summary>Handles the data grid view key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        void HandleDataGridViewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Tab
                    && (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift))
                {
                    var row = _machineListGridView.CurrentRow;
                    if (row != null)
                    {
                        var count = _machineListGridView.Rows.Count;
                        var reverse = e.Modifiers == Keys.Shift;
                        var index = row.Index + (reverse ? -1 : 1);
                        if (index < 0)
                        {
                            index = count - 1;
                        }
                        else if (index >= count)
                        {
                            index = 0;
                        }

                        // Move to the next/previous row
                        _machineListGridView.CurrentCell =
                            _machineListGridView.Rows[index].Cells[0];

                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31598");
            }
        }

        /// <summary>
        /// Handles the remote desktop button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleRemoteDesktopButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_machineListGridView.SelectedRows.Count != 1)
                {
                    return;
                }

                string machineName = (string)_machineListGridView.SelectedRows[0].Cells[
                    (int)GridColumns.MachineName].Value;

                // Launch the remote desktop for the machine
                SystemMethods.RunExecutable("mstsc.exe", new string[] { "/v:" + machineName }, 0);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32160");
            }

        }

        #endregion Event Handler

        #region Methods

        /// <summary>
        /// Refreshes the row data.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="data">The data.</param>
        void RefreshRowData(FAMNetworkDashboardRow row, ServiceStatusUpdateData data)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RefreshRowDataDelegate(RefreshRowData),
                    new object[] { row, data });
                return;
            }

            if (row.Index != -1 && row.Visible)
            {
                row.Cells[(int)GridColumns.FAMService].Value = data.FamServiceStatus;
                row.Cells[(int)GridColumns.FDRSService].Value = data.FdrsServiceStatus;
                row.Cells[(int)GridColumns.CPUUsage].Value = data.CpuPercentageString;
                if (data.Exception != null)
                {
                    row.Exception = data.Exception;
                    row.ErrorText = data.Exception.Message;
                }
                else
                {
                    row.ErrorText = string.Empty;
                    row.Exception = null;
                }
            }
        }

        /// <summary>
        /// Refreshes the status and performance counter data in the grid.
        /// </summary>
        void RefreshData(bool autoUpdate)
        {
            int[] refreshColumns = new int[] { (int)GridColumns.FAMService, (int)GridColumns.FDRSService,
                (int)GridColumns.CPUUsage };

            // Clear all cells
            foreach (FAMNetworkDashboardRow row in _machineListGridView.Rows)
            {
                if (row.Visible)
                {
                    foreach (var column in refreshColumns)
                    {
                        row.Cells[column].Value = _REFRESHING;
                    }
                    row.ErrorText = string.Empty;
                }
            }

            if (autoUpdate)
            {
                _refreshData = true;
            }
            else
            {
                Task.Factory.StartNew(RefreshServiceDataForAllRows);
            }
        }

        /// <summary>
        /// Thread method to refresh the service data.
        /// </summary>
        void RefreshDataThread()
        {
            try
            {
                var handles = new WaitHandle[] {
                    _endThreads, _refreshTimeChanged, _autoRefreshToggled
                };

                do
                {
                    if (_refreshData)
                    {
                        RefreshServiceDataForAllRows();
                    }
                }
                while (WaitHandle.WaitAny(handles, _refreshSleepTime) != 0);
            }
            catch (ThreadAbortException)
            {
                // Just eat this exception
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI30804", ex);
            }
            finally
            {
                if (_refreshThreadEnded != null)
                {
                    _refreshThreadEnded.Set();
                }
            }
        }

        /// <summary>
        /// Performs a single refresh of the service data in the machine list grid.
        /// </summary>
        void RefreshServiceDataForAllRows()
        {
            // Create a list of all current rows in the UI
            foreach (FAMNetworkDashboardRow row in _machineListGridView.Rows)
            {
                // Only launch a refresh task if the row is visible and not already refreshing
                if (row.Visible && !row.RefreshingData)
                {
                    Task.Factory.StartNew(RefreshServiceData, row);
                }
            }
        }

        /// <summary>
        /// Refreshes the service data.
        /// </summary>
        /// <param name="rowObject">The row to update.</param>
        void RefreshServiceData(object rowObject)
        {
            var row = rowObject as FAMNetworkDashboardRow;
            if (row == null)
            {
                return;
            }

            try
            {
                // If the row is still in the collection and visible, get its controller,
                // refresh the data and update the row
                if (!_endThreads.WaitOne(0))
                {
                    var data = row.RefreshData();
                    RefreshRowData(row, data);
                }
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI30809", ex);
                SetRowErrorText(row, ee);
            }
        }

        /// <summary>
        /// Saves the machine list.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        bool SaveMachineList(string fileName = null)
        {
            Refresh();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                using (SaveFileDialog save = new SaveFileDialog())
                {
                    save.Filter = _FILE_FILTER;
                    save.DefaultExt = _DEFAULT_FILE_EXT;
                    save.AddExtension = true;
                    save.CheckPathExists = true;
                    save.OverwritePrompt = true;
                    save.ValidateNames = true;
                    if (save.ShowDialog() == DialogResult.OK)
                    {
                        return SaveMachineList(save.FileName);
                    }
                }

                return false;
            }

            using (TemporaryWaitCursor cursor = new TemporaryWaitCursor())
            {
                List<ServiceMachineController> data = new List<ServiceMachineController>();
                foreach (FAMNetworkDashboardRow row in _machineListGridView.Rows)
                {
                    data.Add(row.DataItem.Controller);
                }
                var serializer = new BinaryFormatter();
                using (FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(stream, data.Count);
                    foreach (var controller in data)
                    {
                        serializer.Serialize(stream, controller);
                    }
                    stream.Flush();
                }

                // Update the current file name to the saved file name
                _currentFile = fileName;

                _dirty = false;
                UpdateTitle();

                return true;
            }
        }

        /// <summary>
        /// Loads the stored list of machines.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        void LoadMachineList(string fileName)
        {
            try
            {
                using (TemporaryWaitCursor cursor = new TemporaryWaitCursor())
                {
                    bool soapFormatted = false;
                    List<ServiceMachineController> controllers = null;
                    try
                    {
                        using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                        {
                            var serializer = new BinaryFormatter();
                            int count = (int)serializer.Deserialize(stream);
                            controllers = new List<ServiceMachineController>(count);
                            for (int i = 0; i < count; i++)
                            {
                                var controller = (ServiceMachineController)serializer.Deserialize(stream);
                                controllers.Add(controller);
                            }
                        }
                    }
                    catch
                    {
                        controllers = null;
                        soapFormatted = true;
                    }

                    // TODO: Bridge code
                    if (soapFormatted)
                    {
                        // Assume this is a soap formatted file, try that
                        var sb = new StringBuilder(File.ReadAllText(fileName));
                        var regex = new Regex(@"\d+\.\d+\.\d+\.\d+");
                        var match = regex.Match(sb.ToString());
                        if (match.Success)
                        {
                            var oldVersion = match.Value;
                            var assemblyName = typeof(ServiceMachineController).Assembly.FullName;
                            match = regex.Match(assemblyName);
                            if (match.Success)
                            {
                                var versionString = match.Value;
                                sb.Replace(oldVersion, versionString);
                                File.WriteAllText(fileName, sb.ToString());
                            }
                        }

                        using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                        {
                            var serializer = new SoapFormatter();
                            int count = (int)serializer.Deserialize(stream);
                            controllers = new List<ServiceMachineController>(count);
                            for (int i = 0; i < count; i++)
                            {
                                var controller = (ServiceMachineController)serializer.Deserialize(stream);
                                controllers.Add(controller);
                            }

                        }
                    }

                    // Clear the current grid
                    ClearAllGridRows();

                    _currentFile = fileName;

                    foreach (var controller in controllers)
                    {
                        var row = new FAMNetworkDashboardRow(
                            new RowDataItem(controller));
                        row.CreateCells(_machineListGridView);
                        row.Cells[0].Value = controller.MachineName;
                        row.Cells[1].Value = controller.GroupName;
                        _machineListGridView.Rows.Add(row);
                    }

                    // TODO: Bridge code
                    if (soapFormatted)
                    {
                        _dirty = true;
                    }
                    else
                    {
                        _dirty = false;
                    }

                    UpdateGroupFilterList();
                    UpdateTitle();

                    // Refresh the data if not currently auto-refreshing
                    if (!_refreshData)
                    {
                        RefreshData(false);
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI30826", ex);
                ee.AddDebugData("File To Load", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Builds the group list.
        /// </summary>
        /// <returns>The group list.</returns>
        SortedSet<string> BuildGroupList()
        {
            SortedSet<string> groups = new SortedSet<string>();
            foreach (FAMNetworkDashboardRow row in _machineListGridView.Rows)
            {
                if (row.DataItem.Controller != null)
                {
                    groups.Add(row.DataItem.Controller.GroupName);
                }
            }

            return groups;
        }

        /// <summary>
        /// Sets the row error text.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="ee">The exception to associate with the row.</param>
        void SetRowErrorText(FAMNetworkDashboardRow row,
            ExtractException ee)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new SetErrorTextDelegate(SetRowErrorText),
                        new object[] { row, ee });
                    return;
                }

                // Check if this is the same error
                if (row.Index != -1
                    && !row.ErrorText.Equals(ee.Message, StringComparison.Ordinal))
                {
                    row.ErrorText = ee.Message;
                    row.Cells[(int)GridColumns.FAMService].Value = "";
                    row.Cells[(int)GridColumns.FDRSService].Value = "";
                    row.Cells[(int)GridColumns.CPUUsage].Value = "";
                    row.Exception = ee;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30806", ex);
            }
        }

        /// <summary>
        /// Controls the service task.
        /// </summary>
        /// <param name="parameter">The thread data (should be of type
        /// <see cref="ServiceControlThreadParameters"/>).</param>
        void ControlServiceTask(object parameter)
        {
            var data = parameter as ServiceControlThreadParameters;
            if (data == null)
            {
                return;
            }

            var row = data.Row;
            try
            {
                row.ControlService(data.ServiceToControl, data.StartService);
                RefreshServiceData(row);
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI30803", ex);
                SetRowErrorText(row, ee);
            }
        }

        /// <summary>
        /// Updates the group filter list.
        /// </summary>
        void UpdateGroupFilterList()
        {
            string currentSelection = _groupFilterComboBox.SelectedText;
            _groupFilterComboBox.Items.Clear();
            foreach (string group in BuildGroupList())
            {
                _groupFilterComboBox.Items.Add(group);
            }

            if (_groupFilterComboBox.FindStringExact(string.Empty) == -1)
            {
                _groupFilterComboBox.Items.Insert(0, string.Empty);
            }

            int index = _groupFilterComboBox.FindStringExact(currentSelection);
            _groupFilterComboBox.SelectedIndex = index != -1 ? index : 0;
        }

        /// <summary>
        /// Updates the enabled states of the toolstrip buttons and menu items.
        /// </summary>
        void UpdateEnabledStates()
        {
            int rowCount = _machineListGridView.Rows.Count;
            int selectedCount = _machineListGridView.SelectedRows.Count;

            var enableSaveAndNew = rowCount > 0 || _dirty;
            _newToolStripMenuItem.Enabled = enableSaveAndNew;
            _saveFileToolStripButton.Enabled = enableSaveAndNew;
            _saveToolStripMenuItem.Enabled = enableSaveAndNew;
            _saveAsToolStripMenuItem.Enabled = enableSaveAndNew;

            _removeMachineToolStripButton.Enabled = selectedCount > 0;
            _modifyServiceDatabaseToolStripButton.Enabled = selectedCount == 1;
            _remoteDesktopToolStripButton.Enabled = selectedCount == 1;
            _editMachineGroupAndNameToolStripButton.Enabled = selectedCount > 0;
        }

        /// <summary>
        /// Controls the service for selected rows.
        /// </summary>
        /// <param name="startService">if set to <see langword="true"/> the service
        /// will be started, if <see langword="false"/> the service will be stopped..</param>
        void ControlServiceForSelectedRows(bool startService)
        {
            var selectedRows = new List<FAMNetworkDashboardRow>();
            var selectedNames = new List<string>();
            foreach (FAMNetworkDashboardRow row in _machineListGridView.SelectedRows)
            {
                if (row.Visible)
                {
                    selectedRows.Add(row);
                    selectedNames.Add(row.Cells[(int)GridColumns.MachineName].Value.ToString());
                }
            }

            // Build a prompt to alert the user before starting/stopping the services
            if (selectedNames.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(startService ? "Start" : "Stop");
                if (_serviceToControl == ServiceToControl.Both)
                {
                    sb.Append(" both the FAM and FDRS service");
                }
                else if (_serviceToControl.HasFlag(ServiceToControl.FamService))
                {
                    sb.Append(" the FAM service");
                }
                else
                {
                    sb.Append(" the FDRS service");
                }
                sb.AppendLine(" on the following machines:");
                sb.Append(string.Join(", ", selectedNames));
                var result = MessageBox.Show(sb.ToString(), "Control Service?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2, 0);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Put the data into the structure to pass to the controller method.
            // Pass in true to start the service, false to stop the services.
            foreach (FAMNetworkDashboardRow row in selectedRows)
            {
                var controllerData = new ServiceControlThreadParameters(row,
                    _serviceToControl, startService);
                Task.Factory.StartNew(ControlServiceTask, controllerData);
            }
        }

        /// <summary>
        /// Thread method that cleans up the deleted rows.
        /// </summary>
        void CleanupRowsThread()
        {
            try
            {
                while (!_endThreads.WaitOne(1000))
                {
                    var list = new List<FAMNetworkDashboardRow>();
                    FAMNetworkDashboardRow row = null;
                    while (!_endThreads.WaitOne(0)
                        && !_deletedRows.IsEmpty
                        && _deletedRows.TryDequeue(out row))
                    {
                        if (row.ControllerOperating)
                        {
                            list.Add(row);
                        }
                        else
                        {
                            row.Dispose();
                        }
                    }
                    if (list.Count > 0)
                    {
                        foreach (var rowToKeep in list)
                        {
                            _deletedRows.Enqueue(rowToKeep);
                        }
                        list.Clear();
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI30335", ex);
            }
            finally
            {
                if (_cleanupThreadEnded != null)
                {
                    _cleanupThreadEnded.Set();
                }
            }
        }

        /// <summary>
        /// Updates the title.
        /// </summary>
        void UpdateTitle()
        {
            StringBuilder sb = new StringBuilder(_FAM_MANAGER_TITLE);
            if (!string.IsNullOrWhiteSpace(_currentFile))
            {
                sb.Append(" - ");
                sb.Append(_currentFile);
            }

            if (_dirty)
            {
                sb.Append("*");
            }

            Text = sb.ToString();
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        void SaveSettings()
        {
            _config.Settings.RefreshSleepTime = _refreshSleepTime;
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        void LoadSettings()
        {
            _refreshSleepTime = _config.Settings.RefreshSleepTime;
            if (_refreshSleepTime <= 0)
            {
                var ee = new ExtractException("ELI30988",
                    "Invalid refresh setting time in persisted settings, restoring default value.");
                ee.AddDebugData("Refresh Time", _refreshSleepTime, false);
                _refreshSleepTime = _DEFAULT_REFRESH_THREAD_SLEEP_TIME;
                throw ee;
            }
        }

        /// <summary>
        /// Prompts for file save if the file is dirty, returns <see langword="true"/>
        /// if the file is saved.
        /// </summary>
        /// <returns></returns>
        bool PromptForDirtyFile()
        {
            bool saved = true;
            if (_dirty)
            {
                var result = MessageBox.Show(
                    "File has not been saved, would you like to save now?",
                    "File Not Saved", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0);
                if (result == DialogResult.Yes)
                {
                    if (!SaveMachineList(_currentFile))
                    {
                        saved = false;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    saved = false;
                }
            }

            return saved;
        }

        /// <summary>
        /// Clears all grid rows.
        /// </summary>
        void ClearAllGridRows()
        {
            // Add all rows to the cleanup queue
            foreach (FAMNetworkDashboardRow row in _machineListGridView.Rows)
            {
                _deletedRows.Enqueue(row);
            }
            _machineListGridView.Rows.Clear();
        }

        #endregion Methods
    }
}
