using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a Windows system command.
    /// </summary>
    public static class SystemCommand
    {
        /// <summary>
        /// Sizes the window.
        /// </summary>
        public const int Size = 0xF000;

        /// <summary>
        /// Moves the window.
        /// </summary>
        public const int Move = 0xF010;

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        public const int Minimize = 0xF020;

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        public const int Maximize = 0xF030;

        /// <summary>
        /// Moves to the next window.
        /// </summary>
        public const int NextWindow = 0xF040;

        /// <summary>
        /// Moves to the previous window.
        /// </summary>
        public const int PreviousWindow = 0xF050;

        /// <summary>
        /// Closes the window.
        /// </summary>
        public const int Close = 0xF060;

        /// <summary>
        /// Scrolls vertically.
        /// </summary>
        public const int VerticalScroll = 0xF070;

        /// <summary>
        /// Scrolls horizontally.
        /// </summary>
        public const int HorizontalScroll = 0xF080;

        /// <summary>
        /// Retrieves the window menu as a result of a mouse click.
        /// </summary>
        public const int MouseMenu = 0xF090;

        /// <summary>
        /// Retrieves the window menu as a result of a keystroke.
        /// </summary>
        public const int KeyMenu = 0xF100;

        /// <summary>
        /// Restores the window to its normal position and size.
        /// </summary>
        public const int Restore = 0xF120;

        /// <summary>
        /// Activates the Start menu.
        /// </summary>
        public const int TaskList = 0xF130;

        /// <summary>
        /// Executes the screen saver application specified in the [boot] section of the 
        /// System.ini file.
        /// </summary>
        public const int ScreenSave = 0xF140;

        /// <summary>
        /// Activates the window associated with the application-specified hot key.
        /// </summary>
        public const int Hotkey = 0xF150;

        /// <summary>
        /// Selects the default item; the user double-clicked the window menu.
        /// </summary>
        public const int Default = 0xF160;

        /// <summary>
        /// Sets the state of the display. This command supports devices that have power-saving 
        /// features, such as a battery-powered personal computer.
        /// </summary>
        public const int MonitorPower = 0xF170;

        /// <summary>
        /// Changes the cursor to a question mark with a pointer. If the user then clicks a 
        /// control in the dialog box, the control receives a <see cref="Help"/> message.
        /// </summary>
        public const int ContextHelp = 0xF180;
    }
}