﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

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

    /// <summary>
    /// The arguments used for the <see cref="T:QueryAndResultsControl.DataChanged"/> event.
    /// </summary>
    public class DataChangedEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataChangedEventArgs"/> class.
        /// </summary>
        /// <param name="dataCommitted"><see langword="true"/> if the changed data was committed;
        /// <see langword="false"/> if the change is in progress.</param>
        /// <param name="refreshSource"><see langword="true"/> if the
        /// <see cref="QueryAndResultsControl"/> that raised the event should be refreshed as well;
        /// otherwise, <see langword="false"/>.</param>
        public DataChangedEventArgs(bool dataCommitted, bool refreshSource)
            : base()
        {
            DataCommitted = dataCommitted;
            RefreshSource = refreshSource;
        }

        /// <summary>
        /// Gets a value indicating whether the changed data was committed.
        /// </summary>
        /// <value><see langword="true"/> if the changed data was committed; <see langword="false"/>
        /// if the change is in progress.
        /// </value>
        public bool DataCommitted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="QueryAndResultsControl"/> that raised
        /// the event should be refreshed.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="QueryAndResultsControl"/> that raised
        /// the event should be refreshed as well; otherwise, <see langword="false"/>.</value>
        public bool RefreshSource
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The arguments used for the <see cref="QueryAndResultsControl.StatusMessageChanged"/> event.
    /// </summary>
    public class StatusMessageChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusMessageChangedEventArgs"/> class.
        /// </summary>
        /// <param name="statusMessage">The status message to display.</param>
        public StatusMessageChangedEventArgs(string statusMessage)
            : base()
        {
            StatusMessage = statusMessage;
            TextColor = Color.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusMessageChangedEventArgs"/> class.
        /// </summary>
        /// <param name="statusMessage">The status message to display.</param>
        /// <param name="textColor">The color the status message text should be or
        /// <see cref="Color.Empty"/> to use the default status message color.</param>
        public StatusMessageChangedEventArgs(string statusMessage, Color textColor)
            : base()
        {
            StatusMessage = statusMessage;
            TextColor = textColor;
        }

        /// <summary>
        /// Gets the status message to display.
        /// </summary>
        /// <value>
        /// The status message to display.
        /// </value>
        public string StatusMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="Color"/> status message text should be or <see cref="Color.Empty"/>
        /// to use the default status message color.
        /// </summary>
        /// <value>
        /// The <see cref="Color"/> status message text should be
        /// </value>
        public Color TextColor
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The arguments used for the <see cref="T:QueryAndResultsControl.SelectionChanged"/> event.
    /// </summary>
    public class GridSelectionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridSelectionEventArgs"/> class.
        /// </summary>
        /// <param name="selectedRows">The <see cref="DataRow"/>s associated with the currently
        /// selected <see cref="System.Windows.Forms.DataGridViewRow"/>s.</param>
        public GridSelectionEventArgs(IEnumerable<DataRow> selectedRows)
            : base()
        {
            SelectedRows = new List<DataRow>(selectedRows);
        }

        /// <summary>
        /// Gets the <see cref="DataRow"/>s associated with the currently selected
        /// <see cref="System.Windows.Forms.DataGridViewRow"/>s.
        /// </summary>
        public IEnumerable<DataRow> SelectedRows
        {
            get;
            private set;
        }
    }
}
