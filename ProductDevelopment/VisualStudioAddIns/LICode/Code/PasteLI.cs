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
    public class PasteLI : ICommand
    {
        #region PasteLI Fields

        readonly DTE2 _dte;

        #endregion PasteLI Fields

        #region PasteLI Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PasteLI"/> class.
        /// </summary>
        public PasteLI(DTE2 dte)
        {
            _dte = dte;
        }

        #endregion PasteLI Constructors

        #region PasteLI Methods

        void PasteWithNewLICodes()
        {
            // Get the clipboard text
            string clipboardText = Clipboard.GetText();

            // Ensure that there is some clipboard text to paste
            if (string.IsNullOrEmpty(clipboardText))
            {
                return;
            }

            // Get the selection text
            TextSelection selection = (TextSelection)_dte.ActiveDocument.Selection;

            // Retrieve the clipboard text with LI codes replaced
            string output = ReplaceLICodesInText(clipboardText);

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

        #endregion PasteLI Methods

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
                return "PasteLI";
            }
        }

        /// <summary>
        /// Performs the action of the settings.
        /// </summary>
        public void Execute()
        {
            try
            {
                PasteWithNewLICodes();
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
                return _dte.ActiveDocument != null && !string.IsNullOrEmpty(Clipboard.GetText());
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
            settings.ToolTip = "Paste With New LI";
            settings.Bindings = "Text Editor::Ctrl+K, Ctrl+Shift+V";

            return settings;
        }

        #endregion
    }
}
