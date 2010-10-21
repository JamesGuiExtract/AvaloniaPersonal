﻿using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Extract.FileActionManager.Utilities
{
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
            readonly List<DataGridViewRow> _rows;

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
            public ServiceControlThreadParameters(List<DataGridViewRow> rows,
                ServiceToControl serviceToControl, bool startService)
            {
                _rows = new List<DataGridViewRow>(rows);
                _serviceToControl = serviceToControl;
                _startService = startService;
            }

            #endregion Constructor

            #region Properties

            /// <summary>
            /// Gets the rows.
            /// </summary>
            /// <value>The rows.</value>
            public List<DataGridViewRow> Rows
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

        const string _DEFAULT_FILE_EXT = "fnm";

        /// <summary>
        /// Sleep time for the service status update thread.
        /// </summary>
        const int _REFRESH_THREAD_SLEEP_TIME = 1000;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Service to control with the start and stop buttons.
        /// </summary>
        ServiceToControl _serviceToControl = ServiceToControl.Both;

        /// <summary>
        /// Dictionary of the rows and their related <see cref="ServiceMachineController"/>
        /// </summary>
        ConcurrentDictionary<DataGridViewRow, ServiceMachineController> _rowsAndControllers =
            new ConcurrentDictionary<DataGridViewRow, ServiceMachineController>();

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
        string _fileToOpen;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Delegate method used to set the error information for a row.
        /// </summary>
        /// <param name="row">The row to set the information for.</param>
        /// <param name="errorText">The text for the error.</param>
        delegate void SetErrorTextDelegate(DataGridViewRow row, string errorText);

        /// <summary>
        /// Delegate method used to update the data for individual rows when a refresh is called.
        /// </summary>
        /// <param name="row">The row to update.</param>
        /// <param name="data">The data to update the row with.</param>
        delegate void RefreshRowDataDelegate(DataGridViewRow row, ServiceStatusUpdateData data);

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

            _fileToOpen = fileToOpen;
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

                if (!string.IsNullOrEmpty(_fileToOpen))
                {
                    LoadSettings(_fileToOpen);
                    _fileToOpen = string.Empty;
                }
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
        void HandleOpenFileButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog open = new OpenFileDialog())
                {
                    open.Filter = _FILE_FILTER;
                    open.DefaultExt = "fnm";
                    open.AddExtension = true;
                    open.CheckFileExists = true;
                    open.ValidateNames = true;
                    if (open.ShowDialog() == DialogResult.OK)
                    {
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
        void HandleSaveFileButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog save = new SaveFileDialog())
                {
                    save.Filter = _FILE_FILTER;
                    save.DefaultExt = "fnm";
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
                ExtractException.Display("ELI30776", ex);
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
                using (AddMachineForm addForm = new AddMachineForm(BuildGroupList()))
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        Refresh();
                        using (TemporaryWaitCursor wait = new TemporaryWaitCursor())
                        {
                            ServiceMachineController controller = new ServiceMachineController(
                                addForm.MachineName, addForm.GroupName);
                            DataGridViewRow row = new DataGridViewRow();
                            if (!_rowsAndControllers.TryAdd(row, controller))
                            {
                                ExtractException ee = new ExtractException("ELI30802",
                                    "Unable to add service controller to collection.");
                                ee.AddDebugData("Machine Name", controller.MachineName, false);
                                ee.AddDebugData("Group Name", controller.GroupName, false);

                                // Dispose the control and row
                                controller.Dispose();
                                row.Dispose();

                                throw ee;
                            }

                            row.CreateCells(_machineListGridView);
                            row.Cells[0].Value = controller.MachineName;
                            row.Cells[1].Value = controller.GroupName;
                            _machineListGridView.Rows.Add(row);

                            Thread thread = new Thread(() =>
                                {
                                    try
                                    {
                                        var data = controller.RefreshData();
                                        RefreshRowData(row, data);
                                    }
                                    catch (Exception ex)
                                    {
                                        var ee = new ExtractException("ELI30821",
                                            "Unable to update row data", ex);
                                        ee.AddDebugData("Machine Name",
                                            controller.MachineName, false);
                                        ee.Log();
                                        SetRowErrorText(row, ex.Message);
                                    }
                                }
                            );
                            thread.SetApartmentState(ApartmentState.MTA);
                            thread.Start();
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
                foreach (DataGridViewRow row in _machineListGridView.SelectedRows)
                {
                    ServiceMachineController controller = null;
                    if (_rowsAndControllers.TryRemove(row, out controller))
                    {
                        controller.Dispose();
                    }
                    _machineListGridView.Rows.RemoveAt(row.Index);
                    row.Dispose();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30778", ex);
            }
        }

        /// <summary>
        /// Handles the edit group button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleEditGroupButtonClick(object sender, EventArgs e)
        {
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
                List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in _machineListGridView.SelectedRows)
                {
                    selectedRows.Add(row);
                }

                // Put the data into the structure to pass to the controller method.
                // Pass in true to start the services.
                var controllerData = new ServiceControlThreadParameters(selectedRows,
                    _serviceToControl, true);
                var thread = new Thread(ControlServices);
                thread.Start(controllerData);
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
                List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in _machineListGridView.SelectedRows)
                {
                    selectedRows.Add(row);
                }

                // Put the data into the structure to pass to the controller method.
                // Pass in false to stop the services.
                var controllerData = new ServiceControlThreadParameters(selectedRows,
                    _serviceToControl, false);
                var thread = new Thread(ControlServices);
                thread.Start(controllerData);
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
        void HandleEditServiceDatabaseButtonClick(object sender, EventArgs e)
        {
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
            if (_machineListGridView.SelectedRows.Count > 0)
            {
                _removeMachineToolStripButton.Enabled = true;
            }
            else
            {
                _removeMachineToolStripButton.Enabled = false;
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
        void RefreshRowData(DataGridViewRow row, ServiceStatusUpdateData data)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RefreshRowDataDelegate(RefreshRowData),
                    new object[] { row, data });
                return;
            }

            row.Cells[2].Value = data.FamServiceStatus;
            row.Cells[3].Value = data.FdrsServiceStatus;
            row.Cells[4].Value = data.CpuPercentage.ToString("F1", CultureInfo.CurrentCulture) + " %";
        }

        /// <summary>
        /// Refreshes the status and performance counter data in the grid.
        /// </summary>
        void RefreshData(bool autoUpdate)
        {
            // Clear all cells
            foreach (DataGridViewRow row in _machineListGridView.Rows)
            {
                row.Cells[2].Value = string.Empty;
                row.Cells[3].Value = string.Empty;
                row.Cells[4].Value = string.Empty;
                row.ErrorText = string.Empty;
            }

            if (autoUpdate)
            {
                _refreshData = true;
            }
            else
            {
                Thread thread = new Thread(() =>
                    {
                        RefreshServiceData();
                    }
                );
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
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
                        RefreshServiceData();
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
        void RefreshServiceData()
        {
            // Create a list of all current rows in the UI
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in _machineListGridView.Rows)
            {
                rows.Add(row);
            }

            Parallel.ForEach<DataGridViewRow>(rows, row =>
                {
                    ServiceMachineController controller = null;
                    try
                    {
                        // If the row is still in the collection, get its controller,
                        // refresh the data and update the row
                        if (!_endRefreshThread.WaitOne(0) &&
                            _rowsAndControllers.TryGetValue(row, out controller))
                        {
                            var data = controller.RefreshData();
                            RefreshRowData(row, data);
                        }
                    }
                    catch (Exception ex)
                    {
                        var ee = new ExtractException("ELI30809", "Unable to update data.", ex);
                        string machineName = null;
                        if (controller != null)
                        {
                            machineName = controller.MachineName;
                        }
                        ee.AddDebugData("Machine Name", machineName ?? "Unknown", false);
                        ee.Log();
                        SetRowErrorText(row, ex.Message);
                    }
                }
            );
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
                foreach (DataGridViewRow row in _machineListGridView.Rows)
                {
                    ServiceMachineController controller = null;
                    if (_rowsAndControllers.TryGetValue(row, out controller))
                    {
                        data.Add(controller);
                    }
                }
                using (FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ServiceMachineController>));
                    serializer.Serialize(stream, data);
                    stream.Flush();
                }
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
                    // Ensure the rows are disposed
                    List<DataGridViewRow> rowsToClear = new List<DataGridViewRow>(
                        _machineListGridView.Rows.Count);
                    foreach (DataGridViewRow row in _machineListGridView.Rows)
                    {
                        rowsToClear.Add(row);
                    }
                    _machineListGridView.Rows.Clear();
                    CollectionMethods.ClearAndDispose(rowsToClear);

                    using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<ServiceMachineController>));
                        List<ServiceMachineController> data =
                            (List<ServiceMachineController>)serializer.Deserialize(stream);
                        foreach (ServiceMachineController controller in data)
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(_machineListGridView);
                            row.Cells[0].Value = controller.MachineName;
                            row.Cells[1].Value = controller.GroupName;
                            if (!_rowsAndControllers.TryAdd(row, controller))
                            {
                                ExtractException ee = new ExtractException("ELI30805",
                                    "Unable to add service controller to collection.");
                                ee.AddDebugData("Machine Name", controller.MachineName, false);
                                ee.AddDebugData("Group Name", controller.GroupName, false);

                                // Dispose the control and row
                                controller.Dispose();
                                row.Dispose();

                                throw ee;
                            }
                            _machineListGridView.Rows.Add(row);
                        }
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
            foreach (var controller in _rowsAndControllers.Values)
            {
                if (controller != null)
                {
                    groups.Add(controller.GroupName);
                }
            }

            return groups;
        }

        /// <summary>
        /// Sets the row error text.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="errorText">The error text.</param>
        void SetRowErrorText(DataGridViewRow row, string errorText)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new SetErrorTextDelegate(SetRowErrorText),
                        new object[] { row, errorText });
                    return;
                }

                row.ErrorText = errorText;
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

            // Loop over the rows and start/stop the specified service for each row
            Parallel.ForEach<DataGridViewRow>(data.Rows, row =>
                {
                    ServiceMachineController controller = null;
                    try
                    {
                        if (_rowsAndControllers.TryGetValue(row, out controller))
                        {
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
                        var ee = new ExtractException("ELI30803", "Failed to control service", ex);
                        string machineName = null;
                        if (controller != null)
                        {
                            machineName = controller.MachineName;
                        }
                        ee.AddDebugData("Machine Name", machineName ?? "Unknown", false);
                        ee.AddDebugData("Starting Service", startService, false);
                        ee.Log();

                        SetRowErrorText(row, ex.Message);
                    }
                }
            );
        }

        #endregion Methods
    }
}
