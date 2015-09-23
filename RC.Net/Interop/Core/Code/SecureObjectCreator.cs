using Extract.Interfaces;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;

namespace Extract.Interop
{
    /// <summary>
    /// Used to create secure objects.
    /// </summary>
    [ComVisible(true)]
    [Guid("C49D5019-85A7-476E-AF49-39463580F804")]
    public class SecureObjectCreator : ISecureObjectCreator
    {
        /// <summary>
        /// A random number generator used to produce the value of <see cref="InstanceID"/> for each
        /// instance.
        /// </summary>
        static Random _random = new Random();

        /// <summary>
        /// Indicates whether the next instance should call
        /// <see cref="LicenseUtilities.InitRegisteredObjects"/>.
        /// </summary>
        static bool _initialize = true;

        /// <summary>
        /// The ID of this instance.
        /// </summary>
        int? _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureObjectCreator"/> class.
        /// </summary>
        public SecureObjectCreator()
        {
            try
            {
                if (_initialize)
                {
                    LicenseUtilities.InitRegisteredObjects(new MapLabel());
                }
                else
                {
                    _initialize = true;
                }

                _id = _random.Next();

                LicenseUtilities.RegisterObject(_id.Value, new MapLabel());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38733");
            }
        }

        /// <summary>
        /// Gets the ID of this instance.
        /// </summary>
        public int InstanceID
        {
            get
            {
                return _id.Value;
            }
        }

        /// <summary>
        /// Gets an instance of the COM class indicated by <see paramref="progId"/>.
        /// </summary>
        /// <param name="progId">The ProgID of the COM class to instantiate.</param>
        /// <returns>An instance of the COM class indicated by <see paramref="progId"/>.</returns>
        public object GetObject(string progId)
        {
            try
            {
                ExtractException.Assert("ELI38734", "ObjectVerifier instance cannot be reused.",
                    _id.HasValue);

                _initialize = false;
                _id = null;

                Type comType = Type.GetTypeFromProgID(progId);
                ExtractException.Assert("ELI38735", "Unable to verify assembly.",
                    LicenseUtilities.VerifyAssemblyData(comType.Assembly));

                return Activator.CreateInstance(comType);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI38736");
                ee.AddDebugData("ProgId", progId, true);
                throw ee.CreateComVisible("ELI38737", "Failed to generate verified object instance.");
            }
        }
    }
}
