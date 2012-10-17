using DexFlow.Client;
using DexFlow.Framework;
using Extract;
using Extract.Interop;
using Extract.Licensing;
using Interop.Weak.UCLID_COMLMLib;
using Interop.Weak.UCLID_COMUTILSLib;
using Interop.Weak.UCLID_FILEPROCESSINGLib;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extract.NetDMSCustomComponents
{
    /// <summary>
    /// The interface definition for <see cref="NetDMSAdvanceWorkItemTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("5F2DD962-505E-4432-AF7A-14DF22E434D1")]
    public interface INetDMSAdvanceWorkItemTask : INetDMSConnectionSettings, IFileProcessingTask,
        ICategorizedComponent, IConfigurableObject, IMustBeConfiguredObject, ICopyableObject,
        ILicensedComponent, IPersistStream
    {
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which extracts an area of the source image as a
    /// separate image based on attribute(s) in a VOA file.
    /// </summary>
    [ComVisible(true)]
    [Guid("E598DDF9-2804-41E2-982F-9C440A345310")]
    [ProgId("NetDMSCustomComponents.NetDMSAdvanceWorkItemTask")]
    public class NetDMSAdvanceWorkItemTask : NetDMSClassBase, INetDMSAdvanceWorkItemTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "NetDMS: Advance work item";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        // [FlexIDSIntegrations:331]
        // Prior to the 9.1 release, this ID should be changed to NetdmsComponents;
        //const LicenseIdName _LICENSE_ID = LicenseIdName.NetdmsComponents;
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSAdvanceWorkItemTask"/> class.
        /// </summary>
        public NetDMSAdvanceWorkItemTask()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34853");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSAdvanceWorkItemTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="NetDMSAdvanceWorkItemTask"/> from which settings should
        /// be copied.</param>
        public NetDMSAdvanceWorkItemTask(NetDMSAdvanceWorkItemTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34854");
            }
        }

        #endregion Constructors

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
        /// Performs configuration needed to create a valid <see cref="NetDMSAdvanceWorkItemTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34856", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                NetDMSAdvanceWorkItemTask cloneOfThis = (NetDMSAdvanceWorkItemTask)Clone();

                using (NetDMSAdvanceWorkItemTaskSettingsDialog dlg
                    = new NetDMSAdvanceWorkItemTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI34857",
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
                throw ex.CreateComVisible("ELI34877",
                    "Unable to check " + _COMPONENT_DESCRIPTION + " configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="NetDMSAdvanceWorkItemTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="NetDMSAdvanceWorkItemTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new NetDMSAdvanceWorkItemTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34858", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="NetDMSAdvanceWorkItemTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as NetDMSAdvanceWorkItemTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to " + this.GetType().ToString());
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34859", "Unable to copy object.", ex);
            }
        }

        #endregion ICopyableObject Members

        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The the minimum stack size needed for the thread in which this task is to be run.
        /// </value>
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
                throw ex.CreateComVisible("ELI34860",
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34861", _COMPONENT_DESCRIPTION);

                Connect();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34862",
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34863", _COMPONENT_DESCRIPTION);

                IDocument document = GetCorrespondingDocument(pFileRecord.Name);

                AdvanceParcelToNextNodeIfComplete(pFileRecord.Name, pDB, nActionID, document);

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34864", "Unable to process the file.");
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
                throw ExtractException.CreateComVisible("ELI34865",
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
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    LoadConnectionSettings(reader);
                }

                // Freshly loaded object is no longer dirty
                Dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI34866",
                    "Unable to load object from stream.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.
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
                throw ExtractException.CreateComVisible("ELI34867",
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
        /// Copies the specified <see cref="NetDMSAdvanceWorkItemTask"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="NetDMSAdvanceWorkItemTask"/> from which to copy.
        /// </param>
        void CopyFrom(NetDMSAdvanceWorkItemTask source)
        {
            CopyConnectionSettings(source);

            Dirty = true;
        }

        /// <summary>
        /// Advances the parcel to next node if complete.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileProcessingDB">The file processing DB.</param>
        /// <param name="actionId">The ID of the current action in the FAM DB.</param>
        /// <param name="document">The <see cref="IDocument"/> whose <see cref="IParcel"/> is to be
        /// advanced in the NetDMS workflow.</param>
        bool AdvanceParcelToNextNodeIfComplete(string fileName, FileProcessingDB fileProcessingDB,
            int actionId, IDocument document)
        {
            // Check to be sure that all other documents in this folder have completed processing
            // for on the current action.
            string actionName = fileProcessingDB.GetActionName(actionId);
            string parcelDirectory = Path.GetDirectoryName(fileName);
            string indexFileName = Path.Combine(parcelDirectory, "Index.txt");
            ExtractException.Assert("ELI34944", "Parcel index file is missing!",
                File.Exists(indexFileName));

            foreach (string documentFileName in File.ReadAllLines(indexFileName)
                    .Where(documentFileName =>
                        !fileName.EndsWith(documentFileName, StringComparison.OrdinalIgnoreCase)))
            {
                string fullDocumentPath = Path.Combine(parcelDirectory, documentFileName);
                int documentFileId = fileProcessingDB.GetFileID(fullDocumentPath);

                EActionStatus documentActionStatus =
                    fileProcessingDB.GetFileStatus(documentFileId, actionName, false);

                ExtractException.Assert("ELI34947", _COMPONENT_DESCRIPTION + " must be run " +
                    "in an FPS configured to run on a single thread and that retrieves one " +
                    "file at a time. Only one instance of this FPS file can be run across " +
                    "all machines targeting the same NetDMS project.",
                    documentActionStatus != EActionStatus.kActionProcessing);

                if (documentActionStatus != EActionStatus.kActionCompleted)
                {
                    return false;
                }
            }

            // Look up the node to advance the work item from using the id encoded in the path
            // rather than using the node the document is currently in. This avoids the possibility
            // that the parcel gets advance from a different node than it was in when the document
            // was exported.
            string[] fileNameParts = Path.GetFileName(parcelDirectory).Split('-');
            long nodeISN = Int64.Parse(fileNameParts[1], CultureInfo.InvariantCulture);
            INode node = GetNetDMSObject<INode>(SystemTables.Node, nodeISN);
            IParcel parcel = GetNetDMSObject<IParcel>(SystemTables.Parcel, document.ParcelISN);

            // Use the NetDMS to move the parcel to the next node in the NetDMS workflow using
            // "SendAuto".
            INode destNode;
            IUser destUser;
            API.SendWorkItemAuto(TaskClient, Session.User, Session.Project, node, parcel,
                out destNode, out destUser);

            return true;
        }

        #endregion Private Members
    }
}
