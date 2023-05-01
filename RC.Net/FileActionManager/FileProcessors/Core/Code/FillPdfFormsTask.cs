using Extract.FileActionManager.FileProcessors.Utilities;
using Extract.FileActionManager.Forms;
using Extract.GdPicture;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using GdPicture14;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface definition for <see cref="FillPdfFormsTask"/>
    /// </summary>
    [ComVisible(true)]
    [Guid("7C9F6220-94C0-4624-B208-FBDAD7BCC5F0")]
    [CLSCompliant(false)]
    public interface IFillPdfFormsTask :
        ICategorizedComponent,
        IConfigurableObject,
        IMustBeConfiguredObject,
        ICopyableObject,
        IFileProcessingTask,
        ILicensedComponent,
        IPersistStream
    {
        IDictionary FieldsToAutoFill { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> that fills in pdf forms.
    /// </summary>
    [ComVisible(true)]
    [Guid("37079D96-ECA9-4BA6-9D9E-FD265CC3FC52")]
    [ProgId("Extract.FileActionManager.FileProcessors.FillPdfFormsTask")]
    public class FillPdfFormsTask : IFillPdfFormsTask
    {
        #region fields
        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Fill PDF Forms";

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FillPdfFormsTask"/> class.
        /// </summary>
        public FillPdfFormsTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FillPdfFormsTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="FillPdfFormsTask"/> from which settings should
        /// be copied.</param>
        public FillPdfFormsTask(FillPdfFormsTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54227");
            }
        }
        #endregion

        #region IFillPdfFormsTaskMembers
        public IDictionary FieldsToAutoFill { get; set; } = new Dictionary<string, string>();
        #endregion

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="FillPdfFormsTask"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI54228", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (FillPdfFormsTask)Clone();

                FileProcessingDBClass fileProcessingDB = new();
                fileProcessingDB.ConnectLastUsedDBThisProcess();

                using FillPdfFormsSettingsDialog dialog = new(this);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    CopyFrom(dialog.FillPdfFormsTask);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI54229",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object has been configured and <c>false</c> otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI54230", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="FillPdfFormsTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="FillPdfFormsTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new FillPdfFormsTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI54231", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="FillPdfFormsTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (pObject is not FillPdfFormsTask task)
                {
                    throw new InvalidCastException("Invalid copy-from object. Requires " + nameof(FillPdfFormsTask));
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI54232", "Unable to copy object.", ex);
            }
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
        /// Returns a value indicating that the task does not display a UI
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
            // Do nothing, this task is not cancellable
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            // Nothing to do
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <c>true</c>. If the processor wants to cancel processing,
        ///	it should return <c>false</c>. If the processor does not immediately know
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
        /// <returns><c>true</c> to standby until the next file is supplied;
        /// <c>false</c> to cancel processing.</returns>
        public bool Standby()
        {
            return true;
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use to expand path tags and
        /// functions.</param>
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
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI54233",
                    "Unable to initialize \"" + _COMPONENT_DESCRIPTION + "\" task.");
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
        /// <param name="bCancelRequested"><c>true</c> if cancel was requested; 
        /// <c>false</c> otherwise.</param>
        /// <returns>An <see cref="EFileProcessingResult"/> indicating the result of the
        /// processing.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(
            FileRecord pFileRecord,
            int nActionID,
            FAMTagManager pFAMTM,
            FileProcessingDB pDB,
            ProgressStatus pProgressStatus,
            bool bCancelRequested)
        {
            try
            {
                var tagManager = new FileActionManagerPathTags(pFAMTM, pFileRecord.Name);

                if (bCancelRequested)
                {
                    return EFileProcessingResult.kProcessingCancelled;
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI54234", _COMPONENT_DESCRIPTION);

                using GdPictureUtility gdPictureUtilityForm = new();

                GdPictureUtility.ThrowIfStatusNotOK(gdPictureUtilityForm.PdfAPI.LoadFromFile(pFileRecord.Name, false),
                "ELI54251", "The PDF document can't be loaded", new(filePath: pFileRecord.Name));

                // Populate the form fields.
                PopulatePDFFormsFields(gdPictureUtilityForm, pFileRecord, tagManager, pDB);

                SavePDF(gdPictureUtilityForm.PdfAPI, pFileRecord.Name);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI54235", "Failed to fill in PDF Form");
            }
        }
        #endregion IFileProcessingTask Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><c>true</c> if the task requires admin access
        /// <c>false</c> if task does not require admin access.</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IAccessRequired Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><c>true</c> if the component is licensed; <c>false</c> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI54236",
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
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using IStreamReader reader = new(stream, _CURRENT_VERSION);
                this.FieldsToAutoFill = JsonConvert.DeserializeObject<IDictionary>(reader.ReadString());

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI54237",
                    "Unable to load object from stream.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="System.Runtime.InteropServices.ComTypes.IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="System.Runtime.InteropServices.ComTypes.IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <c>true</c>, the flag should be cleared. If 
        /// <c>false</c>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using IStreamWriter writer = new(_CURRENT_VERSION);

                writer.Write(JsonConvert.SerializeObject(this.FieldsToAutoFill));

                writer.WriteTo(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI54238",
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
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
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
        /// <see cref="ExtractCategories.FileProcessorsGuid"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileProcessorsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="IConvertEmailToPdfTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="IConvertEmailToPdfTask"/> from which to copy.</param>
        void CopyFrom(IFillPdfFormsTask task)
        {
            this.FieldsToAutoFill = task.FieldsToAutoFill;
            _dirty = true;
        }
        #endregion Private Members

        #region HelperMethods
        private void PopulatePDFFormsFields(
            GdPictureUtility gdPictureUtility
            , FileRecord fileRecord
            , FileActionManagerPathTags fileActionManagerPathTags
            , IFileProcessingDB fileProcessingDB)
        {
            var formFieldIdsAndTitles = GetFormFieldValues(gdPictureUtility.PdfAPI, fileRecord.Name);

            PopulatePDFForm(formFieldIdsAndTitles, gdPictureUtility.PdfAPI, fileRecord, fileActionManagerPathTags, fileProcessingDB);

            GdPictureUtility.ThrowIfStatusNotOK(gdPictureUtility.PdfAPI.FlattenFormFields(),
                "ELI54252", "Error occurred when flattening forms.", new(filePath: fileRecord.Name));
        }

        private void PopulatePDFForm(
            Dictionary<string, int> formFieldIdsAndTitles
            , GdPicturePDF gdPicturePdf
            , FileRecord fileRecord
            , FileActionManagerPathTags fileActionManagerPathTags
            , IFileProcessingDB fileProcessingDB)
        {
            var expandTextHelper = new ExpandTextHelper();
            // Auto populate every value in the PDF Form contained in the FieldsToAutoFill
            foreach (DictionaryEntry entry in FieldsToAutoFill)
            {
                if (formFieldIdsAndTitles.TryGetValue((string)entry.Key, out int fieldID))
                {
                    var debugData = new DebugData();
                    debugData.AdditionalDebugData.Add("FormFieldID", fieldID.ToString());
                    debugData.AdditionalDebugData.Add("FormFieldValue", (string)entry.Value);
                    debugData.AdditionalDebugData.Add("FormFieldToFill", entry.Key.ToString());
                    debugData.AdditionalDebugData.Add("File name", fileRecord.Name);

                    try
                    {
                        string expandedValue = expandTextHelper.ExpandText((string)entry.Value, fileRecord, fileActionManagerPathTags, fileProcessingDB);

                        GdPictureUtility.ThrowIfStatusNotOK(gdPicturePdf.SetFormFieldValue(fieldID, expandedValue),
                        "ELI54258", "Error setting form field value", debugData);
                    }
                    catch (ExtractException extractException)
                    {
                        extractException.AddDebugData("FormFieldID", fieldID.ToString());
                        extractException.AddDebugData("FormFieldValue", (string)entry.Value);
                        extractException.AddDebugData("FormFieldToFill", entry.Key.ToString());
                        debugData.AddDebugData("File name", fileRecord.Name);
                        extractException.Log();
                    }
                }
            }
        }

        /// <summary>
        /// A method to get all of the form field values.
        /// </summary>
        /// <param name="gdPicturePdf">A gdpicture instance with the pdf already loaded.</param>
        /// <param name="filePath">The file path the document was loaded from.</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static Dictionary<string, int> GetFormFieldValues(GdPicturePDF gdPicturePdf, string filePath)
        {
            Dictionary<string, int> formFieldAndIDs = new();
            int FieldCount = gdPicturePdf.GetFormFieldsCount();
            for (int x = 0; x < FieldCount; x++)
            {
                int formFieldId = gdPicturePdf.GetFormFieldId(x);
                GdPictureStatus status = gdPicturePdf.GetStat();
                if (status == GdPictureStatus.OK)
                {
                    string formFieldTitle = gdPicturePdf.GetFormFieldTitle(formFieldId);
                    status = gdPicturePdf.GetStat();
                    var formFieldType = (status == GdPictureStatus.OK)
                        ? gdPicturePdf.GetFormFieldType(formFieldId)
                        : PdfFormFieldType.PdfFormFieldTypeUnknown;

                    if (status == GdPictureStatus.OK && !string.IsNullOrEmpty(formFieldTitle))
                    {
                        if (formFieldAndIDs.ContainsKey(formFieldTitle))
                        {
                            var extractException = new ExtractException("ELI54259", $"Duplicate form field title found: {formFieldTitle}");
                            extractException.AddDebugData("Filename", filePath);
                            extractException.Log();
                        }
                        else if (formFieldType != PdfFormFieldType.PdfFormFieldTypeText)
                        {
                            var extractException = new ExtractException("ELI54299", $"This field is not a text box, and therefore not supported: {formFieldTitle}");
                            extractException.AddDebugData("Filename", filePath);
                            extractException.Log();
                        }
                        else
                        {
                            formFieldAndIDs.Add(formFieldTitle, formFieldId);
                        }
                    }
                    else
                    {
                        var extractException = new ExtractException("ELI54257", $"Unable to load form field title for field id {formFieldId}");
                        extractException.AddDebugData("Filename", filePath);
                        extractException.Log();
                    }
                }
            }

            return formFieldAndIDs;
        }

        private void SavePDF(GdPicturePDF pdfAPI, string fileName)
        {
            // You cannot save to the same file name because GDPicture keeps this file open.
            GdPictureUtility.ThrowIfStatusNotOK(pdfAPI.SaveToFile(fileName + ".filled.pdf"),
            "ELI54273", "The PDF document can't be saved", new(filePath: fileName));

            pdfAPI.Dispose();

            // Delete the original file, and rename the filled in one back to the original.
            FileSystemMethods.DeleteFile(fileName);

            File.Move(fileName + ".filled.pdf", fileName);
        }
        #endregion
    }
}
