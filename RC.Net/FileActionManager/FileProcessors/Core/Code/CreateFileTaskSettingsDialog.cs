using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    ///  A <see cref="Form"/> to view and modify settings for an <see cref="CreateFileTask"/> instance.
    /// </summary>
    public partial class CreateFileTaskSettingsDialog : Form
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFileTaskSettingsDialog"/> class.
        /// </summary>
        public CreateFileTaskSettingsDialog()
        {
            try
            {
                InitializeComponent();

                _fileNameTagsButton.PathTags = new FileActionManagerPathTags();
                _fileNameTagsButton.TagSelected += HandleFileNameTagSelected;

                _fileContentsTagsButton.PathTags = new FileActionManagerPathTags();
                _fileContentsTagsButton.TagSelected += HandleFileContentsTagSelected;

                _okButton.Click += HandleOkButtonClick;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31843");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the file to be generated.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName
        {
            get
            {
                return _fileNameTextBox.Text;
            }

            set
            {
                _fileNameTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the contents of the file to be generated.
        /// </summary>
        /// <value>
        /// The contents of the file.
        /// </value>
        public string FileContents
        {
            get
            {
                return _fileContentsTextBox.Text;
            }

            set
            {
                _fileContentsTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the create file conflict resolution.
        /// </summary>
        /// <value>
        /// The create file conflict resolution.
        /// </value>
        public CreateFileConflictResolution CreateFileConflictResolution
        {
            get
            {
                try
                {
                    if (_generateErrorRadioButton.Checked)
                    {
                        return CreateFileConflictResolution.GenerateError;
                    }
                    else if (_skipWithoutErrorRadioButton.Checked)
                    {
                        return CreateFileConflictResolution.SkipWithoutError;
                    }
                    else if (_overwriteRadioButton.Checked)
                    {
                        return CreateFileConflictResolution.Overwrite;
                    }
                    else if (_appendRadioButton.Checked)
                    {
                        return CreateFileConflictResolution.Append;
                    }

                    return CreateFileConflictResolution.GenerateError;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI31846");
                }
            }

            set
            {
                try
                {
                    switch (value)
                    {
                        case CreateFileConflictResolution.GenerateError:
                            {
                                _generateErrorRadioButton.Checked = true;
                            }
                            break;

                        case CreateFileConflictResolution.SkipWithoutError:
                            {
                                _skipWithoutErrorRadioButton.Checked = true;
                            }
                            break;

                        case CreateFileConflictResolution.Overwrite:
                            {
                                _overwriteRadioButton.Checked = true;
                            }
                            break;

                        case CreateFileConflictResolution.Append:
                            {
                                _appendRadioButton.Checked = true;
                            }
                            break;

                        default:
                            {
                                ExtractException.ThrowLogicException("ELI31844");
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI31845");
                }
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
        void HandleFileNameTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _fileNameTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31847");
            }
        }

        /// <summary>
        /// Handles the <see cref="PathTagsButton.TagSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        void HandleFileContentsTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _fileContentsTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31853");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31842", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_fileNameTextBox.Text))
            {
                _fileNameTextBox.Focus();
                MessageBox.Show("Please specify the name of the file to be created.",
                    "Missing filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (_fileNameTextBox.Text.Equals("<SourceDocName>", StringComparison.OrdinalIgnoreCase))
            {
                _fileNameTextBox.Focus();
                MessageBox.Show("Cannot create file with the same name as the source document.",
                    "Invalid filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
