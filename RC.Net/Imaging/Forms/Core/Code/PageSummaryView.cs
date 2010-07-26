using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a control that displays a grid of visited pages.
    /// </summary>
    public partial class PageSummaryView : UserControl, IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The number of cells in each row of the <see cref="PageSummaryView"/> grid.
        /// </summary>
        const int _CELLS_PER_ROW = 10;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The image viewer associated with the <see cref="PageSummaryView"/>.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// The number of pages that have been visited.
        /// </summary>
        int _visitedPageCount;

        /// <summary>
        /// The cell style associated with pages that have been visited.
        /// </summary>
        DataGridViewCellStyle _visitedPageStyle;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="PageSummaryView"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PageSummaryView()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the number of pages that are marked as visited.
        /// </summary>
        /// <value>The number of pages that are marked as visited.</value>
        public int VisitedPageCount
        {
            get
            {
                return _visitedPageCount;
            }
        }

        /// <summary>
        /// Gets whether all pages have been visited.
        /// </summary>
        /// <value><see langword="true"/> if all pages have been visited;
        /// <see langword="false"/> if at least one page has not been visited.</value>
        public bool HasVisitedAllPages
        {
            get
            {
                return _visitedPageCount == _imageViewer.PageCount;
            }
        }

        /// <summary>
        /// Gets the style for visited page cells.
        /// </summary>
        /// <returns>The style for visited page cells.</returns>
        DataGridViewCellStyle VisitedPageStyle
        {
            get
            {
                if (_visitedPageStyle == null)
                {
                    DataGridViewCellStyle style = _dataGridView.DefaultCellStyle.Clone();
                    style.BackColor = Color.Green;
                    _visitedPageStyle = style;
                }

                return _visitedPageStyle;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the first unvisited page on or before the specified page.
        /// </summary>
        /// <param name="startPage">The first page to check for being unvisited.</param>
        /// <returns>The first unvisited page on or before the specified page.</returns>
        // Complex operations are better suited as methods.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetPreviousUnvisitedPage(int startPage)
        {
            try
            {
                // Iterate over each page on or before the specified page
                for (int i = startPage; i > 0; i--)
                {
                    // If this cell is unvisited (untagged), return the page number
                    DataGridViewCell cell = GetCellByPageNumber(i);
                    if (cell.Tag == null)
                    {
                        return i;
                    }
                }

                // All prior pages were visited
                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27668",
                    "Unable to get previous unvisited page.", ex);
            }
        }

        /// <summary>
        /// Gets the first unvisited page on or after the specified page.
        /// </summary>
        /// <param name="startPage">The first page to check for being unvisited.</param>
        /// <returns>The first unvisited page on or after the specified page.</returns>
        // Complex operations are better suited as methods.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetNextUnvisitedPage(int startPage)
        {
            try
            {
                // Iterate over each page on or after the specified page
                for (int i = startPage; i <= _imageViewer.PageCount; i++)
                {
                    // If this cell is unvisited (untagged), return the page number
                    DataGridViewCell cell = GetCellByPageNumber(i);
                    if (cell.Tag == null)
                    {
                        return i;
                    }
                }

                // All subsequent pages were visited
                return -1;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI28016",
                    "Unable to get next unvisited page.", ex);
            }
        }

        /// <summary>
        /// Determines whether it is valid to move forward the specified number of pages.
        /// </summary>
        /// <param name="pages">The number of pages to move forward. May be negative.</param>
        /// <returns><see langword="true"/> if moving forward by <paramref name="pages"/> would 
        /// result in a valid page number; <see langword="false"/> if moving forward would be an 
        /// invalid page number.</returns>
        bool IsValidToMoveBy(int pages)
        {
            int newPage = _imageViewer.PageNumber + pages;
            return newPage > 0 && newPage <= _imageViewer.PageCount;
        }

        /// <summary>
        /// Resets the cells of the <see cref="PageSummaryView"/> based on the status of the 
        /// <see cref="_imageViewer"/>.
        /// </summary>
        void Reset()
        {
            // Check if the image viewer's image is available
            bool imageAvailable = _imageViewer != null && _imageViewer.IsImageAvailable;

            // Reset the list view
            _dataGridView.Rows.Clear();
            _visitedPageCount = 0;
            _dataGridView.Enabled = imageAvailable;

            if (imageAvailable)
            {
                CreateCells();
            }
        }

        /// <summary>
        /// Creates a cell corresponding to each page of the currently open image. 
        /// </summary>
        void CreateCells()
        {
            _dataGridView.CurrentCellChanged -= HandleDataGridViewCurrentCellChanged;
            try
            {
                // Compute the number of full rows and remaining cells needed to represent the pages
                int fullRows = _imageViewer.PageCount / _CELLS_PER_ROW;
                int remainingCells = _imageViewer.PageCount % _CELLS_PER_ROW;

                // Create the full rows
                for (int i = 0; i < fullRows; i++)
                {
                    string[] row = CreateRow(i);
                    _dataGridView.Rows.Add(row);
                }

                // Create the last row of remaining cells if necessary
                if (remainingCells > 0)
                {
                    string[] row = CreateRow(fullRows, remainingCells);
                    _dataGridView.Rows.Add(row);
                }
            }
            finally
            {
                _dataGridView.CurrentCellChanged += HandleDataGridViewCurrentCellChanged;
            }
        }

        /// <summary>
        /// Creates the specified row fully populated with page numbers.
        /// </summary>
        /// <param name="rowNumber">The zero-based row to create.</param>
        /// <returns>The row corresponding to <paramref name="rowNumber"/>.</returns>
        static string[] CreateRow(int rowNumber)
        {
            return CreateRow(rowNumber, _CELLS_PER_ROW);
        }

        /// <summary>
        /// Creates the specified row partially populated with page numbers.
        /// </summary>
        /// <param name="rowNumber">The zero-based row to create.</param>
        /// <param name="cellsInRow">The number of cells in the row to contain page numbers.</param>
        /// <returns>The row corresponding to <paramref name="rowNumber"/> with populated with 
        /// <paramref name="cellsInRow"/> number of page number cells.</returns>
        static string[] CreateRow(int rowNumber, int cellsInRow)
        {
            // Prepare the cells of the row
            string[] row = new string[_CELLS_PER_ROW];

            // Set the text for each cell
            int lower = rowNumber * _CELLS_PER_ROW + 1;
            int upper = lower + cellsInRow;
            for (int i = lower; i < upper; i++)
            {
                row[i - lower] = i.ToString(CultureInfo.CurrentCulture);
            }

            // Any remaining cells should be blank
            for (int i = cellsInRow; i < _CELLS_PER_ROW; i++)
            {
                row[i] = "";
            }

            return row;
        }

        /// <summary>
        /// Retrieves the cell that corresponds to the specified page number.
        /// </summary>
        /// <param name="pageNumber">The one-based page number corresponding to the cell to 
        /// retrieve.</param>
        /// <returns>The cell that corresponds to the specified <paramref name="pageNumber"/>.
        /// </returns>
        DataGridViewCell GetCellByPageNumber(int pageNumber)
        {
            int row = (pageNumber - 1) / _CELLS_PER_ROW;
            int column = (pageNumber - 1) % _CELLS_PER_ROW;

            return _dataGridView.Rows[row].Cells[column];
        }

        /// <summary>
        /// Gets the page number to which the specified indices correspond.
        /// </summary>
        /// <param name="row">The zero-based row.</param>
        /// <param name="column">The zero-based column.</param>
        /// <returns>The page number corresponding to the cell at <paramref name="row"/> and 
        /// <paramref name="column"/>.</returns>
        static int GetPageNumberByIndices(int row, int column)
        {
            return row * _CELLS_PER_ROW + column + 1;
        }

        /// <summary>
        /// Creates a collection representing the 0-based pages that have been visited.
        /// </summary>
        /// <returns>A collection representing the 0-based pages that have been visited.</returns>
        // Complex operations are better suited as methods.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public VisitedItemsCollection GetVisitedPages()
        {
            try
            {
                // Iterate over each page
                VisitedItemsCollection items = new VisitedItemsCollection(_imageViewer.PageCount);
                for (int i = 0; i < _imageViewer.PageCount; i++)
                {
                    // If this page is visited (ie. tagged) mark it in the visited items collection
                    DataGridViewCell cell = GetCellByPageNumber(i + 1);
                    if (cell.Tag != null)
                    {
                        items[i] = true;
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27733",
                    "Unable to determine visited pages.", ex);
            }
        }

        /// <summary>
        /// Marks the specified pages as visited.
        /// </summary>
        /// <param name="pages">A collection of 0-based page numbers representing the visited 
        /// pages.</param>
        public void SetVisitedPages(VisitedItemsCollection pages)
        {
            try
            {
                if (pages != null)
                {
                    // Mark each page as visited
                    foreach (int page in pages)
                    {
                        DataGridViewCell cell = GetCellByPageNumber(page + 1);
                        MarkAsVisited(cell);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27734",
                    "Unable to set visited pages.", ex);
            }
        }

        /// <summary>
        /// Marks the specified cell has having been visited.
        /// </summary>
        /// <param name="currentCell">The cell to mark as visited.</param>
        void MarkAsVisited(DataGridViewCell currentCell)
        {
            if (currentCell.Tag == null)
            {
                currentCell.Tag = new object();
                currentCell.Style = VisitedPageStyle;

                _visitedPageCount++;
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// otherwise, <see langword="false"/>.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg == WindowsMessage.KeyDown || msg.Msg == WindowsMessage.SystemKeyDown)
            {
                switch (keyData)
                {
                    case Keys.Down:

                        // If moving would move to an invalid page cell, don't process the key.
                        if (!IsValidToMoveBy(_CELLS_PER_ROW))
                        {
                            return true;
                        }
                        break;

                    case Keys.Right:

                        // If moving would move to an invalid page cell, don't process the key.
                        if (!IsValidToMoveBy(1))
                        {
                            return true;
                        }
                        break;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                Reset();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26984", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.</param>
        void HandlePageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // Select the appropriate cell if it is not already selected
                DataGridViewCell currentCell = GetCellByPageNumber(e.PageNumber);
                if (!currentCell.Selected)
                {
                    currentCell.Selected = true;
                }

                // Tag the cell as visited, unless it is already tagged
                MarkAsVisited(currentCell);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27003", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CurrentCellChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DataGridView.CurrentCellChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DataGridView.CurrentCellChanged"/> event.</param>
        void HandleDataGridViewCurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridViewCell cell = _dataGridView.CurrentCell;
                if (cell != null)
                {
                    // Set the image viewer's page to correspond to the current cell
                    int page = GetPageNumberByIndices(cell.RowIndex, cell.ColumnIndex);
                    if (_imageViewer.PageNumber != page)
                    {
                        _imageViewer.PageNumber = page;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27004", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValidating"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DataGridView.CellValidating"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DataGridView.CellValidating"/> event.</param>
        void HandleDataGridViewCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                // Check if a mouse event is about to occur
                if (MouseButtons != MouseButtons.None)
                {
                    // Is a cell being clicked?
                    Point mouse = _dataGridView.PointToClient(MousePosition);
                    DataGridView.HitTestInfo info = _dataGridView.HitTest(mouse.X, mouse.Y);
                    if (info.Type == DataGridViewHitTestType.Cell)
                    {
                        // If the cell being clicked is invalid, disallow the new selection
                        int page = GetPageNumberByIndices(info.RowIndex, info.ColumnIndex);
                        if (page > _imageViewer.PageCount)
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27043", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="PageSummaryView"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="PageSummaryView"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="PageSummaryView"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
        public ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                try
                {
                    // Unregister from previously subscribed-to events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                        _imageViewer.PageChanged -= HandlePageChanged;
                    }

                    // Store the new image viewer
                    _imageViewer = value;
                    Reset();

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.PageChanged += HandlePageChanged;
                    }
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI26983",
                        "Unable to establish connection to image viewer.", ex);
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
