using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a dialog that allows the user to edit a highlight's text.
    /// </summary>
    public partial class EditHighlightTextForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(EditHighlightTextForm).ToString();

        #endregion Constants

        #region EditHighlightTextForm Constructors

        /// <summary>
        /// Initializes a new <see cref="EditHighlightTextForm"/> class.
        /// </summary>
        /// <param name="text">The initial highlight text.</param>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public EditHighlightTextForm(string text)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23175",
					_OBJECT_NAME);

                InitializeComponent();

                // Set the highlight text textbox
                _highlightTextBox.Text = text;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23176", ex);
            }
        }

        #endregion EditHighlightTextForm Constructors

        #region EditHighlightTextForm Properties

        /// <summary>
        /// Gets the text in highlight text textbox.
        /// </summary>
        /// <value>The text in highlight text textbox.</value>
        public string HighlightText
        {
            get
            {
                return _highlightTextBox.Text;
            }
        }

        #endregion EditHighlightTextForm Properties

    }
}
