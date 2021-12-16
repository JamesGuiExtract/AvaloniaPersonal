using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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
        // EncryptedResult buffer must have twice as many chars as inputBytes
        // Returns the bytes written into the encryptedResult buffer
        internal static uint Encrypt(byte[] inputBytes, char[] encryptedResult)
        {
            string plainHexText = StringMethods.ConvertBytesToHexString(inputBytes);

            // Create a pointer to a buffer to hold the encrypted data
            IntPtr buffer = IntPtr.Zero;
            uint dataLength = 0;
            uint outputLength = 0;
            byte[] cipherText = null;

            // Wrap this in a try/catch block so we guarantee even if an exception is thrown that:
            // 1) The exception will be eaten.
            // 2) The empty string will be returned.
            // 3) Memory allocated for the buffer will be released.
            try
            {
                buffer = EncryptBytes(plainHexText, ref dataLength);
                outputLength = dataLength * 2;

                ExtractException.Assert("ELI53047", "Buffer too small",
                    encryptedResult.Length >= outputLength);
                Array.Clear(encryptedResult, 0, encryptedResult.Length);

                // Create a byte array to hold the encrypted text
                cipherText = new byte[dataLength];

                // Copy the data from the buffer to the byte array
                Marshal.Copy(buffer, cipherText, 0, (int)dataLength);

                // Add the encrypted bytes one at a time as a 2 character HEX strings
                for (int i = 0; i < cipherText.Length; i++)
                {
                    // Format each byte as a 2 character HEX string
                    string nextByte = cipherText[i].ToString("x2",
                        System.Globalization.CultureInfo.InvariantCulture);
                    encryptedResult[i * 2] = nextByte[0];
                    encryptedResult[i * 2 + 1] = nextByte[1];
                }
            }
            catch
            {
                // Ignore exceptions
            }
            finally
            {
                // Clear the bytes of the array when done to prevent it from living in process memory.
                if (cipherText != null)
                {
                    Array.Clear(cipherText, 0, cipherText.Length);
                }

                // Free the buffer used by the EncyptBytes p/invoke call
                if (buffer != IntPtr.Zero)
                {
                    byte[] zeros = new byte[dataLength];
                    Marshal.Copy(zeros, 0, buffer, (int)dataLength);
                    Marshal.FreeCoTaskMem(buffer);
                }
            }

            return outputLength;
        }
    }
}
