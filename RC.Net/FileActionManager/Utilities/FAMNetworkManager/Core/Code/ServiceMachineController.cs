using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceProcess;
using System.Text;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Class to hold the updated data results for the <see cref="ServiceMachineController"/> objects.
    /// </summary>
    class ServiceStatusUpdateData
    {
        #region Fields

        /// <summary>
        /// String representing the current status for the FAM service.
        /// </summary>
        readonly string _famServiceStatus;

        /// <summary>
        /// String representing the current status for the FDRS service.
        /// </summary>
        readonly string _fdrsServiceStatus;

        /// <summary>
        /// The current percentage of the CPU that is being used on the service machine.
        /// </summary>
        readonly float? _cpuPercentage;

        /// <summary>
        /// Any exceptions associated with this data
        /// </summary>
        readonly ExtractException _exception;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceStatusUpdateData"/> class.
        /// </summary>
        /// <param name="famServiceStatus">The FAM service status.</param>
        /// <param name="fdrsServiceStatus">The FDRS service status.</param>
        /// <param name="cpuPercentage">The cpu percentage.</param>
        /// <param name="exception">The exception.</param>
        public ServiceStatusUpdateData(string famServiceStatus, string fdrsServiceStatus,
            float? cpuPercentage, ExtractException exception = null)
        {
            _famServiceStatus = famServiceStatus;
            _fdrsServiceStatus = fdrsServiceStatus;
            _cpuPercentage = cpuPercentage;
            _exception = exception;
        }

        #region Properties

        /// <summary>
        /// Gets the FAM service status.
        /// </summary>
        /// <value>The fam service status.</value>
        public string FamServiceStatus
        {
            get
            {
                return _famServiceStatus;
            }
        }

        /// <summary>
        /// Gets the FDRS service status.
        /// </summary>
        /// <value>The FDRS service status.</value>
        public string FdrsServiceStatus
        {
            get
            {
                return _fdrsServiceStatus;
            }
        }

        /// <summary>
        /// Gets the cpu percentage as a string.
        /// </summary>
        /// <value>The cpu percentage as a string.</value>
        public string CpuPercentageString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (_cpuPercentage.HasValue)
                {
                    sb.Append(_cpuPercentage.Value.ToString("F1", CultureInfo.CurrentCulture));
                    sb.Append(" %");
                }
                else
                {
                    sb.Append("Counter N/A");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public ExtractException Exception
        {
            get
            {
                return _exception;
            }
        }

        #endregion Properties
    }

    /// <summary>
    /// Class to maintain a service controller and performance counter for a particular machine.
    /// </summary>
    [Serializable]
    public sealed class ServiceMachineController : IDisposable, ISerializable
    {
        #region Constants

        /// <summary>
        /// Current version of this object.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// A performance counter for measuring the CPU usage on the specified machine.
        /// </summary>
        PerformanceCounter _cpuCounter;

        /// <summary>
        /// A service controller for controlling the FAM service on the specified machine.
        /// </summary>
        ServiceController _famServiceController;

        /// <summary>
        /// A service controller for controlling the FDRS service on the specified machine.
        /// </summary>
        ServiceController _fdrsServiceController;

        /// <summary>
        /// The name of the machine.
        /// </summary>
        string _machineName;

        /// <summary>
        /// The group that this machine belongs to.
        /// </summary>
        string _groupName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMachineController"/> class.
        /// </summary>
        ServiceMachineController()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMachineController"/> class.
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        public ServiceMachineController(string machineName)
            : this(machineName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMachineController"/> class.
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="groupName">The name of the group.</param>
        public ServiceMachineController(string machineName, string groupName)
        {
            try
            {
                _machineName = machineName ?? string.Empty;
                _groupName = groupName ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30788", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMachineController"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization context.</param>
        ServiceMachineController(SerializationInfo info, StreamingContext context)
        {
            try
            {
                int version = info.GetInt32("ObjectVersion");
                if (version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI30789", "Unable to load newer version");
                    ee.AddDebugData("Max Version", _CURRENT_VERSION, false);
                    ee.AddDebugData("Version To Load", version, false);

                    throw ee;
                }

                _machineName = info.GetString("MachineName");

                if (info.GetBoolean("HasGroupName"))
                {
                    _groupName = info.GetString("GroupName");
                }

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30790", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the machine.
        /// </summary>
        /// <value>The name of the machine.</value>
        public string MachineName
        {
            get
            {
                return _machineName;
            }
            set
            {
                try
                {
                    ExtractException.Assert("ELI30791", "Machine name cannot be null or empty.",
                        !string.IsNullOrEmpty(value));
                    if (!_machineName.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        _machineName = value;
                        DisposeRemoteObjects();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30810", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>The name of the group.</value>
        public string GroupName
        {
            get
            {
                return _groupName;
            }
            set
            {
                _groupName = value ?? string.Empty;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Controls the fam service.
        /// </summary>
        /// <param name="startService">if set to <see langword="true"/> start service.</param>
        internal void ControlFamService(bool startService)
        {
            try
            {
                if (_famServiceController == null)
                {
                    InitializeRemoteObjects();
                }

                if (_famServiceController != null)
                {
                    _famServiceController.Refresh();
                    ServiceControllerStatus status = _famServiceController.Status;
                    if (startService)
                    {
                        if (status != ServiceControllerStatus.Running
                            || status != ServiceControllerStatus.StartPending)
                        {
                            _famServiceController.Start();
                        }
                    }
                    else
                    {
                        if (status != ServiceControllerStatus.Stopped
                            || status != ServiceControllerStatus.StopPending)
                        {
                            _famServiceController.Stop();
                        }
                    }
                }
            }
            // Invalid operation exception means that the service is not installed.
            catch (InvalidOperationException)
            {
                _famServiceController.Dispose();
                _famServiceController = null;
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI30792", ex);
                ee.AddDebugData("Machine Name", _machineName ?? "Unknown", false);
                ee.AddDebugData("Start/Stop FAM Service", startService ? "Start" : "Stop", false);
                throw ee;
            }
        }

        /// <summary>
        /// Controls the FDRS service.
        /// </summary>
        /// <param name="startService">if set to <see langword="true"/> start service.</param>
        internal void ControlFdrsService(bool startService)
        {
            try
            {
                if (_fdrsServiceController == null)
                {
                    InitializeRemoteObjects();
                }

                if (_fdrsServiceController != null)
                {
                    _fdrsServiceController.Refresh();
                    ServiceControllerStatus status = _fdrsServiceController.Status;
                    if (startService)
                    {
                        if (status != ServiceControllerStatus.Running
                            || status != ServiceControllerStatus.StartPending)
                        {
                            _fdrsServiceController.Start();
                        }
                    }
                    else
                    {
                        if (status != ServiceControllerStatus.Stopped
                            || status != ServiceControllerStatus.StopPending)
                        {
                            _fdrsServiceController.Stop();
                        }
                    }
                }
            }
            // Invalid operation exception means that the service is not installed.
            catch (InvalidOperationException)
            {
                _fdrsServiceController.Dispose();
                _fdrsServiceController = null;
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI30794", ex);
                ee.AddDebugData("Machine Name", _machineName ?? "Unknown", false);
                ee.AddDebugData("Start/Stop FDRS Service", startService ? "Start" : "Stop", false);
                throw ee;
            }
        }

        /// <summary>
        /// Disposes the remote objects.
        /// </summary>
        void DisposeRemoteObjects()
        {
            if (_famServiceController != null)
            {
                _famServiceController.Dispose();
                _famServiceController = null;
            }
            if (_fdrsServiceController != null)
            {
                _fdrsServiceController.Dispose();
                _fdrsServiceController = null;
            }
            if (_cpuCounter != null)
            {
                _cpuCounter.Dispose();
                _cpuCounter = null;
            }
        }

        /// <summary>
        /// Initializes the remote objects.
        /// </summary>
        ExtractException InitializeRemoteObjects()
        {
            try
            {
                var exceptions = new List<Exception>();

                // Try to get the FAM service controller
                var ex = TryGetServiceController("ESFAMService", _machineName,
                    ref _famServiceController);
                if (ex != null)
                {
                    exceptions.Add(ex);
                }

                // Try to get the FDRS service controller
                ex = TryGetServiceController("ESFDRSService", _machineName,
                    ref _fdrsServiceController);
                if (ex != null)
                {
                    exceptions.Add(ex);
                }

                // Try to get the performance counter
                if (_cpuCounter == null)
                {
                    try
                    {
                        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total",
                            _machineName);
                        _cpuCounter.NextValue();
                    }
                    catch (Exception e)
                    {
                        if (_cpuCounter != null)
                        {
                            _cpuCounter.Dispose();
                            _cpuCounter = null;
                        }
                        exceptions.Add(e);
                    }
                }

                if (exceptions.Count > 0)
                {
                    if (exceptions.Count == 1)
                    {
                        throw ExtractException.AsExtractException("ELI31038", exceptions[0]);
                    }
                    else
                    {
                        var ae = new AggregateException(exceptions.ToArray());
                        throw ExtractException.AsExtractException("ELI31028", ae.Flatten());
                    }
                }

                return null;
            }
            catch (Exception ex1)
            {
                return ExtractException.AsExtractException("ELI31029", ex1);
            }
        }

        /// <summary>
        /// Tries the get service controller.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="controller">The controller.</param>
        static Exception TryGetServiceController(string serviceName, string machineName,
            ref ServiceController controller)
        {
            if (controller == null)
            {
                if (SystemMethods.CheckServiceExists(serviceName, machineName))
                {
                    try
                    {
                        controller = new ServiceController(serviceName, machineName);
                    }
                    catch(Exception ex)
                    {
                        if (controller != null)
                        {
                            controller.Dispose();
                            controller = null;
                        }

                        return ex;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Refreshes the data from the service controllers and performance counters.
        /// </summary>
        /// <returns>The updated data.</returns>
        internal ServiceStatusUpdateData RefreshData()
        {
            try
            {
                var exceptions = new List<Exception>();
                var ee = InitializeRemoteObjects();
                if (ee != null)
                {
                    exceptions.Add(ee);
                }

                string famServiceStatus = GetServiceStatus(ref _famServiceController, exceptions);
                string fdrsServiceStatus = GetServiceStatus(ref _fdrsServiceController, exceptions);
                if (exceptions.Count == 1)
                {
                    ee = ExtractException.AsExtractException("ELI31062", exceptions[0]);
                }
                else if (exceptions.Count > 0)
                {
                    AggregateException ae = new AggregateException(exceptions.ToArray());
                    ee = ExtractException.AsExtractException("ELI31063", ae.Flatten());
                }

                return new ServiceStatusUpdateData(famServiceStatus ?? "Not Available",
                    fdrsServiceStatus ?? "Not Available",
                    _cpuCounter != null ? (float?)_cpuCounter.NextValue() : null, ee);
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI30996", ex);
                ee.AddDebugData("Machine Name", _machineName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the service status.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="exceptions">If an exception occurs when getting the service status, it
        /// should be added to this collection.</param>
        /// <returns>The status of the service.</returns>
        static string GetServiceStatus(ref ServiceController controller, List<Exception> exceptions)
        {
            string status = null;
            if (controller != null)
            {
                try
                {
                    controller.Refresh();
                    status = controller.Status.ToString();
                }
                catch(Exception ex)
                {
                    controller.Dispose();
                    controller = null;
                    exceptions.Add(ex);
                }
            }

            return status;
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed
        /// and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeRemoteObjects();
            }
        }

        #endregion

        #region ISerializable Members

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with
        /// the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/>
        /// to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>)
        /// for this serialization.</param>
        /// <exception cref="T:ExtractException">The caller does not have the
        /// required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            try
            {
                bool hasGroupName = _groupName != null;
                info.AddValue("ObjectVersion", _CURRENT_VERSION);
                info.AddValue("MachineName", _machineName);
                info.AddValue("HasGroupName", hasGroupName);
                if (hasGroupName)
                {
                    info.AddValue("GroupName", _groupName);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30797", ex);
            }
        }

        #endregion
    }
}
