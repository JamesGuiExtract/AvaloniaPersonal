using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.DataEntry.LabDE;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// An <see cref="IPaginationDocumentDataPanel"/> implementation that allow for full DEP support
    /// in the context of a pagination data editing panel.
    /// </summary>
    public class DataEntryDocumentDataPanel : DataEntryControlHost
    {
        #region Fields

        /// <summary>
        /// The <see cref="ImageViewer"/> to be used.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// The currently loaded <see cref="DataEntryPaginationDocumentData"/>.
        /// </summary>
        DataEntryPaginationDocumentData _documentData;

        /// <summary>
        /// The query used to generate the <see cref="PaginationDocumentData.Summary"/>.
        /// </summary>
        DataEntryQuery _summaryDataEntryQuery;

        /// <summary>
        /// Indicates whether the PageLayoutControl's PrimarySelection corresponds with the output
        /// document for which this DEP is editing data.
        /// </summary>
        bool _primaryPageIsForActiveDocument;

        /// <summary>
        /// Indicates whether this panel should be presented as having input focus.
        /// </summary>
        bool _indicateFocus = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryDocumentDataPanel"/> class.
        /// </summary>
        public DataEntryDocumentDataPanel()
        {
            try
            {
                AttributeStatusInfo.UndoManager.UndoAvailabilityChanged += HandleUndoManager_UndoAvailabilityChanged;
                AttributeStatusInfo.UndoManager.RedoAvailabilityChanged += HandleUndoManager_RedoAvailabilityChanged;

                ControlRegistered += HandleDataEntryDocumentDataPanel_ControlRegistered;
                ControlUnregistered += HandelDataEntryDocumentDataPanel_ControlUnregistered;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41423");
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
        /// Gets or the data entry query text used to generate a summary for the document.
        /// </summary>
        public virtual string SummaryQuery
        {
            get;
            protected set;
        }

        /// <summary>
        /// Function that when set is to be used so that the panel can specify whether a document is
        /// to be sent for rules reprocessing.
        /// </summary>
        public virtual bool? SendForReprocessingFunc(DataEntryPaginationDocumentData documentData)
        {
            return null;
        }

        /// <summary>
        /// Gets the data entry query text that should be used to identify any order numbers in the
        /// file to be recorded in the LabDEOrderFile table.
        /// </summary>
        public virtual string OrderNumberQuery
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the data entry query text that should be used to identify the date for each order.
        /// Any attribute queries should be relative to an order number attribute.
        /// </summary>
        public virtual string OrderDateQuery
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets whether to prompt about order numbers for which a document has already been filed.
        /// </summary>
        public virtual bool PromptForDuplicateOrders
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the data entry query text that should be used to identify any encounter numbers in the
        /// file to be recorded in the LabDEOrderFile table.
        /// </summary>
        public virtual string EncounterNumberQuery
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the data entry query text that should be used to identify the date for each encounter.
        /// Any attribute queries should be relative to an encoutner number attribute.
        /// </summary>
        public virtual string EncounterDateQuery
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets whether to prompt about encounter numbers for which a document has already been filed.
        /// </summary>
        public virtual bool PromptForDuplicateEncounters
        {
            get;
            protected set;
        }

        #endregion Properties

        #region IPaginationDocumentDataPanel

        /// <summary>
        /// Raised to indicate the panel is requesting a specific image page to be loaded.
        /// </summary>
        public event EventHandler<PageLoadRequestEventArgs> PageLoadRequest;

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Undo"/> method to
        /// revert has changed.
        /// </summary>
        public event EventHandler<EventArgs> UndoAvailabilityChanged;

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Redo"/> method to
        /// redo has changed.
        /// </summary>
        public event EventHandler<EventArgs> RedoAvailabilityChanged;

        /// <summary>
        /// The <see cref="UserControl"/> to be displayed for viewing/editing of document data.
        /// </summary>
        public UserControl PanelControl
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an undo operation is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if an undo operation is available; otherwise, <c>false</c>.
        /// </value>
        public static bool UndoOperationAvailable
        {
            get
            {
                return AttributeStatusInfo.UndoManager.UndoOperationAvailable;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataEntryPaginationDocumentData" /> instance based on the provided
        /// <see paramref="attributes" />.
        /// </summary>
        /// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData" />
        /// instance is needed.</param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <returns>
        /// The <see cref="DataEntryPaginationDocumentData"/> instance.
        /// </returns>
        public virtual DataEntryPaginationDocumentData GetDocumentData(IUnknownVector attributes, string sourceDocName)
        {
            try
            {
                return new DataEntryPaginationDocumentData(attributes, sourceDocName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44681");
            }
        }

        /// <summary>
        /// Gets a <see cref="DataEntryPaginationDocumentData" /> instance based on the provided
        /// <see paramref="attributes" />.
        /// </summary>
        /// <param name="documentDataAttribute">The <see cref="IAttribute"/> hierarchy (voa data) on which this
        /// instance is based including this top-level attribute which contains document data status info.</param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <returns>
        /// The <see cref="DataEntryPaginationDocumentData"/> instance.
        /// </returns>
        public virtual DataEntryPaginationDocumentData GetDocumentData(IAttribute documentDataAttribute, string sourceDocName)
        {
            try
            {
                return new DataEntryPaginationDocumentData(documentDataAttribute, sourceDocName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45985");
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="data" />.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <param name="forDisplay"><c>true</c> if the loaded data is to be displayed; <c>false</c>
        /// if the data is being loaded only for data manipulation or validation.</param>
        public virtual void LoadData(PaginationDocumentData data, bool forDisplay)
        {
            try
            {
                base.ImageViewer = _imageViewer;

                // Prevent cached query values from overriding differing data in the document being loaded.
                _summaryDataEntryQuery = null;

                _documentData = (DataEntryPaginationDocumentData)data;

                LoadData(_documentData.WorkingAttributes, _documentData.SourceDocName, forDisplay);

                if (_imageViewer != null && _imageViewer.Visible)
                {
                    _documentData.SetSummary(SummaryDataEntryQuery?.Evaluate().ToString());
                    _documentData.SetSendForReprocessing(SendForReprocessingFunc(_documentData));
                    _documentData.SetModified(UndoOperationAvailable);
                    if (!Config.Settings.PerformanceTesting)
                    {
                        _documentData.SetDataError(DataValidity == DataValidity.Invalid);
                    }

                    _documentData.SetInitialized();
                }

                if (InPaginationPanel)
                {
                    UpdateSwipingState();
                }

                if (_documentData.UndoState != null)
                {
                    AttributeStatusInfo.UndoManager.RestoreState(_documentData.UndoState);
                    OnUndoAvailabilityChanged();
                    OnRedoAvailabilityChanged();
                }

                if (_imageViewer != null)
                {
                    _imageViewer.ImageFileChanged += HandleImageViewer_ImageFileChanged;
                    _imageViewer.ImageFileClosing += HandleImageViewer_ImageFileClosing;
                }

                Active = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41348");
            }
        }

        /// <summary>
        /// Applies any data to the specified <see paramref="data"/>.
        /// <para><b>Note</b></para>
        /// In addition to returning <see langword="false"/>, it is the implementor's responsibility
        /// to notify the user of any problems with the data that needs to be corrected before it
        /// can be saved.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="validateData"><see langword="true"/> if the <see paramref="data"/> should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns><see langword="true"/> if the data was saved correctly or
        /// <see langword="false"/> if corrections are needed before it can be saved.</returns>
        public virtual bool SaveData(PaginationDocumentData data, bool validateData)
        {
            try
            {
                if (validateData && !DataCanBeSaved())
                {
                    return false;
                }

                // GetData contains attributes that have been prepared for output.
                var attributes = GetData(validateData);
                if (attributes == null)
                {
                    // DataCanBeSaved should have caught any cases where data cannot be saved, but
                    // it is possible for GetData to return null if for some reason there is a
                    // reason data cannot be saved that DataCanBeSaved did not detect.
                    return false;
                }
                attributes.ReportMemoryUsage();

                var dataEntryData = (DataEntryPaginationDocumentData)data;
                if (!Config.Settings.PerformanceTesting)
                {
                    dataEntryData.SetDataError(DataValidity == DataValidity.Invalid);
                }
                dataEntryData.UndoState = AttributeStatusInfo.UndoManager.GetState();

                data.Attributes = attributes;

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41349");
            }
        }

        /// <summary>
        /// Clears the state of all data associated with the previously loaded document.
        /// </summary>
        public new void ClearData()
        {
            try
            {
                Active = false;

                _documentData = null;

                base.ClearData();

                base.ImageViewer = null;

                if (_imageViewer != null)
                {
                    _imageViewer.ImageFileChanged -= HandleImageViewer_ImageFileChanged;
                    _imageViewer.ImageFileClosing -= HandleImageViewer_ImageFileClosing;

                    _imageViewer.Invalidate();

                    UpdateSwipingState();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41488");
            }
        }

        #endregion IPaginationDocumentDataPanel

        #region Internal Members

        /// <summary>
        /// Sets the <see cref="ImageViewer"/> to use.
        /// </summary>
        /// <param name="_imageViewer">The <see cref="ImageViewer"/> to use.</param>
        internal void SetImageViewer(ImageViewer imageViewer)
        {
            try
            {
                _imageViewer = imageViewer;

                // https://extract.atlassian.net/browse/ISSUE-14328
                // If the PageLayoutControl's PrimarySelection corresponds with the output document
                // for which data is being edited, share the image viewer with the DEP. Otherwise,
                // the DEP should be allowed access because its spatial data will not correspond with
                // the displayed document.
                if (PrimarySelectionIsForActiveDocument)
                {
                    base.ImageViewer = imageViewer;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41679");
            }
        }

        /// <summary>
        /// Gets the <see cref="DataEntryQuery"/> used to generate the <see cref="PaginationDocumentData.Summary"/>.
        /// </summary>
        /// <value>
        /// The <see cref="DataEntryQuery"/> used to generate the <see cref="PaginationDocumentData.Summary"/>.
        /// </value>
        internal DataEntryQuery SummaryDataEntryQuery
        {
            get
            {
                if (_summaryDataEntryQuery == null && !string.IsNullOrWhiteSpace(SummaryQuery))
                {
                    _summaryDataEntryQuery = DataEntryQuery.Create(SummaryQuery, null, DatabaseConnections);
                }

                return _summaryDataEntryQuery;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the PageLayoutControl's PrimarySelection
        /// corresponds with the output document for which this DEP is editing data.
        /// </summary>
        /// <value>
        /// <c>true</c> if he PageLayoutControl's PrimarySelection corresponds with the output
        /// document for which this DEP is editing data; otherwise, <c>false</c>.
        /// </value>
        internal bool PrimarySelectionIsForActiveDocument
        {
            get
            {
                return _primaryPageIsForActiveDocument && _imageViewer != null;
            }

            set
            {
                if (value != _primaryPageIsForActiveDocument)
                {
                    if (value && _imageViewer != null)
                    {
                        base.ImageViewer = _imageViewer;
                    }
                    else
                    {
                        base.ImageViewer = null;
                    }

                    _primaryPageIsForActiveDocument = value;
                }
            }
        }

        #endregion Internal Members

        #region Overrides

        /// <summary>
        /// Indicates whether all data must be viewed before saving and, if not, whether a prompt
        /// will be displayed before allowing unviewed data to be saved.
        /// </summary>
        /// <value><see langword="UnviewedDataSaveMode.Allow"/> to allow unviewed data to be 
        /// saved without prompting, <see langword="UnviewedDataSaveMode.Prompt"/> to allow unviewed
        /// data to be saved, but only after prompting (once for all unviewed fields) or 
        /// <see langword="UnviewedDataSaveMode.Disallow"/> to require that all data be viewed
        /// before saving.</value>
        /// <returns><see langword="UnviewedDataSaveMode.Allow"/> if unviewed data can be
        /// saved without prompting, <see langword="UnviewedDataSaveMode.Prompt"/> if unviewed data
        /// can be saved, but only after prompting (once for all unviewed fields) or 
        /// <see langword="UnviewedDataSaveMode.Disallow"/> if all data must be viewed before
        /// saving.</returns>
        public override UnviewedDataSaveMode UnviewedDataSaveMode
        {
            get
            {
                // Allow is currently the only supported mode in the pagination panel
                return InPaginationPanel
                    ? UnviewedDataSaveMode.Allow
                    : base.UnviewedDataSaveMode;
            }

            set
            {
                base.UnviewedDataSaveMode = value;
            }
        }

        /// <summary>
        /// Indicates whether all data must conform to validation rules before saving and, if not,
        /// whether a prompt will be displayed before allowing invalid data to be saved.
        /// </summary>
        /// <value>
        /// <see langword="InvalidDataSaveMode.Allow" /> to allow invalid data to be saved
        /// without prompting, <see langword="InvalidDataSaveMode.Prompt" /> to allow invalid data
        /// to be saved, but only after prompting (once for each invalid field) or
        /// <see langword="InvalidDataSaveMode.Disallow" /> to require that all data meet validation
        /// requirements before saving.
        /// </value>
        public override InvalidDataSaveMode InvalidDataSaveMode
        {
            get
            {
                if (InPaginationPanel)
                {
                    // https://extract.atlassian.net/browse/ISSUE-14216
                    // AllowWithWarnings is currently the only supported mode in the pagination panel
                    return Config.Settings.PerformanceTesting
                        ? InvalidDataSaveMode.Allow
                        : InvalidDataSaveMode.AllowWithWarnings;
                    
                }
                else
                {
                    return base.InvalidDataSaveMode;
                }
            }

            set
            {
                base.InvalidDataSaveMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this panel should be presented as having input focus.
        /// </summary>
        /// <value><c>true</c> if this panel should be presented as having input focus;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool IndicateFocus
        {
            get
            {
                return _indicateFocus;
            }

            set
            {
                try
                {
                    if (value != _indicateFocus)
                    {
                        var activeControlColor = value
                            ? ActiveSelectionColor
                            : Color.LightGray;

                        // DataEntryTable cells will not update cell style properly while in edit
                        // mode. To ensure active highlight color is properly updated, set control
                        // to inactive before indicating active in the new color.
                        ActiveDataControl?.IndicateActive(false, activeControlColor);
                        ActiveDataControl?.IndicateActive(true, activeControlColor);

                        // The image page relating to the currently active field will be displayed
                        // if this focus change is in conjunction with another control gaining focus.
                        // However, if no focus change is occurring, we need to call for the related
                        // page to be displayed here.
                        if (ImageViewer != null)
                        {
                            if (FocusingControl == null || FocusingControl == ActiveDataControl)
                            {
                                LoadPageInImageViewer(allowSourceDocumentSwitch: true);
                            }
                        }

                        _indicateFocus = value;

                        OnSwipingStateChanged(new SwipingStateChangedEventArgs(SwipingEnabled));
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI44698");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether swiping is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if swiping is enabled; otherwise, <c>false</c>.
        /// </value>
        public override bool SwipingEnabled => 
            // Do not allow swiping when the DEP is not indicating focus.
            IndicateFocus && base.SwipingEnabled;

        /// <summary>
        /// Gets or sets whether keyboard input should be disabled.
        /// </summary>
        public override bool DisableKeyboardInput
        {
            // Don't allow keyboard input when the DEP is not indicating focus.
            get => !IndicateFocus || base.DisableKeyboardInput;

            set => base.DisableKeyboardInput = value;
        }

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Reset the colors to prevent the DEP from incorporating the special colors
                // applied to this separator window.
                ForeColor = DefaultForeColor;
                BackColor = DefaultBackColor;

                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41365");
            }
        }

        /// <summary>
        /// Navigates to the specified page, settings _performingProgrammaticZoom in the process to
        /// avoid handling scroll and zoom events that occur as a result.
        /// </summary>
        /// <param name="pageNumber">The page to be displayed</param>
        protected override void SetImageViewerPageNumber(int pageNumber)
        {
            try
            {
                // Special logic applies only if the panel is being used in the pagination
                // context.
                if (InPaginationPanel)
                {
                    // https://extract.atlassian.net/browse/ISSUE-14208
                    // https://extract.atlassian.net/browse/ISSUE-14328
                    // Don't switch to a page that is not part of this document. There are many
                    // issues with doing so including unexpected page rotation behavior.
                    LoadPageInImageViewer(allowSourceDocumentSwitch: false);
                }
                else
                {
                    base.SetImageViewerPageNumber(pageNumber);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41351");
            }
        }

        /// <summary>
        /// Raises the <see cref="SwipingStateChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="SwipingStateChangedEventArgs"/> that contains the event
        /// data.</param>
        protected override void OnSwipingStateChanged(SwipingStateChangedEventArgs e)
        {
            try
            {
                base.OnSwipingStateChanged(e);

                // Special logic applies only if the panel is not being used in the pagination
                // context.
                if (InPaginationPanel)
                {
                    UpdateSwipingState();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41352");
            }
        }

        /// <summary>
        /// Indicates document data has changed.
        /// </summary>
        protected override void OnDataChanged()
        {
            try
            {
                base.OnDataChanged();

                if (_documentData != null)
                {
                    _documentData.SetSummary(
                        SummaryDataEntryQuery?.Evaluate().ToString());
                    _documentData.SetSendForReprocessing(SendForReprocessingFunc(_documentData));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41466");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Extract.DataEntry.DataEntryControlHost.DataValidityChanged" /> event.
        /// </summary>
        protected override void OnDataValidityChanged()
        {
            try
            {
                base.OnDataValidityChanged();

                if (_documentData != null && !Config.Settings.PerformanceTesting)
                {
                    _documentData.SetDataError(DataValidity == DataValidity.Invalid);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41418");
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.Enter"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs "/> that contains the event data.</param>
        protected override void OnEnter(EventArgs e)
        {
            try
            {
                // Special logic applies only if the panel is being used in the pagination context.
                if (InPaginationPanel)
                {
                    // If OnEnter, the primary page selection is not for this document, it needs to
                    // be returned to the document. Otherwise, the DEP will not have access to the
                    // ImageViewer and exceptions will occur.
                    if (!PrimarySelectionIsForActiveDocument)
                    {
                        LoadPageInImageViewer(allowSourceDocumentSwitch: true);
                    }
                }

                base.OnEnter(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41353");
            }
        }

        /// <summary>
        /// Gets a value indicating whether keyboard input directed at the <see cref="ImageViewer"/>
        /// should be processed by this panel.
        /// </summary>
        /// <value><c>true</c> if keyboard input directed at the <see cref="ImageViewer"/>
        /// should be processed by this panel; otherwise, <c>false</c>.
        protected override bool ProcessImageViewerKeyboardInput
        {
            get
            {
                return IndicateFocus ? base.ProcessImageViewerKeyboardInput : false;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="P:Extract.DataEntry.DataEntryControlHost.ImageViewer" /> with which to display the document corresponding to data
        /// contained in the <see cref="T:Extract.DataEntry.DataEntryControlHost" />'s data controls.
        /// </summary>
        /// <value>
        /// Sets the <see cref="P:Extract.DataEntry.DataEntryControlHost.ImageViewer" /> used to display the open document. <see langword="null" />
        /// to disconnect the <see cref="T:Extract.DataEntry.DataEntryControlHost" /> from the image viewer.
        /// </value>
        /// <seealso cref="T:Extract.Imaging.Forms.IImageViewerControl" />
        public override ImageViewer ImageViewer
        {
            get
            {
                try
                {
                    if (base.ImageViewer == null && _imageViewer != null &&
                        PrimarySelectionIsForActiveDocument)
                    {
                        base.ImageViewer = _imageViewer;
                    }

                    return base.ImageViewer;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41640");
                }
            }

            set
            {
                base.ImageViewer = value;
            }
        }

        /// <summary>
        /// Renders the <see cref="T:Extract.Imaging.Forms.CompositeHighlightLayerObject" />s associated with the
        /// <see cref="T:Extract.DataEntry.IDataEntryControl" />s.
        /// </summary>
        /// <param name="ensureActiveAttributeVisible">If <see langword="true" />, the portion of
        /// the document currently in view will be adjusted to ensure all active attribute(s) and
        /// their associated tooltip is visible.  If <see langword="false" /> the view will be
        /// unchanged even if the attribute and/or tooltip is not currently in the view.</param>
        protected override void DrawHighlights(bool ensureActiveAttributeVisible)
        {
            try
            {
                if (InPaginationPanel)
                {
                    // If ensureActiveAttributeVisible is true, we need to be sure the correct document
                    // is currently loaded in the image viewer.
                    if (ensureActiveAttributeVisible)
                    {
                        LoadPageInImageViewer(allowSourceDocumentSwitch: true);
                    }
                }

                base.DrawHighlights(ensureActiveAttributeVisible);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41680");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> if managed resources should be disposed;
        /// otherwise, <see langword="false" />.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // Unless unregistered, these events will hold referenced to this panel and prevent
                // it from being finalized.
                AttributeStatusInfo.UndoManager.UndoAvailabilityChanged -= HandleUndoManager_UndoAvailabilityChanged;
                AttributeStatusInfo.UndoManager.RedoAvailabilityChanged -= HandleUndoManager_RedoAvailabilityChanged;

                if (_summaryDataEntryQuery != null)
                {
                    _summaryDataEntryQuery.Dispose();
                    _summaryDataEntryQuery = null;
                }

                try
                {
                    // Reduce memory leaks
                    // https://extract.atlassian.net/browse/ISSUE-14288
                    ClearData();
                }
                catch { }
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The <see cref="ImageFileChangedEventArgs"/> that contains the event data.</param>
        void HandleImageViewer_ImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-14328
                // If the PageLayoutControl's PrimarySelection corresponds with the output document
                // for which data is being edited, share the image viewer with the DEP. Otherwise,
                // the DEP should be allowed access because its spatial data will not correspond with
                // the displayed document.
                if (_imageViewer.IsImageAvailable && Visible && PrimarySelectionIsForActiveDocument)
                {
                    base.ImageViewer = _imageViewer;

                    UpdateSwipingState();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41354");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileClosing"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The <see cref="ImageFileClosingEventArgs"/> that contains the event data.</param>
        void HandleImageViewer_ImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                base.ImageViewer = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41355");
            }
        }

        /// <summary>
        /// Handles the <see cref="UndoManager.UndoAvailabilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleUndoManager_UndoAvailabilityChanged(object sender, EventArgs e)
        {
            try
            {
                if (_documentData != null)
                {
                    _documentData.SetModified(UndoOperationAvailable);
                }

                OnUndoAvailabilityChanged();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41424");
            }
        }

        /// <summary>
        /// Handles the <see cref="UndoManager.RedoAvailabilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleUndoManager_RedoAvailabilityChanged(object sender, EventArgs e)
        {
            try
            {
                OnRedoAvailabilityChanged();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41425");
            }
        }

        /// <summary>
        /// Handles the <see cref="ControlRegistered"/> event of this panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataEntryControlEventArgs"/> instance containing the event data.</param>
        void HandleDataEntryDocumentDataPanel_ControlRegistered(object sender, DataEntryControlEventArgs e)
        {
            try
            {
                if (e.DataEntryControl is PaginationDuplicateDocumentsButton dupDocButton)
                {
                    dupDocButton.ActionColumn.DuplicateDocumentsApplied += HandleActionColumn_DuplicateDocumentsApplied;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46499");
            }
        }

        /// <summary>
        /// Handles the <see cref="ControlUnregistered"/> event of this panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataEntryControlEventArgs"/> instance containing the event data.</param>
        void HandelDataEntryDocumentDataPanel_ControlUnregistered(object sender, DataEntryControlEventArgs e)
        {
            try
            {
                if (e.DataEntryControl is PaginationDuplicateDocumentsButton dupDocButton)
                {
                    dupDocButton.ActionColumn.DuplicateDocumentsApplied -= HandleActionColumn_DuplicateDocumentsApplied;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46500");
            }
        }

        /// <summary>
        /// Handles the DuplicateDocumentsApplied event of any <see cref="PaginationDuplicateDocumentsButton"/>
        /// in the panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DuplicateDocumentsAppliedEventArgs"/> instance containing the event data.</param>
        void HandleActionColumn_DuplicateDocumentsApplied(object sender, DuplicateDocumentsAppliedEventArgs e)
        {
            try
            {
                DuplicateDocumentsApplied?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46496");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets whether this DEP is currently displayed in <see cref="PaginationPanel"/>.
        /// <returns><see langword="true"/> if this DEP is currently displayed in
        /// a <see langword="PaginationPanel"/>; otherwise, <see langword="false"/>.</returns>
        /// </summary>
        bool InPaginationPanel
        {
            get
            {
                return _imageViewer != null;
            }
        }

        /// <summary>
        /// Selects the appropriate page to load the appropriate document for the current selection
        /// in the DEP.
        /// </summary>
        /// <param name="allowSourceDocumentSwitch"><c>true</c> if the page change should be allowed
        /// if the page is from a different source document; otherwise, <c>false</c>.</param>
        void LoadPageInImageViewer(bool allowSourceDocumentSwitch)
        {
            var outputDocument = this.GetAncestors()
                .OfType<PaginationSeparator>()
                .SingleOrDefault()
                ?.Document;

            if (outputDocument == null)
            {
                return;
            }

            // Find the page control that corresponds to the active attribute.
            var pageControl = GetActiveAttributes()
                .Select(attribute => attribute.Value.HasSpatialInfo() ? attribute.Value : null)
                .Select(spatialString => outputDocument.PageControls.FirstOrDefault(c =>
                    spatialString != null && c != null &&
                    FileSystemMethods.ArePathsEqual(c.Page.OriginalDocumentName, spatialString.SourceDocName) &&
                    c.Page.OriginalPageNumber == spatialString.GetFirstPageNumber()))
                .FirstOrDefault();

            // If the page control corresponding to the active attribute is not from the
            // document currently active in the image viewer, raise the PageLoadRequest
            // to load the correct page (and the highlights for that page).
            if (pageControl != null)
            {
                if (FileSystemMethods.ArePathsEqual(pageControl.Page.OriginalDocumentName, _imageViewer.ImageFile))
                {
                    OnPageLoadRequest(pageControl.Page.OriginalDocumentName, pageControl.Page.OriginalPageNumber);
                }
                else if (allowSourceDocumentSwitch)
                {
                    ClearHighlights();

                    if (OnPageLoadRequest(pageControl.Page.OriginalDocumentName, pageControl.Page.OriginalPageNumber))
                    {
                        CreateAllAttributeHighlights(Attributes, null);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the enabled state of the swiping tools based on the current state of the DEP.
        /// </summary>
        public void UpdateSwipingState()
        {
            try
            {
                _imageViewer.AllowHighlight = SwipingEnabled;
                if (!SwipingEnabled &&
                    (_imageViewer.CursorTool == CursorTool.AngularHighlight ||
                        _imageViewer.CursorTool == CursorTool.RectangularHighlight ||
                        _imageViewer.CursorTool == CursorTool.WordHighlight))
                {
                    _imageViewer.CursorTool = CursorTool.None;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41665");
            }
        }

        /// <summary>
        /// Raises the <see cref="PageLoadRequest"/>
        /// </summary>
        /// <param name="sourceDocName">The source document that needs to be loaded in the image viewer.
        /// </param>
        /// <param name="pageNum">The page number that needs to be loaded in <see cref="_sourceDocName"/>.
        /// </param>
        bool OnPageLoadRequest(string sourceDocName, int pageNum)
        {
            var args = new PageLoadRequestEventArgs(sourceDocName, pageNum);
            PageLoadRequest?.Invoke(this, args);
            return args.Handled;
        }

        /// <summary>
        /// Raises the <see cref="UndoAvailabilityChanged"/>
        /// </summary>
        void OnUndoAvailabilityChanged()
        {
            UndoAvailabilityChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="RedoAvailabilityChanged"/>
        /// </summary>
        void OnRedoAvailabilityChanged()
        {
            RedoAvailabilityChanged?.Invoke(this, new EventArgs());
        }

        #endregion Private Members
    }
}
