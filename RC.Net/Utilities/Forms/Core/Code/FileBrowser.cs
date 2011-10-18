using Extract.Interfaces;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Exposes <see cref="BrowseForFolder"/> and <see cref="BrowseForFile"/> functionality via COM.
    /// </summary>
    [ComVisible(true)]
    [Guid("5EDFD467-C02C-4E66-B751-7B154FD2F89F")]
    [CLSCompliant(false)]
    public class FileBrowser : IFileBrowser
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FileBrowser).ToString();

        #endregion Constants

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBrowser"/> class.
        /// </summary>
        public FileBrowser()
        {
            try
            {
                LicenseUtilities.ValidateLicense(
                        LicenseIdName.ExtractCoreObjects, "ELI34032", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34033");
            }
        }

        /// <summary>
        /// Allows the user to select a folder using the folder browser.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="initialFolder">The initial folder for the folder browser.</param>
        /// <returns>The result of the user's selection or <see langword="null"/> if the user
        /// canceled the dialog.</returns>
        public string BrowseForFolder(string description, string initialFolder)
        {
            try
            {
                return FormsMethods.BrowseForFolder(description, initialFolder);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34030", "Error in folder dialog.");
            }
        }

        /// <summary>
        /// Allows the user to select a file using the file dialog.
        /// </summary>
        /// <param name="fileFilter">Allows the displayed files to be limited.</param>
        /// <param name="initialFolder">The initial folder for the file browser.</param>
        /// <returns>The result of the user's selection or <see langword="null"/> if the user
        /// canceled the dialog.</returns>
        public string BrowseForFile(string fileFilter, string initialFolder)
        {
            try
            {
                return FormsMethods.BrowseForFile(fileFilter, initialFolder);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI34031", "Error in file dialog.");
            }
        }
    }
}
