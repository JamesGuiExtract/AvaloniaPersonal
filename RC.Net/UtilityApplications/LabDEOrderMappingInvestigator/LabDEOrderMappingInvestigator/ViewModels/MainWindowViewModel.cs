using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData.Binding;
using LabDEOrderMappingInvestigator.Models;
using LabDEOrderMappingInvestigator.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;

namespace LabDEOrderMappingInvestigator.ViewModels
{
    /// <summary>
    /// A factory to create a <see cref="MainWindowViewModel"/> from a <see cref="MainWindowModel"/> and injected dependencies
    /// </summary>
    public class MainWindowViewModelFactory : IAutoSuspendViewModelFactory<MainWindowViewModel, MainWindowModel>
    {
        readonly IAnalysisService _analysisService;
        readonly IFileBrowserDialogService _fileBrowserService;
        readonly IAFUtility _afutil;
        readonly ICustomerDatabaseService _customerOMDBService;

        public MainWindowViewModelFactory(
            IAnalysisService analysisService,
            IFileBrowserDialogService fileBrowserService,
            IAFUtility afutil,
            ICustomerDatabaseService customerOMDBService)
        {
            _analysisService = analysisService;
            _fileBrowserService = fileBrowserService;
            _afutil = afutil;
            _customerOMDBService = customerOMDBService;
        }

        /// <inheritdoc/>
        public MainWindowViewModel CreateViewModel(MainWindowModel? model = null)
        {
            return new MainWindowViewModel(
                serializedModel: model,
                analysisService: _analysisService,
                fileBrowserService: _fileBrowserService,
                afutil: _afutil,
                customerOMDBService: _customerOMDBService);
        }

        /// <inheritdoc/>
        public MainWindowModel CreateModel(MainWindowViewModel viewModel)
        {
            return new MainWindowModel(
                ProjectPath: viewModel?.ProjectPath,
                DocumentPath: viewModel?.DocumentPath,
                ExpectedDataPathTagFunction: viewModel?.ExpectedDataPathTagFunction,
                FoundDataPathTagFunction: viewModel?.FoundDataPathTagFunction,
                MappingSuggestionsLabTestFilter: viewModel?.MappingSuggestionsLabTestFilter,
                Width: viewModel?.Width,
                Height: viewModel?.Height,
                WindowState: viewModel?.WindowState);
        }
    }

    /// <summary>
    /// The view model for the main window
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        readonly IAFUtility _afutil;
        readonly ICustomerDatabaseService _customerOMDBService;
        readonly IAnalysisService _analysisService;
        readonly IFileBrowserDialogService _fileBrowserService;

        [Reactive]
        public string? ProjectPath { get; set; } = @"C:\Demo_LabDE";
        
        [Reactive]
        public string? DocumentPath { get; set; } = @"C:\Demo_LabDE\Input\A418.tif";

        [ObservableAsProperty]
        public string? DocumentFolder { get; }

        [ObservableAsProperty]
        public string? CustomerOMDBPath { get; }

        [ObservableAsProperty]
        public string? FKBVersion { get; }

        [ObservableAsProperty]
        public string? FKBFolder { get; }

        [ObservableAsProperty]
        public string? ExtractOMDBPath { get; }

        [ObservableAsProperty]
        public string ProjectStatus { get; } = "";

        [ObservableAsProperty]
        public IDictionary<string, int>? DocumentsInFolder { get; }

        [ObservableAsProperty]
        public int DocumentIndex { get; }

        [ObservableAsProperty]
        public string DocumentIndexStatus { get; } = "";

        [ObservableAsProperty]
        public string? PreviousDocumentPath { get; }

        [ObservableAsProperty]
        public string? NextDocumentPath { get; }

        [Reactive]
        public string? ExpectedDataPathTagFunction { get; set; } = @"<SourceDocName>.DataAfterLastVerifyOrQA.voa";

        [ObservableAsProperty]
        public string? ExpectedDataPath { get; }

