using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a dialog that allows the user to select modify pdf file settings.
    /// </summary>
    internal partial class ModifyPdfFileSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The <see cref="ModifyPdfFileTaskSettings"/> that will be modified by the dialog.
        /// </summary>
        readonly ModifyPdfFileTaskSettings _settings;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="ModifyPdfFileSettingsDialog"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileSettingsDialog"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public ModifyPdfFileSettingsDialog()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyPdfFileSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings for modifying a PDF file.</param>
        public ModifyPdfFileSettingsDialog(ModifyPdfFileTaskSettings settings)
        {
            try
            {
                InitializeComponent();

                _settings = settings ?? new ModifyPdfFileTaskSettings();

                _pdfFilePathTagsButton.PathTags = new FileActionManagerPathTags();
                _dataFilePathTagsButton.PathTags = new FileActionManagerPathTags();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36273");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _pdfFileTextBox.Text = _settings.PdfFile ?? "";
                _pdfFileTextBox.Select(_pdfFileTextBox.Text.Length, 0);
                _removeAnnotationsCheckBox.Checked = _settings.RemoveAnnotations;
                _addHyperLinksCheckBox.Checked = _settings.AddHyperlinks;
                _hyperlinkAttributesTextBox.Text = _settings.HyperlinkAttributes;
                _useValueAsAddressRadioButton.Checked = _settings.UseValueAsAddress;
                _useStaticAddressRadioButton.Checked = !_settings.UseValueAsAddress;
                _hyperlinkAddressTextBox.Text = _settings.HyperlinkAddress;
                _dataFileTextBox.Text = _settings.DataFileName;

                SetControlsEnabledState();

                _addHyperLinksCheckBox.CheckedChanged +=
                    (sender, eventArgs) => SetControlsEnabledState();
                _useStaticAddressRadioButton.CheckedChanged +=
                    (sender, eventArgs) => SetControlsEnabledState();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29635", ex);
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the Ok button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleOkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                // Store the values
                _settings.PdfFile = _pdfFileTextBox.Text;
                _settings.RemoveAnnotations = _removeAnnotationsCheckBox.Checked;
                _settings.AddHyperlinks = _addHyperLinksCheckBox.Checked;
                _settings.HyperlinkAttributes = _hyperlinkAttributesTextBox.Text;
                _settings.UseValueAsAddress = _useValueAsAddressRadioButton.Checked;
                _settings.HyperlinkAddress = _hyperlinkAddressTextBox.Text;
                _settings.DataFileName = _dataFileTextBox.Text;

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29636", ex);
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Checks the settings in the dialog and will prompt the user if there are invalid
        /// configurations.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid.</returns>
        bool WarnIfInvalid()
        {
            var tagManager = new FAMTagManagerClass();

            if (string.IsNullOrEmpty(_pdfFileTextBox.Text) ||
                tagManager.StringContainsInvalidTags(_pdfFileTextBox.Text))
            {
                MessageBox.Show("Please specify a valid input PDF file.", "Invalid PDF file",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _pdfFileTextBox.Focus();

                return true;
            }

            if (!_removeAnnotationsCheckBox.Checked && !_addHyperLinksCheckBox.Checked)
            {
                MessageBox.Show("Task has not been configured to do anything",
                    "Incomplete configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _removeAnnotationsCheckBox.Focus();

                return true;
            }

            if (_addHyperLinksCheckBox.Checked)
            {
                if (string.IsNullOrWhiteSpace(_hyperlinkAttributesTextBox.Text))
                {
                    MessageBox.Show(
                        "Please specify which attributes should be used to create hyperlinks",
                        "Missing attribute names", MessageBoxButtons.OK, MessageBoxIcon.None,
                        MessageBoxDefaultButton.Button1, 0);
                    _hyperlinkAttributesTextBox.Focus();

                    return true;
                }

                if (_useStaticAddressRadioButton.Checked &&
                    string.IsNullOrWhiteSpace(_hyperlinkAddressTextBox.Text))
                {
                    MessageBox.Show(
                        "Please specify the address that should be used for hyperlinks",
                        "Missing hyperlink address", MessageBoxButtons.OK, MessageBoxIcon.None,
                        MessageBoxDefaultButton.Button1, 0);
                    _hyperlinkAddressTextBox.Focus();

                    return true;
                }

                if (string.IsNullOrWhiteSpace(_dataFileTextBox.Text) ||
                    tagManager.StringContainsInvalidTags(_dataFileTextBox.Text))
                {
                    MessageBox.Show("Please specify a valid data file containing the attributes " +
                        "to use for creating hyperlinks", "Invalid data file", MessageBoxButtons.OK,
                        MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    _dataFileTextBox.Focus();

                    return true;
                }
            }

            return false;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the <see cref="ModifyPdfFileTaskSettings"/> that have been
        /// configured by this <see cref="Form"/>.
        /// </summary>
        /// <value>The <see cref="ModifyPdfFileTaskSettings"/> that have been
        /// configured by this <see cref="Form"/>.</value>
        public ModifyPdfFileTaskSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        #endregion Properties

        #region Private Members

        /// <summary>
        /// Sets the enabled state of UI controls based on the current settings.
        /// </summary>
        void SetControlsEnabledState()
        {
            try
            {
                _hyperlinkAttributesTextBox.Enabled = _addHyperLinksCheckBox.Checked;
                _useValueAsAddressRadioButton.Enabled = _addHyperLinksCheckBox.Checked;
                _useStaticAddressRadioButton.Enabled = _addHyperLinksCheckBox.Checked;
                _hyperlinkAddressTextBox.Enabled =
                    _addHyperLinksCheckBox.Checked && _useStaticAddressRadioButton.Checked;
                _dataFileTextBox.Enabled = _addHyperLinksCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36274");
            }
        }

        #endregion Private Members
    }
}