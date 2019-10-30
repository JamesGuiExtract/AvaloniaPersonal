using Extract.AttributeFinder;
using Extract.AttributeFinder.Rules;
using Extract.Utilities;
using Extract.Utilities.FSharp.NERAnnotation;
using Extract.Utilities.FSharp;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

using GetCharIndexesFunc =
    System.Func<Microsoft.FSharp.Collections.FSharpList<UCLID_RASTERANDOCRMGMTLib.RasterZone>,
        UCLID_RASTERANDOCRMGMTLib.SpatialString,
        bool,
        System.Collections.Generic.HashSet<int>>;

namespace Extract.UtilityApplications.NERAnnotation
{
    public class NERAnnotator : IDisposable
    {
        #region Constants

        static readonly string _GET_FILE_LIST =
            @"SELECT DISTINCT FAMFile.FileName FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
            JOIN FileTaskSession ON AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID
            JOIN FAMFile ON FileTaskSession.FileID = FAMFile.ID
                WHERE Description = @AttributeSetName
                AND AttributeSetForFile.ID >= @FirstIDToProcess
                AND AttributeSetForFile.ID <= @LastIDToProcess";

        static readonly string _GET_VOA_FROM_DB =
            @"SELECT TOP(1) AttributeSetForFile.VOA
            FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
            JOIN FileTaskSession FTS ON AttributeSetForFile.FileTaskSessionID = FTS.ID
            JOIN FAMFile ON FTS.FileID = FAMFile.ID
                WHERE Description = @AttributeSetName
                AND FamFile.FileName = @FileName
            ORDER BY DateTimeStamp DESC";

        // Use enum for easy renaming
        enum _entityFilteringFunctionNames
        {
            setExpectedValuesFromDefinitions,
            resolveToPage,
            limitToFinishable
        }

        /// <summary>
        /// These are the function names that this object looks for
        /// </summary>
        public static ReadOnlyCollection<string> EntityFilteringFunctionNames { get; }
            = new ReadOnlyCollection<string>(new[]
                {
                    nameof(_entityFilteringFunctionNames.setExpectedValuesFromDefinitions),
                    nameof(_entityFilteringFunctionNames.resolveToPage),
                    nameof(_entityFilteringFunctionNames.limitToFinishable)
                });

        #endregion Constants

        #region Fields

        Random _rng;
        ThreadLocal<Tokenizer> _tokenizer;
        ThreadLocal<SentenceDetectorME> _sentenceDetector;
        ThreadLocal<AttributeFinderPathTags> _pathTags;
        ThreadLocal<SpatialStringSearcher> _searcher = new ThreadLocal<SpatialStringSearcher>(() =>
        {
            var searcher = new SpatialStringSearcher();
            searcher.SetIncludeDataOnBoundary(true);
            searcher.SetBoundaryResolution(ESpatialEntity.kCharacter);
            return searcher;
        });

        Dictionary<string, FSharpFunc<EntitiesAndPage, EntitiesAndPage>> _entityFilteringFunctions;

        NERAnnotatorSettings _settings;
        string _trainingOutputFile;
        string _testingOutputFile;
        Action<StatusArgs> _updateStatus;
        CancellationToken _cancellationToken;
        FSharpFunc<AFDocument, AFDocument> _preprocessingFunction;
        FSharpFunc<string, string> _characterReplacingFunction;
        string _entityFilteringScriptPath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Create an instance. Private so that status updates and cancellationTokens are not reused.
        /// </summary>
        /// <param name="settings">The <see cref="NERAnnotatorSettings"/> to be used.</param>
        /// <param name="updateStatus">The action to call with status updates.</param>
        /// <param name="cancellationToken">The token to be checked for cancellation requests</param>
        private NERAnnotator(NERAnnotatorSettings settings, Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            _settings = settings;
            if (_settings.UseDatabase)
            {
                ExtractException.Assert("ELI45035", "DatabaseServer must be specified when UseDatabase is true",
                    !string.IsNullOrWhiteSpace(_settings.DatabaseServer));
                ExtractException.Assert("ELI45036", "DatabaseName must be specified when UseDatabase is true",
                    !string.IsNullOrWhiteSpace(_settings.DatabaseName));
                ExtractException.Assert("ELI45037", "AttributeSetName must be specified when UseDatabase is true",
                    !string.IsNullOrWhiteSpace(_settings.AttributeSetName));

            }
            else
            {
                if (string.IsNullOrEmpty(_settings.TrainingOutputFileName))
                {
                    _trainingOutputFile = _settings.OutputFileBaseName + ".train.txt";
                }
                else
                {
                    _trainingOutputFile = _settings.TrainingOutputFileName;
                }

                if (string.IsNullOrEmpty(_settings.TestingOutputFileName))
                {
                    _testingOutputFile = _settings.OutputFileBaseName + ".test.txt";
                }
                else
                {
                    _testingOutputFile = _settings.TestingOutputFileName;
                }
            }

            _updateStatus = updateStatus;
            _cancellationToken = cancellationToken;

            Init();
        }

        #endregion Constructors


        #region Public Methods

