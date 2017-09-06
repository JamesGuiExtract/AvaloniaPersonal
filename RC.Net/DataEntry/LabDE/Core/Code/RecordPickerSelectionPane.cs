using Extract.FileActionManager.Utilities;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Defines the custom <see cref="IFFIFileSelectionPane"/> to be used in the FFI instance opened
    /// by an <see cref="RecordPickerTableColumn"/>. This pane will display the records that may be
    /// associated with the currently selected row in a LabDE DEP.
    /// </summary>
    [ToolboxItem(false)]
    public partial class RecordPickerSelectionPane : UserControl, IFFIFileSelectionPane, IFFIDataManager
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(RecordPickerSelectionPane).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// The cell style to apply for any row in <see cref="_recordsDataGridView"/> for which
        /// results have already been filed.
        /// </summary>
        DataGridViewCellStyle _matchedRecordCellStyle;

        /// <summary>
        /// Row indices pending application of a style that indicates records for which results have
        /// previously been filed.
        /// </summary>
        List<int> _rowsPendingMatchedRecordStyle = new List<int>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordPickerSelectionPane"/> class.
        /// </summary>
        public RecordPickerSelectionPane()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI38143", _OBJECT_NAME);

                InitializeComponent();

                // The cell style to apply for any row in _recordsDataGridView for which results have
                // already been filed.
                _matchedRecordCellStyle = new DataGridViewCellStyle(_recordsDataGridView.DefaultCellStyle);
                _matchedRecordCellStyle.BackColor = Color.LightYellow;
                _matchedRecordCellStyle.SelectionForeColor = Color.Yellow;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38144");
            }
        }

        #endregion Constructors

        #region Public Members

        /// <summary>
        /// Gets the record ID that was selected in the UI.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public string SelectedRecordId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the filter on the displayed matching rows that determine which rows are
        /// candidates for auto-selection when the picker UI is displayed. If <see langword="null"/>
        /// records will not be automatically selected (unless a record ID had already been
        /// assigned).
        /// </summary>
        /// <value>
        /// The filter on the displayed matching rows that determine which rows are candidates for
        /// auto-selection. The syntax is as specified for the <see cref="DataColumn.Expression"/>
        /// property.
        /// </value>
        public virtual string AutoSelectionFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the record in which rows matching <see cref="AutoSelectionRecord"/> are to be
        /// considered for auto-selection where the first matching row is the row that is selected.
        /// <see langword="null"/> if a row should be auto-selected only if it is the only row
        /// matching <see cref="AutoSelectionFilter"/>.
        /// </summary>
        /// <value>
        /// The record in which rows matching <see cref="AutoSelectionRecord"/> are to be considered
        /// for auto-selection. The syntax is as described for the <see cref="DataView.Sort"/>
        /// property.
        /// </value>
        public virtual string AutoSelectionRecord
        {
            get;
            set;
        }

        /// <summary>
        /// Updates the data in <see cref="_recordsDataGridView"/> to display the current possible
        /// matching records in the FAM database.
        /// </summary>
        public virtual void UpdateRecordSelectionGrid()
        {
            try
            {
                var disposableSource = _recordsDataGridView.DataSource as IDisposable;

                _recordsDataGridView.DataSource = RowData.UnmappedMatchingRecords;
                for (int i = 0; i < _recordsDataGridView.ColumnCount; i++)
                {
                    _recordsDataGridView.Columns[i].HeaderText =
                        RowData.UnmappedMatchingRecords.Table.Columns[i].Caption;
                }

                if (RowData.DefaultSort != null)
                {
                    var sortedColumn = _recordsDataGridView.Columns
                        .OfType<DataGridViewColumn>()
                        .Single(column => column.Name == RowData.DefaultSort.Item1);
                    _recordsDataGridView.Sort(sortedColumn, RowData.DefaultSort.Item2);
                }

                // https://extract.atlassian.net/browse/ISSUE-14421
                // Indicate already matched records with a different cell style.
                // NOTE: This needs to occur *after* the rows have had default sort applied.
                foreach (var row in _recordsDataGridView.Rows.OfType<DataGridViewRow>())
                {
                    if (RowData.GetCorrespondingFileIds((string)row.Cells[0].Value).Any())
                    {
                        // Style updates are ignored if done before the form is shown, defer the
                        // style update until the form has been displayed if necessary.
                        if (IsHandleCreated)
                        {
                            foreach (var cell in row.Cells.OfType<DataGridViewCell>())
                            {
                                cell.Style = _matchedRecordCellStyle;
                            }
                        }
                        else
                        {
                            _rowsPendingMatchedRecordStyle.Add(row.Index);
                        }
                    }
                }

                if (disposableSource != null)
                {
                    disposableSource.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38149");
            }
        }

        /// <summary>
        /// Gets or sets the accept action to be run when this instance needs to trigger, e.g., its
        /// parent form to close as if the accept button were clicked.
        /// https://extract.atlassian.net/browse/ISSUE-14308
        /// </summary>
        public Action<object, EventArgs> AcceptFunction { get; set; }

        #endregion Public Members

        #region IFFIFileSelectionPane

        /// <summary>
        /// Raised when the file list indicated by the selection pane has changed.
        /// </summary>
        public event EventHandler<EventArgs> RefreshRequired;

        /// <summary>
        /// Gets the title of the pane.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return "Available records";
            }
        }

        /// <summary>
        /// The IDs of the files to be populated in the FFI file list.
        /// </summary>
        public virtual IEnumerable<int> SelectedFileIds
        {
            get
            {
                try
                {
                    string recordId = GetSelectedRecordId();

                    if (string.IsNullOrEmpty(recordId))
                    {
                        return new int[0];
                    }
                    else
                    {
                        return RowData.GetCorrespondingFileIds(recordId);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38145");
                }
            }
        }

        /// <summary>
        /// The position of the pane in the FFI.
        /// </summary>
        public virtual SelectionPanePosition PanePosition
        {
            get
            {
                return SelectionPanePosition.Top;
            }
        }

        /// <summary>
        /// Gets the <see cref="Control"/> that is to be added into the FFI.
        /// </summary>
        public Control Control
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether FFI menu main and context menu options should be limited
        /// to basic non-custom options. The main database menu and custom file handlers context
        /// menu options will not be shown.
        /// </summary>
        /// <value><see langword="true"/> to limit menu options to basic options only; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool BasicMenuOptionsOnly
        {
            get
            {
                return true;
            }
        }

        #endregion IFFIFileSelectionPane

        #region IFFIDataManager

        /// <summary>
        /// Gets if there is any changes that need to be applied. 
        /// </summary>
        public virtual bool Dirty
        {
            get
            {
                try
                {
                    return ( !string.IsNullOrWhiteSpace(GetSelectedRecordId()) &&
                        SelectedRecordId != GetSelectedRecordId() );
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38200");
                }
            }
        }

        /// <summary>
        /// Gets a description of changes that should be displayed to the user in a prompt when
        /// applying changes. If <see langword="null"/>, no prompt will be displayed when applying
        /// changed.
        /// </summary>
        public virtual string ApplyPrompt
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a description of changes that should be displayed to the user in a prompt when
        /// the user is canceling changes. If <see langword="null"/>, no prompt will be displayed
        /// when canceling except if the FFI is closed via the form's cancel button (red X).
        /// </summary>
        public virtual string CancelPrompt
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Applies all uncommitted values specified via SetValue.
        /// </summary>
        /// <returns><see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        public virtual bool Apply()
        {
            try
            {
                // On OK, set the SelectedRecordId property for the caller.
                string newSelectedRecordId = GetSelectedRecordId();

                if (string.IsNullOrWhiteSpace(newSelectedRecordId))
                {
                    UtilityMethods.ShowMessageBox("No record has been selected.",
                        "No record selected", true);
                    return false;
                }

                SelectedRecordId = newSelectedRecordId;

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38195");
            }
        }

        /// <summary>
        /// Cancels all uncommitted data changes specified via SetValue.
        /// </summary>
        public virtual void Cancel()
        {
            try
            {
                ResetSelection();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38196");
            }
        }

        #endregion IFFIDataManager

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // https://extract.atlassian.net/browse/ISSUE-14421
                // Style updates are ignored if done before the form is show, so this must be done
                // here rather than UpdateRecordSelectionGrid();
                if (_rowsPendingMatchedRecordStyle.Any())
                {
                    foreach (var cell in _rowsPendingMatchedRecordStyle
                        .Select(rowIndex => _recordsDataGridView.Rows[rowIndex])
                        .SelectMany(row => row.Cells.OfType<DataGridViewCell>()))
                    {
                        cell.Style = _matchedRecordCellStyle;
                    }

                    _rowsPendingMatchedRecordStyle.Clear();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44819");
            }
        }

        #endregion Overrides

        #region Internal Members

        /// <summary>
        /// The <see cref="DocumentDataRecord"/> that is used to retrieve and cache record information for
        /// the currently selected <see cref="DataEntryTableRow"/>.
        /// </summary>
        internal virtual DocumentDataRecord RowData
        {
            get;
            set;
        }

        #endregion Internal Members

        #region Event Handlers


        /// <summary>
        /// Handles the <see cref="DataGridView.SelectionChanged"/>event of the
        /// <see cref="_recordsDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRecordsDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Refresh to display the that have been filed against the newly selected record.
                OnRefreshRequired();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38146");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.DataBindingComplete"/> event of the
        /// <see cref="_recordsDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewBindingCompleteEventArgs"/> instance
        /// containing the event data.</param>
        void HandleRecordsDataGridView_DataBindingComplete(object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                // Upon completion of binding, selection will automatically be set to the first row
                // of the grid unless an alternate selection is applied here. Set the initial
                // selection to SelectedRecordId (if specified), otherwise, don't start with any
                // selection.
                ResetSelection();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38209");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellDoubleClick"/> event of the <see cref="_recordsDataGridView"/>
        /// by executing the <see cref="AcceptFunction"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        void HandleRecordsDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Don't treat double-clicks in the row header as a row selection.
                if (e.RowIndex >= 0)
                {
                    AcceptFunction?.Invoke(sender, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41671");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the record ID of the currently selected row in
        /// <see cref="_recordsDataGridView"/>.
        /// </summary>
        /// <returns>The record ID of the currently selected row.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual string GetSelectedRecordId()
        {
            if (_recordsDataGridView.SelectedRows.Count != 1)
            {
                return null;
            }

            return _recordsDataGridView.SelectedRows
                .OfType<DataGridViewRow>()
                .Single()
                .Cells[0].Value.ToString();
        }

        /// <summary>
        /// Resets the record selection to <see cref="SelectedRecordId"/>.
        /// </summary>
        protected virtual void ResetSelection()
        {
            _recordsDataGridView.ClearSelection();

            string recordIdToSelect = SelectedRecordId;

            // If there is not already a selected record, see if there is an record that matches any
            // AutoSelectionFilter and AutoSelectionRecord criteria
            if (string.IsNullOrWhiteSpace(recordIdToSelect) &&
                !string.IsNullOrWhiteSpace(AutoSelectionFilter))
            {
                using (DataTable selectionTable = RowData.UnmappedMatchingRecords.ToTable())
                using (DataView selectionView = new DataView(selectionTable))
                {
                    selectionView.RowFilter = AutoSelectionFilter;
                    selectionView.Sort = AutoSelectionRecord;

                    if (selectionView.Count > 0)
                    {
                        if (selectionView.Count == 1 ||
                            !string.IsNullOrWhiteSpace(AutoSelectionRecord))
                        {
                            recordIdToSelect = (string)(selectionView[0].Row.ItemArray[0]);
                        }
                    }
                }
            }

            // Select the row with recordNumberToSelect (if any).
            _recordsDataGridView.CurrentCell =
                _recordsDataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Select(row => row.Cells[0])
                    .Where(cell => !string.IsNullOrEmpty(recordIdToSelect) &&
                        recordIdToSelect.Equals(cell.Value.ToString(), StringComparison.Ordinal))
                    .SingleOrDefault();
            if (_recordsDataGridView.CurrentCell != null)
            {
                _recordsDataGridView.CurrentCell.OwningRow.Selected = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="RefreshRequired"/> event.
        /// </summary>
        protected virtual void OnRefreshRequired()
        {
            if (RefreshRequired != null)
            {
                RefreshRequired(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
