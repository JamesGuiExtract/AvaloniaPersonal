using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Extract;
using Extract.Licensing;
using Extract.ReportViewer.Properties;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Extract.ReportViewer
{
    /// <summary>
    /// A form for displaying a crystal report document.
    /// </summary>
    internal partial class ReportViewerForm : Form
    {
        #region Constants

        /// <summary>
        /// The filter string containing the file formats that RunReport supports exporting.
        /// </summary>
        static readonly string _EXPORT_FORMAT_FILTER =
            "PDF files (*.pdf)|*.pdf||";

        /// <summary>
        /// The caption displayed in the title bar of the report viewer.
        /// </summary>
        static readonly string _FORM_CAPTION = "Report Viewer";

        /// <summary>
        /// The extension for the preview images.
        /// </summary>
        static readonly string _PREVIEW_EXTENSION = ".jpg";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(ReportViewerForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path to the report that will be displayed.
        /// </summary>
        string _reportFileName;

        /// <summary>
        /// The database server that the report should attach to.
        /// </summary>
        readonly string _serverName;

        /// <summary>
        /// The database name that the report should attach to.
        /// </summary>
        readonly string _databaseName;

        /// <summary>
        /// The <see cref="ExtractReport"/> object that contains the report to
        /// be displayed.
        /// </summary>
        ExtractReport _report;

        /// <summary>
        /// List that will contain the names of all the standard reports.  This is used
        /// in the report saving but is only instantiated when it is actually used.
        /// </summary>
        private List<string> _standardReports;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="ReportViewerForm"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportViewerForm"/> class.
        /// </summary>
        /// <param name="serverName">The database server to attach the report to.</param>
        /// <param name="databaseName">The database name to attach the report to.</param>
        public ReportViewerForm(string serverName, string databaseName)
            : this(null, serverName, databaseName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportViewerForm"/> class.
        /// </summary>
        /// <param name="report">The <see cref="ExtractReport"/> report to
        /// attach to.</param>
        /// <param name="serverName">The database server to attach the report to.</param>
        /// <param name="databaseName">The database name to attach the report to.</param>
        public ReportViewerForm(ExtractReport report, string serverName, string databaseName)
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI23504", _OBJECT_NAME);

                InitializeComponent();

                _report = report;

                // Store the report and database information
                _reportFileName = report != null ? report.FileName : "";
                _serverName = serverName;
                _databaseName = databaseName;

                // Enable/disable the save template menu item depending on whether the
                // report is a standard report or not
                _saveReportTemplateToolStripMenuItem.Enabled =
                    !string.IsNullOrEmpty(_reportFileName)
                     && ExtractReport.StandardReportFolder.Equals(
                    Path.GetDirectoryName(_reportFileName), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23505", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="Form.Shown"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnShown(EventArgs e)
        {
            try
            {
                base.OnShown(e);

                if (_report != null)
                {
                    // Load the new report
                    AttachReportToReportViewer();
                }
                else
                {
                    // Show the open report dialog (click the menu item)
                    _openReportToolStripMenuItem.PerformClick();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23506", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
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
                // If UsePersistedSettings flag is set, load the saved settings
                if (Settings.Default.ReportViewerUsePersistedSettings)
                {
                    // Need to set the start position value to manual
                    this.StartPosition = FormStartPosition.Manual;

                    // Get the window state
                    this.WindowState = Settings.Default.ReportViewerState;

                    // Get the stored size and location
                    this.Size = Settings.Default.ReportViewerSize;
                    this.Location = Settings.Default.ReportViewerLocation;
                }

                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24744", ex);
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

                // If close was not cancelled then save window position
                if (!e.Cancel)
                {
                    Settings.Default.ReportViewerState = this.WindowState;

                    // Check for normal (not min or max) state
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        Settings.Default.ReportViewerSize = this.Bounds.Size;
                        Settings.Default.ReportViewerLocation = this.Bounds.Location;
                    }
                    else
                    {
                        // RestoreBounds only valid if in Min/Max window state
                        Settings.Default.ReportViewerSize = this.RestoreBounds.Size;
                        Settings.Default.ReportViewerLocation = this.RestoreBounds.Location;
                    }

                    Settings.Default.ReportViewerUsePersistedSettings = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI24746", ex);
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// otherwise, <see langword="false"/>.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                if (msg.Msg == WindowsMessage.KeyDown
                    && (keyData != Keys.Control || keyData != Keys.Alt || keyData != Keys.Shift)
                    && _crystalReportViewer.ReportSource != null)
                {
                    if (keyData == Keys.PageDown)
                    {
                        _crystalReportViewer.ShowNextPage();
                        return true;
                    }
                    else if (keyData == Keys.PageUp)
                    {
                        _crystalReportViewer.ShowPreviousPage();
                        return true;
                    }
                }

                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29949", ex);
            }

            return true;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleFileExitClick(object sender, EventArgs e)
        {
            try
            {
                // Close the form
                this.Close();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23507", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleFileOpenReportClick(object sender, EventArgs e)
        {
            try
            {
                using (OpenReportForm openReport = new OpenReportForm())
                {
                    openReport.Icon = this.Icon;
                    if (openReport.ShowDialog() == DialogResult.OK)
                    {
                        // Clear the report viewer
                        _crystalReportViewer.ReportSource = null;

                        // Set the title bar back to default
                        this.Text = _FORM_CAPTION;

                        // Set the report file name
                        _reportFileName = openReport.ReportFileName;

                        // Dispose of the old report
                        if (_report != null)
                        {
                            _report.Dispose();
                            _report = null;
                        }

                        // Refresh the window
                        this.Refresh();

                        // Load the new report
                        _report = new ExtractReport(_serverName, _databaseName, _reportFileName,
                            openReport.StandardReport);

                        // Enable/disable the save template menu item depending on whether the
                        // report is a standard report or a saved report
                        _saveReportTemplateToolStripMenuItem.Enabled = openReport.StandardReport;

                        // Attach the report to the viewer (if the user did not cancel)
                        if (_report != null && !_report.CanceledInitialization)
                        {
                            AttachReportToReportViewer();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23748", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleFileSaveReportTemplateClick(object sender, EventArgs e)
        {
            try
            {
                string response = null;
                string baseSavedName = null;
                do
                {
                    if (InputBox.Show(this, "Report name",
                        "Save Report Template", ref response) != DialogResult.OK)
                    {
                        // User canceled, just return
                        return;
                    }

                    // Build the file name from the response
                    baseSavedName = ExtractReport.SavedReportFolder + response;
                    
                    // Check if there is a standard report with the same name
                    if (IsSameNameAsStandardReport(response))
                    {
                        MessageBox.Show("The specified report name already exists in the "
                            + "Standard reports folder. Please specify a different "
                            + "name.", "Invalid Report Name", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                    }
                    // Validate the file name
                    else if (!FileSystemMethods.IsFileNameValid(response + ".rpt"))
                    {
                        MessageBox.Show("The specified report name contains invalid characters."
                            + " The following characters are not valid:" + Environment.NewLine
                            + "* \\ | / : \" < > ?", "Invalid Report Name", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                    }
                    else
                    {
                        // If the file does not exist or the user wants to overwrite then
                        // break from the loop
                        if (!File.Exists(baseSavedName + ".rpt")
                            || MessageBox.Show("There is already a saved report with that name."
                                + " Would you like to overwrite it?", "Overwrite Existing Report",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes)
                        {
                            break;
                        }
                    }
                }
                while(true);

                // Ensure baseSavedName is not null
                ExtractException.Assert("ELI23861", "Base name was null!", baseSavedName != null);

                try
                {
                    // Copy the report and preview file (overwriting if necessary)
                    string previewName = FileSystemMethods.PathCombine(
                        ExtractReport.StandardReportFolder,
                        Path.GetFileNameWithoutExtension(_report.FileName),
                        _PREVIEW_EXTENSION);
                    File.Copy(_report.FileName, baseSavedName + ".rpt", true);
                    if (File.Exists(previewName))
                    {
                        File.Copy(previewName, baseSavedName + _PREVIEW_EXTENSION, true);
                    }

                    // Write the xml file
                    _report.WriteXmlFile(baseSavedName + ".xml", true);
                }
                catch (Exception ex)
                {

                    // Ensure all files are cleaned up since copying failed
                    // Delete report file
                    string tempName = baseSavedName + ".rpt";
                    if (File.Exists(tempName))
                    {
                        FileSystemMethods.TryDeleteFile(tempName);
                    }
                    // Delete preview file
                    tempName = baseSavedName + _PREVIEW_EXTENSION;
                    if (File.Exists(tempName))
                    {
                        FileSystemMethods.TryDeleteFile(tempName);
                    }
                    // Delete xml file
                    tempName = baseSavedName + ".xml";
                    if (File.Exists(tempName))
                    {
                        FileSystemMethods.TryDeleteFile(tempName);
                    }

                    ExtractException ee = new ExtractException("ELI23866",
                        "Failed saving report template!", ex);
                    ee.AddDebugData("Base Template Name", baseSavedName, false);

                    throw ee;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23749", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleFileExportReportToPdfClick(object sender, EventArgs e)
        {
            try
            {
                if (_report != null)
                {
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.OverwritePrompt = true;
                    saveDialog.CheckPathExists = true;
                    saveDialog.Filter = _EXPORT_FORMAT_FILTER;

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _report.ExportReportToFile(saveDialog.FileName, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23750", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the export report file menu item.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleFileExportReportClick(object sender, EventArgs e)
        {
            try
            {
                // Open the export report dialog
                _crystalReportViewer.ExportReport();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24991", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleHelpAboutClick(object sender, EventArgs e)
        {
            try
            {
                using (AboutReportViewer about = new AboutReportViewer(this.Icon))
                {
                    about.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23774", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Method for attaching the currently loaded report object to the report viewer. This
        /// method ensures that the setting of the report is done in the UI thread.
        /// </summary>
        private void AttachReportToReportViewer()
        {
            ExtractException.Assert("ELI23751", "Report object cannot be null!",
                _report != null);

            // Hide the report viewer and show progress bar while the report is loaded
            _crystalReportViewer.Visible = false;
            _pleaseWaitLabel.Visible = true;
            _progressBar.Visible = true;
            this.Refresh();

            // Attach the report to the control and move to the first page
            // (this forces the data to be loaded into the control)
            // NOTE: The UI will be unresponsive at this point, the progress
            // marquee will continue to scroll but the user will be unable to
            // interact with the UI.
            _crystalReportViewer.ReportSource = _report.ReportDocument;
            _crystalReportViewer.ShowFirstPage();
            _crystalReportViewer.Zoom(75);

            // Set the title based on the report file
            this.Text = _FORM_CAPTION + " - " + Path.GetFileNameWithoutExtension(_reportFileName);

            // Show viewer and hide progress bar now that report is loaded
            _crystalReportViewer.Visible = true;
            _pleaseWaitLabel.Visible = false;
            _progressBar.Visible = false;

            // Enable the export buttons
            _exportReportToPDFToolStripMenuItem.Enabled = true;
            _exportReportToolStripMenuItem.Enabled = true;

            // Invalidate the form
            this.Invalidate();
        }

        /// <summary>
        /// Checks whether the specified report name exists in the standard reports folder.
        /// </summary>
        /// <param name="reportName">The report to check.</param>
        /// <returns><see langword="true"/> if there is a standard report with the same name,
        /// and <see langword="false"/> if there is not.</returns>
        // [LRCAU #5178]
        private bool IsSameNameAsStandardReport(string reportName)
        {
            // If the list of reports has not been built yet, build it
            if (_standardReports == null)
            {
                _standardReports = new List<string>();
                foreach (string fileName in
                    Directory.GetFiles(ExtractReport.StandardReportFolder, "*.rpt"))
                {
                    _standardReports.Add(
                        Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant());
                }
            }

            // Return true if the item is in the list
            return _standardReports.Contains(reportName.ToUpperInvariant());
        }

        #endregion Methods
    }
}