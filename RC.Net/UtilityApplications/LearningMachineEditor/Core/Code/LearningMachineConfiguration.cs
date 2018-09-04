using Extract.AttributeFinder;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Main editor form
    /// </summary>
    public partial class LearningMachineConfiguration : Form
    {
        #region Constants

        private static readonly string _CONFIGURATION_CHANGED_SINCE_TRAINING =
            "Configuration has changed since machine was trained";
        private static readonly string _CONFIGURATION_CHANGED_SINCE_FEATURES_COMPUTED =
            "Configuration has changed since features were computed";
        private static readonly string _TRAINED_STATUS = "Machine is trained";
        private static readonly string _UNTRAINED_STATUS = "Machine is not trained";
        private static readonly string _INVALID_STATUS = "Invalid configuration";
        private static readonly string _TITLE_TEXT = "{0} - Learning Machine Editor";
        private static readonly string _TITLE_TEXT_DIRTY = "*{0} - Learning Machine Editor";
        private static readonly string _NEW_FILE_NAME = "[Unsaved file]";
        private static readonly string _MATCH = "match";
        private static readonly string _DO_NOT_MATCH = "don't match";
        private static readonly string _DEFAULT_INPUT_LOCATION = @"K:\SecureSamples\";
        private static readonly string _DRAG_FILE_EXT = ".lm";

        #endregion Constants

        #region Fields

        // Currently configured learning machine instance. Configuration of this instance reflects the
        // values of UI controls
        private LearningMachine _currentLearningMachine;

        // Saved (or unsaved, default) learning machine instance. Used to calculate dirty state
        private LearningMachine _savedLearningMachine;

        // Used to preserve classifier training and/or encoder computed configuration when possible.
        // A change in the UI (e.g., a text box value) causes the current instance to be rebuilt. When
        // the change does not invalidate encoder or classifier then these are copied to the newly built
        // instance. This instance also allows classifier training or encoder configuration to be restored
        // when the user changes a value back to the previous value.
        private LearningMachine _previousLearningMachine;

        // Set to prevent triggering updates to learning machine instance when they are not desired or
        // not needed. Set to true while the UI is being updated from the current machine instance or while
        // syncing invisible UI controls with visible counterparts.
        private bool _suspendMachineUpdates;

        // Set while machine is being updated. When updating the current machine instance from the UI,
        // some invalid control values will be changed and there is no need for these changes to trigger
        // another UpdateLearningMachine call.
        private bool _updatingMachine;

        // Indicates whether the current configuration is valid or not. When false, prevents certain actions
        // such as saving and training. Calculated by UpdateLearningMachine
        private bool _valid;

        // Whether the current machine is different from the default or from the last saved version
        private bool _dirty;

        // Used to determine whether current values should be retained (if true) or if default values
        // for certain controls should be used when changing machine type or usage (e.g., if true, the
        // current setting for useAutoBagOfWordsCheckBox will be respected when changing from document
        // categorization to pagination but if false then the default value, false, will be applied) 
        private bool _textValueOrCheckStateChangedSinceCreation;

        // The path and file name that the current machine was saved to last
        private string _fileName = _NEW_FILE_NAME;

        // Reuse this dialog so that the last-used radio button remains selected
        // after closing the dialog and changing a classifier parameter
        private TrainingTesting _trainingTestingDialog;

        private bool _dangerMode = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets/sets the currently configured learning machine
        /// </summary>
        public LearningMachine CurrentLearningMachine
        {
            get
            {
                return _currentLearningMachine;
            }
            set
            {
                try
                {
                    if (value != null && !Object.ReferenceEquals(_currentLearningMachine, value))
                    {
                        _currentLearningMachine = value;

                        // Compare to previous and saved machines to calculate dirty states and applicable training data
                        AfterCurrentLearningMachineChanged();
                        UpdateControlValues();

                        // Set any warnings that might not be enforced by the LearningMachine class itself
                        UpdateLearningMachine(validateOnly: true);
                    }
                }
                catch (Exception e)
                {
                    throw e.AsExtract("ELI39894");
                }
            }
        }

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
                    ? string.Format(CultureInfo.CurrentCulture, _TITLE_TEXT_DIRTY, _fileName)
                    : string.Format(CultureInfo.CurrentCulture, _TITLE_TEXT, _fileName);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes readable names for <see cref="LearningMachineType"/>
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LearningMachineConfiguration()
        {
            LearningMachineType.ActivationNetwork.SetReadableValue("Activation network (Neural Network)");
            LearningMachineType.MulticlassSVM.SetReadableValue("Multi-class SVM (one-vs-one)");
            LearningMachineType.MultilabelSVM.SetReadableValue("Multi-label SVM (one-vs-many)");
        }

        /// <summary>
        /// Initialize controls
        /// </summary>
        public LearningMachineConfiguration(string learningMachinePath = null)
        {
            try
            {
                InitializeComponent();

                // Stack panels
                paginationInputPanel.Location
                    = attributeCategorizationInputPanel.Location
                    = documentCategorizationFolderInputPanel.Location
                    = documentCategorizationCsvInputPanel.Location;
                paginationInputPanel.Size
                    = attributeCategorizationInputPanel.Size
                    = documentCategorizationFolderInputPanel.Size
                    = documentCategorizationCsvInputPanel.Size;
                svmPanel.Location = neuralNetPanel.Location;
                probabilisticSvmPanel.Left = svmPanel.Left;

                // Assign appropriate path tags
                documentCategorizationCsvFeatureVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
                paginationFeatureVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
                paginationAnswerVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
                documentCategorizationFolderAnswerPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
                documentCategorizationFolderFeatureVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
                attributeCategorizationCandidateVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();

                if (!string.IsNullOrWhiteSpace(learningMachinePath))
                {
                    _fileName = learningMachinePath;
                }
            }
            catch (Exception e)
            {
                e.ExtractDisplay("ELI40026");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Set window size, now that controls are in their proper places
                Size = MinimumSize;

                // Anchor stacked controls
                paginationInputPanel.Anchor
                    = attributeCategorizationInputPanel.Anchor
                    = documentCategorizationFolderInputPanel.Anchor
                    = documentCategorizationCsvInputPanel.Anchor
                    = System.Windows.Forms.AnchorStyles.Top
                      | System.Windows.Forms.AnchorStyles.Bottom
                      | System.Windows.Forms.AnchorStyles.Left
                      | System.Windows.Forms.AnchorStyles.Right;

                // Initialize Machine Type combo
                machineTypeComboBox.InitializeWithReadableEnum<LearningMachineType>(true);

                _suspendMachineUpdates = true;
                machineTypeComboBox.SelectedIndex = 0;
                _suspendMachineUpdates = false;

                // Initialize feature filter
                attributeFeatureFilterComboBox.Items.Add(_MATCH);
                attributeFeatureFilterComboBox.Items.Add(_DO_NOT_MATCH);

                // Set default values for controls
                SetDefaultValues();

                BeginInvoke((MethodInvoker)(() =>
                {
                    using (new TemporaryWaitCursor())
                    {
                        // Open specified file if available
                        if (!_fileName.Equals(_NEW_FILE_NAME, StringComparison.Ordinal))
                        {
                            try
                            {
                                if (File.Exists(_fileName))
                                {
                                    _savedLearningMachine = LearningMachine.Load(_fileName);
                                    _previousLearningMachine = null;

                                    // Ensure that corresponding text controls get updated
                                    _textValueOrCheckStateChangedSinceCreation = true;
                                }
                                else
                                {
                                    var ue = new ExtractException("ELI39923", "File does not exist");
                                    ue.AddDebugData("Filename", _fileName, false);
                                    ue.Display();
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.ExtractDisplay("ELI39924");
                            }
                        }

                        if (_savedLearningMachine == null)
                        {
                            _savedLearningMachine = MakeNewMachine();
                            _fileName = _NEW_FILE_NAME;
                        }

                        // Set current learning machine to be a clone of the saved copy
                        CurrentLearningMachine = _savedLearningMachine;
                    }
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39922");
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
                    HandleSaveMachineButton_Click(this, EventArgs.Empty);
                    return true;
                }

                // Ctrl+O = Open
                if (keyData == (Keys.Control | Keys.O))
                {
                    openMachineButton.Focus();
                    HandleOpenMachineButton_Click(this, EventArgs.Empty);
                    return true;
                }
            }
            catch (Exception e)
            {
                e.ExtractDisplay("ELI40027");
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40033");
            }
            
            base.OnClosing(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragEnter"/> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs"/> that contains the event data.</param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            try
            {
                // Check if this is a file drop
                if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the files being dragged
                    string[] fileNames = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                    // Check that there is only 1 file and that the extension is .lm
                    if (fileNames.Length == 1
                        && Path.GetExtension(fileNames[0]).Equals(_DRAG_FILE_EXT,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        drgevent.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        drgevent.Effect = DragDropEffects.None;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI40055", ex);
            }
            base.OnDragEnter(drgevent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragDrop"/> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs"/> that contains the event data.</param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            try
            {
                // Check if this is a file drop event
                if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the files being dragged
                    string[] fileNames = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                    // Check that there is only 1 file and that the extension is .lm
                    if (fileNames.Length == 1
                        && Path.GetExtension(fileNames[0]).Equals(_DRAG_FILE_EXT,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        if (!PromptForDirtyFile())
                        {
                            return;
                        }

                        Cursor.Current = Cursors.WaitCursor;
                        _savedLearningMachine = LearningMachine.Load(fileNames[0]);

                        // Ensure that corresponding text controls get updated
                        _textValueOrCheckStateChangedSinceCreation = true;

                        _previousLearningMachine = null;
                        _fileName = fileNames[0];
                        _valid = true;
                        CurrentLearningMachine = _savedLearningMachine;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI40056", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            base.OnDragDrop(drgevent);
        }
        #endregion Overrides

        #region Internal Methods

        /// <summary>
        /// Clears the training log. Sets saved machine to null so that dirty flag will be computed properly.
        /// </summary>
        internal void ClearTrainingLog()
        {
            if (!String.IsNullOrEmpty(CurrentLearningMachine.TrainingLog))
            {
                _savedLearningMachine = null;
                CurrentLearningMachine.TrainingLog = "";
                Dirty = true;
            }
        }

        #endregion Internal Methods

        #region Private Methods

        private void AfterCurrentLearningMachineChanged()
        {
            if (!string.IsNullOrEmpty(_previousLearningMachine?.TrainingLog))
            {
                // Preserve training log
                _currentLearningMachine.TrainingLog = _previousLearningMachine.TrainingLog;
            }

            // If current machine is trained or no previous machine,
            // then set previous machine to current machine
            if (_currentLearningMachine.IsTrained
                || _currentLearningMachine.Encoder.AreEncodingsComputed
                || _previousLearningMachine == null)
            {
                _previousLearningMachine = _currentLearningMachine;
                if (_currentLearningMachine.IsTrained)
                {
                    machineStateToolStripStatusLabel.Text = _TRAINED_STATUS;
                }
                else
                {
                    machineStateToolStripStatusLabel.Text = _UNTRAINED_STATUS;
                }
            }
            // Else, if configurations are the same, set current machine to previous machine
            // (preserves any training or computed features)
            else if (_currentLearningMachine.IsConfigurationEqualTo(_previousLearningMachine))
            {
                _currentLearningMachine = _previousLearningMachine;
                if (_currentLearningMachine.IsTrained)
                {
                    machineStateToolStripStatusLabel.Text = _TRAINED_STATUS;
                }
                else
                {
                    machineStateToolStripStatusLabel.Text = _UNTRAINED_STATUS;
                }
            }
            // Else if encoder settings are the same, set encoder and possible classifier to match previous machine
            // so that changing input config does not clear encodings or classifier
            else if (_previousLearningMachine.Encoder.AreEncodingsComputed)
            {
                // If encoders are configured the same, then copy the previous version into the current
                // (they could have the same settings but the new one is not computed or would not have gotten to this point)
                if (_currentLearningMachine.Encoder.IsConfigurationEqualTo(_previousLearningMachine.Encoder))
                {
                    _currentLearningMachine.Encoder = _previousLearningMachine.Encoder;

                    // If classifiers are configured the same, then copy the previous version into the current
                    if (_previousLearningMachine.IsTrained
                        && _currentLearningMachine.Classifier.IsConfigurationEqualTo(_previousLearningMachine.Classifier))
                    {
                        _currentLearningMachine.Classifier = _previousLearningMachine.Classifier;
                        machineStateToolStripStatusLabel.Text = "";
                    }
                    else if (_previousLearningMachine.IsTrained)
                    {
                        machineStateToolStripStatusLabel.Text = _CONFIGURATION_CHANGED_SINCE_TRAINING;
                    }
                }
                else
                {
                    machineStateToolStripStatusLabel.Text = _CONFIGURATION_CHANGED_SINCE_FEATURES_COMPUTED;
                }

                // If no status message set, set to trained or untrained
                if (string.IsNullOrEmpty(machineStateToolStripStatusLabel.Text))
                {
                    if (_currentLearningMachine.IsTrained)
                    {
                        machineStateToolStripStatusLabel.Text = _TRAINED_STATUS;
                    }
                    else
                    {
                        machineStateToolStripStatusLabel.Text = _UNTRAINED_STATUS;
                    }
                }
            }
            else
            {
                machineStateToolStripStatusLabel.Text = _UNTRAINED_STATUS;
            }

            // Enable/disable feature editing and answer viewing
            editFeaturesButton.Enabled
                = viewAnswerListButton.Enabled
                = writeDataToCsvButton.Enabled
                = CurrentLearningMachine.Encoder.AreEncodingsComputed;

            // Check against saved machine configuration
            Dirty = !_currentLearningMachine.IsConfigurationEqualTo(_savedLearningMachine)
                || (_currentLearningMachine.Encoder.AreEncodingsComputed
                    != _savedLearningMachine.Encoder.AreEncodingsComputed)
                || (_currentLearningMachine.IsTrained != _savedLearningMachine.IsTrained)
                || (_currentLearningMachine.IsTrained
                    && _currentLearningMachine.Classifier.LastTrainedOn != _savedLearningMachine.Classifier.LastTrainedOn);
        }

        /// <summary>
        /// Update control values from <see cref="CurrentLearningMachine"/>
        /// </summary>
        private void UpdateControlValues()
        {
            try
            {
                // Prevent machine updates while updating controls
                _suspendMachineUpdates = true;

                ClearErrors();

                var inputConfig = CurrentLearningMachine.InputConfig;
                var encoder = CurrentLearningMachine.Encoder;

                // Set input config controls
                if (CurrentLearningMachine.Usage == LearningMachineUsage.DocumentCategorization)
                {
                    documentCategorizationRadioButton.Checked = true;
                    if (inputConfig.InputPathType == InputType.TextFileOrCsv)
                    {
                        textFileOrCsvRadioButton.Checked = true;
                        documentCategorizationCsvTextBox.Text = inputConfig.InputPath ?? "";
                        documentCategorizationCsvFeatureVoaTextBox.Text = inputConfig.AttributesPath ?? "";
                        documentCategorizationCsvTrainingPercentageTextBox.Text = inputConfig.TrainingSetPercentage.ToString(CultureInfo.CurrentCulture);
                        documentCategorizationCsvRandomNumberSeedTextBox.Text = CurrentLearningMachine.RandomNumberSeed.ToString(CultureInfo.CurrentCulture);
                        documentCategorizationCsvNegativeClassNameTextBox.Text = encoder.NegativeClassName;
                    }
                    else
                    {
                        folderSearchRadioButton.Checked = true;
                        documentCategorizationInputFolderTextBox.Text = inputConfig.InputPath ?? "";
                        documentCategorizationFolderFeatureVoaTextBox.Text = inputConfig.AttributesPath ?? "";
                        documentCategorizationFolderAnswerTextBox.Text = inputConfig.AnswerPath ?? "";
                        documentCategorizationFolderTrainingPercentageTextBox.Text = inputConfig.TrainingSetPercentage.ToString(CultureInfo.CurrentCulture);
                        documentCategorizationFolderRandomNumberSeedTextBox.Text = CurrentLearningMachine.RandomNumberSeed.ToString(CultureInfo.CurrentCulture);
                        documentCategorizationFolderNegativeClassNameTextBox.Text = encoder.NegativeClassName;
                    }
                }
                else if (CurrentLearningMachine.Usage == LearningMachineUsage.Pagination
                    || CurrentLearningMachine.Usage == LearningMachineUsage.Deletion)
                {
                    if (CurrentLearningMachine.Usage == LearningMachineUsage.Pagination)
                    {
                        paginationRadioButton.Checked = true;
                    }
                    else
                    {
                        deletionRadioButton.Checked = true;
                    }
                    textFileOrCsvRadioButton.Checked = inputConfig.InputPathType == InputType.TextFileOrCsv;
                    paginationFileListOrFolderTextBox.Text = inputConfig.InputPath ?? "";
                    paginationFeatureVoaTextBox.Text = inputConfig.AttributesPath ?? "";
                    paginationAnswerVoaTextBox.Text = inputConfig.AnswerPath ?? "";
                    paginationTrainingPercentageTextBox.Text = inputConfig.TrainingSetPercentage.ToString(CultureInfo.CurrentCulture);
                    paginationRandomNumberSeedTextBox.Text = CurrentLearningMachine.RandomNumberSeed.ToString(CultureInfo.CurrentCulture);
                    paginationNegativeClassNameTextBox.Text = encoder.NegativeClassName;
                }
                else if (CurrentLearningMachine.Usage == LearningMachineUsage.AttributeCategorization)
                {
                    attributeCategorizationRadioButton.Checked = true;
                    textFileOrCsvRadioButton.Checked = inputConfig.InputPathType == InputType.TextFileOrCsv;
                    attributeCategorizationFileListOrFolderTextBox.Text = inputConfig.InputPath ?? "";
                    attributeCategorizationCandidateVoaTextBox.Text = inputConfig.AttributesPath ?? "";
                    attributeCategorizationTrainingPercentageTextBox.Text = inputConfig.TrainingSetPercentage.ToString(CultureInfo.CurrentCulture);
                    attributeCategorizationRandomNumberSeedTextBox.Text = CurrentLearningMachine.RandomNumberSeed.ToString(CultureInfo.CurrentCulture);
                    attributeCategorizationNegativeClassNameTextBox.Text = encoder.NegativeClassName;
                }
                else
                {
                    throw new ExtractException("ELI39797", "Unknown LearningMachineUsage: " + CurrentLearningMachine.Usage.ToString());
                }

                // Set encoder controls
                if (encoder.AutoBagOfWords == null)
                {
                    useAutoBagOfWordsCheckBox.Checked = false;
                }
                else
                {
                    useAutoBagOfWordsCheckBox.Checked = true;
                    maxShingleSizeTextBox.Text = encoder.AutoBagOfWords.ShingleSize.ToString(CultureInfo.CurrentCulture);
                    maxFeaturesTextBox.Text = encoder.AutoBagOfWords.MaxFeatures.ToString(CultureInfo.CurrentCulture);
                    specifiedPagesTextBox.Text = encoder.AutoBagOfWords.PagesToProcess ?? "";
                    specifiedPagesCheckBox.Checked = !string.IsNullOrWhiteSpace(encoder.AutoBagOfWords.PagesToProcess);
                    useFeatureHashingForAutoBagOfWordsCheckBox.Checked = encoder.AutoBagOfWords.UseFeatureHashing;
                }
                useAttributeFeatureFilterCheckBox.Checked = !string.IsNullOrWhiteSpace(encoder.AttributeFilter);
                attributeFeatureFilterComboBox.SelectedItem = encoder.NegateFilter ? _DO_NOT_MATCH : _MATCH;
                attributeFeatureFilterTextBox.Text = encoder.AttributeFilter ?? "";

                maxFeaturesPerVectorizerTextBox.Text =
                    encoder.AttributeVectorizerMaxDiscreteTermsFeatures.ToString(CultureInfo.CurrentCulture);

                tokenizeAttributesFilterCheckBox.Checked = !string.IsNullOrWhiteSpace(encoder.AttributesToTokenizeFilter);
                attributesToTokenizeFilterTextBox.Text = encoder.AttributesToTokenizeFilter ?? "";
                maxShingleSizeForAttributeFeaturesTextBox.Text =
                    encoder.AttributeVectorizerShingleSize.ToString(CultureInfo.CurrentCulture);

                // Set machine controls
                machineTypeComboBox.SelectEnumValue(CurrentLearningMachine.MachineType);
                csvOutputTextBox.Text = CurrentLearningMachine.CsvOutputFile ?? "";
                standardizeFeaturesForCsvOutputCheckBox.Checked = CurrentLearningMachine.StandardizeFeaturesForCsvOutput;

                // Neural net
                if (CurrentLearningMachine.MachineType == LearningMachineType.ActivationNetwork)
                {
                    NeuralNetworkClassifier classifier = (NeuralNetworkClassifier)CurrentLearningMachine.Classifier;
                    sizeOfHiddenLayersTextBox.Text = string.Join(", ", classifier.HiddenLayers);
                    maximumTrainingIterationsTextBox.Text = classifier.MaxTrainingIterations.ToString(CultureInfo.CurrentCulture);
                    useCrossValidationSetsCheckBox.Checked = classifier.UseCrossValidationSets;
                    if (classifier.UseCrossValidationSets)
                    {
                        numberOfCandidateNetwordsTextBox.Text = classifier.NumberOfCandidateNetworksToBuild.ToString(CultureInfo.CurrentCulture);
                    }
                    sigmoidAlphaTextBox.Text = classifier.SigmoidAlpha.ToString("r", CultureInfo.CurrentCulture);
                }

                // SVM
                else if (CurrentLearningMachine.MachineType == LearningMachineType.MulticlassSVM
                    || CurrentLearningMachine.MachineType == LearningMachineType.MultilabelSVM)
                {
                    var classifier = (SupportVectorMachineClassifier)CurrentLearningMachine.Classifier;
                    svmComplexityTextBox.Text = classifier.Complexity.ToString("r", CultureInfo.CurrentCulture);
                    svmAutoComplexityCheckBox.Checked = classifier.AutomaticallyChooseComplexityValue;
                    switch (classifier.ScoreTypeToUseForComplexityChoosingAlgorithm)
                    {
                        case MachineScoreType.Precision:
                            svmUsePrecisionRadioButton.Checked = true;
                            break;
                        case MachineScoreType.Recall:
                            svmUseRecallRadioButton.Checked = true;
                            break;
                        default:
                            svmUseF1ScoreRadioButton.Checked = true;
                            break;
                    }
                    svmConditionallyApplyWeightRatioCheckBox.Checked = classifier.ConditionallyApplyWeightRatio;
                    svmWeightRatioTextBox.Text = classifier.PositiveToNegativeWeightRatio.HasValue
                        ? classifier.PositiveToNegativeWeightRatio.Value.ToString("r", CultureInfo.CurrentCulture)
                        : "";
                    svmCacheSizeTextBox.Text = classifier.TrainingAlgorithmCacheSize.HasValue
                        ? classifier.TrainingAlgorithmCacheSize.Value.ToString(CultureInfo.CurrentCulture)
                        : "";

                    // Probability options for SVM
                    calibrateForProbabilitiesCheckBox.Checked = classifier.CalibrateMachineToProduceProbabilities;
                    useUnknownCheckBox.Checked = CurrentLearningMachine.UseUnknownCategory;
                    unknownCutoffTextBox.Text = CurrentLearningMachine.UnknownCategoryCutoff.ToString("r", CultureInfo.CurrentCulture);
                    translateUnknownCheckbox.Checked = CurrentLearningMachine.TranslateUnknownCategory;
                    translateUnknownTextBox.Text = CurrentLearningMachine.TranslateUnknownCategoryTo ?? "";
                }
                else
                {
                    throw new ExtractException("ELI39798", "Unknown LearningMachineMachineType: " + CurrentLearningMachine.MachineType);
                }
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI39814");
                ue.Display();
            }
            finally
            {
                _suspendMachineUpdates = false;
            }
        }
        
        /// <summary>
        /// Build a new learning machine from UI values
        /// </summary>
        internal LearningMachine BuildLearningMachine()
        {
            LearningMachine learningMachine = null;
            if (_dangerMode)
            {
                learningMachine = CurrentLearningMachine;
            }
            else
            {
                learningMachine = new LearningMachine();

                // Preserve existing label attributes settings since there are no UI controls to recreate them with
                learningMachine.LabelAttributesSettings = CurrentLearningMachine.LabelAttributesSettings
                    ?.DeepClone();
            }

            SetInputConfigValues(learningMachine, out string negativeClassName);
            SetEncoderValues(learningMachine, negativeClassName);
            SetClassifierValues(learningMachine);

            return learningMachine;
        }

        /// <summary>
        /// Build a new learning machine. Overwrite <see cref="CurrentLearningMachine"/> if all goes well.
        /// </summary>
        /// <param name="validateOnly">Whether to validate without updating the current machine</param>
        private void UpdateLearningMachine(bool validateOnly = false)
        {
            // Don't update if in the process of initializing the controls from a learning machine object
            if (_suspendMachineUpdates || _updatingMachine)
            {
                return;
            }

            try
            {
                _updatingMachine = true;
                ClearErrors();
                _valid = true;
                var learningMachine = BuildLearningMachine();

                // Building the learning machine from UI after certain UI changes
                // (e.g., clearing random seed) can cause other changes to occur
                if (_textValueOrCheckStateChangedSinceCreation)
                {
                    SyncCorrespondingControls();
                }

                if (!validateOnly)
                {
                    if (_valid)
                    {
                        // Set field directly so as not to trigger an UpdateControlValues
                        // Updating control values from machine could overwrite a partial modification
                        // E.g., it would un-check 'Use only text from these pages' if the text box was empty
                        _currentLearningMachine = learningMachine;

                        AfterCurrentLearningMachineChanged();
                    }
                    // If in an invalid state then must be dirty
                    else
                    {
                        Dirty = true;
                    }
                }

                if (!_valid)
                {
                    machineStateToolStripStatusLabel.Text = _INVALID_STATUS;
                }
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI39815");
                ue.Display();
            }
            finally
            {
                _updatingMachine = false;
            }
        }

        /// <summary>
        /// Initializes a <see cref="CurrentLearningMachine"/> with input configuration values
        /// </summary>
        /// <param name="learningMachine">The <see cref="CurrentLearningMachine"/> to be initialized</param>
        /// <param name="negativeClassName">The name to be used for the negative class. The control to use depends on usage
        /// and input config settings so it is determined here rather than in <see cref="SetEncoderValues"/></param>
        private void SetInputConfigValues(LearningMachine learningMachine, out string negativeClassName)
        {
            if (!_dangerMode)
            {
                learningMachine.InputConfig = new InputConfiguration();
            }

            var inputConfig = learningMachine.InputConfig;
            int randomSeed;
            TextBox inputPathTextBox;
            TextBox attributesPathTextBox;
            TextBox answerPathTextBox = null;
            TextBox trainingPercentageTextBox;
            TextBox randomSeedTextBox;

            if (documentCategorizationRadioButton.Checked)
            {
                if (textFileOrCsvRadioButton.Checked)
                {
                    inputConfig.InputPathType = InputType.TextFileOrCsv;
                    inputPathTextBox = documentCategorizationCsvTextBox;
                    attributesPathTextBox = documentCategorizationCsvFeatureVoaTextBox;
                    inputConfig.AnswerPath = null;
                    trainingPercentageTextBox = documentCategorizationCsvTrainingPercentageTextBox;
                    randomSeedTextBox = documentCategorizationCsvRandomNumberSeedTextBox;
                    negativeClassName = documentCategorizationCsvNegativeClassNameTextBox.Text;
                }
                else // folderSearchRadioButton.Checked
                {
                    inputConfig.InputPathType = InputType.Folder;
                    inputPathTextBox = documentCategorizationInputFolderTextBox;
                    attributesPathTextBox = documentCategorizationFolderFeatureVoaTextBox;
                    answerPathTextBox = documentCategorizationFolderAnswerTextBox;
                    trainingPercentageTextBox = documentCategorizationFolderTrainingPercentageTextBox;
                    randomSeedTextBox = documentCategorizationFolderRandomNumberSeedTextBox;
                    negativeClassName = documentCategorizationFolderNegativeClassNameTextBox.Text;
                }
            }
            else if (paginationRadioButton.Checked || deletionRadioButton.Checked)
            {
                inputConfig.InputPathType = textFileOrCsvRadioButton.Checked
                    ? InputType.TextFileOrCsv
                    : InputType.Folder;
                inputPathTextBox = paginationFileListOrFolderTextBox;
                attributesPathTextBox = paginationFeatureVoaTextBox;
                answerPathTextBox = paginationAnswerVoaTextBox;
                trainingPercentageTextBox = paginationTrainingPercentageTextBox;
                randomSeedTextBox = paginationRandomNumberSeedTextBox;
                negativeClassName = paginationNegativeClassNameTextBox.Text;
            }
            else // LearningMachineUsage.AttributeCategorization
            {
                inputConfig.InputPathType = textFileOrCsvRadioButton.Checked
                    ? InputType.TextFileOrCsv
                    : InputType.Folder;
                inputPathTextBox = attributeCategorizationFileListOrFolderTextBox;
                attributesPathTextBox = attributeCategorizationCandidateVoaTextBox;
                trainingPercentageTextBox = attributeCategorizationTrainingPercentageTextBox;
                randomSeedTextBox = attributeCategorizationRandomNumberSeedTextBox;
                negativeClassName = attributeCategorizationNegativeClassNameTextBox.Text;
            }

            // Set input path
            inputConfig.InputPath = inputPathTextBox.Text.Trim('"');
            if (string.IsNullOrWhiteSpace(inputPathTextBox.Text))
            {
                inputPathTextBox.SetError(configurationErrorProvider, "Input path required");
                _valid = false;
            }

            // Set attributes path
            inputConfig.AttributesPath = attributesPathTextBox.Text;
            // Require attribute path if auto-BoW feature is not enabled
            if (!useAutoBagOfWordsCheckBox.Checked
                && string.IsNullOrWhiteSpace(attributesPathTextBox.Text))
            {
                attributesPathTextBox.SetError(configurationErrorProvider,
                    "Feature attributes path required");
                _valid = false;
            }

            // Set answer path
            if (answerPathTextBox != null)
            {
                inputConfig.AnswerPath = answerPathTextBox.Text;
                if (string.IsNullOrWhiteSpace(answerPathTextBox.Text))
                {
                    answerPathTextBox.SetError(configurationErrorProvider,
                        "Answer path required");
                    _valid = false;
                }
            }

            // Parse/validate training set percentage
            if (string.IsNullOrWhiteSpace(trainingPercentageTextBox.Text))
            {
                trainingPercentageTextBox.SetError(configurationErrorProvider, "Must specify a training percentage");
                _valid = false;
            }
            else
            {
                int trainingPercentage = 0;
                if (!int.TryParse(trainingPercentageTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out trainingPercentage)
                    || trainingPercentage < 0 || trainingPercentage > 100)
                {
                    trainingPercentageTextBox.SetError(configurationErrorProvider, "Training percentage must be an integer between 0 and 100");
                    _valid = false;
                }
                inputConfig.TrainingSetPercentage = trainingPercentage;
            }

            // Parse random seed
            if (string.IsNullOrWhiteSpace(randomSeedTextBox.Text))
            {
                randomSeedTextBox.Text = new Random().Next().ToString(CultureInfo.InvariantCulture);
            }
            if (!int.TryParse(randomSeedTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out randomSeed))
            {
                randomSeedTextBox.SetError(configurationErrorProvider, "Unable to parse random number seed text as an integer");
                _valid = false;
            }
            learningMachine.RandomNumberSeed = randomSeed;
        }

        /// <summary>
        /// Initializes a <see cref="CurrentLearningMachine"/> with encoder values
        /// </summary>
        /// <param name="learningMachine">The <see cref="CurrentLearningMachine"/> to be initialized</param>
        private void SetEncoderValues(LearningMachine learningMachine, string negativeClassName)
        {
            var usage = LearningMachineUsage.DocumentCategorization;
            if (paginationRadioButton.Checked)
            {
                usage = LearningMachineUsage.Pagination;
            }
            else if (deletionRadioButton.Checked)
            {
                usage = LearningMachineUsage.Deletion;
            }
            else if (attributeCategorizationRadioButton.Checked)
            {
                usage = LearningMachineUsage.AttributeCategorization;
            }

            SpatialStringFeatureVectorizer autoBoW = null;
            if (useAutoBagOfWordsCheckBox.Checked)
            {
                int shingleSize;
                if (!int.TryParse(maxShingleSizeTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out shingleSize)
                    || shingleSize < 1)
                {
                    maxShingleSizeTextBox.SetError(configurationErrorProvider, "Shingle size must be an integer greater than zero");
                    _valid = false;
                }
                int maxFeatures;
                if (!int.TryParse(this.maxFeaturesTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxFeatures)
                    || maxFeatures < 1)
                {
                    maxFeaturesTextBox.SetError(configurationErrorProvider, "Max features value must be an integer greater than zero");
                    _valid = false;
                }
                try
                {
                    string pagesToProcess = specifiedPagesTextBox.Enabled ? specifiedPagesTextBox.Text : null;
                    if (usage == LearningMachineUsage.Pagination && pagesToProcess != null)
                    {
                        ExtractException.Assert("ELI45541", "Value must be a positive integer",
                            int.TryParse(pagesToProcess, out int page) && page > 0);
                    }
                    if (_dangerMode && learningMachine.Encoder.AutoBagOfWords != null)
                    {
                        autoBoW = learningMachine.Encoder.AutoBagOfWords;
                        autoBoW.PagesToProcess = pagesToProcess;
                        autoBoW.ShingleSize = shingleSize;
                        autoBoW.MaxFeatures = maxFeatures;
                    }
                    else
                    {
                        autoBoW = new SpatialStringFeatureVectorizer(pagesToProcess, shingleSize, maxFeatures);
                    }
                    autoBoW.UseFeatureHashing = useFeatureHashingForAutoBagOfWordsCheckBox.Checked;
                }
                catch (ExtractException ue)
                {
                    specifiedPagesTextBox.SetError(configurationErrorProvider, ue.Message);
                    _valid = false;
                }
            }
            string attributeFilter = null;
            bool negateFilter = false;
            if (useAttributeFeatureFilterCheckBox.Checked)
            {
                // Trim any whitespace from query
                attributeFilter = string.Join("", attributeFeatureFilterTextBox.Text.Split(
                    new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

                negateFilter = (string)attributeFeatureFilterComboBox.SelectedItem == _DO_NOT_MATCH;
            }

            int maxAttributeVectorizerFeatures;
            if (!int.TryParse(this.maxFeaturesPerVectorizerTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out maxAttributeVectorizerFeatures)
                || maxAttributeVectorizerFeatures < 1)
            {
                maxFeaturesPerVectorizerTextBox.SetError(configurationErrorProvider, "Max features value must be an integer greater than zero");
                _valid = false;
            }

            string attributesToTokenize = null;
            int shingleSizeForTokens = 1;
            if (tokenizeAttributesFilterCheckBox.Checked)
            {
                // Trim any whitespace from query
                attributesToTokenize = string.Join("", attributesToTokenizeFilterTextBox.Text.Split(
                    new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

                if (!int.TryParse(maxShingleSizeForAttributeFeaturesTextBox.Text,
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out shingleSizeForTokens)
                    || shingleSizeForTokens < 1)
                {
                    maxShingleSizeForAttributeFeaturesTextBox.SetError(configurationErrorProvider,
                        "Shingle size must be an integer greater than zero");
                    _valid = false;
                }
            }

            if (_dangerMode)
            {
                var encoder = learningMachine.Encoder;
                encoder.MachineUsage = usage;
                encoder.AutoBagOfWords = autoBoW;
                encoder.AttributeFilter = attributeFilter;
                encoder.NegateFilter = negateFilter;
                encoder.AttributeVectorizerMaxDiscreteTermsFeatures = maxAttributeVectorizerFeatures;
                encoder.AttributesToTokenizeFilter = attributesToTokenize;
                encoder.AttributeVectorizerShingleSize = shingleSizeForTokens;
                encoder.NegativeClassName = negativeClassName;
            }
            else
            {
                learningMachine.Encoder = new LearningMachineDataEncoder(usage, autoBoW, attributeFilter,
                    negateFilter, maxAttributeVectorizerFeatures, attributesToTokenize, shingleSizeForTokens, negativeClassName);
            }
        }

        /// <summary>
        /// Initializes a <see cref="CurrentLearningMachine"/> with classifier values
        /// </summary>
        /// <param name="learningMachine">The <see cref="CurrentLearningMachine"/> to be initialized</param>
        private void SetClassifierValues(LearningMachine learningMachine)
        {
            var machineType = machineTypeComboBox.ToEnumValue<LearningMachineType>();
            learningMachine.CsvOutputFile = csvOutputTextBox.Text;
            learningMachine.StandardizeFeaturesForCsvOutput = standardizeFeaturesForCsvOutputCheckBox.Checked;

            // Neural Network Classifier
            if (machineType == LearningMachineType.ActivationNetwork)
            {
                SetNeuralNetworkClassifierValues(learningMachine);
            }
            // Multiclass SVM Classifier
            else if (machineType == LearningMachineType.MulticlassSVM)
            {
                SetMulticlassSVMClassifierValues(learningMachine);
            }
            // Multilabel SVM Classifier
            else if (machineType == LearningMachineType.MultilabelSVM)
            {
                SetMultilabelSVMClassifierValues(learningMachine);
            }
            else
            {
                throw new ExtractException("ELI39899", "Unknown learning machine type: " + machineType.ToString());
            }
        }

        /// <summary>
        /// Initializes a <see cref="CurrentLearningMachine"/> with neural network classifier values
        /// </summary>
        /// <param name="learningMachine">The <see cref="CurrentLearningMachine"/> to be initialized</param>
        private void SetNeuralNetworkClassifierValues(LearningMachine learningMachine)
        {
            IEnumerable<int> hiddenLayers = Enumerable.Empty<int>();
            if (!Regex.IsMatch(sizeOfHiddenLayersTextBox.Text, @"^\s*[1-9]\d*(,\s*[1-9]\d*)*\s*$"))
            {
                sizeOfHiddenLayersTextBox.SetError(configurationErrorProvider,
                    "Could not parse hidden layers text");
                _valid = false;
            }
            try
            {
                hiddenLayers = Regex.Matches(sizeOfHiddenLayersTextBox.Text, @"\d+").Cast<Match>()
                    .Select(layerSize => int.Parse(layerSize.Value, NumberStyles.Integer, CultureInfo.InvariantCulture));
            }
            catch (ArgumentException)
            {
                sizeOfHiddenLayersTextBox.SetError(configurationErrorProvider,
                    "Could not parse hidden layers text");
                _valid = false;
            }

            int maxTrainingIterations, numberOfCandidateNetworks = 1;
            if (!int.TryParse(maximumTrainingIterationsTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out maxTrainingIterations) || maxTrainingIterations < 1)
            {
                maximumTrainingIterationsTextBox.SetError(configurationErrorProvider,
                    "Maximum training iterations must be an integer greater than zero");
                _valid = false;
            }
            if (useCrossValidationSetsCheckBox.Checked)
            {
                if (!int.TryParse(numberOfCandidateNetwordsTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out numberOfCandidateNetworks) || numberOfCandidateNetworks < 1)
                {
                    numberOfCandidateNetwordsTextBox.SetError(configurationErrorProvider,
                        "Number of candidate networks to try must be greater than zero");
                    _valid = false;
                }
            }
            double sigmoidAlpha;
            if (!Double.TryParse(sigmoidAlphaTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture,
                out sigmoidAlpha))
            {
                sigmoidAlphaTextBox.SetError(configurationErrorProvider,
                    "Could not parse sigmoid alpha text as a number");
                _valid = false;
            }

            if (_dangerMode && learningMachine.Classifier is NeuralNetworkClassifier nn)
            {
                nn.HiddenLayers = hiddenLayers;
                nn.MaxTrainingIterations = maxTrainingIterations;
                nn.NumberOfCandidateNetworksToBuild = numberOfCandidateNetworks;
                nn.SigmoidAlpha = sigmoidAlpha;
                nn.UseCrossValidationSets = useCrossValidationSetsCheckBox.Checked;
            }
            else
            {
                learningMachine.Classifier = new NeuralNetworkClassifier
                {
                    HiddenLayers = hiddenLayers,
                    MaxTrainingIterations = maxTrainingIterations,
                    NumberOfCandidateNetworksToBuild = numberOfCandidateNetworks,
                    SigmoidAlpha = sigmoidAlpha,
                    UseCrossValidationSets = useCrossValidationSetsCheckBox.Checked
                };
            }
        }

        /// <summary>
        /// Initializes a <see cref="LearningMachine"/> with classifier values
        /// </summary>
        /// <param name="learningMachine">The <see cref="LearningMachine"/> to be initialized</param>
        private void SetSVMClassifierValues(LearningMachine learningMachine)
        {
            SupportVectorMachineClassifier classifier = (SupportVectorMachineClassifier)learningMachine.Classifier;
            double complexity;
            if (!double.TryParse(svmComplexityTextBox.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                CultureInfo.InvariantCulture, out complexity)
                || complexity == 0)
            {
                svmComplexityTextBox.SetError(configurationErrorProvider,
                    "Complexity must be a number greater than zero");
                _valid = false;
            }

            string ratioText = svmWeightRatioTextBox.Text;
            bool isWeightRatioSpecified = !String.IsNullOrWhiteSpace(ratioText);
            double weightRatio = 1;
            if (isWeightRatioSpecified
                && !double.TryParse(ratioText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                    CultureInfo.InvariantCulture, out weightRatio)
                || weightRatio == 0)
            {
                svmWeightRatioTextBox.SetError(configurationErrorProvider,
                    "Weight ratio must be a number greater than zero");
                _valid = false;
            }

            string cacheSizeText = svmCacheSizeTextBox.Text;
            bool isCacheSizeSpecified = !String.IsNullOrWhiteSpace(cacheSizeText);
            int cacheSize = 0;
            if (isCacheSizeSpecified
                && !int.TryParse(cacheSizeText, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out cacheSize) || cacheSize < 0)
            {
                svmCacheSizeTextBox.SetError(configurationErrorProvider,
                    "Cache size must be empty or a non-negative integer");
                _valid = false;
            }

            classifier.Complexity = complexity;
            classifier.AutomaticallyChooseComplexityValue = svmAutoComplexityCheckBox.Checked;
            classifier.PositiveToNegativeWeightRatio = isWeightRatioSpecified ? weightRatio : (double?) null;
            classifier.TrainingAlgorithmCacheSize = isCacheSizeSpecified ? cacheSize : (int?) null;
            classifier.ConditionallyApplyWeightRatio = svmConditionallyApplyWeightRatioCheckBox.Checked;

            if (svmUsePrecisionRadioButton.Checked)
            {
                classifier.ScoreTypeToUseForComplexityChoosingAlgorithm = MachineScoreType.Precision;
            }
            else if (svmUseRecallRadioButton.Checked)
            {
                classifier.ScoreTypeToUseForComplexityChoosingAlgorithm = MachineScoreType.Recall;
            }
            else
            {
                classifier.ScoreTypeToUseForComplexityChoosingAlgorithm = MachineScoreType.OverallAgreementOrF1;
            }

            // Set probability values
            classifier.CalibrateMachineToProduceProbabilities = calibrateForProbabilitiesCheckBox.Checked;
            learningMachine.UseUnknownCategory = useUnknownCheckBox.Checked;

            double unknownCategoryCutoff = 0;
            if (!double.TryParse(unknownCutoffTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture,
                out unknownCategoryCutoff))
            {
                unknownCutoffTextBox.SetError(configurationErrorProvider,
                    "Could not parse unknown category cutoff text as a number");
                _valid = false;
            }
            learningMachine.UnknownCategoryCutoff = unknownCategoryCutoff;
            learningMachine.TranslateUnknownCategory = translateUnknownCheckbox.Checked;
            learningMachine.TranslateUnknownCategoryTo = translateUnknownTextBox.Text;
        }

        /// <summary>
        /// Initializes a <see cref="CurrentLearningMachine"/> with multi-class SVM classifier values
        /// </summary>
        /// <param name="learningMachine">The <see cref="CurrentLearningMachine"/> to be initialized</param>
        private void SetMulticlassSVMClassifierValues(LearningMachine learningMachine)
        {
            if (_dangerMode && learningMachine.Classifier is MulticlassSupportVectorMachineClassifier)
            {
                SetSVMClassifierValues(learningMachine);
            }
            else
            {
                learningMachine.Classifier = new MulticlassSupportVectorMachineClassifier();
                SetSVMClassifierValues(learningMachine);
            }
        }


        /// <summary>
        /// Initializes a <see cref="CurrentLearningMachine"/> with classifier values
        /// </summary>
        /// <param name="learningMachine">The <see cref="CurrentLearningMachine"/> to be initialized</param>
        private void SetMultilabelSVMClassifierValues(LearningMachine learningMachine)
        {
            if (_dangerMode && learningMachine.Classifier is MultilabelSupportVectorMachineClassifier)
            {
                SetSVMClassifierValues(learningMachine);
            }
            else
            {
                learningMachine.Classifier = new MultilabelSupportVectorMachineClassifier();
                SetSVMClassifierValues(learningMachine);
            }
        }

        /// <summary>
        /// Set enabled/disabled/visible states of all dependent controls
        /// </summary>
        private void SetControlStates()
        {
            // Input panels
            if (documentCategorizationRadioButton.Checked)
            {
                paginationInputPanel.Visible = false;
                attributeCategorizationInputPanel.Visible = false;
                documentCategorizationCsvInputPanel.Visible = textFileOrCsvRadioButton.Checked;
                documentCategorizationFolderInputPanel.Visible = folderSearchRadioButton.Checked;
                specifiedPagesCheckBox.Enabled = true;

                // Set default state for auto-bow
                if (!_textValueOrCheckStateChangedSinceCreation)
                {
                    useAutoBagOfWordsCheckBox.Checked = true;
                }
            }
            else if (paginationRadioButton.Checked)
            {
                paginationInputPanel.Visible = true;
                documentCategorizationCsvInputPanel.Visible = false;
                documentCategorizationFolderInputPanel.Visible = false;
                attributeCategorizationInputPanel.Visible = false;
                specifiedPagesCheckBox.Enabled = false;

                // Adjust labels and browse buttons for pagination panel
                if (textFileOrCsvRadioButton.Checked)
                {
                    paginationFileListOrFolderLabel.Text = "Train/testing file list";
                    paginationFileListOrFolderBrowseButton.FolderBrowser = false;
                }
                else
                {
                    paginationFileListOrFolderLabel.Text = "Train/testing file folder";
                    paginationFileListOrFolderBrowseButton.FolderBrowser = true;
                }

                // Set default state for auto-bow
                if (!_textValueOrCheckStateChangedSinceCreation)
                {
                    useAutoBagOfWordsCheckBox.Checked = false;
                }
            }
            else if (deletionRadioButton.Checked)
            {
                paginationInputPanel.Visible = true;
                documentCategorizationCsvInputPanel.Visible = false;
                documentCategorizationFolderInputPanel.Visible = false;
                attributeCategorizationInputPanel.Visible = false;
                specifiedPagesCheckBox.Enabled = true;

                // Adjust labels and browse buttons for pagination panel
                if (textFileOrCsvRadioButton.Checked)
                {
                    paginationFileListOrFolderLabel.Text = "Train/testing file list";
                    paginationFileListOrFolderBrowseButton.FolderBrowser = false;
                }
                else
                {
                    paginationFileListOrFolderLabel.Text = "Train/testing file folder";
                    paginationFileListOrFolderBrowseButton.FolderBrowser = true;
                }

                // Set default state for auto-bow
                if (!_textValueOrCheckStateChangedSinceCreation)
                {
                    useAutoBagOfWordsCheckBox.Checked = true;
                }
            }
            else
            {
                attributeCategorizationInputPanel.Visible = true;
                documentCategorizationCsvInputPanel.Visible = false;
                documentCategorizationFolderInputPanel.Visible = false;
                paginationInputPanel.Visible = false;
                specifiedPagesCheckBox.Enabled = false;

                // Adjust labels and browse buttons for attribute categorization panel
                if (textFileOrCsvRadioButton.Checked)
                {
                    attributeCategorizationFileListOrFolderLabel.Text = "Train/testing file list";
                    attributeCategorizationFileListOrFolderBrowseButton.FolderBrowser = false;
                }
                else
                {
                    attributeCategorizationFileListOrFolderLabel.Text = "Train/testing file folder";
                    attributeCategorizationFileListOrFolderBrowseButton.FolderBrowser = true;
                }

                // Set default state for auto-bow
                if (!_textValueOrCheckStateChangedSinceCreation)
                {
                    useAutoBagOfWordsCheckBox.Checked = false;
                }
            }

            // Feature panel
            maxShingleSizeTextBox.Enabled = maxFeaturesTextBox.Enabled
                                          = maxShingleSizeLabel.Enabled
                                          = maxFeaturesLabel.Enabled
                                          = useFeatureHashingForAutoBagOfWordsCheckBox.Enabled
                                          = useAutoBagOfWordsCheckBox.Checked;

            specifiedPagesCheckBox.Enabled = useAutoBagOfWordsCheckBox.Checked;
            specifiedPagesTextBox.Enabled = 
                paginationPagesLabel.Enabled =
                specifiedPagesCheckBox.Enabled && specifiedPagesCheckBox.Checked;

            attributeFeatureFilterTextBox.Enabled = useAttributeFeatureFilterCheckBox.Checked;
            attributeFeatureFilterComboBox.Enabled = useAttributeFeatureFilterCheckBox.Checked;

            attributesToTokenizeFilterTextBox.Enabled = tokenizeAttributesFilterCheckBox.Checked;
            maxShingleSizeForAttributeFeaturesTextBox.Enabled = tokenizeAttributesFilterCheckBox.Checked;

            // Machine panels
            var machineType = machineTypeComboBox.ToEnumValue<LearningMachineType>();
            if (machineType == LearningMachineType.ActivationNetwork)
            {
                neuralNetPanel.Visible = true;
                probabilisticSvmPanel.Visible = false;
                svmPanel.Visible = false;
                numberOfCandidateNetwordsTextBox.Enabled = useCrossValidationSetsCheckBox.Checked;
            }
            else if (machineType == LearningMachineType.MulticlassSVM
                || machineType == LearningMachineType.MultilabelSVM)
            {
                neuralNetPanel.Visible = false;
                svmPanel.Visible = true;
                svmScoreTypeGroupBox.Enabled = svmAutoComplexityCheckBox.Checked;

                probabilisticSvmPanel.Visible = true;
                useUnknownCheckBox.Enabled = calibrateForProbabilitiesCheckBox.Checked;
                unknownCutoffTextBox.Enabled = calibrateForProbabilitiesCheckBox.Checked && useUnknownCheckBox.Checked;
                translateUnknownCheckbox.Enabled = calibrateForProbabilitiesCheckBox.Checked && useUnknownCheckBox.Checked;
                translateUnknownTextBox.Enabled = calibrateForProbabilitiesCheckBox.Checked && translateUnknownCheckbox.Checked;
            }
        }

        /// <summary>
        /// Create a default learning machine object
        /// </summary>
        /// <returns>A <see cref="LearningMachine"/> with some useful settings</returns>
        private static LearningMachine MakeNewMachine()
        {
            return new LearningMachine
            {
                InputConfig = new InputConfiguration
                    {
                        InputPath = _DEFAULT_INPUT_LOCATION,
                        InputPathType = InputType.Folder,
                        AttributesPath = "",
                        AnswerPath = @"$FileOf($DirOf(<SourceDocName>))",
                        TrainingSetPercentage = 80
                    },
                Encoder = new LearningMachineDataEncoder
                    (
                        usage: LearningMachineUsage.DocumentCategorization,
                        autoBagOfWords: new SpatialStringFeatureVectorizer(null, 5, 2000),
                        attributeFilter: "*@Feature"
                    ),
                Classifier = new MulticlassSupportVectorMachineClassifier()
            };
        }

        /// <summary>
        /// Clear errors from all text boxes
        /// </summary>
        private void ClearErrors()
        {
            foreach (var textBox in this.GetAllControls().OfType<TextBox>())
            {
                textBox.SetError(configurationErrorProvider, "");
            }
        }

        /// <summary>
        /// Sets focus to first invalid item
        /// </summary>
        private void FocusFirstInvalid()
        {
            var firstInvalid = this.GetAllControls().OfType<TextBox>()
                .Where(textBox => !string.IsNullOrWhiteSpace(configurationErrorProvider.GetError(textBox)))
                .OrderBy(textBox => textBox.TabIndex)
                .FirstOrDefault();
            if (firstInvalid != null)
            {
                var tabPage = firstInvalid.GetAncestors().OfType<TabPage>().FirstOrDefault();
                if (tabPage != null)
                {
                    ((TabControl)tabPage.Parent).SelectedTab = tabPage;
                }
                firstInvalid.Focus();
            }
        }

        /// <summary>
        /// Sets the values of controls to be the same as the first visible control
        /// </summary>
        /// <remarks>Controls are expected to be either all <see cref="TextBox"/>es or all
        /// <see cref="CheckBox"/>es. There should be exactly one Visible control</remarks>
        /// <param name="controls">The <see cref="Control"/>s to sync</param>
        private static void SyncControls(params Control[] controls)
        {
            var textBoxes = controls.OfType<TextBox>();
            var enabledTextBox = textBoxes.Where(textBox => textBox.Visible).FirstOrDefault();
            if (enabledTextBox != null)
            {
                foreach (var textBox in textBoxes.Where(textBox => !textBox.Visible))
                {
                    textBox.Text = enabledTextBox.Text;
                }
            }
            var checkBoxes = controls.OfType<CheckBox>();
            var enabledCheckBox = checkBoxes.Where(checkBox => checkBox.Visible).FirstOrDefault();
            if (enabledCheckBox != null)
            {
                foreach (var checkBox in checkBoxes.Where(checkBox => !checkBox.Visible))
                {
                    checkBox.Checked = enabledCheckBox.Checked;
                }
            }
        }

        /// <summary>
        /// Sets the values of corresponding controls to be the same as the first visible control
        /// </summary>
        private void SyncCorrespondingControls()
        {
            // No need to update machine since only invisible (irrelevant) controls are being updated
            // so set flag to ignore triggered updates
            _suspendMachineUpdates = true;
            try
            {
                // Input paths
                SyncControls(
                    paginationFileListOrFolderTextBox,
                    documentCategorizationInputFolderTextBox,
                    documentCategorizationCsvTextBox,
                    attributeCategorizationFileListOrFolderTextBox
                );

                // Answer paths
                SyncControls(
                    documentCategorizationFolderAnswerTextBox,
                    paginationAnswerVoaTextBox
                );

                // Random number seed
                SyncControls(
                    documentCategorizationCsvRandomNumberSeedTextBox,
                    documentCategorizationFolderRandomNumberSeedTextBox,
                    paginationRandomNumberSeedTextBox,
                    attributeCategorizationRandomNumberSeedTextBox
                );

                // Negative class name
                SyncControls(
                    documentCategorizationCsvNegativeClassNameTextBox,
                    documentCategorizationFolderNegativeClassNameTextBox,
                    paginationNegativeClassNameTextBox,
                    attributeCategorizationNegativeClassNameTextBox
                );

                // Training percentage
                SyncControls(
                    documentCategorizationCsvTrainingPercentageTextBox,
                    documentCategorizationFolderTrainingPercentageTextBox,
                    paginationTrainingPercentageTextBox,
                    attributeCategorizationTrainingPercentageTextBox
                );

                // Feature VOA location
                SyncControls(
                    documentCategorizationCsvFeatureVoaTextBox,
                    documentCategorizationFolderFeatureVoaTextBox,
                    paginationFeatureVoaTextBox,
                    attributeCategorizationCandidateVoaTextBox
                );
            }
            finally
            {
                _suspendMachineUpdates = false;
            }
        }

        /// <summary>
        /// Set default values for all UI controls so that proper defaults are presented after clicking
        /// New
        /// </summary>
        private void SetDefaultValues()
        {
            try
            {
                _suspendMachineUpdates = true;
                _textValueOrCheckStateChangedSinceCreation = false;
                attributeFeatureFilterTextBox.Text = "*@Feature";
                documentCategorizationCsvFeatureVoaTextBox.Text = "<SourceDocName>.protofeatures.voa";
                documentCategorizationCsvRandomNumberSeedTextBox.Text = "0";
                documentCategorizationCsvTextBox.Text = _DEFAULT_INPUT_LOCATION;
                documentCategorizationCsvTrainingPercentageTextBox.Text = "80";
                documentCategorizationCsvNegativeClassNameTextBox.Text = LearningMachineDataEncoder.UnknownCategoryName;
                documentCategorizationFolderAnswerTextBox.Text = "$FileOf($DirOf(<SourceDocName>))";
                documentCategorizationFolderFeatureVoaTextBox.Text = "<SourceDocName>.protofeatures.voa";
                documentCategorizationFolderRandomNumberSeedTextBox.Text = "0";
                documentCategorizationFolderTrainingPercentageTextBox.Text = "80";
                documentCategorizationFolderNegativeClassNameTextBox.Text = LearningMachineDataEncoder.UnknownCategoryName;
                documentCategorizationInputFolderTextBox.Text = _DEFAULT_INPUT_LOCATION;
                documentCategorizationRadioButton.Checked = true;
                folderSearchRadioButton.Checked = true;
                maxFeaturesTextBox.Text = "2000";
                maxShingleSizeTextBox.Text = "5";
                maximumTrainingIterationsTextBox.Text = "500";
                svmAutoComplexityCheckBox.Checked = true;
                svmComplexityTextBox.Text = "1.0";
                svmUseF1ScoreRadioButton.Checked = true;
                svmWeightRatioTextBox.Text = "";
                svmConditionallyApplyWeightRatioCheckBox.Checked = true;
                svmCacheSizeTextBox.Text = "4000";
                calibrateForProbabilitiesCheckBox.Checked = true;
                unknownCutoffTextBox.Text = "0.5";
                useUnknownCheckBox.Checked = true;
                numberOfCandidateNetwordsTextBox.Text = "5";
                paginationAnswerVoaTextBox.Text = "<SourceDocName>.evoa";
                paginationFeatureVoaTextBox.Text = "<SourceDocName>.protofeatures.voa";
                paginationFileListOrFolderTextBox.Text = _DEFAULT_INPUT_LOCATION;
                paginationRandomNumberSeedTextBox.Text = "0";
                paginationTrainingPercentageTextBox.Text = "80";
                paginationNegativeClassNameTextBox.Text = LearningMachineDataEncoder.NotFirstPageCategory;
                attributeCategorizationCandidateVoaTextBox.Text = "<SourceDocName>.labeled.voa";
                attributeCategorizationFileListOrFolderTextBox.Text = _DEFAULT_INPUT_LOCATION;
                attributeCategorizationRandomNumberSeedTextBox.Text = "0";
                attributeCategorizationTrainingPercentageTextBox.Text = "80";
                attributeCategorizationNegativeClassNameTextBox.Text = "";
                sigmoidAlphaTextBox.Text = "2.0";
                sizeOfHiddenLayersTextBox.Text = "25";
                specifiedPagesTextBox.Text = "";
                useAttributeFeatureFilterCheckBox.Checked = true;
                useAutoBagOfWordsCheckBox.Checked = true;
                useCrossValidationSetsCheckBox.Checked = true;
            }
            finally
            {
                _suspendMachineUpdates = false;
            }
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
                    HandleSaveMachineButton_Click(this, cancel);
                    if (cancel.Cancel)
                    {
                        confirm = false;
                    }
                }
                else if (response == System.Windows.Forms.DialogResult.Cancel)
                {
                    confirm = false;
                }
            }

            return confirm;
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Opens the <see cref="EditFeatures"/> form and handles the result
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleEditFeaturesButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentLearningMachine.Encoder.AreEncodingsComputed)
                {
                    LearningMachineDataEncoder tempEncoder = null;
                    using (new TemporaryWaitCursor())
                        tempEncoder = CurrentLearningMachine.Encoder.DeepClone();

                    using (var win = new EditFeatures(tempEncoder,
                        _fileName == _NEW_FILE_NAME ? null : Path.GetDirectoryName(_fileName)))
                    {
                        var result = win.ShowDialog();

                        if (result == System.Windows.Forms.DialogResult.OK
                            && !tempEncoder.IsConfigurationEqualTo(CurrentLearningMachine.Encoder))
                        {
                            // Create an untrained copy
                            var tempLM = BuildLearningMachine();

                            // Set encoder to modified encoder
                            tempLM.Encoder = tempEncoder;

                            // Update current machine
                            CurrentLearningMachine = tempLM;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39920");
            }
        }

        /// <summary>
        /// Opens a dialog for training/testing the currently configured learning machine
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleTrainTestButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_valid)
                {
                    FocusFirstInvalid();
                    return;
                }

                if (_trainingTestingDialog == null)
                {
                    _trainingTestingDialog = new TrainingTesting(this);
                }

                _trainingTestingDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40028");
            }
        }

        /// <summary>
        /// Opens a dialog and starts compute features/encodings process
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleComputeFeaturesButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_valid)
                {
                    FocusFirstInvalid();
                    return;
                }

                LearningMachine tempLM = BuildLearningMachine();

                using (var win = new ComputingFeaturesStatus(tempLM))
                {
                    var result = win.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && tempLM.Encoder.AreEncodingsComputed)
                    {
                        // Set the current machine to the updated copy
                        CurrentLearningMachine = tempLM;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39921");
            }
        }

        /// <summary>
        /// Opens the <see cref="ViewAnswers"/> form
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleViewAnswerListButton_Click(object sender, EventArgs e)
        {
            try
            {
                var copy = _currentLearningMachine.Encoder.DeepClone();
                using (var win = new ViewAnswers(copy, _fileName == _NEW_FILE_NAME ? null : _fileName))
                {
                    win.ShowDialog();
                }
                if (!copy.AnswerCodeToName
                    .SequenceEqual(_currentLearningMachine.Encoder.AnswerCodeToName))
                {
                    _currentLearningMachine.Encoder = copy;
                    Dirty = true;
                }
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI41469").Display();
            }
        }

        /// <summary>
        /// Handles various events that affect the enabled/visible states of other controls,
        /// e.g., <see cref="CheckBox.CheckedChanged"/> and <see cref="ComboBox.SelectedIndexChanged"/> events
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleControlStateChanged(object sender, EventArgs e)
        {
            try
            {
                var checkBox = sender as CheckBox;
                if (checkBox != null && !_suspendMachineUpdates && !_updatingMachine)
                {
                    _textValueOrCheckStateChangedSinceCreation = true;
                }
                SetControlStates();
                UpdateLearningMachine();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40029");
            }
        }

        /// <summary>
        /// Handles the text changed event for text box controls.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_suspendMachineUpdates && !_updatingMachine)
                {
                    Dirty = true;
                    var textBox = sender as TextBox;
                    if (textBox != null && !textBox.Focused)
                    {
                        UpdateLearningMachine();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40030");
            }
        }

        /// <summary>
        /// Handles the leave event for text box controls. Updates the current 
        /// learning machine instance (UpdateLearningMachine)
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                if (!_suspendMachineUpdates && !_updatingMachine)
                {
                    _textValueOrCheckStateChangedSinceCreation = true;
                }
                UpdateLearningMachine();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41468");
            }
        }

        /// <summary>
        /// Handles the selected index changed event for the tab control so that values of any newly
        /// visible controls are copied to corresponding invisible controls
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleConfigurationTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_textValueOrCheckStateChangedSinceCreation)
                {
                    SyncCorrespondingControls();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40031");
            }
        }

        /// <summary>
        /// Handles the click event for folder/file browse buttons, populates initial directory based
        /// on current value of the corresponding text control
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleBrowseButtonClick(object sender, EventArgs e)
        {
            try
            {
                var browseButton = (Extract.Utilities.Forms.BrowseButton)sender;
                browseButton.FileOrFolderPath = Path.GetDirectoryName(browseButton.TextControl.Text);
            }
            // Ignore errors setting initial directory (in case illegal characters are in the path text)
            catch (ArgumentException)
            { }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40032");
            }
        }

        /// <summary>
        /// Handles the click event for the save button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleSaveMachineButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Don't allow saving if in invalid state
                if (!_valid)
                {
                    // Set Cancel in case this is called from PromptForDirtyFile()
                    var cancelEventArgs = e as CancelEventArgs;
                    if (cancelEventArgs != null)
                    {
                        cancelEventArgs.Cancel = true;
                    }

                    FocusFirstInvalid();
                    return;
                }

                if (_fileName.Equals(_NEW_FILE_NAME, StringComparison.Ordinal))
                {
                    HandleSaveMachineAsButton_Click(sender, e);
                }
                else
                {
                    Cursor.Current = Cursors.WaitCursor;
                    _currentLearningMachine.Save(_fileName);
                    _savedLearningMachine = _currentLearningMachine.DeepClone();
                    Dirty = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39917");
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
        private void HandleSaveMachineAsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_valid)
                {
                    FocusFirstInvalid();
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Learning machine|*.lm|All files|*.*";
                    if (!_fileName.Equals(_NEW_FILE_NAME, StringComparison.Ordinal))
                    {
                        saveDialog.FileName = Path.GetFileName(_fileName);
                        saveDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }

                    var result = saveDialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        _currentLearningMachine.Save(saveDialog.FileName);
                        _savedLearningMachine = _currentLearningMachine.DeepClone();
                        _fileName = saveDialog.FileName;
                        Dirty = false;
                    }
                    else if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        // Set Cancel in case this is called from PromptForDirtyFile()
                        var cancelEventArgs = e as CancelEventArgs;
                        if (cancelEventArgs != null)
                        {
                            cancelEventArgs.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39918");
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
        private void HandleNewMachineButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    return;
                }

                SetDefaultValues();
                _savedLearningMachine = MakeNewMachine();
                _previousLearningMachine = null;
                _fileName = _NEW_FILE_NAME;
                CurrentLearningMachine = _savedLearningMachine;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39919");
            }
        }

        /// <summary>
        /// Handles the click event for the open button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleOpenMachineButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!PromptForDirtyFile())
                {
                    return;
                }

                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "Learning machine|*.lm|All files|*.*";
                    if (!_fileName.Equals(_NEW_FILE_NAME, StringComparison.Ordinal))
                    {
                        openDialog.InitialDirectory = Path.GetDirectoryName(_fileName);
                    }
                    if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Treat saved machine as having non-default values
                        _textValueOrCheckStateChangedSinceCreation = true;
                        Cursor.Current = Cursors.WaitCursor;
                        _savedLearningMachine = LearningMachine.Load(openDialog.FileName);
                        _previousLearningMachine = null;
                        _fileName = openDialog.FileName;
                        _valid = true;
                        CurrentLearningMachine = _savedLearningMachine;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39926");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the Click event of the AttributeCategorizationCreateCandidateVoaButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleAttributeCategorizationCreateCandidateVoaButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_valid)
                {
                    FocusFirstInvalid();
                    return;
                }

                using (var win = new LabelAttributesConfigurationDialog(this))
                {
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI41470").Display();
            }
        }

        /// <summary>
        /// Handles the Click event of the writeDataToCsvButton by opening a <see cref="WritingDataToCsvStatus"/>
        /// dialog.
        /// </summary>
        private void HandleWriteDataToCsvButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_valid)
                {
                    FocusFirstInvalid();
                    return;
                }

                using (var win = new WritingDataToCsvStatus(CurrentLearningMachine))
                {
                    win.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44892");
            }
        }

        private void HandleDangerModeButton_Click(object sender, EventArgs e)
        {
            if (!_dangerMode)
            {
                _dangerMode = true;
                dangerModeToolStripStatusLabel.Text = "[Danger mode]";
                dangerModeButton.Text = "Safe mode";
            }
            else
            {
                _dangerMode = false;
                dangerModeToolStripStatusLabel.Text = "[Safe mode]";
                dangerModeButton.Text = "Danger mode";
            }

        }

        #endregion Event Handlers
    }
}