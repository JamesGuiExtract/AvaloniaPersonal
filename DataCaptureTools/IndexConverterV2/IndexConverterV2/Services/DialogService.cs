using Avalonia.Controls;
using IndexConverterV2.Views;
using System.Threading.Tasks;
using System.Linq;

namespace IndexConverterV2.Services
{
    public interface IDialogService
    {
        /// <summary>
        /// Show a dialog to pick an existing text file.
        /// </summary>
        /// <param name="view">View to start the dialog from.</param>
        /// <returns>A task that will get a file.</returns>
        Task<string?> OpenTxtDialog(IView view);

        /// <summary>
        /// Show a dialog to save a new text file.
        /// </summary>
        /// <param name="view">View to start the dialog from.</param>
        /// <returns>A task that will get a new file.</returns>
        Task<string?> SaveTxtDialog(IView view);

        /// <summary>
        /// Show a dialog to pick an existing csv file.
        /// </summary>
        /// <param name="view">View to start the dialog from.</param>
        /// <returns>A task that will get a file.</returns>
        Task<string?> OpenCSVDialog(IView view);

        /// <summary>
        /// Show a dialog to pick a folder.
        /// </summary>
        /// <param name="view">View to start the dialog from.</param>
        /// <returns>A task that will get a folder.</returns>
        Task<string?> OpenFolderDialog(IView view);
    }

    public class IndexConverterDialogService : IDialogService
    {
        /// <inheritdoc/>
        public async Task<string?> OpenTxtDialog(IView view)
        {
            OpenFileDialog dialog = new()
            {
                Filters = new()
                {
                    new FileDialogFilter() { Name = "TXT Files", Extensions = { "txt" } },
                    new FileDialogFilter() { Name = "All Files", Extensions = { "*" } }
                },
                AllowMultiple = false
            };
            return (await dialog.ShowAsync((Window)view))?.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<string?> SaveTxtDialog(IView view)
        {
            SaveFileDialog dialog = new()
            {
                Filters = new()
                {
                    new FileDialogFilter() { Name = "TXT Files", Extensions = { "txt" } },
                    new FileDialogFilter() { Name = "All Files", Extensions = { "*" } }
                }
            };
            return await dialog.ShowAsync((Window)view);
        }

        /// <inheritdoc/>
        public async Task<string?> OpenCSVDialog(IView view)
        {
            OpenFileDialog dialog = new()
            {
                Filters = new()
                {
                    new FileDialogFilter() { Name = "CSV Files", Extensions = { "csv" } },
                    new FileDialogFilter() { Name = "All Files", Extensions = { "*" } }
                },
                AllowMultiple = false
            };
            return (await dialog.ShowAsync((Window)view))?.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<string?> OpenFolderDialog(IView view)
        {
            OpenFolderDialog dialog = new();
            return await dialog.ShowAsync((Window)view);
        }
    }
}
