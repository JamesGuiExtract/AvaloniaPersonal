using Extract.Utilities.Forms;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    /// Service to wrap a file browser dialog. Used to facility view model unit testing
    public interface IFileBrowserDialogService
    {
        /// <summary>
        /// Open a file browser for existing files and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <param name="fileFilter">The filename pattern filter</param>
        /// <returns>The selected file path or null if no file was selected</returns>
        string SelectExistingFile(string dialogTitle, string fileFilter);

        /// <summary>
        /// Open a file browser that doesn't require an existing file and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <param name="fileFilter">The filename pattern filter</param>
        /// <returns>The selected file path or null if no file was selected</returns>
        string SelectFile(string dialogTitle, string fileFilter);
    }

    /// <inheritdoc/>
    public class FileBrowserDialogService : IFileBrowserDialogService
    {
        public string SelectExistingFile(string dialogTitle, string fileFilter)
        {
            return FormsMethods.BrowseForFileOrFolder(
                description: dialogTitle,
                initialFolder: null,
                pickFolder: false,
                fileFilter: fileFilter,
                multipleSelect: false,
                ensurePathExists: false,
                ensureFileExists: true);
        }

        public string SelectFile(string dialogTitle, string fileFilter)
        {
            return FormsMethods.BrowseForFileOrFolder(
                description: dialogTitle,
                initialFolder: null,
                pickFolder: false,
                fileFilter: fileFilter,
                multipleSelect: false,
                ensurePathExists: false,
                ensureFileExists: false);
        }
    }
}
