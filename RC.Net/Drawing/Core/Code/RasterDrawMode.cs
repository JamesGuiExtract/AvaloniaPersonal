using System.Diagnostics.CodeAnalysis;

namespace Extract.Drawing
{
    /// <summary>
    /// Binary raster operations defined by the Windows graphics device interface (GDI).
    /// </summary>
    /// <seealso href="http://msdn.microsoft.com/en-us/library/ms534907(VS.85).aspx">
    /// BinaryRasterOperations</seealso>
    // This enum is defined in Win32
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum RasterDrawMode
    {
        /// <summary>
        /// Pixel is always 0.
        /// </summary>
        Black = 1,

        /// <summary>
        /// Pixel is the inverse of the <see cref="MergePen"/> color.
        /// </summary>
        NotMergePen = 2,

        /// <summary>
        /// Pixel is a combination of the colors common to
        /// both the screen and the inverse of the pen.
        /// </summary>
        MaskNotPen = 3,

        /// <summary>
        /// Pixel is the inverse of the pen color.
        /// </summary>
        NotCopyPen = 4,

        /// <summary>
        /// Pixel is a combination of the colors common to 
        /// both the pen and the inverse of the screen.
        /// </summary>
        MaskPenNot = 5,

        /// <summary>
        /// Pixel is the inverse of the screen color.
        /// </summary>
        Not = 6,

        /// <summary>
        /// Pixel is a combination of the colors in the pen and in the screen, but not in both.
        /// </summary>
        XorPen = 7,

        /// <summary>
        /// Pixel is the inverse of the <see cref="MaskPen"/> color.
        /// </summary>
        NotMaskPen = 8,

        /// <summary>
        /// Pixel is a combination of the colors common to both the pen and the screen.
        /// </summary>
        MaskPen = 9,

        /// <summary>
        /// Pixel is the inverse of the <see cref="XorPen"/> color.
        /// </summary>
        NotXorPen = 10,

        /// <summary>
        /// Pixel remains unchanged.
        /// </summary>
        None = 11,

        /// <summary>
        /// Pixel is a combination of the screen color and the inverse of the pen color.
        /// </summary>
        MergeNotPen = 12,

        /// <summary>
        /// Pixel is the pen color.
        /// </summary>
        CopyPen = 13,

        /// <summary>
        /// Pixel is a combination of the pen color and the inverse of the screen color.
        /// </summary>
        MergePenNot = 14,

        /// <summary>
        /// Pixel is a combination of the pen color and the screen color.
        /// </summary>
        MergePen = 15,

        /// <summary>
        /// Pixel is always 1.
        /// </summary>
        White = 16,
    }
}
