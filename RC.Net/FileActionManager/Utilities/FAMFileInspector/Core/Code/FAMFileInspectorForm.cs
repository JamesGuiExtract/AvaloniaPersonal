using ADODB;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_FILEPROCESSINGLib;

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

//        /// <summary>
//        /// The full path to the file that contains information about persisting the 
//        /// <see cref="FAMFileInspectorForm"/>.
//        /// </summary>
//        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
//            FileSystemMethods.ApplicationDataPath, "FAMFileInspector", "FAMFileInspector.xml");
//
//        /// <summary>
//        /// Name for the mutex used to serialize persistance of the control and form layout.
//        /// </summary>
//        static readonly string _FORM_PERSISTENCE_MUTEX_STRING =
//            "24440334-DE0C-46C1-920F-45D064A10DBF";

        /// <summary>
        /// The maximum number of files to display at once.
        /// </summary>
        static readonly int _MAX_FILES_TO_DISPLAY = 1000;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> whose files are being inspected.
        /// </summary>
        FileProcessingDB _fileProcessingDB = new FileProcessingDB();

        /// <summary>
        /// The <see cref="IFAMFileSelector"/> used to specify the domain of files being inspected.
        /// </summary>
        IFAMFileSelector _fileSelector = new FAMFileSelector();

        /// <summary>
        /// A <see cref="Task"/> that performs database query operations on a background thread.
        /// </summary>
        Task _queryTask;

        /// <summary>
        /// Allows the <see cref="_queryTask"/> to be canceled.
        /// </summary>
        volatile CancellationTokenSource _queryCanceler;

        /// <summary>
        /// Allows any overlay text on the image viewer to be canceled.
        /// </summary>
        volatile CancellationTokenSource _overlayTextCanceler;

        /// <summary>
        /// Indicates whether a search is currently active.
        /// </summary>
        volatile bool _searchIsActive = true;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

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

                InitializeComponent();

                // Settings PopuSize to 1 for the dockable window prevents it from popuping over
                // other windows when hovering while collapsed. (I found this behavior to be
                // confusing)
                _searchDockableWindow.PopupSize = 1;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35712");
            }
        }

        #endregion Constructors

        #region Properties

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
                return _fileProcessingDB.DatabaseServer;
            }

            set
            {
                _fileProcessingDB.DatabaseServer = value;
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
                return _fileProcessingDB.DatabaseName;
            }

            set
            {
                _fileProcessingDB.DatabaseName = value;
            }
        }

        #endregion Properties

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

                // Temporary: 
                DatabaseServer = "(local)";
                DatabaseName = "Demo_IDShield";

                UpdateFileSelectionSummary();

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
                if (_fileSelector.Configure(_fileProcessingDB, "Select the files to be searched",
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
        /// Handles the <see cref="DataGridView.CurrentCellChanged"/> event of the
        /// <see cref="_resultsDataGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleResultsDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            try 
	        {
		        if (_resultsDataGridView.CurrentRow != null)
                {
                    // Get the full path of the selected file.
                    string directory = (string)_resultsDataGridView.CurrentRow.Cells[3].Value;
                    string filename = (string)_resultsDataGridView.CurrentRow.Cells[0].Value;
                    string path = Path.Combine(directory, filename);

                    if (_overlayTextCanceler != null)
                    {
                        _overlayTextCanceler.Cancel();
                        _overlayTextCanceler = null;
                    }

                    if (File.Exists(path))
                    {
                        // ... and open it in the image viewer.
                        _imageViewer.OpenImage(path, false);
                    }
                    else
                    {
                        if (_imageViewer.IsImageAvailable)
                        {
                            _imageViewer.CloseImage();
                        }

                        _overlayTextCanceler = OverlayText.ShowText(_imageViewer, "File not found",
                            Font, Color.FromArgb(100, Color.Red), null, 0);
                    }
                }
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
                // Todo.
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
                _resultsDataGridView.Rows.Clear();
                _statusToolStripLabel.Text = "Ready";
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35720");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets or sets a value indicating whether a search is currenty active.
        /// </summary>
        /// <value><see langword="true"/> if a search is currenty active; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool SearchIsActive
        {
            get
            {
                return _searchIsActive;
            }

            set
            {
                _searchIsActive = value;

                // Update UI elements to reflect the current search state.
                _searchButton.Text = value ? "Cancel" : "Search";
                _selectFilesButton.Enabled = !value;
                _searchModifierComboBox.Enabled = !value;
                _searchTypeComboBox.Enabled = !value;
                _textSearchTermsDataGridView.Enabled = !value;
                _clearButton.Enabled = !value;
                if (value)
                {
                    _statusToolStripLabel.Text = "Searching...";
                }
                Update();
            }
        }

        /// <summary>
        /// Starts a new database query for files based on the <see cref="_fileSelector"/>'s current
        /// settings.
        /// </summary>
        void StartDatabaseQuery()
        {
            // Ensure tany 
            CancelDatabaseQuery();

            try
            {
                if (!_fileProcessingDB.IsConnected)
                {
                    _fileProcessingDB.ResetDBConnection();
                }

                // Update UI to reflect an ongoing search and clear the previous file list.
                SearchIsActive = true;
                _resultsDataGridView.Rows.Clear();

                string query = _fileSelector.BuildQuery(_fileProcessingDB,
                    "[FAMFile].[ID], [FAMFile].[FileName], [FAMFile].[Pages]",
                    " ORDER BY [FAMFile].[ID]");

                // Generate a background task which will perform the search.
                _queryCanceler = new CancellationTokenSource();
                _queryTask = new Task(() =>
                    RunDatabaseQuery(query, _queryCanceler.Token), _queryCanceler.Token);
                _queryTask.ContinueWith((task) =>
                    this.SafeBeginInvoke("ELI35721",
                        () => SearchIsActive = false),
                        TaskContinuationOptions.NotOnFaulted);
                _queryTask.ContinueWith((task) =>
                    this.SafeBeginInvoke("ELI35722",
                        () =>
                        {
                            SearchIsActive = false;
                            _statusToolStripLabel.Text = "Search error";
                            task.Exception.ExtractDisplay("ELI35723");
                        }),
                        TaskContinuationOptions.OnlyOnFaulted);

                // Kick off the background search and return.
                _queryTask.Start();
            }
            catch (Exception ex)
            {
                // If there was and error starting the search, be sure to reset the UI to reflect
                // the fact that the search is not active.
                SearchIsActive = false;

                throw ex.AsExtract("ELI35724");
            }
        }

        /// <summary>
        /// Runs a database query on a background thread.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancelToken">The cancel token.</param>
        void RunDatabaseQuery(string query, CancellationToken cancelToken)
        {
            try
            {
                Recordset queryResults = _fileProcessingDB.GetResultsForQuery(query);

                int fileCount = 0;

                // If there are any query results, populate _resultsDataGridView.
                if (queryResults.RecordCount > 0)
                {
                    // Continue populating up to _MAX_FILES_TO_DISPLAY.
                    queryResults.MoveFirst();
                    while (fileCount < _MAX_FILES_TO_DISPLAY && !queryResults.EOF)
                    {
                        // Abort if the user cancelled.
                        cancelToken.ThrowIfCancellationRequested();

                        // Retrieve the fields necessary for the results table.
                        string filename = (string)queryResults.Fields[1].Value;
                        string path = Path.GetDirectoryName(filename);
                        filename = Path.GetFileName(filename);
                        string pageCount = ((int)queryResults.Fields[2].Value)
                            .ToString(CultureInfo.CurrentCulture);

                        // Invoke the new row to be added on the UI thread.
                        this.SafeBeginInvoke("ELI35725", () =>
                            _resultsDataGridView.Rows.Add(filename, pageCount, "", path));

                        queryResults.MoveNext();
                        fileCount++;
                    }
                }

                // Update the status text with the number of files displayed.
                _statusToolStripLabel.Text =
                    string.Format(CultureInfo.CurrentCulture, "Displaying {0:D} files", fileCount);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35726");
            }
        }

        /// <summary>
        /// Cancels the any actively running database query.
        /// </summary>
        void CancelDatabaseQuery()
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

            SearchIsActive = false;
        }

        /// <summary>
        /// Updates the file selection summary label.
        /// </summary>
        void UpdateFileSelectionSummary()
        {
            string summaryText = _fileSelector.GetSummaryString();
            _selectFilesSummaryLabel.Text = "Currently searching ";
            _selectFilesSummaryLabel.Text +=
                summaryText.Substring(0, 1).ToLower(CultureInfo.CurrentCulture);
            _selectFilesSummaryLabel.Text += summaryText.Substring(1);
        }

        #endregion Private Members
    }
}
