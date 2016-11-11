﻿using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
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
    public class DataEntryDocumentDataPanel : DataEntryControlHost
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

        /// <summary>
        /// The query used to generate the <see cref="PaginationDocumentData.Summary"/>.
        /// </summary>
        DataEntryQuery _summaryDataEntryQuery;

        ///// <summary>
        ///// Keeps track of the documents for which a <see cref="UpdateDocumentStatus"/> call is in
        ///// progress on a background thread.
        ///// </summary>
        //ConcurrentDictionary<PaginationDocumentData, int> _pendingDocumentStatusUpdate =
        //    new ConcurrentDictionary<PaginationDocumentData, int>();

        ///// <summary>
        ///// Limits the number of threads that can run concurrently for <see cref="UpdateDocumentStatus"/> calls.
        ///// (1 - 3 where the number does no exceed the num of CPUs - 1).
        ///// </summary>
        //Semaphore _documentStatusUpdateSemaphore = new Semaphore(
        //    Math.Max(1, Math.Min(3, Environment.ProcessorCount - 1)),
        //    Math.Max(1, Math.Min(3, Environment.ProcessorCount - 1)));

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
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41423");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the data entry query text used to generate a summary for the document.
        /// </summary>
        /// <value>
        /// The data entry query text used to generate a summary for the document.
        /// </value>
        public string SummaryQuery
        {
            get;
            set;
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
        /// Gets a value indicating whether advanced data entry operations (such as undo/redo) are
        /// supported.
        /// </summary>
        /// <value><see langword="true"/> if advanced data entry operations (such as undo/redo) are
        /// supported; otherwise,<see langword="false"/>.
        /// </value>
        public bool AdvancedDataEntryOperationsSupported
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

                //// If this data is getting loaded, there is no need to proceed with any pending
                //// document status update.
                //int temp;
                //_pendingDocumentStatusUpdate.TryRemove(data, out temp);

                base.ImageViewer = _imageViewer;

                // Prevent cached query values from overriding differing data in the document being loaded.
                _summaryDataEntryQuery = null;

                _documentData = (DataEntryPaginationDocumentData)data;

                LoadData(_documentData.WorkingAttributes);

                if (_imageViewer.Visible)
                {
                    _documentData.SetSummary(SummaryDataEntryQuery?.Evaluate().ToString());
                    _documentData.SetModified(UndoOperationAvailable);
                    _documentData.SetDataError(DataValidity == DataValidity.Invalid);
                }

                UpdateSwipingState();

                if (_documentData.UndoState != null)
                {
                    AttributeStatusInfo.UndoManager.RestoreState(_documentData.UndoState);
                    OnUndoAvailabilityChanged();
                    OnRedoAvailabilityChanged();
                }

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

                var dataEntryData = (DataEntryPaginationDocumentData)data;
                dataEntryData.SetDataError(DataValidity == DataValidity.Invalid);
                dataEntryData.UndoState = AttributeStatusInfo.UndoManager.GetState();

                // GetData contains attributes that have been prepared for output.
                data.Attributes = GetData();
                data.Attributes.ReportMemoryUsage();

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
                _sourceDocName = null;

                base.ClearData();

                base.ImageViewer = null;

                if (_imageViewer != null)
                {
                    _imageViewer.ImageFileChanged -= HandleImageViewer_ImageFileChanged;
                    _imageViewer.ImageFileClosing -= HandleImageViewer_ImageFileClosing;
                    _imageViewer.PageChanged -= HandleImageViewer_PageChanged;

                    _imageViewer.Invalidate();

                    UpdateSwipingState();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41488");
            }
        }

        ///// <summary>
        ///// Gets a <see cref="PaginationDocumentData"/> instance based on the provided
        ///// <see paramref="attributes"/>.
        ///// </summary>
        ///// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData"/>
        ///// instance is needed.</param>
        ///// <param name="sourceDocName">The name of the source document for which data is being
        ///// loaded.</param>
        ///// <param name="fileProcessingDB"></param>
        ///// <param name="imageViewer"></param>
        ///// <returns>The <see cref="PaginationDocumentData"/> instance.</returns>
        //public PaginationDocumentData GetDocumentData(IUnknownVector attributes,
        //    string sourceDocName, FileProcessingDB fileProcessingDB, ImageViewer imageViewer)
        //{
        //    try
        //    {
        //        _imageViewer = imageViewer;
        //        var documentData = new DataEntryPaginationDocumentData(attributes, sourceDocName);
        //        if (!IsHandleCreated || !Visible)
        //        {
        //            UpdateDocumentStatus(documentData);
        //        }

        //        return documentData;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex.AsExtract("ELI41350");
        //    }
        //}

        ///// <summary>
        ///// Updates the <see cref="PaginationDocumentData.Summary"/>, <see cref="PaginationDocumentData.Modified"/>
        ///// and <see cref="PaginationDocumentData.DataError"/> properties of <see paramref="data"/> based on
        ///// the current document data.
        ///// </summary>
        ///// <param name="data">The <see cref="PaginationDocumentData"/> instance to update.</param>
        //public void UpdateDocumentDataStatus(PaginationDocumentData data)
        //{
        //    try
        //    {
        //        var dataEntryData = data as DataEntryPaginationDocumentData;
        //        UpdateDocumentStatus(dataEntryData);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex.AsExtract("ELI41474");
        //    }
        //}
        
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
                // Special logic applies only if the panel is being used in the pagination
                // context.
                if (InPaginationPanel)
                {
                    // Don't switch to a page that is not part of this document. There are many
                    // issues with doing so including unexpected page rotation behavior.
                    // https://extract.atlassian.net/browse/ISSUE-14208
                    var isValidPage = this.GetAncestors()
                        .OfType<PaginationSeparator>()
                        .Single()
                        .Document
                        .PageControls
                        .Where(c => c.Page.OriginalPageNumber == pageNumber)
                        .Any();

                    if (isValidPage)
                    {
                        base.SetImageViewerPageNumber(pageNumber);

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
        /// Raises the <see cref="E:System.Windows.Forms.Control.EnabledChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
        }

        /// <summary>
        /// Indicates document data has changed.
        /// </summary>
        protected override void OnDataChanged()
        {
            try
            {
                base.OnDataChanged();

                _documentData?.SetSummary(SummaryDataEntryQuery?.Evaluate().ToString());
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

                if (_documentData != null)
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
                if (_summaryDataEntryQuery != null)
                {
                    _summaryDataEntryQuery.Dispose();
                    _summaryDataEntryQuery = null;
                }

                //if (_documentStatusUpdateSemaphore != null)
                //{
                //    _documentStatusUpdateSemaphore.Dispose();
                //    _documentStatusUpdateSemaphore = null;
                //}
            }
        }

        /// <summary>
        /// Sets the image viewer.
        /// </summary>
        /// <param name="_imageViewer">The image viewer.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal void SetImageViewer(ImageViewer imageViewer)
        {
            _imageViewer = imageViewer;
            base.ImageViewer = imageViewer;
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

   //     /// <summary>
   //     /// Updates the <see cref="Summary"/>, <see cref="DataModified"/> and  <see cref="DataError"/>
   //     /// properties of <see paramref="documentData"/> by loading the data into a background panel.
   //     /// </summary>
   //     /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData"/> for which
   //     /// document status should be updated.</param>
   //     void UpdateDocumentStatus(DataEntryPaginationDocumentData documentData)
   //     {
			//string serializedAttributes = _miscUtils.GetObjectAsStringizedByteStream(documentData.WorkingAttributes);

   //         if (!_pendingDocumentStatusUpdate.TryAdd(documentData, 0))
   //         {
   //             // Document status is already being updated.
   //             return;
   //         }

   //         var thread = new Thread(new ThreadStart(() =>
   //         {
   //             try
   //             {
   //                 UpdateDocumentStatusThread(documentData, serializedAttributes);
   //             }
   //             catch (Exception ex)
   //             {
   //                 // Exceptions will be thrown if the FAM task has been stopped while a
   //                 // UpdateDocumentStatusThread was still running, but we don't care to know about
   //                 // the exceptions in this case.
   //                 if (IsHandleCreated)
   //                 {
   //                     ex.ExtractLog("ELI41494");
   //                 }
   //             }
   //         }));

   //         thread.SetApartmentState(ApartmentState.STA);
   //         thread.Start();
   //     }

   //     /// <summary>
   //     /// Code running in a background thread in support of <see cref="UpdateDocumentStatus"/>
   //     /// </summary>
   //     /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData"/> for which
   //     /// data validity should be checked.</param>
   //     void UpdateDocumentStatusThread(DataEntryPaginationDocumentData documentData, string serializedAttributes)
   //     {
   //         ExtractException ee = null;
   //         bool dataModified = false;
   //         bool dataError = false;
   //         string summary = null;
   //         Dictionary<string, DbConnection> connectionCopies = null;

   //         try
   //         {
   //             // If the document was loaded by the time this thread was spun up, abort.
   //             if (!_pendingDocumentStatusUpdate.ContainsKey(documentData))
   //             {
   //                 return;
   //             }

   //             _documentStatusUpdateSemaphore.WaitOne();

   //             // If by the time this thread has a chance to update, the document was loaded, abort.
   //             if (!_pendingDocumentStatusUpdate.ContainsKey(documentData))
   //             {
   //                 return;
   //             }

   //             var miscUtils = new MiscUtils();
   //             var deserializedAttributes = (IUnknownVector)miscUtils.GetObjectFromStringizedByteStream(serializedAttributes);

   //             using (var form = new Form())
   //             using (var tempPanel = (DataEntryDocumentDataPanel)Activator.CreateInstance(GetType()))
   //             using (tempPanel._imageViewer = new ImageViewer())
   //             using (var tempData = new DataEntryPaginationDocumentData(deserializedAttributes, documentData.SourceDocName))
   //             {
   //                 Config.ApplyObjectSettings(tempPanel);
   //                 tempPanel.DataEntryApplication = DataEntryApplication;
   //                 // DBConnections are not threadsafe; create copies for the new thread.
   //                 connectionCopies = CopyDatabaseConnections();
   //                 tempPanel.SetDatabaseConnections(connectionCopies);
   //                 form.MakeFormInvisible();
   //                 form.Show();
   //                 form.Controls.Add(tempPanel);
   //                 form.Controls.Add(tempPanel._imageViewer);

   //                 tempPanel.LoadData(tempData);
   //                 dataModified = tempPanel.UndoOperationAvailable;
   //                 dataError = (tempPanel.DataValidity != DataValidity.Valid);
   //                 summary = tempPanel.SummaryDataEntryQuery?.Evaluate().ToString();
   //             }
   //         }
   //         catch (Exception ex)
   //         {
   //             ee = ex.AsExtract("ELI41453");
   //         }
   //         finally
   //         {
   //             _documentStatusUpdateSemaphore.Release();
   //             if (connectionCopies != null)
   //             {
   //                 CollectionMethods.ClearAndDispose(connectionCopies);
   //             }
   //         }

   //         _imageViewer.SafeBeginInvoke("ELI41465", () =>
   //         {
   //             try
   //             {
   //                 if (ee != null)
   //                 {
   //                     ee.ExtractDisplay("ELI41464");
   //                 }

   //                 if (_pendingDocumentStatusUpdate.ContainsKey(documentData))
   //                 {
   //                     documentData.SetModified(dataModified);
   //                     documentData.SetDataError(dataError);
   //                     documentData.SetSummary(summary);
   //                 }
   //             }
   //             finally
   //             {
   //                 int temp;
   //                 _pendingDocumentStatusUpdate.TryRemove(documentData, out temp);
   //             }

   //         });
   //     }

        /// <summary>
        /// Creates a new database connection dictionary where the the <see cref="DbConnection"/>
        /// values have been cloned.
        /// </summary>
        Dictionary<string, DbConnection> CopyDatabaseConnections()
        {
            var connectionCopies = new Dictionary<string, DbConnection>();
            var createdConnections = new Dictionary<Tuple<Type, string>, DbConnection>();
            foreach (var item in DatabaseConnections)
            {
                var connectionKey = new Tuple<Type, string>(item.Value.GetType(), item.Value.ConnectionString);
                DbConnection connection = null;
                if (!createdConnections.TryGetValue(connectionKey, out connection))
                {
                    connection = (DbConnection)Activator.CreateInstance(item.Value.GetType());
                    connection.ConnectionString = item.Value.ConnectionString;
                    connection.Open();
                    createdConnections[connectionKey] = connection;
                }
                connectionCopies.Add(item.Key, connection);
            }

            return connectionCopies;
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