using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extract.FileActionManager.Utilities;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry.DEP.UWTransplantCenter
{
    /// <summary>
    /// A <see cref="IFAMFileInspectorColumn"/> used in conjunction with a
    /// <see cref="DuplicateDocumentsFFIColumn"/> to display the action status of files before being
    /// checked out into the FFI being used to manage duplicate documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("16AA6888-AFE6-46FE-A5C2-635CC3913406")]
    [CLSCompliant(false)]
    public class PreviousStatusFFIColumn : IFAMFileInspectorColumn
    {
        #region Constants

        /// <summary>
        /// The name of the column in the FFI.
        /// </summary>
        static readonly string _HEADER_TEXT = "Status";

        /// <summary>
        /// The default width of the column in pixels.
        /// </summary>
        static readonly int _DEFAULT_WIDTH = 65;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="DuplicateDocumentsFFIColumn"/> instance this column is being used in
        /// conjunction with.
        /// </summary>
        DuplicateDocumentsFFIColumn _duplicateDocumentsColumn;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviousStatusFFIColumn"/> class.
        /// </summary>
        public PreviousStatusFFIColumn()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviousStatusFFIColumn"/> class.
        /// </summary>
        /// <param name="duplicateDocumentsColumn">The <see cref="DuplicateDocumentsFFIColumn"/>
        /// instance this column is being used in conjunction with.</param>
        internal PreviousStatusFFIColumn(DuplicateDocumentsFFIColumn duplicateDocumentsColumn)
        {
            try
            {
                _duplicateDocumentsColumn = duplicateDocumentsColumn;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37569");
            }
        }

        #endregion Constructors

        #region IFAMFileInspectorColumn Members

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
                return FFIColumnType.Text;
            }
        }

        /// <summary>
        /// Gets whether this column is read only.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return true;
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
                return false;
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
                return false;
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
            return null;
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
            return null;
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
                EActionStatus previousStatus = _duplicateDocumentsColumn.GetPreviousStatus(fileID);

                switch (previousStatus)
                {
                    case EActionStatus.kActionCompleted:    return "Complete";
                    case EActionStatus.kActionFailed:       return "Failed";
                    case EActionStatus.kActionPending:      return "Pending";
                    case EActionStatus.kActionProcessing:   return "Processing";
                    case EActionStatus.kActionSkipped:      return "Skipped";
                    case EActionStatus.kActionUnattempted:  return "Unattempted";
                }

                ExtractException.ThrowLogicException("ELI37570");
                return "";
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI37571", "Failed to get value.");
            }
        }

        /// <summary>
        /// Sets the specified <see paramref="value"/> for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID for which the value should be set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>
        ///   <see langword="true"/> if the values was updated, <see langword="false"/> if
        /// the new value could not be applied.
        /// </returns>
        public bool SetValue(int fileId, string value)
        {
            throw new NotImplementedException();
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
            yield break;
        }

        /// <summary>
        /// Applies all uncommitted values specified via SetValue. (Unused if
        /// <see cref="RequireOkCancel"/> is <see langword="false"/>.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Apply()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancels all uncommitted data changes specified via SetValue. (Unused if
        /// <see cref="RequireOkCancel"/> is <see langword="false"/>).
        /// </summary>
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        #endregion IFAMFileInspectorColumn Members
    }
}
