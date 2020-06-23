using DevExpress.XtraEditors;
using Extract.Imaging;
using Extract.ReportingDevExpress.Properties;
using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Extract.ReportingDevExpress
{
    /// <summary>
    /// Form for listing all available reports and allowing the user to choose which one
    /// to open.
    /// </summary>
    public partial class OpenReportForm : XtraForm
    {
        #region Constants

        /// <summary>
        /// The extension for the preview thumbnail images.
        /// </summary>
        private static readonly string _PREVIEW_IMAGE_EXTENSION = ".jpg";

        /// <summary>
        /// The index of the standard report tab
        /// </summary>
        private const int _STANDARD_REPORT_TAB_INDEX = 0;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The report file that was selected.
        /// </summary>
        private string _reportFileName;

        /// <summary>
        /// Whether the selected report is a standard report.
        /// </summary>
        private bool _standardReport;

        /// <summary>
        /// The current image in the preview window.
        /// </summary>
        private string _currentPreviewImage;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="OpenReportForm"/> class.
        /// </summary>
        public OpenReportForm()
        {
            InitializeComponent();

            // Fill in the list boxes
            PopulateListBoxes();

            // Update the button states
            UpdateButtonStates();
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ListBox.SelectedIndexChanged"/> event for the
        /// standard report list.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleStandardReportSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Check for a selection
                if (_standardReportList.SelectedIndex != -1)
                {
                    // Get the base report information
                    string baseReport = GetReportBase(_standardReportList.Text, true);

                    // Get the preview image name and load it in the preview pane
                    string reportPreviewImageName = baseReport + _PREVIEW_IMAGE_EXTENSION;
                    UpdatePreviewImage(reportPreviewImageName);

                    // Update the report file name value and set standard report to true
                    _reportFileName = baseReport + ".repx";
                    _standardReport = true;
                }
                else
                {
                    // No selection, clear the preview and report file name
                    UpdatePreviewImage(null);
                    _reportFileName = "";
                    _standardReport = false;
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23735", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ListBox.SelectedIndexChanged"/> event for the
        /// saved report list.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleSavedReportSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Check for a selection
                if (_savedReportList.SelectedIndex != -1)
                {
                    // Get the base report information
                    string baseReport = GetReportBase(_savedReportList.Text, false);

                    // Get the preview image name and load it in the preview pane
                    string reportPreviewImageName = baseReport + _PREVIEW_IMAGE_EXTENSION;
                    UpdatePreviewImage(reportPreviewImageName);

                    // Update the report file name value and set standard report to false
                    _reportFileName = baseReport + ".repx";
                    _standardReport = false;
                }
                else
                {
                    // No selection, clear the preview and report file name
                    UpdatePreviewImage(null);
                    _reportFileName = "";
                    _standardReport = false;
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23736", ex);
            }
        }

        /// <summary>
        /// Handles double click on the report lists.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleReportDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                // Check for left mouse button
                if (e.Button == MouseButtons.Left)
                {
                    // Get the current list box control
                    ListBox listBox = GetCurrentListBox();

                    // Get the index of the item at the mouse down location
                    int index = listBox.IndexFromPoint(e.Location);

                    // Ensure the location corresponds to an item and the report file is
                    // not empty
                    if (index != ListBox.NoMatches && !string.IsNullOrEmpty(_reportFileName))
                    {
                        this.DialogResult = DialogResult.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25373", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="TabControl.Selected"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleTabPageChanged(object sender, TabControlEventArgs e)
        {
            try
            {
                // Update the preview image
                UpdatePreviewImage(null);

                bool savedReportTab = e.TabPageIndex != _STANDARD_REPORT_TAB_INDEX;

                // Clear the selection in the list that is no longer showing
                if (savedReportTab)
                {
                    _standardReportList.ClearSelected();
                }
                else
                {
                    _savedReportList.ClearSelected();
                }

                // Get the visible list
                ListBox listBox = savedReportTab ? _savedReportList : _standardReportList;

                // If there is at least 1 item in the list, select the first one
                if (listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                // Update the delete button state
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23737", ex);
                ee.AddDebugData("Tab Control Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleDeleteReportClicked(object sender, EventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI23738", "No report selected!",
                    _savedReportList.SelectedIndex != -1);

                string reportName = _savedReportList.Text;

                // Show confirmation dialog
                if (MessageBox.Show("Do you want to permanently delete " + reportName + "?",
                    "Delete Report", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                     MessageBoxDefaultButton.Button2, 0) == DialogResult.No)
                {
                    return;
                }

                // Build main file name in preparation for deletion
                string selectedReport = GetReportBase(reportName, false);

                // Delete the report, preview, and (if it exists) the xml file
                bool deleteSuccessful =
                    FileSystemMethods.TryDeleteFile(selectedReport + ".repx");
                deleteSuccessful &= FileSystemMethods.TryDeleteFile(selectedReport +
                    _PREVIEW_IMAGE_EXTENSION);
                if (File.Exists(selectedReport + ".xml"))
                {
                    deleteSuccessful &= FileSystemMethods.TryDeleteFile(selectedReport + ".xml");
                }

                // If there was an exception while deleting the files display an exception
                // to the user (the details about the deletion error will be contained
                // in the exception log file)
                if (!deleteSuccessful)
                {
                    ExtractException ee = new ExtractException("ELI23739",
                        "Failed deleting report files!");
                    ee.AddDebugData("Base Report Name", selectedReport, false);
                    throw ee;
                }

                // Remove the selected report from the list and clear the selection
                _savedReportList.Items.RemoveAt(_savedReportList.SelectedIndex);
                _savedReportList.ClearSelected();

                // Select the first item in the list after deletion
                if (_savedReportList.Items.Count > 0)
                {
                    _savedReportList.SelectedIndex = 0;
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23740", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Restore form to previous settings if the UsePersistedSettings flag is set
                if (Settings.Default.OpenReportUsePersistedSettings)
                {
                    // Need to set the start position value to manual
                    this.StartPosition = FormStartPosition.Manual;

                    // Get the window state
                    this.WindowState = Settings.Default.OpenReportState;

                    // Get the stored size and location
                    this.Size = Settings.Default.OpenReportSize;
                    this.Location = Settings.Default.OpenReportLocation;

                    // Adjust the splitter distance
                    _topPanelSplitContainer.SplitterDistance =
                        Settings.Default.OpenReportSplitterDistance;
                }

                base.OnLoad(e);

                // Ensure the standard report list is displayed first
                _tabReportsList.SelectedIndex = _STANDARD_REPORT_TAB_INDEX;

                // Select the first item in the standard report list
                if (_standardReportList.Items.Count > 0)
                {
                    _standardReportList.SelectedIndex = 0;
                }

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23741", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                base.OnClosing(e);

                if (this.DialogResult != DialogResult.OK)
                {
                    _reportFileName = "";
                }
                else
                {
                    // If the user selected OK, but there is no selection, prompt them
                    // and cancel the closing event.
                    if (string.IsNullOrEmpty(_reportFileName))
                    {
                        MessageBox.Show("No report selected. Please select a report or cancel",
                            "No Report Selected", MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        e.Cancel = true;
                    }
                }

                // If the close was not cancelled then store the forms location before closing
                if (!e.Cancel)
                {
                    Settings.Default.OpenReportState = this.WindowState;

                    // Check for normal (not min or max) state
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        Settings.Default.OpenReportSize = this.Bounds.Size;
                        Settings.Default.OpenReportLocation = this.Bounds.Location;
                    }
                    else
                    {
                        // RestoreBounds only valid if in Min/Max window state
                        Settings.Default.OpenReportSize = this.RestoreBounds.Size;
                        Settings.Default.OpenReportLocation = this.RestoreBounds.Location;
                    }

                    Settings.Default.OpenReportSplitterDistance =
                        _topPanelSplitContainer.SplitterDistance;
                    Settings.Default.OpenReportUsePersistedSettings = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23742", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.ResizeEnd"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            try
            {
                base.OnResizeEnd(e);

                // Update the preview for the current preview image
                UpdatePreviewImage(_currentPreviewImage);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24710", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Resize"/> event for the picture box control.
        /// Maintains the centering for the no preview label
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandlePictureBoxResize(object sender, EventArgs e)
        {
            try
            {
                // Compute the new x and y position for the label
                int newX = (_reportPreview.Width - _labelNoPreview.Width) / 2;
                int newY = (_reportPreview.Height - _labelNoPreview.Height) / 2;

                _labelNoPreview.Location =
                    new Point(newX > 0 ? newX : 0, newY > 0 ? newY : 0);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24962", ex);
            }
        }

        #endregion Event Handlers

        #region Properties

        /// <summary>
        /// Gets the name of the report that was selected in the dialog.
        /// </summary>
        /// <returns>The name of the report that was selected.</returns>
        public string ReportFileName
        {
            get
            {
                return _reportFileName;
            }
        }

        /// <summary>
        /// Gets whether the selected report was a standard report.
        /// </summary>
        /// <returns><see langword="true"/> if the report was a standard
        /// report and <see langword="false"/> otherwise.</returns>
        public bool StandardReport
        {
            get
            {
                return _standardReport;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the preview image pane to display the specified image
        /// </summary>
        /// <param name="fileName">The image file to display.</param>
        private void UpdatePreviewImage(string fileName)
        {
            // Check for a valid file name
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                // Load the image
                using (Bitmap bitmap = new Bitmap(fileName))
                {
                    // Compute expansion/compression factor for preview thumbnail
                    // based on the image height and the preview pane height
                    // Note: It seems to work better to scale it a little bigger than
                    // the preview pane and allow the preview pane to scale it back
                    // [LRCAU #5113]
                    int percentage = (int)(
                        (((double)(_reportPreview.Height+20)) / ((double)bitmap.Height)) * 100.0);

                    // Generate a thumbnail of appropriate size
                    _reportPreview.Image = ImageMethods.GenerateThumbnail(bitmap, percentage);
                    _labelNoPreview.Visible = false;
                }
            }
            else
            {
                _reportPreview.Image = null;
                _labelNoPreview.Visible = true;
            }

            // Update the current image name
            _currentPreviewImage = fileName;
        }

        /// <summary>
        /// Updates the enabled/disabled state of the forms buttons.
        /// </summary>
        private void UpdateButtonStates()
        {
            // Enable the OK button if a report is selected
            _btnOk.Enabled = _standardReportList.SelectedIndex != -1
                || _savedReportList.SelectedIndex != -1;

            // Enable the delete button if:
            // 1. The saved reports tab is currently enabled
            // 2. There is a selected item in the list
            _btnDeleteReport.Enabled =
                _tabReportsList.SelectedTab.Text.Equals("Saved reports", StringComparison.Ordinal)
                && _savedReportList.SelectedIndex != -1;

            // Cancel button is always enabled
        }

        /// <summary>
        /// Gets the path to the base report name (complete path not including the file extension)
        /// </summary>
        /// <param name="reportName">The name of the report (from the list control). Must
        /// not be <see langword="null"/> or empty.</param>
        /// <param name="standard">If <see langword="true"/> then returns the path to the
        /// standard report directory if <see langword="false"/> then returns the path to
        /// the saved report directory.</param>
        /// <returns>The complete path to the report objects (does not include the file
        /// extension).</returns>
        private static string GetReportBase(string reportName, bool standard)
        {
            ExtractException.Assert("ELI23743", "Report name cannot be null or empty!",
                !string.IsNullOrEmpty(reportName));

             // Build main file name
            string selectedReport = Path.Combine(
                (standard ? ExtractReport.StandardReportFolder : ExtractReport.SavedReportFolder),
                reportName);

            return selectedReport;
        }

        /// <summary>
        /// Populates the two list controls with the reports contained in the standard
        /// and saved reports folders.
        /// </summary>
        private void PopulateListBoxes()
        {
            // Ensure the list boxes are empty
            _standardReportList.Items.Clear();
            _savedReportList.Items.Clear();

            // Get the paths to the standard and saved report directories
            string standardReportDir = ExtractReport.StandardReportFolder;
            string savedReportDir = ExtractReport.SavedReportFolder;

            // Get a list of files in the standard report directory that have a .repx extension
            foreach (string fileName in
                Directory.EnumerateFiles(standardReportDir, "*.repx", SearchOption.TopDirectoryOnly))
            {
                _standardReportList.Items.Add(Path.GetFileNameWithoutExtension(fileName));
            }

            // Get a list of files in the saved report directory that have a .repx extension
            foreach (string fileName in
                Directory.EnumerateFiles(savedReportDir, "*.repx", SearchOption.TopDirectoryOnly))
            {
                _savedReportList.Items.Add(Path.GetFileNameWithoutExtension(fileName));
            }
        }

        /// <summary>
        /// Gets the current list box from the tab control
        /// </summary>
        /// <returns>The currently visible ListBox from the tab control.</returns>
        private ListBox GetCurrentListBox()
        {
            ListBox listBox = _tabReportsList.SelectedIndex == _STANDARD_REPORT_TAB_INDEX ?
                _standardReportList : _savedReportList;
            return listBox;
        }

        #endregion Methods
    }
}