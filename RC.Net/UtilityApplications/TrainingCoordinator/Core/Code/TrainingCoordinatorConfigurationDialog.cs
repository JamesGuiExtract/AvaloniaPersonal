using AttributeDbMgrComponentsLib;
using Extract.ETL;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.MachineLearning
{
    /// <summary>
    /// Dialog to configure and run a training coordinator
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TrainingCoordinatorConfigurationDialog : Form
    {
        #region Fields

        // Flag to short-circuit value-changed handler
        bool _suspendUpdatesToSettingsObject;

        TrainingCoordinator _settings;
        bool _dirty;

        static readonly string _TITLE_TEXT = "Training coordinator";
        static readonly string _TITLE_TEXT_DIRTY = "*" + _TITLE_TEXT;
        FileProcessingDB _database;
        bool _running;

        // Use a task and cancellation token for manual running
        Task _mainTask;
        CancellationTokenSource _cancellationTokenSource;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether anything has been modified since loading
        /// </summary>
        private bool Dirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
                Text = _dirty
                    ? _TITLE_TEXT_DIRTY
                    : _TITLE_TEXT;
            }
        }

        string DatabaseServer
        {
            get => _settings.DatabaseServer;
            set => _settings.DatabaseServer = value;
        }

        string DatabaseName
        {
            get => _settings.DatabaseName;
            set => _settings.DatabaseName = value;
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a configuration dialog for an <see cref="TrainingCoordinator"/>
        /// </summary>
        /// <param name="coordinator">The instance to configure</param>
        /// <param name="databaseServer">The server to use to resolve MLModel.Names and AttributeSetNames</param>
        /// <param name="databaseName">The database to use to resolve MLModel.Names and AttributeSetNames</param>
        public TrainingCoordinatorConfigurationDialog(TrainingCoordinator coordinator, string databaseServer, string databaseName)
        {
            try
            {
                _settings = coordinator;
                DatabaseServer = databaseServer;
                DatabaseName = databaseName;

                InitializeComponent();
                InitializeDataGridView(_dataCollectorsDataGridView);
                InitializeDataGridView(_modelTrainersDataGridView);

                SetControlValues(_settings);
            }
            catch (Exception ex)
            {
                _settings = new TrainingCoordinator();
                ex.ExtractDisplay("ELI45813");
            }
            _settings.PropertyChanged += HandleSettings_PropertyChanged;
        }

        private static void InitializeDataGridView(DataGridView dataGridView)
        {
            dataGridView.AutoGenerateColumns = false;

            // Add Description column
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Description";
            column.Name = "Description";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns.Add(column);

            // Add ModelName column
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "ModelName";
            column.Name = "ModelName";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns.Add(column);

            // Add Enabled check-box
            column = new DataGridViewCheckBoxColumn();
            column.DataPropertyName = "Enabled";
            column.Name = "Enabled";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridView.Columns.Add(column);
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

                try
                {
                    _database = new FileProcessingDB
                    {
                        DatabaseServer = DatabaseServer,
                        DatabaseName = DatabaseName
                    };
                }
                catch
                {
                    _database = null;
                    throw;
                }

                if (string.IsNullOrWhiteSpace(DatabaseServer) || string.IsNullOrWhiteSpace(DatabaseName))
                {
                    var result = _database.ShowSelectDB("Select database", false, false);
                    ExtractException.Assert("ELI45814", "No database configured", result);

                    DatabaseServer = _database.DatabaseServer;
                    DatabaseName = _database.DatabaseName;
                }

                UpdateButtonStatesForCollectors();
                UpdateButtonStatesForTrainers();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45815").Display();
                Close();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // End edit so that no changes are lost
                if (_dataCollectorsDataGridView.IsCurrentCellInEditMode)
                {
                    _dataCollectorsDataGridView.EndEdit();
                }

                if (_modelTrainersDataGridView.IsCurrentCellInEditMode)
                {
                    _modelTrainersDataGridView.EndEdit();
                }

                // Check for dirty changes unless the OK button is the source of the close
                if (DialogResult == DialogResult.OK)
                {
                    _settings.PropertyChanged -= HandleSettings_PropertyChanged;
                }
                else
                {
                    switch (this.PromptForSaveChanges(Dirty))
                    {
                        case DialogResult.Yes:
                            _settings.PropertyChanged -= HandleSettings_PropertyChanged;
                            HandleOkButton_Click(this, e);
                            break;
                        case DialogResult.No:
                            _settings.PropertyChanged -= HandleSettings_PropertyChanged;
                            DialogResult = DialogResult.Cancel;
                            break;
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45816");
            }

            base.OnFormClosing(e);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Writes the field values to the settings object
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                _descriptionTextBox.Focus();
                ExtractException.Assert("ELI45837", "Description cannot be empty.",
                    !string.IsNullOrWhiteSpace(_descriptionTextBox.Text));
                _settings.Description = _descriptionTextBox.Text;

                ValidateModelNames();

                var badCollectors = _settings.DataCollectors
                    .Cast<IConfigSettings>()
                    .Select((s, i) => s.IsConfigured() ? -1 : i)
                    .Where(i => i >= 0);

                if (badCollectors.Any())
                {
                    int idx = badCollectors.First();
                    _dataCollectorsDataGridView.Rows[idx].Selected = true;
                    throw new ExtractException("ELI45838",
                        UtilityMethods.FormatCurrent($"Data collector at index {idx} is not configured"));
                }

                var badTrainers = _settings.ModelTrainers
                    .Cast<IConfigSettings>()
                    .Select((s, i) => s.IsConfigured() ? -1 : i)
                    .Where(i => i >= 0);

                if (badTrainers.Any())
                {
                    int idx = badTrainers.First();
                    _modelTrainersDataGridView.Rows[idx].Selected = true;
                    throw new ExtractException("ELI45839",
                        UtilityMethods.FormatCurrent($"Model trainer at index {idx} is not configured"));
                }

                // Save any status modifications to the status column
                _settings.SaveStatus(_settings.Status);

                Dirty = false;
                DialogResult = DialogResult.OK;

                // This is needed for running the configuration from the command line
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45817");
                DialogResult = DialogResult.None;
            }
        }

        /// <summary>
        /// Closes the form without updating the settings object
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleCancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;

                // This is needed for running the configuration from the command line
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45818");
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

                ApplyControlValues();

                Dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45819");
            }
        }

        /// <summary>
        /// Set control values from _settings when a property changes. Used to update the log
        /// text while running manually.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Action Update = delegate
            {
                try
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    _suspendUpdatesToSettingsObject = true;

                    // Short-circuit for log value changed
                    if (e.PropertyName.Equals("Log", StringComparison.Ordinal))
                    {
                        if (_settings.Log.StartsWith(_logTextBox.Text, StringComparison.Ordinal))
                        {
                            if (_settings.Log.Length > _logTextBox.Text.Length)
                            {
                                string newText = _settings.Log.Substring(_logTextBox.Text.Length);
                                _logTextBox.AppendText(newText);
                            }
                        }
                        else
                        {
                            _logTextBox.Text = _settings.Log;

                            // Scroll to end of log
                            _logTextBox.SelectionStart = _logTextBox.TextLength;
                            _logTextBox.ScrollToCaret();
                        }
                    }
                    else
                    {
                        SetControlValues(_settings);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI45820");
                }
                finally
                {
                    _suspendUpdatesToSettingsObject = false;
                }
            };

            if (InvokeRequired)
            {
                Invoke(Update);
            }
            else
            {
                Update();
            }
        }

        /// <summary>
        /// Handles the Click event of the _runStopButton
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleRunStopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_running)
                {
                    _cancellationTokenSource?.Cancel();
                    _runStopButton.Enabled = false;

                    // Wait for task to complete
                    using (var _ = new TemporaryWaitCursor())
                        while (!(_mainTask == null || _mainTask.IsCompleted || _mainTask.IsCanceled || _mainTask.IsFaulted))
                        {
                            Application.DoEvents();
                            Thread.Sleep(100);
                        }

                    _globalSettingsGroupBox.Enabled =
                        _mlServicesTabPage.Enabled =
                        _okButton.Enabled =
                        _cancelButton.Enabled =
                        _runStopButton.Enabled = true;
                    _runStopButton.Text = "Run now";
                    _running = false;
                }
                else
                {
                    _globalSettingsGroupBox.Enabled =
                        _mlServicesTabPage.Enabled =
                        _okButton.Enabled =
                        _cancelButton.Enabled = false;
                    _runStopButton.Text = "Stop running";
                    _servicesAndLogTabControl.SelectedTab = _logTabPage;
                    _running = true;

                    _cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = _cancellationTokenSource.Token;

                    // Start processing
                    _mainTask = Task.Factory.StartNew(() =>
                        _settings.Process(cancellationToken), cancellationToken)

                    // Cleanup
                    .ContinueWith(task =>
                    {
                        var firstException = task.Exception?.InnerExceptions.FirstOrDefault();
                        if (firstException != null)
                        {
                            Action displayException = delegate { firstException.ExtractDisplay("ELI45821"); };
                            Invoke(displayException);
                        }

                        // Re-enable buttons
                        Action reEnable = delegate
                        {
                            _globalSettingsGroupBox.Enabled =
                                _mlServicesTabPage.Enabled =
                                _okButton.Enabled =
                                _cancelButton.Enabled = true;
                            _runStopButton.Text = "Run now";
                            _running = false;
                        };
                        if (InvokeRequired)
                        {
                            Invoke(reEnable);
                        }
                        else
                        {
                            reEnable();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45822");
            }
        }

        /// <summary>
        /// Handles the Click event of the AddButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleAddButton_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = null;
                MachineLearningService service = null;
                if (sender == _addCollectorButton)
                {
                    dgv = _dataCollectorsDataGridView;
                    service = new TrainingDataCollector();
                }
                else
                {
                    dgv = _modelTrainersDataGridView;
                    service = new MLModelTrainer();
                }
                var dataSource = (BindingList<MachineLearningService>)dgv.DataSource;
                int currentRow = dgv.CurrentCell?.RowIndex ?? -1;
                service.Container = _settings;
                dataSource.Insert(currentRow + 1, service);
                dgv.CurrentCell = dgv.Rows[currentRow + 1].Cells[0];
                dgv.Select();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45823").Display();
            }
        }

        /// <summary>
        /// Handles the RowEnter event of the dataGridView controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void HandleDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (sender == _dataCollectorsDataGridView)
                {
                    UpdateButtonStatesForCollectors(e.RowIndex);
                }
                else
                {
                    UpdateButtonStatesForTrainers(e.RowIndex);
                }
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45824").Display();
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
                DataGridView dgv = null;
                if (sender == _removeCollectorButton)
                {
                    dgv = _dataCollectorsDataGridView;
                }
                else
                {
                    dgv = _modelTrainersDataGridView;
                }
                var dataSource = (BindingList<MachineLearningService>)dgv.DataSource;
                int currentRow = dgv.CurrentCell?.RowIndex ?? -1;
                if (currentRow != -1)
                {
                    dataSource.RemoveAt(currentRow);
                    if (dataSource.Count > currentRow)
                    {
                        dgv.CurrentCell = dgv.Rows[currentRow].Cells[0];
                    }
                    else if (currentRow > 0)
                    {
                        dgv.CurrentCell = dgv.Rows[currentRow - 1].Cells[0];
                    }
                    dgv.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45825");
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
                DataGridView dgv = null;
                if (sender == _upCollectorButton)
                {
                    dgv = _dataCollectorsDataGridView;
                }
                else
                {
                    dgv = _modelTrainersDataGridView;
                }
                var dataSource = (BindingList<MachineLearningService>)dgv.DataSource;
                int currentRow = dgv.CurrentCell?.RowIndex ?? -1;
                if (currentRow > 0)
                {
                    var tmp = dataSource[currentRow];
                    dataSource.RemoveAt(currentRow);
                    dataSource.Insert(currentRow - 1, tmp);
                    dgv.CurrentCell = dgv.Rows[currentRow - 1].Cells[0];
                    dgv.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45826");
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
                DataGridView dgv = null;
                if (sender == _downCollectorButton)
                {
                    dgv = _dataCollectorsDataGridView;
                }
                else
                {
                    dgv = _modelTrainersDataGridView;
                }
                var dataSource = (BindingList<MachineLearningService>)dgv.DataSource;
                int currentRow = dgv.CurrentCell?.RowIndex ?? -1;
                if (currentRow >= 0 && currentRow < dataSource.Count)
                {
                    var tmp = dataSource[currentRow];
                    dataSource.RemoveAt(currentRow);
                    dataSource.Insert(currentRow + 1, tmp);
                    dgv.CurrentCell = dgv.Rows[currentRow + 1].Cells[0];
                    dgv.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45827");
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
                DataGridView dgv = null;
                if (sender == _duplicateCollectorButton)
                {
                    dgv = _dataCollectorsDataGridView;
                }
                else
                {
                    dgv = _modelTrainersDataGridView;
                }
                var dataSource = (BindingList<MachineLearningService>)dgv.DataSource;
                int currentRow = dgv.CurrentCell?.RowIndex ?? -1;
                if (currentRow >= 0)
                {
                    var copy = dataSource[currentRow].Duplicate();
                    copy.Container = _settings;
                    dataSource.Insert(currentRow + 1, copy);
                    dgv.CurrentCell = dgv.Rows[currentRow + 1].Cells[0];
                    dgv.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45828");
            }
        }

        /// <summary>
        /// Handles the Click event of the duplicateButton
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleModifyButton_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = null;
                if (sender == _modifyCollectorButton)
                {
                    dgv = _dataCollectorsDataGridView;
                }
                else
                {
                    dgv = _modelTrainersDataGridView;
                }
                var dataSource = (BindingList<MachineLearningService>)dgv.DataSource;
                int currentRow = dgv.CurrentCell?.RowIndex ?? -1;
                if (currentRow >= 0)
                {
                    var clone = (MachineLearningService)dataSource[currentRow].Clone();
                    clone.Container = _settings;

                    if (((IConfigSettings)clone).Configure())
                    {
                        dataSource[currentRow] = clone;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45829");
            }
        }

        /// <summary>
        /// Handles the CellMouseDoubleClick event of the _dataCollectorsDataGridView
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleDataCollectorsDataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    int columnIndex = e.ColumnIndex;
                    if (e.ColumnIndex < 0)
                    {
                        columnIndex = 0;
                    }
                    _dataCollectorsDataGridView.CurrentCell = _dataCollectorsDataGridView.Rows[e.RowIndex].Cells[columnIndex];
                    HandleModifyButton_Click(_modifyCollectorButton, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45830");
            }
        }

        /// <summary>
        /// Handles the CellMouseDoubleClick event of the _modelTrainersDataGridView
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleModelTrainersDataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    int columnIndex = e.ColumnIndex;
                    if (e.ColumnIndex < 0)
                    {
                        columnIndex = 0;
                    }
                    _modelTrainersDataGridView.CurrentCell = _modelTrainersDataGridView.Rows[e.RowIndex].Cells[columnIndex];
                    HandleModifyButton_Click(_modifyTrainerButton, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45831");
            }
        }

        /// <summary>
        /// Handles importing settings from a JSON file
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleImportButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "Training coordinator|*.coordinator|All files|*.*";
                    openDialog.Title = "Import";
                    if (!string.IsNullOrWhiteSpace(_rootDirectoryTextBox.Text)
                        && Directory.Exists(_rootDirectoryTextBox.Text))
                    {
                        openDialog.InitialDirectory = _rootDirectoryTextBox.Text;
                    }
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openDialog.FileName);
                        var newSettings = TrainingCoordinator.FromJson(json);

                        newSettings.RootDir = Path.GetDirectoryName(openDialog.FileName);

                        _settings.PropertyChanged -= HandleSettings_PropertyChanged;

                        // Apply loaded values through the UI
                        SetControlValues(newSettings);
                        ApplyControlValues();

                        // Not sure how to do lists besides directly copying the references...
                        _settings.SetModelTrainers(newSettings.ModelTrainers);
                        _settings.DataCollectors = newSettings.DataCollectors;
                        foreach (var service in _settings.Services)
                        {
                            service.Container = _settings;
                        }

                        _settings.PropertyChanged += HandleSettings_PropertyChanged;

                        // Check for missing model names and attribute sets and prompt to add them
                        ValidateModelNames();
                        ValidateAttributeSets();

                        UpdateButtonStatesForCollectors();
                        UpdateButtonStatesForTrainers();
                        Dirty = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45832");
            }
        }

        /// <summary>
        /// Checks-for/prompts-to-add missing referenced model names
        /// </summary>
        private void ValidateModelNames()
        {
            var existingModels = new HashSet<string>(_database.GetMLModels().GetKeys().ToIEnumerable<string>());
            var referencedModels = new HashSet<string>(_settings.Services
                .Select(s => s.QualifiedModelName)
                .Where(n => !string.IsNullOrWhiteSpace(n)));

            referencedModels.ExceptWith(existingModels);
            if (referencedModels.Any())
            {
                var result = MessageBox.Show(UtilityMethods.FormatCurrent(
                    $"The following ML Model Names do not exist. Add?",
                    $"\r\n{string.Join(Environment.NewLine, referencedModels)}"),
                    "Add missing model names", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                if (result == DialogResult.Yes)
                {
                    _settings.AddModels(referencedModels);
                }
            }
        }

        /// <summary>
        /// Checks-for/prompts-to-add missing referenced model names
        /// </summary>
        private void ValidateAttributeSets()
        {
            var attributeDBMgr = new AttributeDBMgr
            {
                FAMDB = _database
            };
            var existingAttributeSets = new HashSet<string>(attributeDBMgr.GetAllAttributeSetNames().GetKeys().ToIEnumerable<string>());
            var referencedAttributeSets = new HashSet<string>(_settings.DataCollectors
                .OfType<TrainingDataCollector>()
                .Select(s => s.AttributeSetName)
                .Where(n => !string.IsNullOrWhiteSpace(n)));

            referencedAttributeSets.ExceptWith(existingAttributeSets);
            if (referencedAttributeSets.Any())
            {
                var result = MessageBox.Show(UtilityMethods.FormatCurrent(
                    $"The following Attribute Sets do not exist. Add?",
                    $"\r\n{string.Join(Environment.NewLine, referencedAttributeSets)}"),
                    "Add missing attribute sets", MessageBoxButtons.YesNo,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1,MessageBoxOptions.ServiceNotification);
                if (result == DialogResult.Yes)
                {
                    foreach (var missingName in referencedAttributeSets)
                    {
                        attributeDBMgr.CreateNewAttributeSetName(missingName);
                    }
                }
            }
        }

        /// <summary>
        /// Handles exporting settings to a JSON file
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Training coordinator|*.coordinator|All files|*.*";
                    saveDialog.Title = "Export";
                    if (!string.IsNullOrWhiteSpace(_rootDirectoryTextBox.Text)
                        && Directory.Exists(_rootDirectoryTextBox.Text))
                    {
                        saveDialog.InitialDirectory = _rootDirectoryTextBox.Text;
                    }

                    var result = saveDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string json = _settings.ToJson();
                        File.WriteAllText(saveDialog.FileName, json);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45833");
            }
        }

        /// <summary>
        /// Scroll log to end to show most recent entries
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleServicesAndLogTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _logTextBox.SelectionStart = _logTextBox.TextLength;
                _logTextBox.ScrollToCaret();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45834");
            }
        }

        /// <summary>
        /// Sets the LastIDProcessed value to zero for every <see cref="MachineLearningService"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleResetProcessedStatusButton_Click(object sender, EventArgs e)
        {
            try
            {
                _settings.ResetProcessedStatus();
                Dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45993");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Sets the control values from the settings object.
        /// </summary>
        private void SetControlValues(TrainingCoordinator settings)
        {
            try
            {
                _suspendUpdatesToSettingsObject = true;

                _rootDirectoryTextBox.Text = settings.RootDir;
                _descriptionTextBox.Text = settings.Description;
                _schedulerControl.Value = settings.Schedule;
                _projectNameTextBox.Text = settings.ProjectName;
                _minimumRecordsRequiredNumericUpDown.Value = settings.MinimumNewRecordsRequiredForTraining;
                _deleteDataCheckBox.Checked = settings.DeleteMarkedMLData;
                _logTextBox.Text = settings.Log;
                _maxModelBackupsNumericUpDown.Value = Math.Max(settings.NumberOfBackupModelsToKeep, 0);

                // Scroll to end of log
                _logTextBox.SelectionStart = _logTextBox.TextLength;
                _logTextBox.ScrollToCaret();

                var collectors = new BindingList<MachineLearningService>(settings.DataCollectors);
                collectors.RaiseListChangedEvents = true;
                collectors.ListChanged += HandleValueChanged;
                _dataCollectorsDataGridView.DataSource = collectors;

                var trainers = new BindingList<MachineLearningService>(settings.ModelTrainers);
                trainers.RaiseListChangedEvents = true;
                trainers.ListChanged += HandleValueChanged;
                _modelTrainersDataGridView.DataSource = trainers;
            }
            finally
            {
                _suspendUpdatesToSettingsObject = false;
            }
        }

        private void ApplyControlValues()
        {
            _settings.RootDir = _rootDirectoryTextBox.Text;
            _settings.Description = _descriptionTextBox.Text;
            _settings.Schedule = _schedulerControl.Value;
            _settings.ProjectName = _projectNameTextBox.Text;
            _settings.MinimumNewRecordsRequiredForTraining = (int)_minimumRecordsRequiredNumericUpDown.Value;
            _settings.DeleteMarkedMLData = _deleteDataCheckBox.Checked;
            _settings.Log = _logTextBox.Text;
            _settings.NumberOfBackupModelsToKeep = (int)_maxModelBackupsNumericUpDown.Value;
        }

        /// <summary>
        /// Updates the state (enable/disable) of the up, down, remove and duplicate buttons.
        /// </summary>
        /// <param name="currentRowOption">The current row number in case update is being called before
        /// the current cell has changed (e.g., from RowEnter event).</param>
        void UpdateButtonStatesForCollectors(int? currentRowOption = null)
        {
            try
            {
                var currentRow = currentRowOption ?? _dataCollectorsDataGridView.CurrentCell?.RowIndex ?? -1;
                int rowCount = _dataCollectorsDataGridView.RowCount;

                if (currentRow == -1 || rowCount == 0)
                {
                    _upCollectorButton.Enabled =
                        _downCollectorButton.Enabled =
                        _removeCollectorButton.Enabled =
                        _modifyCollectorButton.Enabled =
                        _duplicateCollectorButton.Enabled = false;
                    return;
                }
                _removeCollectorButton.Enabled =
                    _modifyCollectorButton.Enabled =
                    _duplicateCollectorButton.Enabled = true;

                _upCollectorButton.Enabled = currentRow > 0;
                _downCollectorButton.Enabled = currentRow < rowCount - 1;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45835");
            }
        }

        /// <summary>
        /// Updates the state (enable/disable) of the up, down, remove and duplicate buttons.
        /// </summary>
        /// <param name="currentRowOption">The current row number in case update is being called before
        /// the current cell has changed (e.g., from RowEnter event).</param>
        void UpdateButtonStatesForTrainers(int? currentRowOption = null)
        {
            try
            {
                var currentRow = currentRowOption ?? _modelTrainersDataGridView.CurrentCell?.RowIndex ?? -1;
                int rowCount = _modelTrainersDataGridView.RowCount;

                if (currentRow == -1 || rowCount == 0)
                {
                    _upTrainerButton.Enabled =
                        _downTrainerButton.Enabled =
                        _removeTrainerButton.Enabled =
                        _modifyTrainerButton.Enabled =
                        _duplicateTrainerButton.Enabled = false;
                    return;
                }
                _removeTrainerButton.Enabled =
                    _modifyTrainerButton.Enabled =
                    _duplicateTrainerButton.Enabled = true;

                _upTrainerButton.Enabled = currentRow > 0;
                _downTrainerButton.Enabled = currentRow < rowCount - 1;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45836");
            }
        }

        #endregion Private Methods
    }
}