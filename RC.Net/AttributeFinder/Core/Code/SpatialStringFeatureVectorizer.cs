using Extract.Utilities;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Shingle;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UCLID_RASTERANDOCRMGMTLib;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// An IFeatureVectorizer that produces feature vectors from <see cref="ISpatialString"/>s
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    public class SpatialStringFeatureVectorizer : IFeatureVectorizer
    {
        #region Private Fields

        /// <summary>
        /// Bag-of-words object used to create this object's feature vector
        /// </summary>
        private Accord.MachineLearning.BagOfWords _bagOfWords;

        /// <summary>
        /// A string representation of pages to be processed, e.g., 1-2 or -1
        /// If computing pagination feature vectors then no pages should be specified.
        /// </summary>
        private string _pagesToProcess;

        /// <summary>
        /// The name of this feature vectorizer
        /// </summary>
        private string _name;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="SpatialStringFeatureVectorizer"/>
        /// </summary>
        /// <param name="pagesToProcess">The string representation of which pages to process
        /// to compute the feature vector. If empty or null then all pages will be used.
        /// If computing pagination feature vectors then no pages should be specified.</param>
        /// <param name="shingleSize">The maximum size of shingles (word-n-grams) to be considered for terms.
        /// If less than 2 then only single word terms will be used.</param>
        /// <param name="maxFeatures">The maximum size feature vector to produce</param>
        public SpatialStringFeatureVectorizer(string pagesToProcess, int shingleSize, int maxFeatures)
        {
            try
            {
                Enabled = true;
                Name = "Auto Bag of Words";
                PagesToProcess = pagesToProcess;
                ShingleSize = shingleSize;
                MaxFeatures = maxFeatures;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39533");
            }
        }

        #endregion Constructors


        #region Properties

        /// <summary>
        /// The name of the feature vectorizer. Used for display purposes only
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        /// <summary>
        /// Gets the <see cref="FeatureVectorizerType"/> of this feature vectorizer
        /// (always <see cref="FeatureVectorizerType.DiscreteTerms"/>)
        /// Calling Set will throw an exception.
        /// </summary>
        public FeatureVectorizerType FeatureType
        {
            get
            {
                return FeatureVectorizerType.DiscreteTerms;
            }
            set
            {
                throw new ExtractException("ELI39528", "Cannot change type of SpatialStringFeatureVectorizer");
            }
        }

        /// <summary>
        /// Whether changing the <see cref="FeatureVectorizerType"/> is allowed. Always <see langword="false"/>.
        /// </summary>
        public bool IsFeatureTypeChangeable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Whether this feature vectorizer will produce a feature vector of length
        /// <see cref="FeatureVectorLength"/> (if <see langword="true"/>) or of zero length (if <see langword="false"/>)
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets/sets the string representation of which pages to process.
        /// If computing pagination feature vectors then no pages should be specified.
        /// </summary>
        public string PagesToProcess
        {
            get
            {
                return _pagesToProcess;
            }
            set
            {
                try
                {
                    string newValue = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (newValue != _pagesToProcess)
                    {
                        if (!String.IsNullOrWhiteSpace(newValue))
                        {
                            UtilityMethods.ValidatePageNumbers(newValue);
                        }

                        _pagesToProcess = newValue;
                    }
                }
                catch (Exception e)
                {
                    throw e.AsExtract("ELI39616");
                }
            }
        }

        /// <summary>
        /// Gets/sets the maximum size of shingles (word-n-grams) to be considered for terms.
        /// If less than 2 then only single word terms will be used.
        /// </summary>
        public int ShingleSize { get; set; }

        /// <summary>
        /// Gets/sets the maximum size feature vector to produce
        /// </summary>
        public int MaxFeatures { get; set; }

        /// <summary>
        /// A collection of distinct attribute values seen during configuration
        /// </summary>
        public IEnumerable<string> DistinctValuesSeen
        {
            get
            {
                // Order by code to match original order of terms passed to constructor
                // This could be interesting to see (since terms will have been ordered by TFIDF score)
                // and simplifies reconstructing the bag of words when cloning
                return _bagOfWords == null
                    ? Enumerable.Empty<string>()
                    : _bagOfWords.CodeToString.OrderBy(p => p.Key).Select(p => p.Value);
            }
        }

        /// <summary>
        /// The length of the feature vector that this vectorizer will produce.
        /// </summary>
        public int FeatureVectorLength
        {
            get
            {
                return DistinctValuesSeen.Count();
            }
        }

        /// <summary>
        /// Whether this instance is ready to generate feature vectors
        /// </summary>
        public bool AreEncodingsComputed
        {
            get
            {
                return _bagOfWords != null;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates a deep clone of this instance
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public SpatialStringFeatureVectorizer DeepClone()
        {
            try
            {
                var clone = (SpatialStringFeatureVectorizer)MemberwiseClone();

                var valuesSeen = DistinctValuesSeen.ToArray();
                if (valuesSeen.Length > 0)
                {
                    clone._bagOfWords = new Accord.MachineLearning.BagOfWords(valuesSeen);
                }
                return clone;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39804");
            }
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Configures this instance for document categorization by examining all <see paramref="ussFiles"/>.
        /// After this method has been run, this object will be ready to produce feature vectors.
        /// </summary>
        /// <param name="ussFiles">Paths to the spatial string files from which to collect terms for the bag-of-words
        /// vocabulary.</param>
        /// <param name="answers">The categories that each uss file belongs to.</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        internal void ComputeEncodingsFromDocumentTrainingData(IEnumerable<string> ussFiles, IEnumerable<string> answers,
            Action<StatusArgs> updateStatus, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var inputTexts = ussFiles.Select(GetDocumentText);
                var textAndCategory = inputTexts.Zip(answers, (example, answer) => Tuple.Create(example, answer));

                string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                DirectoryInfo tempDirectory = System.IO.Directory.CreateDirectory(tempDirectoryPath);
                try
                {
                    using (var directory = FSDirectory.Open(tempDirectory))
                    {
                        // Create a Lucene inverted index out of the input texts
                        WriteTermsToIndex(directory, textAndCategory, updateStatus, cancellationToken);

                        // Use the index to score the terms and pick the top scoring
                        string[] topScoringTerms = GetTopScoringTerms(directory, updateStatus, cancellationToken);

                        // Create the bag-of-words object
                        _bagOfWords = new Accord.MachineLearning.BagOfWords(topScoringTerms);
                    }
                }
                finally
                {
                    // Delete index
                    foreach (var file in System.IO.Directory.GetFiles(tempDirectoryPath))
                    {
                        FileSystemMethods.DeleteFile(file);
                    }
                    System.IO.Directory.Delete(tempDirectoryPath, true);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39536");
            }
        }

        /// <summary>
        /// Configures this instance for pagination by examining all <see paramref="ussFiles"/>.
        /// After this method has been run, this object will be ready to produce feature vectors.
        /// </summary>
        /// <param name="ussFiles">Paths to the spatial string files from which to collect terms for the bag-of-words
        /// vocabulary.</param>
        /// <param name="answerFiles">The paths to the VOA files that contain the expected pagination boundary information
        /// for each uss file.</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        internal void ComputeEncodingsFromPaginationTrainingData(IEnumerable<string> ussFiles, IEnumerable<string> answerFiles,
            Action<StatusArgs> updateStatus, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var inputTextsCollection = ussFiles.Select(GetPaginationTexts);

                // Pass the page count of each image along so that missing pages in the answer VOA can be filled in
                var textAndCategory = inputTextsCollection.Zip(answerFiles, (examples, answerFile) =>
                    {
                        var answers = LearningMachineDataEncoder.ExpandPaginationAnswerVOA(answerFile, examples.Count() + 1);
                        return examples.Zip(answers, (example, answer) => Tuple.Create(example, answer));
                    })
                    .SelectMany(answersForFile => answersForFile);

                string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                DirectoryInfo tempDirectory = System.IO.Directory.CreateDirectory(tempDirectoryPath);
                try
                {
                    using (var directory = FSDirectory.Open(tempDirectory))
                    {
                        // Create a Lucene inverted index out of the input texts
                        WriteTermsToIndex(directory, textAndCategory, updateStatus, cancellationToken);

                        // Use the index to score the terms and pick the top scoring
                        string[] topScoringTerms = GetTopScoringTerms(directory, updateStatus, cancellationToken);

                        // Create the bag-of-words object
                        _bagOfWords = new Accord.MachineLearning.BagOfWords(topScoringTerms);
                    }
                }
                finally
                {
                    // Delete index
                    foreach (var file in System.IO.Directory.GetFiles(tempDirectoryPath))
                    {
                        FileSystemMethods.DeleteFile(file);
                    }
                    System.IO.Directory.Delete(tempDirectoryPath, true);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39536");
            }
        }

        /// <summary>
        /// Get feature vector from document text using computed <see cref="_bagOfWords"/>. Throws
        /// an exception if <see cref="ComputeEncodingsFromDocumentTrainingData"/> has not been called.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to be used to generate the feature vector.</param>
        /// <returns>Array of doubles of <see cref="FeatureVectorLength"/> or length of zero if not <see cref="Enabled"/></returns>
        internal double[] GetDocumentFeatureVector(ISpatialString document)
        {
            try
            {
                ExtractException.Assert("ELI39529", "This object has not been fully configured.", AreEncodingsComputed);

                string[] terms = GetTerms(GetDocumentText(document)).ToArray();
                return _bagOfWords.GetFeatureVector(terms).Select(i => (double)i).ToArray();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39530");
            }
        }

        /// <summary>
        /// Get feature vectors from page text using computed <see cref="_bagOfWords"/>. Throws
        /// an exception if <see cref="ComputeEncodingsFromPaginationTrainingData"/> has not been called.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to be used to generate the feature vectors.</param>
        /// <returns>If <see cref="Enabled"/> then an enumeration of feature vectors that has a length of
        /// the number of pages in <see paramref="document"/>-1. Else an empty enumeration.</returns>
        internal IEnumerable<double[]> GetPaginationFeatureVectors(ISpatialString document)
        {
            try
            {
                ExtractException.Assert("ELI39531", "Bag of Words has not been computed", _bagOfWords != null);

                return GetPaginationTexts(document).Select(s =>
                {
                    string[] terms = GetTerms(s).ToArray();
                    return _bagOfWords.GetFeatureVector(terms).Select(i => (double)i).ToArray();
                });
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39532");
            }
        }

        /// <summary>
        /// Whether this instance is configured as another
        /// </summary>
        /// <param name="other">The other <see cref="SpatialStringFeatureVectorizer"/> to compare with</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        internal bool IsConfigurationEqualTo(SpatialStringFeatureVectorizer other)
        {
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null
                || other.Enabled != Enabled
                || other.MaxFeatures != MaxFeatures
                || other.Name != Name
                || other.PagesToProcess != PagesToProcess
                || other.ShingleSize != ShingleSize
                )
            {
                return false;
            }

            return true;
        }

        #endregion Internal Methods

        #region Overrides

        /// <summary>
        /// Whether this instance is equal to another.
        /// </summary>
        /// <param name="obj">The instance to compare with</param>
        /// <returns><see langword="true"/> if this instance has equal property values, else <see langword="false"/></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as SpatialStringFeatureVectorizer;
            if (other == null
                || !IsConfigurationEqualTo(other)
                || other.FeatureVectorLength != FeatureVectorLength
                || other._bagOfWords != null && _bagOfWords != null &&
                    !other._bagOfWords.CodeToString.SequenceEqual(_bagOfWords.CodeToString)
                )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the hash code for this object
        /// </summary>
        /// <returns>The hash code for this object</returns>
        public override int GetHashCode()
        {
            var hash = HashCode.Start
                .Hash(AreEncodingsComputed)
                .Hash(Enabled)
                .Hash(MaxFeatures)
                .Hash(Name)
                .Hash(PagesToProcess)
                .Hash(ShingleSize);
            foreach (var keyValuePair in _bagOfWords.CodeToString)
            {
                hash = hash.Hash(keyValuePair.Value);
            }

            return hash;
        }

        #endregion Overrides

        #region Private Methods

        /// <summary>
        /// Gets the string value from a <see cref="ISpatialString"/> limited to <see cref="PagesToProcess"/>
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> from which to get text.</param>
        /// <returns>A substring of the <see cref="ISpatialString"/> as a plain <see langword="string"/>.</returns>
        private string GetDocumentText(ISpatialString document)
        {
            if (String.IsNullOrWhiteSpace(PagesToProcess))
            {
                return document.String;
            }
            else
            {
                var pages = document.GetPages(true, " ").ToIEnumerable<ISpatialString>().ToArray();
                var pageNumbers = UtilityMethods.GetPageNumbersFromString(PagesToProcess, pages.Length, false);
                return String.Join("\r\n\r\n", pageNumbers.Select(i => pages[i - 1].String).ToArray());
            }
        }

        /// <summary>
        /// Gets the string value from a <see cref="ISpatialString"/> limited to <see cref="PagesToProcess"/>
        /// </summary>
        /// <param name="ussPath">The path to the saved <see cref="ISpatialString"/> from which to get text.</param>
        /// <returns>A substring of the <see cref="ISpatialString"/> as a plain <see langword="string"/>.</returns>
        private string GetDocumentText(string ussPath)
        {
            try
            {
                var ussObject = new SpatialStringClass();
                ussObject.LoadFrom(ussPath, false);
                return GetDocumentText(ussObject);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39534");
            }
        }

        /// <summary>
        /// Gets the string values of each page in <see paramref="document"/> except for the first.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> from which to get text.</param>
        /// <returns>An enumeration of strings containing the text for each candidate first page
        /// (first page after possible pagination boundary)</returns>
        internal static IEnumerable<string> GetPaginationTexts(ISpatialString document)
        {
            var pages = document.GetPages(true, " ").ToIEnumerable<ISpatialString>();
            return pages.Skip(1).Select(s => s.String);
        }

        /// <summary>
        /// Gets the string values of each page in <see paramref="ussPath"/> except for the first.
        /// </summary>
        /// <param name="ussPath">The path to the USS file</param>
        /// <returns>An enumeration of strings containing the text for each candidate first page
        /// (first page after possible pagination boundary)</returns>
        private static IEnumerable<string> GetPaginationTexts(string ussPath)
        {
            try
            {
                var document = new SpatialStringClass();
                document.LoadFrom(ussPath, false);
                return GetPaginationTexts(document);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39535");
            }
        }

        /// <summary>
        /// Builds a Lucene inverted index out of the <see paramref="inputTexts"/> so that there is
        /// one document for each distinct value of <see paramref="textCategories"/>.
        /// </summary>
        /// <param name="directory">The <see cref="FSDirectory"/> in which to write the index files.</param>
        /// <param name="textAndCategoryCollection">Collection of text and category pairs</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The number of examples (<see paramref="inputTexts"/>) written</returns>
        private void WriteTermsToIndex(FSDirectory directory, IEnumerable<Tuple<string, string>> textAndCategoryCollection,
            Action<StatusArgs> updateStatus, System.Threading.CancellationToken cancellationToken)
        {
            // I don't think it makes any difference which analyzer is used since an already-tokenized streams are passed in
            var analyzer = new SimpleAnalyzer();
            using (var writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                Parallel.ForEach(textAndCategoryCollection, (textAndCategory, loopState) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                    }

                    var text = textAndCategory.Item1;
                    var category = textAndCategory.Item2;
                    var tokenStream = GetTokenStream(text);
                    var document = new Document();
                    document.Add(new Field("category", category, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    document.Add(new Field("shingles", tokenStream));
                    writer.AddDocument(document);
                    updateStatus(new StatusArgs { StatusMessage = "Writing terms to index... Texts processed: {0:N0}", Int32Value = 1 });
                });

                cancellationToken.ThrowIfCancellationRequested();

                updateStatus(new StatusArgs { StatusMessage = "Writing index..." });
                writer.Optimize();
                cancellationToken.ThrowIfCancellationRequested();

                writer.Commit();
            }
        }

        /// <summary>
        /// Helper function to turn a while loop into an <see langword="IEnumerable"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition) 
        { 
            while (condition()) yield return true; 
        }

        /// <summary>
        /// Computes a relevance score for each term in a Lucene inverted index in which
        /// each document is a collection of terms for a single classification category built
        /// by <see cref="WriteTermsToIndex"/>.
        /// </summary>
        /// <param name="directory">The <see cref="FSDirectory"/> where the index files have been written.</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The <see cref="MaxFeatures"/> top-scoring terms in the index.</returns>
        private string[] GetTopScoringTerms(FSDirectory directory,
            Action<StatusArgs> updateStatus, System.Threading.CancellationToken cancellationToken)
        {
            // Collect all terms (words and shingles)
            using (var reader = DirectoryReader.Open(directory, true))
            {
                var searcher = new SimpleFacetedSearch(reader, "category");
                Query query = new MatchAllDocsQuery();
                var hits = searcher.Search(query);
                int numberOfExamples = (int)hits.TotalHitCount;
                int numberOfCategories = hits.HitsPerFacet.Length;

                // Store the number of documents for each category in order to normalize term frequency numbers
                var documentsForCategory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var hitsPerFacet in hits.HitsPerFacet)
                {
                    documentsForCategory[hitsPerFacet.Name[0]] = (int)hitsPerFacet.HitCount;
                }

                var terms = reader.Terms();
                var topTerms = IterateUntilFalse(() => !cancellationToken.IsCancellationRequested && terms.Next())
                    .Select(_ => terms.Term)
                    .Where(term => term.Field != "category")
                    .Select(term =>
                    {
                        double augmentedTermFrequency = 0.0;
                        query = new TermQuery(term);
                        hits = searcher.Search(query);
                        double categoryFrequency = hits.HitsPerFacet.Length;
                        double documentFrequency = hits.TotalHitCount;
                        foreach (var category in hits.HitsPerFacet)
                        {
                            var categoryName = category.Name[0];
                            double tf = category.HitCount;
                            // Because using a DistinctFilter, the maximum possible term frequency for a category
                            // is the number of documents in that category. Use this number to normalize the
                            // term-frequency number.
                            double maxTf = documentsForCategory[categoryName];
                            augmentedTermFrequency += tf / maxTf;
                        }
                        double inverseDocumentFrequency = Math.Log(numberOfExamples / documentFrequency);
                        double inverseCategoryFrequency = Math.Log((numberOfCategories + 0.5) / categoryFrequency);

                        // Use harmonic mean of inverse category frequency and inverse document frequency
                        double idf = 2 * inverseDocumentFrequency * inverseCategoryFrequency
                                     / (inverseDocumentFrequency + inverseCategoryFrequency);

                        double tfidf = augmentedTermFrequency * idf;
                        updateStatus(new StatusArgs { StatusMessage = "Scoring Terms... Terms processed: {0:N0}", Int32Value = 1 });
                        return new { tfidf, term };
                    })
                    .OrderByDescending(pair => pair.tfidf)
                    .Take(MaxFeatures)
                    .Select(t => t.term.Text)
                    .ToArray();

                cancellationToken.ThrowIfCancellationRequested();

                return topTerms;
            }
        }

        /// <summary>
        /// Gets a <see cref="TokenFilter"/> to be used to generate terms. The terms returned from
        /// this <see cref="TokenFilter"/> will be upper-case and, for each input, will not contain
        /// duplicate values.
        /// </summary>
        /// <param name="input">The source text.</param>
        /// <returns>The <see cref="TokenFilter"/></returns>
        private TokenFilter GetTokenStream(string input)
        {
            var reader = new System.IO.StringReader(input.ToUpper(System.Globalization.CultureInfo.InvariantCulture));
            var tokenizer = new Lucene.Net.Analysis.Standard.StandardTokenizer(Lucene.Net.Util.Version.LUCENE_30, reader);
            var stdFilter = new StandardFilter(tokenizer);
            var lengthFilter = new LengthFilter(stdFilter, 2, 500);
            TokenFilter shingleFilter = lengthFilter;
            if (ShingleSize > 1)
            {
                shingleFilter = new ShingleFilter(lengthFilter, ShingleSize);
            }
            var distinctFilter = new DistinctFilter(shingleFilter);
            return distinctFilter;
        }

        /// <summary>
        /// An enumeration of distinct, upper-case string values (words and/or shingles) from a source text.
        /// </summary>
        /// <param name="input">The source text.</param>
        /// <returns>An enumeration of distinct, upper-case string values</returns>
        private IEnumerable<string> GetTerms(string input)
        {
            using(var stream = GetTokenStream(input))
            while(stream.IncrementToken())
            {
                yield return stream.GetAttribute<ITermAttribute>().Term;
            }
        }

        /// <summary>
        /// Sets the internal bag-of-words object to null
        /// </summary>
        internal void Clear()
        {
            _bagOfWords = null;
        }

        #endregion Private Methods


        #region Private Classes

        /// <summary>
        /// Token filter that produces only distinct terms for each input
        /// </summary>
        private class DistinctFilter : TokenFilter
        {
            private HashSet<string> tokensSeen = new HashSet<string>();
            public DistinctFilter(TokenStream tokenStream)
                : base(tokenStream) { }

            public override bool IncrementToken()
            {
                while (input.IncrementToken())
                {
                    string currentTerm = input.GetAttribute<ITermAttribute>().Term;
                    if (tokensSeen.Add(currentTerm))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion Private Classes
    }
}