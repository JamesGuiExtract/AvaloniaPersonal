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
        #region NativeMethods P/Invokes

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

        #endregion NativeMethods P/Invokes

        #region NativeMethods Methods

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

        #endregion NativeMethods Methods
    }
}
