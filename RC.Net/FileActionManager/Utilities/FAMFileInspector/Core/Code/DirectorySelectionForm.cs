using Extract.Utilities;
using System;
using System.IO;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Allows for configuration of the directory currently being displayed in a
    /// <see cref="FAMFileInspectorForm"/>.
    /// </summary>
    public partial class DirectorySelectionForm : Form
    {
        /// <summary>
        /// The <see cref="FAMFileInspectorForm"/> instance for which the directory is to be
        /// reconfigured.
        /// </summary>
        FAMFileInspectorForm _inspectorForm;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectorySelectionForm"/> class.
        /// </summary>
        /// <param name="inspectorForm">The <see cref="FAMFileInspectorForm"/> instance for which
        /// the directory is to be reconfigured.</param>
        public DirectorySelectionForm(FAMFileInspectorForm inspectorForm)
        {
            try
            {
                _inspectorForm = inspectorForm;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36780");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _directoryTextBox.Text = _inspectorForm.SourceDirectory;
                _fileFilterComboBox.Text = _inspectorForm.FileFilter;
                _includeSubFoldersCheckBox.Checked = _inspectorForm.Recursive;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36781");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/>event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        private void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(_directoryTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "Please specify a valid directory.", "Invalid directory", true);
                    return;
                }

                _inspectorForm.SourceDirectory = _directoryTextBox.Text;
                _inspectorForm.FileFilter = _fileFilterComboBox.Text;
                _inspectorForm.Recursive = _includeSubFoldersCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36782");
            }
        }
    }
}
