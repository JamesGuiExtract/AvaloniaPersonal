using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

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
        /// Retrieves a handle to the top-level window whose class name match the specified string.
        /// This function does not search child windows. This function does not perform a
        /// case-sensitive search. This function is to be used to search for windows by class only.
        /// </summary>
        /// <param name="lpClassName">Pointer to a null-terminated string that specifies the class
        /// name.</param>
        /// <param name="zeroOnly">Must be <see cref="IntPtr.Zero"/>.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpClassName, IntPtr zeroOnly);

        /// <summary>
        /// Retrieves information about the specified window.
        /// </summary>
        /// <param name="hWnd">Handle to the window and, indirectly, the class to which the window
        /// belongs.</param>
        /// <param name="nIndex">Specifies the zero-based offset to the value to be retrieved.
        /// </param>
        /// <returns>If the function succeeds, the return value is the requested 32-bit value. If
        /// the function fails, the return value is zero.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        #endregion P/Invoke statements

        #region Constants

        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;

        const uint WS_VISIBLE = 0x10000000;
        const uint WS_EX_TOPMOST = 0x0008;

        #endregion Constants

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
        /// Checks whether a .Net auto-complete list is displayed.
        /// </summary>
        /// <returns><see langword="true"/> if an auto-complete list is displayed;
        /// <see langword="false"/> otherwise.</returns>
        public static bool IsAutoCompleteDisplayed()
        {
            try
            {
                // Look for a top-level Auto-Suggest window
                IntPtr handle = FindWindow("Auto-Suggest Dropdown", IntPtr.Zero);
                if (handle != IntPtr.Zero)
                {
                    // If such a window is found and it is both visible and top-most, it is
                    // displayed.
                    if ((GetWindowLong(handle, GWL_STYLE) & WS_VISIBLE) != 0 &&
                        (GetWindowLong(handle, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27647", ex);
            }
        }

        #endregion Methods
    }
}
