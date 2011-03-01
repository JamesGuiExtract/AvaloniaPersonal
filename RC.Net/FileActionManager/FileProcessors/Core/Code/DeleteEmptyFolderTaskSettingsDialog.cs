using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    ///  A <see cref="Form"/> to view and modify settings for an <see cref="DeleteEmptyFolderTask"/> instance.
    /// </summary>
    public partial class DeleteEmptyFolderTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The default folder to delete.
        /// </summary>
        static readonly string _DEFAULT_FOLDER = "$DirOf(<SourceDocName>)";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteEmptyFolderTaskSettingsDialog"/> class.
        /// </summary>
        public DeleteEmptyFolderTaskSettingsDialog()
        {
            try
            {
                InitializeComponent();

                _folderNameTagsButton.PathTags = new FileActionManagerPathTags();
                _folderNameTagsButton.TagSelected += HandleFolderNameTagSelected;

                _recursionLimitTagsButton.PathTags = new FileActionManagerPathTags();
                _recursionLimitTagsButton.TagSelected += HandleRecursionLimitTagSelected;

                _recursionLimitBrowseButton.TextControl = _recursionLimitTextBox;

                // Specify enabled/disabled status of controls dependent on
                // _deleteRecursivelyCheckBox.
                _deleteRecursivelyCheckBox.CheckedChanged += ((sender, e) =>
                    {
                        bool enabled = _deleteRecursivelyCheckBox.Checked;
                        _limitRecursionCheckBox.Enabled = enabled;

                        enabled &= _limitRecursionCheckBox.Checked;
                        _recursionLimitTextBox.Enabled = enabled;
                        _recursionLimitBrowseButton.Enabled = enabled;
                        _recursionLimitTagsButton.Enabled = enabled;
                    });

                // Specify enabled/disabled status of controls dependent on
                // _limitRecursionCheckBox.
                _limitRecursionCheckBox.CheckedChanged += ((sender, e) =>
                    {
                        bool enabled =
                            _deleteRecursivelyCheckBox.Checked && _limitRecursionCheckBox.Checked;
                        _recursionLimitTextBox.Enabled = enabled;
                        _recursionLimitBrowseButton.Enabled = enabled;
                        _recursionLimitTagsButton.Enabled = enabled;
                    });

                _okButton.Click += HandleOkButtonClick;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31867");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Enable controls dependent on _deleteRecursivelyCheckBox if it is checked.
                if (_deleteRecursivelyCheckBox.Checked)
                {
                    _limitRecursionCheckBox.Enabled = true;

                    // Enable controls dependent on _limitRecursionCheckBox if it is checked.
                    if (_limitRecursionCheckBox.Checked)
                    {
                        _recursionLimitTextBox.Enabled = true;
                        _recursionLimitBrowseButton.Enabled = true;
                        _recursionLimitTagsButton.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31885");
            }
        }

        #endregion Overrides

        #region Properties

        /// <summary>
        /// Gets or sets the name of the folder to be deleted if empty.
        /// </summary>
        /// <value>
        /// The name of the folder.
        /// </value>
        public string FolderName
        {
            get
            {
                return _folderNameTextBox.Text;
            }

            set
            {
                _folderNameTextBox.Text =
                    string.IsNullOrWhiteSpace(value) ? _DEFAULT_FOLDER : value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether delete parent folder(s) if they are empty after
        /// deleting the current folder.
        /// </summary>
        /// <value><see langword="true"/> to delete parent folders recursively; otherwise,
        /// <see langword="false"/>.</value>
        public bool DeleteRecursively
        {
            get
            {
                return _deleteRecursivelyCheckBox.Checked;
            }

            set
            {
                _deleteRecursivelyCheckBox.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether recursion to delete parent folders as well
        /// should be prevented beyond a specified folder.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to limit recursion; otherwise, <see langword="false"/>.
        /// </value>
        public bool LimitRecursion
        {
            get
            {
                return _limitRecursionCheckBox.Checked;
            }

            set
            {
                _limitRecursionCheckBox.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of a parent folder which should not be deleted even if it is
        /// empty.
        /// </summary>
        /// <value>The name of a parent folder which should not be deleted even if it is empty.
        /// </value>
        public string RecursionLimit
        {
            get
            {
                return _recursionLimitTextBox.Text;
            }

            set
            {
                _recursionLimitTextBox.Text = value;
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
        void HandleFolderNameTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _folderNameTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31868");
            }
        }

        /// <summary>
        /// Handles the <see cref="PathTagsButton.TagSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        void HandleRecursionLimitTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _recursionLimitTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31874");
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
                ExtractException.Display("ELI31869", ex);
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
            if (string.IsNullOrWhiteSpace(_folderNameTextBox.Text))
            {
                _folderNameTextBox.Focus();
                MessageBox.Show("Please specify the name of the folder to be deleted if empty.",
                    "Missing folder name", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (_deleteRecursivelyCheckBox.Checked && _limitRecursionCheckBox.Checked &&
                string.IsNullOrWhiteSpace(_recursionLimitTextBox.Text))
            {
                _recursionLimitTextBox.Focus();
                MessageBox.Show("Please specify the name of the folder at which recursive deletion " +
                                "should stop (the specified folder will not be deleted).",
                    "Missing folder name", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
