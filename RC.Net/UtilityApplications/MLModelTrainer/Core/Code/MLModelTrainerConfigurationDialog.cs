using Extract.ETL;
using Extract.FileActionManager.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.MLModelTrainer
{
    /// <summary>
    /// Dialog to configure and run NER training
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class MLModelTrainerConfigurationDialog : Form
    {
        #region Fields

        // Flag to short-circuit value-changed handler
        bool _suspendUpdatesToSettingsObject;

        MLModelTrainer _settings;
        bool _dirty;


        static readonly string _TITLE_TEXT = "ML Model trainer";
        static readonly string _TITLE_TEXT_DIRTY = "*" + _TITLE_TEXT;
        string _databaseServer;
        string _databaseName;
        FileProcessingDB _database;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether anything has been modified since loading
        /// </summary>
        bool Dirty
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

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a configuration dialog for an <see cref="MLModelTrainer"/>
        /// </summary>
        /// <param name="trainer">The instance to configure</param>
        /// <param name="databaseServer">The server to use to resolve MLModel.Names and AttributeSetNames</param>
        /// <param name="databaseName">The database to use to resolve MLModel.Names and AttributeSetNames</param>
        public MLModelTrainerConfigurationDialog(MLModelTrainer trainer, string databaseServer, string databaseName)
        {
            try
            {
                _settings = trainer;
                _databaseServer = databaseServer;
                _databaseName = databaseName;

                InitializeComponent();

                _lastIDProcessedNumericUpDown.Maximum = long.MaxValue;

                // Schedule is unused if this is a child of a training coordinator
                if (_settings.Container != null)
                {
                    tabControl1.TabPages.Remove(_scheduleTabPage);
                }

                SetControlValues();
            }
            catch (Exception ex)
            {
                _settings = new MLModelTrainer();
                ex.ExtractDisplay("ELI45109");
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

                try
                {
                    _database = new FileProcessingDB
                    {
                        DatabaseServer = _databaseServer,
                        DatabaseName = _databaseName
                    };
                }
                catch
                {
                    _database = null;
                    throw;
                }

                if (string.IsNullOrWhiteSpace(_databaseServer) || string.IsNullOrWhiteSpace(_databaseName))
                {
                    var result = _database.ShowSelectDB("Select database", false, false);
                    ExtractException.Assert("ELI45126", "No database configured", result);

                    _databaseServer = _database.DatabaseServer;
                    _databaseName = _database.DatabaseName;
                }

                var models = _database.GetMLModels().GetKeys().ToIEnumerable<string>().ToArray();
                _modelNameComboBox.Items.AddRange(models);

                _trainingCommandPathTagsButton.PathTags.AddTag(MLModelTrainer.DataFilePathTag, null);
                _trainingCommandPathTagsButton.PathTags.AddTag(MLModelTrainer.TempModelPathTag, null);

                _testingCommandPathTagsButton.PathTags.AddTag(MLModelTrainer.DataFilePathTag, null);
                _testingCommandPathTagsButton.PathTags.AddTag(MLModelTrainer.TempModelPathTag, null);

                _testingCommandPathTagsButton.PathTags.BuiltInTagFilter =
                    _trainingCommandPathTagsButton.PathTags.BuiltInTagFilter =
                    new[]
                    {
                        "<ComponentDataDir>",
                        SourceDocumentPathTags.CommonComponentsDir,
                        MLModelTrainer.DataFilePathTag,
                        MLModelTrainer.TempModelPathTag
                    };

                SetEnabledStates();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45110").Display();
                Close();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (DialogResult != DialogResult.OK)
                {
                    switch (this.PromptForSaveChanges(Dirty))
                    {
                        case DialogResult.Yes:
                            HandleOkButton_Click(this, e);
                            break;
                        case DialogResult.No:
                            HandleCancelButton_Click(this, e);
                            break;
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45995");
            }
            
            base.OnClosing(e);
        }
        #endregion Overrides

        #region Event Handlers


        /// <summary>
        /// Writes the field values to the settings object
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                var modelName = _modelNameComboBox.Text;
                _modelNameComboBox.Focus();
                ExtractException.Assert("ELI45127", "Model name is undefined", ValidateModel(), "Model name", modelName);

                _descriptionTextBox.Focus();
                ExtractException.Assert("ELI45673", "Description cannot be empty",
                    !string.IsNullOrWhiteSpace(_descriptionTextBox.Text));

                ApplySettings();

                Dirty = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45111");
                DialogResult = DialogResult.None;
            }
        }


        /// <summary>
        /// Closes the form without updating the settings object
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleCancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Update settings from UI controls
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendUpdatesToSettingsObject)
                {
                    return;
                }

                Dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45112");
            }
        }


        /// <summary>
        /// Shows an AddMLModel dialog
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleAddModelButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new AddMLModel(_database, _settings.ModelNamePrefix))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        var models = _database.GetMLModels().GetKeys().ToIEnumerable<string>().ToArray();
                        _modelNameComboBox.Items.Clear();
                        _modelNameComboBox.Items.AddRange(models);
                        _modelNameComboBox.SelectedItem = form.NewValue;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45133");
            }
        }

        /// <summary>
        /// Shows a ManageMLModels dialog
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Handle_ManageMLModelsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new EditTableData(_databaseServer, _databaseName, "MLModel"))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        var models = _database.GetMLModels().GetKeys().ToIEnumerable<string>().ToArray();
                        _modelNameComboBox.Items.Clear();
                        _modelNameComboBox.Items.AddRange(models);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45251");
            }
        }

        /// <summary>
        /// Enables/disabled the training/testing command controls
        /// </summary>
        private void Handle_ModelTypeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendUpdatesToSettingsObject)
                {
                    return;
                }

                Dirty = true;

                SetEnabledStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45709");
            }
        }

        /// <summary>
        /// Shows a ChangeAnswerDialog dialog
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleChangeAnswerButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Make sure settings are current
                ApplySettings();

                using (var form = new ChangeAnswerForm(_settings))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45851");
            }
        }

        /// <summary>
        /// After confirmation, marks for deletion all data associated with this model name
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleDeleteMLDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(UtilityMethods.FormatInvariant($"Mark all {_settings.QualifiedModelName} data for deletion?"),
                    "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0);
                if (result == DialogResult.OK)
                {
                    // Make sure settings are up to date
                    ApplySettings();

                    using (new TemporaryWaitCursor())
                    {
                        _settings.MarkAllDataForDeletion();
                        UtilityMethods.ShowMessageBox(UtilityMethods.FormatCurrent($"Marked all data for deletion"), "Success", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45971");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Sets the control values from the settings object.
        /// </summary>
        void SetControlValues()
        {
            try
            {
                _suspendUpdatesToSettingsObject = true;

                _nerModelTypeRadioButton.Checked = _settings.ModelType == TrainingDataCollector.ModelType.NamedEntityRecognition;
                _lmModelTypeRadioButton.Checked = _settings.ModelType == TrainingDataCollector.ModelType.LearningMachine;
                _modelNameComboBox.Text = _settings.QualifiedModelName;
                _trainingCommandTextBox.Text = _settings.TrainingCommand;
                _testingCommandTextBox.Text = _settings.TestingCommand;
                _modelDestinationPathTextBox.Text = _settings.ModelDestination;
                _lastIDProcessedNumericUpDown.Value = _settings.LastIDProcessed;
                _lastF1ScoreNumericUpDown.Value = (decimal)_settings.LastF1Score;
                _minF1ScoreNumericUpDown.Value = (decimal)_settings.MinimumF1Score;
                _allowableAccuracyDropNumericUpDown.Value = (decimal)_settings.AllowableAccuracyDrop;
                _maxTrainingRecordsNumericUpDown.Value = _settings.MaximumTrainingRecords;
                _maxTestingRecordsNumericUpDown.Value = _settings.MaximumTestingRecords;
                _markOldDataForDeletionCheckBox.Checked = _settings.MarkOldDataForDeletion;
                _emailAddressesTextBox.Text = _settings.EmailAddressesToNotifyOnFailure;
                _emailSubjectTextBox.Text = _settings.EmailSubject;
                _descriptionTextBox.Text = _settings.Description;
                _schedulerControl.Value = _settings.Schedule;

                Dirty = false;
            }
            finally
            {
                _suspendUpdatesToSettingsObject = false;
            }
        }

        /// <summary>
        /// Checks the FAM DB for model name existence
        /// </summary>
        /// <returns></returns>
        bool ValidateModel()
        {
            var modelName = _modelNameComboBox.Text;
            var definedModels = _database.GetMLModels().GetKeys().ToIEnumerable<string>();
            var valid = definedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
            if (!valid)
            {
                _modelNameComboBox.Focus();
            }

            return valid;
        }
        private void ApplySettings()
        {
            _settings.ModelType = _nerModelTypeRadioButton.Checked
                ? TrainingDataCollector.ModelType.NamedEntityRecognition
                : TrainingDataCollector.ModelType.LearningMachine;

            _settings.QualifiedModelName = _modelNameComboBox.Text;
            _settings.Description = _descriptionTextBox.Text;
            _settings.TrainingCommand = _trainingCommandTextBox.Text;
            _settings.TestingCommand = _testingCommandTextBox.Text;
            _settings.ModelDestination = _modelDestinationPathTextBox.Text;
            _settings.LastIDProcessed = (int)_lastIDProcessedNumericUpDown.Value;
            _settings.LastF1Score = (double)_lastF1ScoreNumericUpDown.Value;
            _settings.MinimumF1Score = (double)_minF1ScoreNumericUpDown.Value;
            _settings.AllowableAccuracyDrop = (double)_allowableAccuracyDropNumericUpDown.Value;
            _settings.MaximumTrainingRecords = (int)_maxTrainingRecordsNumericUpDown.Value;
            _settings.MaximumTestingRecords = (int)_maxTestingRecordsNumericUpDown.Value;
            _settings.MarkOldDataForDeletion = _markOldDataForDeletionCheckBox.Checked;
            _settings.EmailAddressesToNotifyOnFailure = _emailAddressesTextBox.Text;
            _settings.EmailSubject = _emailSubjectTextBox.Text;
            _settings.Schedule = _schedulerControl.Value;
        }

        private void SetEnabledStates()
        {
            if (_lmModelTypeRadioButton.Checked)
            {
                _modelPathLabel.Text = "LM file path";
                _trainingCommandTextBox.Enabled =
                    _trainingCommandPathTagsButton.Enabled =
                    _testingCommandTextBox.Enabled =
                    _testingCommandPathTagsButton.Enabled = false;
                _changeAnswerButton.Enabled = true;
            }
            else
            {
                _modelPathLabel.Text = "Destination path";
                _trainingCommandTextBox.Enabled =
                    _trainingCommandPathTagsButton.Enabled =
                    _testingCommandTextBox.Enabled =
                    _testingCommandPathTagsButton.Enabled = true;
                _changeAnswerButton.Enabled = false;
            }
        }

        #endregion Private Methods
    }
}