﻿//------------------------------------------------------------------------------
// <copyright file="ELICode.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using LICodeDB;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LICodeExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LICode
    {
        #region Constants

        /// <summary>
        /// Insert ELI ID.
        /// </summary>
        const int InsertELIId = 0x0100;

        /// <summary>
        /// Insert MLI ID
        /// </summary>
        const int InsertMLIId = 0x0101;

        /// <summary>
        /// Replace LI ID.
        /// </summary>
        const int ReplaceLiId = 0x0102;

        /// <summary>
        /// Paste With new LI ID
        /// </summary>
        const int PasteWithNewID = 0x0103;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        static readonly Guid CommandSet = new Guid("54767123-2e00-42d2-b479-847795e074b5");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        readonly Package package;

        #endregion Constants

        /// <summary>
        /// Initializes a new instance of the <see cref="LICode"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private LICode(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            // Add the commands
            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, InsertELIId);
                var menuItem = new MenuCommand(InsertELICallback, menuCommandID);
                commandService.AddCommand(menuItem);

                menuCommandID = new CommandID(CommandSet, InsertMLIId);
                menuItem = new MenuCommand(InsertMLICallback, menuCommandID);
                commandService.AddCommand(menuItem);

                menuCommandID = new CommandID(CommandSet, ReplaceLiId);
                menuItem = new MenuCommand(ReplaceLICallback, menuCommandID);
                commandService.AddCommand(menuItem);

                menuCommandID = new CommandID(CommandSet, PasteWithNewID);
                menuItem = new MenuCommand(PasteWithNewLICallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static LICode Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new LICode(package);
        }

        #region Menu Callbacks

        /// <summary>
        /// Method inserts a ELI code at the current selection in the active document
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        void InsertELICallback(object sender, EventArgs e)
        {
            try
            {
                DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));

                if (dte == null || dte.ActiveDocument == null)
                {
                    return;
                }

                // Get the selected text
                TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;

                using (LICodeDBDataContext LIDB = new LICodeDBDataContext())
                {
                    var codeToInsert = LIDB.GetEliCodes(1).First().LICode;

                    // Insert the LI code, replacing text if any is selected
                    selection.Insert(codeToInsert, (int)vsInsertFlags.vsInsertFlagsContainNewText);

                    // Deselect the inserted text and set the active cursor 
                    // to the right of the recently inserted text
                    selection.CharRight(false, 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method inserts a MLI code at the current selection in the active document
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        void InsertMLICallback(object sender, EventArgs e)
        {
            try
            {
                DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));

                if (dte == null || dte.ActiveDocument == null)
                {
                    return;
                }

                // Get the selected text
                TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;

                using (LICodeDBDataContext LIDB = new LICodeDBDataContext())
                {
                    var codeToInsert = LIDB.GetMliCodes(1).First().LICode;

                    // Insert the LI code, replacing text if any is selected
                    selection.Insert(codeToInsert, (int)vsInsertFlags.vsInsertFlagsContainNewText);

                    // Deselect the inserted text and set the active cursor 
                    // to the right of the recently inserted text
                    selection.CharRight(false, 1); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Method replaces ELI and MLI codes in the current selection in the active document
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        void ReplaceLICallback(object sender, EventArgs e)
        {
            try
            {
                DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));

                if (dte == null || dte.ActiveDocument == null)
                {
                    return;
                }

                // Get the selection text
                TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;

                // Ensure that some text has been selected
                if (string.IsNullOrEmpty(selection.Text))
                {
                    return;
                }

                // Retrieve the selected text with LI codes replaced
                string output = ReplaceLICodesInText(selection.Text);

                // Replace the selected text with the next LI codes
                selection.Insert(output, (int)vsInsertFlags.vsInsertFlagsContainNewText);

                // Deselect the inserted text and set the active cursor to the right of the recently inserted text
                selection.CharRight(false, 1);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method pastes the current clipboard contents with the ELI and MLI codes replaced at the current selection in the active document
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        void PasteWithNewLICallback(object sender, EventArgs e)
        {
            try
            {
                DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));

                if (dte == null || dte.ActiveDocument == null)
                {
                    return;
                }

                // Get the clipboard text
                string clipboardText = Clipboard.GetText();

                // Ensure that there is some clipboard text to paste
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                // Get the selection text
                TextSelection selection = (TextSelection)dte.ActiveDocument.Selection;

                // Retrieve the clipboard text with LI codes replaced
                string output = ReplaceLICodesInText(clipboardText);

                // Replace the selected text with the next LI codes
                selection.Insert(output, (int)vsInsertFlags.vsInsertFlagsContainNewText);

                // Deselect the inserted text and set the active cursor to the right of the recently inserted text
                selection.CharRight(false, 1);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Menu Callbacks

        #region Helper Functions

        /// <summary>
        /// Searches the <paramref name="textToReplace"/> for MLI and ELI codes and returns the <paramref name="textToReplace"/> with
        /// all of the codes replace
        /// </summary>
        /// <param name="textToReplace">Text that needs MLI and ELI codes replaced</param>
        /// <returns><paramref name="textToReplace"/> with all MLI and ELI codes replaced with new codes</returns>
        string ReplaceLICodesInText(string textToReplace)
        {
            // replace the selected text with new LI codes
            Regex regex = new Regex("\"(M|E)LI\\d+\"", RegexOptions.Compiled);
            return regex.Replace(textToReplace, ReplaceLIMatch);
        }

        /// <summary>
        /// Determines type of code that is being replaced using match and returns the appropiate code
        /// </summary>
        /// <param name="match">The match that needs to be replcaed</param>
        /// <returns>The LI Code to replace the match with</returns>
        string ReplaceLIMatch(Match match)
        {
            // Instantiate an LICodeHandler to retrieve LI code
            using (LICodeDBDataContext LIDB = new LICodeDBDataContext())
            {
                // Get one new li code
                var LIRecords = match.Value[1] == 'E' ? LIDB.GetEliCodes(1) : LIDB.GetMliCodes(1);

                var rec = LIRecords.First();

                // Get the next LI code
                return rec.LICode; 
            }
        }

        #endregion Helper Functions
    }
}
