using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
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
    /// Interface definition for the <see cref="TransformXmlTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("279B0E05-BDF6-4798-A6A8-12890F23C73C")]
    [CLSCompliant(false)]
    public interface ITransformXmlTask :
        ICategorizedComponent,
        IConfigurableObject,
        IMustBeConfiguredObject,
        ICopyableObject,
        IFileProcessingTask,
        ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// The XML file to transform
        /// </summary>
        string InputPath { get; set; }

        /// <summary>
        /// The path to a stylesheet file containing the transformation
        /// </summary>
        string StyleSheet { get; set; }

        /// <summary>
        /// Whether to use the value of <see cref="StyleSheet"/> or a default
        /// </summary>
        bool UseSpecifiedStyleSheet { get; set; }

        /// <summary>
        /// The output path for the result of the transformation
        /// </summary>
        string OutputPath { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> that
    /// </summary>
    [ComVisible(true)]
    [Guid("4DF5BAB3-8AE1-4D77-AAF1-7D919CF8D442")]
    [ProgId("Extract.FileActionManager.FileProcessors.TransformXmlTask")]
    public class TransformXmlTask : ITransformXmlTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Transform XML";

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
        bool dirty;

        XmlTransformer transformer;
        string inputPath = "<SourceDocName>.xml";
        string styleSheet;
        bool useSpecifiedStyleSheet;
        string outputPath = "<SourceDocName>.xml";

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformXmlTask"/> class.
        /// </summary>
        public TransformXmlTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformXmlTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="TransformXmlTask"/> from which settings should
        /// be copied.</param>
        public TransformXmlTask(TransformXmlTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50275");
            }
        }

        #endregion Constructors

        #region ITransformXmlTask Members

        /// <summary>
        /// The XML file to transform
        /// </summary>
        public string InputPath { get => inputPath; set => SetFieldAndDirtyFlag(ref inputPath, value); }

        /// <summary>
        /// The path to a stylesheet file containing the transformation
        /// </summary>
        public string StyleSheet { get => styleSheet; set => SetFieldAndDirtyFlag(ref styleSheet, value); }

        /// <summary>
        /// Whether to use the value of <see cref="StyleSheet"/> or a default
        /// </summary>
        public bool UseSpecifiedStyleSheet { get => useSpecifiedStyleSheet; set => SetFieldAndDirtyFlag(ref useSpecifiedStyleSheet, value); }

        /// <summary>
        /// The output path for the result of the transformation
        /// </summary>
        public string OutputPath { get => outputPath; set => SetFieldAndDirtyFlag(ref outputPath, value); }

        #endregion ITransformXmlTask Members

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
        /// Performs configuration needed to create a valid <see cref="TransformXmlTask"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI50276", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (TransformXmlTask)Clone();

                using (var dialog = new TransformXmlTaskSettingsDialog(cloneOfThis))
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
                throw ExtractException.CreateComVisible("ELI50277",
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
                throw ex.CreateComVisible("ELI50278", "Unable to check configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="TransformXmlTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="TransformXmlTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new TransformXmlTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI50279", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="TransformXmlTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                if (!(pObject is TransformXmlTask task))
                {
                    throw new InvalidCastException("Invalid copy-from object. Requires TransformXmlTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI50280", "Unable to copy object.", ex);
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
                if (UseSpecifiedStyleSheet)
                {
                    transformer = new XmlTransformer(StyleSheet, new FileActionManagerPathTags(pFAMTM));
                }
                else
                {
                    transformer = new XmlTransformer(XmlTransformer.StyleSheets.AlphaSortName);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI50281",
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
                    "ELI50282", _COMPONENT_DESCRIPTION);

                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFAMTM, pFileRecord.Name);

                var inputPath = pathTags.Expand(InputPath);
                var outputPath = pathTags.Expand(OutputPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                using var tmpFile = new TemporaryFile(true);

                TransformXml(inputPath, tmpFile.FileName);

                File.Copy(tmpFile.FileName, outputPath, true);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI50283", "Error transforming XML");
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
                throw ExtractException.CreateComVisible("ELI50284",
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
            return HResult.FromBoolean(dirty);
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
                    InputPath = reader.ReadString();
                    StyleSheet = reader.ReadString();
                    UseSpecifiedStyleSheet = reader.ReadBoolean();
                    OutputPath = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI50285",
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
                    writer.Write(InputPath);
                    writer.Write(StyleSheet);
                    writer.Write(UseSpecifiedStyleSheet);
                    writer.Write(OutputPath);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI50286",
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
        /// Copies the specified <see cref="TransformXmlTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="TransformXmlTask"/> from which to copy.</param>
        void CopyFrom(TransformXmlTask task)
        {
            InputPath = task.InputPath;
            StyleSheet = task.StyleSheet;
            UseSpecifiedStyleSheet = task.UseSpecifiedStyleSheet;
            OutputPath = task.OutputPath;

            dirty = true;
        }

        /// <summary>
        /// Run the transformation
        /// </summary>
        void TransformXml(string inputPath, string outputPath)
        {
            using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var outputStream = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            transformer.TransformXml(inputStream, outputStream);
        }

        /// <summary>
        /// Update the field and set dirty if the new value is different
        /// </summary>
        void SetFieldAndDirtyFlag<T>(ref T oldValue,  T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                dirty = true;
            }
        }

        #endregion Private Members
    }
}
