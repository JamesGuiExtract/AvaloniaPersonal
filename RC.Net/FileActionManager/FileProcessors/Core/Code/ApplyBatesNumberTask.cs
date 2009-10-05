using Extract.Imaging;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that applies Bates numbers to documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("8DD63918-D299-48DB-BA54-FD1CFAAAF0E2")]
    [ProgId("Extract.FileActionManager.FileProcessors.ApplyBatesNumberTask")]
    public class ApplyBatesNumberTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
        IFileProcessingTask, ILicensedComponent, IPersistStream, IDisposable
    {
        #region Constants

        /// <summary>
        /// The current version of the <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The description to be displayed in the categorized component selection list.
        /// </summary>
        static readonly string _COMPONENT_DESCRIPTION = "Apply Bates number";

        #endregion Constants

        #region Fields

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _COMPONENT_DESCRIPTION);

        /// <summary>
        /// The <see cref="BatesNumberFormat"/> to use to apply Bates numbers.
        /// </summary>
        BatesNumberFormat _format = new BatesNumberFormat(true);

        /// <summary>
        /// The file to operate on.
        /// </summary>
        string _fileName;

        /// <summary>
        /// Indicates whether this task object is dirty or not
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyBatesNumberTask"/> class.
        /// </summary>
        public ApplyBatesNumberTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyBatesNumberTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="ApplyBatesNumberTask"/> to initialize from.</param>
        public ApplyBatesNumberTask(ApplyBatesNumberTask task)
        {
            CopyFrom(task);
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
        /// Performs configuration needed to create a valid <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Get a db manager and connect to the database
                FileProcessingDBClass databaseManager = new FileProcessingDBClass();
                databaseManager.ConnectLastUsedDBThisProcess();

                // Create a new BatesNumberGenerator
                using (BatesNumberGeneratorWithDatabase generator =
                    new BatesNumberGeneratorWithDatabase(_format, databaseManager))
                {

                    // Create the settings dialog
                    using (ApplyBatesNumberSettingsDialog dialog =
                        new ApplyBatesNumberSettingsDialog(generator, _fileName))
                    {

                        // Show the dialog and return whether the settings where modified or not
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            // Dispose of the existing format
                            if (_format != null)
                            {
                                _format.Dispose();
                            }

                            // Get the new format and file name from the dialog
                            _format = dialog.BatesNumberGenerator.Format;
                            _fileName = dialog.FileName;

                            _dirty = true;

                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27888", "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ApplyBatesNumberTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ApplyBatesNumberTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new ApplyBatesNumberTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27889", "Unable to clone task.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ApplyBatesNumberTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom(pObject as ApplyBatesNumberTask);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27890", "Unable to copy the task.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ApplyBatesNumberTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="ApplyBatesNumberTask"/> from which to copy.</param>
        public void CopyFrom(ApplyBatesNumberTask task)
        {
            try
            {
                ExtractException.Assert("ELI27891", "Task cannot be NULL.", task != null);

                if (_format != null)
                {
                    _format.Dispose();
                }

                _format = task._format.Clone();

                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27892", "Unable to copy the task.", ex);
            }
        }

        #endregion ICopyableObject Members 

        #region IFileProcessingTask Members

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            // Nothing to do (this task is not cancellable
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            // Nothing to do to close this task
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        public void Init()
        {
            // Nothing to do to initiliaze the task
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
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(string bstrFileFullName, int nFileID,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI27893");

                // TODO: Actually apply the bates number
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27894", "Unable to process the file.", ex);
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
            return LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects);
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
            classID = this.GetType().GUID;
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
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the file name from the stream
                    _fileName = reader.ReadString();

                    // Ensure the format is disposed if it exists
                    if (_format != null)
                    {
                        _format.Dispose();
                        _format = null;
                    }

                    // Read the new format from the stream
                    _format = reader.ReadObject<BatesNumberFormat>();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27895",
                    "Unable to load object from stream", ex);
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
        public void Save(IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Serialize the settings
                    writer.Write(_fileName);
                    writer.WriteObject<BatesNumberFormat>(_format);

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
                throw ExtractException.CreateComVisible("ELI27896",
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

        #endregion Methods

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ApplyBatesNumberTask"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_format != null)
                {
                    _format.Dispose();
                    _format = null;
                }
            }

            // No unmanaged resources to release
        }

        #endregion IDisposable
    }
}
