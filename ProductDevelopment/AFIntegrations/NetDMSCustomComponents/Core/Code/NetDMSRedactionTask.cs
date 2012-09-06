using DexFlow.Framework;
using Dx_DocContent;
using Dx_DocDescriptor;
using Extract;
using Extract.Interop;
using Extract.Licensing;
using Interop.Weak.UCLID_COMLMLib;
using Interop.Weak.UCLID_COMUTILSLib;
using Interop.Weak.UCLID_FILEPROCESSINGLib;
using NetDMSUtilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetDMSCustomComponents
{
    /// <summary>
    /// The interface definition for <see cref="NetDMSRedactionTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("F464E489-0FFB-48F6-9D83-9A5AB0D3432F")]
    public interface INetDMSRedactionTask : INetDMSConnectionSettings, IFileProcessingTask,
        ICategorizedComponent, IConfigurableObject, IMustBeConfiguredObject, ICopyableObject,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the name of the VOA file to be used to determine where to place redactions.
        /// </summary>
        /// <value>
        /// The name of the VOA file to be used to determine where to place redactions.
        /// </value>
        string DataFileName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which extracts an area of the source image as a
    /// separate image based on attribute(s) in a VOA file.
    /// </summary>
    [ComVisible(true)]
    [Guid("C24BD7A6-D5DC-48D1-81B3-F410D6437224")]
    [ProgId("NetDMSCustomComponents.NetDMSRedactionTask")]
    public class NetDMSRedactionTask : NetDMSClassBase, INetDMSRedactionTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "NetDMS: Add redactions";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the VOA file to be used to determine where to place redactions.
        /// </summary>
        string _dataFileName = "<SourceDocName>.voa";

        /// <summary>
        /// Helper class used to execute code that cannot be run as part of this assembly due to the
        /// fact that the NetDMS API (and subsequently this assembly) are not strong-named. The
        /// code that cannot be used is code which passes or returns parameter or value types that
        /// are defined in one of our COM modules. Since this assembly is not strong-named it must
        /// use weak references to those COM objects, but the method signature being used will
        /// expect a strongly named type and will fail to compile.
        /// RedactionFileLoader.LoadFrom(IUnknownVector attributes, strng sourceDocument) is
        /// one example of a method that cannot be used in this assembly because the signature calls
        /// for a strongly named IUnknownVector, but assembly code is only capable of providing a
        /// weakly named IUnknownVector.
        /// </summary>
        NetDMSMethods _netDMSMethods = new NetDMSMethods();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSRedactionTask"/> class.
        /// </summary>
        public NetDMSRedactionTask()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34907");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSRedactionTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="NetDMSRedactionTask"/> from which settings should
        /// be copied.</param>
        public NetDMSRedactionTask(NetDMSRedactionTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34908");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the VOA file to be used to determine where to place redactions.
        /// </summary>
        /// <value>
        /// The name of the VOA file to be used to determine where to place redactions.
        /// </value>
        public string DataFileName
        {
            get
            {
                return _dataFileName;
            }

            set
            {
                try
                {
                    if (value != _dataFileName)
                    {
                        _dataFileName = value;
                        Dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34943");
                }
            }
        }

        #endregion Properties

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
        /// Performs configuration needed to create a valid <see cref="NetDMSRedactionTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34909", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                NetDMSRedactionTask cloneOfThis = (NetDMSRedactionTask)Clone();

                using (NetDMSRedactionTaskSettingsDialog dlg
                    = new NetDMSRedactionTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI34910",
                    "Error configuring" + _COMPONENT_DESCRIPTION + ".", ex);
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
                return IsConnectionConfigured;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34911",
                    "Unable to check " + _COMPONENT_DESCRIPTION + " configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="NetDMSRedactionTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="NetDMSRedactionTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new NetDMSRedactionTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34912", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="NetDMSRedactionTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as NetDMSRedactionTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to " + this.GetType().ToString());
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34913", "Unable to copy object.", ex);
            }
        }

        #endregion ICopyableObject Members

        #region IFileProcessingTask Members

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            // Do nothing, this task is not cancellable
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34914",
                    "Error stopping " + _COMPONENT_DESCRIPTION + ".");
            }
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
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
            return true;
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34915", _COMPONENT_DESCRIPTION);

                Connect();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34916",
                    "Unable to initialize " + _COMPONENT_DESCRIPTION + " task.");
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
        /// <returns>An <see cref="EFileProcessingResult"/> indicating the result of the
        /// processing.</returns>
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34917", _COMPONENT_DESCRIPTION);

                IDocument document = GetCorrespondingDocument(pFileRecord.Name);

                AddRedactions(pFileRecord, pFAMTM, document);

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34918", "Unable to process the file.");
            }
        }

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access.</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFileProcessingTask Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34919",
                    "Unable to determine license status.", ex);
            }
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
            return HResult.FromBoolean(Dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    LoadConnectionSettings(reader);
                    DataFileName = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                Dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34920",
                    "Unable to load object from stream.", ex);
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
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    SaveConnectionSettings(writer);
                    writer.Write(DataFileName);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    Dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34921",
                    "Unable to save object to stream", ex);
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

        #region Private Members

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
        /// Copies the specified <see cref="NetDMSRedactionTask"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="NetDMSRedactionTask"/> from which to copy.
        /// </param>
        void CopyFrom(NetDMSRedactionTask source)
        {
            CopyConnectionSettings(source);
            DataFileName = source.DataFileName;

            Dirty = true;
        }

        /// <summary>
        /// Adds redactions to the NetDMS <see paramref="document"/> that corresponds with the
        /// <see paramref="pFileRecord"/>.
        /// </summary>
        /// <param name="pFileRecord">The file record that contains the info of the file being
        /// processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="document">The <see cref="IDocument"/> representing the document in NetDMS.
        /// </param>
        void AddRedactions(FileRecord pFileRecord, FAMTagManager pFAMTM, IDocument document)
        {
            DocumentContent documentContent =
                new DocumentContent(TaskClient, document.ContentID, Session, true);

            try
            {
                string voaFileName = pFAMTM.ExpandTagsAndFunctions(DataFileName, pFileRecord.Name);

                foreach (RedactionArea redactionArea in
                    _netDMSMethods.GetDocumentRedactionAreas(pFileRecord.Name, voaFileName))
                {
                    HideRectangleAnnotation redaction = new HideRectangleAnnotation();
                    redaction.Location.X = redactionArea.Bounds.Left;
                    redaction.Location.Y = redactionArea.Bounds.Top;
                    redaction.Width = redactionArea.Bounds.Width;
                    redaction.Height = redactionArea.Bounds.Height;

                    documentContent.Descriptor.AddHideRectangleAnnotation(redactionArea.Page, redaction);
                }

                documentContent.Save(Session.Project);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34922");
            }
            finally
            {
                documentContent.Close();
            }
        }

        #endregion Private Members
    }
}
