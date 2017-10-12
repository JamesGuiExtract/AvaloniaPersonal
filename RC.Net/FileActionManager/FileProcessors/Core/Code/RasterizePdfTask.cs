using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface definition for the <see cref="RasterizePdfTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("56B6BA0C-B8E6-41A5-A7C3-931DEE37472B")]
    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rasterize")]
    public interface IRasterizePdfTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask, ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// Gets or sets the PDF file.
        /// </summary>
        /// <value>
        /// The PDF file.
        /// </value>
        string PdfFile { get; set; }

        /// <summary>
        /// Gets or sets the destination file.
        /// </summary>
        /// <value>
        /// The destination file.
        /// </value>
        string DestinationFile { get; set; }

        /// <summary>
        /// Gets or sets whether the input PDF file should be deleted after conversion.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the input PDF file will be deleted after conversion.
        /// </value>
        bool DeletePdfFile { get; set; }

        /// <summary>
        /// Gets or sets whether the task should fail if deleting the PDF file fails.
        /// <para>Note:</para>
        /// This only applies if <see cref="DeletePdfFile"/> is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the task will fail if deleting the PDF file fails.
        /// </value>
        bool FailTaskIfDeleteFails { get; set; }

        /// <summary>
        /// Gets or sets whether the the SourceDocName should be modified in the database
        /// after successful rasterization.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the the SourceDocName will be modified to point to
        /// the converted file.
        /// </value>
        bool ChangeSourceDocName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the alternate method of rasterization.
        /// </summary>
        /// <value><see langword="true"/> to use the alternate method of rasterization; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool UseAlternateMethod { get; set; }

        /// <summary>
        /// Gets or sets whether the color depth of the input document should be preserved in the
        /// output.
        /// </summary>
        /// <value><see langword="true"/> if the output should match the color depth of the source;
        /// <see langword="false"/> if the output should be a bitonal tif.
        /// </value>
        bool PreserveColorDepth { get; set; }
    }

    /// <summary>
    /// Represents a file processing task that will rasterize a PDF.
    /// </summary>
    [ComVisible(true)]
    [Guid("C14287E9-9531-4DF5-87DD-CB64298442D8")]
    [ProgId("Extract.FileActionManager.FileProcessors.RasterizePdfTask")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rasterize")]
    public class RasterizePdfTask : IRasterizePdfTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Rasterize PDF";

        /// <summary>
        /// Current task version.
        /// <para><b>Version 2</b></para>
        /// Added UseAlternateMethod.
        /// <para><b>Version 3</b></para>
        /// Alternate method now = Nuance
        /// <para><b>Version 4</b></para>
        /// PreserveColorDepth
        /// </summary>
        const int _CURRENT_VERSION = 4;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The PDF file to to rasterize
        /// </summary>
        string _pdfFile;

        /// <summary>
        /// The destination for the converted file.
        /// </summary>
        string _destinationFile;

        /// <summary>
        /// Whether or not the PDF file should be deleted after conversion.
        /// </summary>
        bool _deletePdf;

        /// <summary>
        /// Whether or not the task should fail if the deletion fails.
        /// </summary>
        bool _failTask = true;

        /// <summary>
        /// Whether or not the source doc name should be modified in the database.
        /// </summary>
        bool _changeSourceDocName;

        /// <summary>
        /// Indicates whether to use the alternate method of rasterization.
        /// Nuance produces better results on color PDFs and does not suffer from
        /// [DotNetRCAndUtils:742], so it is the default.
        /// </summary>
        bool _useAlternateMethod = true;

        /// <summary>
        /// https://extract.atlassian.net/browse/ISSUE-12467
        /// Indicates whether the color depth of the input document should be preserved in the
        /// output. Otherwise the output will be a bitonal tif.
        /// </summary>
        bool _preserveColorDepth = true;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The license ids to validate in licensing calls
        /// </summary>
        static readonly LicenseIdName _licenseId1 = LicenseIdName.PdfReadWriteFeature;
        static readonly LicenseIdName _licenseId2 = LicenseIdName.OcrOnClientFeature;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizePdfTask"/> class.
        /// </summary>
        public RasterizePdfTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizePdfTask"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        public RasterizePdfTask(RasterizePdfTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32224");
            }
        }

        #endregion Constructor

        #region IRasterizePdfTask Members

        /// <summary>
        /// Gets or sets the PDF file.
        /// </summary>
        /// <value>
        /// The PDF file.
        /// </value>
        public string PdfFile
        {
            get
            {
                return _pdfFile;
            }
            set
            {
                try
                {
                    _dirty |= _pdfFile == null
                        || !_pdfFile.Equals(value, StringComparison.OrdinalIgnoreCase);
                    _pdfFile = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32281", "Unable to set PDF file.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the destination file.
        /// </summary>
        /// <value>
        /// The destination file.
        /// </value>
        public string DestinationFile
        {
            get
            {
                return _destinationFile;
            }
            set
            {
                try
                {
                    _dirty |= _destinationFile == null
                        || !_destinationFile.Equals(value, StringComparison.OrdinalIgnoreCase);
                    _destinationFile = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32282", "Unable to set destination file.");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the input PDF file should be deleted after conversion.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the input PDF file will be deleted after conversion.
        /// </value>
        public bool DeletePdfFile
        {
            get
            {
                return _deletePdf;
            }
            set
            {
                _dirty |= _deletePdf != value;
                _deletePdf = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the task should fail if deleting the PDF file fails.
        /// <para>Note:</para>
        /// This only applies if <see cref="DeletePdfFile"/> is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the task will fail if deleting the PDF file fails.
        /// </value>
        public bool FailTaskIfDeleteFails
        {
            get
            {
                return _failTask;
            }
            set
            {
                _dirty |= _failTask != value;
                _failTask = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the the SourceDocName should be modified in the database
        /// after successful rasterization.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> then the SourceDocName will be modified to point to
        /// the converted file.
        /// </value>
        public bool ChangeSourceDocName
        {
            get
            {
                return _changeSourceDocName;
            }
            set
            {
                try
                {
                    if (value && !_pdfFile.Equals(FileActionManagerPathTags.SourceDocumentTag))
                    {
                        throw new ExtractException("ELI32245",
                            "Cannot modify SourceDocName if the input file is not SourceDocName.");
                    }
                    _dirty |= _changeSourceDocName != value;
                    _changeSourceDocName = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32246",
                        "Unable to set ChangeSourceDocName property.");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the alternate method of rasterization.
        /// </summary>
        /// <value><see langword="true"/> to use the alternate method of rasterization; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool UseAlternateMethod
        {
            get
            {
                return _useAlternateMethod;
            }

            set
            {
                _dirty |= _useAlternateMethod != value;
                _useAlternateMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the color depth of the input document should be preserved in the
        /// output.
        /// </summary>
        /// <value><see langword="true"/> if the output should match the color depth of the source;
        /// <see langword="false"/> if the output should be a bitonal tif.
        /// </value>
        public bool PreserveColorDepth
        {
            get
            {
                return _preserveColorDepth;
            }

            set
            {
                _dirty |= _preserveColorDepth != value;
                _preserveColorDepth = value;
            }
        }

        #endregion IRasterizePdfTask Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="RasterizePdfTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (var dialog = new RasterizePdfTaskSettingsDialog(
                    PdfFile, DestinationFile, DeletePdfFile, FailTaskIfDeleteFails,
                    ChangeSourceDocName, UseAlternateMethod, PreserveColorDepth))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Use the set properties so that _dirty flag is updated properly
                        PdfFile = dialog.PdfFile;
                        DestinationFile = dialog.ConvertedFile;
                        DeletePdfFile = dialog.DeletePdf;
                        FailTaskIfDeleteFails = dialog.FailIfDeleteFails;
                        ChangeSourceDocName = dialog.ChangeSourceDocName;
                        UseAlternateMethod = dialog.UseAlternateMethod;
                        PreserveColorDepth = dialog.PreserveColorDepth;

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32225", "Error running configuration.");
            }
        }

        #endregion

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                // Configured if:
                // 1. PDF File is defined
                // 2. Conversion destination is defined
                // 3. Either not changing source doc name OR the input file is source doc name 
                return !string.IsNullOrWhiteSpace(PdfFile)
                    && !string.IsNullOrWhiteSpace(DestinationFile)
                    && (!ChangeSourceDocName
                    || PdfFile.Equals(FileActionManagerPathTags.SourceDocumentTag, StringComparison.Ordinal));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32241", "Unable to check configuration.");
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="RasterizePdfTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="RasterizePdfTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                var task =
                    new RasterizePdfTask(this);

                return task;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32226", "Unable to clone object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="RasterizePdfTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as RasterizePdfTask;
                if (task == null)
                {
                    throw new InvalidCastException("Object is not a Rasterize PDF task.");
                }

                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32227", "Unable to copy object.");
            }
        }

        #endregion

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
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                LicenseUtilities.ValidateLicense(_licenseId1, "ELI32248", _COMPONENT_DESCRIPTION);
                LicenseUtilities.ValidateLicense(_licenseId2, "ELI34321", _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32247", "Unable to init task.");
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
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            string pdfFile = string.Empty;
            string destFile = string.Empty;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_licenseId1, "ELI32228", _COMPONENT_DESCRIPTION);
                LicenseUtilities.ValidateLicense(_licenseId2, "ELI34322", _COMPONENT_DESCRIPTION);
                
                // Name of the file being processed that will have it's name changed in the database
                var fileName = pFileRecord.Name;

                // Create a tag manager and expand the tags in the file name
                var tags = new FileActionManagerPathTags(pFAMTM, fileName);

                // Get the source and destination files
                pdfFile = Path.GetFullPath(tags.Expand(PdfFile));
                destFile = Path.GetFullPath(tags.Expand(DestinationFile));

                // Ensure the input file is a PDF
                if (!ImageMethods.IsPdf(pdfFile))
                {
                    var ee = new ExtractException("ELI32229", "Input image must be a PDF.");
                    ee.AddDebugData("Input File", pdfFile, false);
                    throw ee;
                }

                try
                {
                    ImageMethods.ConvertPdfToTif(
                        pdfFile, destFile, UseAlternateMethod, PreserveColorDepth);
                }
                catch (Exception ex)
                {
                    var ee = new ExtractException("ELI35267",
                        (UseAlternateMethod ? "Alternate" : "Normal") +
                        " rasterization method failed; attempting " +
                        (UseAlternateMethod ? "normal method." : "alternate method."), ex);
                    ee.Log();

                    ImageMethods.ConvertPdfToTif(
                        pdfFile, destFile, !UseAlternateMethod, PreserveColorDepth);
                }

                // Delete PDF if specified
                if (DeletePdfFile)
                {
                    try
                    {
                        FileSystemMethods.DeleteFile(pdfFile);
                    }
                    catch (Exception ex)
                    {
                        var ee = ex.AsExtract("ELI32243");
                        if (FailTaskIfDeleteFails)
                        {
                            throw ee;
                        }

                        ee.Log();
                    }
                }

                // Update the source doc name if specified (Do this as the last step so
                // that if there is an exception, the database will not be updated on the
                // task failure).
                if (ChangeSourceDocName)
                {
                    pDB.RenameFile(pFileRecord, destFile);
                }

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                // [DotNetRCAndUtils:766]
                // ImageMethods.ConvertPdfToTif will only write the output to the final destination
                // if the rasterization succeeded; no need to delete destFile here.

                var ee = ex.AsExtract("ELI32230");
                ee.AddDebugData("File ID", pFileRecord.FileID, false);
                ee.AddDebugData("Action ID", nActionID, false);
                ee.AddDebugData("PDF File", pdfFile, false);
                ee.AddDebugData("Converted File", destFile, false);

                // Throw the extract exception as a COM visible exception
                throw ee.CreateComVisible("ELI32231", "Unable to Rasterize the PDF.");
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
            // This task requires admin access if modifying the SourceDocName
            return ChangeSourceDocName;
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
            try
            {
                return LicenseUtilities.IsLicensed(_licenseId1) &&
                       LicenseUtilities.IsLicensed(_licenseId2);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32232", "Unable to determine license status.");
            }
        }

        #endregion

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
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _pdfFile = reader.ReadString();
                    _destinationFile = reader.ReadString();
                    _deletePdf = reader.ReadBoolean();
                    _failTask = reader.ReadBoolean();
                    _changeSourceDocName = reader.ReadBoolean();
                    if (reader.Version == 2)
                    {
                        // In version 2, alternate method was LeadTools. In version >= 3 Nuance is
                        // the alternate method.
                        _useAlternateMethod = !reader.ReadBoolean();
                    }
                    
                    if (reader.Version >= 3)
                    {
                        _useAlternateMethod = reader.ReadBoolean();
                    }

                    if (reader.Version >= 4)
                    {
                        _preserveColorDepth = reader.ReadBoolean();
                    }
                    else
                    {
                        // https://extract.atlassian.net/browse/ISSUE-12795
                        // The default is now true, but to ensure we are not modifying behavior of
                        // existing workflows, set to false for a legacy version.
                        _preserveColorDepth = false;
                    }
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32233", "Unable to load object from stream.");
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
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Serialize the settings
                    writer.Write(PdfFile);
                    writer.Write(DestinationFile);
                    writer.Write(DeletePdfFile);
                    writer.Write(FailTaskIfDeleteFails);
                    writer.Write(ChangeSourceDocName);
                    writer.Write(UseAlternateMethod);
                    writer.Write(PreserveColorDepth);

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
                throw ex.CreateComVisible("ELI32234", "Unable to save object to stream.");
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

        #endregion

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
        /// Copies the settings from the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        public void CopyFrom(RasterizePdfTask task)
        {
            try
            {
                _pdfFile = task._pdfFile;
                _destinationFile = task.DestinationFile;
                _deletePdf = task.DeletePdfFile;
                _failTask = task.FailTaskIfDeleteFails;
                _changeSourceDocName = task.ChangeSourceDocName;
                _useAlternateMethod = task.UseAlternateMethod;
                _preserveColorDepth = task.PreserveColorDepth;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32235");
            }
        }

        #endregion Methods
    }
}
