using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// The generic application used to run all data entry forms.  The application consists of two
    /// panes:
    /// <list type="bullet">
    /// <item>The Data Entry Panel (DEP) will display the content from a document and allow for the content
    /// to be verified/corrected.  The DEP consists of a <see cref="DataEntryControlHost"/> instance 
    /// populated by controls which implement <see cref="IDataEntryControl"/>.</item>
    /// <item>The image viewer will display the document image itself and allow for interaction with the
    /// DEP such as highlighting the image area associated with the content currently selected in the DEP
    /// or allowing DEP controls to be populated via OCR "swipes" in the image viewer.</item>
    /// </list>
    /// </summary>
    [Guid(Constants.TaskClassDataEntryVerification)]
    [ProgId("Extract.DataEntry.Utilities.DataEntryApplication")]
    [ComVisible(true)]
    public class ComClass : IFileProcessingTask, ICategorizedComponent, ILicensedComponent,
        ICopyableObject, IPersistStream, IConfigurableObject, IMustBeConfiguredObject
    {
        #region Constants

        /// <summary>
        /// The default filename that will appear in the FAM to describe the task the data entry
        /// application is fulfilling
        /// </summary>
        static readonly string _DEFAULT_FILE_ACTION_TASK_NAME = "Data Entry: Verify extracted data";

        /// <summary>
        /// The current version of this object.
        /// <para><b>Versions:</b></para>
        /// <list type="bullet">
        /// <item>2: Added _configFileName</item>
        /// <item>3: Added _inputEventTrackingEnabled</item>
        /// <item>4: Added _countersEnabled</item>
        /// <item>5: Added file tag selection settings</item>
        /// <item>6: Added pagination</item>
        /// <time>7: Removed _inputEventTrackingEnabled - now always enabled</time>
        /// </list>
        /// </summary>
        static readonly int _CURRENT_VERSION = 7;

        #endregion Constants

        #region Fields

        /// <summary>
        /// A thread-safe manager class used to funnel calls from multiple threads to a single 
        /// <see cref="DataEntryApplicationForm"/> instance and be able to route exceptions back to
        /// to the calling thread.
        /// </summary>
        static VerificationForm<DataEntryApplicationForm> _dataEntryFormManager;

        /// <summary>
        /// Indicates whether the object has been modified since being loaded via the 
        /// IPersistStream interface. This is an int because that is the return type of 
        /// IPersistStream::IsDirty in order to support COM values of <see cref="HResult.Ok"/> and 
        /// <see cref="HResult.False"/>.
        /// </summary>
        int _dirty;

        /// <summary>
        /// Settings for verification.
        /// </summary>
        VerificationSettings _settings;

        /// <summary>
        /// The name of the action currently being processed.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the action being processed.
        /// </summary>
        int _actionID;

        /// <summary>
        /// The <see cref="FAMTagManager"/> to use to expand path tags and functions.
        /// </summary>
        FAMTagManager _tagManager;

        /// <summary>
        /// The <see cref="IFileRequestHandler"/> that can be used by the task to carry out requests
        /// for files to be checked out, released or re-ordered in the queue.
        /// </summary>
        IFileRequestHandler _fileRequestHandler;

        // Object for mutexing data entry form manager creation
        static object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ComClass"/> class.
        /// </summary>
        public ComClass()
        {
            _settings = new VerificationSettings();

            // Mutex over data entry form manager creation
            lock (_lock)
            {
                if (_dataEntryFormManager == null)
                {
                    _dataEntryFormManager = new VerificationForm<DataEntryApplicationForm>();
                }
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="VerificationSettings"/>.
        /// </summary>
        public VerificationSettings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }

        #endregion Properties

        #region IPersistStreamMembers

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = this.GetType().GUID;
        }

        /// <summary>
        /// Checks if the object for changes since it was last saved.
        /// </summary>
        /// <returns><see langword="true"/> if the object has changes since it was last saved;
        /// <see langword="false"/> otherwise.</returns>
        public int IsDirty()
        {
            return _dirty;
        }

        /// <summary>
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _settings = VerificationSettings.ReadFrom(reader);
                }

                _dirty = HResult.False;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23992", 
                    "Error loading data entry application settings.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Save the settings
                    _settings.WriteTo(writer);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = HResult.False;
                }
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23789", 
                    "Error saving data entry application settings.", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// <para>NOTE: Not implemented.</para>
        /// </summary>
        /// <param name="size">Will always be <see cref="HResult.NotImplemented"/> to indicate this
        /// method is not implemented.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStreamMembers

        #region ILicensedComponent Members

        /// <summary>
        /// Checks if component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if licensed, <see langword="false"/> if not licensed.
        /// </returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(LicenseIdName.DataEntryCoreComponents);
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23993", "License validation failed.", ex);
            }
        }

        #endregion ILicensedComponent Members

        #region ICategorizedComponent

        /// <summary>
        /// Returns the name of this COM object.
        /// </summary>
        /// <returns>The name of this COM object.</returns>
        public string GetComponentDescription()
        {
            try
            {
                // Attempt to obtain the component description from the config file.
                return _DEFAULT_FILE_ACTION_TASK_NAME;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23877", "Unable to get component description.", ex);
            }
        }

        #endregion ICategorizedComponent

        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The minimum stack size needed for the thread in which this task is to be run.
        /// </value>
        public uint MinStackSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a value indicating that the task displays a UI
        /// </summary>
        public bool DisplaysUI
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Initializes the <see cref="DataEntryApplicationForm"/> to receive documents for
        /// processing.
        /// </summary>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.DataEntryCoreComponents,
                    "ELI26896", _DEFAULT_FILE_ACTION_TASK_NAME);

                ExtractException.Assert("ELI43413", 
                    "Verification is not supported for <All workflows>", !pDB.RunningAllWorkflows);

                if (_settings.CountersEnabled)
                {
                    ExtractException.Assert("ELI29827", "Cannot enable " +
                        "data counters without access to a file processing database!", pDB != null);
                }

                _fileProcessingDB = pDB;
                _actionID = nActionID;
                _tagManager = pFAMTM;
                _fileRequestHandler = pFileRequestHandler;

                // Ask the manager to create and display the data entry form.
                // [FlexIDSCore:3088]
                // Use a larger (4MB) stack size for the form for swiping rules... rule execution
                // may use more than the default 1MB stack size.
                _dataEntryFormManager.ShowForm(CreateDataEntryForm, 0x400000);
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23983", 
                    "Failed to initialize data entry form.", ex);
            }
        }

        /// <summary>
        /// Opens the specified document to allow indexed data to be verified/edited.
        /// </summary>
		/// <param name="pFileRecord">The file record that contains the info of the file being 
		/// processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> object to update progress
        /// (not updated by this class).</param>
        /// <param name="bCancelRequested">If <see langword="true"/>, the user has requested that
        /// processing be cancelled. In this case, the provided document will not be processed.
        /// </param>
        /// <returns><see cref="EFileProcessingResult.kProcessingSuccessful"/> if verification of the
        /// document completed successfully, <see cref="EFileProcessingResult.kProcessingCancelled"/>
        /// if verification of the document was cancelled by the user or
        /// <see cref="EFileProcessingResult.kProcessingSkipped"/> if processing of the current file
        /// was skipped, but the user wishes to continue viewing subsequent documents.
        /// </returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.DataEntryCoreComponents,
                    "ELI26897", _DEFAULT_FILE_ACTION_TASK_NAME);

                EFileProcessingResult processingResult;

                if (bCancelRequested)
                {
                    // If a cancel has been requested, since this task is cancelable, don't attempt
                    // verification, just return kProcessingCancelled.
                    processingResult = EFileProcessingResult.kProcessingCancelled;
                }
                else
                {
                    // [FlexIDSCore:5318]
                    // Assign any alternate component data directory root defined in the database
                    // to be used in addition to the default component data directory.
                    if (pDB != null)
                    {
                        DataEntryMethods.AlternateComponentDataDir =
                            pDB.GetDBInfoSetting("AlternateComponentDataDir", false);
                    }

                    // As long as processing has not been cancelled, open the supplied document in the
                    // data entry form.
                    processingResult = _dataEntryFormManager.ShowDocument(pFileRecord.Name,
                        pFileRecord.FileID, nActionID, pFAMTM, pDB);
                }

                return processingResult;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23875", 
                    "Failed processing file.", ex);
            }
        }

        /// <summary>
        /// Caller can cancel processing by using this method. The 
        /// <see cref="DataEntryApplicationForm"/> will be closed.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.DataEntryCoreComponents,
                    "ELI26898", _DEFAULT_FILE_ACTION_TASK_NAME);

                _dataEntryFormManager.Cancel();
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23874", "Unable to cancel.", ex);
            }
        }

        /// <summary>
        /// Ends processing by closing the <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.DataEntryCoreComponents,
                    "ELI26899", _DEFAULT_FILE_ACTION_TASK_NAME);

                _dataEntryFormManager.CloseForm();
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23873", "Unable to close.", ex);
            }
        }

        /// <summary>
        /// Called to the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means that call to <see cref="ProcessFile"/> or <see cref="Close"/> may come
        /// while the Standby call is still occurring. If this happens, the return value of Standby
        /// will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            try
            {
                return _dataEntryFormManager.Standby();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33954", "Error stopping verification.");
            }
        }

		#endregion IFileProcessingTask Members

		#region IAccessRequired Members

		/// <summary>
		/// Returns bool value indicating if the task requires admin access
		/// </summary>
		/// <returns><see langword="true"/> if the task requires admin access
		/// <see langword="false"/> if task does not require admin access</returns>
		public bool RequiresAdminAccess()
		{
			return false;
		}

		#endregion IAccessRequired Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the current <see cref="ComClass"/> instance.
        /// </summary>
        /// <returns>A copy of the current <see cref="ComClass"/> instance.</returns>
        public object Clone()
        {
            try
            {
                ComClass clone = new ComClass();

                clone.CopyFrom(this);

                return clone;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23879", "Clone failed.", ex);
            }
        }

        /// <summary>
        /// Copies the value of the provided <see cref="ComClass"/> instance into the current one.
        /// </summary>
        /// <param name="pObject">The object to copy from.</param>
        /// <exception cref="ExtractException">If the supplied object is not of type
        /// <see cref="ComClass"/>.</exception>
        public void CopyFrom(object pObject)
        {
            try
            {
                ComClass copyThis = pObject as ComClass;
                ExtractException.Assert("ELI23795", "Cannot copy from an object of a different type!",
                    copyThis != null);

                // Copy properties here
                _settings = copyThis._settings;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23880", "Unable to copy COM class.", ex);
            }
        }

        #endregion ICopyableObject Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to run the class as an <see cref="IFileProcessingTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was not successful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.DataEntryCoreComponents,
                    "ELI26900", _DEFAULT_FILE_ACTION_TASK_NAME);

                // Create a new configuration form to display the configurable settings to the user.
                using (ConfigurationForm configForm = new ConfigurationForm(_settings))
                {
                    // Display the configuration screen.
                    if (configForm.Configure())
                    {
                        _settings = configForm.Settings;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI25471", "Configuration failed.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Tests to ensure <see cref="ComClass"/> is properly configured to run as an 
        /// <see cref="IFileProcessingTask"/>.
        /// <para><b>Note:</b></para>
        /// If <see cref="ComClass"/> is not properly configured, and exception will be logged which
        /// provides details about the configuration problem.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="ComClass"/>is properly configured;
        /// <see langword="false"/> if it is not.</returns>
        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrEmpty(_settings.ConfigFileName);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI25486", "Unable to determine if configured.", ex);
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region Private Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <see langword="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <see langword="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Creates a <see cref="DataEntryApplicationForm"/> using the current settings.
        /// </summary>
        /// <returns>A <see cref="DataEntryApplicationForm"/> using the current settings.</returns>
        IVerificationForm CreateDataEntryForm()
        {
            return new DataEntryApplicationForm(_settings, false, _fileProcessingDB, _actionID,
                _tagManager, _fileRequestHandler);
        }

        #endregion Private Methods
    }
}
