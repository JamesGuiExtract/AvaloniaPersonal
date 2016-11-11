﻿using Extract.Database;
using Extract.DataEntry;
using Extract.FileActionManager.Forms;
using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataEntryConfigurationManager<T> : IDisposable where T : ApplicationSettingsBase, new()
    {
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
        /// The <see cref="IAttribute"/> that contains the DocumentType value.
        /// </summary>
        IAttribute _documentTypeAttribute;

        /// <summary>
        /// The <see cref="AttributeStatusInfo"/> associated with _documentTypeAttribute
        /// </summary>
        AttributeStatusInfo _documentTypeAttributeStatusInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryConfigurationManager" /> class.
        /// </summary>
        /// <param name="dataEntryApp">The data entry application.</param>
        /// <param name="tagUtility">The tag utility.</param>
        /// <param name="applicationConfig">The application configuration.</param>
        /// <param name="imageViewer">The image viewer.</param>
        /// <param name="documentTypeComboBox"></param>
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
                throw ex.AsExtract("ELI0");
            }
        }

        /// <summary>
        /// Occurs when [configuration changed].
        /// </summary>
        public event EventHandler<CancelEventArgs> ConfigurationChanging;

        /// <summary>
        /// Occurs when [configuration changed].
        /// </summary>
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// Occurs when [configuration change error].
        /// </summary>
        public event EventHandler<VerificationExceptionGeneratedEventArgs> ConfigurationChangeError;

        /// <summary>
        /// The default data entry configuration
        /// </summary>
        public DataEntryConfiguration DefaultDataEntryConfiguration
        {
            get
            {
                return _defaultDataEntryConfig;
            }
        }

        /// <summary>
        /// The default data entry configuration
        /// </summary>
        public DataEntryConfiguration ActiveDataEntryConfiguration
        {
            get
            {
                return _activeDataEntryConfig;
            }
        }

        /// <summary>
        /// Gets the multiple document types.
        /// </summary>
        /// <value>
        /// The multiple document types.
        /// </value>
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
                    _defaultDataEntryConfig = LoadDataEntryConfiguration(masterConfigFileName, null);

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

                    DataEntryConfiguration config =
                        LoadDataEntryConfiguration(configFileName, masterConfigFileName);
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

                        _documentTypeComboBox.Items.Add(documentType);
                        _documentTypeConfigurations[documentType] = config;
                    }
                    while (documentTypeNode.MoveToNext());
                }
                while (configurationNode.MoveToNext());

                // Register to be notified when the user selects a new document type.
                _documentTypeComboBox.SelectedIndexChanged += HandleDocumentTypeSelectedIndexChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        /// <summary>
        /// Clears the data.
        /// </summary>
        public void ClearData()
        {
            try
            {
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
                throw ex.AsExtract("ELI0");
            }
        }

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
        DataEntryConfiguration LoadDataEntryConfiguration(string configFileName,
            string masterConfigFileName)
        {
            try
            {
                // Load the configuration settings from file.
                ConfigSettings<Extract.DataEntry.Properties.Settings> config =
                    new ConfigSettings<Extract.DataEntry.Properties.Settings>(
                        configFileName, masterConfigFileName, false, false, _tagUtility);

                DataEntryConfiguration configuration =
                    new DataEntryConfiguration(config, _tagUtility, _dataEntryApp.FileProcessingDB);

                // Tie the newly created DEP to this application and its ImageViewer.
                configuration.DataEntryControlHost.DataEntryApplication = _dataEntryApp;
                configuration.DataEntryControlHost.Config = config;
                configuration.DataEntryControlHost.ImageViewer = _imageViewer;

                //QueryNode.QueryCacheLimit = _applicationConfig.Settings.QueryCacheLimit;

                // If HighlightConfidenceBoundary settings has been specified in the config file and
                // the controlHost has exactly two confidence tiers, use the provided value as the
                // minimum OCR confidence value in order to highlight text as confidently OCR'd
                if (!string.IsNullOrEmpty(config.Settings.HighlightConfidenceBoundary)
                    && configuration.DataEntryControlHost.HighlightColors.Length == 2)
                {
                    int confidenceBoundary = Convert.ToInt32(
                        config.Settings.HighlightConfidenceBoundary,
                        CultureInfo.CurrentCulture);

                    ExtractException.Assert("ELI25684", "HighlightConfidenceBoundary settings must " +
                        "be a value between 1 and 100",
                        confidenceBoundary >= 1 && confidenceBoundary <= 100);

                    HighlightColor[] highlightColors = configuration.DataEntryControlHost.HighlightColors;
                    highlightColors[0].MaxOcrConfidence = confidenceBoundary - 1;
                    configuration.DataEntryControlHost.HighlightColors = highlightColors;
                }

                configuration.DataEntryControlHost.DisabledControls = config.Settings.DisabledControls;
                configuration.DataEntryControlHost.DisabledValidationControls =
                    config.Settings.DisabledValidationControls;

                // Apply settings from the config file that pertain to the DEP.
                if (!string.IsNullOrEmpty(masterConfigFileName))
                {
                    _applicationConfig.ApplyObjectSettings(configuration.DataEntryControlHost);
                }
                config.ApplyObjectSettings(configuration.DataEntryControlHost);

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
                        //if (lastDataEntryConfig != null)
                        //{
                        //    lastDataEntryConfig.CloseDatabaseConnections();
                        //}

                        if (ActiveDataEntryConfiguration != null)
                        {
                            ActiveDataEntryConfiguration.OpenDatabaseConnections();
                        }

                        OnConfigurationChanged(lastDataEntryConfig, ActiveDataEntryConfiguration);
                    }

                    if (_activeDataEntryConfig?.DataEntryControlHost != null)
                    {
                        // Apply the new document type to the DocumentType attribute.
                        AssignNewDocumentType(documentType, changedDataEntryConfig);
                    }
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
        /// Assigns a new document type to the DocumentType <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="newDocumentType">The new document type.</param>
        /// <param name="reloadDocumentTypeAttribute">Whether to re-find the DocumentType attribute
        /// rather than use one that has already been found.</param>
        void AssignNewDocumentType(string newDocumentType, bool reloadDocumentTypeAttribute)
        {
            // If reloading the DocumentType attribute, remove event registration from the last
            // DocumentType attribute we found.
            if (reloadDocumentTypeAttribute)
            {
                _documentTypeAttribute = null;

                if (_documentTypeAttributeStatusInfo != null)
                {
                    _documentTypeAttributeStatusInfo.AttributeValueModified -=
                        HandleDocumentTypeAttributeValueModified;
                    _documentTypeAttributeStatusInfo = null;
                }
            }

            // Attempt to find a new DocumentType attribute if we don't currently have one.
            if (_documentTypeAttribute == null && _activeDataEntryConfig?.DataEntryControlHost != null)
            {
                IUnknownVector matchingAttributes =
                DataEntryMethods.AFUtility.QueryAttributes(
                    _activeDataEntryConfig?.DataEntryControlHost.Attributes, "DocumentType", false);

                int matchingAttributeCount = matchingAttributes.Size();
                if (matchingAttributeCount > 0)
                {
                    _documentTypeAttribute = (IAttribute)matchingAttributes.At(0);
                }
                else
                {
                    // Create a new DocumentType attribute if necessary.
                    _documentTypeAttribute = (IAttribute)new AttributeClass();
                    _documentTypeAttribute.Name = "DocumentType";

                    AttributeStatusInfo.Initialize(_documentTypeAttribute,
                        _activeDataEntryConfig?.DataEntryControlHost.Attributes, null);
                }

                // Register to be notified of changes to the attribute.
                _documentTypeAttributeStatusInfo =
                    AttributeStatusInfo.GetStatusInfo(_documentTypeAttribute);
                _documentTypeAttributeStatusInfo.AttributeValueModified +=
                    HandleDocumentTypeAttributeValueModified;
            }

            // If the DocumentType value was changed, refresh any DEP control that displays the
            // DocumentType.
            if (!_documentTypeAttribute.Value.String.Equals(
                    newDocumentType, StringComparison.OrdinalIgnoreCase))
            {
                AttributeStatusInfo.SetValue(_documentTypeAttribute, newDocumentType ?? "", false, true);
                IDataEntryControl dataEntryControl =
                    AttributeStatusInfo.GetOwningControl(_documentTypeAttribute);
                if (dataEntryControl != null)
                {
                    dataEntryControl.RefreshAttributes(false, _documentTypeAttribute);
                }
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
                // If there were document type specific configurations defined, apply the
                // appropriate configuration now.
                bool changedDocumentType;
                if (_documentTypeConfigurations != null)
                {
                    string documentType = GetDocumentType(attributes, false);

                    // If there is a default configuration, add the original document type to the
                    // document type combo and allow the document to be saved with the undefined
                    // document type.
                    if (_defaultDataEntryConfig != null &&
                        _documentTypeComboBox.FindStringExact(documentType) == -1)
                    {
                        _temporaryDocumentType = documentType;
                        _documentTypeComboBox.Items.Insert(0, documentType);
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
        /// <param name="listenForChanges"><see langword="true"/> to watch for changes to the
        /// <see cref="IAttribute"/>'s value, <see langword="false"/> otherwise.</param>
        string GetDocumentType(IUnknownVector attributes, bool listenForChanges)
        {
            string documentType = "";
            _documentTypeAttribute = null;

            // Remove event registration from the last DocumentType attribute we found.
            if (_documentTypeAttributeStatusInfo != null)
            {
                _documentTypeAttributeStatusInfo.AttributeValueModified -=
                    HandleDocumentTypeAttributeValueModified;
                _documentTypeAttributeStatusInfo = null;
            }

            // Search for the DocumentType attribute.
            IUnknownVector matchingAttributes =
                DataEntryMethods.AFUtility.QueryAttributes(
                    attributes, "DocumentType", false);

            int matchingAttributeCount = matchingAttributes.Size();
            if (matchingAttributeCount > 0)
            {
                _documentTypeAttribute = (IAttribute)matchingAttributes.At(0);
            }

            // If one was found, retrieve the document type and register to be notified of changes
            // if specified.
            if (_documentTypeAttribute != null)
            {
                documentType = _documentTypeAttribute.Value.String;

                if (listenForChanges)
                {
                    _documentTypeAttributeStatusInfo =
                        AttributeStatusInfo.GetStatusInfo(_documentTypeAttribute);
                    _documentTypeAttributeStatusInfo.AttributeValueModified +=
                        HandleDocumentTypeAttributeValueModified;
                }
            }

            return documentType;
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event for the
        /// _documentTypeComboBox.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleDocumentTypeSelectedIndexChanged(object sender, EventArgs e)
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
                // Debated on whether this should be displayed instead, since this may be something
                // that happens in the midst of verification not but this could happen during
                // document load and even it if doesn't, it could indicate that the document will
                // not be able to be correctly saved.
                OnConfigurationChangeError(ee, true);
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
                // Debated on whether this should be displayed instead, since this may be something
                // that happens in the midst of verification not but this could happen during
                // document load and even it if doesn't, it could indicate that the document will
                // not be able to be correctly saved.
                OnConfigurationChangeError(ee, true);
            }
        }

        /// <summary>
        /// Called when [configuration changing].
        /// </summary>
        /// <returns></returns>
        bool OnConfigurationChanging()
        {
            var eventArgs = new CancelEventArgs(false);

            ConfigurationChanging?.Invoke(this, eventArgs);

            return !eventArgs.Cancel;
        }

        /// <summary>
        /// Called when [configuration changed].
        /// </summary>
        /// <param name="newDataEntryConfig"></param>
        /// <param name="oldDataEntryConfig"></param>
        void OnConfigurationChanged(DataEntryConfiguration oldDataEntryConfig,
            DataEntryConfiguration newDataEntryConfig)
        {
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(oldDataEntryConfig, newDataEntryConfig));
        }

        /// <summary>
        /// Called when [configuration change error].
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> representing the error.</param>
        /// <param name="canProcessingContinue">if set to <c>true</c> [can processing continue].</param>
        void OnConfigurationChangeError(ExtractException ee, bool canProcessingContinue)
        {
            ConfigurationChangeError?.Invoke(this,
                new VerificationExceptionGeneratedEventArgs(ee, canProcessingContinue));
        }

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
                    _documentTypeConfigurations = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldDataEntryConfig">The old data entry configuration.</param>
        /// <param name="newDataEntryConfig">The new data entry configuration.</param>
        public ConfigurationChangedEventArgs(DataEntryConfiguration oldDataEntryConfig,
            DataEntryConfiguration newDataEntryConfig)
        {
            OldDataEntryConfiguration = oldDataEntryConfig;
            NewDataEntryConfiguration = newDataEntryConfig;
        }

        /// <summary>
        /// Gets the old data entry configuration.
        /// </summary>
        /// <value>
        /// The old data entry configuration.
        /// </value>
        public DataEntryConfiguration OldDataEntryConfiguration
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the new data entry configuration.
        /// </summary>
        /// <value>
        /// The new data entry configuration.
        /// </value>
        public DataEntryConfiguration NewDataEntryConfiguration
        {
            get;
            private set;
        }
    }
}