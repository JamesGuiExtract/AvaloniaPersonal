using Lucene.Net.Analysis;
using Lucene.Net.Util;
using System.IO;
using Lucene.Net.Analysis.Synonym;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis.Pattern;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Snowball;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// An <see cref="Analyzer"/> that tokenizes on whitespace and punctuation, makes lowercase, stems and, optionally, filters synonyms
    /// </summary>
    public sealed class LuceneSuggestionAnalyzer : Analyzer
    {
        const string _tokenPatternDivideHyphenatedWords = @"(?nx) [#%] | (?>\d+([,.-/]\d+)*)(?!\w) | \w+('s(?!\w))?";
        const string _tokenPatternPreserveHyphenatedWords = @"(?nx) [#%] | (?>\d+([,.-/]\d+)*)(?!\w) | \w+(-\w+)?('s(?!\w))?";

        /// <summary>
        /// Optional synonym map. If non-null then a <see cref="SynonymFilter"/> will be added to the pipeline.
        /// </summary>
        public SynonymMap? Synonyms { get; set; }

        /// <summary>
        /// Whether to divide hyphenated words into multiple tokens
        /// </summary>
        public bool DivideHyphenatedWords { get; set; } = true;

        /// <summary>
        /// Whether to stem words (remove prefixes/suffixes)
        /// </summary>
        public bool UseStemmer { get; set; } = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by caller")]
        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Regex regex = new(DivideHyphenatedWords ? _tokenPatternDivideHyphenatedWords : _tokenPatternPreserveHyphenatedWords);
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
    }
}
