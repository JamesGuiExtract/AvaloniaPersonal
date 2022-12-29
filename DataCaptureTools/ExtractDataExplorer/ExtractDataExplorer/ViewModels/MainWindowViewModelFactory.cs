using Extract.Utilities.WPF;
using Extract.Utilities.ReactiveUI;
using ExtractDataExplorer.Models;
using ExtractDataExplorer.Services;
using System;

namespace ExtractDataExplorer.ViewModels
{
    /// <summary>
    /// A factory to create a <see cref="MainWindowViewModel"/> from a <see cref="MainWindowModel"/> and injected dependencies
    /// </summary>
    public sealed class MainWindowViewModelFactory : IAutoSuspendViewModelFactory<MainWindowViewModel, MainWindowModel>
    {
        readonly IFileBrowserDialogService _fileBrowserService;
        readonly IMessageDialogService _messageDialogService;
        readonly IThemingService _themingService;
        readonly IAttributeTreeService _attributeTreeService;

        public MainWindowViewModelFactory(
            IFileBrowserDialogService fileBrowserService,
            IMessageDialogService messageDialogService,
            IThemingService themingService,
            IAttributeTreeService attributeTreeService)
        {
            _fileBrowserService = fileBrowserService;
            _messageDialogService = messageDialogService;
            _themingService = themingService;
            _attributeTreeService = attributeTreeService;
        }

        /// <inheritdoc/>
        public MainWindowViewModel CreateViewModel(MainWindowModel? model = null)
        {
            return new MainWindowViewModel(
                serializedModel: model,
                fileBrowserService: _fileBrowserService,
                messageDialogService: _messageDialogService,
                themingService: _themingService,
                attributeTreeService: _attributeTreeService);
        }

        /// <inheritdoc/>
        public MainWindowModel CreateModel(MainWindowViewModel viewModel)
        {
            _ = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            return new MainWindowModel(
                darkMode: viewModel.DarkMode,
                attributesFilePath: viewModel.AttributesFilePath,
                attributeFilter: viewModel.ConfiguredAttributeFilter,
                isFilterApplied: viewModel.IsFilterApplied && !viewModel.IsFilterChanged);
        }
    }
}
