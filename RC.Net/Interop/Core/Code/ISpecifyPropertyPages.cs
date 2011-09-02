using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Interop
{
    /// <summary>
    /// Specifies a counted array of UUID or GUID types used to receive an array of CLSIDs for the
    /// property pages that the object wants to display.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CAUUID")]
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct CAUUID
    {
        /// <summary>
        /// The size of the array pointed to by pElems.
        /// </summary>
        UInt32 cElems;
        
        /// <summary>
        /// A pointer to an array of values, each of which specifies a CLSID of a particular
        /// property page. This array is allocated by the callee using CoTaskMemAlloc and is freed
        /// by the caller using CoTaskMemFree.
        /// </summary>
        IntPtr pElems;
    }

    /// <summary>
    /// Indicates that an object supports property pages.
    /// </summary>
    [ComImport]
    [Guid("B196B28B-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [CLSCompliant(false)]
    public interface ISpecifyPropertyPages
    {
        /// <summary>
        /// Retrieves a list of property pages that can be displayed in this object's property
        /// sheet.
        /// </summary>
        /// <param name="pages">A pointer to a caller-allocated CAUUID structure that must be
        /// initialized and filled before returning. The pElems member in the structure is allocated
        /// by the callee with CoTaskMemAlloc and freed by the caller with CoTaskMemFree.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        void GetPages(out CAUUID pages);
    }
}
