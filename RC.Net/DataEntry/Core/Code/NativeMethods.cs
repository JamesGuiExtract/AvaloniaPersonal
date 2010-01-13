using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    internal static class NativeMethods
    {
        #region P/Invoke statements

        /// <summary>
        /// The GetParent function retrieves a handle to the specified window's parent or owner.
        /// </summary>
        /// <param name="hWnd">Handle to the window whose parent window handle is to be retrieved.
        /// </param>
        /// <returns>If the window is a child window, the return value is a handle to the parent
        /// window. If the window is a top-level window, the return value is a handle to the owner
        /// window. If the window is a top-level unowned window or if the function fails, the return
        /// value is NULL.</returns>
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        /// <summary>
        /// Creates a new shape for the system caret and assigns ownership of the caret to the 
        /// specified window.
        /// </summary>
        /// <param name="hWnd">Handle to the window that owns the caret.</param>
        /// <param name="hBitmap">Handle to the bitmap that defines the caret shape. If this
        /// parameter is <see cref="IntPtr.Zero"/>, the caret is solid</param>
        /// <param name="nWidth">Specifies the width of the caret in logical units. If this
        /// parameter is zero, the width is set to the system-defined window border width.</param>
        /// <param name="nHeight">Specifies the height, in logical units, of the caret. If this
        /// parameter is zero, the height is set to the system-defined window border height.</param>
        /// <returns><see lanword="true"/> if the function succeeds, <see lanword="false"/>
        /// otherwise.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        /// <summary>
        /// Makes the caret visible on the screen at the caret's current position.
        /// </summary>
        /// <param name="hWnd">Handle to the window that owns the caret. If this parameter is
        /// <see cref="IntPtr.Zero"/>, ShowCaret searches the current task for the window that
        /// owns the caret.</param>
        /// <returns><see lanword="true"/> if the function succeeds, <see lanword="false"/>
        /// otherwise.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowCaret(IntPtr hWnd);

        #endregion P/Invoke statements

        #region Methods

        /// <summary>
        /// Retrieves a handle to the specified window's parent or owner.
        /// </summary>
        /// <param name="hWnd">Handle to the window whose parent window handle is to be retrieved.
        /// </param>
        /// <returns>If the window is a child window, the return value is a handle to the parent
        /// window. If the window is a top-level window, the return value is a handle to the owner
        /// window. If the window is a top-level unowned window or if the function fails, the return
        /// value is NULL.</returns>
        public static IntPtr GetParentWindowHandle(IntPtr hWnd)
        {
            try
            {
                return GetParent(hWnd);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26075", ex);
            }
        }

        /// <summary>
        /// Displays the caret in the specified control.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> which should display a caret.</param>
        public static void ShowCaret(Control control)
        {
            try
            {
                Size textSize = TextRenderer.MeasureText("X", control.Font);
                if (!CreateCaret(control.Handle, IntPtr.Zero, 0, textSize.Height))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (!ShowCaret(control.Handle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28832", ex);
            }
        }

        #endregion Methods
    }
}
