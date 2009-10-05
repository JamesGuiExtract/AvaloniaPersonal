using System;
using System.Collections.ObjectModel;

namespace Extract.Imaging
{
    /// <summary>
    /// Interface that defines a Bates number generator that will be used both by the
    /// configuration property page and the object that is applying Bates numbers.
    /// </summary>
    public interface IBatesNumberGenerator : IDisposable
    {
        /// <summary>
        /// Commits the changes to the bates number.
        /// </summary>
        void Commit();

        /// <summary>
        /// Retrieves the next Bates number as text using the page number.
        /// </summary>
        /// <param name="pageNumber">The page number for the Bates number.</param>
        /// <returns>The next Bates number as text.</returns>
        string GetNextNumberString(int pageNumber);

        /// <summary>
        /// Retrieves the next Bates numbers as text using the total page count.
        /// </summary>
        /// <param name="totalPages">The total number of pages for the Bates number.</param>
        /// <returns>The next Bates numbers as text.</returns>
        ReadOnlyCollection<string> GetNextNumberStrings(int totalPages);

        /// <summary>
        /// Returns the next Bates number but does not perform the increment on the number. The
        /// caller should not assume the number returned by this method will ultimately be the
        /// next Bates number, it is just the next Bates number at the time of the call.
        /// </summary>
        /// <returns>The next Bates number (does not perform the Bates number increment).</returns>
        long PeekNextNumber();

        /// <summary>
        /// Retrieves the next Bates number as text without incrementing the Bates number.
        /// </summary>
        /// <param name="pageNumber">The page number on which the Bates number appears.</param>
        /// <returns>The next Bates number as text or the empty string if the Bates number was 
        /// invalid.</returns>
        string PeekNextNumberString(int pageNumber);

        /// <summary>
        /// Gets/sets the underlying Bates number format object.
        /// </summary>
        /// <returns>The <see cref="BatesNumberFormat"/> object.</returns>
        /// <value>The <see cref="BatesNumberFormat"/> object.</value>
        BatesNumberFormat Format
        {
            get;
            set;
        }
    }
}
