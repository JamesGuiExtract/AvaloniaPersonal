using Microsoft.WindowsAPICodePack.Dialogs;
using System;

namespace Extract.Utilities.WPF
{
    /// Service to wrap a file browser dialog. Used to facilitate view model unit testing
    public interface IFileBrowserDialogService
    {
        /// <summary>
        /// Open a file browser for existing files and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <param name="fileFilter">The filename pattern filter</param>
        /// <param name="initialDirectory">Optional directory to show to the user when the dialog opens</param>
        /// <returns>The selected file path or null if no file was selected</returns>
        string? SelectExistingFile(string dialogTitle, string? fileFilter, string? initialDirectory = null);

        /// <summary>
        /// Open a file browser that doesn't require an existing file and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <param name="fileFilter">The filename pattern filter</param>
        /// <param name="initialDirectory">Optional directory to show to the user when the dialog opens</param>
        /// <returns>The selected file path or null if no file was selected</returns>
        string? SelectFile(string dialogTitle, string? fileFilter, string? initialDirectory = null);

        /// <summary>
        /// Open a folder browser and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <param name="initialDirectory">Optional directory to show to the user when the dialog opens</param>
        /// <returns>The selected folder path or null if no folder was selected</returns>
        string? SelectFolder(string dialogTitle, string? initialDirectory = null);
    }

    /// <inheritdoc/>
    public class FileBrowserDialogService : IFileBrowserDialogService
    {
        /// <inheritdoc/>
        public string? SelectExistingFile(string dialogTitle, string? fileFilter, string? initialDirectory = null)
        {
            return BrowseForFileOrFolder(
                description: dialogTitle,
                initialFolder: null,
                pickFolder: false,
                fileFilter: fileFilter,
                multipleSelect: false,
                ensurePathExists: false,
                ensureFileExists: true);
        }

        /// <inheritdoc/>
        public string? SelectFile(string dialogTitle, string? fileFilter, string? initialDirectory = null)
        {
            return BrowseForFileOrFolder(
                description: dialogTitle,
                initialFolder: null,
                pickFolder: false,
                fileFilter: fileFilter,
                multipleSelect: false,
                ensurePathExists: false,
                ensureFileExists: false);
        }

        /// <inheritdoc/>
        public string? SelectFolder(string dialogTitle, string? initialDirectory)
        {
            return BrowseForFileOrFolder(
                description: dialogTitle,
                initialFolder: null,
                pickFolder: true,
                fileFilter: null,
                multipleSelect: false,
                ensurePathExists: true,
                ensureFileExists: false);
        }

        /// <summary>
        /// Allows the user to select a folder using the folder browser.
        /// </summary>
        /// <param name="description">The text to display over the selection control.</param>
        /// <param name="initialFolder">The initial folder for the folder browser.</param>
        /// <param name="fileFilter">The file type filter.</param>
        /// <param name="pickFolder">Whether to allow folders to be selected rather than files.</param>
        /// <param name="ensurePathExists">Whether to validate that the picked path exists.</param>
        /// <param name="ensureFileExists">Whether to validate that the picked file exists.</param>
        /// <returns>The result of the user's selection or <c>null</c> if the user
        /// canceled the dialog.</returns>
        /// <remarks>This is copied from Extract.Utilities.Forms in order to avoid the Forms dependency for WPF projects</remarks>
        static string? BrowseForFileOrFolder(
            string? description,
            string? initialFolder,
            bool pickFolder,
            string? fileFilter,
            bool multipleSelect,
            bool ensurePathExists,
            bool ensureFileExists)
        {
            try
            {
                using CommonOpenFileDialog browser = new()
                {
                    IsFolderPicker = pickFolder,
                    Multiselect = multipleSelect,
                    EnsurePathExists = ensurePathExists,
                    EnsureFileExists = ensureFileExists
                };

                // Set the initial folder if necessary
                if (!string.IsNullOrEmpty(initialFolder))
                {
                    browser.InitialDirectory = initialFolder;
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    // Set the description
                    browser.Title = description;
                }
                else if (pickFolder)
                {
                    browser.Title = "Please select a folder";
                }
                else
                {
                    browser.Title = "Please select a file";
                }

                // Set the filter text and the initial filter
                if (!string.IsNullOrEmpty(fileFilter))
                {
                    var filters = fileFilter!.Split(new[] { '|' });
                    for (int i = 1; i < filters.Length; i += 2)
                    {
                        var filter = new CommonFileDialogFilter(filters[i - 1], filters[i]) { ShowExtensions = false };
                        browser.Filters.Add(filter);
                    }
                }

                // Show the dialog
                var result = browser.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    // Return the selected path.
                    return browser.FileName;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53759");
            }
        }
    }
}
