using AttributeDbMgrComponentsLib;
using Extract.FileActionManager.Forms;
using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.NERDataCollector
{
    /// <summary>
    /// Dialog to configure and run NER data collection
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class NERDataCollectorConfigurationDialog : Form
    {
        #region Fields

        // Flag to short-circuit value-changed handler
        private bool _suspendUpdatesToSettingsObject;

        private NERDataCollector _settings;
        private bool _dirty;

        private static readonly string _TITLE_TEXT = "NER training data collector";
        private static readonly string _TITLE_TEXT_DIRTY = "*" + _TITLE_TEXT;
        private string _databaseServer;
        private string _databaseName;
        FileProcessingDB _database;

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

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a configuration dialogue for an <see cref="NERDataCollector"/>
        /// </summary>
        /// <param name="collector">The instance to configure</param>
        /// <param name="databaseServer">The server to use to resolve MLModel.Names and AttributeSetNames</param>
        /// <param name="databaseName">The database to use to resolve MLModel.Names and AttributeSetNames</param>
        public NERDataCollectorConfigurationDialog(NERDataCollector collector, string databaseServer, string databaseName)
        {
            try
            {
                _settings = collector;
                _databaseServer = databaseServer;
                _databaseName = databaseName;

                InitializeComponent();

                SetControlValues();
            }
            catch (Exception ex)
            {
                _settings = new NERDataCollector();
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
                var modelName = _modelNameComboBox.Text;
                ExtractException.Assert("ELI45127", "Model name is undefined", ValidateModel(), "Model name", modelName);
                _settings.ModelName = modelName;

                var attributeSet = _attributeSetNameComboBox.Text;
                ExtractException.Assert("ELI45131", "Attribute set is undefined", ValidateAttributeSet(), "Attribute set", attributeSet);
                _settings.AttributeSetName = attributeSet;

                _settings.AnnotatorSettingsPath = _annotatorSettingsPathTextBox.Text;
                _settings.LastIDProcessed = (int)_lastIDProcessedNumericUpDown.Value;

                Dirty = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45047");
            }
        }

        /// <summary>
        /// Closes the form without updating the settings object
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleCancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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

                Dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45048");
            }
        }

        /// <summary>
        /// Shows an AddMLModel dialog
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleAddModelButton_Click(object sender, EventArgs e)
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
                ex.ExtractDisplay("ELI45132");
            }
        }

        /// <summary>
        /// Shows a ManageMLModels dialog
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Handle_ManageMLModelsButton_Click(object sender, EventArgs e)
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

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Sets the control values from the settings object.
        /// </summary>
        private void SetControlValues()
        {
            try
            {
                _suspendUpdatesToSettingsObject = true;

                _attributeSetNameComboBox.Text = _settings.AttributeSetName;
                _modelNameComboBox.Text = _settings.ModelName;
                _annotatorSettingsPathTextBox.Text = _settings.AnnotatorSettingsPath;
                _lastIDProcessedNumericUpDown.Value = _settings.LastIDProcessed;

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

        #endregion Private Methods
    }
}