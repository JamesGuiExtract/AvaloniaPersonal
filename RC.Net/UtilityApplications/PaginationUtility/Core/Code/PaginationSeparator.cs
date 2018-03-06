﻿using Extract.Utilities.Forms;
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
        /// Indicates when a click event has been handled internal to this class and should not be
        /// raised.
        /// </summary>
        bool _clickEventHandledInternally;

        /// <summary>
        /// Indicates whether the visibility of the data editing panel is currently being toggled.
        /// </summary>
        bool _changingDataPanelVisiblity;

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

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSeparator"/> class.
        /// </summary>
        /// <param name="showSelectionCheckBox"><see langword="true"/> if the selection check box
        /// should be visible; otherwise, <see langword="false"/>.</param>
        public PaginationSeparator(bool showSelectionCheckBox)
            : base()
        {
            try
            {
                InitializeComponent();

                _showSelectionCheckBox = showSelectionCheckBox;
                if (!_showSelectionCheckBox)
                {
                    // If the selection check box is not to be displayed, allow the checkbox column
                    // to collapse.
                    _tableLayoutPanel.ColumnStyles[2].Width = 0;
                }
                
                _toolTip.SetToolTip(_newDocumentGlyph, "This is a new document that will be created");
                _toolTip.SetToolTip(_editedPaginationGlyph, "Manual pagination has been applied");
                _toolTip.SetToolTip(_reprocessDocumentPictureBox, "This document will be returned to the server");
                _toolTip.SetToolTip(_editedDataPictureBox, "The data for this document has been modified");
                _toolTip.SetToolTip(_dataErrorPictureBox, "The data for this document has error(s)");
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
                        using (var separator = new PaginationSeparator(false))
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
        /// 
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
                return (_invalidatePending || _controlUpdatePending);
            }

            set
            {
                _invalidatePending = value;
            }
        }

        #endregion Properties

        #region Public Members

        /// <summary>
        /// Gets the <see cref="OutputDocument"/> with which this separator is associated.
        /// </summary>
        /// <returns></returns>
        public OutputDocument Document
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
                        }

                        _outputDocument = document;

                        if (_outputDocument != null)
                        {
                            _collapsed = _outputDocument.PageControls.Any(c => !c.Visible);
                            _outputDocument.Invalidated += HandleDocument_Invalidated;
                            _outputDocument.DocumentStateChanged += HandleOutputDocument_DocumentStateChanged;
                        }
                        else
                        {
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
                            if (value)
                            {
                                if (!CloseDataPanel(true, false))
                                {
                                    return;
                                }
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
        public void OpenDataPanel()
        {
            bool locked = false;

            try
            {
                _changingDataPanelVisiblity = true;

                if (Document != null && !IsDataPanelOpen)
                {
                    var args = new DocumentDataPanelRequestEventArgs(Document);
                    OnDocumentDataPanelRequest(args);

                    if (args.DocumentDataPanel != null)
                    {
                        FormsMethods.LockControlUpdate(this, true);
                        locked = true;
                        _documentDataPanelControl = (Control)args.DocumentDataPanel;
                        _documentDataPanelControl.Width = _tableLayoutPanel.Width;
                        _tableLayoutPanel.Controls.Add(_documentDataPanelControl, 0, 1);
                        _tableLayoutPanel.SetColumnSpan(_documentDataPanelControl, _tableLayoutPanel.ColumnCount);
                        _documentDataPanelControl.Dock = DockStyle.Fill;

                        args.DocumentDataPanel.LoadData(args.OutputDocument.DocumentData, forDisplay: true);

                        _editDocumentDataButton.Checked = true;

                        args.DocumentDataPanel.DataPanelChanged += DocumentDataPanel_DataPanelChanged;

                        // Ensure this control gets sized based upon the added _documentDataPanelControl.
                        PerformLayout();
                    }
                    else
                    {
                        // The data panel was not able to be opened.
                        _editDocumentDataButton.Checked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _editDocumentDataButton.Checked = IsDataPanelOpen;
                }
                catch (Exception ex2)
                {
                    ex2.ExtractLog("ELI40268");
                }

                throw ex.AsExtract("ELI40237");
            }
            finally
            {
                _changingDataPanelVisiblity = false;

                if (locked)
                {
                    try
                    {
                        FormsMethods.LockControlUpdate(this, false);
                        Refresh();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI40208");
                    }
                }
            }
        }

        /// <summary>
        /// Closes the <see cref="DocumentDataPanel"/> (if visible), applying any changed data in
        /// the process.
        /// </summary>
        /// <param name="saveData"><see langword="true"/> to save the
        /// document's data; otherwise, <see langwor="false"/>.</param>
        /// <param name="validateData"><see langword="true"/> if the document's data should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns><see langword="true"/> if the data was saved or <see langword="false"/> if the
        /// data could not be saved and needs to be corrected.</returns>
        public bool CloseDataPanel(bool saveData, bool validateData)
        {
            try
            {
                _changingDataPanelVisiblity = true;

                if (IsDataPanelOpen)
                {
                    var documentDataPanel = (IPaginationDocumentDataPanel)_documentDataPanelControl;
                    if (Document != null && _outputDocument.DocumentData != null)
                    {
                        if (saveData && !documentDataPanel.SaveData(_outputDocument.DocumentData, validateData))
                        {
                            _editDocumentDataButton.Checked = true;
                            return false;
                        }

                        documentDataPanel.ClearData();
                    }

                    documentDataPanel.DataPanelChanged -= DocumentDataPanel_DataPanelChanged;

                    // Report the DEP to be closed before it is removed so that events that trigger
                    // as part of its removal do not assume the DEP to be open and usable.
                    _documentDataPanelControl = null;
                    OnDocumentDataPanelClosed();

                    _tableLayoutPanel.Controls.Remove(documentDataPanel.PanelControl);
                    _tableLayoutPanel.Height = _tableLayoutPanel.Controls.OfType<Control>().Max(c => c.Bottom);
                    Height = _tableLayoutPanel.Height;
                }

                _editDocumentDataButton.Checked = false;

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    _editDocumentDataButton.Checked = IsDataPanelOpen;
                }
                catch (Exception ex2)
                {
                    ex2.ExtractLog("ELI40269");
                }

                throw ex.AsExtract("ELI40267");
            }
            finally
            {
                _changingDataPanelVisiblity = false;
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
                if (Parent != null)
                {
                    var parentPanel = (ScrollableControl)Parent;

                    int rightPadding = parentPanel.VerticalScroll.Visible
                        ? 0
                        : SystemInformation.VerticalScrollBarWidth;

                    if (Padding.Right != rightPadding)
                    {
                        Padding = new Padding(0, 0, rightPadding, 0);
                    }
                }

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
                    base.OnInvalidated(e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40183");
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
                    ActiveControl == _selectedCheckBox ||
                    ActiveControl == _editDocumentDataButton)
                {
                    // Prevent confusion that space key is intended to activate this buttons by
                    // preventing keys from activating these buttons. Instead, send the key event
                    // on to the parent control so it does what it otherwise should have done.
                    KeyMethods.SendKeyToControl((int)keyData, false, false, false, Parent);
                    return false;
                }

                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41278");
                return false;
            }
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
                _clickEventHandledInternally = true;

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
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_editDocumentDataButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEditDocumentDataButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_changingDataPanelVisiblity)
            {
                return;
            }

            try
            {
                _clickEventHandledInternally = true;

                if (_editDocumentDataButton.Checked)
                {
                    OpenDataPanel();
                    Document.Collapsed = false;
                }
                else
                {
                    CloseDataPanel(true, false);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40185");
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
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DocumentDataPanel_DataPanelChanged(object sender, EventArgs e)
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
                _controlUpdatePending = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45612");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets a value indicating whether this instance's data panel is open.
        /// </summary>
        /// <value><see langword="true"/> if this instance's data panel is open; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool IsDataPanelOpen
        {
            get
            {
                return _documentDataPanelControl != null &&
                    _tableLayoutPanel.Controls.Contains(_documentDataPanelControl);
            }
        }

        /// <summary>
        /// Updates UI indications to reflect the current state of the associated document.
        /// </summary>
        bool UpdateControls()
        {
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
            }

            if (_controlUpdatePending &&
                (Parent as PageLayoutControl)?.UIUpdatesSuspended != true &&
                Document?.DocumentData?.Initialized == true)
            {
                _controlUpdatePending = false;

                _hasAppliedStatus = true;
                _selectedCheckBox.Visible = _showSelectionCheckBox;
                _selectedCheckBox.Checked = _showSelectionCheckBox && Document.Selected;
                _collapseDocumentButton.Visible = true;
                _collapseDocumentButton.Image = Document.Collapsed
                    ? Properties.Resources.Expand
                    : Properties.Resources.Collapse;
                _editDocumentDataButton.Visible =
                    Document.DocumentData != null && Document.DocumentData.AllowDataEdit;
                _summaryLabel.Visible = true;
                _summaryLabel.Text = Document.Summary;
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
                _newDocumentGlyph.Visible = !Document.InSourceDocForm;
                _editedPaginationGlyph.Visible = !Document.InOriginalForm;
                _reprocessDocumentPictureBox.Visible = Document.SendForReprocessing && pageCount > 0;
                _editedDataPictureBox.Visible = Document.DataModified;
                _dataErrorPictureBox.Visible = Document.DataError;
                _toolTip.SetToolTip(_dataErrorPictureBox,
                    Document?.DocumentData?.DataErrorMessage ?? "The data for this document has error(s)");

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
                    _editDocumentDataButton.Visible = false;
                    _summaryLabel.Text = "";
                    _pagesLabel.Text = "";
                    _newDocumentGlyph.Visible = false;
                    _editedPaginationGlyph.Visible = false;
                    _reprocessDocumentPictureBox.Visible = false;
                    _editedDataPictureBox.Visible = false;
                    _dataErrorPictureBox.Visible = false;
                }

                return false;
            }
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
