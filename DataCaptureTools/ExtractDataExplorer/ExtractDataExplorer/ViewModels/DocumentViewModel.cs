using Extract.Utilities.WPF;
using ExtractDataExplorer.Models;
using ExtractDataExplorer.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using static ExtractDataExplorer.Utils;

namespace ExtractDataExplorer.ViewModels
{
    /// <summary>
    /// View model to interface with a document viewer and associated controls
    /// </summary>
    public class DocumentViewModel : ViewModelBase
    {
        readonly CompositeDisposable _disposables;
        readonly IFileBrowserDialogService _fileBrowserDialogService;
        readonly ISourceDocNameResolver _sourceDocNameResolver;

        /// <summary>
        /// The image/pdf path that is displayed in the view
        /// </summary>
        [Reactive] public string? DocumentPath { get; set; }

        /// <summary>
        /// The document that is (or is expected to be) shown in the viewer
        /// </summary>
        [Reactive] public string? CurrentlyOpenDocumentPath { get; set; }

        /// <summary>
        /// The text for the load/reload button
        /// </summary>
        [Reactive] public string LoadDocumentText { get; set; } = "Load";

        /// <summary>
        /// The page number that is currently being shown
        /// </summary>
        [Reactive] public int? CurrentPageNumber { get; set; }

        /// <summary>
        /// The page count of the currently loaded document
        /// </summary>
        [Reactive] public int? TotalPages { get; set; }

        /// <summary>
        /// The zones that the viwer should display
        /// </summary>
        [Reactive] public IList<Polygon>? HighlightedAreas { get; set; }

        /// <summary>
        /// Run a file browser to select/load a document
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectDocumentCommand { get; }

        /// <summary>
        /// Load the document specified by <see cref="DocumentPath"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadDocumentCommand { get; }

        /// <summary>
        /// Go to the previous page of the loaded document
        /// </summary>
        public ReactiveCommand<Unit, Unit> PrevPageCommand { get; }

        /// <summary>
        /// Go to the next page of the loaded document
        /// </summary>
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

        public DocumentViewModel(
            MainWindowViewModel parent,
            IFileBrowserDialogService fileBrowserDialogService,
            ISourceDocNameResolver sourceDocNameResolver,
            CompositeDisposable disposables)
        {
            _disposables = disposables;
            _fileBrowserDialogService = fileBrowserDialogService;
            _sourceDocNameResolver = sourceDocNameResolver;

            // Change load/reload button text based on whether DocumentPath == _currentOpenDocument
            this.WhenAnyValue(x => x.DocumentPath, x => x.CurrentlyOpenDocumentPath,
                (document, currentlyOpenDocument) =>
                    string.Equals(document, currentlyOpenDocument, StringComparison.OrdinalIgnoreCase)
                    ? "Reload"
                    : "Load")
                .BindTo(this, x => x.LoadDocumentText);

            // Change document/page based on the selected attribute
            parent.WhenAnyValue(x => x.SelectedAttribute)
                .Subscribe(selectedAttribute => ShowDocumentPage(parent, selectedAttribute))
                .DisposeWith(_disposables);

            // Highlight the selected attribute in the document viewer
            parent.WhenAnyValue(x => x.SelectedAttribute)
                .Select(attribute => attribute?.Zones)
                .BindTo(this, x => x.HighlightedAreas)
                .DisposeWith(_disposables);

            // Commands
            SelectDocumentCommand = ReactiveCommand.CreateFromTask(SelectSourceDocNameFile);

            LoadDocumentCommand = ReactiveCommand.CreateFromTask(
                execute: () => LoadDocument(parent),
                canExecute: this.WhenAnyValue(x => x.DocumentPath)
                    .Select(maybePath => TrimPath(maybePath) is string path && File.Exists(path)));

            PrevPageCommand = ReactiveCommand.Create(
                execute: GoToPrevPage,
                canExecute: this.WhenAnyValue(x => x.CurrentPageNumber,
                    page => page > 1));

            NextPageCommand = ReactiveCommand.Create(
                execute: GoToNextPage,
                canExecute: this.WhenAnyValue(x => x.CurrentPageNumber, x => x.TotalPages,
                    (page, pageCount) => page < pageCount));
        }

        // Set the CurrentlyOpenDocumentPath and CurrentPageNumber based on the selected attribute
        private void ShowDocumentPage(MainWindowViewModel parent, AttributeTreeViewModel? selectedAttribute)
        {
            if (selectedAttribute is null
                || !_sourceDocNameResolver.TryGetSourceDocName(
                        selectedAttribute.SourceDocName,
                        parent.AttributesFilePathExpanded,
                        out string sourceDocName))
            {
                // Leave the current document open if, e.g., the user is changing the VOA path
                if (selectedAttribute is null)
                {
                    return;
                }

                CurrentlyOpenDocumentPath = null;
            }
            else
            {
                CurrentlyOpenDocumentPath = sourceDocName;

                if (selectedAttribute.Zones.Any())
                {
                    var attributePages = new HashSet<int>(selectedAttribute.Zones.Select(poly => poly.PageNumber));

                    // Change pages if needed to show part of the current attribute
                    if (attributePages.Any()
                        && (CurrentPageNumber is not int currentPageNumber || !attributePages.Contains(currentPageNumber)))
                    {
                        // Setting the CurrentPageNumber property will trigger the page change
                        // and cause the currently selected attribute to be highlighted
                        CurrentPageNumber = attributePages.Min();
                    }
                }
            }
        }

        // Load the current DocumentPath
        private async Task LoadDocument(MainWindowViewModel parent)
        {
            if (DocumentPath is string newDocumentPath)
            {
                int pageNumberToShow = CurrentPageNumber ?? 1;

                // Deselect the selected attribute if we are loading a new image
                if (!String.Equals(CurrentlyOpenDocumentPath, newDocumentPath, StringComparison.OrdinalIgnoreCase))
                {
                    parent.SelectedAttributeFromView = null;
                    pageNumberToShow = 1;
                }

                // Set the current path to the new value to trigger the view to load the file
                CurrentlyOpenDocumentPath = newDocumentPath;
                CurrentPageNumber = pageNumberToShow;

                // If the attributes file is specified using <SourceDocName> then load the VOA as well
                if (parent.AttributesFilePath is string path
                    && path.ToUpperInvariant().Contains("<SOURCEDOCNAME>"))
                {
                    if (await parent.LoadAttributesCommand.CanExecute.FirstOrDefaultAsync().ToTask().ConfigureAwait(false))
                    {
                        await parent.LoadAttributesCommand.Execute().ToTask().ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task SelectSourceDocNameFile()
        {
            string _numberExtensions = String.Join(";", Enumerable.Range(1, 99).Select(page => $"*.{page:D3}"));

            if (_fileBrowserDialogService.SelectExistingFile(
                "Select attributes file",
                "Image files (*.tif;*.pdf,*.001,...)|*.tif;*.tiff;*.pdf;"+ _numberExtensions + "|All files|*.*")
                is string selectedPath)
            {
                DocumentPath = selectedPath;

                await LoadDocumentCommand.Execute();
            }
        }

        private void GoToPrevPage()
        {
            if (CurrentPageNumber is int page)
            {
                CurrentPageNumber = page - 1;
            }
        }

        private void GoToNextPage()
        {
            if (CurrentPageNumber is int page)
            {
                CurrentPageNumber = page + 1;
            }
        }
    }
}
