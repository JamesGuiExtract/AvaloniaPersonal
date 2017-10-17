using Extract.AttributeFinder;
using Extract.Utilities;
using Leadtools;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
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

                groupBox3.Enabled = cm.train != null;

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

                InitDataGridView();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45002");
            }
        }

        #endregion Overrides

        #region Private Methods

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

            // Add asterisk to text of negative class columns/rows
            // to explain how precision/recall numbers are calculated
            foreach (var idx in CM.NegativeClassIndexes())
            {
                testDetailsDataGridView.Columns[idx].HeaderText += "*";
                testDetailsDataGridView.Rows[idx].HeaderCell.Value += "*";
            }
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

        #endregion Private Methods

        #region Event Handlers

        private void TestDetailsDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= CM.Labels.Length && e.ColumnIndex >= CM.Labels.Length)
            {
                var p = CM.PrecisionMicroAverage();
                var r = CM.RecallMicroAverage();
                var f = 2 * p * r / (p + r);
                e.Value = UtilityMethods.FormatInvariant($"P: {p:N2}, R: {r:N2}, F1: {f:N2}  *negative class(s)");
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
        private void TestDetailsDataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
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

                testDetailsDataGridView.Refresh();
            }
        }


        private void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Vertical text from column 0
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, true);
                e.Graphics.TranslateTransform(e.CellBounds.Left , e.CellBounds.Bottom);
                e.Graphics.RotateTransform(270);
                e.Graphics.DrawString(e.FormattedValue.ToString(),e.CellStyle.Font,Brushes.Black,5,5);
                e.Graphics.ResetTransform();
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.RowIndex < CM.RowTotals.Length
                && e.ColumnIndex >= 0 && e.ColumnIndex < CM.ColumnTotals.Length)
            {
                e.CellStyle.BackColor = GetColorValue(e.RowIndex, e.ColumnIndex);
            }
        }

        private void HandleNormalizeByColumnsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _normalizeByColumn = normalizeByColumnsRadioButton.Checked;

            _memoizedGetMaxValue = ((Func<int, int>)GetMaxValue).Memoize();
            testDetailsDataGridView.Refresh();
        }

        private void HandleShowAccuracyForTrainingSetRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _testingAccuracy = showAccuracyForTestingSetRadioButton.Checked;
            InitDataGridView();
            testDetailsDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            testDetailsDataGridView.Refresh();
        }

        #endregion Event Handlers
    }
}
