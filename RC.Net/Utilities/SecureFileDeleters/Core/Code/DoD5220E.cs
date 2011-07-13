using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UCLID_COMLMLib;
using Extract.Licensing;
using Extract.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Utilities.SecureFileDeleters
{
    /// <summary>
    /// An <see cref="ISecureFileDeleter"/> implementation that fulfills the data cleansing
    /// specifications US Department of Defense manual 5220.22M (Versions prior to Nov 2007).
    /// Specifically, in the matrix from section 8-306, this implments option E (3 pass: zeros,
    /// ones, random).
    /// </summary>
    [ComVisible(true)]
    [Guid("2AA3693B-8724-4275-B343-DCE7047B1E63")]
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class DoD5220E : SecureFileDeleterBase, ISecureFileDeleter
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "US DoD 5220.22M (E) (3 pass)";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DoD5220E"/> class.
        /// </summary>
        public DoD5220E()
            : base()
        {
            try
            {
                // Configure the overwrite passes to be used.
                OverwritePasses.Add((buffer, count) => StaticOverwrite(buffer, count, 0x00));
                OverwritePasses.Add((buffer, count) => StaticOverwrite(buffer, count, 0xFF));
                OverwritePasses.Add((buffer, count) => RandomOverwrite(buffer, count));

                // Enable verification, obfuscation, and 3 rename operations.
                // (For good measure; not specified by 5220.22M)
                Verify = true;
                Obfuscate = true;
                RenameRepetitions = 3;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32836");
            }
        }

        #endregion Constructors

        #region ISecureFileDeleter

        /// <summary>
        /// A descriptive name of this implmentation.
        /// </summary>
        public string Name
        {
            get
            {
                return _COMPONENT_DESCRIPTION;
            }
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
        public override void SecureDeleteFile(string fileName, bool throwIfUnableToDeleteSecurely)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32837",
                    _COMPONENT_DESCRIPTION);

                base.SecureDeleteFile(fileName, throwIfUnableToDeleteSecurely);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32838", ex.Message);
            }
        }

        #endregion ISecureFileDeleter
    }
}
