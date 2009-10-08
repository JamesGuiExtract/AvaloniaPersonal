using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a grouping of methods for performing operations on collections.
    /// </summary>
    public static class CollectionMethods
    {
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
    }
}
