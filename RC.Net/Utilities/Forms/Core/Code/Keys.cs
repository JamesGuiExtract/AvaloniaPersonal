using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Provides methods for dealing with key strokes.
    /// </summary>
    public static class KeyMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(KeyMethods).ToString();

        /// <summary>
        /// Characters that have special meaning in SendKeys.
        /// </summary>
        static readonly char[] _SPECIAL_SENDKEYS_CHARS =
            { '^', '%', '(', ')', '+', '~', '{', '}', '[', ']' };

        #endregion Constants

        #region Fields

        /// <summary>
        /// The scan codes to virtual key codes that have already been mapped.
        /// </summary>
        static Dictionary<uint, uint> _keyCodeMappings = new Dictionary<uint, uint>();

        /// <summary>
        /// Mutex to control access to _keyCodeMappings.
        /// </summary>
        static object _keyMappingLock = new object();

        #endregion Fields

        /// <summary>
        /// Sends the specified key (in combination with the specified modifiers) to the specfied
        /// control.  
        /// <para><b>Note:</b></para>
        /// If the keycode was able to be mapped and 
        /// <see paramref="controlToActivate"/> is not <see langword="null"/>, the specified control
        /// will be focused and active following this call. If the key was not able to be mapped,
        /// no change in focus will occur.
        /// <para><b>Warning:</b></para>
        /// If this control is being used to propagate a keystroke from one control to another,
        /// ensure <see paramref="controlToActivate"/> is not <see langword="null"/> or the same
        /// control that is sending the key to avoid an infinite loop.
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
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32357",
                    _OBJECT_NAME);

                string keyValue = "";

                switch ((Keys)keyCode)
                {
                    // Attempt to map any special keys to the value needed by SendKeys.
                    case Keys.Back: keyValue = "{BACKSPACE}"; break;
                    case Keys.Pause: keyValue = "{BREAK}"; break;
                    case Keys.CapsLock: keyValue = "{CAPSLOCK}"; break;
                    case Keys.Delete: keyValue = "{DELETE}"; break;
                    case Keys.Down: keyValue = "{DOWN}"; break;
                    case Keys.End: keyValue = "{END}"; break;
                    case Keys.Enter: keyValue = "{ENTER}"; break;
                    case Keys.Escape: keyValue = "{ESC}"; break;
                    case Keys.Help: keyValue = "{HELP}"; break;
                    case Keys.Home: keyValue = "{HOME}"; break;
                    case Keys.Insert: keyValue = "{INSERT}"; break;
                    case Keys.Left: keyValue = "{LEFT}"; break;
                    case Keys.NumLock: keyValue = "{NUMLOCK}"; break;
                    case Keys.PageDown: keyValue = "{PGDN}"; break;
                    case Keys.PageUp: keyValue = "{PGUP}"; break;
                    case Keys.PrintScreen: keyValue = "{PRTSC}"; break;
                    case Keys.Right: keyValue = "{RIGHT}"; break;
                    case Keys.Scroll: keyValue = "{SCROLLLOCK}"; break;
                    case Keys.Tab: keyValue = "{TAB}"; break;
                    case Keys.Up: keyValue = "{UP}"; break;
                    case Keys.F1: keyValue = "{F1}"; break;
                    case Keys.F2: keyValue = "{F2}"; break;
                    case Keys.F3: keyValue = "{F3}"; break;
                    case Keys.F4: keyValue = "{F4}"; break;
                    case Keys.F5: keyValue = "{F5}"; break;
                    case Keys.F6: keyValue = "{F6}"; break;
                    case Keys.F7: keyValue = "{F7}"; break;
                    case Keys.F8: keyValue = "{F8}"; break;
                    case Keys.F9: keyValue = "{F9}"; break;
                    case Keys.F10: keyValue = "{F10}"; break;
                    case Keys.F11: keyValue = "{F11}"; break;
                    case Keys.F12: keyValue = "{F12}"; break;
                    case Keys.F13: keyValue = "{F13}"; break;
                    case Keys.F14: keyValue = "{F14}"; break;
                    case Keys.F15: keyValue = "{F15}"; break;
                    case Keys.F16: keyValue = "{F16}"; break;

                    // If the keystroke was not a special key recognized by SendKeys, attempt
                    // to map the keystroke as a normal input character.
                    default:
                        {
                            char? keyChar =
                                NativeMethods.VirtualKeyToChar(keyCode);

                            // If the map was successful, set the string value for SendKeys.  Use the 
                            // lower case value. (If shift is depressed, it will convert the char to
                            // upper case on the receiving side).
                            if (keyChar != null)
                            {
                                // Escape any special sendKeys chars in braces.
                                if (Array.IndexOf(_SPECIAL_SENDKEYS_CHARS, keyChar.Value) >= 0)
                                {
                                    keyValue += "{" + keyChar.Value + "}";
                                }
                                else
                                {
                                    keyValue += keyChar.Value;
                                }

                                // Before sending the value via SendKeys, make it lower case otherwise caps lock
                                // will reverse the case.
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
                        if (!controlToActivate.Focus())
                        {
                            // [DataEntry:412] A key can't be sent to the control if it can't
                            // receive focus. Return false without sending the key so that code
                            // intended to be directing input to another control doesn't get caught
                            // in an infinite loop.
                            return false;
                        }
                    }

                    SendKeys.SendWait(keyValue);

                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26497", ex);
            }
        }

        /// <summary>
        /// Sends the specified character to the specfied control.  
        /// </summary>
        /// <param name="character">The character that should be sent to the control.</param>
        /// <param name="control">The <see cref="Control"/> the character should be sent to.</param>
        static public void SendCharacterToControl(char character, Control control)
        {
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32359",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI27439", "Null argument exception!", control != null);

                NativeMethods.SendMessage(control.Handle, WindowsMessage.Character, (IntPtr)character, (IntPtr)0);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27440", ex);
            }
        }

        /// <summary>
        /// Retrieves the <see cref="Keys"/> value associated from the <see paramref="m"/>
        /// associated with a key event.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> associated with a windows key message.
        /// </param>
        /// <param name="distinguishLeftRight"><see langword="true"/> the returned value should
        /// distinguish between left and right keys, <see langword="false"/> to return the generic
        /// <see cref="Keys"/> for keys with a left and right.</param>
        /// <returns>The <see cref="Keys"/> value associated with the specified
        /// <see paramref="message"/>.</returns>
        public static Keys GetKeyFromMessage(Message message, bool distinguishLeftRight)
        {
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32358",
                    _OBJECT_NAME);

                // Extract the scan code for the key press
                // http://msdn.microsoft.com/en-us/library/ms646280(VS.85).aspx
                uint scanCode = (uint)(message.LParam.ToInt32() >> 16);
                bool isExtendedKey = (scanCode & 0x0100) != 0;
                scanCode = scanCode & 0x00FF;

                // Although it is not supporte on all OS's, track extended keys by using an 0xe0
                // prefix when possible.
                // http://msdn.microsoft.com/en-us/library/ms646307(v=VS.85).aspx
                if (isExtendedKey)
                {
                    scanCode |= 0xe000;
                }

                uint virtualKeyCode;

                lock (_keyMappingLock)
                {
                    if (!_keyCodeMappings.TryGetValue(scanCode, out virtualKeyCode))
                    {
                        // If we can convert the scan code to a virtual key code, then convert it
                        // back to the original scan code, we have a proper mapping.
                        virtualKeyCode =
                            NativeMethods.ConvertKeyCode(scanCode, false, distinguishLeftRight);
                        if (!isExtendedKey ||
                            scanCode == NativeMethods.ConvertKeyCode(
                                virtualKeyCode, true, distinguishLeftRight))
                        {
                            // virtualKeyCode is valid
                        }
                        else if (isExtendedKey && distinguishLeftRight)
                        {
                            // If on a system that doesn't support extended key scan codes, manually
                            // handle special cases.
                            uint nonExtendedScanCode = scanCode & 0x00FF;
                            virtualKeyCode = MapSpecialExtendedKey(nonExtendedScanCode);
                        }
                        else
                        {
                            // Otherwise, we will return Keys.None.
                            virtualKeyCode = 0;
                        }

                        _keyCodeMappings[scanCode] = virtualKeyCode;
                    }
                }

                return (Keys)virtualKeyCode;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32351");
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="key"/> can be mapped to/from a scan code.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value to test.</param>
        /// <param name="distinguishLeftRight"></param>
        /// <returns>
        /// <see langword="true"/> if the <see paramref="key"/> can be mapped; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool IsRecognizedKey(Keys key, bool distinguishLeftRight)
        {
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32360",
                    _OBJECT_NAME);

                lock (_keyMappingLock)
                {
                    // Check to see if the virtual key code can be converted to a scancode, then back to
                    // the same virtual key code. If so, it is recognized.
                    uint virtualKeyCode = (uint)key;
                    uint scanCode =
                        NativeMethods.ConvertKeyCode(virtualKeyCode, true, distinguishLeftRight);
                    if (virtualKeyCode ==
                        NativeMethods.ConvertKeyCode(scanCode, false, distinguishLeftRight))
                    {
                        if (_keyCodeMappings.ContainsKey(scanCode))
                        {
                            return true;
                        }

                        // Map the scan code to the virtual key code for faster lookup in the future.
                        if (virtualKeyCode ==
                            NativeMethods.ConvertKeyCode(scanCode, false, distinguishLeftRight))
                        {
                            _keyCodeMappings[scanCode] = virtualKeyCode;
                            return true;
                        }
                    }
                    // If we failed to find a mapping, but distinguishLeftRight is true, see if a
                    // special extended key case applies that we can use to map the key.
                    else if (distinguishLeftRight)
                    {
                        // If the virtual key code can be converted to a scancode without left/right
                        // distinction, then back to the same virtual key code using
                        // MapSpecialExtendedKey, consider it a recognized mapping.
                        scanCode = NativeMethods.ConvertKeyCode(virtualKeyCode, true, false);
                        uint newVirtualKeyCode = MapSpecialExtendedKey(scanCode);
                        if (virtualKeyCode == newVirtualKeyCode)
                        {
                            // Apply the extended key prefix to the mapped scan-code.
                            scanCode |= 0xe000;
                            _keyCodeMappings[scanCode] = virtualKeyCode;
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32352");
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="key"/> is pressed.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value to test.</param>
        /// <returns>
        /// <see langword="true"/> if the <see paramref="key"/> is pressed; <see langword="false"/>
        /// otherwise.
        /// </returns>
        public static bool IsKeyPressed(Keys key)
        {
            try
            {
                // Validate license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32361",
                    _OBJECT_NAME);

                return NativeMethods.IsKeyPressed(key);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32356");
            }
        }

        /// <summary>
        /// Assuming <see paramref="nonExtendedScanCode"/> corresponds to an extended key, attempts
        /// to map the code to a virtual key code if it is a known special case.
        /// For example, if the non-extended scan code maps to VK_CONTROL, but we know the key is an
        /// extended key, we can deduce that the key is the right control key since only the right
        /// control key is an extended key.
        /// </summary>
        /// <param name="nonExtendedScanCode">The scan code retrieved by using MapVirtualKeyEx with
        /// MAPVK_VK_TO_VSC (no left/right distinction).</param>
        /// <returns>The virtual key code associated with the specified
        /// <see paramref="nonExtendedScanCode"/> or 0 if the specified scan code is not a handled
        /// special case.</returns>
        static uint MapSpecialExtendedKey(uint nonExtendedScanCode)
        {
            uint virtualKeyCode = NativeMethods.ConvertKeyCode(nonExtendedScanCode, false, false);

            switch ((Keys)virtualKeyCode)
            {
                case Keys.ControlKey:
                    return (uint)Keys.RControlKey;
                case Keys.Menu:
                    return (uint)Keys.RMenu;
                default:
                    return 0;
            }
        }
    }
}
