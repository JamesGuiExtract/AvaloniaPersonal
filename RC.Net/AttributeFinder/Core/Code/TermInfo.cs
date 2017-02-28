using System;
using System.Runtime.CompilerServices;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Container for information about a term (e.g., word or n-gram)
    /// Used for TFIDF scoring
    /// </summary>
    public class TermInfo
    {
        /// <summary>
        /// Gets the string value of the term
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the term-frequency * inverse-document-frequency score of the term
        /// </summary>
        public double TermFrequencyInverseDocumentFrequency { get; }

        /// <summary>
        /// Compute tfidf score
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double GetScore(double termFrequency, double documentFrequency, int numberOfExamples, int numberOfCategories)
        {
            // A factor originally thought to be inverse category frequency (prior to 10.6)
            double idfBalancingFactor = Math.Log((0.5 + numberOfCategories) / numberOfCategories);

            double tf = termFrequency == 0 ? 1 : termFrequency;
            double df = documentFrequency == 0 ? 1 : documentFrequency;
            double idf = Math.Log(numberOfExamples / df);

            // Use harmonic mean of inverse document frequency and balancing factor
            idf = 2 * idf * idfBalancingFactor / (idf + idfBalancingFactor);
            return tf * idf;
        }

        /// <summary>
        /// Sets the Tfidf value to term-frequency * inverse-document-frequency
        /// </summary>
        /// <param name="numberOfExamples">The number of examples (e.g., documents).</param>
        /// <param name="numberOfCategories">The number of categories (e.g., doc types).</param>
        public TermInfo(string text, double termFrequency, double documentFrequency, int numberOfExamples, int numberOfCategories)
        {
            try
            {
                Text = text;
                TermFrequencyInverseDocumentFrequency =
                    GetScore(termFrequency, documentFrequency, numberOfExamples, numberOfCategories);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41830");
            }
        }
    }
}
