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
        /// Gets whether the user should be able to modify the values in this column.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "ReadOnly")]
        bool ReadOnly
        {
            get;
        }

        /// <summary>
        /// <see langword="true"/> if any values specified via <see cref="SetValue"/> are to be
        /// applied via explicit click of an “OK” button or reverted via “Cancel”.
        /// <see langword="false"/> if the column doesn't make any changes or the changes take
        /// effect instantaneously.
        /// The <see cref="FAMFileInspectorForm"/> will only display OK and Cancel buttons are if
        /// this property is <see langword="true"/> for at least one provided
        /// <see cref="IFAMFileInspectorColumn"/>.
        /// </summary>
        bool RequireOkCancel
        {
            get;
        }

        /// <summary>
        /// Gets if there is any data that has been modified via <see cref="SetValue"/> that needs
        /// to be applied. (Not used if <see cref="RequireOkCancel"/> is <see langword="false"/>.
        /// </summary>
        bool Dirty
        {
            get;
        }

        /// <summary>
        /// Gets the possible values to offer for a field.  For Text type, will specify values
        /// for an auto-complete list. Also provides options for a context menu. 
        /// </summary>
        /// <returns>A list of all pre-defined choices to be available for the user to select in
        /// this column. For <see cref="T:FFIColumnType.Combo"/>, at least one value is required for
        /// the column to be usable.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IVariantVector GetValueChoices();

        /// <summary>
        /// Specifies values that can be applied via context menu. The returned values will be
        /// presented as a sub-menu to a context menu option with <see cref="HeaderText"/> as the
        /// option name. (This method is not used is <see cref="ReadOnly"/> is
        /// <see langword="true"/>.
        /// </summary>
        /// <param name="multiple"><see langword="true"/> if the returned choices are valid for more
        /// than one file at a time, <see langword="false"/> if the returned choices are valid for a
        /// singly selected file.
        /// <para><b>Note</b></para>
        /// The options returned when <see paramref="multiple"/> is true should be a subset of the
        /// options returned when <see paramref="multiple"/> is false.
        /// </param>
        /// <returns>The values that should be specifiable via the context menu for the currently
        /// selected row(s). Can be <see langword="null"/> if context menu options should not be
        /// available for this column.</returns>
        IVariantVector GetContextMenuChoices(bool multiple);

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
        void SetValue(int fileId, string value);

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

        /// <summary>
        /// Applies all uncommitted values specified via SetValue. (Unused if
        /// <see cref="RequireOkCancel"/> is <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        bool Apply();
    }
}
