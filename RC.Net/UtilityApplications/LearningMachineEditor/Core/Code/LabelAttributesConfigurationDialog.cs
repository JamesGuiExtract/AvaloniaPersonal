using Extract.AttributeFinder;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Dialog to configure and run attribute labeling
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    [CLSCompliant(false)]
    public partial class LabelAttributesConfigurationDialog : Form
    {
        #region Fields

        // Don't update the settings object if in the process of initializing them from the settings object
        private bool _suspendUpdatesToSettingsObject;

        private LearningMachineConfiguration _editor;
        private LabelAttributes _labelAttributesSettings;
        private BindingList<CategoryQueryPair> _categoryQueryPairs;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelAttributesConfigurationDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="LearningMachineConfiguration"/> instance to configure.
        /// </param>
        public LabelAttributesConfigurationDialog(LearningMachineConfiguration settings)
        {
            try
            {
                _editor = settings;

                // Use a clone of the configuration for editing so that a true dirty/clean state can be calculated
                _labelAttributesSettings = _editor.CurrentLearningMachine.LabelAttributesSettings
                    ?.DeepClone()
                    ?? new LabelAttributes();

                InitializeComponent();

                // Initialize the DataGridView
                categoryAndQueryDataGridView.AutoGenerateColumns = false;

                // Add Category column
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Category";
                column.Name = "Category";
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                categoryAndQueryDataGridView.Columns.Add(column);

                // Add as XPath check-box
                column = new DataGridViewCheckBoxColumn();
                column.DataPropertyName = "CategoryIsXPath";
                column.Name = "Category is XPath";
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                categoryAndQueryDataGridView.Columns.Add(column);

                // Add Query column
                column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Query";
                column.Name = "Query";
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                categoryAndQueryDataGridView.Columns.Add(column);

                SetControlValues();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41443");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI41446").Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                // End edit so that no changes are lost
                if (categoryAndQueryDataGridView.IsCurrentCellInEditMode)
                {
                    categoryAndQueryDataGridView.EndEdit();
                }

                // Make a copy of the learning machine with (possibly) updated label attributes settings
                // so that dirty state will be updated. CurrentLearningMachine setter ensures that any
                // computed features and training are preserved.
                var learningMachineCopy = _editor.BuildLearningMachine();
                learningMachineCopy.LabelAttributesSettings = _labelAttributesSettings;
                _editor.CurrentLearningMachine = learningMachineCopy;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41436");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the AddButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleAddButton_Click(object sender, EventArgs e)
        {
            try
            {
                int currentRow = categoryAndQueryDataGridView.CurrentCell?.RowIndex ?? -1;
                _categoryQueryPairs.Insert(currentRow + 1, new CategoryQueryPair());
                categoryAndQueryDataGridView.CurrentCell
                    = categoryAndQueryDataGridView.Rows[currentRow + 1].Cells[0];
                categoryAndQueryDataGridView.Select();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI41444").Display();
            }
        }

        /// <summary>
        /// Handles the RowEnter event of the categoryAndQueryDataGridView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void HandleCategoryAndQueryDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                UpdateButtonStates(e.RowIndex);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI41445").Display();
            }
        }

        /// <summary>
        /// Update settings from UI controls
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendUpdatesToSettingsObject)
                {
                    return;
                }

                _labelAttributesSettings.AttributesToLabelPath = attributesToLabelTextBox.Text;
                _labelAttributesSettings.SourceOfLabelsPath = sourceOfLabelsTextBox.Text;
                _labelAttributesSettings.DestinationPath = destinationTextBox.Text;
                _labelAttributesSettings.CreateEmptyLabelForNonMatching = createEmptyLabelCheckBox.Checked;
                _labelAttributesSettings.OnlyIfAllCategoriesMatchOnSamePage = onlyIfMatchOnSamePageCheckBox.Checked;

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41438");
            }
        }

        /// <summary>
        /// Handles the Click event of the removeButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleRemoveButton_Click(object sender, EventArgs e)
        {
            try
            {
                int currentRow = categoryAndQueryDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow != -1)
                {
                    _categoryQueryPairs.RemoveAt(currentRow);
                    if (_categoryQueryPairs.Count > currentRow)
                    {
                        categoryAndQueryDataGridView.CurrentCell = categoryAndQueryDataGridView.Rows[currentRow].Cells[0];
                    }
                    else if (currentRow > 0)
                    {
                        categoryAndQueryDataGridView.CurrentCell = categoryAndQueryDataGridView.Rows[currentRow - 1].Cells[0];
                    }
                    categoryAndQueryDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41439");
            }
        }

        /// <summary>
        /// Handles the Click event of the upButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleUpButton_Click(object sender, EventArgs e)
        {
            try
            {
                int currentRow = categoryAndQueryDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow > 0)
                {
                    var tmp = _categoryQueryPairs[currentRow];
                    _categoryQueryPairs.RemoveAt(currentRow);
                    _categoryQueryPairs.Insert(currentRow - 1, tmp);
                    categoryAndQueryDataGridView.CurrentCell = categoryAndQueryDataGridView.Rows[currentRow - 1].Cells[0];
                    categoryAndQueryDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41440");
            }
        }

        /// <summary>
        /// Handles the Click event of the downButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleDownButton_Click(object sender, EventArgs e)
        {
            try
            {
                int currentRow = categoryAndQueryDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow >= 0 && currentRow < _categoryQueryPairs.Count)
                {
                    var tmp = _categoryQueryPairs[currentRow];
                    _categoryQueryPairs.RemoveAt(currentRow);
                    _categoryQueryPairs.Insert(currentRow + 1, tmp);
                    categoryAndQueryDataGridView.CurrentCell = categoryAndQueryDataGridView.Rows[currentRow + 1].Cells[0];
                    categoryAndQueryDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41441");
            }
        }

        /// <summary>
        /// Handles the Click event of the duplicateButton
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleDuplicateButton_Click(object sender, EventArgs e)
        {
            try
            {
                int currentRow = categoryAndQueryDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow >= 0)
                {
                    _categoryQueryPairs.Insert(currentRow + 1, _categoryQueryPairs[currentRow].ShallowClone());
                    categoryAndQueryDataGridView.CurrentCell = categoryAndQueryDataGridView.Rows[currentRow + 1].Cells[0];
                    categoryAndQueryDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41442");
            }
        }

        /// <summary>
        /// Opens a dialog and starts processing attributes
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleProcessButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var win = new LabelAttributesStatus(_labelAttributesSettings, _editor.CurrentLearningMachine.InputConfig))
                {
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41454");
            }
        }

        /// <summary>
        /// Handles the Click event of the RevertButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleRevertButton_Click(object sender, EventArgs e)
        {
            try
            {
                _labelAttributesSettings = _editor.CurrentLearningMachine.LabelAttributesSettings
                    ?.DeepClone()
                    ?? new LabelAttributes();

                SetControlValues();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41820");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Updates the state (enable/disable) of the up, down, remove and duplicate buttons.
        /// </summary>
        /// <param name="currentRowOption">The current row number in case update is being called before
        /// the current cell has changed (e.g., from RowEnter event).</param>
        void UpdateButtonStates(int? currentRowOption = null)
        {
            try
            {
                revertButton.Enabled = !_labelAttributesSettings.Equals(_editor.CurrentLearningMachine.LabelAttributesSettings);

                var currentRow = currentRowOption ?? categoryAndQueryDataGridView.CurrentCell?.RowIndex ?? -1;

                int rowCount = categoryAndQueryDataGridView.RowCount;

                if (currentRow == -1 || rowCount == 0)
                {
                    upButton.Enabled = downButton.Enabled = removeButton.Enabled = duplicateButton.Enabled = false;
                    return;
                }
                removeButton.Enabled = duplicateButton.Enabled = true;

                upButton.Enabled = currentRow > 0;
                downButton.Enabled = currentRow < rowCount - 1;

                onlyIfMatchOnSamePageCheckBox.Enabled = createEmptyLabelCheckBox.Checked;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41437");
            }
        }

        /// <summary>
        /// Sets the control values from the settings object.
        /// </summary>
        private void SetControlValues()
        {
            _suspendUpdatesToSettingsObject = true;

            _categoryQueryPairs = new BindingList<CategoryQueryPair>(_labelAttributesSettings.CategoryQueryPairs);
            _categoryQueryPairs.RaiseListChangedEvents = true;
            _categoryQueryPairs.ListChanged += HandleValueChanged;
            categoryAndQueryDataGridView.DataSource = _categoryQueryPairs;

            attributesToLabelTextBox.Text = _labelAttributesSettings.AttributesToLabelPath;
            sourceOfLabelsTextBox.Text = _labelAttributesSettings.SourceOfLabelsPath;
            destinationTextBox.Text = _labelAttributesSettings.DestinationPath;
            createEmptyLabelCheckBox.Checked = _labelAttributesSettings.CreateEmptyLabelForNonMatching;
            onlyIfMatchOnSamePageCheckBox.Checked = _labelAttributesSettings.OnlyIfAllCategoriesMatchOnSamePage;

            _suspendUpdatesToSettingsObject = false;
        }

        #endregion Private Methods
    }
}
