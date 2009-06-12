using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Provides methods for dealing with key strokes.
    /// </summary>
    public static class KeyMethods
    {
        /// <summary>
        /// Sends the specified key (in combination with the specified modifiers) to the specfied
        /// control.  
        /// <para><b>Note:</b></para>
        /// If the keycode was able to be mapped and 
        /// <see paramref="controlToActivate"/> is not <see langword="null"/>, the specified control
        /// will be focused and active following this call. If the key was not able to be mapped,
        /// no change in focus will occur.
        /// </summary>
        /// <param name="keyCode">A virtual-key code representing the pressed key.</param>
        /// <param name="shift"><see langword="true"/> to apply the shift key as a modifier.</param>
        /// <param name="control"><see langword="true"/> to apply the control key as a modifier.</param>
        /// <param name="alt"><see langword="true"/> to apply the ale key as a modifier.</param>
        /// <param name="controlToActivate">The <see cref="Control"/> to activate before sending the
        /// keystroke. Can be <see langword="null"/> if no control should be activated before sending
        /// the keystroke.</param>
        /// <returns><see langword="true"/> if keys were sent (any specified control will have been
        /// activated. <see langword="false"/> if the key was not able too be mapped and no key was
        /// sent (no change in focues will have occured).</returns>
        static public bool SendKeyToControl(int keyCode, bool shift, bool control, bool alt, 
            Control controlToActivate)
        {
            string keyValue = "";

            switch ((Keys)keyCode)
            {
                // Attempt to map any special keys to the value needed by SendKeys.
                case Keys.Back:     keyValue = "{BACKSPACE}"; break;
                case Keys.Pause:    keyValue = "{BREAK}"; break;
                case Keys.CapsLock: keyValue = "{CAPSLOCK}"; break;
                case Keys.Delete:   keyValue = "{DELETE}"; break;
                case Keys.Down:     keyValue = "{DOWN}"; break;
                case Keys.End:      keyValue = "{END}"; break;
                case Keys.Enter:    keyValue = "{ENTER}"; break;
                case Keys.Escape:   keyValue = "{ESC}"; break;
                case Keys.Help:     keyValue = "{HELP}"; break;
                case Keys.Home:     keyValue = "{HOME}"; break;
                case Keys.Insert:   keyValue = "{INSERT}"; break;
                case Keys.Left:     keyValue = "{LEFT}"; break;
                case Keys.NumLock:  keyValue = "{NUMLOCK}"; break;
                case Keys.PageDown: keyValue = "{PGDN}"; break;
                case Keys.PageUp:   keyValue = "{PGUP}"; break;
                case Keys.PrintScreen: keyValue = "{PRTSC}"; break;
                case Keys.Right:    keyValue = "{RIGHT}"; break;
                case Keys.Scroll:   keyValue = "{SCROLLLOCK}"; break;
                case Keys.Tab:      keyValue = "{TAB}"; break;
                case Keys.Up:       keyValue = "{UP}"; break;
                case Keys.F1:       keyValue = "{F1}"; break;
                case Keys.F2:       keyValue = "{F2}"; break;
                case Keys.F3:       keyValue = "{F3}"; break;
                case Keys.F4:       keyValue = "{F4}"; break;
                case Keys.F5:       keyValue = "{F5}"; break;
                case Keys.F6:       keyValue = "{F6}"; break;
                case Keys.F7:       keyValue = "{F7}"; break;
                case Keys.F8:       keyValue = "{F8}"; break;
                case Keys.F9:       keyValue = "{F9}"; break;
                case Keys.F10:      keyValue = "{F10}"; break;
                case Keys.F11:      keyValue = "{F11}"; break;
                case Keys.F12:      keyValue = "{F12}"; break;
                case Keys.F13:      keyValue = "{F13}"; break;
                case Keys.F14:      keyValue = "{F14}"; break;
                case Keys.F15:      keyValue = "{F15}"; break;
                case Keys.F16:      keyValue = "{F16}"; break;

                // If the keystroke was not a special key recognized by SendKeys, attempt to map the
                // keystroke as a normal input character.
                default:
                    {
                        char? keyChar =
                            NativeMethods.VirtualKeyToChar(keyCode);

                        // If the map was successful, set the string value for SendKeys.  Use the 
                        // lower case value. (If shift is depressed, it will convert the char to
                        // upper case on the receiving side).
                        if (keyChar != null)
                        {
                            keyValue += keyChar.Value;
                            keyValue = keyValue.ToLower(CultureInfo.CurrentCulture);
                        }
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(keyValue))
            {
                // Apply modifier keys as specified.
                if (shift)
                {
                    keyValue = "+" + keyValue;
                }

                if (control)
                {
                    keyValue = "^" + keyValue;
                }

                if (alt)
                {
                    keyValue = "%" + keyValue;
                }

                // Select any specified control before sending the keys.
                if (controlToActivate != null)
                {
                    controlToActivate.Select();
                }

                SendKeys.SendWait(keyValue);

                return true;
            }

            return false;
        }
    }
}
