using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// A dialog window that can be used to configure an on and off schedule for each hour
    /// of the week.
    /// </summary>
    public partial class SetProcessingScheduleForm : Form
    {
        #region Constants

        /// <summary>
        /// The hours in a day (used to configure the hour rows in the grid).
        /// </summary>
        static readonly string[] _HOURS = new string[] {"12 AM", "1 AM", "2 AM", "3 AM",
            "4 AM", "5 AM", "6 AM", "7 AM", "8 AM", "9 AM", "10 AM", "11 AM", "12 PM",
            "1 PM", "2 PM", "3 PM", "4 PM", "5 PM", "6 PM", "7 PM", "8 PM", "9 PM",
            "10 PM", "11 PM"};

        /// <summary>
        /// The color to display for active cells.
        /// </summary>
        static readonly Color _ACTIVE_COLOR = Color.LimeGreen;

        /// <summary>
        /// The color to display for inactive cells.
        /// </summary>
        static readonly Color _INACTIVE_COLOR = Color.White;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The schedule of active and inactive hours (represented by <see cref="bool"/> values).
        /// </summary>
        List<bool> _schedule;

        /// <summary>
        /// The grid style for cells that indicate processing is active for the hour.
        /// </summary>
        DataGridViewCellStyle _activeStyle = new DataGridViewCellStyle();

        /// <summary>
        /// The grid style for cells that indicate processing is inactive for the hour.
        /// </summary>
        DataGridViewCellStyle _inactiveStyle = new DataGridViewCellStyle();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetProcessingScheduleForm "/> class.
        /// </summary>
        /// <param name="schedule">The current schedule to initialize with.</param>
        public SetProcessingScheduleForm(IList<bool> schedule = null)
        {
            try
            {
                ExtractException.Assert("ELI30421", "Invalid schedule size.",
                schedule == null
                || schedule.Count == SetProcessingSchedule._NUMBER_OF_HOURS_IN_WEEK);

                InitializeComponent();

                _schedule = schedule != null ?
                    new List<bool>(schedule) :
                    new List<bool>(SetProcessingSchedule._NUMBER_OF_HOURS_IN_WEEK);

                _activeStyle.BackColor = _ACTIVE_COLOR;
                _inactiveStyle.BackColor = _INACTIVE_COLOR;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30427", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                InitializeDataGrid();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30428", ex);
            }
        }

        /// <summary>
        /// Handles any errors that occur in the datagrid and displays the exception to
        /// the user.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the error.</param>
        void HandleDataGridErrorEvent(object sender, DataGridViewDataErrorEventArgs e)
        {
            ExtractException.Display("ELI30422", e.Exception);
            e.ThrowException = false;
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.RowPrePaint"/> event.
        /// This event handler was added to be able to remove the row indicator
        /// from the row header.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleScheduleGridRowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                e.PaintCells(e.ClipBounds, DataGridViewPaintParts.All);
                e.PaintHeader(DataGridViewPaintParts.Background | DataGridViewPaintParts.Border
                    | DataGridViewPaintParts.Focus | DataGridViewPaintParts.SelectionBackground
                    | DataGridViewPaintParts.ContentForeground);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30423", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the select all button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data asssociated with the event.</param>
        void HandleSelectAllClicked(object sender, EventArgs e)
        {
            try
            {
                SetAllCellStyles(true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30424", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the select none button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data asssociated with the event.</param>
        void HandleSelectNoneClicked(object sender, EventArgs e)
        {
            try
            {
                SetAllCellStyles(false);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30425", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.MouseUp"/> event for the schedule grid.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data asssociated with the event.</param>
        void HandleGridMouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (_scheduleGrid.SelectedCells.Count > 0)
                {
                    foreach (DataGridViewCell cell in _scheduleGrid.SelectedCells)
                    {
                        int index = cell.RowIndex + (cell.ColumnIndex * 24);
                        _schedule[index] = !_schedule[index];
                        cell.Style = _schedule[index] ? _activeStyle : _inactiveStyle;
                    }
                    _scheduleGrid.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30426", ex);
            }
        }

        /// <summary>
        /// Initializes the grid with off and on hours.
        /// </summary>
        void InitializeDataGrid()
        {
            _scheduleGrid.Rows.Add(_HOURS.Length);

            // Fill in the hours column
            for (int i=0; i < _HOURS.Length; i++)
            {
                _scheduleGrid.Rows[i].HeaderCell.Value = _HOURS[i];
            }

            for (int i = 0; i < _scheduleGrid.Rows.Count; i++)
            {
                var row = _scheduleGrid.Rows[i];
                for (int j = 0; j < _scheduleGrid.Columns.Count; j++)
                {
                    var cell = row.Cells[j];
                    cell.Style = _schedule[i + (j * 24)] ? _activeStyle : _inactiveStyle;
                }
            }

            _scheduleGrid.ClearSelection();
        }

        /// <summary>
        /// Sets all cells to active or inactive in the schedule grid.
        /// </summary>
        /// <param name="active">Whether the cells should be set to active or inactive.</param>
        void SetAllCellStyles(bool active)
        {
            var style = active ? _activeStyle : _inactiveStyle;

            for (int i = 0; i < _scheduleGrid.Rows.Count; i++)
            {
                var row = _scheduleGrid.Rows[i];
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    row.Cells[j].Style = style;
                    _schedule[i + (j * 24)] = active;
                }
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the list of scheduled on and off hours.
        /// </summary>
        public ReadOnlyCollection<bool> Schedule
        {
            get
            {
                return _schedule.AsReadOnly();
            }
        }

        #endregion Properties
    }
}
