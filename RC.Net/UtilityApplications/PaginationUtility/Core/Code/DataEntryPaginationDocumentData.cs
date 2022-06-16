using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.Utilities;
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
        /// If specified, the tooltip text that should be associated with the data validation error.
        /// </summary>
        string _dataErrorMessage;

        /// <summary>
        /// If not null, the tooltip text that should be associated with a document-level issue preventing
        /// the document from being commited.
        /// </summary>
        string _documentErrorMessage;

        /// <summary>
        /// <c>true</c> if the document type displayed in the panel is valid.
        /// </summary>
        bool _documentTypeIsValid;

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
        /// Represents data to be shared with other documents along with certain state information.
        /// </summary>
        SharedData _sharedData;

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
        /// <param name="sharedData">If specified, a copy of the shared data provided will be created;
        /// If null, a new/empty SharedData instance will be created.</param>
        public DataEntryPaginationDocumentData(IUnknownVector attributes, string sourceDocName, SharedData sharedData = null)
            : base(attributes, sourceDocName)
        {
            try
            {
                WorkingAttributes = attributes;
                _originalData = (IAttribute)((ICloneIdentifiableObject)DocumentDataAttribute).CloneIdentifiableObject();
                _originalData.ReportMemoryUsage();
                _sharedData = (sharedData == null)
                    ? new SharedData(DocumentId)
                    : new SharedData(sharedData);
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

                _originalData = (IAttribute)((ICloneIdentifiableObject)DocumentDataAttribute).CloneIdentifiableObject();
                _originalData.ReportMemoryUsage();

                _sharedData = new SharedData(DocumentId);
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

        /// <summary>
        /// Indicates whether the the DEP for a document has been opened to initialize the WorkAttributes with
        /// the results of auto-update queries in the DEP.
        /// </summary>
        public bool WorkingAttributeInitialized{ get; set; }

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
            ExtractException dataErrorException = null;
            bool dirty = false;

            try
            {
                ExtractException.Assert("ELI47128", "No status update pending",
                    PendingDocumentStatus != null);

                if (PendingDocumentStatus.DocumentTypeIsValid != _documentTypeIsValid)
                {
                    _documentTypeIsValid = PendingDocumentStatus.DocumentTypeIsValid;
                    dirty = true;
                }

                if (PendingDocumentStatus.DataError != _dataError)
                {
                    _dataError = PendingDocumentStatus.DataError;
                    dirty = true;
                }

                if (PendingDocumentStatus.DataWarning != _dataWarning)
                {
                    _dataWarning = PendingDocumentStatus.DataWarning;
                    dirty = true;
                }

                if (!_dataError && !_dataWarning)
                {
                    _dataErrorMessage = null;
                }

                // If the SharedData instance from the background thread has a higher revision number
                // (i.e., field updates were applied in the background while no such updates have
                // occurred in the forground), then apply the updated field values to the foreground.
                if (PendingDocumentStatus.SharedData.FieldRevisionNumber > _sharedData.FieldRevisionNumber)
                {
                    // If the data has not been loaded into a DEP (WorkingAttributes == false), don't 
                    // consider the field values to be changed even in the case that
                    // PendingDocumentStatus.SharedData has values not currently stored in _sharedData
                    // Otherwise, there is the potential that shared data from multiple documents that don't
                    // have any user-specified changes can enter into a cycle of updates based on each
                    // other's SharedData queries.
                    _sharedData.CopyFieldValues(PendingDocumentStatus.SharedData,
                        copyFieldChangedStatus: WorkingAttributeInitialized);
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

                    if (PendingDocumentStatus.DocumentType != DocumentType)
                    {
                        DocumentType = PendingDocumentStatus.DocumentType;
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
                        if (!PendingDocumentStatus.DocumentTypeIsValid)
                        {
                            dataErrorException = new ExtractException("ELI50179", "Invalid document type");
                        }
                        else
                        {
                            dataErrorException = ExtractException.FromStringizedByteStream("ELI45581", PendingDocumentStatus.StringizedError);
                        }
                    }
                    else
                    {
                        Orders = PendingDocumentStatus.Orders;
                        PromptForDuplicateOrders = PendingDocumentStatus.PromptForDuplicateOrders;
                        Encounters = PendingDocumentStatus.Encounters;
                        PromptForDuplicateEncounters = PendingDocumentStatus.PromptForDuplicateEncounters;
                    }

                    // If WorkingAttribute have not yet been initialized with the results of auto-update
                    // queries, copy this data back to the UI thread so it can be leveraged for OriginData
                    if (!DataError || !WorkingAttributeInitialized)
                    {
                        Attributes =
                            (IUnknownVector)_miscUtils.Value.GetObjectFromStringizedByteStream(PendingDocumentStatus.StringizedData);
                        // "Attributes" reflects data prepared for output while "WorkingAttributes" is the copy of attributes
                        // maintained in the UI thread and loaded into DEPs for editing. One benefit of maintaining a separate copy
                        // is to allow the undo system to work after closing/re-opening the DEP.
                        // However, WorkingAttributes should be updated with the results of queries if they have not yet been loaded
                        // into a DEP. WorkingAttributes should also be updated after committing a document to ensure the DEP
                        // for submitted documents accurately displays submitted data. (DEPs showing submitted documents won't be
                        // using auto-update queries that would otherwise mirror the updates applied to Attributes)
                        WorkingAttributes = Attributes;
                        WorkingAttributeInitialized = true;
                    }
                }

                return dataErrorException;
            }
            catch (Exception ex)
            {
                // If the document status update has an exception, assume that to be the primary
                // cause and only log the secondary exception here.
                if (PendingDocumentStatus.Exception != null)
                {
                    ex.ExtractLog("ELI48296");
                    return null;
                }
                else
                {
                    throw ex.AsExtract("ELI47125");
                }
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
        /// <c>true</c> if the data contains a validation error or the document type is not valid.
        /// </summary>
        public override bool DataError
        {
            get
            {
                return _dataError
                    || !_documentTypeIsValid
                    || !string.IsNullOrEmpty(_documentErrorMessage);
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
        /// Gets the tooltip text that should be associated with the data validation error
        /// or <c>null</c> to use the default error text.
        /// </summary>
        public override string DataErrorMessage
        {
            get
            {
                return _documentErrorMessage
                    ?? _dataErrorMessage
                    ?? (_documentTypeIsValid ? null : "Invalid document type");
            }
        }

        /// <summary>
        /// Gets any explanation for a document-level issue preventing the document from being commited.
        /// </summary>
        public override string DocumentErrorMessage
        {
            get => _documentErrorMessage;
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
        /// Gets data to be shared with other documents along with certain state information.
        /// </summary>
        public override SharedData SharedData => _sharedData;

        /// <summary>
        /// Reverts the data back to its original values.
        /// </summary>
        public override void Revert()
        {
            try
            {
                UndoState = null;
                DocumentDataAttribute = (IAttribute)((ICloneIdentifiableObject)_originalData).CloneIdentifiableObject();
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

                _originalData = (IAttribute)((ICloneIdentifiableObject)DocumentDataAttribute).CloneIdentifiableObject();
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

        /// <summary>
        /// Remove the attribute hierarchy providing a document access to the data of the document
        /// from which it originated.
        /// </summary>
        public override void RemoveOriginData()
        {
            try
            {
                base.RemoveOriginData();

                var originAttribute = WorkingAttributes.ToIEnumerable<IAttribute>()
                    .SingleOrDefault(attribute => attribute.Name == OriginDataName);

                if (originAttribute != null)
                {
                    WorkingAttributes.RemoveValue(originAttribute);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53306");
            }
        }

        /// <summary>
        /// Adds _SharedData attributes representing shared data from other documents for
        /// use in this document auto-update and validation queries.
        /// </summary>
        public override void AddSharedDataAttributes()
        {
            try
            {
                RemoveSharedDataAttributes();

                List<IAttribute> sharedDataAttributes = new List<IAttribute>();
                foreach (var sharedData in OtherDocumentsSharedData.Where(data => !data.IsDeleted))
                {
                    IAttribute sharedDataAttribute = new AttributeClass() { Name = SharedData.AttributeName };
                    sharedDataAttributes.Add(sharedDataAttribute);

                    foreach (var field in sharedData
                        .SelectMany(field => field.Values.Select(value => (field.Name, value))))
                    {
                        sharedDataAttribute.AddSubAttribute(field.Name, field.value);
                    }
                }

                var sharedDataAttributeVector = sharedDataAttributes.ToIUnknownVector();
                sharedDataAttributeVector.ReportMemoryUsage();
                WorkingAttributes.Append(sharedDataAttributeVector);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53461");
            }
        }

        /// <summary>
        /// Remove _SharedData attributes representing shared data from other documents
        /// </summary>
        public override void RemoveSharedDataAttributes()
        {
            try
            {
                base.RemoveSharedDataAttributes();

                WorkingAttributes.ToIEnumerable<IAttribute>()
                    .Where(attribute => attribute.Name == SharedData.AttributeName)
                    .ToList()
                    .ForEach(sharedDataAttribute =>
                    {
                        WorkingAttributes.RemoveValue(sharedDataAttribute);
                    });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53479");
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
                if (SharedDataConfigManager != null)
                {
                    SharedDataConfigManager.Dispose();
                    SharedDataConfigManager = null;
                }

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
        /// Used to manage updates and checks involving shared data without interfering with the state of
        /// the primary configuration manager (DataEntryPanelContainer._configManager)
        /// </summary>
        internal DataEntryConfigurationManager<Properties.Settings> SharedDataConfigManager { get; set; }

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
                if (!dataError && !_dataWarning)
                {
                    _dataErrorMessage = null;
                }

                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        /// Specifies whether the document type should be considered valid.
        /// </summary>
        /// <param name="isValid"><c>true</c> if the document type should be considered valid;
        /// <c>false</c> if it is not valid.</param>
        internal void SetDocumentTypeValidity(bool isValid)
        {
            if (_documentTypeIsValid != isValid)
            {
                _documentTypeIsValid = isValid;

                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        /// Sets the whether this data contains a validation warning.
        /// </summary>
        /// <param name="dataWarning"><c>true</c> if this data contains a validation warning; otherwise,
        /// <c>false</c>.</param>
        internal void SetDataWarning(bool dataWarning)
        {
            if (dataWarning != _dataWarning)
            {
                _dataWarning = dataWarning;
                if (!dataWarning && !_dataError)
                {
                    _dataErrorMessage = null;
                }

                OnDocumentDataStateChanged();
            }
        }

        /// <summary>
        /// Sets the tooltip text that should be associated with the document validation error.
        /// </summary>
        /// <param name="documentLevelError">true for and explanation of an issue committing the
        /// document as a whole; false for a message pertaining to data validation.</param>
        /// <param name="errorMessage">The error message.</param>
        internal void SetErrorMessage(bool documentLevelError, string errorMessage)
        {
            if (documentLevelError)
            {
                if (errorMessage != _documentErrorMessage)
                {
                    _documentErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
                        ? null
                        : errorMessage;

                    OnDocumentDataStateChanged();
                }
            }
            else if (errorMessage != _dataErrorMessage)
            {
                _dataErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
                    ? null
                    : errorMessage;

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
