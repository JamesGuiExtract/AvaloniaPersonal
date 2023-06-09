﻿using System;
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
        /// Raised to indicate the data has changed in the grid.
        /// </summary>
        event EventHandler<DataChangedEventArgs> DataChanged;

        /// <summary>
        /// Raised to indicate a DataGridView Cell is formatting
        /// </summary>
        event EventHandler<DataGridViewCellFormattingEventArgs> DataGridViewCellFormatting;

        /// <summary>
        /// Raised when the context menu for a cell is needed
        /// </summary>
        event DataGridViewCellContextMenuStripNeededEventHandler DataGridViewCellContextMenuStripNeeded;

        /// <summary>
        /// Creates a new <see cref="Button"/> in the plugin toolstrip for use by the plugin.
        /// </summary>
        /// <returns>The <see cref="Button"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Button GetNewButton();

        /// <summary>
        /// Adds a control to the plug-in toolstrip created by the plug-in
        /// </summary>
        /// <param name="control">Control to add to the toolstrip</param>
        void AddControlToPluginToolStrip(Control control);

        /// <summary>
        /// Causes the results of the <see cref="SQLCDBEditorPlugin.Query"/> to be refreshed.
        /// </summary>
        void RefreshQueryResults();

        /// <summary>
        /// Allows plug-in to be able to control if a user and add rows
        /// </summary>
        bool AllowUserToAddRows { get; set; }

        /// <summary>
        /// Allows plug-in to be able to control if a user can delete rows
        /// </summary>
        bool AllowUserToDeleteRows { get; set; }
    }
}
