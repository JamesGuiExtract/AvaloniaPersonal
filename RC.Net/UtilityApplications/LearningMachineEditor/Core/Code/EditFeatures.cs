using Extract.AttributeFinder;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Allows viewing/editing of computed feature vectorizers
    /// </summary>
    public partial class EditFeatures : Form
    {
        #region Fields

        private LearningMachineDataEncoder _encoder;
        private string _workingDir;
        private NumberSelector _numberSelector = new NumberSelector();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="encoder">The <see cref="LearningMachineDataEncoder"/> to be viewed/edited</param>
        [SuppressMessage("Microsoft.Performance", "CA1801", Target = "encoder")]
        public EditFeatures(LearningMachineDataEncoder encoder, string workingDir)
        {
            try
            {
                InitializeComponent();

                _encoder = encoder;
                _workingDir = workingDir ?? Environment.CurrentDirectory;

                SetSummaryStatusText();

                var featureVectorizers = new List<IFeatureVectorizer>();
                if (encoder.AutoBagOfWords != null)
                {
                    featureVectorizers.Add(encoder.AutoBagOfWords);
                }
                featureVectorizers.AddRange(encoder.AttributeFeatureVectorizers);

                // Initialize the DataGridView
                featureListDataGridView.AutoGenerateColumns = false;
                var dataSource = new BindingList<IFeatureVectorizer>(featureVectorizers);
                dataSource.RaiseListChangedEvents = true;
                dataSource.ListChanged += new ListChangedEventHandler(HandleFeatureListDataGridViewDataSource_ListChanged);
                featureListDataGridView.DataSource = dataSource;

                // Add Enabled column
                DataGridViewColumn column = new DataGridViewCheckBoxColumn();
                column.DataPropertyName = "Enabled";
                column.Name = "Enabled";
                featureListDataGridView.Columns.Add(column);

                // Add Name column
                column = new DataGridViewTextBoxColumn();
                column.ReadOnly = true;
                column.DataPropertyName = "Name";
                column.Name = "Name";
                featureListDataGridView.Columns.Add(column);

                // Add Type column
                var combo = new DataGridViewComboBoxColumn();
                combo.DataSource = Enum.GetValues(typeof(FeatureVectorizerType));
                combo.DataPropertyName = "FeatureType";
                combo.Name = "Type";
                featureListDataGridView.Columns.Add(combo);

                // Add # Features column
                column = new DataGridViewTextBoxColumn();
                column.ReadOnly = true;
                column.DataPropertyName = "FeatureVectorLength";
                column.Name = "# Features";
                featureListDataGridView.Columns.Add(column);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40040");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:Load"/> event.
        /// </summary>
        /// <remarks>Sets the Type cell to be read-only for any feature vectorizers where
        /// the feature type is not changeable</remarks>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                foreach (var row in featureListDataGridView.Rows.Cast<DataGridViewRow>()
                    .Where(r => !((IFeatureVectorizer)r.DataBoundItem).IsFeatureTypeChangeable))
                {
                    row.Cells["Type"].ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40052");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Updates the summary info (feature vector length and number of output categories) when a feature
        /// vectorizer is modified
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
        private void HandleFeatureListDataGridViewDataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            SetSummaryStatusText();
        }

        /// <summary>
        /// Updates the feature details information for the current row
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void HandleFeatureListDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int numberOfRows = featureListDataGridView.Rows.Count;
                if (numberOfRows < 1)
                {
                    return;
                }

                var vectorizer = (IFeatureVectorizer)featureListDataGridView.Rows[e.RowIndex].DataBoundItem;

                distinctValuesSeenListBox.DataSource = vectorizer.RecognizedValues.ToList();
                var attributeVectorizer = vectorizer as AttributeFeatureVectorizer;
                if (attributeVectorizer != null)
                {
                    featureDetailsTextBox.Text = string.Format(CultureInfo.CurrentCulture,
                        "Attribute feature. Multiple values: {0:N0}. Numeric values: {1:N0}. Non-numeric values: {2:N0}.",
                        attributeVectorizer.CountOfMultipleValuesOccurred,
                        attributeVectorizer.CountOfNumericValuesOccurred,
                        attributeVectorizer.CountOfNonnumericValuesOccurred);
                }
                else
                {
                    featureDetailsTextBox.Text = "Spatial string feature.";
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40041");
            }
        }

        /// <summary>
        /// Handles the Opening event of the distinctValuesSeenContextMenuStrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void HandleDistinctValuesSeenContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                distinctValuesSeenContextMenuStrip.Items["copyToolStripMenuItem"].Enabled =
                    distinctValuesSeenListBox.SelectedItems.Count > 0;
                distinctValuesSeenContextMenuStrip.Items["selectAllToolStripMenuItem"].Enabled =
                    distinctValuesSeenListBox.Items.Count > 0;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41811");
            }
        }

        /// <summary>
        /// Handles the Opening event of the featureVectorizersContextMenuStrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void HandleFeatureVectorizersContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                var selectionCount = featureListDataGridView.SelectedRows.Count;
                var enable = selectionCount < 1 ||
                    featureListDataGridView.SelectedRows
                    .Cast<DataGridViewRow>()
                    .Select(row => row.DataBoundItem)
                    .OfType<IFeatureVectorizer>()
                    .Where(vectorizer => vectorizer.FeatureType == FeatureVectorizerType.DiscreteTerms)
                    .Count() == selectionCount;

                foreach (var item in featureVectorizersContextMenuStrip
                    .Items
                    .OfType<ToolStripMenuItem>())
                {
                    item.Enabled = enable;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41835");
            }

        }

        /// <summary>
        /// Handles the Click event of the copyToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(
                    string.Join(Environment.NewLine,
                        distinctValuesSeenListBox.SelectedItems
                        .Cast<object>()
                        .Select(o => o.ToString())));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41812");
            }
        }

        /// <summary>
        /// Handles the Click event of the selectAllToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleSelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    distinctValuesSeenListBox.BeginUpdate();
                    foreach (int i in Enumerable.Range(0, distinctValuesSeenListBox.Items.Count))
                    {
                        distinctValuesSeenListBox.SetSelected(i, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41813");
            }
            finally
            {
                distinctValuesSeenListBox.EndUpdate();
            }
        }

        /// <summary>
        /// Handles the Click event of the limitToTopToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleLimitToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int numberOfRows = featureListDataGridView.SelectedRows.Count;
                if (numberOfRows < 1)
                {
                    return;
                }

                if (_numberSelector != null
                    && _numberSelector.ShowDialog() == DialogResult.OK)
                {
                    var vectorizers = featureListDataGridView.SelectedRows
                        .Cast<DataGridViewRow>()
                        .Select(row => row.DataBoundItem)
                        .OfType<IFeatureVectorizer>()
                        .Where(v => v.FeatureType == FeatureVectorizerType.DiscreteTerms);

                    foreach (var vectorizer in vectorizers)
                    {
                        vectorizer.LimitToTopTerms(_numberSelector.Value);
                    }

                    // Refresh items
                    ((BindingList<IFeatureVectorizer>)featureListDataGridView.DataSource).ResetBindings();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49996");
            }
        }

        /// <summary>
        /// Handles the Click event of the exportToFileToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleExportToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ExportFeatureVectorizerTerms(false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41839");
            }

        }

        /// <summary>
        /// Handles the Click event of the exportToFileDistinctToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleExportDistinctToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ExportFeatureVectorizerTerms(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49997");
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the featureListDataGridView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void HandleFeatureListDataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            // Handle row selection for right-click (left-click works automatically)
            if (e.Button == MouseButtons.Left)
            {
                return;
            }

            // Do nothing if click is not in a row or if the row is already selected
            var hti = featureListDataGridView.HitTest(e.X, e.Y);
            if (hti.RowIndex < 0
                || hti.RowIndex >= featureListDataGridView.Rows.Count
                || featureListDataGridView.Rows[hti.RowIndex].Selected)
            {
                return;
            }

            // Clear current selection unless Control key is pressed
            if ((ModifierKeys & Keys.Control) == 0)
            {
                featureListDataGridView.ClearSelection();
            }

            featureListDataGridView.Rows[hti.RowIndex].Selected = true;
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Sets the summary status text (feature vector length and number of output categories).
        /// </summary>
        private void SetSummaryStatusText()
        {
            try
            {
                summaryStatusStrip.Items.Clear();
                this.summaryStatusStrip.Items.Add(string.Format(CultureInfo.CurrentCulture,
                    "Total input features: {0:N0}", _encoder.FeatureVectorLength));
                this.summaryStatusStrip.Items.Add(string.Format(CultureInfo.CurrentCulture,
                    "Total output categories: {0:N0}", _encoder.AnswerCodeToName.Count));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40049");
            }
        }

        /// <summary>
        /// Exports the collected DistinctValuesSeen values of the selected <see cref="IFeatureVectorizer"/>s
        /// </summary>
        /// <param name="distinct">if set to <c>true</c> then only distinct values will be exported, else
        /// values that occur in more than one vectorizer will be repeated</param>
        private void ExportFeatureVectorizerTerms(bool distinct)
        {
            var vectorizers = featureListDataGridView.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => row.DataBoundItem)
                .OfType<IFeatureVectorizer>();

            var lines = vectorizers.SelectMany(v => v.RecognizedValues);
            if (distinct)
            {
                lines = lines.Distinct();
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text file|*.txt|All files|*.*";
                saveDialog.FileName = "Export.txt";
                saveDialog.InitialDirectory = _workingDir;

                var result = saveDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    File.WriteAllLines(saveDialog.FileName, lines);
                }
            }
        }

        #endregion Private Methods
    }
}
