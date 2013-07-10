using Extract.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;

namespace Extract.Interop
{
    /// <summary>
    /// Allows memory of unmanged COM objects to be managed by garbage collection. Each instance of
    /// this class shall be responsible for managing the memory of a single unmanaged COM object.
    /// </summary>
    [ComVisible(true)]
    [Guid("15AEDB68-2693-4754-97E1-4FC9B2A832FE")]
    [CLSCompliant(false)]
    public class MemoryManager : IMemoryManager
    {
        #region Fields

        /// <summary>
        /// The memory
        /// </summary>
        int _lastReportedMemoryUsage;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryManager"/> class.
        /// </summary>
        public MemoryManager()
        {
        }

        #endregion Constructors

        #region IMemoryManager Members

        /// <summary>
        /// Reports the unmanaged memory usage of the calling object.
        /// </summary>
        /// <param name="bytesInUse">The bytes of memory currently being used by the calling COM
        /// object.</param>
        public void ReportUnmanagedMemoryUsage(int bytesInUse)
        {
            try
            {
                int usageDifferential = bytesInUse - _lastReportedMemoryUsage;

                if (usageDifferential > 0)
                {
                    GC.AddMemoryPressure(usageDifferential);
                }
                else if (usageDifferential < 0)
                {
                    GC.RemoveMemoryPressure(-usageDifferential);
                }

                _lastReportedMemoryUsage = bytesInUse;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36017", "Error managing memory usage.");
            }
        }

        #endregion IMemoryManager Members

        #region Static Methods

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
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        [ComVisible(false)]
        public static void ReportComObjectMemoryUsage(object comObject)
        {
            try
            {
                IManageableMemory manageableMemoryObject = comObject as IManageableMemory;
                ExtractException.Assert("ELI36019", "COM object memory is not manageable.",
                    manageableMemoryObject != null);

                manageableMemoryObject.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36018");
            }
        }

        #endregion Static Methods
    }
}
