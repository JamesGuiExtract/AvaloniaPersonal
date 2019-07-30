using AttributeDbMgrComponentsLib;
using Extract.ETL;
using Extract.FileActionManager.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.MachineLearning
{
    /// <summary>
    /// Dialog to configure and run training data collection
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TrainingDataCollectorConfigurationDialog : Form
    {
        #region Fields

        // Flag to short-circuit value-changed handler
        bool _suspendUpdatesToSettingsObject;

        TrainingDataCollector _settings;
        bool _dirty;

        static readonly string _TITLE_TEXT = "Training data collector";
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
        /// Creates a configuration dialog for an <see cref="TrainingDataCollector"/>
        /// </summary>
        /// <param name="collector">The instance to configure</param>
        /// <param name="databaseServer">The server to use to resolve MLModel.Names and AttributeSetNames</param>
        /// <param name="databaseName">The database to use to resolve MLModel.Names and AttributeSetNames</param>
        public TrainingDataCollectorConfigurationDialog(TrainingDataCollector collector, string databaseServer, string databaseName)
        {
            try
            {
                _settings = collector;
                _databaseServer = databaseServer;
                _databaseName = databaseName;

                InitializeComponent();

                _lastIDProcessedNumericUpDown.Maximum = long.MaxValue;
                _limitProcessingByDateNumericUpDown.Maximum = int.MaxValue;

                // Schedule is unused if this is a child of a training coordinator
                if (_settings.Container != null)
                {
                    _tabControl.TabPages.Remove(_scheduleTabPage);
                }

                SetControlValues();
            }
            catch (Exception ex)
            {
                _settings = new TrainingDataCollector();
                ex.ExtractDisplay("ELI45043");
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
                    ExtractException.Assert("ELI45130", "No database configured", result);

                    _databaseServer = _database.DatabaseServer;
                    _databaseName = _database.DatabaseName;
                }

                var models = _database.GetMLModels().GetKeys().ToIEnumerable<string>().ToArray();
                _modelNameComboBox.Items.AddRange(models);

                var attributeDBMgr = new AttributeDBMgr
                {
                    FAMDB = _database
                };
                var attributeSets = attributeDBMgr.GetAllAttributeSetNames().GetKeys().ToIEnumerable<string>().ToArray();
                _attributeSetNameComboBox.Items.AddRange(attributeSets);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45045").Display();
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
                // Check for dirty changes unless the OK button is the source of the close
                if (DialogResult != DialogResult.OK)
                {
                    switch (this.PromptForSaveChanges(Dirty))
                    {
                        case DialogResult.Yes:
                            HandleOkButton_Click(this, e);
                            break;
                        case DialogResult.No:
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
                ex.ExtractDisplay("ELI45999");
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
                _descriptionTextBox.Focus();
                ExtractException.Assert("ELI45675", "Description cannot be empty.",
                    !string.IsNullOrWhiteSpace(_descriptionTextBox.Text));

                var modelName = _modelNameComboBox.Text;
                _modelNameComboBox.Focus();
                ExtractException.Assert("ELI45127", "Model name is undefined", ValidateModel(), "Model name", modelName);

                var attributeSet = _attributeSetNameComboBox.Text;
                _attributeSetNameComboBox.Focus();
                ExtractException.Assert("ELI45131", "Attribute set is undefined", ValidateAttributeSet(), "Attribute set", attributeSet);

                ApplySettings();

                Dirty = false;
                DialogResult = DialogResult.OK;

                // This is needed for running the configuration from the command line
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45047");
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

            // This is needed for running the configuration from the command line
            Close();
        }

        /// <summary>
        /// Set dirty flag
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
                ex.ExtractDisplay("ELI45048");
            }
        }

        /// <summary>
        /// Enable/disable dependant controls. Set dirty flag
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_suspendUpdatesToSettingsObject)
                {
                    return;
                }

                Dirty = true;
                SetControlStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45770");
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
                ex.ExtractDisplay("ELI45132");
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
                ex.ExtractDisplay("ELI45267");
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
                // Make sure settings are up to date
                ApplySettings();

                using (var form = new ChangeAnswerForm(_settings))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45852");
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
                ex.ExtractDisplay("ELI45965");
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

                _attributeSetNameComboBox.Text = _settings.AttributeSetName;
                _modelNameComboBox.Text = _settings.QualifiedModelName;
                _lastIDProcessedNumericUpDown.Value = _settings.LastIDProcessed;
                _lmModelTypeRadioButton.Checked = _settings.ModelType == ModelType.LearningMachine;
                _nerModelTypeRadioButton.Checked = _settings.ModelType == ModelType.NamedEntityRecognition;
                _dataGeneratorPathTextBox.Text = _settings.DataGeneratorPath;
                _descriptionTextBox.Text = _settings.Description;
                _schedulerControl.Value = _settings.Schedule;

                _overrideTrainingTestingSplitCheckBox.Checked = _settings.OverrideTrainingTestingSplit;
                _trainingPercentageNumericUpDown.Value = _settings.TrainingPercent;

                // Use a reasonable default if zero days are specified
                int days = _settings.LimitProcessingToMostRecent.Days;
                _limitProcessingByDateNumericUpDown.Value = days > 0 ? days : 30;

                if (_settings.UseAttributeSetForExpectedValues)
                {
                    _useAttributeSetForExpectedsRadioButton.Checked = true;
                }
                else
                {
                    _useVoaFileForExpectedsRadioButton.Checked = true;
                }

                if (_settings.RunRuleSetForCandidateOrFeatures)
                {
                    _runRulesetForFeaturesRadioButton.Checked = true;
                }
                else
                {
                    _useVoaFileForFeaturesRadioButton.Checked = true;
                }
                _featureRulesetTextBox.Text = _settings.FeatureRuleSetPath;
                _runRulesetIfVoaIsMissingCheckBox.Checked = _settings.RunRuleSetIfVoaIsMissing;

                SetControlStates();

                Dirty = false;

            }
            finally
            {
                _suspendUpdatesToSettingsObject = false;
            }
        }

        /// <summary>
        /// Enables/Disables controls based on configured options
        /// </summary>
        void SetControlStates()
        {
            _trainingPercentageNumericUpDown.Enabled = _overrideTrainingTestingSplitCheckBox.Checked;
            _featuresGroupBox.Enabled =
                _changeAnswerButton.Enabled = _lmModelTypeRadioButton.Checked;
            _featureRulesetTextBox.Enabled =
                _featureRulesetBrowseButton.Enabled =
                _runRulesetForFeaturesRadioButton.Checked || _runRulesetIfVoaIsMissingCheckBox.Checked;
            _runRulesetIfVoaIsMissingCheckBox.Enabled = !_runRulesetForFeaturesRadioButton.Checked;
        }

        /// <summary>
        /// Checks the FAM DB for model name existence
        /// </summary>
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

        /// <summary>
        /// Checks the FAM DB for attribute set existence
        /// </summary>
        bool ValidateAttributeSet()
        {
            var attributeSet = _attributeSetNameComboBox.Text;
            var attributeDBMgr = new AttributeDBMgr
            {
                FAMDB = _database
            };
            var attributeSets = attributeDBMgr.GetAllAttributeSetNames().GetKeys().ToIEnumerable<string>().ToArray();
            var valid = attributeSets.Contains(attributeSet, StringComparer.OrdinalIgnoreCase);
            if (!valid)
            {
                _attributeSetNameComboBox.Focus();
            }

            return valid;
        }

        private void ApplySettings()
        {
            _settings.Description = _descriptionTextBox.Text;
            _settings.QualifiedModelName = _modelNameComboBox.Text;
            _settings.AttributeSetName = _attributeSetNameComboBox.Text;
            _settings.LastIDProcessed = (int)_lastIDProcessedNumericUpDown.Value;

            if (_lmModelTypeRadioButton.Checked)
            {
                _settings.ModelType = ModelType.LearningMachine;
            }
            else
            {
                _settings.ModelType = ModelType.NamedEntityRecognition;
            }
            _settings.DataGeneratorPath = _dataGeneratorPathTextBox.Text;

            _settings.Schedule = _schedulerControl.Value;

            _settings.OverrideTrainingTestingSplit = _overrideTrainingTestingSplitCheckBox.Checked;
            _settings.TrainingPercent = (int)_trainingPercentageNumericUpDown.Value;
            _settings.LimitProcessingToMostRecent = TimeSpan.FromDays((double)_limitProcessingByDateNumericUpDown.Value);
            _settings.UseAttributeSetForExpectedValues = _useAttributeSetForExpectedsRadioButton.Checked;
            _settings.RunRuleSetForCandidateOrFeatures = _runRulesetForFeaturesRadioButton.Checked;
            _settings.FeatureRuleSetPath = _featureRulesetTextBox.Text;
            _settings.RunRuleSetIfVoaIsMissing = _runRulesetIfVoaIsMissingCheckBox.Checked;
        }

        #endregion Private Methods
    }
}