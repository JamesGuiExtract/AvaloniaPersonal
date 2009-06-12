using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Handles mapping orders from the rules output into Lab orders and their
    /// associated EPIC codes based on a database file.
    /// </summary>
    [Guid("ABC13C14-B6C6-4679-A69B-5083D3B4B60C")]
    [ProgId("Extract.DataEntry.LabDE.LabDEOrderMapper")]
    [ComVisible(true)]
    public class LabDEOrderMapper : IOutputHandler, ICopyableObject, ICategorizedComponent,
        IPersistStream, IConfigurableObject, IMustBeConfiguredObject
    {
        #region Constants

        /// <summary>
        /// The default filename that will appear in the FAM to describe the task the data entry
        /// application is fulfilling
        /// </summary>
        private static readonly string _DEFAULT_OUTPUT_HANDLER_NAME = "LabDE order mapper";

        /// <summary>
        /// The current version for this object.
        /// </summary>
        private static readonly int _CURRENT_VERSION = 1;

        /// <summary>
        /// The GUID for the "UCLID Output Handler" COM category;
        /// </summary>
        private static readonly string _UCLID_OUTPUT_HANDLER_GUID =
            "{1B84DB33-2B7E-49d2-BB16-A4A2283D5D9F}";

        /// <summary>
        /// An int to represnet COM's S_OK;
        /// </summary>
        private const int _S_OK = 0;

        /// <summary>
        /// An int to represnet COM's S_FALSE;
        /// </summary>
        private const int _S_FALSE = 1;

        /// <summary>
        /// A long to represnet COM's E_NOTIMPL;
        /// </summary>
        private const long _E_NOTIMPL = 0x80004001;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the database file to use for order mapping.
        /// </summary>
        private string _databaseFile;

        /// <summary>
        /// Flag to indicate whether this object is dirty or not.
        /// </summary>
        private bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        public LabDEOrderMapper() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapper"/> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to attach to.</param>
        public LabDEOrderMapper(string databaseFile)
        {
            try
            {
                _databaseFile = databaseFile;
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26169", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the database file name.
        /// </summary>
        /// <returns>The database file name.</returns>
        public string DatabaseFileName
        {
            get
            {
                return _databaseFile;
            }
            set
            {
                _databaseFile = value;
                _dirty = true;
            }
        }

        #endregion Properties

        #region IOutputHandler Members

        /// <summary>
        /// Processes the attributes for output.
        /// </summary>
        /// <param name="pAttributes">The collection of attributes to process.</param>
        /// <param name="pDoc">The document object.</param>
        /// <param name="pProgressStatus">The progress status to update.</param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc,
            ProgressStatus pProgressStatus)
        {
            try
            {
                // Expand the tags in the database file name
                AFUtility afUtility = new AFUtility();
                string databaseFile = afUtility.ExpandTagsAndFunctions(_databaseFile, pDoc);

                // Check for the database files existence
                if (File.Exists(databaseFile))
                {
                    ExtractException ee = new ExtractException("ELI26170",
                        "Database file does not exist!");
                    ee.AddDebugData("Database File Name", databaseFile, false);
                    throw ee;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26171", ex);
                throw new ExtractException("ELI26172", ee.AsStringizedByteStream());
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Returns a copy of this object.
        /// </summary>
        /// <returns>A copy of this object.</returns>
        public object Clone()
        {
            try
            {
                LabDEOrderMapper newMapper = new LabDEOrderMapper(_databaseFile);

                return newMapper;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26173", ex);
                throw new ExtractException("ELI26174", ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Sets this object from the specified object.
        /// </summary>
        /// <param name="pObject">The object to copy from.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                LabDEOrderMapper mapper = pObject as LabDEOrderMapper;
                if (mapper == null)
                {
                    ExtractException ee = new ExtractException("ELI26175", "Cannot copy from object!");
                    ee.AddDebugData("Object Type",
                        pObject != null ? pObject.GetType().ToString() : "null", false);
                    throw ee;
                }

                _databaseFile = mapper.DatabaseFileName;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26176", ex);
                throw new ExtractException("ELI26177", ee.AsStringizedByteStream());
            }
        }

        #endregion

        #region ICategorizedComponent Members

        /// <summary>
        /// Returns the name of this COM object.
        /// </summary>
        /// <returns>The name of this COM object.</returns>
        public string GetComponentDescription()
        {
            try
            {
                // Return the component description
                return _DEFAULT_OUTPUT_HANDLER_NAME;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26178", ex);
                throw new ExtractException("ELI26179", ee.AsStringizedByteStream());
            }
        }

        #endregion

        #region IPersistStream Members

        /// <summary>
        /// Returns the class ID for this object.
        /// </summary>
        /// <param name="classID"></param>
        public void GetClassID(out Guid classID)
        {
            try
            {
                classID = this.GetType().GUID;
            }
            catch(Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26180", ex);
                throw new ExtractException("ELI26181", ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Returns whether this object is dirty or not.
        /// </summary>
        /// <returns>Whether this object is dirty or not.</returns>
        public int IsDirty()
        {
            try
            {
                return _dirty ? _S_OK : _S_FALSE;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26182", ex);
                throw new ExtractException("ELI26183", ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Loads this object from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                ExtractException.Assert("ELI26184", "Stream is null!", stream != null);

                // Get the size of data stream to load
                byte[] dataLengthBuffer = new Byte[4];
                stream.Read(dataLengthBuffer, dataLengthBuffer.Length, IntPtr.Zero);
                int dataLength = BitConverter.ToInt32(dataLengthBuffer, 0);

                // Read the data from the provided stream into a buffer
                byte[] dataBuffer = new byte[dataLength];
                stream.Read(dataBuffer, dataLength, IntPtr.Zero);

                // Read the settings from the buffer; 
                // Create a memory stream and binary formatter to deserialize the settings.
                using (MemoryStream memoryStream = new MemoryStream(dataBuffer))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    // Read the version of the object being loaded.
                    int version = (int)binaryFormatter.Deserialize(memoryStream);
                    ExtractException.Assert("ELI26185", "Unable to load newer data entry task!",
                        version <= _CURRENT_VERSION);

                    // Read the database file name from the stream
                    _databaseFile = (string)binaryFormatter.Deserialize(memoryStream);
                }

                // False since a new object was just loaded
                _dirty = false;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26186", ex);
                throw new ExtractException("ELI26187", ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Saves this object to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to save to.</param>
        /// <param name="clearDirty">If <see langword="true"/> will clear the dirty flag.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                ExtractException.Assert("ELI26188", "Stream is null!", stream != null);

                // Create a memory stream and binary formatter to serialize the settings.
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    // Write the version of the object being saved.
                    binaryFormatter.Serialize(memoryStream, _CURRENT_VERSION);

                    // Save the settings to the memory stream
                    binaryFormatter.Serialize(memoryStream, _databaseFile ?? "");

                    // Write the memory stream to the provided IStream.
                    byte[] dataBuffer = memoryStream.ToArray();
                    byte[] dataLengthBuffer = BitConverter.GetBytes(dataBuffer.Length);
                    stream.Write(dataLengthBuffer, dataLengthBuffer.Length, IntPtr.Zero);
                    stream.Write(dataBuffer, dataBuffer.Length, IntPtr.Zero);

                    if (clearDirty)
                    {
                        _dirty = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26189", ex);
                throw new ExtractException("ELI26190", ee.AsStringizedByteStream());
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// <para>NOTE: Not implemented.</para>
        /// </summary>
        /// <param name="size">Will always be E_NOTIMPL to indicate this method is not implemented.
        /// </param>
        public void GetSizeMax(out long size)
        {
            try
            {
                size = _E_NOTIMPL;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26191", ex);
                throw new ExtractException("ELI26192", ee.AsStringizedByteStream());
            }
        }

        #endregion

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to run the class as an <see cref="IOutputHandler"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was not successful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Display the configuration form
                using (LabDEOrderMapperConfigurationForm configureForm =
                    new LabDEOrderMapperConfigurationForm(_databaseFile))
                {
                    // If the user clicked OK then set the database file
                    if (configureForm.ShowDialog() == DialogResult.OK)
                    {
                        _databaseFile = configureForm.DatabaseFileName;
                        _dirty = true;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26193", ex);
                throw new ExtractException("ELI26194", ee.AsStringizedByteStream());
            }
        }

        #endregion

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the current object has been configured.
        /// Object is configured if the database file name has been set.
        /// </summary>
        /// <returns><see langword="true"/> if the database file name is not
        /// <see langword="null"/> or empty string and returns <see langword="false"/>
        /// otherwise.</returns>
        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrEmpty(_databaseFile);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26195", ex);
                throw new ExtractException("ELI26196", ee.AsStringizedByteStream());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID Output Handler" COM category.
        /// </summary>
        /// <param name="type">The <see langref="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        private static void RegisterFunction(Type type)
        {
            string keyName = @"\CLSID\{" + type.GUID.ToString() + @"}\Implemented Categories";

            using (RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(keyName, true))
            {
                if (registryKey != null)
                {
                    registryKey.CreateSubKey(_UCLID_OUTPUT_HANDLER_GUID);
                }
            }
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID Output Handler" COM category.
        /// </summary>
        /// <param name="type">The <see langref="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        private static void UnregisterFunction(Type type)
        {
            string keyName = @"\CLSID\{" + type.GUID.ToString() + @"}\Implemented Categories";

            using (RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(keyName, true))
            {
                if (registryKey != null)
                {
                    registryKey.DeleteSubKey(_UCLID_OUTPUT_HANDLER_GUID);
                }
            }
        }

        #endregion Private Methods
    }
}
