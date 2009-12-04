using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents a dialog that allows the user to select file action manager file tag.
    /// </summary>
    public partial class FileTagDialog : Form
    {
        #region Fields
		
        /// <summary>
        /// File action manager file tag.
        /// </summary>
        FileTag _fileTag;
 
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="FileTagDialog"/> class.
        /// </summary>
        public FileTagDialog() 
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagDialog"/> class.
        /// </summary>
        /// <param name="fileTag">File action manager file tag.</param>
        public FileTagDialog(FileTag fileTag)
        {
            InitializeComponent();

            _fileTag = fileTag;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the file action manager file tag.
        /// </summary>
        /// <value>File action manager file tag.</value>
        public FileTag FileTag
        {
            get
            {
                return _fileTag;
            }
            set
            {
                _fileTag = value;
            }
        }
		 
        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the <see cref="FileTag"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="FileTag"/> from the user interface.</returns>
        FileTag GetFileTag()
        {
            string name = _nameTextBox.Text;
            string description = _descriptionTextBox.Text;

            return new FileTag(name, description);
        }

        /// <summary>
        /// Trims whitespace from the front and end of the specified textbox.
        /// </summary>
        /// <param name="textBox">The textbox whose whitespace should be trimmed.</param>
        static void TrimTextBoxText(TextBox textBox)
        {
            string originalText = textBox.Text;
            string trimmedText = textBox.Text.Trim();
            if (originalText.Length != trimmedText.Length)
            {
                textBox.Text = trimmedText;
            }
        }

        /// <summary>
        /// Displays a warning message if the user specified file tag is invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the file tag is invalid; <see langword="false"/> if 
        /// the file tag is valid.</returns>
        bool WarnIfInvalid()
        {
            // Check if the name is valid
            bool invalid = string.IsNullOrEmpty(_nameTextBox.Text);
            if (invalid)
            {
                MessageBox.Show("Please enter a tag name.", "Missing tag name", 
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _nameTextBox.Focus();
            }

            return invalid;
        }
		 
        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (_fileTag != null)
                {
                    _nameTextBox.Text = _fileTag.Name;
                    _descriptionTextBox.Text = _fileTag.Description;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28733", ex);
            }
        }
		 
        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Trim the text of any text box that still has focus
                if (_nameTextBox.Focused)
                {
                    TrimTextBoxText(_nameTextBox);
                }
                if (_descriptionTextBox.Focused)
                {
                    TrimTextBoxText(_descriptionTextBox);
                }

                // Check if the dialog is invalid
                if (WarnIfInvalid())
                {
                    return;
                }

                // Store file tag
                _fileTag = GetFileTag();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28734", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Leave"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Leave"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Leave"/> event.</param>
        void HandleTextBoxLeave(object sender, EventArgs e)
        {
            try
            {
                // Trim whitespace if necessary [LRCAU #5516]
                TrimTextBoxText((TextBox) sender);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI28736", ex);
            }
        }

        #endregion Event Handlers
    }
}