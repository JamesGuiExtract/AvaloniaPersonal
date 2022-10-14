using Avalonia.Threading;
using DynamicData.Binding;
using LabDEOrderMappingInvestigator.Services;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Splat;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;

namespace LabDEOrderMappingInvestigator.ViewModels
{
    [DataContract]
    public class MainWindowViewModel : ViewModelBase
    {
        readonly IAFUtility _afutil;
        readonly IAnalysisService _analysisService;
        readonly IFileBrowserDialogService _fileBrowserService;

        [Reactive]
        [DataMember]
        public string? ProjectPath { get; set; } = @"C:\Demo_LabDE";

        [Reactive]
        [DataMember]
        public string? DocumentPath { get; set; } = @"C:\Demo_LabDE\Input\A418.tif";

        [Reactive]
        [DataMember]
        public string? ExpectedDataPathTagFunction { get; set; } = @"<SourceDocName>.DataAfterLastVerifyOrQA.voa";

        [ObservableAsProperty]
        public string? ExpectedDataPath { get; }

        [Reactive]
        public string? StatusMessage { get; set; }

        public ReactiveCommand<Unit, Unit> SelectProjectPathCommand { get; }
        public ReactiveCommand<Unit, Unit> SelectDocumentPathCommand { get; }
        public ReactiveCommand<Unit, Unit> AnalyzeESComponentMapCommand { get; }

        [JsonConstructor]
        public MainWindowViewModel()
            : this(
                  Locator.Current.GetService<IAnalysisService>()!,
                  Locator.Current.GetService<IFileBrowserDialogService>()!,
                  Locator.Current.GetService<IAFUtility>()!)
        { }

        [DependencyInjectionConstructor]
        public MainWindowViewModel(IAnalysisService analysisService, IFileBrowserDialogService fileBrowserService, IAFUtility afutil)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _fileBrowserService = fileBrowserService ?? throw new ArgumentNullException(nameof(fileBrowserService));
            _afutil = afutil ?? throw new ArgumentNullException(nameof(afutil));

            // Clear the status message if any of the inputs change
            this.WhenAnyPropertyChanged(nameof(ProjectPath), nameof(DocumentPath), nameof(ExpectedDataPathTagFunction))
                .Subscribe(_ => StatusMessage = "");

            // Setup file browser commands
            SelectProjectPathCommand = ReactiveCommand.CreateFromTask(SelectProjectPath);
            SelectDocumentPathCommand = ReactiveCommand.CreateFromTask(SelectDocumentPath);

            // Setup the Analyze button's behavior
            AnalyzeESComponentMapCommand = ReactiveCommand.Create(AnalyzeESComponentMap, this.IsValid());

            // Expand the expected data path tag function
            this.WhenAnyValue(x => x.DocumentPath, x => x.ExpectedDataPathTagFunction,
                (document, expected) =>
                {
                    document = TrimPath(document);
                    expected = TrimPath(expected);
                    if (document is null || expected is null)
                    {
                        return null;
                    }

                    AFDocumentClass doc = new();
                    doc.Text.SourceDocName = document;

                    try
                    {
                        return TrimPath(_afutil.ExpandTagsAndFunctions(expected, doc));
                    }
                    catch
                    {
                        // Path tag function is probably malformed
                        return null;
                    }
                })
                .ToPropertyEx(this, x => x.ExpectedDataPath);

            SetupValidationRules();
        }

        // Validation rules for the input fields
        void SetupValidationRules()
        {
            // Project path rules
            string relativeOMDBPath = @"Solution\Database Files\OrderMappingDB.sqlite";
            var projectPathObservable =
                Observable.CombineLatest
                ( this.WhenAnyValue(x => x.ProjectPath)
                , Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(30), AvaloniaScheduler.Instance) // Re-check the path periodically
                , (path, _) =>
                {
                    path = TrimPath(path);
                    if (path is null)
                    {
                        return new { pathIsEmpty = true, pathExists = false, dbExists = false };
                    }

                    bool pathExists = Directory.Exists(path);
                    bool dbExists = pathExists && File.Exists(Path.Combine(path, relativeOMDBPath));

                    return new { pathIsEmpty = false, pathExists, dbExists };
                });

            this.ValidationRule(
                x => x.ProjectPath,
                projectPathObservable,
                state => state.dbExists,
                state =>
                {
                    if (state.pathIsEmpty)
                    {
                        return "Project path cannot be empty!";
                    }
                    else if (!state.pathExists)
                    {
                        return "Project path does not exist!";
                    }
                    else
                    {
                        return $"{relativeOMDBPath} does not exist in the selected project folder!";
                    }
                });

            // Document path rule
            this.ValidationRule(x => x.DocumentPath, path => TrimPath(path) is not null, "Document path cannot be empty!");

            // Expected data rules
            var expectedDataObservable =
                Observable.CombineLatest
                ( this.WhenAnyValue(x => x.ExpectedDataPathTagFunction)
                , this.WhenAnyValue(x => x.ExpectedDataPath)
                , Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(30), AvaloniaScheduler.Instance) // Re-check the path periodically
                , (pathTagFunction, expandedPath, _) =>
                {
                    pathTagFunction = TrimPath(pathTagFunction);
                    expandedPath = TrimPath(expandedPath);
                    if (pathTagFunction is null || expandedPath is null)
                    {
                        return new { pathTagFunctionIsEmpty = pathTagFunction is null, dataFileExists = false};
                    }

                    return new { pathTagFunctionIsEmpty = false, dataFileExists = File.Exists(expandedPath)};
                });

            this.ValidationRule(x => x.ExpectedDataPathTagFunction, expectedDataObservable,
                state => state.dataFileExists,
                state => state.pathTagFunctionIsEmpty ? "Expected data path cannot be empty!" : "Expected data path does not exist!");
        }

        // Opens a folder browser to select the project path
        async Task SelectProjectPath()
        {
            var selectedFolder = await _fileBrowserService.SelectFolder("Select project folder").ConfigureAwait(true);

            if (selectedFolder.HasValue)
            {
                ProjectPath = selectedFolder.Value;
            }
        }

        // Opens a file browser to select the source document path
        async Task SelectDocumentPath()
        {
            var selectedFile = await _fileBrowserService
                .SelectExistingFile("Select source document", "Image files (*.tif;*.pdf)|tif;pdf|All files (*.*)|*")
                .ConfigureAwait(true);

            if (selectedFile.HasValue)
            {
                DocumentPath = selectedFile.Value;
            }
        }

        void AnalyzeESComponentMap()
        {
            try
            {
                // Trim the paths to remove quotes, etc. Update via the view model properties so that the UI shows the trimmed paths
                ProjectPath = TrimPath(ProjectPath) ?? "";
                DocumentPath = TrimPath(DocumentPath) ?? "";

                // Run the analysis
                StatusMessage = _analysisService.AnalyzeESComponentMap(new(ProjectPath, DocumentPath, TrimPath(ExpectedDataPath) ?? ""));
            }
            catch (Exception e)
            {
                StatusMessage = e.ToString();
            }
        }

        // Trim whitespace and double-quotes from a path
        static string? TrimPath(string? path)
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
