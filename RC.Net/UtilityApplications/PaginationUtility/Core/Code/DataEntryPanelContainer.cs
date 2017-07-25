﻿using Extract.DataEntry;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="UserControl"/> that contains a <see cref="DataEntryDocumentDataPanel"/> and
    /// potentially a document type combo box for multi-DEP DE configurations.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    /// <seealso cref="Extract.UtilityApplications.PaginationUtility.IPaginationDocumentDataPanel" />
    public partial class DataEntryPanelContainer : UserControl, IPaginationDocumentDataPanel
    {
        #region Fields

        /// <summary>
        /// Used to serialize attribute data between threads.
        /// </summary>
        MiscUtils _miscUtils = new MiscUtils();

        /// <summary>
        /// Manages all <see cref="DataEntryConfiguration"/>s currently available. Multiple
        /// configurations will exist when there are multiple DEPs defined where the one used depends
        /// on doc-type.
        /// </summary>
        DataEntryConfigurationManager<Properties.Settings> _configManager;

        /// <summary>
        /// The currently loaded <see cref="DataEntryPaginationDocumentData"/>.
        /// </summary>
        DataEntryPaginationDocumentData _documentData;

        /// <summary>
        /// Keeps track of the documents for which a <see cref="UpdateDocumentStatus"/> call is in
        /// progress on a background thread.
        /// </summary>
        ConcurrentDictionary<PaginationDocumentData, int> _pendingDocumentStatusUpdate =
            new ConcurrentDictionary<PaginationDocumentData, int>();

        /// <summary>
        /// Limits the number of threads that can run concurrently for <see cref="UpdateDocumentStatus"/> calls.
        /// (1 - 3 where the number does no exceed the num of CPUs - 1).
        /// </summary>
        Semaphore _documentStatusUpdateSemaphore = new Semaphore(
            Math.Max(1, Math.Min(3, Environment.ProcessorCount - 1)),
            Math.Max(1, Math.Min(3, Environment.ProcessorCount - 1)));

        /// <summary>
        /// The <see cref="ITagUtility"/> to expand path tags/functions.
        /// </summary>
        ITagUtility _tagUtility;

        /// <summary>
        /// The expanded filename of the config file for the data entry configuration.
        /// </summary>
        string _expandedConfigFileName;

        /// <summary>
        /// The <see cref="IDataEntryApplication"/> for which this instance is being used.
        /// </summary>
        IDataEntryApplication _dataEntryApp;

        /// <summary>
        /// The <see cref="ImageViewer"/> to use.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Indicates whether the PageLayoutControl's PrimarySelection corresponds with the output
        /// document for which this DEP is editing data.
        /// </summary>
        bool _primaryPageIsForActiveDocument;

        /// <summary>
        /// Set during LoadData so that configuration changes during load can be distinguished from
        /// other configuration changes, e.g., when a different doctype is selected by the user.
        /// </summary>
        bool _loading;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPanelContainer" /> class.
        /// </summary>
        /// <param name="configFileName">The name of the config file defining the data entry
        /// configuration.</param>
        /// <param name="dataEntryApp">The <see cref="IDataEntryApplication"/> for which this
        /// instance is being used.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> to expand path tags/functions.
        /// </param>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> to use.</param>
        public DataEntryPanelContainer(string configFileName, IDataEntryApplication dataEntryApp,
            ITagUtility tagUtility, ImageViewer imageViewer)
            : this(configFileName, dataEntryApp, tagUtility, imageViewer, null)
        {
            // Thread manager can't be passed into this() above since "this" doesn't exist at that
            // point.
            StatusUpdateThreadManager = new ThreadManager(this);
        }

        /// <summary>
        /// Private initialization of a new instance of the <see cref="DataEntryPanelContainer" /> class.
        /// Can be used to create an instance with a shared <see cref="ThreadManager"/>.
        /// </summary>
        /// <param name="configFileName">The name of the config file defining the data entry
        /// configuration.</param>
        /// <param name="dataEntryApp">The <see cref="IDataEntryApplication"/> for which this
        /// instance is being used.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> to expand path tags/functions.
        /// </param>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> to use.</param>
        /// <param name="threadManager">The <see cref="ThreadManager"/> to use to manage
        /// <see cref="UpdateDocumentStatus"/> threads.</param>
        DataEntryPanelContainer(string configFileName, IDataEntryApplication dataEntryApp,
            ITagUtility tagUtility, ImageViewer imageViewer, ThreadManager threadManager)
        {
            try
            {
                InitializeComponent();

                StatusUpdateThreadManager = threadManager;

                _expandedConfigFileName = tagUtility.ExpandTagsAndFunctions(configFileName, null, null);
                _dataEntryApp = dataEntryApp;
                _tagUtility = tagUtility;
                _imageViewer = imageViewer;

                // Initialize the root directory the DataEntry framework should use when resolving
                // relative paths.
                DataEntryMethods.SolutionRootDirectory = Path.GetDirectoryName(_expandedConfigFileName);

                var configSettings = new ConfigSettings<Properties.Settings>(_expandedConfigFileName, false, false);

                _configManager = new DataEntryConfigurationManager<Properties.Settings>(
                    dataEntryApp, tagUtility, configSettings, imageViewer, _documentTypeComboBox);

                _configManager.ConfigurationChanged += HandleConfigManager_ConfigurationChanged;
                _configManager.LoadDataEntryConfigurations(_expandedConfigFileName);

                // Hide the _documentTypePanel if there are no RegisteredDocumentTypes that allow for
                // doc type specific DEP configurations.
                if (!_configManager.RegisteredDocumentTypes.Any())
                {
                    _documentTypeLabel.Visible = false;
                    _documentTypeComboBox.Visible = false;
                    // To hide column that had _documentTypeComboBox.
                    _tableLayoutPanel.ColumnStyles[1].SizeType = SizeType.AutoSize;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41598");
            }
        }

        #endregion Constructors

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
        /// Gets the current "active" data entry. This is the last data entry control to have
        /// received input focus (but doesn't necessarily mean the control currently has input
        /// focus).
        /// </summary>
        public Control ActiveDataControl
        {
            get
            {
                return ActiveDataEntryPanel.ActiveDataControl as Control;
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
        public bool PrimaryPageIsForActiveDocument
        {
            get
            {
                return _primaryPageIsForActiveDocument;
            }

            set
            {
                _primaryPageIsForActiveDocument = value;
                if (ActiveDataEntryPanel != null)
                {
                    ActiveDataEntryPanel.PrimarySelectionIsForActiveDocument = value;
                }
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="data" />.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <param name="forDisplay"><c>true</c> if the loaded data is to be displayed; <c>false</c>
        /// if the data is being loaded only for data manipulation or validation.</param>
        public void LoadData(PaginationDocumentData data, bool forDisplay)
        {
            try
            {
                _loading = true;

                // If this data is getting loaded, there is no need to proceed with any pending
                // document status update.
                int temp;
                _pendingDocumentStatusUpdate.TryRemove(data, out temp);

                _documentData = (DataEntryPaginationDocumentData)data;
                _configManager.LoadCorrectConfigForData(_documentData.WorkingAttributes);

                ActiveDataEntryPanel.LoadData(data, forDisplay);

                _documentTypeComboBox.Enabled = true;
                _documentTypeComboBox.SelectedIndexChanged += HandleDocumentTypeComboBox_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41599");
            }
            finally
            {
                _loading = false;
            }
        }

        /// <summary>
        /// Applies any data to the specified <see paramref="data" />.
        /// <para><b>Note</b></para>
        /// In addition to returning <see langword="false" />, it is the implementor's responsibility
        /// to notify the user of any problems with the data that needs to be corrected before it
        /// can be saved.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="validateData"><see langword="true" /> if the <see paramref="data" /> should
        /// be validated for errors when saving; otherwise, <see langwor="false" />.</param>
        /// <returns>
        /// <see langword="true" /> if the data was saved correctly or
        /// <see langword="false" /> if corrections are needed before it can be saved.
        /// </returns>
        public bool SaveData(PaginationDocumentData data, bool validateData)
        {
            try
            {
                return ActiveDataEntryPanel.SaveData(data, validateData);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41600");
            }
        }

        /// <summary>
        /// Clears the state of all data associated with the previously loaded document.
        /// </summary>
        public void ClearData()
        {
            try
            {
                _documentTypeComboBox.Enabled = false;
                if (_documentData != null)
                {
                    _documentTypeComboBox.SelectedIndexChanged -= HandleDocumentTypeComboBox_SelectedIndexChanged;
                }

                _documentData = null;

                ActiveDataEntryPanel.ClearData();

                _undoButton.Enabled = false;
                _redoButton.Enabled = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41601");
            }
        }

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData" /> instance based on the provided
        /// <see paramref="attributes" />.
        /// </summary>
        /// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData" />
        /// instance is needed.</param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="imageViewer"></param>
        /// <returns>
        /// The <see cref="PaginationDocumentData" /> instance.
        /// </returns>
        public PaginationDocumentData GetDocumentData(IUnknownVector attributes, string sourceDocName, FileProcessingDB fileProcessingDB, ImageViewer imageViewer)
        {
            try
            {
                var documentData = ActiveDataEntryPanel.GetDocumentData(attributes, sourceDocName);
                UpdateDocumentStatus(documentData);

                return documentData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41602");
            }
        }

        /// <summary>
        /// Updates the document data status.
        /// </summary>
        /// <param name="data"></param>
        public void UpdateDocumentDataStatus(PaginationDocumentData data)
        {
            try
            {
                var dataEntryData = data as DataEntryPaginationDocumentData;
                UpdateDocumentStatus(dataEntryData);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41603");
            }
        }

        /// <summary>
        /// Provides a message to be displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ShowMessage(string message)
        {
            // Not supported
        }

        /// <summary>
        /// Performs an undo operation.
        /// </summary>
        public void Undo()
        {
            try
            {
                ActiveDataEntryPanel.Undo();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41604");
            }
        }

        /// <summary>
        /// Performs a redo operation.
        /// </summary>
        public void Redo()
        {
            try
            {
                ActiveDataEntryPanel.Redo();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41605");
            }
        }

        /// <summary>
        /// Toggles whether or not tooltip(s) for the active fields are currently visible.
        /// </summary>
        public void ToggleHideTooltips()
        {
            try
            {
                ActiveDataEntryPanel.ToggleHideTooltips();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41606");
            }
        }

        /// <summary>
        /// Refreshes the state of the control.
        /// </summary>
        public void RefreshControlState()
        {
            try
            {
                if (ActiveDataEntryPanel != null)
                {
                    ActiveDataEntryPanel.UpdateSwipingState();
                }
                else
                {
                    _imageViewer.AllowHighlight = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41666");
            }
        }

        #endregion IPaginationDocumentDataPanel

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // If an ActiveDataEntryPanel is assigned prior to the form being loaded, it will
                // not be sized correctly without a layout call here.
                if (ActiveDataEntryPanel != null)
                {
                    PerformLayout();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41630");
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
                if (StatusUpdateThreadManager != null && StatusUpdateThreadManager.Owner == this)
                {
                    // Will wait up to 10 seconds for any document status threads to finish
                    StatusUpdateThreadManager.Dispose();
                    StatusUpdateThreadManager = null;
                }

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_configManager != null)
                {
                    _configManager.Dispose();
                    _configManager = null;
                }

                if (_documentStatusUpdateSemaphore != null)
                {
                    _documentStatusUpdateSemaphore.Dispose();
                    _documentStatusUpdateSemaphore = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the ConfigurationChanged event of the _configManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ConfigurationChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandleConfigManager_ConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            var oldDatEntryControlHost = e.OldDataEntryConfiguration?.DataEntryControlHost as DataEntryDocumentDataPanel;
            var newDataEntryControlHost = e.NewDataEntryConfiguration?.DataEntryControlHost as DataEntryDocumentDataPanel;

            if (oldDatEntryControlHost != newDataEntryControlHost)
            {
                if (oldDatEntryControlHost != null)
                {
                    // Set Active = false for the old DEP so that it no longer tracks image
                    // viewer events.
                    oldDatEntryControlHost.Active = false;

                    oldDatEntryControlHost.PageLoadRequest -= DataEntryControlHost_PageLoadRequest;
                    oldDatEntryControlHost.UndoAvailabilityChanged -= DataEntryControlHost_UndoAvailabilityChanged;
                    oldDatEntryControlHost.RedoAvailabilityChanged -= NewDataEntryControlHost_RedoAvailabilityChanged;

                    oldDatEntryControlHost.ClearData();

                    // Don't preserve undo state between DEPs, but do if the change is during data loading,
                    // e.g., switching between documents
                    // https://extract.atlassian.net/browse/ISSUE-14335
                    if (!_loading)
                    {
                        _documentData.UndoState = null;
                    }
                }

                if (newDataEntryControlHost != null)
                {
                    // Load the panel into the _scrollPane
                    LoadDataEntryControlHostPanel();

                    if (_documentData != null)
                    {
                        newDataEntryControlHost.PrimarySelectionIsForActiveDocument = _primaryPageIsForActiveDocument;
                        if (_documentData.Modified)
                        {
                            // If the DEP is being swapped for a new document type, the panel will
                            // have the modification history wiped and will no longer be able to
                            // track if it is dirty. Therefore, consider it permanently dirty
                            // (cleared only via revert).
                            // NOTE: The doc type change that triggered the panel change will have
                            // marked the data dirty; it is the load where the dirty status would
                            // be cleared. The modified check here prevents a freshly loaded document
                            // from being marked permanently dirty.
                            _documentData.SetPermanentlyModified();
                        }
                        newDataEntryControlHost.LoadData(_documentData, forDisplay: true);

                        _undoButton.Enabled = UndoOperationAvailable;
                        _redoButton.Enabled = RedoOperationAvailable;
                    }

                    newDataEntryControlHost.PageLoadRequest += DataEntryControlHost_PageLoadRequest;
                    newDataEntryControlHost.UndoAvailabilityChanged += DataEntryControlHost_UndoAvailabilityChanged;
                    newDataEntryControlHost.RedoAvailabilityChanged += NewDataEntryControlHost_RedoAvailabilityChanged;

                    // Set Active = true for the new DEP so that it tracks image viewer events.
                    newDataEntryControlHost.Active = true;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the
        /// <see cref="_documentTypeComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleDocumentTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_documentData != null)
                {
                    _documentData.SetModified(AttributeStatusInfo.UndoManager.UndoOperationAvailable);
                    _documentData.SetDataError(ActiveDataEntryPanel.DataValidity == DataValidity.Invalid);
                    _documentData.SetSummary(ActiveDataEntryPanel.SummaryDataEntryQuery?.Evaluate().ToString());
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41607");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataEntryControlHost.PageLoadRequest"/> event of the
        /// <see cref="ActiveDataEntryPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PageLoadRequestEventArgs"/> instance containing the event data.</param>
        void DataEntryControlHost_PageLoadRequest(object sender, PageLoadRequestEventArgs e)
        {
            OnPageLoadRequest(e);
        }

        /// <summary>
        /// Handles the <see cref="DataEntryControlHost.UndoAvailabilityChanged"/> event of the
        /// <see cref="ActiveDataEntryPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void DataEntryControlHost_UndoAvailabilityChanged(object sender, EventArgs e)
        {
            _undoButton.Enabled = UndoOperationAvailable;

            OnUndoAvailabilityChanged();
        }

        /// <summary>
        /// Handles the <see cref="DataEntryControlHost.RedoAvailabilityChanged"/> event of the
        /// <see cref="ActiveDataEntryPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void NewDataEntryControlHost_RedoAvailabilityChanged(object sender, EventArgs e)
        {
            _redoButton.Enabled = RedoOperationAvailable;

            OnRedoAvailabilityChanged();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_undoButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleUndoButton_Click(object sender, EventArgs e)
        {
            try
            {
                Undo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41675");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_undoButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleRedoButton_Click(object sender, EventArgs e)
        {
            try
            {
                Redo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41676");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the active data entry panel.
        /// </summary>
        /// <value>
        /// The active data entry panel.
        /// </value>
        DataEntryDocumentDataPanel ActiveDataEntryPanel
        {
            get
            {
                return _configManager
                    ?.ActiveDataEntryConfiguration
                    ?.DataEntryControlHost
                    as DataEntryDocumentDataPanel;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ThreadManager"/> to use to manage
        /// <see cref="UpdateDocumentStatus"/> threads.
        /// </summary>
        ThreadManager StatusUpdateThreadManager
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the DEP into the left-hand panel or separate window and positions and sizes it
        /// correctly.
        /// </summary>
        void LoadDataEntryControlHostPanel()
        {
            ActiveDataEntryPanel?.SetImageViewer(_imageViewer);

            try
            {
                _scrollPanel.SuspendLayout();

                MinimumSize = new Size(0, 0);
                Size = new Size(_scrollPanel.Width, 0);

                if (ActiveDataEntryPanel == null && _scrollPanel.Controls.Count > 0)
                {
                    _scrollPanel.Controls.Clear();
                }
                else if (ActiveDataEntryPanel != null &&
                        !_scrollPanel.Controls.Contains(ActiveDataEntryPanel))
                {
                    if (_scrollPanel.Controls.Count > 0)
                    {
                        _scrollPanel.Controls.Clear();
                    }

                    // Add the DEP to an auto-scroll pane to allow scrolling if the DEP is too
                    // long. (The scroll pane is sized to allow the full width of the DEP to 
                    // display initially) 
                    _scrollPanel.Controls.Add(ActiveDataEntryPanel);
                    ActiveDataEntryPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                    MinimumSize = new Size(
                        ActiveDataEntryPanel.MinimumSize.Width,
                        ActiveDataEntryPanel.Height + _documentTypePanel.Height);
                }
            }
            finally
            {
                _scrollPanel.ResumeLayout(true);
                // I don't understand why, but if dock or any other sizing is set prior to the
                // initial layout call, ActiveDataEntryPanel ends up wider than _scrollPanel.
                ActiveDataEntryPanel.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// Updates the <see cref="Summary"/>, <see cref="DataModified"/> and  <see cref="DataError"/>
        /// properties of <see paramref="documentData"/> by loading the data into a background panel.
        /// </summary>
        /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData"/> for which
        /// document status should be updated.</param>
        void UpdateDocumentStatus(DataEntryPaginationDocumentData documentData)
        {
            string serializedAttributes = _miscUtils.GetObjectAsStringizedByteStream(documentData.WorkingAttributes);

            if (!_pendingDocumentStatusUpdate.TryAdd(documentData, 0))
            {
                // Document status is already being updated.
                return;
            }

            var thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    UpdateDocumentStatusThread(documentData, serializedAttributes);
                }
                catch (Exception ex)
                {
                    // Exceptions will be thrown if the FAM task has been stopped while a
                    // UpdateDocumentStatusThread was still running, but we don't care to know about
                    // the exceptions in this case.
                    if (IsHandleCreated)
                    {
                        ex.ExtractLog("ELI41494");
                    }
                }
                finally
                {
                    AttributeStatusInfo.DisposeThread();

                    // https://extract.atlassian.net/browse/ISSUE-14246
                    // Without this call the Application.ThreadContext may hold references to
                    // controls created in this thread not unlike the situation described here:
                    // http://blogs.msmvps.com/senthil/2008/05/29/the-case-of-the-leaking-thread-handles/
                    Application.ExitThread();
                }
            }));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Code running in a background thread in support of <see cref="UpdateDocumentStatus"/>
        /// </summary>
        /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData"/> for which
        /// data validity should be checked.</param>
        void UpdateDocumentStatusThread(DataEntryPaginationDocumentData documentData, string serializedAttributes)
        {
            bool registeredThread = false;
            ExtractException ee = null;
            bool dataModified = false;
            bool dataError = false;
            bool? reprocess = false;
            string summary = null;

            try
            {
                if (StatusUpdateThreadManager != null)
                {
                    registeredThread = StatusUpdateThreadManager.TryRegisterThread();
                    if (!registeredThread)
                    {
                        throw new ExtractException("ELI41655",
                            "Failed to register document status update thread.");
                    }
                }

                // If the document was loaded by the time this thread was spun up or the form is disposing, abort.
                if (!_pendingDocumentStatusUpdate.ContainsKey(documentData) ||
                    (StatusUpdateThreadManager != null && StatusUpdateThreadManager.StoppingThreads))
                {
                    return;
                }

                _documentStatusUpdateSemaphore.WaitOne();

                // If by the time this thread has a chance to update, the document was loaded, abort.
                if (!_pendingDocumentStatusUpdate.ContainsKey(documentData) ||
                    (StatusUpdateThreadManager != null && StatusUpdateThreadManager.StoppingThreads))
                {
                    return;
                }

                var miscUtils = new MiscUtils();
                var deserializedAttributes = (IUnknownVector)miscUtils.GetObjectFromStringizedByteStream(serializedAttributes);

                using (var form = new InvisibleForm())
                using (var imageViewer = new ImageViewer())
                using (var tempPanel = new DataEntryPanelContainer(
                    _expandedConfigFileName, _dataEntryApp, _tagUtility, imageViewer, StatusUpdateThreadManager))
                {
                    form.Show();
                    form.Controls.Add(tempPanel);
                    form.Controls.Add(imageViewer);

                    using (var tempData = new DataEntryPaginationDocumentData(deserializedAttributes, documentData.SourceDocName))
                    {
                        tempPanel.LoadData(tempData, forDisplay: false);
                        dataModified = tempPanel.UndoOperationAvailable;
                        reprocess = tempData.SendForReprocessing;
                        if (!tempPanel.ActiveDataEntryPanel.Config.Settings.PerformanceTesting)
                        {
                            dataError = (tempPanel.ActiveDataEntryPanel.DataValidity == DataValidity.Invalid);
                        }
                        summary = tempPanel.ActiveDataEntryPanel.SummaryDataEntryQuery?.Evaluate().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ee = ex.AsExtract("ELI41453");
            }
            finally
            {
                _documentStatusUpdateSemaphore.Release();
                if (registeredThread)
                {
                    StatusUpdateThreadManager.SignalThreadEnded();
                }
            }

            if (StatusUpdateThreadManager != null && StatusUpdateThreadManager.StoppingThreads)
            {
                return;
            }

            _imageViewer.SafeBeginInvoke("ELI41465", () =>
            {
                try
                {
                    if (ee != null)
                    {
                        ee.ExtractDisplay("ELI41464");
                    }

                    if (_pendingDocumentStatusUpdate.ContainsKey(documentData))
                    {
                        documentData.SetModified(dataModified);
                        documentData.SetDataError(dataError);
                        documentData.SetSummary(summary);
                        documentData.SetSendForReprocessing(reprocess);
                        documentData.SetInitialized();
                    }
                }
                finally
                {
                    int temp;
                    _pendingDocumentStatusUpdate.TryRemove(documentData, out temp);
                }
            });
        }

        /// <summary>
        /// Raises the <see cref="PageLoadRequest" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PageLoadRequestEventArgs"/> instance containing the event data.</param>
        void OnPageLoadRequest(PageLoadRequestEventArgs e)
        {
            PageLoadRequest?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="UndoAvailabilityChanged"/> event.
        /// </summary>
        void OnUndoAvailabilityChanged()
        {
            UndoAvailabilityChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="RedoAvailabilityChanged"/> event.
        /// </summary>
        void OnRedoAvailabilityChanged()
        {
            RedoAvailabilityChanged?.Invoke(this, new EventArgs());
        }

        #endregion Private Members
    }
}
