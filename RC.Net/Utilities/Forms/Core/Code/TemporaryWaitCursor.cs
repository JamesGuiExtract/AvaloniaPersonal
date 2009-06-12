using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Stores the current <see cref="Cursor"/>, displays the
    /// <see cref="Cursors.WaitCursor"/>, and then restores the
    /// <see cref="Cursor"/> when <see cref="TemporaryCursor.Dispose()"/> is called.
    /// </summary>
    /// <example>
    /// <code>
    /// // Displays the WaitCursor
    /// using(new TemporaryWaitCursor())
    /// {
    ///     performLongRunningOperation();
    /// } // Cursor will be restored here no matter how the using statement is exited
    /// </code>
    /// </example>
    public class TemporaryWaitCursor : TemporaryCursor
    {
        /// <summary>
        /// Stores the current cursor, loads the <see cref="Cursors.WaitCursor"/> and then
        /// restores the current cursor when <see cref="TemporaryCursor.Dispose()"/> is called.
        /// </summary>
        public TemporaryWaitCursor()
            : base(Cursors.WaitCursor)
        {
        }
    }
}
