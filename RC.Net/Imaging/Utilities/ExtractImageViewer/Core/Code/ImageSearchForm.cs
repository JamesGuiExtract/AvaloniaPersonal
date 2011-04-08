using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    /// <summary>
    /// A form used to search for image files and automatically open them.
    /// </summary>
    internal sealed partial class ImageSearchForm : Form
    {
        #region Fields

        /// <summary>
        /// The <see cref="ImageViewer"/> that will be used to display the found image.
        /// </summary>
        ImageViewer _imageViewer;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSearchForm"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> that this
        /// form should use to display the found image. May not be
        /// <see langword="null"/>.</param>
        /// <exception cref="ExtractException">If <paramref name="imageViewer"/>
        /// is <see langword="null"/>.</exception>
        public ImageSearchForm(ImageViewer imageViewer)
        {
            try
            {
                ExtractException.Assert("ELI30131", "ImageViewer may not be null.",
                    imageViewer != null);

                InitializeComponent();

                _imageViewer = imageViewer;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30132", ex);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Raises the <see cref="Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // Cancel the close event and just hide the form instead
                e.Cancel = true;
                Hide();

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30129", ex);
            }
        }
        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the Find and
        /// open image button.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFindAndOpenClick(object sender, EventArgs e)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    // Check that appropriate data is specified
                    if (string.IsNullOrEmpty(_textRootFolder.Text))
                    {
                        MessageBox.Show("Please specify a root folder for the search.",
                            "No Root Folder", MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _textRootFolder.Focus();

                        return;
                    }
                    if (string.IsNullOrEmpty(_comboImageExtension.Text))
                    {
                        MessageBox.Show("Please specify an image extension to search for.",
                            "No Image Extension", MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _comboImageExtension.Focus();
                        return;
                    }

                    // Build the search pattern
                    string extension = _comboImageExtension.Text;
                    if (extension[0] != '.')
                    {
                        extension = "." + extension;
                    }
                    string end = _textImageNameEnd.Text;
                    string pattern = "*" + end + extension;
                    string fileNamePattern = "*" + end;

                    // Recursively get all files that match the pattern
                    var files = new List<string>();
                    foreach (string file in Directory.EnumerateFiles(
                        _textRootFolder.Text, pattern, SearchOption.AllDirectories))
                    {
                        // Only add files that end with the appropriate character
                        if (Path.GetFileNameWithoutExtension(file)
                            .Like(fileNamePattern, false))
                        {
                            files.Add(file);
                        }
                    }

                    if (files.Count == 1)
                    {
                        // Open the image in the image viewer
                        _imageViewer.OpenImage(files[0], true);
                    }
                    else if (files.Count > 1)
                    {
                        StringBuilder sb = new StringBuilder("Multiple files found:");
                        sb.AppendLine();
                        foreach (string fileName in files)
                        {
                            sb.AppendLine(fileName);
                        }
                        using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                        {
                            messageBox.StandardIcon = MessageBoxIcon.Information;
                            messageBox.Caption = "Multiple Files";
                            messageBox.AddStandardButtons(MessageBoxButtons.OK);
                            messageBox.Text = sb.ToString();
                            messageBox.Show(this);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No files found.", "No Files", MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30127", ex);
            }
        }
    }
}