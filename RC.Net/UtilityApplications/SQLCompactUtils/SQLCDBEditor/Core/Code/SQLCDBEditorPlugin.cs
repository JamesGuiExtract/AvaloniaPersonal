﻿using Extract.Utilities;
using System;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// A base class for any <see cref="SQLCDBEditor"/> plugins.
    /// <para><b>Note</b></para>
    /// In concept, this should be an abstract class. However, overrides of this class will be
    /// instantiated via <see cref="UtilityMethods.CreateTypeFromAssembly"/> with
    /// SQLCDBEditorPlugin as the type to instantiate. For that call to work, SQLCDBEditorPlugin
    /// cannot be abstract.
    /// </summary>
    public class SQLCDBEditorPlugin : UserControl
    {
        #region Events

        /// <summary>
        /// Raised when the plugin has modified data in the database.
        /// </summary>
        public event EventHandler<DataChangedEventArgs> DataChanged;

        /// <summary>
        /// Raised when the plugin has a new status message to display (may be
        /// <see langword="null"/> to clear the existing status message.
        /// </summary>
        public event EventHandler<StatusMessageChangedEventArgs> StatusMessageChanged;

        #endregion Events

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLCDBEditorPlugin"/> class.
        /// </summary>
        public SQLCDBEditorPlugin()
            : base()
        {
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public virtual string DisplayName
        {
            get
            {
                throw new ExtractException("ELI34831",
                    "Cannot use base class instance ofSQLCDBEditorPlugin class.");
            }
        }

        /// <summary>
        /// If not <see langword="null"/>, results of this query are displayed in a pane above the
        /// plugin control.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public virtual string Query
        {
            get
            {
                throw new ExtractException("ELI34832",
                    "Cannot use un-derived instance ofSQLCDBEditorPlugin class.");
            }
        }

        /// <summary>
        /// Allows plugin to initialize.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> manager for
        /// this plugin.</param>
        /// <param name="connection">The <see cref="SqlCeConnection"/> for use by the plugin.</param>
        public virtual void LoadPlugin(ISQLCDBEditorPluginManager pluginManager,
            SqlCeConnection connection)
        {
            throw new ExtractException("ELI34833",
                "Cannot use un-derived instance ofSQLCDBEditorPlugin class.");
        }

        /// <summary>
        /// Clears any currently displayed status message.
        /// </summary>
        protected virtual void ClearStatusMessage()
        {
            OnStatusMessageChanged(null, Color.Empty);
        }

        /// <summary>
        /// Displays the specified <see paramref="statusMessage"/>.
        /// </summary>
        /// <param name="statusMessage">The status message to display.</param>
        /// <param name="textColor">The color the status message text should be or
        /// <see cref="Color.Empty"/> to use the default status message color.</param>
        protected virtual void ShowStatusMessage(string statusMessage, Color textColor)
        {
            OnStatusMessageChanged(statusMessage, textColor);
        }

        /// <summary>
        /// Raises the <see cref="DataChanged"/> event.
        /// </summary>
        /// <param name="dataCommitted"><see langword="true"/> if the changed data was committed;
        /// <see langword="false"/> if the change is in progress.</param>
        protected void OnDataChanged(bool dataCommitted)
        {
            var eventHandler = DataChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new DataChangedEventArgs(dataCommitted));
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessageChanged"/> event.
        /// </summary>
        /// <param name="statusMessage">The message to display or <see langword="null"/> to clear
        /// any existing status message.</param>
        /// <param name="textColor">The color the status message text should be or
        /// <see cref="Color.Empty"/> to use the default status message color.</param>
        void OnStatusMessageChanged(string statusMessage, Color textColor)
        {
            var eventHandler = StatusMessageChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new StatusMessageChangedEventArgs(statusMessage, textColor));
            }
        }
    }
}
