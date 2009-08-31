using Extract.AttributeFinder;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to verify redactions.
    /// </summary>
    [CLSCompliant(false)]
    public partial class VerificationTaskForm : Form, IVerificationForm
    {
        #region VerificationTaskForm Constants

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The maximum number of documents to store in the history.
        /// </summary>
        const int _MAX_DOCUMENT_HISTORY = 20;

        #endregion VerificationTaskForm Constants

        #region VerificationTaskForm Fields

        /// <summary>
        /// The settings for verification.
        /// </summary>
        readonly VerificationSettings _settings;

        /// <summary>
        /// The settings specified in the ID Shield initialization file.
        /// </summary>
        readonly InitializationSettings _iniSettings = new InitializationSettings();

        /// <summary>
        /// The last saved state of the currently processing document.
        /// </summary>
        VerificationMemento _savedMemento;

        /// <summary>
        /// The unsaved state of currently processing document; if <see langword="null"/> no 
        /// changes have been made to the currently processing document.
        /// </summary>
        VerificationMemento _unsavedMemento;

        /// <summary>
        /// <see langword="true"/> if the currently processing document has been modified; 
        /// <see langword="false"/> if the currently processing document has not been modified.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The previously verified documents.
        /// </summary>
        List<VerificationMemento> _history = new List<VerificationMemento>(_MAX_DOCUMENT_HISTORY);

        /// <summary>
        /// Represents the index in <see cref="_history"/> of the currently displayed document. 
        /// If the index is beyond the end of the <see cref="_history"/>, the currently 
        /// displayed document is the currently processing document.
        /// </summary>
        int _historyIndex;

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        readonly static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.IDShieldVerificationObject, "Verification Form");

        #endregion VerificationTaskForm Fields

        #region VerificationTaskForm Events

        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        #endregion VerificationTaskForm Events

        #region VerificationTaskForm Constructors

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
                _licenseCache.Validate("ELI27105");

                // License SandDock before creating the form
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                _settings = settings;

                InitializeComponent();

                // Add the default redaction types
                string[] types = GetRedactionTypes();
                _redactionGridView.AddRedactionTypes(types);

                _imageViewer.DefaultRedactionFillColor = _iniSettings.OutputRedactionColor;

                // Subscribe to layer object events
                _imageViewer.LayerObjects.LayerObjectAdded += HandleImageViewerLayerObjectAdded;
                _imageViewer.LayerObjects.LayerObjectDeleted += HandleImageViewerLayerObjectDeleted;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27104", ex);
            }
        }

        #endregion VerificationTaskForm Constructors

        #region VerificationTaskForm Properties

        /// <summary>
        /// Gets whether the currently viewed document is a history document.
        /// </summary>
        /// <value><see langword="true"/> if a document from the history is what is currently viewed;
        /// <see langword="false"/> if the currently processing document is being viewed.</value>
        public bool IsInHistory
        {
            get
            {
                return _historyIndex < _history.Count;
            }
        }

        #endregion VerificationTaskForm Properties

        #region VerificationTaskForm Methods

        /// <summary>
        /// Gets an array of the default redaction types.
        /// </summary>
        /// <returns>An array of the default redaction types.</returns>
        static string[] GetRedactionTypes()
        {
            // Get the number of types from the ini file
            InitializationFile iniFile = GetInitializationFile();
            int typeCount = iniFile.ReadInt32("RedactionDataTypes", "NumRedactionDataTypes");
            string[] types = new string[typeCount];

            // Get each type
            for (int i = 1; i <= typeCount; i++)
            {
                string key = "RedactionDataType" + i.ToString(CultureInfo.InvariantCulture);
                types[i-1] = iniFile.ReadString("RedactionDataTypes", key);
            }

            return types;
        }

        /// <summary>
        /// Gets the ID Shield initialization file.
        /// </summary>
        /// <returns>The ID Shield initialization file.</returns>
        static InitializationFile GetInitializationFile()
        {
            string path = FileSystemMethods.GetAbsolutePath("IDShield.ini");
            return new InitializationFile(path);
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

            return "";
        }

        /// <summary>
        /// Saves and commits the currently viewed document.
        /// </summary>
        void Commit()
        {
            if (!WarnIfInvalid())
            {
                SaveRedactionCounts();

                Save();

                AdvanceToNextDocument();
            }
        }

        /// <summary>
        /// Saves the ID Shield redaction counts to the ID Shield database.
        /// </summary>
        void SaveRedactionCounts()
        {
            // Get the screen time for the current document
            VerificationMemento memento = GetCurrentDocument();
            double elapsedSeconds = memento.StopScreenTime();

            // TODO: Actually store data in database
            Console.WriteLine(elapsedSeconds.ToString(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Saves the currently viewed voa file.
        /// </summary>
        void Save()
        {
            // Collect feedback if necessary
	        if (ShouldCollectFeedback())
	        {
                CollectFeedback();
	        }

            // Save the voa
            string voaFile = GetDestinationVoa();
            _redactionGridView.SaveTo(voaFile);

            // Clear the unsaved state
            if (!IsInHistory)
            {
                ResetUnsavedMemento();
            }
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
                else if ((types & CollectionTypes.Redacted) > 0 && _redactionGridView.HasRedactions)
                {
                    // Collect because document contains redactions
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Collects the feedback about the currently viewed document.
        /// </summary>
        void CollectFeedback()
        {
            // Get the destination for the feedback image
            VerificationMemento memento = GetCurrentDocument();
            string feedbackImage = memento.FeedbackImage;

            // Check if the original document is being collected
            if (_settings.Feedback.CollectOriginalDocument)
            {
                // Copy the file if the source and destination differ
                string originalImage = _imageViewer.ImageFile;
                if (!originalImage.Equals(feedbackImage, StringComparison.OrdinalIgnoreCase))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(feedbackImage));   
                    File.Copy(originalImage, feedbackImage, true);
                }

                // Create a VOA file relative to the image collected for feedback
                _redactionGridView.SaveTo(feedbackImage + ".voa", feedbackImage);
            }
            else
            {
                // Create a VOA file relative to the original image
                _redactionGridView.SaveTo(feedbackImage + ".voa");
            }
        }

        /// <summary>
        /// Gets the fully expanded path of the feedback image file.
        /// </summary>
        /// <param name="tags">Expands File Action Manager path tags.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="fileId">The id in the database that corresponds to this file.</param>
        /// <returns>The fully expanded path of the feedback image file.</returns>
        string GetFeedbackImageFileName(FileActionManagerPathTags tags, string sourceDocument, 
            int fileId)
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
            if (IsInHistory)
            {
                // Advance the history index
                _historyIndex++;

                // Open the next file
                VerificationMemento memento = GetCurrentDocument();
                _imageViewer.OpenImage(memento.ImageFile, false);
            }
            else
            {
                // Reset the dirty flag
                _dirty = false;

                // If the max document history was reached, drop the first item
                if (_history.Count == _MAX_DOCUMENT_HISTORY)
                {
                    _history.RemoveAt(0);
                    _historyIndex--;
                }

                // Store the current document in the history
                _history.Add(_savedMemento);
                _historyIndex++;

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
            if (_settings.General.VerifyAllPages && !_pageSummaryView.HasVisitedAllPages())
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
                        if (WarnIfInvalid())
                        {
                            return true;
                        }

                        Save();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the current document to view from the history.
        /// </summary>
        /// <returns>The current document to view from the history.</returns>
        VerificationMemento GetCurrentDocument()
        {
            if (IsInHistory)
            {
                return _history[_historyIndex];
            }
            else
            {
                return _unsavedMemento ?? _savedMemento;
            }
        }

        /// <summary>
        /// Gets the voa file to use as the destination voa.
        /// </summary>
        /// <returns>The voa file to use as the destination voa.</returns>
        string GetDestinationVoa()
        {
            // Return either the history VOA or the currently processing VOA
            VerificationMemento memento = IsInHistory ? _history[_historyIndex] : _savedMemento;
            return memento.AttributesFile;
        }

        /// <summary>
        /// Creates a memento to store the last saved state of the processing document.
        /// </summary>
        /// <param name="imageFile">The name of the processing document.</param>
        /// <param name="fileID">The file Id of the <paramref name="imageFile"/> in File Action 
        /// Manager database.</param>
        /// <param name="pathTags">Expands File Action Manager path tags.</param>
        /// <returns>A memento to store the last saved state of the processing document.</returns>
        VerificationMemento CreateSavedMemento(string imageFile, int fileID, 
            FileActionManagerPathTags pathTags)
        {
            string attributesFile = pathTags.Expand(_settings.InputFile);
            string documentType = GetDocumentType(attributesFile);
            string feedbackImage = GetFeedbackImageFileName(pathTags, imageFile, fileID);

            return new VerificationMemento(imageFile, attributesFile, documentType, feedbackImage);
        }

        /// <summary>
        /// Creates a memento to store the current (unsaved) state of the processing document.
        /// </summary>
        /// <returns>A memento to store the current (unsaved) state of the processing document.
        /// </returns>
        VerificationMemento CreateUnsavedMemento()
        {
            string imageFile = _savedMemento.ImageFile;
            string attributesFile = FileSystemMethods.GetTemporaryFileName(".voa");
            string documentType = _savedMemento.DocumentType;
            string feedbackImage = _savedMemento.FeedbackImage;

            return new VerificationMemento(imageFile, attributesFile, documentType, feedbackImage);
        }

        /// <summary>
        /// Resets <see cref="_unsavedMemento"/> to <see langword="null"/>.
        /// </summary>
        void ResetUnsavedMemento()
        {
            if (_unsavedMemento != null)
            {
                string voaFile = _unsavedMemento.AttributesFile;
                if (File.Exists(voaFile))
                {
                    FileSystemMethods.TryDeleteFile(voaFile);
                }

                _unsavedMemento = null;
            }

            _dirty = false;
        }

        #endregion VerificationTaskForm Methods

        #region VerificationTaskForm Overrides

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

                _imageViewer.Shortcuts[Keys.Control | Keys.F4] = null;

                // Set confidence levels
                _redactionGridView.ConfidenceLevels = _iniSettings.ConfidenceLevels;
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
                // TODO: Also check if the main document is dirty, but not in view.
                if (WarnIfDirty())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27116", ex);
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
            // Allow the image viewer to handle keyboard input for shortcuts.
            if (_imageViewer.Shortcuts.ProcessKey(keyData))
            {
                return true;
            }

            // This key was not processed, bubble it up to the base class.
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion VerificationTaskForm Overrides

        #region VerificationTaskForm OnEvents

        /// <summary>
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileComplete"/> 
        /// event.</param>
        protected virtual void OnFileComplete(FileCompleteEventArgs e)
        {
            if (FileComplete != null)
            {
                FileComplete(this, e);
            }
        }

        #endregion VerificationTaskForm OnEvents

        #region VerificationTaskForm Event Handlers

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
                Commit();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26628", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                Commit();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26785", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                    Save();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27044", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                if (!WarnIfDirty())
                {
                    OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSkipped));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27046", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                Close();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27047", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                    // Clear any stored changes
                    if (!IsInHistory)
                    {
                        ResetUnsavedMemento();
                    }

                    // Load the original voa
                    string voaFile = GetDestinationVoa();
                    _redactionGridView.LoadFrom(voaFile);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27048", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27049", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27050", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                // Check if changes have been made before moving away from a history document
                bool inHistory = IsInHistory;
                if (!inHistory || !WarnIfDirty())
                {
                    SaveRedactionCounts();

                    if (!inHistory)
                    {
                        // Preserve the currently processing document
                        if (_unsavedMemento == null)
                        {
                            _unsavedMemento = CreateUnsavedMemento();
                        }

                        // Save the state of the current document before moving back
                        _dirty = _redactionGridView.Dirty;
                        _redactionGridView.SaveTo(_unsavedMemento.AttributesFile);
                    }

                    // Go to the previous document
                    _historyIndex--;
                    VerificationMemento memento = GetCurrentDocument();
                    _imageViewer.OpenImage(memento.ImageFile, false);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27074", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                if (!WarnIfDirty())
                {
                    SaveRedactionCounts();

                    AdvanceToNextDocument();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27075", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27076", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27077", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleOptionsToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27078", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI26710", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI26711", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27051", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                if (_imageViewer.IsImageAvailable)
                {
                    _currentDocumentTextBox.Text = _imageViewer.ImageFile;

                    _previousDocumentToolStripButton.Enabled = _historyIndex > 0;
                    _nextDocumentToolStripButton.Enabled = IsInHistory;

                    _skipProcessingToolStripMenuItem.Enabled = !IsInHistory;
                    _saveToolStripMenuItem.Enabled = !IsInHistory;

                    // Load the voa, if it exists
                    VerificationMemento memento = GetCurrentDocument();
                    string voaFile = memento.AttributesFile;
                    if (File.Exists(voaFile))
                    {
                        _redactionGridView.LoadFrom(voaFile);
                    }

                    if (!IsInHistory)
                    {
                        _redactionGridView.Dirty = _dirty;
                    }

                    // Set the document type
                    _documentTypeTextBox.Text = memento.DocumentType;

                    // Start recording the screen time
                    memento.StartScreenTime();
                }
                else
                {
                    _currentDocumentTextBox.Text = "";

                    _previousDocumentToolStripButton.Enabled = false;
                    _nextDocumentToolStripButton.Enabled = false;

                    _skipProcessingToolStripMenuItem.Enabled = false;
                    _saveToolStripMenuItem.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26760", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        void HandleImageViewerLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                if (_imageViewer.LayerObjects.Count == 1)
                {
                    _applyExemptionToolStripButton.Enabled = true;
                    _lastExemptionToolStripButton.Enabled = _redactionGridView.HasAppliedExemptions;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27052", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        void HandleImageViewerLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                if (_imageViewer.LayerObjects.Count == 0)
                {
                    _applyExemptionToolStripButton.Enabled = false;
                    _lastExemptionToolStripButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27053", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion VerificationTaskForm Event Handlers

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
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new VerificationFormOpen(Open),
                        new object[] { fileName, fileID, actionID, tagManager, fileProcessingDB });
                    return;
                }

                // Get the full path of the source document
                string fullPath = Path.GetFullPath(fileName);
                
                // Create the path tags
                FileActionManagerPathTags pathTags = 
                    new FileActionManagerPathTags(fullPath, tagManager.FPSFileDir);

                // Create the saved memento
                _savedMemento = CreateSavedMemento(fullPath, fileID, pathTags);

                // Reset the unsaved memento
                ResetUnsavedMemento();

                _imageViewer.OpenImage(fullPath, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26627",
                    "Unable to open file for verification.", ex);
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        #endregion IVerificationForm Members
    }
}