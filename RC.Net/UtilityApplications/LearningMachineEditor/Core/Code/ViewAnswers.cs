using Extract.AttributeFinder;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
            var dataSource = new BindingList<KeyValueClass>();
            for (int i=0; i < _encoder.AnswerCodeToName.Count; i++)
            {
                dataSource.Add(new KeyValueClass { Key = i, Value = _encoder.AnswerCodeToName[i] });
            }
            dataSource.AllowEdit = true;
            answerCategoriesDataGridView.DataSource = dataSource;

            // Add code column
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.ReadOnly = true;
            column.DataPropertyName = "Key";
            column.Name = "Code";
            answerCategoriesDataGridView.Columns.Add(column);

            // Add Name column
            column = new DataGridViewTextBoxColumn();
            column.ReadOnly = false;
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
                        string fileNameA = FileSystemMethods.GetTemporaryFileName();
                        string fileNameB = openDialog.FileName;
                        string exe = diffCommandLine.Substring(0, splitPoint);
                        string args = diffCommandLine.Substring(splitPoint)
                            .Replace("%1", fileNameA.Quote())
                            .Replace("%2", fileNameB.Quote());

                        // Write answer category names to the temp file
                        File.WriteAllLines(fileNameA, _encoder.AnswerCodeToName);

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

        /// <summary>
        /// Exports the collected DistinctValuesSeen values of the selected <see cref="IFeatureVectorizer"/>s
        /// </summary>
        /// <param name="distinct">if set to <c>true</c> then only distinct values will be exported, else
        /// values that occur in more than one vectorizer will be repeated</param>
        private void HandleExportFeatureVectorizerTerms_Click(object sender, EventArgs e)
        {
            try
            {
                var lines = _encoder.AnswerCodeToName;

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Document classifier index file|*.idx|All files|*.*";
                    saveDialog.FileName = "DocTypes.idx";
                    if (!string.IsNullOrEmpty(_fileName))
                    {
                        saveDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }

                    var result = saveDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        File.WriteAllLines(saveDialog.FileName, lines);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41841");
            }
        }

        private void HandleAnswerCategoriesDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string previousValue = null;

            try
            {
                if (e.ColumnIndex == 1)
                {
                    previousValue = _encoder.AnswerCodeToName[e.RowIndex];
                    var newValue = (string)answerCategoriesDataGridView.Rows[e.RowIndex].Cells[1].Value;

                    // Do nothing if value hasn't changed
                    if (string.Equals(previousValue, newValue, StringComparison.Ordinal))
                    {
                        return;
                    }

                    _encoder.ChangeAnswer(previousValue, newValue);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45650");

                if (previousValue != null)
                {
                    try
                    {
                        answerCategoriesDataGridView.Rows[e.RowIndex].Cells[1].Value = previousValue;
                    }
                    catch { }
                }
            }
        }

        #endregion Private Methods

        #region Private Classes

        private class KeyValueClass
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
            public int Key { get; set; }

            [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
            public string Value { get; set; }
        }

        #endregion Private Classes
    }
}
