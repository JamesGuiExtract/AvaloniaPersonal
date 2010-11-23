using System;
using System.Text;

namespace Extract.DataEntry
{
    public partial class DataEntryTextBox
    {
        /// <summary>
        /// Represents the selection state of an <see cref="DataEntryTextBox"/> instance.
        /// </summary>
        class SelectionState : Extract.DataEntry.SelectionState
        {
            #region Fields

            /// <summary>
            /// The starting index of the <see cref="DataEntryTextBox"/> selection.
            /// </summary>
            readonly int _selectionStart;

            /// <summary>
            /// The length of the <see cref="DataEntryTextBox"/> selection.
            /// </summary>
            readonly int _selectionLength;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionState"/> class.
            /// </summary>
            /// <param name="textBox">The <see cref="DataEntryTextBox"/> the selection state applies
            /// to.</param>
            public SelectionState(DataEntryTextBox textBox)
                : base(textBox, DataEntryMethods.AttributeAsVector(textBox._attribute), false, true, null)
            {
                try
                {
                    _selectionStart = textBox._lastSelectionStart;
                    _selectionLength = textBox._lastSelectionLength;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31072", ex);
                }
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Gets the starting index of the <see cref="DataEntryTextBox"/> selection.
            /// </summary>
            /// <value>The starting index of the <see cref="DataEntryTextBox"/> selection.</value>
            public int SelectionStart
            {
                get
                {
                    return _selectionStart;
                }
            }

            /// <summary>
            /// Gets the length of the <see cref="DataEntryTextBox"/> selection.
            /// </summary>
            /// <value>The length of the <see cref="DataEntryTextBox"/> selection.</value>
            public int SelectionLength
            {
                get
                {
                    return _selectionLength;
                }
            }

            #endregion Properties
        }
    }
}
