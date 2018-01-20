using Lucene.Net.Analysis;
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
using System.Text.RegularExpressions;

namespace Extract.Utilities
{
    public class LuceneSuggestionProvider<T> : IDisposable
    {

        #region Fields

        FSDirectory _directory;
        Analyzer _analyzer;
        DirectoryInfo _tempDirectory;
        private DirectoryReader _directoryReader;
        IndexSearcher _searcher;
        List<string> _items = new List<string>();
        HashSet<string> _fields = new HashSet<string>();

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

                string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _tempDirectory = System.IO.Directory.CreateDirectory(tempDirectoryPath);
                _directory = FSDirectory.Open(_tempDirectory);
                _analyzer = new LuceneSuggestionAnalyzer();
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

        #region Methods

        /// <summary>
        /// Get suggestions for a search string
        /// </summary>
        /// <param name="searchPhrase">The substring or related phrase to search with</param>
        /// <param name="maxSuggestions">The maximim number of suggestions to return</param>
        public IEnumerable<string> GetSuggestions(string searchPhrase, int maxSuggestions = int.MaxValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchPhrase) || _items.Count == 0)
                {
                    return _items;
                }
                else
                {
                    return GetSuggestionsAndScores(searchPhrase, maxSuggestions).Select(t => t.Item1);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45447");
            }
        }

        /// <summary>
        /// Get suggestions for a search string
        /// </summary>
        /// <param name="searchPhrase">The substring or related phrase to search with</param>
        /// <param name="maxSuggestions">The maximim number of suggestions to return</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<Tuple<string, double>> GetSuggestionsAndScores(string searchPhrase, int maxSuggestions = int.MaxValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchPhrase) || _items.Count == 0)
                {
                    return _items.Select(s => Tuple.Create(s, 0.0));
                }

                var trimmed = searchPhrase.Trim();
                var escaped = QueryParserBase.Escape(trimmed);

                // The analyzer lower-cases and stems the terms so convert the search string to match
                var terms = Regex.Replace(LuceneSuggestionAnalyzer.ProcessString(trimmed), @"[\W-[%#]]", " ")
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var query = new BooleanQuery();
                foreach (var field in _fields)
                {
                    // Parse the search string into a query clause using the same analyzer
                    // that was used for building the index
                    var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, field, _analyzer);
                    var clause = parser.Parse(escaped);
                    query.Add(clause, Occur.SHOULD);

                    if (terms.Any())
                    {
                        // Add a SpanFirstQuery for the first term to give it extra weight.
                        // This query will use position info and will only match items where
                        // the term appears as the first term in the field
                        clause = new SpanFirstQuery(new SpanTermQuery(
                            new Term(field, terms.First())), 1);
                        query.Add(clause, Occur.SHOULD);

                        // Add a wildcard query based on the last term, since this
                        // is likely an incomplete word being typed
                        var lastTerm = terms.Last();
                        clause = new WildcardQuery(new Term(field, lastTerm + "*"));
                        query.Add(clause, Occur.SHOULD);

                        // Add a fuzzy query for the last term in case it is a complete word
                        if (lastTerm.Length > 1)
                        {
                            clause = new FuzzyQuery(new Term(field, lastTerm), lastTerm.Length > 3 ? 2 : 1);
                            query.Add(clause, Occur.SHOULD);
                        }
                    }

                    // Add a fuzzy query for each term except the last, which was added above.
                    foreach (var term in terms.Take(terms.Length - 1).Where(t => t.Length > 1))
                    {
                        clause = new FuzzyQuery(new Term(field, term), term.Length > 3 ? 2 : 1);
                        query.Add(clause, Occur.SHOULD);
                    }

                    // Add a match-everything clause so that the list remains a constant size
                    // https://extract.atlassian.net/browse/ISSUE-15166
                    query.Add(new MatchAllDocsQuery(), Occur.SHOULD);
                }

                var topDocs = _searcher.Search(query, maxSuggestions);
                var scoreDocs = topDocs.ScoreDocs;

                // Order by descending score, then by rank, which is the original ordering of the targets
                return scoreDocs
                    .Select(d => (name: _searcher.Doc(d.Doc).Get("Name"),
                                  score: d.Score,
                                  rank: _searcher.Doc(d.Doc).Get("Rank")))
                    .OrderByDescending(t => t.score)
                    .ThenBy(t => t.rank)
                    .Select(t => Tuple.Create<string, double>(t.name, t.score));
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI45356");
                return Enumerable.Empty<Tuple<string, double>>();
            }
        }

        #endregion Methods

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _analyzer?.Dispose();
                    _directory?.Dispose();
                    _directoryReader?.Dispose();
                }

                // Delete index
                if (_tempDirectory != null)
                {
                    foreach (var file in System.IO.Directory.GetFiles(_tempDirectory.FullName))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        { }
                    }
                    try
                    {
                        System.IO.Directory.Delete(_tempDirectory.FullName, true);
                    }
                    catch
                    { }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
