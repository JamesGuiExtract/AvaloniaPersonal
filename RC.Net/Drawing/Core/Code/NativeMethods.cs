using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents entry points to unmanaged code.
    /// </summary>
    internal static class NativeMethods
    {
        #region P/Invokes

        /// <summary>
        /// Creates a logical brush that has the specified solid color.
        /// </summary>
        /// <param name="color">The color of the brush.</param>
        /// <returns>If the function succeeds, the return value identifies a logical brush. If the 
        /// function fails, the return value is <see cref="IntPtr.Zero"/>. 
        /// </returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms532387.aspx">CreateSolidBrush
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern SafeGdiHandle CreateSolidBrush(int color);

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
        public static extern RasterDrawMode SetROP2(IntPtr deviceContext,
            RasterDrawMode drawMode);

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
        public static extern bool FillRgn(IntPtr deviceContext, IntPtr region, SafeGdiHandle brush);

        /// <summary>
        /// Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system 
        /// resources associated with the object.
        /// </summary>
        /// <param name="obj">Handle to a logical pen, brush, font, bitmap, region, or palette.
        /// </param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the 
        /// specified handle is not valid or is currently selected into a device context, the 
        /// return value is <see langword="false"/>.
        /// </returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms533225.aspx">DeleteObject
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr obj);

        #endregion P/Invokes

        #region Methods

        /// <summary>
        /// Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system 
        /// resources associated with the object.
        /// </summary>
        /// <param name="handle">The handle to free.</param>
        public static void ReleaseGdiHandle(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                bool success = DeleteObject(handle);
                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        #endregion Methods
    }
}
