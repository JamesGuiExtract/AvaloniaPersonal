using EnvDTE;
using EnvDTE80;
using Extract.VisualStudio.AddIns;
using LICodeDB;
using System;
using System.Linq;
using System.Text.RegularExpressions;
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

            // Retrieve the selected text with LI codes replaced
            string output = ReplaceLICodesInText(selection.Text);

            // Replace the selected text with the next LI codes
            selection.Insert(output, (int)vsInsertFlags.vsInsertFlagsContainNewText);

            // Deselect the inserted text and set the active cursor to the right of the recently inserted text
            selection.CharRight(false, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textToReplace"></param>
        /// <returns></returns>
        public string ReplaceLICodesInText(string textToReplace)
        {
            // replace the selected text with new LI codes
            Regex regex = new Regex("\"(M|E)LI\\d+\"", RegexOptions.Compiled);
            return regex.Replace(textToReplace, ReplaceLIMatch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public string ReplaceLIMatch(Match match)
        {
            // Instantiate an LICodeHandler to retrieve LI code
            LICodeDBDataContext LIDB = new LICodeDBDataContext();

            // Get one new li code
            var LIRecords = match.Value[1] == 'E' ? LIDB.GetEliCodes(1) : LIDB.GetMliCodes(1);

            var rec = LIRecords.First();

            // Get the next LI code
            return rec.LICode;
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
