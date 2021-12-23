using DevExpress.DataAccess.Sql;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using Extract.Dashboard.Utilities;
using Extract.Licensing;
using Extract.Reporting;
using Extract.ReportingDevExpress.Properties;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.ReportingDevExpress
{
    /// <summary>
    /// A form for displaying a report document.
    /// </summary>
    public partial class ReportViewerForm : RibbonForm
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
        /// The workflow name that the report should be run on.
        /// </summary>
        readonly string _workflowName;

        /// <summary>
        /// The <see cref="ExtractReport"/> object that contains the report to
        /// be displayed.
        /// </summary>
        IExtractReport _report;

        /// <summary>
        /// List that will contain the names of all the standard reports.  This is used
        /// in the report saving but is only instantiated when it is actually used.
        /// </summary>
        private List<string> _standardReports;

        /// <summary>
        /// Collection of reports that have been generated and emailed. These objects should
        /// be disposed when the form is closed.
        /// </summary>
        List<TemporaryFile> _temporaryFiles = new List<TemporaryFile>();

        /// <summary>
        /// Task that generates the report - currently cannot be canceled
        /// </summary>
        Task ReportGenerationTask = null;

        #endregion Fields

        private class SendFileTemplateCommandHandler : ICommandHandler
        {
            ReportViewerForm ReportForm { get; }

            public SendFileTemplateCommandHandler(ReportViewerForm reportViewerForm)
            {
                ReportForm = reportViewerForm;
            }
            public bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.SendFile ||
                    command == PrintingSystemCommand.SendPdf ||
                    command == PrintingSystemCommand.SendCsv ||
                    command == PrintingSystemCommand.SendDocx ||
                    command == PrintingSystemCommand.SendGraphic ||
                    command == PrintingSystemCommand.SendMht ||
                    command == PrintingSystemCommand.SendRtf ||
                    command == PrintingSystemCommand.SendTxt ||
                    command == PrintingSystemCommand.SendXls ||
                    command == PrintingSystemCommand.SendXlsx ||
                    command ==  PrintingSystemCommand.SendXps ||
                    command == PrintingSystemCommand.ExportCsv ||
                    command == PrintingSystemCommand.ExportDocx ||
                    command == PrintingSystemCommand.ExportFile ||
                    command == PrintingSystemCommand.ExportGraphic ||
                    command == PrintingSystemCommand.ExportHtm ||
                    command == PrintingSystemCommand.ExportMht ||
                    command == PrintingSystemCommand.ExportPdf ||
                    command == PrintingSystemCommand.ExportRtf ||
                    command == PrintingSystemCommand.ExportTxt ||
                    command == PrintingSystemCommand.ExportXls ||
                    command == PrintingSystemCommand.ExportXlsx ||
                    command == PrintingSystemCommand.ExportXps;
            }

            public void HandleCommand(PrintingSystemCommand command,
                                      object[] args,
                                      IPrintControl printControl,
                                      ref bool handled)
            {
                if (!CanHandleCommand(command, printControl))
                    return;
                try
                {
                    var reportName = Path.GetFileNameWithoutExtension(ReportForm._reportFileName);
                    var options = ReportForm.documentViewer.PrintingSystem.ExportOptions;
                    options.PrintPreview.DefaultFileName = reportName;
                    options.Email.Subject =
                        string.Concat("Report: ",
                                      reportName,
                                      " ",
                                      DateTime.Now.ToString("g", CultureInfo.CurrentCulture));
                    // do not mark as handled since this is just setting the options
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI49906");
                }
            }
        }


        private class SaveTemplateCommandHandler : ICommandHandler
        {
            ReportViewerForm ReportForm { get; }

            public SaveTemplateCommandHandler(ReportViewerForm reportViewerForm)
            {
                ReportForm = reportViewerForm;
            }
            public bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.Save;
            }

            public void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl printControl, ref bool handled)
            {
                if (!CanHandleCommand(command, printControl))
                    return;
                
                try
                {
                    string baseSavedName = GetSaveName();
                    if (string.IsNullOrEmpty(baseSavedName))
                        return;

                    try
                    {
                        // Copy the report and preview file (overwriting if necessary)
                        string previewName = Path.Combine(ExtractReportUtils.StandardReportFolder,
                            Path.GetFileNameWithoutExtension(ReportForm._reportFileName) + _PREVIEW_EXTENSION);
                        File.Copy(ReportForm._report.FileName, baseSavedName + ".repx", true);
                        if (File.Exists(previewName))
                        {
                            File.Copy(previewName, baseSavedName + _PREVIEW_EXTENSION, true);
                        }

                        // Write the xml file
                        ReportForm._report.WriteXmlFile(baseSavedName + ".xml", true);
                    }
                    catch (Exception ex)
                    {

                        // Ensure all files are cleaned up since copying failed
                        // Delete report file
                        string tempName = baseSavedName + ".repx";
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
                    ee.Display();
                }
                finally
                {
                    handled = true;
                }
            }

            private string GetSaveName()
            {
                string response = null;
                string baseSavedName = null;
                do
                {
                    if (InputBox.Show(ReportForm, "Report name", "Save Report Template", ref response) !=
                        DialogResult.OK)
                    {
                        // User canceled, just return empty string
                        return "";
                    }

                    // Build the file name from the response
                    baseSavedName = Path.Combine(ExtractReportUtils.SavedReportFolder, response);

                    // Check if there is a standard report with the same name
                    if (ReportForm.IsSameNameAsStandardReport(response))
                    {
                        MessageBox.Show("The specified report name already exists in the " +
                            "Standard reports folder. Please specify a different " +
                            "name.",
                                        "Invalid Report Name",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error,
                                        MessageBoxDefaultButton.Button1,
                                        0);
                    }
                    // Validate the file name
                    else if (!FileSystemMethods.IsFileNameValid(response + ".repx"))
                    {
                        MessageBox.Show("The specified report name contains invalid characters." +
                            " The following characters are not valid:" +
                            Environment.NewLine +
                            "* \\ | / : \" < > ?",
                                        "Invalid Report Name",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error,
                                        MessageBoxDefaultButton.Button1,
                                        0);
                    }
                    else
                    {
                        // If the file does not exist or the user wants to overwrite then
                        // break from the loop
                        if (!File.Exists(baseSavedName + ".repx") ||
                            MessageBox.Show("There is already a saved report with that name." +
                                " Would you like to overwrite it?",
                                            "Overwrite Existing Report",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Warning,
                                            MessageBoxDefaultButton.Button1,
                                            0) ==
                            DialogResult.Yes)
                        {
                            return baseSavedName;
                        }
                    }
                }
                while (true);
            }
        }

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="ReportViewerForm"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportViewerForm"/> class.
        /// </summary>
        /// <param name="serverName">The database server to attach the report to.</param>
        /// <param name="databaseName">The database name to attach the report to.</param>
        /// <param name="workflowName">The workflow to run the report on.</param>
        public ReportViewerForm(string serverName, string databaseName, string workflowName)
            : this(null, serverName, databaseName, workflowName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportViewerForm"/> class.
        /// </summary>
        /// <param name="report">The <see cref="ExtractReport"/> report to
        /// attach to.</param>
        public ReportViewerForm(IExtractReport report)
            : this (report, report.DatabaseServer, report.DatabaseName, report.WorkflowName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportViewerForm"/> class.
        /// </summary>
        /// <param name="report">The <see cref="ExtractReport"/> report to
        /// attach to.</param>
        /// <param name="serverName">The database server to attach the report to.</param>
        /// <param name="databaseName">The database name to attach the report to.</param>
        /// <param name="workflowName">The workflow to run the report on.</param>
        public ReportViewerForm(IExtractReport report, string serverName, string databaseName, string workflowName)
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

                // This is a work around for a problem specifically in 19.2.3 of the DevExpress tools, it is fixed in 
                // later versions. The problem is that the icons for the prev and next buttons are reversed
                var ver = AssemblyName.GetAssemblyName(typeof(ReportPrintTool).Assembly.Location).Version; 
                var swapPageNavigation = ver.Major == 19 && ver.Minor == 2 && ver.Build <= 3;
                if (swapPageNavigation)
                {
                    var temp = printPreviewBarItem19.ImageOptions.DefaultSvgImage;
                    printPreviewBarItem19.ImageOptions.DefaultSvgImage = printPreviewBarItem20.ImageOptions.DefaultSvgImage;
                    printPreviewBarItem20.ImageOptions.DefaultSvgImage = temp;
                }

                _report = report as IExtractReport;

                // Store the report and database information
                _reportFileName = report != null ? report.FileName : "";
                _serverName = serverName;
                _databaseName = databaseName;
                _workflowName = workflowName;

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
                    OpenReportBarButton.PerformClick();
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
                    && documentViewer.DocumentSource != null)
                {
                    if (keyData == Keys.PageDown)
                    {
                        documentViewer.SelectNextPage();
                        return true;
                    }
                    else if (keyData == Keys.PageUp)
                    {
                        documentViewer.SelectPrevPage();
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
        private void HandleFileExitClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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
                        ResetReportInfo();
                        // Set the report file name
                        _reportFileName = openReport.ReportFileName;

                        // Refresh the window
                        this.Refresh();

                        // Load the new report
                        _report = new ExtractReport(_serverName, _databaseName, _workflowName, _reportFileName);

                        bool parametersSet = _report.SetParameters(openReport.StandardReport, false);

                        // Attach the report to the viewer (if the user did not cancel)
                        if (_report != null && parametersSet)
                        {
                            AttachReportToReportViewer();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ResetReportInfo();

                ExtractException ee = ExtractException.AsExtractException("ELI23748", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        private void ResetReportInfo()
        {
            // reset the report related fields
            _reportFileName = "";
            _report = null;
            documentViewer.DocumentSource = null;
            this.Text = _FORM_CAPTION;
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
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleHelpAboutClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
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

        /// <summary>
        /// Handles the refresh open document.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DevExpress.XtraBars.ItemClickEventArgs"/> instance containing the event data.</param>
        private void HandleRefreshBarButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (_report != null)
                {
                    _report.Refresh();

                    AttachReportToReportViewer();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32487");
            }
        }

        private void HandleXtraReportParametersRequestBeforeShow(object sender, ParametersRequestEventArgs e)
        {
            try
            {
                // By setting all of the parameters to be not visible the Parameter panel will not be displayed
                // the Parameters are set in our parameter dialog
                foreach (var p in e.ParametersInformation)
                {
                    p.Parameter.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49978");
            }

        }

        private void HandleBarButtonItemParameters_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var report = _report as ExtractReport;
                if (report is null) return;
                report.SetParameters(true, true);
                AttachReportToReportViewer();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49979");
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Method for attaching the currently loaded report object to the report viewer. This
        /// method ensures that the setting of the report is done in the UI thread.
        /// </summary>
        private async void AttachReportToReportViewer()
        {
            try
            {
                ExtractException.Assert("ELI23751", "Report object cannot be null!", _report != null);

                var reportAsXtraReport = _report.ReportDocument as XtraReport;
                await GenerateReport(reportAsXtraReport);

            }
            catch (ExtractException ee)
            {
                if (ee.EliCode == "ELI50335")
                    ResetReportInfo();
                ee.Display();
                OpenReportBarButton.Enabled = true;
            }
        }

        private async Task GenerateReport(XtraReport reportAsXtraReport)
        {
            try
            {
                reportAsXtraReport.RequestParameters = false;
                reportAsXtraReport.ParametersRequestBeforeShow += HandleXtraReportParametersRequestBeforeShow;
                
                OpenReportBarButton.Enabled = false;
                using (ReportProgressForm progressForm = new ReportProgressForm(_reportFileName))
                {
                    progressForm.Show(this);

                    ReportGenerationTask = new Task(() =>
                    {
                        try
                        {
                            SqlDataSource sqlSource = reportAsXtraReport.DataSource as SqlDataSource;
                            if (!sqlSource?.Connection.IsConnected ?? false)
                            {
                                using var appConfig = new AppRoleConfig(sqlSource.Connection.ConnectionString);
                                if (appConfig.AddAppRoleQuery(sqlSource))
                                {
                                    sqlSource.Fill(DashboardHelpers.AppRoleQueryName(sqlSource));
                                }
                            }
                            reportAsXtraReport.CreateDocument();
                        }
                        catch (Exception ex)
                        {
                            if (ExceptionContainsSetApproleError(ex))
                            {
                                ExtractException ee = new("ELI53011", "Unable to set application role");
                                throw ee;
                            }
                            else
                            {
                                throw ex.AsExtract("ELI53013");
                            }
                        }

                    }, TaskCreationOptions.LongRunning);
                    ReportGenerationTask.Start();
                    await ReportGenerationTask;

                    ProcessGenerateReportComplete();
                    progressForm.CanClose = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50335");
            }
            finally
            {
                ReportGenerationTask.Dispose();
                ReportGenerationTask = null;
            }
        }     
        
        private bool ExceptionContainsSetApproleError(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return false;
            }

            if (ex.Message.Contains("sp_setapprole"))
            {
                return true;
            }

            return ExceptionContainsSetApproleError(ex.InnerException);
        }

        private void ProcessGenerateReportComplete()
        {
            var reportAsXtraReport = _report.ReportDocument as XtraReport;
            documentViewer.DocumentSource = reportAsXtraReport;

            barButtonItemParameters.Enabled = UserParametersExist(reportAsXtraReport);
            documentViewer.SelectFirstPage();

            // Set the title based on the report file
            this.Text = _FORM_CAPTION + " - " + Path.GetFileNameWithoutExtension(_reportFileName);
            bool saveEnabled = !string.IsNullOrEmpty(_reportFileName) &&
                ExtractReport.StandardReportFolder
                    .Equals(Path.GetDirectoryName(_reportFileName), StringComparison.OrdinalIgnoreCase);

            // Enable / disable the save template menu item depending on whether the
            // report is a standard report or not
            documentViewer.PrintingSystem
                .SetCommandVisibility(PrintingSystemCommand.Save,
                                        (saveEnabled) ? CommandVisibility.All : CommandVisibility.None);
            documentViewer.PrintingSystem
                .SetCommandVisibility(PrintingSystemCommand.Parameters, CommandVisibility.None);

            // Add overrides for the Save and SendFile commands
            documentViewer.PrintingSystem.AddCommandHandler(new SaveTemplateCommandHandler(this));
            documentViewer.PrintingSystem.AddCommandHandler(new SendFileTemplateCommandHandler(this));

            Invalidate();

            OpenReportBarButton.Enabled = true;
        }

        private static bool UserParametersExist(XtraReport report)
        {
            int numberOfExtractParameters = 0;
            foreach ( var p in report.Parameters)
            {
                if (p.Name.StartsWith("ES_"))
                    numberOfExtractParameters++;
            }

            return report.Parameters.Count > numberOfExtractParameters;
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
                    Directory.GetFiles(ExtractReport.StandardReportFolder, "*.repx"))
                {
                    _standardReports.Add(
                        Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant());
                }
            }

            // Return true if the item is in the list
            return _standardReports.Contains(reportName.ToUpperInvariant());
        }

        /// <summary>
        /// Gets the name of the temporary pdf file for emailing a report.
        /// <para><b>Note:</b></para>
        /// This method will also create an empty file on the disk.
        /// </summary>
        /// <param name="reportFileName">Name of the report file being exported.</param>
        /// <returns>The name of the temporary file.</returns>
        static string GetTemporaryPdfEmailReportFileName(string reportFileName)
        {
            // Get the temporary file to write to
            var baseFileName = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(reportFileName) + ".pdf");

            // Temporary file name will be a time stamped named version of the
            // the base file name with the time stamp placed before the extension.
            // In the case that the file already exists, sleep for a random amount
            // of time and generate a new timestamped file name. After successful
            // drop out of the loop, create the file.
            // NOTE: There is a potential race condition of two threads creating
            // the same file, but it should be a fairly rare occurrence and since
            // it would mean that two report viewers are open by the same user
            // with the same report and email was clicked at basically the same
            // instant, there really shouldn't be an issue. In all actuality I do
            // not expect that the while loop would ever really be executed.
            var tempFileName =
                FileSystemMethods.BuildTimeStampedBackupFileName(baseFileName, true);
            while (File.Exists(tempFileName))
            {
                Thread.Sleep((new Random().Next(300, 1000)));
                tempFileName =
                    FileSystemMethods.BuildTimeStampedBackupFileName(baseFileName, true);
            }
            File.Create(tempFileName).Dispose();

            return tempFileName;
        }

        #endregion Methods

        private void ReportViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (ReportGenerationTask?.Status == System.Threading.Tasks.TaskStatus.Running)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50345");
            }
        }
    }
}