        /// <summary>
        /// Runs the annotation process
        /// </summary>
        /// <param name="settings">The settings to use for annotation</param>
        /// <param name="updateStatus">Action to be used to pass status updates back to the caller</param>
        /// <param name="cancellationToken">Token to be used by the caller to cancel the processing</param>
        public static void Process(NERAnnotatorSettings settings, Action<StatusArgs> updateStatus, CancellationToken cancellationToken, bool processPagesInParallel = false)
        {
            try
            {
                using (var annotator = new NERAnnotator(settings, updateStatus, cancellationToken))
                {
                    annotator.Process(processPagesInParallel);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44901");
            }
        }

        /// <summary>
        /// Get annotated data for a single file
        /// </summary>
        /// <remarks>For testing purposes</remarks>
        /// <param name="settings">The settings to use for annotation</param>
        /// <param name="ussPath">The uss path to process</param>
        /// <param name="pages">The pages to process. Use null for all pages</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static FSharpOption<(string fileName, string data)> GetRecordsForPages(NERAnnotatorSettings settings, string ussPath, params int[] pages)
        {
            try
            {
                using (var annotator = new NERAnnotator(settings, _ => { }, CancellationToken.None))
                {
                    return annotator.GetRecordsForPages(ussPath, pages, null);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46970");
            }
        }

        /// <summary>
        /// Get labeled tokens using provided helper functions
        /// </summary>
        /// <remarks>For testing purposes</remarks>
        /// <param name="settings">The settings to use for annotation. Helper functions are ignored</param>
        /// <param name="uss">The spatial string to process</param>
        /// <param name="pageNumber">The page number of the spatial string to use</param>
        /// <param name="typesVOA">The expected VOA file to use for source of entities (labeled tokens)</param>
        /// <param name="preprocess">The function to preprocess the page text before other work is done</param>
        /// <param name="setExpectedValuesFromDefinitions">The function used to generate expected textual values for the entities</param>
        /// <param name="resolveToPage">The function used to search for expected value matches on the page</param>
        /// <param name="limitToFinishable">The function used to ensure that text extracted via the typesVOA spatial zones can be translated into correct values (not run on the results of the resolveToPage function)</param>
        public static IEnumerable<LabeledToken> GetTokensForPageFSharp(NERAnnotatorSettings settings, SpatialString uss, int pageNumber, IUnknownVector typesVOA,
            FSharpFunc<AFDocument, AFDocument> preprocess,
            FSharpFunc<EntitiesAndPage, EntitiesAndPage> setExpectedValuesFromDefinitions,
            FSharpFunc<EntitiesAndPage, EntitiesAndPage> resolveToPage,
            FSharpFunc<EntitiesAndPage, EntitiesAndPage> limitToFinishable)
        {
            try
            {
                using (var annotator = new NERAnnotator(settings, _ => { }, CancellationToken.None))
                {
                    return annotator.GetTokensForPage(uss, pageNumber, typesVOA, preprocess, setExpectedValuesFromDefinitions, resolveToPage, limitToFinishable);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46970");
            }
        }

        /// <summary>
        /// Get labeled tokens using provided helper functions
        /// </summary>
        /// <remarks>For testing purposes</remarks>
        /// <param name="settings">The settings to use for annotation. Helper functions are ignored</param>
        /// <param name="uss">The spatial string to process</param>
        /// <param name="pageNumber">The page number of the spatial string to use</param>
        /// <param name="typesVOA">The expected VOA file to use for source of entities (labeled tokens)</param>
        /// <param name="preprocess">The function to preprocess the page text before other work is done</param>
        /// <param name="setExpectedValuesFromDefinitions">The function used to generate expected textual values for the entities</param>
        /// <param name="resolveToPage">The function used to search for expected value matches on the page</param>
        /// <param name="limitToFinishable">The function used to ensure that text extracted via the typesVOA spatial zones can be translated into correct values (not run on the results of the resolveToPage function)</param>
        public static IEnumerable<LabeledToken> GetTokensForPage(NERAnnotatorSettings settings, SpatialString uss, int pageNumber, IUnknownVector typesVOA,
            Func<AFDocument, AFDocument> preprocess,
            Func<EntitiesAndPage, EntitiesAndPage> setExpectedValuesFromDefinitions,
            Func<EntitiesAndPage, EntitiesAndPage> resolveToPage,
            Func<EntitiesAndPage, EntitiesAndPage> limitToFinishable)
        {
            try
            {
                using (var annotator = new NERAnnotator(settings, _ => { }, CancellationToken.None))
                {
                    return annotator.GetTokensForPage(uss, pageNumber, typesVOA, preprocess.ToFSharpFunc(), setExpectedValuesFromDefinitions.ToFSharpFunc(), resolveToPage.ToFSharpFunc(), limitToFinishable.ToFSharpFunc());
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46970");
            }
        }

        #endregion Public Methods

        #region Private Methods

        void Init()
        {
            string previousDirectory = Directory.GetCurrentDirectory();
            try
            {
                string alternateComponentDataDir = null;
                if (_settings.UseDatabase)
                {
                    // Get the alternate FKB dir from the DB
                    var fpdb = new FileProcessingDB
                    {
                        DatabaseServer = _settings.DatabaseServer,
                        DatabaseName = _settings.DatabaseName
                    };
                    alternateComponentDataDir = 
                        fpdb.GetDBInfoSetting("AlternateComponentDataDir", false);
                    fpdb.CloseAllDBConnections();
                }

                // Make a path tags object in order to expand paths to sentence detector and tokenizer model
                // This AFDocument will also be used for script functions, since it will enable them to access the
                // correct component data dir
                _pathTags = new ThreadLocal<AttributeFinderPathTags>(() =>
                {
                    var afdoc = new AFDocument();
                    if (!string.IsNullOrWhiteSpace(_settings.FKBVersion))
                    {
                        afdoc.FKBVersion = _settings.FKBVersion;
                    }
                    if (!string.IsNullOrWhiteSpace(alternateComponentDataDir))
                    {
                        afdoc.AlternateComponentDataDir = alternateComponentDataDir;
                    }

                    // Allow internal RSD files to be run
                    afdoc.PushRSDFileName("NERAnnotator");

                    return new AttributeFinderPathTags { Document = afdoc };
                });

                // Update working dir to match the setting location
                Directory.SetCurrentDirectory(_settings.WorkingDir);

                _rng = _settings.RandomSeedForPageInclusion.HasValue
                    ? new Random(_settings.RandomSeedForPageInclusion.Value)
                    : new Random();

                // Get preprocessing function
                if (_settings.RunPreprocessingFunction)
                {
                    var scriptPath = Path.GetFullPath(_pathTags.Value.Expand(_settings.PreprocessingScript));
                    _preprocessingFunction =
                        FunctionLoader.LoadFunction<AFDocument>(scriptPath, _settings.PreprocessingFunctionName);
                }

                // Load filter functions into the register
                if (_settings.RunEntityFilteringFunctions)
                {
                    _entityFilteringFunctions = new Dictionary<string, FSharpFunc<EntitiesAndPage, EntitiesAndPage>>();
                    _entityFilteringScriptPath = Path.GetFullPath(_pathTags.Value.Expand(_settings.EntityFilteringScript));
                    var entityFilteringFunctions = FunctionLoader.LoadFunctions<EntitiesAndPage>(_entityFilteringScriptPath, EntityFilteringFunctionNames.ToArray());
                    for (int i = 0; i < EntityFilteringFunctionNames.Count; i++)
                    {
                        _entityFilteringFunctions[EntityFilteringFunctionNames[i]] = entityFilteringFunctions[i];
                    }
                }

                // Get char-replacing function
                if (_settings.RunCharacterReplacingFunction)
                {
                    var scriptPath = Path.GetFullPath(_pathTags.Value.Expand(_settings.CharacterReplacingScript));
                    _characterReplacingFunction =
                        FunctionLoader.LoadFunction<string>(scriptPath, _settings.CharacterReplacingFunctionName);
                }

                if (_settings.Format == NamedEntityRecognizer.OpenNLP)
                {
                    if (_settings.TokenizerType == OpenNlpTokenizer.WhiteSpaceTokenizer)
                    {
                        _tokenizer = new ThreadLocal<Tokenizer>(() => WhitespaceTokenizer.INSTANCE);
                    }
                    else if (_settings.TokenizerType == OpenNlpTokenizer.SimpleTokenizer)
                    {
                        _tokenizer = new ThreadLocal<Tokenizer>(() => SimpleTokenizer.INSTANCE);
                    }
                    else
                    {
                        var path = Path.GetFullPath(_pathTags.Value.Expand(_settings.TokenizerModelPath));
                        _tokenizer = new ThreadLocal<Tokenizer>(() => new TokenizerME(NERFinder.GetModel(
                            path,
                            modelIn => new TokenizerModel(modelIn))));
                    }

                    if (_settings.SplitIntoSentences)
                    {
                        var path = Path.GetFullPath(_pathTags.Value.Expand(_settings.SentenceDetectionModelPath));
                        _sentenceDetector = new ThreadLocal<SentenceDetectorME>(() => new SentenceDetectorME(NERFinder.GetModel(
                            path,
                            modelIn => new SentenceModel(modelIn))));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46971");
            }
            finally
            {
                try
                {
                    Directory.SetCurrentDirectory(previousDirectory);
                }
                catch { }
            }
        }

        /// <summary>
        /// Processes the files-to-be-annotated
        /// </summary>
        void Process(bool processPagesInParallel)
        {
            string previousDirectory = Directory.GetCurrentDirectory();
            try
            {
                // Update working dir to match the setting location
                Directory.SetCurrentDirectory(_settings.WorkingDir);

                CheckOutputFile(_trainingOutputFile);

                _updateStatus(new StatusArgs { StatusMessage = "Getting input files..." });

                (string, int)[] trainingFiles = null;
                (string, int)[] testingFiles = null;

                if (_settings.UseDatabase ||
                    _settings.TestingSet == TestingSetType.RandomlyPickedFromTrainingSet)
                {
                    trainingFiles = GetInputFiles(_settings.UseDatabase
                        ? null
                        : _settings.TrainingInput, getPages: true);

                    int testingSetSize = trainingFiles.Length * _settings.PercentToUseForTestingSet / 100;

                    if (testingSetSize > 0)
                    {
                        // Make a separate random generator for splitting the input
                        var rng = _settings.RandomSeedForSetDivision.HasValue
                            ? new Random(_settings.RandomSeedForSetDivision.Value)
                            : new Random();
                        CollectionMethods.Shuffle(trainingFiles, rng);
                        var temp = trainingFiles;
                        testingFiles = new(string ussPath, int page)[testingSetSize];
                        trainingFiles = new(string ussPath, int page)[temp.Length - testingSetSize];
                        Array.Copy(temp, testingFiles, testingSetSize);
                        Array.Copy(temp, testingSetSize, trainingFiles, 0, trainingFiles.Length);
                    }
                }
                else
                {
                    trainingFiles = GetInputFiles(_settings.TrainingInput, getPages: false);
                    testingFiles = _settings.TestingSet == TestingSetType.Specified
                        ? GetInputFiles(_settings.TestingInput, getPages: false)
                        : null;
                }

                ExtractException.Assert("ELI44861", "No training input available", trainingFiles != null);

                if (testingFiles != null && !_settings.OutputSeparateFileForEachCategory)
                {
                    CheckOutputFile(_testingOutputFile);
                }

                ProcessInput(trainingFiles, appendToTrainingSet: true, processPagesInParallel: processPagesInParallel);

                if (testingFiles != null)
                {
                    ProcessInput(testingFiles, appendToTrainingSet: false, processPagesInParallel: processPagesInParallel);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44860");
            }
            finally
            {
                try
                {
                    Directory.SetCurrentDirectory(previousDirectory);
                }
                catch { }
            }
        }

        /// <summary>
        /// Depending on the configuration, checks for output file existence and fails if the file exists
        /// </summary>
        /// <param name="filename"></param>
        void CheckOutputFile(string filename)
        {
            if (!_settings.UseDatabase && _settings.FailIfOutputFileExists && File.Exists(filename))
            {
                var uex = new ExtractException("ELI44859", "Output file exists");
                uex.AddDebugData("Filename", Path.GetFullPath(filename), false);
                throw uex;
            }
        }

        /// <summary>
        /// Gets the input files for the given path
        /// </summary>
        /// <param name="path">The path of a directory or file list of input files</param>
        /// <param name="getPages">Whether to enumerate the pages of each file</param>
        /// <returns>An array of filename to page number, one for each page in the specified input, if <see paramref="getPages"/>, or one per file if not <see paramref="getPages"/></returns>
        (string ussPath, int page)[] GetInputFiles(string path, bool getPages)
        {
            try
            {
                IEnumerable<string> files = null;

                if (_settings.UseDatabase)
                {
                    try
                    {
                        using (var connection = NewSqlDBConnection(enlist: false))
                        {
                            connection.Open();

                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = _GET_FILE_LIST;
                                cmd.Parameters.AddWithValue("@AttributeSetName", _settings.AttributeSetName);
                                cmd.Parameters.AddWithValue("@FirstIDToProcess", _settings.FirstIDToProcess);
                                cmd.Parameters.AddWithValue("@LastIDToProcess", _settings.LastIDToProcess);

                                // Set the timeout so that it waits indefinitely
                                cmd.CommandTimeout = 0;
                                using (var reader = cmd.ExecuteReader())
                                {
                                    files = reader
                                        .Cast<IDataRecord>()
                                        .Select(record => record.GetString(0) + ".uss")
                                        .ToList();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var ee = ex.AsExtract("ELI45027");
                        ee.AddDebugData("Query", _GET_FILE_LIST, true);
                        throw ee;
                    }
                }
                else if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }
                // Folder
                else if (Directory.Exists(path))
                {
                    files = Directory.GetFiles(Path.GetFullPath(path), "*.uss", SearchOption.AllDirectories);
                }
                // File list
                else if (File.Exists(path))
                {
                    files = File.ReadAllLines(Path.GetFullPath(path))
                            .Select(imagePath =>
                            {
                                imagePath = Path.GetFullPath(imagePath.Trim());
                                var ussPath = imagePath.EndsWith(".uss", StringComparison.OrdinalIgnoreCase)
                                    ? imagePath
                                    : imagePath + ".uss";
                                return ussPath;
                            });

                }

                if (getPages)
                {
                    SpatialString uss = new SpatialStringClass();
                    return files.SelectMany(ussPath =>
                        {
                            if (_cancellationToken.IsCancellationRequested)
                            {
                                throw new OperationCanceledException();
                            }
                            _updateStatus(new StatusArgs { StatusMessage = "Getting input files: {0:N0}", Int32Value = 1 });

                            if (File.Exists(ussPath))
                            {
                                uss.LoadFrom(ussPath, false);
                                uss.ReportMemoryUsage();
                                return uss.HasSpatialInfo()
                                    ? uss.GetPages(false, "").ToIEnumerable<SpatialString>()
                                        .Select(page => (ussPath, page.GetFirstPageNumber()))
                                    : Enumerable.Empty<(string, int)>();
                            }
                            else
                            {
                                return Enumerable.Empty<(string, int)>();
                            }
                        })
                        .ToArray();
                }
                else
                {
                    return files
                        .Select(ussPath => (ussPath, 0))
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44870");
            }
        }

        /// <summary>
        /// Process each specified page and append the annotated text to the output file
        /// </summary>
        /// <param name="files">The filename/page number pairs to process</param>
        /// <param name="appendToTrainingSet">Whether to append to the training output file (if true) or the testing output file (if false)</param>
        void ProcessInput((string ussPath, int page)[] files, bool appendToTrainingSet, bool processPagesInParallel)
        {
            try
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var outputFile = appendToTrainingSet ? _trainingOutputFile : _testingOutputFile;
                var statusMessage = appendToTrainingSet
                    ? "Files processed/skipped for training set: {0:N0} / {1:N0}"
                    : "Files processed/skipped for testing set:  {0:N0} / {1:N0}";

                var records = processPagesInParallel ? GetRecordsForInputParallel(files, statusMessage) : GetRecordsForInput(files, statusMessage);
                if (_settings.UseDatabase)
                {
                    var recordsList = records.ToList();

                    using (var connection = NewSqlDBConnection())
                    {
                        connection.Open();
                        try
                        {
                            foreach (var (ussPath, data) in recordsList)
                            {
                                using (var cmd = connection.CreateCommand())
                                {
                                    cmd.CommandText = @"INSERT INTO MLData(MLModelID, FileID, IsTrainingData, DateTimeStamp, Data)
                                    SELECT MLModel.ID, FAMFile.ID, @IsTrainingData, GETDATE(), @Data
                                    FROM MLModel, FAMFILE WHERE MLModel.Name = @ModelName AND FAMFile.FileName = @FileName";
                                    cmd.Parameters.AddWithValue("@IsTrainingData", appendToTrainingSet.ToString());
                                    cmd.Parameters.AddWithValue("@Data", data);
                                    cmd.Parameters.AddWithValue("@ModelName", _settings.ModelName);
                                    cmd.Parameters.AddWithValue("@FileName", ussPath.Substring(0, ussPath.Length - 4));
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new ExtractException("ELI45758", "Unable to write ml data", ex);
                        }
                    }
                }
                else if (processPagesInParallel)
                {
                    // Write extra line as a document separator
                    File.AppendAllLines(outputFile, records.Select(t => t.data));

                    // Write out again with filename as separator for debugging purposes
                    File.AppendAllLines(outputFile + ".WithFileNames.txt", records.Select(t => t.data.TrimEnd() + Environment.NewLine + t.fileName));
                }
                else
                {
                    // Write out results as they are available and save for second file
                    List<(string fileName, string data)> cached = new List<(string fileName, string data)>();
                    File.AppendAllLines(outputFile, records.Select(t =>
                    {
                        cached.Add(t);
                        return t.data;
                    }));

                    // Write out again with filename as separator for debugging purposes
                    File.AppendAllLines(outputFile + ".WithFileNames.txt", cached.Select(t => t.data.TrimEnd() + Environment.NewLine + t.fileName));
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        IEnumerable<(string fileName, string data)> GetRecordsForInput((string ussPath, int page)[] files, string statusMessage)
        {
            return files.GroupBy(t => t.ussPath)
            .OrderBy(g => g.Key)
            .Select(g => GetRecordsForGroup(g, statusMessage))
            .Where(maybe => FSharpOption<(string fileName, string data)>.get_IsSome(maybe))
            .Select(maybe => (fileName: maybe.Value.fileName, data: maybe.Value.data));
        }

        List<(string fileName, string data)> GetRecordsForInputParallel((string ussPath, int page)[] files, string statusMessage)
        {
            return files.GroupBy(t => t.ussPath).AsParallel()
            .Select(g => GetRecordsForGroup(g, statusMessage))
            .Where(maybe => FSharpOption<(string fileName, string data)>.get_IsSome(maybe))
            .Select(maybe => (fileName: maybe.Value.fileName, data: maybe.Value.data))
            .OrderBy(t => t.fileName)
            .ToList();
        }

        FSharpOption<(string fileName, string data)> GetRecordsForGroup(IGrouping<string, (string ussPath, int page)> g, string statusMessage)
        {
            var ussPath = g.Key;
            if (!File.Exists(ussPath))
            {
                // Report a skipped file to the caller
                _updateStatus(new StatusArgs { StatusMessage = statusMessage, DoubleValues = new double[] { 0, 1 } });
                return null;
            }
            // If a file is specified with 0 as the page number that means all pages should be processed.
            // Pass null to GetTokens for this case
            IEnumerable<int> pages = g.Count() == 1 && g.All(t => t.page == 0)
                ? null
                : g.Select(t => t.page).OrderBy(p => p).ToList();

            return GetRecordsForPages(ussPath, pages, statusMessage);
        }

        FSharpOption<(string fileName, string data)> GetRecordsForPages(string ussPath, IEnumerable<int> pages, string statusMessage)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            StringBuilder sb = new StringBuilder();

            if (!File.Exists(ussPath))
            {
                // Report a skipped file to the caller
                _updateStatus(new StatusArgs { StatusMessage = statusMessage, DoubleValues = new double[] { 0, 1 } });
                return FSharpOption<(string, string)>.None;
            }

            SpatialString uss = new SpatialStringClass();
            uss.LoadFrom(ussPath, false);
            uss.ReportMemoryUsage();

            // Update the AFDocument in the thread-local path tags object so that tags in the types VOA file can be expanded
            _pathTags.Value.Document.Text = uss;

            IUnknownVector typesVoa = new IUnknownVectorClass();
            if (_settings.UseDatabase && _settings.UseAttributeSetForTypes)
            {
                typesVoa = GetTypesVoaFromDB(ussPath.Substring(0, ussPath.Length - 4)) ?? typesVoa;
            }
            else
            {
                var typesVoaFile = _pathTags.Value.Expand(_settings.TypesVoaFunction);

                if (!File.Exists(typesVoaFile))
                {
                    // Report a skipped file to the caller
                    _updateStatus(new StatusArgs { StatusMessage = statusMessage, DoubleValues = new double[] { 0, 1 } });
                    return FSharpOption<(string, string)>.None;
                }
                else
                {
                    typesVoa.LoadFrom(typesVoaFile, false);
                    typesVoa.ReportMemoryUsage();
                }
            }

            var tokens = GetLabeledTokens(uss, pages, typesVoa);

            // Open NLP format: <START:EntityName> tok1 <END> tok2
            if (_settings.Format == NamedEntityRecognizer.OpenNLP)
            {
                bool startOfSentence = true;
                foreach (var labeledToken in tokens)
                {
                    if (!startOfSentence)
                    {
                        sb.Append(" ");
                    }
                    if (labeledToken.StartOfEntity)
                    {
                        sb.Append(UtilityMethods.FormatInvariant($"<START:{labeledToken.Label}> "));
                    }
                    sb.Append(labeledToken.Token);
                    if (labeledToken.EndOfEntity)
                    {
                        sb.Append(" <END>");
                    }
                    if (labeledToken.EndOfSentence)
                    {
                        sb.AppendLine();
                        startOfSentence = true;
                    }
                    else
                    {
                        startOfSentence = false;
                    }
                }
            }
            // Stanford format: tok1 EntityName
            //                  tok2 O
            //                  ...
            else
            {
                throw new ExtractException("ELI45543", "Unsupported NER format: " + _settings.Format.ToString());
            }
            _updateStatus(new StatusArgs { StatusMessage = statusMessage, DoubleValues = new double[] { 1, 0 } });

            if (sb.Length == 0)
            {
                return FSharpOption<(string, string)>.None;
            }

            var annotatedText = sb.ToString();

            if (_settings.RunCharacterReplacingFunction)
            {
                annotatedText = _characterReplacingFunction.Invoke(annotatedText);
            }

            return (ussPath, annotatedText);
        }

        /// <summary>
        /// Creates tokens and metadata for the input spatial string
        /// </summary>
        /// <param name="uss">The input string</param>
        /// <param name="pages">The pages to be processed (null for all pages)</param>
        /// <param name="typesVoa">The attributes to be used to annotate the input</param>
        /// <returns>A list of tuples representing each token</returns>
        IEnumerable<LabeledToken> GetLabeledTokens(SpatialString uss, IEnumerable<int> pages, IUnknownVector typesVoa)
        {
            if (pages == null)
            {
                pages = uss.HasSpatialInfo()
                    ? uss.GetPages(false, "").ToIEnumerable<SpatialString>()
                        .Select(page => page.GetFirstPageNumber())
                    : Enumerable.Empty<int>();
            }

            var preprocess = _settings.RunPreprocessingFunction
               ? _preprocessingFunction
               : (FSharpFunc<AFDocument, AFDocument>) Operators.Identity;

            var setExpectedValuesFromDefinitions = GetRegisteredEntityFilteringFunction(nameof(_entityFilteringFunctionNames.setExpectedValuesFromDefinitions));
            var resolveToPage = GetRegisteredEntityFilteringFunction(nameof(_entityFilteringFunctionNames.resolveToPage));
            var limitToFinishable = GetRegisteredEntityFilteringFunction(nameof(_entityFilteringFunctionNames.limitToFinishable));

            List<LabeledToken> tokensAndLabels =
                pages
                .SelectMany(pageNum => GetTokensForPage(uss, pageNum, typesVoa,
                   preprocess,
                   setExpectedValuesFromDefinitions,
                   resolveToPage,
                   limitToFinishable))
                .ToList();

            return tokensAndLabels;
        }

        IEnumerable<LabeledToken> GetTokensForPage(SpatialString uss, int pageNumber, IUnknownVector typesVoa,
            FSharpFunc<AFDocument, AFDocument> preprocess,
            FSharpFunc<EntitiesAndPage, EntitiesAndPage> setExpectedValuesFromDefinitions,
            FSharpFunc<EntitiesAndPage, EntitiesAndPage> resolveToPage,
            FSharpFunc<EntitiesAndPage, EntitiesAndPage> limitToFinishable
            )
        {
            // Clone the AFDocument that has its FKB value set and update the Attribute to have only this page
            var page = _pathTags.Value.Document.PartialClone(false, false);
            page.Attribute = new AttributeClass { Value = uss.GetSpecifiedPages(pageNumber, pageNumber) };

            // Preprocess the page before doing anything with indexes
            page.Text = preprocess.Invoke(page).Text;

            _searcher.Value.InitSpatialStringSearcher(page.Text, false);
            var getCharIndexesMemoized = ((GetCharIndexesFunc)GetCharIndexes).Memoize();

            var entitiesAndPage = GetEntitiesFromDefinition(typesVoa, page);

            entitiesAndPage = setExpectedValuesFromDefinitions.Invoke(entitiesAndPage);

            // Augment and filter out non-spatial
            var expanded = resolveToPage.Invoke(entitiesAndPage).Entities
                .Where(e => e.Zones.Any()).ToList();

            var original = entitiesAndPage.Entities.Where(e => e.Zones.Any()).ToList();

            // Skip this page, depending on uninteresting-page-inclusion settings
            if (original.Count == 0 && expanded.Count == 0 && RandomlyDecide())
            {
                return Enumerable.Empty<LabeledToken>();
            }

            // Filter entities based on whether they can be converted into valid data from the OCR
            // Expanded are assumed to be correctly-findable so don't process these
            var tokenSpanLists = GetTokens(page.Text.String);
            var spatialStringEntities = original
                .Select(e => TryMakeSpatialEntity(e, tokenSpanLists, page.Text, getCharIndexesMemoized))
                .Where(e => e != null)
                .ToFSharpList();

            var limited = limitToFinishable.Invoke(new EntitiesAndPage(spatialStringEntities, page)).Entities;

            var combined = limited.Concat(expanded).ToList();

            // Skip this page, depending on uninteresting-page-inclusion settings
            if (combined.Count == 0 && RandomlyDecide())
            {
                return Enumerable.Empty<LabeledToken>();
            }

            return GetTokensFromEntities(combined, page, tokenSpanLists, getCharIndexesMemoized);
        }

        // Get tokens and match them with candidate attributes
        // Prefer longer matches (more tokens)
        IEnumerable<LabeledToken> GetTokensFromEntities(List<Entity> entities, IAFDocument page, List<List<Token>> tokenSpanLists, GetCharIndexesFunc getCharIndexesMemoized)
        {
            bool isLabelOnPage = false;
            var tokensAndLabelsOnPage = new List<LabeledToken>();


            int currentSentenceSpan = 0;
            foreach (var tokenSpanList in tokenSpanLists)
            {
                currentSentenceSpan++;

                // Iterate through the tokens. In order to pick the longest match, this algorithm will continue to track overlapping matches until all
                // open 'tags' (entities) have been closed, even if the index has reached the end of the tokens.
                Dictionary<Entity, List<int>> openTags = new Dictionary<Entity, List<int>>();
                int finalizedTokenCount = 0;
                for (int i = 0; i <= tokenSpanList.Count; i++)
                {
                    if (openTags.Any())
                    {
                        var (matched, _) = FindMatchesForToken(page.Text, tokenSpanList, i, entities, getCharIndexesMemoized, true);
                        var matchedEntities = matched.Select(t => t.Zones);
                        var openAttributes = openTags.Keys.Select(t => t.Zones);
                        var intersection = matchedEntities.Intersect(openAttributes);

                        // If no currently open entities overlap with attributes matching the current token
                        // (or if there are no matching attributes for the current token)
                        // then select the longest match and add all the tokens up to and including the tokens that overlap the
                        // longest matching attribute.
                        if (!intersection.Any())
                        {
                            List<LabeledToken> finalizedTokens =
                                FinalizeEntities(openTags, tokenSpanList, finalizedTokenCount, currentSentenceSpan, tokenSpanLists.Count(), page.Text);

                            if (finalizedTokens.Where(t => t.Label != null).Count() > 0)
                            {
                                isLabelOnPage = true;
                            }
                            tokensAndLabelsOnPage.AddRange(finalizedTokens);
                            finalizedTokenCount += finalizedTokens.Count();

                            // Reset to begin processing from after the last finalized token
                            i = finalizedTokenCount - 1;
                            openTags = new Dictionary<Entity, List<int>>();
                        }
                        // Else there are on-going entities so just add this token to the
                        // currently open tags collection and continue the loop
                        else
                        {
                            foreach (var m in matched)
                            {
                                openTags.GetOrAdd(m, _ => new List<int>()).Add(i);
                            }
                        }
                    }
                    // If there are no currently open tags then add any matching attributes to the open tags collection or
                    // add the token to the result collection if there are no matches
                    else if (i < tokenSpanList.Count)
                    {
                        var (matched, value) = FindMatchesForToken(page.Text, tokenSpanList, i, entities, getCharIndexesMemoized, false);

                        if (matched.Any())
                        {
                            foreach (var m in matched)
                            {
                                openTags[m] = new List<int> { i };
                            }
                        }
                        else
                        {
                            var endOfSentence = i == tokenSpanList.Count - 1;
                            var endOfPage = endOfSentence && currentSentenceSpan == tokenSpanLists.Count();
                            tokensAndLabelsOnPage.Add(
                                    new LabeledToken
                                    (
                                        token: value,
                                        label: null,
                                        startOfEntity: false,
                                        endOfEntity: false,
                                        endOfSentence: endOfSentence,
                                        endOfPage: endOfPage
                                    ));
                            finalizedTokenCount++;
                        }
                    }
                }
            }

            // Skip page if no label...
            if (!isLabelOnPage && RandomlyDecide())
            {
                return Enumerable.Empty<LabeledToken>();
            }

            return tokensAndLabelsOnPage;
        }

        private FSharpFunc<EntitiesAndPage, EntitiesAndPage> GetRegisteredEntityFilteringFunction(string functionName)
        {
            if (_entityFilteringFunctions != null
                && _entityFilteringFunctions.TryGetValue(functionName, out var fun))
            {
                return fun;
            }

            // Return a function that returns an empty collection for the 'expand' function
            if (functionName == nameof(_entityFilteringFunctionNames.resolveToPage))
            {
                return FSharpFunc<EntitiesAndPage, EntitiesAndPage>.FromConverter(x =>
                    new EntitiesAndPage(entities: ListModule.Empty<Entity>(), page: x.Page));
            }

            return (FSharpFunc<EntitiesAndPage, EntitiesAndPage>) Operators.Identity;
        }

        private bool RandomlyDecide()
        {
            return _settings.PercentUninterestingPagesToInclude < 100
                && _rng.Next(1, 101) > _settings.PercentUninterestingPagesToInclude;
        }

        private EntitiesAndPage GetEntitiesFromDefinition(IUnknownVector attributesOnThisPage, AFDocument page)
        {
            var sourceOfLabels = new XPathContext(attributesOnThisPage);
            var entities = _settings.EntityDefinitions.SelectMany(pair =>
                sourceOfLabels.FindAllOfType<IAttribute>(pair.RootQuery)
                    .Select(a =>
                    {
                        var category = pair.Category;
                        if (pair.CategoryIsXPath)
                        {
                            category = sourceOfLabels
                                .FindAllAsStrings(category, a)
                                .FirstOrDefault();
                        }
                        if (string.Equals(pair.ValueQuery, ".", StringComparison.Ordinal))
                        {
                            return new Entity
                            (
                                expectedValue: FSharpOption<string>.None,
                                valueComponents: FSharpList<IAttribute>.Cons(a, FSharpList<IAttribute>.Empty),
                                zones: GetZonesTranslatedToPage(a.Value, page.Text).ToFSharpList(),
                                spatialString: FSharpOption<SpatialString>.None,
                                category: category
                            );
                        }
                        else
                        {
                            var valueComponents = sourceOfLabels.FindAllOfType<IAttribute>(pair.ValueQuery, a).ToFSharpList();
                            var zones = valueComponents.SelectMany(subattr => GetZonesTranslatedToPage(subattr.Value, page.Text)).ToFSharpList();

                            return new Entity
                            (
                                expectedValue: FSharpOption<string>.None,
                                valueComponents: valueComponents,
                                zones: zones,
                                spatialString: FSharpOption<SpatialString>.None,
                                category: category
                            );
                        }
                    }));

            return new EntitiesAndPage(entities.ToFSharpList(), page);
        }

        // Returns list of sentences, which are made up of token spans
        private List<List<Token>> GetTokens(string input)
        {
            var sentenceSpans = _sentenceDetector != null
                ? _sentenceDetector.Value.sentPosDetect(input)
                : Enumerable.Repeat(new opennlp.tools.util.Span(0, input.Length), 1);

            return sentenceSpans.Select(sentenceSpan =>
            {
                if (_settings.Format == NamedEntityRecognizer.OpenNLP)
                {
                    var substring = input.Substring(sentenceSpan.getStart(), sentenceSpan.getEnd() - sentenceSpan.getStart());
                    return _tokenizer.Value.tokenizePos(substring)
                        .Select(tok =>
                        {
                            var tokenStart = tok.getStart() + sentenceSpan.getStart();
                            var tokenEndExclusive = tok.getEnd() + sentenceSpan.getStart();
                            return new Token
                            {
                                Start = tokenStart,
                                EndExclusive = tokenEndExclusive,
                                Value = input.Substring(tokenStart, tokenEndExclusive - tokenStart)
                            };
                        }).ToList();
                }
                else
                {
                    throw new ExtractException("ELI45542", "Unsupported NER format: " + _settings.Format.ToString());
                }
            }).ToList();
        }

        static Entity TryMakeSpatialEntity(Entity entity,
            List<List<Token>> tokenSpans,
            SpatialString page,
            GetCharIndexesFunc getCharIndexesMemoized)
        {
            var overlappingTokens = tokenSpans.SelectMany(tokens =>
                {
                    bool onGoing = false;
                    return tokens.Where(t => onGoing = DoesEntityOverlapToken(entity, t, page, getCharIndexesMemoized, !onGoing, onGoing));
                })
                .ToList();

            if (overlappingTokens.Count == 0)
            {
                return null;
            }

            var strings = overlappingTokens.Select(token =>
            {
                var startOfEntity = token.Start;
                var endOfEntity = token.EndExclusive - 1;
                if (startOfEntity <= endOfEntity)
                {
                    var substring = page.GetSubString(startOfEntity, endOfEntity);
                    if (substring.HasSpatialInfo())
                    {
                        return substring;
                    }
                }
                return null;
            })
            .Where(s => s != null)
            .ToIUnknownVector();

            if (strings.Size() > 0)
            {
                SpatialString val = new SpatialStringClass();
                val.CreateFromSpatialStrings(strings, false);
                var spatialValue = FSharpOption<SpatialString>.Some(val);
                var zones = val.GetOCRImageRasterZones()
                    .ToIEnumerable<RasterZone>()
                    .ToFSharpList();
                return new Entity(entity.ExpectedValue, zones, entity.ValueComponents, spatialValue, entity.Category);
            }

            return null;
        }


        private List<LabeledToken> FinalizeEntities(
            Dictionary<Entity, List<int>> openTags,
            List<Token> tokenSpans,
            int finalizedTokenCount, int currentSentenceSpan, int totalSentences, SpatialString page)
        {
            var tokensAndLabels = new List<LabeledToken>();

            var longestMatchSize = openTags.Max(kv => kv.Value.Count);
            var longestMatch = openTags.First(kv => kv.Value.Count == longestMatchSize);
            var category = longestMatch.Key.Category;
            var endOfEntity = longestMatch.Value.Last();
            var startOfEntity = longestMatch.Value[0];

            var entitiesTotallyBeforeLongest = openTags.Where(t => t.Value.Last() < startOfEntity);
            if (entitiesTotallyBeforeLongest.Any())
            {
                var prefix = FinalizeEntities(entitiesTotallyBeforeLongest.ToDictionary(kv => kv.Key, kv => kv.Value),
                    tokenSpans, finalizedTokenCount, currentSentenceSpan, totalSentences, page);
                tokensAndLabels.AddRange(prefix);
                finalizedTokenCount += prefix.Count();
            }

            // Add any non-entity tokens up to the start of the entity
            for (int i = finalizedTokenCount; i < startOfEntity; i++)
            {
                var tok = tokenSpans[i];
                var endOfSentence = i == tokenSpans.Count - 1;
                var endOfPage = endOfSentence && currentSentenceSpan == totalSentences;
                tokensAndLabels.Add(
                    new LabeledToken
                    (
                        token: tok.Value,
                        label: null,
                        startOfEntity: false,
                        endOfEntity: false,
                        endOfSentence: endOfSentence,
                        endOfPage: endOfPage
                    ));
                finalizedTokenCount++;
            }

            // Add the tokens that represent the entity
            for (int i = startOfEntity; i <= endOfEntity; i++)
            {
                var tok = tokenSpans[i];
                var endOfSentence = i == tokenSpans.Count - 1;
                var endOfPage = endOfSentence && currentSentenceSpan == totalSentences;
                tokensAndLabels.Add(
                    new LabeledToken
                    (
                        token: tok.Value,
                        label: category,
                        startOfEntity: i == startOfEntity,
                        endOfEntity: i == endOfEntity,
                        endOfSentence: endOfSentence,
                        endOfPage: endOfPage
                    ));
                finalizedTokenCount++;
            }

            var entitiesTotallyAfterLongest = openTags.Where(t => t.Value[0] > endOfEntity);
            if (entitiesTotallyAfterLongest.Any())
            {
                var prefix = FinalizeEntities(entitiesTotallyAfterLongest.ToDictionary(kv => kv.Key, kv => kv.Value),
                    tokenSpans, finalizedTokenCount, currentSentenceSpan, totalSentences, page);
                tokensAndLabels.AddRange(prefix);
                finalizedTokenCount += prefix.Count();
            }

            return tokensAndLabels;
        }

        // Return matching entities and token text given a token index
        // Returns an empty collection and null token text if there are no matches for this index
        // Assumes that the entity's zones are built by including boundary chars
        static (IEnumerable<Entity> matched, string value) FindMatchesForToken(
                    SpatialString page,
                    List<Token> tokensSpans,
                    int i,
                    List<Entity> attributes,
                    GetCharIndexesFunc getCharIndexesMemoized,
                    bool onGoing
                    )
        {
            var matched = Enumerable.Empty<Entity>();
            string value = null;
            if (i < tokensSpans.Count)
            {
                var t = tokensSpans[i];
                value = t.Value;
                matched = attributes.FindAll(entity => DoesEntityOverlapToken(entity, t, page, getCharIndexesMemoized, false, onGoing));
            }
            return (matched, value);
        }

        static bool DoesEntityOverlapToken(
            Entity entity,
            Token token,
            SpatialString page,
            GetCharIndexesFunc getCharIndexesMemoized,
            bool includeIntersecting,
            bool onGoing)
        {
            var indexes = getCharIndexesMemoized(entity.Zones, page, includeIntersecting);

            var tokenLength = token.EndExclusive - token.Start;
            var indexesThisToken = Enumerable.Range(token.Start, tokenLength)
                .Where(i => indexes.Contains(i))
                .ToList();

            // Return true if this token has chars that overlap the entity zones
            if (tokenLength == 1
                && indexesThisToken.Count == 1
                || indexesThisToken.Count > 1)
            {
                return true;
            }

            // Return true if this token is surrounded by overlapping tokens and is close to one of them
            if (indexes.Any() && onGoing)
            {
                int min = indexes.Min();
                int max = indexes.Max();
                if (token.Start >= min && (token.EndExclusive - 1) <= max)
                {
                    var indexesBefore = indexes
                        .Where(i => i < token.Start)
                        .ToList();
                    indexesBefore.Sort();
                    var indexesAfter = indexes
                        .Where(i => i >= token.EndExclusive)
                        .ToList();
                    indexesAfter.Sort();
                    if (indexesBefore.Count > 1 && indexesAfter.Count > 1)
                    {
                        var isTokenCloseToOverlappingTokens =
                            indexesBefore[indexesBefore.Count - 1] >= token.Start - 4
                            || indexesAfter[0] < token.EndExclusive + 4;

                        return isTokenCloseToOverlappingTokens;
                    }
                }
            }

            return false;
        }

        static IEnumerable<RasterZone> GetZonesTranslatedToPage(SpatialString spatialString,
            SpatialString translateToPage)
        {
            if (spatialString.HasSpatialInfo() && translateToPage.HasSpatialInfo())
            {
                // Loop through each page of the attribute.
                foreach (SpatialString pageText in
                    spatialString.GetPages(false, "").ToIEnumerable<SpatialString>())
                {
                    // Don't process any pages without spatial info.
                    int page = pageText.GetFirstPageNumber();
                    if (translateToPage.GetSpecifiedPages(page, page).IsEmpty())
                    {
                        continue;
                    }

                    foreach (RasterZone rasterZone in
                        pageText.GetTranslatedImageRasterZones(translateToPage.SpatialPageInfos)
                            .ToIEnumerable<RasterZone>())
                    {
                        yield return rasterZone;
                    }
                }
            }
        }

        // A to-be-memoized function to return character indexes that an attribute's value overlaps
        HashSet<int> GetCharIndexes(FSharpList<RasterZone> entityZones, SpatialString sourceString, bool includeIntersecting)
        {
            _searcher.Value.SetUseMidpointsOnly(true);
            _searcher.Value.SetIncludeDataOnBoundary(includeIntersecting);
            var indexes = new HashSet<int>();
            foreach (RasterZone rasterZone in entityZones)
            {
                int page = rasterZone.PageNumber;
                LongRectangle bounds = rasterZone.GetRectangularBounds(
                    sourceString.GetOCRImagePageBounds(page));

                var zoneIndexes = _searcher.Value.GetCharacterIndexesInRegion(bounds);
                // Result will be null if there is no OCR result for the region, e.g., handwriting
                if (zoneIndexes != null)
                {
                    foreach (var index in zoneIndexes.ToIEnumerable<int>().OrderBy(i => i))
                    {
                        indexes.Add(index);
                    }
                }
            }
            return indexes;
        }

        IUnknownVector GetTypesVoaFromDB(string imageName)
        {
            // Because this is called via a yielding method, it gets nested into another sql connection
            // Set enlist to false to prevent the transaction from escalating to a distributed transaction
            // (which requires the MSDTC service to be running)
            // (This is not an update command anyway so no need to be in the transaction...)
            using (var connection = NewSqlDBConnection(enlist: false))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = _GET_VOA_FROM_DB;
                cmd.Parameters.AddWithValue("@AttributeSetName", _settings.AttributeSetName);
                cmd.Parameters.AddWithValue("@FileName", imageName);

                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {

                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        using (var stream = reader.GetStream(0))
                        {
                            var voa = AttributeMethods.GetVectorOfAttributesFromSqlBinary(stream);
                            voa.ReportMemoryUsage();
                            return voa;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a connection to the configured database
        /// </summary>
        /// <param name="enlist">Whether to enlist in a transaction scope if there is one</param>
        SqlConnection NewSqlDBConnection(bool enlist = true)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = _settings.DatabaseServer;
            sqlConnectionBuild.InitialCatalog = _settings.DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            sqlConnectionBuild.Enlist = enlist;
            return new SqlConnection( sqlConnectionBuild.ConnectionString);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tokenizer != null)
                {
                    _tokenizer.Dispose();
                    _tokenizer = null;
                }

                if (_sentenceDetector != null)
                {
                    _sentenceDetector.Dispose();
                    _sentenceDetector = null;
                }

                if (_searcher != null)
                {
                    _searcher.Dispose();
                    _searcher = null;
                }

                if (_pathTags != null)
                {
                    _pathTags.Dispose();
                    _pathTags = null;
                }
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #endregion Private Methods
    }

    internal class Token
    {
        public int Start { get; set; }
        public int EndExclusive { get; set; }
        public string Value { get; set; }
    }
}
