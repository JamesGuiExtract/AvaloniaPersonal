using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Extract.Interfaces
{
    /// <summary>
    /// The interface used to provide secure file deletion within Extract Systems software.
    /// </summary>
    [ComVisible(true)]
    [Guid("FFCFA609-2D93-4658-B976-18F7FCC10DA8")]
    public interface ISecureFileDeleter
    {
        /// <summary>
        /// A descriptive name of the implmentation.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Securely deletes the specified <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">Name of the file to securely delete.</param>
        /// <param name="throwIfUnableToDeleteSecurely"><see langword="true"/> if an exception should
        /// be thrown before actually deleting the file if the file could not be securely
        /// overwritten prior to deletion. If <see langword="false"/>, problems overwriting the file
        /// will be logged if the <see cref="Properties.Settings.LogSecureDeleteErrors"/> value is
        /// <see langword="true"/>, otherwise they will be ignored.</param>
        void SecureDeleteFile(string fileName, bool throwIfOverwriteFailed);
    }
}
