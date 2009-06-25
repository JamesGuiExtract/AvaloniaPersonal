using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Forms for configuring a <see cref="LabDEOrderMapper"/> object.
    /// </summary>
    public partial class LabDEOrderMapperConfigurationForm : Form
    {
        /// <summary>
        /// The database file that was selected by this property page (this file name may
        /// contain document tags that need to be expanded - ex. &lt;SourceDocName&gt;)
        /// </summary>
        private string _databaseFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperConfigurationForm"/> class.
        /// </summary>
        public LabDEOrderMapperConfigurationForm() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperConfigurationForm"/> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to display in the
        /// text box.</param>
        public LabDEOrderMapperConfigurationForm(string databaseFile)
        {
            try
            {
                InitializeComponent();

                _databaseFile = databaseFile;

                _textDatabaseFile.Text = _databaseFile ?? "";
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26197", ex);
            }
        }

        #region Properties

        /// <summary>
        /// Gets the database file name.
        /// </summary>
        public string DatabaseFileName
        {
            get
            {
                return _databaseFile;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="PathTagsButton.TagSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        private void HandlePathTagsButtonSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                // Set the selected text based on the tag
                _textDatabaseFile.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26199", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the OK button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleOkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Store the text from the text box
                _databaseFile = _textDatabaseFile.Text;

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26200", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the cancel button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleCancelClicked(object sender, EventArgs e)
        {
            try
            {
                _databaseFile = "";

                this.DialogResult = DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26201", ex);
            }
        }

        #endregion Event Handlers
    }
}