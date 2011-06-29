using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        /// <summary>
        /// Keeps track of readable enum strings that have been renamed within the context of this
        /// combo box.
        /// </summary>
        Dictionary<string, string> _stringToRenamedStrings =
            new Dictionary<string, string>();

        /// <summary>
        /// Keeps track of the original readable enum string for every readable value that has been
        /// renamed within the context of this combo box.
        /// </summary>
        Dictionary<string, string> _renamedToOriginalStrings =
            new Dictionary<string, string>();

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

        /// <summary>
        /// Renames a readable enum item for this combo box.
        /// </summary>
        /// <typeparam name="T">The enum type this combo box is populated by.</typeparam>
        /// <param name="value">The enum value to be renamed.</param>
        /// <param name="renamedValue">The string entry that should be associated with
        /// <see paramref="value"/> in the combo box.</param>
        public void RenameEnumValue<T>(T value, string renamedValue) where T : struct
        {
            try
            {
                // Find the original readable string associated with this enum value.
                string originalValue = value.ToReadableValue();

                // Find the string currently associated with this value in the combo box.
                string currentValue;
                if (_stringToRenamedStrings.TryGetValue(originalValue, out currentValue))
                {
                    // If we are going to be applying a different value, clear the map entries
                    // corresponding to this value.
                    if (currentValue != renamedValue)
                    {
                        _stringToRenamedStrings.Remove(originalValue);
                        _renamedToOriginalStrings.Remove(currentValue);
                    }
                }
                else
                {
                    currentValue = originalValue;
                }

                // Map the original readable value to the renamed value and update the item value.
                if (currentValue != renamedValue)
                {
                    if (Items.Contains(renamedValue))
                    {
                        ExtractException ee = new ExtractException("ELI32761",
                            "Cannot rename value to that of an value that already exists.");
                        ee.AddDebugData("Enum value", value.ToString(), false);
                        ee.AddDebugData("Renamed value", renamedValue, false);
                        throw ee;
                    }

                    _stringToRenamedStrings[originalValue] = renamedValue;
                    _renamedToOriginalStrings[renamedValue] = originalValue;

                    int currentIndex = Items.IndexOf(currentValue);
                    Items[currentIndex] = renamedValue;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32762");
            }
        }

        /// <summary>
        /// Gets the enum value associated with the readable value currently selected in the
        /// <see cref="ComboBox"/> taking into account any renamed values.
        /// </summary>
        /// <typeparam name="T">The enum type for which the readable value is assigned.</typeparam>
        /// <returns>The enum value.</returns>
        /// <throws><see cref="ExtractException"/> if the selected item is not a readable string
        /// assigned to one of the enums values.</throws>
        public T ToEnumValue<T>() where T : struct
        {
            try
            {
                // Lookup the original readable value if it has been renamed.
                string stringValue;
                if (!_renamedToOriginalStrings.TryGetValue(Text, out stringValue))
                {
                    stringValue = Text;
                }

                return stringValue.ToEnumValue<T>();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32763");
            }
        }

        /// <summary>
        /// Selects the specified enum value.
        /// </summary>
        /// <typeparam name="T">The enum type for which readable values are assigned.</typeparam>
        /// <returns>The enum value.</returns>
        /// <param name="value">The value to select.</param>
        /// <throws><see cref="ExtractException"/> if the <see paramref="value"/> is not a readable
        /// enum value that exists in this <see cref="ComboBox"/>.</throws>
        public void SelectEnumValue<T>(T value) where T : struct
        {
            try
            {
                // Lookup the renamed value if it has been renamed.
                string originalValue = value.ToReadableValue<T>();
                string renamedValue;
                if (_stringToRenamedStrings.TryGetValue(originalValue, out renamedValue))
                {
                    Text = renamedValue;
                }
                else
                {
                    Text = originalValue;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32764");
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
