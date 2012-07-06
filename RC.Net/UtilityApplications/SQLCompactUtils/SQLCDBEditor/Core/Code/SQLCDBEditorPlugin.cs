using Extract.Utilities;
using System;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
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
    }
}
