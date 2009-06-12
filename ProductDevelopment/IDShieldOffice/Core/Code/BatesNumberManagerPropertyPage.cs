using Extract;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice
{
    /// <summary>
    /// Represents the property page of a <see cref="BatesNumberManager"/>.
    /// </summary>
    public partial class BatesNumberManagerPropertyPage : UserControl, IPropertyPage
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BatesNumberManagerPropertyPage).ToString();

        #endregion Constants

        #region BatesNumberManagerPropertyPage Fields

        /// <summary>
        /// The <see cref="BatesNumberManager"/> to which settings will be applied.
        /// </summary>
        readonly BatesNumberManager _batesNumberManager;

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// A dialog that allows the user to set the format of the Bates number.
        /// </summary>
        PropertyPageForm _formatDialog;

        /// <summary>
        /// A dialog that allows the user to set the appearance of the Bates number.
        /// </summary>
        PropertyPageForm _appearanceDialog;

        #endregion BatesNumberManagerPropertyPage Fields

        #region BatesNumberManagerPropertyPage Constructors

        /// <summary>
        /// Initializes a new <see cref="BatesNumberManagerPropertyPage"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        internal BatesNumberManagerPropertyPage(BatesNumberManager batesNumberManager)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23188",
                    _OBJECT_NAME);

                InitializeComponent();

                // Store the Bates number manager
                _batesNumberManager = batesNumberManager;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23209", ex);
            }
        }
        
        #endregion BatesNumberManagerPropertyPage Constructors

        #region BatesNumberManagerPropertyPage Methods

        /// <summary>
        /// Resets the all the values to the values stored in <see cref="_batesNumberManager"/> and 
        /// resets the dirty flag to <see langword="false"/>.
        /// </summary>
        private void RefreshSettings()
        {
            // Set the UI elements
            _requireBatesCheckBox.Checked = _batesNumberManager.RequireBates;           
            
            // Set the sample Bates number
            _sampleBatesNumberTextBox.Text = 
                BatesNumberGenerator.PeekNextNumberString(1, _batesNumberManager.Format);

            // Reset the dirty flag
            _dirty = false;
        }

        #endregion BatesNumberManagerPropertyPage Methods

        #region BatesNumberManagerPropertyPage OnEvents

        /// <summary>
        /// Raises the <see cref="UserControl.Load"/> event.
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/> that contain the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Read the next Bates number from the registry
            _batesNumberManager.Format.NextNumber = RegistryManager.NextBatesNumber;

            // Refresh the UI elements
            RefreshSettings();
        }

        #endregion BatesNumberManagerPropertyPage OnEvents

        #region BatesNumberManagerPropertyPage Event Handlers

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        private void HandleRequireBatesCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Raise the property page modified event.
            OnPropertyPageModified();
        }

        #endregion BatesNumberManagerPropertyPage Event Handlers

        #region IPropertyPage Members

        /// <summary>
        /// Event raised when the dirty flag is set.
        /// </summary>
        public event EventHandler PropertyPageModified;

        /// <summary>
        /// Raises the PropertyPageModified event.
        /// </summary>
        private void OnPropertyPageModified()
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
                ExtractException.Display("ELI22218", ex);
            }
        }

        /// <summary>
        /// Applies the changes to the <see cref="BatesNumberManager"/>.
        /// </summary>
        public void Apply()
        {
            // Ensure the settings are valid
            if (!this.IsValid)
            {
                MessageBox.Show("Cannot apply changes. Settings are invalid.", "Invalid settings",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                return;
            }

            // Store the settings
            _batesNumberManager.RequireBates = _requireBatesCheckBox.Checked;

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
                return true;
            }
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        public void SetFocusToFirstControl()
        {
            // Do nothing
        }

        #endregion

        private void HandleChangeFormatButtonClick(object sender, EventArgs e)
        {
            // Create a new Bates number format form if not already created
            if (_formatDialog == null)
            {
                _formatDialog = new PropertyPageForm("Change Bates number format",
                    new BatesNumberManagerFormatPropertyPage(_batesNumberManager));
                ComponentResourceManager resources =
                    new ComponentResourceManager(typeof(IDShieldOfficeForm));
                _formatDialog.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            }

            // Display the format dialog
            if (_formatDialog.ShowDialog() == DialogResult.OK)
            {
                RefreshSettings();
            }
        }

        private void HandleChangeAppearanceButtonClick(object sender, EventArgs e)
        {
            // Create a new Bates number appearance form if not already created
            if (_appearanceDialog == null)
            {
                _appearanceDialog = new PropertyPageForm(
                    "Change Bates number default position and appearance",
                    new BatesNumberManagerAppearancePropertyPage(_batesNumberManager));
                ComponentResourceManager resources =
                    new ComponentResourceManager(typeof(IDShieldOfficeForm));
                _appearanceDialog.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            }

            // Display the appearance dialog
            _appearanceDialog.ShowDialog();
        }
    }
}
