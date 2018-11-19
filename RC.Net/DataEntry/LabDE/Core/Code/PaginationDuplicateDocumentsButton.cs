namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A <see cref="DuplicateDocumentsButton"/> extension that is compatible for use in a
    /// DataEntryDocumentDataPanel for the Paginate Files task.
    /// </summary>
    public class PaginationDuplicateDocumentsButton : DuplicateDocumentsButton
    {
        #region Fields

        /// <summary>
        /// A custom column for the FFI that allows for various actions to be performed on the
        /// displayed documents.
        /// </summary>
        PaginationDuplicateDocumentsFFIColumn _ffiActionColumn;

        /// <summary>
        /// A custom column that displays the status of the files prior to being loaded into the
        /// FFI.
        /// </summary>
        PaginationPreviousStatusFFIColumn _ffiStatusColumn;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDuplicateDocumentsButton"/> class.
        /// </summary>
        public PaginationDuplicateDocumentsButton()
            : base()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// An <see cref="IFAMFileInspectorColumn"/> for the FFI that allows for various actions to
        /// be performed on the displayed documents.
        /// </summary>
        public override DuplicateDocumentsFFIColumn ActionColumn
        {
            get
            {
                if (_ffiActionColumn == null)
                {
                    _ffiActionColumn = new PaginationDuplicateDocumentsFFIColumn();
                }
                return _ffiActionColumn;
            }
        }

        /// <summary>
        /// An <see cref="IFAMFileInspectorColumn"/> that displays the status of the files prior to
        /// being loaded into the FFI.
        /// </summary>
        public override PreviousStatusFFIColumn StatusColumn
        {
            get
            {
                if (_ffiStatusColumn == null)
                {
                    _ffiStatusColumn = new PaginationPreviousStatusFFIColumn(
                        (PaginationDuplicateDocumentsFFIColumn)ActionColumn);
                }
                return _ffiStatusColumn;
            }
        }

        #endregion Properties
    }
}
