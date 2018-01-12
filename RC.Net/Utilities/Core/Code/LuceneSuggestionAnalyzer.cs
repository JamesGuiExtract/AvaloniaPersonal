using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Analysis.TokenAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// An <see cref="Analyzer"/> that tokenizes on whitespace, makes lowercase, stems and, optionally, filters synonyms
    /// </summary>
    public sealed class LuceneSuggestionAnalyzer : Analyzer
    {
        public SynonymMap Synonyms { get; set; }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer tokenizer = new WhitespaceTokenizer(Lucene.Net.Util.LuceneVersion.LUCENE_48, reader);
            TokenStream filter = new LowerCaseFilter(Lucene.Net.Util.LuceneVersion.LUCENE_48, tokenizer);
            filter = new SnowballFilter(filter, new Lucene.Net.Tartarus.Snowball.Ext.EnglishStemmer());
            if (Synonyms != null)
            {
                filter = new SynonymFilter(filter, Synonyms, true);
            }
            return new TokenStreamComponents(tokenizer, filter);
        }

        /// <summary>
        /// Runs analysis on a string and joins the resulting tokens with a space
        /// </summary>
        /// <param name="input">A string to process</param>
        /// <returns>A processed string</returns>
        public static string ProcessString(string input)
        {
            IEnumerable<bool> whileTrue(Func<bool> condition) 
            { 
                while (condition()) yield return true; 
            }

            try
            {
                using (var analyzer = new LuceneSuggestionAnalyzer())
                using (var stream = analyzer.GetTokenStream("", new StringReader(input)))
                {
                    stream.Reset();

                    return string.Join(" ",
                        whileTrue(stream.IncrementToken)
                        .Select(_ => stream.GetAttribute<ICharTermAttribute>().ToString()));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45478");
            }
        }
    }
}
