using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.VisualStudio.AddIns
{
    /// <summary>
    /// Represents the user interface of a Visual Studio instance.
    /// </summary>
    public class VisualStudioUI
    {
        #region VisualStudioUI Fields

        readonly DTE2 _dte;
        CommandBars _commandBars;
        CommandBar _codeWindow;
        CommandBar _mainMenu;
        Commands2 _commands;

        #endregion VisualStudioUI Fields

        #region VisualStudioUI Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioUI"/> class.
        /// </summary>
        /// <param name="dte">The design time extensions object.</param>
        public VisualStudioUI(DTE2 dte)
        {
            _dte = dte;
        }

        #endregion VisualStudioUI Constructors

        #region VisualStudioUI Properties

        /// <summary>
        /// Gets the Visual Studio command bars.
        /// </summary>
        /// <returns>The Visual Studio command bars.</returns>
        public CommandBars CommandBars
        {
            get
            {
                if (_commandBars == null)
                {
                    _commandBars = (CommandBars)_dte.CommandBars;
                }

                return _commandBars;
            }
        }

        /// <summary>
        /// Gets the context menu of the Code Window.
        /// </summary>
        /// <returns>The context menu of the Code Window.</returns>
        public CommandBar CodeWindowMenu
        {
            get
            {
                if (_codeWindow == null)
                {
                    _codeWindow = CommandBars["Code Window"];
                }

                return _codeWindow;
            }
        }

        /// <summary>
        /// Gets the Main Menu command bar.
        /// </summary>
        /// <returns>The Main Menu command bar.</returns>
        public CommandBar MainMenu
        {
            get
            {
                if (_mainMenu == null)
                {
                    _mainMenu = CommandBars["Menu Bar"];
                }

                return _mainMenu;
            }
        }

        /// <summary>
        /// Gets the commands associated with the user interface.
        /// </summary>
        /// <returns>The commands associated with the user interface.</returns>
        public Commands2 Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = (Commands2) _dte.Commands;
                }
                return _commands;
            }
        }

        #endregion VisualStudioUI Properties

        #region VisualStudioUI Methods

        /// <summary>
        /// Adds a menu to the main menu.
        /// </summary>
        /// <param name="name">The name of the menu to add.</param>
        /// <returns>The newly created menu.</returns>
        public CommandBar AddMenu(string name)
        {
            CommandBar menu = (CommandBar) Commands.AddCommandBar(name, 
                vsCommandBarType.vsCommandBarTypeMenu, MainMenu, MainMenu.Controls.Count);
            return menu;
        }

        /// <summary>
        /// Adds a toolbar to Visual Studio.
        /// </summary>
        /// <param name="name">The name of the toolbar.</param>
        /// <returns>The newly created toolbar.</returns>
        public CommandBar AddToolBar(string name)
        {
            CommandBar toolbar = (CommandBar) Commands.AddCommandBar(name, 
                vsCommandBarType.vsCommandBarTypeToolbar, null, 1);
            toolbar.Visible = true;
            toolbar.Position = MsoBarPosition.msoBarTop;
            return toolbar;
        }

        /// <summary>
        /// Adds an add-in command to visual studio.
        /// </summary>
        /// <param name="addIn">The add-in to which the command corresponds.</param>
        /// <param name="settings">The settings for the command.</param>
        /// <returns>The newly added command.</returns>
        public Command AddCommand(AddIn addIn, CommandUISettings settings)
        {
            object[] contextGUIDS = new object[] { };
            return Commands.AddNamedCommand2(addIn, settings.Name, settings.ToolTip, settings.ToolTip, 
                true, 1, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported,
                (int)vsCommandStyle.vsCommandStyleText, vsCommandControlType.vsCommandControlTypeButton);
        }

        /// <summary>
        /// Gets the menu on the Main Menu with the specified name.
        /// </summary>
        /// <param name="name">The name of the menu to find.</param>
        /// <returns>The menu with the specified <paramref name="name"/> or <see langword="null"/>
        /// if no such menu exists.</returns>
        public CommandBarPopup GetMenuPopup(string name)
        {
            return FindControl(name, MainMenu.Controls) as CommandBarPopup;
        }

        /// <summary>
        /// Gets the toolbar with the specified name.
        /// </summary>
        /// <param name="name">The name of the toolbar to find.</param>
        /// <returns>The toolbar with the specified <paramref name="name"/> or 
        /// <see langword="null"/> if no such menu exists.</returns>
        public CommandBar GetToolBar(string name)
        {
            return FindCommandBar(name, CommandBars);
        }

        /// <summary>
        /// Finds the command bar with the specified name.
        /// </summary>
        /// <param name="name">The name of the command bar to find.</param>
        /// <param name="commandBars">The command bars to search.</param>
        /// <returns>The command bar named <paramref name="name"/> or <see langword="null"/> if 
        /// it is not in <paramref name="commandBars"/>.</returns>
        /// <remarks>The indexer for <see cref="CommandBars"/> throws an exception if it is not 
        /// present. This method avoids using exceptions for normal control flow.</remarks>
        static CommandBar FindCommandBar(string name, CommandBars commandBars)
        {
            foreach (CommandBar bar in commandBars)
            {
                if (bar.Name == name)
                {
                    return bar;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the command bar control with the specified name.
        /// </summary>
        /// <param name="name">The name of the command bar control to find.</param>
        /// <param name="controls">The command bar controls to search.</param>
        /// <returns>The command bar control named <paramref name="name"/> or 
        /// <see langword="null"/> if it is not in <paramref name="controls"/>.</returns>
        /// <remarks>The indexer for <see cref="CommandBarControls"/> throws an exception if it is 
        /// not present. This method avoids using exceptions for normal control flow.</remarks>
        static CommandBarControl FindControl(string name, CommandBarControls controls)
        {
            foreach (CommandBarControl control in controls)
            {
                if (control.Caption == name)
                {
                    return control;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the command with the specified name.
        /// </summary>
        /// <param name="name">The name of the command to find.</param>
        /// <param name="commands">The commands to search.</param>
        /// <returns>The command named <paramref name="name"/> or <see langword="null"/> if it is 
        /// not in <paramref name="commands"/>.</returns>
        /// <remarks>The indexer for <see cref="Commands2"/> throws an exception if it is not 
        /// present. This method avoids using exceptions for normal control flow.</remarks>
        public static Command FindCommand(string name, Commands2 commands)
        {
            foreach (Command command in commands)
            {
                if (command.Name == name)
                {
                    return command;
                }
            }
            return null;
        }

        #endregion VisualStudioUI Methods
    }
}
