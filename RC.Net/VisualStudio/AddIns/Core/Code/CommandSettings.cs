

namespace Extract.VisualStudio.AddIns
{
    /// <summary>
    /// Represents the settings associated with a command and its user interface.
    /// </summary>
    public class CommandSettings
    {
        #region CommandSettings Fields

        readonly string _name;
        readonly ICommandAction _action;
        readonly string _category;
        string _toolTip;
        string _bindings;
        bool _isOnMainMenu;
        bool _isOnToolBar;
        bool _isOnCodeWindowMenu;

        #endregion CommandSettings Fields

        #region CommandSettings Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandSettings"/> class.
        /// </summary>
        /// <param name="fullName">The name of the command prefixed with one or more categories
        /// separated by periods.</param>
        /// <param name="action">The action to perform when the command is executed.</param>
        public CommandSettings(string fullName, ICommandAction action)
        {
            _name = fullName;
            _action = action;
            _category = "";
            int i = fullName.LastIndexOf('.');
            if (i >= 0)
            {
                _name = fullName.Substring(i + 1);
                _category = fullName.Substring(0, i);
            }
            _toolTip = _name;
        }

        #endregion CommandSettings Constructors

        #region CommandSettings Properties

        /// <summary>
        /// Gets the name of the command prefixed with one or more categories
        /// separated by periods.
        /// </summary>
        /// <returns>The name of the command prefixed with one or more categories
        /// separated by periods.
        /// </returns>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the action that the command performs.
        /// </summary>
        /// <value>The action that the command performs.</value>
        public ICommandAction Action
        {
            get
            {
                return _action;
            }
        }

        /// <summary>
        /// Gets the category into which the command is grouped (may contain multiple levels 
        /// separated by periods).
        /// </summary>
        /// <returns>The category into which the command is grouped (may contain multiple levels 
        /// separated by periods).</returns>
        public string Category
        {
            get
            {
                return _category;
            }
        }

        /// <summary>
        /// Gets or sets the text displayed when a user hovers the mouse pointer over any control 
        /// bound to the command.
        /// </summary>
        /// <value>The text displayed when a user hovers the mouse pointer over any control bound 
        /// to the command.</value>
        /// <returns>The text displayed when a user hovers the mouse pointer over any control bound 
        /// to the command.</returns>
        public string ToolTip
        {
            get
            {
                return _toolTip;
            }
            set
            {
                _toolTip = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the list of keystrokes used to invoke the command. For example,
        /// "Ctrl+K,Ctrl+S" defines a two-keystroke shortcut.
        /// </summary>
        /// <value>The list of keystrokes used to invoke the command.</value>
        /// <returns>The list of keystrokes used to invoke the command.</returns>
        public string Bindings
        {
            get
            {
                return _bindings;
            }
            set
            {
                _bindings = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the command should appear on the main menu.
        /// </summary>
        /// <value><see langword="true"/> if the command should appear on the main menu;
        /// <see langword="false"/> if the command should not appear on the main menu.</value>
        /// <returns><see langword="true"/> if the command should appear on the main menu;
        /// <see langword="false"/> if the command should not appear on the main menu.</returns>
        public bool IsOnMainMenu
        {
            get
            {
                return _isOnMainMenu;
            }
            set
            {
                _isOnMainMenu = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the command should appear on the add-in toolbar.
        /// </summary>
        /// <value><see langword="true"/> if command should appear on the add-in toolbar;
        /// <see langword="false"/> if it should not appear on the add-in toolbar.</value>
        /// <returns><see langword="true"/> if command should appear on the add-in toolbar;
        /// <see langword="false"/> if it should not appear on the add-in toolbar.</returns>
        public bool IsOnToolBar
        {
            get
            {
                return _isOnToolBar;
            }
            set
            {
                _isOnToolBar = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the command should appear in the Code Window context menu.
        /// </summary>
        /// <value><see langword="true"/> if the command should appear in the Code Window context 
        /// menu; <see langword="false"/> if it should not appear in the context menu.</value>
        /// <returns><see langword="true"/> if the command should appear in the Code Window context 
        /// menu; <see langword="false"/> if it should not appear in the context menu.</returns>
        public bool IsOnCodeWindowMenu
        {
            get
            {
                return _isOnCodeWindowMenu;
            }
            set
            {
                _isOnCodeWindowMenu = value;
            }
        }

        #endregion CommandSettings Properties
    };
}
