using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extract.VisualStudio.AddIns
{
    /// <summary>
    /// Represents a framework for making a Visual Studio addin. 
    /// </summary>
    [ComVisible(true)]
    public abstract class ConnectBase : IDTExtensibility2, IDTCommandTarget, IDisposable
    {
        #region ConnectBase Fields

        AddIn _addIn;
        string _typeName;
        Dictionary<string, ICommand> _addInCommands = new Dictionary<string, ICommand>();
        DTE2 _dte;
        VisualStudioUI _ui;

        #endregion ConnectBase Fields

        #region ConnectBase Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectBase"/> class.
        /// </summary>
        protected ConnectBase()
        {
        }

        #endregion ConnectBase Constructors

        #region ConnectBase Properties

        /// <summary>
        /// Gets the user displayable name of the add-in.
        /// </summary>
        /// <returns>The user displayable name of the add-in.</returns>
        protected abstract string AddInName
        {
            get;
        }

        #endregion ConnectBase Properties

        #region ConnectBase Methods

        /// <summary>
        /// Called when the add-in is loaded for the very first time. It is only called once. The 
        /// add-in is responsible for creating <see cref="CommandUISettings"/> structures that 
        /// describe the UI and then calling <see cref="AddCommand"/> to create them.
        /// </summary>
        /// <param name="dte">The design time extensions object.</param>
        abstract protected void Initialize(DTE2 dte);

        /// <summary>
        /// Should be called from within <see cref="Initialize"/> to specify a new command to add 
        /// to the Visual Studio environment.
        /// </summary>
        /// <param name="command">The command to add to the Visual Studio environment.</param>
        protected void AddCommand(ICommand command)
        {
            _addInCommands.Add(command.Name, command);
        }

        /// <summary>
        /// Creates the user interface for the commands.
        /// </summary>
        void CreateUserInterface()
        {
            CommandBar addinMenu = null;
            CommandBar addinToolbar = null;
            CommandBar codeWindowContextMenu = null;

            foreach (ICommand iCommand in _addInCommands.Values)
            {
                CommandUISettings settings = iCommand.GetUISettings();

                Command command = _ui.AddCommand(_addIn, settings);

                if (settings.Bindings != null)
                {
                    command.Bindings = new object[] { settings.Bindings };
                }

                if (settings.IsOnMainMenu)
                {
                    if (addinMenu == null)
                    {
                        addinMenu = GetAddInMenu();
                    }
                    command.AddControl(addinMenu, addinMenu.Controls.Count + 1);
                }

                if (settings.IsOnToolBar)
                {
                    if (addinToolbar == null)
                    {
                        addinToolbar = GetAddInToolbar();
                    }
                    command.AddControl(addinToolbar, addinToolbar.Controls.Count + 1);
                }

                if (settings.IsOnCodeWindowMenu)
                {
                    if (codeWindowContextMenu == null)
                    {
                        codeWindowContextMenu = _ui.CodeWindowMenu;
                    }
                    command.AddControl(codeWindowContextMenu, 1);
                }
            }
        }

        CommandBar GetAddInToolbar()
        {
            string toolbarName = AddInName + " Toolbar";
            return _ui.GetToolBar(toolbarName) ?? _ui.AddToolBar(toolbarName);
        }

        CommandBar GetAddInMenu()
        {
            CommandBarPopup popup = _ui.GetMenuPopup(AddInName);
            return popup == null ? _ui.AddMenu(AddInName) : popup.CommandBar;
        }

        ICommand GetCommandFromName(string name)
        {
            // Commands are prefixed with type name
            if (name.StartsWith(_typeName, StringComparison.Ordinal) && 
                name.Length > _typeName.Length && name[_typeName.Length] == '.')
            {
                // This is the correct prefix, check if the command is present
                string actionName = name.Substring(_typeName.Length + 1);
                ICommand settings;
                if (_addInCommands.TryGetValue(actionName, out settings))
                {
                    return settings;
                }
            }
            return null;
        }

        #endregion ConnectBase Methods

        #region IDTExtensibility2 Implementation

        /// <summary>
        /// Occurs whenever an add-in is loaded into Visual Studio. 
        /// </summary>
        /// <param name="application">A reference to an instance of the integrated development 
        /// environment (IDE), <see cref="DTE"/>, which is the root object of the Visual Studio 
        /// automation model.</param>
        /// <param name="connectMode">Indicates the way the add-in was loaded into Visual Studio.
        /// </param>
        /// <param name="addInInst">An <see cref="AddIn"/> reference to the add-in's own instance. 
        /// This is stored for later use, such as determining the parent collection for the add-in.
        /// </param>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use
        /// in the add-in.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId="2#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId="1#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId="0#")]
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, 
            ref Array custom)
        {
            if (_dte == null)
            {
                _dte = (DTE2)application;
                _ui = new VisualStudioUI(_dte);
                _addIn = (AddIn)addInInst;
                _typeName = GetType().FullName;
                Initialize(_dte);
            }

            try
            {
                if (connectMode == ext_ConnectMode.ext_cm_UISetup)
                {
                    CreateUserInterface();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
            }
        }

        /// <summary>
        /// Occurs whenever an add-in is unloaded from Visual Studio.
        /// </summary>
        /// <param name="removeMode">Informs an add-in why it was unloaded.</param>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use
        /// after the add-in unloads.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId="0#")]
        public void OnDisconnection(ext_DisconnectMode removeMode, ref Array custom)
        {
            Dispose();
        }

        /// <summary>
        /// Occurs whenever an add-in is loaded or unloaded from the Visual Studio integrated 
        /// development environment (IDE). 
        /// </summary>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use
        /// in the add-in.</param>
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>
        /// Occurs whenever an add-in, which is set to load when Visual Studio starts, loads. 
        /// </summary>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use
        /// when the add-in loads.</param>
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>
        /// Occurs whenever the Visual Studio integrated development environment (IDE) shuts down 
        /// while an add-in is running. 
        /// </summary>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use
        /// in the add-in. </param>
        public void OnBeginShutdown(ref Array custom)
        {
        }

        #endregion

        #region IDTCommandTarget Implementation

        /// <summary>
        /// Returns the current status (enabled, disabled, hidden, and so forth) of the specified 
        /// named command.
        /// </summary>
        /// <param name="cmdName">The name of the command to check.</param>
        /// <param name="neededText">Specifies if information is returned from the check, and if 
        /// so, what type of information is returned.</param>
        /// <param name="statusOption">The current status of the command.</param>
        /// <param name="commandText">The text to return if <paramref name="neededText"/> is 
        /// <see cref="vsCommandStatusTextWanted.vsCommandStatusTextWantedStatus"/>.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "3#")]
        public void QueryStatus(string cmdName, vsCommandStatusTextWanted neededText,
            ref vsCommandStatus statusOption, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                ICommand command = GetCommandFromName(cmdName);
                if (command != null)
                {
                    statusOption = vsCommandStatus.vsCommandStatusSupported;
                    if (command.Enabled)
                    {
                        statusOption |= vsCommandStatus.vsCommandStatusEnabled;
                    }
                }
            }
        }

        /// <summary>
        /// Executes the specified named command.
        /// </summary>
        /// <param name="cmdName">The name of the command to execute.</param>
        /// <param name="executeOption">The execution options.</param>
        /// <param name="variantIn">A value passed to the command.</param>
        /// <param name="variantOut">A value passed back to the invoker after the command executes.</param>
        /// <param name="handled"><see langword="true"/> if command was handled; 
        /// <see langword="false"/> if it was not.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "3#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "4#")]
        public void Exec(string cmdName, vsCommandExecOption executeOption, ref object variantIn, 
            ref object variantOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                ICommand command = GetCommandFromName(cmdName);
                if (command != null)
                {
                    command.Execute();
                    handled = true;
                }
                else
                {
                    MessageBox.Show("Invalid command: " + cmdName, "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error, 
                        MessageBoxDefaultButton.Button1, 0);
                }
            }
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ConnectBase"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ConnectBase"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ConnectBase"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
