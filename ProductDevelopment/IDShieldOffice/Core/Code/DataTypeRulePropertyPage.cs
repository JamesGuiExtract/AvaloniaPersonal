using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Rules
{
    /// <summary>
    /// Represents a property page that configure the <see cref="DataTypeRule"/>.
    /// </summary>
    public partial class DataTypeRulePropertyPage : UserControl, IPropertyPage
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataTypeRulePropertyPage).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The rule with which this property page is associated.
        /// </summary>
        readonly DataTypeRule _rule;

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataTypeRulePropertyPage"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public DataTypeRulePropertyPage(DataTypeRule rule)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23194",
                    _OBJECT_NAME);

                InitializeComponent();

                // Store the rule
                _rule = rule;

                // Set the UI elements
                Point checkboxLocation = _dataTypesGroupBox.DisplayRectangle.Location;
                checkboxLocation.Offset(_dataTypesGroupBox.Margin.Left, _dataTypesGroupBox.Margin.Top);

                foreach (string dataType in DataTypeRule.ValidDataTypes.Keys)
                {
                    // Create the checkbox to add to the groupbox
                    CheckBox checkBox = new CheckBox();
                    checkBox.AutoSize = true;
                    checkBox.Text = dataType;
                    checkBox.Location = checkboxLocation;

                    // Set the checked state depending on the rule
                    checkBox.Checked = _rule.DataTypeList.Contains(dataType);

                    // Add an event handler
                    checkBox.CheckedChanged += HandleCheckedChanged;

                    // Add the check box to the groupbox
                    _dataTypesGroupBox.Controls.Add(checkBox);

                    // Increment the next checkbox location
                    checkboxLocation.Offset(0, checkBox.Height + checkBox.Margin.Top);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23195", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Raise the property page modified event
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22131", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region IPropertyPage Members

        /// <summary>
        /// Event raised when the dirty flag is set.
        /// </summary>
        public event EventHandler PropertyPageModified;

        /// <summary>
        /// Raises the PropertyPageModified event.
        /// </summary>
        void OnPropertyPageModified()
        {
            try
            {
                // Set the dirty flag
                _dirty = true;

                // If there is a listener for the event then raise it.
                if (PropertyPageModified != null)
                {
                    PropertyPageModified(this, null);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22164", ex);
            }
        }

        /// <summary>
        /// Applies the changes to the <see cref="DataTypeRule"/>.
        /// </summary>
        public void Apply()
        {
            // Ensure the settings are valid
            if (!IsValid)
            {
                MessageBox.Show("Cannot apply changes. Settings are invalid.", "Invalid settings",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                return;
            }

            // Store the settings
            _rule.DataTypeList.Clear();
            foreach (CheckBox checkBox in _dataTypesGroupBox.Controls)
            {
                if (checkBox.Checked)
	            {
                    _rule.DataTypeList.Add(checkBox.Text);
	            }
            }

            // Reset the dirty flag
            _dirty = false;
        }

        /// <summary>
        /// Gets whether the settings on the property page have been modified.
        /// </summary>
        /// <return><see langword="true"/> if the settings on the property page have been modified;
        /// <see langword="false"/> if they have not been modified.</return>
        public bool IsDirty
        {
            get
            {
                return _dirty;
            }
        }

        /// <summary>
        /// Gets whether the user-specified settings on the property page are valid.
        /// </summary>
        /// <value><see langword="true"/> if the user-specified settings are valid; 
        /// <see langword="false"/> if the settings are not valid.</value>
        public bool IsValid
        {
            get
            {
                // At least one checkbox must be checked
                foreach (CheckBox checkBox in _dataTypesGroupBox.Controls)
                {
                    if (checkBox.Checked)
                    {
                        return true;
                    }
                }

                // No checkbox is checked
                return false;
            }
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        public void SetFocusToFirstControl()
        {
            // Set focus to the first check box
            _dataTypesGroupBox.Controls[0].Focus();
        }

        #endregion
    }
}
