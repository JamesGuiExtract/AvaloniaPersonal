using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Extract.AttributeFinder;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Allows viewing/editing of computed feature vectorizers
    /// </summary>
    public partial class EditFeatures : Form
    {
        #region Fields

        private LearningMachineDataEncoder _encoder;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="encoder">The <see cref="LearningMachineDataEncoder"/> to be viewed/edited</param>
        [SuppressMessage("Microsoft.Performance", "CA1801", Target="encoder")]
        public EditFeatures(LearningMachineDataEncoder encoder)
        {
            try
            {
                InitializeComponent();
                _encoder = encoder;
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

                distinctValuesSeenListBox.DataSource = vectorizer.DistinctValuesSeen.ToList();
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

        #endregion Private Methods

    }
}
