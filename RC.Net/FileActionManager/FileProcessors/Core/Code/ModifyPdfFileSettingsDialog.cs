using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
            InitializeComponent();

            _settings = settings ?? new ModifyPdfFileTaskSettings();
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

                // Currently the remove annotations is set to checked and disabled
                // as the modify task does not make sense without this setting.  In
                // the future we are intending to implement more pdf modification
                // options, at that time the settings for the remove annotations should
                // be read from the settings object.
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

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29636", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> for the <see cref="PathTagsButton"/>.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandlePathTagsTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _pdfFileTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29637", ex);
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
            if (string.IsNullOrEmpty(_pdfFileTextBox.Text))
            {
                MessageBox.Show("Please specify an input PDF file.", "Invalid PDF file",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _pdfFileTextBox.Focus();

                return true;
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
    }
}