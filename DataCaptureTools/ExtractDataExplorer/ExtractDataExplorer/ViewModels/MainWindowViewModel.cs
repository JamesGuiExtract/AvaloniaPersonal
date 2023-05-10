using DynamicData;
using Extract;
using Extract.Utilities.WPF;
using ExtractDataExplorer.Models;
using ExtractDataExplorer.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
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
    /// View model for the main window
    /// </summary>
    public sealed class MainWindowViewModel : ViewModelBase
    {
        readonly IFileBrowserDialogService _fileBrowserDialogService;
        readonly IMessageDialogService _messageDialogService;
        readonly IThemingService _themingService;
        readonly IAttributeTreeService _attributeTreeService;
        readonly IExpandPathTagsService _expandPathTagsService;

        readonly ReadOnlyObservableCollection<AttributeTreeViewModel> _attributeViewModels;
        readonly CompositeDisposable _disposables;

        /// <summary>
        /// Whether or not to use the dark theme
        /// </summary>
        [Reactive] public bool DarkMode { get; set; }

        /// <summary>
        /// The 'top-level' attribute trees
        /// </summary>
        public ReadOnlyObservableCollection<AttributeTreeViewModel> Attributes => _attributeViewModels;

        /// </summary>
        /// The selected attribute view model. This can change a lot when a new hierarchy is being loaded so SelectedAttribute
        /// should be used instead for most purposes
        /// </summary>
        [Reactive] public AttributeTreeViewModel? SelectedAttributeFromView { get; set; }

        /// </summary>
        /// The selected attribute view model that is null if <see cref="IsAttributeTreeLoaded"/> is false
        /// </summary>
        [Reactive] public AttributeTreeViewModel? SelectedAttribute { get; set; }

        /// <summary>
        /// The file that the attributes were/will be loaded from (may contain path tags/functions)
        /// </summary>
        [Reactive] public string? AttributesFilePath { get; set; }

        /// <summary>
        /// The file that the attributes were/will be loaded from (with path tags/functions expanded)
        /// </summary>
        [Reactive] public string? AttributesFilePathExpanded { get; set; }

        /// <summary>
        /// The text for the load button
        /// </summary>
        [Reactive] public string LoadAttributesText { get; set; } = "Load";

        /// <summary>
        /// Whether or not the attributes have been loaded
        /// </summary>
        [Reactive] public bool IsAttributeTreeLoaded { get; set; }

        /// <summary>
        /// Whether there is a load or other operation in progress (show wait cursor/loading animation)
        /// </summary>
        [Reactive] public bool IsBusy { get; set; }

        /// <summary>
        /// Negation of <see cref="IsBusy"/>
        /// </summary>
        [ObservableAsProperty] public bool IsReady { get; }

        public DocumentViewModel Document { get; }

        #region Filter

        [Reactive] public string? AttributeQueryFilter { get; set; }

        [Reactive] public bool IsFilterAFQuery { get; set; } = true;

        [Reactive] public bool StartXPathQueryAtElement { get; set; } = true;

        [Reactive] public bool IsFilterXPath { get; set; }

        [Reactive] public string? AttributePageFilter { get; set; }

        [Reactive] public bool IsAttributePageFilterEnabled { get; set; }

        [ObservableAsProperty] public bool IsFilterApplied { get; }

        [ObservableAsProperty] public bool IsFilterChanged { get; }

        [ObservableAsProperty] public FilterRequest ConfiguredAttributeFilter { get; } = FilterRequest.Empty;

        [Reactive] FilterRequest? AppliedAttributeFilter { get; set; }

        [ObservableAsProperty] bool IsFilterEmpty { get; }

        #endregion Filter

        public ReactiveCommand<Unit, Unit> SelectAttributesFileCommand { get; }

        public ReactiveCommand<Unit, Unit> LoadAttributesCommand { get; }

        [ObservableAsProperty] public bool LoadAttributesCommandIsExecuting { get; }

        public ReactiveCommand<Unit, Unit> ApplyAttributeFilterCommand { get; }

        public ReactiveCommand<Unit, Unit> RemoveAttributeFilterCommand { get; }

        public MainWindowViewModel(
            MainWindowModel? serializedModel,
            IFileBrowserDialogService fileBrowserService,
            IMessageDialogService messageDialogService,
            IThemingService themingService,
            IAttributeTreeService attributeTreeService,
            IExpandPathTagsService expandPathTagsService,
            DocumentViewModelFactory documentViewModelFactory,
            CompositeDisposable disposables)
        {
            _ = documentViewModelFactory ?? throw new ArgumentNullException(nameof(documentViewModelFactory));

            _fileBrowserDialogService = fileBrowserService;
            _messageDialogService = messageDialogService;
            _themingService = themingService;
            _attributeTreeService = attributeTreeService;
            _expandPathTagsService = expandPathTagsService;
            _disposables = disposables;

            // Create a DocumentViewModel, optionally from the saved model
            Document = documentViewModelFactory.CreateViewModel(this, serializedModel?.DocumentModel);

            // Set properties from the serialized model
            if (serializedModel is not null)
            {
                DarkMode = serializedModel.DarkMode;
                AttributesFilePath = serializedModel.AttributesFilePath;

                if (serializedModel.AttributeFilter is FilterRequest filter)
                {
                    AttributeQueryFilter = filter.Query;
                    IsFilterXPath = filter.QueryType == AttributeQueryType.XPath;
                    IsFilterAFQuery = !IsFilterXPath;
                    StartXPathQueryAtElement = filter.StartAtElement;
                    AttributePageFilter = filter.PageRange;
                    IsAttributePageFilterEnabled = filter.IsPageFilterEnabled;
                }
                if (serializedModel.IsFilterApplied)
                {
                    AppliedAttributeFilter = serializedModel.AttributeFilter;
                }
            }

            // IsAttributeTreeLoaded set from the service
            _attributeTreeService.AttributesLoaded
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(this, x => x.IsAttributeTreeLoaded);

            // AttributesFilePathExpanded
            this.WhenAnyValue(x => x.Document.DocumentPath, x => x.AttributesFilePath, _expandPathTagsService.ExpandPathTags)
                .BindTo(this, x => x.AttributesFilePathExpanded);

            // IsAttributeTreeLoaded is false when the file path changes
            this.WhenAnyValue(x => x.AttributesFilePathExpanded)
                .Select(_ => false)
                .BindTo(this, x => x.IsAttributeTreeLoaded);

            // SelectedAttribute
            this.WhenAnyValue(x => x.SelectedAttributeFromView, x => x.IsAttributeTreeLoaded,
                (selectedAttribute, areAttributesLoaded) => (selectedAttribute, areAttributesLoaded))
                .Select(x => x.areAttributesLoaded ? x.selectedAttribute : null)
                .BindTo(this, x => x.SelectedAttribute);

            // Change load/reload button text based on the IsAttributeTreeLoaded property
            this.WhenAnyValue(x => x.IsAttributeTreeLoaded)
                .Subscribe(isLoaded => LoadAttributesText = isLoaded ? "Reload" : "Load");

            // AttributeModels -> _attributeViewModels
            _attributeTreeService.AttributeModels
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .TransformToTree(node => node.ParentID)
                .CreateViewModelsFromAttributeTrees(
                    _attributeTreeService.AttributeFilter.ObserveOn(RxApp.MainThreadScheduler),
                    out _attributeViewModels)
                .DisposeWith(_disposables);

            // Change the theme when the DarkMode flag changes
            this.WhenAnyValue(x => x.DarkMode)
                .Subscribe(darkMode => _themingService.SetTheme(darkMode ? Theme.Dark : Theme.Light));

            // ConfiguredAttributeFilter
            this.WhenAnyValue(
                x => x.AttributeQueryFilter,
                x => x.IsFilterXPath,
                x => x.StartXPathQueryAtElement,
                x => x.AttributePageFilter,
                x => x.IsAttributePageFilterEnabled,
                (query, isXPath, startAtElement, pageFilter, isPageFilterEnabled) => new FilterRequest(
                    query: query,
                    queryType: isXPath ? AttributeQueryType.XPath : AttributeQueryType.AFQuery,
                    startAtElement: startAtElement,
                    pageRange: pageFilter,
                    isPageFilterEnabled: isPageFilterEnabled))
                .ToPropertyEx(this, x => x.ConfiguredAttributeFilter);

            // IsFilterEmpty
            this.WhenAnyValue(x => x.ConfiguredAttributeFilter)
                .Select(filter => filter is null || filter.IsEmpty())
                .ToPropertyEx(this, x => x.IsFilterEmpty);

            // IsFilterChanged
            this.WhenAnyValue(
                x => x.ConfiguredAttributeFilter,
                x => x.AppliedAttributeFilter,
                (configured, applied) => configured != applied)
                .ToPropertyEx(this, x => x.IsFilterChanged);

            // IsFilterApplied
            this.WhenAnyValue(x => x.AppliedAttributeFilter)
                .Select(filter => filter is not null)
                .ToPropertyEx(this, x => x.IsFilterApplied);

            // IsReady
            this.WhenAnyValue(x => x.IsBusy)
                .Select(isBusy => !isBusy)
                .ToPropertyEx(this, x => x.IsReady);

            // Commands
            LoadAttributesCommand = ReactiveCommand.CreateFromTask(
                execute: () => _attributeTreeService.LoadAttributesAsync(AttributesFilePathExpanded),
                canExecute: this.WhenAnyValue(x => x.AttributesFilePathExpanded)
                    .Select(maybePath => maybePath is string path && File.Exists(path)));

            LoadAttributesCommand.IsExecuting
                .ToPropertyEx(this, x => x.LoadAttributesCommandIsExecuting);

            LoadAttributesCommand.IsExecuting
                .BindTo(this, x => x.IsBusy);

            SelectAttributesFileCommand = ReactiveCommand.CreateFromTask(
                execute: SelectAttributesFile,
                canExecute: this.WhenAnyValue(x => x.IsReady));

            ApplyAttributeFilterCommand = ReactiveCommand.CreateFromTask(
                execute: ApplyAttributeFilter,
                canExecute: this.WhenAnyValue(
                    x => x.IsReady,
                    x => x.IsAttributeTreeLoaded,
                    x => x.IsFilterEmpty,
                    x => x.IsFilterApplied,
                    x => x.IsFilterChanged,
                    (isReady, isLoaded, isEmpty, isApplied, isChanged) =>
                        isReady && isLoaded && !isEmpty && (isChanged || !isApplied)));

            RemoveAttributeFilterCommand = ReactiveCommand.Create(
                execute: RemoveAttributeFilter,
                canExecute: this.WhenAnyValue(
                    x => x.IsReady,
                    x => x.IsFilterApplied,
                    (isReady, isApplied) => isReady && isApplied));
        }

        internal async Task UpdateFromCommandLineArgsAsync(Options args)
        {
            try
            {
                bool shouldLoadAttributes = false;
                bool shouldLoadDocument = false;

                if (args.VoaFile is string voaFile)
                {
                    AttributesFilePath = voaFile;
                    shouldLoadAttributes = true;
                }
                if (args.DocumentFile is string documentFile)
                {
                    Document.DocumentPath = args.DocumentFile;
                    shouldLoadDocument = true;
                }

                if (args.InputFile is string inputFile)
                {
                    var ext = Path.GetExtension(inputFile).ToUpperInvariant();
                    if (args.VoaFile is null && (ext == ".VOA" || ext == ".EVOA" || ext == ".EAV"))
                    {
                        AttributesFilePath = inputFile;
                        shouldLoadAttributes = true;
                    }
                    else if (args.DocumentFile is null && IsImageFile(inputFile))
                    {
                        Document.DocumentPath = args.InputFile;
                        shouldLoadDocument = true;
                    }
                }

                if (shouldLoadAttributes)
                {
                    await LoadAttributesCommand.Execute();
                }

                if (shouldLoadDocument)
                {
                    await Document.LoadDocumentCommand.Execute();
                }

                if (IsFilterApplied)
                {
                    await ApplyAttributeFilterCommand.Execute();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53934");
            }
        }

        private async Task SelectAttributesFile()
        {
            if (_fileBrowserDialogService.SelectExistingFile(
                "Select attributes file",
                "Attributes files (*.voa;*.evoa;*.eav)|*.voa;*.evoa;*.eav|All files|*.*")
                is string selectedPath)
            {
                AttributesFilePath = selectedPath;
                await LoadAttributesCommand.Execute().ToTask().ConfigureAwait(false);
            }
        }

        private void RemoveAttributeFilter()
        {
            _attributeTreeService.RemoveAttributeFilter();
            AppliedAttributeFilter = null;
        }

        private async Task ApplyAttributeFilter()
        {
            bool filterApplied = await _attributeTreeService
                .ApplyAttributeFilterAsync(ConfiguredAttributeFilter)
                .ConfigureAwait(true);

            AppliedAttributeFilter = filterApplied
                ? ConfiguredAttributeFilter
                : null;
        }
    }
}
