using Extract.Interop;
using System;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Provides extension methods to allow memory usage of unmanaged COM object to be reported
    /// from managed .Net code.
    /// </summary>
    [CLSCompliant(false)]
    public static class MemoryManagerExtensionMethods
    {
        /// <summary>
        /// Reports the unmanaged memory usage of <see paramref="comObject"/> to the garbage
        /// collector.
        /// <para><b>Note</b></para>
        /// Calling this will cause memory of any IManageableMemory child objects to be reported as
        /// well.
        /// This method should be called:
        /// 1) Before any operations which may cause some previously existing IManageableMemory
        /// child objects to be unreferenced.
        /// 2) Before leaving the scope in which the object exists or returning it as a return value.
        /// </summary>
        /// <param name="comObject">The unmanaged COM object whose memory usage is to be reported.
        /// </param>
        public static void ReportMemoryUsage(this IAttribute comObject)
        {
            MemoryManager.ReportComObjectMemoryUsage(comObject);
        }

        /// <summary>
        /// Reports the unmanaged memory usage of <see paramref="comObject"/> to the garbage
        /// collector.
        /// <para><b>Note</b></para>
        /// Calling this will cause memory of any IManageableMemory child objects to be reported as
        /// well.
        /// This method should be called:
        /// 1) Before any operations which may cause some previously existing IManageableMemory
        /// child objects to be unreferenced.
        /// 2) Before leaving the scope in which the object exists or returning it as a return value.
        /// </summary>
        /// <param name="comObject">The unmanaged COM object whose memory usage is to be reported.
        /// </param>
        public static void ReportMemoryUsage(this IIUnknownVector comObject)
        {
            MemoryManager.ReportComObjectMemoryUsage(comObject);
        }

        /// <summary>
        /// Reports the unmanaged memory usage of <see paramref="comObject"/> to the garbage
        /// collector.
        /// <para><b>Note</b></para>
        /// Calling this will cause memory of any IManageableMemory child objects to be reported as
        /// well.
        /// This method should be called:
        /// 1) Before any operations which may cause some previously existing IManageableMemory
        /// child objects to be unreferenced.
        /// 2) Before leaving the scope in which the object exists or returning it as a return value.
        /// </summary>
        /// <param name="comObject">The unmanaged COM object whose memory usage is to be reported.
        /// </param>
        public static void ReportMemoryUsage(this SpatialString comObject)
        {
            MemoryManager.ReportComObjectMemoryUsage(comObject);
        }

        /// <summary>
        /// Reports the unmanaged memory usage of <see paramref="comObject"/> to the garbage
        /// collector.
        /// <para><b>Note</b></para>
        /// Calling this will cause memory of any IManageableMemory child objects to be reported as
        /// well.
        /// This method should be called:
        /// 1) Before any operations which may cause some previously existing IManageableMemory
        /// child objects to be unreferenced.
        /// 2) Before leaving the scope in which the object exists or returning it as a return value.
        /// </summary>
        /// <param name="comObject">The unmanaged COM object whose memory usage is to be reported.
        /// </param>
        public static void ReportMemoryUsage(this SpatialStringSearcher comObject)
        {
            MemoryManager.ReportComObjectMemoryUsage(comObject);
        }

        /// <summary>
        /// Reports the unmanaged memory usage of <see paramref="comObject"/> to the garbage
        /// collector.
        /// <para><b>Note</b></para>
        /// Calling this will cause memory of any IManageableMemory child objects to be reported as
        /// well.
        /// This method should be called:
        /// 1) Before any operations which may cause some previously existing IManageableMemory
        /// child objects to be unreferenced.
        /// 2) Before leaving the scope in which the object exists or returning it as a return value.
        /// </summary>
        /// <param name="comObject">The unmanaged COM object whose memory usage is to be reported.
        /// </param>
        public static void ReportMemoryUsage(this RasterZone comObject)
        {
            MemoryManager.ReportComObjectMemoryUsage(comObject);
        }
    }
}
