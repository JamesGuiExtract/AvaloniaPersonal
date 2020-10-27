using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Pattern;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extract.Utilities
{
    /// <summary>
    /// An <see cref="Analyzer"/> that tokenizes on whitespace and punctuation, makes lowercase, stems and, optionally, filters synonyms
    /// </summary>
    /// <remarks>This version keeps hyphenated words together</remarks>
    public sealed class LuceneSuggestionAnalyzer : Analyzer
    {
        static readonly string _tokenPatternDivideHyphenatedWords =   @"(?nx) [#%] | (?>\d+([,.-/]\d+)*)(?!\w) | \w+('s(?!\w))?";
        static readonly string _tokenPatternPreserveHyphenatedWords = @"(?nx) [#%] | (?>\d+([,.-/]\d+)*)(?!\w) | \w+(-\w+)?('s(?!\w))?";

        /// <summary>
        /// Optional synonym map. If non-null then a <see cref="SynonymFilter"/> will be added to the pipeline.
        /// </summary>
        public SynonymMap Synonyms { get; set; }

        /// <summary>
        /// Whether to divide hyphenated words into multiple tokens
        /// </summary>
        public bool DivideHyphenatedWords { get; set; } = true;

        /// <summary>
        /// Whether to stem words (remove prefixes/suffixes)
        /// </summary>
        public bool UseStemmer { get; set; } = true;

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Regex regex = new Regex(DivideHyphenatedWords ? _tokenPatternDivideHyphenatedWords : _tokenPatternPreserveHyphenatedWords);
            Tokenizer tokenizer = new PatternTokenizer(reader, regex, 0);
            TokenStream filter = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);
            if (UseStemmer)
            {
                filter = new EnglishPossessiveFilter(LuceneVersion.LUCENE_48, filter);
                filter = new SnowballFilter(filter, new Lucene.Net.Tartarus.Snowball.Ext.EnglishStemmer());
            }
            if (Synonyms != null)
            {
                filter = new SynonymFilter(filter, Synonyms, true);
            }
            return new TokenStreamComponents(tokenizer, filter);
        }

        /// <summary>
        /// Runs analysis on a string and returns the resulting tokens
        /// </summary>
        /// <param name="input">A string to process</param>
        /// <returns>An enumeration of tokens</returns>
        public static IList<string> GetTokens(Analyzer analyzer, string input, string fieldName = "")
        {
            try
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
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51416");
            }
        }

        /// <summary>
        /// Runs analysis on a string and joins the resulting tokens with a space
        /// </summary>
        /// <param name="input">A string to process</param>
        /// <returns>A processed string</returns>
        public static string ProcessString(string input, SynonymMap synonyms = null)
        {
            try
            {
                using var analyzer = new LuceneSuggestionAnalyzer { Synonyms = synonyms };
                return string.Join(" ", GetTokens(analyzer, input));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45478");
            }
        }
    }
}