using Extract.IDShieldStatisticsReporter.Properties;
using Extract.Licensing;
using Extract.Redaction;
using Extract.Utilities;
using Extract.Utilities.Forms;
using EXTRACTREDACTIONTESTERLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_REDACTIONCUSTOMCOMPONENTSLib;
using UCLID_TESTINGFRAMEWORKINTERFACESLib;

namespace Extract.IDShieldStatisticsReporter
{
    /// <summary>
    /// Enables redaction accuracy statistics to be generated from feedback data sets.
    /// </summary>
    public partial class IDShieldStatisticsReporterForm : Form, ITestResultLogger
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(IDShieldStatisticsReporterForm).ToString();

        /// <summary>
        /// The name of the automated redaction method.
        /// </summary>
        const string _AUTOMATED_REDACTION = "Automated redaction";

        /// <summary>
        /// The name of the standard verification method.
        /// </summary>
        const string _STANDARD_VERIFICATION = "Standard verification";

        /// <summary>
        /// The name of the hybrid method.
        /// </summary>
        const string _HYBRID = "Hybrid";

        /// <summary>
        /// The ProgID of the IDShieldTester COM class.
        /// </summary>
        const string _IDSHIELD_TESTER_PROGID = "EXTRACTRedactionTester.IDShieldTester.1";

