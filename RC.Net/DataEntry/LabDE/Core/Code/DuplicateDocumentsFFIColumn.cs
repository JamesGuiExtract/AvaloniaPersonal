using ADODB;
using Extract.FileActionManager.Utilities;
using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A <see cref="IFAMFileInspectorColumn"/> used to allow the user to take action on potential
    /// duplicate files in the FFI.
    /// </summary>
    [ComVisible(true)]
    [Guid("15BF229C-6D8A-41E3-A3ED-5BA211F1AE7B")]
    public class DuplicateDocumentsFFIColumn : IFAMFileInspectorColumn
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DuplicateDocumentsFFIColumn).ToString();

        /// <summary>
        /// The name of the column in the FFI.
        /// </summary>
        static readonly string _HEADER_TEXT = "Action";

        /// <summary>
        /// The default width of the column in pixels.
        /// </summary>
        static readonly int _DEFAULT_WIDTH = 135;

        /// <summary>
        /// The value that indicates another user already has the file locked for processing.
        /// (For use with String.Format where the owning user is specified as the only parameter).
        /// </summary>
        static readonly string _IN_USE = "In use ({0})";

        /// <summary>
        /// The value indicates the document should be the one displayed in the verification UI.
        /// </summary>
        protected static readonly string CurrentOption = "Current document";

        /// <summary>
        /// The value that indicates no action should be taken on the file.
        /// </summary>
        protected static readonly string DoNothingOption = "Do nothing";

        /// <summary>
        /// The value that indicates the document should be stapled into a new unified document.
        /// </summary>
        protected static readonly string StapleOption = "Staple";

        /// <summary>
        /// The value that indicates the document should be stapled into a new unified document but
        /// without the first page of this document.
        /// </summary>
        protected static readonly string StapleWithoutFirstPageOption = "Staple w/o 1st page";

        /// <summary>
        /// The value indicates the document should be removed from the queue without filing the
        /// document's results.
        /// </summary>
        protected static readonly string IgnoreOption = "Ignore document";

        /// <summary>
        /// The value indicates the document should be skipped.
        /// </summary>
        protected static readonly string SkipOption = "Skip";

        /// <summary>
        /// All possible actions for files that are owned (checked-out) by the current process.
        /// </summary>
        static string[] _ALL_FILE_OPTIONS = new[] { DoNothingOption, StapleOption, IgnoreOption, SkipOption, CurrentOption };

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> currently being used.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

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
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI37954", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37955");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the original file id.
        /// </summary>
        public int OriginalFileId
        {
            get
            {
                return _originalFileID;
            }
        }

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
                            _currentValues[_currentFileID] = DoNothingOption;
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
        public virtual EActionStatus GetPreviousStatus(int fileID)
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

        /// <summary>
        /// Indicates whether the specified action for <see paramref="fileID"/> has been changed
        /// from value that was originally associated with the file when loaded.
        /// </summary>
        /// <param name="fileID">The ID of the file.</param>
        /// <returns><see langword="true"/> if the value has changed; otherwise,
        /// <see langword="false"/>.</returns>
        public virtual bool HasValueChanged(int fileID)
        {
            try
            {
                if (_inUseFiles.Contains(fileID))
                {
                    return false;
                }
                else if ((fileID == CurrentFileID || fileID == _originalFileID) &&
                         CurrentFileID != _originalFileID)
                {
                    return true;
                }
                else if (fileID == CurrentFileID || _currentValues[fileID] == DoNothingOption)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37586");
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
            get
            {
                return _fileProcessingDB;
            }

            set
            {
                try
                {
                    _fileProcessingDB = value;
                    if (_fileProcessingDB != null)
                    {
                        // At the time FileProcessingDB is initialized, ensure we have access to an
                        // IFileRequestHandler.
                        ExtractException.Assert("ELI37591",
                            "Duplicate documents cannot be presented in the current context.",
                            DataEntryApplication.FileRequestHandler != null);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37952");
                }
            }
        }

        /// <summary>
        /// Gets the name for the column header. Will also appear in the context menu if
        /// <see cref="GetContextMenuChoices"/> returns a value.
        /// </summary>
        public virtual string HeaderText
        {
            get
            {
                return _HEADER_TEXT;
            }
        }

        /// <summary>
        /// Gets the default width the column in pixels.
        /// </summary>
        public virtual int DefaultWidth
        {
            get
            {
                return _DEFAULT_WIDTH;
            }
        }

        /// <summary>
        /// Gets the <see cref="FFIColumnType"/> defining what type of column is represented.
        /// </summary>
        public virtual FFIColumnType FFIColumnType
        {
            get
            {
                return FFIColumnType.Combo;
            }
        }

        /// <summary>
        /// Gets whether this column is read only.
        /// </summary>
        public virtual bool ReadOnly
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
        public virtual bool RequireOkCancel
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
        public virtual bool Dirty
        {
            get
            {
                return _dirty;
            }
        }

        /// <summary>
        /// Gets a description of changes that should be displayed to the user in a prompt when
        /// applying changes. If <see langword="null"/>, no prompt will be displayed when applying
        /// changed.
        /// </summary>
        public string ApplyPrompt
        {
            get
            {
                return ListOfChanges;
            }
        }

        /// <summary>
        /// Gets a description of changes that should be displayed to the user in a prompt when
        /// the user is canceling changes. If <see langword="null"/>, no prompt will be displayed
        /// when canceling except if the FFI is closed via the form's cancel button (red X).
        /// </summary>
        public virtual string CancelPrompt
        {
            get
            {
                return ListOfChanges;
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
        public virtual IVariantVector GetValueChoices(int fileId)
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
        /// <see langword="true"/>).
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
        public virtual IVariantVector GetContextMenuChoices(HashSet<int> fileIds)
        {
            try
            {
                IEnumerable<string> choices = null;

                if (fileIds == null)
                {
                    choices = AllFileOptions;
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
                        choices = choices.Except(new[] { CurrentOption });
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
        public virtual string GetValue(int fileID)
        {
            try
            {
                string value = null;

                // Attempt to retrieve the value we already have for this file, except in the case
                // of a file that has been locked by another user... in that case attempt to check
                // the file out again in case it has been released by the other user.
                if (_inUseFiles.Contains(fileID) || !_currentValues.TryGetValue(fileID, out value))
                {
                    // This is the first time this file has been loaded into the FFI; initialize
                    // the value.
                    value = DoNothingOption;
                    EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                    if (DataEntryApplication.FileRequestHandler.CheckoutForProcessing(
                        fileID, out previousStatus))
                    {
                        _inUseFiles.Remove(fileID);

                        if (fileID == _currentFileID)
                        {
                            // This is the file currently displayed in verification.
                            value = CurrentOption;
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
                }

                // If the value is different from what it originally was, indicate that it has
                // changed with an asterisk.
                if (HasValueChanged(fileID))
                {
                    value += "*";
                }

                return value;
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
        public virtual bool SetValue(int fileId, string value)
        {
            try
            {
                // If an asterisk was applied to indicate a changed value, do not consider that part
                // of the actual value.
                value = value.TrimEnd('*');

                // Check if the value is being set for the first time or changed
                string currentValue = null;
                if (!_currentValues.TryGetValue(fileId, out currentValue) || value != currentValue)
                {
                    _currentValues[fileId] = value;

                    if (value == CurrentOption)
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

                    // Refresh the value in the UI in order to display an asterisk to indicate when
                    // a value has changed.
                    _valuesToRefresh.Add(fileId);
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
        public virtual IEnumerable<int> GetValuesToRefresh()
        {
            return _valuesToRefresh;
        }

        /// <summary>
        /// Applies all uncommitted values specified via SetValue. (Unused if
        /// <see cref="RequireOkCancel"/> is <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        public virtual bool Apply()
        {
            try
            {
                // Prevent new files from being popped off the FPRecordManager's queue while
                // applying the selected actions. Otherwise, for instance, we might be trying to
                // move a file to complete at the same time the FPRecordManager is popping it off
                // the queue.
                DataEntryApplication.FileRequestHandler.PauseProcessingQueue();

                bool delayCurrentFile = false;

                if (!PromptForActions())
                {
                    return false;
                }

                // If no file is specified to be current, but the previous current file is set to
                // "do nothing", set it as current so that it remains in the UI.
                if (CurrentFileID == -1 && _originalFileID != -1 &&
                    _currentValues[_originalFileID] == DoNothingOption)
                {
                    SetValue(_originalFileID, CurrentOption);
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

                PerformActions();

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
        public virtual void Cancel()
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

        #region Protected members

        /// <summary>
        /// Gets the currently displayed <see cref="FAMFileInspectorForm"/>.
        /// </summary>
        protected static FAMFileInspectorForm FAMFileInspectorForm
        {
            get
            {
                return Application.OpenForms.OfType<FAMFileInspectorForm>().Single();
            }
        }

        /// <summary>
        /// Gets all file options.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<string> AllFileOptions
        {
            get
            {
                return _ALL_FILE_OPTIONS;
            }
        }

        /// <summary>
        /// Gets the value choices available to the specified <see paramref="fileId"/> or all
        /// potential value choices for any file if <see paramref="fileId"/> is -1.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>The value choices available to the specified <see paramref="fileId"/>.
        /// </returns>
        protected IEnumerable<string> GetValueChoicesHelper(int fileId)
        {
            if (_inUseFiles.Contains(fileId))
            {
                return new[] { GetInUseValue(fileId) };
            }
            else
            {
                return AllFileOptions;
            }
        }

        /// <summary>
        /// Gets a value for the column that indicates the file is in use. Includes the name of the
        /// user that currently has the file locked.
        /// </summary>
        /// <param name="fileId">The file id.</param>
        /// <returns>The value for the column that indicates the file is in use.</returns>
        protected string GetInUseValue(int fileId)
        {
            Recordset adoRecordset = null;
            try
            {
                adoRecordset = FileProcessingDB.GetResultsForQuery(
                    "SELECT [UserName] FROM [FAMUser] " +
                    "   INNER JOIN [FAMSession] ON [FAMUserID] = [FAMUser].[ID] " +
                    "   INNER JOIN [ActiveFAM] ON [ActiveFAM].[UPI] = [FAMSession].[UPI] " +
                    "   INNER JOIN [LockedFile] ON [UPIID] = [ActiveFAM].[ID] " +
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
        /// Staples the specified files into a single unified document.
        /// </summary>
        /// <param name="fileIds">The IDs of the files to staple.</param>
        /// <param name="fileIdsWithoutFirstPage">The IDs of the files to be stapled, but without
        /// the their respective first pages.</param>
        protected void CreateStapledOutput(IEnumerable<int> fileIds, IEnumerable<int> fileIdsWithoutFirstPage)
        {
            // Ensure that fileIds includes all files from fileIdsWithoutFirstPage and that the
            // documents are stapled in order of file ID (as they appear in the FFI).
            var allFilesIds = fileIds.Union(fileIdsWithoutFirstPage).OrderBy(id => id);

            if (!allFilesIds.Any())
            {
                return;
            }

            ExtractException.Assert("ELI37568", "Unable to staple a single document", allFilesIds.Count() > 1);

            string outputFileName = null;
            string requiredFolder = null;
            var imagePages = new List<ImagePage>();

            foreach (int fileId in allFilesIds)
            {
                string fileName = "";
                int pageCount = 0;
                FAMFileInspectorForm.GetFileInfo(fileId, out fileName, out pageCount);

                SourceDocumentPathTags pathTags = GetPathTags(fileName);

                // Generate the output document name.
                if (outputFileName == null)
                {
                    outputFileName = pathTags.Expand(StapledDocumentOutput);
                    _stapledOutputDocument = outputFileName;

                    if (StapledDocumentOutput.StartsWith(
                        "$DirOf($DirOf(<SourceDocName>))", StringComparison.OrdinalIgnoreCase))
                    {
                        requiredFolder = pathTags.Expand("$DirOf($DirOf(<SourceDocName>))");
                    }
                }
                // If we are validating that staple documents should not come from separate
                // directory trees, ensure the grandparent folder for the original document
                // is the same as the grandparent folder of this document.
                else if (requiredFolder != null)
                {
                    string folder = pathTags.Expand("$DirOf($DirOf(<SourceDocName>))");
                    ExtractException.Assert("ELI37605", 
                        "Unable to staple documents from separate departments.",
                        folder.Equals(requiredFolder, StringComparison.OrdinalIgnoreCase));
                }

                bool removeFirstPage = fileIdsWithoutFirstPage.Contains(fileId);
                ExtractException.Assert("ELI37584",
                    "Unable to exclude page from a single page document",
                    !removeFirstPage || pageCount > 1);

                var pageRange = removeFirstPage
                    ? Enumerable.Range(2, pageCount - 1)
                    : Enumerable.Range(1, pageCount);

                // Compile the pages to output.
                imagePages.AddRange(pageRange
                    .Select(page => new ImagePage(fileName, page, 0)));
            }

            ImageMethods.StaplePagesAsNewDocument(imagePages, outputFileName);
        }

        /// <summary>
        /// Gets the duplicate document path tags.
        /// </summary>
        /// <param name="sourceDocName"></param>
        protected virtual SourceDocumentPathTags GetPathTags(string sourceDocName)
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
        /// <param name="newStatus">The <see cref="EActionStatus"/> the file should be moved to in
        /// the currently processing action.</param>
        /// <param name="tagNameToApply">The tag name to apply to these files or
        /// <see langword="null"/> if no tag should be applied.</param>
        /// <param name="metadataFieldName">The metadata field name to apply to set for these files
        /// or <see langword="null"/> if no metadata field should be updated.</param>
        /// <param name="metadataFieldValue">The value to apply for the specified
        /// <see paramref="metadataFieldName"/>.</param>
        protected virtual void StandardActionProcessor(IEnumerable<int> fileIds, EActionStatus newStatus,
            string tagNameToApply, string metadataFieldName, string metadataFieldValue)
        {
            foreach (int fileId in fileIds)
            {
                // Since these files are being processed, remove from _checkedOutFileIDs so they
                // don't get released back to their previous status.
                _checkedOutFileIDs.Remove(fileId);

                // Make as complete in the current action.
                EActionStatus oldStatus;
                FileProcessingDB.SetStatusForFile(fileId,
                    DataEntryApplication.DatabaseActionName, newStatus, true, false, out oldStatus);

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
        protected IEnumerable<int> GetFileIdsForAction(string action)
        {
            return _currentValues
                .Where(valuePair => valuePair.Value == action)
                .Select(valuePair => valuePair.Key);
        }

        /// <summary>
        /// Prompts regarding any situation that prevents performing the specified action. 
        /// </summary>
        /// <returns><see langword="true"/> if the actions can be successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        protected virtual bool PromptForActions()
        {
            try
            {
                if (GetFileIdsForAction(StapleOption).Count() +
                        GetFileIdsForAction(StapleWithoutFirstPageOption).Count() == 1)
                {
                    UtilityMethods.ShowMessageBox("At least 2 documents must be set to " + 
                        StapleOption + "' in order create a stapled document.", "Staple error", true);
                    return false;
                }

                var stapledIdsWithoutFirstPage = GetFileIdsForAction(StapleWithoutFirstPageOption);
                if (stapledIdsWithoutFirstPage.Any())
                {
                    if (MessageBox.Show("You have selected to exclude the first page from " +
                        stapledIdsWithoutFirstPage.Count().ToString(CultureInfo.CurrentCulture) +
                        " document(s).\r\n\r\nAre sure you want to exclude these pages?",
                        "Exclude pages?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1, 0) == DialogResult.Cancel)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37959");
            }
        }

        /// <summary>
        /// Performs the actions specified.
        /// </summary>
        protected virtual void PerformActions()
        {
            try
            {
                // Handle files that have been ignored.
                var ignoredIds = GetFileIdsForAction(IgnoreOption);
                StandardActionProcessor(
                    ignoredIds, EActionStatus.kActionCompleted, TagForIgnore, "", "");

                // Handle files that have been skipped.
                var skippedIds = GetFileIdsForAction(SkipOption);
                StandardActionProcessor(skippedIds, EActionStatus.kActionSkipped, "", "", "");

                // Handle files that have been stapled.
                var stapledIdsWithoutFirstPage = GetFileIdsForAction(StapleWithoutFirstPageOption);
                var stapledIds = GetFileIdsForAction(StapleOption).Union(stapledIdsWithoutFirstPage);
                CreateStapledOutput(stapledIds, stapledIdsWithoutFirstPage);
                StandardActionProcessor(stapledIds, EActionStatus.kActionCompleted, TagForStaple,
                    StapledIntoMetadataFieldName, StapledIntoMetadataFieldValue);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37960");
            }
        }

        /// <summary>
        /// Prompts to save any unsaved changes in the document currently displayed for
        /// verification.
        /// </summary>
        /// <returns><see langword="true"/> if the user chose to either save or disregard the
        /// changes; <see langword="false"/> if the user chose to cancel the operation.</returns>
        protected virtual bool PromptToSaveCurrentFileChanges()
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
        /// Gets a description of the changes the user has made thus far.
        /// </summary>
        /// <value>
        /// A description of the changes the user has made thus far.
        /// </value>
        protected virtual string ListOfChanges
        {
            get
            {
                StringBuilder message = new StringBuilder();

                if (CurrentFileID != -1 && CurrentFileID != _originalFileID)
                {
                    string fileName = "";
                    int pageCount = 0;
                    FAMFileInspectorForm.GetFileInfo(CurrentFileID, out fileName, out pageCount);

                    message.Append("Make '");
                    message.Append(Path.GetFileName(fileName));
                    message.AppendLine("' the current document.");
                }

                int stapledDocuments = _currentValues.Values.Count(value =>
                    value == StapleOption || value == StapleWithoutFirstPageOption);
                if (stapledDocuments > 0)
                {
                    message.AppendLine(string.Format(CultureInfo.CurrentCulture,
                        "Staple {0} file(s).", stapledDocuments));
                }

                int ignoredDocuments = _currentValues.Values.Count(value => value == IgnoreOption);
                if (ignoredDocuments > 0)
                {
                    message.AppendLine(string.Format(CultureInfo.CurrentCulture,
                        "Ignore {0} file(s).", ignoredDocuments));
                }

                int skippedDocuments = _currentValues.Values.Count(value => value == SkipOption);
                if (skippedDocuments > 0)
                {
                    message.AppendLine(string.Format(CultureInfo.CurrentCulture,
                        "Skip {0} file(s).", skippedDocuments));
                }

                return message.ToString().TrimEnd(null);
            }
        }

        /// <summary>
        /// Releases the checked out files from the FAM's queue and back to the action status they
        /// were before being checked out by this instance.
        /// </summary>
        protected void ReleaseCheckedOutFiles()
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
        protected virtual void ClearData()
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

        #endregion Protected members
    }
}
