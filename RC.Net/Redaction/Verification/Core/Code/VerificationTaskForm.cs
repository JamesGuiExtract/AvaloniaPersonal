using Extract.AttributeFinder;
using Extract.Drawing;
using Extract.FileActionManager.Forms;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Rules;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using System.Xml;
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
    public sealed partial class VerificationTaskForm : Form, IVerificationForm
    {
        #region Constants

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The maximum number of documents to store in the history.
        /// </summary>
        const int _MAX_DOCUMENT_HISTORY = 20;

        /// <summary>
        /// The title to display for the verification task form.
        /// </summary>
        const string _FORM_TITLE = "ID Shield Verification";

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

        #endregion Constants

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
        /// The settings for the user interface.
        /// </summary>
        VerificationOptions _options;

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
        /// The time the current displayed image was first displayed.
        /// </summary>
        DateTime _screenTimeStart;

        /// <summary>
        /// The duration of time that has passed since <see cref="_screenTimeStart"/>.
        /// </summary>
        Stopwatch _screenTime;

        /// <summary>
        /// <see langword="true"/> if the comment text box has been modified;
        /// <see langword="false"/> if the comment text box has not been modified.
        /// </summary>
        bool _commentChanged;

        /// <summary>
        /// The previously verified documents.
        /// </summary>
        readonly List<VerificationMemento> _history = new List<VerificationMemento>(_MAX_DOCUMENT_HISTORY);

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

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="VerificationTaskForm"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public VerificationTaskForm(VerificationSettings settings)
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

                _actionStatusTask = GetActionStatusTask(_settings.ActionStatusSettings);

                InitializeComponent();

                // Add the default redaction types
                string[] types = _iniSettings.GetRedactionTypes();
                _redactionGridView.AddRedactionTypes(types);
                if (!_settings.General.RequireTypes)
                {
                    _redactionGridView.AddRedactionType("");
                }

                _imageViewer.DefaultRedactionFillColor = _iniSettings.OutputRedactionColor;

                VerificationOptions options = VerificationOptions.ReadFrom(_iniSettings);
                SetVerificationOptions(options);

                _currentVoa = new RedactionFileLoader(_iniSettings.ConfidenceLevels);

                _redactionGridView.ConfidenceLevels = _currentVoa.ConfidenceLevels;

                // Set the selection pen
                LayerObject.SelectionPen = ExtractPens.GetThickPen(Color.Red);

                // Subscribe to layer object events
                _imageViewer.LayerObjects.Selection.LayerObjectAdded += 
                    HandleImageViewerSelectionLayerObjectAdded;
                _imageViewer.LayerObjects.Selection.LayerObjectDeleted += 
                    HandleImageViewerSelectionLayerObjectDeleted;

                _invoker = new ControlInvoker(this);
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
            // Save
            TimeInterval screenTime = StopScreenTime();
            Save(screenTime);

            // Commit
            if (_actionStatusTask != null)
            {
                VerificationMemento memento = GetCurrentDocument();
                _actionStatusTask.ProcessFile(memento.SourceDocument, memento.FileId, memento.ActionId,
                    _tagManager, _fileDatabase, null, false);
            }
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
                counts.Clues, counts.Total, counts.Manual);
        }

        /// <summary>
        /// Starts the screen verification time clock.
        /// </summary>
        void StartScreenTime()
        {
            _screenTimeStart = DateTime.Now;
            _screenTime = new Stopwatch();
            _screenTime.Start();
        }

        /// <summary>
        /// Stops the screen verification time clock.
        /// </summary>
        /// <returns>The total elapsed time of screen verification.</returns>
        TimeInterval StopScreenTime()
        {
            _screenTime.Stop();
            double elapsedSeconds = _screenTime.ElapsedMilliseconds/1000.0;
            return new TimeInterval(_screenTimeStart, elapsedSeconds);
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
                memento.VisitedPages = _pageSummaryView.GetVisitedPages();
                memento.VisitedRedactions = _redactionGridView.GetVisitedRows();
            }
        }

        /// <summary>
        /// Saves the currently viewed voa file.
        /// </summary>
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

            _currentVoa.SaveVerificationSession(memento.AttributesFile, changes, screenTime, _settings);

            // Clear the dirty flag [FIDSC #3846]
            _redactionGridView.Dirty = false;

            // Ensure HasContainedRedactions is set to true if the _redactionGridView was saved with
            // redactions present.
            memento.HasContainedRedactions |= _redactionGridView.HasRedactions;
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
            TimeInterval screenTime = StopScreenTime();
            SaveRedactionCounts(screenTime);

            CommitComment();

            if (IsInHistory)
            {
                // Open the next file
                IncrementHistory(true);
            }
            else
            {
                // If the max document history was reached, drop the first item
                if (_history.Count == _MAX_DOCUMENT_HISTORY)
                {
                    _history.RemoveAt(0);
                    _historyIndex--;
                }

                // Store the current document in the history
                _history.Add(_savedMemento);
                _historyIndex++;

                // Close current image before opening a new one. [FIDSC #3824]
                _imageViewer.CloseImage();

                // Successfully complete this file
                OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSuccessful));
            }
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
                MessageBox.Show("Must visit all pages before saving.", "Must visit all pages", 
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                return true;
            }
            
            // Prompt for requiring all redactions to have a type
            if (_settings.General.RequireTypes)
            {
                foreach (RedactionGridViewRow row in _redactionGridView.Rows)
                {
                    if (string.IsNullOrEmpty(row.RedactionType))
                    {
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
                    if (row.Exemptions.IsEmpty)
                    {
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
            // Check if the viewed document is dirty
            if (_redactionGridView.Dirty)
            {
                using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                {
                    messageBox.Caption = "Save changes?";
                    messageBox.Text = "Changes made to this document have not been saved." +
                                      Environment.NewLine + Environment.NewLine +
                                      "Would you like to save them now?";

                    messageBox.AddButton("Save changes", "Save", false);
                    messageBox.AddButton("Discard changes", "Discard", false);
                    messageBox.AddButton("Cancel", "Cancel", true);

                    string result = messageBox.Show();
                    if (result == "Cancel")
                    {
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
                            TimeInterval screenTime = StopScreenTime();
                            Save(screenTime);
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
        /// is navigating to next document. Allows the user to cancel.
        /// </summary>
        /// <returns><see langword="true"/> if there is invalid data or the user chose to cancel; 
        /// <see langword="false"/> if the data is valid and the user chose to continue.</returns>
        bool WarnBeforeTabCommit()
        {
            // If data is invalid, warn immediately
            if (WarnIfInvalid())
            {
                return true;
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

            return result == DialogResult.Cancel;
        }

        /// <summary>
        /// Displays a warning message if the user is trying to stop when no document is currently 
        /// processing.
        /// </summary>
        /// <returns><see langword="true"/> if a warning was displayed; <see langword="false"/> if 
        /// no warning was displayed.</returns>
        bool WarnIfStopping()
        {
            bool warn = _userClosing && !_imageViewer.IsImageAvailable;
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
            // Return either the history VOA or the currently processing VOA
            return IsInHistory ? _history[_historyIndex] : _savedMemento;
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
            if (_imageViewer.IsImageAvailable)
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
        /// Selects the previous redaction row.
        /// </summary>
        void SelectPreviousRedaction()
        {
            try
            {
                // If no image is available, do nothing
                if (!_imageViewer.IsImageAvailable)
                {
                    return;
                }

                // Determine whether to verify all pages
                bool verifyAllPages = _settings.General.VerifyAllPages;

                // Go to the previous unviewed row (or page) if it exists
                int previousRow = _redactionGridView.GetPreviousRowIndex();
                int previousPage = verifyAllPages ? GetPreviousPage() : -1;
                if (GoToPreviousUnviewed(previousRow, previousPage))
                {
                    return;
                }

                // Go to the previous row if it exists
                if (previousRow >= 0)
                {
                    _redactionGridView.SelectOnly(previousRow);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27593", ex);
            }
        }

        /// <summary>
        /// Selects the next redaction row.
        /// </summary>
        void SelectNextRedaction()
        {
            try
            {
                // If no image is available, do nothing
                if (!_imageViewer.IsImageAvailable)
                {
                    return;
                }
                
                // Determine whether to verify all pages
                bool verifyAllPages = _settings.General.VerifyAllPages;

                // Go to the next unviewed row (or page) if it exists
                int nextRow = _redactionGridView.GetNextRowIndex();
                int nextPage = verifyAllPages ? GetNextPage() : -1;
                if (GoToNextUnviewed(nextRow, nextPage))
                {
                    return;
                }

                // Go to the first unviewed row (or page) if it exists
                if (GoToNextUnviewed(0, verifyAllPages ? 1 : -1))
                {
                    return;
                }

                // Go to the next row
                if (nextRow >= 0)
                {
                    _redactionGridView.SelectOnly(nextRow);
                    return;
                }

                // Go to next document
                if (IsInHistory)
                {
                    GoToNextDocument();
                }
                else
                {
                    _redactionGridView.CommitChanges();

                    if (!WarnBeforeTabCommit())
                    {
                        Commit();

                        AdvanceToNextDocument();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27592", ex);
            }
        }

        /// <summary>
        /// Gets the page before the currently selected redactions or if no redactions are 
        /// selected the page before the currently visible page.
        /// </summary>
        /// <returns>The page before the currently selected redactions or if no redactions are 
        /// selected the page before the currently visible page.</returns>
        int GetPreviousPage()
        {
            int row = _redactionGridView.GetFirstSelectedRowIndex();
            int page = GetActivePageByRowIndex(row) - 1;
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
            int row = _redactionGridView.GetLastSelectedRowIndex();
            int page = GetActivePageByRowIndex(row) + 1;
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
        /// Goes to the first unviewed redaction or the first unvisited page at or before the 
        /// specified redaction and page, whichever comes last. 
        /// </summary>
        /// <param name="startIndex">The first redaction to check if is unviewed; or -1 to only 
        /// check for pages.</param>
        /// <param name="startPage">The first page to check for being unvisited; or -1 to only 
        /// check for redactions.</param>
        /// <returns><see langword="true"/> if there is an unviewed redaction on or before 
        /// <paramref name="startIndex"/> or an unvisited page at or before 
        /// <paramref name="startPage"/>; <see langword="false"/> otherwise.</returns>
        bool GoToPreviousUnviewed(int startIndex, int startPage)
        {
            // Get the next unviewed row
            int unviewedRow =
                startIndex < 0 ? -1 : _redactionGridView.GetPreviousUnviewedRowIndex(startIndex);

            // Get the next unvisited page
            int unvisitedPage = 
                startPage < 0 ? -1 : _pageSummaryView.GetPreviousUnvisitedPage(startPage);

            // Visit the unvisited page if it comes after the unviewed redaction
            if (IsPageAfterRedactionAtIndex(unvisitedPage, unviewedRow))
            {
                VisitPage(unvisitedPage);
                return true;
            }

            // If there is a valid redaction to select, select it.
            if (unviewedRow >= 0)
            {
                _redactionGridView.SelectOnly(unviewedRow);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Goes to the first unviewed redaction or the first unvisited page at or after the 
        /// specified redaction and page, whichever comes first.
        /// </summary>
        /// <param name="startIndex">The first redaction to check if is unviewed; or -1 to only 
        /// check for pages.</param>
        /// <param name="startPage">The first page to check for being unvisited; or -1 to only 
        /// check for redactions.</param>
        /// <returns><see langword="true"/> if there is an unviewed redaction on or after 
        /// <paramref name="startIndex"/> or an unvisited page at or after 
        /// <paramref name="startPage"/>; <see langword="false"/> otherwise.</returns>
        bool GoToNextUnviewed(int startIndex, int startPage)
        {
            // Get the next unviewed row
            int unviewedRow = 
                startIndex < 0 ? -1 : _redactionGridView.GetNextUnviewedRowIndex(startIndex);

            // Get the next unvisited page
            int unvisitedPage = 
                startPage < 0 ? -1 : _pageSummaryView.GetNextUnvisitedPage(startPage);

            // Visit the unvisited page if it comes before the unviewed redaction
            if (IsPageBeforeRedactionAtIndex(unvisitedPage, unviewedRow))
            {
                VisitPage(unvisitedPage);
                return true;
            }

            // If there is a valid redaction to select, select it.
            if (unviewedRow >= 0)
            {
                _redactionGridView.SelectOnly(unviewedRow);
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
            if (_historyIndex > 0)
            {
                _redactionGridView.CommitChanges();

                // Check if changes have been made before moving away from a history document
                if (!WarnIfDirty())
                {
                    TimeInterval screenTime = StopScreenTime();
                    SaveRedactionCounts(screenTime);

                    CommitComment();

                    if (!IsInHistory)
                    {
                        // Prevent outside sources from writing to the processing document
                        _processingStream = File.Open(_savedMemento.DisplayImage, FileMode.Open, 
                            FileAccess.Read, FileShare.Read);
                    }

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
            if (IsInHistory)
            {
                _redactionGridView.CommitChanges();

                if (!WarnIfDirty())
                {
                    AdvanceToNextDocument();
                }
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

                _currentDocumentTextBox.Text = memento.SourceDocument;
                Text = Path.GetFileName(memento.SourceDocument) + " - " + _FORM_TITLE;

                _documentTypeTextBox.Text = memento.DocumentType;
                _commentsTextBox.Text = GetFileActionComment(memento);

                _previousDocumentToolStripButton.Enabled = _historyIndex > 0;
                _nextDocumentToolStripButton.Enabled = IsInHistory;

                _previousRedactionToolStripButton.Enabled = true;
                _nextRedactionToolStripButton.Enabled = true;

                _tagFileToolStripButton.Enabled = _fileDatabase != null;
                _tagFileToolStripButton.FileId = memento.FileId;

                _skipProcessingToolStripMenuItem.Enabled = !IsInHistory;
                _saveAndCommitToolStripButton.Enabled = true;
                _saveAndCommitToolStripMenuItem.Enabled = true;
                _saveToolStripMenuItem.Enabled = !IsInHistory;

                _findOrRedactToolStripMenuItem.Enabled = true;
                _findOrRedactToolStripButton.Enabled = true;
            }
            else
            {
                _currentDocumentTextBox.Text = "";
                Text = _FORM_TITLE + " (Waiting for file)";

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
            }
        }

        /// <summary>
        /// Updates the properties of the exemption code related controls based on the currently 
        /// selected layer objects.
        /// </summary>
        void UpdateExemptionControls()
        {
            bool selected = _imageViewer.LayerObjects.Selection.Count > 0;

            _applyExemptionToolStripButton.Enabled = selected;
            _lastExemptionToolStripButton.Enabled = 
                selected && _redactionGridView.HasAppliedExemptions;
        }

        /// <summary>
        /// Loads the verification user interface state from the current memento.
        /// </summary>
        void LoadCurrentMemento()
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
        }

        /// <summary>
        /// Commits the user specified comment for the current document to the database.
        /// </summary>
        void CommitComment()
        {
            if (_commentChanged && _fileDatabase != null)
            {
                VerificationMemento memento = GetCurrentDocument();
                string comment = _commentsTextBox.Text;
                _fileDatabase.SetFileActionComment(memento.FileId, memento.ActionId, comment);
                _commentChanged = false;
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
        /// <param name="options">The verification options to retain.</param>
        void SetVerificationOptions(VerificationOptions options)
        {
            _options = options;

            _redactionGridView.AutoTool = _options.AutoTool;
            _redactionGridView.AutoZoom = _options.AutoZoom;
            _redactionGridView.AutoZoomScale = _options.AutoZoomScale;
        }

        /// <summary>
        /// Saves the state of the user interface.
        /// </summary>
        void SaveState()
        {
            // Get the pertinent information
            FormMemento formMemento = new FormMemento(this);
            _sandDockManager.SaveLayout();
            ToolStripManager.SaveSettings(this);
            int splitterDistance = _dataWindowSplitContainer.SplitterDistance;

            // Create a memento to represent the state
            VerificationTaskFormMemento memento = 
                new VerificationTaskFormMemento(formMemento, splitterDistance);

            // Convert the memento to XML
            XmlDocument document = new XmlDocument();
            XmlElement element = memento.ToXmlElement(document);
            document.AppendChild(element);

            // Create the directory for the file if necessary
            string directory = Path.GetDirectoryName(_FORM_PERSISTENCE_FILE);
            Directory.CreateDirectory(directory);

            // Save the XML
            document.Save(_FORM_PERSISTENCE_FILE);
        }

        /// <summary>
        /// Loads the previously saved state of the user interface.
        /// </summary>
        void LoadState()
        {
            try
            {
                // Load memento if it exists
                if (File.Exists(_FORM_PERSISTENCE_FILE))
                {
                    // Load the XML
                    XmlDocument document = new XmlDocument();
                    document.Load(_FORM_PERSISTENCE_FILE);

                    // Convert the XML to the memento
                    XmlElement element = (XmlElement)document.FirstChild;
                    VerificationTaskFormMemento memento =
                        VerificationTaskFormMemento.FromXmlElement(element);

                    // Restore the saved state
                    memento.FormMemento.Restore(this);
                    _sandDockManager.LoadLayout();
                    ToolStripManager.LoadSettings(this);
                    _dataWindowSplitContainer.SplitterDistance = memento.SplitterDistance;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28984",
                    "Unable to load previous verification setting state.", ex);
                ee.AddDebugData("Invalid XML file", _FORM_PERSISTENCE_FILE, false);
                ee.Log();
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
            base.OnLoad(e);

            try
            {
                _imageViewer.EstablishConnections(this);

                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    LoadState();
                }

                // It is important that this line comes AFTER EstablishConnections, 
                // because the page summary view needs to handle this event FIRST.
                _imageViewer.ImageFileChanged += HandleImageViewerImageFileChanged;

                // Center on selected layer objects
                _imageViewer.Shortcuts[Keys.F2] = _redactionGridView.BringSelectionIntoView;

                // Disable the close image
                _imageViewer.Shortcuts[Keys.F4 | Keys.Control] = null;

                // Next/previous redaction
                _imageViewer.Shortcuts[Keys.Tab] = SelectNextRedaction;
                _imageViewer.Shortcuts[Keys.Tab | Keys.Shift] = SelectPreviousRedaction;

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

                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    SaveState();
                }
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
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
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
                if (!IsInHistory)
                {
                    _redactionGridView.CommitChanges();

                    TimeInterval screenTime = StopScreenTime();
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
                _redactionGridView.CommitChanges();

                if (!WarnIfDirty())
                {
                    TimeInterval screenTime = StopScreenTime();
                    SaveRedactionCounts(screenTime);

                    CommitComment();

                    // Close current image before opening a new one. [FIDSC #3824]
                    _imageViewer.CloseImage();

                    OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSkipped));
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
                _userClosing = true;
                Close();
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
                if (_imageViewer.IsImageAvailable)
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
                SelectPreviousRedaction();
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
                SelectNextRedaction();
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
                VerificationOptionsDialog dialog = new VerificationOptionsDialog(_options);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SetVerificationOptions(dialog.VerificationOptions);

                    _options.WriteTo(_iniSettings);
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
                UpdateControls();

                if (_imageViewer.IsImageAvailable)
                {
                    // Load the voa, if it exists
                    LoadCurrentMemento();

                    // Go to the first redaction iff:
                    // 1) We are not verifying all pages OR
                    // 2) We are verifying all pages and there is a redaction on page 1
                    if (_redactionGridView.Rows.Count > 0)
                    {
                        if (!_settings.General.VerifyAllPages ||
                            _redactionGridView.Rows[0].PageNumber == 1)
                        {
                            _redactionGridView.SelectOnly(0);
                        }
                    }

                    // Start recording the screen time
                    StartScreenTime();
                }
                else
                {
                    // No image is open. Clear the dirty flag [FIDSC #3846]
                    _redactionGridView.Dirty = false;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26760", ex);
            }
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
                UpdateExemptionControls();
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
                UpdateExemptionControls();
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
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleThumbnailsToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_thumbnailDockableWindow.IsOpen || _thumbnailDockableWindow.Collapsed)
                {
                    _thumbnailDockableWindow.Close();
                }
                else
                {
                    _thumbnailDockableWindow.Open();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28508", ex);
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
                
                // Create the path tags
                FileActionManagerPathTags pathTags = 
                    new FileActionManagerPathTags(fullPath, tagManager.FPSFileDir);
                _tagManager = tagManager;

                // Create the saved memento
                _savedMemento = CreateSavedMemento(fullPath, fileID, actionID, pathTags);

                _imageViewer.OpenImage(_savedMemento.DisplayImage, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26627",
                    "Unable to open file for verification.", ex);
                ee.AddDebugData("File name", fileName, false);
                _invoker.HandleException(ee);
            }
        }

        #endregion IVerificationForm Members
    }
}