        /// <summary>
        /// The prefix that ID Shield tester prepends to the name of every test output folder.
        /// </summary>
        const string _ANAYLYSIS_FOLDER_PREFIX = "Analysis - ";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.ApplicationDataPath, "ID Shield", "StatisticsReporterForm.xml");

        /// <summary>
        /// Name for the mutex used to serialize persistance of the control and form layout.
        /// </summary>
        static readonly string _MUTEX_STRING = "186DAD15-746F-411F-AE2E-12C241D6989F";

        #endregion Constants

        #region Fields 

        /// <summary>
        /// The ID Shield tester settings.
        /// </summary>
        readonly IDShieldTesterSettings _testerSettings = new IDShieldTesterSettings();

        /// <summary>
        /// The configuration file settings for the statistics reporter
        /// </summary>
        readonly ConfigSettings<Settings> _config;

        /// <summary>
        /// The target test folder.
        /// </summary>
        IDShieldTesterFolder _testFolder;

        /// <summary>
        /// A <see cref="IDShieldVOAFileContentsCondition"/> used to configure the automated
        /// redation conditions.
        /// </summary>
        IDShieldVOAFileContentsCondition _automatedConditionObject;

        /// <summary>
        /// A <see cref="IDShieldVOAFileContentsCondition"/> used to configure the verification
        /// conditions.
        /// </summary>
        IDShieldVOAFileContentsCondition _verificationConditionObject;

        /// <summary>
        /// Specifies the filename of a template that should be used to generate a custom report
        /// when analysis is run.
        /// </summary>
        string _customReportTemplate;

        /// <summary>
        /// The results folder based on the current feedback folder settings and result
        /// combo box selection.
        /// </summary>
        string _currentResultsFolder;

        /// <summary>
        /// The total number of test cases in the ID Shield test.
        /// </summary>
        int _testCaseCount;

        /// <summary>
        /// The total number of test cases failures in the ID Shield test. 
        /// </summary>
        int _testCaseFailureCount;

        /// <summary>
        /// The most recently added test case file.
        /// </summary>
        string _currentTestCaseFile;

        /// <summary>
        /// An exception that was thrown when trying to execute the ID Shield component test.
        /// </summary>
        ExtractException _testComponentException;

        /// <summary>
        /// The ID Shield Tester instance used to conduct tests.
        /// </summary>
        readonly IDShieldTesterClass _idShieldTester = new IDShieldTesterClass();

        /// <summary>
        /// Saves/restores window state info and provides full screen mode.
        /// </summary>
        FormStateManager _formStateManager;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="IDShieldStatisticsReporterForm"/> instance.
        /// </summary>
        public IDShieldStatisticsReporterForm()
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI28534",
                    _OBJECT_NAME);

                InitializeComponent();

                _config = new ConfigSettings<Settings>(
                    Path.Combine(FileSystemMethods.ApplicationDataPath, 
                    "IDShieldStatisticsReporter.config"));

                // Create an condition object so that its configuration screen can be used to
                // configure the automated condition.
                _automatedConditionObject = new IDShieldVOAFileContentsConditionClass();
                
                // Don't show the error condition settings; for testing it will always be an error
                // condition.
                _automatedConditionObject.ConfigureConditionsOnly = true;

                // Create an condition object so that its configuration screen can be used to
                // configure the verification condition.
                _verificationConditionObject = new IDShieldVOAFileContentsConditionClass();

                // Don't show the error condition settings; for testing it will always be an error
                // condition.
                _verificationConditionObject.ConfigureConditionsOnly = true;

                _idShieldTester.SetResultLogger(this);

                InitializeTesterSettings();

                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    // Loads/save UI state properties
                    _formStateManager = new FormStateManager(
                        this, _FORM_PERSISTENCE_FILE, _MUTEX_STRING, false, null);
                }

                // Set the form Icon to the IDShieldTester Icon
                Icon = Resources.IDShieldTester;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28535", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="IDShieldTesterSettings"/>.
        /// </summary>
        /// <value>The <see cref="IDShieldTesterSettings"/>.</value>
        public IDShieldTesterSettings TesterSettings
        {
            get
            {
                return _testerSettings;
            }
        }

        /// <summary>
        /// Gets the <see cref="IDShieldTesterFolder"/>.
        /// </summary>
        /// <value>The [TestFolder].</value>
        public IDShieldTesterFolder TestFolder
        {
            get
            {
                return _testFolder;
            }
        }

        /// <summary>
        /// Gets or sets the filename of a template that should be used to generate a custom report
        /// when analysis is run.
        /// </summary>
        /// <value>
        /// The the filename of the custom report template.
        /// </value>
        public string CustomReportTemplate
        {
            get
            {
                return _customReportTemplate;
            }

            set
            {
                _customReportTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether analysis should be run automatically when the
        /// application is launched.
        /// </summary><value><see langword="true"/> if analysis should be run automatically when the
        /// application is launched;otherwise, <see langword="false"/>.
        /// </value>
        public bool AutoRun
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether analysis should be run automatically when the
        /// application is launched without displaying the UI or any message boxes and then
        /// immediately exit. 
        /// </summary>
        /// <value><see langword="true"/> if  analysis should be run automatically and silently;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool Silent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of a report to print every time analysis is run.
        /// </summary>
        /// <value>
        /// The the name of a report to print every time analysis is run.
        /// </value>
        public string ReportToPrint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the log file to log all exceptions to.
        /// </summary>
        /// <value>
        /// The name of the log file to log all exceptions to.
        /// </value>
        public string LogFileName
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="UserControl.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // If running in silent mode, ensure the main form functions, but is not displayed.
                if (Silent)
                {
                    // Makes form invisible.
                    Opacity = 0;

                    // ...and we don't want it to appear in the task bar
                    ShowInTaskbar = false;

                    // ...and we don't want it to have a close button
                    // (for sanity, not sure if it would be possible to use it with opacity = 0)
                    ControlBox = false;

                    // ...and we don't want the user to be able to alt-tab to it
                    FormBorderStyle = FormBorderStyle.FixedToolWindow;

                    // ...and we don't want the user to be able to do anything in it.
                    Enabled = false;
                }

                base.OnLoad(e);

                if (LicenseUtilities.IsLicensed(LicenseIdName.RuleDevelopmentToolkitObjects))
                {
                    _limitTypesCheckBox.Text += " (separate types using a pipe or comma)";
                }
                else
                {
                    _limitTypesCheckBox.Text += " (separate types using a comma)";
                }

                // Determine the type of test being done.
                if (!string.IsNullOrEmpty(_testFolder.TestFolderName))
                {
                    _feedbackFolderTextBox.Text = _testFolder.TestFolderName;
                }
                else
                {
                    _feedbackFolderTextBox.Text = _config.Settings.LastFeedbackFolder;
                }

                if (_testerSettings.OutputHybridStats)
                {
                    _analysisTypeComboBox.Text = _HYBRID;
                }
                else if (_testerSettings.OutputAutomatedStatsOnly)
                {
                    _analysisTypeComboBox.Text = _AUTOMATED_REDACTION;
                }
                else
                {
                    _analysisTypeComboBox.Text = _STANDARD_VERIFICATION;
                }

                // Populate the types to be tested 
                if (!string.IsNullOrEmpty(_testerSettings.TypesToBeTested))
                {
                    _limitTypesCheckBox.Checked = true;
                    _dataTypesTextBox.Text = _testerSettings.TypesToBeTested;
                }

                // Initialize the automated and verification condition objects
                InitializeCondition(_automatedConditionObject, _testerSettings.AutomatedCondition,
                    _testerSettings.AutomatedConditionQuantifier, _testerSettings.DocTypesToRedact);

                InitializeCondition(_verificationConditionObject, _testerSettings.VerificationCondition,
                    _testerSettings.VerificationConditionQuantifier, _testerSettings.DocTypesToVerify);

                // Initialize the automated redaction check boxes
                if (_testerSettings.QueryForAutomatedRedaction != null)
                {
                    List<string> attributesToRedact =
                        new List<string>(_testerSettings.QueryForAutomatedRedaction.Split('|'));
                    _redactHCDataCheckBox.Checked = (attributesToRedact.IndexOf("HCData") >= 0);
                    _redactMCDataCheckBox.Checked = (attributesToRedact.IndexOf("MCData") >= 0);
                    _redactLCDataCheckBox.Checked = (attributesToRedact.IndexOf("LCData") >= 0);
                    _redactManualDataCheckBox.Checked = (attributesToRedact.IndexOf("Manual") >= 0);
                }

                _dataTypesTextBox.Enabled = _limitTypesCheckBox.Checked;
                UpdateControlsBasedOnAnalysisType();
                PopulateResultsList();

                if (AutoRun)
                {
                    Analyze();
                }
            }
            catch (Exception ex)
            {
                ReportException(ex.AsExtract(("ELI28656")));
            }
        }

        #endregion Overrides

        /// <summary>
        /// Initializes an <see cref="IDShieldVOAFileContentsCondition"/> to be used to configure
        /// data file conditions for the test.
        /// </summary>
        /// <param name="conditionObject">The <see cref="IDShieldVOAFileContentsCondition"/>
        /// </param>
        /// <param name="condition">The settings file condition value that needs to be applied to
        /// the <see cref="IDShieldVOAFileContentsCondition"/>.</param>
        /// <param name="quantifier">The settings file quantifier value that needs to be applied to
        /// the <see cref="IDShieldVOAFileContentsCondition"/>.</param>
        /// <param name="docTypes">The settings file doc types value that needs to be applied to
        /// the <see cref="IDShieldVOAFileContentsCondition"/>.</param>
        static void InitializeCondition(IDShieldVOAFileContentsCondition conditionObject, string condition,
            string quantifier, string docTypes)
        {
            // Initialize the condition.
            conditionObject.CheckDataContents = false;
            if (condition != null)
            {
                List<string> attributes = new List<string>(condition.Split('|'));
                if (attributes.Count > 0)
                {
                    conditionObject.CheckDataContents = true;
                    conditionObject.LookForHCData = (attributes.IndexOf("HCData") >= 0);
                    conditionObject.LookForMCData = (attributes.IndexOf("MCData") >= 0);
                    conditionObject.LookForLCData = (attributes.IndexOf("LCData") >= 0);
                    conditionObject.LookForManualData = (attributes.IndexOf("Manual") >= 0);
                    conditionObject.LookForClues = (attributes.IndexOf("Clues") >= 0);
                }
            }

            // Initialize the quatifier.
            if (quantifier != null)
            {
                if (quantifier.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    conditionObject.AttributeQuantifier = EAttributeQuantifier.kAny;
                }
                else if (quantifier.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    conditionObject.AttributeQuantifier = EAttributeQuantifier.kNone;
                }
                else if (quantifier.Equals("at least one of each", StringComparison.OrdinalIgnoreCase))
                {
                    conditionObject.AttributeQuantifier = EAttributeQuantifier.kOneOfEach;
                }
                else if (quantifier.Equals("only any", StringComparison.OrdinalIgnoreCase))
                {
                    conditionObject.AttributeQuantifier = EAttributeQuantifier.kOnlyAny;
                }
            }

            // Initialize the quatifier the doc types.
            if (docTypes != null)
            {
                string[] docTypeArray = docTypes.Split(',');
                if (docTypeArray.Length > 0)
                {
                    conditionObject.CheckDocType = true;

                    foreach (string docType in docTypeArray)
                    {
                        conditionObject.DocTypes.PushBack(docType);
                    }
                }
            }
        }

        /// <summary>
        /// Converts settings from a <see cref="IDShieldVOAFileContentsCondition"/> to strings that
        /// can be saved to the IDShield Tester settings file.
        /// </summary>
        /// <param name="conditionObject">The <see cref="IDShieldVOAFileContentsCondition"/>.</param>
        /// <param name="condition">A <see langword="string"/> representing the condition from the
        /// <see cref="IDShieldVOAFileContentsCondition"/>.</param>
        /// <param name="quantifier">A <see langword="string"/> representing the quantifier from the
        /// <see cref="IDShieldVOAFileContentsCondition"/>.</param>
        /// <param name="docTypes">A <see langword="string"/> represneting the doc types from the
        /// <see cref="IDShieldVOAFileContentsCondition"/>.</param>
        static void ApplyCondition(IDShieldVOAFileContentsCondition conditionObject, out string condition,
            out string quantifier, out string docTypes)
        {
            // Initialize all settings to null.
            condition = null;
            quantifier = null;
            docTypes = null;

            // Convert the condition and quantifier to a string (if specified).
            if (conditionObject.CheckDataContents)
            {
                StringBuilder conditionBuilder = new StringBuilder();
                conditionBuilder.Append(conditionObject.LookForHCData ? "HCData" : "");
                conditionBuilder.Append(conditionObject.LookForMCData 
                    ? (conditionBuilder.Length > 0 ? "|MCData" : "MCData") : "");
                conditionBuilder.Append(conditionObject.LookForLCData 
                    ? (conditionBuilder.Length > 0 ? "|LCData" : "LCData") : "");
                conditionBuilder.Append(conditionObject.LookForManualData ? 
                    (conditionBuilder.Length > 0 ? "|Manual" : "Manual") : "");
                conditionBuilder.Append(conditionObject.LookForClues 
                    ? (conditionBuilder.Length > 0 ? "|Clues" : "Clues") : "");

                condition = conditionBuilder.ToString();

                switch (conditionObject.AttributeQuantifier)
                {
                    case EAttributeQuantifier.kAny: quantifier = "any"; break;
                    case EAttributeQuantifier.kNone: quantifier = "none"; break;
                    case EAttributeQuantifier.kOneOfEach: quantifier = "at least one of each"; break;
                    case EAttributeQuantifier.kOnlyAny: quantifier = "only any"; break;
                    default: throw new ExtractException("ELI28657", "Invalid attribute quantifier!");
                }
            }

            // Convert the doc types to a string (if specified).
            if (conditionObject.CheckDocType)
            {
                VariantVector docTypesVector = conditionObject.DocTypes;
                StringBuilder docTypesBuilder = new StringBuilder();
                int docTypeCount = docTypesVector.Size;
                for (int i = 0; i < docTypeCount; i++)
                {
                    if (i > 0)
                    {
                        docTypesBuilder.Append("|");
                    }
                    docTypesBuilder.Append(docTypesVector[i]);
                }

                docTypes = docTypesBuilder.ToString();
            }
        }

        /// <summary>
        /// Saves the current settings to an ID Shield tester settings file.
        /// </summary>
        /// <param name="settingFileName">The name of the file to write the settings to.</param>
        /// <returns><see langword="true"/> if settings were applied successfully,
        /// <see langword="false"/> otherwise.</returns>
        bool ApplySettings(string settingFileName)
        {
            try
            {
                if (!Directory.Exists(_feedbackFolderTextBox.Text))
                {
                    _tabControl.SelectTab(_feedbackDataTab);
                    ActiveControl = _feedbackFolderTextBox;
                    _feedbackFolderTextBox.SelectAll();

                    if (Silent)
                    {
                        throw new ExtractException("ELI34966",
                            "Feedback data folder has not been specified");
                    }
                    
                    MessageBox.Show("Specify a valid feedback folder.",
                        "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1, 0);

                    return false;
                }

                _testFolder.TestFolderName = _feedbackFolderTextBox.Text;
                if (string.IsNullOrEmpty(_testerSettings.ExplicitOutputFilesFolder))
                {
                    _testerSettings.OutputFilesFolder = _feedbackFolderTextBox.Text;
                }
                _testerSettings.OutputHybridStats = _analysisTypeComboBox.Text == _HYBRID;
                _testerSettings.OutputAutomatedStatsOnly =
                    (_analysisTypeComboBox.Text == _AUTOMATED_REDACTION);

                // Initialize the automated redaction condition
                if (!_testerSettings.OutputAutomatedStatsOnly && !_testerSettings.OutputHybridStats)
                {
                    _testerSettings.AutomatedCondition = "";
                    _testerSettings.AutomatedConditionQuantifier = "any";
                    _testerSettings.DocTypesToRedact = null;
                }
                else
                {
                    string automatedCondition, automatedConditionQuantifier, docTypes;
                    ApplyCondition(_automatedConditionObject,
                        out automatedCondition, out automatedConditionQuantifier, out docTypes);
                    _testerSettings.AutomatedCondition = automatedCondition;
                    _testerSettings.AutomatedConditionQuantifier = automatedConditionQuantifier;
                    _testerSettings.DocTypesToRedact = docTypes;
                }

                // Initialize the verification condition
                if (_testerSettings.OutputAutomatedStatsOnly)
                {
                    _testerSettings.VerificationCondition = "";
                    _testerSettings.VerificationConditionQuantifier = "any";
                    _testerSettings.DocTypesToVerify = null;
                }
                else
                {
                    string verificationCondition, verificationConditionQuantifier, docTypes;
                    ApplyCondition(_verificationConditionObject, out verificationCondition,
                        out verificationConditionQuantifier, out docTypes);
                    _testerSettings.VerificationCondition = verificationCondition;
                    _testerSettings.VerificationConditionQuantifier = verificationConditionQuantifier;
                    _testerSettings.DocTypesToVerify = docTypes;
                }

                if (_limitTypesCheckBox.Checked)
                {
                    if (string.IsNullOrEmpty(_dataTypesTextBox.Text))
                    {
                        _tabControl.SelectTab(_analyzeTab);
                        ActiveControl = _dataTypesTextBox;
                        _dataTypesTextBox.SelectAll();

                        if (Silent)
                        {
                            throw new ExtractException("ELI34967", "Invalid data types filter");
                        }

                        MessageBox.Show("Specify a comma delimited list of types to be tested or " +
                                        "uncheck  the \"Limit data types to be tested box\".",
                            "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1, 0);
                        
                        return false;
                    }

                    _testerSettings.TypesToBeTested = _dataTypesTextBox.Text;
                }
                else
                {
                    _testerSettings.TypesToBeTested = null;
                }

                StringBuilder queryForAutomatedRedaction = new StringBuilder();
                queryForAutomatedRedaction.Append(_redactHCDataCheckBox.Checked ? "HCData" : "");
                queryForAutomatedRedaction.Append(_redactMCDataCheckBox.Checked 
                    ? (queryForAutomatedRedaction.Length > 0 ? "|MCData" : "MCData") : "");
                queryForAutomatedRedaction.Append(_redactLCDataCheckBox.Checked 
                    ? (queryForAutomatedRedaction.Length > 0 ? "|LCData" : "LCData") : "");
                queryForAutomatedRedaction.Append(_redactManualDataCheckBox.Checked
                    ? (queryForAutomatedRedaction.Length > 0 ? "|Manual" : "Manual") : "");
                _testerSettings.QueryForAutomatedRedaction = queryForAutomatedRedaction.ToString();

                _testerSettings.Save(settingFileName);

                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28658", ex);
            }
        }

        #region Event handlers

        /// <summary>
        /// Handles the automated condition config button <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleAutomatedFileConditionButtonClick(object sender, EventArgs e)
        {
            try
            {
                ObjectPropertiesUI configurationScreen = new ObjectPropertiesUIClass();

                ICopyableObject conditionCopySource = (ICopyableObject)_automatedConditionObject;
                IDShieldVOAFileContentsCondition conditionCopy =
                    (IDShieldVOAFileContentsCondition)conditionCopySource.Clone();

                if (configurationScreen.DisplayProperties1(
                        conditionCopy, "Configure automated condition"))
                {
                    _automatedConditionObject = conditionCopy;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28659", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the verification condition config button <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleVerificationFileConditionButtonClick(object sender, EventArgs e)
        {
            try
            {
                ObjectPropertiesUI configurationScreen = new ObjectPropertiesUIClass();

                ICopyableObject conditionCopySource = (ICopyableObject)_verificationConditionObject;
                IDShieldVOAFileContentsCondition conditionCopy =
                    (IDShieldVOAFileContentsCondition)conditionCopySource.Clone();

                if (configurationScreen.DisplayProperties1(
                        conditionCopy, "Configure verification condition"))
                {
                    _verificationConditionObject = conditionCopy;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28660", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the analyze button <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleAnalyzeButtonClick(object sender, EventArgs e)
        {
            try
            {
                Analyze();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28661", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the advanced feedback options button <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFeedbackAdvancedOptionsClick(object sender, EventArgs e)
        {
            try
            {
                FeedbackAdvancedOptionsForm advancedOptionsForm =
                    new FeedbackAdvancedOptionsForm(_testFolder);
                advancedOptionsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28662", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the analysis type combo <see cref="ListBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleAnalysisTypeComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControlsBasedOnAnalysisType();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28663", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the limit types check box <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleLimitTypesCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _dataTypesTextBox.Enabled = _limitTypesCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28664", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the results selection combo <see cref="ListBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleResultsSelectionComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string newOutputFolder = _ANAYLYSIS_FOLDER_PREFIX + _resultsSelectionComboBox.Text;
                newOutputFolder = Path.Combine(_feedbackFolderTextBox.Text, newOutputFolder);
                _currentResultsFolder = Directory.Exists(newOutputFolder) ? newOutputFolder : "";

                PopulateStatisitics();
                PopulateFileLists();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28665", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles feedback folder text box <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFeedbackFolderTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                PopulateResultsList();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28666", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the file list selection combo <see cref="ListBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFileListSelectionComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _fileListListBox.Items.Clear();

                string fileList =
                    Path.Combine(_currentResultsFolder, _fileListSelectionComboBox.Text);

                if (File.Exists(fileList))
                {
                    string[] listItems = File.ReadAllLines(fileList);
                    _fileListListBox.Items.AddRange(listItems);
                }

                _fileListCountTextBox.Text =
                    _fileListListBox.Items.Count.ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28667", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles file list list box <see cref="Control.DoubleClick"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFileListListBoxDoubleClick(object sender, EventArgs e)
        {
            try
            {
                // [FlexIDSCore:3913]
                // If no item was selected, there is nothing to display.
                if (_fileListListBox.SelectedItem == null)
                {
                    return;
                }

                string sourceDocName = _fileListListBox.SelectedItem.ToString();
                string commonComponentsDir = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location);

                // If the failed file list is being displayed, display the exception log.
                if (_fileListSelectionComboBox.Text.EndsWith("FailedTestCases.txt",
                    StringComparison.OrdinalIgnoreCase))
                {
                    string failedTestCaseLog =
                        Path.Combine(_currentResultsFolder, "FailedTestCases.uex");
                    if (File.Exists(failedTestCaseLog))
                    {
                        // Build process information structure to launch the UEX application
                        ProcessStartInfo processInfo = new ProcessStartInfo();
                        processInfo.FileName = Path.Combine(commonComponentsDir, "UEXViewer.exe");
                        processInfo.Arguments = "\"" + failedTestCaseLog + "\"";

                        Process.Start(processInfo);                     
                    }

                    return;
                }

                // Otherwise display the test output for the selected file.
                if (!string.IsNullOrEmpty(sourceDocName))
                {
                    string foundDataFile = sourceDocName + ".testoutput.voa";
                    ExtractException.Assert("ELI28668",
                        "Cannot find data file '" + foundDataFile + "'", File.Exists(foundDataFile));

                    // Build process information structure to launch the VOAFileViewer application
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.FileName = Path.Combine(commonComponentsDir, "VOAFileViewer.exe");
                    processInfo.Arguments = foundDataFile;

                    // Start the email file application
                    Process.Start(processInfo);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28669", ex);
                ee.AddDebugData("Event data", e, false);
                ReportException(ee);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_printButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePrintButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_reviewTabControl.SelectedTab == _customReportTab &&
                    !string.IsNullOrEmpty(_customReportTextBox.Text))
                {
                    Print(CustomReportName);
                }
                else if (_reviewTabControl.SelectedTab == _statisticsTab &&
                    !string.IsNullOrEmpty(_statisticsTextBox.Text))
                {
                    Print("Statistics");
                }
                else if (_reviewTabControl.SelectedTab == _fileListTab &&
                    !string.IsNullOrEmpty(_fileListSelectionComboBox.Text))
                {
                    Print(_fileListSelectionComboBox.Text);
                }
            }
            catch (Exception ex)
            {
                ReportException(ex.AsExtract("ELI34962"));
            }
        }

        #endregion Event handlers

        #region ITestResultLogger Members

        /// <summary>
        /// Handles the case that an exception was thrown from the component test.
        /// </summary>
        /// <param name="strComponentTestException">The stringized version of the exception.</param>
        public void AddComponentTestException(string strComponentTestException)
        {
            _testComponentException =
                ExtractException.FromStringizedByteStream("ELI28675", strComponentTestException);
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        public bool AddEntriesToTestLogger
        {
            get
            {
                return false;
            }
            set
            {
                // Nothing to do.
            }
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="strTitle">unused</param>
        /// <param name="strLabel1">unused</param>
        /// <param name="strInput1">unused</param>
        /// <param name="strLabel2">unused</param>
        /// <param name="strInput2">unused</param>
        public void AddTestCaseCompareData(string strTitle, string strLabel1, string strInput1, string strLabel2, string strInput2)
        {
            // Nothing to do.
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="strTitle">unused</param>
        /// <param name="strTestCaseDetailNote">unused</param>
        public void AddTestCaseDetailNote(string strTitle, string strTestCaseDetailNote)
        {
            // Nothing to do.
        }

        /// <summary>
        /// Logs exceptions that cause tests to fail and appends them to a FailedTestCases list.
        /// </summary>
        /// <param name="strTestCaseException">The stringized exception generated by the test case.
        /// </param>
        /// <param name="vbFailTestCase"><see langword="true"/> if the test case failed as a result
        /// of the exception, <see langword="false"/> otherwise.</param>
        public void AddTestCaseException(string strTestCaseException, bool vbFailTestCase)
        {
            try
            {
                if (vbFailTestCase)
                {
                    string outputFileDirectory = _idShieldTester.OutputFileDirectory;

                    _testCaseFailureCount++;
                    ExtractException.Log(Path.Combine(outputFileDirectory, "FailedTestCases.uex"),
                        "ELI28677",
                        ExtractException.FromStringizedByteStream("ELI28678", strTestCaseException));

                    if (!string.IsNullOrEmpty(_currentTestCaseFile))
                    {
                        string sourceDocName =
                            _currentTestCaseFile.EndsWith(".nte", StringComparison.OrdinalIgnoreCase) 
                            ? FileSystemMethods.GetFullPathWithoutExtension(_currentTestCaseFile) 
                            : _currentTestCaseFile;

                        using (FileStream errorFileListStream =
                            File.Open(Path.Combine(outputFileDirectory, "FailedTestCases.txt"),
                                FileMode.Append, FileAccess.Write))
                        {
                            using (StreamWriter errorFileListWriter =
                                new StreamWriter(errorFileListStream))
                            {
                                errorFileListWriter.WriteLine(sourceDocName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28676", ex);
            }
        }

        /// <summary>
        /// Handles the case that a test case file has been added.
        /// </summary>
        /// <param name="strFileName">The filename of the added test case file.</param>
        public void AddTestCaseFile(string strFileName)
        {
            _currentTestCaseFile = strFileName;
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="strTitle">unused</param>
        /// <param name="strTestCaseMemo">unused</param>
        public void AddTestCaseMemo(string strTitle, string strTestCaseMemo)
        {
            // Nothing to do.
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="strTestCaseNote">unused</param>
        public void AddTestCaseNote(string strTestCaseNote)
        {
            // Nothing to do.
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        public void EndComponentTest()
        {
            // Nothing to do.
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="bResult"></param>
        public void EndTestCase(bool bResult)
        {
            // Nothing to do.
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        public void EndTestHarness()
        {
            // Nothing to do.
        }

        /// <summary>
        /// Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="strComponentDescription">unused</param>
        /// <param name="strOutputFileName">unused</param>
        public void StartComponentTest(string strComponentDescription, string strOutputFileName)
        {
            // Nothing to do.
        }

        /// <summary>
        ///  Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="strTestCaseID">unused</param>
        /// <param name="strTestCaseDescription">unused</param>
        /// <param name="ETestCaseType">unused</param>
        [CLSCompliant(false)]
        public void StartTestCase(string strTestCaseID, string strTestCaseDescription, ETestCaseType ETestCaseType)
        {
            // Note that summary info is added at the end which calls this method with a type of kSummaryTestCase.
            // Don't include these in the test case count.
            if (ETestCaseType != ETestCaseType.kSummaryTestCase)
            {
                _testCaseCount++;
            }
        }

        /// <summary>
        ///  Not used by <see cref="IDShieldStatisticsReporterForm"/>.
        /// </summary>
        /// <param name="strHarnessDescription">unused</param>
        public void StartTestHarness(string strHarnessDescription)
        {
            // Nothing to do.
        }

        #endregion ITestResultLogger

        #region Private Members

        /// <summary>
        /// Gets the name of the statistics report file.
        /// </summary>
        /// <value>
        /// The name of the statistics report file.
        /// </value>
        string StatisticsReportFileName
        {
            get
            {
                return Path.Combine(_currentResultsFolder, "statistics.txt");
            }
        }

        /// <summary>
        /// Gets the name of the custom report file.
        /// </summary>
        /// <value>
        /// The name of the custom report file.
        /// </value>
        string CustomReportName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(CustomReportTemplate);
            }
        }

        /// <summary>
        /// Initializes the tester settings.
        /// </summary>
        void InitializeTesterSettings()
        {
            // If at least one test folder specification was found, use the first, otherwise
            // create a new one to use.
            Collection<IDShieldTesterFolder> testFolders = _testerSettings.GetTestFolders();
            if (testFolders.Count > 0)
            {
                _testFolder = testFolders[0];
            }
            else
            {
                _testFolder = new IDShieldTesterFolder();
                _testerSettings.AddTestFolder(_testFolder);
            }

            // When generating stats with tool, we only want to see the final stats
            _testerSettings.OutputFinalStatsOnly = true;

            // We need file lists.
            _testerSettings.OutputAttributeNamesFileLists = true;

            // We want output files to be created.
            _testerSettings.CreateTestOutputVoaFiles = true;

            // Initialize the found and expected data locations as necessary.
            if (string.IsNullOrEmpty(_testFolder.FoundDataLocation))
            {
                _testFolder.FoundDataLocation = "<SourceDocName>.found.voa";
            }
            if (string.IsNullOrEmpty(_testFolder.ExpectedDataLocation))
            {
                _testFolder.ExpectedDataLocation = "<SourceDocName>.expected.voa";
            }
        }

        /// <summary>
        /// Analyzes the specified feedback data, generates reports and displays the results.
        /// </summary>
        void Analyze()
        {
            TemporaryFile settingsFile = null;
            TemporaryFile tclFile = null;

            try
            {
                using (new TemporaryWaitCursor())
                {
                    // Create a temporary tcl and dat file that will be used for this test.
                    settingsFile = new TemporaryFile(".dat", false);
                    tclFile = new TemporaryFile(".tcl", false);

                    if (!ApplySettings(settingsFile.FileName))
                    {
                        // If the settings could not be applied, don't run the test
                        return;
                    }

                    string tclFileContents = _IDSHIELD_TESTER_PROGID + ";;" + settingsFile.FileName;
                    File.WriteAllText(tclFile.FileName, tclFileContents);

                    VariantVectorClass testParameters = new VariantVectorClass();
                    testParameters.PushBack(_IDSHIELD_TESTER_PROGID);
                    testParameters.PushBack(settingsFile.FileName);

                    // Initialize all per-test fields.
                    _testCaseCount = 0;
                    _testCaseFailureCount = 0;
                    _testComponentException = null;
                    _currentTestCaseFile = null;

                    _idShieldTester.RunAutomatedTests(testParameters, tclFile.FileName);

                    if (!string.IsNullOrEmpty(CustomReportTemplate))
                    {
                        try
                        {
                            ExtractException.Assert("ELI34964",
                                "Custom report template file does not exist.",
                                File.Exists(CustomReportTemplate));

                            _idShieldTester.GenerateCustomReport(CustomReportTemplate);
                        }
                        catch (Exception ex)
                        {
                            ReportException(ex.AsExtract("ELI34965"));
                        }
                    }

                    // If an exception was thrown executing the overall test (usually due to a
                    // settings file issue), throw the exception from here.
                    if (_testComponentException != null)
                    {
                        throw _testComponentException;
                    }

                    // Re-populate the results list and automatically select the results that were
                    // just generated.
                    PopulateResultsList();

                    string folderToSelect = Path.GetFileName(_idShieldTester.OutputFileDirectory);
                    if (string.IsNullOrEmpty(_testerSettings.ExplicitOutputFilesFolder))
                    {
                        folderToSelect = folderToSelect.Remove(0, _ANAYLYSIS_FOLDER_PREFIX.Length);
                        int indexToSelect = _resultsSelectionComboBox.FindStringExact(folderToSelect);
                        _resultsSelectionComboBox.SelectedIndex = indexToSelect;
                    }
                    else
                    {
                        _currentResultsFolder = _testerSettings.ExplicitOutputFilesFolder;

                        PopulateStatisitics();
                        PopulateFileLists();
                    }

                    _tabControl.SelectTab(_reviewTab);
                    if (_reviewTabControl.TabPages.Contains(_customReportTab))
                    {
                        _reviewTabControl.SelectTab(_customReportTab);
                    }
                    else
                    {
                        _reviewTabControl.SelectTab(_statisticsTab);
                    }
                }

                if (!string.IsNullOrEmpty(ReportToPrint))
                {
                    Print(ReportToPrint);
                }

                // Display a message to indicate how many test cases were successfully executed.
                string resultMessage = _testCaseCount.ToString(CultureInfo.CurrentCulture) +
                                       " test cases completed successfully";
                if (_testCaseFailureCount > 0)
                {
                    int succeededCount = _testCaseCount - _testCaseFailureCount;

                    resultMessage = succeededCount.ToString(CultureInfo.CurrentCulture) +
                                    " of " + _testCaseCount.ToString(CultureInfo.CurrentCulture) +
                                    " test cases completed successfully";
                }

                if (Silent)
                {
                    Close();
                }
                else
                {
                    MessageBox.Show(resultMessage, "Test Result", MessageBoxButtons.OK,
                        MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34963");
            }
            finally
            {
                try
                {
                    if (settingsFile != null)
                    {
                        settingsFile.Dispose();
                    }

                    if (tclFile != null)
                    {
                        tclFile.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI28570", ex);
                }
            }
        }

        /// <summary>
        /// Uses the currently seleted analysis type to update dependent controls as necessary.
        /// </summary>
        void UpdateControlsBasedOnAnalysisType()
        {
            _automatedFileConditionButton.Enabled =
                (_analysisTypeComboBox.Text != _STANDARD_VERIFICATION);

            _verificationFileConditionButton.Enabled =
                (_analysisTypeComboBox.Text != _AUTOMATED_REDACTION);
        }

        /// <summary>
        /// Populates all test results to the results combo based on the currently specified
        /// feedback folder.
        /// </summary>
        void PopulateResultsList()
        {
            if (string.IsNullOrEmpty(CustomReportTemplate))
            {
                if (_reviewTabControl.TabPages.Contains(_customReportTab))
                {
                    _reviewTabControl.TabPages.Remove(_customReportTab);
                }
            }
            else
            {
                _customReportTab.Text = CustomReportName;
            }

            if (string.IsNullOrEmpty(_testerSettings.ExplicitOutputFilesFolder))
            {
                _resultsSelectionComboBox.Items.Clear();

                if (Directory.Exists(_feedbackFolderTextBox.Text))
                {
                    // If a valid folder is entered, persist the value next time the statistics reporter
                    // is opened.
                    _config.Settings.LastFeedbackFolder = _feedbackFolderTextBox.Text;

                    string[] outputFolders = Directory.GetDirectories(_feedbackFolderTextBox.Text,
                        _ANAYLYSIS_FOLDER_PREFIX + "*", SearchOption.TopDirectoryOnly);

                    foreach (string folder in outputFolders)
                    {
                        if (File.Exists(Path.Combine(folder, "statistics.txt")))
                        {
                            string listEntry = Path.GetFileName(folder);
                            listEntry = listEntry.Remove(0, _ANAYLYSIS_FOLDER_PREFIX.Length);
                            _resultsSelectionComboBox.Items.Add(listEntry);
                        }
                    }
                }
            }
            else
            {
                _reviewLabel.Visible = false;
                _resultsSelectionComboBox.Visible = false;
                int addedHeight = _reviewTabControl.Top - _resultsSelectionComboBox.Top;
                _reviewTabControl.Location = new Point(_reviewTabControl.Location.X,
                    _reviewTabControl.Location.Y - addedHeight);
                _reviewTabControl.Height += addedHeight;
            }

            PopulateFileLists();
        }

        /// <summary>
        /// Populates the statisitics.
        /// </summary>
        void PopulateStatisitics()
        {
            if (!string.IsNullOrEmpty(CustomReportTemplate))
            {
                string customReportFileName =
                    Path.Combine(_currentResultsFolder, CustomReportName + ".txt");

                if (File.Exists(customReportFileName))
                {
                    _customReportTextBox.Text = File.ReadAllText(customReportFileName);
                }
                else
                {
                    _customReportTextBox.Text = "";
                }
            }

            if (File.Exists(StatisticsReportFileName))
            {
                _statisticsTextBox.Text = File.ReadAllText(StatisticsReportFileName);
            }
            else
            {
                _statisticsTextBox.Text = "";
            }
        }

        /// <summary>
        /// Populates all all file lists to the file list combo based on the currently selected
        /// test results folder.
        /// </summary>
        void PopulateFileLists()
        {
            _fileListSelectionComboBox.Items.Clear();

            if (Directory.Exists(_currentResultsFolder))
            {
                string[] fileLists = Directory.GetFiles(
                    _currentResultsFolder, "*.txt", SearchOption.TopDirectoryOnly);

                foreach (string file in fileLists)
                {
                    if (!file.EndsWith("statistics.txt", StringComparison.OrdinalIgnoreCase) &&
                        (string.IsNullOrEmpty(CustomReportName) || !file.EndsWith(
                            CustomReportName + ".txt", StringComparison.OrdinalIgnoreCase)))
                    {
                        _fileListSelectionComboBox.Items.Add(Path.GetFileName(file));
                    }
                }
            }

            _fileListListBox.Items.Clear();
            _fileListCountTextBox.Text = "";
        }

        /// <summary>
        /// Prints the specified report.
        /// </summary>
        /// <param name="reportName">Name of the report to print.</param>
        void Print(string reportName)
        {
            string fileName = Path.Combine(_currentResultsFolder,
                Path.GetFileNameWithoutExtension(reportName) + ".txt");

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("notepad", "/p " + fileName.Quote());
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
            }
        }

        /// <summary>
        /// Displays the exception unless in silent mode, in which case the exception is logged.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> to display or log.</param>
        void ReportException(ExtractException ee)
        {
            if (Silent)
            {
                if (!string.IsNullOrEmpty(LogFileName))
                {
                    ee.Log(LogFileName);
                }
                else
                {
                    ee.Log();
                }
            }
            else
            {
                ee.Display();
            }
        }

        #endregion Private Members
    }
}
