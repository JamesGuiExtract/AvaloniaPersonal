using LabDEOrderMappingInvestigator.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// A service that suggests possible matches between a customer test and a URS test
    /// </summary>
    public interface ILabTestMappingSuggestionService
    {
        /// <summary>
        /// Search for matches for the provided test
        /// </summary>
        /// <param name="extractLabTests">The URS tests to search for matches against</param>
        /// <param name="customerLabTests">The customer tests to be matched</param>
        /// <param name="maxSuggestions">Maximum results to return for each customer test</param>
        IEnumerable<IList<LabTestMatch>> GetSuggestions(
            IList<LabTestExtract> extractLabTests,
            IEnumerable<LabTestActual> customerLabTests,
            int maxSuggestions);
    }

    /// <summary>
    /// A service that suggests possible matches between a customer test and a URS test using a Lucene index
    /// </summary>
    public class LuceneLabTestMappingSuggestionService : ILabTestMappingSuggestionService
    {
        /// <inheritdoc/>
        public IEnumerable<IList<LabTestMatch>> GetSuggestions(
            IList<LabTestExtract> extractLabTests,
            IEnumerable<LabTestActual> customerLabTests,
            int maxSuggestions)
        {
            _ = extractLabTests ?? throw new ArgumentNullException(nameof(extractLabTests));
            _ = customerLabTests ?? throw new ArgumentNullException(nameof(customerLabTests));

            using LuceneLabTestMappingSuggestor suggestor = new(extractLabTests);

            foreach (var customerLabTest in customerLabTests)
            {
                yield return suggestor.GetSuggestions(customerLabTest, maxSuggestions);
            }
        }

        /// <summary>
        /// A class that suggests possible matches between a customer test and a URS test using a Lucene index
        /// </summary>
        class LuceneLabTestMappingSuggestor : IDisposable
        {
            readonly BaseDirectory _directory;
            readonly Analyzer _analyzer;
            readonly DirectoryReader _directoryReader;
            readonly IndexSearcher _searcher;
            readonly List<LabTestExtract> _items = new();
            const string CODE_FIELD = "CODE";
            const string AKA_FIELD = "AKA";

            bool isDisposed;

            /// <summary>
            /// Create a suggestor
            /// </summary>
            /// <param name="labTests">The URS tests that will make up the index (to be searched for matches)</param>
            public LuceneLabTestMappingSuggestor(IList<LabTestExtract> labTests)
            {
                _directory = new RAMDirectory();
                _analyzer = new LuceneSuggestionAnalyzer();
                using (var writer = new IndexWriter(_directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer)))
                {
                    foreach (var labTest in labTests.DistinctBy(x => x.Code))
                    {
                        _items.Add(labTest);
                        Document doc = new()
                    {
                        new StringField(CODE_FIELD, labTest.Code, Field.Store.YES),
                        new TextField(AKA_FIELD, labTest.Name, Field.Store.YES)
                    };

                        foreach (string aka in labTest.AKAs)
                        {
                            doc.Add(new TextField(AKA_FIELD, aka, Field.Store.YES));
                        }
                        writer.AddDocument(doc);
                    }
                    writer.Commit();
                }
                _directoryReader = DirectoryReader.Open(_directory);
                _searcher = new IndexSearcher(_directoryReader);
            }

            /// <summary>
            /// Search for matches for the provided test
            /// </summary>
            /// <param name="labTest">The test to be matched</param>
            /// <param name="maxSuggestions">Maximum results to return</param>
            public IList<LabTestMatch> GetSuggestions(LabTestActual labTest, int maxSuggestions)
            {
                if (labTest is null || _items.Count == 0)
                {
                    return Array.Empty<LabTestMatch>();
                }
                else
                {
                    IList<ScoreDoc>? scoreDocs = GetScores(labTest, maxSuggestions);

                    if (scoreDocs == null)
                    {
                        return Array.Empty<LabTestMatch>();
                    }

                    var resultValues = new List<LabTestMatch>(Math.Min(scoreDocs.Count, maxSuggestions));

                    resultValues.AddRange(
                        scoreDocs
                        .OrderByDescending(scoreDoc => scoreDoc.Score)
                        .ThenBy(scoreDoc => scoreDoc.Doc)
                        .Select(scoreDoc => new LabTestMatch(labTest, _items[scoreDoc.Doc], scoreDoc.Score))
                        .Take(maxSuggestions));

                    return resultValues;
                }
            }

            /// <summary>
            /// Add clauses to the supplied query
            /// </summary>
            /// <param name="query">The query that is being constructed</param>
            /// <param name="searchPhrase">The words to be added to the query</param>
            /// <param name="field">The name of the indexed field to be searched</param>
            bool UpdateQueryForField(BooleanQuery query, string searchPhrase, string field)
            {
                Query? clause = null;

                // Divide into tokens and normalize
                var terms = _analyzer.GetTokens(searchPhrase, field);

                if (!terms.Any())
                {
                    return false;
                }

                string escaped = QueryParserBase.Escape(string.Join(" ", terms));

                // Parse the search string into a query clause using the same analyzer
                // that was used for building the index
                if (escaped != null)
                {
                    var parser = new QueryParser(LuceneVersion.LUCENE_48, field, _analyzer);
                    clause = parser.Parse(escaped);
                    query.Add(clause, Occur.SHOULD);
                }

                // Add a fuzzy query for each term
                foreach (var term in terms.Where(t => t.Length > 1))
                {
                    clause = new FuzzyQuery(new Term(field, term), term.Length > 3 ? 2 : 1);
                    clause.Boost = 0.1f;
                    query.Add(clause, Occur.SHOULD);
                }

                return true;
            }

            /// <summary>
            /// Get suggestions for mapping a customer test to the URS
            /// </summary>
            /// <param name="searchForTest">The lab test to search for</param>
            /// <param name="maybeMaxSuggestions">The maximum number of suggestions to return</param>
            public IList<ScoreDoc>? GetScores(LabTestActual searchForTest, int? maybeMaxSuggestions = null)
            {
                _ = searchForTest ?? throw new ArgumentNullException(nameof(searchForTest));

                var query = new BooleanQuery();

                if (!UpdateQueryForField(query, searchForTest.Name, AKA_FIELD))
                {
                    return null;
                }

                if (searchForTest.LabTestDefinition.HasValue)
                {
                    var testDefinition = searchForTest.LabTestDefinition.Value;
                    if (!UpdateQueryForField(query, testDefinition.OfficialName, AKA_FIELD))
                    {
                        return null;
                    }
                    foreach (var aka in testDefinition.AKAs)
                    {
                        if (!UpdateQueryForField(query, aka, AKA_FIELD))
                        {
                            return null;
                        }
                    }
                }

                int maxSuggestions = maybeMaxSuggestions ?? _items.Count;

                return _searcher.Search(query, maxSuggestions).ScoreDocs;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    if (disposing)
                    {
                        _analyzer.Dispose();
                        _directoryReader.Dispose();
                        _directory.Dispose();
                    }

                    isDisposed = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

        }
    }

    static class ExtensionMethods
    {
        /// <summary>
        /// Get tokens from an input string using this <see cref="Analyzer"/>
        /// </summary>
        public static IList<string> GetTokens(this Analyzer analyzer, string input, string fieldName = "")
        {
            static IEnumerable<bool> whileTrue(Func<bool> condition)
            {
                while (condition()) yield return true;
            }

            using var stream = analyzer.GetTokenStream(fieldName, new StringReader(input));
            stream.Reset();

            return whileTrue(stream.IncrementToken)
                .Select(_ => stream.GetAttribute<ICharTermAttribute>().ToString())
                .ToList();
        }
    }
}
