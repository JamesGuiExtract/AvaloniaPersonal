﻿using System;
using System.Collections.Generic;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Event arguments for the DuplicateDocumentsApplied event.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class DuplicateDocumentsAppliedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateDocumentsAppliedEventArgs"/> class.
        /// </summary>
        /// <param name="fileActions">A dictionary of duplicate document actions to the set of file
        /// IDs to which each action was applied.</param>
        public DuplicateDocumentsAppliedEventArgs(Dictionary<string, IEnumerable<int>> fileActions)
            : base()
        {
            FileActions = fileActions;
        }

        /// <summary>
        /// Gets a dictionary of duplicate document actions to the set of file IDs to which each
        /// action was applied.
        /// </summary>
        public Dictionary<string, IEnumerable<int>> FileActions
        {
            get;
        }
    }
}
