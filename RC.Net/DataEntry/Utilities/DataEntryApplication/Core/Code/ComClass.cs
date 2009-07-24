using Extract;
using Extract.DataEntry;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// The generic application used to run all data entry forms.  The application consists of two
    /// panes:
    /// <list type="bullet">
    /// <item>The Data Entry Panel (DEP) will display the content from a document and allow for the content
    /// to be verifed/corrected.  The DEP consists of a <see cref="DataEntryControlHost"/> instance 
    /// populated by controls which implement <see cref="IDataEntryControl"/>.</item>
    /// <item>The image viewer will display the document image itself and allow for interaction with the
    /// DEP such as highlighting the image area associated with the content currently selected in the DEP
    /// or allowing DEP controls to be populated via OCR "swipes" in the image viewer.</item>
    /// </list>
    /// </summary>
    [Guid("59496DF7-3951-49b7-B063-8C28F4CD843F")]
    [ProgId("Extract.DataEntry.Utilities.DataEntryApplication")]
    [ComVisible(true)]
    public class ComClass : IFileProcessingTask, ICategorizedComponent, ILicensedComponent,
        ICopyableObject, IPersistStream, IConfigurableObject, IMustBeConfiguredObject
    {
        #region Constants

        /// <summary>
        /// The default filename that will appear in the FAM to describe the task the data entry
        /// application is fulfilling
        /// </summary>
        static readonly string _DEFAULT_FILE_ACTION_TASK_NAME = "Verify extracted data";

        /// <summary>
        /// The current version of this object.
        /// <para><b>Versions:</b></para>
        /// <list type="bullet">
        /// <item>2: Added _configFileName</item>
        /// </list>
        /// </summary>
        static readonly int _CURRENT_VERSION = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// A thread-safe manager class used to funnel calls from multiple threads to a single 
        /// <see cref="DataEntryApplicationForm"/> instance and be able to route exceptions back to
        /// to the calling thread.
        /// </summary>
        static VerificationForm<DataEntryApplicationForm> _dataEntryFormManager =
            new VerificationForm<DataEntryApplicationForm>();

        /// <summary>
        /// Indicates whether the object has been modified since being loaded via the 
        /// IPersistStream interface. This is an int because that is the return type of 
        /// IPersistStream::IsDirty in order to support COM values of <see cref="HResult.Ok"/> and 
        /// <see cref="HResult.False"/>.
        /// </summary>
        int _dirty;

        /// <summary>
        /// The name of the DataEntry configuration file to use for the DataEntryApplicationForm.
        /// </summary>
        string _configFileName;

        #endregion Fields

        #region Contructors

        /// <summary>
        /// Initializes a new <see cref="ComClass"/> class.
        /// </summary>
        public ComClass()
        {
            // Nothing to do.
        }

        #endregion Contructors

        #region Properties

        /// <summary>
        /// Gets or set the name of the DataEntry configuration file to use for the
        /// <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        /// <value>The name of the DataEntry configuration file to use.</value>
        /// <returns>The name of the DataEntry configuration file to use.</returns>
        public string ConfigFileName
        {
            get
            {
                return _configFileName;
            }

            set
            {
                _configFileName = value;

                _dirty = HResult.Ok;
            }
        }

        #endregion Properties

        #region IPersistStreamMembers

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
        /// Checks if the object for changes since it was last saved.
        /// </summary>
        /// <returns><see langword="true"/> if the object has changes since it was last saved;
        /// <see langword="false"/> otherwise.</returns>
        public int IsDirty()
        {
            return _dirty;
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            MemoryStream memoryStream = null;

            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings from the memory stream
                    if (reader.Version >= 2)
                    {
                        _configFileName = reader.ReadString();
                    }
                }

                _dirty = HResult.False;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23992", 
                    "Error loading data entry application settings.", ex);
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                }
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
                    // Save the settings
                    writer.Write(_configFileName);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = HResult.False;
                }
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23789", 
                    "Error saving data entry application settings.", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// <para>NOTE: Not implemented.</para>
        /// </summary>
        /// <param name="size">Will always be <see cref="HResult.NotImplemented"/> to indicate this
        /// method is not implemented.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStreamMembers

        #region ILicensedComponent Members

        /// <summary>
        /// Checks if component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if licensed, <see langword="false"/> if not licensed.
        /// </returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(LicenseIdName.LabdeVerificationUIObject);
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23993", "License validation failed.", ex);
            }
        }

        #endregion ILicensedComponent Members

        #region ICategorizedComponent

        /// <summary>
        /// Returns the name of this COM object.
        /// </summary>
        /// <returns>The name of this COM object.</returns>
        public string GetComponentDescription()
        {
            try
            {
                // Attempt to obtain the component description from the config file.
                return _DEFAULT_FILE_ACTION_TASK_NAME;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23877", "Unable to get component description.", ex);
            }
        }

        #endregion ICategorizedComponent

        #region IFileProcessingTask Members

        /// <summary>
        /// Initializes the <see cref="DataEntryApplicationForm"/> to receive documents for
        /// processing.
        /// </summary>
        public void Init()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.LabdeVerificationUIObject,
                    "ELI26896", _DEFAULT_FILE_ACTION_TASK_NAME);

                // Ask the manager to create and display the data entry form.
                _dataEntryFormManager.ShowForm(CreateDataEntryForm);
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23983", 
                    "Failed to initialize data entry form.", ex);
            }
        }

        /// <summary>
        /// Opens the specified document to allow indexed data to be verified/edited.
        /// </summary>
        /// <param name="bstrFileFullName">A <see langword="string"/> that specifies the file being
        /// processed.</param>
        /// <param name="nFileID">The ID of the file being processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> object to update progress
        /// (not updated by this class).</param>
        /// <param name="bCancelRequested">If <see langword="true"/>, the user has requested that
        /// processing be cancelled. In this case, the provided document will not be processed.
        /// </param>
        /// <returns><see langword="true"/> if processing of the document completed successfully.
        /// <see langword="false"/> if processing of the document was cancelled by the user.
        /// </returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(string bstrFileFullName, int nFileID, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.LabdeVerificationUIObject,
                    "ELI26897", _DEFAULT_FILE_ACTION_TASK_NAME);

                if (!bCancelRequested)
                {
                    // As long as processing has not been cancelled, open the supplied document in the
                    // data entry form.
                    _dataEntryFormManager.ShowDocument(bstrFileFullName);
                }

                return (bCancelRequested || _dataEntryFormManager.Canceled) ?
                    EFileProcessingResult.kProcessingCancelled : EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23875", 
                    "Failed processing file.", ex);
            }
        }

        /// <summary>
        /// Caller can cancel processing by using this method. The 
        /// <see cref="DataEntryApplicationForm"/> will be closed.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.LabdeVerificationUIObject,
                    "ELI26898", _DEFAULT_FILE_ACTION_TASK_NAME);

                _dataEntryFormManager.Cancel();
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23874", "Unable to cancel.", ex);
            }
        }

        /// <summary>
        /// Ends processing by closing the <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.LabdeVerificationUIObject,
                    "ELI26899", _DEFAULT_FILE_ACTION_TASK_NAME);

                _dataEntryFormManager.CloseForm();
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23873", "Unable to close.", ex);
            }
        }

        #endregion IFileProcessingTask Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the current <see cref="ComClass"/> instance.
        /// </summary>
        /// <returns>A copy of the current <see cref="ComClass"/> instance.</returns>
        public object Clone()
        {
            try
            {
                ComClass clone = new ComClass();

                clone.CopyFrom(this);

                return clone;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23879", "Clone failed.", ex);
            }
        }

        /// <summary>
        /// Copies the value of the provided <see cref="ComClass"/> instance into the current one.
        /// </summary>
        /// <param name="pObject">The object to copy from.</param>
        /// <exception cref="ExtractException">If the supplied object is not of type
        /// <see cref="ComClass"/>.</exception>
        public void CopyFrom(object pObject)
        {
            try
            {
                ComClass copyThis = pObject as ComClass;
                ExtractException.Assert("ELI23795", "Cannot copy from an object of a different type!",
                    copyThis != null);

                // Copy properties here
                _configFileName = copyThis._configFileName;
            }
            catch (Exception ex)
            {
                // Memory leak?  See: [DataEntry:143]
                throw ExtractException.CreateComVisible("ELI23880", "Unable to copy COM class.", ex);
            }
        }

        #endregion ICopyableObject Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to run the class as an <see cref="IFileProcessingTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was not successful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.LabdeVerificationUIObject,
                    "ELI26900", _DEFAULT_FILE_ACTION_TASK_NAME);

                // Create a new configuration form to display the configurable settings to the user.
                ConfigurationForm configForm = new ConfigurationForm(this);

                // Display the configuration screen.
                bool savedSettings = configForm.Configure();

                // If the user attempted to apply settings, enter a loop to validate the settings. 
                while (savedSettings)
                {
                    // Refresh the UI to prevent artifacts of the UI screen from lingering.
                    Application.DoEvents();

                    try
                    {
                        // Test the configuration settings.
                        ValidateSettings();

                        // If no exceptions were thrown, exit the validation loop.
                        break;
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = new ExtractException("ELI25490",
                            _DEFAULT_FILE_ACTION_TASK_NAME + " is not properly configured.\r\n\r\n" +
                            "Please correct or complete configuration of all required fields.", ex);
                        ee.Display();
                    }

                    // If an exception was thrown during ValidateSettings, re-display the
                    // configuration screen to allow the user to correct the problem or cancel
                    // configuration.
                    savedSettings = configForm.Configure();
                }

                return savedSettings;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI25471", "Configuration failed.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Tests to ensure <see cref="ComClass"/> is properly configured to run as an 
        /// <see cref="IFileProcessingTask"/>.
        /// <para><b>Note:</b></para>
        /// If <see cref="ComClass"/> is not properly configured, and exception will be logged which
        /// provides details about the configuration problem.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="ComClass"/>is properly configured;
        /// <see langword="false"/> if it is not.</returns>
        public bool IsConfigured()
        {
            try
            {
                try
                {
                    // Test the configuration settings.
                    ValidateSettings();

                    // If no exception was thrown, the calls is properly configured.
                    return true;
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI25488", ex);
                }

                // An exception was thrown from ValidateSettings; the class is not properly
                // configured.
                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI25486", "Unable to determine if configured.", ex);
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region Private Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <see langword="type"/> being registered.</param>
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
        /// <param name="type">The <see langword="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Creates a <see cref="DataEntryApplicationForm"/> using the current settings.
        /// </summary>
        /// <returns>A <see cref="DataEntryApplicationForm"/> using the current settings.</returns>
        IVerificationForm CreateDataEntryForm()
        {
            DataEntryApplicationForm form = new DataEntryApplicationForm(_configFileName);
            form.StandAloneMode = false;
            return form;
        }

        /// <summary>
        /// Validates the current settings to ensure this <see cref="ComClass"/> instance is ready
        /// to run as a <see cref="IFileProcessingTask"/>
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the class is not properly configured.</throws>
        void ValidateSettings()
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    // Ask the manager to validate the DEP can be initialize using the specified
                    // config file
                    _dataEntryFormManager.ValidateForm(CreateDataEntryForm);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25491", ex);
            }
        }

        #endregion Private Methods
    }
}
