using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides entry points to unmanaged code.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Binary raster operations defined by the 
        /// <see href="http://msdn.microsoft.com/en-us/library/ms536795(VS.85).aspx">Windows GDI
        /// </see>.
        /// </summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms534907(VS.85).aspx">
        /// BinaryRasterOperations</seealso>
        public enum BinaryRasterOperations : int
        {
            /// <summary>
            /// Pixel is always 0.
            /// </summary>
            R2_BLACK = 1,

            /// <summary>
            /// Pixel is the inverse of the R2_MERGEPEN color.
            /// </summary>
            R2_NOTMERGEPEN = 2,

            /// <summary>
            /// Pixel is a combination of the colors common to
            /// both the screen and the inverse of the pen.
            /// </summary>
            R2_MASKNOTPEN = 3,

            /// <summary>
            /// Pixel is the inverse of the pen color.
            /// </summary>
            R2_NOTCOPYPEN = 4,

            /// <summary>
            /// Pixel is a combination of the colors common to 
            /// both the pen and the inverse of the screen.
            /// </summary>
            R2_MASKPENNOT = 5,

            /// <summary>
            /// Pixel is the inverse of the screen color.
            /// </summary>
            R2_NOT = 6,

            /// <summary>
            /// Pixel is a combination of the colors in the pen and in the screen, but not in both.
            /// </summary>
            R2_XORPEN = 7,

            /// <summary>
            /// Pixel is the inverse of the R2_MASKPEN color.
            /// </summary>
            R2_NOTMASKPEN = 8,

            /// <summary>
            /// Pixel is a combination of the colors common to both the pen and the screen.
            /// </summary>
            R2_MASKPEN = 9,

            /// <summary>
            /// Pixel is the inverse of the R2_XORPEN color.
            /// </summary>
            R2_NOTXORPEN = 10,

            /// <summary>
            /// Pixel remains unchanged.
            /// </summary>
            R2_NOP = 11,

            /// <summary>
            /// Pixel is a combination of the screen color and the inverse of the pen color.
            /// </summary>
            R2_MERGENOTPEN = 12,

            /// <summary>
            /// Pixel is the pen color.
            /// </summary>
            R2_COPYPEN = 13,

            /// <summary>
            /// Pixel is a combination of the pen color and the inverse of the screen color.
            /// </summary>
            R2_MERGEPENNOT = 14,

            /// <summary>
            /// Pixel is a combination of the pen color and the screen color.
            /// </summary>
            R2_MERGEPEN = 15,

            /// <summary>
            /// Pixel is always 1.
            /// </summary>
            R2_WHITE = 16,

            /// <summary>
            /// Placeholder
            /// </summary>
            R2_LAST = 16          
        }

        /// <summary>
        /// Creates a logical brush that has the specified solid color.
        /// </summary>
        /// <param name="color">The color of the brush.</param>
        /// <returns>If the function succeeds, the return value identifies a logical brush. If the 
        /// function fails, the return value is <see cref="IntPtr.Zero"/>. 
        /// </returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms532387.aspx">CreateSolidBrush
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError=true)]
        public static extern IntPtr CreateSolidBrush(int color);

        /// <summary>
        /// Sets the current foreground mix mode.
        /// </summary>
        /// <param name="deviceContext">Handle to the device context.</param>
        /// <param name="drawMode">Specifies the mix mode.</param>
        /// <returns>If the function succeeds, the return value specifies the previous mix mode. 
        /// If the function fails, the return value is zero.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms534912.aspx">SetROP2
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern BinaryRasterOperations SetROP2(IntPtr deviceContext, 
            BinaryRasterOperations drawMode);

        /// <summary>
        /// Fills a region by using the specified brush.
        /// </summary>
        /// <param name="deviceContext">Handle to the device context.</param>
        /// <param name="region">Handle to the region to be filled in logical units.</param>
        /// <param name="brush">Handle to the brush to be used to fill the region.</param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the 
        /// function fails, the return value is <see langword="false"/>.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms536674(VS.85).aspx">FillRgn
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FillRgn(IntPtr deviceContext, IntPtr region, IntPtr brush);

        /// <summary>
        /// Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system 
        /// resources associated with the object.
        /// </summary>
        /// <param name="obj">Handle to a logical pen, brush, font, bitmap, region, or palette.
        /// </param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the 
        /// specified handle is not valid or is currently selected into a DC, the return value is 
        /// <see langword="false"/>.
        /// </returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms533225.aspx">DeleteObject
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr obj);
    }
}