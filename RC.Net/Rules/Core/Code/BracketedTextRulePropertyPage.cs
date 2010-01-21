using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.Rules
{
    /// <summary>
    /// A class representing the Property page for the <see cref="BracketedTextRule"/> object.
    /// </summary>
    public partial class BracketedTextRulePropertyPage : UserControl, IPropertyPage
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
         static readonly string _OBJECT_NAME = typeof(BracketedTextRulePropertyPage).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="BracketedTextRule"/> that settings will be applied to.
        /// </summary>
        readonly BracketedTextRule _bracketedTextRule;

        /// <summary>
        /// Flag to indicate if the object is dirty.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="BracketedTextRulePropertyPage"/> class.
        /// </summary>
        public BracketedTextRulePropertyPage(BracketedTextRule bracketedTextRule)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23190",
                    _OBJECT_NAME);

                _bracketedTextRule = bracketedTextRule;
                InitializeComponent();

                _squareBracketsCheckBox.Checked = _bracketedTextRule.MatchSquareBrackets;
                _curlyBracketsCheckBox.Checked = _bracketedTextRule.MatchCurlyBrackets;
                _curvedBracketsCheckBox.Checked = _bracketedTextRule.MatchCurvedBrackets;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22060",
                    "Failed initializing BracketedTextRulePropertyPage!", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> which sent the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> association with the event.</param>
        void HandleCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Raise the property page modified event
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22059", ex);
                ee.AddDebugData("Event Arguments", e, false);
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
        protected virtual void OnPropertyPageModified()
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
                ExtractException.Display("ELI22165", ex);
            }
        }

        /// <summary>
        /// Applies the changes made in the UI to the underlying <see cref="BracketedTextRule"/>
        /// object.
        /// </summary>
        public void Apply()
        {
            try
            {
                // Ensure the settings are valid
                if (!IsValid)
                {
                    MessageBox.Show("Cannot apply changes. Settings are invalid.", "Invalid settings",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    return;
                }

                // Apply the settings to the BracketedTextRule object
                _bracketedTextRule.MatchSquareBrackets = _squareBracketsCheckBox.Checked;
                _bracketedTextRule.MatchCurlyBrackets = _curlyBracketsCheckBox.Checked;
                _bracketedTextRule.MatchCurvedBrackets = _curvedBracketsCheckBox.Checked;

                // Reset the dirty flag
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29227", ex);
            }
        }

        /// <summary>
        /// Gets whether the UI has changed and thus whether changes need to be
        /// applied to the underlying <see cref="BracketedTextRule"/> object.
        /// </summary>
        /// <returns><see langword="true"/> if the changes have been made and not applied.</returns>
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
                // Ensure at least one checkbox is checked
                return _squareBracketsCheckBox.Checked || _curlyBracketsCheckBox.Checked ||
                    _curvedBracketsCheckBox.Checked;
            }
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        public void SetFocusToFirstControl()
        {
            // Set focus to the first check box
            groupBox1.Controls[0].Focus();
        }

        #endregion
    }
}
