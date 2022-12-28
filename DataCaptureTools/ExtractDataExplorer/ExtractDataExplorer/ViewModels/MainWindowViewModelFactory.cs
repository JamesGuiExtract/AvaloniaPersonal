using Extract.Utilities.WPF;
using Extract.Utilities.ReactiveUI;
using ExtractDataExplorer.Models;
using UCLID_AFUTILSLib;
using ExtractDataExplorer.Services;

namespace ExtractDataExplorer.ViewModels
{
    /// <summary>
    /// A factory to create a <see cref="MainWindowViewModel"/> from a <see cref="MainWindowModel"/> and injected dependencies
    /// </summary>
    public class MainWindowViewModelFactory : IAutoSuspendViewModelFactory<MainWindowViewModel, MainWindowModel>
    {
        readonly IFileBrowserDialogService _fileBrowserService;
        readonly IMessageDialogService _messageDialogService;
        readonly IThemingService _themingService;
        readonly IAFUtility _afutil;

        public MainWindowViewModelFactory(
            IFileBrowserDialogService fileBrowserService,
            IMessageDialogService messageDialogService,
            IThemingService themingService,
            IAFUtility afutil)
        {
            _fileBrowserService = fileBrowserService;
            _messageDialogService = messageDialogService;
            _themingService = themingService;
            _afutil = afutil;
        }

        /// <inheritdoc/>
        public MainWindowViewModel CreateViewModel(MainWindowModel? model = null)
        {
            return new MainWindowViewModel(
                serializedModel: model,
                fileBrowserService: _fileBrowserService,
                messageDialogService: _messageDialogService,
                themingService: _themingService,
                afutil: _afutil);
        }

        /// <inheritdoc/>
        public MainWindowModel CreateModel(MainWindowViewModel viewModel)
        {
            return new MainWindowModel(viewModel.DarkMode, viewModel.AttributesFilePath);
        }
    }
}
