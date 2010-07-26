using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a <see cref="ComboBox"/> with extended functionality.
    /// </summary>
    public partial class BetterComboBox : ComboBox
    {
        #region Fields

        /// <summary>
        /// <see langword="true"/> if selected text is lost when the combo box loses focus;
        /// <see langword="false"/> if selected text remains when the combo box loses focus.
        /// </summary>
        bool _hideSelection = true;

        /// <summary>
        /// The start index of the selected text. Ignored if <see cref="_hideSelection"/> is 
        /// <see langword="true"/>.
        /// </summary>
        int _selectionStart;

        /// <summary>
        /// The number of characters selected in the editable portion of the combo box. Ignored 
        /// if <see cref="_hideSelection"/> is <see langword="true"/>.
        /// </summary>
        int _selectionLength;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="BetterComboBox"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public BetterComboBox()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether selected text is lost when the combo box loses focus.
        /// </summary>
        /// <value><see langword="true"/> if selected text is lost when the combo box loses focus;
        /// <see langword="false"/> if selected text remains when the combo box loses focus.</value>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Indicates that the selected text should be hidden when the combo box loses focus.")]
        public bool HideSelection
        {
            get
            {
                return _hideSelection;
            }
            set
            {
                _hideSelection = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets or sets the text that is selected in the editable portion of the combo box.
        /// </summary>
        /// <param name="text">The text with which to replace the selected text.</param>
        public void SetSelectedText(string text)
        {
            try
            {
                if (!_hideSelection)
                {
                    SelectionStart = _selectionStart;
                    SelectionLength = _selectionLength;
                }

                SelectedText = text;

                if (!_hideSelection)
                {
                    _selectionStart += text.Length;
                    _selectionLength = 0;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29174", ex);
                ee.AddDebugData("Text", text, false);
                throw ee;
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.MouseClick"/> event.
        /// </summary>
        /// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data. </param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (!_hideSelection && Focused)
            {
                _selectionStart = SelectionStart;
                _selectionLength = SelectionLength;
            }

            base.OnMouseClick(e);
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the character was processed by the control; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        /// <param name="keyData">One of the <see cref="Keys"/> values that represents the key to 
        /// process.</param>
        /// <param name="msg">The window message to process.</param>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_hideSelection && Focused)
            {
                _selectionStart = SelectionStart;
                _selectionLength = SelectionLength;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion Overrides
    }
}
