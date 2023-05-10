using Extract.Utilities.WPF;
using ExtractDataExplorer.Models;
using ExtractDataExplorer.Services;
using System;
using System.Reactive.Disposables;

namespace ExtractDataExplorer.ViewModels
{
    public class DocumentViewModelFactory
    {
        readonly ISourceDocNameResolver _sourceDocNameResolver;
        readonly CompositeDisposable _disposables;
        readonly IFileBrowserDialogService _fileBrowserDialogService;

        public DocumentViewModelFactory(
            ISourceDocNameResolver sourceDocNameResolver,
            IFileBrowserDialogService fileBrowserDialogService,
            CompositeDisposable disposables)
        {
            _sourceDocNameResolver = sourceDocNameResolver;
            _fileBrowserDialogService = fileBrowserDialogService;
            _disposables = disposables;
        }

        public DocumentViewModel CreateViewModel(MainWindowViewModel parent, DocumentModel? model = null)
        {
            return new DocumentViewModel(
                parent: parent,
                fileBrowserDialogService: _fileBrowserDialogService,
                sourceDocNameResolver: _sourceDocNameResolver,
                disposables: _disposables)
            {
                DocumentPath = model?.DocumentPath
            };
        }

        public static DocumentModel CreateModel(DocumentViewModel viewModel)
        {
            _ = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            return new DocumentModel(viewModel.DocumentPath);
        }
    }

}
