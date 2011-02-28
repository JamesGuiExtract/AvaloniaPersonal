using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging
{
    /// <summary>
    /// Constants used by projects that deal with images.
    /// </summary>
    public static class ImageConstants
    {
        /// <summary>
        /// Mutex name for the named PdfXpress mutex.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "PdfXpress")]
        public static readonly string PdfXpressMutex =
            @"Global\EC9E480D-BE75-4749-A5DF-2FD6583F8FAC";
    }
}
