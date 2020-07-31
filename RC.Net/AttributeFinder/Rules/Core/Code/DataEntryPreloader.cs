using Extract.DataEntry;
using Extract.DataEntry.Utilities.DataEntryApplication;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="DataEntryPreloader"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("B6CD3E16-9203-4AC0-B3B9-5E12A4954CA3")]
    [CLSCompliant(false)]
    public interface IDataEntryPreloader : IOutputHandler, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// Gets or sets the data entry configuration file defining the data entry configuration
        /// that should be used to pre-load the data.
        /// </summary>
        /// <value>
        /// The data entry configuration file.
        /// </value>
        string ConfigFileName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IOutputHandler"/> that uses a data entry verification configuration to
    /// load the data in order to organize it as it would be following a save in the data entry UI.
    /// Two potential uses of this rule are:
    /// 1) Increases performance loading the data for verification since the attributes have already
    ///    been re-ordered and populated by default auto-update queries.
    /// 2) Allows for easier comparison of indexing/expected data to rule output by normalizing rule
    ///    output data to data entry UI output.
    /// </summary>
    [ComVisible(true)]
    [Guid("21FE6DF3-6C14-42C5-9170-4F2CC84EB982")]
    [CLSCompliant(false)]
    public class DataEntryPreloader : IdentifiableObject, IDataEntryPreloader, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Data entry preloader";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.DataEntryCoreComponents;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The data entry configuration file defining the data entry configuration that should be
        /// used to pre-load the data.
        /// </summary>
        string _configFileName;

        /// <summary>
        /// <see cref="ConfigFileName"/> with any path tags expanded.
        /// </summary>
        string _expandedConfigFileName;

        /// <summary>
        /// An <see cref="AttributeFinderPathTags"/> to expand any tags in
        /// <see cref="ConfigFileName"/>.
        /// </summary>
        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        /// <summary>
        /// A <see cref="MiscUtils"/> instance to use for converting IPersistStream implementations
        /// to/from a stringized byte stream in the UI thread.
        /// </summary>
        MiscUtils _uiThreadMiscUtils;

        /// <summary>
        /// A <see cref="MiscUtils"/> instance to use for converting IPersistStream implementations
        /// to/from a stringized byte stream in the rule execution thread.
        /// </summary>
        MiscUtils _ruleExectutionThreadMiscUtils = new MiscUtils();

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="DataEntryPreloader"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Static Fields

        // https://extract.atlassian.net/browse/ISSUE-13472
        // It had once been allowed for each rule object instance to have it's own UI thread.
        // However, since the rule objects are instantiated via COM, their finalization is,
        // in the current code at least, not deterministic and the rule objects will often
        // stick around long after they are no longer used (even after stopping and restarting
        // the FAM). This meant many DEPs could get created and needlessly sit on memory. Since
        // access to ProcessOutput was already synchronized, the speed hit of requiring each 
        // instance to use the same UI thread should be negligible (at least if all instances are
        // using the same DEP). That is why all members related to the UI thread are now static.

        /// <summary>
        /// Synchronizes access to the <see cref="ProcessOutput"/> method and the UI thread.
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// Keeps track of all pre-loader instances that have referenced the UI thread. Only after
        /// all instances that have used the UI thread have been disposed should the UI thread be
        /// stopped.
        /// </summary>
        static HashSet<DataEntryPreloader> _uiThreadReferences = new HashSet<DataEntryPreloader>();

        /// <summary>
        /// The <see cref="Thread"/> in which the <see cref="DataEntryApplicationForm"/> that will
        /// load the data will be run.
        /// </summary>
        static Thread _uiThread;

        /// <summary>
        /// The config file used to define the DEP loaded in the UI thread; used to confirm the UI
        /// thread can be used by the current instance.
        /// </summary>
        static string _uiThreadConfigFileName;

        /// <summary>
        /// Indicates whether the UI thread has successfully started.
        /// </summary>
        static ManualResetEvent _uiThreadStartedEvent = new ManualResetEvent(false);

        /// <summary>
        /// Indicates whether the UI thread has been requested to stop.
        /// </summary>
        static ManualResetEvent _stopUiThreadEvent = new ManualResetEvent(false);

        /// <summary>
        /// Indicates whether the UI thread has ended.
        /// </summary>
        static ManualResetEvent _uiThreadEndedEvent = new ManualResetEvent(false);

        /// <summary>
        /// Signals to the UI thread that the rule execution thread has requested a new pre-load
        /// operation.
        /// </summary>
        static AutoResetEvent _preloadRequestedEvent = new AutoResetEvent(false);

        /// <summary>
        /// Signals to the rule execution thread that the UI thread has completed a pre-load
        /// operation.
        /// </summary>
        static AutoResetEvent _preloadCompleteEvent = new AutoResetEvent(false);

        /// <summary>
        /// Used to pass attribute data back and forth between the two threads.
        /// </summary>
        static string _stringizedAttributeData;

        /// <summary>
        /// Keeps track of any exceptions that occur on the UI thread so that the rule execution
        /// thread can use them.
        /// </summary>
        static ExtractException _uiThreadException;

        #endregion Static Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPreloader"/> class.
        /// </summary>
        public DataEntryPreloader()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35047");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPreloader"/> class as a copy of
        /// the specified <see paramref="DataEntryPreloader"/>.
        /// </summary>
        /// <param name="dataEntryPreloader">The <see cref="DataEntryPreloader"/> from which
        /// settings should be copied.</param>
        public DataEntryPreloader(DataEntryPreloader dataEntryPreloader)
        {
            try
            {
                CopyFrom(dataEntryPreloader);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35048");
            }
        }

        #endregion Constructors

        #region Finalizer

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DataEntryPreloader"/> is reclaimed by garbage collection.
        /// </summary>
        ~DataEntryPreloader()
		{
			Dispose(false);
		}

        #endregion Finalizer

        #region Properties

        /// <summary>
        /// Gets or sets the data entry configuration file defining the data entry configuration
        /// that should be used to pre-load the data.
        /// </summary>
        /// <value>
        /// The data entry configuration file.
        /// </value>
        public string ConfigFileName
        {
            get
            {
                return _configFileName;
            }

            set
            {
                try
                {
                    if (value != _configFileName)
                    {
                        _configFileName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35049");
                }
            }
        }

        #endregion Properties

        #region IOutputHandler

        /// <summary>
        /// Processes the output (<see paramref="pAttributes"/>) by 
        /// </summary>
        /// <param name="pAttributes">The output to process.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the output is from.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> that can be used to update
        /// processing status.</param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI35050", _COMPONENT_DESCRIPTION);

                _pathTags.Document = pDoc;

                IUnknownVector preLoadedAttributes = GetPreloadedData(pAttributes);

                // Don't simply re-assign the attributes variable since it is this vector that is
                // output; repopulate it instead.
                pAttributes.Clear();
                pAttributes.Append(preLoadedAttributes); 
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35051", "Failed to pre-load data for data entry.");
            }
        }

        #endregion IOutputHandler

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="DataEntryPreloader"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI35052", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                DataEntryPreloader cloneOfThis = (DataEntryPreloader)Clone();

                using (DataEntryPreloaderSettingsDialog dlg
                    = new DataEntryPreloaderSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI35053",
                    "Error running '" + _COMPONENT_DESCRIPTION + "'configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="DataEntryPreloader"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DataEntryPreloader"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new DataEntryPreloader(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35054",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }
        /// <summary>
        /// Copies the specified <see cref="DataEntryPreloader"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as DataEntryPreloader;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to DataEntryPreloader");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35055",
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
        /// <returns><see cref="HResult.Ok"/> if changes have been made;
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
                    ConfigFileName = reader.ReadString();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35056",
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
                    writer.Write(ConfigFileName);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35057",
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

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is configured; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ConfigFileName))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35058",
                    "Error checking configuration of " + _COMPONENT_DESCRIPTION + ".");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region IDisposable Members

        /// <overloads>
        /// Releases resources used by the <see cref="DataEntryPreloader"/>
        /// </overloads>
        /// <summary>
        /// Releases resources used by the <see cref="DataEntryPreloader"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the <see cref="DataEntryPreloader"/>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            // https://extract.atlassian.net/browse/ISSUE-13542
            // Currently there is no mechanism to trigger disposable rule objects to be disposed.
            if (disposing)
            {
            }

            try
            {
                lock (_lock)
                {
                    if (_uiThreadReferences.Contains(this))
                    {
                        _uiThreadReferences.Remove(this);

                        if (_uiThreadReferences.Count == 0 && UiThreadRunning)
                        {
                            StopUiThread();
                        }
                    }
                }
            }
            catch { }
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="DataEntryPreloader"/> instance into this one.
        /// </summary><param name="source">The <see cref="DataEntryPreloader"/> from which to copy.
        /// </param>
        void CopyFrom(DataEntryPreloader source)
        {
            ConfigFileName = source.ConfigFileName;

            _dirty = true;
        }

        /// <summary>
        /// Gets a value indicating whether the UI thread is currently running.
        /// </summary>
        /// <value><see langword="true"/> if the UI thread is currently running; otherwise,
        /// <see langword="false"/>.
        /// </value>
        static bool UiThreadRunning
        {
            get
            {
                return _uiThread != null &&
                    _uiThreadStartedEvent.WaitOne(0) &&
                    !_uiThreadEndedEvent.WaitOne(0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the UI thread is currently ready to pre-load data..
        /// </summary>
        /// <value><see langword="true"/> if the UI thread is currently ready to pre-load data;
        /// otherwise, <see langword="false"/>.
        /// </value>
        static bool UiThreadReady
        {
            get
            {
                return UiThreadRunning &&
                    !_stopUiThreadEvent.WaitOne(0);
            }
        }

        /// <summary>
        /// Retrieves the result of pre-loaded the specified <see paramref="attributes"/> into the
        /// specified data entry verification configuration.
        /// <para><b>Note</b></para>
        /// Runs on the rule execution thread.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// to load.</param>
        /// <returns>The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s that results from
        /// pre-loaded the specified <see paramref="attributes"/> into the specified data entry
        /// verification configuration.</returns>
        IUnknownVector GetPreloadedData(IUnknownVector attributes)
        {
            try
            {
                lock (_lock)
                {
                    _uiThreadException = null;

                    attributes.ReportMemoryUsage();

                    _expandedConfigFileName = _pathTags.Expand(ConfigFileName);

                    // If the UI thread is currently running a different DEP, re-spawn the UI thread
                    // with the DEP needed here.
                    if (UiThreadReady && _expandedConfigFileName != _uiThreadConfigFileName)
                    {
                        StopUiThread();
                    }

                    _uiThreadReferences.Add(this);

                    // If the UI thread isn't already running and ready load data, start (or
                    // restart) it now.
                    if (!UiThreadReady)
                    {
                        StartUiThread();
                    }

                    // We need to wait until either the UI thread is ready or it ended (in the case
                    // the UI thread failed to initialize).
                    WaitHandle[] waitHandles = new WaitHandle[] 
                    {
                        _uiThreadEndedEvent,
                        _uiThreadStartedEvent
                    };
                    WaitHandle.WaitAny(waitHandles);

                    // If any exception was handled when starting the UI thread, throw it.
                    if (_uiThreadException != null)
                    {
                        throw _uiThreadException;
                    }
                    ExtractException.Assert("ELI35065", "Data entry pre-load thread not ready",
                        UiThreadReady);

                    // Convert the attribute data to a stringized byte stream in order to pass the
                    // data to the UI thread, then notify the UI thread that a load operation is
                    // ready for processing.
                    _stringizedAttributeData =
                        _ruleExectutionThreadMiscUtils.GetObjectAsStringizedByteStream(attributes);
                    _preloadRequestedEvent.Set();

                    // Wait for the load to complete.
                    _preloadCompleteEvent.WaitOne();

                    // If any exception was handled in the UI thread when loading the data, throw
                    // it.
                    if (_uiThreadException != null)
                    {
                        throw _uiThreadException;
                    }
                    // Retrieve the attribute hierarchy from the UI thread.
                    IUnknownVector loadedAttributes = (IUnknownVector)
                        _ruleExectutionThreadMiscUtils.GetObjectFromStringizedByteStream(
                            _stringizedAttributeData);

                    return loadedAttributes; 
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35066");
            }
        }

        /// <summary>
        /// Starts (or restarts) the UI thread.
        /// <para><b>Note</b></para>
        /// Runs on the rule execution thread.
        /// </summary>
        void StartUiThread()
        {
            try
            {
                lock (_lock)
                {
                    // If the UI thread is already running and ready, there is nothing to do.
                    if (UiThreadReady)
                    {
                        return;
                    }

                    // Before attempting to start the UI thread, ensure is reset.
                    _uiThreadStartedEvent.Reset();

                    // Stop any thread that is currently running.
                    if (UiThreadRunning)
                    {
                        StopUiThread();
                    }

                    // Clear the stop and ended events when launching a new thread.
                    _stopUiThreadEvent.Reset();
                    _uiThreadEndedEvent.Reset();

                    // Spawn a new STA thread with a 4MB stack (swiping rule execution may use more than
                    // the default 1MB stack size).
                    _uiThreadConfigFileName = _expandedConfigFileName;
                    var settings = new VerificationSettings(_expandedConfigFileName);
                    _uiThread = new Thread(new ThreadStart(() =>
                        RunUiThread(settings)), 0x400000);
                    _uiThread.SetApartmentState(ApartmentState.STA);
                    _uiThread.Start();
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI35067",
                    "Failed to start data entry pre-load thread.", ex);
            }
        }

        /// <summary>
        /// Stops the UI thread.
        /// <para><b>Note</b></para>
        /// Runs on the rule execution thread.
        /// </summary>
        static void StopUiThread()
        {
            try
            {
                lock (_lock)
                {
                    _uiThreadConfigFileName = null;

                    // If the UI thread is not running, there is nothing to do.
                    if (!UiThreadRunning)
                    {
                        return;
                    }

                    // Signal to the UI thread that it should stop.
                    _stopUiThreadEvent.Set();

                    // Allow for up to 10 seconds for the thread to end cleanly; if it hasn't
                    // stopped by then, force-kill it.
                    if (!_uiThreadEndedEvent.WaitOne(10000))
                    {
                        ExtractException.Log("ELI35068",
                            "DataEntry pre-loader thread failed to end cleanly.");
                        if (_uiThread != null && _uiThread.ThreadState == ThreadState.Running)
                        {
                            _uiThread.Abort();
                        }
                        _uiThreadEndedEvent.Set();
                        _uiThread = null;
                    } 
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI35069",
                    "Failed to stop data entry pre-load thread.", ex);
            }
        }

        /// <summary>
        /// This is the main method for the UI thread. It will create a
        /// <see cref="DataEntryApplicationForm"/> instance, then loop waiting for the next pre-load
        /// request or for the thread to be ended.
        /// <para><b>Note</b></para>
        /// Runs on the UI thread.
        /// </summary>
        /// <param name="settings">The <see cref="VerificationSettings"/> for the
        /// <see cref="DataEntryApplicationForm"/>.</param>
        void RunUiThread(VerificationSettings settings)
        {
            try
            {
                // Create a version of MiscUtils that can be used to handle stringized data on this
                // thread.
                _uiThreadMiscUtils = new MiscUtils();

                using (var dataEntryApplicationForm = CreateDataEntryApplicationForm(settings))
                {
                    // Now the DataEntryApplicationForm is created, let the rule execution
                    // thread know that this thread is ready.
                    _uiThreadStartedEvent.Set();

                    // Wait until the next pre-load event is requested or the UI thread is ended.
                    WaitHandle[] waitHandles = new WaitHandle[] 
                        {
                            _stopUiThreadEvent,
                            _preloadRequestedEvent
                        };
                    while (WaitHandle.WaitAny(waitHandles) == 1)
                    {
                        try
                        {
                            // A pre-load operation has been requested; do it.
                            PreloadData(dataEntryApplicationForm);
                        }
                        catch (Exception ex)
                        {
                            _uiThreadException = ex.AsExtract("ELI35070");

                            _stopUiThreadEvent.Set();
                        }
                        finally
                        {
                            _preloadCompleteEvent.Set();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _uiThreadException = ex.AsExtract("ELI35071");
            }
            finally
            {
                // Mark the thread as ended unless it is not in the running state (such as if it
                // were aborted.
                if (Thread.CurrentThread.ThreadState == ThreadState.Running)
                {
                    _uiThreadEndedEvent.Set();
                }
            }
        }

        /// <summary>
        /// Creates and initializes a new invisible <see cref="DataEntryApplicationForm"/> instance
        /// to use to load data.
        /// <para><b>Note</b></para>
        /// Runs on the UI thread. 
        /// </summary>
        /// <param name="settings">The <see cref="VerificationSettings"/> for the
        /// <see cref="DataEntryApplicationForm"/>.</param>
        /// <returns>A new invisible <see cref="DataEntryApplicationForm"/> instance.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        static DataEntryApplicationForm CreateDataEntryApplicationForm(VerificationSettings settings)
        {
            // If the dataEntryApplicationForm were to display exceptions, it would block execution
            // on this thread. Prevent any exceptions from being displayed while attempting to
            // initialize it.
            ExtractException.BlockExceptionDisplays();

            DataEntryApplicationForm dataEntryApplicationForm = null;
            bool succeeded = false;

            try
            {
                dataEntryApplicationForm = new DataEntryApplicationForm(settings);

                // We need to load the form for the controls to work, but we don't want the
                // form to be visible.
                dataEntryApplicationForm.MakeInvisible();
                dataEntryApplicationForm.Show();
                succeeded = true;

                return dataEntryApplicationForm;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI35072",
                    "Failed to create data entry form for pre-load.", ex);
            }
            finally
            {
                var blockedDisplayExceptions = ExtractException.EndBlockExceptionDisplays();

                // Don't consider the creation successful if any exceptions were displayed when
                // initializing the form.
                succeeded &= (blockedDisplayExceptions.Length == 0);

                if (!succeeded && dataEntryApplicationForm != null)
                {
                    dataEntryApplicationForm.Dispose();
                }

                // If any exceptions were prevented from displaying, throw the first one here as it
                // likely prevented the data from being correctly loaded. This will override any
                // exception thrown from this block; likely the blocked exception would have been
                // the underlying cause for any subsequent thrown exception.
                if (blockedDisplayExceptions.Length > 0)
                {
                    throw blockedDisplayExceptions[0];
                }
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="attributes"/> into the configured data entry
        /// verification configuration in order to re-order them and pre-populate them with default
        /// auto-update queries to match the output that would result from saving
        /// <see paramref="attributes"/> with the data entry configuration.
        /// <para><b>Note</b></para>
        /// Runs on the UI thread.
        /// </summary>
        /// <param name="dataEntryApplicationForm">The <see cref="DataEntryApplicationForm"/>
        /// instance to use to perform the load.</param>
        [SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
        void PreloadData(DataEntryApplicationForm dataEntryApplicationForm)
        {
            // If the dataEntryApplicationForm were to display exceptions, it would block execution
            // on this thread. Prevent any exceptions from being displayed while attempting to use
            // dataEntryApplicationForm to load the data.
            ExtractException.BlockExceptionDisplays();

            try
            {
                IUnknownVector attributes = (IUnknownVector)
                    _uiThreadMiscUtils.GetObjectFromStringizedByteStream(_stringizedAttributeData);
                attributes.ReportMemoryUsage();
                // In case the there are multiple configurations for different document types,
                // ensure the proper configuration is loaded for the supplied data.
                dataEntryApplicationForm.ActiveDataEntryControlHost.LoadData(attributes, null,
                    forEditing: false, initialSelection: FieldSelection.DoNotReset);
                IUnknownVector loadedAttributes =
                    dataEntryApplicationForm.ActiveDataEntryControlHost.GetData(false);
                ExtractException.Assert("ELI41673", "Failed to load data", loadedAttributes != null);

                loadedAttributes.ReportMemoryUsage();
                _stringizedAttributeData =
                    _uiThreadMiscUtils.GetObjectAsStringizedByteStream(loadedAttributes);
                dataEntryApplicationForm.ActiveDataEntryControlHost.ClearData();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35059");
            }
            finally
            {
                // If any exceptions were prevented from displaying, throw the first one here as it
                // likely prevented the data from being correctly loaded. This will override any
                // exception thrown from this block; likely the blocked exception would have been
                // the underlying cause for any subsequent thrown exception.
                var blockedDisplayExceptions = ExtractException.EndBlockExceptionDisplays();
                if (blockedDisplayExceptions.Length > 0)
                {
                    throw blockedDisplayExceptions[0];
                }
            }
        }

        #endregion Private Members
    }
}
