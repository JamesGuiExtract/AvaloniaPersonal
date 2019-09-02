using Extract.DataEntry;
using Extract.DataEntry.LabDE;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_AFCORELib;
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
        /// <summary>
        /// Class to attach <see cref="PaginationPanel"/> specific properties to a
        /// <see cref="DataEntryConfiguration"/>.
        /// </summary>
        class PaginationCustomSettings
        {
            /// <summary>
            /// The data entry query text used to generate a summary for the document.
            /// </summary>
            public string SummaryQuery;

            /// <summary>
            /// Function that when set is to be used so that the panel can specify whether a
            /// document is to be sent for rules reprocessing.
            /// </summary>
            public Func<DataEntryPaginationDocumentData, bool?> SendForReprocessingFunc;

            /// <summary>
            /// The the data entry query text that should be used to identify any order numbers in the
            /// file to be recorded in the LabDEOrderFile table.
            /// </summary>
            public string OrderNumberQuery;

            /// <summary>
            /// The data entry query text that should be used to identify the date for each order.
            /// Any attribute queries should be relative to an order number attribute.
            /// </summary>
            public string OrderDateQuery;

            /// <summary>
            /// Gets whether to prompt about order numbers for which a document has already been filed.
            /// </summary>
            public bool PromptForDuplicateOrders;

            /// <summary>
            /// Gets the data entry query text that should be used to identify any encounter numbers in the
            /// file to be recorded in the LabDEOrderFile table.
            /// </summary>
            public string EncounterNumberQuery;

            /// <summary>
            /// Gets the data entry query text that should be used to identify the date for each encounter.
            /// Any attribute queries should be relative to an encoutner number attribute.
            /// </summary>
            public string EncounterDateQuery;

            /// <summary>
            /// Gets whether to prompt about encounter numbers for which a document has already been filed.
            /// </summary>
            public bool PromptForDuplicateEncounters;
        }

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
        /// Keeps track of the documents for which a <see cref="StartUpdateDocumentStatus"/> call is in
        /// progress on a background thread.
        /// </summary>
        ConcurrentDictionary<PaginationDocumentData, int> _pendingDocumentStatusUpdate =
            new ConcurrentDictionary<PaginationDocumentData, int>();

        /// <summary>
        /// Limits the number of threads that can run concurrently for <see cref="StartUpdateDocumentStatus"/> calls.
        /// I recently changed this from 4 to 10 because threads tend to get tied up in locking for cache access
        /// in SQLQueryNodes which means it doesn't get anywhere near full CPU utilization of the threads here.
        /// The only reason I hesitate to go higher is potential memory usage in complex DEPs (esp that use
        /// legacy UI loading).
        /// </summary>
        Semaphore _documentStatusUpdateSemaphore = new Semaphore(10, 10);

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

        /// <summary>
        /// The document statuses updated
        /// </summary>
        ManualResetEvent _documentStatusesUpdated = new ManualResetEvent(true);

        /// <summary>
        /// Whether this panel's data should be editable (!read-only).
        /// </summary>
        bool _editable = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPanelContainer" /> class to be
        /// used as the foreground container.
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
        {
            try
            {
                InitializeComponent();

                StatusUpdateThreadManager = new ThreadManager(this);

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

                _configManager.ConfigurationInitialized += HandleConfigManager_ConfigurationInitialized;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPanelContainer"/> class to be used
        /// for background loading in <see cref="UpdateDocumentStatusThread"/>.
        /// </summary>
        /// <param name="configManager">The <see cref="DataEntryConfigurationManager{T}"/> to be
        /// used to load documents in the background.
        /// configuration.</param>
        /// <param name="dataEntryApp">The <see cref="IDataEntryApplication"/> for which this
        /// instance is being used.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> to expand path tags/functions.
        /// </param>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> to use.</param>
        /// <param name="threadManager">The <see cref="ThreadManager"/> to use to manage
        /// <see cref="StartUpdateDocumentStatus"/> threads.</param>
        DataEntryPanelContainer(DataEntryConfigurationManager<Properties.Settings> configManager,
            IDataEntryApplication dataEntryApp, ITagUtility tagUtility, ImageViewer imageViewer,
            ThreadManager threadManager)
        {
            try
            {
                InitializeComponent();

                StatusUpdateThreadManager = threadManager;

                _dataEntryApp = dataEntryApp;
                _tagUtility = tagUtility;
                _imageViewer = imageViewer;

                _configManager = configManager;
                _configManager.ConfigurationInitialized += HandleConfigManager_ConfigurationInitialized;

                bool loadRequiresUI = false;

                // When initializing a background manager, the configurations will already have been
                // initialized. Instead of handling ConfigurationInitialized, immediately assign
                // CustomBackgroundLoadSettings.
                foreach (var config in configManager.Configurations)
                {
                    if (!config.Config.Settings.SupportsNoUILoad)
                    {
                        loadRequiresUI = true;
                    }

                    var paginationPanel = config.DataEntryControlHost as DataEntryDocumentDataPanel;
                    if (paginationPanel != null)
                    {
                        config.CustomBackgroundLoadSettings = new PaginationCustomSettings
                        {
                            SummaryQuery = paginationPanel.SummaryQuery,
                            SendForReprocessingFunc = paginationPanel.SendForReprocessingFunc,
                            OrderNumberQuery = paginationPanel.OrderNumberQuery,
                            OrderDateQuery = paginationPanel.OrderDateQuery,
                            PromptForDuplicateOrders = paginationPanel.PromptForDuplicateOrders,
                            EncounterNumberQuery = paginationPanel.EncounterNumberQuery,
                            EncounterDateQuery = paginationPanel.EncounterDateQuery,
                            PromptForDuplicateEncounters = paginationPanel.PromptForDuplicateEncounters
                        };
                    }
                }

                if (loadRequiresUI)
                {
                    LoadDataEntryControlHostPanel();
                }
                _configManager.ConfigurationChanged += HandleConfigManager_ConfigurationChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45544");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when operations are applied via the duplicate document window.
        /// </summary>
        public event EventHandler<DuplicateDocumentsAppliedEventArgs> DuplicateDocumentsApplied;

        #endregion Events

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
        /// Indicates that the displayed panel has been changed such as for document type specific
        /// panels when the document type field changes.
        /// </summary>
        public event EventHandler<EventArgs> DataPanelChanged;

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
        /// Gets or sets whether this panel should be editable (!read-only).
        /// </summary>
        public bool Editable
        {
            get
            {
                return _editable;
            }

            set
            {
                try
                {
                    if (value != _editable)
                    {
                        _editable = value;

                        Enabled = Editable;

                        if (ActiveDataEntryPanel != null)
                        {
                            ActiveDataEntryPanel.Active = Editable;
                            ActiveDataEntryPanel.ShowValidationIcons = Editable;
                            AttributeStatusInfo.DisableAutoUpdateQueries = !Editable;
                            AttributeStatusInfo.DisableValidationQueries = !Editable;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47279");
                }
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="data" />.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <param name="forEditing"><c>true</c> if the loaded data is to be displayed for editing;
        /// <c>false</c> if the data is to be displayed read-only, or if it is being used for
        /// background formatting.</param>
        public void LoadData(PaginationDocumentData data, bool forEditing)
        {
            try
            {
                _loading = true;

                // If this data is getting loaded, there is no need to proceed with any pending
                // document status update.
                _pendingDocumentStatusUpdate.TryRemove(data, out int _);

                // Return quickly if this thread is being stopped
                if (AttributeStatusInfo.ThreadEnding)
                {
                    return;
                }

                _documentData = (DataEntryPaginationDocumentData)data;
                _configManager.LoadCorrectConfigForData(_documentData.WorkingAttributes);
                _configManager.ActiveDataEntryConfiguration.OpenDatabaseConnections();

                ActiveDataEntryPanel.LoadData(data, forEditing);

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
        /// <param name="validateData"><see langword="true"/> if the <see paramref="data" /> should
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
                if (_documentData != null)
                {
                    _documentTypeComboBox.SelectedIndexChanged -= HandleDocumentTypeComboBox_SelectedIndexChanged;
                }
                _documentData = null;

                ActiveDataEntryPanel.ClearData();

                // https://extract.atlassian.net/browse/ISSUE-15351
                // Changing the enabled status triggers the DEP to gain focus and to try to display
                // highlights. Prevent it from doing so by not changing document status until the data
                // has been cleared.
                _documentTypeComboBox.Enabled = false;

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
                StartUpdateDocumentStatus(documentData, statusOnly: true, applyUpdateToUI: true, displayValidationErrors: false);

                return documentData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41602");
            }
        }

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData" /> instance based on the provided
        /// <see paramref="attributes" />.
        /// </summary>
        /// <param name="documentDataAttribute">The VOA data for while a <see cref="PaginationDocumentData" />
        /// instance is needed including this top-level attribute which contains document data status info.
        /// </param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="imageViewer"></param>
        /// <returns>
        /// The <see cref="PaginationDocumentData" /> instance.
        /// </returns>
        public PaginationDocumentData GetDocumentData(IAttribute documentDataAttribute, string sourceDocName,
            FileProcessingDB fileProcessingDB, ImageViewer imageViewer)
        {
            try
            {
                var documentData = ActiveDataEntryPanel.GetDocumentData(documentDataAttribute, sourceDocName);
                StartUpdateDocumentStatus(documentData, statusOnly: true, applyUpdateToUI: true, displayValidationErrors: false);

                return documentData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45983");
            }
        }

        /// <summary>
        /// Updates the document data.
        /// </summary>
        /// <param name="data">The <see cref="PaginationDocumentData"/> to be updated.
        /// </param>
        /// <param name="statusOnly"><c>true</c> to retrieve into <paramref name="data"/> only the high-level
        /// status such as the the summary string and other status flags; <c>false</c> to udpate
        /// <paramref name="data"/> with the complete voa data.</param>
        /// <param name="displayValidationErrors"><c>true</c> if you want to display validation errors;
        /// <c>false</c> if you do not want to display validation errors.</param>
        public void UpdateDocumentData(PaginationDocumentData data, bool statusOnly, bool displayValidationErrors)
        {
            try
            {
                var dataEntryData = data as DataEntryPaginationDocumentData;
                StartUpdateDocumentStatus(dataEntryData, statusOnly, applyUpdateToUI: true, displayValidationErrors: displayValidationErrors);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41603");
            }
        }

        /// <summary>
        /// Waits for all documents status updates (started via <see cref="UpdateDocumentData"/>)
        /// to complete.
        /// </summary>
        public void WaitForDocumentStatusUpdates()
        {
            try
            {
                while (!_documentStatusesUpdated.WaitOne(100))
                {
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45578");
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
                if (StatusUpdateThreadManager != null)
                {
                    if (StatusUpdateThreadManager.Owner == this)
                    {
                        _pendingDocumentStatusUpdate.Clear();

                        // Will wait up to 10 seconds for any document status threads to finish
                        StatusUpdateThreadManager.Dispose();
                        StatusUpdateThreadManager = null;
                    }
                    else
                    {
                        // Signal to avoid unneeded/unwanted processing still to occur in this instance.
                        AttributeStatusInfo.ThreadEnding = true;
                    }
                }

                if (!_configManager.IsBackgroundManager)
                {
                    AttributeStatusInfo.ClearProcessWideCache();
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

                if (_documentStatusesUpdated != null)
                {
                    _documentStatusesUpdated.Dispose();
                    _documentStatusesUpdated = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the ConfigurationInitialized event of the _configManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ConfigurationInitializedEventArgs"/> instance containing
        /// the event data.</param>
        void HandleConfigManager_ConfigurationInitialized(object sender, ConfigurationInitializedEventArgs e)
        {
            try
            {
                var paginationPanel = e.DataEntryConfiguration.DataEntryControlHost as DataEntryDocumentDataPanel;
                if (paginationPanel != null)
                {
                    e.DataEntryConfiguration.CustomBackgroundLoadSettings = new PaginationCustomSettings
                    {
                        SummaryQuery = paginationPanel.SummaryQuery,
                        SendForReprocessingFunc = paginationPanel.SendForReprocessingFunc,
                        OrderNumberQuery = paginationPanel.OrderNumberQuery,
                        OrderDateQuery = paginationPanel.OrderDateQuery,
                        PromptForDuplicateOrders = paginationPanel.PromptForDuplicateOrders,
                        EncounterNumberQuery = paginationPanel.EncounterNumberQuery,
                        EncounterDateQuery = paginationPanel.EncounterDateQuery,
                        PromptForDuplicateEncounters = paginationPanel.PromptForDuplicateEncounters
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45514");
            }
        }

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
                    oldDatEntryControlHost.RedoAvailabilityChanged -= DataEntryControlHost_RedoAvailabilityChanged;

                    oldDatEntryControlHost.ClearData();

                    // Don't preserve undo state between DEPs, but do if the change is during data loading,
                    // e.g., switching between documents
                    // https://extract.atlassian.net/browse/ISSUE-14335
                    if (!_loading)
                    {
                        _documentData.UndoState = null;

                        // After switching panels, the pagination task form's _undoManager may be out of sync with
                        // current undo availability. Force an update.
                        // https://extract.atlassian.net/browse/ISSUE-15317
                        OnUndoAvailabilityChanged();
                        OnRedoAvailabilityChanged();
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
                        newDataEntryControlHost.LoadData(_documentData, forEditing: Editable);

                        _undoButton.Enabled = UndoOperationAvailable;
                        _redoButton.Enabled = RedoOperationAvailable;
                    }

                    newDataEntryControlHost.PageLoadRequest += DataEntryControlHost_PageLoadRequest;
                    newDataEntryControlHost.UndoAvailabilityChanged += DataEntryControlHost_UndoAvailabilityChanged;
                    newDataEntryControlHost.RedoAvailabilityChanged += DataEntryControlHost_RedoAvailabilityChanged;
                    newDataEntryControlHost.DuplicateDocumentsApplied += HandleDataEntryControlHost_DuplicateDocumentsApplied;

                    // Set Active = true for the new DEP so that it tracks image viewer events.
                    newDataEntryControlHost.Active = Editable;
                    newDataEntryControlHost.ShowValidationIcons = Editable;
                }

                OnDataPanelChanged();
            }
        }

        /// <summary>
        /// Handles the DuplicateDocumentsApplied event of the active <see cref="DataEntryDocumentDataPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DuplicateDocumentsAppliedEventArgs"/> instance containing the event data.</param>
        void HandleDataEntryControlHost_DuplicateDocumentsApplied(object sender, DuplicateDocumentsAppliedEventArgs e)
        {
            try
            {
                DuplicateDocumentsApplied?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46497");
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
                    _documentData.SetSendForReprocessing(ActiveDataEntryPanel.SendForReprocessingFunc(_documentData));
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
        void DataEntryControlHost_RedoAvailabilityChanged(object sender, EventArgs e)
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

        /// <summary>
        /// Handles the <see cref="Control.MouseWheel"/> event of the <see cref="_documentTypeComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        void HandleDocumentTypeComboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                // Redirect mouse-scrolling for the document type combo box so it doesn't change
                // document type selection when the combo is collapsed.
                // https://extract.atlassian.net/browse/ISSUE-15318
                if (!_documentTypeComboBox.DroppedDown)
                {
                    // Prevent combo box selection from changing.
                    ((HandledMouseEventArgs)e).Handled = true;

                    // Redirect any scrolling to the _scrollPanel as that is what the user is
                    // likely intending to scroll (first event will be missed, but the subsequent
                    // scroll events will go there.
                    _scrollPanel.Focus();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45658");
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
        internal DataEntryDocumentDataPanel ActiveDataEntryPanel
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
        /// <see cref="StartUpdateDocumentStatus"/> threads.
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
            if (ActiveDataEntryPanel != null)
            {
                ActiveDataEntryPanel.SetImageViewer(_imageViewer);

                // The DataEntryApplication will already be set for the primary foreground
                // container, but it will need to be set here for background loading.
                ActiveDataEntryPanel.DataEntryApplication = _dataEntryApp;
            }

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
                if (ActiveDataEntryPanel != null)
                {
                    ActiveDataEntryPanel.Dock = DockStyle.Fill;
                }
            }
        }

        /// <summary>
        /// Triggers an update to the <see cref="Summary"/>, <see cref="DataModified"/> and
        /// <see cref="DataError"/> properties of <see paramref="documentData"/> by processing the
        /// data in a background thread. If the results of status updates are needed
        /// programmatically rather than to update the UI, <see cref="WaitForDocumentStatusUpdates"/>
        /// should be used before checking the updated status.
        /// </summary>
        /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData"/> for which
        /// document status should be updated.</param>
        /// <param name="statusOnly"><c>true</c> to retrieve into documentData only the high-level
        /// status such as the the summary string and other status flags; <c>false</c> to udpate
        /// documentData with the complete voa data.</param>
        /// <param name="applyUpdateToUI"><c>true</c> if the new status should be applied to the UI;
        /// <c>false</c> if the status is going to be used programmatically (without a UI).</param>
        /// <param name = "displayValidationErrors"><c>true</c> if you want to display validation errors;
        /// <c>false</c> if you do not want to display validation errors.</param>
        public void StartUpdateDocumentStatus(DataEntryPaginationDocumentData documentData,
            bool statusOnly, bool applyUpdateToUI, bool displayValidationErrors)
        {
            try
            {
                if (!_pendingDocumentStatusUpdate.TryAdd(documentData, 0))
                {
                    // Document status is already being updated.
                    return;
                }

                string serializedAttributes = _miscUtils.GetObjectAsStringizedByteStream(documentData.WorkingAttributes);

                var backgroundConfigManager = _configManager.CreateBackgroundManager();

                var thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        UpdateDocumentStatusThread(backgroundConfigManager, documentData,
                            serializedAttributes, statusOnly, applyUpdateToUI, displayValidationErrors);
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
                        backgroundConfigManager.Dispose();

                        AttributeStatusInfo.DisposeThread();

                        // https://extract.atlassian.net/browse/ISSUE-14246
                        // Without this call the Application.ThreadContext may hold references to
                        // controls created in this thread not unlike the situation described here:
                        // http://blogs.msmvps.com/senthil/2008/05/29/the-case-of-the-leaking-thread-handles/
                        Application.ExitThread();
                    }
                }));

                thread.SetApartmentState(ApartmentState.STA);

                _documentStatusesUpdated.Reset();

                thread.Start();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47172");
            }
        }

        /// <summary>
        /// Code running in a background thread in support of <see cref="StartUpdateDocumentStatus" />
        /// </summary>
        /// <param name="backgroundConfigManager">The manager to use to load the data.</param>
        /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData" /> for which
        /// data validity should be checked.</param>
        /// <param name="serializedAttributes">The serialized attributes that represent the document's current data.</param>
        /// <param name="statusOnly"><c>true</c> to retrieve into documentData only the high-level
        /// status such as the the summary string and other status flags; <c>false</c> to udpate
        /// documentData with the complete voa data.</param>
        /// <param name="applyUpdateToUI"><c>true</c> if the new status should be applied to the UI;
        /// <c>false</c> if the status is going to be used programmatically (without a UI).</param>
        /// <param name="displayValidationErrors"><c>true</c> if the data should be validated before being saved;
        /// <c>false</c> if the data should be saved even if not valid.</param>
        void UpdateDocumentStatusThread(DataEntryConfigurationManager<Properties.Settings> backgroundConfigManager,
            DataEntryPaginationDocumentData documentData, string serializedAttributes,
            bool statusOnly, bool applyUpdateToUI, bool displayValidationErrors)
        {
            bool registeredThread = false;
            bool gotSemaphore = false;

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

                // If StatusUpdateThreadManager signaled stop while waiting for the semaphore, abort.
                if (WaitHandle.WaitAny(new[] { _documentStatusUpdateSemaphore, StatusUpdateThreadManager.StopEvent }) == 1)
                {
                    return;
                }

                gotSemaphore = true;

                // If by the time this thread has a chance to update, the document was loaded, abort.
                if (!_pendingDocumentStatusUpdate.ContainsKey(documentData))
                {
                    return;
                }

                // Initialize the root directory the DataEntry framework should use when resolving
                // relative paths.
                DataEntryMethods.SolutionRootDirectory = Path.GetDirectoryName(_expandedConfigFileName);

                var miscUtils = new MiscUtils();
                var deserializedAttributes = (IUnknownVector)miscUtils.GetObjectFromStringizedByteStream(serializedAttributes);

                using (var tempData = new DataEntryPaginationDocumentData(deserializedAttributes, documentData.SourceDocName))
                {
                    AttributeStatusInfo.ProcessWideDataCache = true;
                    if (backgroundConfigManager.ExecuteNoUILoad(tempData.Attributes, documentData.SourceDocName))
                    {
                        documentData.PendingDocumentStatus =
                            GetDocumentStatusFromNoUILoad(backgroundConfigManager, tempData, statusOnly,
                                verboseWarningCheck: !applyUpdateToUI); // If not running in a UI (auto-pagination) rather
                                                                        // than focus on efficiency, focus on getting full
                                                                        // data for PaginationConditions
                    }
                    else
                    {
                        documentData.PendingDocumentStatus =
                            GetDocumentDataFromUILoad(backgroundConfigManager, tempData, statusOnly,
                                verboseWarningCheck: !applyUpdateToUI); // If not running in a UI (auto-pagination) rather
                                                                        // than focus on efficiency, focus on getting full
                                                                        // data for PaginationConditions
                    }
                }
            }
            catch (Exception ex)
            {
                documentData.PendingDocumentStatus = 
                    new DocumentStatus() { Exception = ex.CreateComVisible("ELI41453", "Failed to update document status") };
            }
            finally
            {
                if (registeredThread)
                {
                    StatusUpdateThreadManager.SignalThreadEnded();
                }

                if (gotSemaphore)
                {
                    _documentStatusUpdateSemaphore.Release();
                }

                backgroundConfigManager.Dispose();
            }

            if (StatusUpdateThreadManager != null && StatusUpdateThreadManager.StoppingThreads)
            {
                try
                {
                    _documentStatusesUpdated.Set();
                    _pendingDocumentStatusUpdate.Clear();
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI45579");
                }
                return;
            }

            if (applyUpdateToUI)
            {
                ExtractException.Assert("ELI47127",
                    "Unable to apply status update withou UI", _imageViewer != null);

                _imageViewer.SafeBeginInvoke("ELI41465", () =>
                {
                    try
                    {
                        if (documentData.PendingDocumentStatus?.Exception != null)
                        {
                            documentData.PendingDocumentStatus?.Exception.ExtractDisplay("ELI41464");
                        }

                        if (_pendingDocumentStatusUpdate.ContainsKey(documentData))
                        {
                            var dataError = documentData.ApplyPendingStatusUpdate(statusOnly);

                            if (dataError != null && displayValidationErrors)
                            {
                                // Reset to contain only this document to prevent any subsequent document data
                                // errors from being displayed. (If not fixed subsequent errors on the next
                                // commit attempt).
                                _pendingDocumentStatusUpdate.Clear();
                                _pendingDocumentStatusUpdate.TryAdd(documentData, 0);

                                dataError.Display();
                            }
                        }
                    }
                    finally
                    {
                        if (_pendingDocumentStatusUpdate.TryRemove(documentData, out int _)
                            && _pendingDocumentStatusUpdate.Count == 0)
                        {
                            // Once any active batch of status updates is complete, clear shared cache data.
                            AttributeStatusInfo.ClearProcessWideCache();

                            _documentStatusesUpdated.Set();
                        }
                    }
                });
            }
            else
            {
                if (_pendingDocumentStatusUpdate.TryRemove(documentData, out int _)
                    && _pendingDocumentStatusUpdate.Count == 0)
                {
                    // Once any active batch of status updates is complete, clear shared cache data.
                    AttributeStatusInfo.ClearProcessWideCache();

                    _documentStatusesUpdated.Set();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="DocumentStatus"/> for the <see paramref="documentData"/> which have
        /// been transformed by a no-UI load.
        /// </summary>
        /// <param name="backgroundConfigManager">The configuration manager to use.</param>
        /// <param name="documentData"><see cref="DataEntryPaginationDocumentData"/> representing
        /// the document's data.</param>
        /// <param name="statusOnly"><c>true</c> to retrieve only the high-level status such as the
        /// the summary string and other status flags; <c>false</c> to retrieve the complete voa data
        /// and exception details.</param>
        /// <param name="verboseWarningCheck"><c>true</c> to calculate check for warnings regardless of
        /// errors; <c>false</c> to check for warning only if there are no errors (generally faster).</param>
        /// <returns>The <see cref="DocumentStatus"/>.</returns>
        static DocumentStatus GetDocumentStatusFromNoUILoad(DataEntryConfigurationManager<Properties.Settings> backgroundConfigManager,
            DataEntryPaginationDocumentData documentData, bool statusOnly, bool verboseWarningCheck)
        {
            var documentStatus = new DocumentStatus();

            IAttribute invalidAttribute = null;
            if (!backgroundConfigManager.ActiveDataEntryConfiguration.Config.Settings.PerformanceTesting)
            {
                invalidAttribute = AttributeStatusInfo.FindNextAttributeByValidity(
                    documentData.Attributes, DataValidity.Invalid, null, true, false)
                        ?.LastOrDefault();

                documentStatus.DataError = invalidAttribute != null;

                if (verboseWarningCheck || !documentStatus.DataError)
                {
                    var warningAttribute = AttributeStatusInfo.FindNextAttributeByValidity(
                        documentData.Attributes, DataValidity.ValidationWarning, null, true, false)
                            ?.LastOrDefault();

                    documentStatus.DataWarning = warningAttribute != null;
                }
            }

            var customData = backgroundConfigManager.ActiveDataEntryConfiguration.CustomBackgroundLoadSettings as PaginationCustomSettings;

            if (statusOnly)
            {
                documentStatus.DataModified = AttributeStatusInfo.UndoManager.UndoOperationAvailable;
                if (!string.IsNullOrWhiteSpace(customData?.SummaryQuery))
                {
                    var query = DataEntryQuery.Create(
                        customData?.SummaryQuery,
                        null,
                        backgroundConfigManager.ActiveDataEntryConfiguration.GetDatabaseConnections());
                    documentStatus.Summary = query.Evaluate().ToString();
                }
                documentStatus.Reprocess = customData?.SendForReprocessingFunc?.Invoke(documentData);
            }
            else
            {
                var dbConnections = backgroundConfigManager.ActiveDataEntryConfiguration.GetDatabaseConnections();
                documentStatus.Orders = QueryRecordNumbers(
                    customData?.OrderNumberQuery, customData?.OrderDateQuery, dbConnections);
                documentStatus.PromptForDuplicateOrders = customData.PromptForDuplicateOrders;
                documentStatus.Encounters = QueryRecordNumbers(
                    customData?.EncounterNumberQuery, customData?.EncounterDateQuery, dbConnections);
                documentStatus.PromptForDuplicateEncounters = customData.PromptForDuplicateEncounters;

                var miscUtils = new MiscUtils();
                documentStatus.StringizedData = miscUtils.GetObjectAsStringizedByteStream(documentData.Attributes);

                if (documentStatus.DataError)
                {
                    try
                    {
                        AttributeStatusInfo.Validate(invalidAttribute, true);
                    }
                    catch (Exception ex)
                    {
                        var ee = ex.AsExtract("ELI45580");
                        documentStatus.StringizedError = ee.AsStringizedByteStream();
                    }

                    return documentStatus;
                }
            }

            return documentStatus;
        }

        /// <summary>
        /// Queries for the order/encounter number for the document data currently initialized into
        /// AttributeStatusInfo.
        /// </summary>
        /// <param name="recordNumQuery">The data entry query text to select the order/encounter number</param>
        /// <param name="dateQuery">The data entry query text to select the date associated with any order/encounter number.</param>
        /// <param name="dbConnections">The database connections to use for the query.</param>
        /// <returns></returns>
        static ReadOnlyCollection<(string, DateTime?)> QueryRecordNumbers(string recordNumQuery, string dateQuery,
            Dictionary<string, System.Data.Common.DbConnection> dbConnections)
        {
            ReadOnlyCollection<(string, DateTime?)> recordCollection = null;

            if (!string.IsNullOrWhiteSpace(recordNumQuery))
            {
                var query = DataEntryQuery.Create(recordNumQuery, null, dbConnections);
                var recordResults = query.Evaluate();
                // If no date query, return just the record numbers.
                if (string.IsNullOrWhiteSpace(dateQuery))
                {
                    recordCollection = recordResults.ToStringArray()
                        .Select(result => (result, new DateTime?()))
                        .ToList()
                        .AsReadOnly();
                }
                // If a date query is specified, query for the date associated with each record number.
                else
                {
                    var list = new List<(string, DateTime?)>();
                    foreach (var result in recordResults)
                    {
                        var date = new DateTime?();
                        query = DataEntryQuery.Create(dateQuery, result.IsAttribute ? result.FirstAttribute : null, dbConnections);
                        var dateResult = query.Evaluate();
                        if (!dateResult.IsEmpty)
                        {
                            date = DateTime.Parse(dateResult.ToString(), CultureInfo.CurrentCulture);
                        }
                        list.Add((result.ToString(), date));
                    }
                    recordCollection = list.AsReadOnly();
                }
            }

            return recordCollection;
        }

        /// <summary>
        /// Gets the <see cref="DocumentStatus"/> for the <see paramref="documentData"/> by loading
        /// it into DEP controls.
        /// </summary>
        /// <param name="backgroundConfigManager">The configuration manager to use.</param>
        /// <param name="documentData"><see cref="DataEntryPaginationDocumentData"/> representing
        /// the document's data.</param>
        /// <param name="statusOnly"><c>true</c> to retrieve into documentData only the high-level
        /// status such as the the summary string and other status flags; <c>false</c> to udpate
        /// documentData with the complete voa data.</param>
        /// <param name="verboseWarningCheck"><c>true</c> to calculate check for warnings regardless of
        /// errors; <c>false</c> to check for warning only if there are no errors (generally faster).</param>
        /// <returns>The <see cref="DocumentStatus"/>.</returns>
        DocumentStatus GetDocumentDataFromUILoad(DataEntryConfigurationManager<Properties.Settings> configManager,
            DataEntryPaginationDocumentData tempData, bool statusOnly, bool verboseWarningCheck)
        {
            var documentStatus = new DocumentStatus();

            using (var form = new InvisibleForm())
            using (var imageViewer = new ImageViewer())
            using (var tempPanel = new DataEntryPanelContainer(
                configManager, _dataEntryApp, _tagUtility, imageViewer, StatusUpdateThreadManager))
            {
                form.Show();
                form.Controls.Add(tempPanel);
                form.Controls.Add(imageViewer);

                tempPanel.LoadData(tempData, forEditing: false);

                IAttribute invalidAttribute = null;
                if (!tempPanel.ActiveDataEntryPanel.Config.Settings.PerformanceTesting)
                {
                    invalidAttribute = AttributeStatusInfo.FindNextAttributeByValidity(
                        tempData.Attributes, DataValidity.Invalid, null, true, false)
                            ?.LastOrDefault();

                    documentStatus.DataError = invalidAttribute != null;

                    if (verboseWarningCheck || !documentStatus.DataError)
                    {
                        var warningAttribute = AttributeStatusInfo.FindNextAttributeByValidity(
                            tempData.Attributes, DataValidity.ValidationWarning, null, true, false)
                                ?.LastOrDefault();

                        documentStatus.DataWarning = warningAttribute != null;
                    }
                }

                if (statusOnly)
                {
                    documentStatus.DataModified = tempPanel.UndoOperationAvailable;
                    documentStatus.Reprocess = tempData.SendForReprocessing;
                    documentStatus.Summary = tempPanel.ActiveDataEntryPanel.SummaryDataEntryQuery?.Evaluate().ToString();
                }
                else
                {
                    var dbConnections = tempPanel.ActiveDataEntryPanel.DatabaseConnections;
                    documentStatus.Orders = QueryRecordNumbers(
                        tempPanel.ActiveDataEntryPanel?.OrderNumberQuery, tempPanel.ActiveDataEntryPanel?.OrderDateQuery, dbConnections);
                    documentStatus.PromptForDuplicateOrders = tempPanel.ActiveDataEntryPanel.PromptForDuplicateOrders;
                    documentStatus.Encounters = QueryRecordNumbers(
                        tempPanel.ActiveDataEntryPanel?.EncounterNumberQuery, tempPanel.ActiveDataEntryPanel?.EncounterDateQuery, dbConnections);
                    documentStatus.PromptForDuplicateEncounters = tempPanel.ActiveDataEntryPanel.PromptForDuplicateEncounters;

                    tempPanel.SaveData(tempData, false);

                    var miscUtils = new MiscUtils();
                    documentStatus.StringizedData = miscUtils.GetObjectAsStringizedByteStream(tempData.Attributes);

                    if (documentStatus.DataError)
                    {
                        try
                        {
                            AttributeStatusInfo.Validate(invalidAttribute, true);
                        }
                        catch (Exception ex)
                        {
                            var ee = ex.AsExtract("ELI45580");
                            documentStatus.StringizedError = ee.AsStringizedByteStream();
                        }

                        return documentStatus;
                    }
                }
            }

            return documentStatus;
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

        /// <summary>
        /// Raises the <see cref="DataPanelChanged"/> event.
        /// </summary>
        void OnDataPanelChanged()
        {
            DataPanelChanged?.Invoke(this, new EventArgs());
        }

        #endregion Private Members
    }
}
