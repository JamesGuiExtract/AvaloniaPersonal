using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// An IWin32Window implementation for an hWnd needs to be converted to a IWin32Window.
    /// </summary>
    public class WindowWrapper : IWin32Window
    {
        /// <summary>
        /// The window handle.
        /// </summary>
        IntPtr _handle;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowWrapper"/> class.
        /// </summary>
        /// <param name="handle">The window's handle.</param>
        public WindowWrapper(IntPtr handle)
        {
            _handle = handle;
        }

        #endregion Constructors

        #region IWin32Window Members

        /// <summary>
        /// Gets the window handle.
        /// </summary>
        /// <returns>The window handle.</returns>
        public IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }

        #endregion IWin32Window Members
    }
}
