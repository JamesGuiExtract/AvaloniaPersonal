using ADODB;
using Extract.Drawing;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// A <see cref="Form"/> that allows searching and inspection of files in a File Action Manager database
    /// based on database conditions, OCR content and data content.
    /// </summary>
    public partial class FAMFileInspectorForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMFileInspectorForm).ToString();

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="FAMFileInspectorForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.ApplicationDataPath, "FAMFileInspector", "FAMFileInspector.xml");

        /// <summary>
        /// Name for the mutex used to serialize persistance of the control and form layout.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_MUTEX_STRING =
            "24440334-DE0C-46C1-920F-45D064A10DBF";

        /// <summary>
        /// The maximum number of files to display at once.
        /// </summary>
        static readonly int _MAX_FILES_TO_DISPLAY = 1000;

        /// <summary>
        /// The column from <see cref="_fileListDataGridView"/> that represents the results of the
        /// most recent search.
        /// </summary>
        internal static int _FILE_LIST_MATCH_COLUMN_INDEX = 2;

        /// <summary>
        /// The color of highlights to show found search terms in documents.
        /// </summary>
        static readonly Color _HIGHLIGHT_COLOR = Color.LimeGreen;

        #endregion Constants

        #region Enums

        /// <summary>
        /// Represents the way in which the specified search terms will be used to determine
        /// matching files.
        /// </summary>
        enum SearchModifier
        {
            /// <summary>
            /// A file will be a match if any search term is found.
            /// </summary>
            Any = 0,

            /// <summary>
            /// A file will be a match if all search terms are found.
            /// </summary>
            All = 1,

            /// <summary>
            /// A file will be a match if none of the search terms are found.
            /// </summary>
            None = 2
        }

        /// <summary>
        /// Indicates which type of search is to be performed.
        /// </summary>
        enum SearchType
        {
            /// <summary>
            /// The OCR text of the files should be searched.
            /// </summary>
            Text = 0,

            /// <summary>
            /// The extracted <see cref="IAttribute"/> data of the files should be searched.
            /// </summary>
            Data = 1
        }

        #endregion Enums

        #region Delegates

        /// <summary>
        /// Implements a search operation that occurs on a background thread via
        /// <see cref="StartBackgroundOperation"/>.
        /// </summary>
        /// <typeparam name="T">The data type used to represent the search terms for the operation.
        /// </typeparam>
        /// <param name="searchTerms">The search terms for the operation.</param>
        /// <param name="searchModifier">The <see cref="SearchModifier"/> that indicate how the
        /// <see paramref="searchTerms"/> are to be used to determine matching files.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the operation should check
        /// after searching each file to see if the operation has been canceled.</param>
        delegate void SearchOperation<T>(T searchTerms, SearchModifier searchModifier,
            CancellationToken cancelToken);

        #endregion Delegates

        #region Fields

        /// <summary>
        /// Saves/restores window state info
        /// </summary>
        FormStateManager _formStateManager;

        /// <summary>
        /// The <see cref="IFAMFileSelector"/> used to specify the domain of files being inspected.
        /// </summary>
        IFAMFileSelector _fileSelector = new FAMFileSelector();

        /// <summary>
        /// The number of files currently selected by <see cref="_fileSelector"/>. Not all may be
        /// displayed.
        /// </summary>
        volatile int _fileSelectionCount;

        /// <summary>
        /// An <see cref="IAFUtility"/> instance used to query for <see cref="IAttribute"/>s from
        /// VOA (data) files.
        /// </summary>
        IAFUtility _afUtils = new AFUtility();

        /// <summary>
        /// A <see cref="Task"/> that performs database query operations on a background thread.
        /// </summary>
        volatile Task _queryTask;

        /// <summary>
        /// Allows the <see cref="_queryTask"/> to be canceled.
        /// </summary>
        volatile CancellationTokenSource _queryCanceler;

        /// <summary>
        /// Allows any overlay text on the image viewer to be canceled.
        /// </summary>
        volatile CancellationTokenSource _overlayTextCanceler;

        /// <summary>
        /// Indicates whether a background database query or file search is active.
        /// </summary>
        volatile bool _operationIsActive = true;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="FAMFileInspectorForm"/> class.
        /// </summary>
        // FXCop believes static members are being initialized here.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static FAMFileInspectorForm()
        {
            try
            {
                SearchModifier.Any.SetReadableValue("any");
                SearchModifier.All.SetReadableValue("all");
                SearchModifier.None.SetReadableValue("none");

                SearchType.Text.SetReadableValue("words");
                SearchType.Data.SetReadableValue("extracted data");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35737");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMFileInspectorForm"/> class.
        /// </summary>
        public FAMFileInspectorForm()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI35711", _OBJECT_NAME);

                // License SandDock before creating the form.
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                FileProcessingDB = new FileProcessingDB();

                InitializeComponent();

                if (!_inDesignMode)
                {
                    // Loads/save UI state properties
                    _formStateManager = new FormStateManager(this, _FORM_PERSISTENCE_FILE,
                        _FORM_PERSISTENCE_MUTEX_STRING, _sandDockManager, null);
                }

                _searchModifierComboBox.InitializeWithReadableEnum<SearchModifier>(false);
                _searchTypeComboBox.InitializeWithReadableEnum<SearchType>(false);

                // Settings PopuSize to 1 for the dockable window prevents it from popuping over
                // other windows when hovering while collapsed. (I found this behavior to be
                // confusing)
                _searchDockableWindow.PopupSize = 1;

                LayerObject.SelectionPen = ExtractPens.GetThickDashedPen(_HIGHLIGHT_COLOR);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35712");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FileProcessingDB"/> whose files are being inspected.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> whose files are being inspected.
        /// </value>
        [CLSCompliant(false)]
        public FileProcessingDB FileProcessingDB
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name database server being used.
        /// </summary>
        /// <value>
        /// The name of the database server being used.
        /// </value>
        public string DatabaseServer
        {
            get
            {
                return FileProcessingDB.DatabaseServer;
            }

            set
            {
                FileProcessingDB.DatabaseServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the database being inspected.
        /// </summary>
        /// <value>
        /// The name of the database being inspected.
        /// </value>
        public string DatabaseName
        {
            get
            {
                return FileProcessingDB.DatabaseName;
            }

            set
            {
                FileProcessingDB.DatabaseName = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Resets all changes to file selection back to the default (no conditions, top 1000 files).
        /// </summary>
        public void ResetFileSelectionSettings()
        {
            try
            {
                _fileSelector.Reset();
                _fileSelector.LimitToSubset(false, false, 1000);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35768");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

                UpdateFileSelectionSummary();

                // Initialize the search settings.
                _searchTypeComboBox.SelectEnumValue(SearchType.Text);
                ClearSearch();

                StartDatabaseQuery();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35713");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.
        /// </param>
        // _queryCanceler and _overlayTextCanceler are not disposed of per
        // http://stackoverflow.com/questions/6960520/when-to-dispose-cancellationtokensource
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_queryCanceler")]
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_overlayTextCanceler")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up managed objects
                if (_formStateManager != null)
                {
                    _formStateManager.Dispose();
                    _formStateManager = null;
                }

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_queryTask != null)
                {
                    _queryTask.Dispose();
                    _queryTask = null;
                }
            }

            // Clean up unmanaged objects

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="T:DockableWindow.Closing"/> event for all
        /// <see cref="DockableWindow"/>s on the form.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TD.SandDock.DockControlClosingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleDockWindow_Closing(object sender, DockControlClosingEventArgs e)
        {
            try
            {
                // In order to allow the close (X) button to be used to "close" the dockable window
                // but still have a tab available to re-open them, cancel the close and collapse the
                // pane instead.
                e.Cancel = true;
                e.DockControl.Collapsed = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35714");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:DockableWindow.AutoHidePopupOpened"/> event for all
        /// <see cref="DockableWindow"/>s on the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDockableWindow_AutoHidePopupOpened(object sender, EventArgs e)
        {
            try
            {
                var dockableWindow = (DockableWindow)sender;

                // If a collapsed window has been opened via mouse press, immediately un-collapse it
                // rather than allow it to temporarily popup over all other windows.
                // Ignore if the mouse isn't down (meaning SandDock timer related to hover is
                // triggering).
                if (dockableWindow.Collapsed &&
                    (Control.MouseButtons.HasFlag(System.Windows.Forms.MouseButtons.Left)))
                {
                    dockableWindow.Collapsed = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35715");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:SandDockManager.DockControlActivated"/> event of the
        /// <see cref="_sandDockManager"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TD.SandDock.DockControlEventArgs"/> instance containing
        /// the event data.</param>
        void HandleSandDockManager_DockControlActivated(object sender, DockControlEventArgs e)
        {
            try
            {
                // If a collapsed window has been activated, immediately un-collapse it rather than
                // allow it to temporarily popup over all other windows.
                if (e.DockControl.Collapsed)
                {
                    e.DockControl.Collapsed = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35716");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_selectFilesButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSelectFilesButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileSelector.Configure(FileProcessingDB, "Select the files to be listed",
                    "SELECT [Filename] FROM [FAMFile]"))
                {
                    UpdateFileSelectionSummary();

                    StartDatabaseQuery();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35717");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the
        /// <see cref="_searchTypeComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSearchTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Show the appropriate search term data grid view based on the selected search type.
                bool textSearch = _searchTypeComboBox.ToEnumValue<SearchType>() == SearchType.Text;
                _textSearchTermsDataGridView.Visible = textSearch;
                _dataSearchTermsDataGridView.Visible = !textSearch;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35738");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CurrentCellChanged"/> event of the
        /// <see cref="_fileListDataGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleResultsDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            try 
	        {
                DataGridViewRow row = _fileListDataGridView.CurrentRow;

                UpdateImageViewerDisplay(row);
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI35718");
	        }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_searchButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_searchTypeComboBox.ToEnumValue<SearchType>() == SearchType.Text)
                {
                    StartTextSearch();
                }
                else
                {
                    StartDataSearch();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35719");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_clearButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleClearButton_Click(object sender, EventArgs e)
        {
            try
            {
                ClearSearch();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35720");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.SortCompare"/> event of the
        /// <see cref="_fileListDataGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewSortCompareEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFileListDataGridView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            try
            {
                // If sorting based on the "Matches" column use custom sorting which is numerical
                // except for where "(No OCR)" or "(No Data)" is displayed.
                if (e.Column.Index == _FILE_LIST_MATCH_COLUMN_INDEX)
                {
                    e.SortResult = ((FAMFileData)e.CellValue1).CompareTo((FAMFileData)e.CellValue2);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35739");
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_showOnlyMatchesCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleShowOnlyMatchesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Update the visibility of each row based upon the search results and whether the
                // user has selected to see only matching files.
                foreach (DataGridViewRow row in _fileListDataGridView.Rows)
                {
                    FAMFileData fileData = row.GetFileData();
                    row.Visible = fileData.FileMatchesSearch || !_showOnlyMatchesCheckBox.Checked;
                }

                // After re-displaying all files from having been displaying only search results,
                // the last currently active row may still be selected though it is no longer the
                // current row. Clear the selection if this is the case.
                if (_fileListDataGridView.CurrentRow == null)
                {
                    _fileListDataGridView.ClearSelection();
                }

                UpdateImageViewerDisplay(_fileListDataGridView.CurrentRow);

                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35740");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_logoutToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleLogoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Hide the main form until the user connects to a database.
                Hide();
                ClearSearch();

                if (FileProcessingDB.ShowSelectDB("Select database", false, false))
                {
                    Show();
                    StartDatabaseQuery();
                }
                else
                {
                    // If the user chose to exit from the database selection prompt, exit.
                    Close();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35756");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_exitToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35753");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets or sets a value indicating whether a background database query or file search is
        /// currently active.
        /// </summary>
        /// <value><see langword="true"/> if a background database query or file search is currently
        /// active; otherwise, <see langword="false"/>.
        /// </value>
        bool OperationIsActive
        {
            get
            {
                return _operationIsActive;
            }

            set
            {
                _operationIsActive = value;

                // Update UI elements to reflect the current search state.
                _searchButton.Text = value ? "Cancel" : "Search";
                _selectFilesButton.Enabled = !value;
                _searchModifierComboBox.Enabled = !value;
                _searchTypeComboBox.Enabled = !value;
                _textSearchTermsDataGridView.Enabled = !value;
                _clearButton.Enabled = !value;
                
                UpdateStatusLabel();
                Update();
            }
        }

        /// <summary>
        /// Updates the status label text to reflect the current state of
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        void UpdateStatusLabel()
        {
            if (OperationIsActive)
            {
                _statusToolStripLabel.Text = "Searching...";
            }
            else if (_showOnlyMatchesCheckBox.Checked)
            {
                int resultCount = _fileListDataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Count(row => row.Visible);
                _statusToolStripLabel.Text = string.Format(CultureInfo.CurrentCulture,
                    "Showing {0:D} search results", resultCount);
            }
            else
            {
                if (_fileSelectionCount > _fileListDataGridView.Rows.Count)
                {
                    _statusToolStripLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "Showing {0:D} of {1:D} files", _fileListDataGridView.Rows.Count,
                        _fileSelectionCount);
                }
                else
                {
                    _statusToolStripLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "Showing {0:D} files", _fileListDataGridView.Rows.Count);
                }
            }
        }

        /// <summary>
        /// Clears the search settings as well as all matches indicated in
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        void ClearSearch()
        {
            _searchModifierComboBox.SelectEnumValue(SearchModifier.Any);
            _textSearchTermsDataGridView.Rows.Clear();
            _dataSearchTermsDataGridView.Rows.Clear();
            _showOnlyMatchesCheckBox.Checked = false;
            _showOnlyMatchesCheckBox.Enabled = false;

            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                row.GetFileData().ClearSearchResults();
            }
            _fileListDataGridView.Invalidate();

            UpdateStatusLabel();
        }

        /// <summary>
        /// Starts a new database query for files based on the <see cref="_fileSelector"/>'s current
        /// settings.
        /// </summary>
        void StartDatabaseQuery()
        {
            // Ensure any previous background operation is canceled first.
            CancelBackgroundOperation();

            if (!FileProcessingDB.IsConnected)
            {
                FileProcessingDB.ResetDBConnection();
            }

            _fileListDataGridView.Rows.Clear();

            string query = _fileSelector.BuildQuery(FileProcessingDB,
                "[FAMFile].[ID], [FAMFile].[FileName], [FAMFile].[Pages]",
                " ORDER BY [FAMFile].[ID]");

            // Run the query on a background thread so the UI remains responsive as rows are loaded.
            StartBackgroundOperation(() => RunDatabaseQuery(query, _queryCanceler.Token));
        }

        /// <summary>
        /// Runs a database query to build the file list on a background thread.
        /// </summary>
        /// <param name="query">The query used to generate the file list.</param>
        /// <param name="cancelToken">The <see cref="CancellationToken"/> that should be checked
        /// after adding each file to the list to ensure the operation hasn't been canceled.</param>
        void RunDatabaseQuery(string query, CancellationToken cancelToken)
        {
            try
            {
                Recordset queryResults = FileProcessingDB.GetResultsForQuery(query);

                _fileSelectionCount = 0;

                // If there are any query results, populate _resultsDataGridView.
                if (!queryResults.EOF)
                {
                    queryResults.MoveFirst();
                    while (!queryResults.EOF)
                    {
                        // Abort if the user cancelled.
                        cancelToken.ThrowIfCancellationRequested();

                        // Populate up to _MAX_FILES_TO_DISPLAY in the file list, but iterate all
                        // results to obtain the overall number of files selected.
                        if (_fileSelectionCount < _MAX_FILES_TO_DISPLAY)
                        {
                            // Retrieve the fields necessary for the results table.
                            string fileName = (string)queryResults.Fields[1].Value;
                            var fileData = new FAMFileData(fileName);

                            string directory = Path.GetDirectoryName(fileName);
                            fileName = Path.GetFileName(fileName);
                            int pageCount =
                                (int)queryResults.Fields[_FILE_LIST_MATCH_COLUMN_INDEX].Value;

                            // Invoke the new row to be added on the UI thread.
                            this.SafeBeginInvoke("ELI35725", () =>
                            {
                                _fileListDataGridView.Rows.Add(fileName, pageCount, fileData,
                                    directory);
                            });
                        }

                        queryResults.MoveNext();
                        _fileSelectionCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35726");
            }
        }

        /// <summary>
        /// Starts a text search.
        /// </summary>
        void StartTextSearch()
        {
            var searchTerms = _textSearchTermsDataGridView.Rows
                .OfType<DataGridViewRow>()
                .Select(row => (string)row.Cells[0].Value)
                .Where(term => !string.IsNullOrWhiteSpace(term));
            ExtractException.Assert("ELI35741", "No search terms specified", searchTerms.Count() > 0);

            StartSearch(RunTextSearch, searchTerms);
        }

        /// <summary>
        /// Runs a text search on a background thread.
        /// </summary>
        /// <param name="searchTerms">The terms to search for in each file's OCR data.</param>
        /// <param name="searchModifier">The <see cref="SearchModifier"/> that indicate how the
        /// <see paramref="searchTerms"/> are to be used to determine matching files.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the operation should check
        /// after searching each file to see if the operation has been canceled.</param>
        void RunTextSearch(IEnumerable<string> searchTerms, SearchModifier searchModifier,
            CancellationToken cancelToken)
        {
            // Create a compiled DotNetRegexParser for every search term using an escaped version of
            // the search term to allow any term to be search as a regular expression.
            var regexParsers = new List<DotNetRegexParser>();
            foreach (string regex in searchTerms.Select(term => Regex.Escape(term)))
            {
                DotNetRegexParser regexParser = new DotNetRegexParser();
                regexParser.Pattern = regex;
                regexParser.RegexOptions |= RegexOptions.Compiled;
                regexParsers.Add(regexParser);
            }

            // Search each file in the file list.
            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                // Abort if the user cancelled.
                cancelToken.ThrowIfCancellationRequested();

                // Obtain the OCR text for the file.
                FAMFileData rowData = row.GetFileData();
                rowData.ShowTextResults = true;
                SpatialString ocrText = rowData.OcrText;
                if (ocrText != null)
                {
                    string fileText = ocrText.String;
                    List<Match> allMatches = new List<Match>();

                    // Initialize FileMatchesSearch depending on whether we are looking for any term.
                    rowData.FileMatchesSearch = (searchModifier != SearchModifier.Any);

                    // Search the OCR text with the parser for each search term.
                    foreach (DotNetRegexParser parser in regexParsers)
                    {
                        var matches = parser.Regex.Matches(fileText).OfType<Match>();

                        // Update FileMatchesSearch as appropriate given the results
                        switch (searchModifier)
                        {
                            case SearchModifier.Any: rowData.FileMatchesSearch |= matches.Any();
                                break;
                            case SearchModifier.All: rowData.FileMatchesSearch &= matches.Any();
                                break;
                            case SearchModifier.None: rowData.FileMatchesSearch &= !matches.Any();
                                break;
                        }

                        // Compile all the matches regardless of whether the file is a match
                        // overall.
                        allMatches.AddRange(matches);
                    }

                    rowData.TextMatches = allMatches;
                }

                // Use a separate variable in the below, invoked call to update the row in the UI,
                // because by the time that update happens, row may have been re-assigned to another
                // row.
                var rowToUpdate = row;

                // Update the row in the UI.
                this.SafeBeginInvoke("ELI35742", () =>
                {
                    rowToUpdate.Visible =
                        rowData.FileMatchesSearch || !_showOnlyMatchesCheckBox.Checked;
                    _fileListDataGridView.InvalidateRow(rowToUpdate.Index);
                });
            }
        }

        /// <summary>
        /// Starts a search of VOA file <see cref="IAttribute"/> data.
        /// </summary>
        void StartDataSearch()
        {
            // Compile the search terms into a dictionary where the attribute query is the key and
            // the value to search for in the attribute is the value.
            var searchTerms = new Dictionary<string,string>();
            foreach (KeyValuePair<string, string> pair in
                _dataSearchTermsDataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Select(row => new KeyValuePair<string, string>((string)row.Cells[0].Value, (string)row.Cells[1].Value))
                    .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) &&
                        !string.IsNullOrWhiteSpace(pair.Value)))
            {
                searchTerms.Add(pair.Key, pair.Value);
            }
            ExtractException.Assert("ELI35743", "No search terms specified", searchTerms.Count() > 0);

            StartSearch(RunDataSearch, searchTerms);
        }

        /// <summary>
        /// Runs a data search.
        /// </summary>
        /// <param name="searchTerms">The search terms as a <see cref="T:Dictionary(string,string)"/>
        /// where the attribute query is the key and the value to search for in the
        /// <see cref="IAttribute"/> is the value.</param>
        /// <param name="searchModifier">The <see cref="SearchModifier"/> that indicate how the
        /// <see paramref="searchTerms"/> are to be used to determine matching files.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the operation should check
        /// after searching each file to see if the operation has been canceled.</param>
        void RunDataSearch(Dictionary<string,string> searchTerms, SearchModifier searchModifier,
            CancellationToken cancelToken)
        {
            // Create a dictionary for every attribute query where the value is a compiled
            // DotNetRegexParser for the corresponding search term using an escaped version of the
            // search term to allow any term to be search as a regular expression.
            var regexParsers = new Dictionary<string, DotNetRegexParser>();
            foreach (KeyValuePair<string, string> searchTerm in searchTerms)
            {
                DotNetRegexParser regexParser = new DotNetRegexParser();
                regexParser.Pattern = Regex.Escape(searchTerm.Value);
                regexParser.RegexOptions |= RegexOptions.Compiled;
                regexParsers.Add(searchTerm.Key, regexParser);
            }

            // Search each file in the file list.
            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                // Abort if the user cancelled.
                cancelToken.ThrowIfCancellationRequested();

                // Obtain the VOA data for the file.
                FAMFileData rowData = row.GetFileData();
                rowData.ShowTextResults = false;
                IUnknownVector attributes = rowData.Attributes;
                if (attributes != null)
                {
                    var allMatches = new List<ThreadSafeSpatialString>();

                    // Initialize FileMatchesSearch depending on whether we are looking for any term.
                    rowData.FileMatchesSearch = (searchModifier != SearchModifier.Any);

                    // Search the specified attributes with the parser for each search term.
                    foreach (KeyValuePair<string, DotNetRegexParser> parser in regexParsers)
                    {
                        IEnumerable<ThreadSafeSpatialString> matches =
                            _afUtils.QueryAttributes(attributes, parser.Key, false)
                            .ToIEnumerable<IAttribute>()
                            .Select(attribute => attribute.Value)
                            .SelectMany(value => parser.Value.Regex.Matches(value.String)
                                .OfType<Match>()
                                .Select(match => new ThreadSafeSpatialString(this,
                                    value.GetSubString(match.Index, match.Index + match.Length - 1))));

                        // Update FileMatchesSearch as appropriate given the results
                        switch (searchModifier)
                        {
                            case SearchModifier.Any: rowData.FileMatchesSearch |= matches.Any();
                                break;
                            case SearchModifier.All: rowData.FileMatchesSearch &= matches.Any();
                                break;
                            case SearchModifier.None: rowData.FileMatchesSearch &= !matches.Any();
                                break;
                        }

                        // Compile all the matches regardless of whether the file is a match
                        // overall.
                        allMatches.AddRange(matches);
                    }

                    rowData.DataMatches = allMatches;
                }

                // Use a separate variable in the below, invoked call to update the row in the UI,
                // because by the time that update happens, row may have been re-assigned to another
                // row.
                var rowToUpdate = row;

                // Update the row in the UI.
                this.SafeBeginInvoke("ELI35744", () =>
                {
                    rowToUpdate.Visible =
                        rowData.FileMatchesSearch || !_showOnlyMatchesCheckBox.Checked;
                    _fileListDataGridView.InvalidateRow(rowToUpdate.Index);
                });
            }
        }

        /// <summary>
        /// Starts the specified <see paramref="searchOperation"/>.
        /// </summary>
        /// <typeparam name="T">The data type used to represent the search terms for the operation.
        /// </typeparam>
        /// <param name="searchOperation">The <see cref="SearchOperation{T}"/> to be performed.
        /// </param>
        /// <param name="searchTerms">The search terms for the operation.</param>
        void StartSearch<T>(SearchOperation<T> searchOperation, T searchTerms)
        {
            // Ensure any previous background operation is canceled first.
            CancelBackgroundOperation();

            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                row.GetFileData().ClearSearchResults();
                // All rows should be hidden until they are determined to be a match.
                row.Visible = false;
            }
            _fileListDataGridView.Invalidate();

            // Start by showing only the matching files.
            _showOnlyMatchesCheckBox.Enabled = true;
            _showOnlyMatchesCheckBox.Checked = true;

            var searchModifier = _searchModifierComboBox.ToEnumValue<SearchModifier>();

            // Run the search on a background thread so that the UI remains responsive while the
            // operation is running.
            StartBackgroundOperation(() =>
                searchOperation(searchTerms, searchModifier, _queryCanceler.Token));
        }

        /// <summary>
        /// Starts a background query or search operation.
        /// </summary>
        /// <param name="operation">The <see cref="Action"/> to be performed in the background.
        /// </param>
        void StartBackgroundOperation(Action operation)
        {
            try
            {
                // Update UI to reflect an ongoing operation.
                OperationIsActive = true;

                // Generate a background task which will perform the search.
                _queryCanceler = new CancellationTokenSource();
                _queryTask = new Task(() => operation(), _queryCanceler.Token);
                _queryTask.ContinueWith((task) =>
                    this.SafeBeginInvoke("ELI35721",
                        () => OperationIsActive = false),
                        TaskContinuationOptions.NotOnFaulted);
                _queryTask.ContinueWith((task) =>
                    this.SafeBeginInvoke("ELI35722",
                        () =>
                        {
                            OperationIsActive = false;

                            foreach (Exception ex in task.Exception.InnerExceptions)
                            {
                                ex.ExtractDisplay("ELI35723");
                            }
                        }),
                        TaskContinuationOptions.OnlyOnFaulted);

                // Kick off the background search and return.
                _queryTask.Start();
            }
            catch (Exception ex)
            {
                // If there was and error starting the query, be sure to reset the UI to reflect
                // the fact that the query is not active.
                OperationIsActive = false;

                throw ex.AsExtract("ELI35745");
            }
        }

        /// <summary>
        /// Cancels the any actively running background operation.
        /// </summary>
        void CancelBackgroundOperation()
        {
            if (_queryTask != null)
            {
                if (_queryTask.Wait(0))
                {
                    // The task has already ended; dispose of it.
                    _queryTask.Dispose();
                    _queryTask = null;
                }
                else
                {
                    // The task is still running; since we are going to cancel it, update the
                    // continue with so that it disposes of the task without displaying any
                    // exceptions about being cancelled.
                    _queryTask.ContinueWith((task) =>
                    {
                        task.Dispose();
                        _queryTask = null;
                    });

                    // Cancel _queryTask and wait for it to finish.
                    _queryCanceler.Cancel();
                    try
                    {
                        _queryTask.Wait();
                    }
                    catch { }  // Ignore any exceptions; we don't care about this task anymore.
                }
            }

            OperationIsActive = false;
        }

        /// <summary>
        /// Updates the image displayed in the <see cref="_imageViewer"/> based on the
        /// <see pararef="currentRow"/>.
        /// </summary>
        /// <param name="currentRow">The <see cref="DataGridViewRow"/> that is currently active.
        /// </param>
        void UpdateImageViewerDisplay(DataGridViewRow currentRow)
        {
            if (_overlayTextCanceler != null)
            {
                _overlayTextCanceler.Cancel();
                _overlayTextCanceler = null;
            }

            FAMFileData fileData = (currentRow == null) ? null : currentRow.GetFileData();

            if (fileData != null && File.Exists(fileData.FileName))
            {
                // ... and open it in the image viewer.
                _imageViewer.OpenImage(fileData.FileName, false);

                // Display highlights for all search terms found in the selected file.
                ShowMatchHighlights(fileData);
            }
            else // either there no current row or no file available, close any open image.
            {
                if (_imageViewer.IsImageAvailable)
                {
                    _imageViewer.CloseImage();
                }

                // If there is a current row, display "File not found"
                if (currentRow != null)
                {
                    _overlayTextCanceler = OverlayText.ShowText(_imageViewer, "File not found",
                        Font, Color.FromArgb(100, Color.Red), null, 0);
                }
            }
        }

        /// <summary>
        /// Displays highlights in the <see cref="_imageViewer"/> representing all search terms
        /// found in the corresponding file.
        /// </summary>
        /// <param name="fileData">The <see cref="FAMFileData"/> for which highlights are to be
        /// displayed.</param>
        void ShowMatchHighlights(FAMFileData fileData)
        {
            if (fileData.ShowTextResults.HasValue)
            {
                // If showing the results of a text search
                if (fileData.ShowTextResults.Value)
                {
                    IEnumerable<Match> matches = fileData.TextMatches;
                    if (matches != null)
                    {
                        // Get the OCR text for the file; to save memory, TextMatches does not store
                        // the SpatialString representing the match. The SpatialStrings are created
                        // here using the each match against the OCR text.
                        SpatialString ocrText = fileData.OcrText;
                        if (ocrText != null)
                        {
                            foreach (Match match in matches)
                            {
                                // Create a SpatialString representing the match.
                                SpatialString resultValue =
                                    ocrText.GetSubString(match.Index, match.Index + match.Length - 1);
                                foreach (CompositeHighlightLayerObject highlight in
                                    _imageViewer.CreateHighlights(resultValue, _HIGHLIGHT_COLOR))
                                {
                                    _imageViewer.LayerObjects.Add(highlight);
                                }
                            }
                        }
                    }
                }
                else // If showing the results of a data search
                {
                    IEnumerable<ThreadSafeSpatialString> matches = fileData.DataMatches;
                    if (matches != null)
                    {
                        foreach (ThreadSafeSpatialString match in matches)
                        {
                            foreach (CompositeHighlightLayerObject highlight in
                                _imageViewer.CreateHighlights(match.SpatialString, Color.LimeGreen))
                            {
                                _imageViewer.LayerObjects.Add(highlight);
                            }
                        }
                    }
                }

                _imageViewer.Invalidate();
            }
        }

        /// <summary>
        /// Updates the file selection summary label.
        /// </summary>
        void UpdateFileSelectionSummary()
        {
            string summaryText = _fileSelector.GetSummaryString();
            _selectFilesSummaryLabel.Text = "Listing ";
            _selectFilesSummaryLabel.Text +=
                summaryText.Substring(0, 1).ToLower(CultureInfo.CurrentCulture);
            _selectFilesSummaryLabel.Text += summaryText.Substring(1);
        }

        #endregion Private Members
    }
}
