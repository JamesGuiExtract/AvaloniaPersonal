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
        /// 
        /// </summary>
        public enum Codes : int
        {
            /// <summary>
            /// Allows changes in a window to be redrawn or prevents changes in that window from being 
            /// redrawn.
            /// </summary>
            SetRedraw = 0x000B,

            /// <summary>
            /// Sent when the system or another application makes a request to paint a portion of an
            /// application's window.
            /// </summary>
            Paint = 0x000F,

            /// <summary>
            /// The first of all key related messages.
            /// </summary>
            KeyFirst = 0x0100,

            /// <summary>
            /// Key down message
            /// </summary>
            KeyDown = 0x0100,

            /// <summary>
            /// Key up message
            /// </summary>
            KeyUp = 0x0101,

            /// <summary>
            /// Character message
            /// </summary>
            Character = 0x0102,

            /// <summary>
            /// Dead char message
            /// </summary>
            DeadCharacter = 0x0103,

            /// <summary>
            /// System key down message
            /// </summary>
            SystemKeyDown = 0x0104,

            /// <summary>
            /// System key up message
            /// </summary>
            SystemKeyUp = 0x0105,

            /// <summary>
            /// System char message
            /// </summary>
            SystemCharacter = 0x0106,

            /// <summary>
            /// System dead char message
            /// </summary>
            SystemDeadCharacter = 0x0107,

            /// <summary>
            /// The last of all key related messages.
            /// </summary>
            KeyLast = 0x0109,

            /// <summary>
            /// The user chose a command from the Window menu or chose the maximize button, minimize 
            /// button, restore button, or close button.
            /// </summary>
            SystemCommand = 0x0112,

            /// <summary>
            /// Horizontal scroll message
            /// </summary>
            HorizontalScroll = 0x0114,

            /// <summary>
            /// Vertical scroll message
            /// </summary>
            VerticalScroll = 0x0115,

            /// <summary>
            /// The first of all mouse related messages.
            /// </summary>
            MouseFirst = 0x0200,

            /// <summary>
            /// Mouse move message
            /// </summary>
            MouseMove = 0x0200,

            /// <summary>
            /// Left mouse button down message
            /// </summary>
            LeftButtonDown = 0x0201,

            /// <summary>
            /// Left mouse button up message
            /// </summary>
            LeftButtonUp = 0x0202,

            /// <summary>
            /// Left mouse button double click message
            /// </summary>
            LeftButtonDoubleClick = 0x0203,

            /// <summary>
            /// Right mouse button down message
            /// </summary>
            RightButtonDown = 0x0204,

            /// <summary>
            /// Right mouse button up message
            /// </summary>
            RightButtonUp = 0x0205,

            /// <summary>
            /// Right mouse button double click message
            /// </summary>
            RightButtonDoubleClick = 0x0206,

            /// <summary>
            /// Middle mouse button down message
            /// </summary>
            MiddleButtonDown = 0x0207,

            /// <summary>
            /// Middle mouse button up message
            /// </summary>
            MiddleButtonUp = 0x0208,

            /// <summary>
            /// Middle mouse button double click message
            /// </summary>
            MiddleButtonDoubleClick = 0x0209,

            /// <summary>
            /// Mouse wheel message
            /// </summary>
            MouseWheel = 0x020A,

            /// <summary>
            /// The last of all mouse related messages.
            /// </summary>
            MouseLast = 0x020A,

            /// <summary>
            /// Left button down in non-client area message.
            /// </summary>
            NonClientLeftButtonDown = 0x00A1,

            /// <summary>
            /// Left button up in non-client area message.
            /// </summary>
            NonClientLeftButtonUp = 0x00A2,

            /// <summary>
            /// Left button double click in non-client area message.
            /// </summary>
            NonClientLeftButtonDoubleClick = 0x00A3,

            /// <summary>
            /// Right button down in non client area message.
            /// </summary>
            NonClientRightButtonDown = 0x00A4,

            /// <summary>
            /// Right button up in non-client area message.
            /// </summary>
            NonClientRightButtonUp = 0x00A5,

            /// <summary>
            /// Right button double click in non-client area message.
            /// </summary>
            NonClientRightButtonDoubleClick = 0x00A6,

            /// <summary>
            /// Middle button down in non client area message.
            /// </summary>
            NonClientMiddleButtonDown = 0x00A7,

            /// <summary>
            /// Middle button up in non-client area message.
            /// </summary>
            NonClientMiddleButtonUp = 0x00A8,

            /// <summary>
            /// Middle button double click in non-client area message.
            /// </summary>
            NonClientMiddleButtonDoubleClick = 0x00A9,

            /// <summary>
            /// Set focus message
            /// </summary>
            SetFocus = 0x0007,

            /// <summary>
            /// Kill focus message
            /// </summary>
            KillFocus = 0x0008
        };

        /// <summary>
        /// Allows changes in a window to be redrawn or prevents changes in that window from being 
        /// redrawn.
        /// </summary>
        public const int SetRedraw = (int)Codes.SetRedraw;

        /// <summary>
        /// Sent when the system or another application makes a request to paint a portion of an
        /// application's window.
        /// </summary>
        public const int Paint = (int)Codes.Paint;

        /// <summary>
        /// The first of all key related messages.
        /// </summary>
        public const int KeyFirst = (int)Codes.KeyFirst;

        /// <summary>
        /// Key down message
        /// </summary>
        public const int KeyDown = (int)Codes.KeyDown;

        /// <summary>
        /// Key up message
        /// </summary>
        public const int KeyUp = (int)Codes.KeyUp;

        /// <summary>
        /// Character message
        /// </summary>
        public const int Character = (int)Codes.Character;

        /// <summary>
        /// Dead char message
        /// </summary>
        public const int DeadCharacter = (int)Codes.DeadCharacter;

        /// <summary>
        /// System key down message
        /// </summary>
        public const int SystemKeyDown = (int)Codes.SystemKeyDown;

        /// <summary>
        /// System key up message
        /// </summary>
        public const int SystemKeyUp = (int)Codes.SystemKeyUp;

        /// <summary>
        /// System char message
        /// </summary>
        public const int SystemCharacter = (int)Codes.SystemCharacter;

        /// <summary>
        /// System dead char message
        /// </summary>
        public const int SystemDeadCharacter = (int)Codes.SystemDeadCharacter;

        /// <summary>
        /// The last of all key related messages.
        /// </summary>
        public const int KeyLast = (int)Codes.KeyLast;

        /// <summary>
        /// The user chose a command from the Window menu or chose the maximize button, minimize 
        /// button, restore button, or close button.
        /// </summary>
        public const int SystemCommand = (int)Codes.SystemCommand;

        /// <summary>
        /// Horizontal scroll message
        /// </summary>
        public const int HorizontalScroll = (int)Codes.HorizontalScroll;

        /// <summary>
        /// Vertical scroll message
        /// </summary>
        public const int VerticalScroll = (int)Codes.VerticalScroll;

        /// <summary>
        /// The first of all mouse related messages.
        /// </summary>
        public const int MouseFirst = (int)Codes.MouseFirst;

        /// <summary>
        /// Mouse move message
        /// </summary>
        public const int MouseMove = (int)Codes.MouseMove;

        /// <summary>
        /// Left mouse button down message
        /// </summary>
        public const int LeftButtonDown = (int)Codes.LeftButtonDown;

        /// <summary>
        /// Left mouse button up message
        /// </summary>
        public const int LeftButtonUp = (int)Codes.LeftButtonUp;

        /// <summary>
        /// Left mouse button double click message
        /// </summary>
        public const int LeftButtonDoubleClick = (int)Codes.LeftButtonDoubleClick;

        /// <summary>
        /// Right mouse button down message
        /// </summary>
        public const int RightButtonDown = (int)Codes.RightButtonDown;

        /// <summary>
        /// Right mouse button up message
        /// </summary>
        public const int RightButtonUp = (int)Codes.RightButtonUp;

        /// <summary>
        /// Right mouse button double click message
        /// </summary>
        public const int RightButtonDoubleClick = (int)Codes.RightButtonDoubleClick;

        /// <summary>
        /// Middle mouse button down message
        /// </summary>
        public const int MiddleButtonDown = (int)Codes.MiddleButtonDown;

        /// <summary>
        /// Middle mouse button up message
        /// </summary>
        public const int MiddleButtonUp = (int)Codes.MiddleButtonUp;

        /// <summary>
        /// Middle mouse button double click message
        /// </summary>
        public const int MiddleButtonDoubleClick = (int)Codes.MiddleButtonDoubleClick;

        /// <summary>
        /// Mouse wheel message
        /// </summary>
        public const int MouseWheel = (int)Codes.MouseWheel;

        /// <summary>
        /// The last of all mouse related messages.
        /// </summary>
        public const int MouseLast = (int)Codes.MouseLast;

        /// <summary>
        /// Left button down in non-client area message.
        /// </summary>
        public const int NonClientLeftButtonDown = (int)Codes.NonClientLeftButtonDown;

        /// <summary>
        /// Left button up in non-client area message.
        /// </summary>
        public const int NonClientLeftButtonUp = (int)Codes.NonClientLeftButtonUp;

        /// <summary>
        /// Left button double click in non-client area message.
        /// </summary>
        public const int NonClientLeftButtonDoubleClick = (int)Codes.NonClientLeftButtonDoubleClick;

        /// <summary>
        /// Right button down in non client area message.
        /// </summary>
        public const int NonClientRightButtonDown = (int)Codes.NonClientRightButtonDown;

        /// <summary>
        /// Right button up in non-client area message.
        /// </summary>
        public const int NonClientRightButtonUp = (int)Codes.NonClientRightButtonUp;

        /// <summary>
        /// Right button double click in non-client area message.
        /// </summary>
        public const int NonClientRightButtonDoubleClick = (int)Codes.NonClientRightButtonDoubleClick;

        /// <summary>
        /// Middle button down in non client area message.
        /// </summary>
        public const int NonClientMiddleButtonDown = (int)Codes.NonClientMiddleButtonDown;

        /// <summary>
        /// Middle button up in non-client area message.
        /// </summary>
        public const int NonClientMiddleButtonUp = (int)Codes.NonClientMiddleButtonUp;

        /// <summary>
        /// Middle button double click in non-client area message.
        /// </summary>
        public const int NonClientMiddleButtonDoubleClick = (int)Codes.NonClientMiddleButtonDoubleClick;

        /// <summary>
        /// Set focus message
        /// </summary>
        public const int SetFocus = (int)Codes.SetFocus;

        /// <summary>
        /// Kill focus message
        /// </summary>
        public const int KillFocus = (int)Codes.KillFocus;


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
