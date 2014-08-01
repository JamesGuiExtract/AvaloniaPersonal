using Extract.Interop;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;

namespace Extract.Utilities
{
    /// <summary>
    /// Implementation for the <see cref="IIdentifiableObject"/> to be extended by all
    /// objects that need to be uniquely identifiable.
    /// </summary>
    [ComVisible(true)]
    [Guid("D18ED23E-8D91-4344-9F09-BBCBDBEBC4CE")]
    [CLSCompliant(false)]
    public class IdentifiableObject : IIdentifiableObject
    {
        #region Constants

        /// <summary>
        /// The description.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Identifiable object";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.ExtractCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="Guid"/> identifying a particular instance of an object.
        /// </summary>
        Guid? _guid; 

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifiableObject"/> class.
        /// </summary>
        public IdentifiableObject()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="Guid"/> identifying a particular instance of an object. An 
        /// instance is considered a unique persistence of an object of which there may be multiple
        /// copies in memory if the object has been loaded from different callers.
        /// </summary>
        public Guid InstanceGUID
        {
            get
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33596", _COMPONENT_DESCRIPTION);

                if (_guid == null)
                {
                    _guid = Guid.NewGuid();
                }

                return _guid.Value;
            }
        }

        #endregion Properties

        #region Protected Members

        /// <summary>
        /// Loads the <see cref="InstanceGUID"/> from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the <see cref="InstanceGUID"/> should be loaded.
        /// </param>
        public void LoadGuid(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _guid = Guid.Parse(reader.ReadString());
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33594",
                    "Failed to load identifiable rule object GUID.");
            }
        }

        /// <summary>
        /// Saves the <see cref="InstanceGUID"/> into the specified IStream.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        public void SaveGuid(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(InstanceGUID.ToString());

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33595",
                    "Failed to save to load identifiable rule object GUID.");
            }
        }

        #endregion Protected Members
    }
}
