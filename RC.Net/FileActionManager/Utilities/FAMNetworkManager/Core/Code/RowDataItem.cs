using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Class representing the data item that will be held in each
    /// <see cref="Extract.Utilities.Forms.BetterDataGridViewRow{T}"/>
    /// </summary>
    class RowDataItem : IDisposable
    {
        #region Fields

        /// <summary>
        /// The controller associated with this row.
        /// </summary>
        ServiceMachineController _controller;

        /// <summary>
        /// The exception currently associated with this row (may be
        /// <see langword="null"/>
        /// </summary>
        ExtractException _exception;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RowDataItem"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public RowDataItem(ServiceMachineController controller)
            : this(controller, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowDataItem"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="ee">The ee.</param>
        public RowDataItem(ServiceMachineController controller, ExtractException ee)
        {
            _controller = controller;
            _exception = ee;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the controller.
        /// </summary>
        /// <value>The controller.</value>
        public ServiceMachineController Controller
        {
            get
            {
                return _controller;
            }
        }

        /// <summary>
        /// Gets or sets the exception associated with this row.
        /// </summary>
        /// <value>The exception.</value>
        public ExtractException Exception
        {
            get
            {
                return _exception;
            }
            set
            {
                _exception = value;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_controller != null)
                {
                    _controller.Dispose();
                    _controller = null;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// A data grid view row that manages a <see cref="RowDataItem"/>
    /// and keeps track of whether the underlying <see cref="ServiceMachineController"/>
    /// is currently in the midst of refreshing its data.
    /// </summary>
    class FAMNetworkDashboardRow : BetterDataGridViewRow<RowDataItem>
    {
        #region Fields

        /// <summary>
        /// Flag to indicate whether the data is currently being refreshed or not.
        /// </summary>
        volatile bool _refreshingData;

        /// <summary>
        /// Flag to indicate whether a service is being controlled or not.
        /// </summary>
        volatile bool _controllingService;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMNetworkDashboardRow"/> class.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        public FAMNetworkDashboardRow(RowDataItem dataItem)
            : base(dataItem)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Refreshes the data.
        /// </summary>
        /// <returns>The updated data.</returns>
        public ServiceStatusUpdateData RefreshData()
        {
            try
            {
                _refreshingData = true;
                var controller = DataItem.Controller;
                if (controller != null)
                {
                    return controller.RefreshData();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30983", ex);
            }
            finally
            {
                _refreshingData = false;
            }
        }

        /// <summary>
        /// Controls the service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="startService">if set to <see langword="true"/> will attempt
        /// to start service, otherwise will attempt to stop the service.</param>
        public void ControlService(ServiceToControl service, bool startService)
        {
            try
            {
                _controllingService = true;
                var controller = DataItem.Controller;
                var tasks = new List<Task>();
                if (service.HasFlag(ServiceToControl.FamService))
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                        {
                            controller.ControlFamService(startService);
                        }));
                }
                if (service.HasFlag(ServiceToControl.FdrsService))
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                        {
                            controller.ControlFdrsService(startService);
                        }));
                }

                if (tasks.Count > 0)
                {
                    try
                    {
                        Task.WaitAll(tasks.ToArray());
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30336", ex);
            }
            finally
            {
                _controllingService = false;
            }
        }

        /// <summary>
        /// Updates the name of the machine and group.
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="groupName">Name of the group.</param>
        public void UpdateMachineAndGroupName(string machineName, string groupName)
        {
            try
            {
                ServiceMachineController controller = DataItem.Controller;
                if (!string.IsNullOrWhiteSpace(machineName))
                {
                    controller.MachineName = machineName;
                }
                if (!string.IsNullOrWhiteSpace(groupName))
                {
                    controller.GroupName = groupName;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30984", ex);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the service controller
        /// is currently performing an operation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the controller is performing
        /// an operation; otherwise, <see langword="false"/>.
        /// </value>
        public bool ControllerOperating
        {
            get
            {
                return _refreshingData || _controllingService;
            }
        }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public ExtractException Exception
        {
            get
            {
                return DataItem != null ? DataItem.Exception : null;
            }
            set
            {
                if (DataItem != null)
                {
                    DataItem.Exception = value;
                }
            }
        }
        #endregion Properties
    }
}
