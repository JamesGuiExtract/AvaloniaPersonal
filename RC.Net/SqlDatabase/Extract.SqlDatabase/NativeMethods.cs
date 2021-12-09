using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Extract.SqlDatabase
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
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        static extern IntPtr EncryptBytes([MarshalAs(UnmanagedType.LPStr)] string text,
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
            ref uint length);


        // Provides encryption used in the process to aquire the password for a database application role.
        [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        internal static string Encrypt(byte[] bytes)
        {
            string plainHexText = StringMethods.ConvertBytesToHexString(bytes);

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
                    sb.Append(cipherText[i].ToString("x2",
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
