using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A dialog for configuring the ModifySourceDocName task.
    /// </summary>
    partial class ModifySourceDocNameInDBSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// Gets or sets the name with tags to rename the SourceDocName
        /// </summary>
        /// <value>The name of the file.</value>
        public string RenameFileTo { get; set; }

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifySourceDocNameInDBSettingsDialog"/> class.
        /// </summary>
        public ModifySourceDocNameInDBSettingsDialog()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifySourceDocNameInDBSettingsDialog"/> class.
        /// </summary>
        /// <param name="renameFileTo">Name of the file.</param>
        public ModifySourceDocNameInDBSettingsDialog(string renameFileTo)
        {
            InitializeComponent();
            RenameFileTo = renameFileTo;
        }

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _renameFileToTextBox.Text = RenameFileTo;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31269", ex);
            }
        }

        /// <summary>
        /// Handles the ok button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Validate the file name
                if (string.IsNullOrWhiteSpace(_renameFileToTextBox.Text))
                {
                    MessageBox.Show("You must specify Rename SourceDocName to string.",
                        "No File Name", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _renameFileToTextBox.Focus();
                    return;
                }
                else if (new FAMTagManager().StringContainsInvalidTags(_renameFileToTextBox.Text))
                {
                    MessageBox.Show("File name contains invalid document tags",
                        "Invalid Tags", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);
                    _renameFileToTextBox.Focus();
                    return;
                }
                else if (MessageBox.Show("Modifying SourceDocName in the database is an advanced option " +
                   "that must be used carefully.  \r\n\nAre you sure you want to change the SourceDocName " +
                   "in the database?", "Usage warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                   MessageBoxDefaultButton.Button2, 0) == DialogResult.Yes)
                {
                    RenameFileTo = _renameFileToTextBox.Text;
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31270", ex);
            }
        }

        /// <summary>
        /// Handles the path tag selected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TagSelectedEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePathTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _renameFileToTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31271", ex);
            }
        }

        #endregion Methods
    }
}
