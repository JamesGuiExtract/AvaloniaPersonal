using EnvDTE80;
using Extract.LICodeDB;
using Extract.VisualStudio.AddIns;
using LICode;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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
        /// Called when the add-in is loaded for the very first time.
        /// </summary>
        /// <param name="dte">The design time extensions object.</param>
        protected override void Initialize(DTE2 dte)
        {
            ICommand command = new InsertLI(dte, LIType.Exception);
            AddCommand(command);
            
            command = new InsertLI(dte, LIType.Method);
            AddCommand(command);

            command = new ReplaceLI(dte);
            AddCommand(command);

            command = new PasteLI(dte);
            AddCommand(command);
        }
    }
}