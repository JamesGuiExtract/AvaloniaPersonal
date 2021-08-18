using Extract.Utilities;
using System;
using System.Data.Common;
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
        /// Gets a value indicating whether the plugin will display the editor provided grid with
        /// data populated with the results of the <see cref="Query"/> property.
        /// </summary>
        /// <value><see langword="true"/> if the plugin will display the editor provided grid;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public virtual bool DisplayGrid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Indicates whether the plugin's <see cref="Control"/> should be displayed in the
        /// <see cref="QueryAndResultsControl"/>.
        /// </summary>
        /// <value><see langword="true"/> if the plugin's control should be displayed;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public virtual bool DisplayControl
        {
            get
            {
                return false;
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
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="BindingSource"/> property should be used
        /// to populate the results grid rather that the results of <see cref="Query"/>.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="BindingSource"/> property should be used
        /// to populate the results grid; otherwise, <see langword="false"/>.
        /// </value>
        public virtual bool ProvidesBindingSource
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="BindingSource"/> to use for the results grid data if
        /// <see cref="ProvidesBindingSource"/> is <see langword="true"/>.
        /// </summary>
        public virtual BindingSource BindingSource
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this plugin's data is valid.
        /// </summary>
        /// <value><see langword="true"/> if the plugin data is valid; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool DataIsValid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Performs any custom refresh logic needed by the plugin. Generally a plugin where
        /// <see cref="ProvidesBindingSource"/> is <see langword="true"/> will need to perform the
        /// refresh of the data here.
        /// </summary>
        public virtual void RefreshData()
        {
        }

        /// <summary>
        /// Allows plugin to initialize.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> manager for
        /// this plugin.</param>
        /// <param name="connection">The <see cref="DbConnection"/> for use by the plugin.</param>
        public virtual void LoadPlugin(ISQLCDBEditorPluginManager pluginManager,
            DbConnection connection)
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
        /// <param name="refreshSource"><see langword="true"/> if the
        /// <see cref="QueryAndResultsControl"/> that raised the event should be refreshed as well;
        /// otherwise, <see langword="false"/>.</param>
        protected void OnDataChanged(bool dataCommitted, bool refreshSource)
        {
            var eventHandler = DataChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new DataChangedEventArgs(dataCommitted, refreshSource));
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
