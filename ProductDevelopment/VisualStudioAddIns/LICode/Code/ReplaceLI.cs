using EnvDTE;
using EnvDTE80;
using Extract.VisualStudio.AddIns;
using System;
using System.Windows.Forms;

namespace LICode
{
    /// <summary>
    /// Represents a command that inserts a location identifier code.
    /// </summary>
    public class ReplaceLI : ICommand
    {
        #region ReplaceLI Fields

        readonly DTE2 _dte;

        #endregion ReplaceLI Fields

        #region ReplaceLI Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceLI"/> class.
        /// </summary>
        public ReplaceLI(DTE2 dte)
        {
            _dte = dte;
        }

        #endregion ReplaceLI Constructors

        #region ReplaceLI Methods

        void ReplaceLICodes()
        {
            // Get the selection text
            TextSelection selection = (TextSelection)_dte.ActiveDocument.Selection;

            // Ensure that some text has been selected
            if (string.IsNullOrEmpty(selection.Text))
            {
                return;
            }

            // Instantiate an LICodeHandler to replace LI codes
            using (LICodeHandler LIHandler = new LICodeHandler())
            {
                // Retrieve the selected text with LI codes replaced
                string output = LIHandler.ReplaceLICodesInText(selection.Text);

                // Replace the selected text with the next LI codes
                selection.Insert(output, (int)vsInsertFlags.vsInsertFlagsContainNewText);

                // Deselect the inserted text and set the active cursor to the right of the recently inserted text
                selection.CharRight(false, 1);

                // Commit the changes to LI code files
                LIHandler.CommitChanges();
            }
        }

        #endregion ReplaceLI Methods

        #region ICommand Members

        /// <summary>
        /// Gets the name of the command prefixed with one or more categories separated by periods.
        /// </summary>
        /// <returns>The name of the command prefixed with one or more categories separated by 
        /// periods.
        /// </returns>
        public string Name
        {
            get
            {
                return "ReplaceLI";
            }
        }

        /// <summary>
        /// Performs the action of the settings.
        /// </summary>
        public void Execute()
        {
            try
            {
                ReplaceLICodes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK,
                    MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
            }
        }

        /// <summary>
        /// Gets or sets whether the command is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the command is able to be executed;
        /// <see langword="false"/> if the command is not able to be executed.</value>
        /// <returns><see langword="true"/> if the command is able to be executed;
        /// <see langword="false"/> if the command is not able to be executed.</returns>
        public bool Enabled
        {
            get
            {
                // Get the active document
                Document document = _dte.ActiveDocument;
                if (document == null)
                {
                    return false;
                }

                // Get the selection text
                TextSelection selection = (TextSelection)_dte.ActiveDocument.Selection;

                // Ensure that some text has been selected
                return !string.IsNullOrEmpty(selection.Text);
            }
        }

        /// <summary>
        /// Retrieves the user interface settings for the command.
        /// </summary>
        /// <returns>The user interface settings for the command.</returns>
        public CommandUISettings GetUISettings()
        {
            CommandUISettings settings = new CommandUISettings(Name);

            settings.IsOnCodeWindowMenu = true;
            settings.ToolTip = "Replace LI";

            return settings;
        }

        #endregion
    }
}
