using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using UCLID_COMUTILSLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationDocumentData"/> derivation that tracks data associated with a
    /// <see cref="DataEntryDocumentDataPanel"/>.
    /// </summary>
    public class DataEntryPaginationDocumentData : PaginationDocumentData, IDisposable
    {
        #region Fields

        /// <summary>
        /// A copy of the original data loaded (to implement revert functionality)
        /// </summary>
        IUnknownVector _originalData;

        /// <summary>
        /// Indicates whether the data has been modified.
        /// </summary>
        bool _modified;

        /// <summary>
        /// A description of the document.
        /// </summary>
        string _summary;

        /// <summary>
        /// Indicates whether the data currently contains a validation error.
        /// </summary>
        bool _dataError;

        /// <summary>
        /// An object representing the undo/redo operations that should be restored to the
        /// <see cref="UndoManager"/> when the data is loaded for editing.
        /// </summary>
        IDisposable _undoState;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DataEntryPaginationDocumentData"/>.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// this data is based on.
        /// </param>
        /// <param name="sourceDocName">The source document related to <see cref="_documentData"/>
        /// if there is a singular source document; otherwise <see langword="null"/>.</param>
        public DataEntryPaginationDocumentData(IUnknownVector attributes, string sourceDocName)
            : base(attributes, sourceDocName)
        {
            try
            {
                WorkingAttributes = attributes;
                _originalData = (IUnknownVector)((ICopyableObject)attributes).Clone();
                _originalData.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41357");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets an object representing the undo/redo operations that should be restored to
        /// the <see cref="UndoManager"/> when the data is loaded for editing.
        /// <para><b>Note</b></para>
        /// When setting, any previous UndoState will be disposed of.
        /// </summary>
        public IDisposable UndoState
        {
            get
            {
                return _undoState;
            }

            set
            {
                try
                {
                    if (value != _undoState)
                    {
                        if (_undoState != null)
                        {
                            _undoState.Dispose();
                        }

                        _undoState = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41491");
                }
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// A description of the document.
        /// </summary>
        public override string Summary
        {
            get
            {
                return _summary;
            }
        }

        /// <summary>
        /// Gets a value indicating whether editing of this data is allowed.
        /// </summary>
        /// <value><see langword="true"/> if data editing is allowed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public override bool AllowDataEdit
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether this instances data has been modified.
        /// </summary>
        public override bool Modified
        {
            get
            {
                return _modified;
            }
        }

        /// <summary>
        /// Gets whether the data currently contains a validation error.
        /// </summary>
        public override bool DataError
        {
            get
            {
                return _dataError;
            }
        }

        /// <summary>
        /// Unused base class implementation.
        /// </summary>
        protected override Dictionary<string, PaginationDataField> Fields
        {
            get
            {
                return new Dictionary<string, PaginationDataField>();
            }
        }

        /// <summary>
        /// Reverts the data back to its original values.
        /// </summary>
        public override void Revert()
        {
            try
            {
                UndoState = null;
                base.Attributes = (IUnknownVector)_originalData.Clone();
                base.Attributes.ReportMemoryUsage();
                WorkingAttributes = base.Attributes;
                SetModified(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41358");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance wants to override whether the document
        /// is returned to the server for reprocessing.
        /// </summary>
        /// <value><see langword="null"/> to if the decision should not be overridden, otherwise
        /// a boolean value indicating what the override should be.</value>
        public override bool? SendForReprocessing

        {
            get
            {
                return base.SendForReprocessing;
            }
        }

        #endregion Overrides

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryPaginationDocumentData"/> instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI41490", ex);
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryPaginationDocumentData"/> instance.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_undoState != null)
                {
                    _undoState.Dispose();
                    _undoState = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Internal Members

        /// <summary>
        /// The attributes actually loaded into the UI which are maintained for the undo system to
        /// work after switching documents (as undo mementos rely on being pointed back to the same
        /// attributes). The <see cref="Attributes"/> property will maintain attributes ready for
        /// output (placeholder and non-persisting attributes cleaned up).
        /// </summary>
        internal IUnknownVector WorkingAttributes
        {
            get;
            private set;
        }

        /// <summary>
        /// Sets the whether this data has been modified.
        /// </summary>
        /// <param name="modified"><c>true</c> if this data had been modified; otherwise,
        /// <c>false</c>.</param>
        internal void SetModified(bool modified)
        {
            if (modified != _modified)
            {
                _modified = modified;

                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        /// Sets the whether this data contains a validation error.
        /// </summary>
        /// <param name="dataError"><c>true</c> if this data contains a validation error; otherwise,
        /// <c>false</c>.</param>
        internal void SetDataError(bool dataError)
        {
            if (dataError != _dataError)
            {
                _dataError = dataError;

                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        /// Sets the summary.
        /// </summary>
        /// <param name="summary">The summary.</param>
        internal void SetSummary(string summary)
        {
            if (summary != _summary)
            {
                _summary = summary;

                OnDocumentDataStateChanged();
            }
        }

        #endregion Internal Members
    }
}
