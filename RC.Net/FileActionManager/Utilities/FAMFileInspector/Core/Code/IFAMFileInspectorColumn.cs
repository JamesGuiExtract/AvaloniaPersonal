using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Defines the possible column types for <see cref="IFAMFileInspectorColumn"/> instances.
    /// </summary>
    [ComVisible(true)]
    [Guid("0C41B8E4-6E73-462A-B98E-54EC92D57B1D")]
    [CLSCompliant(false)]
    public enum FFIColumnType
    {
        /// <summary>
        /// Cells in this column should be text box cells.
        /// </summary>
        Text,

        /// <summary>
        /// Cells in this column should be combo box cells.
        /// </summary>
        Combo
    }

    /// <summary>
    /// Defines a custom column to be displayed in the FFI file list table.
    /// </summary>
    [ComVisible(true)]
    [Guid("4DA24E55-B557-4880-9CE1-CEA60AD308EC")]
    [CLSCompliant(false)]
    public interface IFAMFileInspectorColumn
    {
        /// <summary>
        /// Gets or sets the file processing DB.
        /// </summary>
        /// <value>
        /// The file processing DB.
        /// </value>
        IFileProcessingDB FileProcessingDB
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the handle of the <see cref="FAMFileInspectorForm"/> in which this column is being used
        /// or <see cref="IntPtr.Zero"/> in the case the column is not currently initialized in an FFI.
        /// </summary>
        IntPtr FAMFileInspectorFormHandle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name for the column header. Will also appear in the context menu if
        /// <see cref="GetContextMenuChoices"/> returns a value.
        /// </summary>
        string HeaderText
        {
            get;
        }

        /// <summary>
        /// Gets the default width the column
        /// </summary>
        int DefaultWidth
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="FFIColumnType"/> defining what type of column is represented. 
        /// </summary>
        FFIColumnType FFIColumnType
        {
            get;
        }

        /// <summary>
        /// Gets whether this column is read only.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "ReadOnly")]
        bool ReadOnly
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether FFI menu main and context menu options should be limited
        /// to basic non-custom options. The main database menu and custom file handlers context
        /// menu options will not be shown.
        /// </summary>
        /// <value><see langword="true"/> to limit menu options to basic options only; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool BasicMenuOptionsOnly
        {
            get;
        }

        /// <summary>
        /// Gets the possible values to offer for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID for which the possible value choices are needed or -1
        /// for the complete set of possible values across all files.</param>
        /// <returns>A list of all pre-defined choices to be available for the user to select in
        /// this column. For <see cref="T:FFIColumnType.Combo"/>, at least one value is required for
        /// the column to be usable.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IVariantVector GetValueChoices(int fileId);

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
        /// <returns>The values that should be specifiable via the context menu for the currently
        /// selected row(s). Can be <see langword="null"/> if context menu options should not be
        /// available for this column.</returns>
        IVariantVector GetContextMenuChoices(HashSet<int> fileIds);

        /// <summary>
        /// Gets the value to display for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileID">The file ID for which the current value is needed.</param>
        /// <returns>The value for the specified file.</returns>
        string GetValue(int fileID);

        /// <summary>
        /// Sets the specified <see paramref="value"/> for the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID for which the value should be set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns><see langword="true"/> if the values was updated, <see langword="false"/> if
        /// the new value could not be applied.</returns>
        bool SetValue(int fileId, string value);

        /// <summary>
        /// Retrieves the set of file IDs for which the value needs to be refreshed. This method
        /// will be called after every time the the FFI file list is loaded or refreshed and after
        /// every time <see cref="SetValue"/> is called in order to check for updated values to be
        /// displayed. <see cref="GetValue"/> will subsequently be called for every returned file
        /// id.
        /// </summary>
        /// <returns>The file IDs for which values need to be refreshed in the FFI.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IEnumerable<int> GetValuesToRefresh();
    }
}
