using ADODB;
using Extract.FileActionManager.Forms;
using Extract.FileActionManager.Utilities;
using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class DuplicateDocumentsFFIColumn : IFAMFileInspectorColumn, IFFIDataManager
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

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> currently being used.
        /// </summary>
        IFileProcessingDB _fileProcessingDB;

        /// <summary>
        /// <see langword="true"/> if any file actions have changes since the FFI form was displayed
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The name of a stapled document that has been output via the staple action.
        /// </summary>
        string _stapledOutputDocument;

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

        #region Events

        /// <summary>
        /// Raised when operations are applied via the duplicate document window.
        /// </summary>
        public event EventHandler<DuplicateDocumentsAppliedEventArgs> DuplicateDocumentsApplied;

        #endregion Events

        #region Properties

        /// <summary>
        /// Represents the action name and status name associated with a single option available in
        /// the duplicate documents UI.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public struct DuplicateDocumentOption
        {
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public string Action;

            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
            public string Status;
        }

        /// <summary>
        /// The value indicates the document should be the one displayed in the verification UI.
        /// </summary>
        public virtual DuplicateDocumentOption CurrentOption
        {
            get
            {
                return new DuplicateDocumentOption { Action = "Current", Status = "Current" };
            }
        }

        /// <summary>
        /// The value that indicates no action should be taken on the file.
        /// </summary>
        public virtual DuplicateDocumentOption DoNothingOption
        {
            get
            {
                return new DuplicateDocumentOption { Action = "Do nothing", Status = "" };
            }
        }

        /// <summary>
        /// The value that indicates the document should be stapled into a new unified document.
        /// </summary>
        public virtual DuplicateDocumentOption StapleOption
        {
            get
            {
                return new DuplicateDocumentOption { Action = "Staple", Status = "Stapled" };
            }
        }

        /// <summary>
        /// The value that indicates the document should be stapled into a new unified document but
        /// without the first page of this document.
        /// </summary>
        public virtual DuplicateDocumentOption StapleWithoutFirstPageOption
        {
            get
            {
                return new DuplicateDocumentOption { Action = "Staple w/o 1st page", Status = "Stapled" };
            }
        }

        /// <summary>
        /// The value indicates the document should be removed from the queue without filing the
        /// document's results.
        /// </summary>
        public virtual DuplicateDocumentOption IgnoreOption
        {
            get
            {
                return new DuplicateDocumentOption { Action = "Discard", Status = "Discarded" };
            }
        }

        /// <summary>
        /// The value indicates the document should be skipped.
        /// </summary>
        public virtual DuplicateDocumentOption SkipOption
        {
            get
            {
                return new DuplicateDocumentOption { Action = "Skip", Status = "Skipped" };
            }
        }

        /// <summary>
        /// The action name to be set to pending for ignored/stapled documents so they can be cleaned up
        /// </summary>
        public virtual string CleanupAction
        {
            get;
            set;
        }

        /// <summary>
        /// Stores the currently selected action for all files that have been displayed in FFI.
        /// </summary>
        protected Dictionary<int, string> CurrentValues
        {
            get;
        } = new Dictionary<int, string>();

        /// <summary>
        /// Stores the action that was selected by default when the FFI was opened.
        /// </summary>
        protected Dictionary<int, string> InitialValues
        {
            get;
        } = new Dictionary<int, string>();

        /// <summary>
        /// The files that are currently checked out for processing in different processes.
        /// </summary>
        protected HashSet<int> InUseFiles
        {
            get;
        } = new HashSet<int>();

        /// <summary>
        /// Stores the previous file action status for the current action for all files that have
        /// been displayed in the FFI.
        /// </summary>
        protected Dictionary<int, EActionStatus> PreviousStatuses
        {
            get;
        } = new Dictionary<int, EActionStatus>();

        /// <summary>
        /// The values that have been programmatically changed since the last GetValue call, thus
        /// need to be updated in the FFI.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        protected HashSet<int> ValuesToRefresh
        {
            get;
        } = new HashSet<int>();

        /// <summary>
        /// Files that have been checked out by the current process in order to keep these files
        /// from being processed on another action.
        /// </summary>
        protected HashSet<int> CheckedOutFileIds
        {
            get;
        } = new HashSet<int>();

        /// <summary>
        /// The file ID of the document that was displayed in verification at the time the FFI was
        /// launched.
        /// </summary>
        public HashSet<int> OriginalFileIds
        {
            get;
        } = new HashSet<int>();

        /// <summary>
        /// The file ID of the document that has been selected as the one to be displayed in verification.
        /// </summary>
        protected HashSet<int> CurrentFileIds
        {
            get;
        } = new HashSet<int>();

        /// <summary>
        /// Gets the <see cref="IDataEntryApplication"/> instance currently being used in
        /// verification.
        /// </summary>
        /// <value>
        /// The <see cref="IDataEntryApplication"/> instance currently being used in verification.
        /// </value>
        public IDataEntryApplication DataEntryApplication
        {
            get;
            private set;
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
        /// Initializes column with the specified set of files currently loaded in
        /// <see paramref="dataEntryApplication"/>
        /// </summary>
        /// <param name="dataEntryApplication">The <see cref="IDataEntryApplication"/> instance
        /// currently being used in verification.</param>
        /// <param name="fileIds">The IDs of the files being </param>
        public virtual void Initialize(IDataEntryApplication dataEntryApplication)
        {
            try
            {
                ClearData();

                DataEntryApplication = dataEntryApplication;
                FileProcessingDB = dataEntryApplication.FileProcessingDB;
                foreach (int fileId in dataEntryApplication.FileIds)
                {
                    OriginalFileIds.Add(fileId);
                    CurrentFileIds.Add(fileId);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44743");
            }
        }

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
                return PreviousStatuses[fileID];
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
                return
                    CurrentValues.TryGetValue(fileID, out string currentValue) &&
                    InitialValues.TryGetValue(fileID, out string initialValue) &&
                    currentValue != initialValue;
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

                        ExtractException.Assert("ELI43209",
                            "Duplicate document handling cannot be performed for more than one workflow at a time.",
                            !FileProcessingDB.RunningAllWorkflows);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37952");
                }
            }
        }

        /// <summary>
        /// Returns the handle of the <see cref="FAMFileInspectorForm"/> in which this column is being used
        /// or <see cref="IntPtr.Zero"/> in the case the column is not currently initialized in an FFI.
        /// </summary>
        public IntPtr FAMFileInspectorFormHandle
        {
            get
            {
                try
                {
                    return (FAMFileInspectorForm == null || FAMFileInspectorForm.IsDisposed)
                        ? IntPtr.Zero
                        : FAMFileInspectorForm.Handle;
                }
                catch (Exception ex)
                {
                    throw ExtractException.CreateComVisible("ELI49784", "Failed to retrieve FFI handle", ex);
                }
            }

            set
            {
                try
                {
                    if (value != this.FAMFileInspectorFormHandle)
                    {
                        FAMFileInspectorForm = (FAMFileInspectorForm)Form.FromHandle(value);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.CreateComVisible("ELI49785", "Failed to apply FFI handle", ex);
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
        /// Gets a value indicating whether FFI menu main and context menu options should be limited
        /// to basic non-custom options. The main database menu and custom file handlers context
        /// menu options will not be shown.
        /// </summary>
        /// <value><see langword="true"/> to limit menu options to basic options only; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool BasicMenuOptionsOnly
        {
            get
            {
                return true;
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
                        choices = choices.Except(new[] { CurrentOption.Action });
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
                if (InUseFiles.Contains(fileID) || !CurrentValues.TryGetValue(fileID, out value))
                {
                    // This is the first time this file has been loaded into the FFI; initialize
                    // the value.
                    value = DoNothingOption.Action;
                    EActionStatus previousStatus = EActionStatus.kActionUnattempted;
                    if (DataEntryApplication.FileRequestHandler.CheckoutForProcessing(
                        fileID, true, out previousStatus))
                    {
                        InUseFiles.Remove(fileID);

                        if (CurrentFileIds.Contains(fileID))
                        {
                            // This is the file currently displayed in verification.
                            value = CurrentOption.Action;
                        }
                        else if (previousStatus != EActionStatus.kActionProcessing)
                        {
                            // This is the file has been successfully checked out (locked) by the
                            // current process.
                            CheckedOutFileIds.Add(fileID);
                        }

                        PreviousStatuses[fileID] = previousStatus;
                    }
                    else
                    {
                        // If the file could not be checked out, another process has the file locked
                        // for processing. Mark as in-use.
                        InUseFiles.Add(fileID);
                        value = GetInUseValue(fileID);

                        PreviousStatuses[fileID] = EActionStatus.kActionProcessing;
                    }

                    InitialValues[fileID] = value;
                    CurrentValues[fileID] = value;
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
                if (!CurrentValues.TryGetValue(fileId, out currentValue) || value != currentValue)
                {
                    CurrentValues[fileId] = value;

                    if (value == CurrentOption.Action)
                    {
                        foreach (int oldFileId in CurrentFileIds.Except(new[] { fileId }))
                        {
                            CurrentValues[oldFileId] = DoNothingOption.Action;
                            ValuesToRefresh.Add(oldFileId);
                        }

                        CurrentFileIds.Clear();
                        CurrentFileIds.Add(fileId);
                    }
                    else if (CurrentFileIds.Contains(fileId))
                    {
                        // If this file had been the current file, clear CurrentFileID
                        CurrentFileIds.Remove(fileId);
                    }

                    _dirty = true;

                    // Refresh the value in the UI in order to display an asterisk to indicate when
                    // a value has changed.
                    ValuesToRefresh.Add(fileId);
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
            return ValuesToRefresh;
        }

        #endregion IFAMFileInspectorColumn

        #region IFFIDataManager

        /// <summary>
        /// Gets if there is any data that has been modified via <see cref="SetValue"/> that needs
        /// to be applied. 
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
        /// Applies all uncommitted values specified via SetValue.
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

                var filesToDelay = new List<int>();

                if (!PromptForActions())
                {
                    return false;
                }

                // If no file is specified to be current, but the previous current file is set to
                // "do nothing", set it as current so that it remains in the UI.
                if (CurrentFileIds.Count == 0)
                {
                    foreach (int fileId in OriginalFileIds.Where
                        (id => CurrentValues[id] == DoNothingOption.Action))
                    {
                        SetValue(fileId, CurrentOption.Action);
                    }
                }

                // If the currently displayed file is to be changed.
                if (OriginalFileIds.Except(CurrentFileIds).Any())
                {
                    // Make sure changes are saved if use wants them saved.
                    if (!PromptToSaveCurrentFileChanges())
                    {
                        return false;
                    }

                    foreach (int fileId in OriginalFileIds.Except(CurrentFileIds))
                    {
                        CheckedOutFileIds.Remove(fileId);

                        // Since the user does not want the original file displayed in verification any
                        // longer, delay it.
                        filesToDelay.Add(fileId);
                    }
                }

                // If a new current file has been specified, it should be officially moved into
                // the queue (via fallback status) if it is not already and it should be
                // requested to be the next file displayed.
                foreach (int newId in CurrentFileIds.Except(OriginalFileIds))
                {
                    DataEntryApplication.FileRequestHandler.SetFallbackStatus(
                        newId, EActionStatus.kActionPending);

                    if (!DataEntryApplication.RequestFile(newId))
                    {
                        new ExtractException("ELI37565",
                            "Specified current file is not available for processing.").Display();
                    }

                    // https://extract.atlassian.net/browse/ISSUE-15961
                    // If the file is to be made current, remove from CheckedOutFileIds so it isn't
                    // released by ReleaseCheckedOutFiles.
                    CheckedOutFileIds.Remove(newId);
                }

                PerformActions();

                // Any remaining files ("Do nothing") that were checked-out for the purpose of being
                // locked from other users should be released back to their previous status.
                ReleaseCheckedOutFiles();

                // The delay of the current file should happen last (if necessary).
                foreach (int fileID in filesToDelay)
                {
                    DataEntryApplication.DelayFile(fileID);
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
                // https://extract.atlassian.net/browse/ISSUE-15364
                // Prevent new files from being popped off the FPRecordManager's queue while
                // applying the selected actions. Otherwise, for instance, we might be trying to
                // move a file to complete at the same time the FPRecordManager is popping it off
                // the queue.
                DataEntryApplication.FileRequestHandler.PauseProcessingQueue();

                // Return all files checked out in order to lock them from other processes to their
                // previous status.
                ReleaseCheckedOutFiles();

                ClearData();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37567", "Error cancelling document actions.");
            }
            finally
            {
                // Allow the FPRecordManager queue to resume distribution of files.
                DataEntryApplication.FileRequestHandler.ResumeProcessingQueue();
            }
        }

        #endregion IFFIDataManager

        #region Protected members

        /// <summary>
        /// Gets all file options.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<string> AllFileOptions
        {
            get
            {
                return new[] {
                    DoNothingOption.Action,
                    StapleOption.Action,
                    IgnoreOption.Action,
                    SkipOption.Action,
                    CurrentOption.Action };
            }
        }

        /// <summary>
        /// Gets the value choices available to the specified <see paramref="fileId"/> or all
        /// potential value choices for any file if <see paramref="fileId"/> is -1.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>The value choices available to the specified <see paramref="fileId"/>.
        /// </returns>
        protected virtual IEnumerable<string> GetValueChoicesHelper(int fileId)
        {
            try
            {
                if (InUseFiles.Contains(fileId))
                {
                    return new[] { GetInUseValue(fileId) };
                }
                else
                {
                    return AllFileOptions;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44805");
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
                    "   INNER JOIN [ActiveFAM] ON [FAMSessionID] = [FAMSession].[ID] " +
                    "   INNER JOIN [LockedFile] ON [ActiveFAMID] = [ActiveFAM].[ID] " +
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
        /// Returns the <see cref="FAMFileInspectorForm"/> in which this column is being used
        /// or <c>null</c> in the case the column is not currently initialized in an FFI.
        /// </summary>
        protected FAMFileInspectorForm FAMFileInspectorForm
        {
            get;
            set;
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

                IPathTags pathTags = GetPathTags(fileName);

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
        protected virtual IPathTags GetPathTags(string sourceDocName)
        {
            var pathTags = new FileActionManagerPathTags(null, sourceDocName);
            pathTags.DatabaseServer = FileProcessingDB.DatabaseServer;
            pathTags.DatabaseName = FileProcessingDB.DatabaseName;
            pathTags.Workflow = FileProcessingDB.ActiveWorkflow;

            pathTags.AddTag("<FirstName>", FirstName);
            pathTags.AddTag("<LastName>", LastName);
            pathTags.AddTag("<DOB>", DOB);
            pathTags.AddTag("<CollectionDate>", CollectionDate);
            pathTags.AddTag("<StapledDocumentOutput>", _stapledOutputDocument);

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
                CheckedOutFileIds.Remove(fileId);

                // Make as complete in the current action.
                EActionStatus oldStatus;
                FileProcessingDB.SetStatusForFile(fileId,
                    DataEntryApplication.DatabaseActionName, -1, newStatus, true, false, out oldStatus);

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
            return CurrentValues
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
                if (GetFileIdsForAction(StapleOption.Action).Count() +
                        GetFileIdsForAction(StapleWithoutFirstPageOption.Action).Count() == 1)
                {
                    UtilityMethods.ShowMessageBox("At least 2 documents must be set to " + 
                        StapleOption.Action + "' in order create a stapled document.", "Staple error", true);
                    return false;
                }

                var stapledIdsWithoutFirstPage = GetFileIdsForAction(StapleWithoutFirstPageOption.Action);
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
                var ignoredIds = GetFileIdsForAction(IgnoreOption.Action);
                StandardActionProcessor(
                    ignoredIds, EActionStatus.kActionCompleted, TagForIgnore, "", "");

                // Handle files that have been skipped.
                var skippedIds = GetFileIdsForAction(SkipOption.Action);
                StandardActionProcessor(skippedIds, EActionStatus.kActionSkipped, "", "", "");

                // Handle files that have been stapled.
                var stapledIdsWithoutFirstPage = GetFileIdsForAction(StapleWithoutFirstPageOption.Action);
                var stapledIds = GetFileIdsForAction(StapleOption.Action).Union(stapledIdsWithoutFirstPage);
                CreateStapledOutput(stapledIds, stapledIdsWithoutFirstPage);
                StandardActionProcessor(stapledIds, EActionStatus.kActionCompleted, TagForStaple,
                    StapledIntoMetadataFieldName, StapledIntoMetadataFieldValue);

                // https://extract.atlassian.net/browse/ISSUE-13751
                // Allow for ignored/stapled documents to be forwarded to a cleanup action.
                if (!string.IsNullOrWhiteSpace(CleanupAction))
                {
                    foreach (int fileId in ignoredIds
                        .Union(stapledIds)
                        .Union(stapledIdsWithoutFirstPage))
                    {
                        // Set this file ID to Pending for the specified action name 
                        EActionStatus oldStatus;
                        FileProcessingDB.SetStatusForFile(fileId,
                            CleanupAction, -1, EActionStatus.kActionPending, true, false, out oldStatus);
                    }
                }

                // Translate the actions by ID to a dictionary of actions to the associated set of file IDs.
                var fileIDsByAction = CurrentValues
                    .GroupBy(value => value.Value,
                             (key, result) => (Action: key, FileIDs: result.Select(entry => entry.Key)))
                    .ToDictionary(key => key.Action, key => key.FileIDs);

                DuplicateDocumentsApplied?.Invoke(this, new DuplicateDocumentsAppliedEventArgs(fileIDsByAction));
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
                    messageBox.Text = "There are unsaved changes in the previously opened file." +
                        "\r\n\r\nSave changes?";
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

                if (OriginalFileIds.Intersect(CurrentFileIds).Count() != CurrentFileIds.Count)
                {
                    FAMFileInspectorForm.GetFileInfo(CurrentFileIds.Single(), out string fileName, out int pageCount);

                    message.Append("Make '");
                    message.Append(Path.GetFileName(fileName));
                    message.AppendLine("' the current document.");
                }

                int stapledDocuments = CurrentValues.Values.Count(value =>
                    value == StapleOption.Action || value == StapleWithoutFirstPageOption.Action);
                if (stapledDocuments > 0)
                {
                    message.AppendLine(string.Format(CultureInfo.CurrentCulture,
                        "Staple {0} file(s).", stapledDocuments));
                }

                int ignoredDocuments = CurrentValues.Values.Count(value => value == IgnoreOption.Action);
                if (ignoredDocuments > 0)
                {
                    message.AppendLine(string.Format(CultureInfo.CurrentCulture,
                        "Ignore {0} file(s).", ignoredDocuments));
                }

                int skippedDocuments = CurrentValues.Values.Count(value => value == SkipOption.Action);
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
            foreach (int fileId in CheckedOutFileIds.ToArray())
            {
                DataEntryApplication.ReleaseFile(fileId);
            }

            CheckedOutFileIds.Clear();
        }

        /// <summary>
        /// Clears data pertaining to the last file loaded.
        /// </summary>
        protected virtual void ClearData()
        {
            CurrentValues.Clear();
            InUseFiles.Clear();
            PreviousStatuses.Clear();
            ValuesToRefresh.Clear();
            CheckedOutFileIds.Clear();
            OriginalFileIds.Clear();
            CurrentFileIds.Clear();
            _stapledOutputDocument = "";
            _dirty = false;
        }

        #endregion Protected members
    }
}
