using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// The interface definition for <see cref="ExtractImageAreaTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("651E6D32-5181-49E8-92C7-4095DC3E09E5")]
    [CLSCompliant(false)]
    public interface IExtractImageAreaTask : IFileProcessingTask, ICategorizedComponent,
        IConfigurableObject, IMustBeConfiguredObject, ICopyableObject,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the name of the data file to use.
        /// </summary>
        /// <value>
        /// The name of the data file to use.
        /// </value>
        string DataFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute query specifying which attributes specify the areas to be
        /// extracted.
        /// </summary>
        /// <value>
        /// The the attribute query specifying which attributes specify the areas to be extracted.
        /// </value>
        string AttributeQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the overall bounds of the each attribute should be used.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to use the overall bounds of each attribute;
        /// <see langword="false"/> to use each raster zone of the attribute individually.
        /// </value>
        bool UseOverallBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether all qualifying areas should be extracted.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if all qualifying zones should be extracted;
        /// <see langword="false"/> if only the first qualifying zone should be extracted.
        /// </value>
        bool OutputAllAreas
        {
            get;
            set;
        }

        /// <summary>
        /// The name for the file to which the extracted image should be written.
        /// </summary>
        string OutputFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether new image areas can be appended as additional pages if the output
        /// file already exists.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if new image areas can be appended as additional pages if the
        /// output file already exists; otherwise, <see langword="false"/>.
        /// </value>
        bool AllowOutputAppend
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
    [Guid("6DBA4A58-7E3F-4A05-B67F-F386A14E7926")]
    [ProgId("Extract.FileActionManager.FileProcessors.ExtractImageAreaTask")]
    public class ExtractImageAreaTask : IExtractImageAreaTask, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Extract image area";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        /// <summary>
        /// A custom path tag to allow multiple images to be extracted from the same source image.
        /// </summary>
        internal const string _AREA_ID_TAG = "<AreaID>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Used to query attributes from the data file.
        /// </summary>
        AFUtility _afUtility;

        /// <summary>
        /// An <see cref="FileActionManagerPathTags"/> instance with the AreaID tag added to expand
        /// the output filename.
        /// </summary>
        FileActionManagerPathTags _outputPathTags;

        /// <summary>
        /// Used to create the <see cref="_imageReader"/>.
        /// </summary>
        ImageCodecs _codecs;

        /// <summary>
        /// Used to open image pages from the source file.
        /// </summary>
        ImageReader _imageReader;

        /// <summary>
        /// A cache of image pages from the currently processing file.
        /// </summary>
        Dictionary<int, RasterImage> _rasterImagePages = new Dictionary<int, RasterImage>();

        /// <summary>
        /// Indicates whether the output file has already been created by this task (and thus it is
        /// always acceptable to overwrite the file when generating the final output).
        /// </summary>
        bool _outputFileCreated;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractImageAreaTask"/> class.
        /// </summary>
        public ExtractImageAreaTask()
        {
            try
            {
                DataFileName = "<SourceDocName>.voa";
                AttributeQuery = "*";
                UseOverallBounds = true;
                OutputAllAreas = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33204");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractImageAreaTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="ExtractImageAreaTask"/> from which settings should
        /// be copied.</param>
        public ExtractImageAreaTask(ExtractImageAreaTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33186");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets an <see cref="FileActionManagerPathTags"/> instance with the AreaID tag added to
        /// expand <see cref="OutputFileName"/>.
        /// </summary>
        internal FileActionManagerPathTags OutputPathTags
        {
            get
            {
                try
                {
                    if (_outputPathTags == null)
                    {
                        _outputPathTags = new FileActionManagerPathTags();
                        _outputPathTags.AddDelayedExpansionTag(_AREA_ID_TAG, ExpandAreaIDTag);
                    }

                    return _outputPathTags;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33210");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the data file to use.
        /// </summary>
        /// <value>
        /// The name of the data file to use.
        /// </value>
        public string DataFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute query specifying which attributes specify the areas to be
        /// extracted.
        /// </summary>
        /// <value>
        /// The the attribute query specifying which attributes specify the areas to be extracted.
        /// </value>
        public string AttributeQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the overall bounds of the each attribute should be used.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to use the overall bounds of each attribute;
        /// <see langword="false"/> to use each raster zone of the attribute individually.
        /// </value>
        public bool UseOverallBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether all qualifying areas should be extracted.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if all qualifying zones should be extracted;
        /// <see langword="false"/> if only the first qualifying zone should be extracted.
        /// </value>
        public bool OutputAllAreas
        {
            get;
            set;
        }

        /// <summary>
        /// The name for the file to which the extracted image should be written.
        /// </summary>
        public string OutputFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether new image areas can be appended as additional pages if the output
        /// file already exists.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if new image areas can be appended as additional pages if the
        /// output file already exists; otherwise, <see langword="false"/>.
        /// </value>
        public bool AllowOutputAppend
        {
            get;
            set;
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
        /// Performs configuration needed to create a valid <see cref="ExtractImageAreaTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33207", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                ExtractImageAreaTask cloneOfThis = (ExtractImageAreaTask)Clone();

                using (ExtractImageAreaTaskSettingsDialog dlg
                    = new ExtractImageAreaTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI33187",
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
            return !string.IsNullOrWhiteSpace(DataFileName) &&
                   !string.IsNullOrWhiteSpace(OutputFileName);
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ExtractImageAreaTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ExtractImageAreaTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new ExtractImageAreaTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI33188", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ExtractImageAreaTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as ExtractImageAreaTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to " + this.GetType().ToString());
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI33189", "Unable to copy object.", ex);
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
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33190", _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33191",
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
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33192", _COMPONENT_DESCRIPTION);

                _outputFileCreated = false;

                string sourceDocName = pFileRecord.Name;
                _outputPathTags = new FileActionManagerPathTags(pFAMTM, sourceDocName);
                _outputPathTags.AddDelayedExpansionTag(_AREA_ID_TAG, ExpandAreaIDTag);

                // Initialize the path tag instances with sourceDocName.
                string dataFileName = pFAMTM.ExpandTagsAndFunctions(DataFileName, sourceDocName);

                using (var leadtoolsGuard = new LeadtoolsGuard())
                {
                    // Extract image areas.
                    foreach (RasterZone rasterZone in GetZonesToExtract(dataFileName))
                    {
                        OutputImageArea(sourceDocName, rasterZone);
                    }
                }
                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33193", "Unable to process the file.");
            }
            finally
            {
                if (_imageReader != null)
                {
                    _imageReader.Dispose();
                    _imageReader = null;
                }
                
                CollectionMethods.ClearAndDispose(_rasterImagePages);
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
                throw ExtractException.CreateComVisible("ELI33194",
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
                    DataFileName = reader.ReadString();
                    AttributeQuery = reader.ReadString();
                    UseOverallBounds = reader.ReadBoolean();
                    OutputAllAreas = reader.ReadBoolean();
                    OutputFileName = reader.ReadString();
                    AllowOutputAppend = reader.ReadBoolean();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI33195",
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
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(DataFileName);
                    writer.Write(AttributeQuery);
                    writer.Write(UseOverallBounds);
                    writer.Write(OutputAllAreas);
                    writer.Write(OutputFileName);
                    writer.Write(AllowOutputAppend);

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
                throw ExtractException.CreateComVisible("ELI33196",
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

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ExtractImageAreaTask"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ExtractImageAreaTask"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="ExtractImageAreaTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_imageReader != null)
                {
                    _imageReader.Dispose();
                    _imageReader = null;
                }

                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }

                CollectionMethods.ClearAndDispose(_rasterImagePages);
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members

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
        /// Copies the specified <see cref="ExtractImageAreaTask"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="ExtractImageAreaTask"/> from which to copy.
        /// </param>
        void CopyFrom(ExtractImageAreaTask source)
        {
            DataFileName = source.DataFileName;
            AttributeQuery = source.AttributeQuery;
            UseOverallBounds = source.UseOverallBounds;
            OutputAllAreas = source.OutputAllAreas;
            OutputFileName = source.OutputFileName;
            AllowOutputAppend = source.AllowOutputAppend;

            _dirty = true;
        }

        /// <summary>
        /// Gets an instance of <see cref="ImageCodecs"/> to use throughout the lifetime of this
        /// <see cref="ExtractImageAreaTask"/> instance.
        /// </summary>
        /// <returns>An instance of <see cref="ImageCodecs"/> to use throughout the lifetime of 
        /// this <see cref="ExtractImageAreaTask"/> instance.</returns>
        ImageCodecs Codecs
        {
            get
            {
                try
                {
                    if (_codecs == null)
                    {
                        _codecs = new ImageCodecs();
                    }

                    return _codecs;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI33206", ex);
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="AFUtility"/> instance to use to use throughout the lifetime of this
        /// <see cref="ExtractImageAreaTask"/> instance.
        /// </summary>
        /// <returns>An <see cref="AFUtility"/> instance to use to use throughout the lifetime of this
        /// <see cref="ExtractImageAreaTask"/> instance.</returns>
        AFUtility AFUtility
        {
            get
            {
                try
                {
                    if (_afUtility == null)
                    {
                        _afUtility = new AFUtility();
                    }

                    return _afUtility;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI33211", ex);
                }
            }
        }

        /// <summary>
        /// Determines the value to use for the AreaID path tag.
        /// </summary>
        /// <param name="path">The path that is being expanded. (all non-custom tags and path tag
        /// functions will already be expanded.)</param>
        /// <returns>The value to use for the AreaID path tag.</returns>
        string ExpandAreaIDTag(string path)
        {
            string replacementValue;

            // Attempt to use an ID of 1 first then continue to increment areaId until a unique
            // output file name is found.
            for (int areaId = 1; true; areaId++)
            {
                // Create the candidate replacement value and the file name that would result.
                replacementValue = areaId.ToString(CultureInfo.InvariantCulture);
                string outputFileName = path.Replace(_AREA_ID_TAG, replacementValue);

                // If the outputFileName exists, increment areaId and try again.
                if (File.Exists(outputFileName))
                {
                    continue;
                }

                // Ensure the destination directory exists.
                string directory = Path.GetDirectoryName(outputFileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // To account for the possibility of the target file having been created between
                // the time we checked for existence and the time we create the file, attempt to
                // create the file now, and if the creation fails with an "already existed" IO
                // exception, increment areaID and try again.
                try
                {
                    using (FileStream fileStream = new FileStream(outputFileName, FileMode.CreateNew))
                    {
                        fileStream.Close();
                    }

                    // If the file was able to be created, leave it there and return the file name.
                    _outputFileCreated = true;
                    break;
                }
                catch (IOException ioException)
                {
                    if (!ioException.Message.Contains("already exists"))
                    {
                        throw ioException.AsExtract("ELI33205");
                    }
                }
            }

            return replacementValue;
        }

        /// <summary>
        /// Gets a <see cref="RasterImage"/> for the specified <see paramref="page"/> of the
        /// specified <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file containing the image page to open.</param>
        /// <param name="page">The page number to open.</param>
        RasterImage GetPageRasterImage(string fileName, int page)
        {
            // Create the reader if it has not already been created.
            if (_imageReader == null)
            {
                _imageReader = Codecs.CreateReader(fileName);
            }

            // Retrieve a cached image page if it exists.
            RasterImage rasterImage;
            if (!_rasterImagePages.TryGetValue(page, out rasterImage))
            {
                // Otherwise read the page, then cache it.
                rasterImage = _imageReader.ReadPage(page);
                _rasterImagePages[page] = rasterImage;
            }

            return rasterImage;
        }

        /// <summary>
        /// Gets the <see cref="RasterZone"/>s to extract.
        /// </summary>
        /// <param name="dataFileName">The name of the data file used to define the zones to extract.
        /// </param>
        IEnumerable<RasterZone> GetZonesToExtract(string dataFileName)
        {
            // Retrieve the attributes matching AttributeQuery.
            IUnknownVector attributes = new IUnknownVector();
            attributes.LoadFrom(dataFileName, false);
            IUnknownVector targetAttributes =
                AFUtility.QueryAttributes(attributes, AttributeQuery, false);

            // Iterate through the SpatialStrings's of each attribute (divided up across pages if
            // necessary.
            foreach (SpatialString spatialString in targetAttributes
                .ToIEnumerable<IAttribute>()
                .Where(attribute => attribute.Value.HasSpatialInfo())
                .SelectMany(attribute => attribute.Value.GetPages(false, "").ToIEnumerable<SpatialString>()))
            {
                if (UseOverallBounds)
                {
                    ComRasterZone comRasterZone = new ComRasterZone();
                    comRasterZone.CreateFromLongRectangle(spatialString.GetOriginalImageBounds(),
                        spatialString.GetFirstPageNumber());

                    yield return new RasterZone(comRasterZone);
                }
                else
                {
                    // If not using overall bounds, iterate each raster zone of this spatial string.
                    foreach (ComRasterZone comRasterZone in spatialString
                        .GetOriginalImageRasterZones()
                        .ToIEnumerable<ComRasterZone>())
                    {
                        yield return new RasterZone(comRasterZone);

                        // If using only the first area, break;
                        if (!OutputAllAreas)
                        {
                            break;
                        }
                    }
                }

                // If using only the first area, break;
                if (!OutputAllAreas)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Outputs the image area.
        /// </summary>
        /// <param name="sourceDocName">The name of the document from which the image is to be
        /// extracted.</param>
        /// <param name="rasterZone">The <see cref="RasterZone"/> defining the image area to extract.
        /// </param>
        void OutputImageArea(string sourceDocName, RasterZone rasterZone)
        {
            // Compute the name of the file to which the image should be extracted.
            string ouputFileName = OutputPathTags.Expand(OutputFileName);

            // The page images are cached for later use; don't immediately dispose of it.
            RasterImage pageImage = GetPageRasterImage(sourceDocName, rasterZone.PageNumber);
            using (RasterImage rasterZoneImage =
                ImageMethods.ExtractZoneFromPage(rasterZone, pageImage))
            using (ImageWriter writer = Codecs.CreateWriter(
                    ouputFileName, rasterZoneImage.OriginalFormat, AllowOutputAppend))
            {
                writer.AppendImage(rasterZoneImage);

                // If not allowing areas to be appended to an existing area, do not allow
                // overwriting an existing file.
                writer.Commit(AllowOutputAppend || _outputFileCreated);
            }
        }

        #endregion Private Members
    }
}
