using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using static System.FormattableString;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A <see cref="IFAMFileInspectorColumn"/> used to allow the user to take action on potential
    /// duplicate files in the FFI.
    /// </summary>
    [ComVisible(true)]
    [Guid("974DA44B-01F0-4D61-BB54-37DC9D555791")]
    public class PaginationDuplicateDocumentsFFIColumn : DuplicateDocumentsFFIColumn
    {
        #region Fields

        /// <summary>
        /// <see langword="true"/> if any file actions have changes since the FFI form was displayed
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDuplicateDocumentsFFIColumn"/> class.
        /// </summary>
        public PaginationDuplicateDocumentsFFIColumn()
            : base()
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// The value indicates the document should be the one displayed in the verification UI.
        /// </summary>
        public override DuplicateDocumentOption CurrentOption
        {
            get
            {
                return new DuplicateDocumentOption { Action = "Show", Status = "Showing" };
            }
        }

        /// <summary>
        /// Gets if there is any data that has been modified via <see cref="SetValue"/> that needs
        /// to be applied. 
        /// </summary>
        public override bool Dirty
        {
            get
            {
                return _dirty;
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
        public override IVariantVector GetContextMenuChoices(HashSet<int> fileIds)
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
                }

                return choices.ToVariantVector();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44761", "Failed to retrieve context menu choices.");
            }
        }

        /// <summary>
        /// Sets the specified <see paramref="value"/> for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID for which the value should be set.</param>
        /// <param name="value">The value to set.</param>
        public override bool SetValue(int fileId, string value)
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
                throw ex.CreateComVisible("ELI44762", "Failed to set value.");
            }
        }

        /// <summary>
        /// Gets all file options.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<string> AllFileOptions
        {
            get
            {
                return new[] {
                    DoNothingOption.Action,
                    IgnoreOption.Action,
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
        protected override IEnumerable<string> GetValueChoicesHelper(int fileId)
        {
            try
            {
                if (InUseFiles.Contains(fileId))
                {
                    return new[] { GetInUseValue(fileId) };
                }
                else if (CurrentValues.TryGetValue(fileId, out string option) && option == CurrentOption.Action)
                {
                    return new[] { CurrentOption.Action };
                }
                else
                {
                    return AllFileOptions;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44806");
            }
        }

        /// <summary>
        /// Prompts regarding any situation that prevents performing the specified action. 
        /// </summary>
        /// <returns><see langword="true"/> if the actions can be successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        protected override bool PromptForActions()
        {
            try
            {
                if (!base.PromptForActions())
                {
                    return false;
                }

                if (DataEntryApplication.Dirty && OriginalFileIds.Except(CurrentFileIds).Any())
                {
                    UtilityMethods.ShowMessageBox("There are uncommitted changes in the previously shown documents.\r\n" +
                        "Changes would need to be discarded before previously shown documents could be closed.",
                        "Uncommitted changes", true);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44763");
            }
        }

        /// <summary>
        /// Prompts to save any unsaved changes in the document currently displayed for
        /// verification.
        /// </summary>
        /// <returns><see langword="true"/> if the user chose to either save or disregard the
        /// changes; <see langword="false"/> if the user chose to cancel the operation.</returns>
        protected override bool PromptToSaveCurrentFileChanges()
        {
            return true;
        }

        /// <summary>
        /// Gets a description of the changes the user has made thus far.
        /// </summary>
        /// <value>
        /// A description of the changes the user has made thus far.
        /// </value>
        protected override string ListOfChanges
        {
            get
            {
                StringBuilder message = new StringBuilder();

                if (OriginalFileIds.Intersect(CurrentFileIds).Count() != CurrentFileIds.Count)
                {
                    var newFileIDs = CurrentFileIds.Except(OriginalFileIds);

                    message.AppendLine(Invariant($"Show {newFileIDs.Count()} new files."));
                }

                int ignoredDocuments = CurrentValues.Values.Count(value => value == IgnoreOption.Action);
                if (ignoredDocuments > 0)
                {
                    message.AppendLine(string.Format(CultureInfo.CurrentCulture,
                        "Ignore {0} file(s).", ignoredDocuments));
                }

                return message.ToString().TrimEnd(null);
            }
        }

        /// <summary>
        /// Applies all uncommitted values specified via SetValue.
        /// </summary>
        /// <returns><see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        public override bool Apply()
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

                // https://extract.atlassian.net/browse/ISSUE-15690
                // For pagination, there is no ability to remove a file that is displayed via the
                // duplicate document button; so there is no need to delay/release checked out files.

                // If a new current file has been specified, it should be officially moved into
                // the queue (via fallback status) if it is not already and it should be
                // requested to be the next file displayed.
                foreach (int newId in CurrentFileIds.Except(OriginalFileIds))
                {
                    // https://extract.atlassian.net/browse/ISSUE-15690
                    // A new current file should not be "released" to its previous status.
                    CheckedOutFileIds.Remove(newId);

                    DataEntryApplication.FileRequestHandler.SetFallbackStatus(
                        newId, EActionStatus.kActionPending);

                    if (!DataEntryApplication.RequestFile(newId))
                    {
                        new ExtractException("ELI37565",
                            "Specified current file is not available for processing.").Display();
                    }
                }

                PerformActions();

                // Any remaining files ("Do nothing") that were checked-out for the purpose of being
                // locked from other users should be released back to their previous status.
                ReleaseCheckedOutFiles();

                ClearData();

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46645", "Failed to apply document actions.");
            }
            finally
            {
                // Allow the FPRecordManager queue to resume distribution of files.
                DataEntryApplication.FileRequestHandler.ResumeProcessingQueue();
            }
        }

        #endregion Overrides
    }
}
