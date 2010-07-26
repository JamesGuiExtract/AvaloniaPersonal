using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a handle to a logical pen, brush, font, bitmap, region, or palette.
    /// </summary>
    public class SafeGdiHandle : SafeHandle
    {
        #region SafeGdiHandle Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeGdiHandle"/> class.
        /// </summary>
        SafeGdiHandle() 
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeGdiHandle"/> class.
        /// </summary>
        /// <param name="existingHandle">Represents the pre-existing handle to use.</param>
        /// <param name="ownsHandle"><see langword="true"/> to reliably release the handle during 
        /// the finalization phase; <see langword="false"/> to prevent reliable release (not 
        /// recommended).</param>
        public SafeGdiHandle(IntPtr existingHandle, bool ownsHandle) 
            : base (IntPtr.Zero, ownsHandle)
        {
            handle = existingHandle;
        }

        #endregion SafeGdiHandle Constructors

        #region SafeGdiHandle Overrides

        /// <summary>
        /// When overridden in a derived class, executes the code required to free the handle.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the handle is released successfully; otherwise, in the event 
        /// of a catastrophic failure, <see langword="false"/>.
        /// </returns>
        protected override bool ReleaseHandle()
        {
            try
            {
                NativeMethods.ReleaseGdiHandle(handle);

                return true;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27855",
                    "Unable to release GDI handle.", ex);
                ee.Log();
            }

            return false;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the handle value is invalid.
        /// </summary>
        /// <returns>
        /// true if the handle is valid; otherwise, false.
        /// </returns>
        public override bool IsInvalid
        {
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        #endregion SafeGdiHandle Overrides
    }
}
