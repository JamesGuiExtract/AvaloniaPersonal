using Extract.Interfaces;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileSuppliers
{
    /// <summary>
    /// An <see cref="IFileSupplier"/> implementation that adds a context menu into the Window's
    /// shell which will supply selected files if used.
    /// </summary>
    [ComVisible(true)]
    [Guid("AC260C30-E2F5-4EA3-A120-193FF8D94B05")]
    public class ContextMenuFileSupplier : IFileSupplier, ICategorizedComponent, 
        IConfigurableObject, IMustBeConfiguredObject, ICopyableObject, ILicensedComponent,
        IPersistStream, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of this file supplier
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Context menu file supplier";

        /// <summary>
        /// Current file supplier version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        /// <summary>
        /// The frequency in ms with which the service-hosted file receiver should be polled for
        /// supplied files.
        /// </summary>
        const int _POLLING_INTERVAL = 1000;

        /// <summary>
        /// The text of the sub menu that should be added to the context menu.
        /// </summary>
        const string _PARENT_MENU_NAME = "Send to FAM";
        
        /// <summary>
        /// The file containing the icon that should be used for the adde sub-menu.
        /// </summary>
        static readonly string _ICON_FILE =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "FAM.ico");

        #endregion Constants

        #region Fields

        // Target for the files that are being supplier
        IFileSupplierTarget _fileTarget;

        /// <summary>
        /// Provides the connection to the ESIPCService where the file listener will be hosted to
        /// relay selections from the context menu.
        /// </summary>
        ChannelFactory<IWcfFileReceiverManager> _channelFactory;

        /// <summary>
        /// Used to add a file receiver and poll it for added files.
        /// </summary>
        IWcfFileReceiverManager _fileReceiverManager;

        /// <summary>
        /// The ID of the file receiver for this instance.
        /// </summary>
        int _fileReceiverID = -1;

        /// <summary>
        /// Indicates supplying should stop.
        /// </summary>
        ManualResetEvent _stopSupplying = new ManualResetEvent(false);

        /// <summary>
        /// Indicates files have been retrieved from the file listener and are ready to be queued.
        /// </summary>
        ManualResetEvent _filesAreAvailable = new ManualResetEvent(false);

        /// <summary>
        /// Indicates the supplier has stopped.
        /// </summary>
        ManualResetEvent _supplyingStopped = new ManualResetEvent(false);

        /// <summary>
        /// Indicates the supplier is actively supplying (un-paused).
        /// </summary>
        ManualResetEvent _supplyingActivated = new ManualResetEvent(true); 

        /// <summary>
        /// This thread watches the service-hosted file receiver files.
        /// </summary>
        Task _watchingTask;

        /// <summary>
        /// This thread collects the files from the file receiver, expands directories into the
        /// files it contains, and queues them to the FAM DB.
        /// </summary>
        Task _queuingTask;

        /// <summary>
        /// This thread watches for both the _watchingTask and _queingTask to end. Once they have,
        /// it closes the _channelFactory and signals the FAM that supplying has stopped.
        /// </summary>
        Task _waitingForStopTask;

        /// <summary>
        /// A collection of filenames the have been retrieved from the file receiver and are waiting
        /// to be queued to the FAM DB.
        /// </summary>
        ConcurrentQueue<string> _filesToQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Indicates whether a connection failure has been logged since the last successful
        /// connection (used to avoid spamming the log with connection failures).
        /// </summary>
        volatile bool _loggedConnectionFailed;

        /// <summary>
        /// Indicates whether the WCF channel is in the faulted state.
        /// </summary>
        volatile bool _wcfChannelFaulted;

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="ContextMenuFileSupplier"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuFileSupplier"/> class.
        /// </summary>
        public ContextMenuFileSupplier()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33141");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuFileSupplier"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="ContextMenuFileSupplier"/> from which
        /// settings should be copied.</param>
        public ContextMenuFileSupplier(ContextMenuFileSupplier task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33142");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the menu option.
        /// </summary>
        /// <value>
        /// The name of the menu option.
        /// </value>
        public string MenuOptionName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Window's filter string that describes which files the menu should be
        /// available for (multiple filters can be delimited with a semi-colon).
        /// </summary>
        /// <value>
        /// The file filter.
        /// </value>
        public string FileFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the menu option should be availab only beneath
        /// <see cref="PathRoot"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if limiting to <see cref="PathRoot"/>, otherwise
        /// <see langword="false"/>.
        /// </value>
        public bool LimitPathRoot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path a file must be under in order for the context menu to display.
        /// </summary>
        /// <value>
        /// The path a file must be under in order for the context menu to display.
        /// </value>
        public string PathRoot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether files from selected folders will be included.
        /// </summary>
        /// <value><see langword="true"/> if files from selected folders are to be included;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool IncludeFolders
        {
            get;
            set;
        }

        #endregion Properties

        #region IFileSupplier Members

        /// <summary>
        /// Starts file supplying.
        /// </summary>
        /// <param name="pTarget">The IFileSupplerTarget that receives the files</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        [CLSCompliant(false)]
        public void Start(IFileSupplierTarget pTarget, FAMTagManager pFAMTM)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33143", _COMPONENT_DESCRIPTION);

                _fileTarget = pTarget;

                _stopSupplying.Reset();
                _supplyingStopped.Reset();
                _supplyingActivated.Set();
                _filesAreAvailable.Reset();

                _watchingTask = new Task(() => WatchForFiles());
                _watchingTask.Start();

                _queuingTask = new Task(() => QueueFiles());
                _queuingTask.Start();

                _waitingForStopTask = new Task(() => WaitForSupplyingToStop());
                _waitingForStopTask.Start();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33144", "Failed to start " +_COMPONENT_DESCRIPTION);
            }
        }

        /// <summary>
        /// Stops file supplying
        /// </summary>
        public void Stop()
        {
            _stopSupplying.Set();
            _supplyingStopped.WaitOne();
        }

        /// <summary>
        /// Pauses file supplying.
        /// </summary>
        public void Pause()
        {
            _supplyingActivated.Reset();
        }

        /// <summary>
        /// Resumes file supplying.
        /// </summary>
        public void Resume()
        {
            _supplyingActivated.Set();
        }

        /// <summary>
        /// Returns bool value indicating if the task requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFileSupplier Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ContextMenuFileSupplier"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33145", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                ContextMenuFileSupplier cloneOfThis = (ContextMenuFileSupplier)Clone();

                using (ContextMenuFileSupplierSettingsDialog dlg
                    = new ContextMenuFileSupplierSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33146", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrWhiteSpace(MenuOptionName) &&
                       !string.IsNullOrWhiteSpace(FileFilter);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33147",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ContextMenuFileSupplier"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ContextMenuFileSupplier"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new ContextMenuFileSupplier(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33148",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ContextMenuFileSupplier"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as ContextMenuFileSupplier;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to ContextMenuFileSupplier");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33149",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns>
        ///   <see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    MenuOptionName = reader.ReadString();
                    FileFilter = reader.ReadString();
                    LimitPathRoot = reader.ReadBoolean();
                    PathRoot = reader.ReadString();
                    IncludeFolders = reader.ReadBoolean();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33150",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {

                    writer.Write(MenuOptionName);
                    writer.Write(FileFilter);
                    writer.Write(LimitPathRoot);
                    writer.Write(PathRoot);
                    writer.Write(IncludeFolders);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33151",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ContextMenuFileSupplier"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ContextMenuFileSupplier"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="ContextMenuFileSupplier"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_channelFactory != null)
                {
                    try
                    {
                        try
                        {
                            CloseWcfChannel();
                        }
                        catch { }

                        if (_stopSupplying != null)
                        {
                            _stopSupplying.Dispose();
                            _stopSupplying = null;
                        }
                        if (_supplyingStopped != null)
                        {
                            _supplyingStopped.Dispose();
                            _supplyingStopped = null;
                        }
                        if (_filesAreAvailable != null)
                        {
                            _filesAreAvailable.Dispose();
                            _filesAreAvailable = null;
                        }
                        if (_supplyingActivated != null)
                        {
                            _supplyingActivated.Dispose();
                            _supplyingActivated = null;
                        }
                        if (_watchingTask != null)
                        {
                            _watchingTask.Dispose();
                            _watchingTask = null;
                        }
                        if (_queuingTask != null)
                        {
                            _queuingTask.Dispose();
                            _queuingTask = null;
                        }
                        if (_waitingForStopTask != null)
                        {
                            _waitingForStopTask.Dispose();
                            _waitingForStopTask = null;
                        }
                    }
                    catch { }
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "Extract File Suppliers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileSuppliers);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "Extract File Suppliers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileSuppliers);
        }

        /// <summary>
        /// Copies the specified <see cref="ContextMenuFileSupplier"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="ContextMenuFileSupplier"/> from which to copy.
        /// </param>
        void CopyFrom(ContextMenuFileSupplier source)
        {
            MenuOptionName = source.MenuOptionName;
            FileFilter = source.FileFilter;
            LimitPathRoot = source.LimitPathRoot;
            PathRoot = source.PathRoot;
            IncludeFolders = source.IncludeFolders;

            _dirty = true;
        }

        /// <summary>
        /// Opens a WCF channel to the ESIPCService and adds a file receiver for this instance.
        /// </summary>
        bool OpenWcfChannel()
        {
            try
            {
                if ((_channelFactory != null && _channelFactory.State != CommunicationState.Opened) ||
                    (_channelFactory != null && _channelFactory.Endpoint == null) ||
                    (OperationContext.Current != null && OperationContext.Current.Channel != null &&
                     OperationContext.Current.Channel.State != CommunicationState.Opened))
                {
                    ExtractException ee = new ExtractException("ELI33123",
                        "Context menu file supplier: lost connection to Extract IPC Service.");
                    ee.AddDebugData("Name", MenuOptionName, false);
                    ee.Log();

                    AbortWcfConnection();

                    return false;
                }

                if (_channelFactory == null)
                {
                    _channelFactory = new ChannelFactory<IWcfFileReceiverManager>(
                        new NetNamedPipeBinding(), new EndpointAddress(FileReceiver.WcfAddress));
                }

                if (_fileReceiverManager == null)
                {
                    _fileReceiverManager = _channelFactory.CreateChannel();
                }

                // Add a file receiver for this instance.
                if (_fileReceiverID == -1)
                {
                    MenuDefinition menuDefinition = new MenuDefinition(MenuOptionName);
                    menuDefinition.ParentMenuItemName = _PARENT_MENU_NAME;
                    menuDefinition.ParentIconFileName = _ICON_FILE;

                    FileFilter fileFilter =
                        new FileFilter(LimitPathRoot ? PathRoot : null, FileFilter, IncludeFolders);

                    _fileReceiverID = _fileReceiverManager.AddFileReceiver(menuDefinition, fileFilter);

                    _wcfChannelFaulted = false;
                    ((ICommunicationObject)_fileReceiverManager).Faulted += WcfConnectionFaulted;
                }

                // Now that we have successfully connected log that we have if the connection had
                // previously failed, and reset _loggedConnectionFailed so if the connection is
                // unexpectedly lost, another exception will be logged;
                if (_loggedConnectionFailed)
                {
                    ExtractException ee = new ExtractException("ELI33179",
                        "Application trace: Context menu file supplier connection restored.");
                    ee.AddDebugData("Name", MenuOptionName, false);
                    ee.Log();

                    _loggedConnectionFailed = false;
                }

                return true;
            }
            catch (Exception ex)
            {
                if (_channelFactory != null)
                {
                    try
                    {
                        _channelFactory.Close();
                    }
                    catch { }
                    _channelFactory = null;
                }

                _fileReceiverManager = null;
                _fileReceiverID = -1;

                if (!_loggedConnectionFailed)
                {
                    _loggedConnectionFailed = true;
                    ExtractException ee = new ExtractException("ELI33122",
                        "Context menu file supplier: failed to connect to Extract IPC Service.",
                        ex);
                    ee.AddDebugData("Name", MenuOptionName, false);
                    ee.Log();
                }
            }

            return false;
        }

        /// <summary>
        /// Handles the case that the WCF connection faulted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void WcfConnectionFaulted(object sender, EventArgs e)
        {
            try
            {
                _wcfChannelFaulted = true;

                _loggedConnectionFailed = true;

                ExtractException ee = new ExtractException("ELI33178",
                            "Context menu file supplier: lost connection to Extract IPC Service.");
                ee.AddDebugData("Name", MenuOptionName, false);
                ee.Log();

                AbortWcfConnection();
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33177");
            }
        }

        /// <summary>
        /// Attempts to close any open WCF connection. Any exceptions encountered while doing so
        /// are ignored.
        /// </summary>
        void AbortWcfConnection()
        {
            // Closing may throw an exception depending on its current state. Ignore any
            // exceptions.
            try
            {
                if (_fileReceiverManager != null && _fileReceiverID != -1)
                {
                    ((ICommunicationObject)_fileReceiverManager).Faulted -= WcfConnectionFaulted;
                }
                
                if (_channelFactory != null)
                {
                    _channelFactory.Close();
                }
            }
            catch { }

            _channelFactory = null;
            _fileReceiverManager = null;
            _fileReceiverID = -1;
        }

        /// <summary>
        /// Closes the WCF channel.
        /// </summary>
        void CloseWcfChannel()
        {
            if (_fileReceiverManager != null)
            {
                if (_fileReceiverID != -1)
                {
                    _fileReceiverManager.RemoveFileReceiver(_fileReceiverID);
                    _fileReceiverID = -1;
                    ((ICommunicationObject)_fileReceiverManager).Faulted -= WcfConnectionFaulted;
                }

                _fileReceiverManager = null;
            }

            if (_channelFactory != null)
            {
                _channelFactory.Close();
                _channelFactory = null;
            }
        }

        /// <summary>
        /// This thread watches the service-hosted file receiver for files and expands directories
        /// into the files it contains.
        /// </summary>
        void WatchForFiles()
        {
            try
            {
                while (!_stopSupplying.WaitOne(0))
                {
                    // If paused, don't collect any more files until supplyling is resumed.
                    _supplyingActivated.WaitOne();

                    IEnumerable<string> suppliedFiles = null;

                    if (OpenWcfChannel())
                    {
                        try
                        {
                            suppliedFiles = _fileReceiverManager.PopSuppliedFiles(_fileReceiverID);
                        }
                        catch (Exception ex)
                        {
                            // If the channel connection has faulted, we will simply try to
                            // reconnect on the next iteration; ignore this exception.
                            // If the call failed despite the channel not having faulted,
                            // reconnecting isn't likely to resolve the problem. Throw to fail
                            // the supplier.
                            if (!_wcfChannelFaulted)

                            {
                                ExtractException ee = new ExtractException("ELI33176",
                                "Context menu file supplier: error communicating with Extract IPC Service.", ex);
                                ee.AddDebugData("Name", MenuOptionName, false);
                                throw ee;
                            }
                        }
                    }

                    if (suppliedFiles != null && suppliedFiles.Any())
                    {
                        EnqueueFiles(suppliedFiles);

                        _filesAreAvailable.Set();
                    }

                    Thread.Sleep(_POLLING_INTERVAL);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33152");
            }
            finally
            {
                _stopSupplying.Set();
            }
        }

        /// <summary>
        /// Adds the specified files to _filesToQueue. If any of the path are directories and
        /// IncludeFolders is <see langword="true"/>, the directory is expanded into all the
        /// qualifying files beneath that folder (recursively).
        /// </summary>
        /// <param name="fileNames">The fileNames to queue.</param>
        void EnqueueFiles(IEnumerable<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (Directory.Exists(fileName))
                {
                    if (IncludeFolders)
                    {
                        IEnumerable<string> subFolderFiles = FileFilter.Split(';')
                            .SelectMany(filter =>
                                Directory.GetFiles(fileName, filter, SearchOption.AllDirectories));

                        EnqueueFiles(subFolderFiles);
                    }
                }
                else
                {
                    _filesToQueue.Enqueue(fileName);
                }
            }
        }

        /// <summary>
        /// This thread takes the files from the WatchForFiles thread and queues them to the FAM DB.
        /// </summary>
        void QueueFiles()
        {
            try
            {
                WaitHandle[] handlesToWaitFor = new WaitHandle[]
                {
                    _stopSupplying,
                    _filesAreAvailable
                };

                while (WaitHandle.WaitAny(handlesToWaitFor) == 1)
                {
                    _filesAreAvailable.Reset();

                    string fileName;
                    while (!_stopSupplying.WaitOne(0) && _filesToQueue.TryDequeue(out fileName))
                    {
                        // If paused, wait to queue any more until un-paused.
                        _supplyingActivated.WaitOne();

                        _fileTarget.NotifyFileAdded(fileName, this);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33153");
            }
            finally
            {
                _stopSupplying.Set();
            }
        }

        /// <summary>
        /// This thread watches for both the _watchingTask and _queingTask to end. Once they have,
        /// it closes the _channelFactory and signals the FAM that supplying has stopped.
        /// </summary>
        void WaitForSupplyingToStop()
        {
            try
            {
                _watchingTask.Wait();
            }
            catch { }

            try
            {
                _queuingTask.Wait();
            }
            catch { }

            try
            {
                CloseWcfChannel();
            }
            catch { }

            _fileTarget.NotifyFileSupplyingDone(this);
            _supplyingStopped.Set();

            List<ExtractException> exceptions = new List<ExtractException>();

            if (_watchingTask.Exception != null)
            {
                exceptions.Add(_watchingTask.Exception.AsExtract("ELI33154"));
            }

            if (_queuingTask.Exception != null)
            {
                exceptions.Add(_queuingTask.Exception.AsExtract("ELI33155"));
            }

            if (exceptions.Count() > 0)
            {
                ExtractException ee = new ExtractException("ELI33156",
                    MenuOptionName + " context menu supplier has failed.",
                    exceptions.AsAggregateException());
                ee.Display();
            }
        }

        #endregion Private Members
    }
}
