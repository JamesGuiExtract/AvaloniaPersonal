using Extensibility;
using EnvDTE;
using EnvDTE80;
using Extract.VisualStudio.AddIns;
using SourceControl;
using System;
using System.Runtime.InteropServices;

namespace Extract
{
	/// <summary>
	/// Represents the main point of connection between the Source Control Add-In and Visual Studio.
	/// </summary>
    [ComVisible(true)]
    public class SourceControlConnect : ConnectBase
	{
        /// <summary>
        /// Gets the user displayable name of the add-in.
        /// </summary>
        /// <returns>The user displayable name of the add-in.</returns>
        protected override string AddInName
        {
            get
            {
                return "Source Control";
            }
        }

        /// <summary>
        /// Called when the add-in is loaded for the very first time.
        /// </summary>
        /// <param name="dte">The design time extensions object.</param>
        protected override void Initialize(DTE2 dte)
        {
            ICommand command = new Refresh();
            AddCommand(command);

            command = new EmailGetMessage();
            AddCommand(command);

            command = new ClipboardGetMessage(dte);
            AddCommand(command);
        }
    }
}