        [Reactive]
        public string? FoundDataPathTagFunction { get; set; } = @"<SourceDocName>.DataFoundByRules.voa";

        [ObservableAsProperty]
        public string? FoundDataPath { get; }

        [Reactive]
        public OutputMessageViewModelBase? AnalysisResult { get; set; }

        /// <summary>
        /// Current filter used by the child view model, <see cref="MappingSuggestionsOutputMessageViewModel"/> 
        /// </summary>
        public LabTestFilter? MappingSuggestionsLabTestFilter { get; set; }

        /// <summary>
        /// The Window Width
        /// </summary>
        [Reactive]
        public double Width { get; set; }

        /// <summary>
        /// The Window Height
        /// </summary>
        [Reactive]
        public double Height { get; set; }

        /// <summary>
        /// Whether the window is maximized or normal, etc
        /// </summary>
        [Reactive]
        public WindowState WindowState { get; set; }

        public ReactiveCommand<Unit, Unit> SelectProjectPathCommand { get; }
        public ReactiveCommand<Unit, Unit> SelectDocumentPathCommand { get; }
        public ReactiveCommand<Unit, Unit> AnalyzeESComponentMapCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToNextDocumentCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToPreviousDocumentCommand { get; }

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="serializedModel">Optional initial property values</param>
        /// <param name="analysisService">Service to be used to analyze the data</param>
        /// <param name="fileBrowserService">Service to handle browsing for folders/files</param>
        /// <param name="afutil">Service used to expand path tags</param>
        /// <param name="customerOMDBService">Service used to get FKB version info from a database</param>
        public MainWindowViewModel(
            MainWindowModel? serializedModel,
            IAnalysisService analysisService,
            IFileBrowserDialogService fileBrowserService,
            IAFUtility afutil,
            ICustomerDatabaseService customerOMDBService)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _fileBrowserService = fileBrowserService ?? throw new ArgumentNullException(nameof(fileBrowserService));
            _afutil = afutil ?? throw new ArgumentNullException(nameof(afutil));
            _customerOMDBService = customerOMDBService ?? throw new ArgumentNullException(nameof(customerOMDBService));

            // Update properties from the supplied model
            if (serializedModel is not null)
            {
                ProjectPath = serializedModel.ProjectPath;
                DocumentPath = serializedModel.DocumentPath;
                ExpectedDataPathTagFunction = serializedModel.ExpectedDataPathTagFunction;
                FoundDataPathTagFunction = serializedModel.FoundDataPathTagFunction ?? FoundDataPathTagFunction;
                MappingSuggestionsLabTestFilter = serializedModel.MappingSuggestionsLabTestFilter;
                WindowState = serializedModel.WindowState ?? WindowState.Normal;

                // https://github.com/AvaloniaUI/Avalonia/issues/8869
                if (WindowState == WindowState.Maximized)
                {
                    Width = Height = double.NaN;
                }
                else
                {
                    Width = serializedModel.Width ?? 1300;
                    Height = serializedModel.Height ?? 900;
                }
            }

            SetupObservableAsProperties();
            SetupValidationRules();

            // Setup commands now that all the properties have been created
            SelectProjectPathCommand = ReactiveCommand.CreateFromTask(SelectProjectPath);
            SelectDocumentPathCommand = ReactiveCommand.CreateFromTask(SelectDocumentPath);

            GoToNextDocumentCommand = ReactiveCommand.Create(() => { DocumentPath = NextDocumentPath; },
                this.WhenAnyValue(x => x.NextDocumentPath, path => !string.IsNullOrEmpty(path)));

            GoToPreviousDocumentCommand = ReactiveCommand.Create(() => { DocumentPath = PreviousDocumentPath; },
                this.WhenAnyValue(x => x.PreviousDocumentPath, path => !string.IsNullOrEmpty(path)));

            AnalyzeESComponentMapCommand = ReactiveCommand.Create(AnalyzeESComponentMap, this.IsValid());

