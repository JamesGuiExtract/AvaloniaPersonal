using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DynamicData.Kernel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Service to wrap a file browser dialog. Used to facility view model unit testing
    /// </summary>
    public interface IFileBrowserDialogService
    {
        /// <summary>
        /// Open a file browser for existing files and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <param name="fileFilter">The filename pattern filter</param>
        /// <returns>The selected file path or null if no file was selected</returns>
        Task<Optional<string>> SelectExistingFile(string dialogTitle, string fileFilter, string? initialDirectory = null);

        /// <summary>
        /// Open a file browser that doesn't require an existing file and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <param name="fileFilter">The filename pattern filter</param>
        /// <returns>The selected file path or null if no file was selected</returns>
        Task<Optional<string>> SelectFile(string dialogTitle, string fileFilter, string? initialDirectory = null);

        /// <summary>
        /// Open a folder browser and return the result
        /// </summary>
        /// <param name="dialogTitle">The title for the file browser window</param>
        /// <returns>The selected folder path or null if no folder was selected</returns>
        Task<Optional<string>> SelectFolder(string dialogTitle, string? initialDirectory = null);
    }

    /// <inheritdoc/>
    public class FileBrowserDialogService : IFileBrowserDialogService
    {
        /// <inheritdoc/>
        public async Task<Optional<string>> SelectExistingFile(string dialogTitle, string fileFilter, string? initialDirectory = null)
        {
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Title = dialogTitle,
                Filters = ParseFilterString(fileFilter)
            };

            if (initialDirectory is not null)
            {
                dialog.Directory = initialDirectory;
            }

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                string[]? selectedFiles = await dialog.ShowAsync(desktop.MainWindow).ConfigureAwait(false);
                if (selectedFiles is string[] fileNames && fileNames.Length > 0)
                {
                    return fileNames[0];
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<Optional<string>> SelectFile(string dialogTitle, string fileFilter, string? initialDirectory = null)
        {
            var dialog = new SaveFileDialog()
            {
                Title = dialogTitle,
                Filters = ParseFilterString(fileFilter)
            };

            if (initialDirectory is not null)
            {
                dialog.Directory = initialDirectory;
            }

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                string? selectedFile = await dialog.ShowAsync(desktop.MainWindow).ConfigureAwait(false);
                if (selectedFile is string fileName)
                {
                    return fileName;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<Optional<string>> SelectFolder(string dialogTitle, string? initialDirectory = null)
        {
            var dialog = new OpenFolderDialog
            {
                Title = dialogTitle
            };

            if (initialDirectory is not null)
            {
                dialog.Directory = initialDirectory;
            }

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return await dialog.ShowAsync(desktop.MainWindow).ConfigureAwait(false);
            }

            return null;
        }

        static List<FileDialogFilter>? ParseFilterString(string fileFilter)
        {
            if (string.IsNullOrEmpty(fileFilter))
            {
                return null;
            }
            var filters = fileFilter.Split(new[] { '|' });
            if (filters.Length == 0)
            {
                return null;
            }

            List<FileDialogFilter> fileDialogFilters = new();

            for (int i = 1; i < filters.Length; i += 2)
            {
                fileDialogFilters.Add(new()
                {
                    Name = filters[i - 1],
                    Extensions = filters[i].Split(',', ';').ToList()
                });
            }

            return fileDialogFilters;
        }
    }
}
