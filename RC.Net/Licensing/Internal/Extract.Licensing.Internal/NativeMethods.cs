using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Licensing.Internal
{
    internal static class NativeMethods
    {
        // Define four UCLID passwords used for interleaved string of 
        // encrypted license data
        public const UInt32 Key1 = 0x411065D2;
        public const UInt32 Key2 = 0x7F9D16C2;
        public const UInt32 Key3 = 0x6E5C66AA;
        public const UInt32 Key4 = 0x15D27BA6;

        // Define four UCLID passwords used for hard-coded application
        // passwords
        public const UInt32 Key5 = 0x17A64E1D;
        public const UInt32 Key6 = 0xDA80339;
        public const UInt32 Key7 = 0x1A0955D7;
        public const UInt32 Key8 = 0x6EE23DA7;

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



        // P/Invoke the encryption method from BaseUtils.
        // NOTE:    The mangled name appearing below should stay the same unless we switch
        //          to a new version of the compiler.  If there is a problem where the entry
        //          point cannot be found, run DUMPBIN /Exports on InternalLicenseUtils.dll
        //          and search the output for externManipulator.  When you find it, copy
        //          the full name from the output (the output is organized by columns,
        //          there should be three columns of numbers (1 decimal, 2 HEX) and then
        //          the mangled name of the function followed by a space and '='.
        [DllImport("InternalLicenseUtils.dll",
            EntryPoint = "?externManipulatorInternal@@YAPAEPAE_NPAKKKKK@Z", CharSet = CharSet.Ansi,
            BestFitMapping = false, ThrowOnUnmappableChar = true,
            CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr EncryptDecryptBytes([MarshalAs(UnmanagedType.SysInt)] IntPtr input,
            [MarshalAs(UnmanagedType.U1)] bool encrypt, ref uint length, uint k1, uint k2, uint k3, uint k4);

        /// <summary>
        /// Gets a value indicating whether this instance is licensed for internal Extract Systems use.
        /// </summary>
        /// <value><see langword="true"/> if this instance is licensed for internal Extract Systems use;
        /// otherwise, <see langword="false"/>.</value>
        public static bool IsInternalToolsLicensed
        {
            get
            {
                return isInternalToolsLicensed();
            }
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
        internal static byte[] EncryptDecryptBytes(byte[] input, bool encrypt, UInt32 k1, UInt32 k2, UInt32 k3, UInt32 k4)
        {
            if (!IsInternalToolsLicensed)
                throw new Exception("ELI50176: Must be internally licneses.");
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
                outBuffer = EncryptDecryptBytes(inBuffer, encrypt, ref dataLength, k1, k2, k3, k4);

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