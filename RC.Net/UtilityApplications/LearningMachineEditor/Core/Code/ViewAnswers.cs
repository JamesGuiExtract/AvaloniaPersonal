using Extract.AttributeFinder;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Form to view list of answers/categories that the machine can recognize
    /// </summary>
    public partial class ViewAnswers : Form
    {
        #region Constants

        private static readonly string _DEFAULT_DIFF_COMMAND = @"C:\Program Files (x86)\KDiff3\kdiff3.exe %1 %2";

        #endregion Constants

        #region Fields

        private LearningMachineDataEncoder _encoder;
        private string _fileName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public ViewAnswers(LearningMachineDataEncoder encoder, string fileName)
        {
            InitializeComponent();

            _encoder = encoder;
            _fileName = fileName;

            // Initialize the DataGridView.
            answerCategoriesDataGridView.AutoGenerateColumns = false;
            var dataSource = new BindingList<KeyValuePair<int, string>>(
                _encoder.AnswerCodeToName.OrderBy(kv => kv.Key).ToList());
            answerCategoriesDataGridView.DataSource = dataSource;

            // Add code column
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.ReadOnly = true;
            column.DataPropertyName = "Key";
            column.Name = "Code";
            answerCategoriesDataGridView.Columns.Add(column);

            // Add Name column
            column = new DataGridViewTextBoxColumn();
            column.ReadOnly = true;
            column.DataPropertyName = "Value";
            column.Name = "Category Name";
            answerCategoriesDataGridView.Columns.Add(column);
        }

        #endregion Constructors

        #region Private Methods

        /// <summary>
        /// Handles the Click event of the compareToListButton control.
        /// </summary>
        /// <remarks>Opens a file browser to select file to compare with and runs the configured Diff tool</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleCompareToListButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Open browser for selecting compare-to file
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "All files|*.*";
                    if (!string.IsNullOrEmpty(_fileName))
                    {
                        openDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }
                    if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Get diff command line from registry
                        var registrySettings = new RegistrySettings<Properties.Settings>(
                            @"Software\Extract Systems\TestingFramework\Settings").Settings;
                        if (string.IsNullOrWhiteSpace(registrySettings.DiffCommandLine))
                        {
                            registrySettings.DiffCommandLine = _DEFAULT_DIFF_COMMAND;
                        }
                        string diffCommandLine = registrySettings.DiffCommandLine;

                        // Separate command line into exe part and arguments part
                        var match = Regex.Match(diffCommandLine, @"(?inx)\.(exe|bat)\b");

                        ExtractException.Assert("ELI40051", "Could not split DiffCommandLine",
                            match.Success, "DiffCommandLine", diffCommandLine);

                        int splitPoint = match.Index + match.Length;
                        string fileNameA = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        string fileNameB = openDialog.FileName;
                        string exe = diffCommandLine.Substring(0, splitPoint);
                        string args = diffCommandLine.Substring(splitPoint)
                            .Replace("%1", fileNameA.Quote())
                            .Replace("%2", fileNameB.Quote());

                        // Write answer category names to the temp file
                        File.WriteAllLines(fileNameA,
                            _encoder.AnswerCodeToName
                            .OrderBy(kv => kv.Key)
                            .Select(kv => kv.Value));

                        var p = new System.Diagnostics.Process();
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.FileName = exe;
                        p.StartInfo.Arguments = args;

                        // Delete temp file when diff window closes
                        p.EnableRaisingEvents = true;
                        p.Exited += new EventHandler(delegate { FileSystemMethods.DeleteFile(fileNameA); });

                        // Run the diff
                        p.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40050");
            }
        }

        #endregion Private Methods
    }
}
