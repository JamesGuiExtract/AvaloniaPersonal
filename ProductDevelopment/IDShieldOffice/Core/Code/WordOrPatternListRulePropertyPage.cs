using Extract;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IDShieldOffice
{
    /// <summary>
    /// Represents the property page of a <see cref="WordOrPatternsListRule"/>.
    /// </summary>
    internal partial class WordOrPatternListRulePropertyPage : UserControl, IPropertyPage
    {
        #region WordOrPatternListRulePropertyPage Constants

        /// <summary>
        /// The types of word/pattern list files that can be opened.
        /// </summary>
        private static readonly string _PATTERN_LIST_TYPES =
            "Text documents (*.txt)|*.txt*|" +
            "All files (*.*)|*.*||";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(WordOrPatternListRulePropertyPage).ToString();

        #endregion WordOrPatternListRulePropertyPage Constants

        #region WordOrPatternListRulePropertyPage Fields

        /// <summary>
        /// The rule associated with the property page.
        /// </summary>
        WordOrPatternsListRule _rule;

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        bool _dirty;

        #endregion WordOrPatternListRulePropertyPage Fields

        #region WordOrPatternListRulePropertyPage Constructors

        /// <summary>
        /// Initializes a new <see cref="WordOrPatternListRulePropertyPage"/> class.
        /// </summary>
        public WordOrPatternListRulePropertyPage(WordOrPatternsListRule rule)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23206",
                    _OBJECT_NAME);

                InitializeComponent();

                // Store the rule object
                _rule = rule;

                // Set the UI elements
                _wordsOrPatternsTextBox.Text = _rule.Text;
                _matchCaseCheckBox.Checked = _rule.MatchCase;
                _isRegexCheckBox.Checked = _rule.TreatAsRegularExpression;

                // Set the state of the UI elements
                UpdateState();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23207", ex);
            }
        }

        #endregion WordOrPatternListRulePropertyPage Constructors

        #region WordOrPatternListRulePropertyPage Methods

        /// <summary>
        /// Updates the state of the controls on the property page.
        /// </summary>
        private void UpdateState()
        {
            // Enable or disable the export button, depending on whether the textbox is empty.
            // [IDSO #13]
            _exportButton.Enabled = _wordsOrPatternsTextBox.Text.Length > 0;
        }

        #endregion WordOrPatternListRulePropertyPage Methods

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
                ExtractException.Display("ELI22163", ex);
            }
        }

        /// <summary>
        /// Applies the changes to the <see cref="WordOrPatternsListRule"/>.
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
            _rule.Text = _wordsOrPatternsTextBox.Text;
            _rule.MatchCase = _matchCaseCheckBox.Checked;
            _rule.TreatAsRegularExpression = _isRegexCheckBox.Checked;

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
                // A word or pattern must be specified.
                return _wordsOrPatternsTextBox.Text.Length > 0;
            }
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        public void SetFocusToFirstControl()
        {
            // Set the focus to the textbox and the cursor to the end of the string [IDSD #92]
            _wordsOrPatternsTextBox.Focus();
            _wordsOrPatternsTextBox.SelectionStart = _wordsOrPatternsTextBox.Text.Length;
        }

        #endregion IPropertyPage Members

        /// <summary>
        /// Handles the import button click event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        private void _importButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the open file dialog
                using (OpenFileDialog fileDialog = new OpenFileDialog())
                {
                    fileDialog.Filter = _PATTERN_LIST_TYPES;
                    fileDialog.Title = "Open Word or Pattern List";

                    // Show the dialog and bail if the user selected cancel
                    if (fileDialog.ShowDialog() == DialogResult.Cancel)
                    {
                        return;
                    }

                    // Read the words/patterns from the file
                    _wordsOrPatternsTextBox.Text = File.ReadAllText(fileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22166", ex);
            }
        }

        /// <summary>
        /// Handles the export button click event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        private void _exportButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the save file dialog
                using (SaveFileDialog fileDialog = new SaveFileDialog())
                {
                    fileDialog.Filter = _PATTERN_LIST_TYPES;
                    fileDialog.Title = "Save Word or Pattern List";
                    fileDialog.AddExtension = true;
                    fileDialog.DefaultExt = "txt";

                    // Show the dialog and bail if the user selected cancel
                    if (fileDialog.ShowDialog() == DialogResult.Cancel)
                    {
                        return;
                    }

                    // Write the words/patterns to the file
                    File.WriteAllText(fileDialog.FileName, _wordsOrPatternsTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22168", ex);
            }
        }

        /// <summary>
        /// Handles the words/patterns textbox changed event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        private void _wordsOrPatternsTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Raise the property page modified event
                OnPropertyPageModified();

                UpdateState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22169", ex);
            }
        }

        /// <summary>
        /// Handles the match case checkbox changed event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        private void _matchCaseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Raise the property page modified event
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22170", ex);
            }
        }

        /// <summary>
        /// Handles the treat as regular expression checkbox changed event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        private void _isRegexCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Raise the property page modified event
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22171", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LinkLabel.LinkClicked"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LinkLabel.LinkClicked"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LinkLabel.LinkClicked"/> event.</param>
        void HandleRegexHelpLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            IDShieldOfficeForm.ShowRegexHelp(this.TopLevelControl);
        }
    }
}
