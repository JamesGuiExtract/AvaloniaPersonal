using Extract.DataEntry;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    public partial class DataEntryPanelContainer : UserControl, IPaginationDocumentDataPanel
    {
        /// <summary>
        /// Used to serialize attribute data between threads.
        /// </summary>
        MiscUtils _miscUtils = new MiscUtils();

        /// <summary>
        /// The configuration manager
        /// </summary>
        DataEntryConfigurationManager<Properties.Settings> _configManager;

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
        /// The tag utility
        /// </summary>
        ITagUtility _tagUtility;

        /// <summary>
        /// The expanded configuration file name
        /// </summary>
        string _expandedConfigFileName;

        /// <summary>
        /// The data entry application
        /// </summary>
        IDataEntryApplication _dataEntryApp;

        /// <summary>
        /// The image viewer
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPanelContainer" /> class.
        /// </summary>
        /// <param name="configFileName"></param>
        /// <param name="dataEntryApp">The data entry application.</param>
        /// <param name="tagUtility">The tag utility.</param>
        /// <param name="imageViewer">The image viewer.</param>
        public DataEntryPanelContainer(string configFileName, IDataEntryApplication dataEntryApp, ITagUtility tagUtility, ImageViewer imageViewer)
        {
            try
            {
                InitializeComponent();

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

                if (!_configManager.RegisteredDocumentTypes.Any())
                {
                    MinimumSize = new Size(MinimumSize.Width, MinimumSize.Height - _documentTypePanel.Height);
                    Height -= _documentTypePanel.Height;
                    _documentTypePanel.Visible = false;
                    _documentTypePanel.Height = 0;
                    _scrollPanel.Dock = DockStyle.Fill;

                    if (ActiveDataEntryPanel != null)
                    {
                        ActiveDataEntryPanel.Dock = DockStyle.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

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
                // If this data is getting loaded, there is no need to proceed with any pending
                // document status update.
                int temp;
                _pendingDocumentStatusUpdate.TryRemove(data, out temp);

                _configManager.LoadCorrectConfigForData(data.Attributes);

                _documentData = (DataEntryPaginationDocumentData)data;
                _sourceDocName = _documentData.SourceDocName;

                ActiveDataEntryPanel.LoadData(data);

                _documentTypeComboBox.Enabled = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI0");
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
                //// Undo/redo command should be unavailable until a change is actually made.
                //_undoCommand.Enabled = false;
                //_redoCommand.Enabled = false;

                if (oldDatEntryControlHost != null)
                {
                    // Set Active = false for the old DEP so that it no longer tracks image
                    // viewer events.
                    oldDatEntryControlHost.Active = false;

                    newDataEntryControlHost.PageLoadRequest += DataEntryControlHost_PageLoadRequest;
                    newDataEntryControlHost.UndoAvailabilityChanged += DataEntryControlHost_UndoAvailabilityChanged;
                    newDataEntryControlHost.RedoAvailabilityChanged += NewDataEntryControlHost_RedoAvailabilityChanged;

                    //AttributeStatusInfo.ResetData(null, null, null);
                    oldDatEntryControlHost.ClearData();
                }

                if (newDataEntryControlHost != null)
                {
                    // Load the panel into the _scrollPane
                    LoadDataEntryControlHostPanel();

                    if (_documentData != null)
                    {
                        newDataEntryControlHost.LoadData(_documentData);
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
        /// Clears the state of all data associated with the previously loaded document.
        /// </summary>
        public void ClearData()
        {
            try
            {
                _documentTypeComboBox.Enabled = false;

                _documentData = null;
                _sourceDocName = null;

                ActiveDataEntryPanel.ClearData();

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
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
                var documentData = new DataEntryPaginationDocumentData(attributes, sourceDocName);
                UpdateDocumentStatus(documentData);

                return documentData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
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
            ExtractException ee = null;
            bool dataModified = false;
            bool dataError = false;
            string summary = null;
            Dictionary<string, DbConnection> connectionCopies = null;

            try
            {
                // If the document was loaded by the time this thread was spun up, abort.
                if (!_pendingDocumentStatusUpdate.ContainsKey(documentData))
                {
                    return;
                }

                _documentStatusUpdateSemaphore.WaitOne();

                // If by the time this thread has a chance to update, the document was loaded, abort.
                if (!_pendingDocumentStatusUpdate.ContainsKey(documentData))
                {
                    return;
                }

                var miscUtils = new MiscUtils();
                var deserializedAttributes = (IUnknownVector)miscUtils.GetObjectFromStringizedByteStream(serializedAttributes);

                using (var form = new Form())
                using (var imageViewer = new ImageViewer())
                using (var tempPanel = new DataEntryPanelContainer(_expandedConfigFileName, _dataEntryApp, _tagUtility, imageViewer))
                {
                    form.MakeFormInvisible();
                    form.Show();
                    form.Controls.Add(tempPanel);
                    form.Controls.Add(imageViewer);

                    using (var tempData = new DataEntryPaginationDocumentData(deserializedAttributes, documentData.SourceDocName))
                    {
                        tempPanel.LoadData(tempData);
                        dataModified = tempPanel.UndoOperationAvailable;
                        dataError = (tempPanel.ActiveDataEntryPanel.DataValidity != DataValidity.Valid);
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
                if (connectionCopies != null)
                {
                    CollectionMethods.ClearAndDispose(connectionCopies);
                }
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
        /// Performs an undo operation.
        /// </summary>
        public void Undo()
        {
            ActiveDataEntryPanel.Undo();
        }

        /// <summary>
        /// Performs a redo operation.
        /// </summary>
        public void Redo()
        {
            ActiveDataEntryPanel.Redo();
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
            _documentTypeComboBox.Enabled = false;

            return ActiveDataEntryPanel.SaveData(data, validateData);
        }

        /// <summary>
        /// Provides a message to be displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ShowMessage(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Toggles whether or not tooltip(s) for the active fields are currently visible.
        /// </summary>
        public void ToggleHideTooltips()
        {
            ActiveDataEntryPanel.ToggleHideTooltips();
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
                throw ex.AsExtract("ELI0");
            }
        }

        void DataEntryControlHost_PageLoadRequest(object sender, PageLoadRequestEventArgs e)
        {
            OnPageLoadRequest(e.PageNumber);
        }

        void DataEntryControlHost_UndoAvailabilityChanged(object sender, EventArgs e)
        {
            OnUndoAvailabilityChanged();
        }

        void NewDataEntryControlHost_RedoAvailabilityChanged(object sender, EventArgs e)
        {
            OnRedoAvailabilityChanged();
        }

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
                else if (ActiveDataEntryPanel != null && (_scrollPanel.Controls.Count == 0 ||
                            !_scrollPanel.Controls.Contains(ActiveDataEntryPanel)))
                {
                    if (_scrollPanel.Controls.Count > 0)
                    {
                        _scrollPanel.Controls.Clear();
                    }

                    // Add the DEP to an auto-scroll pane to allow scrolling if the DEP is too
                    // long. (The scroll pane is sized to allow the full width of the DEP to 
                    // display initially) 
                    _scrollPanel.Controls.Add(ActiveDataEntryPanel);
                    ActiveDataEntryPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    ActiveDataEntryPanel.Left = _scrollPanel.Left;
                    ActiveDataEntryPanel.Width = _scrollPanel.Width;
                    MinimumSize = new Size(
                        ActiveDataEntryPanel.MinimumSize.Width,
                        ActiveDataEntryPanel.Height + _documentTypePanel.Height);
                }
            }
            finally
            {
                _scrollPanel.ResumeLayout(true);
            }
        }

        /// <summary>
        /// Raises the <see cref="PageLoadRequest"/>
        /// </summary>
        /// <param name="pageNum">The page number that needs to be loaded in <see cref="_sourceDocName"/>.
        /// </param>
        void OnPageLoadRequest(int pageNum)
        {
            PageLoadRequest?.Invoke(this, new PageLoadRequestEventArgs(_sourceDocName, pageNum));
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
