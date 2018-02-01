using Extract.AttributeFinder;
using Extract.AttributeFinder.Rules;
using Extract.Utilities;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.UtilityApplications.NERAnnotator
{
    public class NERAnnotator
    {
        #region Fields

        Random _rng;
        Tokenizer _tokenizer;
        SentenceDetectorME _sentenceDetector;
        UCLID_AFUTILSLib.AFUtilityClass _afutil = new UCLID_AFUTILSLib.AFUtilityClass();
        SpatialStringSearcher _searcher = new SpatialStringSearcher();

        // Function to return character indexes that an attribute's value overlaps
        Func<IEnumerable<RasterZone>, SpatialString, bool, HashSet<int>> _getCharIndexesMemoized;

        Settings _settings;
        Action<StatusArgs> _updateStatus;
        CancellationToken _cancellationToken;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Create an instance. Private so that status updates and cancellationTokens are not reused.
        /// </summary>
        /// <param name="settings">The <see cref="Settings"/> to be used.</param>
        /// <param name="updateStatus">The action to call with status updates.</param>
        /// <param name="cancellationToken">The token to be checked for cancellation requests</param>
        private NERAnnotator(Settings settings, Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            _settings = settings;
            _updateStatus = updateStatus;
            _cancellationToken = cancellationToken;
            _searcher.SetIncludeDataOnBoundary(true);
            _searcher.SetBoundaryResolution(ESpatialEntity.kCharacter);
        }

        #endregion Constructors


        #region Public Methods

        /// <summary>
        /// Runs the annotation process
        /// </summary>
        /// <param name="settings">The settings to use for annotation</param>
        /// <param name="updateStatus">Action to be used to pass status updates back to the caller</param>
        /// <param name="cancellationToken">Token to be used by the caller to cancel the processing</param>
        public static void Process(Settings settings, Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                new NERAnnotator(settings, updateStatus, cancellationToken).Process();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44901");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Processes the files-to-be-annotated
        /// </summary>
        void Process()
        {
            string previousDirectory = Directory.GetCurrentDirectory();
            try
            {
                var pathTags = new AttributeFinderPathTags { Document = new AFDocument() };

                // Update working dir to match the setting location
                Directory.SetCurrentDirectory(_settings.WorkingDir);

                _rng = _settings.RandomSeedForPageInclusion.HasValue
                    ? new Random(_settings.RandomSeedForPageInclusion.Value)
                    : new Random();

                CheckOutputFile(_settings.OutputFileBaseName + ".train.txt");

                _updateStatus(new StatusArgs { StatusMessage = "Getting input files..." });

                (string, int)[] trainingFiles = null;
                (string, int)[] testingFiles = null;

                if (_settings.TestingSet == TestingSetType.RandomlyPickedFromTrainingSet)
                {
                    trainingFiles = GetInputFiles(_settings.TrainingInput, getPages: true);

                    // Make a separate random generator for splitting the input
                    var rng = _settings.RandomSeedForSetDivision.HasValue
                        ? new Random(_settings.RandomSeedForSetDivision.Value)
                        : new Random();
                    CollectionMethods.Shuffle(trainingFiles, rng);
                    var temp = trainingFiles;
                    int testingSetSize = temp.Length * _settings.PercentToUseForTestingSet / 100;
                    testingFiles = new(string ussPath, int page)[testingSetSize];
                    trainingFiles = new(string ussPath, int page)[temp.Length - testingSetSize];
                    Array.Copy(temp, testingFiles, testingSetSize);
                    Array.Copy(temp, testingSetSize, trainingFiles, 0, trainingFiles.Length);
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
                    CheckOutputFile(_settings.OutputFileBaseName + ".test.txt");
                }

                if (_settings.Format == NamedEntityRecognizer.OpenNLP)
                {
                    if (_settings.TokenizerType == OpenNlpTokenizer.WhiteSpaceTokenizer)
                    {
                        _tokenizer = WhitespaceTokenizer.INSTANCE;
                    }
                    else if (_settings.TokenizerType == OpenNlpTokenizer.SimpleTokenizer)
                    {
                        _tokenizer = SimpleTokenizer.INSTANCE;
                    }
                    else
                    {
                        _tokenizer = new TokenizerME(NERFinder.GetModel(
                            pathTags.Expand(_settings.TokenizerModelPath),
                            modelIn => new TokenizerModel(modelIn)));
                    }

                    if (_settings.SplitIntoSentences)
                    {
                        _sentenceDetector = new SentenceDetectorME(NERFinder.GetModel(
                            pathTags.Expand(_settings.SentenceDetectionModelPath),
                            modelIn => new SentenceModel(modelIn)));
                    }
                }

                ProcessInput(trainingFiles, true);

                if (testingFiles != null)
                    ProcessInput(testingFiles, false);

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
            if (_settings.FailIfOutputFileExists && File.Exists(filename))
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
                if (string.IsNullOrWhiteSpace(path))
                {
                    return null;
                }

                // Folder
                else if (Directory.Exists(path))
                {
                    if (getPages)
                    {
                        SpatialStringClass uss = new SpatialStringClass();
                        return Directory.GetFiles(Path.GetFullPath(path), "*.uss", SearchOption.AllDirectories)
                            .SelectMany(ussPath =>
                            {
                                if (_cancellationToken.IsCancellationRequested)
                                {
                                    throw new OperationCanceledException();
                                }
                                _updateStatus(new StatusArgs { StatusMessage = "Getting input files: {0:N0}", Int32Value = 1 });
                                uss.LoadFrom(ussPath, false);
                                uss.ReportMemoryUsage();
                                return uss.HasSpatialInfo()
                                    ? uss.GetPages(false, "").ToIEnumerable<SpatialString>()
                                        .Select(page => (ussPath, page.GetFirstPageNumber()))
                                    : Enumerable.Empty<(string, int)>();
                            })
                            .ToArray();
                    }
                    else
                    {
                        return Directory.GetFiles(Path.GetFullPath(path), "*.uss", SearchOption.AllDirectories)
                            .Select(ussPath => (ussPath, 0))
                            .ToArray();
                    }
                }

                // File list
                else if (File.Exists(path))
                {
                    if (getPages)
                    {
                        SpatialStringClass uss = new SpatialStringClass();
                        return File.ReadAllLines(Path.GetFullPath(path))
                            .SelectMany(imagePath =>
                            {
                                if (_cancellationToken.IsCancellationRequested)
                                {
                                    throw new OperationCanceledException();
                                }
                                _updateStatus(new StatusArgs { StatusMessage = "Getting input files: {0:N0}", Int32Value = 1 });
                                imagePath = Path.GetFullPath(imagePath.Trim());
                                var ussPath = imagePath.EndsWith(".uss", StringComparison.OrdinalIgnoreCase)
                                    ? imagePath
                                    : imagePath + ".uss";
                                uss.LoadFrom(ussPath, false);
                                uss.ReportMemoryUsage();
                                return uss.HasSpatialInfo()
                                    ? uss.GetPages(false, "").ToIEnumerable<SpatialString>()
                                        .Select(page => (ussPath, page.GetFirstPageNumber()))
                                    : Enumerable.Empty<(string, int)>();
                            })
                            .ToArray();
                    }
                    else
                    {
                        return File.ReadAllLines(Path.GetFullPath(path))
                            .Select(imagePath => (Path.GetFullPath(imagePath.Trim()) + ".uss", 0))
                            .ToArray();
                    }
                }

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44870");
            }
        }

        /// <summary>
        // Process each specified page and append the annotated text to the output file
        /// </summary>
        /// <param name="files">The filename/page number pairs to process</param>
        /// <param name="appendToTrainingSet">Whether to append to the training output file (if true) or the testing output file (if false)</param>
        void ProcessInput((string ussPath, int page)[] files, bool appendToTrainingSet)
        {
            var pathTags = new AttributeFinderPathTags();
            var outputFile = _settings.OutputFileBaseName + (appendToTrainingSet ? ".train.txt" : ".test.txt");
            var uss = new SpatialStringClass();
            var typesVoa = new IUnknownVectorClass();
            foreach (var g in files.GroupBy(t => t.ussPath).OrderBy(g => g.Key))
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var statusMessage = appendToTrainingSet
                    ? "Files processed/skipped for training set: {0:N0} / {1:N0}"
                    : "Files processed/skipped for testing set:  {0:N0} / {1:N0}";

                StringBuilder sb = new StringBuilder();
                var ussPath = g.Key;

                // If a file is specified with 0 as the page number that means all pages should be processed.
                // Pass null to GetTokens for this case
                IEnumerable<int> pages = g.Count() == 1 && g.All(t => t.page == 0)
                    ? null
                    : g.Select(t => t.page).OrderBy(p => p).ToList();

                uss.LoadFrom(ussPath, false);
                uss.ReportMemoryUsage();

                pathTags.Document = new AFDocumentClass { Text = uss };
                var typesVoaFile = pathTags.Expand(_settings.TypesVoaFunction);

                if (!File.Exists(typesVoaFile))
                {
                    // Report a skipped file to the caller
                    _updateStatus(new StatusArgs { StatusMessage = statusMessage, DoubleValues = new double[] { 0, 1 } });
                }
                else
                {
                    typesVoa.LoadFrom(typesVoaFile, false);
                    typesVoa.ReportMemoryUsage();

                    var tokens = GetTokens(uss, pages, typesVoa);

                    // Open NLP format: <START:EntityName> tok1 <END> tok2
                    if (_settings.Format == NamedEntityRecognizer.OpenNLP)
                    {
                        bool startOfSentence = true;
                        foreach (var (token, label, startOfEntity, endOfEntity, endOfSentence, endOfPage) in tokens)
                        {
                            if (!startOfSentence)
                            {
                                sb.Append(" ");
                            }
                            if (startOfEntity)
                            {
                                sb.Append(UtilityMethods.FormatInvariant($"<START:{label}> "));
                            }
                            sb.Append(token);
                            if (endOfEntity)
                            {
                                sb.Append(" <END>");
                            }
                            if (endOfSentence)
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
                    else
                    {
                        throw new ExtractException("ELI45543", "Unsupported NER format: " + _settings.Format.ToString());
                    }
                    _updateStatus(new StatusArgs { StatusMessage = statusMessage, DoubleValues = new double[] { 1, 0 } });
                }

                sb.AppendLine();
                File.AppendAllText(outputFile, sb.ToString());
            }
        }

        /// <summary>
        /// Creates tokens and metadata for the input spatial string
        /// </summary>
        /// <param name="uss">The input string</param>
        /// <param name="pages">The pages to be processed (null for all pages)</param>
        /// <param name="typesVoa">The attributes to be used to annotate the input</param>
        /// <returns>A list of tuples representing each token</returns>
        IEnumerable<AnnotationToken> GetTokens(SpatialStringClass uss, IEnumerable<int> pages, IUnknownVectorClass typesVoa)
        {
            var tokensAndLabels = new List<AnnotationToken>();
            if (pages == null)
            {
                pages = uss.HasSpatialInfo()
                    ? uss.GetPages(false, "").ToIEnumerable<SpatialString>()
                        .Select(page => page.GetFirstPageNumber())
                    : Enumerable.Empty<int>();
            }
            foreach (var pageNum in pages)
            {
                var page = uss.GetSpecifiedPages(pageNum, pageNum);
                _searcher.InitSpatialStringSearcher(page, false);
                _getCharIndexesMemoized = ((Func<IEnumerable<RasterZone>, SpatialString, bool, HashSet<int>>)GetCharIndexes).Memoize();

                // Collect all candidate attributes for this page
                var attributesOnThisPage = typesVoa
                    .ToIEnumerable<IAttribute>()
                    .Where(a => a.EnumerateDepthFirst().Any(c =>
                        c.Value.HasSpatialInfo()
                        && c.Value.GetSpecifiedPages(pageNum, pageNum).HasSpatialInfo()))
                    .ToIUnknownVector();

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
                            IEnumerable<RasterZone> value = null;
                            if (string.Equals(pair.ValueQuery, ".", StringComparison.Ordinal))
                            {
                                value = GetZonesTranslatedToPage(a.Value, page);
                            }
                            else
                            {
                                value = sourceOfLabels
                                    .FindAllOfType<IAttribute>(pair.ValueQuery, a)
                                    .SelectMany(subattr => GetZonesTranslatedToPage(subattr.Value, page))
                                    .ToList();
                            }
                            return (entityZones: value, category: category);
                        }))
                        .ToList();

                // Skip this page, depending on uninteresting-page-inclusion settings
                if (_settings.PercentUninterestingPagesToInclude < 100
                    && !entities.Any()
                    && _rng.Next(1, 101) > _settings.PercentUninterestingPagesToInclude)
                {
                    continue;
                }

                //-----------------------------------------------------------------------------------------------------
                // Get tokens and match them with candidate attributes
                // Prefer longer matches (more tokens)
                //-----------------------------------------------------------------------------------------------------
                var input = page.String;
                var sentenceSpans = _sentenceDetector != null
                    ? _sentenceDetector.sentPosDetect(input)
                    : Enumerable.Repeat(new opennlp.tools.util.Span(0, input.Length), 1);

                int currentSentenceSpan = 0;
                foreach (var sentenceSpan in sentenceSpans)
                {
                    currentSentenceSpan++;
                    List<(int tokenStart, int tokenEndExclusive, string value)> tokenSpans = null;
                    if (_settings.Format == NamedEntityRecognizer.OpenNLP)
                    {
                        var substring = input.Substring(sentenceSpan.getStart(), sentenceSpan.getEnd() - sentenceSpan.getStart());
                        tokenSpans =
                            _tokenizer.tokenizePos(substring)
                            .Select(tok =>
                            {
                                var tokenStart = tok.getStart() + sentenceSpan.getStart();
                                var tokenEndExclusive = tok.getEnd() + sentenceSpan.getStart();
                                return (tokenStart: tokenStart, tokenEndExclusive: tokenEndExclusive,
                                        value: input.Substring(tokenStart, tokenEndExclusive - tokenStart));
                            }).ToList();
                    }
                    else
                    {
                        throw new ExtractException("ELI45542", "Unsupported NER format: " + _settings.Format.ToString());
                    }

                    // Iterate through the tokens. In order to pick the longest match, this algorithm will continue to track overlapping matches until all
                    // open 'tags' (entities) have been closed, even if the index has reached the end of the tokens.
                    Dictionary<(IEnumerable<RasterZone> entityZones, string category), List<int>> openTags = new Dictionary<(IEnumerable<RasterZone>, string), List<int>>();
                    int finalizedTokenCount = 0;
                    for (int i = 0; i <= tokenSpans.Count; i++)
                    {
                        if (openTags.Any())
                        {
                            var (matched, value) = FindMatchesForToken(page, tokenSpans, i, entities, false);
                            var matchedEntities = matched.Select(t => t.entityZones);
                            var openAttributes = openTags.Keys.Select(t => t.entityZones);
                            var intersection = matchedEntities.Intersect(openAttributes);

                            // If no currently open entities overlap with attributes matching the current token
                            // (or if there are no matching attributes for the current token)
                            // then select the longest match and add all the tokens up to and including the tokens that overlap the
                            // longest matching attribute.
                            if (!intersection.Any())
                            {
                                IEnumerable<AnnotationToken> finalizedTokens = FinalizeEntities(openTags, tokenSpans, finalizedTokenCount, currentSentenceSpan, sentenceSpans.Count());

                                tokensAndLabels.AddRange(finalizedTokens);
                                finalizedTokenCount += finalizedTokens.Count();

                                // Reset to begin processing from after the last finalized token
                                i = finalizedTokenCount - 1;
                                openTags = new Dictionary<(IEnumerable<RasterZone> entityZones, string category), List<int>>();
                            }
                            // Else there are on-going entities so just add this token to the
                            // currently open tags collection and continue the loop
                            else
                            {
                                foreach(var m in matched)
                                {
                                    openTags.GetOrAdd(m, () => new List<int>()).Add(i);
                                }
                            }
                        }
                        // If there are no currently open tags then add any matching attributes to the open tags collection or
                        // add the token to the result collection if there are no matches
                        else if (i < tokenSpans.Count)
                        {
                            // Since there is not an ongoing match, use the midpoints of zones to determine overlap. This gives better results for attributes
                            // that slightly overlap tokens. E.g., if an expected redaction includes the top of a number that is on the next line.
                            var (matched, value) = FindMatchesForToken(page, tokenSpans, i, entities, useMidpointToDetermineOverlap: true);

                            if (matched.Any())
                            {
                                foreach(var m in matched)
                                {
                                    openTags[m] = new List<int> { i };
                                }
                            }
                            else
                            {
                                var endOfSentence = i == tokenSpans.Count - 1;
                                var endOfPage = endOfSentence && currentSentenceSpan == sentenceSpans.Count();
                                tokensAndLabels.Add(
                                        new AnnotationToken
                                        {
                                            token = value,
                                            label = null,
                                            startOfEntity = false,
                                            endOfEntity = false,
                                            endOfSentence = endOfSentence,
                                            endOfPage = endOfPage
                                        });
                                finalizedTokenCount++;
                            }
                        }
                    }
                }
            }
            return tokensAndLabels;
        }

        private IEnumerable<AnnotationToken> FinalizeEntities(
            Dictionary<(IEnumerable<RasterZone> entityZones, string category), List<int>> openTags,
            List<(int tokenStart, int tokenEndExclusive, string value)> tokenSpans,
            int finalizedTokenCount, int currentSentenceSpan, int totalSentences)
        {
            var tokensAndLabels = new List<AnnotationToken>();

            var longestMatchSize = openTags.Max(kv => kv.Value.Count);
            var longestMatch = openTags.First(kv => kv.Value.Count == longestMatchSize);
            var category = longestMatch.Key.category;
            var endOfEntity = longestMatch.Value.Last();
            var startOfEntity = longestMatch.Value[0];

            var entitiesTotallyBeforeLongest = openTags.Where(t => t.Value.Last() < startOfEntity);
            if (entitiesTotallyBeforeLongest.Any())
            {
                var prefix = FinalizeEntities(entitiesTotallyBeforeLongest.ToDictionary(kv => kv.Key, kv => kv.Value),
                    tokenSpans, finalizedTokenCount, currentSentenceSpan, totalSentences);
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
                    new AnnotationToken
                    {
                        token = tok.value,
                        label = null,
                        startOfEntity = false,
                        endOfEntity = false,
                        endOfSentence = endOfSentence,
                        endOfPage = endOfPage
                    });
                finalizedTokenCount++;
            }

            // Add the tokens that represent the entity
            for (int i = startOfEntity; i <= endOfEntity; i++)
            {
                var tok = tokenSpans[i];
                var endOfSentence = i == tokenSpans.Count - 1;
                var endOfPage = endOfSentence && currentSentenceSpan == totalSentences;
                tokensAndLabels.Add(
                    new AnnotationToken
                    {
                        token = tok.value,
                        label = category,
                        startOfEntity = i == startOfEntity,
                        endOfEntity = i == endOfEntity,
                        endOfSentence = endOfSentence,
                        endOfPage = endOfPage
                    });
                finalizedTokenCount++;
            }

            var entitiesTotallyAfterLongest = openTags.Where(t => t.Value[0] > endOfEntity);
            if (entitiesTotallyAfterLongest.Any())
            {
                var prefix = FinalizeEntities(entitiesTotallyAfterLongest.ToDictionary(kv => kv.Key, kv => kv.Value),
                    tokenSpans, finalizedTokenCount, currentSentenceSpan, totalSentences);
                tokensAndLabels.AddRange(prefix);
                finalizedTokenCount += prefix.Count();
            }

            return tokensAndLabels;
        }

        // Method to return matching attributes and token text given a collection of token spans and an index.
        // Returns an empty collection and null token text if there are no matches
        (IEnumerable<(IEnumerable<RasterZone> entityZones, string category)> matched, string value)
        FindMatchesForToken(
                    SpatialString page,
                    List<(int tokenStart, int tokenEndExclusive, string value)> tokensSpans,
                    int i,
                    List<(IEnumerable<RasterZone> entityZones, string category)> attributes,
                    bool useMidpointToDetermineOverlap)
        {
            var matched = Enumerable.Empty<(IEnumerable<RasterZone> entityZones, string category)>();
            string value = null;
            if (i < tokensSpans.Count)
            {
                var t = tokensSpans[i];
                value = t.value;
                matched = attributes.FindAll(pair =>
                {
                    var indexes = _getCharIndexesMemoized(pair.entityZones, page, useMidpointToDetermineOverlap);
                    for (int j = t.tokenStart; j < t.tokenEndExclusive; j++)
                    {
                        if (indexes.Contains(j))
                        {
                            return true;
                        }
                    }
                    return false;
                });
            }
            return (matched, value);
        }


        static IEnumerable<RasterZone> GetZonesTranslatedToPage(SpatialString spatialString,
            SpatialString translateToPage)
        {
            if (spatialString.HasSpatialInfo())
            {
                spatialString.GetPages(false, "").ToIEnumerable<SpatialString>();
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
        HashSet<int> GetCharIndexes(IEnumerable<RasterZone> entityZones, SpatialString sourceString, bool useMidpoint)
        {
            _searcher.SetUseMidpointsOnly(useMidpoint);
            var indexes = new HashSet<int>();
            foreach (RasterZone rasterZone in entityZones)
            {
                int page = rasterZone.PageNumber;
                LongRectangle bounds = rasterZone.GetRectangularBounds(
                    sourceString.GetOCRImagePageBounds(page));

                var zoneIndexes = _searcher.GetCharacterIndexesInRegion(bounds);
                // Result will be null if there is no OCR result for the region, e.g., handwriting
                if (zoneIndexes != null)
                {
                    foreach (var index in zoneIndexes.ToIEnumerable<int>().OrderBy(i => i).Skip(1))
                    {
                        indexes.Add(index);
                    }
                }
            }
            return indexes;
        }

        #endregion Private Methods
    }

    class AnnotationToken
    {
        public string token;
        public string label;
        public bool startOfEntity;
        public bool endOfEntity;
        public bool endOfSentence;
        public bool endOfPage;

        public void Deconstruct(out string token, out string label, out bool startOfEntity, out bool endOfEntity, out bool endOfSentence, out bool endOfPage)
        {
            token = this.token;
            label = this.label;
            startOfEntity = this.startOfEntity;
            endOfEntity = this.endOfEntity;
            endOfSentence = this.endOfSentence;
            endOfPage = this.endOfPage;
        }

        public void Deconstruct(out string token, out string label)
        {
            token = this.token;
            label = this.label;
        }
    }
}