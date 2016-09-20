﻿using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// An <see cref="IPaginationDocumentDataPanel"/> implementation that allow for full DEP support
    /// in the context of a pagination data editing panel.
    /// </summary>
    public class DataEntryDocumentDataPanel : DataEntryControlHost, IPaginationDocumentDataPanel
    {
        #region Fields

        /// <summary>
        /// The <see cref="ImageViewer"/> to be used.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Indicates whether swiping should be enabled based on the current state of the DEP.
        /// </summary>
        bool _swipingEnabled;

        /// <summary>
        /// The currently loaded <see cref="DataEntryPaginationDocumentData"/>.
        /// </summary>
        DataEntryPaginationDocumentData _documentData;

        /// <summary>
        /// The source document related to <see cref="_documentData"/> if there is a singular source
        /// document; otherwise <see langword="null"/>.
        /// </summary>
        string _sourceDocName;

        #endregion Fields

        #region IPaginationDocumentDataPanel

        /// <summary>
        /// Raised to indicate the panel is requesting a specific image page to be loaded.
        /// </summary>
        public event EventHandler<PageLoadRequestEventArgs> PageLoadRequest;

        /// <summary>
        /// The <see cref="UserControl"/> to be displayed for viewing/editing of document data.
        /// </summary>
        public UserControl Control
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="data"/>.
        /// </summary>
        /// <param name="data">The data to load.</param>
        public void LoadData(PaginationDocumentData data)
        {
            try
            {
                ExtractException.Assert("ELI41360", "Panel not properly initialized.", _imageViewer != null);

                base.ImageViewer = _imageViewer;

                LoadData(data.Attributes);

                UpdateSwipingState();

                _documentData = data as DataEntryPaginationDocumentData;
                _sourceDocName = _documentData.SourceDocName;

                _imageViewer.ImageFileChanged += HandleImageViewer_ImageFileChanged;
                _imageViewer.ImageFileClosing += HandleImageViewer_ImageFileClosing;
                _imageViewer.PageChanged += HandleImageViewer_PageChanged;

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
        public bool SaveData(PaginationDocumentData data, bool validateData)
        {
            try
            {
                ExtractException.Assert("ELI41361", "Panel not properly initialized.", _imageViewer != null);

                _documentData = null;
                _sourceDocName = null;

                data.Attributes = GetData();
                data.Attributes.ReportMemoryUsage();

                Active = false;

                ClearData();

                base.ImageViewer = null;

                _imageViewer.ImageFileChanged -= HandleImageViewer_ImageFileChanged;
                _imageViewer.ImageFileClosing -= HandleImageViewer_ImageFileClosing;
                _imageViewer.PageChanged -= HandleImageViewer_PageChanged;

                UpdateSwipingState();

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41349");
            }
        }

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance based on the provided
        /// <see paramref="attributes"/>.
        /// </summary>
        /// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData"/>
        /// instance is needed.</param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="imageViewer"></param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance.</returns>
        public PaginationDocumentData GetDocumentData(IUnknownVector attributes,
            string sourceDocName, FileProcessingDB fileProcessingDB, ImageViewer imageViewer)
        {
            try
            {
                _imageViewer = imageViewer;
                var documentData = new DataEntryPaginationDocumentData(attributes, sourceDocName);
                return documentData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41350");
            }
        }

        /// <summary>
        /// Provides a message to be displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void ShowMessage(string message)
        {
            throw new NotImplementedException();
        }

        #endregion IPaginationDocumentDataPanel

        #region Overrides

        /// <summary>
        /// Navigates to the specified page, settings _performingProgrammaticZoom in the process to
        /// avoid handling scroll and zoom events that occur as a result.
        /// </summary>
        /// <param name="pageNumber">The page to be displayed</param>
        protected override void SetImageViewerPageNumber(int pageNumber)
        {
            try
            {
                base.SetImageViewerPageNumber(pageNumber);

                // Special logic applies only if the panel is not being used in the pagination
                // context.
                if (InPaginationPanel)
                {
                    var flowLayoutPanel = this.GetAncestors()
                        .OfType<PaginationFlowLayoutPanel>()
                        .Single();

                    int scrollPos = flowLayoutPanel.VerticalScroll.Value;
                    OnPageLoadRequest(pageNumber);
                    flowLayoutPanel.VerticalScroll.Value = scrollPos;
                    base.ImageViewer = _imageViewer;
                    DrawHighlights(true);
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
                    _swipingEnabled = e.SwipingEnabled;

                    UpdateSwipingState();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41352");
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
                // Special logic applies only if the panel is not being used in the pagination
                // context.
                if (InPaginationPanel)
                {
                    if (_imageViewer.ImageFile.Equals(_sourceDocName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (base.ImageViewer == null && !string.IsNullOrWhiteSpace(_sourceDocName))
                        {
                            base.ImageViewer = _imageViewer;
                        }

                        DrawHighlights(true);
                    }
                    else if (!string.IsNullOrWhiteSpace(_sourceDocName))
                    {
                        var flowLayoutPanel = this.GetAncestors()
                            .OfType<PaginationFlowLayoutPanel>()
                            .Single();
                        int scrollPos = flowLayoutPanel.VerticalScroll.Value;
                        OnPageLoadRequest(1);
                        flowLayoutPanel.VerticalScroll.Value = scrollPos;
                        base.ImageViewer = _imageViewer;
                        DrawHighlights(true);
                    }
                }

                base.OnEnter(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41353");
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
                if (Visible &&
                    _imageViewer.ImageFile.Equals(_sourceDocName, StringComparison.OrdinalIgnoreCase))
                {
                    base.ImageViewer = _imageViewer;
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
        /// Handles the <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The <see cref="PageChangedEventArgs"/> that contains the event data.</param>
        void HandleImageViewer_PageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                base.ImageViewer =
                    (_imageViewer.ImageFile.Equals(_sourceDocName, StringComparison.OrdinalIgnoreCase))
                        ? _imageViewer
                        : null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41356");
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
        /// Updates the enabled state of the swiping tools based on the current state of the DEP.
        /// </summary>
        void UpdateSwipingState()
        {
            _imageViewer.AllowHighlight = base.ImageViewer != null && _swipingEnabled;
        }

        /// <summary>
        /// Raises the <see cref="PageLoadRequest"/>
        /// </summary>
        /// <param name="pageNum">The page number that needs to be loaded in <see cref="_sourceDocName"/>.
        /// </param>
        void OnPageLoadRequest(int pageNum)
        {
            var eventHandler = PageLoadRequest;
            if (eventHandler != null)
            {
                eventHandler(this, new PageLoadRequestEventArgs(_sourceDocName, pageNum));
            }
        }

        #endregion Private Members
    }
}
