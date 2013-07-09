using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Extract.Interfaces
{
    /// <summary>
    /// Allows memory of unmanged COM objects to be managed by garbage collection.
    /// </summary>
    [ComVisible(true)]
    [Guid("9174266E-E37C-4E5B-8480-797CE460107F")]
    [CLSCompliant(false)]
    public interface IMemoryManager
    {
        /// <summary>
        /// Reports current unmanaged memory usage by a specific COM object instance.
        /// NOTE: When used, the calling COM object needs to be sure to call this method with
        /// bytesInUse = 0 when the object is destroyed to indicate it is no longer using any
        /// memory. Otherwise, the garbage collector will believe it needs to release more memory
        /// that it actually needs to thereby causing poor performance.
        /// </summary>
        /// <param name="bytesInUse">The bytes currently being used by the COM object.</param>
        void ReportUnmanagedMemoryUsage(int bytesInUse);
    }
}
