using System.Collections.Generic;
using System.Linq;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a Windows message.
    /// </summary>
    public static class WindowsMessage
    {
        #region Public Constants

        /// <summary>
        /// Allows changes in a window to be redrawn or prevents changes in that window from being 
        /// redrawn.
        /// </summary>
        public const int SetRedraw = 0x000B;

        /// <summary>
        /// Sent when the system or another application makes a request to paint a portion of an
        /// application's window.
        /// </summary>
        public const int Paint = 0x000F;

        /// <summary>
        /// The first of all key related messages.
        /// </summary>
        public const int KeyFirst = 0x0100;

        /// <summary>
        /// Key down message
        /// </summary>
        public const int KeyDown = 0x0100;

        /// <summary>
        /// Key up message
        /// </summary>
        public const int KeyUp = 0x0101;

        /// <summary>
        /// Character message
        /// </summary>
        public const int Character = 0x0102;

        /// <summary>
        /// Dead char message
        /// </summary>
        public const int DeadCharacter = 0x0103;

        /// <summary>
        /// System key down message
        /// </summary>
        public const int SystemKeyDown = 0x0104;

        /// <summary>
        /// System key up message
        /// </summary>
        public const int SystemKeyUp = 0x0105;

        /// <summary>
        /// System char message
        /// </summary>
        public const int SystemCharacter = 0x0106;

        /// <summary>
        /// System dead char message
        /// </summary>
        public const int SystemDeadCharacter = 0x0107;

        /// <summary>
        /// The last of all key related messages.
        /// </summary>
        public const int KeyLast = 0x0109;

        /// <summary>
        /// The user chose a command from the Window menu or chose the maximize button, minimize 
        /// button, restore button, or close button.
        /// </summary>
        public const int SystemCommand = 0x0112;

        /// <summary>
        /// Horizontal scroll message
        /// </summary>
        public const int HorizontalScroll = 0x0114;

        /// <summary>
        /// Vertical scroll message
        /// </summary>
        public const int VerticalScroll = 0x0115;

        /// <summary>
        /// The first of all mouse related messages.
        /// </summary>
        public const int MouseFirst = 0x0200;

        /// <summary>
        /// Mouse move message
        /// </summary>
        public const int MouseMove = 0x0200;

        /// <summary>
        /// Left mouse button down message
        /// </summary>
        public const int LeftButtonDown = 0x0201;

        /// <summary>
        /// Left mouse button up message
        /// </summary>
        public const int LeftButtonUp = 0x0202;

        /// <summary>
        /// Left mouse button double click message
        /// </summary>
        public const int LeftButtonDoubleClick = 0x0203;

        /// <summary>
        /// Right mouse button down message
        /// </summary>
        public const int RightButtonDown = 0x0204;

        /// <summary>
        /// Right mouse button up message
        /// </summary>
        public const int RightButtonUp = 0x0205;

        /// <summary>
        /// Right mouse button double click message
        /// </summary>
        public const int RightButtonDoubleClick = 0x0206;

        /// <summary>
        /// Middle mouse button down message
        /// </summary>
        public const int MiddleButtonDown = 0x0207;

        /// <summary>
        /// Middle mouse button up message
        /// </summary>
        public const int MiddleButtonUp = 0x0208;

        /// <summary>
        /// Middle mouse button double click message
        /// </summary>
        public const int MiddleButtonDoubleClick = 0x0209;

        /// <summary>
        /// Mouse wheel message
        /// </summary>
        public const int MouseWheel = 0x020A;

        /// <summary>
        /// The last of all mouse related messages.
        /// </summary>
        public const int MouseLast = 0x020A;

        /// <summary>
        /// Left button down in non-client area message.
        /// </summary>
        public const int NonClientLeftButtonDown = 0x00A1;

        /// <summary>
        /// Left button up in non-client area message.
        /// </summary>
        public const int NonClientLeftButtonUp = 0x00A2;

        /// <summary>
        /// Left button double click in non-client area message.
        /// </summary>
        public const int NonClientLeftButtonDoubleClick = 0x00A3;

        /// <summary>
        /// Right button down in non client area message.
        /// </summary>
        public const int NonClientRightButtonDown = 0x00A4;

        /// <summary>
        /// Right button up in non-client area message.
        /// </summary>
        public const int NonClientRightButtonUp = 0x00A5;

        /// <summary>
        /// Right button double click in non-client area message.
        /// </summary>
        public const int NonClientRightButtonDoubleClick = 0x00A6;

        /// <summary>
        /// Middle button down in non client area message.
        /// </summary>
        public const int NonClientMiddleButtonDown = 0x00A7;

        /// <summary>
        /// Middle button up in non-client area message.
        /// </summary>
        public const int NonClientMiddleButtonUp = 0x00A8;

        /// <summary>
        /// Middle button double click in non-client area message.
        /// </summary>
        public const int NonClientMiddleButtonDoubleClick = 0x00A9;

        /// <summary>
        /// Set focus message
        /// </summary>
        public const int SetFocus = 0x0007;

        /// <summary>
        /// Kill focus message
        /// </summary>
        public const int KillFocus = 0x0008;

        #endregion Public Constants

        #region Private Fields

        static HashSet<int> _userInputMessages = null;

        #endregion Private Fields

        #region Properties

        /// <summary>
        /// All messages that represent user mouse or keyboard input.
        /// </summary>
        public static HashSet<int> UserInputMessages
        {
            get
            {
                try
                {
                    if (_userInputMessages == null)
                    {
                        _userInputMessages = new HashSet<int>(
                            Enumerable.Range(KeyFirst, KeyLast - KeyFirst + 1)
                            .Union(Enumerable.Range(MouseFirst, MouseLast - MouseFirst + 1)));
                    }

                    return _userInputMessages;
                }
                catch (System.Exception ex)
                {
                    throw ex.AsExtract("ELI37753");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Processes all Windows messages currently in the message queue except for
        /// <see paramref="messagesToIgnore"/>.
        /// </summary>
        /// <param name="messagesToIgnore">The window messages that should be ignored/discarded
        /// rather than processed.</param>
        public static void DoEventsExcept(HashSet<int> messagesToIgnore)
        {
            try
            {
                NativeMethods.DoEventsExcept(messagesToIgnore);
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI37754");
            }
        }

        #endregion Methods
    }
}
