﻿using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Specifies a the resolution <see cref="CreateFileTask"/> should take when the target file
    /// already exists.
    /// </summary>
    [ComVisible(true)]
    [Guid("CDE405F7-803B-45C1-BD36-4D53C589DC56")]
    public enum CreateFileConflictResolution
    {
        /// <summary>
        /// Don't write the file; throw an exception.
        /// </summary>
        GenerateError = 0,

        /// <summary>
        /// Don't write the file; continue without exception.
        /// </summary>
        SkipWithoutError = 1,

        /// <summary>
        /// Overwrite the existing file.
        /// </summary>
        Overwrite = 2,

        /// <summary>
        /// Append to the existing file.
        /// </summary>
        Append = 3
    }

    /// <summary>
    /// Interface definition for the Create File Task
    /// </summary>
    [ComVisible(true)]
    [Guid("A569A02E-8498-44BA-B007-0961ED223B98")]
    [CLSCompliant(false)]
    public interface ICreateFileTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask,
        ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets the name of the target file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file contents. This may be <see cref="String.Empty"/>
        /// or <see langword="null"/> to produce an empty file.
        /// </summary>
        /// <value>
        /// The file contents.
        /// </value>
        string FileContents { get; set; }

        /// <summary>
        /// Gets or sets the behavior of the task when the target file exists.
        /// </summary>
        /// <value>
        /// The task behavior when the target file exists.
        /// </value>
        CreateFileConflictResolution FileExistsBehavior { get; set; }
    }

    /// <summary>
    /// An <see cref="IFileProcessingTask"/> which generates a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("4D7F59D3-ECD2-46F0-8750-71194A131777")]
    [ProgId("Extract.FileActionManager.FileProcessors.CreateFileTask")]
    public class CreateFileTask : ICreateFileTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Create file";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the file to be generated.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The contents of the file to be generated.
        /// </summary>
        string _fileContents;

        /// <summary>
        /// The <see cref="CreateFileConflictResolution"/> that should be employed when the target
        /// file already exists.
        /// </summary>
        CreateFileConflictResolution _conflictResolution;

        /// <summary>
        /// Indicates that settings have been changed, but not saved.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFileTask"/> class.
        /// </summary>
        public CreateFileTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFileTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="CreateFileTask"/> from which settings should
        /// be copied.</param>
        public CreateFileTask(CreateFileTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31849");
            }
        }

        #endregion Constructors

        #region ICreateFileTask Members

        /// <summary>
        /// Gets or sets the name of the target file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentNullException("value");
                    }
                    else if (value.Equals("<SourceDocName>", StringComparison.Ordinal))
                    {
                        throw new ExtractException("ELI32393",
                            "Cannot overwrite source document.");
                    }

                    _dirty |= !string.Equals(_fileName, value, StringComparison.OrdinalIgnoreCase);
                    _fileName = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32391", "Unable to set file name.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the file contents. This may be <see cref="String.Empty"/>
        /// or <see langword="null"/> to produce an empty file.
        /// </summary>
        /// <value>
        /// The file contents.
        /// </value>
        public string FileContents
        {
            get
            {
                return _fileContents;
            }
            set
            {
                _dirty |= !string.Equals(_fileContents, value, StringComparison.Ordinal);
                _fileContents = value;
            }
        }

        /// <summary>
        /// Gets or sets the behavior of the task when the target file exists.
        /// </summary>
        /// <value>
        /// The task behavior when the target file exists.
        /// </value>
        public CreateFileConflictResolution FileExistsBehavior
        {
            get
            {
                return _conflictResolution;
            }
            set
            {
                try
                {
                    if (!Enum.IsDefined(typeof(CreateFileConflictResolution), value))
                    {
                        throw new ArgumentOutOfRangeException("value", value,
                            "Invalid value specified.");
                    }

                    _dirty |= _conflictResolution != value;
                    _conflictResolution = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32392",
                        "Unable to set behavior when target file exists.");
                }
            }
        }

        #endregion

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
        /// Performs configuration needed to create a valid <see cref="CreateFileTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (var dialog = new CreateFileTaskSettingsDialog())
                {
                    dialog.FileName = _fileName;
                    dialog.FileContents = _fileContents;
                    dialog.CreateFileConflictResolution = _conflictResolution;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _fileName = dialog.FileName;
                        _fileContents = dialog.FileContents;
                        _conflictResolution = dialog.CreateFileConflictResolution;

                        _dirty = true;
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31833",
                    "Error running configuration.", ex);
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
            return !string.IsNullOrWhiteSpace(_fileName);
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="CreateFileTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="CreateFileTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new CreateFileTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31834", "Unable to clone object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="CreateFileTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var task = pObject as CreateFileTask;
                if (task == null)
                {
                    throw new InvalidCastException("Invalid cast to CreateFileTask");
                }
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31835", "Unable to copy object.", ex);
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
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI31850", _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31851", "Unable to initialize \"Create file\" task.");
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI31836", _COMPONENT_DESCRIPTION);

                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFileRecord.Name, pFAMTM.FPSFileDir,
                        pFAMTM.FPSFileName);
                string fileName = pathTags.Expand(_fileName);
                string fileContents = pathTags.Expand(_fileContents);

                ExtractException.Assert("ELI31854",
                    "\"Create file\" task cannot write to the source document",
                    !fileName.Equals(pFileRecord.Name, StringComparison.OrdinalIgnoreCase));

                // Create the directory the file is to be written to if it does not already exist.
                string directory = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(fileName))
                {
                    switch (_conflictResolution)
                    {
                        case CreateFileConflictResolution.GenerateError:
                            {
                                ExtractException ee = new ExtractException("ELI31852",
                                    "Create file task failed to create the file because it already existed.");
                                ee.AddDebugData("Filename", fileName, false);
                                throw ee;
                            }

                        case CreateFileConflictResolution.SkipWithoutError:
                            {
                                return EFileProcessingResult.kProcessingSuccessful;
                            }
                    }
                }

                if (_conflictResolution == CreateFileConflictResolution.Append)
                {
                    // Perform the append operation in a retry block
                    FileSystemMethods.PerformFileOperationWithRetry(
                        () => File.AppendAllText(fileName, fileContents),
                        true);
                }
                else
                {
                    File.WriteAllText(fileName, fileContents);
                }

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31837", "Unable to process the file.");
            }
        }

        #endregion IFileProcessingTask Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access.
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access.</returns>
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
            try
            {
                return LicenseUtilities.IsLicensed(LicenseIdName.FileActionManagerObjects);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31838",
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
                    _fileName = reader.ReadString();
                    _fileContents = reader.ReadString();
                    _conflictResolution = (CreateFileConflictResolution)reader.ReadInt32();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI31839",
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
                    writer.Write(_fileName);
                    writer.Write(_fileContents);
                    writer.Write((int)_conflictResolution);

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
                throw ExtractException.CreateComVisible("ELI31840",
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

        #endregion

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
        /// Copies the specified <see cref="CreateFileTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="CreateFileTask"/> from which to copy.</param>
        void CopyFrom(CreateFileTask task)
        {
            _fileName = task._fileName;
            _fileContents = task._fileContents;
            _conflictResolution = task._conflictResolution;

            _dirty = true;
        }

        #endregion Private Members
    }
}
