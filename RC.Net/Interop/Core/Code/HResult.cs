using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Interop
{
    /// <summary>
    /// Represents a Com HRESULT.
    /// </summary>
    public static class HResult
    {
        #region HResult Fields

        /// <summary>
        /// Successful completion.
        /// </summary>
        public static readonly int Ok;

        /// <summary>
        /// Completed without error, but only partial results were obtained.
        /// </summary>
        public static readonly int False = 1;

        /// <summary>
        /// Catastrophic failure.
        /// </summary>
        public static readonly int Unexpected = unchecked((int)0x8000FFFF);

        /// <summary>
        /// Not implemented error.
        /// </summary>
        public static readonly int NotImplemented = unchecked((int)0x80004001);

        /// <summary>
        /// Out of memory error.
        /// </summary>
        public static readonly int OutOfMemory = unchecked((int)0x8007000E);

        /// <summary>
        /// One or more arguments are invalid error.
        /// </summary>
        public static readonly int InvalidArgument = unchecked((int)0x80070057);

        /// <summary>
        /// No such interface supported error.
        /// </summary>
        public static readonly int NoInterface = unchecked((int)0x80004002);

        /// <summary>
        /// Invalid pointer error.
        /// </summary>
        public static readonly int Pointer = unchecked((int)0x80004003);

        /// <summary>
        /// Invalid handle error.
        /// </summary>
        public static readonly int Handle = unchecked((int)0x80070006);

        /// <summary>
        /// Operation aborted error.
        /// </summary>
        public static readonly int Abort = unchecked((int)0x80004004);

        /// <summary>
        /// Unspecified error.
        /// </summary>
        public static readonly int Fail = unchecked((int)0x80004005);

        /// <summary>
        /// Access denied error.
        /// </summary>
        public static readonly int AccessDenied = unchecked((int)0x80070005);

        #endregion HResult Fields

        #region HResult Methods

        /// <summary>
        /// Returns the success code that corresponds to the specified <see cref="Boolean"/>.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> from which to return the success code.
        /// </param>
        /// <returns><see cref="Ok"/> if <paramref name="value"/> is <see langword="true"/>; 
        /// <see cref="False"/> if <paramref name="value"/> is <see langword="false"/>.</returns>
        public static int FromBoolean(bool value)
        {
            return value ? Ok : False;
        }

        #endregion HResult Methods
    }
}
