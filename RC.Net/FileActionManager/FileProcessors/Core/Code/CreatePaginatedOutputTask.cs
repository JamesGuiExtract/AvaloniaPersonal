using Extract.AttributeFinder;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that allows pagination of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("D374658D-F63C-410C-82B4-CBE3829793E1")]
    [CLSCompliant(false)]
    public interface ICreatePaginatedOutputTask : ICategorizedComponent, ICopyableObject,
        IFileProcessingTask, ILicensedComponent, IPersistStream
    {
    }

    /// <summary>
    /// Represents a file processing task that allows pagination of documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("B30EB196-5964-4C7E-9350-B5A85B2D75B0")]
    [ProgId("Extract.FileActionManager.CreatePaginatedOutputTask")]
    [CLSCompliant(false)]
    public class CreatePaginatedOutputTask : ICreatePaginatedOutputTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Pagination - Create output";

        /// <summary>
        /// Current task version.
        /// Versions:
        /// 1. Initial version
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        ///// <summary>
        ///// The default path for expected pagination attributes file
        ///// </summary>
        //const string _DEFAULT_EXPECTED_OUTPUT_PATH = "<SourceDocName>.pagination.evoa";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePaginatedOutputTask"/> class.
        /// </summary>
        public CreatePaginatedOutputTask()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46831");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePaginatedOutputTask"/> class.
        /// </summary>
        public CreatePaginatedOutputTask(CreatePaginatedOutputTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46832");
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

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="CreatePaginatedOutputTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="CreatePaginatedOutputTask"/> instance.</returns>
        public object Clone()
        {
            return new CreatePaginatedOutputTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="CreatePaginatedOutputTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            CopyFrom((CreatePaginatedOutputTask)pObject);
        }

        #endregion ICopyableObject Members
        
        #region IFileProcessingTask Members

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        /// <value>
        /// The minimum stack size needed for the thread in which this task is to be run.
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
        /// Returns a value indicating that the task displays a UI
        /// </summary>
        public bool DisplaysUI
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46835", 
                    "Error canceling " + _COMPONENT_DESCRIPTION + " task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46836",
                    "Error closing " + _COMPONENT_DESCRIPTION + " task.", ex);
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
                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46837", "Error stopping " + _COMPONENT_DESCRIPTION + " task.");
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46838", _COMPONENT_DESCRIPTION);

                UnlockLeadtools.UnlockLeadToolsSupport();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46839",
                    "Error initializing " + _COMPONENT_DESCRIPTION + " task.", ex);
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46840", _COMPONENT_DESCRIPTION);

                if (bCancelRequested)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                IUnknownVector voaData = new IUnknownVectorClass();
                voaData.LoadFrom(pFileRecord.Name + ".voa", false);

                var documentAttribute =
                    AttributeMethods.GetSingleAttributeByName(voaData, "Document");
                var requestAttribute =
                    AttributeMethods.GetSingleAttributeByName(documentAttribute.SubAttributes, "PaginationRequest");
                ExtractException.Assert("ELI46852", "Missing PaginationRequest", requestAttribute != null);

                var request = new PaginationRequest(requestAttribute);

                if (File.Exists(pFileRecord.Name))
                {
                    var ee = new ExtractException("ELI46872", "File already exists.");
                    ee.AddDebugData("Filename", pFileRecord.Name, false);
                    throw ee;
                }

                ImageMethods.StaplePagesAsNewDocument(request.ImagePages, pFileRecord.Name);

                var documentData = AttributeMethods.GetSingleAttributeByName(
                    documentAttribute.SubAttributes, "DocumentData");
                AttributeMethods.CreateUssAndVoaForPaginatedDocument(
                    pFileRecord.Name, documentData.SubAttributes, request.ImagePages);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46841", "Unable to create paginated output.", ex);
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
            return HResult.FromBoolean(false);
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
                }

                // Freshly loaded object is not dirty
                //_dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46842",
                    "Unable to load " + _COMPONENT_DESCRIPTION + " task.", ex);
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

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    //_dirty = false;
                }
	        }
	        catch (Exception ex)
	        {
		        throw ExtractException.CreateComVisible("ELI46843",
                    "Unable to save " + _COMPONENT_DESCRIPTION + " task.", ex);
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
        /// Copies the specified <see cref="CreatePaginatedOutputTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="CreatePaginatedOutputTask"/> from which to copy.</param>
        void CopyFrom(CreatePaginatedOutputTask task)
        {
            //_dirty = true;
        }

        #endregion Private Members
    }
}