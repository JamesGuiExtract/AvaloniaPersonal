using Extract.AttributeFinder;
using Extract.Drawing;
using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Rules;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;
using UCLID_REDACTIONCUSTOMCOMPONENTSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to verify redactions.
    /// </summary>
    [CLSCompliant(false)]
    public sealed partial class VerificationTaskForm : Form, IVerificationForm, IMessageFilter
    {
        #region Constants

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The maximum number of documents to store in the history.
        /// </summary>
        const int _DEFAULT_MAX_DOCUMENT_HISTORY = 20;

        /// <summary>
        /// The title to display for the verification task form.
        /// </summary>
        const string _FORM_TASK_TITLE = "ID Shield Verification";

        /// <summary>
        /// The title to display for the verification task form when in stand-alone mode.
        /// </summary>
        const string _FORM_ONDEMAND_TITLE = "ID Shield On Demand";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="VerificationTaskForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.ApplicationDataPath, "ID Shield", "VerificationForm.xml");

        /// <summary>
        /// The full path to the ID Shield help file.
        /// </summary>
        static readonly string _HELP_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.ExtractSystemsPath, "IDShield", "Help", "IDShield.chm");

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(VerificationTaskForm).ToString();

        /// <summary>
        /// The registry sub key where verification settings are stored.
        /// </summary>
        static readonly string _VERIFICATION_SUB_KEY = 
            @"Software\Extract Systems\AttributeFinder\IndustrySpecific\Redaction\RedactionCustomComponents\IDShield";

        /// <summary>
        /// Name for the mutex used to serialize persistance of the control and form layout.
        /// </summary>
        static readonly string _MUTEX_STRING = "13D6A5A4-1E1E-4815-9B81-9A77E4FF4997";

        /// <summary>
        /// System command to execute the screen saver application specified in the [boot] section
        /// of the System.ini file.
        /// </summary>
        static readonly int _SC_SCREENSAVER = 0xF140;

        /// <summary>
        /// System command to set the state of the display. This command supports devices that have
        /// power-saving features, such as a battery-powered personal computer.
        /// The lParam parameter can have the following values:
        ///     -1 (the display is powering on)
        ///     1 (the display is going to low power)
        ///     2 (the display is being shut off)
        /// </summary>
        static readonly int _SC_MONITORPOWER = 0xF170;

        #endregion Constants

        #region Enums

        /// <summary>
        /// Indicates a point in a document to be displayed when the document is loaded.
        /// </summary>
        enum DocumentNavigationTarget
        {
            /// <summary>
            /// Display the portion of the document that was visible the last time the document was
            /// opened. Otherwise, the first page of the document is displayed and the first
            /// sensitive will be selected if it falls on the first page.
            /// </summary>
            LastView = 0,

            /// <summary>
            /// Display the first page or sensitive item depending upon the value of
            /// <see cref="GeneralVerificationSettings.VerifyAllPages"/>.
            /// </summary>
            FirstItem = 1,

            /// <summary>
            /// Display the last page or sensitive item depending upon the value of
            /// <see cref="GeneralVerificationSettings.VerifyAllPages"/>.
            /// </summary>
            LastItem = 2,

            /// <summary>
            /// Display the first page and select the first item if it falls on the first page.
            /// </summary>
            FirstPage = 3,

            /// <summary>
            /// Display the last page and select the last item if it falls on the last page.
            /// </summary>
            LastPage = 4,

            /// <summary>
            /// Display the first document tile using the scale factor from the last displayed image
            /// page.
            /// </summary>
            FirstTile = 5,

            /// <summary>
            /// Display the last document tile using the scale factor from the last displayed image
            /// page.
            /// </summary>
            LastTile = 6
        }

        #endregion Enums

        #region Fields

        /// <summary>
        /// The settings for verification.
        /// </summary>
        readonly VerificationSettings _settings;

        /// <summary>
        /// The settings specified in the ID Shield initialization file.
        /// </summary>
        readonly InitializationSettings _iniSettings = new InitializationSettings();

        /// <summary>
        /// The config file that contains settings for the verification UI.
        /// </summary>
        readonly ConfigSettings<Properties.Settings> _config =
            new ConfigSettings<Properties.Settings>();

        /// <summary>
        /// The file corresponding to the currently open vector of attributes (VOA) file.
        /// </summary>
        readonly RedactionFileLoader _currentVoa;

        /// <summary>
        /// The file processing database.
        /// </summary>
        FileProcessingDB _fileDatabase;

        /// <summary>
        /// Wrapper around <see cref="_fileDatabase"/> for ID Shield specific functionality.
        /// </summary>
        IDShieldProductDBMgr _idShieldDatabase;

        /// <summary>
        /// The last saved state of the currently processing document.
        /// </summary>
        VerificationMemento _savedMemento;

        /// <summary>
        /// Prevents outside sources from writing to the currently processing document if the 
        /// user navigates away from it.
        /// </summary>
        FileStream _processingStream;

        /// <summary>
        /// Measures the duration of time that has passed since the current document was first 
        /// viewed.
        /// </summary>
        IntervalTimer _screenTime = new IntervalTimer();

        /// <summary>
        /// <see langword="true"/> if the comment text box has been modified;
        /// <see langword="false"/> if the comment text box has not been modified.
        /// </summary>
        bool _commentChanged;

        /// <summary>
        /// The previously verified documents.
        /// </summary>
        readonly List<VerificationMemento> _history = 
            new List<VerificationMemento>(GetMaxDocumentHistory());

        /// <summary>
        /// Represents the index in <see cref="_history"/> of the currently displayed document. 
        /// If the index is beyond the end of the <see cref="_history"/>, the currently 
        /// displayed document is the currently processing document.
        /// </summary>
        int _historyIndex;

        /// <summary>
        /// Tracks user input in the file processing database.
        /// </summary>
        InputEventTracker _inputEventTracker;
        
        /// <summary>
        /// Used to set the file action status when a document is committed.
        /// </summary>
        readonly IFileProcessingTask _actionStatusTask;

        /// <summary>
        /// Used to set the file action status when a document is auto-advanced by the slideshow.
        /// </summary>
        readonly IFileProcessingTask _slideshowActionStatusTask;

        /// <summary>
        /// The find or redact dialog.
        /// </summary>
        RuleForm _findOrRedactForm;

        /// <summary>
        /// Provides OCR results to the find or redact form.
        /// </summary>
        VerificationRuleFormHelper _helper;

        /// <summary>
        /// Expands file action manager path tags.
        /// </summary>
        FAMTagManager _tagManager;

        /// <summary>
        /// <see langword="true"/> if the user chose to close the application through the x button, 
        /// ALT+F4, or through stop processing; <see langword="false"/> otherwise.
        /// </summary>
        bool _userClosing;

        /// <summary>
        /// Used to invoke methods on this control.
        /// </summary>
        readonly ControlInvoker _invoker;

        /// <summary>
        /// Saves/restores window state info and provides full screen mode.
        /// </summary>
        VerificationTaskForm.FormStateManager _formStateManager;

        /// <summary>
        /// Indicates whether the slideshow is currently running.
        /// </summary>
        bool _slideshowRunning;

        /// <summary>
        /// A timer that fires when it is time for the slideshow to advance to the next
        /// document/page.
        /// </summary>
        System.Windows.Forms.Timer _slideshowTimer = new System.Windows.Forms.Timer();

        /// <summary>
        /// Keep track of the last time the slideshow timer was stopped so we can automatically stop
        /// the slideshow if an image hasn't been displayed in at least 10 seconds (queue is empty).
        /// </summary>
        DateTime? _slideshowTimerLastStopTime;

        /// <summary>
        /// Allows the "Slideshow Stopped" message that is displayed when the slideshow is
        /// automatically stopped to be removed.
        /// </summary>
        CancellationTokenSource _slideshowMessageCanceler;

        /// <summary>
        /// The pages that have been automatically advanced by the slideshow in the current
        /// document.
        /// </summary>
        HashSet<int> _setSlideshowAdvancedPages = new HashSet<int>();

        /// <summary>
        /// Generates random numbers for use by the slideshow random prompt user alertness feature.
        /// </summary>
        Random _slideshowRandomGenerator = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// The probability of a random prompt appearing for any particular slideshow advanced page.
        /// </summary>
        double _slideshowPromptProbability;

        /// <summary>
        /// The number of pages that have been advanced by the slideshow without prompting.
        /// </summary>
        int _slideshowUnpromptedPageCount;

        /// <summary>
        /// The last key to be released. Used for tracking whether the slideshow run key has been
        /// double-tapped.
        /// </summary>
        Keys _lastKey;

        /// <summary>
        /// The time the _lastKey was released.
        /// </summary>
        DateTime _timeLastKey = DateTime.Now;

        /// <summary>
        /// The start slideshow command
        /// </summary>
        // Include so that the shortcut is disabled when the feature is not enabled.
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        ApplicationCommand _startSlideshowCommand;

        /// <summary>
        /// The stop slideshow command
        /// </summary>
        ApplicationCommand _stopSlideshowCommand;

        /// <summary>
        /// Keeps track of the total number of pages for all documents committed this session and
        /// the total screen time for all documents displayed this session that are not currently
        /// in the history queue. Item1 is the total number of pages, and Item2 is the total
        /// verification time.
        /// </summary>
        Tuple<int, double> _preHistoricPageVerificationTime = new Tuple<int, double>(0, 0);

        /// <summary>
        /// To allow shortcut keys to be handled when focus is in an undocked sanddock pane.
        /// </summary>
        ShortcutsMessageFilter _shortcutsMessageFilter;

        /// <summary>
        /// The <see cref="DocumentNavigationTarget"/> to use when opening the next document.
        /// </summary>
        DocumentNavigationTarget _navigationTarget = DocumentNavigationTarget.LastView;

        /// <summary>
        /// The scale factor in use when the last document was closed.
        /// </summary>
        double _previousDocumentScaleFactor;

        /// <summary>
        /// Indicates whether the verification session is being run independent of the FAM and
        /// database.
        /// </summary>
        readonly bool _standAloneMode = true;

        /// <summary>
        /// The title to display for the form.
        /// </summary>
        readonly string _formTitle;

        /// <summary>
        /// Caches whether or not PDF read/write support is licensed.
        /// </summary>
        readonly bool _isPDFLicensed = false;

        /// <summary>
        /// The <see cref="RedactionTaskClass"/> instance to use to create redacted output in
        /// stand-alone mode.
        /// </summary>
        RedactionTaskClass _redactedOutputTask;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationTaskForm"/> class.
        /// </summary>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if one is not provided
        /// via the Open call.</param>
        /// <param name="settings">The <see cref="VerificationSettings"/> to use.</param>
        public VerificationTaskForm(VerificationSettings settings, FAMTagManager tagManager)
            : this(settings, tagManager, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationTaskForm"/> class with the
        /// ability to indicated the form is not running in standalone mode.
        /// </summary>
        /// <param name="settings">The <see cref="VerificationSettings"/> to use.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if one is not provided
        /// via the Open call.</param>
        /// <param name="standAloneMode"><see langword="true"/> if the verification session is being
        /// run independent of the FAM and database; <see langword="false"/> otherwise.</param>
        internal VerificationTaskForm(VerificationSettings settings, FAMTagManager tagManager,
            bool standAloneMode)
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldVerificationObject, "ELI27105",
                    _OBJECT_NAME);

                // License SandDock before creating the form
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                _settings = settings;
                _tagManager = tagManager;

                _actionStatusTask = GetActionStatusTask(_settings.ActionStatusSettings);

                InitializeComponent();

                _standAloneMode = standAloneMode;
                
                // Adjust controls based on whether or not standalone mode is being used.
                if (_standAloneMode)
                {
                    _formTitle = _FORM_ONDEMAND_TITLE;

                    FormsMethods.RemoveAndDisposeToolStripItems(_nextDocumentToolStripButton,
                        _previousDocumentToolStripButton, _saveToolStripMenuItem);
                    FormsMethods.RemoveUnnecessaryToolStripSeparators(_basicDataGridToolStrip);
                    
                    // In standalone mode, there's no "commit".
                    _saveAndCommitToolStripMenuItem.Text = "Save";
                    _saveAndCommitToolStripButton.Text =
                        _saveAndCommitToolStripButton.Text.Replace(" and Commit", "");
                    _skipProcessingToolStripMenuItem.Text = "Close";
                    _stopProcessingToolStripMenuItem.Text = "Exit";

                    _imageViewer.OpeningImage += HandleImageViewerOpeningImage;
                    _imageViewer.ImageFileClosing += HandleImageViewerImageFileClosing;

                    // Do not maintain any document history in standalone mode.
                    _history.Capacity = 0;

                    // The redacted image output task for standalone mode uses with default settings except:
                    // All turned-on items will be output (even clues, etc)
                    // The redaction text is "<ExemptionCodes>"
                    _redactedOutputTask = new RedactionTaskClass();
                    _redactedOutputTask.AttributeNames = null;
                    _redactedOutputTask.RedactionText = "<ExemptionCodes>";

                    // Cached whether PDF support is used since we will be checking this with every save.
                    _isPDFLicensed = LicenseUtilities.IsLicensed(LicenseIdName.PdfReadWriteFeature);
                }
                else
                {
                    _formTitle = _FORM_TASK_TITLE;

                    _openImageToolStripSplitButton.Visible = false;
                    _openImageToolStripMenuItem.Visible = false;
                }

                // Add the default redaction types
                string[] types = _iniSettings.GetRedactionTypes();
                _redactionGridView.AddRedactionTypes(types);
                if (!_settings.General.RequireTypes)
                {
                    _redactionGridView.AddRedactionType("");
                }

                // Initialize the slideshow timer and _slideshowActionStatusTask if the slideshow is
                // enabled.
                if (_settings.SlideshowSettings.SlideshowEnabled)
                {
                    _slideshowTimer.Tick += HandleSlideshowTimerTick;
                    _slideshowTimer.Interval = _config.Settings.SlideshowInterval * 1000;

                    if (_settings.SlideshowSettings.ApplyAutoAdvanceActionStatus)
                    {
                        _slideshowActionStatusTask = GetActionStatusTask(
                            _settings.SlideshowSettings.AutoAdvanceSetActionStatusSettings);
                    }
                }

                // [FlexIDSCore:4442]
                // For prefetching purposes, allow the ImageViewer will cache images.
                _imageViewer.CacheImages = true;
                
                _imageViewer.DefaultRedactionFillColor = _iniSettings.OutputRedactionColor;

                SetVerificationOptions();

                _currentVoa = new RedactionFileLoader(_iniSettings.ConfidenceLevels);

                _redactionGridView.ConfidenceLevels = _currentVoa.ConfidenceLevels;

                // Set the selection pen
                LayerObject.SelectionPen = ExtractPens.GetThickPen(Color.Red);

                // Subscribe to layer object events
                _imageViewer.LayerObjects.Selection.LayerObjectAdded += 
                    HandleImageViewerSelectionLayerObjectAdded;
                _imageViewer.LayerObjects.Selection.LayerObjectDeleted += 
                    HandleImageViewerSelectionLayerObjectDeleted;

                // [FlexIDSCore:4482]
                // Initialize a message filter to allow shortcuts to be handled even when focus is
                // in an undocked Sandock pane.
                _shortcutsMessageFilter = new ShortcutsMessageFilter(
                    ShortcutsEnabled, _imageViewer.Shortcuts, this);

                _invoker = new ControlInvoker(this);

                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    _formStateManager = new VerificationTaskForm.FormStateManager(
                        this, _FORM_PERSISTENCE_FILE, _MUTEX_STRING, _sandDockManager,
                        "Exit full screen (F11)");
                    _formStateManager.FullScreenChanged += HandleFullScreenModeChanged;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27104", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets whether the control styles of the current Windows theme should be used for the
        /// verification form.
        /// </summary>
        /// <returns><see langword="true"/> to use the control styles of the current Windows theme;
        /// <see langword="false"/> to use Window's classic theme to draw controls.</returns>
        public bool UseVisualStyles
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether the currently viewed document is a history document.
        /// </summary>
        /// <value><see langword="true"/> if a document from the history is what is currently viewed;
        /// <see langword="false"/> if the currently processing document is being viewed.</value>
        bool IsInHistory
        {
            get
            {
                return _historyIndex < _history.Count;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates an action status file processor using the specified settings.
        /// </summary>
        /// <param name="settings">The settings to use to create the file processor.</param>
        /// <returns>An action status file processor.</returns>
        static IFileProcessingTask GetActionStatusTask(
            SetFileActionStatusSettings settings)
        {
            SetActionStatusFileProcessor processor = null;
            if (settings.Enabled)
            {
                processor = new SetActionStatusFileProcessor();
                processor.ActionName = settings.ActionName;
                processor.ActionStatus = (int)settings.ActionStatus;
            }

            return (IFileProcessingTask) processor;
        }

        /// <summary>
        /// Gets the maximum number of documents to store in document history queue.
        /// </summary>
        /// <returns>The maximum number of documents to store in document history queue.</returns>
        static int GetMaxDocumentHistory()
        {
            using (RegistryKey subKey = Registry.CurrentUser.CreateSubKey(_VERIFICATION_SUB_KEY))
            {
                if (subKey != null)
                {
                    string value = subKey.GetValue("NumPreviousDocsToQueue") as string;
                    if (value != null)
                    {
                        int maxDocuments;
                        if (int.TryParse(value, NumberStyles.Integer, 
                            CultureInfo.InvariantCulture, out maxDocuments) && maxDocuments >= 0)
                        {
                            return maxDocuments;
                        }
                    }
                }

                return _DEFAULT_MAX_DOCUMENT_HISTORY;
            }
        }

        /// <summary>
        /// Gets whether shortcuts are enabled.
        /// </summary>
        /// <returns><see langword="true"/> if shortcut keys are enabled;
        /// <see langword="false"/> if shortcut keys are disabled.</returns>
        bool ShortcutsEnabled()
        {
            // Shortcuts are disabled if:
            // 1) The comments text box is active OR
            // 2) A cell of the redaction grid view is being edited
            // 3) The find or redact form has focus
            return !_commentsTextBox.Focused && !_redactionGridView.IsInEditMode &&
                   (_findOrRedactForm == null || !_findOrRedactForm.ContainsFocus);
        }

        /// <summary>
        /// Gets the document type of the specified vector of attributes (VOA) file.
        /// </summary>
        /// <param name="voaFile">The vector of attributes (VOA) file.</param>
        /// <returns>The first document type in <paramref name="voaFile"/>.</returns>
        static string GetDocumentType(string voaFile)
        {
            if (File.Exists(voaFile))
            {
                foreach (ComAttribute attribute in AttributesFile.ReadAll(voaFile))
                {
                    if (attribute.Name.Equals("DocumentType", StringComparison.OrdinalIgnoreCase))
                    {
                        return attribute.Value.String;
                    }
                }
            }

            return "Unknown";
        }

        /// <summary>
        /// Saves and commits the currently viewed document.
        /// </summary>
        void Commit()
        {
            VerificationMemento memento = GetCurrentDocument();

            // Save VOA file if needed.
            if (NeedToSaveVOAFile(memento))
            {
                TimeInterval screenTime = StopScreenTimeTimer();
                Save(screenTime);
            }

            // If in standalone, output the redacted version of the image.
            if (_standAloneMode)
            {
                if (!SaveRedactedImage(memento))
                {
                    return;
                }
            }

            // Commit
            if (_actionStatusTask != null)
            {
				// Set up file record for call to processfile
				FileRecordClass fileRecord = new FileRecordClass();
				fileRecord.Name = memento.SourceDocument;
				fileRecord.FileID = memento.FileId;

                _actionStatusTask.ProcessFile(fileRecord, memento.ActionId,
                    _tagManager, _fileDatabase, null, false);
            }

            // If at least one page has been automatically advanced on the document being saved,
            // apply any tags or action status per the task configuration.
            if (_setSlideshowAdvancedPages.Count > 0)
            {
                if (_settings.SlideshowSettings.ApplyAutoAdvanceTag)
                {
                    _fileDatabase.TagFile(memento.FileId,
                        _settings.SlideshowSettings.AutoAdvanceTag);
                }

                if (_slideshowActionStatusTask != null)
                {
					// Set up file record for call to processfile
					FileRecordClass fileRecord = new FileRecordClass();
					fileRecord.Name = memento.SourceDocument;
					fileRecord.FileID = memento.FileId;

					_slideshowActionStatusTask.ProcessFile(fileRecord,
						memento.ActionId, _tagManager, _fileDatabase,
                        null, false);
                }
            }
        }

        /// <summary>
        /// Determines whether the VOA file needs to be saved during the <see cref="Commit"/> call.
        /// </summary>
        /// <param name="memento">The memento.</param>
        /// <returns><see langword="true"/> if the VOA file should be saved</returns>
        bool NeedToSaveVOAFile(VerificationMemento memento)
        {
            // Save a VOA file in any of these cases:
            // 1) We're not in stand-alone mode
            // 2) The VOA file already exists
            // 3) OnDemandCreateVOAFileMode == Create
            // 4) OnDemandCreateVOAFileMode == Prompt AND the user answers yes.
            if (!_standAloneMode ||
                File.Exists(memento.AttributesFile) ||
                _config.Settings.OnDemandCreateVOAFileMode == OnDemandCreateVOAFileMode.Create)
            {
                return true;
            }
            else if (_config.Settings.OnDemandCreateVOAFileMode == OnDemandCreateVOAFileMode.Prompt &&
                     System.Windows.Forms.DialogResult.Yes ==
                            MessageBox.Show(this,
                                "Would you like to create an ID Shield data file for this document?",
                                "Create ID Shield data file?", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Saves a redacted copy of the loaded document to a location the user chooses.
        /// </summary>
        /// <param name="memento">The <see cref="VerificationMemento"/> representing the currently
        /// loaded document.</param>
        /// <returns><see langword="true"/> if the redacted image was successfully saved,
        /// <see langword="false"/> otherwise.</returns>
        bool SaveRedactedImage(VerificationMemento memento)
        {
            TemporaryFile tempVoaFile = null;

            try
            {
                // Create a dialog to prompt the user for the ouput location.
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(memento.SourceDocument);
                    saveFileDialog.FileName =
                        Path.GetFileNameWithoutExtension(memento.SourceDocument) + ".redacted";
                    saveFileDialog.Filter = "TIFF files (*.tif;*.tiff)|*.tif;*.tiff";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.AddExtension = true;

                    // If PDF read/write is licensed, support outputting to a PDF as well.
                    if (_isPDFLicensed)
                    {
                        saveFileDialog.Filter += "|PDF Files (*.pdf)|*.pdf";
                        if (ImageMethods.IsPdf(memento.SourceDocument))
                        {
                            saveFileDialog.FilterIndex = 2;
                        }
                    }

                    DialogResult result = saveFileDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // Prevent overwriting the source document.
                        if (string.Compare(saveFileDialog.FileName, memento.SourceDocument,
                                StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            UtilityMethods.ShowMessageBox(
                                "Cannot overwrite source image with redacted output. " +
                                "Please choose a different filename.", "Cannot overwrite source",
                                true);

                            return SaveRedactedImage(memento);
                        }

                        // If there isn't a VOA file for the document, create a temporary VOA file
                        // to supply to _redactedOutputTask.
                        if (!File.Exists(memento.AttributesFile))
                        {
                            tempVoaFile = new TemporaryFile(".voa");
                            _redactedOutputTask.VOAFileName = tempVoaFile.FileName;

                            RedactionFileChanges changes = _redactionGridView.SaveChanges(memento.SourceDocument);
                            _currentVoa.SaveVerificationSession(tempVoaFile.FileName,
                                changes, new TimeInterval(DateTime.Now, 0), _settings, _standAloneMode);

                            _redactedOutputTask.VOAFileName = tempVoaFile.FileName;
                        }
                        else
                        {
                            _redactedOutputTask.VOAFileName = memento.AttributesFile;
                        }

                        _redactedOutputTask.OutputFileName = saveFileDialog.FileName;

                        // Set up file record for call to processfile
                        FileRecordClass fileRecord = new FileRecordClass();
                        fileRecord.Name = memento.SourceDocument;
                        fileRecord.FileID = memento.FileId;

                        // Output the image.
                        if (_redactedOutputTask.ProcessFile(fileRecord, 0, _tagManager, null, null, false) ==
                            EFileProcessingResult.kProcessingSuccessful)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                if (tempVoaFile != null)
                {
                    tempVoaFile.Dispose();
                }
            }

            return false;
        }

        /// <summary>
        /// Saves the ID Shield redaction counts to the ID Shield database.
        /// </summary>
        void SaveRedactionCounts(TimeInterval screenTime)
        {
            // Check for null database manager (only add counts to database if it is not null)
            // [FlexIDSCore #3627]
            if (_idShieldDatabase != null)
            {
                // Get all necessary information for the FAM database
                VerificationMemento memento = GetCurrentDocument();
                RedactionCounts counts = _redactionGridView.GetRedactionCounts();

                // Add the data to the database
                AddDatabaseData(memento.FileId, counts, screenTime);
            }
        }

        /// <summary>
        /// Adds IDShield data to the File Action Manager database.
        /// </summary>
        /// <param name="fileId">The unique file ID for the data being added.</param>
        /// <param name="counts">The counts of redaction categories.</param>
        /// <param name="screenTime">The time the user spent verifying the redactions.</param>
        void AddDatabaseData(int fileId, RedactionCounts counts, TimeInterval screenTime)
        {
            _idShieldDatabase.AddIDShieldData(fileId, true, screenTime.ElapsedSeconds, 
                counts.HighConfidence, counts.MediumConfidence, counts.LowConfidence,
                counts.Clues, counts.Total, counts.Manual, _setSlideshowAdvancedPages.Count);
        }

        /// <summary>
        /// Updates the visited redactions and pages in the current verification memento.
        /// </summary>
        void UpdateMemento()
        {
            // Update the visited pages and rows
            VerificationMemento memento = GetCurrentDocument();
            if (memento != null) 
            {
                memento.PageCount = _imageViewer.PageCount;
                memento.VisitedPages = _pageSummaryView.GetVisitedPages();
                memento.VisitedRedactions = _redactionGridView.GetVisitedRows();
                memento.Selection = _redactionGridView.SelectedRowIndexes;

                // Keep track of the scale factor in use for this document in case the next document
                // is being loaded loaded in tile mode and, therefore, should share the same
                // scale factor.
                _previousDocumentScaleFactor = _imageViewer.ZoomInfo.ScaleFactor;
            }
        }

        /// <summary>
        /// Saves the currently viewed voa file.
        /// </summary>
        /// <param name="screenTime">The duration of time spent verifying the document.</param>
        void Save(TimeInterval screenTime)
        {
            bool collectFeedback = ShouldCollectFeedback();

            // Collect original image and found data feedback if necessary (regardless of what
            // reason the file is being saved).
            if (collectFeedback)
            {
                CollectFeedback(true);
            }

            // Save the voa
            SaveCurrentMemento(screenTime);

            // Collect expected data feedback if necessary
            if (collectFeedback)
            {
                CollectFeedback(false);
            }

            // Update visited pages and rows
            UpdateMemento();
        }

        /// <summary>
        /// Saves the current vector of attributes file to the specified location.
        /// </summary>
        /// <param name="screenTime">The duration of time spent verifying the document.</param>
        void SaveCurrentMemento(TimeInterval screenTime)
        {
            VerificationMemento memento = GetCurrentDocument();
            RedactionFileChanges changes = _redactionGridView.SaveChanges(memento.SourceDocument);

            _currentVoa.SaveVerificationSession(memento.AttributesFile, changes, screenTime,
                _settings, _standAloneMode);

            // Clear the dirty flag [FIDSC #3846]
            _redactionGridView.Dirty = false;

            // Ensure HasContainedRedactions is set to true if the _redactionGridView was saved with
            // redactions present.
            memento.HasContainedRedactions |= _redactionGridView.HasRedactions;

            memento.Selection = _redactionGridView.SelectedRowIndexes;
            _previousDocumentScaleFactor = _imageViewer.ZoomInfo.ScaleFactor;
        }

        /// <summary>
        /// Gets whether feedback should be collected for the currently viewed document.
        /// </summary>
        /// <returns><see langword="true"/> if feedback should be collected;
        /// <see langword="false"/> if feedback should not be collected.</returns>
        bool ShouldCollectFeedback()
        {
            FeedbackSettings settings = _settings.Feedback;
            if (settings.Collect)
            {
                CollectionTypes types = settings.CollectionTypes;
                if (types == CollectionTypes.All)
                {
                    // All documents are being collected
                    return true;
                }
                else if ((types & CollectionTypes.Corrected) > 0 && _redactionGridView.Dirty)
                {
                    // Collect because user corrections were made
                    return true;
                }
                else if ((types & CollectionTypes.Redacted) > 0)
                {
                    // If there are any redactions currently in the grid, collect feedback.
                    if (_redactionGridView.HasRedactions)
                    {
                        return true;
                    }

                    // If this memento has ever contained redactions, collect feedback
                    VerificationMemento memento = GetCurrentDocument();
                    if (memento.HasContainedRedactions)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Collects the feedback about the currently viewed document.
        /// </summary>
        /// <param name="found"><see langword="true"/> if found attribute feedback should be
        /// collected; <see langword="false"/> if expected attribute feedback should be collected.
        /// </param>
        void CollectFeedback(bool found)
        {
            // Get the destination for the feedback image
            VerificationMemento memento = GetCurrentDocument();
            string feedbackImage = memento.FeedbackImage;

            if (found)
            {
                // If collecting found data, check to if the original document is being collected.
                if (_settings.Feedback.CollectOriginalDocument)
                {
                    // Copy the file if the source and destination differ
                    string originalImage = memento.DisplayImage;
                    if (!originalImage.Equals(feedbackImage, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(feedbackImage));

                        // Copy the image to feedback if it hasn't already been copied.
                        if (!File.Exists(feedbackImage))
                        {
                            File.Copy(originalImage, feedbackImage, false);
                        }
                    }
                }

                // Copy the found data file only if it doesn't already exists (otherwise we will
                // likely be copying verified data, not the data the rules found).
                if (!File.Exists(memento.FoundAttributesFileName))
                {
                    // Create the destination directory if necessary.
                    Directory.CreateDirectory(Path.GetDirectoryName(memento.FoundAttributesFileName));

                    if (File.Exists(memento.AttributesFile))
                    {
                        // Copy the existing voa file as the found data if it exists
                        File.Copy(memento.AttributesFile, memento.FoundAttributesFileName, false);
                    }
                    else
                    {
                        // Otherwise save a new empty voa file as the found data to ensure we don't
                        // save verified data as found data at a later point in time.
                        IUnknownVector emptyVector = new IUnknownVector();
                        emptyVector.SaveTo(memento.FoundAttributesFileName, false);
                    }
                }
            }
            else
            {
                // If collecting expected data, the image will have already been collected and we
                // we want to overwite any existing expected data file.
                File.Copy(memento.AttributesFile, memento.ExpectedAttributesFileName, true);
            }
        }

        /// <summary>
        /// Gets the fully expanded path of the feedback image file.
        /// </summary>
        /// <param name="tags">Expands File Action Manager path tags.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="fileId">The id in the database that corresponds to this file.</param>
        /// <returns>The fully expanded path of the feedback image file.</returns>
        string GetFeedbackImageFileName(IPathTags tags, string sourceDocument, int fileId)
        {
            // If feedback settings aren't being collected, this is irrelevant
            FeedbackSettings settings = _settings.Feedback;
            if (!settings.Collect)
            {
                return null;
            }

            // Get the feedback directory and file name
            string directory = tags.Expand(settings.DataFolder);
            string fileName;
            if (settings.UseOriginalFileNames)
            {
                // Original file name
                fileName = Path.GetFileName(sourceDocument);
            }
            else
            {
                // Unique file name
                fileName = fileId.ToString(CultureInfo.InvariantCulture) +
                           Path.GetExtension(sourceDocument);
            }

            return Path.Combine(directory, fileName);
        }

        /// <summary>
        /// Moves to the next document either in the history queue or the next document to be 
        /// processed.
        /// </summary>
        void AdvanceToNextDocument()
        {
            AdvanceToNextDocument(DocumentNavigationTarget.LastView);
        }

        /// <summary>
        /// Moves to the next document either in the history queue or the next document to be 
        /// processed.
        /// </summary>
        /// <param name="navigationTarget">The <see cref="DocumentNavigationTarget"/> to use when
        /// the next document is opened.</param>
        void AdvanceToNextDocument(DocumentNavigationTarget navigationTarget)
        {
            if (_standAloneMode)
            {
                return;
            }

            TimeInterval screenTime = StopScreenTimeTimer();
            SaveRedactionCounts(screenTime);

            // Ensure slideshow timer is not running until the next document is completely loaded.
            if (_slideshowTimer.Enabled)
            {
                StopSlideshowTimer();
            }

            CommitComment();

            _navigationTarget = navigationTarget;

            if (IsInHistory)
            {
                // Open the next file
                IncrementHistory(true);
            }
            else
            {
                // If the max document history was reached, drop the first item
                if (_history.Count == _history.Capacity)
                {
                    // If the document being removed from the history queue has been commited, apply
                    // the number of pages in the document and time displayed to
                    // _preHistoricPageVerificationTime.
                    VerificationMemento memento = _history[0];
                    _preHistoricPageVerificationTime = new Tuple<int, double>(
                        _preHistoricPageVerificationTime.Item1 + memento.PageCount,
                        _preHistoricPageVerificationTime.Item2 + memento.ScreenTimeThisSession);

                    _imageViewer.UnloadImage(_history[0].DisplayImage);

                    _history.RemoveAt(0);
                    _historyIndex--;
                }

                // Store the current document in the history
                _history.Add(_savedMemento);
                _historyIndex++;

                // Close current image before opening a new one. [FIDSC #3824]
                _imageViewer.CloseImage();

                // Clear the current memo when closing the image
                // [FIDSC #4237]
                _savedMemento = null;

                // Successfully complete this file
                OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSuccessful));
            }
        }

        /// <summary>
        /// Starts _screenTime and updates _verificationRateStatusLabel and _pagesPerHourStatusLabel.
        /// </summary>
        void StartScreenTimeTimer()
        {
            // If _verificationRateStatusLabel exists, update it.
            if (_verificationRateStatusLabel != null)
            {
                VerificationMemento thisMemento = GetCurrentDocument();
                _verificationRateStatusLabel.Start((thisMemento == null)
                    ? 0 : thisMemento.ScreenTimeThisSession);
            }

            // If _pagesPerHourStatusLabel exists, update it.
            if (_pagesPerHourStatusLabel != null)
            {
                // Calculate a tuple representing the total number of pages in documents committed
                // this session and total screen time of all documents displayed this session
                // (including both documents in and before the history queue).
                int totalPageCount = _preHistoricPageVerificationTime.Item1;
                double totalTime = _preHistoricPageVerificationTime.Item2;
                foreach (var memento in _history)
                {
                    totalPageCount += memento.PageCount;
                    totalTime += memento.ScreenTimeThisSession;
                }

                // Calculate the average number of pages verified per hour.
                double averagePagesPerHour = (totalTime > 0)
                    ? totalPageCount / (totalTime / 3600) : 0;

                // Update the status label text.
                _pagesPerHourStatusLabel.Text = string.Format(
                    CultureInfo.CurrentCulture, "Average pages/hour: {0:#}", averagePagesPerHour);
            }

            _screenTime.Start();
        }

        /// <summary>
        /// Stops _screenTime and updates _verificationRateStatusLabel.
        /// </summary>
        /// <returns>The <see cref="TimeInterval"/> elased between the time the timer was started
        /// and now.</returns>
        TimeInterval StopScreenTimeTimer()
        {
            bool running = _screenTime.Running;

            TimeInterval screenTime = _screenTime.Stop();

            // Only update memento.ScreenTimeThisSession and stop _verificationRateStatusLabel if
            // the timer was running before this call.
            if (running)
            {
                VerificationMemento memento = GetCurrentDocument();
                if (memento != null)
                {
                    memento.ScreenTimeThisSession += screenTime.ElapsedSeconds;
                }

                if (_verificationRateStatusLabel != null)
                {
                    _verificationRateStatusLabel.Stop();
                }
            }

            return screenTime;
        }
        
        /// <summary>
        /// Raises user prompts if any required fields are unfilled.
        /// </summary>
        /// <returns><see langword="true"/> if the user needs to make corrections before 
        /// committing; <see langword="false"/> if the commit can continue.</returns>
        bool WarnIfInvalid()
        {
            // Prompt for verification of all pages
            if (_settings.General.VerifyAllPages && !_pageSummaryView.HasVisitedAllPages)
            {
                VisitPage(_pageSummaryView.GetNextUnvisitedPage(1));

                MessageBox.Show("Must visit all pages before saving.", "Must visit all pages", 
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

                return true;
            }

            // Prompt for verification of all sensitive items.
            if (_settings.General.VerifyAllItems)
            {
                int rowIndex = _redactionGridView.GetNextUnviewedRowIndex(0);
                if (rowIndex >= 0)
                {
                    _redactionGridView.SelectOnly(rowIndex);

                    MessageBox.Show("Must visit all sensitive items and clues before saving.",
                        "Must visit all items",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

                    return true;
                }
            }
            
            // Prompt for requiring all redactions to have a type
            if (_settings.General.RequireTypes)
            {
                foreach (RedactionGridViewRow row in _redactionGridView.Rows)
                {
                    if (string.IsNullOrEmpty(row.RedactionType))
                    {
                        _redactionGridView.SelectOnly(row);

                        MessageBox.Show("Must specify type for all redactions before saving.", 
                            "Must specify type", MessageBoxButtons.OK, MessageBoxIcon.None,
                            MessageBoxDefaultButton.Button1, 0);

                        return true;
                    }
                }
            }

            // Prompt for all redactions to have an exemption code
            if (_settings.General.RequireExemptions)
            {
                foreach (RedactionGridViewRow row in _redactionGridView.Rows)
                {
                    // Only prompt for rows that are being redacted [FlexIDSCore #4223]
                    if (row.Redacted && row.Exemptions.IsEmpty)
                    {
                        _redactionGridView.SelectOnly(row);

                        MessageBox.Show("Must specify exemption codes for all redactions before saving.", 
                            "Must specify exemption codes", MessageBoxButtons.OK, 
                            MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Displays a warning message that allows the user to save or discard any changes. If no 
        /// changes have been made, no message is displayed.
        /// </summary>
        /// <returns><see langword="true"/> if the user chose to cancel or the user tried to save 
        /// with invalid data; <see langword="false"/> if the currently viewed document was not 
        /// modified or if changes were successfully discarded or saved.</returns>
        bool WarnIfDirty()
        {
            // If the user is in standalone mode, the current document doesn't have a VOA file and
            // they are configured no to create one, there's no need to prompt about a dirty file.
            if (_standAloneMode &&
                _config.Settings.OnDemandCreateVOAFileMode == OnDemandCreateVOAFileMode.DoNotCreate)
            {
                VerificationMemento memento = GetCurrentDocument();
                if (memento != null && !File.Exists(memento.AttributesFile))
                {
                    return false;
                }
            }

            // Check if the viewed document is dirty
            if (_redactionGridView.Dirty)
            {
                // Stop the slideshow until we get the user's response.
                bool slideshowRunning = _slideshowRunning;
                if (_slideshowRunning)
                {
                    StopSlideshow(false);
                }

                using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                {
                    messageBox.Caption = "Save changes?";
                    messageBox.Text = "Changes made to this document have not been saved." +
                                      Environment.NewLine + Environment.NewLine +
                                      "Would you like to save them now?";

                    messageBox.AddButton("Save changes", "Save", false);
                    messageBox.AddButton("Discard changes", "Discard", false);
                    messageBox.AddButton("Cancel", "Cancel", true);

                    string result = messageBox.Show(this);
                    if (result == "Cancel")
                    {
                        if (slideshowRunning)
                        {
                            // Display the Slideshow Stopped message
                            StopSlideshow(true);
                        }

                        return true;
                    }
                    else if (result == "Save")
                    {
                        if (IsInHistory)
                        {
                            // Prompt for invalid data only if in the history queue [FIDSC #3863]
                            if (WarnIfInvalid())
                            {
                                return true;
                            }

                            Commit();
                        }
                        else
                        {
                            TimeInterval screenTime = StopScreenTimeTimer();
                            Save(screenTime);
                        }

                        if (slideshowRunning)
                        {
                            // Resume the slideshow
                            StartSlideshow();
                        }
                    }
                }
            }
            else
            {
                UpdateMemento();
            }

            return false;
        }

        /// <summary>
        /// Displays a warning message indicating the current document will be saved and the user 
        /// is navigating to next document without explicity saving/committing the document. Allows
        /// the user to cancel.
        /// </summary>
        /// <returns><see langword="true"/> if there is invalid data or the user chose to cancel; 
        /// <see langword="false"/> if the data is valid and the user chose to continue.</returns>
        bool WarnBeforeAutoCommit()
        {
            bool continueSlideshow = false;

            try
            {
                // If data is invalid, warn immediately
                if (WarnIfInvalid())
                {
                    return true;
                }

                // If a prompt is not required for auto-committed documents, just move on.
                if (!_settings.General.PromptForSaveUntilCommit)
                {
                    return false;
                }

                // Stop the slideshow until we get the user's response.
                bool slideshowRunning = _slideshowRunning;
                if (_slideshowRunning)
                {
                    StopSlideshow(false);
                }

                // Indicate that all sensitive data has been reviewed
                StringBuilder message = new StringBuilder();
                if (_redactionGridView.Rows.Count > 0)
                {
                    message.AppendLine("All found sensitive data and clues have been reviewed.");
                }

                // Indicate how many pages have been visited
                message.Append("You have visited ");
                if (_pageSummaryView.HasVisitedAllPages)
                {
                    message.Append("all");
                }
                else
                {
                    string visitedPages = _pageSummaryView.VisitedPageCount.ToString(CultureInfo.CurrentCulture);
                    string totalPages = _imageViewer.PageCount.ToString(CultureInfo.CurrentCulture);

                    message.Append(visitedPages);
                    message.Append(" of ");
                    message.Append(totalPages);
                }
                message.AppendLine(" pages in this document.");
                message.AppendLine();

                message.Append("Save this document and advance to the next?");

                // Display the message box
                DialogResult result = MessageBox.Show(message.ToString(), "Save document?",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

                // If the slideshow was previously running, either display the stopped message or
                // resume the slideshow.
                if (slideshowRunning)
                {
                    if (result == DialogResult.Cancel)
                    {
                        // Display the stopped message.
                        StopSlideshow(true);
                    }
                    else
                    {
                        // Resume the slideshow
                        StartSlideshow();
                    }
                }

                return result == DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                // If there was an exception, stop the slideshow.
                if (continueSlideshow)
                {
                    continueSlideshow = false;
                    StopSlideshow(true);
                }

                throw ex.AsExtract("ELI32151");
            }
            finally
            {
                if (continueSlideshow)
                {
                    // Set _slideshowRunning back to true so that the slidshow will continue with
                    // the next document.
                    _slideshowRunning = true;
                }
            }
        }

        /// <summary>
        /// When the slideshow is advancing to the next document, if necessary displays a warning
        /// message for invalid data or if data has been modified a message indicating the current
        /// document will be saved and the user is navigating to next document. Allows the user to
        /// cancel.
        /// </summary>
        /// <returns><see langword="true"/> if there is invalid data or the user chose to cancel; 
        /// <see langword="false"/> if the data is valid and either the user chose to continue or
        /// the document's data is unmodified.</returns>
        bool WarnBeforeSlideshowAutoAdvance()
        {
            // If data is invalid, warn immediately.
            if (WarnIfInvalid())
            {
                StopSlideshow(false);
                return true;
            }
            // If the user has changed something and a prompt is required, prompt.
            else if (_redactionGridView.Dirty &&
                     _settings.General.PromptForSaveUntilCommit)
            {
                // Stop the slideshow until we get the user's response.
                StopSlideshow(false);

                string message = "Corrections have been made to this document.\r\n\r\n" +
                    "Save this document and advance to the next?";

                // Display the message box
                DialogResult result = MessageBox.Show(message, "Save document?",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

                bool stopped = (result == DialogResult.Cancel);
                if (stopped)
                {
                    // Display the stopped message.
                    StopSlideshow(true);
                }
                else
                {
                    // Resume the slideshow
                    StartSlideshow();
                }

                return stopped;
            }
            // Just move on.
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Displays a warning message if the user is trying to stop when no document is currently 
        /// processing.
        /// </summary>
        /// <returns><see langword="true"/> if a warning was displayed; <see langword="false"/> if 
        /// no warning was displayed.</returns>
        bool WarnIfStopping()
        {
            bool warn = !_standAloneMode && _userClosing && !_imageViewer.IsImageAvailable;
            if (warn)
            {
                _userClosing = false;
                const string message = "If you are intending to stop processing, "
                                       + "press the stop button in the File Action Manager.";
                MessageBox.Show(this, message, "Stop processing", MessageBoxButtons.OK, 
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
            }

            return warn;
        }

        /// <summary>
        /// Gets the current document to view from the history.
        /// </summary>
        /// <returns>The current document to view from the history.</returns>
        VerificationMemento GetCurrentDocument()
        {
            if (_imageViewer.IsImageAvailable)
            {
                // Return either the history VOA or the currently processing VOA
                return IsInHistory ? _history[_historyIndex] : _savedMemento;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a memento to store the last saved state of the processing document.
        /// </summary>
        /// <param name="sourceDocument">The name of the processing document.</param>
        /// <param name="fileId">The file id of the <paramref name="sourceDocument"/> in File Action 
        /// Manager database.</param>
        /// <param name="actionId">The action id associated with <paramref name="sourceDocument"/> in 
        /// the File Action Manager database.</param>
        /// <param name="pathTags">Expands File Action Manager path tags.</param>
        /// <returns>A memento to store the last saved state of the processing document.</returns>
        VerificationMemento CreateSavedMemento(string sourceDocument, int fileId, int actionId,
            IPathTags pathTags)
        {
            string displayImage = GetDisplayImage(sourceDocument, pathTags);
            string attributesFile = pathTags.Expand(_settings.InputFile);
            string documentType = GetDocumentType(attributesFile);
            string feedbackImage = GetFeedbackImageFileName(pathTags, sourceDocument, fileId);

            return new VerificationMemento(sourceDocument, displayImage, fileId, actionId, 
                attributesFile, documentType, feedbackImage);
        }

        /// <summary>
        /// Gets the display image from the specified source document.
        /// </summary>
        /// <param name="sourceDocument">The original source document.</param>
        /// <param name="pathTags">The path tags to use to expand paths.</param>
        /// <returns>The display image from the specified source document.</returns>
        string GetDisplayImage(string sourceDocument, IPathTags pathTags)
        {
            if (_settings.UseBackdropImage)
            {
                string backdrop = pathTags.Expand(_settings.BackdropImage);
                if (File.Exists(backdrop))
                {
                    return backdrop;
                }
            }

            return sourceDocument;
        }

        /// <summary>
        /// Activates the save and commit command
        /// </summary>
        void SelectSaveAndCommit()
        {
            // Do not allow saving and commiting if the imageviewer is
            // processing a tracking event
            if (_imageViewer.IsImageAvailable && !_imageViewer.IsTracking)
            {
                _redactionGridView.CommitChanges();

                if (!WarnIfInvalid())
                {
                    Commit();

                    AdvanceToNextDocument();
                }
            }
        }

        /// <summary>
        /// Activates the find or redact command.
        /// </summary>
        void SelectFindOrRedact()
        {
            if (_imageViewer.IsImageAvailable)
            {
                if (_helper == null)
                {
                    _helper = new VerificationRuleFormHelper(_imageViewer);
                }

                if (_helper.GetOcrResults() == null)
                {
                    return;
                }

                if (_findOrRedactForm == null)
                {

                    RuleForm ruleForm = new RuleForm("Find or redact text", 
                        new WordOrPatternListRule(), _imageViewer, _helper, this);
                    ruleForm.MatchRedacted += _helper.HandleMatchRedacted;

                    _findOrRedactForm = ruleForm;
                }

                _findOrRedactForm.Show();
            }
        }

        /// <summary>
        /// Activates the prompt for exemption codes command.
        /// </summary>
        void SelectPromptForExemptionCode()
        {
            if (_applyExemptionToolStripButton.Enabled)
            {
                _redactionGridView.PromptForExemptions();
            }
        }

        /// <summary>
        /// Activates the apply last exemption code command.
        /// </summary>
        void SelectApplyLastExemptionCode()
        {
            if (_lastExemptionToolStripButton.Enabled)
            {
                _redactionGridView.ApplyLastExemptions();
            }
        }

        /// <summary>
        /// Selects the previous sensitive item or page.
        /// </summary>
        void SelectPreviousItemOrPage()
        {
            SelectPreviousItemOrPage(false);

            UpdateControlsBasedOnSelection();
        }

        /// <summary>
        /// Selects or checks the availability to select the previous sensitive item or page.
        /// </summary>
        /// <param name="checkAvailability"><see langword="true"/> to only test the availability of
        /// the navigation without actually navigating; <see langword="false"/> to navigate.</param>
        /// <returns><see langword="true"/> if navigation was available/performed;
        /// <see langword="false"/> otherwise.</returns>
        bool SelectPreviousItemOrPage(bool checkAvailability)
        {
            try
            {
                // If no image is available, do nothing
                if (!_imageViewer.IsImageAvailable)
                {
                    return false;
                }

                 // Go to the previous row (or page) if it exists
                int previousRow = _redactionGridView.GetPreviousRowIndex();
                int previousPage = _settings.General.VerifyAllPages ? GetPreviousPage() : -1;
                if (GoToPreviousRowOrPage(previousRow, previousPage, checkAvailability))
                {
                    return true;
                }

                // Go to the previous document if there is one available in history.
                if (_historyIndex > 0)
                {
                    if (!checkAvailability)
                    {
                        GoToPreviousDocument(DocumentNavigationTarget.LastItem);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27593", ex);
            }
        }

        /// <summary>
        /// Selects the next redaction row.
        /// </summary>
        void SelectNextItemOrPage()
        {
            try
            {
                // If no image is available, do nothing
                if (!_imageViewer.IsImageAvailable)
                {
                    return;
                }
                
                // Go to the next row (or page) if it exists
                int nextRow = _redactionGridView.GetNextRowIndex();
                int nextPage = _settings.General.VerifyAllPages ? GetNextPage() : -1;
                if (GoToNextRowOrPage(nextRow, nextPage))
                {
                    return;
                }

                // Go to next document
                GoToNextDocument(false, DocumentNavigationTarget.FirstItem);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27592", ex);
            }
        }

        /// <summary>
        /// Gets the page for the currently selected redactions or if no redactions are 
        /// selected the currently visible page.
        /// </summary>
        /// <returns>The page for the currently selected redactions or if no redactions are 
        /// selected the currently visible page.</returns>
        int GetCurrentPage()
        {
            int row = _redactionGridView.GetFirstSelectedRowIndex();
            return GetActivePageByRowIndex(row);
        }

        /// <summary>
        /// Gets the page before the currently selected redactions or if no redactions are 
        /// selected the page before the currently visible page.
        /// </summary>
        /// <returns>The page before the currently selected redactions or if no redactions are 
        /// selected the page before the currently visible page.</returns>
        int GetPreviousPage()
        {
            int page = GetCurrentPage() - 1;
            return page < 1 ? -1 : page;
        }

        /// <summary>
        /// Gets the page after the currently selected redactions or if no redactions are 
        /// selected the page after the currently visible page.
        /// </summary>
        /// <returns>The page after the currently selected redactions or if no redactions are 
        /// selected the page after the currently visible page.</returns>
        int GetNextPage()
        {
            int page = GetCurrentPage() + 1;
            return page > _imageViewer.PageCount ? -1 : page;
        }

        /// <summary>
        /// Gets the page of the specified row or the current page if the specified row index is 
        /// negative.
        /// </summary>
        /// <param name="row">The row from which to get the page number; or -1 to get the page 
        /// number of the current page.</param>
        /// <returns>The page of the specified row or the current page if the specified row index 
        /// is negative.</returns>
        int GetActivePageByRowIndex(int row)
        {
            return row < 0 ? _imageViewer.PageNumber : _redactionGridView.Rows[row].PageNumber;
        }

        /// <summary>
        /// Goes to or checks the availability to go to the first redaction or page at or before
        /// the specified redaction and page, whichever comes last. 
        /// </summary>
        /// <param name="rowIndex">The previous redaction grid row index or -1 to advance to the
        /// specified page.</param>
        /// <param name="page">The previous page index or -1 to advance to the next specified
        /// index in the redaction grid.</param>
        /// <param name="checkAvailability"><see langword="true"/> to only test the availability of
        /// the navigation without actually navigating; <see langword="false"/> to navigate.</param>
        /// <returns><see langword="true"/> if there is a redaction on or before 
        /// <paramref name="rowIndex"/> or a page at or before <paramref name="page"/>;
        /// <see langword="false"/> otherwise.</returns>
        bool GoToPreviousRowOrPage(int rowIndex, int page, bool checkAvailability)
        {
            // Get the next unviewed row
            rowIndex = rowIndex < 0 ? -1 : rowIndex;

            // Get the next unvisited page
            page = page < 0 ? -1 : page;

            // Visit the unvisited page if it comes after the unviewed redaction
            if (IsPageAfterRedactionAtIndex(page, rowIndex))
            {
                if (!checkAvailability)
                {
                    VisitPage(page);
                }
                return true;
            }

            // If there is a valid redaction to select, select it.
            if (rowIndex >= 0)
            {
                if (!checkAvailability)
                {
                    _redactionGridView.SelectOnly(rowIndex);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Goes to the next redaction or page at or after the specified redaction and page,
        /// whichever comes first.
        /// </summary>
        /// <param name="rowIndex">The next redaction grid row index or -1 to advance to the
        /// specified page.</param>
        /// <param name="page">The next page index or -1 to advance to the next specified
        /// index in the redaction grid.</param>
        /// <returns><see langword="true"/> if there is a redaction on or after
        /// <paramref name="rowIndex"/> or a page at or after <paramref name="page"/>;
        /// <see langword="false"/> otherwise.</returns>
        bool GoToNextRowOrPage(int rowIndex, int page)
        {
            // Visit the unvisited page if it comes before the unviewed redaction
            if (IsPageBeforeRedactionAtIndex(page, rowIndex))
            {
                VisitPage(page);
                return true;
            }

            // If there is a valid redaction to select, select it.
            if (rowIndex >= 0)
            {
                _redactionGridView.SelectOnly(rowIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified page comes before the redaction at the specified index.
        /// </summary>
        /// <param name="page">The 1-based page number of the page to compare; or -1 if there is 
        /// no page to compare and the result should be <see langword="false"/>.</param>
        /// <param name="index">The index of the redaction row to compare; or -1 if there is no 
        /// row to compare and the result should be <see langword="true"/> if 
        /// <paramref name="page"/> is valid.</param>
        /// <returns><see langword="true"/> if <paramref name="page"/> is valid and either 
        /// <paramref name="index"/> is invalid or <paramref name="page"/> comes before the 
        /// redaction at <paramref name="index"/>; <see langword="false"/> if 
        /// <paramref name="page"/> is invalid or <paramref name="index"/> corresponds to a 
        /// redaction on or after <paramref name="page"/>.</returns>
        bool IsPageBeforeRedactionAtIndex(int page, int index)
        {
            return page > 0 && (index < 0 || page < _redactionGridView.Rows[index].PageNumber);
        }

        /// <summary>
        /// Determines whether the specified page comes after the redaction at the specified index.
        /// </summary>
        /// <param name="page">The 1-based page number of the page to compare; or -1 if there is 
        /// no page to compare and the result should be <see langword="false"/>.</param>
        /// <param name="index">The index of the redaction row to compare; or -1 if there is no 
        /// row to compare and the result should be <see langword="true"/> if 
        /// <paramref name="page"/> is valid.</param>
        /// <returns><see langword="true"/> if <paramref name="page"/> is valid and either 
        /// <paramref name="index"/> is invalid or <paramref name="page"/> comes after the 
        /// redaction at <paramref name="index"/>; <see langword="false"/> if 
        /// <paramref name="page"/> is invalid or <paramref name="index"/> corresponds to a 
        /// redaction on or before <paramref name="page"/>.</returns>
        bool IsPageAfterRedactionAtIndex(int page, int index)
        {
            return page > 0 && (index < 0 || page > _redactionGridView.Rows[index].PageNumber);
        }

        /// <summary>
        /// Clears the current selection and goes to the specified page.
        /// </summary>
        /// <param name="page">The 1-based page number to visit.</param>
        void VisitPage(int page)
        {
            _redactionGridView.ClearSelection();
            _imageViewer.PageNumber = page;
        }

        /// <summary>
        /// Moves to the previous document.
        /// </summary>
        void GoToPreviousDocument()
        {
            GoToPreviousDocument(DocumentNavigationTarget.LastView);
        }

        /// <summary>
        /// Moves to the previous document.
        /// </summary>
        /// <param name="navigationTarget">The <see cref="DocumentNavigationTarget"/> to use when
        /// the previous document is opened.</param>
        void GoToPreviousDocument(DocumentNavigationTarget navigationTarget)
        {
            if (!_standAloneMode && _imageViewer.IsImageAvailable && _historyIndex > 0)
            {
                _redactionGridView.CommitChanges();

                // Check if changes have been made before moving away from a history document
                if (!WarnIfDirty())
                {
                    TimeInterval screenTime = StopScreenTimeTimer();
                    SaveRedactionCounts(screenTime);

                    CommitComment();

                    if (!IsInHistory)
                    {
                        // Prevent outside sources from writing to the processing document
                        if (RegistryManager.LockFiles)
                        {
                            _processingStream = File.Open(_savedMemento.DisplayImage, FileMode.Open,
                                FileAccess.Read, FileShare.Read);

                            // Log that the document was locked if necessary
                            if (RegistryManager.LogFileLocking)
                            {
                                ExtractException ee = new ExtractException("ELI29942",
                                    "Application trace: Processing document locked");
                                ee.Log();
                            }
                        }
                    }

                    _navigationTarget = navigationTarget;

                    // Go to the previous document
                    IncrementHistory(false);
                }
            }
        }

        /// <summary>
        /// Moves to the next document.
        /// </summary>
        void GoToNextDocument()
        {
            GoToNextDocument(false, DocumentNavigationTarget.LastView);
        }

        /// <summary>
        /// Moves to the next document.
        /// </summary>
        /// <param name="promptForSlideshowAdvance">If <see langword="true"/>, prompt for a
        /// slideshow commit if applicable for under the current configuration;
        /// <see langword="false"/> otherwise.</param>
        /// <param name="navigationTarget">The <see cref="DocumentNavigationTarget"/> to use when
        /// the next document is opened.</param>
        void GoToNextDocument(bool promptForSlideshowAdvance, DocumentNavigationTarget navigationTarget)
        {
            if (_standAloneMode)
            {
                return;
            }

            _redactionGridView.CommitChanges();

            // If the advancing from a document that has not yet been committed.
            if (!IsInHistory)
            {
                // If the slideshow is advancing, warns only if dirty and
                // GeneralVerificationSettings.PromptForSaveUntilCommit is true.
                // Otherwise, the user is advancing without explicitly hitting the commit button
                // (Tab, seamless navigation, etc). In this case, warns as long as
                // GeneralVerificationSettings.PromptForSaveUntilCommit is true.
                if ((promptForSlideshowAdvance && !WarnBeforeSlideshowAutoAdvance()) ||
                    (!promptForSlideshowAdvance && !WarnBeforeAutoCommit()))
                {
                    Commit();

                    AdvanceToNextDocument(navigationTarget);
                }
            }
            // If the user is advancing past a previously committed document.
            else if (!WarnIfDirty())
            {
                AdvanceToNextDocument(navigationTarget);
            }
        }

        /// <summary>
        /// Increments or decrements the history index and opens the current image.
        /// </summary>
        /// <param name="forward"><see langword="true"/> if incrementing; <see langword="false"/> 
        /// if decrementing.</param>
        void IncrementHistory(bool forward)
        {
            // Prevent write access to the current image 
            VerificationMemento oldMemento = GetCurrentDocument();

            using (File.Open(oldMemento.DisplayImage, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Increment the history
                _historyIndex += forward ? 1 : -1;
                VerificationMemento newMemento = GetCurrentDocument();

                try
                {
                    // Attempt to open the new image
                    _imageViewer.OpenImage(newMemento.DisplayImage, false);
                }
                catch (Exception ex)
                {
                    // Display the exception
                    ExtractException.Display("ELI29135", ex);

                    // Remove the bad image
                    if (IsInHistory)
                    {
                        _imageViewer.UnloadImage(_history[_historyIndex].DisplayImage);
                        _history.RemoveAt(_historyIndex);
                    }

                    // Undo the history increment
                    if (forward)
                    {
                        _historyIndex--;
                    }
                    _imageViewer.OpenImage(oldMemento.DisplayImage, false);
                }

                // If we have moved to the currently processing document, remove the write lock
                if (!IsInHistory && _processingStream != null)
                {
                    _processingStream.Dispose();
                    _processingStream = null;

                    // Log that the document was unlocked if necessary
                    if (RegistryManager.LogFileLocking)
                    {
                        ExtractException ee = new ExtractException("ELI29945",
                            "Application trace: Processing document unlocked");
                        ee.Log();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the properties of the controls based on the currently open image.
        /// </summary>
        void UpdateControls()
        {
            if (_imageViewer.IsImageAvailable)
            {
                VerificationMemento memento = GetCurrentDocument();

                if (memento == null)
                {
                    // If a memento has not yet been created for this document, we can't update the
                    // controls yet. (This can happen in standalone mode.)
                    return;
                }

                _currentDocumentTextBox.Text = memento.SourceDocument;
                Text = Path.GetFileName(memento.SourceDocument) + " - " + _formTitle;

                _documentTypeTextBox.Text = memento.DocumentType;
                _commentsTextBox.Text = GetFileActionComment(memento);

                _previousDocumentToolStripButton.Enabled = _historyIndex > 0;
                _nextDocumentToolStripButton.Enabled = true;

                _tagFileToolStripButton.Enabled = _fileDatabase != null;
                _tagFileToolStripButton.FileId = memento.FileId;

                _skipProcessingToolStripMenuItem.Enabled = !IsInHistory;
                _saveAndCommitToolStripButton.Enabled = true;
                _saveAndCommitToolStripMenuItem.Enabled = true;
                _saveToolStripMenuItem.Enabled = !IsInHistory;

                _findOrRedactToolStripMenuItem.Enabled = true;
                _findOrRedactToolStripButton.Enabled = true;

                _slideshowPlayToolStripButton.Enabled = !_slideshowRunning;
                _slideshowStopToolStripButton.Enabled = _slideshowRunning;

                UpdateControlsBasedOnSelection();
            }
            else
            {
                _currentDocumentTextBox.Text = "";
                Text = _formTitle + " (Waiting for file)";

                _documentTypeTextBox.Text = "";
                _commentsTextBox.Text = "";

                _previousDocumentToolStripButton.Enabled = false;
                _nextDocumentToolStripButton.Enabled = false;

                _previousRedactionToolStripButton.Enabled = false;
                _nextRedactionToolStripButton.Enabled = false;

                _tagFileToolStripButton.Enabled = false;

                _skipProcessingToolStripMenuItem.Enabled = false;
                _saveAndCommitToolStripButton.Enabled = false;
                _saveAndCommitToolStripMenuItem.Enabled = false;
                _saveToolStripMenuItem.Enabled = false;

                _findOrRedactToolStripMenuItem.Enabled = false;
                _findOrRedactToolStripButton.Enabled = false;

                _slideshowPlayToolStripButton.Enabled = false;
                _slideshowStopToolStripButton.Enabled = false;
            }
        }

        /// <summary>
        /// Updates the properties controls based on the currently selected layer objects.
        /// </summary>
        void UpdateControlsBasedOnSelection()
        {
            bool selected = _imageViewer.LayerObjects.Selection.Count > 0;

            _applyExemptionToolStripButton.Enabled = selected;
            _lastExemptionToolStripButton.Enabled = 
                selected && _redactionGridView.HasAppliedExemptions;

            _nextRedactionToolStripButton.Enabled = !_standAloneMode ||
                (_redactionGridView.Rows.Count > 0 &&
                    _redactionGridView.GetFirstSelectedRowIndex() < _redactionGridView.Rows.Count - 1);

            _previousRedactionToolStripButton.Enabled = SelectPreviousItemOrPage(true);
        }

        /// <summary>
        /// Loads the verification user interface state from the current memento.
        /// </summary>
        /// <returns>The <see cref="VerificationMemento"/> for the current document.</returns>
        VerificationMemento LoadCurrentMemento()
        {
            // Load the voa
            VerificationMemento memento = GetCurrentDocument();

            _currentVoa.LoadFrom(memento.AttributesFile, memento.SourceDocument);

            // Set the controls
            _redactionGridView.LoadFrom(_currentVoa, memento.VisitedRedactions);

            _pageSummaryView.SetVisitedPages(memento.VisitedPages);

            // Ensure HasContainedRedactions is set to true if the _redactionGridView was loaded
            // with redactions present.
            memento.HasContainedRedactions |= _redactionGridView.HasRedactions;

            return memento;
        }

        /// <summary>
        /// Commits the user specified comment for the current document to the database.
        /// </summary>
        void CommitComment()
        {
            if (_commentChanged && _fileDatabase != null)
            {
                VerificationMemento memento = GetCurrentDocument();
                if (memento != null)
                {
                    string comment = _commentsTextBox.Text;
                    _fileDatabase.SetFileActionComment(memento.FileId, memento.ActionId, comment);
                    _commentChanged = false;
                }
            }
        }

        /// <summary>
        /// Gets the comment from database that corresponds to specified memento.
        /// </summary>
        /// <param name="memento">The memento for which to retrieve a comment.</param>
        string GetFileActionComment(VerificationMemento memento)
        {
            return _fileDatabase == null 
                       ? "" : _fileDatabase.GetFileActionComment(memento.FileId, memento.ActionId);
        }

        /// <summary>
        /// Stores the current file processing database.
        /// </summary>
        /// <param name="database">The file processing database to store.</param>
        void StoreDatabase(FileProcessingDB database)
        {
            if (_fileDatabase != database)
            {
                if (_fileDatabase != null)
                {
                    throw new ExtractException("ELI27972", "File processing database mismatch.");
                }

                // Store the file processing database
                _fileDatabase = database;
                _tagFileToolStripButton.Database = database;

                // Check if the tag file toolstrip button should be displayed [FlexIDSCore #3886]
                if (_fileDatabase.GetTagNames().Size > 0
                    || _fileDatabase.AllowDynamicTagCreation())
                {
                    _tagFileToolStripButton.Visible = true;
                    _tagFileToolStripSeparator.Visible = true;
                }

                // Create the IDShield database wrapper
                _idShieldDatabase = new IDShieldProductDBMgrClass();
                _idShieldDatabase.FAMDB = _fileDatabase;
            }
        }

        /// <summary>
        /// Stores the verification options specified and sets them on the 
        /// <see cref="RedactionGridView"/>.
        /// </summary>
        void SetVerificationOptions()
        {
            _redactionGridView.AutoTool = _config.Settings.AutoTool;
            _redactionGridView.AutoZoom = _config.Settings.AutoZoom;
            _redactionGridView.AutoZoomScale = _config.Settings.AutoZoomScale;

            _slideshowTimer.Interval = _config.Settings.SlideshowInterval * 1000;
        }

        /// <summary>
        /// Toggles full screen mode.
        /// </summary>
        void ToggleFullScreen()
        {
            try
            {
                _formStateManager.FullScreen = !_formStateManager.FullScreen;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30764", ex);
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WindowsMessage.SystemCommand && m.WParam == new IntPtr(SystemCommand.Close))
            {
                _userClosing = true;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

                base.OnLoad(e);

                // So that PreFilterMessage is called.
                Application.AddMessageFilter(this);

                if (_settings.General.AllowSeamlessNavigation)
                {
                    _imageViewer.ExtendedNavigationCheck += HandleExtendedNavigationCheck;
                    _imageViewer.ExtendedNavigation += HandleExtendedNavigation;
                }

                // Set the dockable window that the thumbnail and magnifier toolstrip button control.
                _thumbnailsToolStripButton.DockableWindow = _thumbnailDockableWindow;
                _magnifierToolStripButton.DockableWindow = _magnifierDockableWindow;

                // It is important that this line comes AFTER EstablishConnections, 
                // because the page summary view needs to handle this event FIRST.
                _imageViewer.ImageFileChanged += HandleImageViewerImageFileChanged;

                // Center on selected layer objects
                _imageViewer.Shortcuts[Keys.F2] = _redactionGridView.BringSelectionIntoView;

                // Disable the close image
                _imageViewer.Shortcuts[Keys.F4 | Keys.Control] = null;

                // Disable open image [FlexIDSCore #4262]
                _imageViewer.Shortcuts[Keys.O | Keys.Control] = null;

                // Next/previous redaction
                _imageViewer.Shortcuts[Keys.Tab] = SelectNextItemOrPage;
                _imageViewer.Shortcuts[Keys.Tab | Keys.Shift] = SelectPreviousItemOrPage;

                // Next/previous document
                _imageViewer.Shortcuts[Keys.Tab | Keys.Control] = GoToNextDocument;
                _imageViewer.Shortcuts[Keys.Tab | Keys.Control | Keys.Shift] = GoToPreviousDocument;

                // Use redaction tool
                _imageViewer.Shortcuts[Keys.H] = _imageViewer.ToggleRedactionTool;

                // Save and commit
                _imageViewer.Shortcuts[Keys.S | Keys.Control] = SelectSaveAndCommit;

                _imageViewer.Shortcuts[Keys.F | Keys.Control] = SelectFindOrRedact;

                // Toggle redacted state
                _imageViewer.Shortcuts[Keys.Space] = _redactionGridView.ToggleRedactedState;

                // Exemption codes
                _imageViewer.Shortcuts[Keys.E] = SelectPromptForExemptionCode;
                _imageViewer.Shortcuts[Keys.E | Keys.Control] = SelectApplyLastExemptionCode;

                // Full screen mode.
                _imageViewer.Shortcuts[Keys.F11] = ToggleFullScreen;

                // Magnifier window
                _imageViewer.Shortcuts[Keys.F12] = (() => _magnifierToolStripButton.PerformClick());

                if (!_settings.General.VerifyAllPages)
                {
                    _nextRedactionToolStripButton.Text =
                        _nextRedactionToolStripButton.Text.Replace("or page ", "");
                    _previousRedactionToolStripButton.Text =
                        _previousRedactionToolStripButton.Text.Replace("or page ", "");
                }

                if (_settings.LaunchInFullScreenMode)
                {
                    _formStateManager.FullScreen = true;
                }
                _fullScreenToolStripMenuItem.Checked = _formStateManager.FullScreen;

                bool slideshowEnabled = !_standAloneMode &&
                                        _settings.SlideshowSettings.SlideshowEnabled &&
                                        !_settings.SlideshowSettings.RequireRunKey;
                _startSlideshowCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.F5 }, StartSlideshow,
                    new ToolStripItem[] { _slideshowPlayToolStripButton, _slideshowPlayToolStripMenuItem },
                    slideshowEnabled, true, slideshowEnabled);

                _stopSlideshowCommand = new ApplicationCommand(null, null, null,
                    new ToolStripItem[] { _slideshowStopToolStripButton, _slideshowStopToolStripMenuItem },
                    false, true, false);

                _slideShowToolStrip.Visible = slideshowEnabled;
                _slideshowToolStripMenuItemSeparator.Visible = slideshowEnabled;
                _slideshowToolStripMenuItem.Visible = slideshowEnabled;

                if (slideshowEnabled && _config.Settings.AutoStartSlideshow)
                {
                    if (_config.Settings.AutoStartSlideshow)
                    {
                        StartSlideshow();

                        // Start the slideshow, but disable the timer until the first document is loaded.
                        StopSlideshowTimer();
                    }
                }

                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26715", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.FormClosing"/> 
        /// event.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                _redactionGridView.CommitChanges();

                // Warn if stopping with no document processing [FIDSC #3885, #3995]
                if (WarnIfStopping())
                {
                    e.Cancel = true;
                    return; 
                }

                // Warn if the currently processing document is dirty
                if (WarnIfDirty())
                {
                    e.Cancel = true;
                    return;
                }

                CommitComment();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27116", ex);
            }
            finally
            {
                _userClosing = false;
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// otherwise, <see langword="false"/>.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // Allow the image viewer to handle keyboard input for shortcuts.
                if (ShortcutsEnabled() && _imageViewer.Shortcuts.ProcessKey(keyData))
                {
                    return true;
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27744", ex);
            }

            return true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Deactivate"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnDeactivate(EventArgs e)
        {
            try
            {
                if (_slideshowRunning)
                {
                    StopSlideshow(true);
                }

                base.OnDeactivate(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32149");
            }
        }

        #endregion Overrides

        #region OnEvents

        /// <summary>
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileComplete"/> 
        /// event.</param>
        void OnFileComplete(FileCompleteEventArgs e)
        {
            if (FileComplete != null)
            {
                FileComplete(this, e);
            }
        }

        #endregion OnEvents

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleSaveToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                SelectSaveAndCommit();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26628", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSaveAndCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                SelectSaveAndCommit();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26785", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Do not allow saving if the image viewer is currently processing
                // a tracking event (i.e. Drawing a redaction)
                if (!IsInHistory && !_imageViewer.IsTracking)
                {
                    _redactionGridView.CommitChanges();

                    TimeInterval screenTime = StopScreenTimeTimer();
                    Save(screenTime);

                    SaveRedactionCounts(screenTime);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27044", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSkipProcessingToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Only allow skipping a document if there is no tracking event taking place
                // in the image viewer (i.e. Drawing a redaction)
                if (!_imageViewer.IsTracking)
                {
                    _redactionGridView.CommitChanges();

                    if (!WarnIfDirty())
                    {
                        TimeInterval screenTime = StopScreenTimeTimer();
                        SaveRedactionCounts(screenTime);

                        VerificationMemento memento = GetCurrentDocument();
                        _preHistoricPageVerificationTime = new Tuple<int, double>(
                            _preHistoricPageVerificationTime.Item1,
                            _preHistoricPageVerificationTime.Item2 + memento.ScreenTimeThisSession);

                        CommitComment();

                        // Close current image before opening a new one. [FIDSC #3824]
                        _imageViewer.CloseImage();

                        OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSkipped));
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27046", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleStopProcessingToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Do not allow closing the form if a tracking event is taking place
                // in the image viewer (i.e. Drawing a redaction)
                if (!_imageViewer.IsTracking)
                {
                    _userClosing = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27047", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleDiscardChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Do not allow discarding changes if a tracking event is
                // currently taking place in the image viewer (i.e. Drawing a redaction)
                if (_imageViewer.IsImageAvailable && !_imageViewer.IsTracking)
                {
                    // Load the original voa
                    LoadCurrentMemento();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27048", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleFindOrRedactToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                SelectFindOrRedact();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29239", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleIDShieldHelpToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Help.ShowHelp(this, _HELP_FILE);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27049", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleAboutIDShieldToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                using (VerificationAboutBox about = new VerificationAboutBox())
                {
                    about.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27050", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandlePreviousDocumentToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                GoToPreviousDocument();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27074", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleNextDocumentToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                GoToNextDocument();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27075", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandlePreviousRedactionToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                SelectPreviousItemOrPage();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27076", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleNextRedactionToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                SelectNextItemOrPage();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27077", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleOptionsToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                VerificationOptionsDialog dialog = new VerificationOptionsDialog(_settings, _standAloneMode);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SetVerificationOptions();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27078", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleApplyExemptionToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                _redactionGridView.PromptForExemptions();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26710", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleLastExemptionToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                _redactionGridView.ApplyLastExemptions();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26711", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="RedactionGridView.ExemptionsApplied"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RedactionGridView.ExemptionsApplied"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RedactionGridView.ExemptionsApplied"/> event.</param>
        void HandleRedactionGridViewExemptionsApplied(object sender, ExemptionsAppliedEventArgs e)
        {
            try
            {
                // Enable the apply last exemption codes tool strip button if there are redactions
                _lastExemptionToolStripButton.Enabled = _imageViewer.LayerObjects.Count > 0;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27051", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // In case the zoom needs to be adjusted on the new document, lock updating of the
                // image viewer until the image change is complete.
                FormsMethods.LockControlUpdate(_imageViewer, true);

                // Ensure slideshow timer is not running until the next document is completely loaded.
                if (_slideshowTimer.Enabled)
                {
                    StopSlideshowTimer();
                }

                UpdateControls();

                if (_imageViewer.IsImageAvailable)
                {
                    // Load the voa, if it exists
                    VerificationMemento memento = LoadCurrentMemento();

                    // Set the appropriate page, zoom & position, and selected sensitive item.
                    InitializeNavigation(memento);

                    if (_slideshowRunning)
                    {
                        // Stop the slideshow if it should be stopped to due to a document type condition.
                        bool stopSlideshow = false;

                        if (_settings.SlideshowSettings.CheckDocumentCondition)
                        {
                            IFAMCondition condition =
                                _settings.SlideshowSettings.DocumentCondition.Object as IFAMCondition;

                            FileRecordClass fileRecord = new FileRecordClass();
                            fileRecord.Name = memento.SourceDocument;
                            fileRecord.FileID = memento.FileId;

                            if (condition.FileMatchesFAMCondition(fileRecord,
                                _fileDatabase, memento.ActionId, _tagManager))
                            {
                                stopSlideshow = true;
                            }
                        }

                        if (stopSlideshow)
                        {
                            // When stopping, display a transparent message across the image viewer
                            // to notify the user that the slideshow has been canceled.
                            StopSlideshow(true);
                        }
                        else
                        {
                            // Restart the slideshow timer now that the document has been loaded.
                            StartSlideshowTimer();
                        }
                    }

                    _setSlideshowAdvancedPages.Clear();

                    // Start recording the screen time
                    StartScreenTimeTimer();
                }
                else
                {
                    // No image is open. Clear the grid.
                    // (This resets the dirty flag as well [FIDSC #3846])
                    _redactionGridView.Clear();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26760", ex);
            }
            finally
            {
                FormsMethods.LockControlUpdate(_imageViewer, false);
                _imageViewer.Invalidate();
            }
        }

        /// <summary>
        /// Sets the appropriate page, zoom and position, and selected sensitive item based on
        /// _navigationTarget
        /// </summary>
        /// <param name="memento">The <see cref="VerificationMemento"/> for the document. (includes
        /// the sensitive items selected the last time the document was viewed)</param>
        void InitializeNavigation(VerificationMemento memento)
        {
            // Restore selection of any sensitive items that were selected if the document was
            // previously viewed.
            _redactionGridView.Select(memento.Selection);

            if (memento.ImagePageData == null)
            {
                // If the document was not previously viewed, get the ImagePageData (ZoomInfo,
                // history) for the document.
                memento.ImagePageData = _imageViewer.ImagePageData;
                if (_navigationTarget == DocumentNavigationTarget.LastView)
                {
                    // In the case that the document was not previously viewed, initialize selection
                    // for LastView as if FirstPage was being used.
                    _navigationTarget = DocumentNavigationTarget.FirstPage;
                }
            }
            else
            {
                // Restore zoom history for the document.
                _imageViewer.ImagePageData = memento.ImagePageData;
            }

            switch (_navigationTarget)
            {
                case DocumentNavigationTarget.FirstItem:
                    {
                        // Go to the first redaction iff:
                        // 1) We are not visiting all pages OR
                        // 2) We are visiting all pages and there is a redaction on page 1
                        if (_redactionGridView.Rows.Count > 0 &&
                                (!_settings.General.VerifyAllPages ||
                                 _redactionGridView.Rows[0].PageNumber == 1))
                        {
                            _redactionGridView.SelectOnly(0);
                        }
                        else
                        {
                            // [FlexIDSCore:4662] Ensure selection is cleared on the new doc.
                            _redactionGridView.ClearSelection();

                            _imageViewer.PageNumber = 1;
                        }
                    }
                    break;

                case DocumentNavigationTarget.FirstPage:
                    {
                        // Go to the first redaction iff:
                        // 1) There is not any previously selected redactions AND
                        // 2) The first redaction is on the first page.
                        if (!memento.Selection.Any() &&
                            _redactionGridView.Rows.Count > 0 &&
                            _redactionGridView.Rows[0].PageNumber == 1)
                        {
                            _redactionGridView.SelectOnly(0);
                        }
                        else
                        {
                            _imageViewer.PageNumber = 1;
                        }
                    }
                    break;

                case DocumentNavigationTarget.LastItem:
                    {
                        // Go to the last redaction iff:
                        // 1) We are not visiting all pages OR
                        // 2) We are visiting all pages and there is a redaction on the last page.
                        int lastRow = _redactionGridView.Rows.Count - 1;
                        int lastPage = _imageViewer.PageCount;
                        if (_redactionGridView.Rows.Count > 0 &&
                                (!_settings.General.VerifyAllPages ||
                                 _redactionGridView.Rows[lastRow].PageNumber == lastPage))
                        {
                            _redactionGridView.SelectOnly(lastRow);
                        }
                        else
                        {
                            // [FlexIDSCore:4662] Ensure selection is cleared on the new doc.
                            _redactionGridView.ClearSelection();

                            _imageViewer.PageNumber = lastPage;
                        }
                    }
                    break;

                case DocumentNavigationTarget.LastPage:
                    {
                        // Go to the last redaction iff:
                        // 1) There is not any previously selected redactions AND
                        // 2) The last redaction is on the first page.
                        int lastRow = _redactionGridView.Rows.Count - 1;
                        int lastPage = _imageViewer.PageCount;
                        if (!memento.Selection.Any() &&
                            _redactionGridView.Rows.Count > 0 &&
                            _redactionGridView.Rows[lastRow].PageNumber == lastPage)
                        {
                            _redactionGridView.SelectOnly(lastRow);
                        }
                        else
                        {
                            _imageViewer.PageNumber = lastPage;
                        }
                    }
                    break;

                case DocumentNavigationTarget.FirstTile:
                    {
                        _imageViewer.SelectFirstDocumentTile(_previousDocumentScaleFactor);
                    }
                    break;

                case DocumentNavigationTarget.LastTile:
                    {
                        _imageViewer.SelectLastDocumentTile(_previousDocumentScaleFactor);
                    }
                    break;
            }

            // After the navigation as been initialized, revert _navigationTarget to the default.
            _navigationTarget = DocumentNavigationTarget.LastView;
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        void HandleImageViewerSelectionLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                UpdateControlsBasedOnSelection();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27052", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        void HandleImageViewerSelectionLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                UpdateControlsBasedOnSelection();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27053", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        void HandleCommentsTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                _commentChanged = true;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27936", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleFindOrRedactToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                SelectFindOrRedact();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29240", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="DockControl.DockSituationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DockControl.DockSituationChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DockControl.DockSituationChanged"/> event.</param>
        void HandleThumbnailDockableWindowDockSituationChanged(object sender, EventArgs e)
        {
            try
            {
                // Don't keep loading thumbnails if _thumbnailDockableWindow is closed.
                _thumbnailViewer.Active = _thumbnailDockableWindow.IsOpen;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30742", ex);
            }
        }

        /// <summary>
        /// Handles the full screen tool strip menu item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFullScreenToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                ToggleFullScreen();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30831", ex);
            }
        }

        /// <summary>
        /// Handles the full screen mode being enabled or disabled by the
        /// <see cref="FormStateManager"/>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFullScreenModeChanged(object sender, EventArgs e)
        {
            try
            {
                _fullScreenToolStripMenuItem.Checked = _formStateManager.FullScreen;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30833", ex);
            }
        }

        /// <summary>
        /// Handles the slideshow play menu item or button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSlideshowPlayUIClick(object sender, EventArgs e)
        {
            try
            {
                StartSlideshow();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31118", ex);
            }
        }

        /// <summary>
        /// Handles the slideshow stop menu item or button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSlideshowStopUIClick(object sender, EventArgs e)
        {
            try
            {
                StopSlideshow(false);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31120", ex);
            }
        }

        /// <summary>
        /// Handles the slideshow timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSlideshowTimerTick(object sender, EventArgs e)
        {
            try
            {
                // Don't re-start the clock on advancing to the next page/document until that
                // document/page is completely loaded.
                StopSlideshowTimer();

                // Ensure an image is still available.
                if (!_imageViewer.IsImageAvailable)
                {
                    return;
                }

                _setSlideshowAdvancedPages.Add(GetCurrentPage());
                                
                if (_settings.SlideshowSettings.PromptRandomly)
                {
                    // Display prompts at random times per the task configuration.
                    if (!PromptRandomly())
                    {
                        // The user didn't respond correctly to the prompt; don't advance and keep
                        // the slideshow stopped.
                        return;
                    }
                }

                int nextPage = _imageViewer.PageNumber + 1;
                if (nextPage <= _imageViewer.PageCount)
                {
                    VisitPage(nextPage);
                    StartSlideshowTimer();
                }
                else
                {
                    GoToNextDocument(true, DocumentNavigationTarget.FirstPage);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    StopSlideshow(false);
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI31121", ex2);
                }

                ExtractException.Display("ELI31122", ex);
            }
        }

        /// <summary>
        /// Handles the extended navigation.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ExtendedNavigationEventArgs"/>
        /// instance containing the event data.</param>
        void HandleExtendedNavigation(object sender, ExtendedNavigationEventArgs e)
        {
            try
            {
                if (e.Forward)
                {
                    e.Handled = true;

                    GoToNextDocument(false, e.TileNavigation
                        ? DocumentNavigationTarget.FirstTile
                        : DocumentNavigationTarget.FirstPage);
                }
                else if (_historyIndex > 0)
                {
                    e.Handled = true;

                    GoToPreviousDocument(e.TileNavigation
                        ? DocumentNavigationTarget.LastTile
                        : DocumentNavigationTarget.LastPage);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32385");
            }
        }

        /// <summary>
        /// Handles the extended navigation check.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ExtendedNavigationCheckEventArgs"/>
        /// instance containing the event data.</param>
        void HandleExtendedNavigationCheck(object sender, ExtendedNavigationCheckEventArgs e)
        {
            try
            {
                e.IsAvailable |= (e.Forward || _historyIndex > 0);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32386");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.PageChanged"/> event in order to reset the slideshow
        /// timer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.PageChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandleImageViewerPageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // [FlexIDSCore:4676]
                // When the page changes while the slideshow is running, the slideshow timer needs
                // to start over.
                if (_slideshowTimer.Enabled)
                {
                    _slideShowTimerBarControl.StartTimer(_slideshowTimer.Interval);
                    
                    // Restart the timer
                    _slideshowTimer.Stop();
                    _slideshowTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32596");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.OpeningImage"/> event. Processing that would normally
        /// happen in the <see cref="AdvanceToNextDocument()"/> and <see cref="Open"/> method in FAM
        /// mode will happen in this method in stand-alone mode.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.OpeningImageEventArgs"/> instance
        /// containing the event data.</param>
        void HandleImageViewerOpeningImage(object sender, OpeningImageEventArgs e)
        {
            try
            {
                if (WarnIfDirty())
                {
                    e.Cancel = true;
                }
                else
                {
                    if (_imageViewer.IsImageAvailable)
                    {
                        _imageViewer.UnloadImage(_imageViewer.ImageFile);
                    }

                    // Get the full path of the source document
                    string fullPath = Path.GetFullPath(e.FileName);

                    // Create the saved memento
                    FileActionManagerPathTags pathTags =
                        new FileActionManagerPathTags(fullPath, _tagManager.FPSFileDir);
                    _savedMemento = CreateSavedMemento(fullPath, 0, 0, pathTags);

                    // If the thumbnail viewer is not visible when a document is opened, don't load the
                    // thumbnails. (They will get loaded if the thumbnail window is opened at a later
                    // time).
                    _thumbnailViewer.Active = _thumbnailDockableWindow.IsOpen;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32610");
            }
        }

        /// <summary>
        /// Handles the<see cref="ImageViewer.ImageFileClosing"/> event to ensure the previously
        /// loaded image is unloaded from the image reader cache in stand-alone mode.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ImageFileClosingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleImageViewerImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                _imageViewer.UnloadImage(e.FileName);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32611");
            }
        }

        #endregion Event Handlers

        #region IVerificationForm Members

        /// <summary>
        /// A thread-safe method that opens a document for verification.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        /// <param name="fileID">The ID of the file being processed.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Open(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB)
        {
            if (InvokeRequired)
            {
                _invoker.Invoke(new VerificationFormOpen(Open),
                    new object[] { fileName, fileID, actionID, tagManager, fileProcessingDB });
                return;
            }

            try
            {
                // Store the file processing database
                StoreDatabase(fileProcessingDB);

                // Enable input tracking if specified and there is a database
                if (_inputEventTracker == null && _settings.EnableInputTracking
                    && fileProcessingDB != null)
                {
                    _inputEventTracker = new InputEventTracker(fileProcessingDB, actionID, this);
                }

                // Get the full path of the source document
                string fullPath = Path.GetFullPath(fileName);

                _tagManager = tagManager ?? _tagManager;

                // Create the path tags
                FileActionManagerPathTags pathTags = 
                    new FileActionManagerPathTags(fullPath, _tagManager.FPSFileDir);

                // Create the saved memento
                _savedMemento = CreateSavedMemento(fullPath, fileID, actionID, pathTags);

                // If the thumbnail viewer is not visible when a document is opened, don't load the
                // thumbnails. (They will get loaded if the thumbnail window is opened at a later
                // time).
                _thumbnailViewer.Active = _thumbnailDockableWindow.IsOpen;

                _imageViewer.OpenImage(_savedMemento.DisplayImage, _standAloneMode);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26627",
                    "Unable to open file for verification.", ex);
                ee.AddDebugData("File name", fileName, false);
                _invoker.HandleException(ee);
            }
        }

        /// <summary>
        /// A thread-safe method that allows loading of data prior to the <see cref="Open"/> call
        /// so as to reduce the time the <see cref="Open"/> call takes once it is called.
        /// </summary>
        /// <param name="fileName">The filename of the document for which to prefetch data.</param>
        /// <param name="fileID">The ID of the file being prefetched.</param>
        /// <param name="actionID">The ID of the action being prefetched.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Prefetch(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB)
        {
            try
            {
                _imageViewer.CacheImage(fileName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30717", ex);
            }
        }

        #endregion IVerificationForm Members

        #region IMessageFilter Members

        /// <summary>
        /// Filters out a message before it is dispatched.
        /// </summary>
        /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
        /// <returns>
        /// true to filter the message and stop it from being dispatched; false to allow the message to continue to the next filter or control.
        /// </returns>
        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                if (_slideshowRunning)
                {
                    bool stopSlideshow = false;
                    bool showStopSlideshowMessage = true;

                    switch (m.Msg)
                    {
                        case WindowsMessage.KeyDown:
                        case WindowsMessage.SystemKeyDown:
                            {
                                Keys key = KeyMethods.GetKeyFromMessage(m, true);

                                // Unless the key is related to starting the slideshow or advancing,
                                // stop the slideshow.
                                if (key != Keys.F5 &&
                                    key != Keys.PageDown &&
                                    key != Keys.Tab &&
                                    key != Keys.LControlKey &&
                                    key != Keys.RControlKey &&
                                    (key != Keys.S || Control.ModifierKeys != Keys.Control) &&
                                    key != _config.Settings.SlideshowRunKey)
                                {
                                    stopSlideshow = true;
                                }
                            }
                            break;

                        case WindowsMessage.KeyUp:
                        case WindowsMessage.SystemKeyUp:
                            {
                                // If the slideshow run key has been released, stop the slideshow.
                                if (KeyMethods.GetKeyFromMessage(m, true) ==
                                    _config.Settings.SlideshowRunKey)
                                {
                                    stopSlideshow = true;
                                    showStopSlideshowMessage = false;
                                }
                            }
                            break;

                        case WindowsMessage.LeftButtonDown:
                            {
                                Control clickedControl = FromHandle(m.HWnd);
                                if (clickedControl != _slideShowToolStrip)
                                {
                                    stopSlideshow = true;
                                }
                            }
                            break;

                        case WindowsMessage.MiddleButtonDown:
                        case WindowsMessage.RightButtonDown:
                        case WindowsMessage.MouseWheel:
                        case WindowsMessage.NonClientLeftButtonDown:
                        case WindowsMessage.NonClientRightButtonDown:
                        case WindowsMessage.NonClientMiddleButtonDown:
                            {
                                stopSlideshow = true;
                            }
                            break;

                        case WindowsMessage.SystemCommand:
                            {
                                if (m.WParam.ToInt32() == _SC_SCREENSAVER ||
                                    (m.WParam.ToInt32() == _SC_MONITORPOWER && m.LParam.ToInt32() > 0))
                                {
                                    stopSlideshow = true;
                                }
                            }
                            break;
                    }

                    if (stopSlideshow)
                    {
                        StopSlideshow(showStopSlideshowMessage);
                    }
                }
                else if (_settings.SlideshowSettings.SlideshowEnabled)
                {
                    // Keep track of whether the slide show run key has been double-tapped.
                    // If so, start the slideshow.
                    switch (m.Msg)
                    {
                        case WindowsMessage.KeyDown:
                        case WindowsMessage.SystemKeyDown:
                            {
                                Keys key = KeyMethods.GetKeyFromMessage(m, true);
                                if (key == _lastKey)
                                {
                                    // Check if the slideshow run key is being pressed after having
                                    // been released in the previous 1/2 second.
                                    if (_lastKey == _config.Settings.SlideshowRunKey &&
                                        DateTime.Now < _timeLastKey.AddMilliseconds(500))
                                    {
                                        StartSlideshow();
                                        return true;
                                    }
                                }  
                            }
                            break;

                        case WindowsMessage.KeyUp:
                        case WindowsMessage.SystemKeyUp:
                            {
                                _lastKey = KeyMethods.GetKeyFromMessage(m, true);
                                _timeLastKey = DateTime.Now;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31123", ex);
            }

            return false;
        }

        #endregion IMessageFilter Members

        #region Private Members

        /// <summary>
        /// Starts or stops the slideshow.
        /// </summary>
        /// <param name="start"><see langword="true"/> to start the slideshow, <see langword="false"/>
        /// to stop it.</param>
        void StartSlideshowHelper(bool start)
        {
            if (!start || _slideshowRunning != start)
            {
                if (start)
                {
                    if (_slideshowMessageCanceler != null)
                    {
                        // If the slideshow is being started, ensure the "Slideshow Paused" is
                        // closed.
                        _slideshowMessageCanceler.Cancel();
                    }

                    // Set fit-to-page mode if so configured.
                    if (_settings.SlideshowSettings.ForceFitToPageMode &&
                        _imageViewer.FitMode != FitMode.FitToPage)
                    {
                        _imageViewer.FitMode = FitMode.FitToPage;
                    }

                    // Set the probability that a prompt should display on any particular page change.
                    // This probability will be such that as the PromptInterval goes to infinity, the
                    // probability of a prompt having been displayed by the time PromptInterval is
                    // reached is slightly less than 2 out of 3.
                    if (_settings.SlideshowSettings.PromptRandomly)
                    {
                        _slideshowPromptProbability =
                            1.0 / (double)_settings.SlideshowSettings.PromptInterval;
                        _slideshowUnpromptedPageCount = 0;
                    }
                }

                _slideshowRunning = start;
                _startSlideshowCommand.Enabled = !start;
                _stopSlideshowCommand.Enabled = start;

                if (start && _imageViewer.IsImageAvailable)
                {
                    StartSlideshowTimer();
                }
                else
                {
                    StopSlideshowTimer();

                    _slideshowTimerLastStopTime = null;
                }
            }
        }

        /// <summary>
        /// Starts the slideshow.
        /// </summary>
        void StartSlideshow()
        {
            StartSlideshowHelper(true);
        }

        /// <summary>
        /// Stops the slideshow.
        /// </summary>
        /// <param name="showMessage"><see langword="true"/> to display a transparent
        /// "Slideshow Stopped" message across then image viewer; <see langword="false"/> otherwise.
        /// </param>
        void StopSlideshow(bool showMessage)
        {
            StartSlideshowHelper(false);

            if (showMessage)
            {
                // Display a transparent message across the image viewer to notify
                // the user that the slideshow has been canceled.
                _slideshowMessageCanceler =
                    OverlayText.ShowText(_imageViewer, "Slideshow Stopped", Font,
                        Color.FromArgb(100, Color.Red), null, 2);
            }
        }

        /// <summary>
        /// Starts the slideshow timer and displays a progress indicator.
        /// </summary>
        void StartSlideshowTimer()
        {
            ExtractException.Assert("ELI32180", "Slideshow error.", _slideshowRunning);

            if (!_slideshowTimer.Enabled)
            {
                // As long as the slideshow timer is not already running (to protected against
                // duplicate registrations of the PageChanged event), add handling of handle
                // PageChanged events so that the timer can be reset when the page changes.
                _imageViewer.PageChanged += HandleImageViewerPageChanged;
            }

            _slideShowTimerBarControl.Visible = true;
            _slideShowTimerBarControl.StartTimer(_slideshowTimer.Interval);
            _slideshowTimer.Start();

            if (_slideshowTimerLastStopTime.HasValue && 
                DateTime.Now.AddSeconds(-10) > _slideshowTimerLastStopTime)
            {
                // [FlexIDSCore:4596]
                // If there is more than a 10 second gap between the last image getting closed and
                // the new one being opened, assume the last file was the last in the queue at the
                // time and stop the slideshow.
                StopSlideshow(true);
            }
        }

        /// <summary>
        /// Stops the slideshow timer and stops the progress indicator.
        /// </summary>
        void StopSlideshowTimer()
        {
            if (_slideshowTimer.Enabled)
            {
                _imageViewer.PageChanged -= HandleImageViewerPageChanged;
            }

            _slideshowTimer.Stop();
            _slideShowTimerBarControl.StopTimer(true);
            if (_slideshowRunning)
            {
                _slideshowTimerLastStopTime = DateTime.Now;
            }
            else
            {
                _slideShowTimerBarControl.Visible = false;
            }
        }

        /// <summary>
        /// Displays the slideshow user alertness prompt at random times per the task configuration.
        /// </summary>
        /// <returns><see langword="true"/> if the slideshow should continue, <see langword="false"/>
        /// if the slideshow should stop due to the fact that the prompt wasn't responded to
        /// correctly.</returns>
        bool PromptRandomly()
        {
            _slideshowUnpromptedPageCount++;

            // Prompt if the run key isn't depressed and a random double is by chance less than
            // _slideshowPromptProbability or if the unprompted page count has reached
            // PromptInterval.
            if (!KeyMethods.IsKeyPressed(_config.Settings.SlideshowRunKey) &&
                (_slideshowUnpromptedPageCount >= _settings.SlideshowSettings.PromptInterval ||
                _slideshowRandomGenerator.NextDouble() <= _slideshowPromptProbability))
            {
                StopSlideshow(false);
                char randomLetter = Convert.ToChar(_slideshowRandomGenerator.Next(65, 90));
                char response = '\0';

                using (CustomizableMessageBox prompt = new CustomizableMessageBox())
                {
                    // Create the prompt.
                    prompt.StandardIcon = MessageBoxIcon.Warning;
                    prompt.Caption = "Confirm alertness";
                    prompt.Text = "Press the " + randomLetter + " key to continue.";
                    // If the user doesn't respond withing 5 seconds, just stop the slideshow.
                    prompt.Timeout = 5000;
                    prompt.PlayAlertSound = false;
                    prompt.KeyPress += ((sender2, e2) =>
                    {
                        try
                        {
                            response = Convert.ToChar(e2.KeyValue);
                            if (response == randomLetter)
                            {
                                prompt.Close(CustomizableMessageBoxResult.Ok);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI32362");
                        }
                    });
                    // Add a cancel button that can be used to stop the slideshow immediately.
                    prompt.AddButton(CustomizableMessageBoxResult.Cancel,
                        CustomizableMessageBoxResult.Cancel, true);

                    prompt.Show(this);
                }

                // If the correct response was entered. Continue the slideshow.
                if (response == randomLetter)
                {
                    StartSlideshow();
                }
                else
                {
                    StopSlideshow(true);

                    // Hack: There seems to be to be a scenario here where sometimes the
                    // invalidate call associated with as part of the stop slideshow message
                    // happens in the wrong order and the message doesn't get displayed.
                    // At this point, the time to investigate doesn't seem worth it; just
                    // invoke another invalidate to ensure it is displayed.
                    BeginInvoke((MethodInvoker)(() => Invalidate()));
                    return false;
                }
            }

            return true;
        }

        #endregion Private Members
    }
}
