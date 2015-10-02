using System;
using System.Runtime.InteropServices;

namespace Extract.FAMDBCounterManager
{
    internal static class NativeMethods
    {
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
            [MarshalAs(UnmanagedType.Bool)] bool encrypt, ref uint length);

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
            try
            {
                // Create a pointer to a buffer to hold the encrypted data
                IntPtr inBuffer = IntPtr.Zero;
                IntPtr outBuffer = IntPtr.Zero;
                uint dataLength = 0;

                inBuffer = Marshal.AllocCoTaskMem(input.Length);
                Marshal.Copy(input, 0, inBuffer, input.Length);
                dataLength = (uint)input.Length;

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
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38797");
                }
                finally
                {
                    // Free buffers used to pass data to/from the p/invoke call.
                    if (inBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(outBuffer);
                    }
                    if (outBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(outBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38802");
            }
        }
    }
}