using Extract.SQLCDBEditor;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

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
    /// Form definition for the network dashboard.
    /// </summary>
    public partial class FAMNetworkDashboardForm : Form
    {
        #region Internal use enum

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

        #endregion Internal use enum

        #region ServiceControlThreadParamaters Class

        class ServiceControlThreadParameters
        {
            #region Fields

            /// <summary>
            /// The selected rows that should have their service started/stopped.
            /// </summary>
            readonly List<BetterDataGridViewRow<RowDataItem>> _rows;

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
            /// <param name="rows">The rows.</param>
            /// <param name="serviceToControl">The service to control.</param>
            /// <param name="startService">if set to <see langword="true"/> start service
            /// otherwise stop service.</param>
            public ServiceControlThreadParameters(List<BetterDataGridViewRow<RowDataItem>> rows,
                ServiceToControl serviceToControl, bool startService)
            {
                _rows = new List<BetterDataGridViewRow<RowDataItem>>(rows);
                _serviceToControl = serviceToControl;
                _startService = startService;
            }

            #endregion Constructor

            #region Properties

            /// <summary>
            /// Gets the rows.
            /// </summary>
            /// <value>The rows.</value>
            public List<BetterDataGridViewRow<RowDataItem>> Rows
            {
                get
                {
                    return _rows;
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
        /// Constant string used to indicate that data is refreshing
        /// </summary>
        const string _REFRESHING = "Refreshing";

        /// <summary>
        /// Sleep time for the service status update thread.
        /// </summary>
        const int _REFRESH_THREAD_SLEEP_TIME = 1000;

        /// <summary>
        /// The title for the the FAM Manager application
        /// </summary>
        const string _FAM_MANAGER_TITLE = "FAM Network Manager";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="FAMNetworkDashboardForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.ApplicationDataPath, "FAM Network Manager", "FamNetworkDashboard.xml");

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
        /// Event to signal the refresh thread to exit
        /// </summary>
        ManualResetEvent _endRefreshThread = new ManualResetEvent(false);

        /// <summary>
        /// Event to indicate the refresh thread has exited
        /// </summary>
        ManualResetEvent _refreshThreadEnded = new ManualResetEvent(false);

        /// <summary>
        /// The file to open when the form loads.
        /// </summary>
        string _currentFile;

        /// <summary>
        /// Saves/restores window state info.
        /// </summary>
        FormStateManager _formStateManager;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Delegate method used to set the error information for a row.
        /// </summary>
        /// <param name="row">The row to set the information for.</param>
        /// <param name="errorText">The text for the error.</param>
        /// <param name="ee">The exception to add to the row.</param>
        delegate void SetErrorTextDelegate(BetterDataGridViewRow<RowDataItem> row,
            string errorText, ExtractException ee);

        /// <summary>
        /// Delegate method used to update the data for individual rows when a refresh is called.
        /// </summary>
        /// <param name="row">The row to update.</param>
        /// <param name="data">The data to update the row with.</param>
        delegate void RefreshRowDataDelegate(BetterDataGridViewRow<RowDataItem> row, ServiceStatusUpdateData data);

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
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMNetworkDashboardForm"/> class.
        /// </summary>
        /// <param name="fileToOpen">The file to open.</param>
        public FAMNetworkDashboardForm(string fileToOpen)
        {
            InitializeComponent();

            _currentFile = fileToOpen;

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                // Loads/save UI state properties
                _formStateManager = new FormStateManager(
                    this, _FORM_PERSISTENCE_FILE, _MUTEX_STRING, true, null);
            }

            Text = _FAM_MANAGER_TITLE;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _startStopTargetComboBox.SelectedIndex = 0;

                // Launch the stats update thread
                Thread thread = new Thread(RefreshDataThread);
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();

                if (!string.IsNullOrEmpty(_currentFile))
                {
                    LoadSettings(_currentFile);
                }

                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30801", ex);
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
                        LoadSettings(open.FileName);
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
                if (string.IsNullOrEmpty(_currentFile))
                {
                    HandleSaveAsClick(sender, e);
                }
                else
                {
                    SaveSettings(_currentFile);
                }
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
                        SaveSettings(save.FileName);
                    }
                }
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
                            var row = new BetterDataGridViewRow<RowDataItem>(
                                new RowDataItem(controller));

                            row.CreateCells(_machineListGridView,
                                new object[] { controller.MachineName, controller.GroupName,
                                _REFRESHING, _REFRESHING, _REFRESHING});
                            _machineListGridView.Rows.Add(row);

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
                                        SetRowErrorText(row, ex.Message, ee);
                                    }
                                }
                            );

                            UpdateGroupFilterList();
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
                foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.SelectedRows)
                {
                    if (row.Visible)
                    {
                        _machineListGridView.Rows.RemoveAt(row.Index);
                        row.Dispose();
                    }
                }
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
                var rows = new List<BetterDataGridViewRow<RowDataItem>>();
                foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.SelectedRows)
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
                                UpdateServiceMachineController(row, groupName, machineName);
                                Task.Factory.StartNew(RefreshServiceData, row);
                            }
                            else
                            {
                                // Update the group for each row
                                foreach (var row in rowsToUpdate)
                                {
                                    row.Cells[(int)GridColumns.GroupName].Value = groupName;
                                    UpdateServiceMachineController(row, groupName);
                                    Task.Factory.StartNew(RefreshServiceData, row);
                                }
                            }

                            UpdateGroupFilterList();
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

                // Compute the name of the file
                StringBuilder dbFile = new StringBuilder();
                dbFile.Append(@"\\");
                dbFile.Append(_machineListGridView.SelectedRows[0].Cells[(int)GridColumns.MachineName].Value);
                dbFile.Append(@"\c$\Program Files\Extract Systems\CommonComponents\ESFAMService.sdf");
                if (!File.Exists(dbFile.ToString()))
                {
                    dbFile.Replace("Program Files", "Program Files (x86)");
                    if (!File.Exists(dbFile.ToString()))
                    {
                        MessageBox.Show("Cannot find service database file to modify."
                            + Environment.NewLine + dbFile.ToString(), "Cannot Find File",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        return;
                    }
                }

                using (TemporaryFile tempDb = new TemporaryFile(".sdf"))
                {
                    File.Copy(dbFile.ToString(), tempDb.FileName, true);
                    using (SQLCDBEditorForm editor = new SQLCDBEditorForm(tempDb.FileName, false))
                    {
                        editor.ShowDialog();
                        if (editor.FileSaved)
                        {
                            File.Copy(tempDb.FileName, dbFile.ToString(), true);
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
        /// Handles the machine grid view row double click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleMachineGridViewRowDoubleClick(object sender, EventArgs e)
        {
            try
            {
                var row = (BetterDataGridViewRow<RowDataItem>)
                    _machineListGridView.SelectedRows[0];
                if (row.DataItem.Exception != null)
                {
                    // Need to invoke the dialog so that the row selection from
                    // the double-click completes
                    BeginInvoke((MethodInvoker)(() =>
                        {
                            row.DataItem.Exception.Display();
                        })
                    );
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30980", ex);
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
                    foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.Rows)
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
                    foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.Rows)
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
        /// Refreshes the row data.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="data">The data.</param>
        void RefreshRowData(BetterDataGridViewRow<RowDataItem> row, ServiceStatusUpdateData data)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RefreshRowDataDelegate(RefreshRowData),
                    new object[] { row, data });
                return;
            }

            if (row.Visible)
            {
                row.ErrorText = string.Empty;
                row.DataItem.Exception = null;
                row.Cells[(int)GridColumns.FAMService].Value = data.FamServiceStatus;
                row.Cells[(int)GridColumns.FDRSService].Value = data.FdrsServiceStatus;
                row.Cells[(int)GridColumns.CPUUsage].Value =
                    data.CpuPercentage.ToString("F1", CultureInfo.CurrentCulture) + " %";
            }
        }

        /// <summary>
        /// Refreshes the status and performance counter data in the grid.
        /// </summary>
        void RefreshData(bool autoUpdate)
        {
            // Clear all cells
            foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.Rows)
            {
                if (row.Visible)
                {
                    for (int i = 2; i < 4; i++)
                    {
                        row.Cells[i].Value = _REFRESHING;
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
                do
                {
                    if (_refreshData)
                    {
                        RefreshServiceDataForAllRows();
                    }
                }
                while (!_endRefreshThread.WaitOne(_REFRESH_THREAD_SLEEP_TIME));
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
            foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.Rows)
            {
                if (row.Visible)
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
            var row = rowObject as BetterDataGridViewRow<RowDataItem>;
            if (row == null)
            {
                return;
            }

            string machineName = null;
            try
            {
                // If the row is still in the collection and visible, get its controller,
                // refresh the data and update the row
                if (!_endRefreshThread.WaitOne(0)
                    && !row.IsDisposed)
                {
                    ServiceMachineController controller = row.DataItem.Controller;
                    machineName = controller.MachineName;
                    var data = controller.RefreshData();
                    RefreshRowData(row, data);
                }
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI30809", ex);
                ee.AddDebugData("Machine Name", machineName ?? "Unknown", false);
                SetRowErrorText(row, ex.Message, ee);
            }
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        void SaveSettings(string fileName)
        {
            Refresh();
            using (TemporaryWaitCursor cursor = new TemporaryWaitCursor())
            {
                List<ServiceMachineController> data = new List<ServiceMachineController>();
                foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.Rows)
                {
                    if (!row.IsDisposed)
                    {
                        data.Add(row.DataItem.Controller);
                    }
                }
                using (FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ServiceMachineController>));
                    serializer.Serialize(stream, data);
                    stream.Flush();
                }

                // Update the current file name to the saved file name
                _currentFile = fileName;

                // Update the title
                Text = _FAM_MANAGER_TITLE + " - " + _currentFile;

            }
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        void LoadSettings(string fileName)
        {
            try
            {
                using (TemporaryWaitCursor cursor = new TemporaryWaitCursor())
                {
                    List<ServiceMachineController> controllers = null;
                    using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<ServiceMachineController>));
                        controllers = (List<ServiceMachineController>)serializer.Deserialize(stream);
                    }

                    // Ensure the rows are disposed before loading the new rows
                    var rowsToDispose = new List<BetterDataGridViewRow<RowDataItem>>();
                    foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.Rows)
                    {
                        rowsToDispose.Add(row);
                    }
                    _machineListGridView.Rows.Clear();
                    CollectionMethods.ClearAndDispose(rowsToDispose);

                    _currentFile = fileName;

                    foreach (ServiceMachineController controller in controllers)
                    {
                        var row = new BetterDataGridViewRow<RowDataItem>(
                            new RowDataItem(controller));
                        row.CreateCells(_machineListGridView);
                        row.Cells[0].Value = controller.MachineName;
                        row.Cells[1].Value = controller.GroupName;
                        _machineListGridView.Rows.Add(row);
                    }

                    UpdateGroupFilterList();

                    // Update the title
                    Text = _FAM_MANAGER_TITLE + " - " + _currentFile;

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
            foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.Rows)
            {
                if (!row.IsDisposed && row.DataItem.Controller != null)
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
        /// <param name="errorText">The error text.</param>
        /// <param name="ee">The exception to associate with the row.</param>
        void SetRowErrorText(BetterDataGridViewRow<RowDataItem> row, string errorText,
            ExtractException ee)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new SetErrorTextDelegate(SetRowErrorText),
                        new object[] { row, errorText, ee });
                    return;
                }

                // Check if this is the same error
                if (!row.IsDisposed && !row.ErrorText.Equals(errorText, StringComparison.Ordinal))
                {
                    row.ErrorText = errorText;
                    row.Cells[(int)GridColumns.FAMService].Value = "error";
                    row.Cells[(int)GridColumns.FDRSService].Value = "error";
                    row.Cells[(int)GridColumns.CPUUsage].Value = "error";
                    row.DataItem.Exception = ee;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30806", ex);
            }
        }

        /// <summary>
        /// Thread method used to start/stop services based on the service control data.
        /// </summary>
        /// <param name="controlServicesData">A <see cref="ServiceControlThreadParameters"/>
        /// object containing the data specifying which machines, which service and whether
        /// to start or stop it.</param>
        void ControlServices(object controlServicesData)
        {
            var data = controlServicesData as ServiceControlThreadParameters;
            if (data == null)
            {
                return;
            }

            // Get the data from arguments
            var famService = data.ServiceToControl.HasFlag(ServiceToControl.FamService);
            var fdrsService = data.ServiceToControl.HasFlag(ServiceToControl.FdrsService);
            var startService = data.StartService;

            foreach (BetterDataGridViewRow<RowDataItem> row in data.Rows)
            {
                Task.Factory.StartNew(() =>
                    {
                        string machineName = null;
                        try
                        {
                            if (!row.IsDisposed)
                            {
                                var controller = row.DataItem.Controller;
                                machineName = controller.MachineName;
                                if (famService)
                                {
                                    controller.ControlFamService(startService);
                                }
                                if (fdrsService)
                                {
                                    controller.ControlFdrsService(startService);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var ee = ExtractException.AsExtractException("ELI30803", ex);
                            ee.AddDebugData("Machine Name", machineName ?? "Unknown", false);
                            ee.AddDebugData("Starting Service", startService, false);

                            SetRowErrorText(row, ex.Message, ee);
                        }
                    }
                );
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
        /// Updates the service machine controller.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="machineName">Name of the machine.</param>
        static void UpdateServiceMachineController(BetterDataGridViewRow<RowDataItem> row, string groupName,
            string machineName = null)
        {
            if (!row.IsDisposed)
            {
                var controller = row.DataItem.Controller;
                controller.GroupName = groupName;
                if (machineName != null)
                {
                    controller.MachineName = machineName;
                }
            }
        }

        /// <summary>
        /// Updates the enabled states of the toolstrip buttons and menu items.
        /// </summary>
        void UpdateEnabledStates()
        {
            int rowCount = _machineListGridView.Rows.Count;
            int selectedCount = _machineListGridView.SelectedRows.Count;

            _saveFileToolStripButton.Enabled = rowCount > 0;
            _saveToolStripMenuItem.Enabled = rowCount > 0;

            _removeMachineToolStripButton.Enabled = selectedCount > 0;
            _modifyServiceDatabaseToolStripButton.Enabled = selectedCount == 1;
            _editMachineGroupAndNameToolStripButton.Enabled = selectedCount > 0;
        }

        /// <summary>
        /// Controls the service for selected rows.
        /// </summary>
        /// <param name="startService">if set to <see langword="true"/> the service
        /// will be started, if <see langword="false"/> the service will be stopped..</param>
        void ControlServiceForSelectedRows(bool startService)
        {
            List<BetterDataGridViewRow<RowDataItem>> selectedRows = new List<BetterDataGridViewRow<RowDataItem>>();
            foreach (BetterDataGridViewRow<RowDataItem> row in _machineListGridView.SelectedRows)
            {
                if (row.Visible)
                {
                    selectedRows.Add(row);
                }
            }

            // Put the data into the structure to pass to the controller method.
            // Pass in true to start the service, false to stop the services.
            var controllerData = new ServiceControlThreadParameters(selectedRows,
                _serviceToControl, startService);
            Task.Factory.StartNew(ControlServices, controllerData);
        }

        #endregion Methods

    }
}
