using Lucene.Net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// A size-limited version of a SortedSet. Keeps only greatest items when size is exceeded
    /// </summary>
    internal sealed class LimitedSizeSortedSet<T> : IEnumerable<T>
    {
        readonly int _size;
        readonly SortedSet<T> _items;

        public LimitedSizeSortedSet(IComparer<T> comparer, int size)
        {
            _size  = size;
            _items = new SortedSet<T>(comparer);
        }

        public LimitedSizeSortedSet(IEnumerable<T> collection, IComparer<T> comparer, int size)
        {
            _size  = size;
            _items = new SortedSet<T>(comparer);
            foreach(var item in collection)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            if (_items.Contains(item))
            {
                return;
            }

            if (_items.Count < _size)
            {
                _items.Add(item);
                return;
            }

            if (_items.Comparer.Compare(item, _items.Min) <= 0)
            {
                return;
            }

            _items.Remove(_items.Min);
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)_items).GetEnumerator();
        }

        public int Count => _items.Count;

        public IEnumerable<T> Reverse() => _items.Reverse();
    }

    /// <summary>
    /// Compare terms by document frequency then by text
    /// </summary>
    /// <remarks>Compares such that lower DF comes first but for ties, alphabetically higher comes first</remarks>
    internal sealed class DocFreqComparer : IComparer<Tuple<BytesRef, int>>
    {
        public int Compare(Tuple<BytesRef, int> a, Tuple<BytesRef, int> b)
        {
            int res = (a.Item2).CompareTo((b.Item2));
            if (res == 0)
            {
                res = string.Compare(b.Item1.Utf8ToString(), a.Item1.Utf8ToString(), StringComparison.CurrentCulture);
            }
            return res;
        }
    }

    /// <summary>
    /// Compare terms by 'tf-idf'
    /// </summary>
    /// <remarks>Compares such that lower TfIdf comes first but for ties, alphabetically higher comes first</remarks>
    internal sealed class TfIdfComparer : IComparer<TermInfo>
    {
        public int Compare(TermInfo a, TermInfo b)
        {
            int res = (a.TermFrequencyInverseDocumentFrequency)
                .CompareTo(b.TermFrequencyInverseDocumentFrequency);
            if (res == 0)
            {
                res = string.Compare(b.Text, a.Text, StringComparison.CurrentCulture);
            }
            return res;
        }
    }
}
