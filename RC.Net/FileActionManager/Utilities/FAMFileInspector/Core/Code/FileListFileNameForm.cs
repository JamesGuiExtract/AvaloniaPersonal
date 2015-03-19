using Extract.Utilities;
using System;
using System.IO;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Allows for configuration of the file that contains the file list currently being displayed in a
    /// <see cref="FAMFileInspectorForm"/>.
    /// </summary>    
    public partial class FileListFileNameForm : Form
    {
        /// <summary>
        /// The <see cref="FAMFileInspectorForm"/> instance for which the file list filename is to be
        /// reconfigured.
        /// </summary>
        FAMFileInspectorForm _inspectorForm;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileListFileNameForm"/> class.
        /// </summary>
        /// <param name="inspectorForm">The <see cref="FAMFileInspectorForm"/> instance for which
        /// the file list filename is to be reconfigured.</param>
        public FileListFileNameForm(FAMFileInspectorForm inspectorForm)
        {
            try
            {
                _inspectorForm = inspectorForm;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37944");
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

                _fileListFileNameTextBox.Text = _inspectorForm.FileListFileName;
                _browseButton.FileOrFolderPath = Path.GetDirectoryName(_fileListFileNameTextBox.Text);
                _browseButton.FileFilter = "Test files(*.txt)|*.txt|All files(*.*)|*.*";
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37945");
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
                if (!File.Exists(_fileListFileNameTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "Please specify an existing file.", "File not found", true);
                    return;
                }

                _inspectorForm.FileListFileName = _fileListFileNameTextBox.Text;
                
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37946");
            }
        }
    }
}
