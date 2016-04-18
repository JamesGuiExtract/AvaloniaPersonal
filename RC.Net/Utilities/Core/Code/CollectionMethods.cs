using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UCLID_COMUTILSLib;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a grouping of methods for performing operations on collections.
    /// </summary>
    public static class CollectionMethods
    {
        #region Private Fields

        /// <summary>
        /// Thread static random number generator used by Shuffle methods
        /// </summary>
        private static readonly ThreadLocal<Random> _shuffleRandom = new ThreadLocal<Random>(() => new Random());

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Removes all elements of the specified list and calls <see cref="IDisposable.Dispose"/> 
        /// on all items that implement <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="list">The list of items to be cleared and disposed.</param>
        public static void ClearAndDisposeObjects(IList list)
        {
            try
            {
                // Iterate backwards through the list
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    // Remove and dispose of the item
                    IDisposable item = list[i] as IDisposable;
                    list.RemoveAt(i);

                    // Dispose of the item if implements IDisposable
                    if (item != null)
                    {
                        item.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22672", ex);
            }
        }

        /// <summary>
        /// Removes all elements of the specified dictionary and calls
        /// <see cref="IDisposable.Dispose"/> on all values that
        /// implement <see cref="IDisposable"/>.
        /// <para><b>Note:</b></para>
        /// This will not call <see cref="IDisposable.Dispose"/> on the keys. The caller is
        /// responsible to dispose of the keys (must do this before calling ClearAndDispose
        /// as this function will clear the collection).
        /// </summary>
        /// <param name="dictionary">The dictionary of items to be cleared and disposed.</param>
        public static void ClearAndDisposeObjects(IDictionary dictionary)
        {
            try
            {
                // Iterate through all of the values in the dictionary
                foreach (object value in dictionary.Values)
                {
                    IDisposable item = value as IDisposable;
                    if (item != null)
                    {
                        item.Dispose();
                    }
                }

                // Clear the dictionary
                dictionary.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27957", ex);
            }
        }

        /// <summary>
        /// Removes all elements of the specified list and calls <see cref="IDisposable.Dispose"/>
        /// on each element before removing it
        /// </summary>
        /// <typeparam name="T">The type stored in the list.</typeparam>
        /// <param name="list">The list of items to be cleared and disposed.</param>
        public static void ClearAndDispose<T>(IList<T> list) where T : class, IDisposable
        {
            try
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    T item = list[i];
                    if (item != null)
                    {
                        item.Dispose();
                    }

                    list.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27973", ex);
            }
        }

        /// <summary>
        /// Removes all elements of the specified <see cref="HashSet{T}"/> and calls
        /// <see cref="IDisposable.Dispose"/> on each element before removing it.
        /// </summary>
        /// <typeparam name="T">The type stored in the set.</typeparam>
        /// <param name="set">The set of items to be cleared and disposed.</param>
        public static void ClearAndDispose<T>(HashSet<T> set) where T : class, IDisposable
        {
            try
            {
                foreach (T item in set)
                {
                    item.Dispose();
                }

                set.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35601");
            }
        }

        /// <summary>
        /// Removes all elements of the specified dictionary and calls
        /// <see cref="IDisposable.Dispose"/> on all values.
        /// <para><b>Note:</b></para>
        /// This will not call <see cref="IDisposable.Dispose"/> on the keys. The caller is
        /// responsible to dispose of the keys (must do this before calling ClearAndDispose
        /// as this function will clear the collection).
        /// </summary>
        /// <param name="dictionary">The dictionary of items to be cleared and disposed.</param>
        /// <typeparam name="TKey">The key type in the dictionary</typeparam>
        /// <typeparam name="TValue">The value type in the dictionary. This type
        /// must implement <see cref="IDisposable"/>.</typeparam>
        public static void ClearAndDispose<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            where TValue : class, IDisposable
        {
            try
            {
                // Iterate through all of the values in the dictionary
                foreach (TValue value in dictionary.Values)
                {
                    if (value != null)
                    {
                        value.Dispose();
                    }
                }

                // Clear the dictionary
                dictionary.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28008", ex);
            }
        }

        /// <summary>
        /// Removes all elements of the specified dictionary and calls
        /// <see cref="IDisposable.Dispose"/> on all keys and values in the collection.
        /// </summary>
        /// <typeparam name="TKey">The key type in the dictionary. This type
        /// must implement <see cref="IDisposable"/>.</typeparam>
        /// <typeparam name="TValue">The value type in the dictionary. This type
        /// must implement <see cref="IDisposable"/>.</typeparam>
        /// <param name="dictionary">The dictionary of items to be cleared and disposed.</param>
        public static void ClearAndDisposeKeysAndValues<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary)
            where TKey : class, IDisposable
            where TValue : class, IDisposable
        {
            try
            {
                // Loop through each key value pair
                foreach (KeyValuePair<TKey, TValue> pair in dictionary)
                {
                    // Dispose of the key
                    if (pair.Key != null)
                    {
                        pair.Key.Dispose();
                    }

                    // Dispose of the value
                    if (pair.Value != null)
                    {
                        pair.Value.Dispose();
                    }
                }

                // Clear the collection
                dictionary.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27974", ex);
            }
        }

        /// <summary>
        /// Creates a shallow copy of the provided stack so that the supplied <see cref="Stack"/> 
        /// so that the resulting <see cref="Stack"/> contains the same elements in the same order.
        /// </summary>
        /// <param name="originalStack">The <see cref="Stack"/> to be copied</param>
        /// <returns>A new <see cref="Stack"/> containing the same elements in the same order as the
        /// original.</returns>
        public static Stack<T> CopyStack<T>(Stack<T> originalStack)
        {
            try
            {
                // Obtain the elements in the stack.
                T[] elementArray = originalStack.ToArray();
                
                // Reverse the elements, otherwise what had been on top of the stack will now be
                // on the bottom.
                Array.Reverse(elementArray);

                // Return a new stack based on the original's elements.
                return new Stack<T>(elementArray);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24917", ex);
            }
        }

        /// <summary>
        /// Copies the elements of <paramref name="source"/> to a new array.
        /// </summary>
        /// <typeparam name="T">The type of elements in <paramref name="source"/>.</typeparam>
        /// <param name="source">The object to convert to an array.</param>
        /// <returns>An array containing the elements of <paramref name="source"/>.</returns>
        public static T[] ToArray<T>(IEnumerable<T> source)
        {
            try
            {
                List<T> list = new List<T>(source);
                return list.ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26708", ex);
            }
        }

        /// <summary>
        /// Converts <see paramref="enumerable"/> into an <see cref="IIUnknownVector"/>.
        /// </summary>
        /// <typeparam name="T">The type of object in the enumerable.</typeparam>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to convert.</param>
        /// <returns>An <see cref="IIUnknownVector"/> of type <see paramref="T"/>.</returns>
        [CLSCompliant(false)]
        public static IUnknownVector ToIUnknownVector<T>(this IEnumerable<T> enumerable)
        {
            try
            {
                IUnknownVector vector = new IUnknownVector();
                foreach (T value in enumerable)
                {
                    vector.PushBack(value);
                }

                return vector;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31777", ex);
            }
        }

        /// <summary>
        /// Converts <see paramref="enumerable"/> into an <see cref="VariantVector"/>.
        /// </summary>
        /// <typeparam name="T">The type of object in the enumerable.</typeparam>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to convert.</param>
        /// <returns>An <see cref="VariantVector"/> of type <see paramref="T"/>.</returns>
        [CLSCompliant(false)]
        public static VariantVector ToVariantVector<T>(this IEnumerable<T> enumerable)
        {
            try
            {
                VariantVector vector = new VariantVector();
                foreach (T value in enumerable)
                {
                    vector.PushBack(value);
                }

                return vector;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35186", ex);
            }
        }

        /// <summary>
        /// Converts <see paramref="comVector"/> into an <see cref="IEnumerable"/>.
        /// </summary>
        /// <typeparam name="T">The type of object in the vector.</typeparam>
        /// <param name="comVector">The <see cref="IIUnknownVector"/> to convert.</param>
        /// <returns>An <see cref="IEnumerable"/> of type <see paramref="T"/>.</returns>
        [CLSCompliant(false)]
        public static IEnumerable<T> ToIEnumerable<T>(this IIUnknownVector comVector)
        {
            int size = comVector.Size();

            for (int i = 0; i < size; i++)
            {
                yield return (T)comVector.At(i);
            }
        }

        /// <summary>
        /// Converts <see paramref="variantVector"/> into an <see cref="IEnumerable"/>.
        /// </summary>
        /// <typeparam name="T">The type of object in the vector.</typeparam>
        /// <param name="variantVector">The <see cref="IVariantVector"/> to convert.</param>
        /// <returns>An <see cref="IEnumerable"/> of type <see paramref="T"/>.</returns>
        [CLSCompliant(false)]
        public static IEnumerable<T> ToIEnumerable<T>(this IVariantVector variantVector)
        {
            int size = variantVector.Size;
            for (int i = 0; i < size; i++)
            {
                yield return (T)variantVector[i];
            }
        }

        /// <summary>
        /// Converts IStrToStrMap into a Dictionary
        /// </summary>
        /// <param name="instance">The IStrToStr instance to convert.</param>
        /// <returns>A dictionary of type Dictionary</returns>
        [CLSCompliant(false)]
        public static Dictionary<string, string> ToDictionary(this IStrToStrMap instance)
        {
            try
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();

                for (int i = 0; i < instance.Size; ++i)
                {
                    string key;
                    string value;
                    instance.GetKeyValue(i, out key, out value);
                    temp.Add(key, value);
                }

                return temp;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38760");
            }
        }

        /// <summary>
        /// Tries to add the key value pair to the <see cref="Dictionary{K,V}"/> if the key is
        /// not already present in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type for the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type for the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><see langword="true"/> if the key and value are added to the dictionary
        /// or <see langword="false"/> otherwise.</returns>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, TValue value)
        {
            try
            {
                if (dictionary.ContainsKey(key))
                {
                    return false;
                }

                dictionary[key] = value;

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31806");
            }
        }

        /// <summary>
        /// Gets the value associated with the given key. If the key is not already present in the
        /// dictionary the <see paramref="valueFactory"/> nullary function is used to create the
        /// value, which is added to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type for the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type for the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The nullary delegate function used to create the value.</param>
        /// <returns>The value associated with the given key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, Func<TValue> valueFactory)
        {
            try
            {
                TValue value;
                if (!dictionary.TryGetValue(key, out value))
                {
                    value = valueFactory();
                    dictionary.Add(key, value);
                }
                return value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38960");
            }
        }

        /// <summary>
        /// Gets the value associated with the given key. If the key is not already present in the
        /// dictionary the <see paramref="valueFactory"/> unary function is used to create the value,
        /// which is added to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type for the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type for the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The unary delegate function used to create the value.</param>
        /// <returns>The value associated with the given key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, Func<TKey, TValue> valueFactory)
        {
            try
            {
                TValue value;
                if (!dictionary.TryGetValue(key, out value))
                {
                    value = valueFactory(key);
                    dictionary.Add(key, value);
                }
                return value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37887");
            }
        }

        /// <summary>
        /// Knuth shuffle (a.k.a. the Fisher-Yates shuffle) for arrays.
        /// Performs an in-place random permutation of an array.
        /// Adapted from https://www.rosettacode.org/wiki/Knuth_shuffle#C.23
        /// </summary>
        /// <typeparam name="T">The type of the objects in the array</typeparam>
        /// <param name="array">The array to shuffle</param>
        /// <param name="randomNumberGenerator">An instance of <see cref="System.Random"/> to be used
        /// to generate the permutation. If <see langword="null"/> then a thread-local, static instance
        /// will be used.</param>
        public static void Shuffle<T>(T[] array, Random randomNumberGenerator=null)
        {
            try
            {
                var rng = randomNumberGenerator ?? _shuffleRandom.Value;

                int length = array.Length;

                for (int i = 0; i < length - 1; i++)
                {
                    // Don't select from the entire array length on subsequent loops or the result will be biased
                    int j = rng.Next(i, length);
                    UtilityMethods.Swap(ref array[i], ref array[j]);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI39499", ex);
            }
        }

        /// <summary>
        /// Knuth shuffle (a.k.a. the Fisher-Yates shuffle) for two arrays.
        /// Performs an in-place random permutation of two arrays.
        /// The same permutation is applied to both arrays and each array must be the same length.
        /// Adapted from https://www.rosettacode.org/wiki/Knuth_shuffle#C.23
        /// </summary>
        /// <typeparam name="T1">The type of the objects in the first array</typeparam>
        /// <typeparam name="T2">The type of the objects in the second array</typeparam>
        /// <param name="array1">The first array to shuffle</param>
        /// <param name="array2">The second array to shuffle</param>
        /// <param name="randomNumberGenerator">An instance of <see cref="System.Random"/> to be used
        /// to generate the permutation. If <see langword="null"/> then a thread-local, static instance
        /// will be used.</param>
        public static void Shuffle<T1, T2>(T1[] array1, T2[] array2, Random randomNumberGenerator=null)
        {
            try
            {
                var rng = randomNumberGenerator ?? _shuffleRandom.Value;

                int length = array1.Length;
                ExtractException.Assert("ELI39500", "Arrays are not of the same length", array2.Length == length);

                for (int i = 0; i < length - 1; i++)
                {
                    // Don't select from the entire array length on subsequent loops or the result will be biased
                    int j = rng.Next(i, length);
                    UtilityMethods.Swap(ref array1[i], ref array1[j]);
                    UtilityMethods.Swap(ref array2[i], ref array2[j]);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI39501", ex);
            }
        }

        #endregion Public Methods
    }
}
