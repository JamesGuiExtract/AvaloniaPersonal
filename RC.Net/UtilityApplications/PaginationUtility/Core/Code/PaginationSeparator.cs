﻿using Extract.DataEntry;
using Extract.Imaging;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that indicates the divider between the end of one document
    /// and the start of another.
    /// </summary>
    internal partial class PaginationSeparator : PaginationControl
    {
        #region Fields

        /// <summary>
        /// The overall <see cref="Size"/> all <see cref="PaginationSeparator"/>s should be.
        /// </summary>
        static Size? _uniformSize;

        /// <summary>
        /// The panel to display for editing any <see cref="PaginationDocumentData"/>
        /// </summary>
        Control _documentDataPanelControl;

        /// <summary>
        /// The <see cref="OutputDocument"/> this with which this separator is currently associated.
        /// </summary>
        OutputDocument _outputDocument;

        /// <summary>
        /// Used to prevent recursion while trying to update the current Document.
        /// </summary>
        bool _changingDocument;

        /// <summary>
        /// Used to prevent recursion while trying to update the current selection state.
        /// </summary>
        bool _changingSelection;

        /// <summary>
        /// Indicates whether the view of the associated <see cref="OutputDocument"/> should be hidden.
        /// </summary>
        bool _collapsed;

        /// <summary>
        /// Indicates whether the associated <see cref="OutputDocument"/> has been selected to
        /// be committed.
        /// </summary>
        bool _documentSelectedToCommit;

        /// <summary>
        /// Indicates whether the associated <see cref="OutputDocument"/>'s has been viewed (in the
        /// data entry panel).
        /// </summary>
        bool? _documentDataHasBeenViewed;

        /// <summary>
        /// Indicates when a click event has been handled internal to this class and should not be
        /// raised.
        /// </summary>
        bool _clickEventHandledInternally;

        /// <summary>
        /// Indicates whether the selection check box should be visible.
        /// </summary>
        bool _showSelectionCheckBox;

        /// <summary>
        /// Indicates whether document status information has been applied to this separator.
        /// </summary>
        bool _hasAppliedStatus;

        /// <summary>
        /// Indicates whether a update of the separator controls is required to reflect a change
        /// in document status.
        /// </summary>
        bool _invalidatePending;

        /// <summary>
        /// Indicates whether an update of the separator controls is pending.
        /// </summary>
        bool _controlUpdatePending;

        /// <summary>
        /// Indicates whether indicator that pages can be dropped into the separator are being displayed.
        /// </summary>
        bool _showDragHints;

        /// <summary>
        /// The current color of the separator bar.
        /// </summary>
        Color _currentColor;

        /// <summary>
        /// The view button icon
        /// </summary>
        Image _viewButtonImage;

        /// <summary>
        /// The error icon to be displayed on the separator bar for documents with invalid data.
        /// </summary>
        Image _errorIconImage;

        /// <summary>
        /// The warning icon to be displayed on the separator bar for documents with validation warnings.
        /// </summary>
        Image _warningIconImage;

        /// <summary>
        /// The green check icon to be displayed on the separator bar for documents that were qualified
        /// to be processed automatically (but weren't).
        /// </summary>
        Image _qualifiedIconImage;

        // Used to delay updating a separator if another separator is updating
        static bool _updatingControlsOfSomeSeparator;

        // Used to skip updating this instance if already updating it
        bool _updatingControls;

        // The OutputDocument that had been assigned to this separator at the start of an on-going
        // operation. If the operation were to require a fresh document to be created, the provisional
        // document should be used instead (to prevent un-intended data loss).
        OutputDocument _provisionalDocument;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSeparator"/> class.
        /// </summary>
        /// <param name="showSelectionCheckBox"><see langword="true"/> if the selection check box
        /// should be visible.</param>
        /// <param name="pageLayoutControl">The <see cref="PageLayoutControl"/> this instance will be used in.</param>
        public PaginationSeparator(PageLayoutControl pageLayoutControl, bool showSelectionCheckBox)
            : base(pageLayoutControl)
        {
            try
            {
                InitializeComponent();

                // Initialize drag/drop indicator columns to hidden.
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    _tableLayoutPanel.ColumnStyles[3].Width = 0;
                    _tableLayoutPanel.ColumnStyles[6].Width = 0;
                }

                _showSelectionCheckBox = showSelectionCheckBox;
                if (!_showSelectionCheckBox)
                {
                    // If the selection check box is not to be displayed, allow the checkbox column
                    // to collapse.
                    _tableLayoutPanel.ColumnStyles[2].Width = 0;
                }

                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaginationSeparator));
                _viewButtonImage = ((System.Drawing.Image)(resources.GetObject("_viewDocumentDataButton.Image")));

                _qualifiedIconImage = Resources.Accept.ResizeHighQuality(16, 16);
                _errorIconImage = Resources.Error.ResizeHighQuality(16, 16);
                using (var warningBitmap = SystemIcons.Warning.ToBitmap())
                {
                    _warningIconImage = warningBitmap.ResizeHighQuality(16, 16);
                }

                _toolTip.SetToolTip(_newDocumentGlyph, "This is a new document that will be created");
                _toolTip.SetToolTip(_editedPaginationGlyph, "Manual pagination has been applied");
                _toolTip.SetToolTip(_reprocessDocumentPictureBox, "This document will be returned to the server");
                _toolTip.SetToolTip(_editedDataPictureBox, "The data for this document has been modified");
                _toolTip.SetToolTip(_dataValidityPictureBox, "The data for this document has error(s)");

                // Consider any time UIUpdateLocks are released to indicate the end of an operation.
                // At this point any previously assigned _provisionalDocument should not be used
                // in place of a new one.
                PageLayoutControl.ResumingUIUpdates += HandlePageLayoutControl_ResumingUIUpdates;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35496");
            }
        }

        #endregion Constructors

        #region Static Members

        /// <summary>
        /// Gets the overall <see cref="Size"/> all <see cref="PaginationSeparator"/>s should
        /// be.
        /// </summary>
        /// <value>
        /// The overall <see cref="Size"/> all <see cref="PaginationSeparator"/>s should be.
        /// </value>
        public static Size UniformSize
        {
            get
            {
                try
                {
                    if (_uniformSize == null)
                    {
                        using (var separator = new PaginationSeparator(null, false))
                        {
                            _uniformSize = new Size(-1, separator.Height);
                        }
                    }

                    return _uniformSize.Value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35658");
                }
            }
        }

        #endregion Static Members

        #region Events

        /// <summary>
        /// Raised when a <see cref="IPaginationDocumentDataPanel"/> is needed in response to the
        /// user selecting to edit data.
        /// </summary>
        public event EventHandler<DocumentDataPanelRequestEventArgs> DocumentDataPanelRequest;

        /// <summary>
        /// Raised when a <see cref="IPaginationDocumentDataPanel"/> is closed.
        /// </summary>
        public event EventHandler<EventArgs> DocumentDataPanelClosed;

        /// <summary>
        /// Raised when the user has used tab navigation in the <see cref="IPaginationDocumentDataPanel"/>
        /// </summary>
        public event EventHandler<TabNavigationEventArgs> TabNavigation;

        /// <summary>
        /// Raised when the view of the associated <see cref="OutputDocument"/> pages have either
        /// been collapsed or re-displayed.
        /// </summary>
        public event EventHandler<EventArgs> DocumentCollapsedChanged;

        /// <summary>
        /// Raised when the value of <see cref="DocumentSelectedToCommit"/> has been changed.
        /// </summary>
        public event EventHandler<EventArgs> DocumentSelectedToCommitChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets whether the associated <see cref="OutputDocument"/> has been selected to be
        /// committed.
        /// </summary>
        /// <value><see langword="true"/> if the document selected to be committed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool DocumentSelectedToCommit
        {
            get
            {
                return _documentSelectedToCommit;
            }

            set
            {
                try
                {
                    if (_showSelectionCheckBox &&
                        value != _documentSelectedToCommit)
                    {
                        if (Document != null)
                        {
                            Document.Selected = value;
                        }
                        else
                        {
                            value = false;
                        }

                        _documentSelectedToCommit = value;
                        _selectedCheckBox.Checked = value;

                        if (_outputDocument?.DocumentData != null)
                        {
                            AttributeStatusInfo.AcceptValue(_outputDocument.DocumentData.DocumentDataAttribute, value);
                        }

                        OnDocumentSelectedToCommitChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40202");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the associated <see cref="OutputDocument"/>'s data has been viewed
        /// (in the data panel).
        /// </summary>
        /// <value><see langword="true"/> if the document data has been viewed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool DocumentDataHasBeenViewed
        {
            get
            {
                return _documentDataHasBeenViewed.HasValue && _documentDataHasBeenViewed.Value;
            }

            set
            {
                try
                {
                    if (!_documentDataHasBeenViewed.HasValue || value != _documentDataHasBeenViewed.Value)
                    {
                        _documentDataHasBeenViewed = value;
                        _summaryLabel.Font = new Font(_summaryLabel.Font, value ? FontStyle.Regular : FontStyle.Bold);

                        if (_outputDocument?.DocumentData != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(_outputDocument.DocumentData.DocumentDataAttribute);
                            statusInfo.HasBeenViewed = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45961");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IPaginationDocumentDataPanel"/> used to display the document data for editing.
        /// </summary>
        public IPaginationDocumentDataPanel DocumentDataPanel
        {
            get
            {
                return _documentDataPanelControl as IPaginationDocumentDataPanel;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a update of the separator controls is required
        /// to reflect a change in document status.
        /// </summary>
        /// <value><c>true</c> if an update of the control is required; otherwise, <c>false</c>.
        /// </value>
        public bool InvalidatePending
        {
            get
            {
                return Parent != null && (_invalidatePending || _controlUpdatePending);
            }

            set
            {
                _invalidatePending = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance's data panel is open.
        /// </summary>
        /// <value><see langword="true"/> if this instance's data panel is open; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDataPanelOpen
        {
            get
            {
                return _documentDataPanelControl != null &&
                    _tableLayoutPanel.Controls.Contains(_documentDataPanelControl);
            }
        }

        /// <summary>
        /// Indicates whether indicator that pages can be dropped into the separator are being displayed.
        /// </summary>
        public bool ShowDragHints
        {
            get
            {
                return _showDragHints;
            }

            set
            {
                if (value != _showDragHints)
                {
                    _showDragHints = value;
                    UpdateDragHints();
                }
            }
        }

        /// <summary>
        /// Shows/hides drag/drop indicator depending on the current state of ShowDragHints
        /// </summary>
        void UpdateDragHints()
        {
            if (_leftArrowPictureBox.Visible != ShowDragHints)
            {
                _tableLayoutPanel.ColumnStyles[3].Width = ShowDragHints ? 21 : 0;
                _tableLayoutPanel.ColumnStyles[6].Width = ShowDragHints ? 21 : 0;
                _leftArrowPictureBox.Visible = ShowDragHints;
                _rightArrowPictureBox.Visible = ShowDragHints;
            }
        }

        #endregion Properties

        #region Public Members

        /// <summary>
        /// Gets the <see cref="OutputDocument"/> with which this separator is associated.
        /// </summary>
        /// <returns></returns>
        public override OutputDocument Document
        {
            get
            {
                if (_changingDocument)
                {
                    return _outputDocument;
                }

                try
                {
                    _changingDocument = true;

                    var firstPageControl = NextControl as PageThumbnailControl;
                    var document = (firstPageControl != null)
                        ? firstPageControl.Document
                        : null;

                    if (document != _outputDocument)
                    {
                        CloseDataPanel(false, false);

                        if (_outputDocument != null)
                        {
                            _outputDocument.Invalidated -= HandleDocument_Invalidated;
                            _outputDocument.DocumentStateChanged -= HandleOutputDocument_DocumentStateChanged;

                            // https://extract.atlassian.net/browse/ISSUE-16619
                            // Output documents will be dynamically assigned a separator based on the position
                            // of the controls in the panel in UpdateControls(). If the document for this separator
                            // is being re-assigned, document needs to be dis-associated with this instance, else
                            // in a case such as when all pages are moved to another document multiple documents
                            // will identify with the same separator causing issues when applying documents.
                            if (_outputDocument.PaginationSeparator == this)
                            {
                                _outputDocument.PaginationSeparator = null;
                            }

                            _provisionalDocument = _outputDocument;
                        }

                        _outputDocument = document;

                        if (_outputDocument != null)
                        {
                            _collapsed = _outputDocument.PageControls.Any(c => !c.Visible);
                            _outputDocument.Invalidated += HandleDocument_Invalidated;
                            _outputDocument.DocumentStateChanged += HandleOutputDocument_DocumentStateChanged;

                            if (_outputDocument?.DocumentData != null)
                            {
                                DocumentSelectedToCommit =
                                    AttributeStatusInfo.IsAccepted(_outputDocument.DocumentData.DocumentDataAttribute);
                                var statusInfo = AttributeStatusInfo.GetStatusInfo(Document.DocumentData.DocumentDataAttribute);
                                DocumentDataHasBeenViewed = statusInfo.HasBeenViewed;
                            }

                            UpdatePagesLabelFont();
                        }
                        else
                        {
                            DocumentSelectedToCommit = false;
                            DocumentDataHasBeenViewed = false;
                            _collapsed = false;
                        }

                        InvalidatePending = true;
                    }

                    return document;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40179");
                }
                finally
                {
                    _changingDocument = false;
                }
            }
        }

        /// <summary>
        /// The <see cref="OutputDocument"/> that had been assigned to this separator at the start of an
        /// on-going operation. If the operation were to require a fresh document to be created, the
        /// provisional document should be used instead (to prevent un-intended data loss)
        /// </summary>
        public OutputDocument ProvisionalDocument => _provisionalDocument;

        /// <summary>
        /// Gets or sets whether the view of the associated <see cref="OutputDocument"/> should be hidden.
        /// </summary>
        public bool Collapsed
        {
            get
            {
                return _collapsed;
            }

            set
            {
                try
                {
                    if (value != Collapsed)
                    {
                        if (Document != null)
                        {
                            if (value && IsDataPanelOpen)
                            {
                                CloseDataPanel(true, false);
                                return;
                            }

                            _collapsed = value;

                            Document.Collapsed = value;

                            // Display the new collapsed state.
                            _collapseDocumentButton.Image = Document.Collapsed
                                ? Properties.Resources.Expand
                                : Properties.Resources.Collapse;

                            OnDocumentCollapsedChanged();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40203");
                }
            }
        }

        /// <summary>
        /// Opens the <see cref="DocumentDataPanel"/>, populating it with the
        /// <see cref="Document"/>'s data.
        /// </summary>
        /// <param name="initialSelection">Indicates which field should be selected upon completion
        /// of the load (if any). If the document type field is available, it will count as the first
        /// field and can also count as the first field with an error if the document type is not valid.
        /// </param>
        public void OpenDataPanel(FieldSelection initialSelection)
        {
            try
            {
                if (Document != null && !IsDataPanelOpen)
                {
                    var args = new DocumentDataPanelRequestEventArgs(Document);
                    OnDocumentDataPanelRequest(args);

                    if (args.DocumentDataPanel != null)
                    {
                        args.DocumentDataPanel.Editable = !Document.OutputProcessed;
                        _documentDataPanelControl = (Control)args.DocumentDataPanel;
                        _documentDataPanelControl.Width = _tableLayoutPanel.Width;
                        _documentDataPanelControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                        _tableLayoutPanel.Controls.Add(_documentDataPanelControl, 0, 3);
                        _tableLayoutPanel.SetColumnSpan(_documentDataPanelControl, _tableLayoutPanel.ColumnCount);

                        args.DocumentDataPanel.LoadData(args.OutputDocument.DocumentData, 
                            forEditing: !Document.OutputProcessed, initialSelection);

                        args.DocumentDataPanel.DocumentLoaded += HandleDataEntryControlHost_DocumentLoaded;
                        args.DocumentDataPanel.DataPanelChanged += HandleDocumentDataPanel_DataPanelChanged;
                        args.DocumentDataPanel.TabNavigation += HandleDocumentDataPanel_TabNavigation;

                        DocumentDataHasBeenViewed = true;

                        // Ensure this control gets sized based upon the added _documentDataPanelControl.
                        PerformLayout();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40237");
            }
        }

        /// <summary>
        /// Closes the <see cref="DocumentDataPanel"/> (if visible), applying any changed data in
        /// the process.
        /// </summary>
        /// <param name="updateData"><see langword="true"/> to apply the panel's data to the
        /// document's data.</param>
        /// <param name="validateData"><see langword="true"/> if the document's data should
        /// be validated for errors when saving</param>
        /// <returns><see langword="true"/> if the data was saved or <see langword="false"/> if the
        /// data could not be saved and needs to be corrected.</returns>
        public bool CloseDataPanel(bool updateData, bool validateData)
        {
            try
            {
                if (IsDataPanelOpen)
                {
                    var documentDataPanel = (IPaginationDocumentDataPanel)_documentDataPanelControl;

                    if (Document != null && _outputDocument.DocumentData != null)
                    {
                        if (updateData && !documentDataPanel.SaveData(_outputDocument.DocumentData, validateData))
                        {
                            return false;
                        }

                        documentDataPanel.ClearData();
                    }

                    documentDataPanel.ActiveDataEntryPanel.EnsureFieldSelection(
                        targetField: FieldSelection.DoNotReset, viaTabKey: false);

                    // Report the DEP to be closed before it is removed so that events that trigger
                    // as part of its removal do not assume the DEP to be open and usable.
                    _documentDataPanelControl = null;
                    OnDocumentDataPanelClosed();

                    _tableLayoutPanel.Controls.Remove(documentDataPanel.PanelControl);
                    _tableLayoutPanel.Height = _tableLayoutPanel.Controls.OfType<Control>().Max(c => c.Bottom);
                    Height = _tableLayoutPanel.Height;

                    documentDataPanel.DocumentLoaded -= HandleDataEntryControlHost_DocumentLoaded;
                    documentDataPanel.DataPanelChanged -= HandleDocumentDataPanel_DataPanelChanged;
                    documentDataPanel.TabNavigation -= HandleDocumentDataPanel_TabNavigation;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40267");
            }
        }

        #endregion Public Members

        #region Overrides

        /// <summary>
        /// Gets or sets whether this control is selected.
        /// </summary>
        /// <value><see langword="true"/> if selected; otherwise, <see langword="false"/>.
        /// </value>
        public override bool Selected
        {
            get
            {
                return base.Selected;
            }   

            set
            {
                if (value != base.Selected)
                {
                    if (_changingSelection)
                    {
                        return;
                    }

                    try
                    {
                        _changingSelection = true;

                        base.Selected = value;

                        // Invalidate so that paint occurs and new selection state is indicated.
                        Invalidate();
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI40182");
                    }
                    finally
                    {
                        _changingSelection = false;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            try
            {
                CheckShouldScrollMarginChange(updateMargin: true);

                if (_tableLayoutPanel != null)
                {
                    Height = _tableLayoutPanel.Height;
                }

                if (_controlUpdatePending)
                {
                    UpdateControls();
                }

                base.OnLayout(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40204");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Invalidated"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Forms.InvalidateEventArgs"/> that
        /// contains the event data.</param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            try
            {
                InvalidatePending = true;

                // If the controls cannot be updated at this time, no need to invalidate (optimization).
                // If UpdateControls returns true it will have executed a full layout such that a call
                // to Invalidate would now be superfluous.
                if (!UpdateControls())
                {
                    SetColor();
                    base.OnInvalidated(e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40183");
            }
        }

        /// <summary>
        /// Updates the color of the separator bar based on the whether the current document has a
        /// page displayed in the image viewer.
        /// </summary>
        void SetColor()
        {
            // Determine what the separator color should be based on whether the current document
            // has a page displayed in the image viewer and whether the document has already been applied.
            // Gray = Processed
            // White = Unselected
            // LightGray = Selected, but not containing the active page selection
            // LightOrange = Selected, with active page selection.
            var newColor = (Document?.OutputProcessed == true)
                ? Color.Gray
                : (Document?.PageControls.Any(pageControl =>
                // Check was changed from pageControl.PageIsDisplayed to PageLayoutControl.PrimarySelection comparison
                // becuse UIUpdateLock can cause PageIsDisplayed to be false even when a page is the PrimarySelection
                // which would cause flickering of the separator color.
                        pageControl == PageLayoutControl.PrimarySelection && pageControl.Highlighted) == true)
                    ? ExtractColors.LightOrange
                    : Selected || (Document?.PageControls.Any(pageControl => pageControl.Selected) == true)
                        ? Color.LightGray
                        : ExtractColors.White;

            // Update the BackColor of the separator itself, as well as any controls except the edit button.
            if (newColor != _currentColor)
            {
                _tableLayoutPanel.BackColor = newColor;
                foreach (var control in _tableLayoutPanel.Controls.OfType<Control>()
                    .Except(new Control[] {
                        _topDividingLinePanel,
                        _bottomDividingLinePanel,
                        _documentDataPanelControl}))
                {
                    control.BackColor = newColor;
                }

                _currentColor = newColor;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                if (_clickEventHandledInternally)
                {
                    _clickEventHandledInternally = false;
                }
                else
                {
                    base.OnClick(e);

                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40206");
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            try
            {
                base.OnDoubleClick(e);

                if (!IsDataPanelOpen)
                {
                    OpenDataPanel(initialSelection: FieldSelection.First);
                    Collapsed = false;
                }
                else
                {
                    Collapsed = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49872");
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// <see langword="false"/> if the character was not processed.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                if (ActiveControl == _collapseDocumentButton ||
                    ActiveControl == _selectedCheckBox)
                {
                    // While needing to remain responsive to mouse clicks, any keyboard handling
                    // should be referred up to the the PageLayoutControl for processing of
                    // registered shortcuts.
                    return Parent.PreProcessMessage(ref msg);
                }

                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41278");
                return false;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (PageLayoutControl != null)
                {
                    PageLayoutControl.ResumingUIUpdates -= HandlePageLayoutControl_ResumingUIUpdates;
                    PageLayoutControl = null;
                }

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_viewButtonImage != null)
                {
                    _viewButtonImage.Dispose();
                    _viewButtonImage = null;
                }

                if (_errorIconImage != null)
                {
                    _errorIconImage.Dispose();
                    _errorIconImage = null;
                }

                if (_warningIconImage != null)
                {
                    _warningIconImage.Dispose();
                    _warningIconImage = null;
                }

                if (_qualifiedIconImage != null)
                {
                    _qualifiedIconImage.Dispose();
                    _qualifiedIconImage = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_collapseDocumentButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCollapseDocumentButton_Click(object sender, EventArgs e)
        {
            try
            {
                _clickEventHandledInternally = true;
                Collapsed = !Collapsed;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40184");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_selectedCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSelectedCheckBox_Click(object sender, EventArgs e)
        {
            _clickEventHandledInternally = true;
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_selectedCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSelectedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (DocumentSelectedToCommit != _selectedCheckBox.Checked)
                {
                    DocumentSelectedToCommit = _selectedCheckBox.Checked;

                    if (DocumentSelectedToCommit)
                    {
                        Collapsed = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40207");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.Invalidated"/> event of
        /// <see cref="_outputDocument"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDocument_Invalidated(object sender, EventArgs e)
        {
            try
            {
                InvalidatePending = true;

                // https://extract.atlassian.net/browse/ISSUE-15261
                // In some operations, the separator may be removed from the panel before the
                // document is invalid. We shouldn't invalidate controls no longer in the UI
                if (Parent != null)
                {
                    // Whenever the associated document is updated, invalidate to ensure displayed icons
                    // reflect the current document state.
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40180");
            }
        }

        /// <summary>
        /// Handles the <see cref="DocumentDataPanel.DataPanelChanged"/> event of the active panel.
        /// </summary>
        private void HandleDocumentDataPanel_DataPanelChanged(object sender, DataPanelChangedEventArgs e)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-15139
                // Ensure this control gets sized based upon the added _documentDataPanelControl.
                PerformLayout();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45337");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentStateChanged"/> event of the
        /// <see cref="_outputDocument"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        void HandleOutputDocument_DocumentStateChanged(object sender, EventArgs e)
        {
            try
            {
                // The below fix (ISSUE-15320) caused and issue where a separator no longer in
                // the PaginationPanel was trying to be refreshed.
                // https://extract.atlassian.net/browse/ISSUE-15331
                if (Parent != null)
                {
                    _controlUpdatePending = true;

                    // Direct calls to invalidate separators on document state changes have been removed
                    // from PaginationPanel. Instead, the separators will be responsible for ensuring
                    // UpdateControls is called at the end of the current event handler that triggered
                    // the change. Invoke to prevent multiple calls to UpdateControls as part of the
                    // same event. (UpdateControls will short-circut if an Invalidate from elsewhere has
                    // triggered UpdateControls in the interim)
                    // https://extract.atlassian.net/browse/ISSUE-15320
                    this.SafeBeginInvoke("ELI45648", () => UpdateControls(), false);
                }

                UpdatePagesLabelFont();
            }
            catch (Exception ex)
            {
                // Errors here are unlikely to be a critical issue; remove risk of displayed
                // exceptions leaving the pagination panel in a bad state by just logging here.
                // https://extract.atlassian.net/browse/ISSUE-15331
                ex.AsExtract("ELI45612").Log();
            }
        }

        /// <summary>
        /// Handles the casetab navigation is being processed by the data panel. This provides
        /// this class as well as the hosting PageLayoutControl an opportunity to enforce tab
        /// navigation scenarios. If not handled, this underlying DEP will.
        /// </summary>
        void HandleDocumentDataPanel_TabNavigation(object sender, TabNavigationEventArgs e)
        {
            try
            {
                if (!IsDataPanelOpen)
                {
                    return;
                }

                var documentDataPanel = (IPaginationDocumentDataPanel)_documentDataPanelControl;

                // If document type was being edited and a change can be applied, the application of
                // the document type will be the extent of handling here; return.
                if (e.Forward && e.LastStop
                    && documentDataPanel.DocumentTypeFocused
                    && documentDataPanel.ApplyDocumentTypeFromComboBox())
                {
                    e.Handled = true;
                    return;
                }

                // If document type is not valid, notify and return.
                if (e.Forward && e.LastStop
                    && !documentDataPanel.DocumentTypeIsValid)
                {
                    documentDataPanel.FocusDocumentType();

                    var ee = new ExtractException("ELI50128", "Please select a valid document type");
                    ee.Display();

                    // Inform the panel that once the user uses tab navigation again, TabNavigation
                    // should be raised.
                    documentDataPanel.PendingTabNavigationEventArgs = e;

                    e.Handled = true;
                    return;
                }
                
                // If there is invalid data, notify and return.
                if (e.Forward && e.LastStop &&
                    !documentDataPanel.SaveData(_outputDocument.DocumentData, validateData: true))
                {
                    // Inform the panel that once the user uses tab navigation again, TabNavigation
                    // should be raised.
                    documentDataPanel.PendingTabNavigationEventArgs = e;

                    e.Handled = true;
                    return;
                }

                // Allow the PageLayoutControl a chance to handle the event.
                if (TabNavigation != null)
                {
                    TabNavigation?.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                if (e.LastStop && !e.Forward)
                {
                    // Tabbing backward from the DocumentType field
                    if (documentDataPanel.DocumentTypeFocused)
                    {
                        CloseDataPanel(updateData: true, validateData: false);
                    }
                    // Tabbing backward from the DEP
                    else if (!documentDataPanel.FocusDocumentType())
                    {
                        CloseDataPanel(updateData: true, validateData: false);
                    }

                    e.Handled = true;
                }
                else if (!e.LastStop && e.Forward && documentDataPanel.DocumentTypeFocused)
                {
                    // Tabbing forward from the DocumentType field
                    documentDataPanel.ApplyDocumentTypeFromComboBox();
                    
                    documentDataPanel.EnsureDEPFieldSelection(
                        targetField: FieldSelection.First, viaTabKey: true);

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49857");
            }
        }

        /// <summary>
        /// Handles the DocumentLoaded event for the current DEP.
        /// </summary>
        void HandleDataEntryControlHost_DocumentLoaded(object sender, EventArgs e)
        {
            try
            {
                // If after initializing the document _documentDataPanelControl does not have focus
                // (either the document type or the DEP), select the first page thumbnail.
                if (!_documentDataPanelControl.Focused)
                {
                    foreach (var pageControl in Document.PageControls.Skip(1))
                    {
                        pageControl.Selected = false;
                    }
                    Document.PageControls.First().Selected = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50177");
            }
        }

        void HandlePageLayoutControl_ResumingUIUpdates(object sender, EventArgs e)
        {
            try
            {
                // Consider any time UIUpdateLocks are released to indicate the end of an operation.
                // At this point any previously assigned _provisionalDocument should not be used
                // in place of a new one.
                _provisionalDocument = null;

                // https://extract.atlassian.net/browse/ISSUE-17149
                // If the PageLayoutControl scroll bar has changed visiblity since the last layout,
                // force a layout to account for the change.
                if (Visible && CheckShouldScrollMarginChange(updateMargin: false))
                {
                    PerformLayout();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50357");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates UI indications to reflect the current state of the associated document.
        /// </summary>
        bool UpdateControls()
        {
            // Skip if already in the midst of updating
            if (_updatingControls)
            {
                return false;
            }
            // Skip if another separator is updating.
            // Prevents error creating window handle that happens when updates cascade
            // https://extract.atlassian.net/browse/ISSUE-17128
            if (_updatingControlsOfSomeSeparator)
            {
                this.SafeBeginInvoke("ELI50295", () => UpdateControls());
                return false;
            }
            try
            {
                _updatingControls = true;
                _updatingControlsOfSomeSeparator = true;
                if (Parent == null || !Parent.ClientRectangle.IntersectsWith(Bounds))
                {
                    return false;
                }

                if (Document != null && Document.PaginationSeparator != this)
                {
                    // Force a follow-up layout to occur after assigning this separator to a new document.
                    _controlUpdatePending = true;
                    Document.PaginationSeparator = this;
                    InvalidatePending = true;

                    if (Document.DocumentData != null)
                    {
                        DocumentSelectedToCommit =
                            AttributeStatusInfo.IsAccepted(_outputDocument.DocumentData.DocumentDataAttribute);
                        var statusInfo = AttributeStatusInfo.GetStatusInfo(Document.DocumentData.DocumentDataAttribute);
                        DocumentDataHasBeenViewed = statusInfo.HasBeenViewed;
                    }
                    else
                    {
                        DocumentSelectedToCommit = false;
                        DocumentDataHasBeenViewed = false;
                    }
                }

                if (_controlUpdatePending &&
                    PageLayoutControl.UIUpdatesSuspended != true &&
                    Document?.DocumentData?.Initialized == true)
                {
                    _controlUpdatePending = false;

                    _hasAppliedStatus = true;

                    _selectedCheckBox.Visible = _showSelectionCheckBox && !Document.OutputProcessed;
                    _selectedCheckBox.Checked = _showSelectionCheckBox && Document.Selected;
                    _collapseDocumentButton.Visible = true;
                    _collapseDocumentButton.Image = Document.Collapsed
                        ? Properties.Resources.Expand
                        : Properties.Resources.Collapse;

                    UpdateDragHints();

                    _summaryLabel.Visible = true;
                    _summaryLabel.Text = Document.Summary
                        + (Document.OutputProcessed ? " (PROCESSED)" : "");
                    _pagesLabel.Visible = true;
                    int pageCount = Document.PageControls.Count(c => !c.Deleted);
                    _pagesLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "{0} page{1}", pageCount, (pageCount == 1) ? "" : "s");
                    if (_pagesLabel.Width < _pagesLabel.PreferredWidth)
                    {
                        // https://extract.atlassian.net/browse/ISSUE-13980
                        // To fix cases where the width has no updated properly
                        _pagesLabel.PerformLayout();
                    }

                    _newDocumentGlyph.Visible = !Document.InSourceDocForm && !Document.OutputProcessed
                        && Document.PageControls.Any(page => !page.Deleted);
                    _editedPaginationGlyph.Visible = !Document.InOriginalForm && !Document.OutputProcessed;
                    _reprocessDocumentPictureBox.Visible = Document.SendForReprocessing && pageCount > 0 && !Document.OutputProcessed;
                    _editedDataPictureBox.Visible = Document.DataModified && !Document.OutputProcessed;

                    UpdateDataValidityIcon();

                    SetColor();

                    PerformLayout();
                    return true;
                }
                else
                {
                    if (_hasAppliedStatus && Document?.DocumentData.Initialized != true)
                    {
                        _hasAppliedStatus = false;
                        _collapseDocumentButton.Visible = false;
                        _selectedCheckBox.Visible = false;
                        _leftArrowPictureBox.Visible = false;
                        _summaryLabel.Text = "";
                        _pagesLabel.Text = "";
                        _rightArrowPictureBox.Visible = false;
                        _newDocumentGlyph.Visible = false;
                        _editedPaginationGlyph.Visible = false;
                        _reprocessDocumentPictureBox.Visible = false;
                        _editedDataPictureBox.Visible = false;
                        _dataValidityPictureBox.Visible = false;

                        // Initialize drag/drop indicator columns to hidden.
                        _tableLayoutPanel.ColumnStyles[3].Width = 0;
                        _tableLayoutPanel.ColumnStyles[6].Width = 0;
                    }

                    return false;
                }
            }
            finally
            {
                _updatingControls = false;
                _updatingControlsOfSomeSeparator = false;
            }
        }

        /// <summary>
        /// Updates the status of the <see cref="_dataValidityPictureBox"/> based on the current document's data.
        /// </summary>
        void UpdateDataValidityIcon()
        {
            bool errorOrWarning = (Document.DataError || Document.DataWarning) && !Document.OutputProcessed;
            if (errorOrWarning)
            {
                _dataValidityPictureBox.Visible = true;
                _dataValidityPictureBox.Image = Document.DataError ? _errorIconImage : _warningIconImage;
                _toolTip.SetToolTip(_dataValidityPictureBox,
                    Document?.DocumentData?.DataErrorMessage ??
                        $"The data for this document has {(Document.DataError ? "error(s)" : "warning(s)")}");
            }
            else if (Document.QualifiedForAutomaticOutput == true 
                && !Document.OutputProcessed
                && Document.InOriginalForm
                && !Document.DocumentData.Modified)
            {
                _dataValidityPictureBox.Visible = true;
                _dataValidityPictureBox.Image = _qualifiedIconImage;
                _toolTip.SetToolTip(_dataValidityPictureBox, "This document qualified for automated processing");
            }
            else
            {
                _dataValidityPictureBox.Visible = false;
            }
        }

        /// <summary>
        /// Updates the font of the pages label so that it is in sync with whether all pages in a
        /// document have been viewed.
        /// </summary>
        void UpdatePagesLabelFont()
        {
            bool allPagesHaveBeenDisplayed =
                _outputDocument.PageControls.All(pageControl => pageControl.Viewed);

            if (_pagesLabel.Font.Bold == allPagesHaveBeenDisplayed)
            {
                _pagesLabel.Font = new Font(_pagesLabel.Font,
                    allPagesHaveBeenDisplayed ? FontStyle.Regular : FontStyle.Bold);
            }
        }

        /// <summary>
        /// Indicates if the margin to account for the scroll bar needs to be updated and,
        /// if <see paramref="updateMargin"/> is <c>true</c>, updates it.
        /// </summary>
        bool CheckShouldScrollMarginChange(bool updateMargin)
        {
            if (Parent is ScrollableControl parentPanel)
            {
                // Set margin when scroll bar is not visible such that when it does become visible,
                // it doesn't force a shift of separator icons/controls to the left.
                int scrollMargin = parentPanel.VerticalScroll.Visible
                    ? 0
                    : SystemInformation.VerticalScrollBarWidth;

                var marginColumnStyle = _tableLayoutPanel.ColumnStyles.OfType<ColumnStyle>().Last();

                if (marginColumnStyle.Width != scrollMargin)
                {
                    if (updateMargin)
                    {
                        _tableLayoutPanel.SuspendLayout();
                        marginColumnStyle.Width = scrollMargin;

                        _tableLayoutPanel.ResumeLayout();
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Raises the <see cref="DocumentDataPanelRequest"/> event.
        /// </summary>
        /// <param name="args">The <see cref="DocumentDataPanelRequestEventArgs"/> instance
        /// containing the event data.</param>
        void OnDocumentDataPanelRequest(DocumentDataPanelRequestEventArgs args)
        {
            var eventHandler = DocumentDataPanelRequest;
            if (eventHandler != null)
            {
                eventHandler(this, args);
            }
        }

        /// <summary>
        /// Raises the <see cref="DocumentDataPanelClosed"/> event.
        /// </summary>
        void OnDocumentDataPanelClosed()
        {
            var eventHandler = DocumentDataPanelClosed;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="DocumentCollapsedChanged"/> event.
        /// </summary>
        void OnDocumentCollapsedChanged()
        {
            var eventHandler = DocumentCollapsedChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="DocumentSelectedToCommit"/> event.
        /// </summary>
        void OnDocumentSelectedToCommitChanged()
        {
            var eventHandler = DocumentSelectedToCommitChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
