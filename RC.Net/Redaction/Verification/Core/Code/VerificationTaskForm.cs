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
        /// The settings for the user interface.
        /// </summary>
        VerificationOptions _options;

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

                _options = VerificationOptions.ReadFrom(_iniSettings);

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
            SaveRedactionCounts();

            Save();

            AdvanceToNextDocument();
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
                    if (!WarnBeforeTabCommit())
                    {
                        Commit();
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

        /// <summary>
        /// Moves to the next document.
        /// </summary>
        void GoToNextDocument()
        {
            if (!WarnIfDirty())
            {
                SaveRedactionCounts();

                AdvanceToNextDocument();
            }
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

                // Disable the close image shortcut key
                _imageViewer.Shortcuts[Keys.Control | Keys.F4] = null;

                // Next/previous redaction shortcut keys
                _imageViewer.Shortcuts[Keys.Tab] = SelectNextRedaction;
                _imageViewer.Shortcuts[Keys.Tab | Keys.Shift] = SelectPreviousRedaction;

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
                if (!WarnIfInvalid())
	            {
                    Commit();
	            }
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
                if (!WarnIfInvalid())
                {
                    Commit();
                }
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
                GoToPreviousDocument();
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
                GoToNextDocument();
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
                SelectPreviousRedaction();
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
                SelectNextRedaction();
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
                VerificationOptionsDialog dialog = new VerificationOptionsDialog(_options);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _options = dialog.VerificationOptions;
                    _options.WriteTo(_iniSettings);

                    _redactionGridView.AutoTool = _options.AutoTool;
                    _redactionGridView.AutoZoom = _options.AutoZoom;
                    _redactionGridView.AutoZoomScale = _options.AutoZoomScale;
                }
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