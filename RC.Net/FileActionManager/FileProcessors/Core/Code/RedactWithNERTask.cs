using Extract.AttributeFinder;
using Extract.AttributeFinder.Rules;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using opennlp.tools.namefind;
using opennlp.tools.tokenize;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface definition for the <see cref="RedactWithNERTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("7D63D759-9AE0-4507-A655-4CECEA833CE3")]
    [CLSCompliant(false)]
    public interface IRedactWithNERTask :
        ICategorizedComponent,
        IConfigurableObject,
        IMustBeConfiguredObject,
        ICopyableObject,
        IFileProcessingTask,
        ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// Path to a trained NER model, can contain path tags/functions
        /// </summary>
        string NERModelPath { get; set; }

        /// <summary>
        /// Path to the redacted image to output, can contain path tags/functions
        /// </summary>
        string OutputImagePath { get; set; }

        /// <summary>
        /// Optional path to a VOA file to output, can contain path tags/functions
        /// </summary>
        string OutputVOAPath { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> that OCRs, searches and redacts using a NER model
    /// </summary>
    [ComVisible(true)]
    [ProgId("Extract.FileActionManager.FileProcessors.RedactWithNERTask")]
    [Guid("F54CAC6D-6BD8-40FF-B16B-ADEE7BA71C79")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class RedactWithNERTask : IRedactWithNERTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Redaction: Redact with NER";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FileActionManagerObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Path to a trained NER model, can contain path tags/functions
        /// </summary>
        string _nerModelPath;

        /// <summary>
        /// Path to the redacted image to output, can contain path tags/functions
        /// </summary>
        string _outputImagePath = "$InsertBeforeExt(<SourceDocName>,.redacted)";

        /// <summary>
        /// Optional path to a VOA file to output, can contain path tags/functions
        /// </summary>
        string _outputVOAPath = "$InsertBeforeExt(<SourceDocName>,.redacted).voa";

        string _ocrParametersFile;
        IOCRParameters _ocrParameters;

        static bool _recInitialized;
        static readonly object _initLock = new object();

        CancellationTokenSource _cancellationTokenSource;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactWithNERTask"/> class.
        /// </summary>
        public RedactWithNERTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactWithNERTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="RedactWithNERTask"/> from which settings should
        /// be copied.</param>
        public RedactWithNERTask(RedactWithNERTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46507");
            }
        }

        #endregion Constructors

        #region IRedactWithNERTask Members

        /// <summary>
        /// Path to a trained NER model, can contain path tags/functions
        /// </summary>
        public string NERModelPath { get => _nerModelPath; set => _nerModelPath = value; }

        /// <summary>
        /// Path to the redacted image to output, can contain path tags/functions
        /// </summary>
        public string OutputImagePath { get => _outputImagePath; set => _outputImagePath = value; }

        /// <summary>
        /// Optional path to a VOA file to output, can contain path tags/functions
        /// </summary>
        public string OutputVOAPath { get => _outputVOAPath; set => _outputVOAPath = value; }

        #endregion IRedactWithNERTask Members

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
        /// Performs configuration needed to create a valid <see cref="RedactWithNERTask"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46508", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (RedactWithNERTask)Clone();

                using (var dialog = new RedactWithNERTaskSettingsDialog(cloneOfThis))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46509",
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
                throw ex.CreateComVisible("ELI46510", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="RedactWithNERTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="RedactWithNERTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new RedactWithNERTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46511", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="RedactWithNERTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (!(pObject is RedactWithNERTask task))
                {
                    throw new InvalidCastException("Invalid copy-from object. Requires RedactWithNERTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46512", "Unable to copy object.", ex);
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
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch { }
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
                if (!_recInitialized)
                {
                    lock(_initLock)
                    {
                        if (!_recInitialized)
                        {
                            try
                            {
                                var rc = RecAPI.kRecSetLicense(null, "9d478fe171d5");
                                RecAssert("ELI46524", "Unable to license RecAPI", rc);

                                rc = RecAPIPlus.RecInitPlus("Extract Systems", "RedactWithNERTask");
                                RecAssert("ELI46525", "Unable to initialize RecAPIPlus", rc);

                                rc = RecAPI.kRecSettingGetHandle("Kernel.OcrMgr.PDF.RecognitionMode", out IntPtr hSetting);
                                RecAssert("ELI46547", "Unable to get OCR setting", rc);
                                rc = RecAPI.kRecSettingSetInt(0, hSetting, (int)PDF_REC_MODE.PDF_RM_MOSTLYGETTEXT);

                                rc = RecAPI.kRecSettingGetHandle("Kernel.OcrMgr.PDF.ProcessingMode", out hSetting);
                                RecAssert("ELI46549", "Unable to get OCR setting", rc);
                                rc = RecAPI.kRecSettingSetInt(0, hSetting, 3); // PDF_PROC_MODE.PDF_PM_TEXT_ONLY, which isn't part of the c# API

                                //rc = RecAPIPlus.RecSetOutputFormat(0, "PDFImageOnText");
                                rc = RecAPIPlus.RecSetOutputFormat(0, "PDF");
                                RecAssert("ELI46540", "Unable to set output format", rc);

                                _recInitialized = true;
                            }
                            catch (Exception ex)
                            {
                                ex.ExtractLog("ELI46541");
                            }
                        }
                    }
                }

                // Load the OCR parameters from the file
                if (!string.IsNullOrEmpty(_ocrParametersFile))
                {
                    ILoadOCRParameters loadOCRParameters = new RuleSetClass();
                    loadOCRParameters.LoadOCRParameters(_ocrParametersFile);
                    _ocrParameters = ((IHasOCRParameters)loadOCRParameters).OCRParameters;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46513",
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
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
                                                  int nActionID,
                                                  FAMTagManager pFAMTM,
                                                  FileProcessingDB pDB,
                                                  ProgressStatus pProgressStatus,
                                                  bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI46514", _COMPONENT_DESCRIPTION);

                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFAMTM, pFileRecord.Name);

                var inputImagePath = pathTags.Expand(pFileRecord.Name);
                var modelPath = pathTags.Expand(_nerModelPath);
                var outputImagePath = pathTags.Expand(_outputImagePath);
                var outputVOAPath = string.IsNullOrWhiteSpace(_outputVOAPath) ? null : pathTags.Expand(_outputVOAPath);

                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    RedactFile(inputImagePath, modelPath, outputImagePath, outputVOAPath, pProgressStatus);
                }

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (OperationCanceledException)
            {
                return EFileProcessingResult.kProcessingCancelled;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46515", "Error redacting with NER");
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
                throw ExtractException.CreateComVisible("ELI46516",
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
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _nerModelPath = reader.ReadString();
                    _outputImagePath = reader.ReadString();

                    if (reader.ReadBoolean())
                    {
                        _outputVOAPath = reader.ReadString();
                    }
                    else
                    {
                        _outputVOAPath = null;
                    }
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI46517",
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
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(_nerModelPath);
                    writer.Write(_outputImagePath);

                    bool hasVoaPath = !string.IsNullOrWhiteSpace(_outputVOAPath);
                    writer.Write(hasVoaPath);
                    if (hasVoaPath)
                    {
                        writer.Write(_outputVOAPath);
                    }

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
                throw ExtractException.CreateComVisible("ELI46518",
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
        /// Copies the specified <see cref="RedactWithNERTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="RedactWithNERTask"/> from which to copy.</param>
        void CopyFrom(RedactWithNERTask task)
        {
            _nerModelPath = task.NERModelPath;
            _outputImagePath = task.OutputImagePath;
            _outputVOAPath = task.OutputVOAPath;

            _ocrParametersFile = null;

            _dirty = true;
        }

        void RedactFile(string inputImagePath, string modelPath, string outputImagePath, string outputVOAPath, ProgressStatus progressStatus)
        {
            RECERR rc;
            IntPtr hDoc = IntPtr.Zero;
            IntPtr hFile = IntPtr.Zero;
            IUnknownVectorClass outputAttributes = outputVOAPath == null ? null : new IUnknownVectorClass();

            string ext = Path.GetExtension(outputImagePath).ToUpperInvariant();
            ExtractException.Assert("ELI46539", "Output must be a PDF file", ext == ".PDF");
            try
            {
                rc = RecAPI.kRecOpenImgFile(inputImagePath, out hFile, FILEOPENMODE.IMGF_READ, IMF_FORMAT.FF_SIZE);
                RecAssert("ELI46543", "Unable to create OCR document", rc);

                rc = RecAPI.kRecGetImgFilePageCount(hFile, out int nPageCount);
                RecAssert("ELI46527", "Unable to get page count", rc);

                progressStatus?.InitProgressStatus("Initializing redact with NER...", 0, nPageCount * 3, true);

                rc = RecAPIPlus.RecCreateDoc(0, "", out hDoc, DOCOPENMODE.DOC_NORMAL);
                RecAssert("ELI46523", "Unable to create OCR document", rc);

                var tokenizer = WhitespaceTokenizer.INSTANCE;
                var model = NERFinder.GetModel(modelPath, strm => new TokenNameFinderModel(strm));
                var nameFinder = new NameFinderME(model);

                for (int i = 0; i < nPageCount; ++i)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    progressStatus?.StartNextItemGroup(UtilityMethods.FormatCurrent($"OCRing page {i + 1}"), 1);

                    IntPtr hPage = LoadPageFromImageHandle(inputImagePath, hFile, i);

                    RecAPI.kRecGetImgInfo(0, hPage, IMAGEINDEX.II_CURRENT, out IMG_INFO pImg);
                    var pageInfo = new SpatialPageInfoClass();
                    pageInfo.Initialize(pImg.Size.cx, pImg.Size.cy, EOrientation.kRotNone, 0);
                    var pageInfoMap = new LongToObjectMapClass();
                    pageInfoMap.Set(i + 1, pageInfo);

                    rc = RecAPI.kRecLocateZones(0, hPage);
                    RecAssert("ELI00000", "Unable to locate zones", rc);
                    rc = RecAPI.kRecGetOCRZoneCount(hPage, out int zoneCount);
                    RecAssert("ELI00000", "Unable to get zone count", rc);
                    var newZones = new List<ZONE>();
                    bool updated = false;

                    // 'remove' check images
                    for (int zi = 0; zi < zoneCount; ++zi)
                    {
                        rc = RecAPI.kRecGetOCRZoneInfo(hPage, IMAGEINDEX.II_CURRENT, out ZONE pZone, zi);
                        RecAssert("ELI00000", "Unable to get zone info", rc);

                        if (pZone.type == ZONETYPE.WT_GRAPHIC)
                        {
                            var bounds = pZone.rectBBox;
                            var height = bounds.bottom - bounds.top;
                            var width = bounds.right - bounds.left;
                            if (width > height * 2 && width > pImg.Size.cx / 3)
                            {
                                //pZone.fm = FILLINGMETHOD.FM_MICR;
                                //pZone.rm = RECOGNITIONMODULE.RM_MAT;
                                //pZone.type = ZONETYPE.WT_FORM;;
                                pZone.type = ZONETYPE.WT_FLOW; ;
                                updated = true;

                                if (outputAttributes != null)
                                {
                                    string attributeName = "MCData";
                                    string attributeType = "AccountNumber";

                                    rc = RecAPI.kRecGetOCRZoneLayout(hPage, IMAGEINDEX.II_CURRENT, out RECT[] rects, zi);
                                    RecAssert("ELI00000", "Unable to get zone layout", rc);
                                    var spatialString = ZoneToSpatialString(rects, " ", inputImagePath, i + 1, pageInfoMap);

                                    var attribute = new AttributeClass
                                    {
                                        Name = attributeName,
                                        Type = attributeType,
                                        Value = spatialString
                                    };
                                    outputAttributes.PushBack(attribute);
                                }
                            }
                        }
                        newZones.Add(pZone);
                    }
                    if (updated)
                    {
                        foreach(ZONE zone in newZones)
                        {
                            rc = RecAPI.kRecInsertZone(hPage, IMAGEINDEX.II_CURRENT, zone, -1);
                            RecAssert("ELI00000", "Unable to insert zone", rc);
                        }
                        //rc = RecAPI.kRecLocateZones(0, hPage);
                        //RecAssert("ELI00000", "Unable to locate zones", rc);
                    }
                    rc = RecAPI.kRecRecognize(0, hPage);
                    if (rc != RECERR.REC_OK)
                    {
                        ExtractException ue;
                        if (rc == RECERR.NO_TXT_WARN)
                        {
                            ue = new ExtractException("ELI46528", "Application trace: No recognized text on page");
                        }
                        else
                        {
                            ue = new ExtractException("ELI46551", "Unable to OCR page");
                        }
                        ue.AddDebugData("Image File", inputImagePath);
                        ue.AddDebugData("Image Page", i + 1);
                        ue.Log();
                    }

                    progressStatus?.CompleteCurrentItemGroup();

                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    progressStatus?.StartNextItemGroup(UtilityMethods.FormatCurrent($"Searching page {i + 1}"), 1);

                    RecAPIPlus.RecInsertPage(0, hDoc, hPage, -1);
                    RecAssert("ELI46544", "Unable to insert page", rc, rc == RECERR.REC_OK || rc == RECERR.NO_TXT_WARN);

                    LETTER[] ppLetter = null;
                    IntPtr hFt = IntPtr.Zero;
                    try
                    {
                        rc = RecAPI.kRecGetLetters(hPage, IMAGEINDEX.II_CURRENT, out ppLetter);
                        RecAssert("ELI46529", "Unable to get page letters", rc, rc == RECERR.REC_OK || rc == RECERR.NO_TXT_WARN);

                        if (rc == RECERR.NO_TXT_WARN)
                        {
                            continue;
                        }

                        var chars = new char[ppLetter.Length];
                        for (int letteri = 0; letteri < chars.Length; ++letteri)
                        {
                            //chars[letteri] = ConvertToCodePage(ppLetter[letteri].code);
                            chars[letteri] = ppLetter[letteri].code;
                        }
                        var pageText = new string(chars);

                        string[] tokens = tokenizer.tokenize(pageText);
                        opennlp.tools.util.Span[] tokenPositions = tokenizer.tokenizePos(pageText);
                        opennlp.tools.util.Span[] nameSpans = nameFinder.find(tokens);

                        var termsToNameTypePair = new Dictionary<string, (string name, string type)>();
                        foreach (var span in nameSpans)
                        {
                            // Find char offsets (end indices are exclusive)
                            int start = tokenPositions[span.getStart()].getStart();
                            int end = tokenPositions[span.getEnd() - 1].getEnd();
                            int length = end - start;
                            string text = pageText.Substring(start, length);
                            termsToNameTypePair[text] = ("HCData", span.getType());
                        }
                        if (termsToNameTypePair.Count > 0)
                        {
                            var searchTerms = termsToNameTypePair.Keys.Concat(new string[] { null }).ToArray();
                            FoundText ft = null;
                            for (int tries = 0; tries < 5; ++tries)
                            {
                                try
                                {
                                    rc = RecAPIPlus.RecFindTextFirst(hDoc, i, 0, searchTerms, FindTextFlags.FT_MATCHCASE | FindTextFlags.FT_WHOLEWORD, 0, out hFt, out ft);
                                    //rc = RecAPIPlus.RecFindTextFirst(hDoc, i, 0, searchTerms, 0, 0, out hFt, out FoundText ft);
                                    RecAssert("ELI46536", "Unable to initialize search", rc, rc == RECERR.REC_OK || rc == RECERR.APIP_NOMORE_WARN);

                                    break;
                                }
                                catch (ExtractException ex)
                                {
                                    if (rc != RECERR.L_ERROR_FILE || tries == 4)
                                    {
                                        throw;
                                    }

                                    new ExtractException("ELI00000", UtilityMethods.FormatCurrent($"Application trace: search failure #{tries + 1}. Retrying..."), ex).Log();
                                }
                            }

                            while (rc != RECERR.APIP_NOMORE_WARN)
                            {
                                if (outputAttributes != null)
                                {
                                    bool hasNameAndType = termsToNameTypePair.TryGetValue(ft.letters, out var nameAndType);
                                    if (!hasNameAndType)
                                    {
                                        nameAndType = ("MCData", "");
                                    }
                                    outputAttributes.PushBack(MakeAttribute(ft, nameAndType.name, nameAndType.type, inputImagePath, pageInfoMap, pImg.DPI));
                                }
                                for (int tries = 0; tries < 5; ++tries)
                                {
                                    try
                                    {
                                        rc = RecAPIPlus.RecProcessText(hFt, ft, FindTextAction.FT_MARKFORREDACT, true);
                                        RecAssert("ELI46535", "Unable to mark item for redaction", rc);

                                        break;
                                    }
                                    catch (ExtractException ex)
                                    {
                                        if (rc != RECERR.L_ERROR_FILE || tries == 4)
                                        {
                                            throw;
                                        }

                                        new ExtractException("ELI00000", UtilityMethods.FormatCurrent($"Application trace: mark text failure #{tries + 1}. Retrying..."), ex).Log();
                                    }
                                }

                                rc = RecAPIPlus.RecFindTextNext(hFt, out ft);
                                RecAssert("ELI46545", "Unable to find next", rc, rc == RECERR.REC_OK || rc == RECERR.APIP_NOMORE_WARN);
                            }
                        }
                    }
                    finally
                    {
                        if (hFt != IntPtr.Zero)
                        {
                            try
                            {
                                rc = RecAPIPlus.RecFindTextClose(hFt);
                                RecAssert("ELI46533", "Unable to close find text", rc);
                            }
                            catch (Exception ex)
                            {
                                ex.ExtractLog("ELI46534");
                            }
                        }
                        progressStatus?.CompleteCurrentItemGroup();
                    }
                }

                _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                outputAttributes?.SaveAttributes(outputVOAPath);

                progressStatus?.StartNextItemGroup("Redacting document...", nPageCount);
                rc = RecAPIPlus.RecExecuteRedaction(hDoc);
                RecAssert("ELI46537", "Unable to redact document", rc);

                rc = RecAPIPlus.RecConvert2Doc(0, hDoc, outputImagePath);
                RecAssert("ELI46538", "Unable to output redacted document", rc);

                progressStatus?.CompleteCurrentItemGroup();
            }
            finally
            {
                if (hDoc != IntPtr.Zero)
                {
                    rc = RecAPIPlus.RecCloseDoc(0, hDoc);
                    RecAssert("ELI46526", "Unable to close OCR document", rc);
                }
                if (hFile != IntPtr.Zero)
                {
                    rc = RecAPI.kRecCloseImgFile(hFile);
                    RecAssert("ELI46546", "Unable to close image", rc);
                }
            }
        }

        private void RecAssert(string eliCode, string message, RECERR rc)
        {
            if (rc != RECERR.REC_OK)
            {
                var ex = new ExtractException(eliCode, message);
                LoadScansoftRecErrInfo(ex, rc);
                throw ex;
            }
        }

        private void RecAssert(string eliCode, string message, RECERR rc, bool condition)
        {
            if (!condition)
            {
                var ex = new ExtractException(eliCode, message);
                LoadScansoftRecErrInfo(ex, rc);
                throw ex;
            }
        }

        IAttribute MakeAttribute(FoundText ft, string name, string type, string sourceDocName, LongToObjectMap pageInfoMap, SIZE DPI)
        {
            int fromTwip(int x)
            {
                // TODO: Check assumption that image should be considered to be 300dpi
                return x * DPI.cx / 1440;
            }
            bool between(int x, int min, int max)
            {
                return x > min && x < max;
            }

            var attribute = new AttributeClass();
            var zones = new IUnknownVectorClass();
            for (int i = 0; i < ft.nLetters; ++i)
            {
                RECT sourceRect = ft.boundBoxes[i];
                int sourceLeft = fromTwip(sourceRect.left);
                int sourceRight = fromTwip(sourceRect.right);
                int sourceTop = fromTwip(sourceRect.top);
                int sourceBottom = fromTwip(sourceRect.bottom);
                for (int j = i + 1; j < ft.nLetters; ++j)
                {
                    int sourceHeight = sourceBottom - sourceTop;
                    int sourceCenter = sourceTop + sourceHeight / 2;

                    RECT nextRect = ft.boundBoxes[j];
                    int nextLeft = fromTwip(nextRect.left);
                    int nextRight = fromTwip(nextRect.right);
                    int nextTop = fromTwip(nextRect.top);
                    int nextBottom = fromTwip(nextRect.bottom);

                    int nextHeight = nextBottom - nextTop;
                    int nextCenter = nextTop + nextHeight / 2;

                    // If the next char appears to be part of the same horizontal line
                    // then combine them into one zone
                    if (between(nextCenter, sourceTop, sourceBottom)
                        || between(sourceCenter, nextTop, nextBottom))
                    {
                        sourceTop = Math.Min(sourceTop, nextTop);
                        sourceBottom = Math.Max(sourceBottom, nextBottom);
                        sourceLeft = Math.Min(sourceLeft, nextLeft);
                        sourceRight = Math.Max(sourceRight, nextRight);
                        i = j;
                    }
                    else
                    {
                        break;
                    }
                }

                // If the next char is on a different line or this is the last char
                // then create the zone
                var rect = new LongRectangleClass();
                rect.SetBounds(sourceLeft, sourceTop, sourceRight, sourceBottom);
                var zone = new RasterZoneClass();
                zone.CreateFromLongRectangle(rect, ft.page + 1);
                zones.PushBack(zone);
            }
            
            attribute.Value.CreateHybridString(zones, ft.letters, sourceDocName, pageInfoMap);
            attribute.Name = name;
            attribute.Type = type;
            return attribute;
        }
        IntPtr LoadPageFromImageHandle(string strImage, IntPtr hImage, int iPageIndex)
        {
            IntPtr phPage = IntPtr.Zero;
            RECERR rc = RecAPI.kRecLoadImg(0, hImage, out phPage, iPageIndex);
            if (rc != RECERR.REC_OK && rc != RECERR.IMF_PASSWORD_WARN && rc != RECERR.IMG_NOMORE_WARN && rc != RECERR.IMF_READ_WARN &&
                rc != RECERR.IMF_COMP_WARN)
            {
                // Determine whether this page was able to be loaded despite errors in the document
                bool bFail = phPage == IntPtr.Zero;

                // Create a scary or friendly exception based on whether page was loaded
                string strEli;
                string strMessage;
                if (bFail)
                {
                    strEli = "ELI05773";
                    strMessage = "Unable to load image file in the OCR engine.";
                }
                else
                {
                    strEli = "ELI29877";
                    strMessage = "Application trace: Image loaded successfully but contains errors.";
                }

                // Create the exception
                var ue = new ExtractException(strEli, strMessage);
                LoadScansoftRecErrInfo(ue, rc);
                ue.AddDebugData("Image File", strImage, false);
                ue.AddDebugData("Page Number", iPageIndex + 1, false);

                // Throw or log the exception
                if (bFail)
                {
                    throw ue;
                }
                else
                {
                    ue.Log();
                }
            }
            return phPage;
        }

        void LoadScansoftRecErrInfo(ExtractException ue, RECERR rc)
        {
            int lExtendedErrorCode = 0;
            string pszExtendedErrorDescription = "";

            // get the extended error code information from the last error
            RecAPI.kRecGetLastError(out lExtendedErrorCode, out pszExtendedErrorDescription);

            string pszSymbolicErrorName;

            // get the symbolic name of the error
            RecAPI.kRecGetErrorInfo(rc, out pszSymbolicErrorName);

            // get the error description
            RecAPI.kRecGetErrorUIText(rc, lExtendedErrorCode, pszExtendedErrorDescription, out string errUIText);

            // add the debug info
            ue.AddDebugData("Error description", errUIText, false);
            ue.AddDebugData("Error code", pszSymbolicErrorName, false);
            
            // add extended debug information if it is available
            if (lExtendedErrorCode != 0)
            {
                ue.AddDebugData("Extended error description", pszExtendedErrorDescription, false);
                ue.AddDebugData("Extended error code", lExtendedErrorCode, false);
            }
        }
        static SpatialString ZoneToSpatialString(RECT[] rects, string value, string imagePath, int pageNum, LongToObjectMap pageInfoMap)
        {
            var zones = rects.Select(sourceRect =>
            {
                var rect = new LongRectangleClass();
                rect.SetBounds(sourceRect.left, sourceRect.top, sourceRect.right, sourceRect.bottom);
                var zone = new RasterZoneClass();
                zone.CreateFromLongRectangle(rect, pageNum);
                return zone;
            })
            .ToIUnknownVector();

            var spatialString = new SpatialStringClass();

            // Template creator/finder needs to handle empty form field names
            // https://extract.atlassian.net/browse/ISSUE-14918
            if (string.IsNullOrEmpty(value))
            {
                value = " ";
            }

            if (zones.Size() == 1)
            {
                spatialString.CreatePseudoSpatialString((RasterZone)zones.At(0), value, imagePath, pageInfoMap);
            }
            else
            {
                spatialString.CreateHybridString(zones, value, imagePath, pageInfoMap);
            }

            return spatialString;
        }
        bool IsBasicLatinCharacter(char letterCode)
        {
            // NOTE: 176 is the degree symbol.
            // Don't allow 0 as a letter code
            return letterCode > 0 && letterCode <= 126 || letterCode == 176;
        }

        char ConvertToCodePage(char letterCode)
        {
            if (IsBasicLatinCharacter(letterCode))
            {
                return letterCode;
            }

            RECERR rc = RecAPI.kRecConvertUnicode2CodePage(0, letterCode, out byte[] pExport);
            if (rc != RECERR.REC_OK)
            {
                return '^';
            }
            else
            {
                return (char)pExport[0];
            }
        }
        #endregion Private Members
    }
}
