using Extract;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice
{
    /// <summary>
    /// Provides entry points to unmanaged code.
    /// </summary>
    internal static class NativeMethods
    {
        #region NativeMethods Constants

        /// <summary>
        /// Specifies a set of Html Help commands.
        /// </summary>
        public enum HtmlHelpCommand
        {
            /// <summary>
            /// Opens a help topic in a specified help window. 
            /// </summary>
            HH_DISPLAY_TOPIC = 0x0000,

            /// <summary>
            /// Selects the Contents tab in the Navigation pane of the HTML Help Viewer. 
            /// </summary>
            HH_DISPLAY_TOC = 0x0001,

            /// <summary>
            /// Selects the Index tab in the Navigation pane of the HTML Help Viewer and searches 
            /// for the keyword specified in the dwData parameter.
            /// </summary>
            HH_DISPLAY_INDEX = 0x0002,

            /// <summary>
            /// Selects the Search tab in the Navigation pane of the HTML Help Viewer, but does 
            /// not actually perform a search.
            /// </summary>
            HH_DISPLAY_SEARCH = 0x0003,

            /// <summary>
            /// Creates a new help window or modifies an existing help window at run time.
            /// </summary>
            HH_SET_WIN_TYPE = 0x0004,

            /// <summary>
            /// Retrieves a pointer to the HH_WINTYPE structure associated with a specified window 
            /// type.
            /// </summary>
            HH_GET_WIN_TYPE = 0x0005,

            /// <summary>
            /// Returns the handle (hwnd) of a specified window type.
            /// </summary>
            HH_GET_WIN_HANDLE = 0x0006,

            /// <summary>
            /// Locates and selects the contents entry for the help topic that is open in the 
            /// Topic pane of the HTML Help Viewer.
            /// </summary>
            HH_SYNC = 0x0009,

            /// <summary>
            /// Looks up one or more keywords in a compiled help (.chm) file.
            /// </summary>
            HH_KEYWORD_LOOKUP = 0x000D,

            /// <summary>
            /// Opens a pop-up window that displays text.
            /// </summary>
            HH_DISPLAY_TEXT_POPUP = 0x000E,

            /// <summary>
            /// Displays a help topic based on a mapped topic ID.
            /// </summary>
            HH_HELP_CONTEXT = 0x000F,

            /// <summary>
            /// Opens a pop-up context menu.
            /// </summary>
            HH_TP_HELP_CONTEXTMENU = 0x0010,

            /// <summary>
            /// Opens a pop-up help topic.
            /// </summary>
            HH_TP_HELP_WM_HELP = 0x0011,

            /// <summary>
            /// Closes all windows opened directly or indirectly by the calling program.
            /// </summary>
            HH_CLOSE_ALL = 0x0012,

            /// <summary>
            /// Looks up one or more Associative link (ALink) names in a compiled help (.chm) file.
            /// </summary>
            HH_ALINK_LOOKUP = 0x0013
        }

        #endregion NativeMethods Constants

        #region NativeMethods Structs

        /// <summary>
        /// Represents a Win32 rectangle.
        /// </summary>
        struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Rectangle ToRectangle()
            {
                return Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }
        }

        /// <summary>
        /// The ASSOCF enum needed for AssocQueryString
        /// </summary>
        public enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        /// <summary>
        /// The ASSOCSTR enum needed for AssocQueryString
        /// </summary>
        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }

        /// <summary>
        /// Represents one or more associative link (ALink) names or keyword link (KLink) keywords 
        /// for which to search.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        class HtmlHelpLink
        {
            /// <summary>
            /// Size of the structure. Must always be filled in before passing the structure to 
            /// the HTML Help API. 
            /// </summary>
            int _cbStruct = Marshal.SizeOf(typeof(HtmlHelpLink));

            /// <summary>
            /// Must be <see langword="false"/>.
            /// </summary>
            bool _fReserved;

            /// <summary>
            /// One or more associative link (ALink) names or keyword link (KLink) keywords to 
            /// look up delimited by semicolons.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            string _pszKeywords;

            /// <summary>
            /// Topic file to navigate to if the lookup fails. Must be a valid topic within the 
            /// specified compiled help (.chm) file, not an internet url.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            string _pszUrl;

            /// <summary>
            /// Text to display in a message box if the lookup fails and 
            /// <see cref="_fIndexOnFail"/> is <see langword="false"/> and <see cref="_pszUrl"/> is 
            /// <see langword="null"/>. 
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            string _pszMsgText;

            /// <summary>
            /// Caption of the message box in which the <see cref="_pszMsgText"/> parameter appears. 
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            string _pszMsgTitle;

            /// <summary>
            /// Name of the window type in which to display.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            string _pszWindow;

            /// <summary>
            /// Display the keyword in the Index tab of the HTML Help Viewer if the lookup fails.
            /// </summary>
            bool _fIndexOnFail = true;

            /// <summary>
            /// Initializes a new instance of the <see cref="HtmlHelpLink"/> class.
            /// </summary>
            /// <param name="keywords">A semi-colon delimited list of keywords to look up.</param>
            public HtmlHelpLink(string keywords)
            {
                _pszKeywords = keywords;
            }
        }

        #endregion NativeMethods Structs

        #region NativeMethods P/Invoke Methods

        /// <summary>
        /// The HTML Help API has one function that displays a help window. Using the API 
        /// commands, you can specify which topic to display in the help window, whether the help 
        /// window is a three-pane Help Viewer or a pop-up window, and whether the HTML topic file 
        /// should be accessed via a context ID, an HTML Help URL, or a Keyword link (KLink) 
        /// lookup. 
        /// </summary>
        /// <param name="hwndCaller">The handle (hwnd) of the owner of the help window.</param>
        /// <param name="pszFile">Depending on the uCommand value, the file path to 
        /// either a compiled help (.chm) file or a topic file within a specified help file.
        /// </param>
        /// <param name="uCommand">The command to complete.</param>
        /// <param name="dwData">Any data that may be required, based on the value of the 
        /// <paramref name="uCommand"/> parameter.</param>
        /// <returns>The handle (hwnd) of the help window, or <see langword="IntPtr.Zero"/> if the 
        /// help window was not openned.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms669985.aspx">HTML Help 
        /// Downloads</seealso>
        [DllImport("hhctrl.ocx", CharSet=CharSet.Unicode)]
        static extern IntPtr HtmlHelp(IntPtr hwndCaller, string pszFile, uint uCommand, 
            HtmlHelpLink dwData);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The 
        /// dimensions are given in screen coordinates that are relative to the upper-left corner 
        /// of the screen.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="lpRect">The screen coordinates of the upper-left and lower-right corners 
        /// of the window.</param>
        /// <returns><see langword="true"/> if the function succeeds; <see langword="false"/> if 
        /// it fails.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms633519(VS.85).aspx"/>
        [DllImport("user32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        /// <summary>
        /// Changes the position and dimensions of the specified window. For a top-level window, 
        /// the position and dimensions are relative to the upper-left corner of the screen. For a 
        /// child window, they are relative to the upper-left corner of the parent window's client 
        /// area.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="X">The new position of the left side of the window.</param>
        /// <param name="Y">The new position of the top of the window.</param>
        /// <param name="nWidth">The new width of the window.</param>
        /// <param name="nHeight">The new height of the window.</param>
        /// <param name="bRepaint">Whether the window is to be repainted. If 
        /// <see langword="true"/>, the window receives a message. If <see langword="false"/>, no 
        /// repainting of any kind occurs.</param>
        /// <returns><see langword="true"/> if the function succeeds; <see langword="false"/> if 
        /// it fails.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms633534.aspx"/>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight,
            [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        /// <summary>
        /// Puts the thread that created the specified window into the foreground and activates 
        /// the window. Keyboard input is directed to the window and various visual cues are 
        /// changed for the user.
        /// </summary>
        /// <param name="hWnd">Handle to the window that should be activated and brought to the 
        /// foreground.</param>
        /// <returns><see langword="true"/> if the window was brought to the foreground; 
        /// <see langword="false"/> if the window was not brought to the foreground.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms633539(VS.85).aspx"/>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Searches for infomation about the application associated with the specified file name.
        /// </summary>
        /// <param name="flags">Flags that can be used to control the search.</param>
        /// <param name="str">Specifies the type of string that is to be returned.</param>
        /// <param name="pszAssoc">String that is used to determine the root key.</param>
        /// <param name="pszExtra">String with additional information about the location of the 
        /// string.</param>
        /// <param name="pszOut">String used to return the requested string. Set this parameter to 
        /// <see langword="null"/> to retrieve the required buffer size.</param>
        /// <param name="puiSize">Pointer to a value that is set to the number of characters 
        /// in the <paramref name="pszOut"/> buffer. When the function returns, it will be set to 
        /// the number of characters actually placed in the buffer.</param>
        /// <returns>0 for success, 1 if <paramref name="puiSize"/> contains the buffer size 
        /// required, or less than 0 for an error.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/bb773471.aspx"/>
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, 
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssoc, 
            [MarshalAs(UnmanagedType.LPWStr)] string pszExtra,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszOut,
            ref uint puiSize);

        #endregion NativeMethods P/Invoke Methods

        #region NativeMethods Methods

        /// <summary>
        /// Retrieves the name of the application associated with the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The name of the application associated with the file or "" if
        /// no such application exists.</returns>
        public static string GetAssociatedApplication(string fileName)
        {
            // Initialize the application name to empty.
            string applicationName = "";
            uint valueLength = 0;

            // Query how many characters are necessary to store the application name.
            if (AssocQueryString(AssocF.Verify, AssocStr.FriendlyAppName, fileName, null, null, ref valueLength) == 1)
            {
                 // Create a buffer to store the name of the application.
                 StringBuilder appNameBuffer = new StringBuilder((int)valueLength);

                 // Retrieve the name of the application.
                 if (AssocQueryString(AssocF.Verify, AssocStr.FriendlyAppName, fileName, null, 
                     appNameBuffer, ref valueLength) == 0)
                 { 
                     applicationName = appNameBuffer.ToString();
                 }
            }

            return applicationName;
        }

        /// <summary>
        /// Gets the dimensions of the bounding rectangle of the specified window. The 
        /// dimensions are given in screen coordinates that are relative to the upper-left corner 
        /// of the screen.
        /// </summary>
        /// <param name="handle">The handle of the window.</param>
        /// <returns>The dimensions of the bounding rectangle of the specified window in screen 
        /// coordinates.</returns>
        public static Rectangle GetScreenBoundsFromWindowHandle(IntPtr handle)
        {
            Rect rect;

            if (!GetWindowRect(handle, out rect))
            {
                throw new ExtractException("ELI23316", "Unable to get window bounds.",
                    new Win32Exception(Marshal.GetLastWin32Error()));
            }

            return rect.ToRectangle();
        }

        /// <summary>
        /// Changes the position and dimensions of the specified window. 
        /// </summary>
        /// <param name="handle">Handle to the window.</param>
        /// <param name="rectangle">The new dimensions of <paramref name="handle"/> in screen 
        /// coordinates for a top-level window or in client coordinates for a child window.</param>
        /// <param name="repaint"><see langword="true"/> if the window should repaint itself; 
        /// <see langword="false"/> if the window should not repaint itself.</param>
        public static void SetWindowBounds(IntPtr handle, Rectangle rectangle, bool repaint)
        {
            if (!MoveWindow(handle, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, 
                repaint))
            {
                throw new ExtractException("ELI23317", "Unable to move window.",
                    new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        /// <summary>
        /// Opens the help file to the topic with the specified index entry, if one exists; 
        /// otherwise, the index entry closest to the specified keyword is displayed. 
        /// </summary>
        /// <param name="ownerHandle">The window handle that identifies the parent of the help 
        /// dialog.</param>
        /// <param name="url">The path and name of the help file.</param>
        /// <param name="keywordIndex">The keyword for which to search.</param>
        /// <returns>The window handle of the help dialog; or <see cref="IntPtr.Zero"/> if no help 
        /// dialog was openned.</returns>
        public static IntPtr ShowKeywordHelp(IntPtr ownerHandle, string url, string keywordIndex)
        {
            // Create the structure that holds the keyword links
            HtmlHelpLink link = new HtmlHelpLink(keywordIndex);
            
            // Open the html help
            return HtmlHelp(ownerHandle, url, (uint) HtmlHelpCommand.HH_KEYWORD_LOOKUP, link);
        }

        #endregion NativeMethods Methods
    }
}
