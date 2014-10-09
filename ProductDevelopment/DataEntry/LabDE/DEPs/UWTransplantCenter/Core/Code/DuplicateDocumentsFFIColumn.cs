using ADODB;
using Extract.FileActionManager.Utilities;
using Extract.Imaging;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.DEP.UWTransplantCenter
{
    /// <summary>
    /// A <see cref="IFAMFileInspectorColumn"/> used to allow the user to take action on potential
    /// duplicate files in the FFI.
    /// </summary>
    [ComVisible(true)]
    [Guid("4D3A7BB6-2BD5-41FE-AAC3-69BDCE3961C5")]
    internal class DuplicateDocumentsFFIColumn : IFAMFileInspectorColumn
    {
        #region Constants

        /// <summary>
        /// The name of the column in the FFI.
        /// </summary>
        static readonly string _HEADER_TEXT = "Action";

        /// <summary>
        /// The default width of the column in pixels.
        /// </summary>
        static readonly int _DEFAULT_WIDTH = 115;

        /// <summary>
        /// The value that indicates another user already has the file locked for processing.
        /// (For use with String.Format where the owning user is specified as the only parameter).
        /// </summary>
        static readonly string _IN_USE = "In use ({0})";

        /// <summary>
        /// The value that indicates no action should be taken on the file.
        /// </summary>
        static readonly string _DO_NOTHING = "Do nothing";

        /// <summary>
        /// The value that indicates the document should be stapled into a new, unified document.
        /// </summary>
        static readonly string _STAPLE = "Staple";

        /// <summary>
        /// The value indicates the document should be removed from the queue without filing the
        /// document's results.
        /// </summary>
        static readonly string _IGNORE = "Ignore document";

        /// <summary>
        /// The value indicates the document should be the one displayed in the verification UI.
        /// </summary>
        static readonly string _CURRENT = "Current document";

        /// <summary>
        /// All possible actions for files that are owned (checked-out) by the current process.
        /// </summary>
        static string[] _ALL_FILE_OPTIONS = new[] { _DO_NOTHING, _STAPLE, _IGNORE, _CURRENT };

        #endregion Constants

        #region Fields

        /// <summary>
        /// Stores the currently selected action for all files that have been displayed in FFI.
        /// </summary>
        Dictionary<int, string> _currentValues = new Dictionary<int, string>();

        /// <summary>
        /// The files that are currently checked out for processing in different processes.
        /// </summary>
        HashSet<int> _inUseFiles = new HashSet<int>();

        /// <summary>
        /// Stores the previous file action status for the current action for all files that have
        /// been displayed in the FFI.
        /// </summary>
        Dictionary<int, EActionStatus> _previousStatuses = new Dictionary<int, EActionStatus>();

        /// <summary>
        /// The values that have been programmatically changed since the last GetValue call, thus
        /// need to be updated in the FFI.
        /// </summary>
        HashSet<int> _valuesToRefresh = new HashSet<int>();

        /// <summary>
        /// Files that have been checked out by the current process in order to keep these files
        /// from being processed on another action.
        /// </summary>
        HashSet<int> _checkedOutFileIDs = new HashSet<int>();

        /// <summary>
        /// The file ID of the document that was displayed in verification at the time the FFI was
        /// launched.
        /// </summary>
        int _originalFileID = -1;

        /// <summary>
        /// The file ID of the document that has been selected as the one to be displayed in verification.
        /// </summary>
        int _currentFileID = -1;

        /// <summary>
        /// The name of a stapled document that has been output via the staple action.
        /// </summary>
        string _stapledOutputDocument;

        /// <summary>
        /// <see langword="true"/> if any file actions have changes since the FFI form was displayed
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateDocumentsFFIColumn"/> class.
        /// </summary>
        public DuplicateDocumentsFFIColumn()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the original file.
        /// </summary>
        /// <value>
        /// The name of the original file.
        /// </value>
        public string OriginalFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file ID of the document currently selected to be displayed in
        /// verification.
        /// </summary>
        /// <value>
        /// The file ID of the document currently selected to be displayed in verification.
        /// </value>
        public int CurrentFileID
        {
            get
            {
                return _currentFileID;
            }

            set
            {
                try
                {
                    if (value != _currentFileID)
                    {
                        // If there was previously a different CurrentFileID value and that value
                        // is not being cleared (set to -1), update the previous CurrentFileID to
                        // ensure there is never more than one "current" file.
                        if (_currentFileID != -1 && value != -1)
                        {
                            _currentValues[_currentFileID] = _DO_NOTHING;
                            _valuesToRefresh.Add(_currentFileID);
                        }

                        _currentFileID = value;

                        if (_originalFileID == -1 && value != -1)
                        {
                            _originalFileID = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37444");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IDataEntryApplication"/> instance currently being used in
        /// verification.
        /// </summary>
        /// <value>
        /// The <see cref="IDataEntryApplication"/> instance currently being used in verification.
        /// </value>
        public IDataEntryApplication DataEntryApplication
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the patient first name for the current document.
        /// </summary>
        /// <value>
        /// The patient first name for the current document.
        /// </value>
        public string FirstName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the patient last name for the current document.
        /// </summary>
        /// <value>
        /// The patient last name for the current document.
        /// </value>
        public string LastName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the patient date of birth for the current document.
        /// </summary>
        /// <value>
        /// The patient date of birth for the current document.
        /// </value>
        public string DOB
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a comma-delimited list of collection dates for the current document.
        /// </summary>
        /// <value>
        /// A comma-delimited list of collection dates for the current document.
        /// </value>
        public string CollectionDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a tag to apply to documents that have been ignored.
        /// </summary>
        /// <value>
        /// A tag to apply to documents that have been ignored or <see langword="null"/> if no tag
        /// is to be applied to ignored documents.
        /// </value>
        public string TagForIgnore
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the output path for stapled document output. Path tags/functions are
        /// supported including the tags &lt;SourceDocName&gt;, &lt;FirstName&gt; &lt;LastName&gt;,
        /// &lt;DOB&gt; and &lt;CollectionDate&gt;
        /// </summary>
        /// <value>
        /// The output path for stapled document output.
        /// </value>
        public string StapledDocumentOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a tag to apply to documents that have been stapled.
        /// </summary>
        /// <value>
        /// A tag to apply to documents that have been stapled or <see langword="null"/> if no tag
        /// is to be applied to stapled documents.
        /// </value>
        public string TagForStaple
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a metadata field name that can be updated whenever a document is stapled.
        /// </summary>
        /// <value>
        /// A metadata field name that can be updated whenever a document is stapled or
        /// <see langword="null"/> if no metadata field is to be updated.
        /// </value>
        public string StapledIntoMetadataFieldName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value to be applied to <see cref="StapledIntoMetadataFieldName"/>
        /// whenever a document is stapled. Path tags/functions are supported including the tags
        /// &lt;StapledDocumentOutput&gt;, &lt;SourceDocName&gt;, &lt;FirstName&gt;,
        /// &lt;LastName&gt;, &lt;DOB&gt; and &lt;CollectionDate&gt;
        /// </summary>
        /// <value>
        /// The metadata value to be applied.
        /// </value>
        public string StapledIntoMetadataFieldValue
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the <see cref="EActionStatus"/> of the document prior to being checked out for
        /// display in the FFI.
        /// </summary>
        /// <param name="fileID">The <see cref="EActionStatus"/> of the document prior to being
        /// checked out for display in the FFI.</param>
        /// <returns></returns>
        public EActionStatus GetPreviousStatus(int fileID)
        {
            try
            {
                return _previousStatuses[fileID];
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37562");
            }
        }

        #endregion Methods

        #region IFAMFileInspectorColumn

        /// <summary>
        /// Gets or sets the <see cref="FileProcessingDB"/> currently being used.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> currently being used.
        /// </value>
        public IFileProcessingDB FileProcessingDB
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name for the column header. Will also appear in the context menu if
        /// <see cref="GetContextMenuChoices"/> returns a value.
        /// </summary>
        public string HeaderText
        {
            get
            {
                return _HEADER_TEXT;
            }
        }

        /// <summary>
        /// Gets the default width the column in pixels.
        /// </summary>
        public int DefaultWidth
        {
            get
            {
                return _DEFAULT_WIDTH;
            }
        }

        /// <summary>
        /// Gets the <see cref="FFIColumnType"/> defining what type of column is represented.
        /// </summary>
        public FFIColumnType FFIColumnType
        {
            get
            {
                return FFIColumnType.Combo;
            }
        }

        /// <summary>
        /// Gets whether this column is read only.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// <see langword="true"/> if any values specified via <see cref="SetValue"/> are to be
        /// applied via explicit click of an “OK” button or reverted via “Cancel”.
        /// <see langword="false"/> if the column doesn't make any changes or the changes take
        /// effect instantaneously.
        /// The <see cref="FAMFileInspectorForm"/> will only display OK and Cancel buttons are if this
        /// property is <see langword="true"/> for at least one provided
        /// <see cref="IFAMFileInspectorColumn"/>.
        /// </summary>
        public bool RequireOkCancel
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets if there is any data that has been modified via <see cref="SetValue"/> that needs
        /// to be applied. (Not used if <see cref="RequireOkCancel"/> is <see langword="false"/>).
        /// </summary>
        public bool Dirty
        {
            get
            {
                return _dirty;
            }
        }

        /// <summary>
        /// Gets the possible values to offer for the specified <see paramref="fileID"/>.
        /// </summary>
        /// <param name="fileId">The file ID for which the possible value choices are needed or -1
        /// for the complete set of possible values across all files.</param>
        /// <returns>A list of all pre-defined choices to be available for the user to select in
        /// this column. For <see cref="T:FFIColumnType.Combo"/>, at least one value is required for
        /// the column to be usable.</returns>
        public IVariantVector GetValueChoices(int fileId)
        {
            try
            {
                return GetValueChoicesHelper(fileId).ToVariantVector();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37563", "Failed to retrieve column value choices.");
            }
        }

        /// <summary>
        /// Specifies values that can be applied via context menu. The returned values will be
        /// presented as a sub-menu to a context menu option with <see cref="HeaderText"/> as the
        /// option name. (This method is not used is <see cref="ReadOnly"/> is
        /// <see langword="true"/>.
        /// </summary>
        /// <param name="fileIds"><see langword="null"/> to get a list of all possible values to be
        /// able to apply via the column's context menu across all possible selections; otherwise,
        /// the values that should be enabled for selection based on the selection of the specified
        /// <see paramref="fileIds"/>.</param>
        /// <returns>
        /// The values that should be specifiable via the context menu for the currently
        /// selected row(s). Can be <see langword="null"/> if context menu options should not be
        /// available for this column.
        /// </returns>
        public IVariantVector GetContextMenuChoices(HashSet<int> fileIds)
        {
            try
            {
                IEnumerable<string> choices = null;

                if (fileIds == null)
                {
                    choices = _ALL_FILE_OPTIONS;
                }
                else
                {
                    foreach (int fileID in fileIds)
                    {
                        if (choices == null)
                        {
                            choices = GetValueChoicesHelper(fileID);
                        }
                        else
                        {
                            choices = choices.Intersect(GetValueChoicesHelper(fileID));
                        }
                    
                        if (!choices.Any())
                        {
                            break;
                        }
                    }

                    if (fileIds.Count() > 1)
                    {
                        choices = choices.Except(new[] { _CURRENT });
                    }
                }

                return choices.ToVariantVector();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37564", "Failed to retrieve context menu choices.");
            }
        }

        /// <summary>
        /// Gets the value to display for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileID">The file ID for which the current value is needed.</param>
        /// <returns>
        /// The value for the specified file.
        /// </returns>
        public string GetValue(int fileID)
        {
            try
            {
                string currentValue = null;

                // Attempt to retrieve the value we already have for this file, except in the case
                // of a file that has been locked by another user... in that case attempt to check
                // the file out again in case it has been released by the other user.
                if (!_inUseFiles.Contains(fileID) &&
                    _currentValues.TryGetValue(fileID, out currentValue))
                {
                    return currentValue;
                }
                else
                {
                    // This is the first time this file has been loaded into the FFI; initialize
                    // the value.
                    string value = _DO_NOTHING;
                    EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                    if (DataEntryApplication.FileRequestHandler.CheckoutForProcessing(
                        fileID, out previousStatus))
                    {
                        _inUseFiles.Remove(fileID);

                        if (fileID == _currentFileID)
                        {
                            // This is the file currently displayed in verification.
                            value = _CURRENT;
                        }
                        else if (previousStatus != EActionStatus.kActionProcessing)
                        {
                            // This is the file has been successfully checked out (locked) by the
                            // current process.
                            _checkedOutFileIDs.Add(fileID);
                        }

                        _previousStatuses[fileID] = previousStatus;
                    }
                    else
                    {
                        // If the file could not be checked out, another process has the file locked
                        // for processing. Mark as in-use.
                        _inUseFiles.Add(fileID);
                        value = GetInUseValue(fileID);

                        _previousStatuses[fileID] = EActionStatus.kActionProcessing;
                    }

                    _currentValues[fileID] = value;

                    return value;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37445", "Failed to get value.");
            }
        }

        /// <summary>
        /// Sets the specified <see paramref="value"/> for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID for which the value should be set.</param>
        /// <param name="value">The value to set.</param>
        public bool SetValue(int fileId, string value)
        {
            try
            {
                // Check if the value is being set for the first time or changed
                string currentValue = null;
                if (!_currentValues.TryGetValue(fileId, out currentValue) || value != currentValue)
                {
                    _currentValues[fileId] = value;

                    if (value == _CURRENT)
                    {
                        // If this file has been make the current file, update CurrentFileID
                        CurrentFileID = fileId;
                    }
                    else if (CurrentFileID == fileId)
                    {
                        // If this file had been the current file, clear CurrentFileID
                        CurrentFileID = -1;
                    }

                    _dirty = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37446", "Failed to set value.");
            }
        }

        /// <summary>
        /// Retrieves the set of file IDs for which the value needs to be refreshed. This method
        /// will be called after every time the the FFI file list is loaded or refreshed and after
        /// every time <see cref="SetValue"/> is called in order to check for updated values to be
        /// displayed. <see cref="GetValue"/> will subsequently be called for every returned file
        /// id.
        /// </summary>
        /// <returns>
        /// The file IDs for which values need to be refreshed in the FFI.
        /// </returns>
        public IEnumerable<int> GetValuesToRefresh()
        {
            return _valuesToRefresh;
        }

        /// <summary>
        /// Applies all uncommitted values specified via SetValue. (Unused if
        /// <see cref="RequireOkCancel"/> is <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        public bool Apply()
        {
            try
            {
                // Prevent new files from being popped off the FPRecordManager's queue while
                // applying the selected actions. Otherwise, for instance, we might be trying to
                // move a file to complete at the same time the FPRecordManager is popping it off
                // the queue.
                DataEntryApplication.FileRequestHandler.PauseProcessingQueue();

                bool delayCurrentFile = false;

                if (GetFileIdsForAction(_STAPLE).Count() == 1)
                {
                    UtilityMethods.ShowMessageBox("At least 2 documents must be set to '" + _STAPLE +
                        "' in order create a stapled document.", "Staple error", true);
                    return false;
                }

                // If the currently displayed file is to be changed.
                if (_originalFileID != CurrentFileID)
                {
                    // Make sure changes are saved if use wants them saved.
                    if (!PromptToSaveCurrentFileChanges())
                    {
                        return false;
                    }

                    // If a new current file has been specified, it should be officially moved into
                    // the queue (via fallback status) if it is not already and it should be
                    // requested to be the next file displayed.
                    if (CurrentFileID != -1)
                    {
                        _checkedOutFileIDs.Remove(CurrentFileID);

                        DataEntryApplication.FileRequestHandler.SetFallbackStatus(
                            CurrentFileID, EActionStatus.kActionPending);

                        if (!DataEntryApplication.RequestFile(CurrentFileID))
                        {
                            new ExtractException("ELI37565",
                                "Specified current file is not available for processing.").Display();
                        }
                    }

                    // Since the user does not want the original file displayed in verification any
                    // longer, delay it.
                    delayCurrentFile = true;
                }

                // Handle files that have been ignored.
                var ignoredIds = GetFileIdsForAction(_IGNORE);
                StandardActionProcessor(ignoredIds, TagForIgnore, "", "");

                // Handle files that have been stapled.
                ProcessStapledPages();

                // Any remaining files ("Do nothing") that were checked-out for the purpose of being
                // locked from other users should be released back to their previous status.
                ReleaseCheckedOutFiles();

                // The delay of the current file should happen last (if necessary).
                if (delayCurrentFile)
                {
                    DataEntryApplication.DelayFile();
                }

                ClearData();

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37566", "Failed to apply document actions.");
            }
            finally
            {
                // Allow the FPRecordManager queue to resume distribution of files.
                DataEntryApplication.FileRequestHandler.ResumeProcessingQueue();
            }
        }

        /// <summary>
        /// Cancels all specified actions without applying them.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Return all files checked out in order to lock them from other processes to their
                // previous status.
                ReleaseCheckedOutFiles();

                ClearData();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37567", "Error cancelling document actions.");
            }
        }

        #endregion IFAMFileInspectorColumn

        #region Private members

        /// <summary>
        /// Gets the currently displayed <see cref="FAMFileInspectorForm"/>.
        /// </summary>
        static FAMFileInspectorForm FAMFileInspectorForm
        {
            get
            {
                return Application.OpenForms.OfType<FAMFileInspectorForm>().Single();
            }
        }

        /// <summary>
        /// Gets the value choices available to the specified <see paramref="fileId"/> or all
        /// potential value choices for any file if <see paramref="fileId"/> is -1.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>The value choices available to the specified <see paramref="fileId"/>.
        /// </returns>
        IEnumerable<string> GetValueChoicesHelper(int fileId)
        {
            if (_inUseFiles.Contains(fileId))
            {
                return new[] { GetInUseValue(fileId) };
            }
            else
            {
                return _ALL_FILE_OPTIONS;
            }
        }

        /// <summary>
        /// Gets a value for the column that indicates the file is in use. Includes the name of the
        /// user that currently has the file locked.
        /// </summary>
        /// <param name="fileId">The file id.</param>
        /// <returns>The value for the column that indicates the file is in use.</returns>
        string GetInUseValue(int fileId)
        {
            Recordset adoRecordset = null;
            try
            {
                adoRecordset = FileProcessingDB.GetResultsForQuery(
                    "SELECT [UserName] FROM [FAMUser] " +
                    "   INNER JOIN [FAMSession] ON [FAMUserID] = [FAMUser].[ID] " +
                    "   INNER JOIN [LockedFile] ON [UPIID] = [FAMSession].[ID] " +
                    "   WHERE [FileID] = " + fileId.ToString(CultureInfo.InvariantCulture));

                string user = adoRecordset.EOF ? "" : (string)adoRecordset.Fields[0].Value;
                if (string.IsNullOrWhiteSpace(user))
                {
                    user = "unknown";
                }

                return string.Format(CultureInfo.CurrentCulture, _IN_USE, user);
            }
            finally
            {
                if (adoRecordset != null)
                {
                    adoRecordset.Close(); 
                }
            }
        }

        /// <summary>
        /// Processes documents to be stapled as one.
        /// </summary>
        void ProcessStapledPages()
        {
            var stapledIds = GetFileIdsForAction(_STAPLE);
            if (!stapledIds.Any())
            {
                return;
            }

            ExtractException.Assert("ELI37568", "Unable to staple a single document", stapledIds.Count() > 1);

            string outputFileName = null;
            var imagePages = new List<ImagePage>();

            foreach (int fileId in stapledIds)
            {
                string fileName = "";
                int pageCount = 0;
                FAMFileInspectorForm.GetFileInfo(fileId, out fileName, out pageCount);

                // Generate the output document name.
                if (outputFileName == null)
                {
                    outputFileName = GetPathTags(fileName).Expand(StapledDocumentOutput);
                    _stapledOutputDocument = outputFileName;
                }

                // Compile the pages to output.
                imagePages.AddRange(Enumerable.Range(1, pageCount)
                    .Select(page => new ImagePage(fileName, page, 0)));
            }

            ImageMethods.StaplePagesAsNewDocument(imagePages, outputFileName);

            // Sets to complete in the current action, and applies a tag or metadata value as
            // configured.
            StandardActionProcessor(stapledIds, TagForStaple,
                StapledIntoMetadataFieldName, StapledIntoMetadataFieldValue);
        }

        /// <summary>
        /// Gets the duplicate document path tags.
        /// </summary>
        /// <param name="sourceDocName"></param>
        SourceDocumentPathTags GetPathTags(string sourceDocName)
        {
            var pathTags = new SourceDocumentPathTags(sourceDocName);
            pathTags.AddCustomTag("<FirstName>", (unused) => FirstName, false);
            pathTags.AddCustomTag("<LastName>", (unused) => LastName, false);
            pathTags.AddCustomTag("<DOB>", (unused) => DOB, false);
            pathTags.AddCustomTag("<CollectionDate>", (unused) => CollectionDate, false);
            pathTags.AddCustomTag("<StapledDocumentOutput>", (unused) => _stapledOutputDocument, false);

            return pathTags;
        }

        /// <summary>
        /// Applies basic handling of a document that has been acted upon by setting the file to
        /// complete in the current action and applies a tag or metadata value as specified.
        /// </summary>
        /// <param name="fileIds">The file IDs to be acted upon.</param>
        /// <param name="tagNameToApply">The tag name to apply to these files or
        /// <see langword="null"/> if no tag should be applied.</param>
        /// <param name="metadataFieldName">The metadata field name to apply to set for these files
        /// or <see langword="null"/> if no metadata field should be updated.</param>
        /// <param name="metadataFieldValue">The value to apply for the specified
        /// <see paramref="metadataFieldName"/>.</param>
        void StandardActionProcessor(IEnumerable<int> fileIds, string tagNameToApply,
            string metadataFieldName, string metadataFieldValue)
        {
            foreach (int fileId in fileIds)
            {
                // Since these files are being processed, remove from _checkedOutFileIDs so they
                // don't get released back to their previous status.
                _checkedOutFileIDs.Remove(fileId);

                // Make as complete in the current action.
                EActionStatus oldStatus;
                FileProcessingDB.SetStatusForFile(fileId,
                    DataEntryApplication.DatabaseActionName, EActionStatus.kActionCompleted, true,
                    false, out oldStatus);

                // Apply any specified tag.
                if (!string.IsNullOrWhiteSpace(tagNameToApply))
                {
                    FileProcessingDB.TagFile(fileId, tagNameToApply);
                }

                // Set any specified metadata field value.
                if (!string.IsNullOrWhiteSpace(metadataFieldName))
                {
                    string fileName = "";
                    int pageCount = 0;
                    FAMFileInspectorForm.GetFileInfo(fileId, out fileName, out pageCount);

                    FileProcessingDB.SetMetadataFieldValue(fileId,
                        metadataFieldName, GetPathTags(fileName).Expand(metadataFieldValue));
                }

                // Direct the file to be released from processing in the current verification UI.
                DataEntryApplication.ReleaseFile(fileId);
            }
        }

        /// <summary>
        /// Gets the file IDs that have been marked with the specified <see paramref="action"/>.
        /// </summary>
        /// <param name="action">The name of the action.</param>
        /// <returns>The file IDs that have been marked with the specified <see paramref="action"/>.
        /// </returns>
        IEnumerable<int> GetFileIdsForAction(string action)
        {
            return _currentValues
                .Where(valuePair => valuePair.Value == action)
                .Select(valuePair => valuePair.Key);
        }

        /// <summary>
        /// Prompts to save any unsaved changes in the document currently displayed for
        /// verification.
        /// </summary>
        /// <returns><see langword="true"/> if the user chose to either save or disregard the
        /// changes; <see langword="false"/> if the user chose to cancel the operation.</returns>
        bool PromptToSaveCurrentFileChanges()
        {
            if (DataEntryApplication.Dirty)
            {
                using (var messageBox = new CustomizableMessageBox())
                {
                    messageBox.Caption = "Save changes?";
                    messageBox.Text = "There are unsaved changes in '" +
                        Path.GetFileName(OriginalFileName) + "'.\r\n\r\nSave changes?";
                    messageBox.AddStandardButtons(MessageBoxButtons.YesNoCancel);
                    string response = messageBox.Show(FAMFileInspectorForm);
                    if (response == "Yes")
                    {
                        DataEntryApplication.SaveData(false);
                    }
                    else if (response == "Cancel")
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Releases the checked out files from the FAM's queue and back to the action status they
        /// were before being checked out by this instance.
        /// </summary>
        void ReleaseCheckedOutFiles()
        {
            foreach (int fileId in _checkedOutFileIDs)
            {
                DataEntryApplication.ReleaseFile(fileId);
            }

            _checkedOutFileIDs.Clear();
        }

        /// <summary>
        /// Clears data pertaining to the last file loaded.
        /// </summary>
        void ClearData()
        {
            _currentValues.Clear();
            _inUseFiles.Clear();
            _valuesToRefresh.Clear();
            _checkedOutFileIDs.Clear();
            _currentFileID = -1;
            _originalFileID = -1;
            _stapledOutputDocument = "";
            _dirty = false;
        }

        #endregion Private members
    }
}
