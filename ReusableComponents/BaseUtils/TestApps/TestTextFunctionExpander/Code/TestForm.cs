using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Extract.FileActionManager.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.BaseUtils.Testing
{
    internal partial class TestForm : Form
    {
        #region Constants

        /// <summary>
        /// The default source doc name value
        /// </summary>
        static readonly string _DEFAULT_SOURCE_DOC = @"C:\InputFiles\TestImage01.tif";

        /// <summary>
        /// The default FPS file name
        /// </summary>
        static readonly string _DEFAULT_FPS_FILE_NAME = @"C:\FPSFiles\TestFpsFile.fps";

        /// <summary>
        /// The default workflow value
        /// </summary>
        static readonly string _DEFAULT_WORKFLOW = "TestWorkflow";

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(TestForm).ToString();

        #endregion Constants

        /// <summary>
        /// Initializes a new instance of the <see cref="TestForm"/> class.
        /// </summary>
        public TestForm()
        {
            try
            {
                InitializeComponent();

                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI28790", _OBJECT_NAME);

                // Set the default SourceDocName and FPSFileDir
                _textSourceDoc.Text = _DEFAULT_SOURCE_DOC;
                _textFpsFileName.Text = _DEFAULT_FPS_FILE_NAME;
                _textWorkflow.Text = _DEFAULT_WORKFLOW;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28791", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the Test button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleTestButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_textValue.Text))
                {
                    MessageBox.Show("You must specify text to expand.", "Empty Expansion",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1, 0);

                    return;
                }

                // Clear the text from the expansion result box
                _textExpansion.Clear();

                var tagManager = new FAMTagManager();
                tagManager.FPSFileDir = Path.GetDirectoryName(_textFpsFileName.Text);
                tagManager.FPSFileName = _textFpsFileName.Text;
                tagManager.Workflow = _textWorkflow.Text;

                // Get a tag manager to use for expanding the tags
                var tags = new FileActionManagerPathTags(tagManager, _textSourceDoc.Text);

                // Expand the value and place it in the expansion result box
                _textExpansion.Text = tags.Expand(_textValue.Text);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28792", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the Clear button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleButtonClearClick(object sender, EventArgs e)
        {
            try
            {
                // Clear the value and result
                _textValue.Clear();
                _textExpansion.Clear();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28793", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }
    }
}