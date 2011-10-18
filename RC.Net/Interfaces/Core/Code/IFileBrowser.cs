using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Extract.Interfaces
{
    /// <summary>
    /// Interface for a class that allows file browsing.
    /// </summary>
    [ComVisible(true)]
    [Guid("C09906CB-993F-4E13-A76E-DFC85218759D")]
    [CLSCompliant(false)]
    public interface IFileBrowser
    {
        /// <summary>
        /// Allows the user to select a folder using the folder browser.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="initialFolder">The initial folder for the folder browser.</param>
        /// <returns>The result of the user's selection or <see langword="null"/> if the user
        /// canceled the dialog.</returns>
        string BrowseForFolder(string description, string initialFolder);

        /// <summary>
        /// Allows the user to select a file using the file dialog.
        /// </summary>
        /// <param name="fileFilter">Allows the displayed files to be limited.</param>
        /// <param name="initialFolder">The initial folder for the file browser.</param>
        /// <returns>The result of the user's selection or <see langword="null"/> if the user
        /// canceled the dialog.</returns>
        string BrowseForFile(string fileFilter, string initialFolder);
    }
}
