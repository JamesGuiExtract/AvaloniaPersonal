using Extract.Encryption;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Interface definition for the <see cref="EncryptDecryptFileTask"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("D813C391-E062-406C-B329-6EE8916BC35A")]
    [CLSCompliant(false)]
    public interface IEncryptDecryptFileTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask, ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        /// <value>
        /// The input file.
        /// </value>
        string InputFile { get; set; }

        /// <summary>
        /// Gets or sets the destination file.
        /// </summary>
        /// <value>
        /// The destination file.
        /// </value>
        string DestinationFile { get; set; }

        /// <summary>
        /// Gets or sets whether the destination file should be overwritten if
        /// it exists. If <see langword="true"/> and the file exists, it will
        /// be overwritten; otherwise the task will fail if the destination file exists.
        /// </summary>
        /// <value><see langword="true"/> to allow overwriting the destination and
        /// <see langword="false"/> to fail the task if destination exists.</value>
        bool OverwriteDestination { get; set; }

        /// <summary>
        /// Gets whether or not the password has been set yet.
        /// </summary>
        /// <value><see langword="true"/> if the password has been set.</value>
        bool PasswordSet { get; }

        /// <summary>
        /// Gets whether this task will encrypt the file or not.
        /// </summary>
        /// <value>If <see langword="true"/> the task will encrypt files.
        /// If <see langword="false"/> the task will decrypt files.</value>
        bool EncryptFile { get; }

        /// <summary>
        /// Sets the operation and the password for this task.
        /// </summary>
        /// <param name="encryptFile">If <see langword="true"/> then the task will
        /// encrypt the file; if <see langword="false"/> then the task will
        /// decrypt the file.</param>
        /// <param name="password"></param>
        void SetOptions(bool encryptFile, string password);
    }

    /// <summary>
    /// Represents a file processing task that will allow encrypting and decrypting files.
    /// </summary>
    [ComVisible(true)]
    [Guid("703D54DB-56BE-4EF5-A7A3-F1BC61F1094B")]
    [ProgId("Extract.FileActionManager.FileProcessors.EncryptDecryptFileTask")]
    public class EncryptDecryptFileTask : IEncryptDecryptFileTask
    {
        #region Constants

        /// <summary>
        /// The description of this task
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Encrypt/Decrypt File";

        /// <summary>
        /// Current task version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// License ID for licensing calls
        /// </summary>
        // TODO: Check what license to use
        const LicenseIdName _LICENSE_ID = LicenseIdName.ExtractCoreObjects;

        /// <summary>
        /// Default input file
        /// </summary>
        const string _DEFAULT_INPUT = "<SourceDocName>";

        /// <summary>
        /// Default destination file
        /// </summary>
        const string _DEFAULT_DESTINATION = "<SourceDocName>.enc";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The input file.
        /// </summary>
        string _inputFile = _DEFAULT_INPUT;

        /// <summary>
        /// The destination file.
        /// </summary>
        string _destinationFile = _DEFAULT_DESTINATION;

        /// <summary>
        /// Whether overwriting of destination is allowed.
        /// </summary>
        bool _overwriteDestination;

        /// <summary>
        /// Whether to encrypt or decrypt. (Default is encrypt)
        /// </summary>
        bool _encryptFile = true;

        /// <summary>
        /// The hashed value of the password
        /// </summary>
        byte[] _passwordHash;

        /// <summary>
        /// Whether this object is dirty or not.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Used for encryption calls
        /// </summary>
        MapLabel _label = new MapLabel();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptDecryptFileTask"/> class.
        /// </summary>
        public EncryptDecryptFileTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptDecryptFileTask"/> class.
        /// </summary>
        /// <param name="task">The task to copy settings from.</param>
        public EncryptDecryptFileTask(EncryptDecryptFileTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32327");
            }
        }

        #endregion Constructors

        #region IEncryptDecryptFileTask Members

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        /// <value>
        /// The input file.
        /// </value>
        public string InputFile
        {
            get
            {
                return _inputFile;
            }
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException("Must not be null or empty.");
                    }

                    // Dirty if new value is null OR new value is different than current value
                    _dirty |= !_inputFile.Equals(value, StringComparison.OrdinalIgnoreCase);
                    _inputFile = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32347", "Unable to set input file.");
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
                return _destinationFile ?? string.Empty;
            }
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException("Must not be null or empty.");
                    }

                    // Dirty of new value is null OR new value is different than current value
                    _dirty |= !_destinationFile.Equals(value, StringComparison.OrdinalIgnoreCase);
                    _destinationFile = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32346", "Unable to set destination file.");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the destination file should be overwritten if
        /// it exists. If <see langword="true"/> and the file exists, it will
        /// be overwritten; otherwise the task will fail if the destination file exists.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> to allow overwriting the destination and
        /// <see langword="false"/> to fail the task if destination exists.
        /// </value>
        public bool OverwriteDestination
        {
            get
            {
                return _overwriteDestination;
            }
            set
            {
                _dirty |= _overwriteDestination != value;
                _overwriteDestination = value;
            }
        }

        /// <summary>
        /// Gets whether this task will encrypt the file or not.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> the task will encrypt files.
        /// If <see langword="false"/> the task will decrypt files.
        /// </value>
        public bool EncryptFile
        {
            get
            {
                return _encryptFile;
            }
        }

        /// <summary>
        /// Gets whether or not the password has been set yet.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the password has been set.
        /// </value>
        public bool PasswordSet
        {
            get
            {
                return _passwordHash != null;
            }
        }

        /// <summary>
        /// Sets the operation and the password for this task.
        /// </summary>
        /// <param name="encryptFile">If <see langword="true"/> then the task will
        /// encrypt the file; if <see langword="false"/> then the task will
        /// decrypt the file.</param>
        /// <param name="password"></param>
        public void SetOptions(bool encryptFile, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException("password");
                }

                _passwordHash = ExtractEncryption.GetHashedBytes(password, 1, _label);
                _encryptFile = encryptFile;

                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32328", "Failed settings password.");
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
                using (var dialog = new EncryptDecryptFileTaskSettingsDialog(this))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        InputFile = dialog.InputFile;
                        DestinationFile = dialog.DestinationFile;
                        OverwriteDestination = dialog.OverwriteDestination;

                        _dirty |= _encryptFile != dialog.EncryptFile;
                        _encryptFile = dialog.EncryptFile;

                        if (dialog.Password != null)
                        {
                            _dirty |= _passwordHash == null
                                || !_passwordHash.SequenceEqual(dialog.Password);

                            _passwordHash = dialog.Password;
                        }

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32329", "Unable to configure object.");
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
                // 1. Input file is defined
                // 2. Destination file is defined
                // 3. A password is defined
                return !string.IsNullOrWhiteSpace(_inputFile)
                    && !string.IsNullOrWhiteSpace(_destinationFile)
                    && _passwordHash != null;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32330", "Unable to check configuration.");
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
                var task = new EncryptDecryptFileTask(this);

                return task;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32331", "Unable to clone object");
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
                var task = pObject as EncryptDecryptFileTask;
                if (task == null)
                {
                    throw new InvalidCastException("Object is not an Encrypt/Decrypt file task.");
                }

                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32332", "Unable to copy object.");
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
            // Nothing to do
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI32333", _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32334", "Unable to init task.");
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
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus,
            bool bCancelRequested)
        {
            string sourceFile = string.Empty;
            string destinationFile = string.Empty;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI32335", _COMPONENT_DESCRIPTION);

                // Name of the file being processed that will have it's name changed in the database
                var fileName = pFileRecord.Name;

                // Create a tag manager and expand the tags in the file name
                var tags = new FileActionManagerPathTags(
                    Path.GetFullPath(fileName), pFAMTM.FPSFileDir);

                // Get the source and destination files
                sourceFile = Path.GetFullPath(tags.Expand(_inputFile));
                destinationFile = Path.GetFullPath(tags.Expand(_destinationFile));

                // Validate the source doc and check for destination file if not allowing overwrite
                FileSystemMethods.ValidateFileExistence(sourceFile, "ELI32336");

                if (!_overwriteDestination && File.Exists(destinationFile))
                {
                    throw new ExtractException("ELI32364", "Destination file already exists.");
                }

                TemporaryFile tempOut = null;
                FileStream inputStream = null;
                FileStream outputStream = null;
                try
                {
                    tempOut = new TemporaryFile();
                    outputStream = File.Open(tempOut.FileName, FileMode.Create,
                        FileAccess.ReadWrite, FileShare.None);

                    // Open the input stream
                    FileSystemMethods.PerformFileOperationWithRetryOnSharingViolation(() =>
                    {
                        try
                        {
                            inputStream = File.Open(sourceFile, FileMode.Open,
                                FileAccess.Read, FileShare.Read);
                        }
                        catch
                        {
                            if (inputStream != null)
                            {
                                inputStream.Dispose();
                                inputStream = null;
                            }

                            throw;
                        }
                    });

                    if (_encryptFile)
                    {
                        inputStream.ExtractEncrypt(outputStream, _passwordHash, _label);
                    }
                    else
                    {
                        inputStream.ExtractDecrypt(outputStream, _passwordHash, _label);
                    }

                    // Ensure the directory exists
                    if (!Directory.Exists(Path.GetDirectoryName(destinationFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                    }

                    File.Copy(tempOut.FileName, destinationFile, true);
                }
                finally
                {
                    if (tempOut != null)
                    {
                        tempOut.Dispose();
                    }
                    if (inputStream != null)
                    {
                        inputStream.Dispose();
                    }
                    if (outputStream != null)
                    {
                        outputStream.Dispose();
                    }
                }

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI32337");
                ee.AddDebugData("File ID", pFileRecord.FileID, false);
                ee.AddDebugData("Action ID", nActionID, false);
                ee.AddDebugData("Input File", sourceFile, false);
                ee.AddDebugData("Destination File", destinationFile, false);

                // Throw the extract exception as a COM visible exception
                throw ee.CreateComVisible("ELI32338", "Unable to "
                    + (_encryptFile ? "encrypt" : "decrypt") + " the file.");
            }
        }

        /// <summary>
        /// Returns bool value indicating if the task requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
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
                return LicenseUtilities.IsLicensed(_LICENSE_ID);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32339", "Unable to determine license status.");
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
                    _inputFile = reader.ReadString();
                    _destinationFile = reader.ReadString();
                    _overwriteDestination = reader.ReadBoolean();
                    _encryptFile = reader.ReadBoolean();
                    var hasPassword = reader.ReadBoolean();
                    if (hasPassword)
                    {
                        // Decrypt the encrypted byte array string and convert back to byte array
                        _passwordHash = reader.ReadString()
                            .ExtractDecrypt(_label)
                            .ToByteArray();
                    }
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32340", "Unable to load object from stream.");
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
                    writer.Write(_inputFile);
                    writer.Write(_destinationFile);
                    writer.Write(_overwriteDestination);
                    writer.Write(_encryptFile);

                    writer.Write(PasswordSet);
                    if (PasswordSet)
                    {
                        // Encrypt the stringized byte stream version of the password hash
                        writer.Write(_passwordHash.ToHexString().ExtractEncrypt(_label));
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
                throw ex.CreateComVisible("ELI32341", "Unable to save object to stream.");
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
        /// Copies the settings from the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        public void CopyFrom(EncryptDecryptFileTask task)
        {
            try
            {
                _inputFile = task._inputFile;
                _destinationFile = task._destinationFile;
                _overwriteDestination = task._overwriteDestination;
                _encryptFile = task._encryptFile;
                _passwordHash = null;
                if (task._passwordHash != null)
                {
                    _passwordHash = new byte[task._passwordHash.Length];
                    task._passwordHash.CopyTo(_passwordHash, 0);
                }

                // Task has been updated, set dirty flag
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32342");
            }
        }

        #endregion Methods
    }
}
