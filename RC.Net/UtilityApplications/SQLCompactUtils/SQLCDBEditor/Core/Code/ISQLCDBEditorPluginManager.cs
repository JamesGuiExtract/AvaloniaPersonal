using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// Interface that <see cref="SQLCDBEditorPlugin"/> instances can use to interact with the
    /// SQLCDBEditor application.
    /// </summary>
    public interface ISQLCDBEditorPluginManager
    {
        /// <summary>
        /// Raised to indicate the selection in the query grid has changed.
        /// </summary>
        event EventHandler<GridSelectionEventArgs> SelectionChanged;

        /// <summary>
        /// Creates a new <see cref="Button"/> in the plugin toolstrip for use by the plugin.
        /// </summary>
        /// <returns>The <see cref="Button"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Button GetNewButton();

        /// <summary>
        /// Causes the results of the <see cref="SQLCDBEditorPlugin.Query"/> to be refreshed.
        /// </summary>
        void RefreshQueryResults();
    }
}