            // Clear AnalysisResult when any input changes
            this.WhenAnyPropertyChanged(nameof(ProjectPath), nameof(DocumentPath), nameof(ExpectedDataPath), nameof(FoundDataPath))
                .Select(_ => default(OutputMessageViewModelBase))
                .BindTo(this, x => x.AnalysisResult);

            // Update the MappingSuggestionsLabTestFilter when it is changed by a user so that their last settings will be saved
            MessageBus.Current.Listen<LabTestFilter>().BindTo(this, x => x.MappingSuggestionsLabTestFilter);
        }

        // Setup observables for computed properties (OAPH)
        void SetupObservableAsProperties()
        {
            // CustomerOMDBPath
            this.WhenAnyValue(x => x.ProjectPath,
                project =>
                {
                    project = TrimPath(project);
                    if (project is null)
                    {
                        return null;
                    }

                    return Path.Combine(project, "Solution", "Database Files", "OrderMappingDB.sqlite");
                })
                .ToPropertyEx(this, x => x.CustomerOMDBPath);

            // FKBVersion
            this.WhenAnyValue(x => x.ProjectPath,
                project =>
                {
                    project = TrimPath(project);
                    if (project is null || !project.IsFullyQualifiedExistingFolder())
                    {
                        return null;
                    }

                    string mainRSDPath = Path.Combine(project, "Solution", "Main.rsd");
                    if (!File.Exists(mainRSDPath))
                    {
                        mainRSDPath += ".etf";
                    }

                    string? fkbVersion = null;
                    if (File.Exists(mainRSDPath))
                    {
                        RuleSetClass mainRSD = new();
                        mainRSD.LoadFrom(mainRSDPath, false);
                        fkbVersion = string.IsNullOrEmpty(mainRSD.FKBVersion) ? null : mainRSD.FKBVersion;
                    }

                    if (fkbVersion is null && CustomerOMDBPath is not null)
                    {
                        string? fkbInDatabase = _customerOMDBService.GetFKBVersion(CustomerOMDBPath);
                        fkbVersion = string.IsNullOrEmpty(fkbInDatabase) ? null : fkbInDatabase;
                    }

                    return fkbVersion;
                })
                .ToPropertyEx(this, x => x.FKBVersion);

            // FKBFolder
            this.WhenAnyValue(x => x.FKBVersion,
                version =>
                {
                    try
                    {
                        AFDocumentClass doc = new()
                        {
                            FKBVersion = version ?? "Latest"
                        };

                        return _afutil.ExpandTagsAndFunctions(@"<ComponentDataDir>", doc);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .ToPropertyEx(this, x => x.FKBFolder);

            // ExtractOMDBPath
            this.WhenAnyValue(x => x.FKBFolder,
                folder =>
                {
                    if (folder is null)
                    {
                        return null;
                    }

                    return Path.Combine(folder, "LabDE", "TestResults", "OrderMapper", "OrderMappingDB.sqlite");
                })
                .ToPropertyEx(this, x => x.ExtractOMDBPath);

            // ProjectStatus
            this.WhenAnyValue(x => x.FKBVersion, x => x.FKBFolder,
                (version, folder) => $"FKB version: {version ?? "Unknown"} ({folder ?? "Not found"})")
                .ToPropertyEx(this, x => x.ProjectStatus);

            // DocumentFolder
            this.WhenAnyValue(x => x.DocumentPath,
                document =>
                {
                    document = TrimPath(document);
                    if (document is null || !document.IsFullyQualifiedExistingFile())
                    {
                        return null;
                    }

                    return Path.GetDirectoryName(Path.GetFullPath(document));
                })
                .ToPropertyEx(this, x => x.DocumentFolder);

            // DocumentsInFolder
            this.WhenAnyValue(x => x.DocumentFolder, folder =>
                {
                    if (folder is null || !Directory.Exists(folder))
                    {
                        return null;
                    }

                    var dict = new Dictionary<string, int>(Directory.EnumerateFiles(folder)
                        .Where(path => Regex.IsMatch(path, @"(?i)\.(tiff?|pdf|\d{3})$"))
                        .OrderBy(path => path, new StringLogicalComparer())
                        .Select((path, i) => new KeyValuePair<string, int>(path, i)), StringComparer.OrdinalIgnoreCase);
                    return dict;
                })
                .ToPropertyEx(this, x => x.DocumentsInFolder);

            // DocumentIndex
            this.WhenAnyValue(x => x.DocumentPath, x => x.DocumentsInFolder,
                (documentPath, files) =>
                {
                    if (string.IsNullOrEmpty(documentPath) || files is null || files.Count == 0)
                    {
                        return -1;
                    }

                    documentPath = Path.GetFullPath(documentPath);
                    if (files.TryGetValue(documentPath, out int index))
                    {
                        return index;
                    }

                    return -1;
                })
                .ToPropertyEx(this, x => x.DocumentIndex);

            // DocumentIndexStatus
            this.WhenAnyValue(x => x.DocumentIndex, x => x.DocumentsInFolder,
                (documentIdx, files) =>
                {
                    if (documentIdx < 0 || files is null || files.Count == 0)
                    {
                        return "";
                    }

                    return $"Document {documentIdx + 1} of {files.Count}";

                })
                .ToPropertyEx(this, x => x.DocumentIndexStatus);

            // PreviousDocumentPath
            this.WhenAnyValue(x => x.DocumentsInFolder, x => x.DocumentIndex, (files, currentIdx) =>
                {
                    if (files is not null && currentIdx > 0 && currentIdx <= files.Count)
                    {
                        return files?.ElementAt(currentIdx - 1).Key;
                    }
                    return null;
                })
                .ToPropertyEx(this, x => x.PreviousDocumentPath);

            // NextDocumentPath
            this.WhenAnyValue(x => x.DocumentsInFolder, x => x.DocumentIndex, (files, currentIdx) =>
                {
                    if (files is not null && currentIdx > -1 && currentIdx < (files.Count - 1))
                    {
                        return files.ElementAt(currentIdx + 1).Key;
                    }
                    return null;
                })
                .ToPropertyEx(this, x => x.NextDocumentPath);

            // ExpectedDataPath
            this.WhenAnyValue(x => x.DocumentPath, x => x.ExpectedDataPathTagFunction,
                (document, expected) =>
                {
                    document = TrimPath(document);
                    expected = TrimPath(expected);
                    if (document is null || expected is null)
                    {
                        return null;
                    }

                    return ExpandPathTagsAndFunctions(document, expected);
                })
                .ToPropertyEx(this, x => x.ExpectedDataPath);

            // FoundDataPath
            this.WhenAnyValue(x => x.DocumentPath, x => x.FoundDataPathTagFunction,
                (document, found) =>
                {
                    document = TrimPath(document);
                    found = TrimPath(found);
                    if (document is null || found is null)
                    {
                        return null;
                    }

                    return ExpandPathTagsAndFunctions(document, found);
                })
                .ToPropertyEx(this, x => x.FoundDataPath);
        }

        // Expand <SourceDocName>, $DirOf(), etc
        string? ExpandPathTagsAndFunctions(string documentPath, string pathTagFunction)
        {
            AFDocumentClass doc = new();
            doc.Text.SourceDocName = documentPath;

            try
            {
                return TrimPath(_afutil.ExpandTagsAndFunctions(pathTagFunction, doc));
            }
            catch
            {
                // Path tag function is probably malformed
                return null;
            }
        }

        // Info about whether (and why not) the project is valid
        record ProjectValidity(bool PathIsEmpty, bool ProjectExists, bool CustomerOMDBExists, bool ExtractOMDBExists);

        // Validation rules for the input fields
        void SetupValidationRules()
        {
            // Project path rules
            string relativeOMDBPath = @"Solution\Database Files\OrderMappingDB.sqlite";
            var projectPathObservable =
                Observable.CombineLatest
                ( this.WhenAnyValue(x => x.ProjectPath)
                , this.WhenAnyValue(x => x.CustomerOMDBPath)
                , this.WhenAnyValue(x => x.ExtractOMDBPath)
                , Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(30), AvaloniaScheduler.Instance) // Re-check the path periodically
                , (project, customerDB, extractDB, _) =>
                {
                    project = TrimPath(project);
                    if (project is null)
                    {
                        return new ProjectValidity(
                            PathIsEmpty: true,
                            ProjectExists: false,
                            CustomerOMDBExists: false,
                            ExtractOMDBExists: false);
                    }

                    // File.Exists is very slow if the folder it is based on looks like \\abc
                    // so skip testing whether customerDB exists if the project folder is missing
                    bool projectExists = project.IsFullyQualifiedExistingFolder();

                    return new ProjectValidity(
                        PathIsEmpty: false,
                        ProjectExists: projectExists,
                        CustomerOMDBExists: projectExists && File.Exists(customerDB),
                        ExtractOMDBExists: extractDB is not null && File.Exists(extractDB));
                });

            this.ValidationRule(
                x => x.ProjectPath,
                projectPathObservable,
                state => state.CustomerOMDBExists && state.ExtractOMDBExists,
                state =>
                {
                    if (state.PathIsEmpty)
                    {
                        return "Project path cannot be empty!";
                    }
                    else if (!state.ProjectExists)
                    {
                        return "Project path does not exist!";
                    }
                    else if (!state.CustomerOMDBExists)
                    {
                        return $"{relativeOMDBPath} does not exist in the selected project folder!";
                    }
                    else
                    {
                        return $"Cannot find the Extract order mapping database for FKB: {FKBVersion}";
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
            var selectedFolder = await _fileBrowserService.SelectFolder("Select project folder", ProjectPath).ConfigureAwait(true);

            if (selectedFolder.HasValue)
            {
                ProjectPath = selectedFolder.Value;
            }
        }

        // Opens a file browser to select the source document path
        async Task SelectDocumentPath()
        {
            var selectedFile = await _fileBrowserService
                .SelectExistingFile("Select source document", "Image files (*.tif;*.pdf)|tif;pdf|All files (*.*)|*", DocumentFolder)
                .ConfigureAwait(true);

            if (selectedFile.HasValue)
            {
                DocumentPath = selectedFile.Value;
            }
        }

        // Run the analysis of the customer order mapping database w.r.t the local-to-URS test mappings
        void AnalyzeESComponentMap()
        {
            try
            {
                // Trim the paths to remove quotes, etc. Update via the view model properties so that the UI shows the trimmed paths
                ProjectPath = TrimPath(ProjectPath) ?? "";
                DocumentPath = TrimPath(DocumentPath) ?? "";

                (CustomerOMDBPath is not null).Assert($"Logic error: {nameof(CustomerOMDBPath)} is null!");
                (ExtractOMDBPath is not null).Assert($"Logic error: {nameof(ExtractOMDBPath)} is null!");

                AnalysisResult = new TextOutputMessageViewModel("Processing...");

                // Timer so that the Processing... text displays (TODO: Figure out COM error and get this working with another thread/task!)
                Observable.Timer(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance)
                    .Subscribe(_ =>
                    {
                        try
                        {
                            AnalysisResult = _analysisService.AnalyzeESComponentMap(new(
                                CustomerOMDBPath: CustomerOMDBPath,
                                ExtractOMDBPath: ExtractOMDBPath,
                                SourceDocName: DocumentPath,
                                ExpectedDataPath: TrimPath(ExpectedDataPath) ?? "",
                                FoundDataPath: TrimPath(FoundDataPath) ?? "",
                                InitialLabTestFilter: MappingSuggestionsLabTestFilter));
                        }
                        catch (Exception e)
                        {
                            AnalysisResult = new ErrorOutputMessageViewModel(e.Message);
                        }
                    });
            }
            catch (Exception e)
            {
                AnalysisResult = new ErrorOutputMessageViewModel(e.Message);
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
