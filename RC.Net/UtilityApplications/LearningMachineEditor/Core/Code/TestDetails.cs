using Extract.AttributeFinder;
using Extract.Utilities;
using Leadtools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    public partial class TestDetails : Form
    {
        #region Fields

        Func<int, int> _memoizedGetMaxValue;
        Func<int, Color> _memoizedGetColorFromHue;

        (SerializableConfusionMatrix train, SerializableConfusionMatrix test) _cm;

        bool _normalizeByColumn = true;
        bool _testingAccuracy = true;
        bool _updateInProgress = false;
        bool _formLoaded = false;

        #endregion Fields

        SerializableConfusionMatrix CM
        {
            get
            {
                return _testingAccuracy
                    ? _cm.test
                    : _cm.train;
            }
        }

        #region Constructors

        /// <summary>
        /// Creates a form to display a confusion matrix
        /// </summary>
        /// <param name="cm"></param>
        public TestDetails((SerializableConfusionMatrix train, SerializableConfusionMatrix test) cm)
        {
            try
            {
                InitializeComponent();

                _cm = cm;

                groupBox3.Enabled = cm.train != null && cm.test != null;

                _testingAccuracy = cm.test != null;

                _memoizedGetColorFromHue = ((Func<int, Color>)GetColorFromHue).Memoize();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45000");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                testDetailsDataGridView.VirtualMode = true;
                testDetailsDataGridView.CellPainting += DataGridView_CellPainting;
                testDetailsDataGridView.CellValueNeeded += TestDetailsDataGridView_CellValueNeeded;
                testDetailsDataGridView.CellValuePushed += TestDetailsDataGridView_CellValuePushed;

                if (_testingAccuracy)
                {
                    showAccuracyForTestingSetRadioButton.Checked = true;
                }
                else
                {
                    showAccuracyForTrainingSetRadioButton.Checked = true;
                }

                _formLoaded = true;

                InitDataGridView();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45002");
            }
        }

        #endregion Overrides

        #region Private Methods

        /// <summary>
        /// Sets the data of the gridview and text fields using the current confusion matrix
        /// </summary>
        private void InitDataGridView()
        {
            _memoizedGetMaxValue = ((Func<int, int>)GetMaxValue).Memoize();

            // Add rows and columns to the view
            testDetailsDataGridView.ColumnCount = CM.Labels.Length + 1;
            testDetailsDataGridView.RowCount = CM.Labels.Length + 1;

            // Set column and row header values
            for (int i = 0; i < CM.Labels.Length; i++)
            {
                var label = CM.Labels[i];
                var col = testDetailsDataGridView.Columns[i];
                col.HeaderText = label;
                col.Name = label;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

                testDetailsDataGridView.Rows[i].HeaderCell.Value = label;
            }
            var totalCol = testDetailsDataGridView.Columns[CM.Labels.Length];
            totalCol.HeaderText = "Total";
            totalCol.Name = "Total";

            testDetailsDataGridView.Rows[CM.Labels.Length].HeaderCell.Value = "Total";

            try
            {
                _updateInProgress = true;
                negativeClassesTextBox.Text =
                    string.Join(", ", CM.NegativeClasses().Select(s => s.QuoteIfNeeded("\"", ',')));
            }
            finally
            {
                _updateInProgress = false;
            }

            UpdateClassLabels();
            SetScoreTextBoxValue();
        }

        int GetMaxValue(int rowOrColumn)
        {
            return _normalizeByColumn
                ? Enumerable.Range(0, CM.RowTotals.Length).Max(row => CM.Data[row][rowOrColumn])
                : Enumerable.Range(0, CM.ColumnTotals.Length).Max(column => CM.Data[rowOrColumn][column]);
        }

        Color GetColorValue(int rowIndex, int columnIndex)
        {
            int max = 0;
            if (_normalizeByColumn)
            {
                max = _memoizedGetMaxValue(columnIndex);
            }
            else
            {
                max = _memoizedGetMaxValue(rowIndex);
            }
            var hue = max == 0 ? 0 : (int)Math.Round((double)CM.Data[rowIndex][columnIndex] / max * 150);

            return _memoizedGetColorFromHue(hue);
        }

        Color GetColorFromHue(int hue)
        {
            var hsv = new RasterHsvColor(hue, 128, 255);
            var rgb = hsv.ToRasterColor();
            return Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }

        /// <summary>
        /// Adds/removes asterisk denoting negative class from column and row headers
        /// </summary>
        private void UpdateClassLabels()
        {
            HashSet<int> needsAsterisk = new HashSet<int>(CM.NegativeClassIndexes());

            // Set column and row header values
            for (int i = 0; i < CM.Labels.Length; i++)
            {
                bool needsUpdate = false;
                var label = CM.Labels[i];
                var col = testDetailsDataGridView.Columns[i];
                var colHeader = col.HeaderCell;
                var rowHeader = testDetailsDataGridView.Rows[i].HeaderCell;
                var oldLabel = col.HeaderText;

                // Add asterisk to text of negative class columns/rows
                // to explain how precision/recall numbers are calculated
                if (oldLabel.EndsWith("*", StringComparison.Ordinal))
                {
                    needsUpdate = true;
                }

                col.HeaderText = label;
                rowHeader.Value = label;

                // Add asterisk to text of negative class columns/rows
                // to explain how precision/recall numbers are calculated
                if (needsAsterisk.Contains(i))
                {
                    col.HeaderText += "*";
                    rowHeader.Value += "*";
                    needsUpdate = !needsUpdate;
                }

                if (needsUpdate)
                {
                    testDetailsDataGridView.InvalidateCell(colHeader);
                    testDetailsDataGridView.InvalidateCell(rowHeader);
                }
            }
        }

        /// <summary>
        /// Calculates P/R/F1Score and updates the score text box
        /// </summary>
        private void SetScoreTextBoxValue()
        {
            var p = CM.PrecisionMicroAverage();
            var r = CM.RecallMicroAverage();
            var f = 2 * p * r / (p + r);
            scoreTextBox.Text = UtilityMethods.FormatInvariant(
                $"F1: {f:N4}, Precision: {p:N4}, Recall: {r:N4}");
        }


        #endregion Private Methods

        #region Event Handlers


        private void TestDetailsDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            try
            {
                if (e.RowIndex >= CM.Labels.Length && e.ColumnIndex >= CM.Labels.Length)
                {
                    return;
                }
                else if (e.RowIndex == CM.Labels.Length)
                {
                    e.Value = CM.ColumnTotals[e.ColumnIndex];
                }
                else if (e.ColumnIndex == CM.Labels.Length)
                {
                    e.Value = CM.RowTotals[e.RowIndex];
                }
                else
                {
                    e.Value = CM.Data[e.RowIndex][e.ColumnIndex];
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46327");
            }
        }

        private void TestDetailsDataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            try
            {
                if (e.RowIndex >= CM.Labels.Length && e.ColumnIndex >= CM.Labels.Length)
                {
                    return;
                }

                if (e.RowIndex == CM.Labels.Length)
                {
                    // Read-only
                    e.Value = CM.ColumnTotals[e.ColumnIndex];
                }
                else if (e.ColumnIndex == CM.Labels.Length)
                {
                    // Read-only
                    e.Value = CM.RowTotals[e.RowIndex];
                }
                else
                {
                    int oldVal;
                    int newVal;
                    int dif;
                    try
                    {
                        oldVal = CM.Data[e.RowIndex][e.ColumnIndex];
                        newVal = Convert.ToInt32(e.Value, CultureInfo.InvariantCulture);
                        dif = newVal - oldVal;
                    }
                    catch
                    {
                        e.Value = CM.Data[e.RowIndex][e.ColumnIndex];
                        return;
                    }

                    CM.Data[e.RowIndex][e.ColumnIndex] = newVal;
                    CM.RowTotals[e.RowIndex] += dif;
                    CM.ColumnTotals[e.ColumnIndex] += dif;

                    testDetailsDataGridView.InvalidateCell(testDetailsDataGridView
                        .Rows[e.RowIndex]
                        .Cells[CM.Labels.Length]);

                    testDetailsDataGridView.InvalidateCell(testDetailsDataGridView
                        .Rows[CM.Labels.Length]
                        .Cells[e.ColumnIndex]);

                    SetScoreTextBoxValue();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46326");
            }
        }

        /// <summary>
        /// Sets text orientation of column headers and background colors of cells
        /// </summary>
        private void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                // Vertical text from column 0
                if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                {
                    e.PaintBackground(e.CellBounds, true);
                    e.Graphics.TranslateTransform(e.CellBounds.Left, e.CellBounds.Bottom);
                    e.Graphics.RotateTransform(270);
                    e.Graphics.DrawString(e.FormattedValue.ToString(), e.CellStyle.Font, Brushes.Black, 5, 5);
                    e.Graphics.ResetTransform();
                    e.Handled = true;
                }
                else if (e.RowIndex >= 0 && e.RowIndex < CM.RowTotals.Length
                                         && e.ColumnIndex >= 0 && e.ColumnIndex < CM.ColumnTotals.Length)
                {
                    e.CellStyle.BackColor = GetColorValue(e.RowIndex, e.ColumnIndex);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46325");
            }
        }

        /// <summary>
        /// Changes how cell colors are calculated
        /// </summary>
        private void HandleNormalizeByColumnsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _normalizeByColumn = normalizeByColumnsRadioButton.Checked;

                _memoizedGetMaxValue = ((Func<int, int>)GetMaxValue).Memoize();
                testDetailsDataGridView.Refresh();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46324");
            }
        }

        /// <summary>
        /// Changes which set of data is shown
        /// </summary>
        private void HandleShowAccuracyForTrainingSetRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_formLoaded)
                {
                    return;
                }
                _testingAccuracy = showAccuracyForTestingSetRadioButton.Checked;
                InitDataGridView();
                testDetailsDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                testDetailsDataGridView.Refresh();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46323");
                
            }
        }

        /// <summary>
        /// Updated the negative class indexes of the confusion matrix being shown
        /// </summary>
        private void HandleNegativeClassesTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_updateInProgress)
                {
                    return;
                }

                var classes = new List<string>();
                using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(
                    new StringReader(negativeClassesTextBox.Text)))
                {
                    csvReader.Delimiters = new[] { "," };
                    csvReader.CommentTokens = new[] { "//", "#" };
                    while (!csvReader.EndOfData)
                    {
                        string[] fields = csvReader.ReadFields();
                        classes.AddRange(fields);
                    }
                }

                CM.SetNegativeClasses(classes);
                UpdateClassLabels();
                SetScoreTextBoxValue();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45575");
            }
        }

        #endregion Event Handlers
    }
}
