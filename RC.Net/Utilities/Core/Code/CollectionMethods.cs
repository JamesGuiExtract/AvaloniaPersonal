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
        public static void ClearAndDispose(IList list)
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
        /// <see cref="IDisposable.Dispose"/> on all values that implement <see cref="IDisposable"/>.
        /// <para><b>Note:</b></para>
        /// This will not call <see cref="IDisposable.Dispose"/> on the keys. The caller is
        /// responsible to dispose of the keys (must do this before calling ClearAndDispose
        /// as this function will clear the collection).
        /// </summary>
        /// <param name="dictionary">The dictionary of items to be cleared and disposed.</param>
        public static void ClearAndDispose(IDictionary dictionary)
        {
            try
            {
                // Iterate through all of the values in the dictionary
                foreach (Object value in dictionary.Values)
                {
                    // Check for IDisposable interface
                    IDisposable item = value as IDisposable;
                    if (item != null)
                    {
                        // Dispose of the item
                        item.Dispose();
                    }
                }

                // Clear the dictionary
                dictionary.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22673", ex);
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
    }
}
