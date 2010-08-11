using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;

namespace Extract
{
    internal static class NativeMethods
    {
        // P/Invoke the encryption method from BaseUtils.
        // NOTE:    The mangled name appearing below should stay the same unless we switch
        //          to a new version of the compiler.  If there is a problem where the entry
        //          point cannot be found, run DUMPBIN /Exports on BaseUtils.dll and
        //          search the output for externManipulator.  When you find it, copy
        //          the full name from the output (the output is organized by columns,
        //          there should be three columns of numbers (1 decimal, 2 HEX) and then
        //          the mangled name of the function followed by a space and '='.
        [DllImport("BaseUtils.dll",
            EntryPoint = "?externManipulator@@YAPAEPBDPAK@Z", CharSet = CharSet.Ansi,
            BestFitMapping = false, ThrowOnUnmappableChar = true,
            CallingConvention = CallingConvention.Cdecl)]
#if DEBUG
        // Need to suppress the AvoidUncalledPrivateCode message in DEBUG builds because
        // these methods (currently) are only accessed when the code is built in Release mode.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
#endif
        internal static extern IntPtr EncryptBytes([MarshalAs(UnmanagedType.LPStr)] string text,
            ref uint length);

        // P/Invoke the LogException method in BaseUtils so a simple exception can be logged.
        // NOTE:    This is needed to allow logging information if the COMUCLIDException object
        //          either cannot be created or it throws an exception in the process of logging
        //          an exception.
        [DllImport("BaseUtils.dll", EntryPoint = "?externLogException@@YAXPAD0@Z",
            CharSet = CharSet.Ansi, BestFitMapping=false, ThrowOnUnmappableChar=true,
            CallingConvention = CallingConvention.Cdecl)]
#if DEBUG
        // Need to suppress the AvoidUncalledPrivateCode message in DEBUG builds because
        // these methods (currently) are only accessed when the code is built in Release mode.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
#endif
        internal static extern void LogException(
            [MarshalAs(UnmanagedType.LPStr)] string EliCode,
            [MarshalAs(UnmanagedType.LPStr)] string Message);

        /// <summary>
        /// Encrypts a given string using the internal Extract Systems passwords for encrypting
        /// Exception debug strings.
        /// </summary>
        /// <param name="plainText">The string to be encrypted</param>
        /// <returns>A string containing the encrypted data</returns>
        [SecurityPermission(SecurityAction.Demand, 
            Flags = SecurityPermissionFlag.UnmanagedCode)]
#if DEBUG
        // Need to suppress the AvoidUncalledPrivateCode message in DEBUG builds because
        // these methods (currently) are only accessed when the code is built in Release mode.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
#endif
        internal static string EncryptString(string plainText)
        {
            string plainHexText = StringMethods.ConvertBytesToHexString(
                StringMethods.ConvertStringToBytes(plainText));

            string encryptedText = "";

            // Create a pointer to a buffer to hold the encrypted data
            IntPtr buffer = IntPtr.Zero;

            // Wrap this in a try/catch block so we guarantee even if an exception is thrown that:
            // 1) The exception will be eaten.
            // 2) The empty string will be returned.
            // 3) Memory allocated for the buffer will be released.
            try
            {
                uint dataLength = 0;
                buffer = EncryptBytes(plainHexText, ref dataLength);

                // Create a byte array to hold the encrypted text
                byte[] cipherText = new byte[dataLength];

                // Copy the data from the buffer to the byte array
                Marshal.Copy(buffer, cipherText, 0, (int)dataLength);

                // Create a new string builder and add the encrypted bytes to
                // the string one byte at a time
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < cipherText.Length; i++)
                {
                    // Format each byte as a 2 character HEX string
                    sb.Append(cipherText[i].ToString("X2", 
                        System.Globalization.CultureInfo.InvariantCulture));
                }

                // Copy the string builder into the encrypted string
                encryptedText = sb.ToString();
            }
            catch
            {
                // Ignore exceptions
            }
            finally
            {
                // Free the memory that was allocated in the encrypt method
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }

            // Return the encrypted text
            return encryptedText;
        }
    }
}
