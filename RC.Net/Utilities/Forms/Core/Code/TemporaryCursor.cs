using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Temporarily changes the mouse cursor to a specified value and resets the
    /// mouse cursor when <see cref="Dispose()"/> is called.  Useful to use in a 
    /// using statement.
    /// </summary>
    /// <example>Using TemporaryCursor<para/>
    /// <code lang="C#">
    /// using(new TemporaryCursor(Cursors.WaitCursor))
    /// {
    ///     performLongRunningFunction();
    /// } // Cursor will be restored here no matter how the using statement is exited
    /// </code>
    /// </example>
    public class TemporaryCursor : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(TemporaryCursor).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Holds the <see cref="Cursor"/> to restore when disposed.
        /// </summary>
        private Cursor saved;

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="TemporaryCursor"/> class.
        /// </summary>
        /// <param name="newCursor">The new <see cref="Cursor"/> to display.</param>
        public TemporaryCursor(Cursor newCursor)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23159");

                // Store the current cursor
                saved = Cursor.Current;

                // Set the cursor to the specified cursor
                Cursor.Current = newCursor;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23160", ex);
            }
        }

        #endregion Constructors

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TemporaryCursor"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="TemporaryCursor"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TemporaryCursor"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        private void Dispose(bool disposing)
        {
            // Dispose of managed resources
            if (disposing)
            {
                // Set the cursor back to the saved cursor
                Cursor.Current = saved;
            }

            // No unmanaged resources to release
        }

        #endregion
    }
}
