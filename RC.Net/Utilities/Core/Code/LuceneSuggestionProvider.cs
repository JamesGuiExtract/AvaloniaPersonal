using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Extract.Utilities
{
    public class LuceneSuggestionProvider<T> : IDisposable
    {
        #region Constants

        const string STEMMED_FIELD = "stemmed";

        #endregion Constants

        #region Fields

        FSDirectory _directory;
        Analyzer _analyzer;
        DirectoryInfo _tempDirectory;
        DirectoryReader _directoryReader;
        IndexSearcher _searcher;
        List<string> _items = new List<string>();
        HashSet<string> _fields = new HashSet<string>() { STEMMED_FIELD };

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="suggestionsSource">A collection of objects to extract suggestion</param>
        /// <param name="nameExtractor">A function that returns the name of the suggestion
        /// (the value that will be listed by <see cref="GetSuggestions(string)"/>)</param>
        /// <param name="fieldValuesExtractor">Function to get one or more searchable fields</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public LuceneSuggestionProvider(
            IEnumerable<T> suggestionsSource,
            Func<T, string> nameExtractor,
            Func<T, IEnumerable<KeyValuePair<string, string>>> fieldValuesExtractor
            )
        {
            try
            {
                if (suggestionsSource == null || !suggestionsSource.Any())
                {
                    return;
                }

                _tempDirectory = FileSystemMethods.GetTemporaryFolder(Path.Combine(Path.GetTempPath(), "SuggestionProvider"), true);
                _directory = FSDirectory.Open(_tempDirectory);
                var stemAnalyzer = new LuceneSuggestionAnalyzer();
                var noStemAnalyzer = new LuceneSuggestionAnalyzer { UseStemmer = false };
                var dict = new Dictionary<string, Analyzer> { { STEMMED_FIELD, stemAnalyzer } };
                _analyzer = new PerFieldAnalyzerWrapper(noStemAnalyzer, dict);
                using (var writer = new IndexWriter(_directory, new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, _analyzer)))
                {
                    int rank = 0;
                    foreach (var suggestion in suggestionsSource)
                    {
                        var name = nameExtractor(suggestion);
                        _items.Add(name);
                        var doc = new Document();
                        doc.Add(new StringField("Name", name, Field.Store.YES));
                        doc.Add(new StringField("Rank", (rank++).ToString(CultureInfo.InvariantCulture), Field.Store.YES));

                        foreach (var (f, value) in fieldValuesExtractor(suggestion))
                        {
                            // Ensure no collision with built-in fields
                            var field = "_" + f;
                            _fields.Add(field);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                doc.Add(new TextField(STEMMED_FIELD, value, Field.Store.YES));
                                doc.Add(new TextField(field, value, Field.Store.YES));
                            }
                        }
                        writer.AddDocument(doc);
                    }
                    writer.Commit();
                }
                _directoryReader = DirectoryReader.Open(_directory);
                _searcher = new IndexSearcher(_directoryReader);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45355");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Whether this instance has been disposed
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Get suggestions for a search string
        /// </summary>
        /// <param name="searchPhrase">The substring or related phrase to search with</param>
        /// <param name="maybeMaxSuggestions">The maximum number of suggestions to return</param>
        /// <param name="excludeLowScoring">Whether to return the best suggestions</param>
        public IList<string> GetSuggestions(string searchPhrase, int? maybeMaxSuggestions = null,
            bool excludeLowScoring = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchPhrase) || _items.Count == 0)
                {
                    return _items;
                }
                else
                {
                    IList<ScoreDoc> scoreDocs = GetScores(searchPhrase, false, excludeLowScoring ? null : maybeMaxSuggestions);

                    if (scoreDocs == null)
                    {
                        return _items;
                    }

                    // If the search phrase is at least two chars long, check the quality of the results
                    if (searchPhrase.Length > 1)
                    {
                        // Try again using a fuzzy term for the last term if the results are undifferentiated
                        if (ShouldTryAgain(searchPhrase, scoreDocs))
                        {
                            scoreDocs = GetScores(searchPhrase, true, excludeLowScoring ? null : maybeMaxSuggestions);
                        }
                    }

                    if (excludeLowScoring)
                    {
                        scoreDocs = ExcludeLowScoring(scoreDocs);
                    }

                    int maxSuggestions = maybeMaxSuggestions ?? scoreDocs.Count;
                    var resultValues = new List<string>(Math.Min(scoreDocs.Count, maxSuggestions));

                    resultValues.AddRange(
                        scoreDocs
                        .OrderByDescending(scoreDoc => scoreDoc.Score)
                        .ThenBy(scoreDoc => scoreDoc.Doc)
                        .Select(scoreDoc => _items[scoreDoc.Doc])
                        .Take(maxSuggestions));

                    return resultValues;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45447");
            }
        }

        static bool ShouldTryAgain(string searchPhrase, IList<ScoreDoc> scores)
        {
            var isLastWordCompleteAlready = searchPhrase.Length > 1 && char.IsWhiteSpace(searchPhrase.Last());
            return !isLastWordCompleteAlready
                && scores.Count > 1
                && scores[0].Score == scores[scores.Count - 1].Score;
        }

        bool UpdateQueryForField(BooleanQuery query, string searchPhrase, string field, bool considerAllWordsComplete)
        {
            Query clause = null;

            // Divide into tokens and normalize (lower-case)
            var terms = LuceneSuggestionAnalyzer.GetTokens(_analyzer, searchPhrase, field);

            if (!terms.Any())
            {
                return false;
            }

            bool lastTermMightBeIncomplete = !(considerAllWordsComplete || char.IsWhiteSpace(searchPhrase.Last()));

            string escaped = QueryParser.Escape(string.Join(" ", terms));

            // Parse the search string into a query clause using the same analyzer
            // that was used for building the index
            if (escaped != null)
            {
                var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, field, _analyzer);
                clause = parser.Parse(escaped);
                query.Add(clause, Occur.SHOULD);
            }

            // Add a wildcard query if the last term is might be incomplete
            var lastTerm = terms.Last();
            if (lastTermMightBeIncomplete)
            {
                // If there is only a single term, include a SpanFirstQuery with extra weight.
                // This query will use position info and will only match items where
                // the term appears as the first term in the field
                if (terms.Count == 1)
                {
                    clause = new SpanFirstQuery(
                        new SpanMultiTermQueryWrapper<PrefixQuery>(
                        new PrefixQuery(new Term(field, lastTerm))), 1);
                    clause.Boost = 1.1f;
                    query.Add(clause, Occur.SHOULD);

                    // Also add a non-span-first query
                    // Exclude the first position from this query so as not to count first position twice
                    // This query seems to count more than it should... so use 0.5 for the boost
                    clause = new SpanNotQuery(
                        include: new SpanMultiTermQueryWrapper<PrefixQuery>(
                            new PrefixQuery(new Term(field, lastTerm))),
                        exclude: new SpanFirstQuery(
                            new SpanMultiTermQueryWrapper<PrefixQuery>(
                            new PrefixQuery(new Term(field, lastTerm))), 1));
                    clause.Boost = 0.5f;
                    query.Add(clause, Occur.SHOULD);
                }
                else
                {
                    clause = new PrefixQuery(new Term(field, lastTerm));
                    query.Add(clause, Occur.SHOULD);

                    // Add another clause for all the other terms
                    // so that the last word isn't counted twice
                    foreach (var term in terms.Take(terms.Count - 1))
                    {
                        query.Add(new TermQuery(new Term(field, term)) { Boost = 0.5f }, Occur.SHOULD);
                    }
                }
            }

            // Add a span first query to boost the first word if there is more than one
            // word or if not lastTermMightBeIncomplete (to compensate for the lacking prefix query above)
            if (terms.Count > 1 || !lastTermMightBeIncomplete)
            {
                clause = new SpanFirstQuery(new SpanTermQuery(
                    new Term(field, terms.First())), 1);
                clause.Boost = 0.1f;
                query.Add(clause, Occur.SHOULD);
            }

            // Add a fuzzy query for the last term in case it is a complete word
            if (!lastTermMightBeIncomplete && lastTerm.Length > 1)
            {
                clause = new FuzzyQuery(
                        new Term(field, lastTerm), lastTerm.Length > 3 ? 2 : 1);

                clause.Boost = 0.1f;
                query.Add(clause, Occur.SHOULD);
            }

            // Add a fuzzy query for each term except the last, which was added above.
            foreach (var term in terms.Take(terms.Count - 1).Where(t => t.Length > 1))
            {
                clause = new FuzzyQuery(new Term(field, term), term.Length > 3 ? 2 : 1);
                clause.Boost = 0.1f;
                query.Add(clause, Occur.SHOULD);
            }

            // Add a match-everything clause so that the list remains a constant size
            // https://extract.atlassian.net/browse/ISSUE-15166
            query.Add(new MatchAllDocsQuery(), Occur.SHOULD);

            return true;
        }

        /// <summary>
        /// Get suggestions for a search string
        /// </summary>
        /// <param name="searchPhrase">The substring or related phrase to search with</param>
        /// <param name="maybeMaxSuggestions">The maximum number of suggestions to return</param>
        /// <param name="considerAllWordsComplete">Whether to skip adding a special wildcard term
        /// for the second part of a word in progress</param>
        public IList<ScoreDoc> GetScores(string searchPhrase, bool considerAllWordsComplete, int? maybeMaxSuggestions = null)
        {
            try
            {
                var query = new BooleanQuery();
                foreach (var field in _fields)
                {
                    if (!UpdateQueryForField(query, searchPhrase, field, considerAllWordsComplete))
                    {
                        return null;
                    }
                }

                int maxSuggestions = maybeMaxSuggestions ?? _items.Count;

                return _searcher.Search(query, maxSuggestions).ScoreDocs;
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI45356");
                return Array.Empty<ScoreDoc>();
            }
        }

        private IList<ScoreDoc> ExcludeLowScoring(IList<ScoreDoc> scoreDocs)
        {
            var stats = new Stats(scoreDocs);
            double cutoff = stats.Median + stats.StandardDeviation;

            return scoreDocs.Where(t => t.Score >= cutoff).ToList();
        }

        #endregion Methods

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                // Set flag so that errors caused by disposing can be attributed properly
                IsDisposed = true;

                // Dispose of these even if this is being called from the
                // destructor, so that they release locks on the index files
                _analyzer?.Dispose();
                _directoryReader?.Dispose();
                _directory?.Dispose();

                // Delete index
                if (_tempDirectory != null)
                {
                    try
                    {
                        System.IO.Directory.Delete(_tempDirectory.FullName, true);
                    }
                    catch
                    { }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Deletes temporary directories before the <see cref="LuceneSuggestionProvider{T}"/>
        /// is reclaimed by garbage collection.
        /// </summary>
        ~LuceneSuggestionProvider()
        {
            Dispose(false);
        }

        #endregion Destructors

        #region Private Classes

        class Stats
        {
            public double Median { get; }
            public double StandardDeviation { get; }
            public Stats(IEnumerable<ScoreDoc> queryResults)
            {
                // Result is sorted by score. See GetSuggestionsAndScores
                var scores = queryResults.Select(t => t.Score).ToList();
                double mean = scores.Average();
                Median = scores[scores.Count / 2];
                StandardDeviation = Math.Sqrt(scores.Sum(s => Math.Pow(s - mean, 2)) / scores.Count);
            }
        }

        #endregion
    }
}
