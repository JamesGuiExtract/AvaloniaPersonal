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

        #endregion Methods
    }
}
