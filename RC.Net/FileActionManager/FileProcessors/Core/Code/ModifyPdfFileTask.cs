using Extract.Imaging;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using PegasusImaging.WinForms.PdfXpress3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that will modify the contents of a PDF file.
    /// </summary>
    [ComVisible(true)]
    [Guid("6D291B26-416E-47D0-9DAB-13BEB318B534")]
    [ProgId("Extract.FileActionManager.FileProcessors.ModifyPdfFileTask")]
    public class ModifyPdfFileTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask, ILicensedComponent,
        IPersistStream, IDisposable
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Modify pdf file";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The XFDF text as a <see cref="T:byte[]"/>.
        /// </summary>
        static readonly byte[] _xfdfBytes = BuildEmptyXfdfStream();

        // Unlock codes for the PdfXpress engine
        static readonly int[] _ul = new int[] {352502263,995632770,1963445779,32594};

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ModifyPdfFileTaskSettings"/> for this task.
        /// </summary>
        ModifyPdfFileTaskSettings _settings;

        /// <summary>
        /// Whether the object is dirty or not.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Used to check whether the document is a PDF file or not.
        /// </summary>
        ImageCodecs _codecs;

        /// <summary>
        /// Mutex used to guarantee single threadedness to the remove annotations method
        /// </summary>
        static Mutex _mutex =
            ThreadingMethods.GetGlobalNamedMutex(@"Global\EC9E480D-BE75-4749-A5DF-2FD6583F8FAC");

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTask"/>.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTask"/>.
        /// </summary>
        public ModifyPdfFileTask() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileTask"/>.
        /// </summary>
        /// <param name="settings">The settings this task should use.</param>
        public ModifyPdfFileTask(ModifyPdfFileTaskSettings settings)
        {
            try
            {
                _settings = settings ?? new ModifyPdfFileTaskSettings();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29644",
                    "Failed to create Modify pdf task object.", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the settings for this task.
        /// </summary>
        /// <value>The settings this task will use.</value>
        /// <exception cref="ExtractException">If the value to be set is
        /// <see langword="null"/>.</exception>
        public ModifyPdfFileTaskSettings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                try
                {
                    // Ensure the new value is not null
                    ExtractException.Assert("ELI29645", "Settings cannot be NULL.",
                        value != null);

                    if (value != _settings)
                    {
                        _settings = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.CreateComVisible("ELI29646",
                        "Unable to set new Modify pdf task settings..", ex);
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
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileProcessors);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Copies the specified <see cref="ModifyPdfFileTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="ModifyPdfFileTask"/> from which to copy.</param>
        public void CopyFrom(ModifyPdfFileTask task)
        {
            try
            {
                // Copy the settings
                _settings = new ModifyPdfFileTaskSettings(task.Settings);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29647", ex);
            }
        }

        /// <summary>
        /// Builds an empty xfdf byte array.
        /// </summary>
        /// <returns>A <see cref="T:byte[]"/> containing an empty xfdf string.</returns>
        static byte[] BuildEmptyXfdfStream()
        {
            // The xfdf string to remove annotations
            string xfdfText = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><xfdf xmlns="
                + "\"http://ns.adobe.com/xfdf/\" xml:space=\"preserve\"><annots/><ids original="
                + "\"0FF75CC9984F9D5DB0952D7ABA83CDE4\" modified=\"0FF75CC9984F9D5DB0952D7ABA83CDE4\"/>"
                + "</xfdf>";

            // Convert the string to a byte array
            byte[] xfdf = ASCIIEncoding.ASCII.GetBytes(xfdfText);

            return xfdf;
        }

        /// <summary>
        /// Removes the annotations from the specified file.
        /// </summary>
        /// <param name="pdfFile">The file to remove annotations from.</param>
        void RemoveAnnotationsFromPdfFile(string pdfFile)
        {
            // Ensure a file name was specified and that it exists.
            ExtractException.Assert("ELI29701", "Specified PDF file does not exist.",
                !string.IsNullOrEmpty(pdfFile) && File.Exists(pdfFile), "PDF File Name", pdfFile);

            // Ensure the file is a PDF file [LRCAU #5729]
            ExtractException.Assert("ELI29695", "File is not a pdf file.",
                ImageMethods.IsPdf(pdfFile, _codecs), "PDF File Name", pdfFile);

            PdfXpress express = null;
            Document document = null;
            TemporaryFile tempFile = null;
            try
            {
                // Block until the mutex is available
                _mutex.WaitOne();

                // Create a temporary PDF file to write to
                tempFile = new TemporaryFile(".pdf");

                // Initialize the PdfXpress engine
                express = new PdfXpress();
                express.Initialize();
                express.Licensing.UnlockRuntime(_ul[0], _ul[1], _ul[2], _ul[3]);

                // Open the file
                document = new Document(express, pdfFile);

                // Check for remove annotations
                if (_settings.RemoveAnnotations)
                {
                    // Create the Xfdf options (set all annotations on all pages and allow delete)
                    XfdfOptions xfdfoptions = new XfdfOptions();
                    xfdfoptions.WhichAnnotation = XfdfOptions.AllAnnotations;
                    xfdfoptions.WhichPage = XfdfOptions.AllPages;
                    xfdfoptions.CanDeleteAnnotations = true;

                    // Import the xfdf bytes containing no annotations (this will remove
                    // the annotations from the document)
                    document.ImportXfdf(xfdfoptions, _xfdfBytes);
                }

                // Set the save file options to save to the temp file
                SaveOptions sfo = new SaveOptions();
                sfo.Filename = tempFile.FileName;
                sfo.Overwrite = true;

                // Save the document
                document.Save(sfo);

                // Dispose of the pdf express document
                document.Dispose();
                document = null;

                // Move the temp file to the destination
                FileSystemMethods.MoveFile(tempFile.FileName, pdfFile, true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29702", ex);
                ee.AddDebugData("PDF File Name", pdfFile, false);
                throw ee;
            }
            finally
            {
                if (document != null)
                {
                    document.Dispose();
                }
                if (tempFile != null)
                {
                    tempFile.Dispose();
                }
                if (express != null)
                {
                    express.Dispose();
                }

                // Ensure the mutex is released (do this as the last step after
                // the PdfXpress engine is released)
                _mutex.ReleaseMutex();
            }
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

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ModifyPdfFileTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.PegasusPdfxpressModifyPdf,
                    "ELI29648", _COMPONENT_DESCRIPTION);

                // Allow the user to set the modify pdf file settings
                using (ModifyPdfFileSettingsDialog dialog
                    = new ModifyPdfFileSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        Settings = dialog.Settings;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29649",
                    "Error running configuration.", ex);
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
                // This class is configured if the settings are valid
                return _settings.ValidSettings;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29650",
                    "Failed checking configuration.", ex);
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ModifyPdfFileTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ModifyPdfFileTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                ModifyPdfFileTask task =
                    new ModifyPdfFileTask(new ModifyPdfFileTaskSettings(_settings));

                return task;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29651", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ModifyPdfFileTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                ModifyPdfFileTask task = (ModifyPdfFileTask)pObject;
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29652", "Unable to copy object.", ex);
            }
        }

        #endregion

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
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29696",
                    "Failed closing Modify pdf task.", ex);
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.PegasusPdfxpressModifyPdf,
                    "ELI29653", _COMPONENT_DESCRIPTION);

                // Create the ImageCodecs object
                _codecs = new ImageCodecs();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29654",
                    "Failed to initialize Modify pdf task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="bstrFileFullName">The file to process.</param>
        /// <param name="nFileID">The ID of the file being processed.</param>
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
        public EFileProcessingResult ProcessFile(string bstrFileFullName, int nFileID,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.PegasusPdfxpressModifyPdf,
                    "ELI29655", _COMPONENT_DESCRIPTION);

                // Get the file name and initialize a path tags class
                string fileName = Path.GetFullPath(bstrFileFullName);
                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(fileName, pFAMTM.FPSFileDir);

                // Expand any path tags in the file name
                string pdfFile = pathTags.Expand(_settings.PdfFile);

                // Remove the annotations
                RemoveAnnotationsFromPdfFile(pdfFile);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29657", ex);
                ee.AddDebugData("Source Document", bstrFileFullName, false);
                ee.AddDebugData("PDF File From Settings", _settings.PdfFile, false);

                throw ExtractException.CreateComVisible("ELI29658", "Failed Modify pdf task.", ee);
            }
        }

        #endregion

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
                return LicenseUtilities.IsLicensed(LicenseIdName.PegasusPdfxpressModifyPdf);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29659",
                    "Unable to determine license status.", ex);
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
            return HResult.FromBoolean(_dirty && _settings.Dirty);
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
                    // Read the settings
                    _settings = ModifyPdfFileTaskSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29660", 
                    "Unable to load Modify pdf task.", ex);
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
                    // Serialize the settings
                    _settings.WriteTo(writer, clearDirty);

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
		        throw ExtractException.CreateComVisible("ELI29661", 
			        "Unable to save verification task.", ex);
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

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ModifyPdfFileTask"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ModifyPdfFileTask"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ModifyPdfFileTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable
    }
}
