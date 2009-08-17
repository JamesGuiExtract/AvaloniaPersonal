using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a dialog that allows the user to apply changes to a property page.
    /// </summary>
    public partial class PropertyPageForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(PropertyPageForm).ToString();

        #endregion Constants

        #region PropertyPageForm Fields

        /// <summary>
        /// The property page to present on the <see cref="PropertyPageForm"/>
        /// </summary>
        IPropertyPage _propertyPage;

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion PropertyPageForm Fields

        #region PropertyPageForm Constructors

        /// <summary>
        /// Initializes a new <see cref="PropertyPageForm"/> class.
        /// </summary>
        /// <param name="title">The text to display on the title bar.</param>
        /// <param name="propertyPage">The property page to display on the 
        /// <see cref="PropertyPageForm"/>.</param>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PropertyPageForm(string title, IPropertyPage propertyPage)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23152");

                InitializeComponent();

                // Set the title of the form
                this.Text = title;

                // Store the property page
                _propertyPage = propertyPage;

                // Add the property page to the split container
                FormsMethods.DockControlIntoContainer(
                    (Control)_propertyPage, this, _splitContainer.Panel1);

                // Handle PropertyPageModified events
                _propertyPage.PropertyPageModified += HandlePropertyPageModified;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23153", ex);
            }
        }

        #endregion PropertyPageForm Constructors

        #region PropertyPageForm Event Handlers

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23305", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        private void HandleOkButtonClick(object sender, EventArgs e)
        {
            // Check if changes need to be applied
            if (_propertyPage.IsDirty)
            {
                // Attempt to apply the changes
                _propertyPage.Apply();

                // Stop here if the changes weren't applied
                if (_propertyPage.IsDirty)
                {
                    // No need to display a message. The Apply method has already done this.
                    return;
                }
            }

            // Return the OK result
            this.DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the <see cref="IPropertyPage.PropertyPageModified"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="IPropertyPage.PropertyPageModified"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="IPropertyPage.PropertyPageModified"/> event.</param>
        private void HandlePropertyPageModified(object sender, EventArgs e)
        {
            // Enable the OK button based on the state of the property page
            _okButton.Enabled = _propertyPage.IsValid;
        }

        #endregion PropertyPageForm Event Handlers
    }
}