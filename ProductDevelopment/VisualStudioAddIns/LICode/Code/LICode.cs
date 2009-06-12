using EnvDTE;
using EnvDTE80;
using Extensibility;
using Extract.VisualStudio.AddIns;
using LICode;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extract
{
	/// <summary>
	/// Represents the main point of connection between the Location Identifier add-in and 
    /// Visual Studio.
	/// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    [ComVisible(true)]
    public class LICode : ConnectBase
	{
        /// <summary>
        /// Gets the user displayable name of the add-in.
        /// </summary>
        /// <returns>The user displayable name of the add-in.</returns>
        protected override string AddInName
        {
            get
            {
                return "Location Identifier";
            }
        }

        /// <summary>
        /// Called when the add-in is loaded for the very first time
        /// </summary>
        /// <param name="dte">The design time extensions object.</param>
        protected override void Initialize(DTE2 dte)
        {
            CommandSettings command = new CommandSettings("InsertELI", new InsertLI(dte, LIType.Exception));
            command.IsOnCodeWindowMenu = true;
            command.ToolTip = "Insert ELI";
            command.Bindings = "Global::Shift+Alt+E";
            AddCommand(command);

            command = new CommandSettings("InsertMLI", new InsertLI(dte, LIType.Method));
            command.IsOnCodeWindowMenu = true;
            command.ToolTip = "Insert MLI";
            command.Bindings = "Global::Shift+Alt+M";
            AddCommand(command);

            command = new CommandSettings("ReplaceLI", new ReplaceLI(dte));
            command.IsOnCodeWindowMenu = true;
            command.ToolTip = "Replace LI";
            AddCommand(command);

            command = new CommandSettings("PasteLI", new PasteLI(dte));
            command.IsOnCodeWindowMenu = true;
            command.ToolTip = "Paste With New LI";
            command.Bindings = "Global::Ctrl+K, Ctrl+Shift+V";
            AddCommand(command);
        }
    }
}