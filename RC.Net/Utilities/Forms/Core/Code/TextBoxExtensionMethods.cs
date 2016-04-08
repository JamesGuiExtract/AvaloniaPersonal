using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// This class contains extensions for the TextBox edit control that implement required field
    /// notification helpers, and helpers for using ErrorProvider as well.
    /// </summary>
    public static class TextBoxExtensionMethods
    {
        // Note that SOH characters have been appended to the string to prevent a user from
        // typing "Required" in a text box and having that incorrectly match this text.
        const string REQUIRED_FIELD_MARKER = "Required\x01\x01";

        /// <summary>
        /// Changes the font color and places required marker text into the textbox.
        /// </summary>
        /// <param name="textBox"></param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetRequiredMarker(this TextBoxBase textBox)
        {
            textBox.ForeColor = System.Drawing.Color.SandyBrown;
            textBox.Text = REQUIRED_FIELD_MARKER;
            textBox.Refresh();
        }

        /// <summary>
        /// Removes marker text from a textbox, and restores the font color to black.
        /// </summary>
        /// <param name="textBox"></param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void RemoveRequiredMarker(this TextBoxBase textBox)
        {
            textBox.ForeColor = System.Drawing.Color.Black;
            textBox.Text = "";
            textBox.Refresh();
        }

        /// <summary>
        /// Tests the textbox text for empty or required text marker is set
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns>true iff text is empty, or set to required marker, otherwise false</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public bool EmptyOrRequiredMarkerIsSet(this TextBoxBase textBox)
        {
            var text = textBox.Text;
            return String.IsNullOrWhiteSpace(text) || REQUIRED_FIELD_MARKER == text;
        }

        /// <summary>
        /// Is the required marker set in a textbox?
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns>true iff the textbox text is set to the required marker, false otherwise</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public bool IsRequiredMarkerSet(this TextBoxBase textBox)
        {
            return REQUIRED_FIELD_MARKER == textBox.Text;
        }

        /// <summary>
        /// Calculate the offset of the error glyph.
        /// </summary>
        /// <returns>The negative offset to use to position the error glyph inside the textbox control</returns>
        static int ErrorGlyphOffset(ErrorProvider errorProvider)
        {
            const int rightMarginInPixels = 4;
            int offset = errorProvider.Icon.Width + rightMarginInPixels;

            // The starting position of the glyph is the right margin of the text box, so
            // using a negative value will move to the left of that starting position.
            const int convertOffsetIntoTextBoxFromRightTextBoxMargin = -1;
            return offset * convertOffsetIntoTextBoxFromRightTextBoxMargin;
        }

        /// <summary>
        /// Sets the position of the error glyph. By default the glyph is outside the textbox 
        /// on the right side. Move the error glyph to inside the text box on the right side.
        /// </summary>
        /// <param name="textBox">The textbox this extension method applies too (this)</param>
        /// <param name="errorProvider">The error provider associated with the textbox</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetErrorGlyphPosition(this TextBoxBase textBox, ErrorProvider errorProvider)
        {
            errorProvider.SetIconPadding(textBox, ErrorGlyphOffset(errorProvider));
        }

        /// <summary>
        /// Sets the position of the error glyph. By default the glyph is outside the textbox 
        /// on the right side. Move the error glyph to inside the text box on the right side.
        /// </summary>
        /// <param name="textBox">The textbox this extension method applies too (this)</param>
        /// <param name="errorProvider">The error provider associated with the textbox</param>
        /// <param name="alignment">The ErrorIconAlignment enum value to use for the control.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetErrorGlyphPosition(this TextBoxBase textBox,
                                                 ErrorProvider errorProvider,
                                                 ErrorIconAlignment alignment)
        {
            errorProvider.SetIconPadding(textBox, ErrorGlyphOffset(errorProvider));
            errorProvider.SetIconAlignment(textBox, alignment);
        }

        /// <summary>
        /// This method makes it easy to get the text value from a required-denoted text box.
        /// </summary>
        /// <param name="textBox">The textbox this extension method applies too (this)</param>
        /// <returns>The textBox.Text is correctly returned (required marker is translated into "")</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public string TextValue(this TextBoxBase textBox)
        {
            return IsRequiredMarkerSet(textBox) ? "" : textBox.Text;
        }

        /// <summary>
        /// Sets the error provider glyph and tooltip, when errorText is non-empty. 
        /// Clears the error provider glyph and tooltip when errorText is String.Empty.
        /// </summary>
        /// <param name="textBox">The textbox this extension method applies too (this)</param>
        /// <param name="errorProvider">The error provider used by the TextBox.</param>
        /// <param name="errorText">The error text.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetError(this TextBoxBase textBox, ErrorProvider errorProvider, string errorText)
        {
            try
            {
                if (null == errorProvider || null == errorText)
                    return;

                errorProvider.SetError(textBox, errorText);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39231");
            }
        }
    }
}
