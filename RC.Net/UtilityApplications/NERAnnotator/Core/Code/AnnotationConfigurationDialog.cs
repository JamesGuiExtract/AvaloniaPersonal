using Extract.AttributeFinder.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace Extract.UtilityApplications.NERAnnotator
{
    /// <summary>
    /// Dialog to configure and run NER data annotation
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class AnnotationConfigurationDialog : Form
    {
        #region Fields

        // Don't update the settings object if in the process of initializing them from the settings object
        private bool _suspendUpdatesToSettingsObject;

        private Settings _settings;
        private string _fileName;
        private BindingList<EntityDefinition> _entityDefinitions;
        private bool _dirty;


        private static readonly string _TITLE_TEXT = "{0} - NER Annotator";
        private static readonly string _TITLE_TEXT_DIRTY = "*{0} - NER Annotator";
        private static readonly string _NEW_FILE_NAME = "[Unsaved file]";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LearningMachineConfiguration" /> is dirty.
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
                    ? string.Format(CultureInfo.CurrentCulture, _TITLE_TEXT_DIRTY, Path.GetFileName(_fileName))
                    : string.Format(CultureInfo.CurrentCulture, _TITLE_TEXT, Path.GetFileName(_fileName));
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationConfigurationDialog"/>
        /// class.
        /// </summary>
        /// <param name="settingsFilePath">The <see cref="AnnotationConfigurationDialog"/> instance to configure.
        /// </param>
        public AnnotationConfigurationDialog(string settingsFilePath = null)
        {
            try
            {
                if (settingsFilePath != null)
                {
                    try
                    {
                        _settings = Settings.LoadFrom(settingsFilePath);
                        _fileName = settingsFilePath;
                    }
                    catch (Exception ex)
                    {
                        _fileName = _NEW_FILE_NAME;
                        _settings = new Settings();
                        ex.ExtractDisplay("ELI44905");
                    }
                }
                else
                {
                    _fileName = _NEW_FILE_NAME;
                    _settings = new Settings();
                }

                InitializeComponent();

                // Initialize the DataGridView
                _entityDefinitionDataGridView.AutoGenerateColumns = false;

                // Add Category column
                DataGridViewColumn column = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Category",
                    Name = "Category",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                };
                _entityDefinitionDataGridView.Columns.Add(column);

                // Add as XPath check-box
                column = new DataGridViewCheckBoxColumn
                {
                    DataPropertyName = "CategoryIsXPath",
                    Name = "Category is XPath",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
                };
                _entityDefinitionDataGridView.Columns.Add(column);

                // Add RootQuery column
                column = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "RootQuery",
                    Name = "Root query",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                };
                _entityDefinitionDataGridView.Columns.Add(column);

                // Add ValueQuery column
                column = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "ValueQuery",
                    Name = "Value query",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                };
                _entityDefinitionDataGridView.Columns.Add(column);

                SetControlValues();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44874");
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
                ex.AsExtract("ELI44873").Display();
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
                if (_entityDefinitionDataGridView.IsCurrentCellInEditMode)
                {
                    _entityDefinitionDataGridView.EndEdit();
                }

                e.Cancel = !PromptForDirtyFile();

                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44872");
            }
        }

        /// <summary>
        /// Processes a command key
        /// </summary>
        /// <param name="msg">A <see cref="Message"/>, passed by reference, that represents the Win32
        /// message to process.</param>
        /// <param name="keyData">One of the Keys values that represents the key to process.</param>
        /// <returns><see langword="true"/> if the keystroke was processed and consumed by the control; otherwise,
        /// <see langword="false"/> to allow further processing.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // Ctrl+S = Save
                if (keyData == (Keys.Control | Keys.S))
                {
                    saveMachineButton.Focus();
                    HandleSaveButton_Click(this, EventArgs.Empty);
                    return true;
                }

                // Ctrl+O = Open
                if (keyData == (Keys.Control | Keys.O))
                {
                    openMachineButton.Focus();
                    HandleOpenButton_Click(this, EventArgs.Empty);
                    return true;
                }
            }
            catch (Exception e)
            {
                e.ExtractDisplay("ELI44871");
            }

            return base.ProcessCmdKey(ref msg, keyData);
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
                int currentRow = _entityDefinitionDataGridView.CurrentCell?.RowIndex ?? -1;
                _entityDefinitions.Insert(currentRow + 1, new EntityDefinition());
                _entityDefinitionDataGridView.CurrentCell
                    = _entityDefinitionDataGridView.Rows[currentRow + 1].Cells[0];
                _entityDefinitionDataGridView.Select();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI44875").Display();
            }
        }

        /// <summary>
        /// Handles the RowEnter event of the _entityDefinitionDataGridView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void HandleEntityDefinitionDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                UpdateButtonStates(e.RowIndex);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI44876").Display();
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
                int currentRow = _entityDefinitionDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow != -1)
                {
                    _entityDefinitions.RemoveAt(currentRow);
                    if (_entityDefinitions.Count > currentRow)
                    {
                        _entityDefinitionDataGridView.CurrentCell = _entityDefinitionDataGridView.Rows[currentRow].Cells[0];
                    }
                    else if (currentRow > 0)
                    {
                        _entityDefinitionDataGridView.CurrentCell = _entityDefinitionDataGridView.Rows[currentRow - 1].Cells[0];
                    }
                    _entityDefinitionDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44877");
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
                int currentRow = _entityDefinitionDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow > 0)
                {
                    var tmp = _entityDefinitions[currentRow];
                    _entityDefinitions.RemoveAt(currentRow);
                    _entityDefinitions.Insert(currentRow - 1, tmp);
                    _entityDefinitionDataGridView.CurrentCell = _entityDefinitionDataGridView.Rows[currentRow - 1].Cells[0];
                    _entityDefinitionDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44878");
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
                int currentRow = _entityDefinitionDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow >= 0 && currentRow < _entityDefinitions.Count)
                {
                    var tmp = _entityDefinitions[currentRow];
                    _entityDefinitions.RemoveAt(currentRow);
                    _entityDefinitions.Insert(currentRow + 1, tmp);
                    _entityDefinitionDataGridView.CurrentCell = _entityDefinitionDataGridView.Rows[currentRow + 1].Cells[0];
                    _entityDefinitionDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44879");
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
                int currentRow = _entityDefinitionDataGridView.CurrentCell?.RowIndex ?? -1;
                if (currentRow >= 0)
                {
                    _entityDefinitions.Insert(currentRow + 1, _entityDefinitions[currentRow].ShallowClone());
                    _entityDefinitionDataGridView.CurrentCell = _entityDefinitionDataGridView.Rows[currentRow + 1].Cells[0];
                    _entityDefinitionDataGridView.Select();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44880");
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
                using (var win = new AnnotationStatus(_settings))
                {
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44881");
            }
        }

        /// <summary>
        /// Handles the click event for the save button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleSaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileName.Equals(_NEW_FILE_NAME, StringComparison.Ordinal))
                {
                    HandleSaveAsButton_Click(sender, e);
                }
                else
                {
                    Cursor.Current = Cursors.WaitCursor;
                    _settings.SaveTo(_fileName);
                    Dirty = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44882");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the click event for the save-as button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleSaveAsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Annotator settings|*.annotator|All files|*.*";
                    if (!_fileName.Equals(_NEW_FILE_NAME, StringComparison.Ordinal))
                    {
                        saveDialog.FileName = Path.GetFileName(_fileName);
                        saveDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }

                    var result = saveDialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        _settings.SaveTo(saveDialog.FileName);
                        _workingDirTextBox.Text = _settings.WorkingDir;
                        _fileName = saveDialog.FileName;
                        Dirty = false;
                    }
                    else if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        // Set Cancel in case this is called from PromptForDirtyFile()
                        if (e is CancelEventArgs cancelEventArgs)
                        {
                            cancelEventArgs.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44883");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the click event for the 'new' button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleNewButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    return;
                }

                _fileName = _NEW_FILE_NAME;
                _settings = new Settings();
                SetControlValues();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44884");
            }
        }

        /// <summary>
        /// Handles the click event for the open button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleOpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    return;
                }

                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "Annotator settings|*.annotator|All files|*.*";
                    if (!_fileName.Equals(_NEW_FILE_NAME, StringComparison.Ordinal))
                    {
                        openDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }
                    if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        _settings = Settings.LoadFrom(_fileName);
                        _fileName = openDialog.FileName;
                        SetControlValues();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44885");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
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

                Dirty = true;

                _settings.WorkingDir = _workingDirTextBox.Text;
                _settings.TrainingInput = _trainingInputTextBox.Text;
                _settings.TestingInput = _testingInputTextBox.Text;
                if (_randomlyTakeFromTrainingSetRadioButton.Checked)
                {
                    _settings.TestingSet = TestingSetType.RandomlyPickedFromTrainingSet;
                }
                else
                {
                    _settings.TestingSet = TestingSetType.Specified;
                }

                if (int.TryParse(_percentToUseForTestingSetTextBox.Text, out int val))
                {
                    _settings.PercentToUseForTestingSet = val;
                }
                if (int.TryParse(_randomSeedForSetDivisionTextBox.Text, out val))
                {
                    _settings.RandomSeedForSetDivision = val;
                }
                else
                {
                    _settings.RandomSeedForSetDivision = null;
                }


                _settings.TypesVoaFunction = _typesVoaFunctionTextBox.Text;
                _settings.OutputFileBaseName = _outputFileBaseNameTextBox.Text;
                _settings.OutputSeparateFileForEachCategory = _outputSeparateFileForEachCategoryCheckBox.Checked;
                _settings.FailIfOutputFileExists = _failIfOutputFileExistsCheckBox.Checked;

                if (int.TryParse(_randomSeedForPageInclusionTextBox.Text, out val))
                {
                    _settings.RandomSeedForPageInclusion = val;
                }
                else
                {
                    _settings.RandomSeedForPageInclusion = null;
                }

                if (int.TryParse(_percentNonInterestingPagesToIncludeTextBox.Text, out val))
                {
                    _settings.PercentUninterestingPagesToInclude = val;
                }

                _settings.Format = NamedEntityRecognizer.OpenNLP;

                _settings.SplitIntoSentences = _splitIntoSentencesCheckBox.Checked;
                _settings.SentenceDetectionModelPath = _sentenceDetectorPathTextBox.Text;
                if (_whitespaceTokenizerRadioButton.Checked)
                {
                    _settings.TokenizerType = OpenNlpTokenizer.WhiteSpaceTokenizer;
                }
                else if (_simpleTokenizerRadioButton.Checked)
                {
                    _settings.TokenizerType = OpenNlpTokenizer.SimpleTokenizer;
                }
                else
                {
                    _settings.TokenizerType = OpenNlpTokenizer.LearnableTokenizer;
                }

                _settings.TokenizerModelPath = _tokenizerPathTextBox.Text;
                _settings.FKBVersion = _fkbVersionTextBox.Text;

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44886");
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
                _percentToUseForTestingSetTextBox.Enabled =
                    _randomSeedForSetDivisionTextBox.Enabled = _randomlyTakeFromTrainingSetRadioButton.Checked;

                _testingInputTextBox.Enabled = _useSpecifiedTestingSetRadioButton.Checked;
                _sentenceDetectorPathTextBox.Enabled =
                    _sentenceDetectorPathBrowseButton.Enabled =
                    _sentenceDetectorPathTagsButton.Enabled = _splitIntoSentencesCheckBox.Checked;
                _tokenizerPathTextBox.Enabled =
                    _tokenizerPathBrowseButton.Enabled =
                    _tokenizerPathTagsButton.Enabled = _learnableTokenizerRadioButton.Checked;

                var currentRow = currentRowOption ?? _entityDefinitionDataGridView.CurrentCell?.RowIndex ?? -1;

                int rowCount = _entityDefinitionDataGridView.RowCount;

                if (currentRow == -1 || rowCount == 0)
                {
                    _upButton.Enabled = _downButton.Enabled = _removeButton.Enabled = _duplicateButton.Enabled = false;
                    return;
                }
                _removeButton.Enabled = _duplicateButton.Enabled = true;

                _upButton.Enabled = currentRow > 0;
                _downButton.Enabled = currentRow < rowCount - 1;

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44887");
            }
        }

        /// <summary>
        /// Sets the control values from the settings object.
        /// </summary>
        private void SetControlValues()
        {
            _suspendUpdatesToSettingsObject = true;

            _entityDefinitions = new BindingList<EntityDefinition>(_settings.EntityDefinitions)
            {
                RaiseListChangedEvents = true
            };
            _entityDefinitions.ListChanged += HandleValueChanged;
            _entityDefinitionDataGridView.DataSource = _entityDefinitions;

            _workingDirTextBox.Text = _settings.WorkingDir;
            _trainingInputTextBox.Text = _settings.TrainingInput;
            _testingInputTextBox.Text = _settings.TestingInput;

            if (_settings.TestingSet == TestingSetType.RandomlyPickedFromTrainingSet)
            {
                _randomlyTakeFromTrainingSetRadioButton.Checked = true;
            }
            else
            {
                _useSpecifiedTestingSetRadioButton.Checked = true;
            }

            _percentToUseForTestingSetTextBox.Text = _settings.PercentToUseForTestingSet.ToString(CultureInfo.CurrentCulture);
            _randomSeedForSetDivisionTextBox.Text = _settings.RandomSeedForSetDivision?.ToString(CultureInfo.CurrentCulture) ?? "";
            _typesVoaFunctionTextBox.Text = _settings.TypesVoaFunction;
            _outputFileBaseNameTextBox.Text = _settings.OutputFileBaseName;
            _outputSeparateFileForEachCategoryCheckBox.Checked = _settings.OutputSeparateFileForEachCategory;
            _failIfOutputFileExistsCheckBox.Checked = _settings.FailIfOutputFileExists;
            _percentNonInterestingPagesToIncludeTextBox.Text = _settings.PercentUninterestingPagesToInclude.ToString(CultureInfo.CurrentCulture);
            _randomSeedForPageInclusionTextBox.Text = _settings.RandomSeedForPageInclusion?.ToString(CultureInfo.CurrentCulture) ?? "";
            _splitIntoSentencesCheckBox.Checked = _settings.SplitIntoSentences;
            _sentenceDetectorPathTextBox.Text = _settings.SentenceDetectionModelPath;
            if (_settings.TokenizerType == OpenNlpTokenizer.WhiteSpaceTokenizer)
            {
                _whitespaceTokenizerRadioButton.Checked = true;
            }
            else if (_settings.TokenizerType == OpenNlpTokenizer.SimpleTokenizer)
            {
                _simpleTokenizerRadioButton.Checked = true;
            }
            else if (_settings.TokenizerType == OpenNlpTokenizer.LearnableTokenizer)
            {
                _learnableTokenizerRadioButton.Checked = true;
            }
            _tokenizerPathTextBox.Text = _settings.TokenizerModelPath;
            _fkbVersionTextBox.Text = _settings.FKBVersion;

            Dirty = false;

            _suspendUpdatesToSettingsObject = false;
        }

        /// <summary>
        /// Prompts for file save if the file is dirty, returns <see langword="true"/>
        /// if the file is not dirty or is saved or if user declines to save changes.
        /// </summary>
        /// <returns></returns>
        private bool PromptForDirtyFile()
        {
            bool confirm = true;
            if (Dirty)
            {
                var response = MessageBox.Show(this,
                    "Changes have not been saved, would you like to save now?",
                    "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button3, 0);
                if (response == System.Windows.Forms.DialogResult.Yes)
                {
                    var cancel = new CancelEventArgs();
                    HandleSaveButton_Click(this, cancel);
                    if (cancel.Cancel)
                    {
                        confirm = false;
                    }
                }
                else if (response == DialogResult.Cancel)
                {
                    confirm = false;
                }
            }

            return confirm;
        }
        #endregion Private Methods
    }

    public enum TestingSetType
    {
        RandomlyPickedFromTrainingSet = 0,
        Specified = 1
    }

    public class Settings
    {
        private bool _outputSeparateFileForEachCategory;

        [YamlIgnore]
        public string WorkingDir { get; set; }

        public string TrainingInput { get; set; }
        public string TestingInput { get; set; }
        public TestingSetType TestingSet { get; set; }
        public int PercentToUseForTestingSet { get; set; }
        public int? RandomSeedForSetDivision { get; set; }

        public string TypesVoaFunction { get; set; }

        /// <summary>
        /// Will be used to build the name of the training and testing data
        /// output files (by adding .train.txt, e.g.) if they are not specified explicitly
        /// </summary>
        public string OutputFileBaseName { get; set; } = "opennlp.annotated";

        /// <summary>
        /// Explicitly set the name of the file to write training data to
        /// </summary>
        public string TrainingOutputFileName { get; set; }

        /// <summary>
        /// Explicitly set the name of the file to write testing data to
        /// </summary>
        public string TestingOutputFileName { get; set; }

        public bool UseDatabase { get; set; }
        public string DatabaseServer { get; set; }
        public string DatabaseName { get; set; }
        public string ModelName { get; set; }
        public string AttributeSetName { get; set; }
        public int PercentUninterestingPagesToInclude { get; set; }
        public int? RandomSeedForPageInclusion { get; set; }

        public bool OutputSeparateFileForEachCategory
        {
            get
            {
                return _outputSeparateFileForEachCategory;
            }
            set
            {
                if (value)
                {
                    throw new NotImplementedException();
                }
                _outputSeparateFileForEachCategory = value;
            }
        }

        public bool FailIfOutputFileExists { get; set; } = true;

        public NamedEntityRecognizer Format { get; set; } = NamedEntityRecognizer.OpenNLP;

        public bool SplitIntoSentences { get; set; } = true;
        public string SentenceDetectionModelPath { get; set; } = "<ComponentDataDir>\\NER\\sentence-detector.nlp.etf";
        public OpenNlpTokenizer TokenizerType { get; set; } = OpenNlpTokenizer.LearnableTokenizer;
        public string TokenizerModelPath { get; set; } = "<ComponentDataDir>\\NER\\tokenizer.nlp.etf";

        public Collection<EntityDefinition> EntityDefinitions { get;} = new Collection<EntityDefinition>();
        public long LastIDToProcess { get;  set; }
        public long FirstIDToProcess { get; set; }
        public bool UseAttributeSetForTypes { get; set; }
        public string FKBVersion { get; set; }

        public static Settings LoadFrom(string fileName)
        {
            var deserializer = new Deserializer();
            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    var retVal = deserializer.Deserialize<Settings>(reader);
                    retVal.WorkingDir = Path.GetDirectoryName(Path.GetFullPath(fileName));
                    return retVal;
                }
            }
            catch(Exception ex)
            {
                ex.ExtractLog("ELI46853");
                ex.ExtractDisplay("ELI46853");
            }
            return null;
        }

        public void SaveTo(string fileName)
        {
            var sb = new SerializerBuilder();
            sb.EmitDefaults();
            var serializer = sb.Build();
            try
            {
                using (var stream = new StreamWriter(fileName))
                {
                    serializer.Serialize(stream, this);
                }
                WorkingDir = Path.GetDirectoryName(Path.GetFullPath(fileName));
            }
            catch(Exception ex)
            {
                ex.ExtractLog("ELI46854");
                ex.ExtractDisplay("ELI46854");
            }
        }
    }
}