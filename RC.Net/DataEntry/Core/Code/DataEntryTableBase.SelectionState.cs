using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    public abstract partial class DataEntryTableBase
    {
        /// <summary>
        /// Represents the selection state of an <see cref="DataEntryTableBase"/> instance.
        /// </summary>
        class SelectionState : Extract.DataEntry.SelectionState
        {
            #region Fields

            /// <summary>
            /// The row/column indexes of all selected cells.
            /// </summary>
            List<Tuple<int, int>> _selectedCellLocations;

            /// <summary>
            /// The indexes of all selected rows.
            /// </summary>
            List<int> _selectedRowIndexes;

            /// <summary>
            /// The indexes of all selected columns.
            /// </summary>
            List<int> _selectedColumnIndexes;

            /// <summary>
            /// The row/column index of the current cell.
            /// </summary>
            Tuple<int, int> _currentCellPosition;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionState"/> class.
            /// </summary>
            /// <param name="table">The <see cref="DataEntryTableBase"/> the selection state applies
            /// to.</param>
            /// <param name="attributes">The selected <see cref="IAttribute"/>s.</param>
            /// <param name="includeSubAttributes">If set to <see langword="true"/> count sub
            /// attributes of <see paramref="attributes"/> as selected as well.</param>
            /// <param name="displayToolTips">If set to <see langword="true"/> tool tips should be
            /// displayed for the selected <see cref="IAttribute"/>s.</param>
            /// <param name="selectedGroupAttribute">The selected group attribute,
            /// <see langword="null"/> if no group attribute is selected.</param>
            public SelectionState(DataEntryTableBase table, IUnknownVector attributes,
                bool includeSubAttributes, bool displayToolTips, IAttribute selectedGroupAttribute)
                : base(table, attributes.ToIEnumerable<IAttribute>(), includeSubAttributes,
                    displayToolTips, selectedGroupAttribute)
            {
                try
                {
                    _selectedCellLocations = new List<Tuple<int, int>>(table.SelectedCells
                        .Cast<DataGridViewCell>()
                        .Select(c => new Tuple<int, int>(c.RowIndex, c.ColumnIndex)));

                    _selectedRowIndexes = new List<int>(table.SelectedRows
                        .Cast<DataGridViewRow>().Select(r => r.Index));

                    _selectedColumnIndexes = new List<int>(table.SelectedColumns
                        .Cast<DataGridViewColumn>().Select(c => c.Index));

                    if (table.CurrentCell != null)
                    {
                        _currentCellPosition = new Tuple<int, int>(
                            table.CurrentCell.RowIndex, table.CurrentCell.ColumnIndex);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31025", ex);
                }
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Gets the row/column indexes of all selected cells.
            /// </summary>
            /// <value>The row/column indexes of all selected cells.</value>
            public IEnumerable<Tuple<int, int>> SelectedCellLocations
            {
                get
                {
                    return _selectedCellLocations;
                }
            }

            /// <summary>
            /// Gets the indexes of all selected rows.
            /// </summary>
            /// <value>The indexes of all selected rows.</value>
            public IEnumerable<int> SelectedRowIndexes
            {
                get
                {
                    return _selectedRowIndexes;
                }
            }

            /// <summary>
            /// Gets the indexes of all selected columns.
            /// </summary>
            /// <value>The indexes of all selected columns.</value>
            public IEnumerable<int> SelectedColumnIndexes
            {
                get
                {
                    return _selectedColumnIndexes;
                }
            }

            /// <summary>
            /// Gets the row/column index of the current cell or <see langword="null"/> if there is
            /// no current cell.
            /// </summary>
            /// <value>The row/column index of the current cell or <see langword="null"/> if there
            /// is no current cell.</value>
            public Tuple<int, int> CurrentCellPosition
            {
                get
                {
                    return _currentCellPosition;
                }
            }

            #endregion Properties
        }
    }
}
