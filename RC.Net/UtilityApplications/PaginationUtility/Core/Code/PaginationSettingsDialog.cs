using Extract.Utilities;
using Extract.UtilityApplications.PaginationUtility.Properties;
using System;
using System.IO;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class PaginationSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        ConfigSettings<Settings> _config;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSettingsDialog"/> class.
        /// </summary>
        /// <param name="config"></param>
        public PaginationSettingsDialog(ConfigSettings<Settings> config)
        {
            try
            {
                InitializeComponent();

                _config = config;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35498");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [read only].
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if [read only]; otherwise, <see langword="false"/>.
        /// </value>
        public bool ReadOnly
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Apply setting values to the UI.
                if (_config != null)
                {
                    LoadSettings(_config);

                    if (ReadOnly)
                    {
                        _inputFolderTextBox.Enabled = false;
                        _inputFolderBrowseButton.Enabled = false;
                        _fileFilterComboBox.Enabled = false;
                        _includeSubfoldersCheckBox.Enabled = false;
                        _inputPageCountUpDown.Enabled = false;
                        _outputFolderTextBox.Enabled = false;
                        _outputFolderBrowseButton.Enabled = false;
                        _randomizeOutputFileNameCheckBox.Enabled = false;
                        _preserveOutputSubFoldersCheckBox.Enabled = false;
                        _moveInputDocumentRadioButton.Enabled = false;
                        _processedDocumentFolderTextBox.Enabled = false;
                        _processedDocumentFolderBrowseButton.Enabled = false; 
                        _deleteInputDocumentRadioButton.Enabled = false;

                        _exportSettingsButton.Visible = false;
                        _importSettingsButton.Visible = false;
                        _okButton.Visible = false;
                        _cancelButton.Text = "OK";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35499");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the HandleExportSettingsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleExportSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Configuration files (*.config)|*.config";
                    openFileDialog.FilterIndex = 0;
                    openFileDialog.AddExtension = true;
                    openFileDialog.Multiselect = false;
                    openFileDialog.CheckFileExists = false;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.DefaultExt = ".config";

                    while (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string newConfigName = openFileDialog.FileName;
                        if (File.Exists(newConfigName))
                        {
                            DialogResult response = MessageBox.Show("Overwrite existing file?",
                                "Overwrite?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1, 0);

                            if (response == DialogResult.Cancel)
                            {
                                continue;
                            }
                        }

                        var targetConfig = new ConfigSettings<Settings>(newConfigName);
                        SaveSettings(targetConfig);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35500");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleImportSettingsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleImportSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Configuration files (*.config)|*.config";
                    openFileDialog.FilterIndex = 0;
                    openFileDialog.AddExtension = true;
                    openFileDialog.Multiselect = false;
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.DefaultExt = ".config";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var sourceConfig =
                            new ConfigSettings<Settings>(openFileDialog.FileName, false, false);
                        LoadSettings(sourceConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35501");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleMoveInputDocumentRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _processedDocumentFolderTextBox.Enabled = _moveInputDocumentRadioButton.Checked;
                _processedDocumentFolderBrowseButton.Enabled = _moveInputDocumentRadioButton.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35502");
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the HandleIncludeSubfoldersCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleIncludeSubfoldersCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _preserveOutputSubFoldersCheckBox.Enabled = _includeSubfoldersCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35503");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleOkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (!ReadOnly && WarnIfInvalid())
                {
                    return;
                }

                SaveSettings(_config);

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35504");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="config"></param>
        void LoadSettings(ConfigSettings<Settings> config)
        {
            _inputFolderTextBox.Text = config.Settings.InputFolder;
            _fileFilterComboBox.Text = config.Settings.FileFilter;
            _includeSubfoldersCheckBox.Checked = config.Settings.IncludeSubfolders;
            _inputPageCountUpDown.Value = config.Settings.InputPageCount;
            _outputFolderTextBox.Text = config.Settings.OutputFolder;
            _randomizeOutputFileNameCheckBox.Checked = config.Settings.RandomizeOutputFileName;
            _preserveOutputSubFoldersCheckBox.Checked = config.Settings.PreserveSubFoldersInOutput;
            _preserveOutputSubFoldersCheckBox.Enabled = _includeSubfoldersCheckBox.Checked;
            _moveInputDocumentRadioButton.Checked = !config.Settings.DeleteProcessedFiles;
            _deleteInputDocumentRadioButton.Checked = config.Settings.DeleteProcessedFiles;
            _processedDocumentFolderTextBox.Text = config.Settings.ProcessedFileFolder;
            _processedDocumentFolderTextBox.Enabled = _moveInputDocumentRadioButton.Checked;
            _processedDocumentFolderBrowseButton.Enabled = _moveInputDocumentRadioButton.Checked;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="config">The config.</param>
        void SaveSettings(ConfigSettings<Settings> config)
        {
            config.Settings.InputFolder = _inputFolderTextBox.Text;
            config.Settings.FileFilter = _fileFilterComboBox.Text;
            config.Settings.IncludeSubfolders = _includeSubfoldersCheckBox.Checked;
            config.Settings.InputPageCount = Decimal.ToInt32(_inputPageCountUpDown.Value);
            config.Settings.OutputFolder = _outputFolderTextBox.Text;
            config.Settings.RandomizeOutputFileName = _randomizeOutputFileNameCheckBox.Checked;
            config.Settings.PreserveSubFoldersInOutput = _preserveOutputSubFoldersCheckBox.Checked;
            config.Settings.DeleteProcessedFiles = _deleteInputDocumentRadioButton.Checked;
            config.Settings.ProcessedFileFolder = _processedDocumentFolderTextBox.Text;
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            ExtractException.Assert("ELI35505",
                "Pagination settings have not been provided.", _config != null);

            if (!Directory.Exists(_inputFolderTextBox.Text))
            {
                _inputFolderTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a valid input directory.",
                    "Invalid settings", false);
                return true;
            }

            if (!Directory.Exists(_outputFolderTextBox.Text))
            {
                _processedDocumentFolderTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a valid output directory.",
                    "Invalid settings", false);
                return true;
            }

            if (_moveInputDocumentRadioButton.Checked &&
                !Directory.Exists(_processedDocumentFolderTextBox.Text))
            {
                _processedDocumentFolderTextBox.Focus();
                UtilityMethods.ShowMessageBox(
                    "Please specify a valid directory for processed documents to be placed.",
                    "Invalid settings", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
