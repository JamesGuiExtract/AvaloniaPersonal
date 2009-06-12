using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    #region ShortcutsManager Event Arguments

    /// <summary>
    /// Provides data for the <see cref="ShortcutsManager.ShortcutKeyChanged"/> event.
    /// </summary>
    public class ShortcutKeyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Shortcut key that changed.
        /// </summary>
        private readonly Keys _shortcutKey;

        /// <summary>
        /// Shortcut handler associated with the shortcut key that changed.
        /// </summary>
        private readonly ShortcutHandler _shortcutHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutKeyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="shortcutKey">The shortcut key that changed.</param>
        /// <param name="shortcutHandler">The shortcut handler that the shortcut key changed to.
        /// </param>
        public ShortcutKeyChangedEventArgs(Keys shortcutKey, ShortcutHandler shortcutHandler)
        {
            _shortcutKey = shortcutKey;
            _shortcutHandler = shortcutHandler;
        }

        /// <summary>
        /// Gets the shortcut key that changed.
        /// </summary>
        /// <returns>The shortcut key that changed.</returns>
        public Keys ShortcutKey
        {
            get
            {
                return _shortcutKey;
            }
        }

        /// <summary>
        /// Gets the shortcut handler that shortcut key changed to.
        /// </summary>
        /// <value>The shortcut handler that shortcut key changed to. May be <see langword="null"/>
        /// if the shortcut key was removed.</value>
        public ShortcutHandler ShortcutHandler
        {
            get
            {
                return _shortcutHandler;
            }
        }
    }

    #endregion
}