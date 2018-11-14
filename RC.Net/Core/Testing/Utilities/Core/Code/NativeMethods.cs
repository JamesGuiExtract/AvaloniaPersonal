using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Extract.Testing.Utilities
{
       #region Structures

        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short Year;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short Month;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short DayOfWeek;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short Day;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short Hour;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short Minute;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short Second;
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public short Milliseconds;

            public SystemTime(SystemTime from)
            {
                Year = from.Year;
                Month = from.Month;
                DayOfWeek = from.DayOfWeek;
                Day = from.Day;
                Hour = from.Hour;
                Minute = from.Minute;
                Second = from.Second;
                Milliseconds = from.Milliseconds;
            }

        }

    #endregion

    public static class NativeMethods
    {
 

        #region Constants

        const uint BM_CLICK = 0x00F5;

        #endregion Constants

        #region P/Invokes

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError=true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc enumProc, IntPtr lParam);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true)]
        extern static void Win32GetSystemTime(ref SystemTime sysTime);

        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool Win32SetSystemTime(ref SystemTime sysTime);

        #endregion P/Invokes

        #region Private Methods

        /// <summary>
        /// Find first window that matches the given filter
        /// </summary>
        /// <param name="parentWindow">The handle of the parent window to start searching, or zero to search top-level windows</param>
        /// <param name="filter">A delegate that returns true for windows that should be returned and false
        /// for windows that should not be returned</param>
        static IntPtr FindChildWindow(IntPtr parentWindow, EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;

            EnumChildWindows(parentWindow, delegate(IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    found = wnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);

            return found;
        }

        #endregion Private Methods


        #region Public Methods

        /// <summary>
        /// Click a button via its handle
        /// </summary>
        /// <param name="buttonHandle">The handle of the button</param>
        public static void ClickButton(IntPtr buttonHandle)
        {
            try
            {
                SendMessage(buttonHandle, BM_CLICK, UIntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46043");
            }
        }

        /// <summary>
        /// Get the text for the window pointed to by windowHandle
        /// </summary>
        /// <param name="windowHandle">The handle of the window</param>
        public static string GetWindowText(IntPtr windowHandle)
        {
            try
            {
                int size = GetWindowTextLength(windowHandle);
                if (size > 0)
                {
                    var builder = new StringBuilder(size + 1);
                    if (GetWindowText(windowHandle, builder, builder.Capacity) == 0)
                    {
                        return "";
                    }

                    return builder.ToString();
                }

                return "";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46044");
            }
        }


        /// <summary>
        /// Find first child window that contains the given title text and is part of the target process
        /// </summary>
        /// <param name="parentWindow">The handle of the parent window to start searching, or zero to search top-level windows</param>
        /// <param name="titleText">The text that the window title must contain. </param>
        /// <param name="targetProcessID">The process ID that the window is associated with or zero to skip the check</param>
        public static IntPtr FindWindowWithText(IntPtr parentWindow, string titleText, int targetProcessID = 0)
        {
            try
            {
                return FindChildWindow(parentWindow, delegate (IntPtr wnd, IntPtr param)
                {
                    uint processID = 0;
                    if (targetProcessID != 0)
                    {
                        uint handle = GetWindowThreadProcessId(wnd, out processID);
                        ExtractException.Assert("ELI46041", "Getting window process ID failed", handle != 0);
                    }
                    return processID == targetProcessID && GetWindowText(wnd) == titleText;
                });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46042");
            }
        }

        /// <summary>
        /// Calls the windows GetSystemTime function to get the SystemTime
        /// </summary>
        /// <returns>System time</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static SystemTime GetWin32SystemTime()
        {
            try
            {
                SystemTime systemTime = new SystemTime();
                Win32GetSystemTime(ref systemTime);
                return systemTime;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46492");
            }
        }

        /// <summary>
        /// Sets the System time to the given time.
        /// NOTE: Application that calls this method must be running as an admin in order for it to work
        /// </summary>
        /// <param name="systemTime">The date and time to set</param>
        /// <returns><c>true</c> if the time was changed. <c>false</c> f the time was not changed</returns> 
        public static bool SetWin32SystemTime(SystemTime systemTime)
        {
            try
            {
                return Win32SetSystemTime(ref systemTime);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46493");
            }
        }

        #endregion Public Methods
    }
}
