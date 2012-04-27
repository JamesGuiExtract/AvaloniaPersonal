using System;
using System.ComponentModel;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// The arguments used for the <see cref="T:QueryAndResultsControl.QueryCreated"/> event.
    /// </summary>
    internal class QueryCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="queryAndResultsControl">The <see cref="QueryAndResultsControl"/> that was
        /// created.</param>
        public QueryCreatedEventArgs(QueryAndResultsControl queryAndResultsControl)
            : base()
        {
            QueryAndResultsControl = queryAndResultsControl;
        }

        /// <summary>
        /// Gets the <see cref="QueryAndResultsControl"/> that was created.
        /// </summary>
        public QueryAndResultsControl QueryAndResultsControl
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The arguments used for the <see cref="T:QueryAndResultsControl.QueryRenaming"/> event.
    /// </summary>
    internal class QueryRenamingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRenamingEventArgs"/> class.
        /// </summary>
        /// <param name="newName">The new name of the query.</param>
        public QueryRenamingEventArgs(string newName)
            : base()
        {
            NewName = newName;
        }

        /// <summary>
        /// Gets the new name of the query.
        /// </summary>
        public string NewName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the reason the <see cref="T:CancelEventArgs.Cancel"/> property was set.
        /// <para><b>Note</b></para>
        /// This property should be set whenever <see cref="T:CancelEventArgs.Cancel"/> is set to
        /// <see langword="true"/>.
        /// </summary>
        /// <value>
        /// The <see cref="T:CancelEventArgs.Cancel"/> property was set.
        /// </value>
        public string CancelReason
        {
            get;
            set;
        }
    }
}
