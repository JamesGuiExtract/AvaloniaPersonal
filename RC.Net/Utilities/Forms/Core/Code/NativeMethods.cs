using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    internal static class NativeMethods
    {
        #region Constants

        // Stop flashing
        const uint FLASHW_STOP = 0;
 
        // Flash the window title
        const uint FLASHW_CAPTION = 0x00000001;
 
        // Flash the taskbar button
        const uint FLASHW_TRAY = 0x00000002;

        // Flash the window title and taskbar button
        const uint FLASHW_ALL = FLASHW_CAPTION | FLASHW_TRAY;

        // Flash continuously
        const uint FLASHW_TIMER = 0x00000004;

        // Flash until the window comes to the foreground
        const uint FLASHW_TIMERNOFG = 0x0000000C;

        // Activates and displays the window. If the window is minimized or maximized, the system
        // restores it to its original size and position. An application should specify this flag
        // when restoring a minimized window.
        const int SW_RESTORE = 0x09;

        /// <summary>
        /// A value for the RemoveMessage parameter of the PeekMessage call.
        /// </summary>
        const uint PM_REMOVE = 1;

        #endregion Constants

        #region Structs

        /// <summary>
        /// Contains the flash status for a window and the number of times the system should flash
        /// the window.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        /// <summary>
        /// Represents a MSG struct used by the PeekMessage, TranslateMessage and DispatchMessage
        /// Windows API calls.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct NativeMessage
        {
            public IntPtr handle;
            public int msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        #endregion Structs

        #region P/Invokes

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
        public static extern IntPtr LoadCursorFromFile(string lpFileName);

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
        static extern bool GetWindowRect(IntPtr hWnd, ref WindowsRectangle rect);

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
        static extern bool SystemParametersInfo(int uiAction, int uiParam, 
            ref NONCLIENTMETRICS ncMetrics, int fWinIni);

        /// <summary>
        /// Gets the system menu from the specified window.
        /// </summary>
        /// <param name="hWnd">The handle for the window.</param>
        /// <param name="bRevert">Whether to reset the menu back to default state.</param>
        // This function will not set the last error flag, no need to specify SetLastError
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, 
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
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

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
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

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
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        /// <summary>
        /// Maps a virtual-key code into a scan code or character value, or translates a scan code
        /// into a virtual-key code. The function translates the codes using the input language and
        /// an input locale identifier.
        /// </summary>
        /// <param name="uCode">Specifies the virtual-key code or scan code for a key. How this
        /// value is interpreted depends on the value of the uMapType parameter.</param>
        /// <param name="uMapType">Specifies the translation to perform.</param>
        /// <param name="dwhkl">(optional) Input locale identifier to use for translating the
        /// specified code.</param>
        /// <returns>The return value is either a scan code, a virtual-key code, or a character
        /// value, depending on the value of uCode and uMapType. If there is no translation, the
        /// return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        /// <summary>
        /// Determines whether a key is up or down at the time the function is called, and whether
        /// the key was pressed after a previous call to GetAsyncKeyState.
        /// </summary>
        /// <param name="vKey">The virtual-key code. For more information, see Virtual Key Codes.
        /// You can use left- and right-distinguishing constants to specify certain keys.</param>
        /// <returns>If the most significant bit is set, the key is down, and if the least
        /// significant bit is set, the key was pressed after the previous call to
        /// GetAsyncKeyState. </returns>
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); 

        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created 
        /// the specified window and returns without waiting for the thread to process the message.
        /// </summary>
        /// <param name="hWnd">Handle to the window whose window procedure is to receive the 
        /// message.</param>
        /// <param name="Msg">The message to be posted.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns><see langword="true"/> if the function succeeds; <see langword="false"/> if 
        /// the function fails.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms644944%28VS.85%29.aspx"/>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Retrieves a handle to the top-level window whose class name match the specified string.
        /// This function does not search child windows. This function does not perform a
        /// case-sensitive search. This function is to be used to search for windows by class only.
        /// </summary>
        /// <param name="lpClassName">Pointer to a null-terminated string that specifies the class
        /// name.</param>
        /// <param name="zeroOnly">Must be <see cref="IntPtr.Zero"/>.</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
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

        /// <summary>
        /// Sets the specified event object to the signaled state.
        /// </summary>
        /// <param name="hEvent">The event handle to set.</param>
        /// <returns><see langword="true"/> if the function succeeds and
        /// <see langword="false"/> if the function fails.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetEvent(IntPtr hEvent);

        /// <summary>
        /// Causes a window to use a different set of visual style information than its class
        /// normally uses.
        /// </summary>
        /// <param name="hWnd">Handle to the window whose visual style information is to be
        /// changed.</param>
        /// <param name="pszSubAppName">A string that contains the application name to use in
        /// place of the calling application's name. If this parameter is NULL, the calling
        /// application's name is used.</param>
        /// <param name="pszSubIdList">A string that contains a semicolon-separated
        /// list of CLSID names to use in place of the actual list passed by the window's class.
        /// If this parameter is NULL, the ID list from the calling class is used.</param>
        /// <returns>0 if the call was successful.</returns>
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, String pszSubAppName, String pszSubIdList);

        /// <summary>
        /// Flashes the specified window. It does not change the active state of the window.
        /// </summary>
        /// <param name="pwfi">A pointer to a <see cref="FLASHWINFO"/> structure.</param>
        /// <returns>The return value specifies the window's state before the call to the
        /// FlashWindowEx function. If the window caption was drawn as active before the call, the
        /// return value is nonzero. Otherwise, the return value is zero.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        /// <summary>
        /// Sets the specified window's show state.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="nCmdShow">Controls how the window is to be shown.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Dispatches incoming sent messages, checks the thread message queue for a posted message,
        /// and retrieves the message (if any exist).
        /// </summary>
        /// <param name="lpMsg">A pointer to an NativeMessage structure that receives message
        /// information.</param>
        /// <param name="hWnd">A handle to the window whose messages are to be retrieved. The
        /// window must belong to the current thread. If hWnd is NULL, PeekMessage retrieves
        /// messages for any window that belongs to the current thread, and any messages on the
        /// current thread's message queue whose hwnd value is NULL</param>
        /// <param name="wMsgFilterMin">The value of the first message in the range of messages to
        /// be examined. If wMsgFilterMin and wMsgFilterMax are both zero, PeekMessage returns all
        /// available messages (that is, no range filtering is performed).</param>
        /// <param name="wMsgFilterMax">The value of the last message in the range of messages to
        /// be examined. If wMsgFilterMin and wMsgFilterMax are both zero, PeekMessage returns all
        /// available messages (that is, no range filtering is performed).</param>
        /// <param name="wRemoveMsg">Specifies how messages are to be handled.</param>
        /// <returns><see langword="true"/> if there was an available message; otherwise,
        /// <see langword="false"/>.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, uint wMsgFilterMin,
           uint wMsgFilterMax, uint wRemoveMsg);

        /// <summary>
        /// Translates virtual-key messages into character messages.
        /// </summary>
        /// <param name="lpMsg">A pointer to a NativeMessage structure that contains message
        /// information retrieved from the calling thread's message queue.</param>
        /// <returns><see langword="true"/> if the message was translated; otherwise,
        /// <see langword="false"/>.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TranslateMessage([In] ref NativeMessage lpMsg);

        /// <summary>
        /// Dispatches a message to a window procedure.
        /// </summary>
        /// <param name="lpmsg">A pointer to a NativeMessage structure that contains the message.
        /// </param>
        /// <returns>The return value specifies the value returned by the window procedure. Although
        /// its meaning depends on the message being dispatched, the return value generally is
        /// ignored.</returns>
        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage([In] ref NativeMessage lpmsg);

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
        internal static extern IntPtr GetParent(IntPtr hWnd);

        #endregion P/Invokes

        #region Constants

        const int _SPI_GETNONCLIENTMETRICS = 41;

        const int _MF_BYCOMMAND  = 0x0;
        const int _MF_GRAYED  = 0x1;

        const uint _MAPVK_VK_TO_VSC = 0x00;
        const uint _MAPVK_VSC_TO_VK = 0x01;
        const uint _MAPVK_VK_TO_CHAR = 0x02;
        const uint _MAPVK_VSC_TO_VK_EX = 0x03;
        const uint _MAPVK_VK_TO_VSC_EX = 0x04;

        const int _GWL_STYLE = -16;
        const int _GWL_EXSTYLE = -20;

        const uint _WS_VISIBLE = 0x10000000;
        const uint _WS_EX_TOPMOST = 0x0008;

        #endregion Constants

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        struct WindowsRectangle
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        struct LOGFONT
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
        struct NONCLIENTMETRICS
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

        #endregion Structs

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

                if (!SystemParametersInfo(_SPI_GETNONCLIENTMETRICS, ncm.cbSize, ref ncm, 0))
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
                EnableMenuItem(GetSystemMenu(form.Handle, false), SystemCommand.Close, 
                    _MF_BYCOMMAND | _MF_GRAYED);
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
                SendMessage(control.Handle, WindowsMessage.SetRedraw, (IntPtr)(lockUpdate ? 0 : 1), 
                    IntPtr.Zero);
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
                uint keyValue = MapVirtualKey((uint)uCode, _MAPVK_VK_TO_CHAR);
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

        /// <summary>
        /// Converts scan codes to virtual key codes and vice-versa.
        /// </summary>
        /// <param name="toScanCode"><see langword="true"/> to map a virtual key code to a scan code,
        /// <see langword="false"/> to map a scan code to a virtual key.</param>
        /// <param name="code">The code to map.</param>
        /// <param name="distinguishLeftRight"><see langword="true"/> to distinguish between left
        /// and right-hand keys where applicable, <see langword="false"/> to use the generic code.</param>
        /// <returns>The mapped code if found, 0 otherwise.</returns>
        public static uint ConvertKeyCode(uint code, bool toScanCode, bool distinguishLeftRight)
        {
            uint mapType;
            if (toScanCode)
            {
                if (System.Environment.OSVersion.Version.Major < 6)
                {
                    // OS's prior to Vista are not able to map a virtual key to an extended scan code.
                    mapType = _MAPVK_VK_TO_VSC;
                }
                else
                {
                    mapType = distinguishLeftRight ? _MAPVK_VK_TO_VSC_EX : _MAPVK_VK_TO_VSC;
                }
            }
            else
            {
                mapType = distinguishLeftRight ? _MAPVK_VSC_TO_VK_EX : _MAPVK_VSC_TO_VK;
            }

            return MapVirtualKeyEx(code, mapType, IntPtr.Zero);
        }

        /// <summary>
        /// Determines whether the specified <see paramref="key"/> is pressed.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value to test.</param>
        /// <returns>
        /// <see langword="true"/> if the <see paramref="key"/> is pressed; <see langword="false"/>
        /// otherwise.
        /// </returns>
        public static bool IsKeyPressed(Keys key)
        {
            short result = GetAsyncKeyState(key);
            return (result & 0x8000) != 0;
        }

        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created 
        /// the specified window and returns without waiting for the thread to process the message.
        /// </summary>
        /// <param name="handle">Handle to the window whose window procedure is to receive the 
        /// message.</param>
        /// <param name="message">The message to be posted.</param>
        public static void BeginSendMessageToHandle(Message message, IntPtr handle)
        {
            try
            {
                bool success = PostMessage(handle, message.Msg, message.WParam, message.LParam);
                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28891", ex);
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
                    uint style = GetWindowLong(handle, _GWL_STYLE);
                    int lastError = Marshal.GetLastWin32Error();
                    if (style == 0 && lastError != 0)
                    {
                        throw new Win32Exception(lastError);
                    }

                    uint extendedStyle = GetWindowLong(handle, _GWL_EXSTYLE);
                    lastError = Marshal.GetLastWin32Error();
                    if (extendedStyle == 0 && lastError != 0)
                    {
                        throw new Win32Exception(lastError);
                    }

                    if ((style & _WS_VISIBLE) != 0 && (extendedStyle & _WS_EX_TOPMOST) != 0)
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

        /// <summary>
        /// Sets the specified event handle to a signaled state.
        /// </summary>
        /// <param name="eventHandle">The event handle to set to a signaled state.</param>
        public static void SignalEvent(IntPtr eventHandle)
        {
            try
            {
                if (!SetEvent(eventHandle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30339", ex);
            }
        }

        /// <summary>
        /// Starts or stops the specified <see paramref="window"/>'s title bar and taskbar button from
        /// flashing.
        /// </summary>
        /// <param name="start"><see langword="true"/> to start flashing; <see langword="false"/> to
        /// stop flashing.</param>
        /// <param name="window">The <see cref="IWin32Window"/> that is to flash.</param>
        /// <param name="stopOnActivate"><see langword="true"/> to stop flashing when the window is
        /// activated (brought to the foreground).</param>
        public static void FlashWindow(IWin32Window window, bool start, bool stopOnActivate)
        {
            try
            {
                FLASHWINFO flashWindowInfo = new FLASHWINFO();
                flashWindowInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(flashWindowInfo));
                flashWindowInfo.hwnd = window.Handle;
                flashWindowInfo.dwFlags = (start ? FLASHW_ALL : FLASHW_STOP) |
                                          (stopOnActivate ? FLASHW_TIMERNOFG : 0);
                flashWindowInfo.uCount = UInt32.MaxValue;
                flashWindowInfo.dwTimeout = 0;

                FlashWindowEx(ref flashWindowInfo);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33219");
            }
        }

        /// <summary>
        /// Restores the specified <see paramref="Form"/> if it is currently minimized.
        /// </summary>
        /// <param name="form">The <see cref="Form"/> to restore.</param>
        public static void Restore(this Form form)
        {
            try
            {
                if (form.WindowState == FormWindowState.Minimized)
                {
                    if (ShowWindow(form.Handle, SW_RESTORE) == 0)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35794");
            }
        }

        /// <summary>
        /// Processes all Windows messages currently in the message queue except for
        /// <see paramref="messagesToIgnore"/>.
        /// </summary>
        /// <param name="messagesToIgnore">The window messages that should be ignored/discarded
        /// rather than processed.</param>
        public static void DoEventsExcept(HashSet<int> messagesToIgnore)
        {
            NativeMessage message;
            while (PeekMessage(out message, IntPtr.Zero, 0, 0, PM_REMOVE))
            {
                if (!messagesToIgnore.Contains(message.msg))
                {
                    TranslateMessage(ref message);
                    DispatchMessage(ref message);
                }
            }
        }

        #endregion Methods
    }
}
