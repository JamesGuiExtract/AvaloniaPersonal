using Extract.Utilities;
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
using System.Reactive.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;

namespace ExtractDataExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly IFileBrowserDialogService _fileBrowserDialogService;
        readonly IMessageDialogService _messageDialogService;
        readonly IThemingService _themingService;
        readonly IAFUtility _afutil;

        [Reactive] public bool DarkMode { get; set; }

        [Reactive] public string? AttributesFilePath { get; set; }

        [Reactive] public string LoadAttributesText { get; set; } = "Load";

        [Reactive] public IAttributeTreeViewModel? RootAttribute { get; set; }

        [ObservableAsProperty] public IList<IAttributeTreeViewModel>? Attributes { get; set; }

        [ObservableAsProperty] public bool HasAttributes { get; set; }

        public ReactiveCommand<Unit, Unit> SelectAttributesFileCommand { get; }

        public ReactiveCommand<Unit, Unit> LoadAttributesCommand { get; }

        public MainWindowViewModel(
            MainWindowModel? serializedModel,
            IFileBrowserDialogService fileBrowserService,
            IMessageDialogService messageDialogService,
            IThemingService themingService,
            IAFUtility afutil)
        {
            if (serializedModel is not null)
            {
                DarkMode = serializedModel.DarkMode;
                AttributesFilePath = serializedModel.AttributesFilePath;
            }

            _fileBrowserDialogService = fileBrowserService;
            _messageDialogService = messageDialogService;
            _themingService = themingService;
            _afutil = afutil;

            // Attributes
            this.WhenAnyValue(x => x.RootAttribute)
                .Select(tree => tree is null
                    ? Array.Empty<IAttributeTreeViewModel>()
                    : tree.Branches)
                .ToPropertyEx(this, x => x.Attributes);

            // HasAttributes
            this.WhenAnyValue(x => x.RootAttribute)
                .Select(tree => tree is not null)
                .ToPropertyEx(this, x => x.HasAttributes);

            // Clear the loaded attributes and set LoadAttributesText from Reload to Load when the path changes
            this.WhenAnyValue(x => x.AttributesFilePath)
                .Do(_ =>
                {
                    LoadAttributesText = "Load";
                    RootAttribute = null;
                })
                .Subscribe();

            // Change the theme when the DarkMode flag changes
            this.WhenAnyValue(x => x.DarkMode)
                .Do(darkMode => _themingService.SetTheme(darkMode ? Theme.Dark : Theme.Light))
                .Subscribe();

            SelectAttributesFileCommand = ReactiveCommand.Create(SelectAttributesFile);
            LoadAttributesCommand = ReactiveCommand.Create(LoadAttributes,
                this.WhenAnyValue(x => x.AttributesFilePath)
                .Select(maybePath => TrimPath(maybePath) is string path && File.Exists(path)));
        }

        public Result<Unit> UpdateFromCommandLineArgs(Options args)
        {
            bool shouldLoad = false;

            if (args.VoaFile is string voaFile)
            {
                AttributesFilePath = voaFile;
                shouldLoad = true;
            }
            else if (args.InputFile is string inputFile)
            {
                var ext = Path.GetExtension(inputFile).ToLowerInvariant();
                if (ext == ".voa" || ext == ".evoa" || ext == ".eav")
                {
                    AttributesFilePath = inputFile;
                    shouldLoad = true;
                }
            }

            if (shouldLoad)
            {
                LoadAttributes();
            }

            return Result.CreateSuccess(Unit.Default);
        }

        private void SelectAttributesFile()
        {
            if (_fileBrowserDialogService.SelectExistingFile(
                "Select attributes file",
                "Attributes files (*.voa;*.evoa;*.eav)|*.voa;*.evoa;*.eav|All files|*.*")
                is string selectedPath)
            {
                AttributesFilePath = selectedPath;
            }
        }

        private void LoadAttributes()
        {
            if (TrimPath(AttributesFilePath) is not string path)
            {
                return;
            }

            var doc = new AFDocumentClass();
            doc.Attribute.SubAttributes = _afutil.GetAttributesFromFile(path);
            RootAttribute = BuildTreeFromAttributeHierarchy(doc.Attribute);
            LoadAttributesText = "Reload";
        }

        private IAttributeTreeViewModel BuildTreeFromAttributeHierarchy(IAttribute attribute)
        {
            return AttributeTreeViewModel.Create(
                value: new AttributeViewModel(
                    name: attribute.Name,
                    value: attribute.Value.String,
                    type: attribute.Type,
                    page: attribute.Value.HasSpatialInfo() ? attribute.Value.GetFirstPageNumber() : 0),
                branches: attribute.SubAttributes
                    .ToIEnumerable<IAttribute>()
                    .Select(BuildTreeFromAttributeHierarchy)
                    .ToArray());
        }

        // Trim whitespace and double-quotes from a path
        private static string? TrimPath(string? path)
        {
            string? trimmed = path?.Trim(' ', '\r', '\n', '\t', '"');
            if (string.IsNullOrEmpty(trimmed))
            {
                return null;
            }

            return trimmed;
        }
    }
}
