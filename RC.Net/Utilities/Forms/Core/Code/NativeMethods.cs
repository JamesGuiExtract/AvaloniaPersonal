using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Extract.Utilities.Forms
{
    internal static class NativeMethods
    {
        #region P/Invoke statements

        /// <summary>
        /// Creates a cursor based on data contained in a file.
        /// </summary>
        /// <remarks>This is needed because the .NET Framework does not support loading color 
        /// cursors from a file.</remarks>
        /// <param name="lpFileName">Specifies the fully qualified path of the file data to be 
        /// used to create the cursor. The data in the file must be in either .CUR or .ANI format.
        /// </param>
        /// <returns>If the function is successful, the handle to the new cursor. If the function 
        /// fails, the return value is <see cref="IntPtr.Zero"/>.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms648392(VS.85).aspx">
        /// LoadCursorFromFile</seealso>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static public extern IntPtr LoadCursorFromFile(string lpFileName);

        /// <summary>
        /// Gets the window rectangle associated with the provided window handle.
        /// </summary>
        /// <param name="hWnd">Window handle to get the rectangle for.</param>
        /// <param name="rect">The struct that will hold the rectangle data.</param>
        /// <returns><see langword="true"/> if the function is successful and
        /// <see langword="false"/> otherwise.
        /// <para><b>Note:</b></para>
        /// If the function returns <see langword="false"/> it will also set the
        /// last error flag.  You can create a new <see cref="Win32Exception"/>
        /// with the return value of <see cref="Marshal.GetLastWin32Error"/> to
        /// get the extended error information.</returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref WindowsRectangle rect);

        /// <summary>
        /// Will send a system beep message.
        /// </summary>
        /// <param name="type">The type of beep to perform.</param>
        /// <returns><see langword="true"/> if the function is successful and
        /// <see langword="false"/> otherwise.
        /// <para><b>Note:</b></para>
        /// If the function returns <see langword="false"/> it will also set the
        /// last error flag.  You can create a new <see cref="Win32Exception"/>
        /// with the return value of <see cref="Marshal.GetLastWin32Error"/> to
        /// get the extended error information.</returns>
		[DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool MessageBeep(uint type);

        /// <summary>
        /// Gets the non-client metrics system parameter information.
        /// </summary>
        /// <param name="uiAction">The system parameter to be retrieved or set.</param>
        /// <param name="uiParam">A parameter whose usage and format depends on the system
        /// parameter being queried or set.</param>
        /// <param name="ncMetrics">A parameter whose usage and format depends on the system
        /// parameter being queried or set.</param>
        /// <param name="fWinIni">If a system parameter is being set, specifies whether
        /// the user profile is being updated, and if so, whether the
        /// WM_SETTINGCHANGE message is to be broadcast to all top level windows to
        /// notify them of the change.</param>
        /// <returns><see langword="true"/> if the function is successful and
        /// <see langword="false"/> otherwise.
        /// <para><b>Note:</b></para>
        /// If the function returns <see langword="false"/> it will also set the
        /// last error flag.  You can create a new <see cref="Win32Exception"/>
        /// with the return value of <see cref="Marshal.GetLastWin32Error"/> to
        /// get the extended error information.</returns>
		[DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SystemParametersInfo(int uiAction, int uiParam,
			ref NONCLIENTMETRICS ncMetrics, int fWinIni);

        /// <summary>
        /// Gets the system menu from the specified window.
        /// </summary>
        /// <param name="hWnd">The handle for the window.</param>
        /// <param name="bRevert">Whether to reset the menu back to default state.</param>
        // This function will not set the last error flag, no need to specify SetLastError
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd,
            [MarshalAs(UnmanagedType.Bool)] bool bRevert);

        /// <summary>
        /// Enables/Disables the specified menu item.
        /// </summary>
        /// <param name="hMenu">Handle to the menu.</param>
        /// <param name="uIDEnableItem">The menu item to enable/disable.</param>
        /// <param name="uEnable">Whether to enable/disable/gray the specified menu item.</param>
        // This function will not set the last error flag, no need to specify SetLastError
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the
        /// window procedure for the specified window and does not return until the window procedure
        /// has processed the message. 
        /// </summary>
        /// <param name="hWnd">Handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST, the message is sent to all top-level windows in the
        /// system, including disabled or invisible unowned windows, overlapped windows, and pop-up
        /// windows; but the message is not sent to child windows.</param>
        /// <param name="msg">Specifies the message to be sent.</param>
        /// <param name="wParam">Specifies additional message-specific information.</param>
        /// <param name="lParam">Specifies additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing and depends on 
        /// the message sent.</returns>
        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Maps a virtual-key code into a scan code or character value, or translates a scan code
        /// into a virtual-key code.
        /// </summary>
        /// <param name="uCode">Specifies the virtual-key code or scan code for a key. How this
        /// value is interpreted depends on the value of the uMapType parameter.</param>
        /// <param name="uMapType">Specifies the translation to perform.</param>
        /// <returns>The return value is either a scan code, a virtual-key code, or a character
        /// value, depending on the value of uCode and uMapType. If there is no translation, the
        /// return value is zero.
        /// </returns>
        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static uint MapVirtualKey(uint uCode, uint uMapType);

        #endregion

        #region constants

        private const int SPI_GETNONCLIENTMETRICS = 41;
		private const int LF_FACESIZE = 32;
		

		private const int SC_CLOSE  = 0xF060;
		private const int MF_BYCOMMAND  = 0x0;
		private const int MF_GRAYED  = 0x1;
		private const int MF_ENABLED  = 0x0;

        private const int WM_SETREDRAW = 0x000B;

        private const uint MAPVK_VK_TO_CHAR = 0x02;

        #endregion

        #region structs

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowsRectangle
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		private struct LOGFONT
		{ 
			public int lfHeight; 
			public int lfWidth; 
			public int lfEscapement; 
			public int lfOrientation; 
			public int lfWeight; 
			public byte lfItalic; 
			public byte lfUnderline; 
			public byte lfStrikeOut; 
			public byte lfCharSet; 
			public byte lfOutPrecision; 
			public byte lfClipPrecision; 
			public byte lfQuality; 
			public byte lfPitchAndFamily; 
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string lfFaceSize;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		private struct NONCLIENTMETRICS
		{
			public int cbSize;
			public int iBorderWidth;
			public int iScrollWidth;
			public int iScrollHeight;
			public int iCaptionWidth;
			public int iCaptionHeight;
			public LOGFONT lfCaptionFont;
			public int iSmCaptionWidth;
			public int iSmCaptionHeight;
			public LOGFONT lfSmCaptionFont;
			public int iMenuWidth;
			public int iMenuHeight;
			public LOGFONT lfMenuFont;
			public LOGFONT lfStatusFont;
			public LOGFONT lfMessageFont;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the rectangle containing the specified window in screen coordinates.
        /// </summary>
        /// <param name="window">The window to get the rectangle for. May not
        /// be <see langword="null"/>.</param>
        /// <returns>A <see cref="Rectangle"/> representing the location and
        /// the bounds of the specified window in screen coordinates.</returns>
        public static Rectangle GetWindowScreenRectangle(IWin32Window window)
        {
            try
            {
                // Ensure that the window object is not null
                ExtractException.Assert("ELI21970", "Window object is null!",
                    window != null);

                // Declare a new windows rectangle to hold the return data
                WindowsRectangle windowRectangle = new WindowsRectangle();

                // Call the win32API GetWindowRect function
                if (!GetWindowRect(window.Handle, ref windowRectangle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Return a Rectangle containing the window position and size
                return new Rectangle(windowRectangle.left, windowRectangle.top,
                    windowRectangle.right - windowRectangle.left,
                    windowRectangle.bottom - windowRectangle.top);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI21968",
                    "Unable to get window rectangle", ex);
            }
        }

        /// <summary>Get the default system font to display in the caption.
        /// </summary>
        /// <returns>Either the default <see cref="Font"/> from the system, or
        /// <see langword="null"> if unable to determine font from system.</see></returns>
		public static Font GetCaptionFont()
		{
			try
			{
    			NONCLIENTMETRICS ncm = new NONCLIENTMETRICS();
    			ncm.cbSize = Marshal.SizeOf(typeof(NONCLIENTMETRICS));

				if (!SystemParametersInfo(SPI_GETNONCLIENTMETRICS, ncm.cbSize, ref ncm, 0))
				{
                    throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				return Font.FromLogFont(ncm.lfCaptionFont);
			}
			catch(Exception ex)
			{
                ExtractException ee = new ExtractException("ELI21684",
                    "Unable to get caption font!", ex);
                ee.Log();
			}
			
			return null;
		}

        /// <summary>
        /// Will disable the close button on the message box form.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> on which to disable the close button.</param>
		public static void DisableCloseButton(Form form)
		{
			try
			{
				EnableMenuItem(GetSystemMenu(form.Handle, false),
                    SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
			}
			catch(Exception ex)
			{
                throw new ExtractException("ELI21692", "Unable to disable close button!", ex);
			}
        }

        /// <summary>
        /// Prevents the specified <see cref="Control"/> from updating (redrawing) until the lock 
        /// is released.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> for which updating is to be locked/
        /// unlocked.</param>
        /// <param name="lockUpdate"><see langword="true"/> to lock the
        /// <see cref="Control"/>from updating or <see langword="false"/> to release the lock and
        /// allow updates again.</param>
        public static void LockControlUpdate(Control control, bool lockUpdate)
        {
            try
            {
                SendMessage(control.Handle, WM_SETREDRAW, (IntPtr)(lockUpdate ? 0 : 1), IntPtr.Zero);
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI25202", ex);
            }
        }

        /// <summary>
        /// Maps a virtual-key code into a character value.
        /// </summary>
        /// <param name="uCode">Specifies the virtual-key code for a key.</param>
        /// <returns>A character value. If there is no translation, the return value is
        /// <see langword="null"/>.</returns>
        public static char? VirtualKeyToChar(int uCode)
        {
            try
            {
                // Attempt a map from virtual key code to character value.
                uint keyValue = MapVirtualKey((uint)uCode, MAPVK_VK_TO_CHAR);
                if (keyValue != 0)
                {
                    // If successful, convert the mapped value to a character
                    return Convert.ToChar(keyValue);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25607", ex);
            }
        }

        #endregion
    }
}
