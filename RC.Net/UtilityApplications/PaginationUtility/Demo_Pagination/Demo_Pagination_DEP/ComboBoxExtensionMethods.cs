using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Demo_Pagination
{
    /// <summary>
    /// This class contains extensions for the ComboBox edit control that implement required field
    /// notification helpers, and helpers for using ErrorProvider as well.
    /// </summary>
    public static class ComboBoxExtensionMethods
    {
        // Note that SOH characters have been appended to the string to prevent a user from
        // typing "Required" in a text box and having that incorrectly match this text.
        const string REQUIRED_FIELD_MARKER = "Required\x01\x01";

        /// <summary>
        /// Changes the font color and places required marker text into the combobox.
        /// </summary>
        /// <param name="combobox"></param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetRequiredMarker(this ComboBox comboBox)
        {
            try
            {
                if (!comboBox.IsRequiredMarkerSet())
                {
                    comboBox.ForeColor = System.Drawing.Color.SandyBrown;
                    comboBox.Text = REQUIRED_FIELD_MARKER;
                    comboBox.Invalidate();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40244");
            }
        }

        /// <summary>
        /// Removes marker text from a combobox, and restores the font color to black, BUT only
        /// if the required marker is actually already set in the combobox.
        /// </summary>
        /// <param name="comboBox"></param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void RemoveRequiredMarker(this ComboBox comboBox)
        {
            try
            {
                if (comboBox.IsRequiredMarkerSet())
                {
                    comboBox.ForeColor = System.Drawing.Color.Black;
                    comboBox.Text = "";
                    comboBox.Invalidate();
                }
                else
                {
                    // 5/20/2016 SNK
                    // This may be dead code. The intent was that if a cursor is moved into a
                    // control and a character is entered at the front, that the entered character
                    // is not also erased as part of clearing REQUIRED_FIELD_MARKER. At one point,
                    // this was necessary. However, I believe proper handling of OnEnter should
                    // result in the marker being cleared before text is entered.
                    var text = comboBox.Text;
                    if (text.Contains(REQUIRED_FIELD_MARKER))
                    {
                        var contents = RemoveTrailingRequiredText(text);
                        comboBox.ForeColor = System.Drawing.Color.Black;
                        comboBox.Text = contents;
                        comboBox.Invalidate();

                        comboBox.SafeBeginInvoke("ELI40245", () =>
                            {
                                comboBox.SelectionStart = contents.Length;
                                comboBox.SelectionLength = 0;
                                comboBox.Focus();
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40246");
            }
        }

        /// <summary>
        /// Tests the combobox text for empty or required text marker is set
        /// </summary>
        /// <param name="combobox"></param>
        /// <returns>true iff text is empty, or set to required marker, otherwise false</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public bool EmptyOrRequiredMarkerIsSet(this ComboBox comboBox)
        {
            var text = comboBox.Text;
            return String.IsNullOrWhiteSpace(text) || REQUIRED_FIELD_MARKER == text;
        }

        /// <summary>
        /// Is the required marker set in a combobox?
        /// </summary>
        /// <param name="comboBox"></param>
        /// <returns>true iff the combobox text is set to the required marker, false otherwise</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public bool IsRequiredMarkerSet(this ComboBox combobox)
        {
            return REQUIRED_FIELD_MARKER == combobox.Text;
        }

        /// <summary>
        /// Calculate the offset of the error glyph.
        /// </summary>
        /// <returns>The negative offset to use to position the error glyph inside the combobox control</returns>
        static int ErrorGlyphOffset(ErrorProvider errorProvider)
        {
            int rightMarginInPixels = 4 + SystemInformation.VerticalScrollBarWidth;
            int offset = errorProvider.Icon.Width + rightMarginInPixels;

            // The starting position of the glyph is the right margin of the text box, so
            // using a negative value will move to the left of that starting position.
            const int convertOffsetIntoComboBoxFromRightComboBoxMargin = -1;
            return offset * convertOffsetIntoComboBoxFromRightComboBoxMargin;
        }

        /// <summary>
        /// Sets the position of the error glyph. By default the glyph is outside the combobox 
        /// on the right side. Move the error glyph to inside the text box on the right side.
        /// </summary>
        /// <param name="combobox">The combobox this extension method applies too (this)</param>
        /// <param name="errorProvider">The error provider associated with the combobox</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetErrorGlyphPosition(this ComboBox comboBox, ErrorProvider errorProvider)
        {
            errorProvider.SetIconPadding(comboBox, ErrorGlyphOffset(errorProvider));
        }

        /// <summary>
        /// Sets the position of the error glyph. By default the glyph is outside the combobox 
        /// on the right side. Move the error glyph to inside the text box on the right side.
        /// </summary>
        /// <param name="combobox">The combobox this extension method applies too (this)</param>
        /// <param name="errorProvider">The error provider associated with the combobox</param>
        /// <param name="alignment">The ErrorIconAlignment enum value to use for the control.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetErrorGlyphPosition(this ComboBox comboBox,
                                                 ErrorProvider errorProvider,
                                                 ErrorIconAlignment alignment)
        {
            errorProvider.SetIconPadding(comboBox, ErrorGlyphOffset(errorProvider));
            errorProvider.SetIconAlignment(comboBox, alignment);
        }

        /// <summary>
        /// This method makes it easy to get the text value from a required-denoted text box.
        /// </summary>
        /// <param name="comboBox">The combobox this extension method applies too (this)</param>
        /// <returns>The comboBox.Text is correctly returned (required marker is translated into "")</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public string TextValue(this ComboBox comboBox)
        {
            try
            {
                if (IsRequiredMarkerSet(comboBox))
                {
                    return "";
                }
                else
                {
                    var text = comboBox.Text;
                    if (text.Contains(REQUIRED_FIELD_MARKER))
                    {
                        return RemoveTrailingRequiredText(text);
                    }
                    else
                    {
                        return text;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40247");
            }
        }

        /// <summary>
        /// Sets the error provider glyph and tooltip, when errorText is non-empty. 
        /// Clears the error provider glyph and tooltip when errorText is String.Empty.
        /// </summary>
        /// <param name="comboBox">The combobox this extension method applies too (this)</param>
        /// <param name="errorProvider">The error provider used by the ComboBox.</param>
        /// <param name="errorText">The error text.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static public void SetError(this ComboBox comboBox, ErrorProvider errorProvider, string errorText)
        {
            try
            {
                if (null == errorProvider || null == errorText)
                    return;

                errorProvider.SetError(comboBox, errorText);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40248");
            }
        }

        /// <summary>
        /// Removes the trailing required field marker text. This can occur when performing
        /// character-by-character validation in combobox fields.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text that remains when the required marker has been removed.</returns>
        static string RemoveTrailingRequiredText(string text)
        {
            int end = text.IndexOf(REQUIRED_FIELD_MARKER, StringComparison.Ordinal);
            return text.Substring(0, length: end);
        }
    }
}
