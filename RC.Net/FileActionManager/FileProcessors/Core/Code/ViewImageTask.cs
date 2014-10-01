using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that allows viewing of image files.
    /// </summary>
    [ComVisible(true)]
    [Guid("2F23BB19-0D6E-4188-A520-DE11B3E9C208")]
    [CLSCompliant(false)]
    public interface IViewImageTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
        IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets whether the users are able to apply tags.
        /// </summary>
        /// <value>
        /// Whether the users are able to apply tags.
        /// </value>
        bool AllowTags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets which tags should be available to the users.
        /// </summary>
        /// <value>
        /// A <see cref="FileTagSelectionSettings"/> instance defining which tags should be
        /// available to the users.
        /// </value>
        FileTagSelectionSettings TagSettings
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a file processing task that allows viewing of image files.
    /// </summary>
    [ComVisible(true)]
    [Guid("B7AEF282-5335-4AF2-AC97-4AF30B1A9043")]
    [ProgId("Extract.FileActionManager.ViewImageTask")]
    public class ViewImageTask : IViewImageTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: View image";

        /// <summary>
        /// Current task version.
        /// <para><b>Version 2</b></para>
        /// Added <see cref="AllowTags"/> and <see cref="TagSettings"/>
        /// </summary>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.ExtractCoreObjects;
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// The form to display for viewing images.
        /// </summary>
        static VerificationForm<ViewImageTaskForm> _form;

        /// <summary>
        /// Object used to mutex around the verification form creation.
        /// </summary>
        static readonly object _lock = new object();

        /// <summary>
        /// Specifies whether the users are able to apply tags.
        /// </summary>
        bool _allowTags = true;

        /// <summary>
        /// Specifies which tags should be available to the users.
        /// </summary>
        FileTagSelectionSettings _tagSettings = new FileTagSelectionSettings();

        /// <summary>
        /// Indicates whether this task object is dirty or not
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageTask"/> class.
        /// </summary>
        public ViewImageTask()
        {
            // Lock around form creation
            lock (_lock)
            {
                if (_form == null)
                {
                    _form = new VerificationForm<ViewImageTaskForm>();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageTask"/> class.
        /// </summary>
        public ViewImageTask(ViewImageTask task)
        {
            CopyFrom(task);
        }
        
        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the users are able to apply tags.
        /// </summary>
        /// <value>
        /// Whether the users are able to apply tags.
        /// </value>
        public bool AllowTags
        {
            get
            {
                return _allowTags;
            }

            set
            {
                try 
	            {	        
		            if (value != _allowTags)
                    {
                        _allowTags = value;

                        _dirty = true;
                    }
	            }
	            catch (Exception ex)
	            {
		            throw ex.AsExtract("ELI37257");
	            }
            }
        }

        /// <summary>
        /// Gets or sets which tags should be available to the users.
        /// </summary>
        /// <value>
        /// A <see cref="FileTagSelectionSettings"/> instance defining which tags should be
        /// available to the users.
        /// </value>
        public FileTagSelectionSettings TagSettings
        {
            get
            {
                return _tagSettings;
            }

            set
            {
                try
                {
                    if (value != _tagSettings)
                    {
                        _tagSettings = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37258");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
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
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="ViewImageTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="ViewImageTask"/> from which to copy.</param>
        void CopyFrom(ViewImageTask task)
        {
            _allowTags = task.AllowTags;
            _tagSettings = new FileTagSelectionSettings(task.TagSettings);

            _dirty = true;
        }

        /// <summary>
        /// Creates a <see cref="ViewImageTaskForm"/> with the current settings.
        /// </summary>
        /// <returns>A <see cref="ViewImageTaskForm"/> with the current settings.</returns>
        IVerificationForm CreateViewImageTaskForm()
        {
            return new ViewImageTaskForm(this);
        }
        
        #endregion Methods

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

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ExtractImageAreaTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI37241", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                ViewImageTask cloneOfThis = (ViewImageTask)Clone();

                using (ViewImageTaskSettingsDialog dlg
                    = new ViewImageTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI37242",
                    "Error configuring" + _COMPONENT_DESCRIPTION + ".", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ViewImageTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ViewImageTask"/> instance.</returns>
        public object Clone()
        {
            return new ViewImageTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="ViewImageTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            CopyFrom((ViewImageTask)pObject);
        }

        #endregion ICopyableObject Members
        
        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The the minimum stack size needed for the thread in which this task is to be run.
        /// </value>
        [CLSCompliant(false)]
        public uint MinStackSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI37040", _COMPONENT_DESCRIPTION);

                _form.Cancel();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI37041", 
                    "Error canceling view image task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI37042", _COMPONENT_DESCRIPTION);

                _form.CloseForm();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI37043",
                    "Error closing view image task.", ex);
            }
        }

        /// <summary>
        /// Called notify to the file processor that the pending document queue is empty, but
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
        /// while the Standby call is still ocurring. If this happens, the return value of Standby
        /// will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            try
            {
                return _form.Standby();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37044", "Error stopping view image task.");
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI37045", _COMPONENT_DESCRIPTION);

                _form.ShowForm(CreateViewImageTaskForm);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI37046",
                    "Error initializing view image task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
		/// <param name="pFileRecord">The file record that contains the info of the file being 
		/// processed.</param>
		/// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI37047", _COMPONENT_DESCRIPTION);

                if (bCancelRequested)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                EFileProcessingResult result = _form.ShowDocument(pFileRecord.Name, pFileRecord.FileID,
                    nActionID, pFAMTM, pDB);

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI37048", "Unable to view image.", ex);
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
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings
                    if (reader.Version >= 2)
                    {
                        _allowTags = reader.ReadBoolean();
                        _tagSettings = FileTagSelectionSettings.ReadFrom(reader);
                    }
                }

                // Freshly loaded object is not dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI37049",
                    "Unable to load view image task.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
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
                    writer.Write(_allowTags);
                    _tagSettings.WriteTo(writer);

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
		        throw ExtractException.CreateComVisible("ELI37050", 
			        "Unable to save view image task.", ex);
	        }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members
    }
}
