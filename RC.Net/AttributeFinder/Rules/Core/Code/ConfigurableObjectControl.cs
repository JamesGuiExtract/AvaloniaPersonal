using Extract.Interop;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

// NOTE:
// If there are other reusable controls that are created, we should consider breaking them out into
// a separate assembly (perhaps Extract.AttributeFinder.Forms).
namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// A control that allows selection and configuration of an <see cref="ICategorizedComponent"/>
    /// that may implement either <see cref="IConfigurableObject"/> or
    /// <see cref="ISpecifyPropertyPages"/>.
    /// </summary>
    [CLSCompliant(false)]
    public partial class ConfigurableObjectControl : UserControl
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ConfigurableObjectControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// The COM category from which objects may be selected.
        /// </summary>
        string _categoryName;

        /// <summary>
        /// The currently selected configurable object.
        /// </summary>
        ICategorizedComponent _configurableObject;

        /// <summary>
        /// A map of all configurable object names to their ProgIds
        /// </summary>
        Dictionary<string, string> _namesToProgIds = new Dictionary<string, string>();

        /// <summary>
        /// A map of all configurable object names to their the most recently configured
        /// configurable objects instance.
        /// </summary>
        Dictionary<string, ICategorizedComponent> _cachedObjects =
            new Dictionary<string, ICategorizedComponent>();

        /// <summary>
        /// <see langword="true"/> if the control has been loaded, otherwise <see langword="false"/>.
        /// </summary>
        bool _isLoaded;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurableObjectControl"/> class.
        /// </summary>
        public ConfigurableObjectControl()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleWritingCoreObjects, "ELI33476",
                    _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33488");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the COM category from which objects may be selected.
        /// </summary>
        /// <value>The COM category from which objects may be selected.</value>
        public string CategoryName
        {
            get
            {
                return _categoryName;
            }

            set
            {
                try
                {
                    if (value != _categoryName)
                    {
                        _categoryName = value;

                        _namesToProgIds.Clear();

                        // Compile the names and ProgIds of all the object in the new category.
                        CategoryManager categoryManager = new CategoryManager();
                        StrToStrMap objectMap =
                            categoryManager.GetDescriptionToProgIDMap1(_categoryName);

                        int size = objectMap.Size;
                        for (int i = 0; i < size; i++)
                        {
                            string name;
                            string progId;
                            objectMap.GetKeyValue(i, out name, out progId);
                            _namesToProgIds[name] = progId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33737");
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected configurable object.
        /// </summary>
        /// <value>
        /// The currently selected configurable object.
        /// </value>
        public ICategorizedComponent ConfigurableObject
        {
            get
            {
                return _configurableObject;
            }

            set
            {
                try
                {
                    if (_configurableObject != value)
                    {
                        _configurableObject = value;

                        // Cache this object instance in case the user chooses a different type
                        // but decides to come back to this one.
                        string name = _configurableObject.GetComponentDescription();
                        _cachedObjects[name] = _configurableObject;

                        // If the form is loaded, indicate the selection in the combo box.
                        if (_isLoaded)
                        {
                            _objectSelectionComboBox.SelectedItem = name;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33495");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is configured.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is configured; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsConfigured
        {
            get
            {
                try
                {
                    if (ConfigurableObject == null)
                    {
                        return false;
                    }

                    IMustBeConfiguredObject configurable = ConfigurableObject as IMustBeConfiguredObject;
                    return (configurable == null || configurable.IsConfigured());
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33499");
                }
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                ExtractException.Assert("ELI33736",
                    "Configurable object list has not been initialized.",
                    _inDesignMode || _namesToProgIds.Count > 0);

                // Populate the combo box with the name of all objects in the current category.
                _objectSelectionComboBox.Items.AddRange(_namesToProgIds.Keys.ToArray());

                // Set the combo box to the current object.
                if (ConfigurableObject != null)
                {
                    ICategorizedComponent categorizedComponent =
                        (ICategorizedComponent)ConfigurableObject;
                    _objectSelectionComboBox.SelectedItem = categorizedComponent.GetComponentDescription();
                }

                // Update button and configuration reminder based on ConfigurableObject.
                UpdateButtonAndReminder();

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33487");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="_objectSelectionComboBox"/> <see cref="ComboBox.SelectedIndexChanged"/>
        /// event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleObjectSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedName = _objectSelectionComboBox.Text;

                // Look for any instances that have already been created/configured.
                ICategorizedComponent configurableObject = null;
                if (!_cachedObjects.TryGetValue(selectedName, out configurableObject))
                {
                    // If one was not already created, create an instance now.
                    string progId = null;
                    _namesToProgIds.TryGetValue(selectedName, out progId);

                    Type objectType = Type.GetTypeFromProgID(progId);
                    if (objectType == null)
                    {
                        ExtractException ee = new ExtractException("ELI33486",
                            "Failed to find registered configurable object type.");
                        ee.AddDebugData("Object name", selectedName, false);
                        ee.AddDebugData("Object type", progId, false);
                        throw ee;
                    }

                    configurableObject = (ICategorizedComponent)Activator.CreateInstance(objectType);
                    _cachedObjects[selectedName] = configurableObject;
                }

                ConfigurableObject = configurableObject;
                
                // Update button and configuration reminder based on the new selection.
                UpdateButtonAndReminder();
            }
            catch (Exception ex)
            {
                _configureButton.Enabled = false;
                ex.ExtractDisplay("ELI33489");
            }
        }

        /// <summary>
        /// Handles the configure button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleConfigureButtonClick(object sender, EventArgs e)
        {
            try
            {
                ICategorizedComponent configuredObject =
                    ComUtilities.ConfigureComObject(ConfigurableObject);

                // If the user clicked okay on the object configuration, we'll get the updated
                // configuration back. Use it.
                if (configuredObject != null)
                {
                    ConfigurableObject = configuredObject;
                    _cachedObjects[_objectSelectionComboBox.Text] = ConfigurableObject;
                }

                // Update button and configuration reminder based on the new configuration.
                UpdateButtonAndReminder();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33490");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates the configuration button state and whether the configuration reminder label
        /// should be visible based on the currently selected object.
        /// </summary>
        void UpdateButtonAndReminder()
        {
            if (_inDesignMode)
            {
                _configureButton.Enabled = true;
                _configurationReminderLabel.Visible = true;
            }
            else
            {
                // Enable the configure button if the object is configurable.
                _configureButton.Enabled = ComUtilities.IsComObjectConfigurable(ConfigurableObject);

                IMustBeConfiguredObject mustBeConfiguredObject =
                    ConfigurableObject as IMustBeConfiguredObject;
                if (mustBeConfiguredObject != null && !mustBeConfiguredObject.IsConfigured())
                {
                    _configurationReminderLabel.Visible = true;
                }
                else
                {
                    _configurationReminderLabel.Visible = false;
                }
            }
        }

        #endregion Private Members
    }
}
