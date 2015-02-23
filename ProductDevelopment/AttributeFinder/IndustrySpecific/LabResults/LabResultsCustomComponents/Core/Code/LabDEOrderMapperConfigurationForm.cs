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
        public LabDEOrderMapperConfigurationForm() : this(null, false, true, false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabDEOrderMapperConfigurationForm"/> class.
        /// </summary>
        /// <param name="databaseFile">The name of the database file to display in the
        /// text box.</param>
        /// <param name="requireMandatoryTests">Whether mandatory tests are required or not.</param>
        /// <param name="useFilledRequirement">Whether the filled requirement of an order should be
        /// used when deciding whether an order can be used.</param>
        /// <param name="useOutstandingOrders">Whether only orders with codes matching the
        /// provided OutstandingOrderCode attributes should be used. If <see langword="false"/> then
        /// the outstanding order codes will be used if possible but other codes will be considered
        /// if necessary.</param>
        /// <param name="eliminateDuplicateTestSubAttributes">Whether to eliminate duplicate Test
        /// subattributes after mapping is finished.</param>
        public LabDEOrderMapperConfigurationForm(string databaseFile, bool requireMandatoryTests,
            bool useFilledRequirement, bool useOutstandingOrders,
            bool eliminateDuplicateTestSubAttributes)
        {
            try
            {
                InitializeComponent();

                _databaseFile = databaseFile;

                _textDatabaseFile.Text = _databaseFile ?? "";

                _checkRequireMandatoryTests.Checked = requireMandatoryTests;

                _checkUseFilledRequirement.Checked = useFilledRequirement;

                _checkUseOutstandingOrders.Checked = useOutstandingOrders;

                _checkEliminateDuplicateTestSubAttributes.Checked = eliminateDuplicateTestSubAttributes;
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

        /// <summary>
        /// Gets whether mandatory tests are required or not.
        /// </summary>
        public bool RequireMandatoryTests
        {
            get
            {
                return _checkRequireMandatoryTests.Checked;
            }
        }

        /// <summary>
        /// Gets whether to require that orders meet their filled requirement
        /// </summary>
        public bool UseFilledRequirement
        {
            get
            {
                return _checkUseFilledRequirement.Checked;
            }
        }

        /// <summary>
        /// Gets whether to limit orders to be considered based on known, outstanding order codes
        /// </summary>
        public bool UseOutstandingOrders
        {
            get
            {
                return _checkUseOutstandingOrders.Checked;
            }
        }

        /// <summary>
        /// Whether to remove any duplicate Test sub-attributes after the mapping is finished.
        /// </summary>
        public bool EliminateDuplicateTestSubAttributes
        {
            get
            {
                return _checkEliminateDuplicateTestSubAttributes.Checked;
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
