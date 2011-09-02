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
    /// A control that allows selection and configuration of an <see cref="IAttributeSelector"/>.
    /// </summary>
    [CLSCompliant(false)]
    public partial class AttributeSelectorControl : UserControl
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(AttributeSelectorControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The currently selected <see cref="AttributeSelector"/>.
        /// </summary>
        IAttributeSelector _attributeSelector;

        /// <summary>
        /// A map of all <see cref="AttributeSelector"/> names to their ProgIds
        /// </summary>
        Dictionary<string, string> _namesToProgIds = new Dictionary<string, string>();

        /// <summary>
        /// A map of all <see cref="AttributeSelector"/> names to their the most recently configured
        /// <see cref="AttributeSelector"/>s instance.
        /// </summary>
        Dictionary<string, IAttributeSelector> _cachedSelectors =
            new Dictionary<string, IAttributeSelector>();

        /// <summary>
        /// <see langword="true"/> if the control has been loaded, otherwise <see langword="false"/>.
        /// </summary>
        bool _isLoaded;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeSelectorControl"/> class.
        /// </summary>
        public AttributeSelectorControl()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI33476",
                    _OBJECT_NAME);

                // Compile the names and ProgIds of all attribute selectors.
                CategoryManager categoryManager = new CategoryManager();
                StrToStrMap selectorMap = categoryManager.GetDescriptionToProgIDMap1(
                    ExtractCategories.AttributeSelectorsName);

                int size = selectorMap.Size;
                for (int i = 0; i < size; i++)
                {
                    string name;
                    string progId;
                    selectorMap.GetKeyValue(i, out name, out progId);
                    _namesToProgIds[name] = progId;
                }

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
        /// Gets or sets the attribute selector.
        /// </summary>
        /// <value>
        /// The attribute selector.
        /// </value>
        public IAttributeSelector AttributeSelector
        {
            get
            {
                return _attributeSelector;
            }

            set
            {
                try
                {
                    if (_attributeSelector != value)
                    {
                        _attributeSelector = value;

                        // Cache this selector instance in case the user chooses a different type
                        // but decides to come back to this one.
                        ICategorizedComponent categorizedComponent =
                            (ICategorizedComponent)_attributeSelector;
                        string name = categorizedComponent.GetComponentDescription();
                        _cachedSelectors[name] = _attributeSelector;

                        // If the form is loaded, indicate the selection in the combo box.
                        if (_isLoaded)
                        {
                            _selectorComboBox.SelectedItem = name;
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
                    if (AttributeSelector == null)
                    {
                        return false;
                    }

                    IMustBeConfiguredObject configurable = AttributeSelector as IMustBeConfiguredObject;
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

                // Populate the combo box with the name of all attribute selectors.
                _selectorComboBox.Items.AddRange(_namesToProgIds.Keys.ToArray());

                // Set the combo box to the current attribute selector.
                if (AttributeSelector != null)
                {
                    ICategorizedComponent categorizedComponent =
                        (ICategorizedComponent)AttributeSelector;
                    _selectorComboBox.SelectedItem = categorizedComponent.GetComponentDescription();
                }

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
        /// Handles the <see cref="_selectorComboBox"/> <see cref="ComboBox.SelectedIndexChanged"/>
        /// event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAttributeSelectorSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedName = _selectorComboBox.Text;

                // Look for any instances that have already been created/configured.
                IAttributeSelector selector = null;
                if (!_cachedSelectors.TryGetValue(selectedName, out selector))
                {
                    // If one was not already created, create an instance now.
                    string progId = null;
                    _namesToProgIds.TryGetValue(selectedName, out progId);

                    Type selectorType = Type.GetTypeFromProgID(progId);
                    if (selectorType == null)
                    {
                        ExtractException ee = new ExtractException("ELI33486",
                            "Failed to find registered attribute selector type.");
                        ee.AddDebugData("Selector Name", selectedName, false);
                        ee.AddDebugData("Selector Type", progId, false);
                        throw ee;
                    }

                    selector = (IAttributeSelector)Activator.CreateInstance(selectorType);
                    _cachedSelectors[selectedName] = selector;
                }

                AttributeSelector = selector;

                // Enable the configure button if the selector is configurable.
                _configureButton.Enabled = ComUtilities.IsComObjectConfigurable(AttributeSelector);
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
                IAttributeSelector configuredSelector =
                    ComUtilities.ConfigureComObject(AttributeSelector);

                // If the user clicked okay on the selector configuration, we'll get the updated
                // configuration back. Use it.
                if (configuredSelector != null)
                {
                    AttributeSelector = configuredSelector;
                    _cachedSelectors[_selectorComboBox.Text] = AttributeSelector;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33490");
            }
        }

        #endregion Event Handlers
    }
}
