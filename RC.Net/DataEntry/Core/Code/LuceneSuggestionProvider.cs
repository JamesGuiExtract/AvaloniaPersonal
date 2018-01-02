using Extract.Utilities;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extract.DataEntry
{
    class LuceneSuggestionProvider<T> : IDisposable
    {

        #region Fields

        FSDirectory _directory;
        Analyzer _analyzer;
        DirectoryInfo _tempDirectory;
        IndexSearcher _searcher;
        List<string> _items = new List<string>();
        HashSet<string> _fields = new HashSet<string>();

        #endregion Fields

        #region Properties

        /// <summary>
        /// The maximum number of items to be returned by <see cref="GetSuggestions(string)"/>.
        /// Default is <see cref="int.MaxValue"/>
        /// </summary>
        public int MaxSuggestions { get; set; } = int.MaxValue;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="suggestionsSource">A collection of objects to extract suggestion</param>
        /// <param name="nameExtractor">A function that returns the name of the suggestion
        /// (the value that will be listed by <see cref="GetSuggestions(string)"/>)</param>
        /// <param name="fieldValuesExtractor">Function to get one or more searchable fields</param>
        public LuceneSuggestionProvider(
            IEnumerable<T> suggestionsSource,
            Func<T, string> nameExtractor,
            Func<T, IEnumerable<KeyValuePair<string, string>>> fieldValuesExtractor)
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
                _analyzer = new WhitespacePlusAnalyzer();
                using (var writer = new IndexWriter(_directory, _analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    foreach (var suggestion in suggestionsSource)
                    {
                        var name = nameExtractor(suggestion);
                        _items.Add(name);
                        var doc = new Document();
                        doc.Add(new Field("Name", name, Field.Store.YES, Field.Index.NOT_ANALYZED));

                        foreach (var (f, value) in fieldValuesExtractor(suggestion))
                        {
                            // Ensure no collision with Name field
                            var field = "_" + f;
                            _fields.Add(field);
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                doc.Add(new Field(field, value, Field.Store.YES, Field.Index.ANALYZED));
                            }
                        }
                        writer.AddDocument(doc);
                    }
                    writer.Optimize();
                    writer.Commit();
                }

                _searcher = new IndexSearcher(_directory, true);
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
        /// <param name="searchString">The substring or related phrase to search with</param>
        public IEnumerable<string> GetSuggestions(string searchString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchString) || _items.Count == 0)
                {
                    return _items;
                }

                var trimmed = searchString.Trim();
                var escaped = QueryParser.Escape(trimmed);

                // The analyzer lower-cases the terms so convert the search string to match
                var terms = Regex.Replace(trimmed.ToLower(CultureInfo.CurrentCulture), @"[\W-[%#]]", " ")
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var query = new BooleanQuery();
                foreach (var field in _fields)
                {
                    // Parse the search string into a query clause using the same analyzer
                    // that was used for building the index
                    var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, field, _analyzer);
                    var clause = parser.Parse(escaped);
                    clause.Boost = field == "Name" ? 2 : 1;
                    query.Add(clause, Occur.SHOULD);

                    if (terms.Any())
                    {
                        // Add a SpanFirstQuery for the first term to give it extra weight.
                        // This query will use position info and will only match items where
                        // the term appears as the first term in the field
                        clause = new SpanFirstQuery(new SpanTermQuery(
                            new Term(field, terms.First())), 1);
                        clause.Boost = field == "Name" ? 2 : 1;
                        query.Add(clause, Occur.SHOULD);

                        // Add a wildcard query based on the last term, since this
                        // is likely an incomplete word being typed
                        var lastTerm = terms.Last();
                        clause = new WildcardQuery(new Term(field, lastTerm + "*"));
                        clause.Boost = field == "Name" ? 2 : 1;
                        query.Add(clause, Occur.SHOULD);

                        // Add a fuzzy query for the last term in case it is a complete word
                        clause = new FuzzyQuery(new Term(field, lastTerm), 0.5f);
                        clause.Boost = field == "Name" ? 2 : 1;
                        query.Add(clause, Occur.SHOULD);
                    }

                    // Add a fuzzy query for each term except the last, which was added above.
                    foreach (var term in terms.Take(terms.Length - 1))
                    {
                        clause = new FuzzyQuery(new Term(field, term), 0.5f);
                        clause.Boost = field == "Name" ? 2 : 1;
                        query.Add(clause, Occur.SHOULD);
                    }

                    // Add a match-everything clause so that the list remains a constant size
                    // https://extract.atlassian.net/browse/ISSUE-15166
                    query.Add(new MatchAllDocsQuery(), Occur.SHOULD);
                }

                var topDocs = _searcher.Search(query, MaxSuggestions);
                var scoreDocs = topDocs.ScoreDocs;
                return scoreDocs
                    .Select(d => (name: _searcher.Doc(d.Doc).Get("Name"),
                                  score: d.Score))
                    .OrderByDescending(d => d.score)
                    .Select(d => d.name);
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI45356");
                return Enumerable.Empty<string>();
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
                    _searcher?.Dispose();
                }

                // Delete index
                if (_tempDirectory != null)
                {
                    foreach (var file in System.IO.Directory.GetFiles(_tempDirectory.FullName))
                    {
                        try
                        {
                            // FileSystemMethods.DeleteFile(file);
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

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }

    sealed class WhitespacePlusAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new LowerCaseFilter(new WhitespaceTokenizer(reader));
        }
    }
}
