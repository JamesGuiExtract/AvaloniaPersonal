using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Extract.Utilities
{
    public class LuceneBestMatchProvider : IDisposable
    {
        #region Constants

        string DIVIDE_HYPHEN_TOKENIZER_FIELD = "divi_tokenizer";
        string PRESERVE_HYPHEN_TOKENIZER_FIELD = "pres_tokenizer";

        #endregion Constants

        #region Fields

        FSDirectory _directory;
        Dictionary<string, Analyzer> _fieldAn;
        Analyzer _analyzer;
        DirectoryInfo _tempDirectory;
        private DirectoryReader _directoryReader;
        IndexSearcher _searcher;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="targets">The translate-to targets</param>
        /// <param name="expandingSynonyms">A few-to-many-word, small-to-large value <see cref="SynonymMap"/></param>
        public LuceneBestMatchProvider(IEnumerable<string> targets, SynonymMap expandingSynonyms = null)
        {
            try
            {
                if (targets == null || !targets.Any())
                {
                    return;
                }

                string tempDirectoryPath = Path.Combine(Path.GetTempPath(), "BestMatchProvider", Path.GetRandomFileName());
                _tempDirectory = System.IO.Directory.CreateDirectory(tempDirectoryPath);
                _directory = FSDirectory.Open(_tempDirectory);
                var divideHyphenAnalyzer = new LuceneSuggestionAnalyzer { Synonyms = expandingSynonyms };
                var preserveHyphenAnalyzer = new LuceneSuggestionAnalyzer { Synonyms = expandingSynonyms, DivideHyphenatedWords = false };
                _fieldAn = new Dictionary<string, Analyzer> { { DIVIDE_HYPHEN_TOKENIZER_FIELD, divideHyphenAnalyzer }, { PRESERVE_HYPHEN_TOKENIZER_FIELD, preserveHyphenAnalyzer } };
                _analyzer = new PerFieldAnalyzerWrapper(divideHyphenAnalyzer, _fieldAn);

                // Use more precise field normalization than default since fields are often very small.
                // Otherwise there is no difference between a 4 term and a 3 term field, e.g., between JOHN DOE'S ADDITION and JOHN DOE'S 2ND ADDITION
                using (var writer = new IndexWriter(_directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer)
                    { Similarity = new PreciseDefaultSimilarity() }))
                {
                    int rank = 0;
                    foreach (var target in targets.Where(target => !string.IsNullOrWhiteSpace(target)))
                    {
                        var stdExpanded = string.Join(" ", ProcessString(_fieldAn[DIVIDE_HYPHEN_TOKENIZER_FIELD], target));
                        var wsExpanded = string.Join(" ", ProcessString(_fieldAn[PRESERVE_HYPHEN_TOKENIZER_FIELD], target));
                        var doc = new Document
                        {
                            new StringField("Name", target, Field.Store.YES),
                            new StringField("Rank", (rank++).ToString(CultureInfo.InvariantCulture), Field.Store.YES),

                            new TextField(DIVIDE_HYPHEN_TOKENIZER_FIELD, stdExpanded, Field.Store.YES),
                            new TextField(PRESERVE_HYPHEN_TOKENIZER_FIELD, wsExpanded, Field.Store.YES)
                        };
                        writer.AddDocument(doc);
                    }
                    writer.Commit();
                }
                _directoryReader =  DirectoryReader.Open(_directory);
                _searcher = new IndexSearcher(_directoryReader)
                {
                    Similarity = new PreciseDefaultSimilarity()
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45489");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Get suggestions for a search string
        /// </summary>
        /// <param name="searchPhrase">The substring or related phrase to search with</param>
        /// <param name="maxSuggestions">The maximim number of suggestions to return</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public List<Tuple<string, double>> GetSuggestionsAndScores(string searchPhrase, int maxSuggestions = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchPhrase))
                {
                    return new List<Tuple<string, double>>(0);
                }

                var query = new BooleanQuery();

                var stdProcessed = ProcessString(_fieldAn[DIVIDE_HYPHEN_TOKENIZER_FIELD], searchPhrase).ToList();
                var stdExpanded = string.Join(" ", stdProcessed);
                var stdParser = new QueryParser(LuceneVersion.LUCENE_48, DIVIDE_HYPHEN_TOKENIZER_FIELD, _analyzer);
                query.Add(stdParser.Parse(QueryParser.Escape(stdExpanded)), Occur.SHOULD);

                var wsProcessed = ProcessString(_fieldAn[PRESERVE_HYPHEN_TOKENIZER_FIELD], searchPhrase).ToList();
                var wsExpanded = string.Join(" ", wsProcessed);
                var wsParser = new QueryParser(LuceneVersion.LUCENE_48, PRESERVE_HYPHEN_TOKENIZER_FIELD, _analyzer);
                Query clause = wsParser.Parse(QueryParser.Escape(wsExpanded));
                clause.Boost = 0.25f;
                query.Add(clause, Occur.SHOULD);

                if (stdProcessed.Any())
                {
                    // Add clauses to match first word and first two words
                    // To give preference to prefix matches
                    clause = new SpanFirstQuery(new SpanTermQuery(
                        new Term(DIVIDE_HYPHEN_TOKENIZER_FIELD, stdProcessed[0])), 1);
                    query.Add(clause, Occur.SHOULD);

                    if (stdProcessed.Count() > 1)
                    {
                        clause = new SpanFirstQuery(new SpanTermQuery(
                            new Term(DIVIDE_HYPHEN_TOKENIZER_FIELD, stdProcessed[1])), 2);
                        query.Add(clause, Occur.SHOULD);
                    }

                    // Add fuzzy clauses
                    foreach (var term in stdProcessed.Where(t => t.Length > 1 && !decimal.TryParse(t, out var _)))
                    {
                        clause = new FuzzyQuery(new Term(DIVIDE_HYPHEN_TOKENIZER_FIELD, term), term.Length > 3 ? 2 : 1);
                        query.Add(clause, Occur.SHOULD);
                    }
                }

                var topDocs = _searcher.Search(query, maxSuggestions);
                var scoreDocs = topDocs.ScoreDocs;

                // Order by descending score, then by rank, which is the original ordering of the targets
                var result = scoreDocs
                    .Select(d => (name: _searcher.Doc(d.Doc).Get("Name"),
                                  score: d.Score,
                                  rank: _searcher.Doc(d.Doc).Get("Rank")))
                    .OrderByDescending(t => t.score)
                    .ThenBy(t => t.rank)
                    .Select(t => Tuple.Create<string, double>(t.name, t.score))
                    .ToList();
                return result;
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI45490");
                return new List<Tuple<string, double>>(0);
            }
        }

        /// <summary>
        /// Runs analysis on a string and joins the resulting tokens with a space
        /// </summary>
        /// <param name="input">A string to process</param>
        /// <returns>A processed string</returns>
        static IEnumerable<string> ProcessString(Analyzer analyzer, string input)
        {
            IEnumerable<bool> whileTrue(Func<bool> condition) 
            { 
                while (condition()) yield return true; 
            }

            try
            {
                using (var stream = analyzer.GetTokenStream("", new StringReader(input)))
                {
                    stream.Reset();

                    return whileTrue(stream.IncrementToken)
                        .Select(_ => stream.GetAttribute<ICharTermAttribute>().ToString())
                        .ToList();

                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45491");
            }
        }
        #endregion Methods

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        // It is disposed
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_analyzer")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // Dispose of these even if this is being called from the
                // destructor, so that they release locks on the index files
                foreach(var an in _fieldAn.Values)
                {
                    an.Dispose();
                }
                _analyzer?.Dispose();
                _directory?.Dispose();
                _directoryReader?.Dispose();

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

                disposedValue = true;
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
        /// Implement finalize to ensure the temporary file is deleted even when Dispose is not
        /// called.
        /// </summary>
        ~LuceneBestMatchProvider()
        {
            Dispose(false);
        }

        #endregion Destructors
    }

    //--------------------------------------------------------------------------------
    //
    // This code copied from a unit test in the Lucene project
    //
    //--------------------------------------------------------------------------------

    /// <summary>
    /// Encodes norm as 4-byte float. </summary>
    internal class PreciseDefaultSimilarity : TFIDFSimilarity
    {
        /// <summary>
        /// Sole constructor: parameter-free </summary>
        public PreciseDefaultSimilarity()
        {
        }

        /// <summary>
        /// Implemented as <code>overlap / maxOverlap</code>. </summary>
        public override float Coord(int overlap, int maxOverlap)
        {
            return overlap / (float)maxOverlap;
        }

        /// <summary>
        /// Implemented as <code>1/sqrt(sumOfSquaredWeights)</code>. </summary>
        public override float QueryNorm(float sumOfSquaredWeights)
        {
            return (float)(1.0 / Math.Sqrt(sumOfSquaredWeights));
        }

        /// <summary>
        /// Encodes a normalization factor for storage in an index.
        /// <p>
        /// The encoding uses a three-bit mantissa, a five-bit exponent, and the
        /// zero-exponent point at 15, thus representing values from around 7x10^9 to
        /// 2x10^-9 with about one significant decimal digit of accuracy. Zero is also
        /// represented. Negative numbers are rounded up to zero. Values too large to
        /// represent are rounded down to the largest representable value. Positive
        /// values too small to represent are rounded up to the smallest positive
        /// representable value.
        /// </summary>
        /// <seealso cref= org.apache.lucene.document.Field#setBoost(float) </seealso>
        /// <seealso cref= org.apache.lucene.util.SmallFloat </seealso>
        public override long EncodeNormValue(float f)
        {
            return BitConverter.DoubleToInt64Bits(f);
        }

        /// <summary>
        /// Decodes the norm value, assuming it is a single byte.
        /// </summary>
        /// <seealso cref= #encodeNormValue(float) </seealso>
        public override float DecodeNormValue(long norm)
        {
            return (float) BitConverter.Int64BitsToDouble(norm);
        }

        /// <summary>
        /// Implemented as
        /// <c>state.Boost*LengthNorm(numTerms)</c>, where
        /// <c>numTerms</c> is <see cref="FieldInvertState.Length"/> if 
        /// <see cref="DiscountOverlaps"/># is false, else it's 
        /// <see cref="FieldInvertState.Length"/> - 
        /// <see cref="FieldInvertState.NumOverlap"/>.
        /// <para/>
        /// @lucene.experimental 
        /// </summary>
        public override float LengthNorm(FieldInvertState state)
        {
            int numTerms;
            if (discountOverlaps)
            {
                numTerms = state.Length - state.NumOverlap;
            }
            else
            {
                numTerms = state.Length;
            }
            return state.Boost * ((float)(1.0 / Math.Sqrt(numTerms)));
        }

        /// <summary>
        /// Implemented as <code>sqrt(freq)</code>. </summary>
        public override float Tf(float freq)
        {
            return (float)Math.Sqrt(freq);
        }

        /// <summary>
        /// Implemented as <code>1 / (distance + 1)</code>. 
        /// </summary>
        public override float SloppyFreq(int distance)
        {
            return 1.0f / (distance + 1);
        }

        /// <summary>
        /// The default implementation returns <code>1</code>
        /// </summary>
        public override float ScorePayload(int doc, int start, int end, BytesRef payload)
        {
            return 1;
        }

        /// <summary>
        /// Implemented as <code>log(numDocs/(docFreq+1)) + 1</code>. 
        /// </summary>
        public override float Idf(long docFreq, long numDocs)
        {
            return (float)(Math.Log(numDocs / (double)(docFreq + 1)) + 1.0);
        }

        /// <summary>
        /// True if overlap tokens (tokens with a position of increment of zero) are
        /// discounted from the document's length.
        /// </summary>
        protected internal bool discountOverlaps = true;

        /// <summary>
        /// Determines whether overlap tokens (Tokens with
        ///  0 position increment) are ignored when computing
        ///  norm.  By default this is true, meaning overlap
        ///  tokens do not count when computing norms.
        /// 
        ///  @lucene.experimental
        /// </summary>
        ///  <seealso cref= #computeNorm </seealso>
        public virtual bool DiscountOverlaps
        {
            set { discountOverlaps = value; }
            get { return discountOverlaps; }
        }

        public override string ToString()
        {
            return "PreciseDefaultSimilarity";
        }
    }
}
