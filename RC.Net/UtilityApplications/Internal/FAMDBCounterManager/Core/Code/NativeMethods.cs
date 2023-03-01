using Extract.Licensing.Internal;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    internal static class NativeMethods
    {
        /// <summary>
        /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct WindowsRectangle
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        // P/Invoke the isInternalToolsLicensed();.
        // NOTE:    If there is a problem where the entry point cannot be found, run DUMPBIN
        //          /Exports on baseutils.dll and search the output for isInternalToolsLicensed.
        //          When you find it, copy the full name from the output (the output is organized by
        //          columns, there should be three columns of numbers (1 decimal, 2 HEX) and then
        //          the mangled name of the function followed by a space and '='.
        [DllImport("BaseUtils.dll", EntryPoint = "?isInternalToolsLicensed@@YA_NXZ")]
        // It turns out that using UnmanagedType.Bool results in true for c++ bools. 
        // Type I1 (one byte) must be used to be able to get the correct return value.
        // http://stackoverflow.com/questions/4608876/c-sharp-dllimport-with-c-boolean-function-not-returning-correctly
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool isInternalToolsLicensed();

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

        // P/Invoke the encryption method from BaseUtils.
        // NOTE:    The mangled name appearing below should stay the same unless we switch
        //          to a new version of the compiler.  If there is a problem where the entry
        //          point cannot be found, run DUMPBIN /Exports on InternalLicenseUtils.dll
        //          and search the output for externManipulator.  When you find it, copy
        //          the full name from the output (the output is organized by columns,
        //          there should be three columns of numbers (1 decimal, 2 HEX) and then
        //          the mangled name of the function followed by a space and '='.
        [DllImport("InternalLicenseUtils.dll",
            EntryPoint = "?externManipulator@@YAPAEPAE_NPAK@Z", CharSet = CharSet.Ansi,
            BestFitMapping = false, ThrowOnUnmappableChar = true,
            CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr EncryptDecryptBytes([MarshalAs(UnmanagedType.SysInt)] IntPtr input,
            [MarshalAs(UnmanagedType.U1)] bool encrypt, ref uint length);

        /// <summary>
        /// Gets a value indicating whether this instance is licensed for internal Extract Systems use.
        /// </summary>
        /// <value><see langword="true"/> if this instance is licensed for internal Extract Systems use;
        /// otherwise, <see langword="false"/>.</value>
        public static bool IsInternalToolsLicensed
        {
            get
            {
                return UtilityMethods.IsOnExtractDomain();
            }
        }

        /// <summary>
        /// Get the rectangle containing the specified window in screen coordinates.
        /// </summary>
        /// <param name="window">The window to get the rectangle for. May not
        /// be <see langword="null"/>.</param>
        /// <returns>A <see cref="Rectangle"/> representing the location and
        /// the bounds of the specified window in screen coordinates.</returns>
        public static Rectangle GetWindowScreenRectangle(IWin32Window window)
        {
            UtilityMethods.Assert(window != null, "Window object is null");

            // Declare a new windows rectangle to hold the return data
            WindowsRectangle windowRectangle = new();

            // Call the win32API GetWindowRect function
            UtilityMethods.Assert(GetWindowRect(window.Handle, ref windowRectangle),
                "Failed to get window rect");

            // Return a Rectangle containing the window position and size
            return new Rectangle(windowRectangle.left, windowRectangle.top,
                windowRectangle.right - windowRectangle.left,
                windowRectangle.bottom - windowRectangle.top);
        }

        /// <summary>
        /// Encrypts or decrypts the specified <see paramref="input"/> data using the internal
        /// Extract Systems password for secure FAM counters.
        /// </summary>
        /// <param name="input">The data to be encrypted/decrypted. The length of this data must be
        /// a multiple of 8 bytes.</param>
        /// <param name="encrypt"><see langword="true"/> to encrypt <see paramref="input"/>;
        /// <see langword="false"/> to decrypt it.</param>
        /// <returns>The encrypted data.</returns>
        //[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal static byte[] EncryptDecryptBytes(byte[] input, bool encrypt)
        {
            // Create a pointer to a buffer to hold the encrypted data
            IntPtr outBuffer = IntPtr.Zero;

            IntPtr inBuffer = Marshal.AllocCoTaskMem(input.Length);
            Marshal.Copy(input, 0, inBuffer, input.Length);
            uint dataLength = (uint)input.Length;

            // Wrap this in a try/catch block so we guarantee even if an exception is thrown that:
            // 1) The exception will be eaten.
            // 2) The empty string will be returned.
            // 3) Memory allocated for the buffer will be released.
            try
            {
                outBuffer = EncryptDecryptBytes(inBuffer, encrypt, ref dataLength);

                // Copy the data from the buffer to a managed byte array.
                var outputData = new byte[dataLength];
                Marshal.Copy(outBuffer, outputData, 0, (int)dataLength);

                return outputData;
            }
            finally
            {
                // Free buffers used to pass data to/from the p/invoke call.
                if (inBuffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(inBuffer);
                }
                if (outBuffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(outBuffer);
                }
            }
        }
    }
}