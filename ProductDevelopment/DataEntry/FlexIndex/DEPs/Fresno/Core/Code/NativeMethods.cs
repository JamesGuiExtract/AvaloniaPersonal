using System;
using System.Runtime.InteropServices;

namespace Extract.DataEntry.DEP.Fresno
{
    internal static class NativeMethods
    {
        /// <summary>
        /// Hides the caret.
        /// </summary>
        /// <param name="hWnd">The handle for the control in which the caret should be hidden.
        /// </param>
        /// <returns><see langword="true"/> if the method succeeded; otherwise,
        /// <see langword="true"/>.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool HideCaret(IntPtr hWnd);
    }
}
