using Extract.FileActionManager.Forms;
using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.NERTrainer
{
    /// <summary>
    /// Dialog to configure and run NER training
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class NERTrainerConfigurationDialog : Form
    {
        #region Fields

        // Flag to short-circuit value-changed handler
        bool _suspendUpdatesToSettingsObject;

        NERTrainer _settings;
        bool _dirty;


        static readonly string _TITLE_TEXT = "NER trainer";
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
        /// Creates a configuration dialog for an <see cref="NERTrainer"/>
        /// </summary>
        /// <param name="trainer">The instance to configure</param>
        /// <param name="databaseServer">The server to use to resolve MLModel.Names and AttributeSetNames</param>
        /// <param name="databaseName">The database to use to resolve MLModel.Names and AttributeSetNames</param>
        public NERTrainerConfigurationDialog(NERTrainer trainer, string databaseServer, string databaseName)
        {
            try
            {
                _settings = trainer;
                _databaseServer = databaseServer;
                _databaseName = databaseName;

                InitializeComponent();

                _lastIDProcessedNumericUpDown.Maximum = long.MaxValue;

                SetControlValues();
            }
            catch (Exception ex)
            {
                _settings = new NERTrainer();
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

                _trainingCommandPathTagsButton.PathTags.AddTag(NERTrainer.DataFilePathTag, null);
                _trainingCommandPathTagsButton.PathTags.AddTag(NERTrainer.TempModelPathTag, null);

                _testingCommandPathTagsButton.PathTags.AddTag(NERTrainer.DataFilePathTag, null);
                _testingCommandPathTagsButton.PathTags.AddTag(NERTrainer.TempModelPathTag, null);

                _testingCommandPathTagsButton.PathTags.BuiltInTagFilter =
                    _trainingCommandPathTagsButton.PathTags.BuiltInTagFilter =
                    new[] { SourceDocumentPathTags.CommonComponentsDir, NERTrainer.DataFilePathTag, NERTrainer.TempModelPathTag };

                _modelDestinationPathTagsButton.PathTags.BuiltInTagFilter =
                    new[] { SourceDocumentPathTags.CommonComponentsDir };
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45110").Display();
                Close();
            }
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

                _settings.ModelName = modelName;

                _settings.TrainingCommand = _trainingCommandTextBox.Text;
                _settings.TestingCommand = _testingCommandTextBox.Text;
                _settings.ModelDestination = _modelDestinationPathTextBox.Text;
                _settings.LastIDProcessed = (int)_lastIDProcessedNumericUpDown.Value;
                _settings.LastF1Score = (double)_lastF1ScoreNumericUpDown.Value;
                _settings.MinimumF1Score = (double)_minF1ScoreNumericUpDown.Value;
                _settings.AllowableAccuracyDrop = (double)_allowableAccuracyDropNumericUpDown.Value;
                _settings.MaximumTrainingDocuments = (int)_maxTrainingDocsNumericUpDown.Value;
                _settings.MaximumTestingDocuments = (int)_maxTestingDocsNumericUpDown.Value;
                _settings.EmailAddressesToNotifyOnFailure = _emailAddressesTextBox.Text;
                _settings.EmailSubject = _emailSubjectTextBox.Text;
                _settings.Description = _descriptionTextBox.Text;
                _settings.Schedule = _schedulerControl.Value;

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
            Close();
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
                using (var form = new AddMLModel(_database))
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

                _modelNameComboBox.Text = _settings.ModelName;
                _trainingCommandTextBox.Text = _settings.TrainingCommand;
                _testingCommandTextBox.Text = _settings.TestingCommand;
                _modelDestinationPathTextBox.Text = _settings.ModelDestination;
                _lastIDProcessedNumericUpDown.Value = _settings.LastIDProcessed;
                _lastF1ScoreNumericUpDown.Value = (decimal)_settings.LastF1Score;
                _minF1ScoreNumericUpDown.Value = (decimal)_settings.MinimumF1Score;
                _allowableAccuracyDropNumericUpDown.Value = (decimal)_settings.AllowableAccuracyDrop;
                _maxTrainingDocsNumericUpDown.Value = _settings.MaximumTrainingDocuments;
                _maxTestingDocsNumericUpDown.Value = _settings.MaximumTestingDocuments;
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

        #endregion Private Methods
    }
}