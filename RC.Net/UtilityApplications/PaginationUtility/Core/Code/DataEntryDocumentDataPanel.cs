using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using System;
using System.Linq;
using System.Threading;
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
        /// Used to serialize attribute data between threads.
        /// </summary>
        MiscUtils _miscUtils = new MiscUtils();

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryDocumentDataPanel"/> class.
        /// </summary>
        public DataEntryDocumentDataPanel()
        {
            try
            {
                AttributeStatusInfo.UndoManager.UndoAvailabilityChanged += HandleUndoManager_UndoAvailabilityChanged;
                AttributeStatusInfo.UndoManager.RedoAvailabilityChanged += HandleUndoManager_RedoAvailabilityChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41423");
            }
        }

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
        /// Gets a value indicating whether undo/redo operations are supported.
        /// </summary>
        /// <value><see langword="true"/> if undo/redo operations are supported.; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool AllowUndo
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an undo operation is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if an undo operation is available; otherwise, <c>false</c>.
        /// </value>
        public bool UndoOperationAvailable
        {
            get
            {
                return AttributeStatusInfo.UndoManager.UndoOperationAvailable;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an redo operation is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if an redo operation is available; otherwise, <c>false</c>.
        /// </value>
        public bool RedoOperationAvailable
        {
            get
            {
                return AttributeStatusInfo.UndoManager.RedoOperationAvailable;
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="data" />.
        /// </summary>
        /// <param name="data">The data to load.</param>
        public void LoadData(PaginationDocumentData data)
        {
            try
            {
                ExtractException.Assert("ELI41360", "Panel not properly initialized.", _imageViewer != null);

                base.ImageViewer = _imageViewer;

                LoadData(data.Attributes);

                _documentData = (DataEntryPaginationDocumentData)data;
                _documentData.SetDataError(DataValidity != DataValidity.Valid);

                UpdateSwipingState();

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

                if (validateData && !DataCanBeSaved())
                {
                    return false;
                }

                _documentData = null;
                _sourceDocName = null;

                ((DataEntryPaginationDocumentData)data).SetDataError(DataValidity != DataValidity.Valid);
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
                CheckDataValidity(documentData);

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
        /// Raises the <see cref="E:Extract.DataEntry.DataEntryControlHost.DataValidityChanged" /> event.
        /// </summary>
        protected override void OnDataValidityChanged()
        {
            try
            {
                base.OnDataValidityChanged();

                if (_documentData != null)
                {
                    _documentData.SetDataError(DataValidity != DataValidity.Valid);
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

        /// <summary>
        /// Handles the <see cref="UndoManager.UndoAvailabilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleUndoManager_UndoAvailabilityChanged(object sender, EventArgs e)
        {
            try
            {
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
        /// Updates the <see cref="DataError"/> property of <see paramref="documentData"/> by
        /// loading the data into a background panel.
        /// </summary>
        /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData"/> for which
        /// data validity should be checked.</param>
        void CheckDataValidity(DataEntryPaginationDocumentData documentData)
        {
            bool dataError = false;

            // Needed to determine if data is invalid.
            if (!IsHandleCreated || !Visible)
            {
                string serializedAttributes = _miscUtils.GetObjectAsStringizedByteStream(documentData.Attributes);

                var thread = new Thread(new ThreadStart(() =>
                {
                    var miscUtils = new MiscUtils();
                    var deserializedAttributes = (IUnknownVector)miscUtils.GetObjectFromStringizedByteStream(serializedAttributes);
                    var tempData = new DataEntryPaginationDocumentData(deserializedAttributes, documentData.SourceDocName);

                    using (var form = new Form())
                    using (var tempPanel = (DataEntryDocumentDataPanel)Activator.CreateInstance(GetType()))
                    using (tempPanel._imageViewer = new ImageViewer())
                    {
                        Config.ApplyObjectSettings(tempPanel);
                        tempPanel.DataEntryApplication = DataEntryApplication;
                        tempPanel.SetDatabaseConnections(DatabaseConnections);
                        form.MakeFormInvisible();
                        form.Show();
                        form.Controls.Add(tempPanel);
                        form.Controls.Add(tempPanel._imageViewer);

                        tempPanel.LoadData(tempData);
                        dataError = (tempPanel.DataValidity != DataValidity.Valid);
                    }

                }));

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }

            documentData.SetDataError(dataError);
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

        /// <summary>
        /// Raises the <see cref="UndoAvailabilityChanged"/>
        /// </summary>
        void OnUndoAvailabilityChanged()
        {
            var eventHandler = UndoAvailabilityChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="RedoAvailabilityChanged"/>
        /// </summary>
        void OnRedoAvailabilityChanged()
        {
            var eventHandler = RedoAvailabilityChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
