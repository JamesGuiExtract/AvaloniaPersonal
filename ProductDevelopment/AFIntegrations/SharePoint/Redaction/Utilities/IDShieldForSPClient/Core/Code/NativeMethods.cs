using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace Extract.SharePoint.Redaction.Utilities
{
    internal static class NativeMethods
    {
        #region Constants

        // constants for use with ShowWindow
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_RESTORE = 9;
    
        #endregion

        #region P/Invoke statements

        /// <summary>
        /// Imports the Windows ShowWindow function
        /// </summary>
        /// <param name="hWnd">Window handle for the window to show</param>
        /// <param name="nCmdShow">Show window constant </param>
        /// <returns>Returns true if window was previously visible 
        /// and returns false if it was not previously visible</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Import of the Windows SetForegroundWindow function
        /// </summary>
        /// <param name="hWnd">Window handle for the window to bring to the front</param>
        /// <returns>Returns true if window was brought to the foreground 
        /// and returns false if window was not brought to the front</returns>
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion
    }
}
