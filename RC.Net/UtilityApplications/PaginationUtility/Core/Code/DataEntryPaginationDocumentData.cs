using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UCLID_AFCORELib;
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
        IAttribute _originalData;

        /// <summary>
        /// Indicates whether the data has been modified.
        /// </summary>
        bool _modified;

        /// <summary>
        /// Indicates whether the data should be indicated as modified even if SetModified is called
        /// to clear the modified status.
        /// </summary>
        bool _permanentlyModified;

        /// <summary>
        /// A description of the document.
        /// </summary>
        string _summary;

        /// <summary>
        /// Indicates whether the data currently contains a validation error.
        /// </summary>
        bool _dataError;

        /// <summary>
        /// Indicates whether the data currently contains a validation warning.
        /// </summary>
        bool _dataWarning;

        /// <summary>
        /// If specified, the tooltip text that should be associated with the document validation error.
        /// </summary>
        string _dataErrorMessage;

        /// <summary>
        /// An object representing the undo/redo operations that should be restored to the
        /// <see cref="UndoManager"/> when the data is loaded for editing.
        /// </summary>
        IDisposable _undoState;

        /// <summary>
        /// Indicates whether the data has been initialized for display.
        /// </summary>
        bool _initialized;

        /// <summary>
        /// Indicates whether this instance wants to override whether the document is returned to
        /// the server for reprocessing.
        /// </summary>
        bool? _sendForReprocessing = null;

        /// <summary>
        /// To read attributes to bytestreams.
        /// </summary>
        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtils());

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
                _originalData = (IAttribute)((ICopyableObject)DocumentDataAttribute).Clone();
                _originalData.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41357");
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DataEntryPaginationDocumentData"/>.
        /// </summary>
        /// <param name="documentDataAttribute">The <see cref="IAttribute"/> hierarchy (voa data) on which this
        /// instance is based including this top-level attribute which contains document data status info.</param>
        /// <param name="sourceDocName">The source document related to <see cref="_documentData"/>
        /// if there is a singular source document; otherwise <see langword="null"/>.</param>
        public DataEntryPaginationDocumentData(IAttribute documentDataAttribute, string sourceDocName)
            : base(documentDataAttribute, sourceDocName)
        {
            try
            {
                WorkingAttributes = Attributes;

                _originalData = (IAttribute)((ICopyableObject)DocumentDataAttribute).Clone();
                _originalData.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45952");
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

        /// <summary>
        /// A <see cref="DocumentStatus"/> update that has been retrieved, but not yet applied to this instance.
        /// </summary>
        public DocumentStatus PendingDocumentStatus { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets the value of <see cref="SendForReprocessing"/>.
        /// </summary>
        /// <param name="sendForReprocessing">The value to apply.</param>
        public void SetSendForReprocessing(bool? sendForReprocessing)
        {
            try
            {
                if (_sendForReprocessing != sendForReprocessing)
                {
                    _sendForReprocessing = sendForReprocessing;

                    OnDocumentDataStateChanged();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45611");
            }
        }

        /// <summary>
        /// Applies the <see cref="ApplyPendingStatusUpdate"/> to this instance.
        /// </summary>
        /// <param name="statusOnly"><c>true</c> if only the status metadata should be updated; 
        /// <c>false</c> to update the document data attributes.</param>
        /// <returns></returns>
        public ExtractException ApplyPendingStatusUpdate(bool statusOnly)
        {
            bool dirty = false;

            try
            {
                ExtractException.Assert("ELI47128", "No status update pending",
                    PendingDocumentStatus != null);

                if (PendingDocumentStatus.DataError != _dataError)
                {
                    _dataError = PendingDocumentStatus.DataError;
                    if (!PendingDocumentStatus.DataError)
                    {
                        _dataErrorMessage = null;
                    }
                    dirty = true;
                }

                if (PendingDocumentStatus.DataWarning != _dataWarning)
                {
                    _dataWarning = PendingDocumentStatus.DataWarning;
                    dirty = true;
                }

                if (statusOnly)
                {
                    if (PendingDocumentStatus.DataModified != _modified)
                    {
                        _modified = PendingDocumentStatus.DataModified;
                        dirty = true;
                    }

                    if (PendingDocumentStatus.Summary != _summary)
                    {
                        _summary = PendingDocumentStatus.Summary;
                        dirty = true;
                    }

                    if (PendingDocumentStatus.Reprocess != _sendForReprocessing)
                    {
                        _sendForReprocessing = PendingDocumentStatus.Reprocess;
                        dirty = true;
                    }

                    if (!_initialized)
                    {
                        _initialized = true;
                        dirty = true;
                    }
                }
                else
                {
                    // This section is concerned with gathering data preparing for output;
                    // No need to raise DocumentDataStateChanged for any changed properties here.
                    if (DataError)
                    {
                        return ExtractException.FromStringizedByteStream("ELI45581", PendingDocumentStatus.StringizedError);
                    }
                    else
                    {
                        Orders = PendingDocumentStatus.Orders;
                        PromptForDuplicateOrders = PendingDocumentStatus.PromptForDuplicateOrders;
                        Encounters = PendingDocumentStatus.Encounters;
                        PromptForDuplicateEncounters = PendingDocumentStatus.PromptForDuplicateEncounters;
                        Attributes =
                            (IUnknownVector)_miscUtils.Value.GetObjectFromStringizedByteStream(PendingDocumentStatus.StringizedData);
                        // Any data updates that happened as part of the save should be reflected in the DEP via WorkingAttributes.
                        // In general, will be restricted to attributes that hadn't be auto-updated via queries when the user opens
                        // a DEP. This is needed after applying a document to ensure the DEP that displays submitted data properly
                        // (It won't be using auto-update queries that would otherwise mirror the updates that happened in Attributes)
                        WorkingAttributes = Attributes;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47125");
            }
            finally
            {
                PendingDocumentStatus = null;

                if (dirty)
                {
                    OnDocumentDataStateChanged();
                }
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether the data has been initialized for display.
        /// </summary>
        public override bool Initialized
        {
            get
            {
                return _initialized;
            }
        }

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
                return _permanentlyModified || _modified;
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
        /// Gets whether the data currently contains a validation warning.
        /// </summary>
        public override bool DataWarning
        {
            get
            {
                return _dataWarning;
            }
        }

        /// <summary>
        /// Gets the tooltip text that should be associated with the document validation error
        /// or <c>null</c> to use the default error text.
        /// </summary>
        public override string DataErrorMessage
        {
            get
            {
                return _dataErrorMessage;
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
                DocumentDataAttribute = (IAttribute)((ICopyableObject)_originalData).Clone();
                DocumentDataAttribute.ReportMemoryUsage();
                WorkingAttributes = base.Attributes;
                _permanentlyModified = false;
                SetModified(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41358");
            }
        }

        /// <summary>
        /// Defines the current state of the <see cref="Attributes"/> hierarchy as the original
        /// state (used to determine the value of <see cref="Modified"/>).
        /// </summary>
        public override void SetOriginalForm()
        {
            try
            {
                base.SetOriginalForm();

                _originalData = (IAttribute)((ICopyableObject)DocumentDataAttribute).Clone();
                _originalData.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47281");
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
                return _sendForReprocessing;
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
        /// Sets whether the data should be indicated as modified even if <see cref="SetModified"/>
        /// is called to clear the modified status.
        /// </summary>
        internal void SetPermanentlyModified()
        {
            _permanentlyModified = true;
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
                if (!dataError)
                {
                    _dataErrorMessage = null;
                }

                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        /// Sets the tooltip text that should be associated with the document validation error.
        /// </summary>
        /// <param name="dataErrorMessage">The data error message.</param>
        internal void SetDataErrorMessage(string dataErrorMessage)
        {
            if (dataErrorMessage != _dataErrorMessage)
            {
                _dataErrorMessage = string.IsNullOrWhiteSpace(dataErrorMessage)
                    ? null
                    : dataErrorMessage;

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
        
        /// <summary>
        /// Marks this data as initialized for display.
        /// </summary>
        internal void SetInitialized()
        {
            if (!_initialized)
            {
                _initialized = true;

                OnDocumentDataStateChanged();
            }
        }

        #endregion Internal Members
    }
}
