using Extract.AttributeFinder;
using Extract.FileActionManager.Forms;
using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.XPath;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Manages all <see cref="DataEntryConfiguration" />s currently available. Multiple
    /// configurations will exist when there are multiple DEPs defined where the one used depends
    /// on doc-type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataEntryConfigurationManager<T> : IDisposable where T : ApplicationSettingsBase, new()
    {
        #region Fields

        /// <summary>
        /// The date entry application
        /// </summary>
        IDataEntryApplication _dataEntryApp;

        /// <summary>
        /// The <see cref="ITagUtility"/> interface of the <see cref="FAMTagManager"/> provided to
        /// expand path tags/functions.
        /// </summary>
        ITagUtility _tagUtility;

        /// <summary>
        /// An <see cref="IPathTags"/> interface for <see cref="_tagUtility"/>.
        /// </summary>
        IPathTags _pathTags;

        /// <summary>
        /// A map of defined document types to the configuration to be used.
        /// </summary>
        Dictionary<string, DataEntryConfiguration> _documentTypeConfigurations;

        /// <summary>
        /// If not <see langword="null"/> this configuration should be used for documents with
        /// missing or undefined document types.
        /// </summary>
        DataEntryConfiguration _defaultDataEntryConfig;

        /// <summary>
        /// The configuration that is currently loaded.
        /// </summary>
        DataEntryConfiguration _activeDataEntryConfig;

        /// <summary>
        /// The application configuration
        /// </summary>
        ConfigSettings<T> _applicationConfig;

        /// <summary>
        /// The image viewer
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// The current document type
        /// </summary>
        string _activeDocumentType;

        /// <summary>
        /// Indicates whether the document type is in the process of being changed.
        /// </summary>
        bool _changingDocumentType;

        /// <summary>
        /// An undefined document type that should temporarily be made available in
        /// _documentTypeComboBox so that the document can be saved with its original DocumentType.
        /// </summary>
        string _temporaryDocumentType;

        /// <summary>
        /// The document type ComboBox
        /// </summary>
        ComboBox _documentTypeComboBox;

        /// <summary>
        /// The attributes used to determine the current data configuration.
        /// </summary>
        IUnknownVector _attributes;

        /// <summary>
        /// The <see cref="IAttribute"/> that contains the DocumentType value.
        /// </summary>
        IAttribute _documentTypeAttribute;

        /// <summary>
        /// The master configuration file name
        /// </summary>
        string _masterConfigFileName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryConfigurationManager" /> class.
        /// </summary>
        /// <param name="dataEntryApp">The <see cref="IDataEntryApplication"/> for which the
        /// configurations are being managed.</param>
        /// <param name="tagUtility">The <see cref="ITagUtility"/> being used to expand path
        /// tags/functions.</param>
        /// <param name="applicationConfig">The application level settings that would contain any
        /// documentTypeConfigurations.</param>
        /// <param name="imageViewer">The image viewer.</param>
        /// <param name="documentTypeComboBox">The <see cref="ComboBox"/> that offers the user the
        /// ability to see and change the current doc type.</param>
        public DataEntryConfigurationManager(IDataEntryApplication dataEntryApp, ITagUtility tagUtility,
            ConfigSettings<T> applicationConfig, ImageViewer imageViewer, ComboBox documentTypeComboBox)
        {
            try
            {
                _tagUtility = tagUtility;
                _applicationConfig = applicationConfig;
                _imageViewer = imageViewer;
                _documentTypeComboBox = documentTypeComboBox;

                _dataEntryApp = dataEntryApp;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41594");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when each of this instances <see cref="DataEntryConfiguration"/>s has finished initializing.
        /// </summary>
        public event EventHandler<ConfigurationInitializedEventArgs> ConfigurationInitialized;

        /// <summary>
        /// Raised when the active configuration is about to change (initial load or when doc type
        /// changes).
        /// </summary>
        public event EventHandler<CancelEventArgs> ConfigurationChanging;

        /// <summary>
        /// Raised when the active configuration has been changed (initial load or when doc type
        /// changes).
        /// </summary>
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// Occurs when the document type changes (initial load included).
        /// </summary>
        public event EventHandler<EventArgs> DocumentTypeChanged;

        /// <summary>
        /// Raised if there was an error changing configurations. An exception will not otherwise be
        /// raised in this situation, so it is the owner's responsibility to handle the error
        /// appropriately.
        /// </summary>
        public event EventHandler<VerificationExceptionGeneratedEventArgs> ConfigurationChangeError;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the default <see cref="DataEntryConfiguration"/> to use in the case that document
        /// type does not indicate the usage of another configuration.
        /// </summary>
        public DataEntryConfiguration DefaultDataEntryConfiguration
        {
            get
            {
                return _defaultDataEntryConfig;
            }
        }

        /// <summary>
        /// Gets the currently active <see cref="DataEntryConfiguration"/>.
        /// </summary>
        public DataEntryConfiguration ActiveDataEntryConfiguration
        {
            get
            {
                return _activeDataEntryConfig;
            }
        }

        /// <summary>
        /// Gets all the <see cref="DataEntryConfiguration"/>s.
        /// </summary>
        public IEnumerable<DataEntryConfiguration> Configurations
        {
            get
            {
                return (_documentTypeConfigurations == null)
                    ? new[] { DefaultDataEntryConfiguration }
                    : _documentTypeConfigurations.Values.OfType<DataEntryConfiguration>();
            }
        }

        /// <summary>
        /// Gets/sets the attributes used to determine the current data configuration.
        /// </summary>
        public IUnknownVector Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a background manager for
        /// loading document status information.
        /// </summary>
        public bool IsBackgroundManager { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets all document types specified via documentTypeConfigurations in the config file.
        /// </summary>
        public IEnumerable<string> RegisteredDocumentTypes
        {
            get
            {
                if (_documentTypeConfigurations != null)
                {
                    foreach (string documentType in _documentTypeConfigurations.Keys)
                    {
                        yield return documentType;
                    }
                }
            }
        }

        /// <summary>
        /// Loads all defined document types and their associated configurations from the specified
        /// master config file.
        /// </summary>
        /// <param name="masterConfigFileName">The config file which will specify document type
        /// configurations (if available).</param>
        public void LoadDataEntryConfigurations(string masterConfigFileName)
        {
            try
            {
                _masterConfigFileName = masterConfigFileName;

                // Retrieve the documentTypeConfigurations XML section if it exists
                IXPathNavigable documentTypeConfiguration =
                    _applicationConfig.GetSectionXml("documentTypeConfigurations");

                XPathNavigator configurationNode = null;
                if (documentTypeConfiguration != null)
                {
                    configurationNode = documentTypeConfiguration.CreateNavigator();
                }

                // If unable to find the documentTypeConfigurations or find a defined configuration,
                // use the master config file as the one and only configuration.
                if (configurationNode == null || !configurationNode.MoveToFirstChild())
                {
                    _defaultDataEntryConfig = LoadDataEntryConfiguration(masterConfigFileName);
                    ChangeActiveDocumentType(null, true);
                    return;
                }

                // Document type configurations have been defined.
                _documentTypeConfigurations = new Dictionary<string, DataEntryConfiguration>
                    (StringComparer.OrdinalIgnoreCase);

                // Load each configuration.
                do
                {
                    if (!configurationNode.Name.Equals("configuration", StringComparison.OrdinalIgnoreCase))
                    {
                        ExtractException ee = new ExtractException("ELI30617",
                            "Config file error: Unknown DocumentTypeConfiguration element.");
                        ee.AddDebugData("Name", configurationNode.Name, false);
                        throw ee;
                    }

                    XPathNavigator attribute = configurationNode.Clone();
                    if (!attribute.MoveToFirstAttribute())
                    {
                        throw new ExtractException("ELI30618",
                            "Config file error: Missing required DocumentTypeConfiguration elements.");
                    }

                    // Load the configurations element's attributes
                    string configFileName = null;
                    bool defaultConfiguration = false;
                    do
                    {
                        if (attribute.Name.Equals("configFile", StringComparison.OrdinalIgnoreCase))
                        {
                            configFileName = attribute.Value;
                        }
                        else if (attribute.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                        {
                            defaultConfiguration = attribute.ValueAsBoolean;
                            if (defaultConfiguration && _defaultDataEntryConfig != null)
                            {
                                throw new ExtractException("ELI30664",
                                    "Only one document type configuration may be set as the default.");
                            }
                        }
                        else
                        {
                            ExtractException ee = new ExtractException("ELI30619",
                                "Config file error: Unknown attribute in Configuration node.");
                            ee.AddDebugData("Name", attribute.Name, false);
                            throw ee;
                        }
                    }
                    while (attribute.MoveToNextAttribute());

                    ExtractException.Assert("ELI30620",
                        "Config file error: Missing configFile attribute in Configuration node.",
                        !string.IsNullOrEmpty(configFileName));

                    configFileName = DataEntryMethods.ResolvePath(configFileName);

                    DataEntryConfiguration config = LoadDataEntryConfiguration(configFileName);
                    if (defaultConfiguration)
                    {
                        _defaultDataEntryConfig = config;
                        ChangeActiveDocumentType(null, true);
                    }

                    XPathNavigator documentTypeNode = configurationNode.Clone();
                    if (!documentTypeNode.MoveToFirstChild())
                    {
                        throw new ExtractException("ELI30621",
                            "Config file error: At least one DocumentType element is required for each Configuration.");
                    }

                    // Load all document types that will use this configuration.
                    do
                    {
                        if (!documentTypeNode.Name.Equals("DocumentType",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            ExtractException ee = new ExtractException("ELI30622",
                                "Unknown DocumentTypeConfiguration element.");
                            ee.AddDebugData("Name", documentTypeNode.Name, false);
                            throw ee;
                        }

                        string documentType = documentTypeNode.Value;
                        if (_documentTypeConfigurations.ContainsKey(documentType))
                        {
                            ExtractException ee = new ExtractException("ELI30623",
                                "Config file error: Duplicate documentType element.");
                            ee.AddDebugData("DocumentType", documentType, false);
                            throw ee;
                        }

                        _documentTypeComboBox?.Items.Add(documentType);
                        _documentTypeConfigurations[documentType] = config;
                    }
                    while (documentTypeNode.MoveToNext());
                }
                while (configurationNode.MoveToNext());

                // Register to be notified when the user selects a new document type.
                if (_documentTypeConfigurations != null)
                {
                    _documentTypeComboBox.DropDownClosed += HandleDocumentTypeDropDownClosed;
                    _documentTypeComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
                    _documentTypeComboBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41595");
            }
        }

        /// <summary>
        /// Creates a copy of this instance for use in background data loading.
        /// </summary>
        public DataEntryConfigurationManager<T> CreateBackgroundManager()
        {
            try
            {
                var manager = new DataEntryConfigurationManager<T>(
                    new NullDataEntryApp(), _tagUtility, _applicationConfig, null, null);

                manager.IsBackgroundManager = true;
                manager.ChangeActiveDocumentType(null, true);

                if (_documentTypeConfigurations != null)
                {
                    manager._documentTypeConfigurations = new Dictionary<string, DataEntryConfiguration>();

                    foreach (var configuration in _documentTypeConfigurations)
                    {
                        var backgroundConfig = CreateBackgroundConfiguration(configuration.Value);

                        manager._documentTypeConfigurations[configuration.Key] = backgroundConfig;

                        // Initialize active and default configurations
                        if (_activeDataEntryConfig == configuration.Value)
                        {
                            manager._activeDataEntryConfig = backgroundConfig;
                        }
                        if (_defaultDataEntryConfig == configuration.Value)
                        {
                            manager._defaultDataEntryConfig = backgroundConfig;
                        }

                        backgroundConfig.PanelCreated += BackgroundConfig_PanelCreated;
                    }
                }
                else
                {
                    manager._defaultDataEntryConfig = CreateBackgroundConfiguration(_defaultDataEntryConfig);
                    manager._activeDataEntryConfig = manager._defaultDataEntryConfig;

                    manager._defaultDataEntryConfig.PanelCreated += BackgroundConfig_PanelCreated;
                }

                // _pathTags needed for AttributeStatusInfo.ExecuteNoUILoad so that workflow-specific
                // tags are available
                // https://extract.atlassian.net/browse/ISSUE-15297
                if (_dataEntryApp.FileProcessingDB != null)
                {
                    var famPathTags = new FileActionManagerPathTags();
                    famPathTags.DatabaseServer = _dataEntryApp.FileProcessingDB.DatabaseServer;
                    famPathTags.DatabaseName = _dataEntryApp.FileProcessingDB.DatabaseName;
                    famPathTags.Workflow = _dataEntryApp.FileProcessingDB.ActiveWorkflow;
                    manager._pathTags = famPathTags;
                }

                return manager;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45509");
            }
        }

        /// <summary>
        /// Clears all data currently loaded in the active <see cref="DataEntry Configuration"/> and
        /// its associated <see cref="DataEntryControlHost"/>.
        /// </summary>
        public void ClearData()
        {
            try
            {
                UnregisterDocumentTypeHook();
                _attributes = null;

                // If a _temporaryDocumentType was added to the _documentTypeMenu to match the
                // original type of the last document loaded, remove it now that the image has
                // changed.
                if (_temporaryDocumentType != null)
                {
                    _documentTypeComboBox.Items.Remove(_temporaryDocumentType);
                    _temporaryDocumentType = null;
                }

                if (_activeDataEntryConfig?.DataEntryControlHost != null)
                {
                    _activeDataEntryConfig.DataEntryControlHost.ClearData();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41596");
            }
        }

        #endregion Methods

        #region IDisposable

        /// <overloads>Releases resources used by the <see cref="DataEntryConfiguration"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryConfiguration"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataEntryConfiguration"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_documentTypeConfigurations != null)
                {
                    CollectionMethods.ClearAndDispose(_documentTypeConfigurations);
                }

                if (_defaultDataEntryConfig != null)
                {
                    _defaultDataEntryConfig.Dispose();
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable

        #region Private Members

        /// <summary>
        /// Applies the specified document type and loads the DEP associated with the document
        /// types configuration if necessary.
        /// </summary>
        /// <param name="documentType">The new document type.</param>
        DataEntryConfiguration GetConfigurationForDocumentType(string documentType)
        {
            try
            {
                DataEntryConfiguration dataEntryConfig = _defaultDataEntryConfig;

                if (_documentTypeConfigurations != null && documentType != null &&
                    !_documentTypeConfigurations.TryGetValue(documentType, out dataEntryConfig))
                {
                    dataEntryConfig = _defaultDataEntryConfig;
                }

                return dataEntryConfig;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30655", ex);
            }
        }

        /// <summary>
        /// Loads the data entry configuration defined by the specified config file.
        /// </summary>
        /// <param name="configFileName">The configuration file that defines the configuration to
        /// be loaded.</param>
        /// <param name="masterConfigFileName">If not <see langword="null"/>, the configuration that
        /// may provide defaults for DataEntry and objectSettings config file values.</param>
        /// <returns>The loaded <see cref="DataEntryConfiguration"/>.</returns>
        DataEntryConfiguration LoadDataEntryConfiguration(string configFileName)
        {
            try
            {
                // Load the configuration settings from file.
                ConfigSettings<Extract.DataEntry.Properties.Settings> config =
                    new ConfigSettings<Extract.DataEntry.Properties.Settings>(
                        configFileName, _masterConfigFileName, false, false, _tagUtility);

                DataEntryConfiguration configuration =
                    new DataEntryConfiguration(config, _tagUtility, _dataEntryApp.FileProcessingDB, false);

                // Tie the newly created DEP to this application and its ImageViewer.
                configuration.DataEntryControlHost.DataEntryApplication = _dataEntryApp;
                configuration.DataEntryControlHost.Config = config;
                configuration.DataEntryControlHost.ImageViewer = _imageViewer;

                QueryNode.QueryCacheLimit = config.Settings.QueryCacheLimit;
                InitializePanel(configuration);

                if (config.Settings.SupportsNoUILoad)
                {
                    configuration.BuildFieldModels(config);
                }

                OnConfigurationInitialized(configuration);

                return configuration;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30539",
                    "Failed to load data entry configuration", ex);
                ee.AddDebugData("Config file", configFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Initializes the panel.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void InitializePanel(DataEntryConfiguration configuration)
        {
            try
            {
                // If HighlightConfidenceBoundary settings has been specified in the config file and
                // the controlHost has exactly two confidence tiers, use the provided value as the
                // minimum OCR confidence value in order to highlight text as confidently OCR'd
                if (!string.IsNullOrEmpty(configuration.Config.Settings.HighlightConfidenceBoundary)
                    && configuration.DataEntryControlHost.HighlightColors.Length == 2)
                {
                    int confidenceBoundary = Convert.ToInt32(
                        configuration.Config.Settings.HighlightConfidenceBoundary,
                        CultureInfo.CurrentCulture);

                    ExtractException.Assert("ELI25684", "HighlightConfidenceBoundary settings must " +
                        "be a value between 1 and 100",
                        confidenceBoundary >= 1 && confidenceBoundary <= 100);

                    HighlightColor[] highlightColors = configuration.DataEntryControlHost.HighlightColors;
                    highlightColors[0].MaxOcrConfidence = confidenceBoundary - 1;
                    configuration.DataEntryControlHost.HighlightColors = highlightColors;
                }

                configuration.DataEntryControlHost.DisabledControls = configuration.Config.Settings.DisabledControls;
                configuration.DataEntryControlHost.DisabledValidationControls =
                    configuration.Config.Settings.DisabledValidationControls;

                // Apply settings from the config file that pertain to the DEP.
                if (!string.IsNullOrEmpty(_masterConfigFileName))
                {
                    _applicationConfig.ApplyObjectSettings(configuration.DataEntryControlHost);
                }
                configuration.Config.ApplyObjectSettings(configuration.DataEntryControlHost);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45637");
            }
        }

        /// <summary>
        /// Changes the current document's document type to the specified value.
        /// </summary>
        /// <param name="documentType">The new document type</param>
        /// <param name="allowConfigurationChange"><see langword="true"/> if the configuration
        /// should be changed if the new document type calls for it, <see langword="false"/> if
        /// the current configuration should not be changed.</param>
        bool ChangeActiveDocumentType(string documentType, bool allowConfigurationChange)
        {
            if (_changingDocumentType)
            {
                return false;
            }

            try
            {
                _changingDocumentType = true;

                DataEntryConfiguration lastDataEntryConfig = ActiveDataEntryConfiguration;
                bool changedDocumentType, changedDataEntryConfig;
                SetActiveDocumentType(documentType, allowConfigurationChange,
                    out changedDocumentType, out changedDataEntryConfig);
                if (changedDocumentType)
                {
                    if (changedDataEntryConfig)
                    {
                        if (lastDataEntryConfig != null)
                        {
                            lastDataEntryConfig.CloseDatabaseConnections();
                        }

                        if (ActiveDataEntryConfiguration != null)
                        {
                            ActiveDataEntryConfiguration.OpenDatabaseConnections();
                        }

                        OnConfigurationChanged(lastDataEntryConfig, ActiveDataEntryConfiguration);
                    }

                    OnDocumentTypeChanged();
                }

                return changedDocumentType;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30663", ex);
            }
            finally
            {
                _changingDocumentType = false;
            }
        }

        /// <summary>
        /// Loads the type of the correct config for document.
        /// </summary>
        /// <param name="attributes">The attributes representing the document data.</param>
        /// <returns><see langword="true"/> if the configuration was changed based on the data;
        /// otherwise, <see langword="false"/>.</returns>
        public bool LoadCorrectConfigForData(IUnknownVector attributes)
        {
            try
            {
                _attributes = attributes;

                // If there were document type specific configurations defined, apply the
                // appropriate configuration now.
                bool changedDocumentType;
                if (_documentTypeConfigurations != null)
                {
                    GetDocumentTypeAttribute();
                    string documentType = _documentTypeAttribute?.Value.String;

                    if (_documentTypeComboBox != null)
                    {
                        // If there is a default configuration, add the original document type to the
                        // document type combo and allow the document to be saved with the undefined
                        // document type.
                        if (_defaultDataEntryConfig != null &&
                            _documentTypeComboBox.FindStringExact(documentType) == -1)
                        {
                            _temporaryDocumentType = documentType;
                            _documentTypeComboBox.Items.Insert(0, documentType);
                        }
                    }

                    changedDocumentType = ChangeActiveDocumentType(documentType, true);
                }
                else
                {
                    changedDocumentType = false;
                }

                return changedDocumentType;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35079");
            }
        }

        /// <summary>
        /// Creates a <see cref="DataEntryConfiguration"/> to be used for background document status
        /// loading based on the specified source <see paramref="configuration"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="DataEntryConfiguration"/> on which this
        /// background configuration is based.</param>
        /// <returns></returns>
        DataEntryConfiguration CreateBackgroundConfiguration(DataEntryConfiguration configuration)
        {
            // Create a background configuration as long as the configuration supports a NoUI load.
            DataEntryConfiguration backgroundConfig = null;
            if (configuration.Config.Settings.SupportsNoUILoad)
            {
                backgroundConfig = configuration.CreateNoUIConfiguration();
            }
            else
            {
                backgroundConfig = new DataEntryConfiguration(
                    configuration.Config, _tagUtility, configuration.FileProcessingDB, true);
            }

            return backgroundConfig;
        }

        /// <summary>
        /// Attempts to load and transform the <see paramref="attributes"/> without a UI.
        /// </summary>
        /// <param name="attributes">The attributes to load/transform</param>
        /// <param name="sourceDocName">Name of the source document.</param>
        /// <returns><c>true</c> if the data was loaded in the background; <c>false</c> if no UI
        /// loading is not supported in this configuration.</returns>
        public bool ExecuteNoUILoad(IUnknownVector attributes, string sourceDocName)
        {
            try
            {
                LoadCorrectConfigForData(attributes);
                if (!ActiveDataEntryConfiguration.Config.Settings.SupportsNoUILoad)
                {
                    return false;
                }

                AttributeStatusInfo.ExecuteNoUILoad(attributes, sourceDocName,
                    ActiveDataEntryConfiguration.GetDatabaseConnections(),
                    ActiveDataEntryConfiguration.BackgroundFieldModels, _pathTags);

                // While validation queries are executed by AttributeStatusInfo.ExecuteNoUILoad, the
                // attributes are not explicitly validated. Validate all attributes here to ensure
                // ValidationPatterns are considered.
                // https://extract.atlassian.net/browse/ISSUE-15327
                foreach (var attribute in attributes
                    .ToIEnumerable<IAttribute>()
                    .SelectMany(attribute => attribute.EnumerateDepthFirst()))
                {
                    if (AttributeStatusInfo.Validate(attribute, false) == DataValidity.Invalid)
                    {
                        // Don't prune attributes if there is invalid data; otherwise the invalid
                        // attribute might be pruned such that the caller doesn't know of the error.
                        // https://extract.atlassian.net/browse/ISSUE-15328
                        return true;
                    }
                }
                
                DataEntryMethods.PruneNonPersistingAttributes(attributes);

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45511");
            }
        }

        /// <summary>
        /// Applies the specified document type and loads the DEP associated with the document
        /// types configuration if necessary.
        /// </summary>
        /// <param name="documentType">The new document type.</param>
        /// <param name="allowConfigurationChange"><see langword="true"/> if the configuration
        /// should be changed if the new document type calls for it, <see langword="false"/> if
        /// the current configuration should not be changed.</param>
        /// <param name="changedDocumentType"><see langword="true"/> if the active document type
        /// was changed, <see langword="false"/> otherwise.</param>
        /// <param name="changedDataEntryConfig"><see langword="true"/> if the active configuration
        /// was changed, <see langword="false"/> otherwise</param>
        void SetActiveDocumentType(string documentType, bool allowConfigurationChange,
            out bool changedDocumentType, out bool changedDataEntryConfig)
        {
            try
            {
                changedDocumentType = false;
                changedDataEntryConfig = false;
                DataEntryConfiguration newDataEntryConfig = GetConfigurationForDocumentType(documentType);

                if (_activeDataEntryConfig == null ||
                    !string.Equals(documentType, _activeDocumentType, StringComparison.OrdinalIgnoreCase))
                {
                    changedDocumentType = true;
                    bool blockedConfigurationChange = false;

                    // If a configuration was found and it differs from the active one, load it.
                    if (newDataEntryConfig != _activeDataEntryConfig)
                    {
                        if (!allowConfigurationChange)
                        {
                            // The document type calls for the configuration to be changed, but
                            // configuration changes are disallowed. This change is to be blocked.
                            blockedConfigurationChange = true;
                        }
                        else if (!OnConfigurationChanging())
                        {
                            // If the user cancelled the change, restore the _activeDocumentType
                            // selection in the document type combo box.
                            _documentTypeComboBox.Text = _activeDocumentType;
                            changedDocumentType = false;
                        }
                        else
                        {
                            // Apply the new configuration and load its DEP.
                            changedDataEntryConfig = true;
                            _activeDataEntryConfig = newDataEntryConfig;
                        }
                    }

                    if (changedDocumentType)
                    {
                        changedDocumentType = !blockedConfigurationChange;
                        if (changedDocumentType)
                        {
                            SetDocumentTypeAttribute(documentType);
                        }

                        if (_documentTypeComboBox != null)
                        {
                            if (blockedConfigurationChange ||
                                _documentTypeComboBox.FindStringExact(documentType) == -1)
                            {
                                // The new documentType is not valid.
                                _documentTypeComboBox.SelectedIndex = -1;
                            }
                            else
                            {
                                // Assign the new document type.
                                _activeDocumentType = documentType;
                                _documentTypeComboBox.Text = documentType;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30655", ex);
            }
        }

        /// <summary>
        /// Gets the document type for the specified data using the first root-level DocumentType
        /// <see cref="IAttribute"/>. 
        /// </summary>
        /// <param name="attributes">The attributes representing the document's data.</param>
        void GetDocumentTypeAttribute()
        {
            // Remove event registration from the last DocumentType attribute we found.
            UnregisterDocumentTypeHook();

            if (_attributes != null)
            {
                // Search for the DocumentType attribute.
                IUnknownVector matchingAttributes =
                    DataEntryMethods.AFUtility.QueryAttributes(
                        _attributes, "DocumentType", false);

                int matchingAttributeCount = matchingAttributes.Size();
                if (matchingAttributeCount > 0)
                {
                    _documentTypeAttribute = (IAttribute)matchingAttributes.At(0);
                }
                else
                {
                    // Add the document type attribute if it didn't previously exist.
                    _documentTypeAttribute = new UCLID_AFCORELib.Attribute();
                    _documentTypeAttribute.Name = "DocumentType";
                    _attributes.PushBack(_documentTypeAttribute);
                }
            }

            RegisterDocumentTypeHook();
        }

        /// <summary>
        /// Sets the document type attribute to <see paramref="documentType"/>.
        /// </summary>
        /// <param name="documentType">The value to assign to the document type attribute.</param>
        void SetDocumentTypeAttribute(string documentType)
        {
            GetDocumentTypeAttribute();

            if (_documentTypeAttribute != null)
            {
                AttributeStatusInfo.SetValue(_documentTypeAttribute, documentType, true, true);
            }
        }

        /// <summary>
        /// Registers to be notified of changes to the document type made from withing a DEP.
        /// </summary>
        void RegisterDocumentTypeHook()
        {
            if (_documentTypeAttribute != null)
            {
                var statusInfo =
                    AttributeStatusInfo.GetStatusInfo(_documentTypeAttribute);
                statusInfo.AttributeValueModified += HandleDocumentTypeAttributeValueModified;
            }
        }

        /// <summary>
        /// Unregisters from being notified of changes to the document type made from withing a DEP.
        /// </summary>
        void UnregisterDocumentTypeHook()
        {
            if (_documentTypeAttribute != null)
            {
                var statusInfo =
                    AttributeStatusInfo.GetStatusInfo(_documentTypeAttribute);
                statusInfo.AttributeValueModified -= HandleDocumentTypeAttributeValueModified;
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.DropDownClosed"/> event for the
        /// _documentTypeComboBox.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleDocumentTypeDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                // Update the active document type, changing the current configuration if
                // appropriate.
                ChangeActiveDocumentType(_documentTypeComboBox.Text, true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30616", ex);
                ee.AddDebugData("Event arguments", e, false);
                // Raising ConfigurationChangeError will fail the document.
                // Debated on whether this should be displayed instead, since this may be something
                // that happens in the midst of verification not but this could happen during
                // document load and even it if doesn't, it could indicate that the document will
                // not be able to be correctly saved.
                OnConfigurationChangeError(ee);
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeValueModified"/> event for the
        /// <see cref="IAttribute"/> containing the document type.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleDocumentTypeAttributeValueModified(object sender,
                AttributeValueModifiedEventArgs e)
        {
            try
            {
                // Update the active document type, but don't allow the current configuration to be
                // changed.
                ChangeActiveDocumentType(e.Attribute.Value.String, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30653", ex);
                ee.AddDebugData("Event data", e, false);
                // Raising ConfigurationChangeError will fail the document.
                // Debated on whether this should be displayed instead, since this may be something
                // that happens in the midst of verification not but this could happen during
                // document load and even it if doesn't, it could indicate that the document will
                // not be able to be correctly saved.
                OnConfigurationChangeError(ee);
            }
        }

        /// <summary>
        /// Handles the PanelCreated event for configurations in the background.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void BackgroundConfig_PanelCreated(object sender, EventArgs e)
        {
            try
            {
                var configuration = (DataEntryConfiguration)sender;

                InitializePanel(configuration);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45638");
            }
        }

        /// <summary>
        /// Raises the <see cref="ConfigurationInitialized"/> event.
        /// </summary>
        /// <param name="dataEntryConfig">The <see cref="DataEntryConfiguration"/> that has been initialized.
        /// </param>
        void OnConfigurationInitialized(DataEntryConfiguration dataEntryConfig)
        {
            ConfigurationInitialized?.Invoke(this, new ConfigurationInitializedEventArgs(dataEntryConfig));
        }

        /// <summary>
        /// Raises the <see cref="ConfigurationChanging"/> event.
        /// </summary>
        /// <returns><c>true</c> if the configuration change can proceed; <c>false</c> if a handler
        /// requested that the configuration change be cancelled.</returns>
        bool OnConfigurationChanging()
        {
            var eventArgs = new CancelEventArgs(false);

            ConfigurationChanging?.Invoke(this, eventArgs);

            return !eventArgs.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="ConfigurationChanged"/> event.
        /// </summary>
        /// <param name="oldDataEntryConfig">The <see cref="DataEntryConfiguration"/> that is being
        /// switched out.</param>
        /// <param name="newDataEntryConfig">The <see cref="DataEntryConfiguration"/> that is now in
        /// use.</param>
        void OnConfigurationChanged(DataEntryConfiguration oldDataEntryConfig,
            DataEntryConfiguration newDataEntryConfig)
        {
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(oldDataEntryConfig, newDataEntryConfig));
        }

        /// <summary>
        /// Raises the <see cref="DocumentTypeChanged"/> event.
        /// </summary>
        void OnDocumentTypeChanged()
        {
            DocumentTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ConfigurationChangeError"/> event.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> representing the error.</param>
        void OnConfigurationChangeError(ExtractException ee)
        {
            var eventHandler = ConfigurationChangeError;
            if (eventHandler == null)
            {
                ee.Display();
            }
            else
            {
                eventHandler(this,
                    new VerificationExceptionGeneratedEventArgs(ee, true));
            }
        }

        #endregion Private Members
    }
}
