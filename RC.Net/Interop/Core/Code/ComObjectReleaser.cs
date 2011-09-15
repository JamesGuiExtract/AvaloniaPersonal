using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Extract.Licensing;

namespace Extract.Interop
{
    /// <summary>
    /// Allow for COM objects accessed via RCWs to be deterministically released.
    /// </summary>
    public class ComObjectReleaser : IDisposable
    {
        #region Constants

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.ExtractCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The COM objects that will be released by this instance when disposed.
        /// </summary>
        ConcurrentBag<object> _managedObjects = new ConcurrentBag<object>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ComObjectReleaser"/> class.
        /// </summary>
        public ComObjectReleaser()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33669", "ComObjectReleaser");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33670");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Specifies one or more COM objects that will be released by this instance when disposed.
        /// </summary>
        /// <param name="objectsToManage">The objects to manage.</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "objects")]
        public void ManageObjects(params object[] objectsToManage)
        {
            try
            {
                foreach (object objectToManage in objectsToManage
                    .Where(objectToManage => objectToManage != null))
                {
                    ExtractException.Assert("ELI33668",
                        "Cannot manage object that is not a COM object.",
                        Marshal.IsComObject(objectToManage));

                    _managedObjects.Add(objectToManage);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33667");
            }
        }

        #endregion Methods
    
        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ComObjectReleaser"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ComObjectReleaser"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ComObjectReleaser"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources

                if (_managedObjects != null)
                {
                    try
                    {
                        foreach (object objectToRelease in _managedObjects)
                        {
                            Marshal.ReleaseComObject(objectToRelease);
                        }
                    }
                    // Ensure dispose doesn't throw an exception.
                    catch { }

                    _managedObjects = null;
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members
    }
}
